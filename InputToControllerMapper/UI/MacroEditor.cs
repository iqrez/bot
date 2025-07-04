using System;
using System.Drawing;
using System.Windows.Forms;

namespace InputToControllerMapper.UI
{
    public class MacroEditor : Form
    {
        private readonly TextBox text;
        private readonly Button okBtn;

        public string MacrosJson => text.Text;

        public MacroEditor()
        {
            Text = "Macro Editor";
            Size = new Size(400, 300);

            text = new TextBox { Multiline = true, Dock = DockStyle.Fill, ScrollBars = ScrollBars.Vertical };
            Controls.Add(text);

            okBtn = new Button { Text = "Close", Dock = DockStyle.Bottom };
            okBtn.Click += (s, e) => Close();
            Controls.Add(okBtn);
        }
    }
}
