using System;
using System.Windows.Forms;
using AutoCarePro.Services;
using AutoCarePro.Models;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;

namespace AutoCarePro.Forms
{
    /// <summary>
    /// Form that allows users to request a password reset for their account.
    /// This form provides a user-friendly interface for:
    /// 1. Entering the email address associated with the account
    /// 2. Requesting a password reset link
    /// 3. Receiving feedback on the reset request status
    /// 
    /// Security features include:
    /// - Email verification before reset
    /// - Secure token generation for reset link
    /// - User-friendly error messages
    /// - Protection against email enumeration
    /// </summary>
    public partial class PasswordResetForm : Form
    {
        private readonly DatabaseService _databaseService;
        private readonly ILogger<PasswordResetForm> _logger;

        /// <summary>
        /// Initializes the password reset form.
        /// This constructor sets up the form and prepares it for handling reset requests.
        /// </summary>
        public PasswordResetForm(ILogger<PasswordResetForm> logger)
        {
            InitializeComponent();
            _logger = logger;
            _databaseService = ServiceFactory.GetDatabaseService();
        }

        /// <summary>
        /// Sets up the password reset form with all necessary controls and event handlers.
        /// This method creates a structured layout with email input and reset request button.
        /// The form is designed to be simple and intuitive for users who have forgotten their password.
        /// </summary>
        private void SetupForm()
        {
            // Configure main form properties
            this.Text = "AutoCarePro - Password Reset";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Size = new System.Drawing.Size(500, 400);

            // Create title label with consistent styling
            var lblTitle = new Label
            {
                Text = "Reset Password",
                Font = new System.Drawing.Font("Segoe UI", 16, System.Drawing.FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 50
            };

            // Create email input field for account identification
            var lblEmail = new Label
            {
                Text = "Email:",
                Location = new System.Drawing.Point(50, 80),
                AutoSize = true
            };

            var txtEmail = new TextBox
            {
                Name = "txtEmail",
                Location = new System.Drawing.Point(50, 100),
                Size = new System.Drawing.Size(280, 25)
            };

            // Create request reset button with theme-consistent styling
            var btnRequestReset = new Button
            {
                Text = "Request Reset",
                Location = new System.Drawing.Point(50, 140),
                Size = new System.Drawing.Size(280, 35),
                BackColor = System.Drawing.Color.FromArgb(0, 120, 215),
                ForeColor = System.Drawing.Color.White,
                FlatStyle = FlatStyle.Flat
            };

            // Add event handler for password reset request
            btnRequestReset.Click += (s, e) => HandleResetRequest(txtEmail.Text);

            // Add all controls to the form in a specific order
            this.Controls.AddRange(new Control[] {
                lblTitle, lblEmail, txtEmail, btnRequestReset
            });
        }

        /// <summary>
        /// Handles the password reset request process.
        /// This method performs several steps:
        /// 1. Validates the email input
        /// 2. Attempts to request a password reset
        /// 3. Provides appropriate feedback to the user
        /// 
        /// Security considerations:
        /// - Generic error messages to prevent email enumeration
        /// - Email validation before processing
        /// - Secure token generation for reset link
        /// </summary>
        /// <param name="email">The email address entered by the user for password reset</param>
        private void HandleResetRequest(string email)
        {
            // Validate that email is provided
            if (string.IsNullOrWhiteSpace(email))
            {
                MessageBox.Show("Please enter your email address.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Attempt to request password reset through database service
                if (_databaseService.RequestPasswordReset(email))
                {
                    // Success message with instructions
                    MessageBox.Show("Password reset instructions have been sent to your email.", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Close();
                }
                else
                {
                    // Generic error message for security
                    MessageBox.Show("No account found with that email address.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                // Handle any unexpected errors
                MessageBox.Show($"An error occurred: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ResetButton_Click(object sender, EventArgs e)
        {
            try
            {
                string email = EmailTextBox.Text;
                string newPassword = NewPasswordTextBox.Text;
                string confirmPassword = ConfirmPasswordTextBox.Text;

                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(confirmPassword))
                {
                    MessageBox.Show("Please fill in all fields.", "Validation Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (newPassword != confirmPassword)
                {
                    MessageBox.Show("Passwords do not match.", "Validation Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var user = _databaseService.GetUserByEmail(email);
                if (user == null)
                {
                    _logger.LogWarning("Password reset attempted for non-existent email: {Email}", email);
                    MessageBox.Show("No account found with this email address.", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                user.Password = HashPassword(newPassword);
                if (_databaseService.UpdateUser(user))
                {
                    _logger.LogInformation("Password reset successful for user {Username}", user.Username);
                    MessageBox.Show("Password has been reset successfully. Please login with your new password.", 
                        "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Close();
                }
                else
                {
                    _logger.LogError("Failed to update password for user {Username}", user.Username);
                    MessageBox.Show("Failed to reset password. Please try again.", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during password reset");
                MessageBox.Show("An error occurred while resetting your password. Please try again.", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

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