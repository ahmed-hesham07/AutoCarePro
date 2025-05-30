using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;
using AutoCarePro.Models;
using AutoCarePro.Services;
using System.Threading.Tasks;
using AutoCarePro.UI;

namespace AutoCarePro.Forms
{
    /// <summary>
    /// Form that displays the maintenance history for a specific vehicle.
    /// This form provides a comprehensive interface for users to:
    /// 1. View all maintenance records for their vehicle
    /// 2. Filter records by type and date range
    /// 3. View maintenance statistics and costs
    /// 4. Export maintenance history to CSV
    /// 5. Print maintenance records
    /// 
    /// The form is organized into three main sections:
    /// - Filter panel: For filtering maintenance records
    /// - Statistics panel: Shows summary information
    /// - Maintenance list: Displays detailed maintenance records
    /// </summary>
    public partial class MaintenanceHistoryForm : Form
    {
        // Service instance for database operations
        private readonly DatabaseService _dbService;  // Handles all database operations
        private readonly int _vehicleId;             // ID of the vehicle being viewed
        private Vehicle? _vehicle = null;

        // UI Controls for displaying and filtering maintenance history
        private ListView _maintenanceList = new ListView();
        private ComboBox _filterTypeComboBox = new ComboBox();
        private DateTimePicker _startDatePicker = new DateTimePicker();
        private DateTimePicker _endDatePicker = new DateTimePicker();
        private Button _applyFilterBtn = new Button();
        private Button _exportBtn = new Button();
        private Button _printBtn = new Button();
        private Label _totalCostLabel = new Label();
        private Label _lastServiceLabel = new Label();
        private Button _darkModeToggleBtn = new Button();
        private Button _accentColorBtn = new Button();
        private System.Windows.Forms.Timer _fadeInTimer = new System.Windows.Forms.Timer();
        private double _fadeStep = 0.08;

        // Pagination state for printing
        private int _printRecordIndex = 0;

        /// <summary>
        /// Initializes the maintenance history form for a specific vehicle.
        /// This constructor is called when creating a new instance of the form.
        /// It sets up the database service, loads vehicle data, and initializes the UI.
        /// </summary>
        /// <param name="vehicleId">ID of the vehicle to show maintenance history for</param>
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
        /// Loads the vehicle data from the database.
        /// This method retrieves the vehicle information needed to display the form title
        /// and validate that the vehicle exists.
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
        /// Sets up the form layout and initializes all components.
        /// This method creates the three-panel layout and initializes all UI elements.
        /// The form is designed to provide easy access to maintenance history and filtering options.
        /// </summary>
        private void InitializeForm()
        {
            // Configure main form properties
            UIStyles.ApplyFormStyle(this);
            this.Text = _vehicle != null ? $"Maintenance History - {_vehicle.Make} {_vehicle.Model}" : "Maintenance History";
            this.Size = new Size(1200, 700);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Add dark mode toggle and accent color picker (top-right)
            _darkModeToggleBtn = new Button
            {
                Text = ThemeManager.Instance.IsDarkMode ? "☀️" : "🌙",
                Width = 40,
                Height = 40,
                Top = 10,
                Left = this.ClientSize.Width - 100,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            UIStyles.ApplyButtonStyle(_darkModeToggleBtn, true);
            _darkModeToggleBtn.Click += (s, e) => {
                ThemeManager.Instance.IsDarkMode = !ThemeManager.Instance.IsDarkMode;
                _darkModeToggleBtn.Text = ThemeManager.Instance.IsDarkMode ? "☀️" : "🌙";
                ThemeManager.Instance.ApplyTheme(this);
                UIStyles.RefreshStyles(this);
            };

            _accentColorBtn = new Button
            {
                Text = "🎨",
                Width = 40,
                Height = 40,
                Top = 10,
                Left = this.ClientSize.Width - 50,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            UIStyles.ApplyButtonStyle(_accentColorBtn, true);
            _accentColorBtn.Click += (s, e) => {
                using (var colorDialog = new ColorDialog())
                {
                    colorDialog.Color = ThemeManager.Instance.AccentColor;
                    if (colorDialog.ShowDialog() == DialogResult.OK)
                    {
                        ThemeManager.Instance.SetAccentColor(colorDialog.Color);
                        UIStyles.RefreshStyles(this);
                    }
                }
            };

            this.Controls.Add(_darkModeToggleBtn);
            this.Controls.Add(_accentColorBtn);

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
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(10)
            };

            // Create filter panel for filtering maintenance records
            var filterPanel = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = UIStyles.Colors.Secondary };
            InitializeFilterPanel(filterPanel);

            // Create statistics panel for showing summary information
            var statsPanel = new Panel { Dock = DockStyle.Top, Height = 40, BackColor = UIStyles.Colors.Secondary };
            InitializeStatsPanel(statsPanel);

            // Create maintenance list panel for displaying records
            var listPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
            InitializeMaintenanceList(listPanel);

            // Add panels to main layout
            mainPanel.Controls.Add(filterPanel);
            mainPanel.Controls.Add(statsPanel);
            mainPanel.Controls.Add(listPanel);

            this.Controls.Add(mainPanel);
        }

        /// <summary>
        /// Initializes the filter panel with controls for filtering maintenance history.
        /// This panel allows users to filter records by maintenance type and date range.
        /// </summary>
        /// <param name="panel">The panel to initialize with filter controls</param>
        private void InitializeFilterPanel(Panel panel)
        {
            // Create a flow layout panel for horizontal arrangement of controls
            var layout = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(5),
                BackColor = UIStyles.Colors.Secondary
            };

            // Maintenance type filter dropdown
            var lblType = new Label { Text = "Type:", AutoSize = true };
            UIStyles.ApplyLabelStyle(lblType);
            layout.Controls.Add(lblType);
            _filterTypeComboBox = new ComboBox
            {
                Width = 120,
                DropDownStyle = ComboBoxStyle.DropDownList  // Prevents user from typing in the combo box
            };
            UIStyles.ApplyComboBoxStyle(_filterTypeComboBox);
            _filterTypeComboBox.Items.AddRange(new string[] { "All", "Oil Change", "Tire Rotation", "Brake Service", "Other" });
            _filterTypeComboBox.SelectedIndex = 0;  // Select "All" by default
            layout.Controls.Add(_filterTypeComboBox);

            // Date range filters with date pickers
            var lblFrom = new Label { Text = "From:", AutoSize = true };
            UIStyles.ApplyLabelStyle(lblFrom);
            layout.Controls.Add(lblFrom);
            _startDatePicker = new DateTimePicker { Width = 120 };
            layout.Controls.Add(_startDatePicker);

            var lblTo = new Label { Text = "To:", AutoSize = true };
            UIStyles.ApplyLabelStyle(lblTo);
            layout.Controls.Add(lblTo);
            _endDatePicker = new DateTimePicker { Width = 120 };
            layout.Controls.Add(_endDatePicker);

            // Apply filter button
            _applyFilterBtn = new Button
            {
                Text = "Apply Filter",
                Width = 100
            };
            UIStyles.ApplyButtonStyle(_applyFilterBtn, true);
            _applyFilterBtn.Click += ApplyFilterBtn_Click;
            layout.Controls.Add(_applyFilterBtn);

            panel.Controls.Add(layout);
        }

        /// <summary>
        /// Initializes the statistics panel showing maintenance summary.
        /// This panel displays total maintenance cost and last service date,
        /// along with buttons for exporting and printing.
        /// </summary>
        /// <param name="panel">The panel to initialize with statistics and action buttons</param>
        private void InitializeStatsPanel(Panel panel)
        {
            // Create a flow layout panel for horizontal arrangement of controls
            var layout = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(5),
                BackColor = UIStyles.Colors.Secondary
            };

            // Total cost label for displaying maintenance expenses
            _totalCostLabel = new Label { AutoSize = true };
            UIStyles.ApplyLabelStyle(_totalCostLabel, true);
            layout.Controls.Add(_totalCostLabel);

            // Separator between statistics
            var lblSep = new Label { Text = "|", AutoSize = true };
            UIStyles.ApplyLabelStyle(lblSep);
            layout.Controls.Add(lblSep);

            // Last service label for displaying most recent maintenance date
            _lastServiceLabel = new Label { AutoSize = true };
            UIStyles.ApplyLabelStyle(_lastServiceLabel);
            layout.Controls.Add(_lastServiceLabel);

            // Export button for saving history to CSV
            _exportBtn = new Button
            {
                Text = "Export to CSV",
                Width = 100
            };
            UIStyles.ApplyButtonStyle(_exportBtn);
            _exportBtn.Click += ExportBtn_Click;
            layout.Controls.Add(_exportBtn);

            // Print button for printing maintenance history
            _printBtn = new Button
            {
                Text = "Print History",
                Width = 100
            };
            UIStyles.ApplyButtonStyle(_printBtn);
            _printBtn.Click += PrintBtn_Click;
            layout.Controls.Add(_printBtn);

            panel.Controls.Add(layout);
        }

        /// <summary>
        /// Initializes the maintenance history list view.
        /// This list displays all maintenance records in a grid format with sortable columns.
        /// </summary>
        /// <param name="panel">The panel to initialize with the maintenance list</param>
        private void InitializeMaintenanceList(Panel panel)
        {
            // Create and configure the list view
            _maintenanceList = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true
            };
            UIStyles.ApplyListViewStyle(_maintenanceList);

            // Add columns to list view for displaying maintenance information
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
        /// Loads and displays maintenance history based on current filters.
        /// This method retrieves records from the database, applies any active filters,
        /// and updates both the list view and statistics.
        /// </summary>
        private void LoadMaintenanceHistory()
        {
            try
            {
                // Get all maintenance records for the vehicle
                var records = _dbService.GetMaintenanceRecords(_vehicleId).ToList();

                // Apply type filter if selected
                if (_filterTypeComboBox.SelectedItem != null && _filterTypeComboBox.SelectedItem.ToString() != "All")
                {
                    records = records.Where(r => r.MaintenanceType == _filterTypeComboBox.SelectedItem.ToString()).ToList();
                }

                // Apply date range filter
                records = records.Where(r => r.MaintenanceDate >= _startDatePicker.Value && 
                                          r.MaintenanceDate <= _endDatePicker.Value).ToList();

                // Sort records by date (most recent first)
                records = records.OrderByDescending(r => r.MaintenanceDate).ToList();

                // Clear existing items
                _maintenanceList.Items.Clear();

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
            decimal totalCost = records.Sum(r => r.Cost);
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
        private async void ExportBtn_Click(object sender, EventArgs e)
        {
            try
            {
                var saveDialog = new SaveFileDialog
                {
                    Filter = "CSV Files (*.csv)|*.csv",
                    Title = "Export Maintenance History",
                    FileName = $"Maintenance_History_{_vehicle.Make}_{_vehicle.Model}_{DateTime.Now:yyyyMMdd}.csv"
                };

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    // Disable the export button and show progress
                    _exportBtn.Enabled = false;
                    _exportBtn.Text = "Exporting...";
                    Application.DoEvents();

                    await Task.Run(() =>
                    {
                        using (var writer = new System.IO.StreamWriter(saveDialog.FileName))
                        {
                            // Write header
                            writer.WriteLine("Date,Type,Description,Mileage,Cost,Provider,Notes");
                            
                            // Write data in chunks to prevent memory issues
                            const int chunkSize = 100;
                            for (int i = 0; i < _maintenanceList.Items.Count; i += chunkSize)
                            {
                                var chunk = _maintenanceList.Items.Cast<ListViewItem>()
                                    .Skip(i)
                                    .Take(chunkSize);
                                
                                foreach (var item in chunk)
                                {
                                    writer.WriteLine(string.Join(",", item.SubItems.Cast<ListViewItem.ListViewSubItem>()
                                        .Select(subItem => $"\"{subItem.Text.Replace("\"", "\"\"")}\"")));
                                }
                            }
                        }
                    });

                    MessageBox.Show("Maintenance history exported successfully!", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting maintenance history: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Re-enable the export button and restore its text
                _exportBtn.Enabled = true;
                _exportBtn.Text = "Export to CSV";
            }
        }

        /// <summary>
        /// Handles click event for Print History button
        /// </summary>
        private void PrintBtn_Click(object sender, EventArgs e)
        {
            try
            {
                _printRecordIndex = 0; // Reset before printing
                var printDocument = new System.Drawing.Printing.PrintDocument();
                printDocument.PrintPage += PrintDocument_PrintPage;

                var printDialog = new PrintDialog
                {
                    Document = printDocument,
                    AllowSomePages = true
                };

                if (printDialog.ShowDialog() == DialogResult.OK)
                {
                    printDocument.Print();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error printing maintenance history: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Handles the print page event for printing maintenance history
        /// </summary>
        private void PrintDocument_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            try
            {
                // Set up fonts
                var titleFont = new Font("Arial", 14, FontStyle.Bold);
                var headerFont = new Font("Arial", 10, FontStyle.Bold);
                var contentFont = new Font("Arial", 10);

                // Print title
                e.Graphics.DrawString($"Maintenance History - {_vehicle.Make} {_vehicle.Model}",
                    titleFont, Brushes.Black, 50, 50);

                // Print statistics
                e.Graphics.DrawString(_totalCostLabel.Text, contentFont, Brushes.Black, 50, 80);
                e.Graphics.DrawString(_lastServiceLabel.Text, contentFont, Brushes.Black, 50, 100);

                // Print column headers
                var y = 140;
                var x = 50;
                foreach (ColumnHeader column in _maintenanceList.Columns)
                {
                    e.Graphics.DrawString(column.Text, headerFont, Brushes.Black, x, y);
                    x += column.Width;
                }

                // Print records with pagination
                y = 160;
                int recordsPerPage = (int)((e.MarginBounds.Height - (y - e.MarginBounds.Top)) / 20);
                int recordCount = 0;
                for (; _printRecordIndex < _maintenanceList.Items.Count && recordCount < recordsPerPage; _printRecordIndex++, recordCount++)
                {
                    x = 50;
                    var item = _maintenanceList.Items[_printRecordIndex];
                    foreach (ListViewItem.ListViewSubItem subItem in item.SubItems)
                    {
                        e.Graphics.DrawString(subItem.Text, contentFont, Brushes.Black, x, y);
                        x += _maintenanceList.Columns[item.SubItems.IndexOf(subItem)].Width;
                    }
                    y += 20;
                }

                // If there are more records, indicate another page is needed
                e.HasMorePages = _printRecordIndex < _maintenanceList.Items.Count;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error printing page: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
