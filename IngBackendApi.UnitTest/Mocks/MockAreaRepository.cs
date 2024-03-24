namespace IngBackendApi.UnitTest.Mocks;

using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using Moq;
using IngBackendApi.Interfaces.Repository;
using IngBackendApi.Models.DBEntity;
using IngBackendApi.Repository;
using IngBackendApi.Test.Fixtures;
using IngBackendApi.Context;

internal class MockAreaRepository
{
    public static Mock<IAreaRepository> GetMock(IngDbContext context)
    {
        var areaFixture = new AreaFixture();

        var mockAreaRepository = new Mock<IAreaRepository>();
        var stubAreaRepository = new AreaRepository(context);

        SeedData(context, areaFixture); // Call the method to seed data

        // Setup mock behavior
        mockAreaRepository.Setup(m => m.AddAsync(It.IsAny<Area>()))
            .Callback(() => { return; });
        mockAreaRepository.Setup(m => m.UpdateAsync(It.IsAny<Area>()))
            .Callback(() => { return; });
        mockAreaRepository.Setup(m => m.DeleteByIdAsync(It.IsAny<Guid>()))
            .Callback(() => { return; });
        mockAreaRepository.Setup(m => m.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(context.Area.First());

        return mockAreaRepository;
    }

    // Private method to seed data
    private static void SeedData(IngDbContext context, AreaFixture areaFixture)
    {
        var area1 = areaFixture.Fixture.Create<Area>();
        var area2 = areaFixture.Fixture.Create<Area>();
        var areaType1 = areaFixture.Fixture.Create<AreaType>();
        var areaType2 = areaFixture.Fixture.Create<AreaType>();

        List<Area> areas = [area1, area2];
        List<AreaType> areaTypes = [areaType1, areaType2];

        context.AddRange(areas);
        context.AddRange(areaTypes);

        context.SaveChangesAsync(); // Make sure to await this
    }
}
