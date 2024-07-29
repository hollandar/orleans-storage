using Microsoft.EntityFrameworkCore;

namespace Orleans.NpgsqlTenancy;

public class StateDbContext : GrainStoreDbContext
{
    public StateDbContext(DbContextOptions options):base(options)
    {
    }
}
