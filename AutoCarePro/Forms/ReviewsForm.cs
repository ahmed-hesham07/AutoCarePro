using System;
using System.Windows.Forms;
using System.Drawing;
using System.Linq;
using AutoCarePro.Models;
using AutoCarePro.Services;
using AutoCarePro.UI;

namespace AutoCarePro.Forms
{
    public partial class ReviewsForm : BaseForm
    {
        private readonly DatabaseService _dbService;
        private readonly User _currentUser;
        private readonly User _serviceProvider;
        private DataGridView _reviewsGrid;
        private Button _addReviewButton;
        private Button _editReviewButton;
        private Button _deleteReviewButton;
        private ComboBox _ratingFilterComboBox;
        private TextBox _searchTextBox;
        private Button _applyFiltersButton;
        private Button _clearFiltersButton;

        public ReviewsForm(User currentUser, User serviceProvider)
        {
            InitializeComponent();
            _currentUser = currentUser;
            _serviceProvider = serviceProvider;
            _dbService = new DatabaseService();
            InitializeUI();
            LoadReviews();
        }

        private void InitializeUI()
        {
            this.Text = "Reviews";
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

            // Rating filter
            filterPanel.Controls.Add(new Label { Text = "Rating:", Location = new Point(10, 15) });
            _ratingFilterComboBox = new ComboBox
            {
                Location = new Point(70, 12),
                Width = 150,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            filterPanel.Controls.Add(_ratingFilterComboBox);

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
            _addReviewButton = new Button
            {
                Text = "Add Review",
                Location = new Point(10, 10),
                Size = new Size(100, 30)
            };
            UIStyles.ApplyButtonStyle(_addReviewButton);

            _editReviewButton = new Button
            {
                Text = "Edit Review",
                Location = new Point(120, 10),
                Size = new Size(100, 30),
                Enabled = false
            };
            UIStyles.ApplyButtonStyle(_editReviewButton);

            _deleteReviewButton = new Button
            {
                Text = "Delete Review",
                Location = new Point(230, 10),
                Size = new Size(100, 30),
                Enabled = false
            };
            UIStyles.ApplyButtonStyle(_deleteReviewButton);

            // Add buttons to toolbar
            toolbarPanel.Controls.AddRange(new Control[] { 
                _addReviewButton, _editReviewButton, _deleteReviewButton 
            });

            // Create reviews grid
            _reviewsGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            UIStyles.ApplyDataGridViewStyle(_reviewsGrid);

            // Add event handlers
            _addReviewButton.Click += AddReviewButton_Click;
            _editReviewButton.Click += EditReviewButton_Click;
            _deleteReviewButton.Click += DeleteReviewButton_Click;
            _reviewsGrid.SelectionChanged += ReviewsGrid_SelectionChanged;
            _applyFiltersButton.Click += ApplyFiltersButton_Click;
            _clearFiltersButton.Click += ClearFiltersButton_Click;
            _searchTextBox.TextChanged += SearchTextBox_TextChanged;

            // Add controls to form
            this.Controls.Add(_reviewsGrid);
            this.Controls.Add(filterPanel);
            this.Controls.Add(toolbarPanel);

            // Initialize filters
            InitializeFilters();
        }

        private void InitializeFilters()
        {
            // Load rating filter options
            _ratingFilterComboBox.Items.Add("All Ratings");
            for (int i = 5; i >= 1; i--)
            {
                _ratingFilterComboBox.Items.Add($"{i} Stars");
            }
            _ratingFilterComboBox.SelectedIndex = 0;
        }

        private async void LoadReviews()
        {
            var reviews = await _dbService.GetReviewsByServiceProviderAsync(_serviceProvider.Id);
            var reviewsList = reviews.ToList();
            
            // Apply filters
            if (_ratingFilterComboBox.SelectedIndex > 0)
            {
                var selectedRating = _ratingFilterComboBox.SelectedIndex;
                reviewsList = reviewsList.Where(r => r.Rating == selectedRating).ToList();
            }

            if (!string.IsNullOrWhiteSpace(_searchTextBox.Text))
            {
                var searchText = _searchTextBox.Text.ToLower();
                reviewsList = reviewsList.Where(r => 
                    r.Comment.ToLower().Contains(searchText) ||
                    r.CustomerName.ToLower().Contains(searchText)).ToList();
            }

            _reviewsGrid.DataSource = reviewsList.Select(r => new
            {
                r.Id,
                Customer = r.CustomerName,
                r.Rating,
                r.Comment,
                r.Date
            }).ToList();
        }

        private void ReviewsGrid_SelectionChanged(object sender, EventArgs e)
        {
            bool hasSelection = _reviewsGrid.SelectedRows.Count > 0;
            _editReviewButton.Enabled = hasSelection;
            _deleteReviewButton.Enabled = hasSelection;
        }

        private void AddReviewButton_Click(object sender, EventArgs e)
        {
            using (var form = new ReviewForm(_currentUser, _serviceProvider))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadReviews();
                }
            }
        }

        private void EditReviewButton_Click(object sender, EventArgs e)
        {
            if (_reviewsGrid.SelectedRows.Count > 0)
            {
                var reviewId = (int)_reviewsGrid.SelectedRows[0].Cells["Id"].Value;
                var review = _dbService.GetReviewById(reviewId);
                
                if (review != null)
                {
                    using (var form = new ReviewForm(_currentUser, _serviceProvider, review))
                    {
                        if (form.ShowDialog() == DialogResult.OK)
                        {
                            LoadReviews();
                        }
                    }
                }
            }
        }

        private void DeleteReviewButton_Click(object sender, EventArgs e)
        {
            if (_reviewsGrid.SelectedRows.Count > 0)
            {
                var reviewId = (int)_reviewsGrid.SelectedRows[0].Cells["Id"].Value;
                var review = _dbService.GetReviewById(reviewId);

                if (review != null)
                {
                    var result = MessageBox.Show(
                        $"Are you sure you want to delete this review?\n\n" +
                        $"Rating: {review.Rating} Stars\n" +
                        $"Comment: {review.Comment}",
                        "Confirm Delete",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);

                    if (result == DialogResult.Yes)
                    {
                        try
                        {
                            _dbService.DeleteReview(reviewId);
                            LoadReviews();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(
                                $"Error deleting review: {ex.Message}",
                                "Error",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                        }
                    }
                }
            }
        }

        private void ApplyFiltersButton_Click(object sender, EventArgs e)
        {
            LoadReviews();
        }

        private void ClearFiltersButton_Click(object sender, EventArgs e)
        {
            _ratingFilterComboBox.SelectedIndex = 0;
            _searchTextBox.Clear();
            LoadReviews();
        }

        private void SearchTextBox_TextChanged(object sender, EventArgs e)
        {
            LoadReviews();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            _dbService.Dispose();
        }
    }
} 