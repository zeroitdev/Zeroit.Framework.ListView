namespace Zeroit.Framework.ListView.Editors
{
    partial class SystemColorPickerDialog
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
            this.systemColorPicker = new Zeroit.Framework.ListView.Editors.SystemColorPicker();
            this.SuspendLayout();
            // 
            // systemColorPicker
            // 
            this.systemColorPicker.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(56)))), ((int)(((byte)(56)))), ((int)(((byte)(56)))));
            this.systemColorPicker.ColorBoxOffset = 2;
            this.systemColorPicker.ColorBoxWidth = 40;
            this.systemColorPicker.ColorFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.systemColorPicker.Dock = System.Windows.Forms.DockStyle.Fill;
            this.systemColorPicker.Location = new System.Drawing.Point(0, 0);
            this.systemColorPicker.Name = "systemColorPicker";
            this.systemColorPicker.Size = new System.Drawing.Size(188, 164);
            this.systemColorPicker.TabIndex = 0;
            this.systemColorPicker.TitleColor = System.Drawing.Color.DarkGray;
            this.systemColorPicker.TitleFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            this.systemColorPicker.ColorSelected += new Zeroit.Framework.ListView.Editors.BaseColorPicker.ColorSelectedEventHandler(this.systemColorPicker_ColorSelected);
            // 
            // SystemColorPickerDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(56)))), ((int)(((byte)(56)))), ((int)(((byte)(56)))));
            this.ClientSize = new System.Drawing.Size(188, 164);
            this.Controls.Add(this.systemColorPicker);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "SystemColorPickerDialog";
            this.Text = "System Color Picker";
            this.ResumeLayout(false);

        }

        #endregion

        private Zeroit.Framework.ListView.Editors.SystemColorPicker systemColorPicker;
    }
}