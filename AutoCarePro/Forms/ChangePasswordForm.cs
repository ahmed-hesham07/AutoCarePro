using System;
using System.Windows.Forms;
using AutoCarePro.Models;
using AutoCarePro.Services;
using AutoCarePro.UI;

namespace AutoCarePro.Forms
{
    /// <summary>
    /// Form that allows users to securely change their account password.
    /// This form provides a user-friendly interface for:
    /// 1. Verifying the current password
    /// 2. Setting a new password with confirmation
    /// 3. Validating password requirements
    /// 
    /// Security features include:
    /// - Password masking for privacy
    /// - Current password verification
    /// - Password confirmation to prevent typos
    /// - Minimum password length requirement
    /// - Secure password update in database
    /// </summary>
    public partial class ChangePasswordForm : Form
    {
        // Core data fields for password management
        private readonly User _user;                // User whose password is being changed
        private readonly DatabaseService _dbService = new DatabaseService(); // Service for database operations

        /// <summary>
        /// Initializes the password change form for a specific user.
        /// This constructor sets up the form and applies the current theme.
        /// </summary>
        /// <param name="user">The user whose password is being changed</param>
        public ChangePasswordForm(User user)
        {
            InitializeComponent();
            _user = user;
            SetupForm();
            ThemeManager.Instance.ApplyTheme(this);
            ThemeManager.Instance.ThemeChanged += (s, e) => ThemeManager.Instance.ApplyTheme(this);
        }

        /// <summary>
        /// Sets up the password change form with all necessary controls and event handlers.
        /// This method creates a structured layout with password input fields and action buttons.
        /// The form uses the application's theme system for consistent styling.
        /// </summary>
        private void SetupForm()
        {
            // Configure main form properties
            this.Text = "AutoCarePro - Change Password";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Size = new System.Drawing.Size(400, 400);
            this.Padding = ThemeManager.Sizes.FormPadding;

            // Create title label with theme-consistent styling
            var lblTitle = new Label
            {
                Text = "Change Password",
                Font = ThemeManager.Fonts.Title,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 50
            };

            // Create current password field with password masking
            var lblCurrentPassword = new Label
            {
                Text = "Current Password:",
                Font = ThemeManager.Fonts.Normal,
                Location = new System.Drawing.Point(0, 80),
                AutoSize = true
            };

            var txtCurrentPassword = new TextBox
            {
                Name = "txtCurrentPassword",
                Location = new System.Drawing.Point(0, 100),
                Size = ThemeManager.Sizes.TextBox,
                PasswordChar = '•'  // Mask password characters for security
            };

            // Create new password field with password masking
            var lblNewPassword = new Label
            {
                Text = "New Password:",
                Font = ThemeManager.Fonts.Normal,
                Location = new System.Drawing.Point(0, 140),
                AutoSize = true
            };

            var txtNewPassword = new TextBox
            {
                Name = "txtNewPassword",
                Location = new System.Drawing.Point(0, 160),
                Size = ThemeManager.Sizes.TextBox,
                PasswordChar = '•'  // Mask password characters for security
            };

            // Create confirm password field with password masking
            var lblConfirmPassword = new Label
            {
                Text = "Confirm New Password:",
                Font = ThemeManager.Fonts.Normal,
                Location = new System.Drawing.Point(0, 200),
                AutoSize = true
            };

            var txtConfirmPassword = new TextBox
            {
                Name = "txtConfirmPassword",
                Location = new System.Drawing.Point(0, 220),
                Size = ThemeManager.Sizes.TextBox,
                PasswordChar = '•'  // Mask password characters for security
            };

            // Create change password button with theme styling
            var btnChange = new Button
            {
                Text = "Change Password",
                Location = new System.Drawing.Point(0, 270),
                Size = ThemeManager.Sizes.Button,
                BackColor = ThemeManager.Colors.Primary,
                ForeColor = System.Drawing.Color.White,
                FlatStyle = FlatStyle.Flat
            };

            // Create cancel button with theme styling
            var btnCancel = new Button
            {
                Text = "Cancel",
                Location = new System.Drawing.Point(0, 320),
                Size = ThemeManager.Sizes.Button,
                BackColor = ThemeManager.Colors.Secondary,
                FlatStyle = FlatStyle.Flat
            };

            // Add event handlers for button actions
            btnChange.Click += (s, e) => ChangePassword(
                txtCurrentPassword.Text,
                txtNewPassword.Text,
                txtConfirmPassword.Text
            );
            btnCancel.Click += (s, e) => this.Close();

            // Add all controls to the form in a specific order
            this.Controls.AddRange(new Control[] {
                lblTitle, lblCurrentPassword, txtCurrentPassword,
                lblNewPassword, txtNewPassword, lblConfirmPassword,
                txtConfirmPassword, btnChange, btnCancel
            });
        }

        /// <summary>
        /// Handles the password change process, including validation and password update.
        /// This method performs several security checks:
        /// 1. Validates that all fields are filled
        /// 2. Ensures new password and confirmation match
        /// 3. Verifies minimum password length
        /// 4. Validates current password before making changes
        /// </summary>
        /// <param name="currentPassword">The user's current password for verification</param>
        /// <param name="newPassword">The new password to set</param>
        /// <param name="confirmPassword">Confirmation of the new password to prevent typos</param>
        private void ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            try
            {
                // Validate that all required fields are filled
                if (string.IsNullOrWhiteSpace(currentPassword) ||
                    string.IsNullOrWhiteSpace(newPassword) ||
                    string.IsNullOrWhiteSpace(confirmPassword))
                {
                    MessageBox.Show("All fields are required.", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Validate that new password and confirmation match
                if (newPassword != confirmPassword)
                {
                    MessageBox.Show("New passwords do not match.", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Validate minimum password length requirement
                if (newPassword.Length < 8)
                {
                    MessageBox.Show("New password must be at least 8 characters long.", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Attempt to change password in database
                if (_dbService.ChangePassword(_user.Id, currentPassword, newPassword))
                {
                    MessageBox.Show("Password changed successfully!", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Current password is incorrect.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
} 