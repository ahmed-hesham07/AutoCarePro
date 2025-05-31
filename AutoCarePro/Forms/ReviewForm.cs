using System;
using System.Windows.Forms;
using System.Drawing;
using AutoCarePro.Models;
using AutoCarePro.Services;
using AutoCarePro.UI;

namespace AutoCarePro.Forms
{
    public partial class ReviewForm : BaseForm
    {
        private readonly DatabaseService _dbService;
        private readonly User _currentUser;
        private readonly User _serviceProvider;
        private readonly Review _review;
        private readonly bool _isEditMode;
        private NumericUpDown _ratingNumeric;
        private TextBox _commentTextBox;
        private Button _saveButton;
        private Button _cancelButton;

        public ReviewForm(User currentUser, User serviceProvider, Review? review = null)
        {
            InitializeComponent();
            _currentUser = currentUser;
            _serviceProvider = serviceProvider;
            _review = review ?? new Review 
            { 
                CustomerId = currentUser.Id,
                ServiceProviderId = serviceProvider.Id,
                Date = DateTime.Now
            };
            _isEditMode = review != null;
            _dbService = new DatabaseService();
            InitializeUI();
            if (_isEditMode)
            {
                LoadReviewData();
            }
        }

        private void InitializeUI()
        {
            this.Text = _isEditMode ? "Edit Review" : "Add Review";
            this.Size = new Size(500, 400);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Create form layout
            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                ColumnCount = 2,
                RowCount = 2
            };

            // Add controls
            mainPanel.Controls.Add(new Label { Text = "Rating (1-5):", AutoSize = true }, 0, 0);
            _ratingNumeric = new NumericUpDown
            {
                Width = 300,
                Minimum = 1,
                Maximum = 5,
                Value = 5
            };
            mainPanel.Controls.Add(_ratingNumeric, 1, 0);

            mainPanel.Controls.Add(new Label { Text = "Comment:", AutoSize = true }, 0, 1);
            _commentTextBox = new TextBox
            {
                Width = 300,
                Multiline = true,
                Height = 200
            };
            mainPanel.Controls.Add(_commentTextBox, 1, 1);

            // Add buttons
            var buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                FlowDirection = FlowDirection.RightToLeft,
                Height = 50,
                Padding = new Padding(0, 10, 0, 0)
            };

            _cancelButton = new Button
            {
                Text = "Cancel",
                DialogResult = DialogResult.Cancel,
                Width = 80
            };
            UIStyles.ApplyButtonStyle(_cancelButton);

            _saveButton = new Button
            {
                Text = "Save",
                Width = 80
            };
            UIStyles.ApplyButtonStyle(_saveButton, true);

            buttonPanel.Controls.Add(_cancelButton);
            buttonPanel.Controls.Add(_saveButton);

            // Add event handlers
            _saveButton.Click += SaveButton_Click;

            // Add panels to form
            this.Controls.Add(mainPanel);
            this.Controls.Add(buttonPanel);

            // Set accept and cancel buttons
            this.AcceptButton = _saveButton;
            this.CancelButton = _cancelButton;
        }

        private void LoadReviewData()
        {
            _ratingNumeric.Value = _review.Rating;
            _commentTextBox.Text = _review.Comment;
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            try
            {
                // Update review data
                _review.Rating = (int)_ratingNumeric.Value;
                _review.Comment = _commentTextBox.Text;
                _review.Date = DateTime.Now;

                // Validate review data
                var validationService = new UnifiedValidationService();
                var validationResult = validationService.ValidateReview(_review);

                if (!validationResult.IsValid)
                {
                    MessageBox.Show(
                        validationResult.GetErrorMessage(),
                        "Validation Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                // Save review
                if (_isEditMode)
                {
                    _dbService.UpdateReview(_review);
                }
                else
                {
                    _dbService.AddReview(_review);
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error saving review: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            _dbService.Dispose();
        }
    }
} 