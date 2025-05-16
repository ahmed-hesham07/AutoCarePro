using System;
using System.Windows.Forms;
using System.Drawing;
using AutoCarePro.Models;
using AutoCarePro.Services;

namespace AutoCarePro.Forms
{
    public partial class AddVehicleForm : Form
    {
        private readonly int _userId;
        private readonly DatabaseService _dbService;
        private TextBox _makeTextBox;
        private TextBox _modelTextBox;
        private TextBox _yearTextBox;
        private TextBox _vinTextBox;
        private TextBox _mileageTextBox;
        private TextBox _licensePlateTextBox;
        private ComboBox _fuelTypeComboBox;
        private DateTimePicker _purchaseDatePicker;
        private TextBox _notesTextBox;
        private readonly Vehicle _existingVehicle;

        public AddVehicleForm(int userId, Vehicle existingVehicle = null)
        {
            InitializeComponent();
            _userId = userId;
            _existingVehicle = existingVehicle;
            _dbService = new DatabaseService();
            InitializeForm();
            if (existingVehicle != null)
            {
                LoadExistingVehicle();
            }
        }

        private void InitializeForm()
        {
            // Set up the form
            this.Text = "Add Vehicle";
            this.Size = new Size(500, 700);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Create main layout
            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(10)
            };

            // Form fields panel
            var fieldsPanel = new Panel { Dock = DockStyle.Fill };
            InitializeFields(fieldsPanel);

            // Buttons panel
            var buttonsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                FlowDirection = FlowDirection.RightToLeft,
                Height = 50
            };
            InitializeButtons(buttonsPanel);

            mainPanel.Controls.Add(fieldsPanel);
            mainPanel.Controls.Add(buttonsPanel);

            this.Controls.Add(mainPanel);
        }

        private void InitializeFields(Panel panel)
        {
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 9,
                Padding = new Padding(10)
            };

            // Make
            layout.Controls.Add(new Label { Text = "Make:", AutoSize = true }, 0, 0);
            _makeTextBox = new TextBox { Width = 200 };
            layout.Controls.Add(_makeTextBox, 1, 0);

            // Model
            layout.Controls.Add(new Label { Text = "Model:", AutoSize = true }, 0, 1);
            _modelTextBox = new TextBox { Width = 200 };
            layout.Controls.Add(_modelTextBox, 1, 1);

            // Year
            layout.Controls.Add(new Label { Text = "Year:", AutoSize = true }, 0, 2);
            _yearTextBox = new TextBox { Width = 200 };
            layout.Controls.Add(_yearTextBox, 1, 2);

            // VIN
            layout.Controls.Add(new Label { Text = "VIN:", AutoSize = true }, 0, 3);
            _vinTextBox = new TextBox { Width = 200 };
            layout.Controls.Add(_vinTextBox, 1, 3);

            // License Plate
            layout.Controls.Add(new Label { Text = "License Plate:", AutoSize = true }, 0, 4);
            _licensePlateTextBox = new TextBox { Width = 200 };
            layout.Controls.Add(_licensePlateTextBox, 1, 4);

            // Current Mileage
            layout.Controls.Add(new Label { Text = "Current Mileage:", AutoSize = true }, 0, 5);
            _mileageTextBox = new TextBox { Width = 200 };
            layout.Controls.Add(_mileageTextBox, 1, 5);

            // Fuel Type
            layout.Controls.Add(new Label { Text = "Fuel Type:", AutoSize = true }, 0, 6);
            _fuelTypeComboBox = new ComboBox
            {
                Width = 200,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _fuelTypeComboBox.Items.AddRange(new string[] { "Gasoline", "Diesel", "Electric", "Hybrid" });
            _fuelTypeComboBox.SelectedIndex = 0;
            layout.Controls.Add(_fuelTypeComboBox, 1, 6);

            // Purchase Date
            layout.Controls.Add(new Label { Text = "Purchase Date:", AutoSize = true }, 0, 7);
            _purchaseDatePicker = new DateTimePicker { Width = 200 };
            layout.Controls.Add(_purchaseDatePicker, 1, 7);

            // Notes
            layout.Controls.Add(new Label { Text = "Notes:", AutoSize = true }, 0, 8);
            _notesTextBox = new TextBox
            {
                Width = 200,
                Height = 100,
                Multiline = true
            };
            layout.Controls.Add(_notesTextBox, 1, 8);

            panel.Controls.Add(layout);
        }

        private void InitializeButtons(FlowLayoutPanel panel)
        {
            var saveButton = new Button
            {
                Text = "Save",
                DialogResult = DialogResult.OK
            };
            saveButton.Click += SaveButton_Click;

            var cancelButton = new Button
            {
                Text = "Cancel",
                DialogResult = DialogResult.Cancel
            };

            panel.Controls.Add(cancelButton);
            panel.Controls.Add(saveButton);
        }

        private void LoadExistingVehicle()
        {
            this.Text = "Edit Vehicle";
            _makeTextBox.Text = _existingVehicle.Make;
            _modelTextBox.Text = _existingVehicle.Model;
            _yearTextBox.Text = _existingVehicle.Year.ToString();
            _vinTextBox.Text = _existingVehicle.VIN;
            _mileageTextBox.Text = _existingVehicle.CurrentMileage.ToString();
            _licensePlateTextBox.Text = _existingVehicle.LicensePlate;
            _fuelTypeComboBox.SelectedItem = _existingVehicle.FuelType;
            _purchaseDatePicker.Value = _existingVehicle.PurchaseDate;
            _notesTextBox.Text = _existingVehicle.Notes;
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            try
            {
                // Validate inputs
                if (string.IsNullOrWhiteSpace(_makeTextBox.Text))
                {
                    MessageBox.Show("Please enter the vehicle make.", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(_modelTextBox.Text))
                {
                    MessageBox.Show("Please enter the vehicle model.", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!int.TryParse(_yearTextBox.Text, out int year) || year < 1900 || year > DateTime.Now.Year)
                {
                    MessageBox.Show("Please enter a valid year.", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!double.TryParse(_mileageTextBox.Text, out double mileage))
                {
                    MessageBox.Show("Please enter a valid mileage.", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Create or update vehicle
                var vehicle = _existingVehicle ?? new Car { UserId = _userId };
                vehicle.Make = _makeTextBox.Text;
                vehicle.Model = _modelTextBox.Text;
                vehicle.Year = year;
                vehicle.VIN = _vinTextBox.Text;
                vehicle.LicensePlate = _licensePlateTextBox.Text;
                vehicle.CurrentMileage = mileage;
                vehicle.FuelType = _fuelTypeComboBox.SelectedItem.ToString();
                vehicle.PurchaseDate = _purchaseDatePicker.Value;
                vehicle.Notes = _notesTextBox.Text;

                // Save to database
                if (_existingVehicle == null)
                {
                    _dbService.AddVehicle(vehicle);
                }
                _dbService.SaveChanges();

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving vehicle: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
} 