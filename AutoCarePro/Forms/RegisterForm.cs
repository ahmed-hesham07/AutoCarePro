using System;
using System.Windows.Forms;
using AutoCarePro.Models;
using AutoCarePro.Services;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;

namespace AutoCarePro.Forms
{
    /// <summary>
    /// RegisterForm class represents the user registration interface for the AutoCarePro application.
    /// This form provides a user-friendly interface for new users to:
    /// 1. Create a new account with their personal information
    /// 2. Choose their account type (Car Owner or Maintenance Center)
    /// 3. Set up secure login credentials
    /// 
    /// The form includes comprehensive validation to ensure:
    /// - All required fields are filled
    /// - Email format is valid
    /// - Passwords match
    /// - Username is unique
    /// </summary>
    public partial class RegisterForm : Form
    {
        private readonly DatabaseService _databaseService;
        private readonly ILogger<RegisterForm> _logger;

        /// <summary>
        /// Constructor initializes the registration form.
        /// This is called when creating a new instance of the registration form.
        /// It sets up the database service and initializes the form layout.
        /// </summary>
        public RegisterForm(ILogger<RegisterForm> logger)
        {
            InitializeComponent();
            _logger = logger;
            _databaseService = ServiceFactory.GetDatabaseService();
            SetupForm();
        }

        /// <summary>
        /// Sets up the registration form with all necessary controls and event handlers.
        /// This method creates and configures all UI elements including:
        /// - Form properties (size, position, style)
        /// - Input fields for user information
        /// - Password fields with secure input
        /// - User type selection
        /// - Registration button with event handling
        /// </summary>
        private void SetupForm()
        {
            // Configure basic form properties
            this.Text = "AutoCarePro - Register";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Size = new System.Drawing.Size(500, 600);

            // Create and configure the title label
            var lblTitle = new Label
            {
                Text = "Create New Account",
                Font = new System.Drawing.Font("Segoe UI", 16, System.Drawing.FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 50
            };

            // Create username input field with label
            var lblUsername = new Label
            {
                Text = "Username:",
                Location = new System.Drawing.Point(50, 80),
                AutoSize = true
            };

            var txtUsername = new TextBox
            {
                Name = "txtUsername",
                Location = new System.Drawing.Point(50, 100),
                Size = new System.Drawing.Size(280, 25)
            };

            // Create email input field with label
            var lblEmail = new Label
            {
                Text = "Email:",
                Location = new System.Drawing.Point(50, 130),
                AutoSize = true
            };

            var txtEmail = new TextBox
            {
                Name = "txtEmail",
                Location = new System.Drawing.Point(50, 150),
                Size = new System.Drawing.Size(280, 25)
            };

            // Create full name input field with label
            var lblFullName = new Label
            {
                Text = "Full Name:",
                Location = new System.Drawing.Point(50, 180),
                AutoSize = true
            };

            var txtFullName = new TextBox
            {
                Name = "txtFullName",
                Location = new System.Drawing.Point(50, 200),
                Size = new System.Drawing.Size(280, 25)
            };

            // Create phone number input field with label
            var lblPhone = new Label
            {
                Text = "Phone Number:",
                Location = new System.Drawing.Point(50, 230),
                AutoSize = true
            };

            var txtPhone = new TextBox
            {
                Name = "txtPhone",
                Location = new System.Drawing.Point(50, 250),
                Size = new System.Drawing.Size(280, 25)
            };

            // Create password input field with label
            // PasswordChar is set to '•' to mask the input for security
            var lblPassword = new Label
            {
                Text = "Password:",
                Location = new System.Drawing.Point(50, 280),
                AutoSize = true
            };

            var txtPassword = new TextBox
            {
                Name = "txtPassword",
                Location = new System.Drawing.Point(50, 300),
                Size = new System.Drawing.Size(280, 25),
                PasswordChar = '•'  // Masks the password input for security
            };

            // Create confirm password input field with label
            var lblConfirmPassword = new Label
            {
                Text = "Confirm Password:",
                Location = new System.Drawing.Point(50, 330),
                AutoSize = true
            };

            var txtConfirmPassword = new TextBox
            {
                Name = "txtConfirmPassword",
                Location = new System.Drawing.Point(50, 350),
                Size = new System.Drawing.Size(280, 25),
                PasswordChar = '•'  // Masks the password input for security
            };

            // Create user type selection dropdown with label
            var lblUserType = new Label
            {
                Text = "Account Type:",
                Location = new System.Drawing.Point(50, 380),
                AutoSize = true
            };

            var cmbUserType = new ComboBox
            {
                Name = "cmbUserType",
                Location = new System.Drawing.Point(50, 400),
                Size = new System.Drawing.Size(280, 25),
                DropDownStyle = ComboBoxStyle.DropDownList  // Prevents user from typing in the combo box
            };
            // Add user type options to the dropdown
            cmbUserType.Items.AddRange(Enum.GetNames(typeof(UserType)));
            cmbUserType.SelectedIndex = 0;  // Select first item by default

            // Create and configure the register button
            var btnRegister = new Button
            {
                Text = "Register",
                Location = new System.Drawing.Point(50, 440),
                Size = new System.Drawing.Size(280, 35),
                BackColor = System.Drawing.Color.FromArgb(0, 120, 215),  // Modern blue color
                ForeColor = System.Drawing.Color.White,
                FlatStyle = FlatStyle.Flat
            };

            // Add event handler for the register button click
            // This uses a lambda expression to handle the click event
            btnRegister.Click += (s, e) => HandleRegistration(
                txtUsername.Text,
                txtEmail.Text,
                txtFullName.Text,
                txtPhone.Text,
                txtPassword.Text,
                txtConfirmPassword.Text,
                (UserType)cmbUserType.SelectedIndex
            );

            // Add all controls to the form in a specific order
            this.Controls.AddRange(new Control[] {
                lblTitle, lblUsername, txtUsername, lblEmail, txtEmail,
                lblFullName, txtFullName, lblPhone, txtPhone,
                lblPassword, txtPassword, lblConfirmPassword, txtConfirmPassword,
                lblUserType, cmbUserType, btnRegister
            });
        }

        /// <summary>
        /// Handles the registration process, including validation and user creation.
        /// This method is called when the user clicks the Register button.
        /// It performs several validation steps before attempting to create the user account:
        /// 1. Validates that all required fields are filled
        /// 2. Ensures passwords match
        /// 3. Validates email format
        /// 4. Creates and saves the user account if all validations pass
        /// </summary>
        /// <param name="username">The username entered by the user - must be unique</param>
        /// <param name="email">The email address entered by the user - must be valid format</param>
        /// <param name="fullName">The full name entered by the user</param>
        /// <param name="phone">The phone number entered by the user - optional</param>
        /// <param name="password">The password entered by the user</param>
        /// <param name="confirmPassword">The password confirmation entered by the user - must match password</param>
        /// <param name="userType">The type of user account being created (Car Owner or Maintenance Center)</param>
        private void HandleRegistration(string username, string email, string fullName, string phone,
            string password, string confirmPassword, UserType userType)
        {
            try
            {
                if (!ValidateInputs())
                {
                    return;
                }

                var user = new User
                {
                    Username = username,
                    Email = email,
                    FullName = fullName,
                    PhoneNumber = phone,
                    Password = HashPassword(password),
                    Type = userType,
                    CreatedDate = DateTime.Now
                };

                if (_databaseService.RegisterUser(user))
                {
                    _logger.LogInformation("User {Username} registered successfully", user.Username);
                    MessageBox.Show("Registration successful! Please login.", "Success", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Close();
                }
                else
                {
                    _logger.LogWarning("Failed to register user {Username}", user.Username);
                    MessageBox.Show("Registration failed. Username or email may already exist.", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration");
                MessageBox.Show("An error occurred during registration. Please try again.", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidateInputs()
        {
            if (string.IsNullOrEmpty(txtUsername.Text) ||
                string.IsNullOrEmpty(txtEmail.Text) ||
                string.IsNullOrEmpty(txtFullName.Text) ||
                string.IsNullOrEmpty(txtPhone.Text) ||
                string.IsNullOrEmpty(txtPassword.Text) ||
                string.IsNullOrEmpty(txtConfirmPassword.Text))
            {
                MessageBox.Show("Please fill in all fields.", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (txtPassword.Text != txtConfirmPassword.Text)
            {
                MessageBox.Show("Passwords do not match.", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!IsValidEmail(txtEmail.Text))
            {
                MessageBox.Show("Please enter a valid email address.", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validates an email address format
        /// </summary>
        /// <param name="email">The email address to validate</param>
        /// <returns>True if the email is valid, false otherwise</returns>
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Hashes a password using SHA256
        /// </summary>
        /// <param name="password">The password to hash</param>
        /// <returns>The hashed password as a base64 string</returns>
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }
    }
} 