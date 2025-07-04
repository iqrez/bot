using System;
using System.Drawing;
using System.Windows.Forms;

namespace InputToControllerMapper
{
    public enum Theme
    {
        Light,
        Dark
    }

    public static class ThemeManager
    {
        public static Theme CurrentTheme { get; set; } = Theme.Light;

        public static void ApplyTheme(Control root)
        {
            Color back;
            Color fore;

            if (CurrentTheme == Theme.Dark)
            {
                back = Color.FromArgb(30, 30, 30);
                fore = Color.White;
            }
            else
            {
                back = SystemColors.Window;
                fore = SystemColors.ControlText;
            }

            ApplyRecursive(root, back, fore);
        }

        private static void ApplyRecursive(Control control, Color back, Color fore)
        {
            control.BackColor = back;
            control.ForeColor = fore;
            foreach (Control child in control.Controls)
            {
                ApplyRecursive(child, back, fore);
            }
        }
    }
}
