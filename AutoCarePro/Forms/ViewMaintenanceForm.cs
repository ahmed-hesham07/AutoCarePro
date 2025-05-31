using AutoCarePro.Services;
using AutoCarePro.Models;
using Microsoft.Extensions.Logging;

namespace AutoCarePro.Forms
{
    public partial class ViewMaintenanceForm : Form
    {
        private readonly DatabaseService _databaseService;
        private readonly ILogger<ViewMaintenanceForm> _logger;
        private readonly MaintenanceRecord _record;

        public ViewMaintenanceForm(ILogger<ViewMaintenanceForm> logger, MaintenanceRecord record)
        {
            InitializeComponent();
            _logger = logger;
            _databaseService = ServiceFactory.GetDatabaseService();
            _record = record;

            SetupForm();
        }

        private void SetupForm()
        {
            // Set form title
            this.Text = $"View Maintenance Record - {_record.Vehicle.Make} {_record.Vehicle.Model}";

            // Load maintenance record data
            LoadMaintenanceData();

            // Setup event handlers
            CloseButton.Click += CloseButton_Click;
        }

        private void LoadMaintenanceData()
        {
            try
            {
                // Load vehicle information
                VehicleLabel.Text = $"{_record.Vehicle.Make} {_record.Vehicle.Model} ({_record.Vehicle.Year})";
                LicensePlateLabel.Text = _record.Vehicle.LicensePlate;
                VINLabel.Text = _record.Vehicle.VIN;

                // Load maintenance information
                ServiceTypeLabel.Text = _record.ServiceType;
                DescriptionTextBox.Text = _record.Description;
                CostLabel.Text = _record.Cost.ToString("C");
                DateLabel.Text = _record.ServiceDate.ToShortDateString();
                StatusLabel.Text = _record.Status;
                TechnicianLabel.Text = _record.Technician;

                // Load parts information
                PartsDataGridView.DataSource = _record.Parts;

                // Load notes
                NotesTextBox.Text = _record.Notes;

                _logger.LogInformation("Loaded maintenance record {RecordId} for vehicle {VehicleId}", 
                    _record.Id, _record.VehicleId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading maintenance record {RecordId}", _record.Id);
                MessageBox.Show("Error loading maintenance record. Please try again.", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
} 