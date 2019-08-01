using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Zeroit.Framework.ListView.Editors.Brushes
{
    /// <summary>
    /// 	Implements a dialog which allows design and editing of a <c>BrushPainter</c> object.
	/// 	May be used in designer.
    /// </summary>
    public partial class BrushPainterEditorDialog : System.Windows.Forms.Form
    {
        private const int gradientLinear = 0;
        private const int gradientPathRect = 1;
        private const int gradientPathRadial = 2;

        /// <summary>
		///		Initializes a new instance of <c>BrushPainterEditorDialog</c> using an empty <c>BrushPainter</c>
		/// 	at the default window position.
        /// </summary>
        public BrushPainterEditorDialog() : this(BrushPainter.Empty())
        {
        }

        /// <summary>
		/// 	Initializes a new instance of <c>BrushPainterEditorDialog</c> using an existing <c>BrushPainter</c>
		/// 	at the default window position.
        /// </summary>
        /// <param name="filler">Existing <c>BrushPainter</c> object.</param>
		/// <exception cref="System.ArgumentNullException">
		///		Thrown if <paramref name="filler" /> is null.
		///	</exception>
        public BrushPainterEditorDialog(BrushPainter filler)
        {
            if (filler == null)
            {
                throw new ArgumentNullException("filler");
            }

            InitializeComponent();
            FillGradientComboBox();
            AdjustDialogSize();
            SetControlsToInitialValues(filler);
        }

        /// <summary>
		///		Initializes a new instance of <c>BrushPainterEditorDialog</c> using an empty <c>BrushPainter</c>
		///		and positioned beneath the specified control.
        /// </summary>
        /// <param name="c">Control beneath which the dialog should be placed.</param>
        public BrushPainterEditorDialog(Control c) : this(BrushPainter.Empty(), c)
        {
        }

        /// <summary>
		///		Initializes a new instance of <c>BrushPainterEditorDialog</c> using an existing <c>BrushPainter</c>
		///		and positioned beneath the specified control.
        /// </summary>
        /// <param name="filler">Existing <c>BrushPainter</c> object.</param>
        /// <param name="c">Control beneath which the dialog should be placed.</param>
		/// <exception cref="System.ArgumentNullException">
		///		Thrown if <paramref name="filler" /> is null.
		///	</exception>
        public BrushPainterEditorDialog(BrushPainter filler, Control c) : this(filler)
        {
            Utils.SetStartPositionBelowControl(this, c);
        }

        private void FillGradientComboBox()
        {
            gradientTypeComboBox.Items.Clear();
            gradientTypeComboBox.Items.Add("Linear");
            gradientTypeComboBox.Items.Add("Path Rect");
            gradientTypeComboBox.Items.Add("Path Radial");
        }

        private void AdjustDialogSize()
        {
            // Three different possible group boxes - move them all to one coordinate
            int x = solidGroupBox.Location.X;
            int y = typeGroupBox.Location.Y;

            solidGroupBox.Location = new Point(x, y);
            hatchGroupBox.Location = new Point(x, y);
            gradientGroupBox.Location = new Point(x, y);
            //formName.Location = new Point(typeGroupBox.Location.X, formName.Location.Y - 5);

            int bottomY = Math.Max(solidGroupBox.Bounds.Bottom,
                          Math.Max(hatchGroupBox.Bounds.Bottom,
                             Math.Max(gradientGroupBox.Bounds.Bottom,
                                   typeGroupBox.Bounds.Bottom)));



            int newHeight = bottomY + typeGroupBox.Location.Y;

            this.Size = new Size(Size.Width, Size.Height - (ClientSize.Height - newHeight));
        }

        private void SetControlsToInitialValues(BrushPainter filler)
        {
            // Fill with defaults
            Init(filler.SolidColor, solidColorLabel, solidAlphaNud);
            Init(filler.HatchColor, hatchColorLabel, hatchAlphaNud);
            Init(filler.BackColor, backColorLabel, backAlphaNud);
            gradientEditor.Blend = filler.GradientColors;

            if (filler.FillType == BrushPainterType.PathGradient)
            {
                if (filler.BrushPathGradientType == BrushPathGradientType.Rect)
                {
                    gradientTypeComboBox.SelectedIndex = gradientPathRect;
                }
                else
                {
                    gradientTypeComboBox.SelectedIndex = gradientPathRadial;
                }
            }
            else
            {
                gradientTypeComboBox.SelectedIndex = gradientLinear;
                gradientAngleNud.Value = (decimal)filler.LinearGradientAngle;
            }
            UpdateGradient();

            hatchComboBox.SelectedIndex = 0;
            for (int i = 0; i < hatchComboBox.Items.Count; i++)
            {
                if (filler.HatchStyle == (HatchStyle)(hatchComboBox.Items[i]))
                {
                    hatchComboBox.SelectedIndex = i;
                }
            }
            UpdateHatch();

            UpdateSolid();

            if (filler.FillType == BrushPainterType.None)
            {
                noneRadioButton.Checked = true;
            }
            else if (filler.FillType == BrushPainterType.Solid)
            {
                solidRadioButton.Checked = true;
            }
            else if (filler.FillType == BrushPainterType.Hatch)
            {
                hatchRadioButton.Checked = true;
            }
            else
            {
                gradientRadioButton.Checked = true;
            }
        }

        private void Init(Color c, Label label, NumericUpDown alphaNud)
        {
            label.BackColor = RGB(c);
            alphaNud.Value = (decimal)c.A;
        }

        private Color FromLabelNud(Label label, NumericUpDown alphaNud)
        {
            return Color.FromArgb((int)alphaNud.Value, label.BackColor);
        }

        private Color RGB(Color c)
        {
            return Color.FromArgb(255, c);
        }

        private BrushPainter filler;
        /// <summary>
        /// 	Gets current filler.
        /// </summary>
        /// <value>
        /// 	Current filler.
        /// </value>
        public BrushPainter BrushPainter
        {
            get { return filler; }
        }

        private void UpdateSolid()
        {
            sampleSolidPanel.BackColor = FromLabelNud(solidColorLabel, solidAlphaNud);
        }

        private void UpdateHatch()
        {
            sampleHatchPanel.Set(hatchComboBox.SelectedHatchStyle,
                                 FromLabelNud(hatchColorLabel, hatchAlphaNud),
                                 FromLabelNud(backColorLabel, backAlphaNud));
        }

        private void UpdateGradient()
        {
            gradientAngleTextLabel.Enabled = gradientTypeComboBox.SelectedIndex == gradientLinear;
            gradientAngleNud.Enabled = gradientTypeComboBox.SelectedIndex == gradientLinear;
        }

        private void solidColorButton_Click(object sender, EventArgs e)
        {
            ComboColorPickerDialog d = new ComboColorPickerDialog(solidColorLabel.BackColor, solidColorButton);
            if (d.ShowDialog() == DialogResult.OK)
            {
                solidColorLabel.BackColor = d.Color;
            }
            UpdateSolid();
        }

        private void solidAlphaNud_ValueChanged(object sender, EventArgs e)
        {
            UpdateSolid();
        }

        private void hatchColorButton_Click(object sender, EventArgs e)
        {
            ComboColorPickerDialog d = new ComboColorPickerDialog(hatchColorLabel.BackColor, hatchColorButton);
            if (d.ShowDialog() == DialogResult.OK)
            {
                hatchColorLabel.BackColor = Color.FromArgb((int)hatchAlphaNud.Value, d.Color);
            }
            UpdateHatch();
        }

        private void hatchAlphaNud_ValueChanged(object sender, EventArgs e)
        {
            UpdateHatch();
        }

        private void backColorButton_Click(object sender, EventArgs e)
        {
            ComboColorPickerDialog d = new ComboColorPickerDialog(backColorLabel.BackColor, backColorButton);
            if (d.ShowDialog() == DialogResult.OK)
            {
                backColorLabel.BackColor = d.Color;
            }
            UpdateHatch();
        }

        private void backAlphaNud_ValueChanged(object sender, EventArgs e)
        {
            UpdateHatch();
        }

        private void hatchComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateHatch();
        }

        private void gradientTypeComboBox_SelectedValueChanged(object sender, EventArgs e)
        {
            UpdateGradient();
        }

        private void fillerTypeChanged(object sender, EventArgs e)
        {
            solidGroupBox.Visible = false;
            hatchGroupBox.Visible = false;
            gradientGroupBox.Visible = false;


            if (solidRadioButton.Checked)
            {
                solidGroupBox.Visible = true;

            }
            else if (hatchRadioButton.Checked)
            {
                hatchGroupBox.Visible = true;
            }
            else if (gradientRadioButton.Checked)
            {
                gradientGroupBox.Visible = true;
            }
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            if (solidRadioButton.Checked)
            {
                filler = new BrushPainter(FromLabelNud(solidColorLabel, solidAlphaNud));
            }
            else if (hatchRadioButton.Checked)
            {
                filler = new BrushPainter(hatchComboBox.SelectedHatchStyle,
                                     FromLabelNud(hatchColorLabel, hatchAlphaNud),
                                     FromLabelNud(backColorLabel, backAlphaNud));
            }
            else if (gradientRadioButton.Checked)
            {
                if (gradientTypeComboBox.SelectedIndex == gradientLinear)
                {
                    filler = new BrushPainter((float)gradientAngleNud.Value, gradientEditor.Blend);
                }
                else if (gradientTypeComboBox.SelectedIndex == gradientPathRect)
                {
                    filler = new BrushPainter(BrushPathGradientType.Rect, gradientEditor.Blend);
                }
                else
                {
                    filler = new BrushPainter(BrushPathGradientType.Radial, gradientEditor.Blend);
                }
            }
            else
            {
                filler = BrushPainter.Empty();
            }
            DialogResult = DialogResult.OK;
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void exit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void solidColorButton_MouseEnter(object sender, EventArgs e)
        {
            solidColorButton.FlatAppearance.BorderColor = Color.FromArgb(128, 255, 255);
        }

        private void solidColorButton_MouseLeave(object sender, EventArgs e)
        {
            solidColorButton.FlatAppearance.BorderColor = Color.FromArgb(56, 56, 56);
        }

        private void hatchColorButton_MouseEnter(object sender, EventArgs e)
        {
            hatchColorButton.FlatAppearance.BorderColor = Color.FromArgb(128, 255, 255);
        }

        private void hatchColorButton_MouseLeave(object sender, EventArgs e)
        {
            hatchColorButton.FlatAppearance.BorderColor = Color.FromArgb(56, 56, 56);
        }

        private void backColorButton_MouseEnter(object sender, EventArgs e)
        {
            backColorButton.FlatAppearance.BorderColor = Color.FromArgb(128, 255, 255);
        }

        private void backColorButton_MouseLeave(object sender, EventArgs e)
        {
            backColorButton.FlatAppearance.BorderColor = Color.FromArgb(56, 56, 56);
        }

        private void CloseBtn_MouseEnter(object sender, EventArgs e)
        {
            CloseBtn.BackColor = Color.Red;
        }

        private void CloseBtn_MouseLeave(object sender, EventArgs e)
        {
            CloseBtn.BackColor = Color.FromArgb(34, 34, 34);
        }
    }

    /// <summary>
    ///     The <c>UITypeEditor</c> derived class which indicates how a <c>BrushPainter</c>
    ///     object can be edited directly from Visual Studio Designer.
    /// </summary>
    /// <remarks>
    ///     Note that this class is <b>NOT</b> meant to be invoked directly
    /// </remarks>
	public class BrushPainterEditor : System.Drawing.Design.UITypeEditor
    {
        /// <summary>
        ///     Gets the editor style used by the <c>EditValue</c> method.
        /// </summary>
        /// <param name="context">An ITypeDescriptorContext that can be used to gain additional context information.</param>
        /// <returns><c>UITypeEditorEditStyle.Modal</c></returns>
        public override UITypeEditorEditStyle GetEditStyle(System.ComponentModel.ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        /// <summary>
        ///     Creates and displays a <c>BrushPainterEditorDialog</c> dialog if <c>value</c> is a <c>BrushPainter</c>.
        /// </summary>
        /// <param name="context">An ITypeDescriptorContext that can be used to gain additional context information.</param>
        /// <param name="provider">An IServiceProvider through which editing services may be obtained.</param>
        /// <param name="value">An instance of <c>BrushPainter</c> being edited.</param>
        /// <returns>The new value of the <c>BrushPainter</c> being edited.</returns>
		public override object EditValue(System.ComponentModel.ITypeDescriptorContext context,
                                         System.IServiceProvider provider,
                                         object value)
        {
            if (value is BrushPainter)
            {
                Editors.Brushes.BrushPainterEditorDialog dialog = new Editors.Brushes.BrushPainterEditorDialog((BrushPainter)value);
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    return dialog.BrushPainter;
                }
            }
            return value;
        }

        /// <summary>
        ///     Indicates that painting is supported.
        /// </summary>
        /// <param name="context">An ITypeDescriptorContext that can be used to gain additional context information.</param>
        /// <returns><c>true</c>.</returns>
		public override bool GetPaintValueSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        /// <summary>
        ///     Paint a representation of the simple filler (usually in designer).
        /// </summary>
        /// <param name="e">A <c>PaintValueEventArgs</c> that indicates what to paint and where to paint it.</param>
		public override void PaintValue(PaintValueEventArgs e)
        {
            if (e.Value is BrushPainter)
            {
                System.Drawing.Brush br = ((BrushPainter)e.Value).GetUITypeEditorBrush(e.Bounds);
                if (br != null)
                {
                    e.Graphics.FillRectangle(br, e.Bounds /*r*/);
                }
            }
        }
    }
}
