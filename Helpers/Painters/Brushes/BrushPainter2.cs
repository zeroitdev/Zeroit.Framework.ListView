using System;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Reflection;

namespace Zeroit.Framework.ListView.Editors.Brushes
{
    /// <summary>
    /// 	Specifies the type of fill.
    /// </summary>
	public enum BrushPainter2Type
	{
	    /// <summary>
	    /// 	Specifies no fill.
	    /// </summary>
	    None,
	    
	    /// <summary>
	    /// 	Specifies a solid fill in a single color.
	    /// </summary>
	    Solid,
	    
	    /// <summary>
	    /// 	Specifies a hatched fill.
	    /// </summary>
	    Hatch,
	    
	    /// <summary>
	    /// 	Specifies a color gradient fill.
	    /// </summary>
	    Gradient,
    };

    /// <summary>
    /// 	Class representing a solid, hatched, or gradient fill.
    /// </summary>
    //[TypeConverter(typeof(BrushPainter2.Converter))]
    [EditorAttribute(typeof(BrushPainter2Editor), typeof(System.Drawing.Design.UITypeEditor))]
    public class BrushPainter2
    {
        /// <summary>
        /// 	Constructor for no fill.
        /// </summary>
        public BrushPainter2()
            : this(BrushPainter2Type.None,
                   Color.Empty, Color.Empty,
                   HatchStyle.Cross,
                   null) /* gradientColors */
        {
        }

        /// <summary>
        /// 	Constructor for solid fill.
        /// </summary>
        /// <param name="solidColor">Fill color.</param>
        public BrushPainter2(Color solidColor)
            : this(BrushPainter2Type.Solid,
                   solidColor, solidColor,
                   HatchStyle.Cross,
                   null) /* gradientColors */
        {
        }

        /// <summary>
        /// 	Constructor for hatched fill.
        /// </summary>
        /// <param name="hatchStyle">A <c>System.Drawing.Drawing2D.HatchStyle</c> enumeration value that specifies the style of hatching.</param>
        /// <param name="hatchColor">Hatch lines color.</param>
        /// <param name="backColor">Background color.</param>
        public BrushPainter2(HatchStyle hatchStyle, Color hatchColor, Color backColor)
            : this(BrushPainter2Type.Hatch,
                   hatchColor, backColor,
                   hatchStyle,
                   null) /* gradientColors */
        {
        }

        /// <summary>
        /// 	Constructor for a two color gradient fill.
        /// </summary>
        /// <param name="startColor">Starting color.</param>
        /// <param name="endColor">Ending color.</param>
        public BrushPainter2(Color startColor, Color endColor)
            : this(Utils.NewColorBlend(startColor, endColor))
        {
        }

        /// <summary>
        /// 	Constructor for a multi color gradient fill.
        /// </summary>
        /// <param name="colors">Array of colors to use at corresponding positions along the gradient.</param>
        /// <param name="positions">Array of positions along the gradient.</param>
        /// <remarks>
        /// 	Refer to the documentation of <c>System.Drawing.Drawing2D.ColorBlend</c> for more information about <c>colors</c> and <c>positions</c>.
        /// </remarks>
        /// <exception cref="System.ArgumentNullException">
        ///		Thrown if <paramref name="colors" /> or <paramref name="positions" /> is null.
        ///	</exception>
        public BrushPainter2(Color[] colors, float[] positions)
            : this(Utils.NewColorBlend(colors, positions))
        {
        }

        /// <summary>
        /// 	Constructor for a multi color gradient fill.
        /// </summary>
        /// <param name="gradientColors">A <c>System.Drawing.Drawing2D.ColorBlend</c> object containing arrays of colors and positions defining a multi color gradient.</param>
        /// <exception cref="System.ArgumentNullException">
        ///		Thrown if <paramref name="gradientColors" /> is null.
        ///	</exception>
        public BrushPainter2(ColorBlend gradientColors)
            : this(BrushPainter2Type.Gradient,
                   gradientColors.Colors[0], gradientColors.Colors[gradientColors.Colors.Length - 1],
                   HatchStyle.Cross,
                   gradientColors)
        {
        }

        private BrushPainter2(BrushPainter2Type type,
		                Color color1, Color color2,
        		        HatchStyle hatchStyle,
                		ColorBlend gradientColors)
        {
            FillType = type;
            this.color1 = color1;
            this.color2 = color2;
            HatchStyle = hatchStyle;
            GradientColors = (gradientColors == null) ? Utils.GetDefaultColorBlend() : gradientColors;
        }

        /// <summary>
        ///     Creates an exact copy of this <c>BrushPainter2</c>.
        /// </summary>
        /// <returns>A <c>BrushPainter2</c>.</returns>
        public BrushPainter2 Clone()
        {
            return new BrushPainter2(FillType,
		                       color1, color2,
        		               HatchStyle,
                		       Utils.CloneColorBlend(GradientColors));
        }

        /// <summary>
        /// 	No fill constructor.	
        /// </summary>
        /// <returns><c>BrushPainter2</c> no-fill object.</returns>
        public static BrushPainter2 Empty()
        {
            return new BrushPainter2();
        }

		private readonly Color color1;
		private readonly Color color2;

        /// <summary>
        /// 	Type of fill.
        /// </summary>
        public readonly BrushPainter2Type FillType;

        /// <summary>
        /// 	Hatch style in a hatched fill.
        /// </summary>
        public readonly HatchStyle HatchStyle;

        /// <summary>
        /// 	Color blend for a linear or path gradient fill.
        /// </summary>
        public readonly ColorBlend GradientColors;

		/// <summary>
		/// 	Gets foreground color in a solid fill.
		/// </summary>
		/// <value>
		/// 	Foreground color in a solid fill.
		/// </value>
        public Color SolidColor { get { return color1; } }

		/// <summary>
		/// 	Gets foreground color in a hatched fill.
		/// </summary>
		/// <value>
		/// 	Foreground color in a hatched fill.
		/// </value>
        public Color HatchColor { get { return color1; } }

		/// <summary>
		/// 	Gets background color in a hatched fill.
		/// </summary>
		/// <value>
		/// 	Background color in a hatched fill.
		/// </value>
        public Color BackColor { get { return color2; } }

		internal Brush GetUITypeEditorBrush(Rectangle bounds)
		{
			return GetBrush(bounds);
		}

		internal Brush NonGradientBrush()
		{
            if (FillType == BrushPainter2Type.Solid)
            {
                return new SolidBrush(SolidColor);
            }
            if (FillType == BrushPainter2Type.Hatch)
            {
                return new HatchBrush(HatchStyle, HatchColor, BackColor);
            }
			return null;
		}

        /// <summary>
        ///     Get <c>Brush</c> for this fill.
        /// </summary>
        /// <param name="p1">Starting point of gradient.</param>
        /// <param name="p2">Ending point of gradient.</param>
        /// <returns>Brush.</returns>
		/// <remarks>
		/// 	The <c>p1</c> and <c>p2</c> parameters only affect the brush if <c>FillType</c> is <c>Gradient</c>.
		/// 	<para>
		///     The caller is responsible for disposing of the returned brush.
		/// 	</para>
		/// </remarks>
        public Brush GetBrush(Point p1, Point p2)
        {
            if (FillType == BrushPainter2Type.Gradient)
            {
                LinearGradientBrush brush = new LinearGradientBrush(p1, p2,
	                                                                GradientColors.Colors[0],
    	                                                            GradientColors.Colors[GradientColors.Colors.Length - 1]);
                brush.InterpolationColors = GradientColors;
                return brush;
            }
			return NonGradientBrush();
        }

        /// <summary>
        ///     Get <c>Brush</c> for this fill.
        /// </summary>
        /// <param name="p1">Starting point of gradient.</param>
        /// <param name="p2">Ending point of gradient.</param>
        /// <returns>Brush.</returns>
		/// <remarks>
		/// 	The <c>p1</c> and <c>p2</c> parameters only affect the brush if <c>FillType</c> is <c>Gradient</c>.
		/// 	<para>
		///     The caller is responsible for disposing of the returned brush.
		/// 	</para>
		/// </remarks>
        public Brush GetBrush(PointF p1, PointF p2)
        {
            if (FillType == BrushPainter2Type.Gradient)
            {
                LinearGradientBrush brush = new LinearGradientBrush(p1, p2,
	                                                                GradientColors.Colors[0],
    	                                                            GradientColors.Colors[GradientColors.Colors.Length - 1]);
                brush.InterpolationColors = GradientColors;
                return brush;
            }
			return NonGradientBrush();
        }

        /// <summary>
        ///     Get <c>Brush</c> for this fill.
        /// </summary>
        /// <param name="rect">Rectangle area to which fill is to be applied.</param>
        /// <returns>Brush.</returns>
		/// <remarks>
		/// 	The <c>rect</c> parameter only affects the brush if <c>FillType</c> is <c>Gradient</c>.
		/// 	<para>
		///     The caller is responsible for disposing of the returned brush.
		/// 	</para>
		/// </remarks>
        public Brush GetBrush(Rectangle rect)
        {
            if (FillType == BrushPainter2Type.Gradient)
            {
                LinearGradientBrush brush = new LinearGradientBrush(rect,
	                                                                GradientColors.Colors[0],
    	                                                            GradientColors.Colors[GradientColors.Colors.Length - 1],
        	                                                        LinearGradientMode.Horizontal);
                brush.InterpolationColors = GradientColors;
                return brush;
            }
			return NonGradientBrush();
        }

        /// <summary>
        ///     Get <c>Brush</c> for this fill.
        /// </summary>
        /// <param name="rect">Rectangle area to which fill is to be applied.</param>
		/// <param name="angle">The angle, measured in degrees clockwise from the x-axis, of the gradient's orientation line.</param>
        /// <returns>Brush.</returns>
		/// <remarks>
		/// 	The <c>rect</c> and <c>angle</c> parameters only affect the brush if <c>FillType</c> is <c>Gradient</c>.
		/// 	<para>
		///     The caller is responsible for disposing of the returned brush.
		/// 	</para>
		/// </remarks>
        public Brush GetBrush(Rectangle rect, float angle)
        {
            if (FillType == BrushPainter2Type.Gradient)
            {
                LinearGradientBrush brush = new LinearGradientBrush(rect,
	                                                                GradientColors.Colors[0],
    	                                                            GradientColors.Colors[GradientColors.Colors.Length - 1],
        	                                                        angle);
                brush.InterpolationColors = GradientColors;
                return brush;
            }
			return NonGradientBrush();
        }

        /// <summary>
        ///     Get <c>Brush</c> for this fill.
        /// </summary>
        /// <param name="rect">Rectangle area to which fill is to be applied.</param>
        /// <returns>Brush.</returns>
		/// <remarks>
		/// 	The <c>rect</c> parameter only affects the brush if <c>BrushPainter2Type</c> is <c>Gradient</c>.
		/// 	<para>
		///     The caller is responsible for disposing of the returned brush.
		/// 	</para>
		/// </remarks>
        public Brush GetBrush(RectangleF rect)
        {
            if (FillType == BrushPainter2Type.Gradient)
            {
                LinearGradientBrush brush = new LinearGradientBrush(rect,
	                                                                GradientColors.Colors[0],
    	                                                            GradientColors.Colors[GradientColors.Colors.Length - 1],
        	                                                        LinearGradientMode.Horizontal);
                brush.InterpolationColors = GradientColors;
                return brush;
            }
			return NonGradientBrush();
        }

        /// <summary>
        ///     Get <c>Brush</c> for this fill.
        /// </summary>
        /// <param name="rect">Rectangle area to which fill is to be applied.</param>
		/// <param name="angle">The angle, measured in degrees clockwise from the x-axis, of the gradient's orientation line.</param>
        /// <returns>Brush.</returns>
		/// <remarks>
		/// 	The <c>rect</c> and <c>angle</c> parameters only affect the brush if <c>FillType</c> is <c>Gradient</c>.
		/// 	<para>
		///     The caller is responsible for disposing of the returned brush.
		/// 	</para>
		/// </remarks>
        public Brush GetBrush(RectangleF rect, float angle)
        {
            if (FillType == BrushPainter2Type.Gradient)
            {
                LinearGradientBrush brush = new LinearGradientBrush(rect,
	                                                                GradientColors.Colors[0],
    	                                                            GradientColors.Colors[GradientColors.Colors.Length - 1],
        	                                                        angle);
                brush.InterpolationColors = GradientColors;
                return brush;
            }
			return NonGradientBrush();
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
				if (value is BrushPainter2)
				{
					if (destinationType == typeof(string))
					{
						// Display string in designer
						return "(BrushPainter2)";
					}
					else if (destinationType == typeof(InstanceDescriptor))
					{
						BrushPainter2 filler = (BrushPainter2)value;

						if (filler.FillType == BrushPainter2Type.Solid)
						{
		                    ConstructorInfo ctor = typeof(BrushPainter2).GetConstructor(new Type[] { typeof(Color) });
							if (ctor != null)
							{
								return new InstanceDescriptor(ctor, new object[] { filler.SolidColor });
							}
						}
		                else if (filler.FillType == BrushPainter2Type.Hatch)
						{
                            ConstructorInfo ctor = typeof(BrushPainter2).GetConstructor(new Type[] { typeof(HatchStyle),
				                                                                               typeof(Color),
				                                                                               typeof(Color) });
							if (ctor != null)
							{
								return new InstanceDescriptor(ctor, new object[] { filler.HatchStyle,
																				   filler.HatchColor,
																				   filler.BackColor });
							}
						}
		                else if (filler.FillType == BrushPainter2Type.Gradient)
						{
		                    ConstructorInfo ctor = typeof(BrushPainter2).GetConstructor(new Type[] { typeof(Color[]),
		        																			   typeof(float[]) });
							if (ctor != null)
							{
								return new InstanceDescriptor(ctor, new object[] { filler.GradientColors.Colors,
																				   filler.GradientColors.Positions });
							}
						}
						else
						{
		                    ConstructorInfo ctor = typeof(BrushPainter2).GetConstructor(Type.EmptyTypes);
							if (ctor != null)
							{
								return new InstanceDescriptor(ctor, null);
							}
						}
					}				
				}
				return base.ConvertTo(context, culture, value, destinationType);
			}
        }
    }
    
}

