using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;
using AutoCarePro.Models;
using AutoCarePro.Services;

namespace AutoCarePro.Forms
{
    /// <summary>
    /// Form that displays the maintenance history for a specific vehicle
    /// </summary>
    public partial class MaintenanceHistoryForm : Form
    {
        // Service instance for database operations
        private readonly DatabaseService _dbService;
        private readonly int _vehicleId;
        private Vehicle _vehicle;

        // UI Controls for displaying and filtering maintenance history
        private ListView _maintenanceList;     // Displays maintenance records
        private ComboBox _filterTypeComboBox; // Filter by maintenance type
        private DateTimePicker _startDatePicker; // Filter by date range
        private DateTimePicker _endDatePicker;   // Filter by date range
        private Button _applyFilterBtn;        // Apply filter button
        private Button _exportBtn;             // Export to CSV button
        private Button _printBtn;              // Print history button
        private Label _totalCostLabel;         // Shows total maintenance cost
        private Label _lastServiceLabel;       // Shows last service date

        /// <summary>
        /// Initializes the maintenance history form for a specific vehicle
        /// </summary>
        /// <param name="vehicleId">ID of the vehicle to show history for</param>
        public MaintenanceHistoryForm(int vehicleId)
        {
            InitializeComponent();
            _vehicleId = vehicleId;
            _dbService = new DatabaseService();
            LoadVehicleData();
            InitializeForm();
            LoadMaintenanceHistory();
        }

        /// <summary>
        /// Loads the vehicle data from the database
        /// </summary>
        private void LoadVehicleData()
        {
            _vehicle = _dbService.GetVehicleById(_vehicleId);
            if (_vehicle == null)
            {
                MessageBox.Show("Vehicle not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }
        }

        /// <summary>
        /// Sets up the form layout and initializes all components
        /// </summary>
        private void InitializeForm()
        {
            // Configure main form properties
            this.Text = $"Maintenance History - {_vehicle.Make} {_vehicle.Model}";
            this.Size = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Create main layout panel
            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(10)
            };

            // Create filter panel
            var filterPanel = new Panel { Dock = DockStyle.Top, Height = 60 };
            InitializeFilterPanel(filterPanel);

            // Create statistics panel
            var statsPanel = new Panel { Dock = DockStyle.Top, Height = 40 };
            InitializeStatsPanel(statsPanel);

            // Create maintenance list panel
            var listPanel = new Panel { Dock = DockStyle.Fill };
            InitializeMaintenanceList(listPanel);

            // Add panels to main layout
            mainPanel.Controls.Add(filterPanel);
            mainPanel.Controls.Add(statsPanel);
            mainPanel.Controls.Add(listPanel);

            this.Controls.Add(mainPanel);
        }

        /// <summary>
        /// Initializes the filter panel with controls for filtering maintenance history
        /// </summary>
        private void InitializeFilterPanel(Panel panel)
        {
            var layout = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(5)
            };

            // Maintenance type filter
            layout.Controls.Add(new Label { Text = "Type:", AutoSize = true });
            _filterTypeComboBox = new ComboBox
            {
                Width = 120,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _filterTypeComboBox.Items.AddRange(new string[] { "All", "Oil Change", "Tire Rotation", "Brake Service", "Other" });
            _filterTypeComboBox.SelectedIndex = 0;
            layout.Controls.Add(_filterTypeComboBox);

            // Date range filters
            layout.Controls.Add(new Label { Text = "From:", AutoSize = true });
            _startDatePicker = new DateTimePicker { Width = 120 };
            layout.Controls.Add(_startDatePicker);

            layout.Controls.Add(new Label { Text = "To:", AutoSize = true });
            _endDatePicker = new DateTimePicker { Width = 120 };
            layout.Controls.Add(_endDatePicker);

            // Apply filter button
            _applyFilterBtn = new Button
            {
                Text = "Apply Filter",
                Width = 100
            };
            _applyFilterBtn.Click += ApplyFilterBtn_Click;
            layout.Controls.Add(_applyFilterBtn);

            panel.Controls.Add(layout);
        }

        /// <summary>
        /// Initializes the statistics panel showing maintenance summary
        /// </summary>
        private void InitializeStatsPanel(Panel panel)
        {
            var layout = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(5)
            };

            // Total cost label
            _totalCostLabel = new Label { AutoSize = true };
            layout.Controls.Add(_totalCostLabel);

            layout.Controls.Add(new Label { Text = "|", AutoSize = true });

            // Last service label
            _lastServiceLabel = new Label { AutoSize = true };
            layout.Controls.Add(_lastServiceLabel);

            // Export and print buttons
            _exportBtn = new Button
            {
                Text = "Export to CSV",
                Width = 100
            };
            _exportBtn.Click += ExportBtn_Click;
            layout.Controls.Add(_exportBtn);

            _printBtn = new Button
            {
                Text = "Print History",
                Width = 100
            };
            _printBtn.Click += PrintBtn_Click;
            layout.Controls.Add(_printBtn);

            panel.Controls.Add(layout);
        }

        /// <summary>
        /// Initializes the maintenance history list view
        /// </summary>
        private void InitializeMaintenanceList(Panel panel)
        {
            _maintenanceList = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true
            };

            // Add columns to list view
            _maintenanceList.Columns.Add("Date", 100);           // Maintenance date
            _maintenanceList.Columns.Add("Type", 100);          // Maintenance type
            _maintenanceList.Columns.Add("Description", 200);   // Maintenance description
            _maintenanceList.Columns.Add("Mileage", 100);       // Mileage at maintenance
            _maintenanceList.Columns.Add("Cost", 100);          // Maintenance cost
            _maintenanceList.Columns.Add("Provider", 150);      // Service provider
            _maintenanceList.Columns.Add("Notes", 200);         // Additional notes

            panel.Controls.Add(_maintenanceList);
        }

        /// <summary>
        /// Loads and displays maintenance history based on current filters
        /// </summary>
        private void LoadMaintenanceHistory()
        {
            try
            {
                // Clear existing items
                _maintenanceList.Items.Clear();

                // Get maintenance records from database
                var records = _dbService.GetMaintenanceRecords(_vehicleId);

                // Apply filters
                if (_filterTypeComboBox.SelectedIndex > 0)
                {
                    records = records.Where(r => r.MaintenanceType == _filterTypeComboBox.SelectedItem.ToString());
                }

                records = records.Where(r => r.MaintenanceDate >= _startDatePicker.Value &&
                                          r.MaintenanceDate <= _endDatePicker.Value);

                // Sort by date (newest first)
                records = records.OrderByDescending(r => r.MaintenanceDate);

                // Add records to list view
                foreach (var record in records)
                {
                    var item = new ListViewItem(record.MaintenanceDate.ToShortDateString());
                    item.SubItems.Add(record.MaintenanceType);
                    item.SubItems.Add(record.Description);
                    item.SubItems.Add(record.MileageAtMaintenance.ToString());
                    item.SubItems.Add(record.Cost.ToString("C"));
                    item.SubItems.Add(record.ServiceProvider);
                    item.SubItems.Add(record.Notes);
                    _maintenanceList.Items.Add(item);
                }

                // Update statistics
                UpdateStatistics(records);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading maintenance history: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Updates the statistics panel with maintenance summary
        /// </summary>
        private void UpdateStatistics(IEnumerable<MaintenanceRecord> records)
        {
            // Calculate total cost
            var totalCost = records.Sum(r => r.Cost);
            _totalCostLabel.Text = $"Total Cost: {totalCost:C}";

            // Get last service date
            var lastService = records.OrderByDescending(r => r.MaintenanceDate).FirstOrDefault();
            _lastServiceLabel.Text = lastService != null
                ? $"Last Service: {lastService.MaintenanceDate.ToShortDateString()}"
                : "No maintenance records";
        }

        /// <summary>
        /// Handles click event for Apply Filter button
        /// </summary>
        private void ApplyFilterBtn_Click(object sender, EventArgs e)
        {
            LoadMaintenanceHistory();
        }

        /// <summary>
        /// Handles click event for Export to CSV button
        /// </summary>
        private void ExportBtn_Click(object sender, EventArgs e)
        {
            try
            {
                var saveDialog = new SaveFileDialog
                {
                    Filter = "CSV Files (*.csv)|*.csv",
                    FileName = $"Maintenance_History_{_vehicle.Make}_{_vehicle.Model}_{DateTime.Now:yyyyMMdd}.csv"
                };

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    // Create CSV content
                    var csv = new System.Text.StringBuilder();
                    csv.AppendLine("Date,Type,Description,Mileage,Cost,Provider,Notes");

                    foreach (ListViewItem item in _maintenanceList.Items)
                    {
                        var line = string.Join(",", item.SubItems.Cast<ListViewItem.ListViewSubItem>()
                            .Select(subItem => $"\"{subItem.Text}\""));
                        csv.AppendLine(line);
                    }

                    // Save to file
                    System.IO.File.WriteAllText(saveDialog.FileName, csv.ToString());
                    MessageBox.Show("Maintenance history exported successfully!", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting to CSV: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Handles click event for Print History button
        /// </summary>
        private void PrintBtn_Click(object sender, EventArgs e)
        {
            try
            {
                var printDocument = new System.Drawing.Printing.PrintDocument();
                printDocument.PrintPage += PrintDocument_PrintPage;

                var printDialog = new PrintDialog
                {
                    Document = printDocument
                };

                if (printDialog.ShowDialog() == DialogResult.OK)
                {
                    printDocument.Print();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error printing history: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Handles the print page event for printing maintenance history
        /// </summary>
        private void PrintDocument_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            var graphics = e.Graphics;
            var font = new Font("Arial", 10);
            var boldFont = new Font("Arial", 12, FontStyle.Bold);
            var yPos = 50f;
            var leftMargin = 50f;

            // Print header
            graphics.DrawString($"Maintenance History - {_vehicle.Make} {_vehicle.Model}", boldFont,
                Brushes.Black, leftMargin, yPos);
            yPos += 30;

            // Print column headers
            var xPos = leftMargin;
            foreach (ColumnHeader column in _maintenanceList.Columns)
            {
                graphics.DrawString(column.Text, boldFont, Brushes.Black, xPos, yPos);
                xPos += column.Width;
            }
            yPos += 20;

            // Print records
            foreach (ListViewItem item in _maintenanceList.Items)
            {
                xPos = leftMargin;
                foreach (ListViewItem.ListViewSubItem subItem in item.SubItems)
                {
                    graphics.DrawString(subItem.Text, font, Brushes.Black, xPos, yPos);
                    xPos += _maintenanceList.Columns[item.SubItems.IndexOf(subItem)].Width;
                }
                yPos += 20;

                // Check if we need a new page
                if (yPos > e.MarginBounds.Bottom)
                {
                    e.HasMorePages = true;
                    return;
                }
            }
        }
    }
}
