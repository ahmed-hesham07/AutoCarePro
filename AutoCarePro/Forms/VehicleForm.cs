using System;
using System.Windows.Forms;
using System.Drawing;
using AutoCarePro.Models;
using AutoCarePro.Services;
using AutoCarePro.UI;

namespace AutoCarePro.Forms
{
    public partial class VehicleForm : BaseForm
    {
        private readonly DatabaseService _dbService;
        private readonly User _currentUser;
        private readonly Vehicle _vehicle;
        private readonly bool _isEditMode;
        private TextBox _makeTextBox = null!;
        private TextBox _modelTextBox = null!;
        private NumericUpDown _yearNumeric = null!;
        private TextBox _vinTextBox = null!;
        private NumericUpDown _mileageNumeric = null!;
        private Button _saveButton = null!;
        private Button _cancelButton = null!;

        public VehicleForm(User user, Vehicle? vehicle = null)
        {
            InitializeComponent();
            _currentUser = user;
            _vehicle = vehicle ?? new Vehicle 
            { 
                UserId = user.Id,
                Make = string.Empty,
                Model = string.Empty,
                VIN = string.Empty,
                LicensePlate = string.Empty,
                FuelType = string.Empty,
                Notes = string.Empty,
                User = user
            };
            _isEditMode = vehicle != null;
            _dbService = new DatabaseService();
            InitializeUI();
            if (_isEditMode)
            {
                LoadVehicleData();
            }
        }

        private void InitializeUI()
        {
            this.Text = _isEditMode ? "Edit Vehicle" : "Add Vehicle";
            this.Size = new Size(400, 400);
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
            mainPanel.Controls.Add(new Label { Text = "Make:", AutoSize = true }, 0, 0);
            _makeTextBox = new TextBox { Width = 200 };
            mainPanel.Controls.Add(_makeTextBox, 1, 0);

            mainPanel.Controls.Add(new Label { Text = "Model:", AutoSize = true }, 0, 1);
            _modelTextBox = new TextBox { Width = 200 };
            mainPanel.Controls.Add(_modelTextBox, 1, 1);

            mainPanel.Controls.Add(new Label { Text = "Year:", AutoSize = true }, 0, 2);
            _yearNumeric = new NumericUpDown
            {
                Minimum = 1900,
                Maximum = DateTime.Now.Year + 1,
                Value = DateTime.Now.Year,
                Width = 200
            };
            mainPanel.Controls.Add(_yearNumeric, 1, 2);

            mainPanel.Controls.Add(new Label { Text = "VIN:", AutoSize = true }, 0, 3);
            _vinTextBox = new TextBox { Width = 200 };
            mainPanel.Controls.Add(_vinTextBox, 1, 3);

            mainPanel.Controls.Add(new Label { Text = "Current Mileage:", AutoSize = true }, 0, 4);
            _mileageNumeric = new NumericUpDown
            {
                Minimum = 0,
                Maximum = 1000000,
                Value = 0,
                Width = 200
            };
            mainPanel.Controls.Add(_mileageNumeric, 1, 4);

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
        }

        private void LoadVehicleData()
        {
            _makeTextBox.Text = _vehicle.Make;
            _modelTextBox.Text = _vehicle.Model;
            _yearNumeric.Value = _vehicle.Year;
            _vinTextBox.Text = _vehicle.VIN;
            _mileageNumeric.Value = _vehicle.CurrentMileage;
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            try
            {
                // Update vehicle data
                _vehicle.Make = _makeTextBox.Text;
                _vehicle.Model = _modelTextBox.Text;
                _vehicle.Year = (int)_yearNumeric.Value;
                _vehicle.VIN = _vinTextBox.Text;
                _vehicle.CurrentMileage = (int)_mileageNumeric.Value;

                // Validate vehicle data
                var validationService = new UnifiedValidationService();
                var validationResult = validationService.ValidateVehicle(_vehicle);

                if (!validationResult.IsValid)
                {
                    MessageBox.Show(
                        validationResult.GetErrorMessage(),
                        "Validation Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                // Save vehicle
                if (_isEditMode)
                {
                    _dbService.UpdateVehicle(_vehicle);
                }
                else
                {
                    _dbService.AddVehicle(_vehicle);
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error saving vehicle: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            _dbService.Dispose();
        }
    }
} 