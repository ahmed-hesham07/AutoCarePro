using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;
using AutoCarePro.Models;
using AutoCarePro.Services;
using AutoCarePro.UI;

namespace AutoCarePro.Forms
{
    public partial class UpcomingVisitsForm : Form
    {
        private readonly DatabaseService _dbService;
        private readonly int _vehicleId;
        private readonly User _currentUser;

        // UI Controls
        private ListView _appointmentsList = new ListView();
        private Button _addAppointmentBtn = new Button();
        private Button _editAppointmentBtn = new Button();
        private Button _deleteAppointmentBtn = new Button();
        private Button _clearFiltersBtn = new Button();
        private ComboBox _statusFilterCombo = new ComboBox();
        private DateTimePicker _dateFilterPicker = new DateTimePicker();
        private System.Windows.Forms.Timer _fadeInTimer = new System.Windows.Forms.Timer();
        private double _fadeStep = 0.08;

        public UpcomingVisitsForm(int vehicleId, User currentUser)
        {
            _vehicleId = vehicleId;
            _currentUser = currentUser;
            _dbService = new DatabaseService();

            InitializeComponent();
            InitializeForm();
            LoadAppointments();
        }

        private void InitializeForm()
        {
            this.Text = "Upcoming Maintenance Visits";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterParent;

            // Create main layout panel
            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(10)
            };

            // Create filter panel
            var filterPanel = new Panel { Dock = DockStyle.Top, Height = 40 };
            InitializeFilterPanel(filterPanel);
            mainPanel.Controls.Add(filterPanel);

            // Create appointments panel
            var appointmentsPanel = new Panel { Dock = DockStyle.Fill };
            InitializeAppointmentsPanel(appointmentsPanel);
            mainPanel.Controls.Add(appointmentsPanel);

            this.Controls.Add(mainPanel);
        }

        private void InitializeFilterPanel(Panel panel)
        {
            var layout = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(5)
            };

            // Status filter
            _statusFilterCombo = new ComboBox
            {
                Width = 150,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _statusFilterCombo.Items.AddRange(new string[] { "All Status", "Scheduled", "In Progress", "Completed", "Cancelled" });
            _statusFilterCombo.SelectedIndex = 0;
            _statusFilterCombo.SelectedIndexChanged += (s, e) => LoadAppointments();
            layout.Controls.Add(_statusFilterCombo);

            // Date filter
            _dateFilterPicker = new DateTimePicker
            {
                Width = 150,
                Format = DateTimePickerFormat.Short
            };
            _dateFilterPicker.ValueChanged += (s, e) => LoadAppointments();
            layout.Controls.Add(_dateFilterPicker);

            // Clear filters button
            _clearFiltersBtn = new Button
            {
                Text = "Clear Filters",
                Width = 100
            };
            UIStyles.ApplyButtonStyle(_clearFiltersBtn);
            _clearFiltersBtn.Click += (s, e) =>
            {
                _statusFilterCombo.SelectedIndex = 0;
                _dateFilterPicker.Value = DateTime.Now;
                LoadAppointments();
            };
            layout.Controls.Add(_clearFiltersBtn);

            panel.Controls.Add(layout);
        }

        private void InitializeAppointmentsPanel(Panel panel)
        {
            // Create header panel with buttons
            var headerPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 40,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(5)
            };

            _addAppointmentBtn = new Button { Text = "Add Appointment", Width = 120 };
            _editAppointmentBtn = new Button { Text = "Edit", Width = 80 };
            _deleteAppointmentBtn = new Button { Text = "Delete", Width = 80 };

            UIStyles.ApplyButtonStyle(_addAppointmentBtn, true);
            UIStyles.ApplyButtonStyle(_editAppointmentBtn);
            UIStyles.ApplyButtonStyle(_deleteAppointmentBtn);

            _addAppointmentBtn.Click += AddAppointmentBtn_Click;
            _editAppointmentBtn.Click += EditAppointmentBtn_Click;
            _deleteAppointmentBtn.Click += DeleteAppointmentBtn_Click;

            headerPanel.Controls.AddRange(new Control[] { _deleteAppointmentBtn, _editAppointmentBtn, _addAppointmentBtn });
            panel.Controls.Add(headerPanel);

            // Create appointments list
            _appointmentsList = new ListView();
            UIStyles.ApplyListViewStyle(_appointmentsList);
            _appointmentsList.Dock = DockStyle.Fill;
            _appointmentsList.FullRowSelect = true;
            _appointmentsList.MultiSelect = false;
            _appointmentsList.View = View.Details;

            _appointmentsList.Columns.Add("Date", 100);
            _appointmentsList.Columns.Add("Time", 100);
            _appointmentsList.Columns.Add("Service Type", 150);
            _appointmentsList.Columns.Add("Status", 100);
            _appointmentsList.Columns.Add("Notes", 300);

            _appointmentsList.SelectedIndexChanged += (s, e) =>
            {
                var hasSelection = _appointmentsList.SelectedItems.Count > 0;
                _editAppointmentBtn.Enabled = hasSelection;
                _deleteAppointmentBtn.Enabled = hasSelection;
            };

            panel.Controls.Add(_appointmentsList);
        }

        private void LoadAppointments()
        {
            try
            {
                _appointmentsList.Items.Clear();
                var appointments = _dbService.GetAppointmentsByServiceProvider(_currentUser.Id)
                    .Where(a => a.VehicleId == _vehicleId)
                    .OrderBy(a => a.AppointmentDate)
                    .ToList();

                // Apply filters
                if (_statusFilterCombo.SelectedItem?.ToString() != "All Status")
                {
                    appointments = appointments.Where(a => a.Status == _statusFilterCombo.SelectedItem.ToString()).ToList();
                }

                if (_dateFilterPicker.Checked)
                {
                    var filterDate = _dateFilterPicker.Value.Date;
                    appointments = appointments.Where(a => a.AppointmentDate.Date == filterDate).ToList();
                }

                foreach (var appointment in appointments)
                {
                    var item = new ListViewItem(appointment.AppointmentDate.ToShortDateString());
                    item.SubItems.Add(appointment.AppointmentDate.ToShortTimeString());
                    item.SubItems.Add(appointment.ServiceType);
                    item.SubItems.Add(appointment.Status);
                    item.SubItems.Add(appointment.Notes);
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

        private void AddAppointmentBtn_Click(object sender, EventArgs e)
        {
            var addAppointmentForm = new AddAppointmentForm(_vehicleId, _currentUser);
            if (addAppointmentForm.ShowDialog() == DialogResult.OK)
            {
                LoadAppointments();
            }
        }

        private void EditAppointmentBtn_Click(object sender, EventArgs e)
        {
            if (_appointmentsList.SelectedItems.Count > 0)
            {
                var appointmentId = (int)_appointmentsList.SelectedItems[0].Tag;
                var editAppointmentForm = new EditAppointmentForm(appointmentId, _currentUser);
                if (editAppointmentForm.ShowDialog() == DialogResult.OK)
                {
                    LoadAppointments();
                }
            }
        }

        private void DeleteAppointmentBtn_Click(object sender, EventArgs e)
        {
            if (_appointmentsList.SelectedItems.Count > 0)
            {
                var result = MessageBox.Show("Are you sure you want to delete this appointment?",
                    "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    try
                    {
                        var appointmentId = (int)_appointmentsList.SelectedItems[0].Tag;
                        _dbService.DeleteAppointment(appointmentId);
                        LoadAppointments();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting appointment: {ex.Message}", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }
} 