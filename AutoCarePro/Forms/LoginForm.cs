using System;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.Text;
using AutoCarePro.Models;
using AutoCarePro.Services;
using AutoCarePro.UI;

namespace AutoCarePro.Forms
{
    /// <summary>
    /// LoginForm class represents the main login interface for the AutoCarePro application.
    /// This form provides a user-friendly interface for users to:
    /// 1. Log in to their existing account
    /// 2. Access the registration form to create a new account
    /// 3. Reset their password if forgotten
    /// 4. Enable "Remember Me" for automatic login
    /// 
    /// The form includes features such as:
    /// - Secure password handling with SHA256 hashing
    /// - Session management for persistent login
    /// - Input validation
    /// - Error handling and user feedback
    /// </summary>
    public partial class LoginForm : Form
    {
        // Service for database operations - handles user authentication and data persistence
        private readonly DatabaseService _dbService = new DatabaseService();
        // Flag to track authentication status - used to prevent multiple login attempts
        private bool _isAuthenticated = false;

        /// <summary>
        /// Constructor initializes the login form and checks for saved sessions.
        /// This is called when creating a new instance of the login form.
        /// It sets up the database service, initializes the form layout,
        /// and checks if there's a saved session for automatic login.
        /// </summary>
        public LoginForm()
        {
            InitializeComponent();
            SetupForm();
            CheckForSavedSession();
        }

        /// <summary>
        /// Checks for a saved user session and automatically logs in if found.
        /// This method is called when the form loads to provide a seamless login experience
        /// for returning users who chose "Remember Me" in their previous session.
        /// </summary>
        private void CheckForSavedSession()
        {
            // Load any saved session data
            var session = SessionManager.LoadSession();
            if (session != null)
            {
                // Verify the user still exists in the database
                var user = _dbService.GetUserById(session.UserId);
                if (user != null)
                {
                    // Update authentication status and last login time
                    _isAuthenticated = true;
                    user.LastLoginDate = DateTime.Now;
                    _dbService.UpdateUser(user);

                    // Open the dashboard and handle form closing
                    this.Hide();
                    var dashboard = new DashboardForm(user);
                    dashboard.FormClosed += (s, e) => this.Close();
                    dashboard.Show();
                }
                else
                {
                    // If user no longer exists, delete the saved session
                    SessionManager.DeleteSession();
                }
            }
        }

        /// <summary>
        /// Sets up the login form with all necessary controls and event handlers.
        /// This method creates and configures all UI elements including:
        /// - Form properties (size, position, style)
        /// - Username and password input fields
        /// - Remember me checkbox
        /// - Login and registration buttons
        /// - Forgot password link
        /// </summary>
        private void SetupForm()
        {
            // Apply form styling
            UIStyles.ApplyFormStyle(this);
            this.Text = "AutoCarePro - Login";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Size = new System.Drawing.Size(500, 400);

            // Create and configure the title label
            var lblTitle = new Label
            {
                Text = "Welcome to AutoCarePro",
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 50
            };
            UIStyles.ApplyLabelStyle(lblTitle, true);

            // Create username input field with label
            var lblUsername = new Label
            {
                Text = "Username:",
                Location = new System.Drawing.Point(50, 80),
                AutoSize = true
            };
            UIStyles.ApplyLabelStyle(lblUsername);

            var txtUsername = new TextBox
            {
                Name = "txtUsername",
                Location = new System.Drawing.Point(50, 100),
                Size = new System.Drawing.Size(280, 25)
            };
            UIStyles.ApplyTextBoxStyle(txtUsername);

            // Create password input field with label
            var lblPassword = new Label
            {
                Text = "Password:",
                Location = new System.Drawing.Point(50, 130),
                AutoSize = true
            };
            UIStyles.ApplyLabelStyle(lblPassword);

            var txtPassword = new TextBox
            {
                Name = "txtPassword",
                Location = new System.Drawing.Point(50, 150),
                Size = new System.Drawing.Size(280, 25),
                PasswordChar = '•'
            };
            UIStyles.ApplyTextBoxStyle(txtPassword);

            // Create forgot password link
            var lblForgotPassword = new LinkLabel
            {
                Text = "Forgot Password?",
                Location = new System.Drawing.Point(50, 180),
                AutoSize = true
            };
            UIStyles.ApplyLinkLabelStyle(lblForgotPassword);

            // Create remember me checkbox
            var chkRememberMe = new CheckBox
            {
                Text = "Remember me",
                Location = new System.Drawing.Point(50, 210),
                AutoSize = true
            };
            UIStyles.ApplyCheckBoxStyle(chkRememberMe);

            // Create and configure the login button
            var btnLogin = new Button
            {
                Text = "Login",
                Location = new System.Drawing.Point(50, 240),
                Size = new System.Drawing.Size(280, 35)
            };
            UIStyles.ApplyButtonStyle(btnLogin, true);

            // Create and configure the register button
            var btnRegister = new Button
            {
                Text = "Register New Account",
                Location = new System.Drawing.Point(50, 285),
                Size = new System.Drawing.Size(280, 35)
            };
            UIStyles.ApplyButtonStyle(btnRegister);

            // Add event handlers for buttons and links
            btnLogin.Click += (s, e) => HandleLogin(txtUsername.Text, txtPassword.Text, chkRememberMe.Checked);
            btnRegister.Click += (s, e) => OpenRegistrationForm();
            lblForgotPassword.LinkClicked += (s, e) => OpenPasswordResetForm();

            // Add all controls to the form
            this.Controls.AddRange(new Control[] { 
                lblTitle, lblUsername, txtUsername, lblPassword, txtPassword,
                lblForgotPassword, chkRememberMe, btnLogin, btnRegister
            });
        }

        /// <summary>
        /// Handles the login process, including validation and authentication.
        /// This method is called when the user clicks the Login button.
        /// It performs several steps:
        /// 1. Validates that username and password are provided
        /// 2. Hashes the password for secure comparison
        /// 3. Attempts to authenticate the user
        /// 4. Updates the user's last login time
        /// 5. Handles session management if "Remember Me" is checked
        /// 6. Opens the dashboard on successful login
        /// </summary>
        /// <param name="username">The username entered by the user</param>
        /// <param name="password">The password entered by the user</param>
        /// <param name="rememberMe">Whether to remember the user's session for automatic login</param>
        private void HandleLogin(string username, string password, bool rememberMe)
        {
            // Validate that both username and password are provided
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Please enter both username and password.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Hash password for secure comparison
                var hashedPassword = HashPassword(password);
                Console.WriteLine($"Attempting login for user: {username}");
                Console.WriteLine($"Hashed password: {hashedPassword}");

                var user = _dbService.AuthenticateUser(username, hashedPassword);
                Console.WriteLine($"Authentication result: {(user != null ? "Success" : "Failed")}");

                if (user != null)
                {
                    // Seed test data for test user
                    if (username == "test" && password == "test123")
                    {
                        Console.WriteLine("Test user detected, seeding test data...");
                        var seeder = new DataSeeder(_dbService);
                        seeder.SeedTestData(user.Id);
                        Console.WriteLine("Test data seeded successfully");
                    }

                    // Update authentication status and last login time
                    _isAuthenticated = true;
                    user.LastLoginDate = DateTime.Now;
                    _dbService.UpdateUser(user);

                    // Save session if "Remember Me" is checked
                    if (rememberMe)
                    {
                        SessionManager.SaveSession(user, true);
                    }

                    // Open the appropriate dashboard based on user type
                    this.Hide();
                    Form dashboard;
                    switch (user.Type)
                    {
                        case UserType.CarOwner:
                            dashboard = new CarOwnerDashboardForm(user);
                            break;
                        case UserType.MaintenanceCenter:
                            dashboard = new MaintenanceCenterDashboardForm(user);
                            break;
                        default:
                            MessageBox.Show("Unsupported user type.", "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                    }
                    dashboard.FormClosed += (s, e) => this.Close();
                    dashboard.Show();
                }
                else
                {
                    MessageBox.Show("Invalid username or password.", "Authentication Failed",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Opens the registration form for new user sign-up.
        /// This method is called when the user clicks the Register button.
        /// It hides the login form and shows the registration form,
        /// with a handler to show the login form again when registration is closed.
        /// </summary>
        private void OpenRegistrationForm()
        {
            var registerForm = new RegisterForm();
            registerForm.FormClosed += (s, e) => this.Show();
            this.Hide();
            registerForm.Show();
        }

        /// <summary>
        /// Opens the password reset form for users who forgot their password.
        /// This method is called when the user clicks the Forgot Password link.
        /// It hides the login form and shows the password reset form,
        /// with a handler to show the login form again when password reset is closed.
        /// </summary>
        private void OpenPasswordResetForm()
        {
            var resetForm = new PasswordResetForm();
            resetForm.FormClosed += (s, e) => this.Show();
            this.Hide();
            resetForm.Show();
        }

        /// <summary>
        /// Hashes a password using SHA256 for secure storage and comparison.
        /// This method is used to ensure passwords are never stored in plain text.
        /// </summary>
        /// <param name="password">The plain text password to hash</param>
        /// <returns>The hashed password as a base64 string</returns>
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        /// <summary>
        /// Handles form closing event to ensure proper application exit
        /// </summary>
        /// <param name="e">Form closing event arguments</param>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (!_isAuthenticated)
            {
                Application.Exit();
            }
            base.OnFormClosing(e);
        }
    }
}
