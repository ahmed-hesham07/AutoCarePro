using AutoCarePro.Services;
using AutoCarePro.Models;
using Microsoft.Extensions.Logging;

namespace AutoCarePro.Forms
{
    public partial class CompleteMaintenanceForm : Form
    {
        private readonly DatabaseService _databaseService;
        private readonly ILogger<CompleteMaintenanceForm> _logger;
        private readonly MaintenanceRecord _record;

        public CompleteMaintenanceForm(ILogger<CompleteMaintenanceForm> logger, MaintenanceRecord record)
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
            this.Text = $"Complete Maintenance - {_record.Vehicle.Make} {_record.Vehicle.Model}";

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
                // Load vehicle information
                VehicleLabel.Text = $"{_record.Vehicle.Make} {_record.Vehicle.Model} ({_record.Vehicle.Year})";
                LicensePlateLabel.Text = _record.Vehicle.LicensePlate;
                VINLabel.Text = _record.Vehicle.VIN;

                // Load maintenance information
                ServiceTypeLabel.Text = _record.ServiceType;
                DescriptionTextBox.Text = _record.Description;
                DateLabel.Text = _record.ServiceDate.ToShortDateString();
                TechnicianLabel.Text = _record.Technician;

                // Initialize parts list
                PartsDataGridView.DataSource = _record.Parts;

                _logger.LogInformation("Loaded maintenance record {RecordId} for completion", _record.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading maintenance record {RecordId}", _record.Id);
                MessageBox.Show("Error loading maintenance record. Please try again.", "Error", 
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

                _record.Status = "Completed";
                _record.Cost = decimal.Parse(CostTextBox.Text);
                _record.Notes = NotesTextBox.Text;
                _record.CompletedDate = DateTime.Now;

                if (_databaseService.UpdateMaintenanceRecord(_record))
                {
                    _logger.LogInformation("Maintenance record {RecordId} completed successfully", 
                        _record.Id);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    _logger.LogWarning("Failed to complete maintenance record {RecordId}", 
                        _record.Id);
                    MessageBox.Show("Failed to complete maintenance record. Please try again.", 
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing maintenance record {RecordId}", _record.Id);
                MessageBox.Show("An error occurred while completing the maintenance record.", 
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
                _record.Parts.Add(part);
                PartsDataGridView.DataSource = null;
                PartsDataGridView.DataSource = _record.Parts;
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
            _record.Parts.Remove(selectedPart);
            PartsDataGridView.DataSource = null;
            PartsDataGridView.DataSource = _record.Parts;
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
} 