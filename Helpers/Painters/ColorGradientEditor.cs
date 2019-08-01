using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Zeroit.Framework.ListView.Editors
{
    /// <summary>
    /// 	Implements a control for designing a color gradient.
    /// </summary>
    [ToolboxItem(false)]
    public partial class ColorGradientEditor : UserControl
    {
        /// <summary>
        /// 	Default contructor.
        /// </summary>
        public ColorGradientEditor()
        {
            InitializeComponent();

			this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
			this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
			this.UpdateStyles();

			SetControlPanelLocation();

			vals = new List<Val>();
			SetVal(Utils.GetDefaultColorBlend());

			selMouse = false;
			ChangeSel(1);
			RedrawGradient();
        }

		private class Val
		{
			public Val(int position, Color color)
			{
				Debug.Assert(position >= 0 && position <= 100);
				Position = position;
				Color = color;
			}

			public override string ToString()
			{
				return string.Format("Pos {0}; Color.RGB {1},{2},{3}; Color.A {4}",
										Position,
										Color.R, Color.G, Color.B, Color.A);
			}

			public readonly int Position;
			public readonly Color Color;
		}

		private int sel;
		private List<Val> vals;

		private int minPos; // min position for sel
		private int maxPos; // max position for sel

		private bool selMouse; // is mouse down on sel?
		private int selMouseX; // x position of mouse when mouse goes down
		private int selMousePos; // pos of sel when mouse goes down

		private void SetVal(ColorBlend cb)
		{
			if (cb == null)
			{
				throw new ArgumentNullException("Blend");
			}
			if (   cb.Positions.Length < 2
				|| cb.Positions.Length != cb.Colors.Length
				|| cb.Positions[0] != 0.0f
				|| cb.Positions[cb.Positions.Length - 1] != 1.0f)
			{
				throw new ArgumentException("Blend");
			}

			for (int i = 1; i < cb.Positions.Length - 1; i++)
			{
				if (   cb.Positions[i] < cb.Positions[i - 1]
					|| cb.Positions[i] > cb.Positions[i + 1])
				{
					throw new ArgumentException("Blend");
				}
			}

			vals.Clear();
			vals.Add(new Val(0, cb.Colors[0]));
			for (int i = 1; i < cb.Positions.Length - 1; i++)
			{
				vals.Add(new Val((int)Math.Round(100.0 * cb.Positions[i]), cb.Colors[i]));
			}
			vals.Add(new Val(100, cb.Colors[cb.Colors.Length - 1]));
			
		}

        /// <summary>
        /// 	Gets or sets color blend.
        /// </summary>
        /// <value>
        /// 	Color blend.
        /// </value>
    	public ColorBlend Blend
		{
			get
			{
				ColorBlend blend = new ColorBlend(vals.Count);
				for (int i = 0; i < vals.Count; i++)
				{
					blend.Positions[i] = (float)vals[i].Position * 0.01f;
					blend.Colors[i] = vals[i].Color;
				}
				return blend;
			}
			set
			{
				SetVal(value);
				ChangeSel(Math.Min(sel, vals.Count - 1));
				ClearGradientBrush();
				RedrawGradient();
			}
        }

		private Color gradientBorderColor = Color.DarkGray;
        /// <summary>
        /// 	Gets or sets color of border around gradient display.
        /// </summary>
        /// <value>
        /// 	Color of border around gradient display.
        /// </value>
		[Category("Appearance"), DefaultValue("DarkGray")]
		public Color GradientBorderColor
		{
			get { return gradientBorderColor; }
			set
			{
				gradientBorderColor = value;
				ClearBorderBrush();
				RedrawGradient();
			}
		}

		private int gradientBorderSize = 1;
        /// <summary>
        /// 	Gets or sets pixel size of border around gradient display.
        /// </summary>
        /// <value>
        /// 	Pixel size of border around gradient display.
        /// </value>
		[Category("Appearance"), DefaultValue(1)]
		public int GradientBorderSize
		{
			get { return gradientBorderSize; }
			set
			{
				int val = Math.Max(Math.Min(5, value), 0);
				if (gradientBorderSize != val)
				{
					gradientBorderSize = val;
					ClearGradientBrush();
					ClearMarkers();
					RedrawGradient();
				}
			}
		}

		private Color gradientBackColor = Color.White;
        /// <summary>
        /// 	Gets or sets background color in gradient display.
        /// </summary>
        /// <value>
        /// 	Background color in gradient display.
        /// </value>
		[Category("Appearance"), DefaultValue("White")]
		public Color GradientBackColor
		{
			get { return gradientBackColor; }
			set
			{
				gradientBackColor = value;
				ClearBackGradientBrush();
				RedrawGradient();
			}
		}

		private Color gradientHatchColor = Color.Black;
        /// <summary>
        /// 	Gets or sets hatch pattern in background of gradient display.
        /// </summary>
        /// <value>
        /// 	Hatch pattern in background of gradient display.
        /// </value>
		[Category("Appearance"), DefaultValue("Black")]
		public Color GradientHatchColor
		{
			get { return gradientHatchColor; }
			set
			{
				gradientHatchColor = value;
				ClearBackGradientBrush();
				RedrawGradient();
			}
		}

		private Color markerBorderColor = Color.Black;
        /// <summary>
        /// 	Gets or sets marker border color.
        /// </summary>
        /// <value>
        /// 	Marker border color.
        /// </value>
		[Category("Appearance"), DefaultValue("Black")]
		public Color MarkerBorderColor
		{
			get { return markerBorderColor; }
			set
			{
				markerBorderColor = value;
				ClearMarkerBorderPen();
				RedrawGradient();
			}
		}

		private Color markerFillColor = Color.White;
        /// <summary>
        /// 	Gets or sets marker fill color.
        /// </summary>
        /// <value>
        /// 	Marker fill color.
        /// </value>
		[Category("Appearance"), DefaultValue("White")]
		public Color MarkerFillColor
		{
			get { return markerFillColor; }
			set
			{
				markerFillColor = value;
				ClearMarkerFillBrush();
				RedrawGradient();
			}
		}

		private Color selMarkerFillColor = Color.Yellow;
        /// <summary>
        /// 	Gets or sets selected marker fill color.
        /// </summary>
        /// <value>
        /// 	Selected marker fill color.
        /// </value>
		[Category("Appearance"), DefaultValue("Yellow")]
		public Color SelMarkerFillColor
		{
			get { return selMarkerFillColor; }
			set
			{
				selMarkerFillColor = value;
				ClearSelMarkerFillBrush();
				RedrawGradient();
			}
		}

        public Color Color
        {
            get { return Color.FromArgb(255, vals[sel].Color); }
        }

        private void gradientPanel_SizeChanged(object sender, EventArgs e)
        {
			ClearGradientBrush();
			ClearMarkers();
			RedrawGradient();
        }

        private void mainSplit_Panel2_SizeChanged(object sender, EventArgs e)
        {
			SetControlPanelLocation();
        }

		private void SetControlPanelLocation()
		{
        	//controlPanel.Location = new Point((mainSplit.Panel2.Size.Width - controlPanel.Size.Width) / 2, 0);
		}

		private void ChangeSel(int newSel)
		{
			ClearSelMarker();
			ClearMarker(newSel);
			sel = newSel;
			ChangeSel();
		}

        private bool insideChangeSel = false;

		private void ChangeSel()
		{
            insideChangeSel = true;
            
			stopLabel.Text = string.Format("Stop {0}/{1}", sel + 1, vals.Count);

			delButton.Enabled = (sel > 0) && (sel < vals.Count - 1);

			firstButton.Enabled = (sel > 0);
			prevButton.Enabled = (sel > 0);
			nextButton.Enabled = (sel < vals.Count - 1);
			lastButton.Enabled = (sel < vals.Count - 1);

			int nextPos = -1;
			int prevPos = -1;

			if (sel == 0)
			{
				minPos = 0;
				maxPos = 0;
				prevPos = 0;
				nextPos = vals[sel + 1].Position - 1;
			}
			else if (sel == vals.Count - 1)
			{
				minPos = 100;
				maxPos = 100;
				prevPos = vals[sel - 1].Position + 1;
				nextPos = 100;
			}
			else
			{
				minPos = vals[sel - 1].Position + 1;
				maxPos = vals[sel + 1].Position - 1;
				prevPos = minPos;
				nextPos = maxPos;
			}

			newBeforeButton.Enabled = ((vals[sel].Position - prevPos) > 0);
			newAfterButton.Enabled  = ((nextPos - vals[sel].Position) > 0);

			positionNud.Enabled = (sel > 0 && sel < vals.Count - 1);
			positionNud.Minimum = minPos;
			positionNud.Maximum = maxPos;

			ShowSel();
			RedrawGradient();
			
            insideChangeSel = false;
		}

		private void ShowSel()
		{
			positionNud.Value = vals[sel].Position;
			colorLabel.BackColor = Color.FromArgb(255, vals[sel].Color);
			alphaNud.Value = vals[sel].Color.A;
		}

		private void ChangePosition(int newPos)
		{
            if (!insideChangeSel)
            {
                vals[sel] = new Val(Math.Max(Math.Min(newPos, maxPos), minPos), vals[sel].Color);
                ClearGradientBrush();
                ClearSelMarker();
                ShowSel();
                RedrawGradient();
            }
		}

		private void ChangeOpacity(int newOpacity)
        {
            if (!insideChangeSel)
            {
                vals[sel] = new Val(vals[sel].Position, Color.FromArgb(newOpacity, vals[sel].Color));
                ClearGradientBrush();
                ShowSel();
                RedrawGradient();
            }
		}

		private void ChangeColor(Color newColor)
		{
            if (!insideChangeSel)
            {
                vals[sel] = new Val(vals[sel].Position, Color.FromArgb(vals[sel].Color.A, newColor));
                ClearGradientBrush();
                ShowSel();
                RedrawGradient();
            }
		}

        private void delButton_Click(object sender, EventArgs e)
        {
			if (sel > 0 && sel < vals.Count - 1)
			{
				ClearGradientBrush();
				ClearMarkers();

				vals.RemoveAt(sel);
				ChangeSel();

				RedrawGradient();
			}
        }

        private void newBeforeButton_Click(object sender, EventArgs e)
        {
			NewSel(sel - 1);
        }

        private void newAfterButton_Click(object sender, EventArgs e)
        {
			NewSel(sel);
        }

		private void NewSel(int selLeft)
		{
			ClearGradientBrush();
			ClearMarkers();
			
			Val v1 = vals[selLeft];
			Val v2 = vals[selLeft + 1];
			Val v = new Val((v1.Position + v2.Position) / 2,
							Color.FromArgb((v1.Color.A + v2.Color.A) / 2, 
										   (v1.Color.R + v2.Color.R) / 2, 
										   (v1.Color.G + v2.Color.G) / 2,
										   (v1.Color.B + v2.Color.B) / 2));
			vals.Insert(selLeft + 1, v);

			ChangeSel(selLeft + 1);

			RedrawGradient();
		}

        private void firstButton_Click(object sender, EventArgs e)
        {
			ChangeSel(0);
        }

        private void lastButton_Click(object sender, EventArgs e)
        {
			ChangeSel(vals.Count - 1);
        }

        private void prevButton_Click(object sender, EventArgs e)
        {
			ChangeSel(sel - 1);
        }

        private void nextButton_Click(object sender, EventArgs e)
        {
			ChangeSel(sel + 1);
        }

        private void positionNud_ValueChanged(object sender, EventArgs e)
        {
			ChangePosition((int)positionNud.Value);
        }

        private void alphaNud_ValueChanged(object sender, EventArgs e)
        {
			ChangeOpacity((int)alphaNud.Value);
        }

        private void colorButton_Click(object sender, EventArgs e)
        {
            Zeroit.Framework.ListView.Editors.ComboColorPickerDialog d = new Zeroit.Framework.ListView.Editors.ComboColorPickerDialog(colorLabel.BackColor, colorButton);
			if (d.ShowDialog() == DialogResult.OK)
			{
				ChangeColor(d.Color);
			}
        }

        private void gradientPanel_MouseDown(object sender, MouseEventArgs e)
        {
			if (selMarker != null && selMarker.IsVisible(e.Location))
			{
				selMouse = true;
			}
			else
			{
				// Is mouse down inside a non-sel marker?
				for (int s = vals.Count - 1; s >= 0; s--)
				{
					if (s != sel && markers[s] != null && markers[s].IsVisible(e.Location))
					{
						ChangeSel(s);
						selMouse = true;
						break;
					}
				}
			}

			if (selMouse)
			{
				selMouseX = e.X;
				selMousePos = vals[sel].Position;
			}
        }

        private void gradientPanel_MouseMove(object sender, MouseEventArgs e)
        {
			if (selMouse)
			{
				int dx = e.X - selMouseX;
				int newPos = Math.Max(Math.Min(maxPos, selMousePos + DxToDpos(dx)), minPos);
				if (newPos != vals[sel].Position)
				{
					ChangePosition(newPos);
				}
			}
        }

        private void gradientPanel_MouseUp(object sender, MouseEventArgs e)
        {
			selMouse = false;
        }

		private void RedrawGradient()
		{
			gradientPanel.Invalidate(true);
		}

		private Brush borderBrush = null;
		private HatchBrush backGradientBrush = null;

		private LinearGradientBrush2 gradientBrush = null;
		private Rectangle gradientRect = Rectangle.Empty;

		private Brush scaleFontBrush = null;
		private Pen markerBorderPen = null;
		private Brush markerFillBrush = null;
		private Brush selMarkerFillBrush = null;

		private void ClearBorderBrush()
		{
			if (borderBrush != null)
			{
				borderBrush.Dispose();
				borderBrush = null;
			}
		}

		private void ClearBackGradientBrush()
		{
			if (backGradientBrush != null)
			{
				backGradientBrush.Dispose();
				backGradientBrush = null;
			}
		}

		private void ClearGradientBrush()
		{
			if (gradientBrush != null)
			{
				gradientBrush.Dispose();
				gradientBrush = null;
			}
		}

		private void ClearMarkerBorderPen()
		{
			if (markerBorderPen != null)
			{
				markerBorderPen.Dispose();
				markerBorderPen = null;
			}
		}

		private void ClearMarkerFillBrush()
		{
			if (markerFillBrush != null)
			{
				markerFillBrush.Dispose();
				markerFillBrush = null;
			}
		}

		private void ClearSelMarkerFillBrush()
		{
			if (selMarkerFillBrush != null)
			{
				selMarkerFillBrush.Dispose();
				selMarkerFillBrush = null;
			}
		}

		private void AllocPensAndBrushes()
		{
			if (borderBrush == null)
			{
				borderBrush = new SolidBrush(Color.Black /*gradientBorderColor*/);
			}
			if (backGradientBrush == null)
			{
				backGradientBrush = new HatchBrush(HatchStyle.DiagonalCross, gradientHatchColor, gradientBackColor);
			}
			if (gradientBrush == null)
			{
				gradientBrush = new LinearGradientBrush2(gradientRect, Blend, LinearGradientMode.Horizontal);
			}
			if (markerBorderPen == null)
			{
				markerBorderPen = new Pen(markerBorderColor, markerBorderSize);
			}
			if (markerFillBrush == null)
			{
				markerFillBrush = new SolidBrush(markerFillColor);
			}
			if (selMarkerFillBrush == null)
			{
				selMarkerFillBrush = new SolidBrush(selMarkerFillColor);
			}
		}

		private void ClearScaleFontBrush()
		{
			if (scaleFontBrush != null)
			{
				scaleFontBrush.Dispose();
				scaleFontBrush = null;
			}
		}

		private void ClearMarkers()
		{
			if (markers != null)
			{
                for (int s = 0; s < markers.Length; s++)
                {
                    ClearMarker(s);
                }
				markers = null;
			}
			ClearSelMarker();
		}

		private void ClearMarker(int s)
		{
			if (markers != null && markers[s] != null)
			{
				markers[s].Dispose();
				markers[s] = null;
			}
		}

		private void ClearSelMarker()
		{
			if (selMarker != null)
			{
				selMarker.Dispose();
				selMarker = null;
			}
		}

		private int PosToX(int pos)
		{
			return gradientRect.Left + (int)Math.Round((double)(pos * (gradientRect.Width - 1)) * 0.01);
		}

		private int DxToDpos(int dx)
		{
			return (int)Math.Round((100.0 * dx)/(gradientRect.Width - 1));
		}

		private GraphicsPath AllocMarker(int pos)
		{
			int h2 = markerHeight / 2;
			int w2 = markerHeight / 2;

			int x = PosToX(pos);
			Point[] pts = new Point[] { new Point(x,      yMarker               ),
										new Point(x + w2, yMarker + h2          ),
										new Point(x,      yMarker + markerHeight),
										new Point(x - w2, yMarker + h2          ),
										new Point(x,      yMarker               ) };

			GraphicsPath p = new GraphicsPath();
			p.AddLines(pts);
			return p;
		}

		private GraphicsPath AllocSelMarker(int pos)
		{
			int w2 = selMarkerWidth / 2;

			int x = PosToX(pos);
            Point[] pts = new Point[] { new Point(x,          yMarker                  ),
										new Point(x + w2,     yMarker + selMarkerHeight),
										new Point(x - w2,     yMarker + selMarkerHeight),
										new Point(x,          yMarker                  ) };

			GraphicsPath p = new GraphicsPath();
			p.AddLines(pts);
			return p;
		}

		private void DrawMarker(GraphicsPath p)
		{
			g.FillPath(markerFillBrush, p);
			g.DrawPath(markerBorderPen, p);
		}

		private void DrawSelMarker(GraphicsPath p)
		{
			g.FillPath(selMarkerFillBrush, p);
			g.DrawPath(markerBorderPen, p);
		}

		private const int markerHeight = 10;
		private const int markerWidth = 10;
		private const int markerBorderSize = 1;
		private const int selMarkerHeight = 14;
		private const int selMarkerWidth = 12;

		private const float scalePointSize = 8.0f;

		private Graphics g;
		private int yMarker;

		private GraphicsPath[] markers = null;
		private GraphicsPath selMarker = null;

        private void gradientPanel_Paint(object sender, PaintEventArgs e)
        {
			Size cs = gradientPanel.ClientSize;

			int xpad = Math.Max(Math.Max(markerWidth / 2, selMarkerWidth / 2), gradientBorderSize);
			int ytop = gradientBorderSize;
			int dx = cs.Width - 2 * xpad;
			int dy = cs.Height - 2 * gradientBorderSize - 3 - Math.Max(markerHeight, selMarkerHeight) - 2;

			if (dx < 1 || dy < 1)
			{
				return; // too small to draw anything
			}

			g = e.Graphics;
			Rectangle gr = new Rectangle(xpad, ytop, dx, dy);
			if (gr != gradientRect)
			{
				ClearGradientBrush();
				gradientRect = gr;
			}
			yMarker = ytop + dy + gradientBorderSize + 3;

			AllocPensAndBrushes();

			// Fill background + border with border color
			if (gradientBorderSize > 0)
			{
				Rectangle r = Rectangle.Inflate(gr, gradientBorderSize, gradientBorderSize);
				g.FillRectangle(borderBrush, r);
			}

			// Draw gradient
			gradientBrush.FillRectangle(g, gr, backGradientBrush);

			// Draw markers
			if (markers == null)
			{
				markers = new GraphicsPath[vals.Count];
			}
			for (int s = 0; s < vals.Count; s++)
			{
				if (s != sel)
				{
					if (markers[s] == null)
					{
						markers[s] = AllocMarker(vals[s].Position);
					}
					DrawMarker(markers[s]);
				}
			}
			if (selMarker == null)
			{
				selMarker = AllocSelMarker(vals[sel].Position);
			}
			DrawSelMarker(selMarker);
        }

        private void newBeforeButton_MouseEnter(object sender, EventArgs e)
        {
            newBeforeButton.FlatAppearance.BorderColor = Color.FromArgb(128, 255, 255);
            newBeforeButton.FlatAppearance.BorderSize = 1;
        }

        private void Button_MouseEnter(object sender, EventArgs e)
        {
            newBeforeButton.FlatAppearance.BorderColor = Color.FromArgb(128, 255, 255);
        }

        private void Button_MouseLeave(object sender, EventArgs e)
        {
            newBeforeButton.FlatAppearance.BorderColor = Color.FromArgb(56, 56, 56);
        }

        private void newBeforeButton_MouseLeave(object sender, EventArgs e)
        {
            newBeforeButton.FlatAppearance.BorderColor = Color.FromArgb(56, 56, 56);
            newBeforeButton.FlatAppearance.BorderSize = 0;
        }

        private void newBeforeButton_MouseEnter_1(object sender, EventArgs e)
        {
            newBeforeButton.FlatAppearance.BorderColor = Color.FromArgb(128, 255, 255);
            newBeforeButton.FlatAppearance.BorderSize = 1;
        }

        private void newBeforeButton_MouseLeave_1(object sender, EventArgs e)
        {
            newBeforeButton.FlatAppearance.BorderColor = Color.FromArgb(56, 56, 56);
            newBeforeButton.FlatAppearance.BorderSize = 1;
        }

        private void newAfterButton_MouseEnter(object sender, EventArgs e)
        {
            newAfterButton.FlatAppearance.BorderColor = Color.FromArgb(128, 255, 255);
            newAfterButton.FlatAppearance.BorderSize = 1;
        }

        private void newAfterButton_MouseLeave(object sender, EventArgs e)
        {
            newAfterButton.FlatAppearance.BorderColor = Color.FromArgb(56, 56, 56);
            //newAfterButton.FlatAppearance.BorderSize = 0;
        }

        private void delButton_MouseEnter(object sender, EventArgs e)
        {
            delButton.FlatAppearance.BorderColor = Color.FromArgb(128, 255, 255);
            delButton.FlatAppearance.BorderSize = 1;
        }

        private void delButton_MouseLeave(object sender, EventArgs e)
        {
            delButton.FlatAppearance.BorderColor = Color.FromArgb(56, 56, 56);
        }

        private void firstButton_MouseEnter(object sender, EventArgs e)
        {
            firstButton.FlatAppearance.BorderColor = Color.FromArgb(128, 255, 255);
            firstButton.FlatAppearance.BorderSize = 1;
        }

        private void firstButton_MouseLeave(object sender, EventArgs e)
        {
            firstButton.FlatAppearance.BorderColor = Color.FromArgb(56, 56, 56);
        }

        private void prevButton_MouseEnter(object sender, EventArgs e)
        {
            prevButton.FlatAppearance.BorderColor = Color.FromArgb(128, 255, 255);
            prevButton.FlatAppearance.BorderSize = 1;
        }

        private void prevButton_MouseLeave(object sender, EventArgs e)
        {
            prevButton.FlatAppearance.BorderColor = Color.FromArgb(56, 56, 56);
        }

        private void nextButton_MouseEnter(object sender, EventArgs e)
        {
            nextButton.FlatAppearance.BorderColor = Color.FromArgb(128, 255, 255);
            nextButton.FlatAppearance.BorderSize = 1;
        }

        private void nextButton_MouseLeave(object sender, EventArgs e)
        {
            nextButton.FlatAppearance.BorderColor = Color.FromArgb(56, 56, 56);
        }

        private void lastButton_MouseEnter(object sender, EventArgs e)
        {
            lastButton.FlatAppearance.BorderColor = Color.FromArgb(128, 255, 255);
            lastButton.FlatAppearance.BorderSize = 1;
        }

        private void lastButton_MouseLeave(object sender, EventArgs e)
        {
            lastButton.FlatAppearance.BorderColor = Color.FromArgb(56, 56, 56);
        }
    }
}


