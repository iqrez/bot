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

            text = new TextBox
            {
                Multiline = true,
                Dock = DockStyle.Fill,
                ScrollBars = ScrollBars.Vertical,
                TabIndex = 0,
                AccessibleName = "Macro text",
                AccessibleDescription = "JSON describing macro actions"
            };
            Controls.Add(text);

            okBtn = new Button
            {
                Text = "&Close",
                Dock = DockStyle.Bottom,
                TabIndex = 1,
                AccessibleName = "Close",
                AccessibleDescription = "Close the editor"
            };
            okBtn.Click += (s, e) => Close();
            Controls.Add(okBtn);

            AcceptButton = okBtn;
        }
    }
}
