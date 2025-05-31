using System;
using System.Windows.Forms;
using AutoCarePro.Services;
using AutoCarePro.Models;
using System.Drawing;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace AutoCarePro.Forms
{
    public partial class UnifiedDashboardForm : BaseForm
    {
        private readonly User _currentUser;
        private readonly DatabaseService _dbService;
        private readonly ILogger<UnifiedDashboardForm> _logger;
        private Panel _mainContentPanel;
        private Panel _sidebarPanel;
        private Button _currentActiveButton;

        public UnifiedDashboardForm(User user, ILogger<UnifiedDashboardForm> logger)
        {
            _logger = logger;
            InitializeComponent();
            _currentUser = user;
            _dbService = new DatabaseService(Program.CreateLogger<DatabaseService>());
            InitializeUI();
            LoadUserSpecificContent();
        }

        private void InitializeUI()
        {
            this.Size = new Size(1200, 800);
            this.Text = $"AutoCarePro - {_currentUser.Username}'s Dashboard";
            this.StartPosition = FormStartPosition.CenterScreen;

            // Initialize main panels
            _mainContentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            _sidebarPanel = new Panel
            {
                Dock = DockStyle.Left,
                Width = 200,
                BackColor = Color.FromArgb(45, 45, 48)
            };

            // Add panels to form
            this.Controls.Add(_mainContentPanel);
            this.Controls.Add(_sidebarPanel);

            // Create sidebar buttons
            CreateSidebarButtons();
        }

        private void CreateSidebarButtons()
        {
            var buttonHeight = 40;
            var buttonMargin = 5;
            var currentY = 20;

            // Common buttons for all users
            AddSidebarButton("Dashboard", currentY, () => ShowDashboard());
            currentY += buttonHeight + buttonMargin;

            AddSidebarButton("Profile", currentY, () => ShowProfile());
            currentY += buttonHeight + buttonMargin;

            // Role-specific buttons
            switch (_currentUser.Type)
            {
                case UserType.CarOwner:
                    AddSidebarButton("My Vehicles", currentY, () => ShowVehicles());
                    currentY += buttonHeight + buttonMargin;
                    AddSidebarButton("Appointments", currentY, () => ShowAppointments());
                    currentY += buttonHeight + buttonMargin;
                    AddSidebarButton("Maintenance History", currentY, () => ShowMaintenanceHistory());
                    break;

                case UserType.ServiceProvider:
                    AddSidebarButton("Service Management", currentY, () => ShowServiceManagement());
                    currentY += buttonHeight + buttonMargin;
                    AddSidebarButton("Appointments", currentY, () => ShowAppointments());
                    currentY += buttonHeight + buttonMargin;
                    AddSidebarButton("Reviews", currentY, () => ShowReviews());
                    break;

                case UserType.MaintenanceCenter:
                    AddSidebarButton("Service Management", currentY, () => ShowServiceManagement());
                    currentY += buttonHeight + buttonMargin;
                    AddSidebarButton("Appointments", currentY, () => ShowAppointments());
                    currentY += buttonHeight + buttonMargin;
                    AddSidebarButton("Reviews", currentY, () => ShowReviews());
                    currentY += buttonHeight + buttonMargin;
                    AddSidebarButton("Vehicle Management", currentY, () => ShowVehicleManagement());
                    currentY += buttonHeight + buttonMargin;
                    AddSidebarButton("Maintenance Records", currentY, () => ShowMaintenanceRecords());
                    currentY += buttonHeight + buttonMargin;
                    AddSidebarButton("Diagnostics", currentY, () => ShowDiagnostics());
                    break;
            }

            // Logout button at bottom
            var logoutButton = new Button
            {
                Text = "Logout",
                Location = new Point(10, _sidebarPanel.Height - 60),
                Size = new Size(180, 40),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(45, 45, 48)
            };
            logoutButton.Click += (s, e) => Logout();
            _sidebarPanel.Controls.Add(logoutButton);
        }

        private void AddSidebarButton(string text, int y, Action clickAction)
        {
            var button = new Button
            {
                Text = text,
                Location = new Point(10, y),
                Size = new Size(180, 40),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(45, 45, 48)
            };

            button.Click += (s, e) =>
            {
                if (_currentActiveButton != null)
                    _currentActiveButton.BackColor = Color.FromArgb(45, 45, 48);
                button.BackColor = Color.FromArgb(0, 122, 204);
                _currentActiveButton = button;
                clickAction();
            };

            _sidebarPanel.Controls.Add(button);
        }

        private void LoadUserSpecificContent()
        {
            ShowDashboard();
        }

        private void ShowDashboard()
        {
            _mainContentPanel.Controls.Clear();
            var dashboardContent = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20)
            };

            // Add welcome message
            var welcomeLabel = new Label
            {
                Text = $"Welcome, {_currentUser.Username}!",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(20, 20)
            };
            dashboardContent.Controls.Add(welcomeLabel);

            // Add role-specific dashboard content
            switch (_currentUser.Type)
            {
                case UserType.CarOwner:
                    AddCarOwnerDashboardContent(dashboardContent);
                    break;
                case UserType.ServiceProvider:
                    AddServiceProviderDashboardContent(dashboardContent);
                    break;
                case UserType.MaintenanceCenter:
                    AddMaintenanceCenterDashboardContent(dashboardContent);
                    break;
            }

            _mainContentPanel.Controls.Add(dashboardContent);
        }

        private void AddCarOwnerDashboardContent(Panel panel)
        {
            var vehicles = _dbService.GetVehiclesByUserId(_currentUser.Id);
            var appointments = _dbService.GetAppointmentsByVehicle(vehicles.FirstOrDefault()?.Id ?? 0);

            // Add quick stats
            AddQuickStatsPanel(panel, new[]
            {
                ("Vehicles", vehicles.Count.ToString()),
                ("Upcoming Appointments", appointments.Count.ToString()),
                ("Pending Maintenance", vehicles.Count(v => v.NeedsMaintenance).ToString())
            });
        }

        private void AddServiceProviderDashboardContent(Panel panel)
        {
            var appointments = _dbService.GetAppointmentsByServiceProvider(_currentUser.Id);
            var services = _dbService.GetServicesByProvider(_currentUser.Id);
            var reviews = _dbService.GetReviewsByServiceProvider(_currentUser.Id);

            // Add quick stats
            AddQuickStatsPanel(panel, new[]
            {
                ("Services Offered", services.Count.ToString()),
                ("Upcoming Appointments", appointments.Count.ToString()),
                ("Total Reviews", reviews.Count.ToString())
            });
        }

        private void AddMaintenanceCenterDashboardContent(Panel panel)
        {
            var vehicles = _dbService.GetAllVehicles();
            var maintenanceRecords = _dbService.GetRecentMaintenanceRecords(10);

            // Add quick stats
            AddQuickStatsPanel(panel, new[]
            {
                ("Total Vehicles", vehicles.Count.ToString()),
                ("Recent Maintenance Records", maintenanceRecords.Count.ToString()),
                ("Pending Diagnoses", maintenanceRecords.Count(r => r.DiagnosisRecommendations.Any()).ToString())
            });
        }

        private void AddQuickStatsPanel(Panel parent, (string label, string value)[] stats)
        {
            var statsPanel = new Panel
            {
                Location = new Point(20, 80),
                Size = new Size(parent.Width - 40, 100),
                BackColor = Color.White
            };

            var x = 20;
            foreach (var (label, value) in stats)
            {
                var statPanel = new Panel
                {
                    Location = new Point(x, 10),
                    Size = new Size(200, 80),
                    BackColor = Color.FromArgb(240, 240, 240)
                };

                var valueLabel = new Label
                {
                    Text = value,
                    Font = new Font("Segoe UI", 24, FontStyle.Bold),
                    AutoSize = true,
                    Location = new Point(10, 10)
                };

                var nameLabel = new Label
                {
                    Text = label,
                    Font = new Font("Segoe UI", 10),
                    AutoSize = true,
                    Location = new Point(10, 50)
                };

                statPanel.Controls.Add(valueLabel);
                statPanel.Controls.Add(nameLabel);
                statsPanel.Controls.Add(statPanel);

                x += 220;
            }

            parent.Controls.Add(statsPanel);
        }

        private void ShowProfile()
        {
            _mainContentPanel.Controls.Clear();
            var profileForm = new UserProfileForm(_currentUser);
            profileForm.TopLevel = false;
            profileForm.FormBorderStyle = FormBorderStyle.None;
            profileForm.Dock = DockStyle.Fill;
            _mainContentPanel.Controls.Add(profileForm);
            profileForm.Show();
        }

        private void ShowVehicles()
        {
            _mainContentPanel.Controls.Clear();
            var vehicles = _dbService.GetVehiclesByUserId(_currentUser.Id);
            // Implement vehicle list view
        }

        private void ShowAppointments()
        {
            _mainContentPanel.Controls.Clear();
            var appointmentsForm = new UpcomingVisitsForm(_currentUser);
            appointmentsForm.TopLevel = false;
            appointmentsForm.FormBorderStyle = FormBorderStyle.None;
            appointmentsForm.Dock = DockStyle.Fill;
            _mainContentPanel.Controls.Add(appointmentsForm);
            appointmentsForm.Show();
        }

        private void ShowMaintenanceHistory()
        {
            _mainContentPanel.Controls.Clear();
            var maintenanceForm = new MaintenanceHistoryForm(_currentUser);
            maintenanceForm.TopLevel = false;
            maintenanceForm.FormBorderStyle = FormBorderStyle.None;
            maintenanceForm.Dock = DockStyle.Fill;
            _mainContentPanel.Controls.Add(maintenanceForm);
            maintenanceForm.Show();
        }

        private void ShowServiceManagement()
        {
            _mainContentPanel.Controls.Clear();
            // Implement service management view
        }

        private void ShowReviews()
        {
            _mainContentPanel.Controls.Clear();
            // Implement reviews view
        }

        private void ShowVehicleManagement()
        {
            _mainContentPanel.Controls.Clear();
            var vehicleManagementForm = new VehicleManagementForm(_currentUser);
            vehicleManagementForm.TopLevel = false;
            vehicleManagementForm.FormBorderStyle = FormBorderStyle.None;
            vehicleManagementForm.Dock = DockStyle.Fill;
            _mainContentPanel.Controls.Add(vehicleManagementForm);
            vehicleManagementForm.Show();
        }

        private void ShowMaintenanceRecords()
        {
            _mainContentPanel.Controls.Clear();
            var maintenanceRecordsForm = new MaintenanceRecordsForm(_currentUser);
            maintenanceRecordsForm.TopLevel = false;
            maintenanceRecordsForm.FormBorderStyle = FormBorderStyle.None;
            maintenanceRecordsForm.Dock = DockStyle.Fill;
            _mainContentPanel.Controls.Add(maintenanceRecordsForm);
            maintenanceRecordsForm.Show();
        }

        private void ShowDiagnostics()
        {
            _mainContentPanel.Controls.Clear();
            // Implement diagnostics view
        }

        private void Logout()
        {
            SessionManager.ClearSession();
            this.Hide();
            new LoginForm().Show();
            this.Close();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            _dbService.Dispose();
        }
    }
} 