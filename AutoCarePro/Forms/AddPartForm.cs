using AutoCarePro.Services;
using AutoCarePro.Models;
using Microsoft.Extensions.Logging;

namespace AutoCarePro.Forms
{
    public partial class AddPartForm : Form
    {
        private readonly DatabaseService _databaseService;
        private readonly ILogger<AddPartForm> _logger;
        public Part Part { get; private set; }

        public AddPartForm(ILogger<AddPartForm> logger)
        {
            InitializeComponent();
            _logger = logger;
            _databaseService = ServiceFactory.GetDatabaseService();
            Part = new Part();

            SetupForm();
        }

        private void SetupForm()
        {
            // Set form title
            this.Text = "Add Part";

            // Setup event handlers
            SaveButton.Click += SaveButton_Click;
            CancelButton.Click += CancelButton_Click;
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (!ValidateInputs())
                {
                    return;
                }

                Part.Name = NameTextBox.Text;
                Part.PartNumber = PartNumberTextBox.Text;
                Part.Description = DescriptionTextBox.Text;
                Part.Quantity = int.Parse(QuantityTextBox.Text);
                Part.UnitPrice = decimal.Parse(PriceTextBox.Text);
                Part.Manufacturer = ManufacturerTextBox.Text;
                Part.WarrantyPeriod = int.Parse(WarrantyTextBox.Text);

                _logger.LogInformation("Part {PartNumber} added successfully", Part.PartNumber);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding part");
                MessageBox.Show("An error occurred while adding the part.", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidateInputs()
        {
            if (string.IsNullOrEmpty(NameTextBox.Text) ||
                string.IsNullOrEmpty(PartNumberTextBox.Text) ||
                string.IsNullOrEmpty(DescriptionTextBox.Text) ||
                string.IsNullOrEmpty(QuantityTextBox.Text) ||
                string.IsNullOrEmpty(PriceTextBox.Text) ||
                string.IsNullOrEmpty(ManufacturerTextBox.Text) ||
                string.IsNullOrEmpty(WarrantyTextBox.Text))
            {
                MessageBox.Show("Please fill in all fields.", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!int.TryParse(QuantityTextBox.Text, out int quantity) || quantity <= 0)
            {
                MessageBox.Show("Please enter a valid quantity.", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!decimal.TryParse(PriceTextBox.Text, out decimal price) || price < 0)
            {
                MessageBox.Show("Please enter a valid price.", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!int.TryParse(WarrantyTextBox.Text, out int warranty) || warranty < 0)
            {
                MessageBox.Show("Please enter a valid warranty period.", "Validation Error", 
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