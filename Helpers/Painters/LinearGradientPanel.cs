using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Zeroit.Framework.ListView.Editors
{
    /// <summary>
    ///     Represents a control for displaying a linear color gradient.
    /// </summary>
    [ToolboxItem(false)]
    public partial class LinearGradientPanel : UserControl
    {
        /// <summary>
        ///     Default constructor.
        /// </summary>
        public LinearGradientPanel()
        {
            InitializeComponent();

			this.SetStyle(ControlStyles.OptimizedDoubleBuffer |
						  ControlStyles.AllPaintingInWmPaint |
						  ControlStyles.ResizeRedraw |
						  ControlStyles.UserPaint, true);

			this.UpdateStyles();
        }

		private ColorBlend blend;
        /// <summary>
        ///     Gets or sets color blend.
        /// </summary>
        /// <value>
        ///     Color blend.
        /// </value>
		public ColorBlend Blend
		{
			get { return blend; }
			set
			{
				blend = value;
				Redraw();
			}
		}

		private LinearGradientMode mode = LinearGradientMode.Horizontal;
        /// <summary>
        ///     Gets or sets linear gradient mode.
        /// </summary>
        /// <value>
        ///     Linear gradient mode.
        /// </value>
		public LinearGradientMode Mode
		{
			get { return mode; }
			set
			{
				mode = value;
				Redraw();
			}
		}

        private LinearGradientBrush2 br = null;

        private void Redraw()
        {
            if (br != null)
            {
                br.Dispose();
                br = null;
            }
            Invalidate(true);
        }

        private void this_Paint(object sender, PaintEventArgs e)
        {
			if (blend != null)
			{
				if (br == null)
				{
					br = new LinearGradientBrush2(this.ClientRectangle, blend, mode);
				}
	            br.FillRectangle(e.Graphics, this.ClientRectangle, SystemBrushes.Window);
			}
        }
    }
}
