using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Linq;
using AutoCarePro.Models;
using AutoCarePro.Services;
using AutoCarePro.UI;
using Microsoft.Extensions.Logging;

namespace AutoCarePro.Forms
{
    /// <summary>
    /// Dashboard form specifically designed for maintenance centers.
    /// This form provides a focused interface for service providers to:
    /// 1. View and manage all vehicles they service
    /// 2. Add maintenance records and diagnoses
    /// 3. View maintenance history
    /// 4. Manage service appointments
    /// 5. Generate maintenance recommendations
    /// </summary>
    public partial class MaintenanceCenterDashboardForm : Form
    {
        private readonly DatabaseService _databaseService;
        private readonly ILogger<MaintenanceCenterDashboardForm> _logger;
        private readonly User _currentUser;

        // UI Controls
        private ListView _vehicleList = new ListView();
        private ListView _maintenanceList = new ListView();
        private ListView _appointmentsList = new ListView();
        private Button _addMaintenanceBtn = new Button();
        private Button _addDiagnosisBtn = new Button();
        private Button _viewHistoryBtn = new Button();
        private Button _profileBtn = new Button();
        private Button _addAppointmentBtn = new Button();
        private Label _welcomeLabel = new Label();
        private ComboBox _filterComboBox = new ComboBox();
        private TextBox _searchUsernameTextBox = new TextBox();
        private Button _searchUsernameBtn = new Button();
        private ToolTip _toolTip = new ToolTip();
        private System.Windows.Forms.Timer _fadeInTimer = new System.Windows.Forms.Timer();
        private double _fadeStep = 0.08;

        public MaintenanceCenterDashboardForm(ILogger<MaintenanceCenterDashboardForm> logger)
        {
            InitializeComponent();
            _logger = logger;
            _databaseService = ServiceFactory.GetDatabaseService();
            _currentUser = SessionManager.CurrentUser;

            if (_currentUser == null)
            {
                _logger.LogError("No user session found");
                MessageBox.Show("Session expired. Please login again.", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
                return;
            }

            SetupForm();
        }

        private void SetupForm()
        {
            // Set form title with center's name
            this.Text = $"Maintenance Center Dashboard - {_currentUser.Username}";

            // Load pending appointments
            LoadPendingAppointments();

            // Load maintenance history
            LoadMaintenanceHistory();

            // Setup event handlers
            AcceptAppointmentButton.Click += AcceptAppointmentButton_Click;
            RejectAppointmentButton.Click += RejectAppointmentButton_Click;
            CompleteMaintenanceButton.Click += CompleteMaintenanceButton_Click;
            ViewHistoryButton.Click += ViewHistoryButton_Click;
            LogoutButton.Click += LogoutButton_Click;

            // Apply form styling
            UIStyles.ApplyFormStyle(this);
            this.Size = new Size(1400, 900);

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
                ColumnCount = 2,
                RowCount = 1,
                ColumnStyles = {
                    new ColumnStyle(SizeType.Percent, 50),
                    new ColumnStyle(SizeType.Percent, 50)
                },
                Padding = new Padding(20)
            };

            // Create left and right panels
            var leftPanel = new Panel { Dock = DockStyle.Fill };
            var rightPanel = new Panel { Dock = DockStyle.Fill };

            // Apply panel styling with transitions
            UIStyles.ApplyPanelStyle(leftPanel, true);
            UIStyles.ApplyPanelStyle(rightPanel, true);

            // Add panels to main layout
            mainPanel.Controls.Add(leftPanel, 0, 0);
            mainPanel.Controls.Add(rightPanel, 1, 0);

            // Initialize the panels
            InitializeLeftPanel(leftPanel);
            InitializeRightPanel(rightPanel);

            // Add main panel to form
            this.Controls.Add(mainPanel);

            // Load initial data
            LoadVehicleData();
            LoadMaintenanceData();
            LoadAppointments();
        }

        private void InitializeLeftPanel(Panel panel)
        {
            // Welcome section
            _welcomeLabel = new Label
            {
                Text = $"Welcome, {_currentUser.FullName}",
                Dock = DockStyle.Top,
                Height = 40,
                Padding = new Padding(10)
            };
            UIStyles.ApplyLabelStyle(_welcomeLabel, true);
            panel.Controls.Add(_welcomeLabel);

            // Search bar for username
            var searchPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 40,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(5),
                BackColor = UIStyles.Colors.Secondary
            };
            var searchLabel = new Label { Text = "Search by Username:", AutoSize = true };
            UIStyles.ApplyLabelStyle(searchLabel);
            _searchUsernameTextBox = new TextBox { Width = 200 };
            UIStyles.ApplyTextBoxStyle(_searchUsernameTextBox);
            _searchUsernameBtn = new Button { Text = "Search", Width = 100, Height = 30 };
            UIStyles.ApplyButtonStyle(_searchUsernameBtn, true);
            _searchUsernameBtn.Click += SearchUsernameBtn_Click;
            searchPanel.Controls.Add(searchLabel);
            searchPanel.Controls.Add(_searchUsernameTextBox);
            searchPanel.Controls.Add(_searchUsernameBtn);
            panel.Controls.Add(searchPanel);

            // Vehicle list section
            var vehicleGroup = new GroupBox
            {
                Text = "Vehicles Under Service",
                Dock = DockStyle.Top,
                Height = 300,
                Padding = new Padding(10)
            };
            UIStyles.ApplyGroupBoxStyle(vehicleGroup);

            // Filter controls
            var filterPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 30,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(5),
                BackColor = UIStyles.Colors.Secondary
            };

            var filterLabel = new Label { Text = "Filter:", AutoSize = true };
            UIStyles.ApplyLabelStyle(filterLabel);
            filterPanel.Controls.Add(filterLabel);

            _filterComboBox = new ComboBox
            {
                Width = 150,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            UIStyles.ApplyComboBoxStyle(_filterComboBox);
            _filterComboBox.Items.AddRange(new string[] { "All Vehicles", "Active Service", "Pending Service", "Completed Service" });
            _filterComboBox.SelectedIndex = 0;
            _filterComboBox.SelectedIndexChanged += FilterComboBox_SelectedIndexChanged;
            filterPanel.Controls.Add(_filterComboBox);

            vehicleGroup.Controls.Add(filterPanel);

            _vehicleList = new ListView();
            UIStyles.ApplyListViewStyle(_vehicleList);
            _vehicleList.Dock = DockStyle.Fill;
            _vehicleList.Columns.Add("Make", 100);
            _vehicleList.Columns.Add("Model", 100);
            _vehicleList.Columns.Add("Year", 50);
            _vehicleList.Columns.Add("Status", 100);
            _vehicleList.Columns.Add("Owner Username", 150);
            _vehicleList.SelectedIndexChanged += VehicleList_SelectedIndexChanged;

            vehicleGroup.Controls.Add(_vehicleList);
            panel.Controls.Add(vehicleGroup);

            // Action buttons
            var buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 50,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(10),
                BackColor = UIStyles.Colors.Secondary
            };

            _addMaintenanceBtn = new Button { Text = "Add Maintenance Record", Width = 150, Height = 35, Enabled = false };
            _addDiagnosisBtn = new Button { Text = "Add Diagnosis Report", Width = 150, Height = 35, Enabled = false };
            _viewHistoryBtn = new Button { Text = "View History", Width = 120, Height = 35, Enabled = false };
            _profileBtn = new Button { Text = "My Profile", Width = 120, Height = 35 };

            // Apply button styling with press animations
            UIStyles.ApplyButtonStyle(_addMaintenanceBtn, true);
            UIStyles.ApplyButtonStyle(_addDiagnosisBtn);
            UIStyles.ApplyButtonStyle(_viewHistoryBtn);
            UIStyles.ApplyButtonStyle(_profileBtn);

            // Add tooltips
            _toolTip.SetToolTip(_addMaintenanceBtn, "Add a new maintenance record for the selected vehicle");
            _toolTip.SetToolTip(_addDiagnosisBtn, "Add a new diagnosis report for the selected vehicle");

            _addMaintenanceBtn.Click += AddMaintenanceBtn_Click;
            _addDiagnosisBtn.Click += AddDiagnosisBtn_Click;
            _viewHistoryBtn.Click += ViewHistoryBtn_Click;
            _profileBtn.Click += ProfileBtn_Click;

            buttonPanel.Controls.AddRange(new Control[] { _addMaintenanceBtn, _addDiagnosisBtn, _viewHistoryBtn, _profileBtn });
            panel.Controls.Add(buttonPanel);
        }

        private void InitializeRightPanel(Panel panel)
        {
            // Maintenance records panel
            var maintenancePanel = new GroupBox
            {
                Text = "Recent Maintenance Records",
                Dock = DockStyle.Top,
                Height = 300,
                Padding = new Padding(10)
            };
            UIStyles.ApplyGroupBoxStyle(maintenancePanel);

            _maintenanceList = new ListView();
            UIStyles.ApplyListViewStyle(_maintenanceList);
            _maintenanceList.Dock = DockStyle.Fill;
            _maintenanceList.Columns.Add("Date", 100);
            _maintenanceList.Columns.Add("Vehicle", 150);
            _maintenanceList.Columns.Add("Type", 100);
            _maintenanceList.Columns.Add("Status", 100);
            _maintenanceList.Columns.Add("Cost", 100);

            maintenancePanel.Controls.Add(_maintenanceList);
            panel.Controls.Add(maintenancePanel);

            // Appointments panel
            var appointmentsPanel = new GroupBox
            {
                Text = "Upcoming Appointments",
                Dock = DockStyle.Bottom,
                Height = 300,
                Padding = new Padding(10)
            };
            UIStyles.ApplyGroupBoxStyle(appointmentsPanel);

            var appointmentsHeader = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 40,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(5),
                BackColor = UIStyles.Colors.Secondary
            };

            _addAppointmentBtn = new Button { Text = "Add Appointment", Width = 120, Height = 35 };
            UIStyles.ApplyButtonStyle(_addAppointmentBtn, true);
            _addAppointmentBtn.Click += AddAppointmentBtn_Click;
            appointmentsHeader.Controls.Add(_addAppointmentBtn);

            appointmentsPanel.Controls.Add(appointmentsHeader);

            _appointmentsList = new ListView();
            UIStyles.ApplyListViewStyle(_appointmentsList);
            _appointmentsList.Dock = DockStyle.Fill;
            _appointmentsList.Columns.Add("Date", 100);
            _appointmentsList.Columns.Add("Time", 100);
            _appointmentsList.Columns.Add("Vehicle", 150);
            _appointmentsList.Columns.Add("Owner", 150);
            _appointmentsList.Columns.Add("Service Type", 150);

            appointmentsPanel.Controls.Add(_appointmentsList);
            panel.Controls.Add(appointmentsPanel);
        }

        private void LoadVehicleData(string usernameFilter = "")
        {
            try
            {
                _vehicleList.Items.Clear();
                var vehicles = _databaseService.GetAllVehicles(); // Get all vehicles for maintenance center
                if (!string.IsNullOrWhiteSpace(usernameFilter))
                {
                    vehicles = vehicles.Where(v => v.User != null && v.User.Username != null && v.User.Username.ToLower().Contains(usernameFilter.ToLower())).ToList();
                }
                foreach (var vehicle in vehicles)
                {
                    var item = new ListViewItem(vehicle.Make);
                    item.SubItems.Add(vehicle.Model);
                    item.SubItems.Add(vehicle.Year.ToString());
                    item.SubItems.Add(GetVehicleStatus(vehicle));
                    item.SubItems.Add(vehicle.User?.Username ?? "");
                    item.Tag = vehicle.Id;
                    _vehicleList.Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading vehicles: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadMaintenanceData()
        {
            try
            {
                _maintenanceList.Items.Clear();
                var records = _databaseService.GetRecentMaintenanceRecords(10); // Get last 10 records

                foreach (var record in records)
                {
                    var item = new ListViewItem(record.MaintenanceDate.ToShortDateString());
                    item.SubItems.Add($"{record.Vehicle.Make} {record.Vehicle.Model}");
                    item.SubItems.Add(record.MaintenanceType);
                    item.SubItems.Add(record.IsCompleted ? "Completed" : "Pending");
                    item.SubItems.Add(record.Cost.ToString("C"));
                    item.Tag = record.Id;
                    _maintenanceList.Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading maintenance records: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadAppointments()
        {
            try
            {
                _appointmentsList.Items.Clear();
                var appointments = _databaseService.GetUpcomingAppointments(_currentUser.Id);

                foreach (var appointment in appointments)
                {
                    var item = new ListViewItem(appointment.AppointmentDate.ToShortDateString());
                    item.SubItems.Add(appointment.AppointmentDate.ToShortTimeString());
                    item.SubItems.Add($"{appointment.Vehicle.Make} {appointment.Vehicle.Model}");
                    item.SubItems.Add(appointment.Vehicle.User.FullName);
                    item.SubItems.Add(appointment.Notes ?? "");
                    item.Tag = appointment.Id;
                    _appointmentsList.Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading appointments: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string GetVehicleStatus(Vehicle vehicle)
        {
            // Implement logic to determine vehicle status
            // This could be based on maintenance records, appointments, etc.
            return "Active"; // Placeholder
        }

        private void FilterComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadVehicleData(); // Reload with filter
        }

        private void VehicleList_SelectedIndexChanged(object sender, EventArgs e)
        {
            var hasSelection = _vehicleList.SelectedItems.Count > 0;
            _addMaintenanceBtn.Enabled = hasSelection;
            _addDiagnosisBtn.Enabled = hasSelection;
            _viewHistoryBtn.Enabled = hasSelection;
        }

        private void AddMaintenanceBtn_Click(object sender, EventArgs e)
        {
            if (_vehicleList.SelectedItems.Count > 0)
            {
                var vehicleId = (int)_vehicleList.SelectedItems[0].Tag;
                var addMaintenanceForm = new AddMaintenanceForm(vehicleId, _currentUser);
                if (addMaintenanceForm.ShowDialog() == DialogResult.OK)
                {
                    LoadMaintenanceData();
                }
            }
        }

        private void AddDiagnosisBtn_Click(object sender, EventArgs e)
        {
            if (_vehicleList.SelectedItems.Count > 0)
            {
                var vehicleId = (int)_vehicleList.SelectedItems[0].Tag;
                var diagnosisForm = new DiagnosisForm(vehicleId, _currentUser.Id);
                diagnosisForm.ShowDialog();
            }
        }

        private void ViewHistoryBtn_Click(object sender, EventArgs e)
        {
            if (_vehicleList.SelectedItems.Count > 0)
            {
                var vehicleId = (int)_vehicleList.SelectedItems[0].Tag;
                var historyForm = new MaintenanceHistoryForm(vehicleId, _currentUser);
                historyForm.ShowDialog();
            }
        }

        private void ProfileBtn_Click(object sender, EventArgs e)
        {
            var profileForm = new UserProfileForm(_currentUser);
            profileForm.ShowDialog();
        }

        private void AddAppointmentBtn_Click(object sender, EventArgs e)
        {
            if (_vehicleList.SelectedItems.Count > 0)
            {
                var vehicleId = (int)_vehicleList.SelectedItems[0].Tag;
                var addAppointmentForm = new AddAppointmentForm(vehicleId, _currentUser);
                if (addAppointmentForm.ShowDialog() == DialogResult.OK)
                {
                    LoadAppointments();
                }
            }
            else
            {
                MessageBox.Show("Please select a vehicle first.", "No Vehicle Selected", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void SearchUsernameBtn_Click(object sender, EventArgs e)
        {
            LoadVehicleData(_searchUsernameTextBox.Text.Trim());
        }

        private void LoadPendingAppointments()
        {
            try
            {
                var appointments = _databaseService.GetPendingAppointmentsByCenter(_currentUser.Id);
                PendingAppointmentsDataGridView.DataSource = appointments;
                _logger.LogInformation("Loaded {Count} pending appointments for center {CenterId}", 
                    appointments.Count, _currentUser.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading pending appointments");
                MessageBox.Show("Error loading pending appointments. Please try again.", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadMaintenanceHistory()
        {
            try
            {
                var history = _databaseService.GetMaintenanceHistoryByCenter(_currentUser.Id);
                MaintenanceHistoryDataGridView.DataSource = history;
                _logger.LogInformation("Loaded maintenance history for center {CenterId}", 
                    _currentUser.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading maintenance history");
                MessageBox.Show("Error loading maintenance history. Please try again.", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AcceptAppointmentButton_Click(object sender, EventArgs e)
        {
            if (PendingAppointmentsDataGridView.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select an appointment to accept.", "Information", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var selectedAppointment = (Appointment)PendingAppointmentsDataGridView.SelectedRows[0].DataBoundItem;
            try
            {
                if (_databaseService.AcceptAppointment(selectedAppointment.Id))
                {
                    _logger.LogInformation("Appointment {AppointmentId} accepted successfully", 
                        selectedAppointment.Id);
                    LoadPendingAppointments();
                }
                else
                {
                    _logger.LogWarning("Failed to accept appointment {AppointmentId}", 
                        selectedAppointment.Id);
                    MessageBox.Show("Failed to accept appointment. Please try again.", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error accepting appointment {AppointmentId}", 
                    selectedAppointment.Id);
                MessageBox.Show("An error occurred while accepting the appointment.", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RejectAppointmentButton_Click(object sender, EventArgs e)
        {
            if (PendingAppointmentsDataGridView.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select an appointment to reject.", "Information", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var selectedAppointment = (Appointment)PendingAppointmentsDataGridView.SelectedRows[0].DataBoundItem;
            var reason = Microsoft.VisualBasic.Interaction.InputBox(
                "Please enter the reason for rejection:", 
                "Reject Appointment", 
                "");

            if (string.IsNullOrEmpty(reason))
            {
                return;
            }

            try
            {
                if (_databaseService.RejectAppointment(selectedAppointment.Id, reason))
                {
                    _logger.LogInformation("Appointment {AppointmentId} rejected successfully", 
                        selectedAppointment.Id);
                    LoadPendingAppointments();
                }
                else
                {
                    _logger.LogWarning("Failed to reject appointment {AppointmentId}", 
                        selectedAppointment.Id);
                    MessageBox.Show("Failed to reject appointment. Please try again.", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting appointment {AppointmentId}", 
                    selectedAppointment.Id);
                MessageBox.Show("An error occurred while rejecting the appointment.", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CompleteMaintenanceButton_Click(object sender, EventArgs e)
        {
            if (PendingAppointmentsDataGridView.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a maintenance record to complete.", "Information", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var selectedRecord = (MaintenanceRecord)PendingAppointmentsDataGridView.SelectedRows[0].DataBoundItem;
            var completeMaintenanceForm = new CompleteMaintenanceForm(_logger, selectedRecord);
            if (completeMaintenanceForm.ShowDialog() == DialogResult.OK)
            {
                LoadPendingAppointments();
                LoadMaintenanceHistory();
            }
        }

        private void ViewHistoryButton_Click(object sender, EventArgs e)
        {
            if (MaintenanceHistoryDataGridView.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a maintenance record to view.", "Information", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var selectedRecord = (MaintenanceRecord)MaintenanceHistoryDataGridView.SelectedRows[0].DataBoundItem;
            var viewMaintenanceForm = new ViewMaintenanceForm(_logger, selectedRecord);
            viewMaintenanceForm.ShowDialog();
        }

        private void LogoutButton_Click(object sender, EventArgs e)
        {
            SessionManager.EndSession();
            _logger.LogInformation("User {Username} logged out", _currentUser.Username);
            this.Close();
        }
    }
} 