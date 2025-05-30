using System;
using System.Drawing;
using System.Windows.Forms;

namespace AutoCarePro.Services
{
    /// <summary>
    /// ThemeManager class implements the Singleton pattern to manage application themes.
    /// It provides functionality to switch between light and dark themes, and applies
    /// consistent styling to Windows Forms controls.
    /// </summary>
    public class ThemeManager
    {
        // Singleton instance
        private static ThemeManager _instance;
        // Current theme state
        private bool _isDarkMode;
        private Color _accentColor = Color.FromArgb(0, 120, 215);

        /// <summary>
        /// Gets the singleton instance of ThemeManager
        /// </summary>
        public static ThemeManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ThemeManager();
                }
                return _instance;
            }
        }

        /// <summary>
        /// Private constructor to enforce singleton pattern
        /// </summary>
        private ThemeManager()
        {
            _isDarkMode = false;
        }

        /// <summary>
        /// Gets or sets the dark mode state. When changed, triggers the ThemeChanged event.
        /// </summary>
        public bool IsDarkMode
        {
            get => _isDarkMode;
            set
            {
                if (_isDarkMode != value)
                {
                    _isDarkMode = value;
                    OnThemeChanged();
                }
            }
        }

        /// <summary>
        /// Event that is raised when the theme changes
        /// </summary>
        public event EventHandler ThemeChanged;

        /// <summary>
        /// Raises the ThemeChanged event
        /// </summary>
        private void OnThemeChanged()
        {
            ThemeChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Applies the current theme to a form and all its controls
        /// </summary>
        /// <param name="form">The form to apply the theme to</param>
        public void ApplyTheme(Form form)
        {
            if (_isDarkMode)
            {
                // Apply dark theme colors to form
                form.BackColor = Color.FromArgb(32, 32, 32);
                form.ForeColor = Color.White;

                // Apply dark theme to all controls
                foreach (Control control in form.Controls)
                {
                    ApplyDarkThemeToControl(control);
                }
            }
            else
            {
                // Apply light theme colors to form
                form.BackColor = SystemColors.Control;
                form.ForeColor = SystemColors.ControlText;

                // Apply light theme to all controls
                foreach (Control control in form.Controls)
                {
                    ApplyLightThemeToControl(control);
                }
            }
        }

        /// <summary>
        /// Applies dark theme styling to a specific control and its child controls
        /// </summary>
        /// <param name="control">The control to apply dark theme to</param>
        private void ApplyDarkThemeToControl(Control control)
        {
            if (control is TextBox textBox)
            {
                // Style text boxes with dark theme colors
                textBox.BackColor = Color.FromArgb(45, 45, 48);
                textBox.ForeColor = Color.White;
                textBox.BorderStyle = BorderStyle.FixedSingle;
            }
            else if (control is Button button)
            {
                // Style buttons based on their flat style
                if (button.FlatStyle == FlatStyle.Flat)
                {
                    button.BackColor = Color.FromArgb(0, 120, 215);
                    button.ForeColor = Color.White;
                }
                else
                {
                    button.BackColor = Color.FromArgb(45, 45, 48);
                    button.ForeColor = Color.White;
                }
            }
            else if (control is Label label)
            {
                // Style labels with white text
                label.ForeColor = Color.White;
            }
            else if (control is ComboBox comboBox)
            {
                // Style combo boxes with dark theme colors
                comboBox.BackColor = Color.FromArgb(45, 45, 48);
                comboBox.ForeColor = Color.White;
            }
            else if (control is DataGridView grid)
            {
                // Style data grid views with dark theme colors
                grid.BackgroundColor = Color.FromArgb(32, 32, 32);
                grid.DefaultCellStyle.BackColor = Color.FromArgb(45, 45, 48);
                grid.DefaultCellStyle.ForeColor = Color.White;
                grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(45, 45, 48);
                grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
                grid.RowHeadersDefaultCellStyle.BackColor = Color.FromArgb(45, 45, 48);
                grid.RowHeadersDefaultCellStyle.ForeColor = Color.White;
            }
            else if (control is Panel panel)
            {
                // Style panels and recursively style their child controls
                panel.BackColor = Color.FromArgb(32, 32, 32);
                foreach (Control childControl in panel.Controls)
                {
                    ApplyDarkThemeToControl(childControl);
                }
            }
            else if (control is GroupBox groupBox)
            {
                // Style group boxes and recursively style their child controls
                groupBox.ForeColor = Color.White;
                foreach (Control childControl in groupBox.Controls)
                {
                    ApplyDarkThemeToControl(childControl);
                }
            }
        }

        /// <summary>
        /// Applies light theme styling to a specific control and its child controls
        /// </summary>
        /// <param name="control">The control to apply light theme to</param>
        private void ApplyLightThemeToControl(Control control)
        {
            if (control is TextBox textBox)
            {
                // Style text boxes with system colors
                textBox.BackColor = SystemColors.Window;
                textBox.ForeColor = SystemColors.WindowText;
                textBox.BorderStyle = BorderStyle.FixedSingle;
            }
            else if (control is Button button)
            {
                // Style buttons based on their flat style
                if (button.FlatStyle == FlatStyle.Flat)
                {
                    button.BackColor = Color.FromArgb(0, 120, 215);
                    button.ForeColor = Color.White;
                }
                else
                {
                    button.BackColor = SystemColors.Control;
                    button.ForeColor = SystemColors.ControlText;
                }
            }
            else if (control is Label label)
            {
                // Style labels with system colors
                label.ForeColor = SystemColors.ControlText;
            }
            else if (control is ComboBox comboBox)
            {
                // Style combo boxes with system colors
                comboBox.BackColor = SystemColors.Window;
                comboBox.ForeColor = SystemColors.WindowText;
            }
            else if (control is DataGridView grid)
            {
                // Style data grid views with system colors
                grid.BackgroundColor = SystemColors.Window;
                grid.DefaultCellStyle.BackColor = SystemColors.Window;
                grid.DefaultCellStyle.ForeColor = SystemColors.WindowText;
                grid.ColumnHeadersDefaultCellStyle.BackColor = SystemColors.Control;
                grid.ColumnHeadersDefaultCellStyle.ForeColor = SystemColors.ControlText;
                grid.RowHeadersDefaultCellStyle.BackColor = SystemColors.Control;
                grid.RowHeadersDefaultCellStyle.ForeColor = SystemColors.ControlText;
            }
            else if (control is Panel panel)
            {
                // Style panels and recursively style their child controls
                panel.BackColor = SystemColors.Control;
                foreach (Control childControl in panel.Controls)
                {
                    ApplyLightThemeToControl(childControl);
                }
            }
            else if (control is GroupBox groupBox)
            {
                // Style group boxes and recursively style their child controls
                groupBox.ForeColor = SystemColors.ControlText;
                foreach (Control childControl in groupBox.Controls)
                {
                    ApplyLightThemeToControl(childControl);
                }
            }
        }

        /// <summary>
        /// Static class containing predefined colors for consistent theming
        /// </summary>
        public static class Colors
        {
            public static Color Primary => Color.FromArgb(0, 120, 215);    // Blue
            public static Color Secondary => Color.FromArgb(240, 240, 240); // Light Gray
            public static Color Success => Color.FromArgb(40, 167, 69);    // Green
            public static Color Warning => Color.FromArgb(255, 193, 7);    // Yellow
            public static Color Danger => Color.FromArgb(220, 53, 69);     // Red
            public static Color Info => Color.FromArgb(23, 162, 184);      // Light Blue
        }

        /// <summary>
        /// Static class containing predefined fonts for consistent typography
        /// </summary>
        public static class Fonts
        {
            public static Font Title => new Font("Segoe UI", 16, FontStyle.Bold);
            public static Font Subtitle => new Font("Segoe UI", 14, FontStyle.Regular);
            public static Font Normal => new Font("Segoe UI", 10, FontStyle.Regular);
            public static Font Small => new Font("Segoe UI", 8, FontStyle.Regular);
        }

        /// <summary>
        /// Static class containing predefined sizes and spacing for consistent layout
        /// </summary>
        public static class Sizes
        {
            public static Size Button => new Size(400, 35);
            public static Size TextBox => new Size(400, 25);
            public static Padding FormPadding => new Padding(50);
            public static int ControlSpacing => 20;
        }

        public Color AccentColor
        {
            get => _accentColor;
            set
            {
                if (_accentColor != value)
                {
                    _accentColor = value;
                    AutoCarePro.UI.UIStyles.SetAccentColor(value);
                    OnThemeChanged();
                }
            }
        }

        // Set accent color and refresh all open forms
        public void SetAccentColor(Color color)
        {
            AccentColor = color;
            foreach (Form form in Application.OpenForms)
            {
                ApplyTheme(form);
                AutoCarePro.UI.UIStyles.RefreshStyles(form);
            }
        }
    }
} 