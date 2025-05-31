using System;
using System.Windows.Forms;
using System.Drawing;
using AutoCarePro.Models;
using AutoCarePro.Services;
using AutoCarePro.UI;

namespace AutoCarePro.Forms
{
    public partial class ServiceForm : BaseForm
    {
        private readonly DatabaseService _dbService;
        private readonly User _currentUser;
        private readonly Service _service;
        private readonly bool _isEditMode;
        private TextBox _nameTextBox;
        private ComboBox _categoryComboBox;
        private TextBox _descriptionTextBox;
        private NumericUpDown _priceNumeric;
        private NumericUpDown _durationNumeric;
        private CheckBox _isAvailableCheckBox;
        private Button _saveButton;
        private Button _cancelButton;

        public ServiceForm(User user, Service? service = null)
        {
            InitializeComponent();
            _currentUser = user;
            _service = service ?? new Service { ServiceProviderId = user.Id };
            _isEditMode = service != null;
            _dbService = new DatabaseService();
            InitializeUI();
            if (_isEditMode)
            {
                LoadServiceData();
            }
        }

        private void InitializeUI()
        {
            this.Text = _isEditMode ? "Edit Service" : "Add Service";
            this.Size = new Size(500, 500);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Create form layout
            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                ColumnCount = 2,
                RowCount = 6
            };

            // Add controls
            mainPanel.Controls.Add(new Label { Text = "Name:", AutoSize = true }, 0, 0);
            _nameTextBox = new TextBox { Width = 300 };
            mainPanel.Controls.Add(_nameTextBox, 1, 0);

            mainPanel.Controls.Add(new Label { Text = "Category:", AutoSize = true }, 0, 1);
            _categoryComboBox = new ComboBox
            {
                Width = 300,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            mainPanel.Controls.Add(_categoryComboBox, 1, 1);

            mainPanel.Controls.Add(new Label { Text = "Description:", AutoSize = true }, 0, 2);
            _descriptionTextBox = new TextBox
            {
                Width = 300,
                Multiline = true,
                Height = 60
            };
            mainPanel.Controls.Add(_descriptionTextBox, 1, 2);

            mainPanel.Controls.Add(new Label { Text = "Price:", AutoSize = true }, 0, 3);
            _priceNumeric = new NumericUpDown
            {
                Width = 300,
                Maximum = 10000,
                DecimalPlaces = 2,
                Value = 0
            };
            mainPanel.Controls.Add(_priceNumeric, 1, 3);

            mainPanel.Controls.Add(new Label { Text = "Duration (minutes):", AutoSize = true }, 0, 4);
            _durationNumeric = new NumericUpDown
            {
                Width = 300,
                Maximum = 480,
                Minimum = 15,
                Value = 60
            };
            mainPanel.Controls.Add(_durationNumeric, 1, 4);

            mainPanel.Controls.Add(new Label { Text = "Available:", AutoSize = true }, 0, 5);
            _isAvailableCheckBox = new CheckBox
            {
                AutoSize = true,
                Checked = true
            };
            mainPanel.Controls.Add(_isAvailableCheckBox, 1, 5);

            // Add buttons
            var buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                FlowDirection = FlowDirection.RightToLeft,
                Height = 50,
                Padding = new Padding(0, 10, 0, 0)
            };

            _cancelButton = new Button
            {
                Text = "Cancel",
                DialogResult = DialogResult.Cancel,
                Width = 80
            };
            UIStyles.ApplyButtonStyle(_cancelButton);

            _saveButton = new Button
            {
                Text = "Save",
                Width = 80
            };
            UIStyles.ApplyButtonStyle(_saveButton, true);

            buttonPanel.Controls.Add(_cancelButton);
            buttonPanel.Controls.Add(_saveButton);

            // Add event handlers
            _saveButton.Click += SaveButton_Click;

            // Add panels to form
            this.Controls.Add(mainPanel);
            this.Controls.Add(buttonPanel);

            // Set accept and cancel buttons
            this.AcceptButton = _saveButton;
            this.CancelButton = _cancelButton;

            // Initialize controls
            InitializeControls();
        }

        private void InitializeControls()
        {
            // Load service categories
            _categoryComboBox.Items.AddRange(new string[] {
                "Maintenance",
                "Repair",
                "Diagnostic",
                "Cleaning",
                "Inspection",
                "Other"
            });
            _categoryComboBox.SelectedIndex = 0;
        }

        private void LoadServiceData()
        {
            _nameTextBox.Text = _service.Name;
            _categoryComboBox.Text = _service.Category;
            _descriptionTextBox.Text = _service.Description;
            _priceNumeric.Value = _service.Price;
            _durationNumeric.Value = _service.DurationMinutes;
            _isAvailableCheckBox.Checked = _service.IsAvailable;
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_nameTextBox.Text) ||
                    string.IsNullOrWhiteSpace(_descriptionTextBox.Text) ||
                    string.IsNullOrWhiteSpace(_categoryComboBox.Text) ||
                    !decimal.TryParse(_priceNumeric.Text, out decimal price) ||
                    !int.TryParse(_durationNumeric.Text, out int durationMinutes))
                {
                    MessageBox.Show("Please fill in all required fields with valid values.", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var service = new Service
                {
                    Name = _nameTextBox.Text.Trim(),
                    Description = _descriptionTextBox.Text.Trim(),
                    Price = price,
                    Category = _categoryComboBox.Text.Trim(),
                    DurationMinutes = durationMinutes,
                    IsAvailable = _isAvailableCheckBox.Checked,
                    ServiceProviderId = _currentUser.Id
                };

                if (_isEditMode)
                {
                    service.Id = _service.Id;
                    service.UpdatedAt = DateTime.UtcNow;
                    _dbService.UpdateService(service);
                }
                else
                {
                    _dbService.AddService(service);
                }

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving service: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            _dbService.Dispose();
        }
    }
} 