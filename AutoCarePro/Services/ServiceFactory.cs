using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace AutoCarePro.Services
{
    public static class ServiceFactory
    {
        private static ILoggerFactory? _loggerFactory;
        private static DatabaseService? _databaseService;

        public static void Initialize(ILoggerFactory? loggerFactory = null)
        {
            _loggerFactory = loggerFactory ?? new NullLoggerFactory();
        }

        public static DatabaseService GetDatabaseService()
        {
            if (_databaseService == null)
            {
                var logger = _loggerFactory?.CreateLogger<DatabaseService>() ?? 
                           NullLogger<DatabaseService>.Instance;
                _databaseService = new DatabaseService(logger);
            }
            return _databaseService;
        }
    }
} 