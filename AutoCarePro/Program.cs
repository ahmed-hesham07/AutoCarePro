using System;
using System.Windows.Forms;
using AutoCarePro.Services;
using AutoCarePro.Forms;
using AutoCarePro.Data;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace AutoCarePro
{
    static class Program
    {
        private static ILoggerFactory? _loggerFactory;

        /// <summary>
        /// The main entry point for the application.
        /// This is where the program starts executing.
        /// </summary>
        [STAThread] // Indicates this is a Single-Threaded Apartment model application
        static async Task Main()
        {
            // Enable visual styles for modern Windows appearance
            Application.EnableVisualStyles();
            // Set text rendering to be compatible with older Windows versions
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                // Initialize logging
                _loggerFactory = LoggerFactory.Create(builder =>
                {
                    builder
                        .SetMinimumLevel(LogLevel.Information)
                        .AddConsole()
                        .AddDebug();
                });

                // Initialize ServiceFactory
                ServiceFactory.Initialize(_loggerFactory);
                SessionManager.Initialize(_loggerFactory.CreateLogger<SessionManager>());

                using (var dbInitializer = new DatabaseInitializer())
                {
                    await dbInitializer.InitializeAsync();
                    LoggingService.LogInfo("Database initialized successfully");
                }

                using (var dbService = new DatabaseService(CreateLogger<DatabaseService>()))
                {
                    // Seed the database with initial data
                    // This populates the database with default/sample data
                    DatabaseSeeder.Seed(dbService);
                    LoggingService.LogInfo("Database seeded successfully");

                    // Launch the application starting with the login form
                    // This is the first form users will see
                    var loginForm = new LoginForm(CreateLogger<LoginForm>());
                    Application.Run(loginForm);

                    // If login was successful, show the dashboard
                    if (loginForm.IsAuthenticated)
                    {
                        var dashboardForm = new UnifiedDashboardForm(loginForm.CurrentUser, CreateLogger<UnifiedDashboardForm>());
                        Application.Run(dashboardForm);
                    }
                }
            }
            catch (Exception ex)
            {
                LoggingService.LogError("Critical error during application startup", ex);
                new ErrorForm(
                    "A critical error occurred while starting the application. Please contact support if this persists.",
                    "Startup Error",
                    ex
                ).ShowDialog();
            }
        }

        public static ILogger<T> CreateLogger<T>()
        {
            return _loggerFactory.CreateLogger<T>();
        }
    }
}