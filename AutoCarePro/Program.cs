using System;
using System.Windows.Forms;
using AutoCarePro.Services;
using AutoCarePro.Forms;
using AutoCarePro.Data;
using System.Threading.Tasks;

namespace AutoCarePro
{
    static class Program
    {
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
                using (var dbInitializer = new DatabaseInitializer())
                {
                    await dbInitializer.InitializeAsync();
                }

                using (var dbService = new DatabaseService())
                {
                    // Seed the database with initial data
                    // This populates the database with default/sample data
                    DatabaseSeeder.Seed(dbService);

                    // Launch the application starting with the login form
                    // This is the first form users will see
                    Application.Run(new LoginForm());
                }
            }
            catch (Exception ex)
            {
                // Show full exception details including inner exceptions
                string details = ex.ToString();
                if (ex.InnerException != null)
                    details += "\n\nInner Exception:\n" + ex.InnerException.ToString();

                MessageBox.Show($"Error starting application: {details}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}