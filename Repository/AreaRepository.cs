namespace IngBackend.Repository;

using AutoMapper;
using IngBackend.Context;
using IngBackend.Interfaces.Repository;
using IngBackend.Models.DBEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public class AreaRepository(IngDbContext context, IMapper mapper) : Repository<Area, Guid>(context), IAreaRepository
{
    private readonly IngDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    public IQueryable<Area> GetAreaByIdIncludeAll(Guid id) => _context.Area
            .Include(a => a.TextLayout)
            .Include(a => a.ImageTextLayout)
                .ThenInclude(it => it.Image)
            .Include(a => a.ListLayout)
                .ThenInclude(l => l.Items)
                    .ThenInclude(t => t.Type)
             .Include(a => a.KeyValueListLayout)
                .ThenInclude(kv => kv.Items)
            .Include(a => a.AreaType)
            .Include(a => a.User)
            .Where(a => a.Id == id);

    public IQueryable<Area> GetAreaByIdIncludeUser(Guid id) => _context.Area.Include(a => a.User).Where(a => a.Id == id);

    public IQueryable<AreaType> GetAreaTypeByIdIncludeAll(int id) => _context.AreaType
        .Include(at => at.ListTagTypes)
        .Where(a => a.Id == id);


    private void SetDetached<T>(T entity)
    {
        _context.Entry(entity).State = EntityState.Detached;
    }

    private void SetUnchanged<T>(T entity)
    {
        _context.Entry(entity).State = EntityState.Unchanged;
    }

    private void SetModified<T>(T entity)
    {
        _context.Entry(entity).State = EntityState.Modified;
    }

    private void MapTagList(ref List<Tag> dbTags, ref List<Tag> newTags, ListLayout listLayout, Guid? userId)
    {
        // update & add tag to database
        // TODO: map tag with authentication
        var newTagsIds = newTags.Select(x => x.Id).ToList();
        var dbTagsIds = dbTags.Select(x => x.Id).ToList();

        dbTags.ForEach(t => SetUnchanged(t.Type));

        // 1. 要刪除的 Tag  3.要新增的 Tag
        var tagsToDelete = dbTags.Where(tag => !newTagsIds.Contains(tag.Id)).ToList();
        // 斷開與 ListLayout 的連接
        tagsToDelete.ForEach(tag =>
        {
            if (tag.ListLayouts != null)
            {
                tag.ListLayouts.RemoveAll(x => x.Id == listLayout.Id);
            }
        });
        _context.UpdateRange(tagsToDelete);

        // 2.要更新的 Tag
        var tagsToUpdateIds = newTags.Where(t => dbTagsIds.Contains(t.Id)).Select(x => x.Id).ToList();
        _mapper.Map(newTags, dbTags);
        var tagsToUpdate = dbTags.Where(x => tagsToUpdateIds.Contains(x.Id));
        _context.UpdateRange(tagsToDelete);

        // 3.要新增的 Tag (在資料庫內)
        newTags.RemoveAll(x => tagsToUpdateIds.Contains(x.Id));
        var tagsInDbToAdd = newTags.Where(x => !x.Id.Equals(Guid.Empty)).Select(x => _context.Tag.Find(x.Id)).ToList();
        tagsInDbToAdd.ForEach(t => SetUnchanged(t.Type));
        // 加入與 ListLayout 的關聯
        tagsInDbToAdd.ForEach(tag =>
        {
            tag.ListLayouts.Add(listLayout);
        });
        _context.UpdateRange(tagsInDbToAdd);
        // 更新到 dbTags
        dbTags = dbTags.Concat(tagsInDbToAdd)
                        .GroupBy(item => item.Id)
                        .Select(group => group.Last())
                        .ToList();

        // 4.要新增的 Tag (不在資料庫內)
        var tagsToAdd = newTags.Where(x => x.Id.Equals(Guid.Empty));
        _context.UpdateRange(tagsToAdd);
    }

    private bool IsTagOwner(Tag tag, Guid userId)
    {
        // TODO: do tag owner authentication
        return true;
        // return tag.OwnerId == userId;
    }

    // Map ListLayout
    private async Task MapListLayout(Area area, Area newArea, Guid userId)
    {
        var areaTagList = area.ListLayout.Items;
        var newAreaTagList = newArea.ListLayout.Items;

        // MAP TAGS
        MapTagList(ref areaTagList, ref newAreaTagList, area.ListLayout, userId);
        // MAP area without tag
        area.ListLayout.Items = areaTagList;
    }

    private async Task AddListLayout(Area newArea, Guid userId)
    {
        var areaTagList = new List<Tag>() { };
        var newAreaTagList = newArea.ListLayout.Items;

        // MAP TAGS
        MapTagList(ref areaTagList, ref newAreaTagList, newArea.ListLayout, userId);

        // ADD AREA
        newArea.ListLayout.Items = areaTagList;
        _context.Add(newArea);
    }



    public async Task PostArea(Area postArea, Guid userId)
    {
        var area = await GetAreaByIdIncludeAll(postArea.Id).FirstOrDefaultAsync();
        // Add relationship between user and area
        postArea.UserId = userId;

        // Update Area
        if (area != null)
        {
            // ListLayout
            if (area.ListLayout != null && postArea.ListLayout != null)
            {
                await MapListLayout(area, postArea, userId);
                _mapper.Map(postArea, area);
            }

            _context.Update(area);
            await _context.SaveChangesAsync();
            return;
            // TODO: Detach all tracking entities
        }

        // Add new Area
        if (postArea.ListLayout != null)
        {
            await AddListLayout(postArea, userId);
            _mapper.Map(postArea, area);
            await _context.SaveChangesAsync();
            return;
        }

        // TODO: Detach all tracking entities
        return;


        // var foundArea = areas.Find(x => x.Id == updatedArea.Id);
        // if (foundArea != null)
        // {
        //     _mapper.Map(updatedArea, foundArea);

        //     if (updatedArea.ListLayout != null && foundArea.ListLayout != null)
        //     {
        //         // Remove not shared Tags in origin
        //         foundArea.ListLayout.Items.RemoveAll(x => !updatedArea.ListLayout.Items.Any(t => t.Id == x.Id));

        //         // Update Tag
        //         foreach (var x in updatedArea.ListLayout.Items)
        //         {
        //             var foundTag = foundArea.ListLayout?.Items.Find(t => t.Id == x.Id);

        //             // Update Tags which exist in origin
        //             if (foundTag != null)
        //             {
        //                 // TODO: auth check
        //                 _mapper.Map(x, foundTag);

        //             }
        //             else // Tag not in origin
        //             {
        //                 var entityTag = await _context.Tag.Where(t => t.Id == x.Id).AsNoTracking().FirstOrDefaultAsync();

        //                 // Add exist Entity Tag
        //                 if (entityTag != null)
        //                 {

        //                     foundArea.ListLayout.Items.Add(entityTag);
        //                 }

        //                 // Add new Tag Entity to database
        //                 else
        //                 {
        //                     var newTag = x;
        //                     _context.Entry(newTag.Type).State = EntityState.Unchanged;
        //                     await _context.Tag.AddAsync(newTag);
        //                     await _context.SaveChangesAsync();

        //                     foundArea.ListLayout.Items.Add(newTag);
        //                 }
        //             }
        //         }

        //     }
        // }
        // else
        // {
        //     var newArea = new Area
        //     {
        //         Sequence = updatedArea.Sequence,
        //         Title = updatedArea.Title,
        //         IsDisplayed = updatedArea.IsDisplayed
        //     };

        //     if (updatedArea.ListLayout != null)
        //     {
        //         foreach (var x in updatedArea.ListLayout.Items)
        //         {

        //             var entityTag = await _context.Tag.Where(t => t.Id == x.Id).AsNoTracking().FirstOrDefaultAsync();


        //             // Add Entity Tag
        //             if (entityTag != null)
        //             {
        //                 newArea.ListLayout ??= new ListLayout
        //                 {
        //                     Area = newArea,
        //                 };
        //                 newArea.ListLayout.Items.Add(entityTag);
        //             }

        //             // Add New Tag
        //             else
        //             {
        //                 var newTag = _mapper.Map<Tag>(x);
        //                 _context.Entry(newTag.Type).State = EntityState.Unchanged;
        //                 newArea.ListLayout ??= new ListLayout
        //                 {
        //                     Area = newArea,
        //                 };
        //                 await _context.Tag.AddAsync(newTag);
        //                 await _context.SaveChangesAsync();
        //                 newArea.ListLayout.Items.Add(newTag);
        //             }
        //         }
        //     }

        //     await _context.Area.AddAsync(newArea);
        //     await _context.SaveChangesAsync();
        //     areas.Add(newArea);
        // }





        // var area = _context.Area.Include(a => a.ListLayout).ThenInclude(a => a.Items).ThenInclude(a => a.Type).AsNoTracking().FirstOrDefault(x => x.Id == updatedArea.Id);
        // if (area != null)
        // {
        //     area.ListLayout.Items.ForEach(z =>
        //         _context.Entry(z).State = EntityState.Detached
        //     );
        //     // TODO: 新增 tag 權限問題 
        //     var existTagIds = _context.Tag.AsNoTracking().ToList().Select(x => x.Id);
        //     var newTags = area.ListLayout.Items.Where(x => !existTagIds.Contains(x.Id)).ToList();
        //     await _context.Tag.AddRangeAsync(newTags);
        //     newTags.ForEach(z =>
        //         _context.Entry(z).State = EntityState.Detached
        //     );
        //     return area;
        // }

        // var user = _context.User.Include(u => u.Areas).Where(u => u.Id == updatedArea.UserId).AsNoTracking().FirstOrDefault();
        // var newArea = _mapper.Map<Area>(updatedArea);

        // if (newArea.ListLayout != null)
        // {
        //     newArea.ListLayout.Items.ForEach(z =>
        //         _context.Entry(z).State = EntityState.Unchanged
        //     );
        //     var existTagIds = _context.Tag.AsNoTracking().ToList().Select(x => x.Id);
        //     var newTags = newArea.ListLayout.Items.Where(x => !existTagIds.Contains(x.Id));
        //     await _context.Tag.AddRangeAsync(newTags);
        // }
        // return newArea;

        // user.Areas.Add(newArea);

        // else
        // {
        //     // area.ListLayout?.Items.ForEach(z =>
        //     //         _context.Entry(z).State = EntityState.Detached
        //     //     );
        //     var w = GetLocal();
        //     _mapper.Map(updatedArea, area);
        //     area.User = null;
        //     _context.Area.Update(area);
        //     _context.Entry(area).State = EntityState.Detached;
        // }


        // await _context.SaveChangesAsync();
    }
}