// ***********************************************************************
// Assembly         : Zeroit.Framework.ListView
// Author           : ZEROIT
// Created          : 06-30-2019
//
// Last Modified By : ZEROIT
// Last Modified On : 08-01-2019
// ***********************************************************************
// <copyright file="SimpleListView.cs" company="Zeroit Dev Technologies">
//     Copyright © 2019 Zeroit Dev Technologies. All Rights Reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Globalization;
using System.Windows.Forms;

namespace Zeroit.Framework.ListView
{
    /// <summary>
    /// Class SimpleListView.
    /// </summary>
    /// <seealso cref="System.Windows.Forms.ListView" />
    [ToolboxItem(true)]
    public class SimpleListView : System.Windows.Forms.ListView
    {

        #region Enums

        public enum drawMode
        {
            Default,
            Stylish
        }

        public enum headerAlignment
        {
            Left,
            Center,
            Right
        }


        #endregion

        #region Private Fields        

        private Color[] unfocusedfill = new Color[] { Color.AliceBlue, Color.Lime };

        private Color[] focusedFill = new Color[] { Color.AliceBlue, Color.Lime };

        private Color[] columnHeader = new Color[] { Color.AliceBlue, Color.Lime };
        
        private Color headerBorder = Color.White;

        private Color cellBorderFocused = Color.White;

        private Color cellBorderUnFocused = Color.White;
                
        private Color subTextColor = Color.Red;

        private Color headerColor = Color.Black;

        private Color lineColor = Color.Yellow;

        private NumberStyles numberStyles = NumberStyles.Currency;

        private Font headerFont = new Font("Helvetica", 10, FontStyle.Bold);

        private bool hideHeader = false;

        private bool showBorder = false; 

        private bool showCellBorder = false;

        private bool surroundBorder = false;

        private bool showHeaderLine = false;

        private bool rounding = false;

        private int lineHeight = 1;
                
        private float radius = 1;

        private float[] gradientAngle = { 90 , 90};

        private headerAlignment _headerAlignment = headerAlignment.Center;

        private drawMode _drawMode = drawMode.Stylish;

        private SmoothingMode smoothing = SmoothingMode.HighQuality;

        private TextRenderingHint textrendering = TextRenderingHint.AntiAlias;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="SimpleListView"/> is rounding.
        /// </summary>
        /// <value><c>true</c> if rounding; otherwise, <c>false</c>.</value>
        public bool Rounding
        {
            get { return rounding; }
            set
            {
                rounding = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the radius.
        /// </summary>
        /// <value>The radius.</value>
        public float Radius
        {
            get { return radius; }
            set
            {
                radius = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [hide header].
        /// </summary>
        /// <value><c>true</c> if [hide header]; otherwise, <c>false</c>.</value>
        public bool HideHeader
        {
            get { return hideHeader; }
            set
            {
                hideHeader = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the draw mode.
        /// </summary>
        /// <value>The draw mode.</value>
        public drawMode DrawMode
        {
            get { return _drawMode; }
            set
            {
                switch (value)
                {
                    case drawMode.Default:
                        OwnerDraw = false;
                        break;
                    case drawMode.Stylish:
                        // Configure the ListView control for owner-draw and add 
                        // handlers for the owner-draw events.
                        OwnerDraw = true;
                        break;

                }
                _drawMode = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the header alignment.
        /// </summary>
        /// <value>The header alignment.</value>
        public headerAlignment HeaderAlignment
        {
            get { return _headerAlignment; }
            set
            {
                _headerAlignment = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the fill unfocused.
        /// </summary>
        /// <value>The fill unfocused.</value>
        public Color[] FillUnfocused
        {
            get { return unfocusedfill; }
            set
            {
                unfocusedfill = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the fill focused.
        /// </summary>
        /// <value>The fill focused.</value>
        public Color[] FillFocused
        {
            get { return focusedFill; }
            set
            {
                focusedFill = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the column header.
        /// </summary>
        /// <value>The column header.</value>
        public Color[] ColumnHeader
        {
            get { return columnHeader; }
            set
            {
                columnHeader = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the height of the line.
        /// </summary>
        /// <value>The height of the line.</value>
        public int LineHeight
        {
            get { return lineHeight; }
            set
            {
                lineHeight = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the color of the line.
        /// </summary>
        /// <value>The color of the line.</value>
        public Color LineColor
        {
            get { return lineColor; }
            set
            {
                lineColor = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the color of the sub text.
        /// </summary>
        /// <value>The color of the sub text.</value>
        public Color SubTextColor
        {
            get { return subTextColor; }
            set
            {
                subTextColor = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the color of the header.
        /// </summary>
        /// <value>The color of the header.</value>
        public Color HeaderColor
        {
            get { return headerColor; }
            set
            {
                headerColor = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the number styles.
        /// </summary>
        /// <value>The number styles.</value>
        public NumberStyles NumberStyles
        {
            get { return numberStyles; }
            set
            {
                numberStyles = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the header font.
        /// </summary>
        /// <value>The header font.</value>
        public Font HeaderFont
        {
            get { return headerFont; }
            set
            {
                headerFont = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the header border.
        /// </summary>
        /// <value>The header border.</value>
        public Color HeaderBorder
        {
            get { return headerBorder; }
            set
            {
                headerBorder = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the cell border focused.
        /// </summary>
        /// <value>The cell border focused.</value>
        public Color CellBorderFocused
        {
            get { return cellBorderFocused; }
            set
            {
                cellBorderFocused = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the cell border un focused.
        /// </summary>
        /// <value>The cell border un focused.</value>
        public Color CellBorderUnFocused
        {
            get { return cellBorderUnFocused; }
            set
            {
                cellBorderUnFocused = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show border].
        /// </summary>
        /// <value><c>true</c> if [show border]; otherwise, <c>false</c>.</value>
        public bool ShowBorder
        {
            get { return showBorder; }
            set
            {
                showBorder = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show cell border].
        /// </summary>
        /// <value><c>true</c> if [show cell border]; otherwise, <c>false</c>.</value>
        public bool ShowCellBorder
        {
            get { return showCellBorder; }
            set
            {
                showCellBorder = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [surrond border].
        /// </summary>
        /// <value><c>true</c> if [surrond border]; otherwise, <c>false</c>.</value>
        public bool SurrondBorder
        {
            get { return surroundBorder; }
            set
            {
                surroundBorder = value;
                Invalidate();
            }
        }


        /// <summary>
        /// Gets or sets the smoothing.
        /// </summary>
        /// <value>The smoothing.</value>
        public SmoothingMode Smoothing
        {
            get { return smoothing; }
            set
            {
                smoothing = value;
                Invalidate();
            }
        }


        /// <summary>
        /// Gets or sets the text rendering.
        /// </summary>
        /// <value>The text rendering.</value>
        public TextRenderingHint TextRendering
        {
            get { return textrendering; }
            set
            {
                textrendering = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show header line].
        /// </summary>
        /// <value><c>true</c> if [show header line]; otherwise, <c>false</c>.</value>
        public bool ShowHeaderLine
        {
            get { return showHeaderLine; }
            set
            {
                showHeaderLine = value;
                Invalidate();
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Creates the round rect.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="radius">The radius.</param>
        /// <returns>GraphicsPath.</returns>
        public static GraphicsPath CreateRoundRect(float x, float y, float width, float height, float radius)
        {
            GraphicsPath gp = new GraphicsPath();
            gp.AddLine(x + radius, y, x + width - (radius * 2), y);
            gp.AddArc(x + width - (radius * 2), y, radius * 2, radius * 2, 270, 90);

            gp.AddLine(x + width, y + radius, x + width, y + height - (radius * 2));
            gp.AddArc(x + width - (radius * 2), y + height - (radius * 2), radius * 2, radius * 2, 0, 90);

            gp.AddLine(x + width - (radius * 2), y + height, x + radius, y + height);
            gp.AddArc(x, y + height - (radius * 2), radius * 2, radius * 2, 90, 90);

            gp.AddLine(x, y + height - (radius * 2), x, y + radius);
            gp.AddArc(x, y, radius * 2, radius * 2, 180, 90);

            gp.CloseFigure();
            return gp;
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleListView"/> class.
        /// </summary>
        public SimpleListView()
        {

            SetStyle(ControlStyles.DoubleBuffer | ControlStyles.OptimizedDoubleBuffer, true);
            
        }

        #endregion

        #region Events and Overrides

        protected override void OnDrawItem(DrawListViewItemEventArgs e)
        {
            LinearGradientBrush fillFocusedBrush = new LinearGradientBrush(e.Bounds, FillFocused[0], FillFocused[1], gradientAngle[0]);
            LinearGradientBrush fillUnFocusedBrush = new LinearGradientBrush(e.Bounds, FillUnfocused[0], FillUnfocused[1], gradientAngle[1]);

            Pen cellBorderFocusedBrush = new Pen(CellBorderFocused);
            Pen cellBorderUnFocusedBrush = new Pen(CellBorderUnFocused);

            switch (DrawMode)
            {
                case drawMode.Default:
                    base.OnDrawItem(e);

                    break;
                case drawMode.Stylish:

                    base.OnDrawItem(e);

                    if ((e.State & ListViewItemStates.Selected) != 0)
                    {
                        // Draw the background and focus rectangle for a selected item.
                        e.Graphics.FillRectangle(fillFocusedBrush, e.Bounds);

                        if(SurrondBorder)
                        {
                            e.Graphics.DrawRectangle(cellBorderFocusedBrush, new Rectangle(e.Bounds.X + 1, e.Bounds.Y + 1, e.Bounds.Width - 2, e.Bounds.Height - 2));

                        }

                        e.DrawFocusRectangle();
                    }
                    else
                    {

                        e.Graphics.FillRectangle(fillUnFocusedBrush, e.Bounds);

                        if (SurrondBorder)
                        {
                            e.Graphics.DrawRectangle(cellBorderUnFocusedBrush, new Rectangle(e.Bounds.X + 1, e.Bounds.Y + 1, e.Bounds.Width - 2, e.Bounds.Height - 2));
                            
                        }
                    }

                    // Draw the item text for views other than the Details view.
                    if (View != View.Details)
                    {
                        e.DrawText();
                    }
                    break;

            }

        }

        protected override void OnDrawSubItem(DrawListViewSubItemEventArgs e)
        {
            Pen cellBorderFocusedBrush = new Pen(CellBorderFocused);

            switch (DrawMode)
            {
                case drawMode.Default:
                    base.OnDrawSubItem(e);

                    break;
                case drawMode.Stylish:
                    base.OnDrawSubItem(e);

                    Graphics g = e.Graphics;
                    g.SmoothingMode = Smoothing;
                    g.TextRenderingHint = TextRendering;

                    TextFormatFlags flags = TextFormatFlags.Left;

                    using (StringFormat sf = new StringFormat())
                    {
                        // Store the column text alignment, letting it default
                        // to Left if it has not been set to Center or Right.
                        switch (e.Header.TextAlign)
                        {
                            case HorizontalAlignment.Center:
                                sf.Alignment = StringAlignment.Center;
                                flags = TextFormatFlags.HorizontalCenter;
                                break;
                            case HorizontalAlignment.Right:
                                sf.Alignment = StringAlignment.Far;
                                flags = TextFormatFlags.Right;
                                break;
                        }

                        // Draw the text and background for a subitem with a 
                        // negative value. 
                        double subItemValue;
                        if (e.ColumnIndex > 0 && Double.TryParse(
                                e.SubItem.Text, NumberStyles,
                                NumberFormatInfo.CurrentInfo, out subItemValue) &&
                            subItemValue < 0)
                        {
                            // Unless the item is selected, draw the standard 
                            // background to make it stand out from the gradient.
                            if ((e.ItemState & ListViewItemStates.Selected) == 0)
                            {
                                e.DrawBackground();
                            }

                            // Draw the subitem text in red to highlight it. 
                            g.DrawString(e.SubItem.Text,
                                 Font, new SolidBrush(SubTextColor), e.Bounds, sf);

                            return;
                        }

                        if (ShowCellBorder)
                        {
                            foreach (var items in Items)
                            {
                                e.Graphics.DrawRectangle(cellBorderFocusedBrush, new Rectangle(e.Bounds.X + 1, e.Bounds.Y + 1, e.Bounds.Width - 2, e.Bounds.Height - 2));

                            }

                        }

                        // Draw normal text for a subitem with a nonnegative 
                        // or nonnumerical value.
                        e.DrawText(flags);
                    }
                    break;

            }


        }

        protected override void OnDrawColumnHeader(DrawListViewColumnHeaderEventArgs e)
        {
            LinearGradientBrush ColumnHeaderBrush = new LinearGradientBrush(e.Bounds, ColumnHeader[0], ColumnHeader[1], gradientAngle[0]);

            Pen HeaderBorderBrush = new Pen(CellBorderFocused);
            
            switch (DrawMode)
            {
                case drawMode.Default:

                    base.OnDrawColumnHeader(e);

                    break;
                case drawMode.Stylish:

                    base.OnDrawColumnHeader(e);
                    
                    if (!HideHeader)
                    {
                        int internalWidth = e.Bounds.Width - (2 * (int)HeaderBorderBrush.Width);
                        int internalHeight = (e.Bounds.Height - (2 * (int)HeaderBorderBrush.Width));

                        Graphics g = e.Graphics;
                        GraphicsPath BG = CreateRoundRect(e.Bounds.X + (int)HeaderBorderBrush.Width, e.Bounds.Y + (int)HeaderBorderBrush.Width, internalWidth - 3, internalHeight - 3, radius);
                        g.SmoothingMode = Smoothing;
                        g.TextRenderingHint = TextRendering;

                        SizeF fs = g.MeasureString(e.Header.Text, HeaderFont, e.Bounds.Width).ToSize();

                        using (StringFormat sf = new StringFormat())
                        {

                            // Store the column text alignment, letting it default
                            // to Left if it has not been set to Center or Right.
                            switch (e.Header.TextAlign)
                            {
                                case HorizontalAlignment.Center:
                                    sf.Alignment = StringAlignment.Center;
                                    break;
                                case HorizontalAlignment.Right:
                                    sf.Alignment = StringAlignment.Far;
                                    break;
                            }

                            
                            if(rounding)
                            {
                                if (ShowBorder)
                                {
                                    g.FillPath(new LinearGradientBrush(new Rectangle(e.Bounds.X + (int)HeaderBorderBrush.Width, e.Bounds.Y + (int)HeaderBorderBrush.Width, internalWidth - 3, internalHeight - 3), ColumnHeader[0], ColumnHeader[1], gradientAngle[0]), BG);

                                    g.DrawPath(HeaderBorderBrush, BG);
                                    //g.DrawRectangle(HeaderBorder.GetPen(), new Rectangle(e.Bounds.X + (int)HeaderBorder.GetPen().Width, e.Bounds.Y + (int)HeaderBorder.GetPen().Width, e.Bounds.Width - (2 * (int)HeaderBorder.GetPen().Width), e.Bounds.Height - (2 * (int)HeaderBorder.GetPen().Width)));

                                }
                                else
                                {
                                    BG = CreateRoundRect(e.Bounds.X, e.Bounds.Y, internalWidth, internalHeight, radius);

                                    g.FillPath(new LinearGradientBrush(new Rectangle(e.Bounds.X, e.Bounds.Y, internalWidth, internalHeight), ColumnHeader[0], ColumnHeader[1], gradientAngle[0]), BG);

                                }
                            }
                            else
                            {
                                g.FillRectangle(ColumnHeaderBrush, e.Bounds);

                                if (ShowBorder)
                                {
                                    g.DrawRectangle(HeaderBorderBrush, new Rectangle(e.Bounds.X + (int)HeaderBorderBrush.Width, e.Bounds.Y + (int)HeaderBorderBrush.Width, e.Bounds.Width - (2 * (int)HeaderBorderBrush.Width), e.Bounds.Height - (2 * (int)HeaderBorderBrush.Width)));

                                }
                                
                            }
                            
                            if (ShowHeaderLine)
                            {
                                foreach (ColumnHeader items in Columns)
                                {
                                    //g.DrawRectangle(new Pen(LineColor,lineHeight), new Rectangle(e.Bounds.X, e.Bounds.Y + (int)fs.Height + (int)(fs.Height / 4), items.Width, LineHeight));

                                    g.DrawLine(new Pen(LineColor, lineHeight), new Point(e.Bounds.X, e.Bounds.Y + (int)fs.Height /*+ (int)(fs.Height / 10)*/), new Point(e.Bounds.X + e.Bounds.Width, e.Bounds.Y + (int)fs.Height /*+ (int)(fs.Height / 10)*/));

                                }
                            }
                            
                            switch (HeaderAlignment)
                            {
                                case headerAlignment.Left:
                                    g.DrawString(e.Header.Text, HeaderFont,
                                        new SolidBrush(HeaderColor), e.Bounds, sf);
                                    break;
                                case headerAlignment.Center:
                                    foreach (ColumnHeader items in Columns)
                                    {
                                        g.DrawString(e.Header.Text, HeaderFont,
                                            new SolidBrush(HeaderColor), new Rectangle(e.Bounds.X + (e.Bounds.Width / 2 - (int)(fs.Width / 2)), e.Bounds.Y, e.Bounds.Width, e.Bounds.Height), sf);
                                    }

                                    break;
                                case headerAlignment.Right:
                                    foreach (ColumnHeader items in Columns)
                                    {

                                        g.DrawString(e.Header.Text, HeaderFont,
                                            new SolidBrush(HeaderColor), new Rectangle(e.Bounds.X + (e.Bounds.Width - (int)fs.Width), e.Bounds.Y, e.Bounds.Width, e.Bounds.Height), sf);
                                    }
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }

                        }
                                               

                    }

                    if (!DesignMode)
                    {
                        GC.Collect();
                    }

                    return;                   

            }

        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            switch (DrawMode)
            {
                case drawMode.Default:
                    base.OnMouseMove(e);

                    break;
                case drawMode.Stylish:
                    base.OnMouseMove(e);

                    ListViewItem item = GetItemAt(e.X, e.Y);
                    if (item != null && item.Tag == null)
                    {
                        Invalidate(item.Bounds);
                        item.Tag = "tagged";
                    }

                    break;

            }


        }

        protected override void OnInvalidated(InvalidateEventArgs e)
        {
            switch (DrawMode)
            {
                case drawMode.Default:
                    base.OnInvalidated(e);

                    break;
                case drawMode.Stylish:
                    base.OnInvalidated(e);

                    foreach (ListViewItem item in Items)
                    {
                        if (item == null) return;
                        item.Tag = null;
                    }

                    break;

            }


        }

        protected override void OnColumnWidthChanged(ColumnWidthChangedEventArgs e)
        {
            switch (DrawMode)
            {
                case drawMode.Default:
                    base.OnColumnWidthChanged(e);

                    break;
                case drawMode.Stylish:
                    base.OnColumnWidthChanged(e);

                    Invalidate();
                    break;

            }


        }


        #endregion

    }
}
