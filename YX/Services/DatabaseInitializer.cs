using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using YX.Data;

namespace YX.Services
{
    public class DatabaseInitializer : IDatabaseInitializer
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DatabaseInitializer> _logger;

        public DatabaseInitializer(IServiceProvider serviceProvider, ILogger<DatabaseInitializer> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public void Initialize()
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<MotorDbContext>();
                db.Database.EnsureCreated();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize database");
            }
        }
    }
}
