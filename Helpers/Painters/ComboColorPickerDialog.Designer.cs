namespace Zeroit.Framework.ListView.Editors
{
    partial class ComboColorPickerDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.comboColorPicker = new Zeroit.Framework.ListView.Editors.ComboColorPicker();
            this.SuspendLayout();
            // 
            // comboColorPicker
            // 
            this.comboColorPicker.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(22)))), ((int)(((byte)(22)))));
            this.comboColorPicker.Dock = System.Windows.Forms.DockStyle.Fill;
            this.comboColorPicker.Location = new System.Drawing.Point(0, 0);
            this.comboColorPicker.MinimumSize = new System.Drawing.Size(304, 338);
            this.comboColorPicker.Name = "comboColorPicker";
            this.comboColorPicker.Size = new System.Drawing.Size(324, 398);
            this.comboColorPicker.TabIndex = 0;
            this.comboColorPicker.ColorSelected += new Zeroit.Framework.ListView.Editors.BaseColorPicker.ColorSelectedEventHandler(this.comboColorPicker_ColorSelected);
            // 
            // ComboColorPickerDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(22)))), ((int)(((byte)(22)))));
            this.ClientSize = new System.Drawing.Size(324, 398);
            this.Controls.Add(this.comboColorPicker);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(324, 398);
            this.Name = "ComboColorPickerDialog";
            this.Text = "Color Picker";
            this.ResumeLayout(false);

        }

        #endregion

        private ComboColorPicker comboColorPicker;
    }
}