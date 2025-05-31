using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;
using AutoCarePro.Models;
using AutoCarePro.Services;
using System.Threading.Tasks;
using AutoCarePro.UI;
using System.IO;
using System.Data;
using ClosedXML.Excel;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Threading;
using Font = System.Drawing.Font;  // Explicitly use System.Drawing.Font

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
        private readonly User _currentUser;          // Current user viewing the form
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
        private System.Windows.Forms.Timer _fadeInTimer = new System.Windows.Forms.Timer();
        private double _fadeStep = 0.08;

        // Pagination state for printing
        private int _printRecordIndex = 0;

        private CancellationTokenSource _loadingCancellation;
        private ProgressDialog _progressDialog;

        /// <summary>
        /// Initializes the maintenance history form for a specific vehicle.
        /// This constructor is called when creating a new instance of the form.
        /// It sets up the database service, loads vehicle data, and initializes the UI.
        /// </summary>
        /// <param name="vehicleId">ID of the vehicle to show maintenance history for</param>
        /// <param name="currentUser">The current user viewing the form</param>
        public MaintenanceHistoryForm(int vehicleId, User currentUser)
        {
            InitializeComponent();
            _vehicleId = vehicleId;
            _currentUser = currentUser;
            _dbService = new DatabaseService();
            LoadVehicleData();
            InitializeForm();
            _ = LoadMaintenanceHistoryAsync();  // Call the async method
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

            // Add type filter
            _filterTypeComboBox = new ComboBox
            {
                Width = 150,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _filterTypeComboBox.Items.AddRange(new string[] { "All", "Regular Maintenance", "Oil Change", "Tire Rotation", "Brake Service", "Engine Repair", "Transmission Service", "Electrical System", "Air Conditioning", "Other" });
            _filterTypeComboBox.SelectedIndex = 0;
            _filterTypeComboBox.SelectedIndexChanged += (s, e) => _ = LoadMaintenanceHistoryAsync();
            layout.Controls.Add(_filterTypeComboBox);

            // Add date range filters
            _startDatePicker = new DateTimePicker
            {
                Width = 150,
                Format = DateTimePickerFormat.Short
            };
            _startDatePicker.ValueChanged += (s, e) => _ = LoadMaintenanceHistoryAsync();
            layout.Controls.Add(_startDatePicker);

            _endDatePicker = new DateTimePicker
            {
                Width = 150,
                Format = DateTimePickerFormat.Short
            };
            _endDatePicker.ValueChanged += (s, e) => _ = LoadMaintenanceHistoryAsync();
            layout.Controls.Add(_endDatePicker);

            // Add upcoming visits button
            var upcomingVisitsBtn = new Button
            {
                Text = "View Upcoming Visits",
                Width = 150
            };
            UIStyles.ApplyButtonStyle(upcomingVisitsBtn);
            upcomingVisitsBtn.Click += (s, e) =>
            {
                var upcomingVisitsForm = new UpcomingVisitsForm(_vehicleId, _currentUser);
                upcomingVisitsForm.ShowDialog();
            };
            layout.Controls.Add(upcomingVisitsBtn);

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
        private async Task LoadMaintenanceHistoryAsync()
        {
            try
            {
                // Cancel any existing loading operation
                _loadingCancellation?.Cancel();
                _loadingCancellation = new CancellationTokenSource();

                // Show progress dialog
                using (_progressDialog = new ProgressDialog("Loading Maintenance History", "Please wait..."))
                {
                    _progressDialog.Show();

                    // Get all maintenance records for the vehicle
                    var records = await Task.Run(() => _dbService.GetMaintenanceRecords(_vehicleId).ToList(), 
                        _loadingCancellation.Token);

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

                    // Add records to list view in chunks
                    const int chunkSize = 100;
                    for (int i = 0; i < records.Count; i += chunkSize)
                    {
                        var chunk = records.Skip(i).Take(chunkSize);
                        foreach (var record in chunk)
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

                        // Update progress
                        _progressDialog.UpdateProgress((i + chunkSize) * 100 / records.Count);
                    }

                    // Update statistics
                    UpdateStatistics(records);
                }
            }
            catch (OperationCanceledException)
            {
                // Loading was cancelled, do nothing
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading maintenance history: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _progressDialog?.Close();
                _progressDialog = null;
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
        private async void ApplyFilterBtn_Click(object sender, EventArgs e)
        {
            await LoadMaintenanceHistoryAsync();
        }

        /// <summary>
        /// Handles click event for Export button
        /// </summary>
        private async void ExportBtn_Click(object sender, EventArgs e)
        {
            try
            {
                var saveDialog = new SaveFileDialog
                {
                    Filter = "Excel Files (*.xlsx)|*.xlsx|PDF Files (*.pdf)|*.pdf|CSV Files (*.csv)|*.csv",
                    Title = "Export Maintenance History",
                    FileName = $"Maintenance_History_{_vehicle.Make}_{_vehicle.Model}_{DateTime.Now:yyyyMMdd}"
                };

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    // Disable the export button and show progress
                    _exportBtn.Enabled = false;
                    _exportBtn.Text = "Exporting...";

                    using (_progressDialog = new ProgressDialog("Exporting Maintenance History", "Please wait..."))
                    {
                        _progressDialog.Show();

                        switch (Path.GetExtension(saveDialog.FileName).ToLower())
                        {
                            case ".xlsx":
                                await ExportToExcelAsync(saveDialog.FileName);
                                break;
                            case ".pdf":
                                await ExportToPdfAsync(saveDialog.FileName);
                                break;
                            case ".csv":
                                await ExportToCsvAsync(saveDialog.FileName);
                                break;
                        }

                        MessageBox.Show("Maintenance history exported successfully!", "Success",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting maintenance history: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _exportBtn.Enabled = true;
                _exportBtn.Text = "Export";
                _progressDialog?.Close();
                _progressDialog = null;
            }
        }

        private async Task ExportToExcelAsync(string filePath)
        {
            await Task.Run(() =>
            {
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Maintenance History");

                    // Add title
                    worksheet.Cell(1, 1).Value = $"Maintenance History - {_vehicle.Make} {_vehicle.Model}";
                    worksheet.Cell(1, 1).Style.Font.Bold = true;
                    worksheet.Cell(1, 1).Style.Font.FontSize = 14;

                    // Add statistics
                    worksheet.Cell(2, 1).Value = _totalCostLabel.Text;
                    worksheet.Cell(3, 1).Value = _lastServiceLabel.Text;

                    // Add headers
                    for (int i = 0; i < _maintenanceList.Columns.Count; i++)
                    {
                        worksheet.Cell(5, i + 1).Value = _maintenanceList.Columns[i].Text;
                        worksheet.Cell(5, i + 1).Style.Font.Bold = true;
                    }

                    // Add data in chunks with progress updates
                    const int chunkSize = 100;
                    int totalItems = _maintenanceList.Items.Count;
                    
                    for (int i = 0; i < totalItems; i += chunkSize)
                    {
                        var chunk = _maintenanceList.Items.Cast<ListViewItem>()
                            .Skip(i)
                            .Take(chunkSize);

                        foreach (var item in chunk)
                        {
                            for (int j = 0; j < item.SubItems.Count; j++)
                            {
                                worksheet.Cell(i + 6, j + 1).Value = item.SubItems[j].Text;
                            }
                        }

                        // Update progress
                        int progress = (i + chunkSize) * 100 / totalItems;
                        _progressDialog?.UpdateProgress(Math.Min(progress, 100));
                        _progressDialog?.UpdateStatus($"Exporting data... {progress}%");
                    }

                    // Auto-fit columns
                    worksheet.Columns().AdjustToContents();

                    // Save the workbook
                    _progressDialog?.UpdateStatus("Saving file...");
                    workbook.SaveAs(filePath);
                }
            });
        }

        private async Task ExportToPdfAsync(string filePath)
        {
            await Task.Run(() =>
            {
                using (var document = new Document(PageSize.A4.Rotate()))
                {
                    PdfWriter.GetInstance(document, new FileStream(filePath, FileMode.Create));
                    document.Open();

                    // Add title
                    var titleFont = iTextSharp.text.FontFactory.GetFont(iTextSharp.text.FontFactory.HELVETICA_BOLD, 14);
                    var title = new Paragraph($"Maintenance History - {_vehicle.Make} {_vehicle.Model}", titleFont);
                    title.Alignment = Element.ALIGN_CENTER;
                    document.Add(title);
                    document.Add(new Paragraph("\n"));

                    // Add statistics
                    var normalFont = iTextSharp.text.FontFactory.GetFont(iTextSharp.text.FontFactory.HELVETICA, 10);
                    document.Add(new Paragraph(_totalCostLabel.Text, normalFont));
                    document.Add(new Paragraph(_lastServiceLabel.Text, normalFont));
                    document.Add(new Paragraph("\n"));

                    // Create table
                    var table = new PdfPTable(_maintenanceList.Columns.Count);
                    table.WidthPercentage = 100;

                    // Add headers
                    var headerFont = iTextSharp.text.FontFactory.GetFont(iTextSharp.text.FontFactory.HELVETICA_BOLD, 10);
                    foreach (ColumnHeader column in _maintenanceList.Columns)
                    {
                        table.AddCell(new PdfPCell(new Phrase(column.Text, headerFont)));
                    }

                    // Add data in chunks with progress updates
                    const int chunkSize = 100;
                    int totalItems = _maintenanceList.Items.Count;

                    for (int i = 0; i < totalItems; i += chunkSize)
                    {
                        var chunk = _maintenanceList.Items.Cast<ListViewItem>()
                            .Skip(i)
                            .Take(chunkSize);

                        foreach (var item in chunk)
                        {
                            foreach (ListViewItem.ListViewSubItem subItem in item.SubItems)
                            {
                                table.AddCell(new PdfPCell(new Phrase(subItem.Text, normalFont)));
                            }
                        }

                        // Update progress
                        int progress = (i + chunkSize) * 100 / totalItems;
                        _progressDialog?.UpdateProgress(Math.Min(progress, 100));
                        _progressDialog?.UpdateStatus($"Exporting data... {progress}%");
                    }

                    _progressDialog?.UpdateStatus("Saving file...");
                    document.Add(table);
                    document.Close();
                }
            });
        }

        private async Task ExportToCsvAsync(string filePath)
        {
            await Task.Run(() =>
            {
                using (var writer = new StreamWriter(filePath))
                {
                    // Write header
                    writer.WriteLine(string.Join(",", _maintenanceList.Columns.Cast<ColumnHeader>()
                        .Select(c => $"\"{c.Text}\"")));

                    // Write data in chunks with progress updates
                    const int chunkSize = 100;
                    int totalItems = _maintenanceList.Items.Count;

                    for (int i = 0; i < totalItems; i += chunkSize)
                    {
                        var chunk = _maintenanceList.Items.Cast<ListViewItem>()
                            .Skip(i)
                            .Take(chunkSize);

                        foreach (var item in chunk)
                        {
                            writer.WriteLine(string.Join(",", item.SubItems.Cast<ListViewItem.ListViewSubItem>()
                                .Select(subItem => $"\"{subItem.Text.Replace("\"", "\"\"")}\"")));
                        }

                        // Update progress
                        int progress = (i + chunkSize) * 100 / totalItems;
                        _progressDialog?.UpdateProgress(Math.Min(progress, 100));
                        _progressDialog?.UpdateStatus($"Exporting data... {progress}%");
                    }
                }
            });
        }

        /// <summary>
        /// Handles click event for Print History button
        /// </summary>
        private async void PrintBtn_Click(object sender, EventArgs e)
        {
            try
            {
                _printRecordIndex = 0; // Reset before printing
                var printDocument = new System.Drawing.Printing.PrintDocument();
                printDocument.PrintPage += PrintDocument_PrintPage;

                using (_progressDialog = new ProgressDialog("Preparing Print Preview", "Please wait..."))
                {
                    _progressDialog.Show();

                    // Calculate total pages in background
                    int totalPages = await Task.Run(() =>
                    {
                        int recordsPerPage = 30; // Approximate number of records per page
                        return (int)Math.Ceiling(_maintenanceList.Items.Count / (double)recordsPerPage);
                    });

                    using (var previewDialog = new PrintPreviewDialog
                    {
                        Document = printDocument,
                        Width = 1000,
                        Height = 800,
                        StartPosition = FormStartPosition.CenterParent
                    })
                    {
                        _progressDialog.Close();
                        previewDialog.ShowDialog();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error printing maintenance history: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _progressDialog?.Close();
                _progressDialog = null;
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

                // Calculate progress
                int totalPages = (int)Math.Ceiling(_maintenanceList.Items.Count / (double)recordsPerPage);
                int currentPage = _printRecordIndex / recordsPerPage + 1;
                _progressDialog?.UpdateStatus($"Printing page {currentPage} of {totalPages}");

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

    /// <summary>
    /// A reusable progress dialog form
    /// </summary>
    public class ProgressDialog : Form
    {
        private ProgressBar _progressBar;
        private Label _statusLabel;

        public ProgressDialog(string title, string initialStatus)
        {
            this.Text = title;
            this.Size = new Size(400, 150);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MinimizeBox = false;
            this.MaximizeBox = false;

            _progressBar = new ProgressBar
            {
                Dock = DockStyle.Bottom,
                Height = 23,
                Style = ProgressBarStyle.Marquee
            };

            _statusLabel = new Label
            {
                Dock = DockStyle.Fill,
                Text = initialStatus,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 10)
            };

            this.Controls.Add(_statusLabel);
            this.Controls.Add(_progressBar);
        }

        public void UpdateProgress(int percentage)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<int>(UpdateProgress), percentage);
                return;
            }

            _progressBar.Style = ProgressBarStyle.Continuous;
            _progressBar.Value = percentage;
        }

        public void UpdateStatus(string status)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(UpdateStatus), status);
                return;
            }

            _statusLabel.Text = status;
        }
    }
}
