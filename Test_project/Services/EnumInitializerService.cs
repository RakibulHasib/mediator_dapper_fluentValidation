using Microsoft.EntityFrameworkCore;
using Test_project.Context;
using Test_project.Entity;
using Test_project.EnumList;

namespace Test_project.Services
{
    public class EnumInitializerService : IHostedService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        public EnumInitializerService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }


        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var scope = _scopeFactory.CreateScope();
            var _context = scope.ServiceProvider.GetRequiredService<TestDbContext>();
            List<PermissionTbl> enumList = Enum.GetValues(typeof(PermissionList))
                .Cast<PermissionList>()
                .Select(permission => new PermissionTbl
                {
                    PermissionId = (int)permission,
                    PermissionName = permission.ToString()
                })
                .ToList();

            var existingPermissions = (await _context.PermissionTbl
                .Where(a => enumList.Select(a => a.PermissionName).Contains(a.PermissionName))
                .Select(a => a.PermissionName)
                .ToListAsync());

            var newPermissions = enumList.Where(a => !existingPermissions.Contains(a.PermissionName)).ToList();
            if (newPermissions.Count != 0)
            {
                _context.PermissionTbl.AddRange(newPermissions);
                await _context.SaveChangesAsync();
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            var scope = _scopeFactory.CreateScope();
            var _context = scope.ServiceProvider.GetRequiredService<TestDbContext>();
            if (_context != null)
            {
                await _context.DisposeAsync();
            }
        }
    }
}
