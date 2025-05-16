using System;
using System.Windows.Forms;
using System.Drawing;
using AutoCarePro.Models;
using AutoCarePro.Services;

namespace AutoCarePro.Forms
{
    // This form allows the user to add or edit a maintenance record for a vehicle
    public partial class AddMaintenanceForm : Form
    {
        // Fields to store information about the vehicle, user, and database
        private readonly int _vehicleId; // The ID of the vehicle being maintained
        private readonly DatabaseService _dbService; // Service for database operations
        private readonly User _currentUser; // The user currently logged in
        // UI controls for data entry
        private TextBox _descriptionTextBox;
        private TextBox _mileageTextBox;
        private TextBox _costTextBox;
        private TextBox _providerTextBox;
        private TextBox _notesTextBox;
        private ComboBox _typeComboBox;
        private DateTimePicker _datePicker;
        private TextBox _ownerUsernameTextBox; // For maintenance centers to enter owner's username
        private Button _addDiagnosisBtn; // For maintenance centers to add diagnosis
        private readonly MaintenanceRecord _existingRecord; // If editing, the record being edited

        // Constructor: initializes the form for adding or editing a maintenance record
        public AddMaintenanceForm(int vehicleId, User currentUser, MaintenanceRecord existingRecord = null)
        {
            InitializeComponent(); // Set up the form's UI
            _vehicleId = vehicleId; // Store the vehicle ID
            _currentUser = currentUser; // Store the current user
            _existingRecord = existingRecord; // Store the record if editing
            _dbService = new DatabaseService(); // Create a new database service
            InitializeForm(); // Set up the form layout and controls
            if (existingRecord != null)
            {
                LoadExistingRecord(); // If editing, load the record's data into the form
            }
        }

        // Set up the main layout and add all controls
        private void InitializeForm()
        {
            // Set up the form window properties
            this.Text = "Add Maintenance Record";
            this.Size = new Size(500, 700);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Create a main panel to hold everything
            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(10)
            };

            // Create a panel for the form fields
            var fieldsPanel = new Panel { Dock = DockStyle.Fill };
            InitializeFields(fieldsPanel); // Add the input fields

            // Create a panel for the Save/Cancel buttons
            var buttonsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(10)
            };
            InitializeButtons(buttonsPanel); // Add the buttons

            // Add the field and button panels to the main panel
            mainPanel.Controls.Add(fieldsPanel);
            mainPanel.Controls.Add(buttonsPanel);

            // Add the main panel to the form
            this.Controls.Add(mainPanel);
        }

        // Set up all the input fields for the form
        private void InitializeFields(Panel panel)
        {
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 10,
                Padding = new Padding(10)
            };

            // Maintenance Type dropdown
            layout.Controls.Add(new Label { Text = "Maintenance Type:", AutoSize = true }, 0, 0);
            _typeComboBox = new ComboBox
            {
                Width = 200,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _typeComboBox.Items.AddRange(new string[] { "Oil Change", "Brake Service", "Tire Rotation", "Engine Repair", "Other" });
            _typeComboBox.SelectedIndex = 0;
            layout.Controls.Add(_typeComboBox, 1, 0);

            // Description textbox
            layout.Controls.Add(new Label { Text = "Description:", AutoSize = true }, 0, 1);
            _descriptionTextBox = new TextBox { Width = 200 };
            layout.Controls.Add(_descriptionTextBox, 1, 1);

            // Mileage textbox
            layout.Controls.Add(new Label { Text = "Mileage:", AutoSize = true }, 0, 2);
            _mileageTextBox = new TextBox { Width = 200 };
            layout.Controls.Add(_mileageTextBox, 1, 2);

            // Cost textbox
            layout.Controls.Add(new Label { Text = "Cost:", AutoSize = true }, 0, 3);
            _costTextBox = new TextBox { Width = 200 };
            layout.Controls.Add(_costTextBox, 1, 3);

            // Service Provider textbox
            layout.Controls.Add(new Label { Text = "Service Provider:", AutoSize = true }, 0, 4);
            _providerTextBox = new TextBox { Width = 200 };
            layout.Controls.Add(_providerTextBox, 1, 4);

            // Date picker
            layout.Controls.Add(new Label { Text = "Date:", AutoSize = true }, 0, 5);
            _datePicker = new DateTimePicker { Width = 200 };
            layout.Controls.Add(_datePicker, 1, 5);

            // Notes textbox (multiline)
            layout.Controls.Add(new Label { Text = "Notes:", AutoSize = true }, 0, 6);
            _notesTextBox = new TextBox
            {
                Width = 200,
                Height = 100,
                Multiline = true
            };
            layout.Controls.Add(_notesTextBox, 1, 6);

            // If the user is a maintenance center, add owner username and diagnosis button
            if (_currentUser.Type == "Maintenance Center")
            {
                layout.Controls.Add(new Label { Text = "Owner Username:", AutoSize = true }, 0, 7);
                _ownerUsernameTextBox = new TextBox { Width = 200 };
                layout.Controls.Add(_ownerUsernameTextBox, 1, 7);

                // Button to open the diagnosis form
                _addDiagnosisBtn = new Button
                {
                    Text = "Add Diagnosis",
                    Width = 200
                };
                _addDiagnosisBtn.Click += AddDiagnosisBtn_Click;
                layout.Controls.Add(_addDiagnosisBtn, 1, 8);
            }

            panel.Controls.Add(layout);
        }

        // Event handler for the Add Diagnosis button
        private void AddDiagnosisBtn_Click(object sender, EventArgs e)
        {
            // Open the diagnosis form for this vehicle and user
            var diagnosisForm = new DiagnosisForm(_vehicleId, _currentUser.Id);
            if (diagnosisForm.ShowDialog() == DialogResult.OK)
            {
                // Show a message if diagnosis was added successfully
                MessageBox.Show("Diagnosis added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        // Set up the Save and Cancel buttons
        private void InitializeButtons(FlowLayoutPanel panel)
        {
            var saveButton = new Button
            {
                Text = "Save",
                DialogResult = DialogResult.OK
            };
            saveButton.Click += SaveButton_Click;

            var cancelButton = new Button
            {
                Text = "Cancel",
                DialogResult = DialogResult.Cancel
            };

            panel.Controls.Add(cancelButton);
            panel.Controls.Add(saveButton);
        }

        // If editing, load the existing record's data into the form fields
        private void LoadExistingRecord()
        {
            this.Text = "Edit Maintenance Record";
            _typeComboBox.SelectedItem = _existingRecord.MaintenanceType;
            _descriptionTextBox.Text = _existingRecord.Description;
            _mileageTextBox.Text = _existingRecord.MileageAtMaintenance.ToString();
            _costTextBox.Text = _existingRecord.Cost.ToString();
            _providerTextBox.Text = _existingRecord.ServiceProvider;
            _datePicker.Value = _existingRecord.MaintenanceDate;
            _notesTextBox.Text = _existingRecord.Notes;
        }

        // Event handler for the Save button
        private void SaveButton_Click(object sender, EventArgs e)
        {
            try
            {
                // Validate that the description is not empty
                if (string.IsNullOrWhiteSpace(_descriptionTextBox.Text))
                {
                    MessageBox.Show("Please enter a description.", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Validate that mileage is a valid decimal number
                if (!decimal.TryParse(_mileageTextBox.Text, out decimal mileage))
                {
                    MessageBox.Show("Please enter a valid mileage.", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Validate that cost is a valid decimal number
                if (!decimal.TryParse(_costTextBox.Text, out decimal cost))
                {
                    MessageBox.Show("Please enter a valid cost.", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // If the user is a maintenance center, validate the owner's username
                if (_currentUser.Type == "Maintenance Center")
                {
                    if (string.IsNullOrWhiteSpace(_ownerUsernameTextBox.Text))
                    {
                        MessageBox.Show("Please enter the owner's username.", "Validation Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // Check if the owner exists in the database
                    var owner = _dbService.GetUserByUsername(_ownerUsernameTextBox.Text);
                    if (owner == null)
                    {
                        MessageBox.Show("Owner username not found.", "Validation Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                // Create a new record or update the existing one
                var record = _existingRecord ?? new MaintenanceRecord { VehicleId = _vehicleId };
                record.MaintenanceType = _typeComboBox.SelectedItem.ToString();
                record.Description = _descriptionTextBox.Text;
                record.MileageAtMaintenance = mileage;
                record.Cost = cost;
                record.ServiceProvider = _providerTextBox.Text;
                record.MaintenanceDate = _datePicker.Value;
                record.Notes = _notesTextBox.Text;

                // If the user is a maintenance center, set the DiagnosedByUserId
                if (_currentUser.Type == "Maintenance Center")
                {
                    record.DiagnosedByUserId = _currentUser.Id;
                }

                // Save the record to the database
                if (_existingRecord == null)
                {
                    _dbService.AddMaintenanceRecord(record);
                }
                _dbService.SaveChanges();

                // Close the form and return OK
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                // Show an error message if something goes wrong
                MessageBox.Show($"Error saving maintenance record: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
} 