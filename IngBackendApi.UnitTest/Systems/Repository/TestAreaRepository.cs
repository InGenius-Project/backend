namespace IngBackendApi.UnitTest.Systems.Repository;

using IngBackendApi.Context;
using IngBackendApi.Models.DBEntity;
using IngBackendApi.Repository;
using IngBackendApi.UnitTest.Fixtures;
using Microsoft.EntityFrameworkCore;

public class TestAreaRepository : IDisposable
{
    private readonly IngDbContext _context;
    private readonly AreaRepository _areaRepository;

    private readonly AreaFixture _areaFixture = new();

    private readonly TagFixture _tagFixture = new();
    public TestAreaRepository()
    {
        _context = MemoryContextFixture.Generate();

        _areaRepository = new AreaRepository(_context);
    }

    [Fact]
    public async Task AreaUpdateAsync_ValidNormalArea_ShouldUpdateArea()
    {
        // Arrange

        var area = _areaFixture.Fixture.Create<Area>();
        _context.Area.Add(area);
        _context.SaveChanges();
        _context.Entry(area).State = EntityState.Detached;


        var reqArea = _areaFixture.Fixture.Create<Area>();
        reqArea.Id = area.Id;

        // Act
        await _areaRepository.UpdateAsync(reqArea);
        await _context.SaveChangesAsync();

        // Assert
        var updatedArea = await _context.Area.FindAsync(area.Id);
        updatedArea.Should().Be(reqArea);

    }

    [Fact]
    public async Task AreaUpdateAsync_ValidListLayoutArea_ShouldUpdateArea()
    {
        // Arrange
        var area = _areaFixture.Fixture.Create<Area>();
        var listLayout = _areaFixture.Fixture.Create<ListLayout>();
        var tagType = _tagFixture.Fixture.Create<TagType>();
        var tag1 = _tagFixture.Fixture.Create<Tag>();
        var tag2 = _tagFixture.Fixture.Create<Tag>();
        tag1.TagTypeId = tagType.Id;
        tag2.TagTypeId = tagType.Id;
        listLayout.Items = [tag1, tag2];
        area.ListLayout = listLayout;


        _context.Area.Add(area);
        _context.Set<Tag>().AddRange([tag1, tag2]);
        _context.SaveChanges();
        _context.Entry(area).State = EntityState.Detached;

        var reqArea = area;
        reqArea.ListLayout.Items = [tag1];

        // Act
        await _areaRepository.UpdateAsync(reqArea);

        // Assert
        var updatedArea = await _context.Area.FindAsync(area.Id);
        updatedArea.Should().Be(reqArea);

    }

    public void Dispose() => GC.SuppressFinalize(this);
}
