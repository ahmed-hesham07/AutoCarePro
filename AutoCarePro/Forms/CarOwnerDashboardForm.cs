using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Linq;
using AutoCarePro.Models;
using AutoCarePro.Services;
using AutoCarePro.UI;

namespace AutoCarePro.Forms
{
    /// <summary>
    /// Dashboard form specifically designed for car owners.
    /// This form provides a focused interface for car owners to:
    /// 1. View and manage their vehicles
    /// 2. Receive maintenance recommendations
    /// 3. Monitor critical alerts
    /// 4. Access maintenance history
    /// 5. Add new vehicles and maintenance records
    /// </summary>
    public partial class CarOwnerDashboardForm : Form
    {
        private readonly DatabaseService _dbService;
        private readonly RecommendationEngine _recommendationEngine;
        private readonly User _currentUser;

        // UI Controls
        private ListView _vehicleList = new ListView();
        private ListView _recommendationsList = new ListView();
        private ListView _alertsList = new ListView();
        private Button _addVehicleBtn = new Button();
        private Button _addMaintenanceBtn = new Button();
        private Button _viewHistoryBtn = new Button();
        private Button _profileBtn = new Button();
        private Label _welcomeLabel = new Label();
        private System.Windows.Forms.Timer _fadeInTimer = new System.Windows.Forms.Timer();
        private double _fadeStep = 0.08;

        public CarOwnerDashboardForm(User user)
        {
            InitializeComponent();
            _currentUser = user;
            _dbService = new DatabaseService();
            _recommendationEngine = new RecommendationEngine(_dbService);
            InitializeDashboard();
        }

        private void InitializeDashboard()
        {
            // Apply form styling
            UIStyles.ApplyFormStyle(this);
            this.Text = $"AutoCarePro - Car Owner Dashboard";
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
            LoadRecommendations();
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

            // Vehicle list section
            var vehicleGroup = new GroupBox
            {
                Text = "My Vehicles",
                Dock = DockStyle.Top,
                Height = 300,
                Padding = new Padding(10)
            };
            UIStyles.ApplyGroupBoxStyle(vehicleGroup);

            _vehicleList = new ListView();
            UIStyles.ApplyListViewStyle(_vehicleList);
            _vehicleList.Dock = DockStyle.Fill;
            _vehicleList.Columns.Add("Make", 100);
            _vehicleList.Columns.Add("Model", 100);
            _vehicleList.Columns.Add("Year", 50);
            _vehicleList.Columns.Add("Mileage", 100);
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

            _addVehicleBtn = new Button { Text = "Add Vehicle", Width = 120, Height = 35 };
            _addMaintenanceBtn = new Button { Text = "Add Maintenance", Width = 120, Height = 35, Enabled = false };
            _viewHistoryBtn = new Button { Text = "View History", Width = 120, Height = 35, Enabled = false };
            _profileBtn = new Button { Text = "My Profile", Width = 120, Height = 35 };

            // Apply button styling with press animations
            UIStyles.ApplyButtonStyle(_addVehicleBtn, true);
            UIStyles.ApplyButtonStyle(_addMaintenanceBtn);
            UIStyles.ApplyButtonStyle(_viewHistoryBtn);
            UIStyles.ApplyButtonStyle(_profileBtn);

            _addVehicleBtn.Click += AddVehicleBtn_Click;
            _addMaintenanceBtn.Click += AddMaintenanceBtn_Click;
            _viewHistoryBtn.Click += ViewHistoryBtn_Click;
            _profileBtn.Click += ProfileBtn_Click;

            buttonPanel.Controls.AddRange(new Control[] { _addVehicleBtn, _addMaintenanceBtn, _viewHistoryBtn, _profileBtn });
            panel.Controls.Add(buttonPanel);
        }

        private void InitializeRightPanel(Panel panel)
        {
            // Recommendations panel
            var recommendationsPanel = new GroupBox
            {
                Text = "Maintenance Recommendations",
                Dock = DockStyle.Top,
                Height = 400,
                Padding = new Padding(10)
            };
            UIStyles.ApplyGroupBoxStyle(recommendationsPanel);

            _recommendationsList = new ListView();
            UIStyles.ApplyListViewStyle(_recommendationsList);
            _recommendationsList.Dock = DockStyle.Fill;
            _recommendationsList.Columns.Add("Component", 100);
            _recommendationsList.Columns.Add("Description", 200);
            _recommendationsList.Columns.Add("Priority", 100);
            _recommendationsList.Columns.Add("Due Date", 100);

            recommendationsPanel.Controls.Add(_recommendationsList);
            panel.Controls.Add(recommendationsPanel);

            // Alerts panel
            var alertsPanel = new GroupBox
            {
                Text = "Critical Alerts",
                Dock = DockStyle.Bottom,
                Height = 200,
                Padding = new Padding(10)
            };
            UIStyles.ApplyGroupBoxStyle(alertsPanel);

            _alertsList = new ListView();
            UIStyles.ApplyListViewStyle(_alertsList);
            _alertsList.Dock = DockStyle.Fill;
            _alertsList.Columns.Add("Alert", 300);
            _alertsList.Columns.Add("Date", 100);

            alertsPanel.Controls.Add(_alertsList);
            panel.Controls.Add(alertsPanel);
        }

        private void LoadVehicleData()
        {
            try
            {
                _vehicleList.Items.Clear();
                var vehicles = _dbService.GetVehiclesByUserId(_currentUser.Id);

                foreach (var vehicle in vehicles)
                {
                    var item = new ListViewItem(vehicle.Make);
                    item.SubItems.Add(vehicle.Model);
                    item.SubItems.Add(vehicle.Year.ToString());
                    item.SubItems.Add(vehicle.CurrentMileage.ToString());
                    item.Tag = vehicle.Id;
                    _vehicleList.Items.Add(item);
                }

                _addMaintenanceBtn.Enabled = _vehicleList.Items.Count > 0;
                _viewHistoryBtn.Enabled = _vehicleList.Items.Count > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading vehicles: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadRecommendations()
        {
            try
            {
                _recommendationsList.Items.Clear();
                _alertsList.Items.Clear();

                var vehicles = _dbService.GetVehiclesByUserId(_currentUser.Id);
                var allRecommendations = new List<MaintenanceRecommendation>();

                foreach (var vehicle in vehicles)
                {
                    var recommendations = _recommendationEngine.GenerateRecommendations(vehicle.Id);
                    allRecommendations.AddRange(recommendations);
                }

                allRecommendations = allRecommendations.OrderByDescending(r => r.Priority).ToList();

                foreach (var recommendation in allRecommendations)
                {
                    var item = new ListViewItem(recommendation.Component);
                    item.SubItems.Add(recommendation.Description);
                    item.SubItems.Add(recommendation.Priority.ToString());
                    item.SubItems.Add(recommendation.RecommendedDate.ToShortDateString());
                    item.Tag = recommendation;
                    _recommendationsList.Items.Add(item);

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

        private void VehicleList_SelectedIndexChanged(object sender, EventArgs e)
        {
            var hasSelection = _vehicleList.SelectedItems.Count > 0;
            _addMaintenanceBtn.Enabled = hasSelection;
            _viewHistoryBtn.Enabled = hasSelection;
        }

        private void AddVehicleBtn_Click(object sender, EventArgs e)
        {
            var addVehicleForm = new AddVehicleForm(_currentUser.Id);
            if (addVehicleForm.ShowDialog() == DialogResult.OK)
            {
                LoadVehicleData();
                LoadRecommendations();
            }
        }

        private void AddMaintenanceBtn_Click(object sender, EventArgs e)
        {
            if (_vehicleList.SelectedItems.Count > 0)
            {
                var vehicleId = (int)_vehicleList.SelectedItems[0].Tag;
                var addMaintenanceForm = new AddMaintenanceForm(vehicleId, _currentUser);
                if (addMaintenanceForm.ShowDialog() == DialogResult.OK)
                {
                    LoadVehicleData();
                    LoadRecommendations();
                }
            }
        }

        private void ViewHistoryBtn_Click(object sender, EventArgs e)
        {
            if (_vehicleList.SelectedItems.Count > 0)
            {
                var vehicleId = (int)_vehicleList.SelectedItems[0].Tag;
                var historyForm = new MaintenanceHistoryForm(vehicleId);
                historyForm.ShowDialog();
            }
        }

        private void ProfileBtn_Click(object sender, EventArgs e)
        {
            var profileForm = new UserProfileForm(_currentUser);
            profileForm.ShowDialog();
        }
    }
} 