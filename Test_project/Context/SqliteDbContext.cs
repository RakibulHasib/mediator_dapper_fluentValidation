using Microsoft.EntityFrameworkCore;
using Test_project.SqliteEntity;

namespace Test_project.Context
{
    public class SqliteDbContext:DbContext
    {
        public SqliteDbContext(DbContextOptions<SqliteDbContext> options):base(options)
        {
            
        }
        public DbSet<UserLogInfo> UserLogInfo { get; set; }
    }
}
