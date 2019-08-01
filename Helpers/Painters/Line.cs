using System;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Reflection;

namespace Zeroit.Framework.ListView.Editors
{
    /// <summary>
    ///     Class representing a line of any color, thickness, and dash style.
    /// </summary>
	[TypeConverter(typeof(Line.Converter))]
	[EditorAttribute(typeof(LineEditor), typeof(System.Drawing.Design.UITypeEditor))]
	public class Line
	{
	    /// <summary>
	    ///     Default constructor.
	    /// </summary>
		public Line() : this(Color.Black)
		{
		}

        /// <summary>
        ///     Constructor for a solid line of width 1.
        /// </summary>
        /// <param name="color">Line color.</param>
		public Line(Color color) : this(color, 1.0f)
		{
		}

        /// <summary>
        ///     Constructor for a aolid line of any color and width.
        /// </summary>
        /// <param name="color">Line color.</param>
        /// <param name="width">Width in pixels.  A value of zero or less indicates an empty line.</param>
		public Line(Color color, float width) : this(color, width, DashStyle.Solid)
		{
		}

        /// <summary>
        ///     Constructor for a line of any color, width and dashstyle.
        /// </summary>
        /// <param name="color">Line color.</param>
        /// <param name="width">Width in pixels.  A value of zero or less indicates an empty line.</param>
        /// <param name="dashStyle">Dashstyle.</param>
		public Line(Color color, float width, DashStyle dashStyle)
		{
			this.Color = color;
			this.Width = Math.Max(0.0f, width);
			this.DashStyle = dashStyle;
			this.DashPattern = null;
		}

        /// <summary>
        ///     Constructor for a line with a custom dash pattern.
        /// </summary>
        /// <param name="color">Line color.</param>
        /// <param name="width">Width in pixels.  A value of zero or less indicates an empty line.</param>
        /// <param name="dashPattern">An array of real numbers that specifies the lengths of alternating dashes and spaces.</param>
		public Line(Color color, float width, float[] dashPattern)
		{
			this.Color = color;
			this.Width = Math.Max(0.0f, width);
			this.DashStyle = DashStyle.Custom;
			this.DashPattern = dashPattern;
		}

        /// <summary>
        ///     Constructor for a line from another line.
        /// </summary>
        /// <param name="other"><c>Line</c> to copy.</param>
		public Line(Line other)
		{
			this.Color = other.Color;
			this.Width = other.Width;
			this.DashStyle = other.DashStyle;
			this.DashPattern = other.DashPattern;
		}

        /// <summary>
        /// 	Constructs an empty line.
        /// </summary>
        /// <returns>A <c>Line</c> with color <c>Color.Empty</c> and zero width.</returns>
		public static Line Empty()
		{
			return new Line(Color.Empty, 0.0f);
		}

        /// <summary>
        ///     Creates an exact copy of this <c>Line</c>.
        /// </summary>
        /// <returns>A <c>Line</c>.</returns>
		public Line Clone()
		{
			return new Line(this);
		}

        /// <summary>
        ///     Line color.
        /// </summary>
		public readonly Color Color;
		
		/// <summary>
		///     Line width (in pixels).
		/// </summary>
		public readonly float Width;
		
		/// <summary>
		///     Dashstyle.
		/// </summary>
		public readonly DashStyle DashStyle;
		
		/// <summary>
		///     Dash pattern.  Is <c>null</c> unless <c>DashStyle</c> is <c>DashStyle.Custom</c>.
		/// </summary>
		public readonly float[] DashPattern;

        /// <summary>
        /// 	<c>True</c> if empty, false otherwise.
        /// </summary>
		public bool IsEmpty
		{
			get { return Color.IsEmpty && (Width == 0.0f); }
		}

		private Pen pen = null;

		private void DisposePen()
		{
			if (pen != null)
			{
				pen.Dispose();
				pen = null;
			}
		}

        /// <summary>
        ///     Get pen for this line.
        /// </summary>
        /// <returns>Pen.</returns>
		public Pen GetPen()
		{
			if (pen == null)
			{
				pen = new Pen(Color, Width);
				pen.DashStyle = DashStyle;
				if (DashPattern != null)
				{
					pen.DashPattern = DashPattern;
				}
			}
			return pen;
		}

		internal class Converter : TypeConverter
		{
			public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) 
			{
				if (destinationType == typeof(InstanceDescriptor) || destinationType == typeof(string))
				{
					return true;
				}
				return base.CanConvertTo(context, destinationType);
			}

			// This code allows the designer to generate the Fill constructor

			public override object ConvertTo(ITypeDescriptorContext context,
											 CultureInfo culture,
											 object value,
											 Type destinationType)
			{
				if (value is Line)
				{
					if (destinationType == typeof(string))
					{
						// Display string in designer
						return "(Line)";
					}
					else if (destinationType == typeof(InstanceDescriptor))
					{
						Line line = (Line)value;

						if (line.DashStyle == DashStyle.Custom)
						{
		                    ConstructorInfo ctor = typeof(Line).GetConstructor(new Type[] { typeof(Color),
																							typeof(float),
																							typeof(float[]) });
							if (ctor != null)
							{
								return new InstanceDescriptor(ctor, new object[] { line.Color,
																				   line.Width,
																				   line.DashPattern });
							}
						}
						else
						{
		                    ConstructorInfo ctor = typeof(Line).GetConstructor(new Type[] { typeof(Color),
																							typeof(float),
																							typeof(DashStyle) });
							if (ctor != null)
							{
								return new InstanceDescriptor(ctor, new object[] { line.Color,
																				   line.Width,
																				   line.DashStyle });
							}
						}
					}				
				}
				return base.ConvertTo(context, culture, value, destinationType);
			}
		}
	}
}

