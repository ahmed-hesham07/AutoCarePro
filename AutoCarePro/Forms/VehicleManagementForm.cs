using System;
using System.Windows.Forms;
using System.Drawing;
using System.Linq;
using AutoCarePro.Models;
using AutoCarePro.Services;
using AutoCarePro.UI;

namespace AutoCarePro.Forms
{
    public partial class VehicleManagementForm : BaseForm
    {
        private readonly DatabaseService _dbService;
        private readonly User _currentUser;
        private DataGridView _vehiclesGrid = null!;
        private Button _addVehicleButton = null!;
        private Button _editVehicleButton = null!;
        private Button _deleteVehicleButton = null!;
        private Button _viewMaintenanceButton = null!;

        public VehicleManagementForm(User user)
        {
            InitializeComponent();
            _currentUser = user;
            _dbService = new DatabaseService();
            InitializeUI();
            LoadVehicles();
        }

        private void InitializeUI()
        {
            this.Text = "Vehicle Management";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Create toolbar panel
            var toolbarPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = Color.FromArgb(240, 240, 240)
            };

            // Create buttons
            _addVehicleButton = new Button
            {
                Text = "Add Vehicle",
                Location = new Point(10, 10),
                Size = new Size(100, 30)
            };
            UIStyles.ApplyButtonStyle(_addVehicleButton);

            _editVehicleButton = new Button
            {
                Text = "Edit Vehicle",
                Location = new Point(120, 10),
                Size = new Size(100, 30),
                Enabled = false
            };
            UIStyles.ApplyButtonStyle(_editVehicleButton);

            _deleteVehicleButton = new Button
            {
                Text = "Delete Vehicle",
                Location = new Point(230, 10),
                Size = new Size(100, 30),
                Enabled = false
            };
            UIStyles.ApplyButtonStyle(_deleteVehicleButton);

            _viewMaintenanceButton = new Button
            {
                Text = "View Maintenance",
                Location = new Point(340, 10),
                Size = new Size(120, 30),
                Enabled = false
            };
            UIStyles.ApplyButtonStyle(_viewMaintenanceButton);

            // Add buttons to toolbar
            toolbarPanel.Controls.AddRange(new Control[] { 
                _addVehicleButton, _editVehicleButton, _deleteVehicleButton, _viewMaintenanceButton 
            });

            // Create vehicles grid
            _vehiclesGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            UIStyles.ApplyDataGridViewStyle(_vehiclesGrid);

            // Add event handlers
            _addVehicleButton.Click += AddVehicleButton_Click;
            _editVehicleButton.Click += EditVehicleButton_Click;
            _deleteVehicleButton.Click += DeleteVehicleButton_Click;
            _viewMaintenanceButton.Click += ViewMaintenanceButton_Click;
            _vehiclesGrid.SelectionChanged += VehiclesGrid_SelectionChanged;

            // Add controls to form
            this.Controls.Add(_vehiclesGrid);
            this.Controls.Add(toolbarPanel);
        }

        private void LoadVehicles()
        {
            var vehicles = _dbService.GetVehiclesByUserId(_currentUser.Id);
            _vehiclesGrid.DataSource = vehicles.Select(v => new
            {
                v.Id,
                v.Make,
                v.Model,
                v.Year,
                v.VIN,
                v.CurrentMileage,
                LastMaintenance = v.LastMaintenanceDate?.ToString("d") ?? "Never",
                NeedsMaintenance = v.NeedsMaintenance ? "Yes" : "No"
            }).ToList();
        }

        private void VehiclesGrid_SelectionChanged(object sender, EventArgs e)
        {
            bool hasSelection = _vehiclesGrid.SelectedRows.Count > 0;
            _editVehicleButton.Enabled = hasSelection;
            _deleteVehicleButton.Enabled = hasSelection;
            _viewMaintenanceButton.Enabled = hasSelection;
        }

        private void AddVehicleButton_Click(object sender, EventArgs e)
        {
            using (var form = new VehicleForm(_currentUser))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadVehicles();
                }
            }
        }

        private void EditVehicleButton_Click(object sender, EventArgs e)
        {
            if (_vehiclesGrid.SelectedRows.Count > 0)
            {
                var vehicleId = (int)_vehiclesGrid.SelectedRows[0].Cells["Id"].Value;
                var vehicle = _dbService.GetVehicleById(vehicleId);
                
                if (vehicle != null)
                {
                    using (var form = new VehicleForm(_currentUser, vehicle))
                    {
                        if (form.ShowDialog() == DialogResult.OK)
                        {
                            LoadVehicles();
                        }
                    }
                }
            }
        }

        private void DeleteVehicleButton_Click(object sender, EventArgs e)
        {
            if (_vehiclesGrid.SelectedRows.Count > 0)
            {
                var vehicleId = (int)_vehiclesGrid.SelectedRows[0].Cells["Id"].Value;
                var vehicle = _dbService.GetVehicleById(vehicleId);

                if (vehicle != null)
                {
                    var result = MessageBox.Show(
                        $"Are you sure you want to delete this vehicle?\n\nMake: {vehicle.Make}\nModel: {vehicle.Model}\nYear: {vehicle.Year}",
                        "Confirm Delete",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);

                    if (result == DialogResult.Yes)
                    {
                        try
                        {
                            _dbService.DeleteVehicle(vehicleId);
                            LoadVehicles();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(
                                $"Error deleting vehicle: {ex.Message}",
                                "Error",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                        }
                    }
                }
            }
        }

        private void ViewMaintenanceButton_Click(object? sender, EventArgs e)
        {
            if (_vehiclesGrid.SelectedRows.Count > 0)
            {
                var vehicleId = (int)_vehiclesGrid.SelectedRows[0].Cells["Id"].Value;
                using (var form = new MaintenanceRecordForm(vehicleId, _dbService))
                {
                    form.ShowDialog();
                }
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            _dbService.Dispose();
        }
    }
} 