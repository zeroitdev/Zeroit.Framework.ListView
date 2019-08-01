using System.Drawing;
using System.Windows.Forms;

namespace Zeroit.Framework.ListView.Editors
{
    /// <summary>
    ///     Represents a dialog that enabled the user to select a web color.
    /// </summary>
    public partial class WebColorPickerDialog : System.Windows.Forms.Form
    {
        /// <summary>
        ///     Default constructor (no starting color and default window position).
        /// </summary>
        public WebColorPickerDialog() : this(Color.Empty)
        {
		}

        /// <summary>
        ///     Constructor with starting color and default window position.
        /// </summary>
        /// <param name="color">Starting color.</param>
        public WebColorPickerDialog(Color color)
        {
            InitializeComponent();
			webColorPicker.SetColor(color);
        }

        /// <summary>
        ///     Constructor with no starting color and starting position beneath specified control.
        /// </summary>
        /// <param name="c">Control beneath which the dialog should be placed.</param>
		public WebColorPickerDialog(Control c) : this(Color.Empty, c)
		{
		}

        /// <summary>
        ///     Constructor with starting color and starting position beneath specified control.
        /// </summary>
        /// <param name="color">Starting color.</param>
        /// <param name="c">Control beneath which the dialog should be placed.</param>
		public WebColorPickerDialog(Color color, Control c)
		{
            InitializeComponent();
			Utils.SetStartPositionBelowControl(this, c);
			webColorPicker.SetColor(color);
		}

        /// <summary>
        ///     Consructor with no starting color and starting position.
        /// </summary>
        /// <param name="p">Starting position.</param>
		public WebColorPickerDialog(Point p) : this(Color.Empty, p)
		{
		}

        /// <summary>
        ///     Constructor with starting color and starting position.
        /// </summary>
        /// <param name="color">Starting color.</param>
        /// <param name="p">Starting position.</param>
		public WebColorPickerDialog(Color color, Point p)
		{
            InitializeComponent();
			Utils.SetStartPosition(this, p);
			webColorPicker.SetColor(color);
		}

		private Color color;
		/// <summary>
		///     Gets or sets selected color.
		/// </summary>
		/// <value>
		///     Selected color.
		/// </value>
		public Color Color
		{
			get { return color; }
			set	{ webColorPicker.SetColor(value); }
		}

		private string colorName;
		/// <summary>
		///     Gets name of selected color.
		/// </summary>
		/// <value>
		///     Name of selected color.
		/// </value>
		public string ColorName
		{
			get { return colorName; }
		}

        /// <summary>
        ///     Override to capture Esc key.
        /// </summary>
        /// <param name="keyCode">Key.</param>
        /// <returns><c>True</c> if key handled, <c>false</c> otherwise.</returns>
		protected override bool ProcessDialogKey(Keys keyCode)
		{
			if (keyCode == Keys.Escape)
			{
				DialogResult = DialogResult.Cancel;
                return true;
			}
            return false;
		}

        private void webColorPicker_ColorSelected(object sender, Zeroit.Framework.ListView.Editors.ColorSelectedEventArgs args)
        {
            color = args.Color;
			colorName = args.ColorName;
            DialogResult = DialogResult.OK;
        }
    }
}
