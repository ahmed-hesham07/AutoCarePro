using System;
using System.Windows.Forms;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using AutoCarePro.Models;
using AutoCarePro.Services;
using AutoCarePro.UI;

namespace AutoCarePro.Forms
{
    public partial class MaintenanceRecordForm : BaseForm
    {
        private readonly DatabaseService _dbService;
        private readonly int _vehicleId;
        private readonly int? _recordId;
        private readonly User _currentUser;

        private ComboBox _vehicleComboBox = null!;
        private ComboBox _typeComboBox = null!;
        private DateTimePicker _datePicker = null!;
        private NumericUpDown _mileageNumeric = null!;
        private NumericUpDown _costNumeric = null!;
        private TextBox _serviceProviderTextBox = null!;
        private TextBox _descriptionTextBox = null!;
        private TextBox _notesTextBox = null!;
        private CheckBox _isCompletedCheckBox = null!;
        private Button _saveButton = null!;
        private Button _cancelButton = null!;

        public MaintenanceRecordForm(int vehicleId, User currentUser, int? recordId = null)
        {
            InitializeComponent();
            _vehicleId = vehicleId;
            _recordId = recordId;
            _currentUser = currentUser;
            _dbService = new DatabaseService();
            InitializeUI();
            LoadData();
        }

        private void InitializeUI()
        {
            this.Text = _recordId.HasValue ? "Edit Maintenance Record" : "Add Maintenance Record";
            this.Size = new Size(500, 600);
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
                RowCount = 8
            };

            // Add controls
            mainPanel.Controls.Add(new Label { Text = "Vehicle:", AutoSize = true }, 0, 0);
            _vehicleComboBox = new ComboBox
            {
                Width = 300,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            mainPanel.Controls.Add(_vehicleComboBox, 1, 0);

            mainPanel.Controls.Add(new Label { Text = "Type:", AutoSize = true }, 0, 1);
            _typeComboBox = new ComboBox
            {
                Width = 300,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            mainPanel.Controls.Add(_typeComboBox, 1, 1);

            mainPanel.Controls.Add(new Label { Text = "Date:", AutoSize = true }, 0, 2);
            _datePicker = new DateTimePicker
            {
                Width = 300,
                Format = DateTimePickerFormat.Short
            };
            mainPanel.Controls.Add(_datePicker, 1, 2);

            mainPanel.Controls.Add(new Label { Text = "Mileage:", AutoSize = true }, 0, 3);
            _mileageNumeric = new NumericUpDown
            {
                Width = 300,
                Maximum = 1000000,
                Value = 0
            };
            mainPanel.Controls.Add(_mileageNumeric, 1, 3);

            mainPanel.Controls.Add(new Label { Text = "Cost:", AutoSize = true }, 0, 4);
            _costNumeric = new NumericUpDown
            {
                Width = 300,
                Maximum = 100000,
                DecimalPlaces = 2,
                Value = 0
            };
            mainPanel.Controls.Add(_costNumeric, 1, 4);

            mainPanel.Controls.Add(new Label { Text = "Service Provider:", AutoSize = true }, 0, 5);
            _serviceProviderTextBox = new TextBox { Width = 300 };
            mainPanel.Controls.Add(_serviceProviderTextBox, 1, 5);

            mainPanel.Controls.Add(new Label { Text = "Description:", AutoSize = true }, 0, 6);
            _descriptionTextBox = new TextBox
            {
                Width = 300,
                Multiline = true,
                Height = 60
            };
            mainPanel.Controls.Add(_descriptionTextBox, 1, 6);

            mainPanel.Controls.Add(new Label { Text = "Notes:", AutoSize = true }, 0, 7);
            _notesTextBox = new TextBox
            {
                Width = 300,
                Multiline = true,
                Height = 60
            };
            mainPanel.Controls.Add(_notesTextBox, 1, 7);

            mainPanel.Controls.Add(new Label { Text = "Completed:", AutoSize = true }, 0, 8);
            _isCompletedCheckBox = new CheckBox
            {
                AutoSize = true
            };
            mainPanel.Controls.Add(_isCompletedCheckBox, 1, 8);

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
            // Load vehicles
            var vehicles = _dbService.GetVehiclesByUserId(_currentUser.Id);
            foreach (var vehicle in vehicles)
            {
                _vehicleComboBox.Items.Add(new
                {
                    Id = vehicle.Id,
                    DisplayText = $"{vehicle.Make} {vehicle.Model} ({vehicle.Year})"
                });
            }
            _vehicleComboBox.DisplayMember = "DisplayText";
            _vehicleComboBox.ValueMember = "Id";

            // Load maintenance types
            _typeComboBox.Items.AddRange(new string[] {
                "Oil Change",
                "Brake Service",
                "Tire Rotation",
                "Engine Service",
                "Transmission Service",
                "Electrical System",
                "Suspension",
                "Other"
            });

            // Set default values
            _datePicker.Value = DateTime.Now;
            _isCompletedCheckBox.Checked = false;
        }

        private async void LoadData()
        {
            try
            {
                var vehicle = await _dbService.GetVehicleByIdAsync(_vehicleId);
                if (vehicle != null)
                {
                    _vehicleComboBox.Items.Add(vehicle);
                    _vehicleComboBox.SelectedItem = vehicle;
                }

                if (_recordId.HasValue)
                {
                    var record = await _dbService.GetMaintenanceRecordByIdAsync(_recordId.Value);
                    if (record != null)
                    {
                        _typeComboBox.Text = record.MaintenanceType;
                        _datePicker.Value = record.ServiceDate;
                        _mileageNumeric.Value = record.Mileage;
                        _costNumeric.Value = (decimal)record.Cost;
                        _serviceProviderTextBox.Text = record.ServiceProvider?.Username ?? string.Empty;
                        _descriptionTextBox.Text = record.Description;
                        _notesTextBox.Text = record.Notes;
                        _isCompletedCheckBox.Checked = record.IsCompleted;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void SaveButton_Click(object? sender, EventArgs e)
        {
            try
            {
                if (!ValidateInputs())
                    return;

                var record = new MaintenanceRecord
                {
                    VehicleId = _vehicleId,
                    ServiceProviderId = _currentUser.Id,
                    ServiceDate = _datePicker.Value,
                    MaintenanceType = _typeComboBox.Text,
                    Description = _descriptionTextBox.Text,
                    Mileage = (int)_mileageNumeric.Value,
                    Cost = (decimal)_costNumeric.Value,
                    Notes = _notesTextBox.Text,
                    IsCompleted = _isCompletedCheckBox.Checked
                };

                if (_recordId.HasValue)
                {
                    record.Id = _recordId.Value;
                    await _dbService.UpdateMaintenanceRecordAsync(record);
                }
                else
                {
                    await _dbService.AddMaintenanceRecordAsync(record);
                }

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving record: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(_typeComboBox.Text))
            {
                MessageBox.Show("Please select a maintenance type.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(_descriptionTextBox.Text))
            {
                MessageBox.Show("Please enter a description.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            _dbService.Dispose();
        }
    }
} 