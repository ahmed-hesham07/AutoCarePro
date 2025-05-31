using System;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;
using AutoCarePro.Models;
using AutoCarePro.Services;
using AutoCarePro.UI;

namespace AutoCarePro.Forms
{
    /// <summary>
    /// Form that allows maintenance centers to create and manage vehicle diagnoses.
    /// This form provides a comprehensive interface for:
    /// 1. Adding multiple maintenance recommendations for a vehicle
    /// 2. Specifying component issues, descriptions, and priorities
    /// 3. Estimating costs and recommended service dates
    /// 4. Managing a list of recommendations before saving
    /// 
    /// The form is organized into two main sections:
    /// - Input panel: For entering new recommendations
    /// - Recommendations grid: For viewing and managing added recommendations
    /// </summary>
    public partial class DiagnosisForm : Form
    {
        // Core data fields for managing diagnoses
        private readonly int _vehicleId;           // ID of the vehicle being diagnosed
        private readonly int _diagnosedByUserId;   // ID of the maintenance center user
        private readonly DatabaseService _dbService; // Service for database operations
        private readonly List<DiagnosisRecommendation> _recommendations; // List of recommendations
        private int _currentStep = 0;
        private const int TotalSteps = 3;

        // UI controls for data entry and display
        private DataGridView _recommendationsGrid = null!;    // Grid showing all recommendations
        private TextBox _componentTextBox = null!;           // For entering affected component
        private TextBox _descriptionTextBox = null!;         // For entering issue description
        private TextBox _estimatedCostTextBox = null!;       // For entering estimated repair cost
        private ComboBox _priorityComboBox = null!;          // For selecting issue priority
        private DateTimePicker _recommendedDatePicker = null!; // For selecting recommended service date

        // Wizard navigation controls
        private Button _nextButton = new Button();
        private Button _previousButton = new Button();
        private Button _finishButton = new Button();
        private Label _stepLabel = new Label();
        private Panel _contentPanel = new Panel();

        // New controls for fade-in animation
        private System.Windows.Forms.Timer _fadeInTimer;
        private double _fadeStep = 0.08;

        /// <summary>
        /// Initializes the diagnosis form for a specific vehicle.
        /// This constructor sets up the form and prepares it for adding maintenance recommendations.
        /// </summary>
        /// <param name="vehicleId">ID of the vehicle to diagnose</param>
        /// <param name="diagnosedByUserId">ID of the maintenance center user creating the diagnosis</param>
        public DiagnosisForm(int vehicleId, int diagnosedByUserId)
        {
            InitializeComponent();
            _vehicleId = vehicleId;
            _diagnosedByUserId = diagnosedByUserId;
            _dbService = new DatabaseService();
            _recommendations = new List<DiagnosisRecommendation>();
            InitializeForm();
        }

        /// <summary>
        /// Sets up the main form layout and initializes all components.
        /// This method creates a structured layout with input fields and a recommendations grid.
        /// </summary>
        private void InitializeForm()
        {
            // Configure main form properties
            UIStyles.ApplyFormStyle(this);
            this.Text = "Vehicle Diagnosis - Step 1 of 3";
            this.Size = new Size(1000, 800); // Increased height for better visibility
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
                    ShowIssueDetailsStep();
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
                1 => "Issue Details",
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

            // Component field
            var lblComponent = new Label { Text = "Component:", AutoSize = true };
            UIStyles.ApplyLabelStyle(lblComponent);
            layout.Controls.Add(lblComponent, 0, 0);
            _componentTextBox = new TextBox { Width = 400, Anchor = AnchorStyles.Left };
            UIStyles.ApplyTextBoxStyle(_componentTextBox);
            layout.Controls.Add(_componentTextBox, 1, 0);

            // Priority field
            var lblPriority = new Label { Text = "Priority:", AutoSize = true };
            UIStyles.ApplyLabelStyle(lblPriority);
            layout.Controls.Add(lblPriority, 0, 1);
            _priorityComboBox = new ComboBox
            {
                Width = 400,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Anchor = AnchorStyles.Left
            };
            _priorityComboBox.Items.AddRange(new string[] { "Low", "Medium", "High", "Critical" });
            _priorityComboBox.SelectedIndex = 0;
            UIStyles.ApplyComboBoxStyle(_priorityComboBox);
            layout.Controls.Add(_priorityComboBox, 1, 1);

            _contentPanel.Controls.Add(layout);
        }

        private void ShowIssueDetailsStep()
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

            // Description field
            var lblDescription = new Label { Text = "Description:", AutoSize = true };
            UIStyles.ApplyLabelStyle(lblDescription);
            layout.Controls.Add(lblDescription, 0, 0);
            _descriptionTextBox = new TextBox
            {
                Width = 400,
                Height = 100,
                Multiline = true,
                Anchor = AnchorStyles.Left
            };
            UIStyles.ApplyTextBoxStyle(_descriptionTextBox);
            layout.Controls.Add(_descriptionTextBox, 1, 0);

            // Estimated cost field
            var lblCost = new Label { Text = "Estimated Cost:", AutoSize = true };
            UIStyles.ApplyLabelStyle(lblCost);
            layout.Controls.Add(lblCost, 0, 1);
            _estimatedCostTextBox = new TextBox { Width = 400, Anchor = AnchorStyles.Left };
            UIStyles.ApplyTextBoxStyle(_estimatedCostTextBox);
            layout.Controls.Add(_estimatedCostTextBox, 1, 1);

            // Recommended date field
            var lblDate = new Label { Text = "Recommended Date:", AutoSize = true };
            UIStyles.ApplyLabelStyle(lblDate);
            layout.Controls.Add(lblDate, 0, 2);
            _recommendedDatePicker = new DateTimePicker { Width = 400, Anchor = AnchorStyles.Left };
            layout.Controls.Add(_recommendedDatePicker, 1, 2);

            _contentPanel.Controls.Add(layout);
        }

        private void ShowReviewStep()
        {
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(20),
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = Color.White
            };

            // Add recommendation button
            var addButton = new Button
            {
                Text = "Add Recommendation",
                Width = 200,
                Height = 35,
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };
            UIStyles.ApplyButtonStyle(addButton, true);
            addButton.Click += AddRecommendation_Click;
            layout.Controls.Add(addButton, 0, 0);

            // Recommendations grid
            _recommendationsGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            _recommendationsGrid.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name = "Component", HeaderText = "Component" },
                new DataGridViewTextBoxColumn { Name = "Description", HeaderText = "Description" },
                new DataGridViewTextBoxColumn { Name = "Priority", HeaderText = "Priority" },
                new DataGridViewTextBoxColumn { Name = "EstimatedCost", HeaderText = "Est. Cost" },
                new DataGridViewTextBoxColumn { Name = "RecommendedDate", HeaderText = "Rec. Date" }
            });

            layout.Controls.Add(_recommendationsGrid, 0, 1);

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
                SaveDiagnosis();
            }
        }

        private bool ValidateCurrentStep()
        {
            switch (_currentStep)
            {
                case 0:
                    if (string.IsNullOrWhiteSpace(_componentTextBox.Text))
                    {
                        MessageBox.Show("Please enter the affected component.", "Validation Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return false;
                    }
                    break;

                case 1:
                    if (string.IsNullOrWhiteSpace(_descriptionTextBox.Text))
                    {
                        MessageBox.Show("Please enter a description of the issue.", "Validation Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return false;
                    }

                    if (!decimal.TryParse(_estimatedCostTextBox.Text, out decimal estimatedCost) || estimatedCost < 0)
                    {
                        MessageBox.Show("Please enter a valid estimated cost.", "Validation Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return false;
                    }
                    break;

                case 2:
                    if (_recommendations.Count == 0)
                    {
                        MessageBox.Show("Please add at least one recommendation before saving.", "Validation Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return false;
                    }
                    break;
            }
            return true;
        }

        private void AddRecommendation_Click(object sender, EventArgs e)
        {
            try
            {
                // Create new recommendation
                var recommendation = new DiagnosisRecommendation
                {
                    Component = _componentTextBox.Text,
                    Description = _descriptionTextBox.Text,
                    Priority = (PriorityLevel)Enum.Parse(typeof(PriorityLevel), _priorityComboBox.SelectedItem.ToString()),
                    EstimatedCost = decimal.Parse(_estimatedCostTextBox.Text),
                    RecommendedDate = _recommendedDatePicker.Value,
                    DiagnosedByUserId = _diagnosedByUserId,
                    DiagnosedByUser = _dbService.GetUserById(_diagnosedByUserId),
                    Notes = "Diagnosis recommendation added",
                    RecommendedAction = "Repair or replace affected component",
                    MaintenanceRecord = null!
                };

                // Add to list and grid
                _recommendations.Add(recommendation);
                _recommendationsGrid.Rows.Add(
                    recommendation.Component,
                    recommendation.Description,
                    recommendation.Priority.ToString(),
                    recommendation.EstimatedCost.ToString("C"),
                    recommendation.RecommendedDate.ToShortDateString()
                );

                // Clear input fields
                _componentTextBox.Clear();
                _descriptionTextBox.Clear();
                _estimatedCostTextBox.Clear();
                _priorityComboBox.SelectedIndex = 0;
                _recommendedDatePicker.Value = DateTime.Now;

                // Go back to first step
                ShowStep(0);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding recommendation: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveDiagnosis()
        {
            try
            {
                // Create maintenance record
                var maintenanceRecord = new MaintenanceRecord
                {
                    VehicleId = _vehicleId,
                    Vehicle = _dbService.GetVehicleById(_vehicleId),
                    MaintenanceDate = DateTime.Now,
                    MaintenanceType = "Diagnosis",
                    Description = "Vehicle diagnosis with recommendations",
                    MileageAtMaintenance = 0,
                    Cost = 0,
                    ServiceProvider = "AutoCarePro",
                    Notes = "Diagnosis performed by maintenance center",
                    IsCompleted = false,
                    DiagnosedByUserId = _diagnosedByUserId
                };

                // Add recommendations to the maintenance record
                foreach (var rec in _recommendations)
                {
                    rec.MaintenanceRecord = maintenanceRecord;
                }
                maintenanceRecord.DiagnosisRecommendations = _recommendations;

                // Save to database
                _dbService.AddMaintenanceRecord(maintenanceRecord);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving diagnosis: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
} 