using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Zeroit.Framework.ListView.Editors
{
    /// <summary>
    ///     Represents a control for displaying a hatch pattern.
    /// </summary>
    [ToolboxItem(false)]
    public partial class HatchStylePanel : UserControl
    {
        /// <summary>
        ///     Default constructor.
        /// </summary>
        public HatchStylePanel()
        {
            InitializeComponent();

			this.SetStyle(ControlStyles.OptimizedDoubleBuffer |
						  ControlStyles.AllPaintingInWmPaint |
						  ControlStyles.ResizeRedraw |
						  ControlStyles.UserPaint, true);

			this.UpdateStyles();
        }

        private Color hatchColor = Color.Black;
        /// <summary>
        ///     Gets or sets hatch color.
        /// </summary>
        /// <value>
        ///     Hatch color.
        /// </value>
        public Color HatchColor
        {
            get { return hatchColor; }
            set
            {
                hatchColor = value;
                Redraw();
            }
        }

        /// <summary>
        ///     Gets or sets background color.
        /// </summary>
        /// <value>
        ///     Background color.
        /// </value>
        public override Color BackColor
        {
            get { return base.BackColor; }
            set
            {
                base.BackColor = value;
                Redraw();
            }
        }

        private HatchStyle hatchStyle = HatchStyle.Cross;
        /// <summary>
        ///     Gets or sets hatch style.
        /// </summary>
        /// <value>
        ///     Hatch style.
        /// </value>
        public HatchStyle HatchStyle
        {
            get { return hatchStyle; }
            set
            {
                hatchStyle = value;
                Redraw();
            }
        }

        /// <summary>
        ///     Set hatch style, hatch color, and background color.
        /// </summary>
        /// <param name="hatchStyle">Hatch style.</param>
        /// <param name="hatchColor">Hatch color.</param>
        /// <param name="backColor">Background color.</param>
		public void Set(HatchStyle hatchStyle, Color hatchColor, Color backColor)
		{
			this.hatchStyle = hatchStyle;
			this.hatchColor = hatchColor;
			this.BackColor = backColor;
			Redraw();
		}
        
        private Brush br = null;

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
            if (br == null)
            {
                br = new HatchBrush(hatchStyle, hatchColor, BackColor);
			}
            e.Graphics.FillRectangle(br, this.ClientRectangle);
        }
    }
}
