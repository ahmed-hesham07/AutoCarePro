using AutoCarePro.Services;
using AutoCarePro.Models;
using Microsoft.Extensions.Logging;

namespace AutoCarePro.Forms
{
    public partial class ViewServiceForm : Form
    {
        private readonly DatabaseService _databaseService;
        private readonly ILogger<ViewServiceForm> _logger;
        private readonly ServiceRecord _record;

        public ViewServiceForm(ILogger<ViewServiceForm> logger, ServiceRecord record)
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
            this.Text = $"View Service Record - {_record.ServiceType}";

            // Load service record data
            LoadServiceData();

            // Setup event handlers
            CloseButton.Click += CloseButton_Click;
        }

        private void LoadServiceData()
        {
            try
            {
                // Load service information
                ServiceTypeLabel.Text = _record.ServiceType;
                DescriptionTextBox.Text = _record.Description;
                CostLabel.Text = _record.Cost.ToString("C");
                DateLabel.Text = _record.ServiceDate.ToShortDateString();
                StatusLabel.Text = _record.Status;
                ProviderLabel.Text = _record.ServiceProvider.Name;

                // Load customer information
                CustomerLabel.Text = _record.Customer.Name;
                CustomerEmailLabel.Text = _record.Customer.Email;
                CustomerPhoneLabel.Text = _record.Customer.PhoneNumber;

                // Load vehicle information
                VehicleLabel.Text = $"{_record.Vehicle.Make} {_record.Vehicle.Model} ({_record.Vehicle.Year})";
                LicensePlateLabel.Text = _record.Vehicle.LicensePlate;
                VINLabel.Text = _record.Vehicle.VIN;

                // Load parts information
                PartsDataGridView.DataSource = _record.Parts;

                // Load notes
                NotesTextBox.Text = _record.Notes;

                _logger.LogInformation("Loaded service record {RecordId} for service {ServiceType}", 
                    _record.Id, _record.ServiceType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading service record {RecordId}", _record.Id);
                MessageBox.Show("Error loading service record. Please try again.", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
} 