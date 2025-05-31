using System;
using System.Windows.Forms;
using System.Drawing;
using AutoCarePro.Models;
using AutoCarePro.Services;
using AutoCarePro.UI;

namespace AutoCarePro.Forms
{
    public partial class EditAppointmentForm : Form
    {
        private readonly DatabaseService _dbService;
        private readonly int _appointmentId;
        private readonly User _currentUser;
        private DateTimePicker _datePicker;
        private DateTimePicker _timePicker;
        private ComboBox _serviceTypeCombo;
        private ComboBox _statusCombo;
        private TextBox _notesTextBox;
        private Button _saveBtn;
        private Button _cancelBtn;

        public EditAppointmentForm(int appointmentId, User currentUser)
        {
            InitializeComponent();
            _appointmentId = appointmentId;
            _currentUser = currentUser;
            _dbService = new DatabaseService();
            InitializeForm();
            LoadAppointmentData();
        }

        private void InitializeForm()
        {
            this.Text = "Edit Appointment";
            this.Size = new Size(400, 350);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Create main layout panel
            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 6,
                Padding = new Padding(10)
            };

            // Date picker
            mainPanel.Controls.Add(new Label { Text = "Date:", Dock = DockStyle.Fill }, 0, 0);
            _datePicker = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Dock = DockStyle.Fill
            };
            mainPanel.Controls.Add(_datePicker, 1, 0);

            // Time picker
            mainPanel.Controls.Add(new Label { Text = "Time:", Dock = DockStyle.Fill }, 0, 1);
            _timePicker = new DateTimePicker
            {
                Format = DateTimePickerFormat.Time,
                ShowUpDown = true,
                Dock = DockStyle.Fill
            };
            mainPanel.Controls.Add(_timePicker, 1, 1);

            // Service type combo
            mainPanel.Controls.Add(new Label { Text = "Service Type:", Dock = DockStyle.Fill }, 0, 2);
            _serviceTypeCombo = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _serviceTypeCombo.Items.AddRange(new string[] {
                "Regular Maintenance",
                "Oil Change",
                "Tire Rotation",
                "Brake Service",
                "Engine Repair",
                "Transmission Service",
                "Electrical System",
                "Air Conditioning",
                "Other"
            });
            mainPanel.Controls.Add(_serviceTypeCombo, 1, 2);

            // Status combo
            mainPanel.Controls.Add(new Label { Text = "Status:", Dock = DockStyle.Fill }, 0, 3);
            _statusCombo = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _statusCombo.Items.AddRange(new string[] {
                "Scheduled",
                "In Progress",
                "Completed",
                "Cancelled"
            });
            mainPanel.Controls.Add(_statusCombo, 1, 3);

            // Notes textbox
            mainPanel.Controls.Add(new Label { Text = "Notes:", Dock = DockStyle.Fill }, 0, 4);
            _notesTextBox = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                Height = 60
            };
            mainPanel.Controls.Add(_notesTextBox, 1, 4);

            // Buttons panel
            var buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                Height = 40
            };

            _saveBtn = new Button { Text = "Save", Width = 80 };
            _cancelBtn = new Button { Text = "Cancel", Width = 80 };

            UIStyles.ApplyButtonStyle(_saveBtn, true);
            UIStyles.ApplyButtonStyle(_cancelBtn);

            _saveBtn.Click += SaveBtn_Click;
            _cancelBtn.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            buttonPanel.Controls.AddRange(new Control[] { _cancelBtn, _saveBtn });
            mainPanel.Controls.Add(buttonPanel, 1, 5);

            this.Controls.Add(mainPanel);
        }

        private void LoadAppointmentData()
        {
            try
            {
                var appointment = _dbService.GetAppointmentById(_appointmentId);
                if (appointment != null)
                {
                    _datePicker.Value = appointment.AppointmentDate;
                    _timePicker.Value = appointment.AppointmentDate;
                    _serviceTypeCombo.SelectedItem = appointment.ServiceType;
                    _statusCombo.SelectedItem = appointment.Status;
                    _notesTextBox.Text = appointment.Notes;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading appointment data: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveBtn_Click(object sender, EventArgs e)
        {
            try
            {
                var appointment = _dbService.GetAppointmentById(_appointmentId);
                if (appointment != null)
                {
                    // Combine date and time
                    appointment.AppointmentDate = _datePicker.Value.Date.Add(_timePicker.Value.TimeOfDay);
                    appointment.ServiceType = _serviceTypeCombo.SelectedItem.ToString();
                    appointment.Status = _statusCombo.SelectedItem.ToString();
                    appointment.Notes = _notesTextBox.Text.Trim();
                    appointment.UpdatedAt = DateTime.Now;

                    // Update in database
                    _dbService.UpdateAppointment(appointment);

                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving appointment: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
} 