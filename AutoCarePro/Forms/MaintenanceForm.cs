using System;
using System.Windows.Forms;
using AutoCarePro.Services;

namespace AutoCarePro.Forms
{
    public partial class MaintenanceForm : Form
    {
        private readonly int? _recordId;
        private readonly int _vehicleId;
        private readonly int _serviceProviderId;
        private readonly DatabaseService _databaseService;

        private DateTimePicker _datePicker;
        private ComboBox _typeComboBox;
        private TextBox _descriptionTextBox;
        private TextBox _mileageTextBox;
        private TextBox _costTextBox;
        private Button _saveButton;
        private Button _cancelButton;

        public MaintenanceForm(int? recordId, int vehicleId, int serviceProviderId, DatabaseService databaseService)
        {
            _recordId = recordId;
            _vehicleId = vehicleId;
            _serviceProviderId = serviceProviderId;
            _databaseService = databaseService;
            InitializeComponent();
            LoadMaintenanceTypes();
            if (_recordId.HasValue)
            {
                LoadExistingRecord();
            }
        }

        private void InitializeComponent()
        {
            this.Text = "Maintenance Record";
            this.Size = new System.Drawing.Size(500, 400);
            this.StartPosition = FormStartPosition.CenterParent;

            var dateLabel = new Label
            {
                Text = "Service Date:",
                Location = new System.Drawing.Point(20, 20),
                AutoSize = true
            };

            _datePicker = new DateTimePicker
            {
                Location = new System.Drawing.Point(150, 20),
                Width = 200
            };

            var typeLabel = new Label
            {
                Text = "Maintenance Type:",
                Location = new System.Drawing.Point(20, 60),
                AutoSize = true
            };

            _typeComboBox = new ComboBox
            {
                Location = new System.Drawing.Point(150, 60),
                Width = 200,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            var descriptionLabel = new Label
            {
                Text = "Description:",
                Location = new System.Drawing.Point(20, 100),
                AutoSize = true
            };

            _descriptionTextBox = new TextBox
            {
                Location = new System.Drawing.Point(150, 100),
                Width = 200,
                Multiline = true,
                Height = 60
            };

            var mileageLabel = new Label
            {
                Text = "Mileage:",
                Location = new System.Drawing.Point(20, 180),
                AutoSize = true
            };

            _mileageTextBox = new TextBox
            {
                Location = new System.Drawing.Point(150, 180),
                Width = 200
            };

            var costLabel = new Label
            {
                Text = "Cost:",
                Location = new System.Drawing.Point(20, 220),
                AutoSize = true
            };

            _costTextBox = new TextBox
            {
                Location = new System.Drawing.Point(150, 220),
                Width = 200
            };

            _saveButton = new Button
            {
                Text = "Save",
                Location = new System.Drawing.Point(150, 280),
                Width = 80
            };
            _saveButton.Click += SaveButton_Click;

            _cancelButton = new Button
            {
                Text = "Cancel",
                Location = new System.Drawing.Point(250, 280),
                Width = 80
            };
            _cancelButton.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            this.Controls.AddRange(new Control[] {
                dateLabel, _datePicker,
                typeLabel, _typeComboBox,
                descriptionLabel, _descriptionTextBox,
                mileageLabel, _mileageTextBox,
                costLabel, _costTextBox,
                _saveButton, _cancelButton
            });
        }

        private void LoadMaintenanceTypes()
        {
            _typeComboBox.Items.AddRange(new string[] {
                "Oil Change",
                "Tire Rotation",
                "Brake Service",
                "Engine Tune-up",
                "Transmission Service",
                "Air Filter Replacement",
                "Battery Replacement",
                "Coolant Flush",
                "Other"
            });
        }

        private async void LoadExistingRecord()
        {
            try
            {
                var record = await _databaseService.GetMaintenanceRecordById(_recordId.Value);
                if (record != null)
                {
                    _datePicker.Value = record.ServiceDate;
                    _typeComboBox.Text = record.MaintenanceType;
                    _descriptionTextBox.Text = record.Description;
                    _mileageTextBox.Text = record.Mileage.ToString();
                    _costTextBox.Text = record.Cost.ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading maintenance record: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (!ValidateInputs())
                    return;

                var record = new MaintenanceRecord
                {
                    VehicleId = _vehicleId,
                    ServiceProviderId = _serviceProviderId,
                    ServiceDate = _datePicker.Value,
                    MaintenanceType = _typeComboBox.Text,
                    Description = _descriptionTextBox.Text,
                    Mileage = Convert.ToInt32(_mileageTextBox.Text),
                    Cost = Convert.ToDecimal(_costTextBox.Text)
                };

                if (_recordId.HasValue)
                {
                    record.Id = _recordId.Value;
                    _databaseService.UpdateMaintenanceRecord(record);
                }
                else
                {
                    _databaseService.AddMaintenanceRecord(record);
                }

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving maintenance record: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

            if (!int.TryParse(_mileageTextBox.Text, out int mileage) || mileage <= 0)
            {
                MessageBox.Show("Please enter a valid mileage.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!decimal.TryParse(_costTextBox.Text, out decimal cost) || cost <= 0)
            {
                MessageBox.Show("Please enter a valid cost.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }
    }
} 