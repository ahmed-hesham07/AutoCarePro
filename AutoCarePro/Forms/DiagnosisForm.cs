using System;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;
using AutoCarePro.Models;
using AutoCarePro.Services;

namespace AutoCarePro.Forms
{
    public partial class DiagnosisForm : Form
    {
        private readonly int _vehicleId;
        private readonly int _diagnosedByUserId;
        private readonly DatabaseService _dbService;
        private readonly List<DiagnosisRecommendation> _recommendations;
        private DataGridView _recommendationsGrid;
        private TextBox _componentTextBox;
        private TextBox _descriptionTextBox;
        private TextBox _estimatedCostTextBox;
        private ComboBox _priorityComboBox;
        private DateTimePicker _recommendedDatePicker;

        public DiagnosisForm(int vehicleId, int diagnosedByUserId)
        {
            InitializeComponent();
            _vehicleId = vehicleId;
            _diagnosedByUserId = diagnosedByUserId;
            _dbService = new DatabaseService();
            _recommendations = new List<DiagnosisRecommendation>();
            InitializeForm();
        }

        private void InitializeForm()
        {
            // Set up the form
            this.Text = "Vehicle Diagnosis";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Create main layout
            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(10)
            };

            // Form fields panel
            var fieldsPanel = new Panel { Dock = DockStyle.Fill };
            InitializeFields(fieldsPanel);

            // Buttons panel
            var buttonsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(10)
            };
            InitializeButtons(buttonsPanel);

            // Add panels to main layout
            mainPanel.Controls.Add(fieldsPanel);
            mainPanel.Controls.Add(buttonsPanel);

            // Add main panel to form
            this.Controls.Add(mainPanel);
        }

        private void InitializeFields(Panel panel)
        {
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2,
                Padding = new Padding(10)
            };

            // Left panel for input fields
            var inputPanel = new Panel { Dock = DockStyle.Fill };
            InitializeInputFields(inputPanel);
            layout.Controls.Add(inputPanel, 0, 0);
            layout.SetRowSpan(inputPanel, 2);

            // Right panel for recommendations grid
            var gridPanel = new Panel { Dock = DockStyle.Fill };
            InitializeRecommendationsGrid(gridPanel);
            layout.Controls.Add(gridPanel, 1, 0);
            layout.SetRowSpan(gridPanel, 2);

            panel.Controls.Add(layout);
        }

        private void InitializeInputFields(Panel panel)
        {
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 6,
                Padding = new Padding(10)
            };

            // Component
            layout.Controls.Add(new Label { Text = "Component:", AutoSize = true }, 0, 0);
            _componentTextBox = new TextBox { Width = 200 };
            layout.Controls.Add(_componentTextBox, 1, 0);

            // Description
            layout.Controls.Add(new Label { Text = "Description:", AutoSize = true }, 0, 1);
            _descriptionTextBox = new TextBox
            {
                Width = 200,
                Height = 100,
                Multiline = true
            };
            layout.Controls.Add(_descriptionTextBox, 1, 1);

            // Priority
            layout.Controls.Add(new Label { Text = "Priority:", AutoSize = true }, 0, 2);
            _priorityComboBox = new ComboBox
            {
                Width = 200,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _priorityComboBox.Items.AddRange(new string[] { "Low", "Medium", "High", "Critical" });
            _priorityComboBox.SelectedIndex = 0;
            layout.Controls.Add(_priorityComboBox, 1, 2);

            // Estimated Cost
            layout.Controls.Add(new Label { Text = "Estimated Cost:", AutoSize = true }, 0, 3);
            _estimatedCostTextBox = new TextBox { Width = 200 };
            layout.Controls.Add(_estimatedCostTextBox, 1, 3);

            // Recommended Date
            layout.Controls.Add(new Label { Text = "Recommended Date:", AutoSize = true }, 0, 4);
            _recommendedDatePicker = new DateTimePicker { Width = 200 };
            layout.Controls.Add(_recommendedDatePicker, 1, 4);

            // Add Recommendation Button
            var addButton = new Button
            {
                Text = "Add Recommendation",
                Width = 200
            };
            addButton.Click += AddRecommendation_Click;
            layout.Controls.Add(addButton, 1, 5);

            panel.Controls.Add(layout);
        }

        private void InitializeRecommendationsGrid(Panel panel)
        {
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

            panel.Controls.Add(_recommendationsGrid);
        }

        private void InitializeButtons(FlowLayoutPanel panel)
        {
            var saveButton = new Button
            {
                Text = "Save Diagnosis",
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

        private void AddRecommendation_Click(object sender, EventArgs e)
        {
            try
            {
                // Validate inputs
                if (string.IsNullOrWhiteSpace(_componentTextBox.Text))
                {
                    MessageBox.Show("Please enter a component.", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(_descriptionTextBox.Text))
                {
                    MessageBox.Show("Please enter a description.", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!decimal.TryParse(_estimatedCostTextBox.Text, out decimal estimatedCost))
                {
                    MessageBox.Show("Please enter a valid estimated cost.", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Create new recommendation
                var recommendation = new DiagnosisRecommendation
                {
                    Component = _componentTextBox.Text,
                    Description = _descriptionTextBox.Text,
                    Priority = _priorityComboBox.SelectedItem.ToString(),
                    EstimatedCost = estimatedCost,
                    RecommendedDate = _recommendedDatePicker.Value,
                    IsCompleted = false
                };

                // Add to list and grid
                _recommendations.Add(recommendation);
                _recommendationsGrid.Rows.Add(
                    recommendation.Component,
                    recommendation.Description,
                    recommendation.Priority,
                    recommendation.EstimatedCost.ToString("C"),
                    recommendation.RecommendedDate.ToShortDateString()
                );

                // Clear input fields
                _componentTextBox.Clear();
                _descriptionTextBox.Clear();
                _estimatedCostTextBox.Clear();
                _priorityComboBox.SelectedIndex = 0;
                _recommendedDatePicker.Value = DateTime.Now.AddDays(7);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding recommendation: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (_recommendations.Count == 0)
                {
                    MessageBox.Show("Please add at least one recommendation.", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Create maintenance record
                var record = new MaintenanceRecord
                {
                    VehicleId = _vehicleId,
                    DiagnosedByUserId = _diagnosedByUserId,
                    MaintenanceType = "Diagnosis",
                    Description = "Vehicle diagnosis with recommendations",
                    MaintenanceDate = DateTime.Now,
                    HasDiagnosisRecommendations = true
                };

                // Save maintenance record and recommendations
                _dbService.AddMaintenanceRecord(record);
                _dbService.SaveChanges();

                foreach (var recommendation in _recommendations)
                {
                    recommendation.MaintenanceRecordId = record.Id;
                    recommendation.DiagnosedByUserId = _diagnosedByUserId;
                    _dbService.AddDiagnosisRecommendation(recommendation);
                }
                _dbService.SaveChanges();

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