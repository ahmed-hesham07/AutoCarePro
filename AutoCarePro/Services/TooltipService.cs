using System.Windows.Forms;

namespace AutoCarePro.Services
{
    /// <summary>
    /// Provides centralized tooltip management for the application.
    /// Ensures consistent tooltip behavior and styling across all forms.
    /// </summary>
    public static class TooltipService
    {
        private static readonly ToolTip _tooltip = new ToolTip();

        static TooltipService()
        {
            // Configure default tooltip settings
            _tooltip.AutoPopDelay = 5000;
            _tooltip.InitialDelay = 500;
            _tooltip.ReshowDelay = 500;
            _tooltip.ShowAlways = true;
            _tooltip.ToolTipTitle = "Help";
            _tooltip.UseAnimation = true;
            _tooltip.UseFading = true;
        }

        /// <summary>
        /// Sets a tooltip for a control with the specified message.
        /// </summary>
        /// <param name="control">The control to add the tooltip to.</param>
        /// <param name="message">The tooltip message to display.</param>
        public static void SetTooltip(Control control, string message)
        {
            _tooltip.SetToolTip(control, message);
        }

        /// <summary>
        /// Sets a tooltip for a control with a title and message.
        /// </summary>
        /// <param name="control">The control to add the tooltip to.</param>
        /// <param name="title">The tooltip title.</param>
        /// <param name="message">The tooltip message to display.</param>
        public static void SetTooltip(Control control, string title, string message)
        {
            _tooltip.ToolTipTitle = title;
            _tooltip.SetToolTip(control, message);
        }

        /// <summary>
        /// Removes the tooltip from a control.
        /// </summary>
        /// <param name="control">The control to remove the tooltip from.</param>
        public static void RemoveTooltip(Control control)
        {
            _tooltip.SetToolTip(control, string.Empty);
        }
    }
} 