using System.ComponentModel;
using System.Windows.Forms;

namespace Zeroit.Framework.ListView.Helper
{
    // This exists because sometimes we need a panel which can be set to double-buffer
    // And you can't do dat with a reg'lar panel

    /// <summary>
    ///     Represents a panel control with double buffering enabled (does not flicker on update).
    /// </summary>
    [ToolboxItem(false)]
    public partial class NoFlickerPanel : UserControl
    {
        /// <summary>
        ///     Default constructor.
        /// </summary>
        public NoFlickerPanel()
        {
            InitializeComponent();

			this.SetStyle(ControlStyles.OptimizedDoubleBuffer |
						  ControlStyles.AllPaintingInWmPaint |
						  ControlStyles.UserPaint, true);
			this.UpdateStyles();
        }

		private Keys[] inputKeys;

        /// <summary>
        ///     Set list of input keys.
        /// </summary>
        /// <param name="inputKeys">Array of key values.</param>
		public void SetInputKeys(Keys[] inputKeys)
		{
			this.inputKeys = inputKeys;
		}

        /// <summary>
        ///     Override to capture keys specified in <c>SetInputKeys</c>
        /// </summary>
        /// <param name="keyCode">Keycode.</param>
        /// <returns><c>True</c> if handled, <c>false</c> otherwise.</returns>
		protected override bool IsInputKey(Keys keyCode)
		{
			if (inputKeys != null)
			{
				foreach (Keys key in inputKeys)
				{
					if (keyCode == key)
					{
						return true;
					}
				}
			}
			return false;
		}
    }
}
