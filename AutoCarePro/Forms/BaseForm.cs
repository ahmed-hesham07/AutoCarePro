using System;
using System.Drawing;
using System.Windows.Forms;

namespace AutoCarePro.Forms
{
    /// <summary>
    /// Base form class that provides consistent styling and behavior for all forms in the application.
    /// </summary>
    public class BaseForm : Form
    {
        /// <summary>
        /// Initializes a new instance of the BaseForm class.
        /// </summary>
        public BaseForm()
        {
            // Set default form properties
            this.Font = new Font("Segoe UI", 9F);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimizeBox = true;
            this.MaximizeBox = true;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.BackColor = Color.White;
        }

        /// <summary>
        /// Shows a loading form while executing the specified action.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        /// <param name="loadingMessage">The message to display in the loading form.</param>
        protected void ShowLoading(Action action, string loadingMessage = "Please wait...")
        {
            using (var loadingForm = new LoadingForm(loadingMessage))
            {
                loadingForm.Show(this);
                try
                {
                    action();
                }
                finally
                {
                    loadingForm.Close();
                }
            }
        }

        /// <summary>
        /// Shows a loading form while executing the specified async action.
        /// </summary>
        /// <param name="action">The async action to execute.</param>
        /// <param name="loadingMessage">The message to display in the loading form.</param>
        protected async Task ShowLoadingAsync(Func<Task> action, string loadingMessage = "Please wait...")
        {
            using (var loadingForm = new LoadingForm(loadingMessage))
            {
                loadingForm.Show(this);
                try
                {
                    await action();
                }
                finally
                {
                    loadingForm.Close();
                }
            }
        }

        /// <summary>
        /// Shows an error message using the ErrorForm.
        /// </summary>
        /// <param name="message">The error message to display.</param>
        /// <param name="title">The title of the error form.</param>
        /// <param name="ex">Optional exception to include detailed error information.</param>
        protected void ShowError(string message, string title = "Error", Exception? ex = null)
        {
            new ErrorForm(message, title, ex).ShowDialog(this);
        }

        /// <summary>
        /// Shows a confirmation dialog.
        /// </summary>
        /// <param name="message">The confirmation message to display.</param>
        /// <param name="title">The title of the confirmation dialog.</param>
        /// <returns>True if the user confirmed, false otherwise.</returns>
        protected bool ShowConfirmation(string message, string title = "Confirm")
        {
            return MessageBox.Show(
                this,
                message,
                title,
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            ) == DialogResult.Yes;
        }

        /// <summary>
        /// Shows an information message.
        /// </summary>
        /// <param name="message">The message to display.</param>
        /// <param name="title">The title of the message box.</param>
        protected void ShowInfo(string message, string title = "Information")
        {
            MessageBox.Show(
                this,
                message,
                title,
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }
    }
} 