using System;
using System.Windows.Forms;
using AutoCarePro.UI;
using AutoCarePro.Models;
using AutoCarePro.Services;

namespace AutoCarePro.Forms
{
    /// <summary>
    /// Form that allows users to add maintenance recommendations for vehicles.
    /// This form provides a step-by-step wizard interface for:
    /// 1. Selecting the component and priority
    /// 2. Entering detailed description and estimated cost
    /// 3. Reviewing and saving the recommendation
    /// </summary>
    public partial class RecommendationForm : Form
    {
        // Core data fields
        private readonly int _vehicleId;
        private readonly DatabaseService _dbService;
        private readonly User _currentUser;
        private int _currentStep = 0;
        private const int TotalSteps = 3;

        // UI controls
        private TextBox _componentTextBox = new();
        private TextBox _descriptionTextBox = new();
        private TextBox _estimatedCostTextBox = new();
        private ComboBox _priorityComboBox = new();
        private DateTimePicker _recommendedDatePicker = new();
        private TextBox _notesTextBox = new();

        // Wizard navigation controls
        private Button _nextButton = new Button();
        private Button _previousButton = new Button();
        private Button _finishButton = new Button();
        private Label _stepLabel = new Label();
        private Panel _contentPanel = new Panel();

        public RecommendationForm(int vehicleId, User currentUser)
        {
            _vehicleId = vehicleId;
            _currentUser = currentUser;
            _dbService = new DatabaseService();
            InitializeForm();
        }

        private void InitializeForm()
        {
            // Configure main form properties
            this.Text = "Add Maintenance Recommendation - Step 1 of 3";
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
                1 => "Details & Cost",
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
                ColumnStyles = {
                    new ColumnStyle(SizeType.Percent, 30),
                    new ColumnStyle(SizeType.Percent, 70)
                }
            };

            // Component field
            layout.Controls.Add(new Label { Text = "Component:", AutoSize = true }, 0, 0);
            _componentTextBox = new TextBox { Width = 400, Anchor = AnchorStyles.Left };
            layout.Controls.Add(_componentTextBox, 1, 0);

            // Priority field
            layout.Controls.Add(new Label { Text = "Priority:", AutoSize = true }, 0, 1);
            _priorityComboBox = new ComboBox
            {
                Width = 400,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Anchor = AnchorStyles.Left
            };
            _priorityComboBox.Items.AddRange(Enum.GetNames(typeof(PriorityLevel)));
            _priorityComboBox.SelectedIndex = 0;
            layout.Controls.Add(_priorityComboBox, 1, 1);

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

            // Description field
            layout.Controls.Add(new Label { Text = "Description:", AutoSize = true }, 0, 0);
            _descriptionTextBox = new TextBox
            {
                Width = 400,
                Height = 100,
                Multiline = true,
                Anchor = AnchorStyles.Left
            };
            layout.Controls.Add(_descriptionTextBox, 1, 0);

            // Estimated cost field
            layout.Controls.Add(new Label { Text = "Estimated Cost:", AutoSize = true }, 0, 1);
            _estimatedCostTextBox = new TextBox { Width = 400, Anchor = AnchorStyles.Left };
            layout.Controls.Add(_estimatedCostTextBox, 1, 1);

            // Recommended date field
            layout.Controls.Add(new Label { Text = "Recommended Date:", AutoSize = true }, 0, 2);
            _recommendedDatePicker = new DateTimePicker { Width = 400, Anchor = AnchorStyles.Left };
            layout.Controls.Add(_recommendedDatePicker, 1, 2);

            _contentPanel.Controls.Add(layout);
        }

        private void ShowReviewStep()
        {
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2,
                Padding = new Padding(20),
                ColumnStyles = {
                    new ColumnStyle(SizeType.Percent, 30),
                    new ColumnStyle(SizeType.Percent, 70)
                }
            };

            // Notes field
            layout.Controls.Add(new Label { Text = "Additional Notes:", AutoSize = true }, 0, 0);
            _notesTextBox = new TextBox
            {
                Width = 400,
                Height = 100,
                Multiline = true,
                Anchor = AnchorStyles.Left
            };
            layout.Controls.Add(_notesTextBox, 1, 0);

            // Summary panel
            var summaryPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(10)
            };

            var summaryLabel = new Label
            {
                Text = $"Component: {_componentTextBox.Text}\n" +
                      $"Priority: {_priorityComboBox.SelectedItem}\n" +
                      $"Description: {_descriptionTextBox.Text}\n" +
                      $"Estimated Cost: {_estimatedCostTextBox.Text}\n" +
                      $"Recommended Date: {_recommendedDatePicker.Value.ToShortDateString()}",
                AutoSize = true,
                Dock = DockStyle.Top
            };

            summaryPanel.Controls.Add(summaryLabel);
            layout.Controls.Add(summaryPanel, 1, 1);

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
                SaveRecommendation();
            }
        }

        private bool ValidateCurrentStep()
        {
            switch (_currentStep)
            {
                case 0:
                    if (string.IsNullOrWhiteSpace(_componentTextBox.Text))
                    {
                        MessageBox.Show("Please enter the component name.", "Validation Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return false;
                    }
                    break;

                case 1:
                    if (string.IsNullOrWhiteSpace(_descriptionTextBox.Text))
                    {
                        MessageBox.Show("Please enter a description of the maintenance needed.", "Validation Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return false;
                    }

                    if (!decimal.TryParse(_estimatedCostTextBox.Text, out decimal cost) || cost < 0)
                    {
                        MessageBox.Show("Please enter a valid estimated cost.", "Validation Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return false;
                    }
                    break;
            }
            return true;
        }

        private void SaveRecommendation()
        {
            try
            {
                var vehicle = _dbService.GetVehicleById(_vehicleId);
                if (vehicle == null)
                {
                    MessageBox.Show("Vehicle not found.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var recommendation = new MaintenanceRecommendation
                {
                    Vehicle = vehicle,
                    Component = _componentTextBox.Text,
                    Description = _descriptionTextBox.Text,
                    Priority = (PriorityLevel)Enum.Parse(typeof(PriorityLevel), _priorityComboBox.SelectedItem.ToString()),
                    EstimatedCost = decimal.Parse(_estimatedCostTextBox.Text),
                    RecommendedDate = _recommendedDatePicker.Value,
                    Notes = _notesTextBox.Text
                };

                _dbService.AddMaintenanceRecommendation(recommendation);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving recommendation: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
} 