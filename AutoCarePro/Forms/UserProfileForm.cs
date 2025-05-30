using System;
using System.Windows.Forms;
using AutoCarePro.Models;
using AutoCarePro.Services;

namespace AutoCarePro.Forms
{
    /// <summary>
    /// Form that allows users to manage their profile information in the AutoCarePro application.
    /// This form provides a user-friendly interface for:
    /// 1. Viewing and updating personal information (email, full name, phone number)
    /// 2. Changing account password
    /// 3. Viewing account type and username (read-only)
    /// 
    /// The form includes:
    /// - Input validation for all fields
    /// - Theme support for consistent UI appearance
    /// - Error handling for database operations
    /// - Secure password change functionality
    /// </summary>
    public partial class UserProfileForm : Form
    {
        // Core data fields for managing user profile
        private readonly User _user;                    // User whose profile is being managed
        private readonly DatabaseService _dbService = new DatabaseService();    // Service for database operations
        private readonly DataValidationService _validationService; // Service for data validation

        /// <summary>
        /// Initializes the user profile form for a specific user.
        /// This constructor sets up the form, loads user data, and applies the current theme.
        /// </summary>
        /// <param name="user">The user whose profile is being managed</param>
        public UserProfileForm(User user)
        {
            InitializeComponent();
            _user = user;
            _validationService = new DataValidationService();
            SetupForm();
            LoadUserData();
            ThemeManager.Instance.ApplyTheme(this);
            ThemeManager.Instance.ThemeChanged += (s, e) => ThemeManager.Instance.ApplyTheme(this);
        }

        /// <summary>
        /// Sets up the user profile form with all necessary controls and event handlers.
        /// This method creates a structured layout with input fields, labels, and action buttons.
        /// The form uses the application's theme system for consistent styling.
        /// </summary>
        private void SetupForm()
        {
            // Configure main form properties
            this.Text = "AutoCarePro - User Profile";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Size = new System.Drawing.Size(500, 600);
            this.Padding = ThemeManager.Sizes.FormPadding;

            // Create title label with theme-consistent styling
            var lblTitle = new Label
            {
                Text = "User Profile",
                Font = ThemeManager.Fonts.Title,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 50
            };

            // Create username field (read-only)
            var lblUsername = new Label
            {
                Text = "Username:",
                Font = ThemeManager.Fonts.Normal,
                Location = new System.Drawing.Point(0, 80),
                AutoSize = true
            };

            var txtUsername = new TextBox
            {
                Name = "txtUsername",
                Location = new System.Drawing.Point(0, 100),
                Size = ThemeManager.Sizes.TextBox,
                ReadOnly = true  // Username cannot be changed
            };

            // Create email field for contact information
            var lblEmail = new Label
            {
                Text = "Email:",
                Font = ThemeManager.Fonts.Normal,
                Location = new System.Drawing.Point(0, 140),
                AutoSize = true
            };

            var txtEmail = new TextBox
            {
                Name = "txtEmail",
                Location = new System.Drawing.Point(0, 160),
                Size = ThemeManager.Sizes.TextBox
            };

            // Create full name field for user identification
            var lblFullName = new Label
            {
                Text = "Full Name:",
                Font = ThemeManager.Fonts.Normal,
                Location = new System.Drawing.Point(0, 200),
                AutoSize = true
            };

            var txtFullName = new TextBox
            {
                Name = "txtFullName",
                Location = new System.Drawing.Point(0, 220),
                Size = ThemeManager.Sizes.TextBox
            };

            // Create phone number field for contact information
            var lblPhone = new Label
            {
                Text = "Phone Number:",
                Font = ThemeManager.Fonts.Normal,
                Location = new System.Drawing.Point(0, 260),
                AutoSize = true
            };

            var txtPhone = new TextBox
            {
                Name = "txtPhone",
                Location = new System.Drawing.Point(0, 280),
                Size = ThemeManager.Sizes.TextBox
            };

            // Create account type display (read-only)
            var lblAccountType = new Label
            {
                Text = "Account Type:",
                Font = ThemeManager.Fonts.Normal,
                Location = new System.Drawing.Point(0, 320),
                AutoSize = true
            };

            var lblAccountTypeValue = new Label
            {
                Name = "lblAccountTypeValue",
                Font = ThemeManager.Fonts.Normal,
                Location = new System.Drawing.Point(0, 340),
                Size = ThemeManager.Sizes.TextBox,
                Text = _user.Type.ToString()
            };

            // Create change password button with theme styling
            var btnChangePassword = new Button
            {
                Text = "Change Password",
                Location = new System.Drawing.Point(0, 380),
                Size = ThemeManager.Sizes.Button,
                BackColor = ThemeManager.Colors.Primary,
                ForeColor = System.Drawing.Color.White,
                FlatStyle = FlatStyle.Flat
            };

            // Create save changes button with theme styling
            var btnSave = new Button
            {
                Text = "Save Changes",
                Location = new System.Drawing.Point(0, 430),
                Size = ThemeManager.Sizes.Button,
                BackColor = ThemeManager.Colors.Primary,
                ForeColor = System.Drawing.Color.White,
                FlatStyle = FlatStyle.Flat
            };

            // Create cancel button with theme styling
            var btnCancel = new Button
            {
                Text = "Cancel",
                Location = new System.Drawing.Point(0, 480),
                Size = ThemeManager.Sizes.Button,
                BackColor = ThemeManager.Colors.Secondary,
                FlatStyle = FlatStyle.Flat
            };

            // Add event handlers for button actions
            btnSave.Click += (s, e) => SaveChanges(txtEmail.Text, txtFullName.Text, txtPhone.Text);
            btnCancel.Click += (s, e) => this.Close();
            btnChangePassword.Click += (s, e) => OpenChangePasswordForm();

            // Add all controls to the form in a specific order
            this.Controls.AddRange(new Control[] {
                lblTitle, lblUsername, txtUsername, lblEmail, txtEmail,
                lblFullName, txtFullName, lblPhone, txtPhone,
                lblAccountType, lblAccountTypeValue,
                btnChangePassword, btnSave, btnCancel
            });
        }

        /// <summary>
        /// Loads the current user's data into the form controls.
        /// This method populates all input fields with the user's existing information.
        /// </summary>
        private void LoadUserData()
        {
            // Find and populate each input control with user data
            var username = this.Controls.Find("txtUsername", true)[0] as TextBox;
            var email = this.Controls.Find("txtEmail", true)[0] as TextBox;
            var fullName = this.Controls.Find("txtFullName", true)[0] as TextBox;
            var phone = this.Controls.Find("txtPhone", true)[0] as TextBox;

            username.Text = _user.Username;
            email.Text = _user.Email;
            fullName.Text = _user.FullName;
            phone.Text = _user.PhoneNumber;
        }

        /// <summary>
        /// Saves the changes made to the user's profile information.
        /// This method validates the input data and updates the user's profile in the database.
        /// </summary>
        /// <param name="email">The updated email address</param>
        /// <param name="fullName">The updated full name</param>
        /// <param name="phone">The updated phone number</param>
        private void SaveChanges(string email, string fullName, string phone)
        {
            try
            {
                // Update user object with new values
                _user.Email = email;
                _user.FullName = fullName;
                _user.PhoneNumber = phone;

                // Validate the updated user data
                var validationResult = _validationService.ValidateUser(_user);
                if (!validationResult.IsValid)
                {
                    MessageBox.Show(validationResult.GetErrorMessage(), "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Save changes to database
                _dbService.UpdateUser(_user);
                MessageBox.Show("Profile updated successfully!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Opens the change password form for the current user.
        /// This method hides the current form and shows the password change form.
        /// When the password change form is closed, this form is shown again.
        /// </summary>
        private void OpenChangePasswordForm()
        {
            var changePasswordForm = new ChangePasswordForm(_user);
            changePasswordForm.FormClosed += (s, e) => this.Show();
            this.Hide();
            changePasswordForm.Show();
        }
    }
} 