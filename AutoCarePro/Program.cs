using System;
using System.Windows.Forms;
using AutoCarePro.Services;
using AutoCarePro.Forms;

namespace AutoCarePro
{
    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                // Initialize database
                var dbInitializer = new DatabaseInitializer();
                dbInitializer.Initialize();

                // Seed database
                var dbSeeder = new DatabaseSeeder();
                dbSeeder.Seed(db);

                // Start with login form
                Application.Run(new LoginForm());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error starting application: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }


        }
    }
}