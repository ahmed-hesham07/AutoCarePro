using System;
using System.Windows.Forms;
using System.Drawing;
using System.Linq;
using System.Collections.Generic;
using AutoCarePro.Models;
using AutoCarePro.Services;
using AutoCarePro.UI;

namespace AutoCarePro.Forms
{
    public partial class ServiceManagementForm : BaseForm
    {
        private readonly DatabaseService _dbService;
        private readonly User _currentUser;

        private DataGridView _servicesGrid = null!;
        private Button _addServiceButton = null!;
        private Button _editServiceButton = null!;
        private Button _deleteServiceButton = null!;
        private Button _viewBookingsButton = null!;
        private ComboBox _categoryFilterComboBox = null!;
        private TextBox _searchTextBox = null!;
        private Button _applyFiltersButton = null!;
        private Button _clearFiltersButton = null!;

        private List<Service> _services = new List<Service>();

        public ServiceManagementForm(User currentUser)
        {
            InitializeComponent();
            _currentUser = currentUser;
            _dbService = new DatabaseService();
            InitializeUI();
            LoadServices();
        }

        private void InitializeUI()
        {
            this.Text = "Service Management";
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

            // Category filter
            filterPanel.Controls.Add(new Label { Text = "Category:", Location = new Point(10, 15) });
            _categoryFilterComboBox = new ComboBox
            {
                Location = new Point(70, 12),
                Width = 150,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            filterPanel.Controls.Add(_categoryFilterComboBox);

            // Search box
            filterPanel.Controls.Add(new Label { Text = "Search:", Location = new Point(230, 15) });
            _searchTextBox = new TextBox
            {
                Location = new Point(280, 12),
                Width = 200
            };
            filterPanel.Controls.Add(_searchTextBox);

            // Filter buttons
            _applyFiltersButton = new Button
            {
                Text = "Apply Filters",
                Location = new Point(490, 10),
                Width = 100
            };
            UIStyles.ApplyButtonStyle(_applyFiltersButton);
            filterPanel.Controls.Add(_applyFiltersButton);

            _clearFiltersButton = new Button
            {
                Text = "Clear Filters",
                Location = new Point(600, 10),
                Width = 100
            };
            UIStyles.ApplyButtonStyle(_clearFiltersButton);
            filterPanel.Controls.Add(_clearFiltersButton);

            // Create action buttons
            _addServiceButton = new Button
            {
                Text = "Add Service",
                Location = new Point(10, 10),
                Size = new Size(100, 30)
            };
            UIStyles.ApplyButtonStyle(_addServiceButton);

            _editServiceButton = new Button
            {
                Text = "Edit Service",
                Location = new Point(120, 10),
                Size = new Size(100, 30),
                Enabled = false
            };
            UIStyles.ApplyButtonStyle(_editServiceButton);

            _deleteServiceButton = new Button
            {
                Text = "Delete Service",
                Location = new Point(230, 10),
                Size = new Size(100, 30),
                Enabled = false
            };
            UIStyles.ApplyButtonStyle(_deleteServiceButton);

            _viewBookingsButton = new Button
            {
                Text = "View Bookings",
                Location = new Point(340, 10),
                Size = new Size(120, 30),
                Enabled = false
            };
            UIStyles.ApplyButtonStyle(_viewBookingsButton);

            // Add buttons to toolbar
            toolbarPanel.Controls.AddRange(new Control[] { 
                _addServiceButton, _editServiceButton, _deleteServiceButton, _viewBookingsButton 
            });

            // Create services grid
            _servicesGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            UIStyles.ApplyDataGridViewStyle(_servicesGrid);

            // Add event handlers
            _addServiceButton.Click += AddServiceButton_Click;
            _editServiceButton.Click += EditServiceButton_Click;
            _deleteServiceButton.Click += DeleteServiceButton_Click;
            _viewBookingsButton.Click += ViewBookingsButton_Click;
            _servicesGrid.SelectionChanged += ServicesGrid_SelectionChanged;
            _applyFiltersButton.Click += ApplyFiltersButton_Click;
            _clearFiltersButton.Click += ClearFiltersButton_Click;
            _searchTextBox.TextChanged += SearchTextBox_TextChanged;

            // Add controls to form
            this.Controls.Add(_servicesGrid);
            this.Controls.Add(filterPanel);
            this.Controls.Add(toolbarPanel);

            // Initialize filters
            InitializeFilters();
        }

        private void InitializeFilters()
        {
            // Load service categories
            _categoryFilterComboBox.Items.Add("All Categories");
            _categoryFilterComboBox.Items.AddRange(new string[] {
                "Maintenance",
                "Repair",
                "Diagnostic",
                "Cleaning",
                "Inspection",
                "Other"
            });
            _categoryFilterComboBox.SelectedIndex = 0;
        }

        private async void LoadServices()
        {
            try
            {
                var services = await _dbService.GetServicesByProviderIdAsync(_currentUser.Id);
                _services = services.ToList();
                UpdateServicesGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading services: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateServicesGrid()
        {
            _servicesGrid.DataSource = null;
            _servicesGrid.DataSource = _services.Select(s => new
            {
                s.Id,
                s.Name,
                s.Category,
                s.Price,
                Duration = $"{s.DurationMinutes} minutes",
                s.IsAvailable
            }).ToList();
        }

        private void ServicesGrid_SelectionChanged(object? sender, EventArgs e)
        {
            bool hasSelection = _servicesGrid.SelectedRows.Count > 0;
            _editServiceButton.Enabled = hasSelection;
            _deleteServiceButton.Enabled = hasSelection;
            _viewBookingsButton.Enabled = hasSelection;
        }

        private void AddServiceButton_Click(object? sender, EventArgs e)
        {
            using (var form = new ServiceForm(_currentUser))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadServices();
                }
            }
        }

        private void EditServiceButton_Click(object? sender, EventArgs e)
        {
            if (_servicesGrid.SelectedRows.Count > 0)
            {
                var service = (Service)_servicesGrid.SelectedRows[0].DataBoundItem;
                using (var form = new ServiceForm(_currentUser, service))
                {
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        LoadServices();
                    }
                }
            }
        }

        private async void DeleteServiceButton_Click(object? sender, EventArgs e)
        {
            if (_servicesGrid.SelectedRows.Count > 0)
            {
                var service = (Service)_servicesGrid.SelectedRows[0].DataBoundItem;
                var result = MessageBox.Show(
                    "Are you sure you want to delete this service?",
                    "Confirm Delete",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    try
                    {
                        await _dbService.DeleteServiceAsync(service.Id);
                        LoadServices();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting service: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void ViewBookingsButton_Click(object? sender, EventArgs e)
        {
            if (_servicesGrid.SelectedRows.Count > 0)
            {
                var service = (Service)_servicesGrid.SelectedRows[0].DataBoundItem;
                using (var form = new ServiceBookingsForm(service))
                {
                    form.ShowDialog();
                }
            }
        }

        private async void ApplyFiltersButton_Click(object? sender, EventArgs e)
        {
            try
            {
                var services = await _dbService.GetServicesByProviderIdAsync(_currentUser.Id);
                var filteredServices = services;

                if (!string.IsNullOrWhiteSpace(_searchTextBox.Text))
                {
                    filteredServices = filteredServices.Where(s => 
                        s.Name.Contains(_searchTextBox.Text, StringComparison.OrdinalIgnoreCase) ||
                        s.Description.Contains(_searchTextBox.Text, StringComparison.OrdinalIgnoreCase));
                }

                if (_categoryFilterComboBox.SelectedItem != null)
                {
                    var category = (string)_categoryFilterComboBox.SelectedItem;
                    filteredServices = filteredServices.Where(s => s.Category == category);
                }

                _services = filteredServices.ToList();
                UpdateServicesGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error applying filters: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ClearFiltersButton_Click(object? sender, EventArgs e)
        {
            _searchTextBox.Clear();
            _categoryFilterComboBox.SelectedIndex = -1;
            LoadServices();
        }

        private void SearchTextBox_TextChanged(object? sender, EventArgs e)
        {
            ApplyFiltersButton_Click(sender, e);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            _dbService.Dispose();
        }
    }
} 