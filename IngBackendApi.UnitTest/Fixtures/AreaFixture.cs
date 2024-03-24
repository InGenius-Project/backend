namespace IngBackendApi.Test.Fixtures;

using IngBackendApi.Enum;
using IngBackendApi.Models.DBEntity;
using IngBackendApi.Models.DTO;
using Microsoft.AspNetCore.Http;

public class AreaFixture
{
    public Fixture Fixture { get; }

    public AreaFixture()
    {
        Fixture = new Fixture();

        Fixture.Behaviors
            .OfType<ThrowingRecursionBehavior>()
            .ToList()
            .ForEach(b => Fixture.Behaviors.Remove(b));
        Fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        // Customize BaseEntity
        Fixture.Customize<BaseEntity>(c => c
            .OmitAutoProperties());


        #region  Customize Area
        Fixture.Customize<Area>(c => c
            .With(p => p.Id, Guid.Empty)
            .With(p => p.Sequence, Fixture.Create<int>())
            .With(p => p.IsDisplayed, Fixture.Create<bool>())
            .With(p => p.Title, Fixture.Create<string>())
            .With(p => p.LayoutType, Fixture.Create<LayoutType>())
            .Without(p => p.ResumeId)
            .Without(p => p.Resume)
            .Without(p => p.UserId)
            .Without(p => p.User)
            .Without(p => p.RecruitmentId)
            .Without(p => p.Recruitment)
            .Without(p => p.AreaTypeId)
            .Without(p => p.AreaType)
            .Without(p => p.TextLayoutId)
            .Without(p => p.TextLayout)
            .Without(p => p.ImageTextLayoutId)
            .Without(p => p.ImageTextLayout)
            .Without(p => p.ListLayoutId)
            .Without(p => p.ListLayout)
            .Without(p => p.KeyValueListLayoutId)
            .Without(p => p.KeyValueListLayout)
        );
        #endregion

        #region  Customize AreaType
        Fixture.Customize<AreaType>(c => c
            .With(p => p.Name, Fixture.Create<string>())
            .With(p => p.Value, Fixture.Create<string>())
            .With(p => p.Description, Fixture.Create<string>())
            .With(p => p.UserRole, Fixture.Create<List<UserRole>>())
            .With(p => p.LayoutType, Fixture.Create<LayoutType>())
            .Without(p => p.Areas)
            .Without(p => p.ListTagTypes)
        );
        #endregion

        #region Customize TextLayout
        Fixture.Customize<TextLayout>(c => c
            .With(p => p.Id, Guid.Empty)
            .With(p => p.Content, Fixture.Create<string>())
            .Without(p => p.AreaId)
            .Without(p => p.Area)
        );
        #endregion

        #region Customize Image
        Fixture.Customize<Image>(c => c
            .With(p => p.Id, Guid.Empty)
            .With(p => p.Filename, Fixture.Create<string>())
            .With(p => p.ContentType, Fixture.Create<string>())
            .With(p => p.Data, Fixture.Create<byte[]>())
        );
        #endregion

        #region Customize ImageTextLayout
        Fixture.Customize<ImageTextLayout>(c => c
            .With(p => p.Id, Guid.Empty)
            .With(p => p.Content, Fixture.Create<string>())
            .With(p => p.Image, Fixture.Create<Image>())
            .Without(p => p.AreaId)
            .Without(p => p.Area)
        );
        #endregion

        #region  Customize ListLayout
        Fixture.Customize<ListLayout>(c => c
            .With(p => p.Id, Guid.Empty)
            .With(p => p.Items, Fixture.Create<List<Tag>>())
            .Without(p => p.AreaId)
            .Without(p => p.Area)
        );
        #endregion

        #region Customize KeyValueListLayout
        Fixture.Customize<KeyValueListLayout>(c => c
            .With(p => p.Id, Guid.Empty)
            .With(p => p.Items, Fixture.Create<List<KeyValueItem>>())
            .Without(p => p.AreaId)
            .Without(p => p.Area)
        );
        #endregion

        #region  Customize KeyValueItem
        Fixture.Customize<KeyValueItem>(c => c
            .With(p => p.Id, Guid.Empty)
            .With(p => p.Key, Fixture.Create<Tag>())
            .With(p => p.Value, Fixture.Create<string>())
        );
        #endregion

        #region  Customize DTOs
        Fixture.Customize<AreaDTO>(c => c
            .With(p => p.Id, Guid.Empty)
            .With(p => p.Sequence, Fixture.Create<int>())
            .With(p => p.IsDisplayed, Fixture.Create<bool>())
            .With(p => p.Title, Fixture.Create<string>())
            .With(p => p.LayoutType, Fixture.Create<LayoutType?>())
            .With(p => p.AreaTypeId, Fixture.Create<int?>())
            .With(p => p.TextLayout, Fixture.Create<TextLayoutDTO>())
            .With(p => p.ImageTextLayout, Fixture.Create<ImageTextLayoutDTO>())
            .With(p => p.ListLayout, Fixture.Create<ListLayoutDTO>())
            .With(p => p.KeyValueListLayout, Fixture.Create<KeyValueListLayoutDTO>())
            .With(p => p.ResumeId, Fixture.Create<Guid?>())
            .With(p => p.RecruitmentId, Fixture.Create<Guid?>())
            .With(p => p.UserId, Fixture.Create<Guid?>())
        );
        #endregion

        #region Customize AreaPostDTO
        Fixture.Customize<AreaPostDTO>(c => c
            .With(p => p.Id, Fixture.Create<Guid?>())
            .With(p => p.Sequence, Fixture.Create<int>())
            .With(p => p.IsDisplayed, Fixture.Create<bool>())
            .With(p => p.Title, Fixture.Create<string>())
            .With(p => p.LayoutType, Fixture.Create<LayoutType?>())
            .With(p => p.AreaTypeId, Fixture.Create<int?>())
            .With(p => p.UserId, Fixture.Create<Guid?>())
            .With(p => p.ResumeId, Fixture.Create<Guid?>())
            .With(p => p.RecruitmentId, Fixture.Create<Guid?>())
        );
        #endregion

        #region Customize AreaTypeDTO
        Fixture.Customize<AreaTypeDTO>(c => c
            .With(p => p.Name, Fixture.Create<string>())
            .With(p => p.Value, Fixture.Create<string>())
            .With(p => p.Description, Fixture.Create<string>())
            .With(p => p.UserRole, Fixture.Create<List<UserRole>>())
            .With(p => p.LayoutType, Fixture.Create<LayoutType>())
            .With(p => p.ListTagTypes, [])
            .Without(p => p.Id)
        );
        #endregion

        #region Customize AreaTypePostDTO
        Fixture.Customize<AreaTypePostDTO>(c => c
            .With(p => p.Name, Fixture.Create<string>())
            .With(p => p.Value, Fixture.Create<string>())
            .With(p => p.Description, Fixture.Create<string>())
            .With(p => p.UserRole, Fixture.Create<List<UserRole>>())
            .With(p => p.LayoutType, Fixture.Create<LayoutType>())
            .With(p => p.ListTagTypes, [])
            .Without(p => p.Id)
        );
        #endregion

        #region Customize AreaFormDataDTO
        Fixture.Customize<AreaFormDataDTO>(c => c
            .With(p => p.Image, null as IFormFile)
            .With(p => p.AreaPost, Fixture.Create<AreaPostDTO>())
        );
        #endregion

        #region
        Fixture.Customize<TextLayoutDTO>(c => c
            .With(p => p.Id, Guid.Empty)
            .With(p => p.Content, Fixture.Create<string>())
        );
        #endregion

        #region
        Fixture.Customize<ImageDTO>(c => c
            .With(p => p.Id, Guid.Empty)
            .With(p => p.Filename, Fixture.Create<string>())
            .With(p => p.ContentType, Fixture.Create<string>())
            .With(p => p.Data, Fixture.Create<byte[]>())
        );
        #endregion

        #region
        Fixture.Customize<ImageTextLayoutDTO>(c => c
            .With(p => p.Id, Guid.Empty)
            .With(p => p.Image, Fixture.Create<ImageDTO>())
        );
        #endregion

        #region
        Fixture.Customize<ListLayoutDTO>(c => c
            .With(p => p.Id, Fixture.Create<Guid?>())
            .With(p => p.Items, Fixture.Create<List<TagDTO>>())
        );
        #endregion

        #region
        Fixture.Customize<ListLayoutPostDTO>(c => c
            .With(p => p.Id, Fixture.Create<Guid?>())
            .With(p => p.Items, Fixture.Create<List<TagPostDTO>>())
        );
        #endregion

        #region
        Fixture.Customize<KeyValueListLayoutDTO>(c => c
            .With(p => p.Id, Guid.Empty)
            .With(p => p.Items, Fixture.Create<List<KeyValueItemDTO>>())
        );
        #endregion

        #region
        Fixture.Customize<KeyValueItemDTO>(c => c
            .With(p => p.Id, Guid.Empty)
            .With(p => p.Key, Fixture.Create<Tag>())
            .With(p => p.Value, Fixture.Create<string>())
        );
        #endregion

    }


}
