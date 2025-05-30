using System.Drawing;
using System.Windows.Forms;
using System.Threading.Tasks;
using System;

namespace AutoCarePro.UI
{
    public static class UIStyles
    {
        // Color Scheme
        public static class Colors
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

        // Allow dynamic accent color change
        public static void SetAccentColor(Color color)
        {
            Colors.Primary = color;
        }

        // Refresh all styled controls in a form (for accent color/theme changes)
        public static void RefreshStyles(Control control)
        {
            if (control is Button btn)
            {
                ApplyButtonStyle(btn, btn.BackColor == Colors.Primary);
            }
            else if (control is Label lbl)
            {
                ApplyLabelStyle(lbl);
            }
            else if (control is TextBox txt)
            {
                ApplyTextBoxStyle(txt);
            }
            else if (control is ComboBox cb)
            {
                ApplyComboBoxStyle(cb);
            }
            else if (control is ListView lv)
            {
                ApplyListViewStyle(lv);
            }
            else if (control is GroupBox gb)
            {
                ApplyGroupBoxStyle(gb);
            }
            else if (control is Panel pnl)
            {
                ApplyPanelStyle(pnl);
            }
            // Recursively refresh child controls
            foreach (Control child in control.Controls)
            {
                RefreshStyles(child);
            }
        }

        // Fonts
        public static class Fonts
        {
            public static Font Title = new Font("Segoe UI", 16, FontStyle.Bold);
            public static Font Subtitle = new Font("Segoe UI", 14, FontStyle.Bold);
            public static Font Normal = new Font("Segoe UI", 10);
            public static Font Small = new Font("Segoe UI", 9);
        }

        // Animation Methods
        public static async Task AnimatePanelTransition(Panel panel)
        {
            panel.Visible = true;
            panel.BringToFront();

            // Slide-in animation
            var originalLocation = panel.Location;
            panel.Location = new Point(originalLocation.X + 50, originalLocation.Y);
            panel.Visible = true;

            for (int i = 0; i < 10; i++)
            {
                panel.Location = new Point(
                    originalLocation.X + (50 * (10 - i) / 10),
                    originalLocation.Y
                );
                await Task.Delay(20);
            }

            panel.Location = originalLocation;
        }

        public static async Task AnimateButtonPress(Button button)
        {
            var originalSize = button.Size;
            var originalLocation = button.Location;
            const int pressDepth = 3; // Slightly increased for more noticeable effect
            const int pressDuration = 40; // Reduced for snappier feedback
            const int releaseDuration = 60; // Longer release for smoother return

            // Store original colors
            var originalBackColor = button.BackColor;
            var originalForeColor = button.ForeColor;

            // Press down animation
            button.Size = new Size(originalSize.Width - pressDepth, originalSize.Height - pressDepth);
            button.Location = new Point(originalLocation.X + pressDepth, originalLocation.Y + pressDepth);
            
            // Darken the button slightly when pressed
            button.BackColor = DarkenColor(originalBackColor, 0.1f);
            button.ForeColor = DarkenColor(originalForeColor, 0.1f);
            
            await Task.Delay(pressDuration);

            // Release animation with easing
            for (int i = 0; i <= 10; i++)
            {
                var progress = (double)i / 10;
                var easedProgress = EaseOutElastic(progress);
                
                button.Size = new Size(
                    (int)(originalSize.Width - pressDepth * (1 - easedProgress)),
                    (int)(originalSize.Height - pressDepth * (1 - easedProgress))
                );
                
                button.Location = new Point(
                    (int)(originalLocation.X + pressDepth * (1 - easedProgress)),
                    (int)(originalLocation.Y + pressDepth * (1 - easedProgress))
                );
                
                // Gradually restore original colors
                button.BackColor = InterpolateColor(DarkenColor(originalBackColor, 0.1f), originalBackColor, easedProgress);
                button.ForeColor = InterpolateColor(DarkenColor(originalForeColor, 0.1f), originalForeColor, easedProgress);
                
                await Task.Delay(releaseDuration / 10);
            }

            // Ensure final state is exactly as original
            button.Size = originalSize;
            button.Location = originalLocation;
            button.BackColor = originalBackColor;
            button.ForeColor = originalForeColor;
        }

        // Easing functions for smoother animations
        private static double EaseOutCubic(double x)
        {
            return 1 - Math.Pow(1 - x, 3);
        }

        private static double EaseOutElastic(double x)
        {
            const double c4 = (2 * Math.PI) / 3;
            return x == 0 ? 0 : x == 1 ? 1 : Math.Pow(2, -10 * x) * Math.Sin((x * 10 - 0.75) * c4) + 1;
        }

        // Color manipulation methods
        private static Color DarkenColor(Color color, float factor)
        {
            return Color.FromArgb(
                color.A,
                (int)(color.R * (1 - factor)),
                (int)(color.G * (1 - factor)),
                (int)(color.B * (1 - factor))
            );
        }

        private static Color InterpolateColor(Color color1, Color color2, double factor)
        {
            return Color.FromArgb(
                Clamp((int)(color1.A + (color2.A - color1.A) * factor)),
                Clamp((int)(color1.R + (color2.R - color1.R) * factor)),
                Clamp((int)(color1.G + (color2.G - color1.G) * factor)),
                Clamp((int)(color1.B + (color2.B - color1.B) * factor))
            );
        }

        private static int Clamp(int value)
        {
            return Math.Max(0, Math.Min(255, value));
        }

        // Button Styles
        public static void ApplyButtonStyle(Button button, bool isPrimary = false)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.BackColor = isPrimary ? Colors.Primary : Colors.Secondary;
            button.ForeColor = isPrimary ? Color.White : Colors.Text;
            button.Font = Fonts.Normal;
            button.Padding = new Padding(10, 5, 10, 5);
            button.Cursor = Cursors.Hand;

            // Add hover effect with smooth transition
            button.MouseEnter += async (s, e) => {
                var targetColor = isPrimary ? 
                    Color.FromArgb(0, 100, 195) : 
                    Color.FromArgb(230, 230, 230);
                
                // Smooth color transition
                for (int i = 0; i <= 10; i++)
                {
                    var progress = (double)i / 10;
                    button.BackColor = InterpolateColor(button.BackColor, targetColor, progress);
                    await Task.Delay(5);
                }
            };

            button.MouseLeave += async (s, e) => {
                var targetColor = isPrimary ? Colors.Primary : Colors.Secondary;
                
                // Smooth color transition
                for (int i = 0; i <= 10; i++)
                {
                    var progress = (double)i / 10;
                    button.BackColor = InterpolateColor(button.BackColor, targetColor, progress);
                    await Task.Delay(5);
                }
            };

            // Add press animation
            button.MouseDown += async (s, e) => {
                if (e.Button == MouseButtons.Left)
                {
                    await AnimateButtonPress(button);
                }
            };
        }

        // ListView Styles
        public static void ApplyListViewStyle(ListView listView)
        {
            listView.View = View.Details;
            listView.FullRowSelect = true;
            listView.GridLines = true;
            listView.Font = Fonts.Normal;
            listView.BackColor = Color.White;
            listView.BorderStyle = BorderStyle.FixedSingle;
            
            // Custom header style
            listView.DrawColumnHeader += (s, e) => {
                e.DrawDefault = true;
                e.Graphics.FillRectangle(new SolidBrush(Colors.Primary), e.Bounds);
                e.Graphics.DrawString(e.Header.Text, Fonts.Normal, Brushes.White, 
                    e.Bounds.X + 5, e.Bounds.Y + 5);
            };
        }

        // GroupBox Styles
        public static void ApplyGroupBoxStyle(GroupBox groupBox)
        {
            groupBox.Font = Fonts.Subtitle;
            groupBox.ForeColor = Colors.Primary;
            groupBox.Padding = new Padding(10);
        }

        // TextBox Styles
        public static void ApplyTextBoxStyle(TextBox textBox)
        {
            textBox.Font = Fonts.Normal;
            textBox.BorderStyle = BorderStyle.FixedSingle;
            textBox.BackColor = Color.White;
            textBox.Padding = new Padding(5);
        }

        // Label Styles
        public static void ApplyLabelStyle(Label label, bool isTitle = false)
        {
            label.Font = isTitle ? Fonts.Title : Fonts.Normal;
            label.ForeColor = Colors.Text;
        }

        // Form Styles
        public static void ApplyFormStyle(Form form)
        {
            form.BackColor = Colors.Background;
            form.Font = Fonts.Normal;
            form.Icon = SystemIcons.Application;
            form.StartPosition = FormStartPosition.CenterScreen;
        }

        // Panel Styles
        public static void ApplyPanelStyle(Panel panel, bool enableTransition = true)
        {
            panel.BackColor = Color.White;
            panel.Padding = new Padding(10);
            panel.BorderStyle = BorderStyle.FixedSingle;

            if (enableTransition)
            {
                panel.Visible = false;
                panel.Parent?.Controls.Add(panel);
                _ = AnimatePanelTransition(panel);
            }
        }

        // ComboBox Styles
        public static void ApplyComboBoxStyle(ComboBox comboBox)
        {
            comboBox.Font = Fonts.Normal;
            comboBox.FlatStyle = FlatStyle.Flat;
            comboBox.BackColor = Color.White;
            comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        }

        // CheckBox Styles
        public static void ApplyCheckBoxStyle(CheckBox checkBox)
        {
            checkBox.Font = Fonts.Normal;
            checkBox.ForeColor = Colors.Text;
        }

        // LinkLabel Styles
        public static void ApplyLinkLabelStyle(LinkLabel linkLabel)
        {
            linkLabel.Font = Fonts.Normal;
            linkLabel.LinkColor = Colors.Primary;
            linkLabel.ActiveLinkColor = Color.FromArgb(0, 100, 195);
        }
    }
} 