using System;
using System.Windows.Forms;
using System.Drawing;
using AutoCarePro.Models;
using AutoCarePro.Services;
using AutoCarePro.UI;

namespace AutoCarePro.Forms
{
    /// <summary>
    /// Form that allows users to add or edit maintenance records for vehicles.
    /// This form provides a user-friendly interface for:
    /// 1. Adding new maintenance records with details like type, description, cost, etc.
    /// 2. Editing existing maintenance records
    /// 3. Adding diagnoses (for maintenance centers)
    /// 4. Validating user input to ensure data quality
    /// 
    /// The form adapts its interface based on the user type:
    /// - Regular users can add/edit maintenance records for their vehicles
    /// - Maintenance centers can add records for any vehicle and include diagnoses
    /// </summary>
    public partial class AddMaintenanceForm : Form
    {
        // Core data fields for managing maintenance records
        private readonly int _vehicleId;             // ID of the vehicle being maintained
        private readonly DatabaseService _dbService; // Service for database operations
        private readonly User _currentUser;          // Currently logged-in user
        private readonly MaintenanceRecord? _existingRecord; // Record being edited (if in edit mode)
        private int _currentStep = 0;
        private const int TotalSteps = 3;

        // UI controls for data entry and validation
        private TextBox _descriptionTextBox = new();    // For entering maintenance description
        private TextBox _mileageTextBox = new();        // For entering vehicle mileage
        private TextBox _costTextBox = new();           // For entering maintenance cost
        private TextBox _providerTextBox = new();       // For entering service provider name
        private TextBox _notesTextBox = new();          // For additional maintenance notes
        private ComboBox _typeComboBox = new();         // For selecting maintenance type
        private DateTimePicker _datePicker = new();     // For selecting maintenance date
        private TextBox _ownerUsernameTextBox = new();  // For maintenance centers to enter owner's username
        private Button _addDiagnosisBtn = new();        // For maintenance centers to add diagnosis

        // Wizard navigation controls
        private Button _nextButton = new Button();
        private Button _previousButton = new Button();
        private Button _finishButton = new Button();
        private Label _stepLabel = new Label();
        private Panel _contentPanel = new Panel();
        private System.Windows.Forms.Timer _fadeInTimer = new System.Windows.Forms.Timer();
        private double _fadeStep = 0.08;

        /// <summary>
        /// Initializes the maintenance form for adding or editing a record.
        /// This constructor sets up the form based on whether it's being used to add a new record
        /// or edit an existing one.
        /// </summary>
        /// <param name="vehicleId">ID of the vehicle to add maintenance for</param>
        /// <param name="currentUser">The user currently logged in</param>
        /// <param name="existingRecord">Optional: The record to edit if in edit mode</param>
        public AddMaintenanceForm(int vehicleId, User currentUser, MaintenanceRecord? existingRecord = null)
        {
            InitializeComponent();
            _vehicleId = vehicleId;
            _currentUser = currentUser;
            _existingRecord = existingRecord;
            _dbService = new DatabaseService();
            InitializeForm();
            if (existingRecord != null)
            {
                LoadExistingRecord();
            }
        }

        /// <summary>
        /// Sets up the main form layout and initializes all components.
        /// This method creates a structured layout with input fields and action buttons.
        /// The form is designed to be user-friendly and intuitive.
        /// </summary>
        private void InitializeForm()
        {
            // Configure main form properties
            UIStyles.ApplyFormStyle(this);
            this.Text = "Add Maintenance Record - Step 1 of 3";
            this.Size = new Size(900, 700); // Increased height for better visibility
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Fade-in animation
            this.Opacity = 0;
            _fadeInTimer = new System.Windows.Forms.Timer { Interval = 20 };
            _fadeInTimer.Tick += (s, e) => {
                if (this.Opacity < 1)
                {
                    this.Opacity += _fadeStep;
                }
                else
                {
                    this.Opacity = 1;
                    _fadeInTimer.Stop();
                }
            };
            this.Load += (s, e) => _fadeInTimer.Start();

            // Create main layout panel
            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(20)
            };

            // Step indicator
            _stepLabel = new Label
            {
                Text = "Step 1: Basic Information",
                Height = 40,
                Dock = DockStyle.Top,
                TextAlign = ContentAlignment.MiddleLeft
            };
            UIStyles.ApplyLabelStyle(_stepLabel, true);

            // Content panel for each step (with scrolling)
            _contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = UIStyles.Colors.Secondary
            };

            // Navigation buttons panel
            var buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                FlowDirection = FlowDirection.RightToLeft,
                Height = 50,
                Padding = new Padding(10),
                BackColor = UIStyles.Colors.Secondary
            };

            // Initialize navigation buttons
            _nextButton = new Button
            {
                Text = "Next",
                Width = 100,
                Height = 35
            };
            _nextButton.Click += NextButton_Click;

            _previousButton = new Button
            {
                Text = "Previous",
                Width = 100,
                Height = 35,
                Enabled = false
            };
            _previousButton.Click += PreviousButton_Click;

            _finishButton = new Button
            {
                Text = "Finish",
                Width = 100,
                Height = 35,
                Visible = false
            };
            _finishButton.Click += FinishButton_Click;

            buttonPanel.Controls.AddRange(new Control[] { _finishButton, _nextButton, _previousButton });

            // Add controls to main panel
            mainPanel.Controls.Add(_stepLabel);
            mainPanel.Controls.Add(_contentPanel);
            mainPanel.Controls.Add(buttonPanel);

            this.Controls.Add(mainPanel);

            // Show first step
            ShowStep(0);
        }

        private void ShowStep(int step)
        {
            _currentStep = step;
            _contentPanel.Controls.Clear();

            switch (step)
            {
                case 0:
                    ShowBasicInfoStep();
                    break;
                case 1:
                    ShowServiceDetailsStep();
                    break;
                case 2:
                    ShowReviewStep();
                    break;
            }

            // Update navigation buttons
            _previousButton.Enabled = step > 0;
            _nextButton.Visible = step < TotalSteps - 1;
            _finishButton.Visible = step == TotalSteps - 1;

            // Update step label
            _stepLabel.Text = $"Step {step + 1} of {TotalSteps}: {GetStepTitle(step)}";
        }

        private string GetStepTitle(int step)
        {
            return step switch
            {
                0 => "Basic Information",
                1 => "Service Details",
                2 => "Review & Save",
                _ => string.Empty
            };
        }

        private void ShowBasicInfoStep()
        {
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2,
                Padding = new Padding(20),
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = Color.White,
                ColumnStyles = {
                    new ColumnStyle(SizeType.Percent, 30),
                    new ColumnStyle(SizeType.Percent, 70)
                }
            };

            // Maintenance Type field
            var lblType = new Label { Text = "Maintenance Type:", AutoSize = true };
            UIStyles.ApplyLabelStyle(lblType);
            layout.Controls.Add(lblType, 0, 0);
            _typeComboBox = new ComboBox
            {
                Width = 400,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Anchor = AnchorStyles.Left
            };
            _typeComboBox.Items.AddRange(new string[] { "Oil Change", "Brake Service", "Tire Rotation", "Engine Repair", "Other" });
            _typeComboBox.SelectedIndex = 0;
            UIStyles.ApplyComboBoxStyle(_typeComboBox);
            layout.Controls.Add(_typeComboBox, 1, 0);

            // Description field
            var lblDescription = new Label { Text = "Description:", AutoSize = true };
            UIStyles.ApplyLabelStyle(lblDescription);
            layout.Controls.Add(lblDescription, 0, 1);
            _descriptionTextBox = new TextBox { Width = 400, Anchor = AnchorStyles.Left };
            UIStyles.ApplyTextBoxStyle(_descriptionTextBox);
            layout.Controls.Add(_descriptionTextBox, 1, 1);

            _contentPanel.Controls.Add(layout);
        }

        private void ShowServiceDetailsStep()
        {
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 4,
                Padding = new Padding(20),
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = Color.White,
                ColumnStyles = {
                    new ColumnStyle(SizeType.Percent, 30),
                    new ColumnStyle(SizeType.Percent, 70)
                }
            };

            // Mileage field
            var lblMileage = new Label { Text = "Mileage:", AutoSize = true };
            UIStyles.ApplyLabelStyle(lblMileage);
            layout.Controls.Add(lblMileage, 0, 0);
            _mileageTextBox = new TextBox { Width = 400, Anchor = AnchorStyles.Left };
            UIStyles.ApplyTextBoxStyle(_mileageTextBox);
            layout.Controls.Add(_mileageTextBox, 1, 0);

            // Cost field
            var lblCost = new Label { Text = "Cost:", AutoSize = true };
            UIStyles.ApplyLabelStyle(lblCost);
            layout.Controls.Add(lblCost, 0, 1);
            _costTextBox = new TextBox { Width = 400, Anchor = AnchorStyles.Left };
            UIStyles.ApplyTextBoxStyle(_costTextBox);
            layout.Controls.Add(_costTextBox, 1, 1);

            // Service Provider field
            var lblProvider = new Label { Text = "Service Provider:", AutoSize = true };
            UIStyles.ApplyLabelStyle(lblProvider);
            layout.Controls.Add(lblProvider, 0, 2);
            _providerTextBox = new TextBox { Width = 400, Anchor = AnchorStyles.Left };
            UIStyles.ApplyTextBoxStyle(_providerTextBox);
            layout.Controls.Add(_providerTextBox, 1, 2);

            // Date field
            var lblDate = new Label { Text = "Date:", AutoSize = true };
            UIStyles.ApplyLabelStyle(lblDate);
            layout.Controls.Add(lblDate, 0, 3);
            _datePicker = new DateTimePicker { Width = 400, Anchor = AnchorStyles.Left };
            layout.Controls.Add(_datePicker, 1, 3);

            _contentPanel.Controls.Add(layout);
        }

        private void ShowReviewStep()
        {
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 3,
                Padding = new Padding(20),
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = Color.White,
                ColumnStyles = {
                    new ColumnStyle(SizeType.Percent, 30),
                    new ColumnStyle(SizeType.Percent, 70)
                }
            };

            // Notes field
            var lblNotes = new Label { Text = "Notes:", AutoSize = true };
            UIStyles.ApplyLabelStyle(lblNotes);
            layout.Controls.Add(lblNotes, 0, 0);
            _notesTextBox = new TextBox
            {
                Width = 400,
                Height = 100,
                Multiline = true,
                Anchor = AnchorStyles.Left
            };
            UIStyles.ApplyTextBoxStyle(_notesTextBox);
            layout.Controls.Add(_notesTextBox, 1, 0);

            // Additional fields for maintenance centers
            if (_currentUser.Type == UserType.MaintenanceCenter)
            {
                // Owner username field
                var lblOwner = new Label { Text = "Owner Username:", AutoSize = true };
                UIStyles.ApplyLabelStyle(lblOwner);
                layout.Controls.Add(lblOwner, 0, 1);
                _ownerUsernameTextBox = new TextBox { Width = 400, Anchor = AnchorStyles.Left };
                UIStyles.ApplyTextBoxStyle(_ownerUsernameTextBox);
                layout.Controls.Add(_ownerUsernameTextBox, 1, 1);

                // Diagnosis button
                _addDiagnosisBtn = new Button
                {
                    Text = "Add Diagnosis",
                    Width = 200,
                    Height = 35,
                    Anchor = AnchorStyles.Left
                };
                UIStyles.ApplyButtonStyle(_addDiagnosisBtn, true);
                _addDiagnosisBtn.Click += AddDiagnosisBtn_Click;
                layout.Controls.Add(_addDiagnosisBtn, 1, 2);
            }

            _contentPanel.Controls.Add(layout);
        }

        private void NextButton_Click(object sender, EventArgs e)
        {
            if (ValidateCurrentStep())
            {
                ShowStep(_currentStep + 1);
            }
        }

        private void PreviousButton_Click(object sender, EventArgs e)
        {
            ShowStep(_currentStep - 1);
        }

        private void FinishButton_Click(object sender, EventArgs e)
        {
            if (ValidateCurrentStep())
            {
                SaveMaintenanceRecord();
            }
        }

        private bool ValidateCurrentStep()
        {
            switch (_currentStep)
            {
                case 0:
                    if (string.IsNullOrWhiteSpace(_descriptionTextBox.Text))
                    {
                        MessageBox.Show("Please enter a description of the maintenance work.", "Validation Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return false;
                    }
                    break;

                case 1:
                    if (!int.TryParse(_mileageTextBox.Text, out int mileage) || mileage < 0)
                    {
                        MessageBox.Show("Please enter a valid mileage.", "Validation Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return false;
                    }

                    if (!decimal.TryParse(_costTextBox.Text, out decimal cost) || cost < 0)
                    {
                        MessageBox.Show("Please enter a valid cost.", "Validation Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return false;
                    }

                    if (string.IsNullOrWhiteSpace(_providerTextBox.Text))
                    {
                        MessageBox.Show("Please enter the service provider.", "Validation Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return false;
                    }
                    break;

                case 2:
                    if (_currentUser.Type == UserType.MaintenanceCenter && string.IsNullOrWhiteSpace(_ownerUsernameTextBox.Text))
                    {
                        MessageBox.Show("Please enter the owner's username.", "Validation Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return false;
                    }
                    break;
            }
            return true;
        }

        /// <summary>
        /// Event handler for the Add Diagnosis button.
        /// Opens a new diagnosis form for the current vehicle and user.
        /// </summary>
        /// <param name="sender">The object that triggered the event</param>
        /// <param name="e">Event arguments</param>
        private void AddDiagnosisBtn_Click(object sender, EventArgs e)
        {
            var diagnosisForm = new DiagnosisForm(_vehicleId, _currentUser.Id);
            if (diagnosisForm.ShowDialog() == DialogResult.OK)
            {
                MessageBox.Show("Diagnosis added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// Loads an existing maintenance record into the form for editing.
        /// This method populates all form fields with the record's current values.
        /// </summary>
        private void LoadExistingRecord()
        {
            this.Text = "Edit Maintenance Record - Step 1 of 3";
            _typeComboBox.SelectedItem = _existingRecord.MaintenanceType;
            _descriptionTextBox.Text = _existingRecord.Description;
            _mileageTextBox.Text = _existingRecord.MileageAtMaintenance.ToString();
            _costTextBox.Text = _existingRecord.Cost.ToString();
            _providerTextBox.Text = _existingRecord.ServiceProvider;
            _datePicker.Value = _existingRecord.MaintenanceDate;
            _notesTextBox.Text = _existingRecord.Notes;
        }

        private void SaveMaintenanceRecord()
        {
            try
            {
                // Create or update maintenance record
                var record = _existingRecord ?? new MaintenanceRecord
                {
                    Vehicle = _dbService.GetVehicleById(_vehicleId),
                    MaintenanceType = _typeComboBox.SelectedItem.ToString(),
                    Description = _descriptionTextBox.Text,
                    ServiceProvider = _providerTextBox.Text,
                    Notes = _notesTextBox.Text
                };

                record.VehicleId = _vehicleId;
                record.MileageAtMaintenance = int.Parse(_mileageTextBox.Text);
                record.Cost = decimal.Parse(_costTextBox.Text);
                record.MaintenanceDate = _datePicker.Value;
                record.IsCompleted = true;

                // Additional fields for maintenance centers
                if (_currentUser.Type == UserType.MaintenanceCenter)
                {
                    record.DiagnosedByUserId = _currentUser.Id;
                }

                // Save to database
                if (_existingRecord == null)
                {
                    _dbService.AddMaintenanceRecord(record);
                }
                else
                {
                    _dbService.UpdateMaintenanceRecord(record);
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving maintenance record: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
} 