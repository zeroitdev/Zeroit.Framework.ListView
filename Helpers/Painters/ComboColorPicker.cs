using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace Zeroit.Framework.ListView.Editors
{
    /// <summary>
    /// 	Implements a color picker which combines the web, system, and custom color pickers.
    /// </summary>
    [ToolboxItem(false)]
    public partial class ComboColorPicker : BaseColorPicker // UserControl
    {
        /// <summary>
        /// 	Constructor with no starting color.
        /// </summary>
        public ComboColorPicker() : this(Color.Empty)
        {
        }

        /// <summary>
        /// 	Constructor with starting color.
        /// </summary>
        /// <param name="color">Starting color.</param>
		public ComboColorPicker(Color color)
		{
            InitializeComponent();
			SetColor(color);
		}

        /// <summary>
        /// 	Set current selected color.
        /// </summary>
        /// <param name="color">Current color.</param>
        /// <returns><c>True</c>.</returns>
		public override bool SetColor(Color color)
		{
			customColorPicker.SetColor(color);
			if (webColorPicker.SetColor(color))
			{
				tabControl.SelectedTab = tabPageWeb;
			}
			else if (systemColorPicker.SetColor(color))
			{
				tabControl.SelectedTab = tabPageSystem;
			}
			else
			{
				tabControl.SelectedTab = tabPageCustom;
			}
            return true;
		}

        private void tab_ColorSelected(object sender, ColorSelectedEventArgs args)
        {
			SelectColor(args);
        }
	}
}
