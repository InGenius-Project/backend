namespace IngBackendApi.Test.Fixtures;

using IngBackendApi.Context;
using Microsoft.EntityFrameworkCore;

public static class MemoryContextFixture
{
    public static IngDbContext Generate()
    {
        var optionBuilder = new DbContextOptionsBuilder<IngDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString());
        return new IngDbContext(optionBuilder.Options);
    }
}
