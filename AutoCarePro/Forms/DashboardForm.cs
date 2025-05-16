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
    /// Main dashboard form that displays vehicle information, maintenance recommendations, and alerts
    /// </summary>
    public partial class DashboardForm : Form
    {
        // Service instances for database operations and generating maintenance recommendations
        private readonly DatabaseService _dbService;
        private readonly RecommendationEngine _recommendationEngine;
        private User _currentUser;

        // UI Controls for displaying and interacting with data
        private ListView _vehicleList;        // Displays list of user's vehicles
        private ListView _recommendationsList; // Shows maintenance recommendations
        private ListView _alertsList;         // Displays critical maintenance alerts
        private Button _addVehicleBtn;        // Button to add new vehicle
        private Button _addMaintenanceBtn;    // Button to add maintenance record
        private Button _viewHistoryBtn;       // Button to view maintenance history

        /// <summary>
        /// Initializes the dashboard form with user data and services
        /// </summary>
        /// <param name="user">The currently logged-in user</param>
        public DashboardForm(User user)
        {
            InitializeComponent();
            _currentUser = user;
            _dbService = new DatabaseService();
            _recommendationEngine = new RecommendationEngine(_dbService);
            InitializeDashboard();
        }

        /// <summary>
        /// Sets up the main dashboard layout and initializes all components
        /// </summary>
        private void InitializeDashboard()
        {
            // Configure main form properties
            this.Text = $"AutoCarePro Dashboard - Welcome {_currentUser.FullName}";
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Create main layout panel with two columns
            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                ColumnStyles = {
                    new ColumnStyle(SizeType.Percent, 50), // Left panel takes 50%
                    new ColumnStyle(SizeType.Percent, 50)  // Right panel takes 50%
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
        /// Initializes the left panel containing vehicle list and quick action buttons
        /// </summary>
        private void InitializeLeftPanel(Panel panel)
        {
            // Configure vehicle list view
            _vehicleList = new ListView
            {
                Dock = DockStyle.Top,
                Height = 300,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true
            };

            // Add columns to vehicle list
            _vehicleList.Columns.Add("Make", 100);    // Vehicle manufacturer
            _vehicleList.Columns.Add("Model", 100);   // Vehicle model
            _vehicleList.Columns.Add("Year", 50);     // Manufacturing year
            _vehicleList.Columns.Add("Mileage", 100); // Current mileage
            _vehicleList.SelectedIndexChanged += VehicleList_SelectedIndexChanged;

            // Create context menu for vehicle list
            var contextMenu = new ContextMenuStrip();
            var editMenuItem = new ToolStripMenuItem("Edit Vehicle");
            var deleteMenuItem = new ToolStripMenuItem("Delete Vehicle");
            editMenuItem.Click += EditVehicleMenuItem_Click;
            deleteMenuItem.Click += DeleteVehicleMenuItem_Click;
            contextMenu.Items.Add(editMenuItem);
            contextMenu.Items.Add(deleteMenuItem);
            _vehicleList.ContextMenuStrip = contextMenu;

            // Create quick actions panel at bottom
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
        /// Initializes the right panel containing recommendations and alerts
        /// </summary>
        private void InitializeRightPanel(Panel panel)
        {
            // Create recommendations panel
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
        /// Loads and displays the user's vehicles in the vehicle list
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

                // Enable/disable buttons based on whether user has vehicles
                _addMaintenanceBtn.Enabled = _vehicleList.Items.Count > 0;
                _viewHistoryBtn.Enabled = _vehicleList.Items.Count > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading vehicles: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Loads and displays maintenance recommendations and alerts
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
                MessageBox.Show($"Error loading recommendations: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Handles vehicle selection change in the vehicle list
        /// </summary>
        private void VehicleList_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Enable/disable buttons based on selection
            var hasSelection = _vehicleList.SelectedItems.Count > 0;
            _addMaintenanceBtn.Enabled = hasSelection;
            _viewHistoryBtn.Enabled = hasSelection;
        }

        /// <summary>
        /// Handles click event for Add Vehicle button
        /// </summary>
        private void AddVehicleBtn_Click(object sender, EventArgs e)
        {
            var addVehicleForm = new AddVehicleForm(_currentUser.Id);
            if (addVehicleForm.ShowDialog() == DialogResult.OK)
            {
                LoadVehicleData();
                LoadRecommendations();
            }
        }

        /// <summary>
        /// Handles click event for Add Maintenance button
        /// </summary>
        private void AddMaintenanceBtn_Click(object sender, EventArgs e)
        {
            if (_vehicleList.SelectedItems.Count > 0)
            {
                var vehicleId = (int)_vehicleList.SelectedItems[0].Tag;
                var addMaintenanceForm = new AddMaintenanceForm(vehicleId);
                if (addMaintenanceForm.ShowDialog() == DialogResult.OK)
                {
                    LoadVehicleData();
                    LoadRecommendations();
                }
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
                    LoadVehicleData();
                    LoadRecommendations();
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
                    "Are you sure you want to delete this vehicle?",
                    "Confirm Delete",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    var vehicleId = (int)_vehicleList.SelectedItems[0].Tag;
                    _dbService.DeleteVehicle(vehicleId);
                    LoadVehicleData();
                    LoadRecommendations();
                }
            }
        }
    }
}
