using System;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using AutoCarePro.Services;
using AutoCarePro.Models;
using AutoCarePro.UI;
using Microsoft.Extensions.Logging;

namespace AutoCarePro.Forms
{
    public partial class ServiceProviderDashboardForm : Form
    {
        private readonly DatabaseService _databaseService;
        private readonly ILogger<ServiceProviderDashboardForm> _logger;
        private readonly User _currentUser;

        // UI Controls
        private ListView _appointmentsList = new ListView();
        private ListView _servicesList = new ListView();
        private ListView _reviewsList = new ListView();
        private Button _addServiceBtn = new Button();
        private Button _viewHistoryBtn = new Button();
        private Button _profileBtn = new Button();
        private Label _welcomeLabel = new Label();
        private System.Windows.Forms.Timer _fadeInTimer = new System.Windows.Forms.Timer();
        private double _fadeStep = 0.08;
        private Button _addAppointmentBtn = new Button();

        // Search Controls
        private TextBox _searchBox = new TextBox();
        private ComboBox _searchFilterCombo = new ComboBox();
        private ComboBox _statusFilterCombo = new ComboBox();
        private DateTimePicker _dateFilterPicker = new DateTimePicker();
        private Button _clearFiltersBtn = new Button();
        private System.Windows.Forms.Timer _searchDebounceTimer = new System.Windows.Forms.Timer();

        private ListView _upcomingAppointmentsList = new ListView();

        public ServiceProviderDashboardForm(ILogger<ServiceProviderDashboardForm> logger)
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
            // Set form title with provider's name
            this.Text = $"Service Provider Dashboard - {_currentUser.Username}";

            // Load service requests
            LoadServiceRequests();

            // Load service history
            LoadServiceHistory();

            // Setup event handlers
            AcceptRequestButton.Click += AcceptRequestButton_Click;
            RejectRequestButton.Click += RejectRequestButton_Click;
            CompleteServiceButton.Click += CompleteServiceButton_Click;
            ViewHistoryButton.Click += ViewHistoryButton_Click;
            LogoutButton.Click += LogoutButton_Click;

            InitializeSearchControls();
            InitializeDashboard();
        }

        private void LoadServiceRequests()
        {
            try
            {
                var requests = _databaseService.GetPendingServiceRequestsByProvider(_currentUser.Id);
                ServiceRequestsDataGridView.DataSource = requests;
                _logger.LogInformation("Loaded {Count} pending service requests for provider {ProviderId}", 
                    requests.Count, _currentUser.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading service requests");
                MessageBox.Show("Error loading service requests. Please try again.", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadServiceHistory()
        {
            try
            {
                var history = _databaseService.GetServiceHistoryByProvider(_currentUser.Id);
                ServiceHistoryDataGridView.DataSource = history;
                _logger.LogInformation("Loaded service history for provider {ProviderId}", 
                    _currentUser.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading service history");
                MessageBox.Show("Error loading service history. Please try again.", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AcceptRequestButton_Click(object sender, EventArgs e)
        {
            if (ServiceRequestsDataGridView.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a service request to accept.", "Information", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var selectedRequest = (ServiceRequest)ServiceRequestsDataGridView.SelectedRows[0].DataBoundItem;
            try
            {
                if (_databaseService.AcceptServiceRequest(selectedRequest.Id))
                {
                    _logger.LogInformation("Service request {RequestId} accepted successfully", 
                        selectedRequest.Id);
                    LoadServiceRequests();
                }
                else
                {
                    _logger.LogWarning("Failed to accept service request {RequestId}", 
                        selectedRequest.Id);
                    MessageBox.Show("Failed to accept service request. Please try again.", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error accepting service request {RequestId}", 
                    selectedRequest.Id);
                MessageBox.Show("An error occurred while accepting the service request.", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RejectRequestButton_Click(object sender, EventArgs e)
        {
            if (ServiceRequestsDataGridView.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a service request to reject.", "Information", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var selectedRequest = (ServiceRequest)ServiceRequestsDataGridView.SelectedRows[0].DataBoundItem;
            var reason = Microsoft.VisualBasic.Interaction.InputBox(
                "Please enter the reason for rejection:", 
                "Reject Service Request", 
                "");

            if (string.IsNullOrEmpty(reason))
            {
                return;
            }

            try
            {
                if (_databaseService.RejectServiceRequest(selectedRequest.Id, reason))
                {
                    _logger.LogInformation("Service request {RequestId} rejected successfully", 
                        selectedRequest.Id);
                    LoadServiceRequests();
                }
                else
                {
                    _logger.LogWarning("Failed to reject service request {RequestId}", 
                        selectedRequest.Id);
                    MessageBox.Show("Failed to reject service request. Please try again.", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting service request {RequestId}", 
                    selectedRequest.Id);
                MessageBox.Show("An error occurred while rejecting the service request.", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CompleteServiceButton_Click(object sender, EventArgs e)
        {
            if (ServiceRequestsDataGridView.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a service request to complete.", "Information", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var selectedRequest = (ServiceRequest)ServiceRequestsDataGridView.SelectedRows[0].DataBoundItem;
            var completeServiceForm = new CompleteServiceForm(_logger, selectedRequest);
            if (completeServiceForm.ShowDialog() == DialogResult.OK)
            {
                LoadServiceRequests();
                LoadServiceHistory();
            }
        }

        private void ViewHistoryButton_Click(object sender, EventArgs e)
        {
            if (ServiceHistoryDataGridView.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a service record to view.", "Information", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var selectedRecord = (ServiceRecord)ServiceHistoryDataGridView.SelectedRows[0].DataBoundItem;
            var viewServiceForm = new ViewServiceForm(_logger, selectedRecord);
            viewServiceForm.ShowDialog();
        }

        private void LogoutButton_Click(object sender, EventArgs e)
        {
            SessionManager.EndSession();
            _logger.LogInformation("User {Username} logged out", _currentUser.Username);
            this.Close();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            
            // Form settings
            this.Text = "Service Provider Dashboard";
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(1000, 600);
            
            // Apply form styling
            UIStyles.ApplyFormStyle(this);
            
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
            
            this.ResumeLayout(false);
        }

        private void InitializeSearchControls()
        {
            // Create search panel
            var searchPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                Padding = new Padding(10)
            };
            UIStyles.ApplyPanelStyle(searchPanel);

            // Search textbox with icon
            _searchBox = new TextBox
            {
                Width = 200,
                Height = 30,
                Location = new Point(10, 15),
                PlaceholderText = "Search appointments..."
            };
            UIStyles.ApplyTextBoxStyle(_searchBox);

            // Search filter dropdown
            _searchFilterCombo = new ComboBox
            {
                Width = 150,
                Height = 30,
                Location = new Point(220, 15),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _searchFilterCombo.Items.AddRange(new string[] { "All", "Customer Name", "Vehicle", "Service Type" });
            _searchFilterCombo.SelectedIndex = 0;
            UIStyles.ApplyComboBoxStyle(_searchFilterCombo);

            // Status filter dropdown
            _statusFilterCombo = new ComboBox
            {
                Width = 150,
                Height = 30,
                Location = new Point(380, 15),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _statusFilterCombo.Items.AddRange(new string[] { "All Status", "Pending", "In Progress", "Completed", "Cancelled" });
            _statusFilterCombo.SelectedIndex = 0;
            UIStyles.ApplyComboBoxStyle(_statusFilterCombo);

            // Date filter
            _dateFilterPicker = new DateTimePicker
            {
                Width = 150,
                Height = 30,
                Location = new Point(540, 15),
                Format = DateTimePickerFormat.Short,
                ShowCheckBox = true,
                Checked = false
            };

            // Clear filters button
            _clearFiltersBtn = new Button
            {
                Text = "Clear Filters",
                Width = 100,
                Height = 30,
                Location = new Point(700, 15)
            };
            UIStyles.ApplyButtonStyle(_clearFiltersBtn);

            // Add controls to search panel
            searchPanel.Controls.AddRange(new Control[] { 
                _searchBox, 
                _searchFilterCombo, 
                _statusFilterCombo, 
                _dateFilterPicker, 
                _clearFiltersBtn 
            });

            // Add search panel to form
            this.Controls.Add(searchPanel);

            // Initialize search debounce timer
            _searchDebounceTimer = new System.Windows.Forms.Timer { Interval = 300 };
            _searchDebounceTimer.Tick += (s, e) => {
                _searchDebounceTimer.Stop();
                PerformSearch();
            };

            // Wire up events
            _searchBox.TextChanged += (s, e) => {
                _searchDebounceTimer.Stop();
                _searchDebounceTimer.Start();
            };
            _searchFilterCombo.SelectedIndexChanged += (s, e) => PerformSearch();
            _statusFilterCombo.SelectedIndexChanged += (s, e) => PerformSearch();
            _dateFilterPicker.ValueChanged += (s, e) => PerformSearch();
            _clearFiltersBtn.Click += (s, e) => {
                _searchBox.Clear();
                _searchFilterCombo.SelectedIndex = 0;
                _statusFilterCombo.SelectedIndex = 0;
                _dateFilterPicker.Checked = false;
                PerformSearch();
            };
        }

        private void PerformSearch()
        {
            try
            {
                var searchText = _searchBox.Text.ToLower();
                var searchFilter = _searchFilterCombo.SelectedItem.ToString();
                var statusFilter = _statusFilterCombo.SelectedItem.ToString();
                var dateFilter = _dateFilterPicker.Checked ? _dateFilterPicker.Value.Date : (DateTime?)null;

                _appointmentsList.Items.Clear();
                var appointments = _databaseService.GetAppointmentsByServiceProvider(_currentUser.Id);

                var filteredAppointments = appointments.Where(a => {
                    // Apply search text filter
                    if (!string.IsNullOrEmpty(searchText))
                    {
                        switch (searchFilter)
                        {
                            case "Customer Name":
                                if (!a.CustomerName.ToLower().Contains(searchText)) return false;
                                break;
                            case "Vehicle":
                                if (!a.VehicleInfo.ToLower().Contains(searchText)) return false;
                                break;
                            case "Service Type":
                                if (!a.ServiceType.ToLower().Contains(searchText)) return false;
                                break;
                            default: // "All"
                                if (!a.CustomerName.ToLower().Contains(searchText) &&
                                    !a.VehicleInfo.ToLower().Contains(searchText) &&
                                    !a.ServiceType.ToLower().Contains(searchText))
                                    return false;
                                break;
                        }
                    }

                    // Apply status filter
                    if (statusFilter != "All Status" && a.Status.ToString() != statusFilter)
                        return false;

                    // Apply date filter
                    if (dateFilter.HasValue && a.AppointmentDate.Date != dateFilter.Value)
                        return false;

                    return true;
                });

                foreach (var appointment in filteredAppointments)
                {
                    var item = new ListViewItem(appointment.CustomerName);
                    item.SubItems.Add(appointment.VehicleInfo);
                    item.SubItems.Add(appointment.ServiceType);
                    item.SubItems.Add(appointment.AppointmentDate.ToString("g"));
                    item.SubItems.Add(appointment.Status.ToString());
                    item.Tag = appointment.Id;
                    _appointmentsList.Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error performing search: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InitializeDashboard()
        {
            // Create main layout panel
            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                ColumnStyles = {
                    new ColumnStyle(SizeType.Percent, 60),
                    new ColumnStyle(SizeType.Percent, 40)
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
            LoadAppointments();
            LoadServices();
            LoadReviews();
            LoadUpcomingAppointments();
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

            // Button panel for actions
            var buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 50,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(10)
            };

            // Initialize and style buttons
            _viewHistoryBtn = new Button
            {
                Text = "View History",
                Width = 120,
                Height = 35,
                Enabled = false
            };
            UIStyles.ApplyButtonStyle(_viewHistoryBtn);
            _viewHistoryBtn.Click += ViewHistoryBtn_Click;

            _profileBtn = new Button
            {
                Text = "My Profile",
                Width = 120,
                Height = 35
            };
            UIStyles.ApplyButtonStyle(_profileBtn);
            _profileBtn.Click += ProfileBtn_Click;

            buttonPanel.Controls.AddRange(new Control[] { _viewHistoryBtn, _profileBtn });
            panel.Controls.Add(buttonPanel);

            // Add Appointment button
            _addAppointmentBtn = new Button
            {
                Text = "Add Appointment",
                Width = 150,
                Height = 35
            };
            UIStyles.ApplyButtonStyle(_addAppointmentBtn, true);
            _addAppointmentBtn.Click += AddAppointmentBtn_Click;
            panel.Controls.Add(_addAppointmentBtn);

            // Appointments list section
            var appointmentsGroup = new GroupBox
            {
                Text = "Appointments",
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };
            UIStyles.ApplyGroupBoxStyle(appointmentsGroup);

            _appointmentsList = new ListView();
            UIStyles.ApplyListViewStyle(_appointmentsList);
            _appointmentsList.Dock = DockStyle.Fill;
            _appointmentsList.Columns.Add("Customer", 150);
            _appointmentsList.Columns.Add("Vehicle", 150);
            _appointmentsList.Columns.Add("Service", 150);
            _appointmentsList.Columns.Add("Date", 150);
            _appointmentsList.Columns.Add("Status", 100);
            _appointmentsList.FullRowSelect = true;
            _appointmentsList.MultiSelect = false;
            _appointmentsList.SelectedIndexChanged += AppointmentsList_SelectedIndexChanged;

            appointmentsGroup.Controls.Add(_appointmentsList);
            panel.Controls.Add(appointmentsGroup);
        }

        private void AppointmentsList_SelectedIndexChanged(object sender, EventArgs e)
        {
            _viewHistoryBtn.Enabled = _appointmentsList.SelectedItems.Count > 0;
        }

        private void ViewHistoryBtn_Click(object sender, EventArgs e)
        {
            if (_appointmentsList.SelectedItems.Count > 0)
            {
                var appointmentId = (int)_appointmentsList.SelectedItems[0].Tag;
                var appointment = _databaseService.GetAppointmentById(appointmentId);
                if (appointment != null)
                {
                    var historyForm = new MaintenanceHistoryForm(appointment.VehicleId, _currentUser);
                    historyForm.ShowDialog();
                }
            }
        }

        private void ProfileBtn_Click(object sender, EventArgs e)
        {
            // Implementation of ProfileBtn_Click method
        }

        private void AddAppointmentBtn_Click(object sender, EventArgs e)
        {
            var addAppointmentForm = new AddAppointmentForm(_currentUser.Id, _currentUser);
            if (addAppointmentForm.ShowDialog() == DialogResult.OK)
            {
                LoadAppointments();
            }
        }

        private void InitializeRightPanel(Panel panel)
        {
            // Services panel
            var servicesPanel = new GroupBox
            {
                Text = "Available Services",
                Dock = DockStyle.Top,
                Height = 200,
                Padding = new Padding(10)
            };
            UIStyles.ApplyGroupBoxStyle(servicesPanel);

            _servicesList = new ListView();
            UIStyles.ApplyListViewStyle(_servicesList);
            _servicesList.Dock = DockStyle.Fill;
            _servicesList.Columns.Add("Service", 200);
            _servicesList.Columns.Add("Price", 100);
            _servicesList.Columns.Add("Duration", 100);

            servicesPanel.Controls.Add(_servicesList);
            panel.Controls.Add(servicesPanel);

            // Reviews panel
            var reviewsPanel = new GroupBox
            {
                Text = "Recent Reviews",
                Dock = DockStyle.Top,
                Height = 200,
                Padding = new Padding(10)
            };
            UIStyles.ApplyGroupBoxStyle(reviewsPanel);

            _reviewsList = new ListView();
            UIStyles.ApplyListViewStyle(_reviewsList);
            _reviewsList.Dock = DockStyle.Fill;
            _reviewsList.Columns.Add("Customer", 150);
            _reviewsList.Columns.Add("Rating", 80);
            _reviewsList.Columns.Add("Comment", 250);
            _reviewsList.Columns.Add("Date", 120);

            reviewsPanel.Controls.Add(_reviewsList);
            panel.Controls.Add(reviewsPanel);

            // Upcoming Appointments panel
            var upcomingPanel = new GroupBox
            {
                Text = "Upcoming Appointments",
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };
            UIStyles.ApplyGroupBoxStyle(upcomingPanel);

            var upcomingLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 1,
                RowStyles = {
                    new RowStyle(SizeType.Absolute, 40),
                    new RowStyle(SizeType.Percent, 100)
                }
            };

            var addUpcomingBtn = new Button
            {
                Text = "Add",
                Width = 100,
                Height = 30,
                Anchor = AnchorStyles.Right
            };
            UIStyles.ApplyButtonStyle(addUpcomingBtn, true);
            addUpcomingBtn.Click += AddUpcomingAppointmentBtn_Click;
            upcomingLayout.Controls.Add(addUpcomingBtn, 0, 0);

            _upcomingAppointmentsList = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true
            };
            UIStyles.ApplyListViewStyle(_upcomingAppointmentsList);
            _upcomingAppointmentsList.Columns.Add("Date", 100);
            _upcomingAppointmentsList.Columns.Add("Time", 100);
            _upcomingAppointmentsList.Columns.Add("Service Type", 150);
            _upcomingAppointmentsList.Columns.Add("Status", 100);
            _upcomingAppointmentsList.Columns.Add("Notes", 200);
            upcomingLayout.Controls.Add(_upcomingAppointmentsList, 0, 1);

            upcomingPanel.Controls.Add(upcomingLayout);
            panel.Controls.Add(upcomingPanel);
        }

        private void AddUpcomingAppointmentBtn_Click(object sender, EventArgs e)
        {
            var addAppointmentForm = new AddAppointmentForm(_currentUser.Id, _currentUser);
            if (addAppointmentForm.ShowDialog() == DialogResult.OK)
            {
                LoadUpcomingAppointments();
            }
        }

        private void LoadUpcomingAppointments()
        {
            try
            {
                _upcomingAppointmentsList.Items.Clear();
                var now = DateTime.Now;
                var appointments = _databaseService.GetAppointmentsByServiceProvider(_currentUser.Id)
                    .Where(a => a.AppointmentDate >= now && a.Status != "Cancelled")
                    .OrderBy(a => a.AppointmentDate)
                    .ToList();
                foreach (var appointment in appointments)
                {
                    var item = new ListViewItem(appointment.AppointmentDate.ToShortDateString());
                    item.SubItems.Add(appointment.AppointmentDate.ToShortTimeString());
                    item.SubItems.Add(appointment.ServiceType);
                    item.SubItems.Add(appointment.Status);
                    item.SubItems.Add(appointment.Notes);
                    item.Tag = appointment.Id;
                    _upcomingAppointmentsList.Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading upcoming appointments: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadAppointments()
        {
            try
            {
                PerformSearch(); // Use the new search functionality
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading appointments: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadServices()
        {
            try
            {
                _servicesList.Items.Clear();
                var services = _databaseService.GetServicesByProvider(_currentUser.Id);

                foreach (var service in services)
                {
                    var item = new ListViewItem(service.Name);
                    item.SubItems.Add($"${service.Price:F2}");
                    item.SubItems.Add($"{service.DurationMinutes} min");
                    item.Tag = service.Id;
                    _servicesList.Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading services: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadReviews()
        {
            try
            {
                _reviewsList.Items.Clear();
                var reviews = _databaseService.GetReviewsByServiceProvider(_currentUser.Id);

                foreach (var review in reviews)
                {
                    var item = new ListViewItem(review.CustomerName);
                    item.SubItems.Add(review.Rating.ToString());
                    item.SubItems.Add(review.Comment);
                    item.SubItems.Add(review.Date.ToString("g"));
                    item.Tag = review.Id;
                    _reviewsList.Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading reviews: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
} 