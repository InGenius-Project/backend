namespace IngBackend.Repository;

using AutoMapper;
using IngBackend.Context;
using IngBackend.Interfaces.Repository;
using IngBackend.Models.DBEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public class AreaRepository(IngDbContext context, IMapper mapper)
    : Repository<Area, Guid>(context),
        IAreaRepository
{
    private readonly IngDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    public IQueryable<Area> GetAreaByIdIncludeAll(Guid id) =>
        _context
            .Area.Include(a => a.TextLayout)
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

    public IQueryable<Area> GetAreaByIdIncludeUser(Guid id) =>
        _context.Area.Include(a => a.User).Where(a => a.Id == id);

    public IQueryable<AreaType> GetAreaTypeByIdIncludeAll(int id) =>
        _context.AreaType.Include(at => at.ListTagTypes).Where(a => a.Id == id);

    private void SetDetached<T>(T entity)
    {
        _context.Entry(entity).State = EntityState.Detached;
    }

    private void SetUnchanged<T>(T entity)
    {
        _context.Entry(entity).State = EntityState.Unchanged;
    }

    public void Update<T>(T entity)
    {
        _context.Update(entity);
    }

    // 在 ListLayout 中替換成追蹤中的 Tag 與 TagType 實體
    private void AttachLocalTagAndTagTypeToListLayout(
        ListLayout listLayout,
        IEnumerable<Tag> tags,
        IEnumerable<TagType> tagTypes
    )
    {
        for (var i = 0; i < listLayout.Items.Count; i++)
        {
            var tag = listLayout.Items[i];
            // 替換 TagType 實體
            if (!tag.TypeId.Equals(Guid.Empty))
            {
                tag.Type = tagTypes.Single(x => x.Id.Equals(tag.TypeId));
            }
            // 替換 Tag 實體
            if (!tag.Id.Equals(Guid.Empty))
            {
                // 找尋 Database 中的 Tag
                var tagFind = tags.Single(x => tag.Id.Equals(x.Id));
                if (tagFind != null)
                {
                    // 更新 Tag 的內容
                    _mapper.Map(tag, tagFind);
                    listLayout.Items[i] = tagFind;
                }
            }
        }
    }

    private void PostUserAreas(User user)
    {
        // 獲取 Tags 與 TagTypes 的追蹤實體
        var tags = _context.Tag;
        var tagTypes = _context.TagType;

        void SaveListLayout(Area area)
        {
            // 為防止重複插入關聯 (Many-To-Many)
            // 需要追蹤 Tag 以及 ListLayout
            if (area.ListLayout != null)
            {
                // 先把 Tag 的 Type 設為 null
                // 否則開始追蹤 ListLayout 的時候會有一樣的 TagType 被追蹤
                // 進而導致 Concurrency 問題
                area.ListLayout.Items.ForEach(tag => tag.Type = null);
                _context.Attach(area.ListLayout);
                _context.AttachRange(area.ListLayout.Items);
                area.ListLayout.Items.ForEach(tag => _context.Attach(tag.Type));

                // 在 ListLayout 中替換成追蹤中的 Tag 與 TagType 實體
                AttachLocalTagAndTagTypeToListLayout(area.ListLayout, tags, tagTypes);
                _context.Update(area.ListLayout);
            }
        }
        user.Areas.ForEach(SaveListLayout);
    }

    private void PostUserAreas(IEnumerable<Area> areas, Guid? userId)
    {
        // 獲取 Tags 與 TagTypes 的追蹤實體
        var tags = _context.Tag;
        var tagTypes = _context.TagType;

        void SaveListLayout(Area area)
        {
            // 為防止重複插入關聯 (Many-To-Many)
            // 需要追蹤 Tag 以及 ListLayout
            if (area.ListLayout != null)
            {
                // 先把 Tag 的 Type 設為 null
                // 否則開始追蹤 ListLayout 的時候會有一樣的 TagType 被追蹤
                // 進而導致 Concurrency 問題
                area.ListLayout.Items.ForEach(tag => tag.Type = null);
                _context.Attach(area.ListLayout);

                // 在 ListLayout 中替換成追蹤中的 Tag 與 TagType 實體
                AttachLocalTagAndTagTypeToListLayout(area.ListLayout, tags, tagTypes);
                _context.Update(area.ListLayout);
            }

            // 加入使用者關聯
            if (userId == null)
            {
                return;
            }
            area.UserId = userId;
        }
        areas.ToList().ForEach(SaveListLayout);
    }

    public async Task PostAreas(IEnumerable<Area> areas, Guid? userId) =>
        PostUserAreas(areas, userId);

    public async Task PostAreas(User user) => PostUserAreas(user);
}
