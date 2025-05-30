using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Linq;
using AutoCarePro.Models;
using AutoCarePro.Services;

namespace AutoCarePro.Forms
{
    /// <summary>
    /// Main dashboard form that serves as the central hub for the AutoCarePro application.
    /// This form provides a comprehensive interface for users to:
    /// 1. View and manage their vehicles
    /// 2. Receive maintenance recommendations
    /// 3. Monitor critical alerts
    /// 4. Access maintenance history
    /// 5. Add new vehicles and maintenance records
    /// 
    /// The dashboard is divided into two main sections:
    /// - Left panel: Vehicle list and quick actions
    /// - Right panel: Maintenance recommendations and alerts
    /// </summary>
    public partial class DashboardForm : Form
    {
        // Service instances for database operations and generating maintenance recommendations
        private readonly DatabaseService _dbService;           // Handles all database operations
        private readonly RecommendationEngine _recommendationEngine;  // Generates maintenance recommendations
        private User _currentUser;                            // Stores the currently logged-in user

        // UI Controls for displaying and interacting with data
        private ListView _vehicleList = new ListView();
        private ListView _recommendationsList = new ListView();
        private ListView _alertsList = new ListView();
        private Button _addVehicleBtn;        // Button to add new vehicle to the user's profile
        private Button _addMaintenanceBtn;    // Button to add maintenance record for selected vehicle
        private Button _viewHistoryBtn;       // Button to view maintenance history for selected vehicle

        /// <summary>
        /// Initializes the dashboard form with user data and services.
        /// This constructor is called when creating a new instance of the dashboard.
        /// It sets up the database service, recommendation engine, and initializes the UI.
        /// </summary>
        /// <param name="user">The currently logged-in user - used to load their vehicles and data</param>
        public DashboardForm(User user)
        {
            InitializeComponent();
            _currentUser = user;
            _dbService = new DatabaseService();
            _recommendationEngine = new RecommendationEngine(_dbService);
            InitializeDashboard();
        }

        /// <summary>
        /// Sets up the main dashboard layout and initializes all components.
        /// This method creates the two-panel layout and initializes all UI elements.
        /// The dashboard is designed to provide easy access to all main features.
        /// </summary>
        private void InitializeDashboard()
        {
            // Configure main form properties
            this.Text = $"AutoCarePro Dashboard - Welcome {_currentUser.FullName}";
            this.Size = new Size(1400, 900);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Create main layout panel with two columns
            // TableLayoutPanel provides a grid-like structure for organizing controls
            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                ColumnStyles = {
                    new ColumnStyle(SizeType.Percent, 50), // Left panel takes 50% of width
                    new ColumnStyle(SizeType.Percent, 50)  // Right panel takes 50% of width
                }
            };

            // Create left and right panels
            var leftPanel = new Panel { Dock = DockStyle.Fill };  // For vehicle list and actions
            var rightPanel = new Panel { Dock = DockStyle.Fill }; // For recommendations and alerts

            // Add panels to main layout
            mainPanel.Controls.Add(leftPanel, 0, 0);
            mainPanel.Controls.Add(rightPanel, 1, 0);

            // Initialize the panels with their components
            InitializeLeftPanel(leftPanel);
            InitializeRightPanel(rightPanel);

            // Add main panel to form
            this.Controls.Add(mainPanel);

            // Load initial data
            LoadVehicleData();
            LoadRecommendations();
        }

        /// <summary>
        /// Initializes the left panel containing vehicle list and quick action buttons.
        /// This panel is the main interaction area for vehicle management.
        /// </summary>
        /// <param name="panel">The panel to initialize with vehicle list and actions</param>
        private void InitializeLeftPanel(Panel panel)
        {
            // Configure vehicle list view
            // ListView provides a grid-like display of vehicle information
            _vehicleList = new ListView
            {
                Dock = DockStyle.Top,
                Height = 300,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true
            };

            // Add columns to vehicle list for displaying vehicle information
            _vehicleList.Columns.Add("Make", 100);    // Vehicle manufacturer
            _vehicleList.Columns.Add("Model", 100);   // Vehicle model
            _vehicleList.Columns.Add("Year", 50);     // Manufacturing year
            _vehicleList.Columns.Add("Mileage", 100); // Current mileage
            _vehicleList.SelectedIndexChanged += VehicleList_SelectedIndexChanged;

            // Create context menu for vehicle list
            // This provides right-click options for each vehicle
            var contextMenu = new ContextMenuStrip();
            var editMenuItem = new ToolStripMenuItem("Edit Vehicle");
            var deleteMenuItem = new ToolStripMenuItem("Delete Vehicle");
            editMenuItem.Click += EditVehicleMenuItem_Click;
            deleteMenuItem.Click += DeleteVehicleMenuItem_Click;
            contextMenu.Items.Add(editMenuItem);
            contextMenu.Items.Add(deleteMenuItem);
            _vehicleList.ContextMenuStrip = contextMenu;

            // Create quick actions panel at bottom
            // FlowLayoutPanel arranges buttons horizontally
            var quickActionsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 100,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(10)
            };

            // Create and configure action buttons
            _addVehicleBtn = new Button { Text = "Add Vehicle", Width = 120 };
            _addMaintenanceBtn = new Button { Text = "Add Maintenance", Width = 120 };
            _viewHistoryBtn = new Button { Text = "View History", Width = 120 };

            // Add event handlers for buttons
            _addVehicleBtn.Click += AddVehicleBtn_Click;
            _addMaintenanceBtn.Click += AddMaintenanceBtn_Click;
            _viewHistoryBtn.Click += ViewHistoryBtn_Click;

            // Add buttons to quick actions panel
            quickActionsPanel.Controls.AddRange(new Control[] { _addVehicleBtn, _addMaintenanceBtn, _viewHistoryBtn });

            // Add controls to left panel
            panel.Controls.Add(_vehicleList);
            panel.Controls.Add(quickActionsPanel);
        }

        /// <summary>
        /// Initializes the right panel containing recommendations and alerts.
        /// This panel provides important maintenance information and warnings.
        /// </summary>
        /// <param name="panel">The panel to initialize with recommendations and alerts</param>
        private void InitializeRightPanel(Panel panel)
        {
            // Create recommendations panel
            // GroupBox provides a titled container for the recommendations list
            var recommendationsPanel = new GroupBox
            {
                Text = "Maintenance Recommendations",
                Dock = DockStyle.Top,
                Height = 400,
                Padding = new Padding(10)
            };

            // Configure recommendations list view
            _recommendationsList = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true
            };

            // Add columns to recommendations list
            _recommendationsList.Columns.Add("Component", 100);    // Part that needs maintenance
            _recommendationsList.Columns.Add("Description", 200);  // Maintenance description
            _recommendationsList.Columns.Add("Priority", 100);     // Priority level
            _recommendationsList.Columns.Add("Due Date", 100);     // Recommended date

            recommendationsPanel.Controls.Add(_recommendationsList);

            // Create alerts panel
            // GroupBox provides a titled container for the alerts list
            var alertsPanel = new GroupBox
            {
                Text = "Critical Alerts",
                Dock = DockStyle.Bottom,
                Height = 200,
                Padding = new Padding(10)
            };

            // Configure alerts list view
            _alertsList = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true
            };

            // Add columns to alerts list
            _alertsList.Columns.Add("Alert", 300);    // Alert message
            _alertsList.Columns.Add("Date", 100);     // Alert date

            alertsPanel.Controls.Add(_alertsList);

            // Add panels to right panel
            panel.Controls.Add(recommendationsPanel);
            panel.Controls.Add(alertsPanel);
        }

        /// <summary>
        /// Updates the UI state based on current selection and data availability
        /// </summary>
        private void UpdateUIState()
        {
            bool hasVehicles = _vehicleList.Items.Count > 0;
            bool hasSelection = _vehicleList.SelectedItems.Count > 0;

            _addMaintenanceBtn.Enabled = hasSelection;
            _viewHistoryBtn.Enabled = hasSelection;
            _addVehicleBtn.Enabled = true; // Always enabled
        }

        /// <summary>
        /// Handles vehicle selection change in the vehicle list
        /// </summary>
        private void VehicleList_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateUIState();
        }

        /// <summary>
        /// Handles error logging and displays user-friendly error messages
        /// </summary>
        /// <param name="errorMessage">The error message to display to the user</param>
        /// <param name="ex">The exception that occurred</param>
        /// <param name="operation">The operation that was being performed when the error occurred</param>
        private void HandleError(string errorMessage, Exception ex, string operation)
        {
            // Log the error (in a real application, this would write to a log file or database)
            Console.WriteLine($"Error during {operation}: {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");

            // Show user-friendly error message
            MessageBox.Show(errorMessage, "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        /// <summary>
        /// Loads and displays the user's vehicles in the vehicle list.
        /// This method retrieves vehicle data from the database and populates the list view.
        /// It also enables/disables buttons based on whether the user has any vehicles.
        /// </summary>
        private void LoadVehicleData()
        {
            try
            {
                // Clear existing items from list
                _vehicleList.Items.Clear();

                // Get vehicles for current user from database
                var vehicles = _dbService.GetVehiclesByUserId(_currentUser.Id);

                // Add each vehicle to the list view
                foreach (var vehicle in vehicles)
                {
                    var item = new ListViewItem(vehicle.Make);
                    item.SubItems.Add(vehicle.Model);
                    item.SubItems.Add(vehicle.Year.ToString());
                    item.SubItems.Add(vehicle.CurrentMileage.ToString());
                    item.Tag = vehicle.Id; // Store vehicle ID for reference
                    _vehicleList.Items.Add(item);
                }

                UpdateUIState();
            }
            catch (Exception ex)
            {
                HandleError("Failed to load vehicle data. Please try again later.", ex, "LoadVehicleData");
            }
        }

        /// <summary>
        /// Loads and displays maintenance recommendations and alerts.
        /// This method retrieves data from the recommendation engine and updates both lists.
        /// Recommendations are based on vehicle data and maintenance schedules.
        /// </summary>
        private void LoadRecommendations()
        {
            try
            {
                // Clear existing items
                _recommendationsList.Items.Clear();
                _alertsList.Items.Clear();

                // Get all vehicles for current user
                var vehicles = _dbService.GetVehiclesByUserId(_currentUser.Id);
                var allRecommendations = new List<MaintenanceRecommendation>();

                // Get recommendations for each vehicle
                foreach (var vehicle in vehicles)
                {
                    var recommendations = _recommendationEngine.GenerateRecommendations(vehicle.Id);
                    allRecommendations.AddRange(recommendations);
                }

                // Sort recommendations by priority (highest first)
                allRecommendations = allRecommendations.OrderByDescending(r => r.Priority).ToList();

                // Add recommendations to the list view
                foreach (var recommendation in allRecommendations)
                {
                    var item = new ListViewItem(recommendation.Component);
                    item.SubItems.Add(recommendation.Description);
                    item.SubItems.Add(recommendation.Priority.ToString());
                    item.SubItems.Add(recommendation.RecommendedDate.ToShortDateString());
                    item.Tag = recommendation;
                    _recommendationsList.Items.Add(item);

                    // Add critical recommendations to alerts list
                    if (recommendation.Priority == PriorityLevel.Critical)
                    {
                        var alertItem = new ListViewItem($"Critical: {recommendation.Description}");
                        alertItem.SubItems.Add(recommendation.RecommendedDate.ToShortDateString());
                        _alertsList.Items.Add(alertItem);
                    }
                }
            }
            catch (Exception ex)
            {
                HandleError("Failed to load maintenance recommendations. Please try again later.", ex, "LoadRecommendations");
            }
        }

        /// <summary>
        /// Refreshes all data on the dashboard
        /// </summary>
        private void RefreshDashboardData()
        {
            LoadVehicleData();
            LoadRecommendations();
        }

        /// <summary>
        /// Handles click event for Add Vehicle button
        /// </summary>
        private void AddVehicleBtn_Click(object sender, EventArgs e)
        {
            var addVehicleForm = new AddVehicleForm(_currentUser.Id);
            if (addVehicleForm.ShowDialog() == DialogResult.OK)
            {
                RefreshDashboardData();
            }
        }

        /// <summary>
        /// Handles click event for Add Maintenance button
        /// </summary>
        private void AddMaintenanceBtn_Click(object sender, EventArgs e)
        {
            try
            {
                if (_vehicleList.SelectedItems.Count > 0)
                {
                    var vehicleId = (int)_vehicleList.SelectedItems[0].Tag;
                    var addMaintenanceForm = new AddMaintenanceForm(vehicleId, _currentUser);
                    if (addMaintenanceForm.ShowDialog() == DialogResult.OK)
                    {
                        RefreshDashboardData();
                    }
                }
                else
                {
                    MessageBox.Show("Please select a vehicle first.", "No Vehicle Selected", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding maintenance record: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Handles click event for View History button
        /// </summary>
        private void ViewHistoryBtn_Click(object sender, EventArgs e)
        {
            if (_vehicleList.SelectedItems.Count > 0)
            {
                var vehicleId = (int)_vehicleList.SelectedItems[0].Tag;
                var historyForm = new MaintenanceHistoryForm(vehicleId);
                historyForm.ShowDialog();
            }
        }

        /// <summary>
        /// Handles click event for Edit Vehicle menu item
        /// </summary>
        private void EditVehicleMenuItem_Click(object sender, EventArgs e)
        {
            if (_vehicleList.SelectedItems.Count > 0)
            {
                var vehicleId = (int)_vehicleList.SelectedItems[0].Tag;
                var vehicle = _dbService.GetVehicleById(vehicleId);
                var editForm = new AddVehicleForm(_currentUser.Id, vehicle);
                if (editForm.ShowDialog() == DialogResult.OK)
                {
                    RefreshDashboardData();
                }
            }
        }

        /// <summary>
        /// Handles click event for Delete Vehicle menu item
        /// </summary>
        private void DeleteVehicleMenuItem_Click(object sender, EventArgs e)
        {
            if (_vehicleList.SelectedItems.Count > 0)
            {
                var result = MessageBox.Show(
                    "Are you sure you want to delete this vehicle? This action cannot be undone.",
                    "Confirm Delete",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    try
                    {
                        var vehicleId = (int)_vehicleList.SelectedItems[0].Tag;
                        _dbService.DeleteVehicle(vehicleId);
                        RefreshDashboardData();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting vehicle: {ex.Message}", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }
}
