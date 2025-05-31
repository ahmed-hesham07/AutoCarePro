using System;
using System.Windows.Forms;
using System.Drawing;
using System.Linq;
using AutoCarePro.Models;
using AutoCarePro.Services;
using AutoCarePro.UI;

namespace AutoCarePro.Forms
{
    public partial class MaintenanceRecordsForm : BaseForm
    {
        private readonly DatabaseService _dbService;
        private readonly User _currentUser;
        private DataGridView _recordsGrid;
        private Button _addRecordButton;
        private Button _editRecordButton;
        private Button _deleteRecordButton;
        private Button _viewDiagnosisButton;
        private ComboBox _vehicleFilterComboBox;
        private ComboBox _typeFilterComboBox;
        private DateTimePicker _dateFromPicker;
        private DateTimePicker _dateToPicker;
        private Button _applyFiltersButton;
        private Button _clearFiltersButton;
        private List<MaintenanceRecord> _records;

        public MaintenanceRecordsForm(User user)
        {
            InitializeComponent();
            _currentUser = user;
            _dbService = new DatabaseService();
            InitializeUI();
            LoadMaintenanceRecords();
        }

        private void InitializeUI()
        {
            this.Text = "Maintenance Records";
            this.Size = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Create toolbar panel
            var toolbarPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 100,
                BackColor = Color.FromArgb(240, 240, 240)
            };

            // Create filter controls
            var filterPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = Color.FromArgb(240, 240, 240)
            };

            // Vehicle filter
            filterPanel.Controls.Add(new Label { Text = "Vehicle:", Location = new Point(10, 15) });
            _vehicleFilterComboBox = new ComboBox
            {
                Location = new Point(60, 12),
                Width = 150,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            filterPanel.Controls.Add(_vehicleFilterComboBox);

            // Type filter
            filterPanel.Controls.Add(new Label { Text = "Type:", Location = new Point(220, 15) });
            _typeFilterComboBox = new ComboBox
            {
                Location = new Point(260, 12),
                Width = 120,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            filterPanel.Controls.Add(_typeFilterComboBox);

            // Date range filters
            filterPanel.Controls.Add(new Label { Text = "From:", Location = new Point(390, 15) });
            _dateFromPicker = new DateTimePicker
            {
                Location = new Point(430, 12),
                Width = 120,
                Format = DateTimePickerFormat.Short
            };
            filterPanel.Controls.Add(_dateFromPicker);

            filterPanel.Controls.Add(new Label { Text = "To:", Location = new Point(560, 15) });
            _dateToPicker = new DateTimePicker
            {
                Location = new Point(590, 12),
                Width = 120,
                Format = DateTimePickerFormat.Short
            };
            filterPanel.Controls.Add(_dateToPicker);

            // Filter buttons
            _applyFiltersButton = new Button
            {
                Text = "Apply Filters",
                Location = new Point(720, 10),
                Width = 100
            };
            UIStyles.ApplyButtonStyle(_applyFiltersButton);
            filterPanel.Controls.Add(_applyFiltersButton);

            _clearFiltersButton = new Button
            {
                Text = "Clear Filters",
                Location = new Point(830, 10),
                Width = 100
            };
            UIStyles.ApplyButtonStyle(_clearFiltersButton);
            filterPanel.Controls.Add(_clearFiltersButton);

            // Create action buttons
            _addRecordButton = new Button
            {
                Text = "Add Record",
                Location = new Point(10, 10),
                Size = new Size(100, 30)
            };
            UIStyles.ApplyButtonStyle(_addRecordButton);

            _editRecordButton = new Button
            {
                Text = "Edit Record",
                Location = new Point(120, 10),
                Size = new Size(100, 30),
                Enabled = false
            };
            UIStyles.ApplyButtonStyle(_editRecordButton);

            _deleteRecordButton = new Button
            {
                Text = "Delete Record",
                Location = new Point(230, 10),
                Size = new Size(100, 30),
                Enabled = false
            };
            UIStyles.ApplyButtonStyle(_deleteRecordButton);

            _viewDiagnosisButton = new Button
            {
                Text = "View Diagnosis",
                Location = new Point(340, 10),
                Size = new Size(120, 30),
                Enabled = false
            };
            UIStyles.ApplyButtonStyle(_viewDiagnosisButton);

            // Add buttons to toolbar
            toolbarPanel.Controls.AddRange(new Control[] { 
                _addRecordButton, _editRecordButton, _deleteRecordButton, _viewDiagnosisButton 
            });

            // Create records grid
            _recordsGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            UIStyles.ApplyDataGridViewStyle(_recordsGrid);

            // Add event handlers
            _addRecordButton.Click += AddRecordButton_Click;
            _editRecordButton.Click += EditRecordButton_Click;
            _deleteRecordButton.Click += DeleteRecordButton_Click;
            _viewDiagnosisButton.Click += ViewDiagnosisButton_Click;
            _recordsGrid.SelectionChanged += RecordsGrid_SelectionChanged;
            _applyFiltersButton.Click += ApplyFiltersButton_Click;
            _clearFiltersButton.Click += ClearFiltersButton_Click;

            // Add controls to form
            this.Controls.Add(_recordsGrid);
            this.Controls.Add(filterPanel);
            this.Controls.Add(toolbarPanel);

            // Initialize filters
            InitializeFilters();
        }

        private void InitializeFilters()
        {
            // Load vehicles
            var vehicles = _dbService.GetVehiclesByUserId(_currentUser.Id);
            _vehicleFilterComboBox.Items.Add("All Vehicles");
            foreach (var vehicle in vehicles)
            {
                _vehicleFilterComboBox.Items.Add($"{vehicle.Make} {vehicle.Model} ({vehicle.Year})");
            }
            _vehicleFilterComboBox.SelectedIndex = 0;

            // Load maintenance types
            _typeFilterComboBox.Items.Add("All Types");
            _typeFilterComboBox.Items.AddRange(new string[] {
                "Oil Change",
                "Brake Service",
                "Tire Rotation",
                "Engine Service",
                "Transmission Service",
                "Electrical System",
                "Suspension",
                "Other"
            });
            _typeFilterComboBox.SelectedIndex = 0;

            // Set default date range
            _dateFromPicker.Value = DateTime.Now.AddMonths(-6);
            _dateToPicker.Value = DateTime.Now;
        }

        private async void LoadMaintenanceRecords()
        {
            try
            {
                _records = await _dbService.GetMaintenanceRecordsByUserIdAsync(_currentUser.Id);
                UpdateRecordsGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading records: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateRecordsGrid()
        {
            _recordsGrid.DataSource = null;
            _recordsGrid.DataSource = _records.Select(r => new
            {
                r.Id,
                Vehicle = $"{r.Vehicle.Make} {r.Vehicle.Model} ({r.Vehicle.Year})",
                r.MaintenanceType,
                r.MaintenanceDate,
                r.MileageAtMaintenance,
                r.Cost,
                ServiceProvider = r.ServiceProvider?.FullName ?? "N/A",
                r.IsCompleted,
                HasDiagnosis = r.HasDiagnosisRecommendations ? "Yes" : "No"
            }).ToList();
        }

        private void RecordsGrid_SelectionChanged(object sender, EventArgs e)
        {
            bool hasSelection = _recordsGrid.SelectedRows.Count > 0;
            _editRecordButton.Enabled = hasSelection;
            _deleteRecordButton.Enabled = hasSelection;
            _viewDiagnosisButton.Enabled = hasSelection && 
                _recordsGrid.SelectedRows[0].Cells["HasDiagnosis"].Value.ToString() == "Yes";
        }

        private void AddRecordButton_Click(object sender, EventArgs e)
        {
            using (var form = new MaintenanceRecordForm(_currentUser))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadMaintenanceRecords();
                }
            }
        }

        private void EditRecordButton_Click(object sender, EventArgs e)
        {
            if (_recordsGrid.SelectedRows.Count > 0)
            {
                var recordId = (int)_recordsGrid.SelectedRows[0].Cells["Id"].Value;
                var record = _dbService.GetMaintenanceRecordById(recordId);
                
                if (record != null)
                {
                    using (var form = new MaintenanceRecordForm(_currentUser, record))
                    {
                        if (form.ShowDialog() == DialogResult.OK)
                        {
                            LoadMaintenanceRecords();
                        }
                    }
                }
            }
        }

        private void DeleteRecordButton_Click(object sender, EventArgs e)
        {
            if (_recordsGrid.SelectedRows.Count > 0)
            {
                var recordId = (int)_recordsGrid.SelectedRows[0].Cells["Id"].Value;
                var record = _dbService.GetMaintenanceRecordById(recordId);

                if (record != null)
                {
                    var result = MessageBox.Show(
                        $"Are you sure you want to delete this maintenance record?\n\n" +
                        $"Vehicle: {record.Vehicle.Make} {record.Vehicle.Model}\n" +
                        $"Type: {record.MaintenanceType}\n" +
                        $"Date: {record.MaintenanceDate:d}",
                        "Confirm Delete",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);

                    if (result == DialogResult.Yes)
                    {
                        try
                        {
                            _dbService.DeleteMaintenanceRecord(recordId);
                            LoadMaintenanceRecords();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(
                                $"Error deleting maintenance record: {ex.Message}",
                                "Error",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                        }
                    }
                }
            }
        }

        private void ViewDiagnosisButton_Click(object sender, EventArgs e)
        {
            if (_recordsGrid.SelectedRows.Count > 0)
            {
                var recordId = (int)_recordsGrid.SelectedRows[0].Cells["Id"].Value;
                var record = _dbService.GetMaintenanceRecordById(recordId);

                if (record != null && record.HasDiagnosisRecommendations)
                {
                    using (var form = new DiagnosisForm(record))
                    {
                        form.ShowDialog();
                    }
                }
            }
        }

        private void ApplyFiltersButton_Click(object sender, EventArgs e)
        {
            LoadMaintenanceRecords();
        }

        private void ClearFiltersButton_Click(object sender, EventArgs e)
        {
            _vehicleFilterComboBox.SelectedIndex = 0;
            _typeFilterComboBox.SelectedIndex = 0;
            _dateFromPicker.Value = DateTime.Now.AddMonths(-6);
            _dateToPicker.Value = DateTime.Now;
            LoadMaintenanceRecords();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            _dbService.Dispose();
        }
    }
} 