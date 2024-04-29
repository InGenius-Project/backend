namespace IngBackendApi.UnitTest.Fixtures;

using IngBackendApi.Models.DBEntity;

public class TagFixture
{
    public Fixture Fixture { get; }

    public TagFixture()
    {
        Fixture = new Fixture();

        Fixture
            .Behaviors.OfType<ThrowingRecursionBehavior>()
            .ToList()
            .ForEach(b => Fixture.Behaviors.Remove(b));
        Fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        #region Customize BaseEntity
        Fixture.Customize<BaseEntity>(c => c.OmitAutoProperties());
        #endregion

        #region Customize Tag
        Fixture.Customize<Tag>(c =>
            c.With(t => t.Id, Guid.Empty)
                .With(t => t.Name, "Sample Tag")
                .With(t => t.Count, 0)
                .Without(t => t.ListLayouts)
                .Without(t => t.KeyValueItems)
                .Without(t => t.Owners)
        );
        #endregion

        #region Customize TagType
        Fixture.Customize<TagType>(c =>
            c.With(tt => tt.Name, "TagName")
                .With(tt => tt.Value, "Value")
                .With(tt => tt.Color, "#123")
                .Without(tt => tt.AreaTypes)
        );
        #endregion
    }
}
