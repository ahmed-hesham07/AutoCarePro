using AutoCarePro.Services;
using AutoCarePro.Models;
using Microsoft.Extensions.Logging;

namespace AutoCarePro.Forms
{
    public partial class CompleteServiceForm : Form
    {
        private readonly DatabaseService _databaseService;
        private readonly ILogger<CompleteServiceForm> _logger;
        private readonly ServiceRequest _request;

        public CompleteServiceForm(ILogger<CompleteServiceForm> logger, ServiceRequest request)
        {
            InitializeComponent();
            _logger = logger;
            _databaseService = ServiceFactory.GetDatabaseService();
            _request = request;

            SetupForm();
        }

        private void SetupForm()
        {
            // Set form title
            this.Text = $"Complete Service - {_request.ServiceType}";

            // Load initial data
            LoadInitialData();

            // Setup event handlers
            SaveButton.Click += SaveButton_Click;
            CancelButton.Click += CancelButton_Click;
            AddPartButton.Click += AddPartButton_Click;
            RemovePartButton.Click += RemovePartButton_Click;
        }

        private void LoadInitialData()
        {
            try
            {
                // Load service information
                ServiceTypeLabel.Text = _request.ServiceType;
                DescriptionTextBox.Text = _request.Description;
                DateLabel.Text = _request.RequestDate.ToShortDateString();
                ProviderLabel.Text = _request.ServiceProvider.Name;

                // Load customer information
                CustomerLabel.Text = _request.Customer.Name;
                CustomerEmailLabel.Text = _request.Customer.Email;
                CustomerPhoneLabel.Text = _request.Customer.PhoneNumber;

                // Load vehicle information
                VehicleLabel.Text = $"{_request.Vehicle.Make} {_request.Vehicle.Model} ({_request.Vehicle.Year})";
                LicensePlateLabel.Text = _request.Vehicle.LicensePlate;
                VINLabel.Text = _request.Vehicle.VIN;

                // Initialize parts list
                PartsDataGridView.DataSource = _request.Parts;

                _logger.LogInformation("Loaded service request {RequestId} for completion", _request.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading service request {RequestId}", _request.Id);
                MessageBox.Show("Error loading service request. Please try again.", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (!ValidateInputs())
                {
                    return;
                }

                var serviceRecord = new ServiceRecord
                {
                    ServiceType = _request.ServiceType,
                    Description = _request.Description,
                    Cost = decimal.Parse(CostTextBox.Text),
                    ServiceDate = _request.RequestDate,
                    Status = "Completed",
                    ServiceProviderId = _request.ServiceProviderId,
                    CustomerId = _request.CustomerId,
                    VehicleId = _request.VehicleId,
                    Parts = _request.Parts,
                    Notes = NotesTextBox.Text,
                    CompletedDate = DateTime.Now
                };

                if (_databaseService.CompleteServiceRequest(_request.Id, serviceRecord))
                {
                    _logger.LogInformation("Service request {RequestId} completed successfully", 
                        _request.Id);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    _logger.LogWarning("Failed to complete service request {RequestId}", 
                        _request.Id);
                    MessageBox.Show("Failed to complete service request. Please try again.", 
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing service request {RequestId}", _request.Id);
                MessageBox.Show("An error occurred while completing the service request.", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidateInputs()
        {
            if (string.IsNullOrEmpty(CostTextBox.Text))
            {
                MessageBox.Show("Please enter the total cost.", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!decimal.TryParse(CostTextBox.Text, out decimal cost) || cost < 0)
            {
                MessageBox.Show("Please enter a valid cost amount.", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        private void AddPartButton_Click(object sender, EventArgs e)
        {
            var addPartForm = new AddPartForm(_logger);
            if (addPartForm.ShowDialog() == DialogResult.OK)
            {
                var part = addPartForm.Part;
                _request.Parts.Add(part);
                PartsDataGridView.DataSource = null;
                PartsDataGridView.DataSource = _request.Parts;
            }
        }

        private void RemovePartButton_Click(object sender, EventArgs e)
        {
            if (PartsDataGridView.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a part to remove.", "Information", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var selectedPart = (Part)PartsDataGridView.SelectedRows[0].DataBoundItem;
            _request.Parts.Remove(selectedPart);
            PartsDataGridView.DataSource = null;
            PartsDataGridView.DataSource = _request.Parts;
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
} 