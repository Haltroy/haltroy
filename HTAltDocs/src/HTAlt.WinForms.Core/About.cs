/*

Copyright © 2018 - 2021 haltroy

Use of this source code is governed by an MIT License that can be found in github.com/Haltroy/HTAlt/blob/master/LICENSE

*/

using System;
using System.Drawing;
using System.Windows.Forms;

namespace HTAlt.WinForms
{
    public static class UI
    {
        /// <summary>
        /// Shows information about this library.
        /// </summary>
        public static void ShowAbout()
        {
            About().Show();
        }

        /// <summary>
        /// Form that shows information about this library.
        /// </summary>
        /// <returns><see cref="About"/></returns>
        public static Form About()
        {
            return new About();
        }
    }

    internal class About : Form
    {
        public About()
        {
            SuspendLayout();
            //
            // About
            //
            Size = new System.Drawing.Size(690, 230);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            Icon = Properties.Resources.HTAlt;
            Name = "About";
            Text = "About HTAlt";
            //
            // pictureBox1
            //
            var pictureBox1 = new System.Windows.Forms.PictureBox
            {
                Image = Properties.Resources.logo,
                Location = new System.Drawing.Point(10, 10),
                Size = new System.Drawing.Size(150, 150),
                SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage
            };
            Controls.Add(pictureBox1);
            var label1 = new System.Windows.Forms.Label
            {
                Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, FontStyle.Bold),
                Location = new System.Drawing.Point(pictureBox1.Location.X + pictureBox1.Width + 5, pictureBox1.Location.Y),
                Text = "HTAlt.WinForms",
                AutoSize = true
            };
            Controls.Add(label1);
            var label2 = new System.Windows.Forms.Label
            {
                Font = new System.Drawing.Font("Microsoft Sans Serif", 12.5F),
                Location = new System.Drawing.Point(label1.Location.X +2, label1.Location.Y + label1.Height + 5),
                Text = HTInfo.ProjectVersion.ToString() + " [" + HTInfo.ProjectCodeName + "] (for " + System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription + ")",
                AutoSize = true
            };
            Controls.Add(label2);
            var label3 = new System.Windows.Forms.Label
            {
                Font = new System.Drawing.Font("Microsoft Sans Serif", 15F),
                Location = new System.Drawing.Point(label1.Location.X, label2.Location.Y + label2.Height +5),
                Text = "by haltroy",
                AutoSize = true
            };
            Controls.Add(label3);
            var label4 = new System.Windows.Forms.Label
            {
                Font = new System.Drawing.Font("Microsoft Sans Serif", 15F),
                Location = new System.Drawing.Point(label1.Location.X + 1, label3.Location.Y + label3.Height + 5),
                Text = "Loaded modules:",
                AutoSize = true
            };
            Controls.Add(label4);
            var textBox1 = new System.Windows.Forms.TextBox
            {
                Location = new System.Drawing.Point(label1.Location.X + 5, label4.Location.Y + label4.Height + 5),
                Multiline = true,
                ReadOnly = true,
                Size = new Size(500, 60)
            };
            textBox1.Text += "HTAlt.Standart : " + HTInfo.ProjectVersion.ToString() + " [" + HTInfo.ProjectCodeName + "]" + Environment.NewLine;
            Controls.Add(textBox1);
            var label5 = new System.Windows.Forms.Label
            {
                Font = new System.Drawing.Font("Microsoft Sans Serif", 11F),
                Location = new System.Drawing.Point(pictureBox1.Location.X + 2, pictureBox1.Location.Y + pictureBox1.Height +5 ),
                Text = "Dark Mode:",
                AutoSize = true
            };
            Controls.Add(label5);
            var htSwitch1 = new HTAlt.WinForms.HTSwitch
            {
                Size = new System.Drawing.Size(50, 20)
            };
            htSwitch1.Location = new System.Drawing.Point((pictureBox1.Location.X + pictureBox1.Width) - (htSwitch1.Width + 10), label5.Location.Y - 2);
            htSwitch1.CheckedChanged += new HTSwitch.CheckedChangedDelegate((sender,e) => 
            {
                BackColor = htSwitch1.Checked ? Color.Black : Color.White;
                ForeColor = htSwitch1.Checked ? Color.White : Color.Black;
                htSwitch1.BackColor = BackColor;
                htSwitch1.OverlayColor = HTAlt.Tools.ShiftBrightness(BackColor, 20, false);
                htSwitch1.ButtonColor = ForeColor;
                htSwitch1.ButtonHoverColor = ForeColor;
                htSwitch1.ButtonPressedColor = ForeColor;
                textBox1.BackColor = HTAlt.Tools.ShiftBrightness(BackColor, 20, false);
                textBox1.ForeColor = ForeColor;
            });
            Controls.Add(htSwitch1);
            ResumeLayout(true);
        }
    }
}