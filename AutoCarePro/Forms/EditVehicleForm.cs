using AutoCarePro.Services;
using AutoCarePro.Models;
using Microsoft.Extensions.Logging;

namespace AutoCarePro.Forms
{
    public partial class EditVehicleForm : Form
    {
        private readonly DatabaseService _databaseService;
        private readonly ILogger<EditVehicleForm> _logger;
        private readonly Vehicle _vehicle;

        public EditVehicleForm(ILogger<EditVehicleForm> logger, Vehicle vehicle)
        {
            InitializeComponent();
            _logger = logger;
            _databaseService = ServiceFactory.GetDatabaseService();
            _vehicle = vehicle;

            SetupForm();
        }

        private void SetupForm()
        {
            // Set form title
            this.Text = $"Edit Vehicle - {_vehicle.Make} {_vehicle.Model}";

            // Load vehicle data
            LoadVehicleData();

            // Setup event handlers
            SaveButton.Click += SaveButton_Click;
            CancelButton.Click += CancelButton_Click;
        }

        private void LoadVehicleData()
        {
            MakeTextBox.Text = _vehicle.Make;
            ModelTextBox.Text = _vehicle.Model;
            YearTextBox.Text = _vehicle.Year.ToString();
            LicensePlateTextBox.Text = _vehicle.LicensePlate;
            VINTextBox.Text = _vehicle.VIN;
            ColorTextBox.Text = _vehicle.Color;
            MileageTextBox.Text = _vehicle.Mileage.ToString();
            LastServiceDatePicker.Value = _vehicle.LastServiceDate;
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (!ValidateInputs())
                {
                    return;
                }

                _vehicle.Make = MakeTextBox.Text;
                _vehicle.Model = ModelTextBox.Text;
                _vehicle.Year = int.Parse(YearTextBox.Text);
                _vehicle.LicensePlate = LicensePlateTextBox.Text;
                _vehicle.VIN = VINTextBox.Text;
                _vehicle.Color = ColorTextBox.Text;
                _vehicle.Mileage = int.Parse(MileageTextBox.Text);
                _vehicle.LastServiceDate = LastServiceDatePicker.Value;
                _vehicle.UpdatedAt = DateTime.Now;

                if (_databaseService.UpdateVehicle(_vehicle))
                {
                    _logger.LogInformation("Vehicle {VehicleId} updated successfully", _vehicle.Id);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    _logger.LogWarning("Failed to update vehicle {VehicleId}", _vehicle.Id);
                    MessageBox.Show("Failed to update vehicle. Please try again.", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating vehicle {VehicleId}", _vehicle.Id);
                MessageBox.Show("An error occurred while updating the vehicle.", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidateInputs()
        {
            if (string.IsNullOrEmpty(MakeTextBox.Text) ||
                string.IsNullOrEmpty(ModelTextBox.Text) ||
                string.IsNullOrEmpty(YearTextBox.Text) ||
                string.IsNullOrEmpty(LicensePlateTextBox.Text) ||
                string.IsNullOrEmpty(VINTextBox.Text) ||
                string.IsNullOrEmpty(ColorTextBox.Text) ||
                string.IsNullOrEmpty(MileageTextBox.Text))
            {
                MessageBox.Show("Please fill in all fields.", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!int.TryParse(YearTextBox.Text, out int year) || year < 1900 || year > DateTime.Now.Year)
            {
                MessageBox.Show("Please enter a valid year.", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!int.TryParse(MileageTextBox.Text, out int mileage) || mileage < 0)
            {
                MessageBox.Show("Please enter a valid mileage.", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
} 