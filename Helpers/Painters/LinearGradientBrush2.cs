using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace Zeroit.Framework.ListView.Editors
{
    /// <summary>
    ///     Represents a replacement for the <c>System.Drawing.Drawing2D.LinearGradientBrush</c>.
    /// </summary>
	/// <remarks>
	/// 	There is a bug in the <c>System.Drawing.Drawing2D.LinearGradientBrush</c> where somtimes,
	/// 	the gradient wraps around and fills the first pixel column/row with the last color.
	/// 	It was explained as one-of-many off-by-one bugs in GDI+:
	///		http://stackoverflow.com/questions/5326473/weird-behavior-of-lineargradientbrush
    /// </remarks>
	public class LinearGradientBrush2
	{
	    /// <summary>
	    ///     Constructor for a two color linear gradient.
	    /// </summary>
	    /// <param name="rect">Bounding rectangle of area to be filled.</param>
	    /// <param name="c1">Starting color.</param>
	    /// <param name="c2">Ending color.</param>
        /// <param name="mode">A <c>System.Drawing.Drawing2D.LinearGradientMode</c> enumeration value that specifies the orientation of the gradient.</param>
		public LinearGradientBrush2(Rectangle rect, Color c1, Color c2, LinearGradientMode mode)
		{
			this.mode = mode;
			gradientBrush = new LinearGradientBrush(rect, c1, c2, mode);
		}

        /// <summary>
        ///     Constructor for a multi color linear gradient.
        /// </summary>
        /// <param name="rect">Bounding rectangle of area to be filled.</param>
        /// <param name="blend">A <c>System.Drawing.Drawing2D.ColorBlend</c> object containing arrays of colors and positions defining a multi color gradient.</param>
        /// <param name="mode">A <c>System.Drawing.Drawing2D.LinearGradientMode</c> enumeration value that specifies the orientation of the gradient.</param>
        /// <exception cref="System.ArgumentNullException">
        ///		Thrown if <paramref name="blend" /> is null.
        ///	</exception>
        public LinearGradientBrush2(Rectangle rect, ColorBlend blend, LinearGradientMode mode)
		{
			if (blend == null)
			{
				throw new ArgumentNullException("blend");
			}
			this.mode = mode;
			gradientBrush = new LinearGradientBrush(rect, blend.Colors[0], blend.Colors[blend.Colors.Length - 1], mode);
			gradientBrush.InterpolationColors = blend;
		}

        /// <summary>
        ///     Dispose of brush.
        /// </summary>
		public void Dispose()
		{
			if (gradientBrush != null)
			{
				gradientBrush.Dispose();
				gradientBrush = null;
			}
		}

		private LinearGradientMode mode;
		private LinearGradientBrush gradientBrush = null;

        /// <summary>
        ///     Fill a rectangular area with the linear gradient.
        /// </summary>
        /// <param name="g">Graphics object.</param>
        /// <param name="r">Rectangle area to fill.</param>
        /// <param name="backBrush">Brush to fill the background before filling with the linear gradient.</param>
        /// <exception cref="System.ArgumentNullException">
        ///		Thrown if <paramref name="g" /> is null.
        ///	</exception>
		public void FillRectangle(Graphics g, Rectangle r, Brush backBrush)
		{
			if (g == null)
			{
				throw new ArgumentNullException("g");
			}

			g.FillRectangle(backBrush, r);

			PixelOffsetMode oldMode = g.PixelOffsetMode;
			g.PixelOffsetMode = PixelOffsetMode.Half;
			g.FillRectangle(gradientBrush, r);
			g.PixelOffsetMode = oldMode;
		}
	}
}
