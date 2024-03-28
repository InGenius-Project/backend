namespace IngBackendApi.Interfaces.Service;

using IngBackendApi.Models.DBEntity;
using IngBackendApi.Models.DTO;

public interface ITagService : IService<Tag, TagDTO, Guid>
{
    Task<List<TagDTO>> GetAllTagsByTypes(string[] type);
    new Task<TagDTO> AddAsync(TagDTO tagDto);

    /// <summary>
    /// Checks the ownership of a tag by a user.
    /// </summary>
    /// <param name="tagId">The ID of the tag.</param>
    /// <param name="userId">The ID of the user.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task CheckOwnerShip(Guid tagId, Guid userId);
}
