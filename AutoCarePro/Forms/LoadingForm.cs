using System;
using System.Windows.Forms;

namespace AutoCarePro.Forms
{
    /// <summary>
    /// A form that displays a loading indicator with a customizable message.
    /// Used to provide visual feedback during long-running operations.
    /// </summary>
    public partial class LoadingForm : Form
    {
        /// <summary>
        /// Initializes a new instance of the LoadingForm class.
        /// </summary>
        /// <param name="message">The message to display while loading. Defaults to "Please wait...".</param>
        public LoadingForm(string message = "Please wait...")
        {
            InitializeComponent();
            lblMessage.Text = message;
        }

        /// <summary>
        /// Updates the loading message displayed to the user.
        /// Thread-safe implementation that can be called from any thread.
        /// </summary>
        /// <param name="message">The new message to display.</param>
        public void UpdateMessage(string message)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(UpdateMessage), message);
                return;
            }
            lblMessage.Text = message;
        }
    }
} 