using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using Test_project.Entity;

namespace Test_project.Context
{
    public class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }
        public DbSet<Employee> Employee { get; set; }

        public SqlConnection CreateConnection() => (SqlConnection)Database.GetDbConnection();

        //private readonly IConfiguration _configuration;
        //private readonly string _connectionString;
        //public TestDbContext(IConfiguration configuration)
        //{
        //    _configuration = configuration;
        //    this._connectionString = _configuration.GetConnectionString("DefaultConnection");
        //}

        //public IDbConnection CreateConnection() => new SqlConnection(_connectionString);
    }
}
