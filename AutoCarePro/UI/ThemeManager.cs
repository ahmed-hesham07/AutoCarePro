using System;
using System.Drawing;
using System.Windows.Forms;

namespace AutoCarePro.UI
{
    public class ThemeManager
    {
        private static ThemeManager _instance;
        public static ThemeManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new ThemeManager();
                return _instance;
            }
        }

        public event EventHandler ThemeChanged;

        public class Colors
        {
            public static Color Primary = Color.FromArgb(0, 120, 215);      // Modern Blue
            public static Color Secondary = Color.FromArgb(240, 240, 240);  // Light Gray
            public static Color Success = Color.FromArgb(40, 167, 69);      // Green
            public static Color Warning = Color.FromArgb(255, 193, 7);      // Yellow
            public static Color Danger = Color.FromArgb(220, 53, 69);       // Red
            public static Color Background = Color.FromArgb(248, 249, 250); // Light Background
            public static Color Text = Color.FromArgb(33, 37, 41);          // Dark Text
            public static Color Border = Color.FromArgb(206, 212, 218);     // Light Border
        }

        public class Fonts
        {
            public static Font Title = new Font("Segoe UI", 16, FontStyle.Bold);
            public static Font Subtitle = new Font("Segoe UI", 14, FontStyle.Bold);
            public static Font Normal = new Font("Segoe UI", 10);
            public static Font Small = new Font("Segoe UI", 9);
        }

        public class Sizes
        {
            public static Size Button = new Size(200, 35);
            public static Size TextBox = new Size(300, 25);
            public static Padding FormPadding = new Padding(20);
        }

        public void ApplyTheme(Form form)
        {
            form.BackColor = Colors.Background;
            form.Font = Fonts.Normal;

            foreach (Control control in form.Controls)
            {
                ApplyControlTheme(control);
            }
        }

        private void ApplyControlTheme(Control control)
        {
            if (control is Button button)
            {
                button.FlatStyle = FlatStyle.Flat;
                button.BackColor = Colors.Primary;
                button.ForeColor = Color.White;
                button.Font = Fonts.Normal;
            }
            else if (control is TextBox textBox)
            {
                textBox.Font = Fonts.Normal;
                textBox.BorderStyle = BorderStyle.FixedSingle;
            }
            else if (control is Label label)
            {
                label.Font = Fonts.Normal;
                label.ForeColor = Colors.Text;
            }
            else if (control is ComboBox comboBox)
            {
                comboBox.Font = Fonts.Normal;
                comboBox.FlatStyle = FlatStyle.Flat;
            }
            else if (control is ListView listView)
            {
                listView.Font = Fonts.Normal;
                listView.BackColor = Color.White;
                listView.BorderStyle = BorderStyle.FixedSingle;
            }
            else if (control is GroupBox groupBox)
            {
                groupBox.Font = Fonts.Subtitle;
                groupBox.ForeColor = Colors.Primary;
            }

            // Recursively apply theme to child controls
            foreach (Control childControl in control.Controls)
            {
                ApplyControlTheme(childControl);
            }
        }

        public void RaiseThemeChanged()
        {
            ThemeChanged?.Invoke(this, EventArgs.Empty);
        }
    }
} 