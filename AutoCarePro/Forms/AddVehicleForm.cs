using System;
using System.Windows.Forms;
using System.Drawing;
using AutoCarePro.Models;
using AutoCarePro.Services;
using AutoCarePro.UI;

namespace AutoCarePro.Forms
{
    /// <summary>
    /// AddVehicleForm class represents the vehicle addition/editing interface for the AutoCarePro application.
    /// This form provides a user-friendly interface for users to:
    /// 1. Add new vehicles to their profile
    /// 2. Edit existing vehicle information
    /// 3. Input and validate important vehicle details like make, model, VIN, etc.
    /// 
    /// The form includes comprehensive validation to ensure data accuracy and completeness.
    /// </summary>
    public partial class AddVehicleForm : Form
    {
        // Private fields to store form data and services
        private readonly int _userId;                    // Stores the ID of the user who owns the vehicle
        private readonly DatabaseService _dbService;     // Service for database operations
        private readonly Vehicle? _existingVehicle;       // Reference to existing vehicle if editing
        private int _currentStep = 0;
        private const int TotalSteps = 3;

        // Form control fields - these are the UI elements that users interact with
        private TextBox _makeTextBox = new();           // Input field for vehicle manufacturer
        private TextBox _modelTextBox = new();          // Input field for vehicle model
        private TextBox _yearTextBox = new();           // Input field for vehicle year
        private TextBox _vinTextBox = new();            // Input field for Vehicle Identification Number
        private TextBox _mileageTextBox = new();        // Input field for current mileage
        private TextBox _licensePlateTextBox = new();   // Input field for license plate number
        private ComboBox _fuelTypeComboBox = new();     // Dropdown for selecting fuel type
        private DateTimePicker _purchaseDatePicker = new();  // Date picker for purchase date
        private TextBox _notesTextBox = new();          // Multiline text box for additional notes

        // Wizard navigation controls
        private Button _nextButton = new Button();
        private Button _previousButton = new Button();
        private Button _finishButton = new Button();
        private Label _stepLabel = new Label();
        private Panel _contentPanel = new Panel();

        /// <summary>
        /// Constructor initializes the vehicle form for adding or editing a vehicle.
        /// This is called when creating a new instance of the form.
        /// </summary>
        /// <param name="userId">ID of the user who owns the vehicle - used to associate the vehicle with the correct user</param>
        /// <param name="existingVehicle">Optional existing vehicle to edit - if provided, the form will be in edit mode</param>
        public AddVehicleForm(int userId, Vehicle? existingVehicle = null)
        {
            InitializeComponent();
            _userId = userId;
            _existingVehicle = existingVehicle;
            _dbService = new DatabaseService();
            InitializeForm();
            if (existingVehicle != null)
            {
                LoadExistingVehicle();
            }
        }

        /// <summary>
        /// Initializes the form layout and controls.
        /// This method sets up the visual appearance and structure of the form,
        /// including its size, position, and the arrangement of all UI elements.
        /// </summary>
        private void InitializeForm()
        {
            // Configure basic form properties
            this.Text = "Add Vehicle - Step 1 of 3";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Fade-in animation
            this.Opacity = 0;
            var fadeInTimer = new System.Windows.Forms.Timer { Interval = 20 };
            fadeInTimer.Tick += (s, e) => {
                if (this.Opacity < 1)
                {
                    this.Opacity += 0.05;
                }
                else
                {
                    this.Opacity = 1;
                    fadeInTimer.Stop();
                }
            };
            this.Load += (s, e) => fadeInTimer.Start();

            // Create main layout
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
                Dock = DockStyle.Top,
                Height = 40,
                TextAlign = ContentAlignment.MiddleLeft
            };
            UIStyles.ApplyLabelStyle(_stepLabel, true);

            // Content panel for each step
            _contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = UIStyles.Colors.Secondary
            };
            UIStyles.ApplyPanelStyle(_contentPanel, true);

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
            UIStyles.ApplyButtonStyle(_nextButton, true);
            _nextButton.Click += NextButton_Click;

            _previousButton = new Button
            {
                Text = "Previous",
                Width = 100,
                Height = 35,
                Enabled = false
            };
            UIStyles.ApplyButtonStyle(_previousButton, true);
            _previousButton.Click += PreviousButton_Click;

            _finishButton = new Button
            {
                Text = "Finish",
                Width = 100,
                Height = 35,
                Visible = false
            };
            UIStyles.ApplyButtonStyle(_finishButton, true);
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
                    ShowDetailsStep();
                    break;
                case 2:
                    ShowAdditionalInfoStep();
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
                1 => "Vehicle Details",
                2 => "Additional Information",
                _ => string.Empty
            };
        }

        private void ShowBasicInfoStep()
        {
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 3,
                Padding = new Padding(20),
                ColumnStyles = {
                    new ColumnStyle(SizeType.Percent, 30),
                    new ColumnStyle(SizeType.Percent, 70)
                }
            };

            // Make field
            layout.Controls.Add(new Label { Text = "Make:", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 0);
            _makeTextBox = new TextBox { Width = 400, Anchor = AnchorStyles.Left };
            layout.Controls.Add(_makeTextBox, 1, 0);

            // Model field
            layout.Controls.Add(new Label { Text = "Model:", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 1);
            _modelTextBox = new TextBox { Width = 400, Anchor = AnchorStyles.Left };
            layout.Controls.Add(_modelTextBox, 1, 1);

            // Year field
            layout.Controls.Add(new Label { Text = "Year:", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 2);
            _yearTextBox = new TextBox { Width = 400, Anchor = AnchorStyles.Left };
            layout.Controls.Add(_yearTextBox, 1, 2);

            _contentPanel.Controls.Add(layout);
        }

        private void ShowDetailsStep()
        {
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 3,
                Padding = new Padding(20),
                ColumnStyles = {
                    new ColumnStyle(SizeType.Percent, 30),
                    new ColumnStyle(SizeType.Percent, 70)
                }
            };

            // VIN field
            layout.Controls.Add(new Label { Text = "VIN:", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 0);
            _vinTextBox = new TextBox { Width = 400, Anchor = AnchorStyles.Left };
            layout.Controls.Add(_vinTextBox, 1, 0);

            // License Plate field
            layout.Controls.Add(new Label { Text = "License Plate:", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 1);
            _licensePlateTextBox = new TextBox { Width = 400, Anchor = AnchorStyles.Left };
            layout.Controls.Add(_licensePlateTextBox, 1, 1);

            // Current Mileage field
            layout.Controls.Add(new Label { Text = "Current Mileage:", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 2);
            _mileageTextBox = new TextBox { Width = 400, Anchor = AnchorStyles.Left };
            layout.Controls.Add(_mileageTextBox, 1, 2);

            _contentPanel.Controls.Add(layout);
        }

        private void ShowAdditionalInfoStep()
        {
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 3,
                Padding = new Padding(20),
                ColumnStyles = {
                    new ColumnStyle(SizeType.Percent, 30),
                    new ColumnStyle(SizeType.Percent, 70)
                }
            };

            // Fuel Type field
            layout.Controls.Add(new Label { Text = "Fuel Type:", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 0);
            _fuelTypeComboBox = new ComboBox
            {
                Width = 400,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Anchor = AnchorStyles.Left
            };
            _fuelTypeComboBox.Items.AddRange(new string[] { "Gasoline", "Diesel", "Electric", "Hybrid" });
            _fuelTypeComboBox.SelectedIndex = 0;
            layout.Controls.Add(_fuelTypeComboBox, 1, 0);

            // Purchase Date field
            layout.Controls.Add(new Label { Text = "Purchase Date:", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 1);
            _purchaseDatePicker = new DateTimePicker { Width = 400, Anchor = AnchorStyles.Left };
            layout.Controls.Add(_purchaseDatePicker, 1, 1);

            // Notes field
            layout.Controls.Add(new Label { Text = "Notes:", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 2);
            _notesTextBox = new TextBox
            {
                Width = 400,
                Height = 100,
                Multiline = true,
                Anchor = AnchorStyles.Left
            };
            layout.Controls.Add(_notesTextBox, 1, 2);

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
                SaveVehicle();
            }
        }

        private bool ValidateCurrentStep()
        {
            switch (_currentStep)
            {
                case 0:
                    if (string.IsNullOrWhiteSpace(_makeTextBox.Text) ||
                        string.IsNullOrWhiteSpace(_modelTextBox.Text) ||
                        string.IsNullOrWhiteSpace(_yearTextBox.Text))
                    {
                        MessageBox.Show("Please fill in all basic information fields.", "Validation Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return false;
                    }
                    if (!int.TryParse(_yearTextBox.Text, out int year) || year < 1900 || year > DateTime.Now.Year)
                    {
                        MessageBox.Show("Please enter a valid year.", "Validation Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return false;
                    }
                    break;

                case 1:
                    if (string.IsNullOrWhiteSpace(_vinTextBox.Text) ||
                        string.IsNullOrWhiteSpace(_licensePlateTextBox.Text) ||
                        string.IsNullOrWhiteSpace(_mileageTextBox.Text))
                    {
                        MessageBox.Show("Please fill in all vehicle details.", "Validation Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return false;
                    }
                    if (!double.TryParse(_mileageTextBox.Text, out double mileage) || mileage < 0)
                    {
                        MessageBox.Show("Please enter a valid mileage.", "Validation Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return false;
                    }
                    break;
            }
            return true;
        }

        private void SaveVehicle()
        {
            try
            {
                var vehicle = _existingVehicle ?? new Vehicle
                {
                    Make = _makeTextBox.Text,
                    Model = _modelTextBox.Text,
                    Year = int.Parse(_yearTextBox.Text),
                    VIN = _vinTextBox.Text,
                    CurrentMileage = (int)double.Parse(_mileageTextBox.Text),
                    LicensePlate = _licensePlateTextBox.Text,
                    FuelType = _fuelTypeComboBox.SelectedItem?.ToString() ?? "Gasoline",
                    PurchaseDate = _purchaseDatePicker.Value,
                    Notes = _notesTextBox.Text,
                    UserId = _userId,
                    User = _dbService.GetUserById(_userId)
                };

                if (_existingVehicle == null)
                {
                    _dbService.AddVehicle(vehicle);
                }
                else
                {
                    _dbService.UpdateVehicle(vehicle);
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving vehicle: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Loads existing vehicle data into the form fields when editing.
        /// This method is called when editing an existing vehicle to populate the form with current data.
        /// </summary>
        private void LoadExistingVehicle()
        {
            // Update form title to indicate edit mode
            this.Text = "Edit Vehicle - Step 1 of 3";
            
            // Populate all fields with existing vehicle data
            _makeTextBox.Text = _existingVehicle.Make;
            _modelTextBox.Text = _existingVehicle.Model;
            _yearTextBox.Text = _existingVehicle.Year.ToString();
            _vinTextBox.Text = _existingVehicle.VIN;
            _mileageTextBox.Text = _existingVehicle.CurrentMileage.ToString();
            _licensePlateTextBox.Text = _existingVehicle.LicensePlate;
            _fuelTypeComboBox.SelectedItem = _existingVehicle.FuelType;
            _purchaseDatePicker.Value = _existingVehicle.PurchaseDate;
            _notesTextBox.Text = _existingVehicle.Notes;
        }
    }
} 