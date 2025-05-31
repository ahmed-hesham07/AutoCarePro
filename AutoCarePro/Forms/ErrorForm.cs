using System;
using System.Windows.Forms;

namespace AutoCarePro.Forms
{
    /// <summary>
    /// A form that displays error messages to the user in a user-friendly format.
    /// Supports showing detailed error information including stack traces.
    /// </summary>
    public partial class ErrorForm : Form
    {
        /// <summary>
        /// Initializes a new instance of the ErrorForm class.
        /// </summary>
        /// <param name="message">The error message to display.</param>
        /// <param name="title">The title of the error form. Defaults to "Error".</param>
        /// <param name="ex">Optional exception to include detailed error information.</param>
        public ErrorForm(string message, string title = "Error", Exception? ex = null)
        {
            InitializeComponent();
            this.Text = title;
            lblMessage.Text = message;

            if (ex != null)
            {
                txtDetails.Text = $"Exception: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}";
                if (ex.InnerException != null)
                {
                    txtDetails.Text += $"\n\nInner Exception:\n{ex.InnerException.Message}";
                }
            }
            else
            {
                txtDetails.Visible = false;
                btnDetails.Visible = false;
            }
        }

        /// <summary>
        /// Handles the OK button click event by closing the form.
        /// </summary>
        private void btnOK_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Handles the Details button click event by toggling the visibility of detailed error information.
        /// </summary>
        private void btnDetails_Click(object sender, EventArgs e)
        {
            txtDetails.Visible = !txtDetails.Visible;
            this.Height = txtDetails.Visible ? 400 : 150;
        }
    }
} 