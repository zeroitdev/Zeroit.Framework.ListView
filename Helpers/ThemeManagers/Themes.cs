using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.ComponentModel;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;

namespace Zeroit.Framework.ListView.ThemeManagers
{


    #region Theme Base


    //------------------
    //Creator: aeonhack
    //Site: elitevs.net
    //Created: 08/02/2011
    //Changed: 12/06/2011
    //Version: 1.5.4
    //------------------

    //---------------ThemeContainer154
    public abstract class ThemeContainer : ContainerControl
    {

        #region " Initialization "

        protected Graphics G;

        protected Bitmap B;
        public ThemeContainer()
        {
            SetStyle((ControlStyles)139270, true);

            _ImageSize = Size.Empty;
            Font = new Font("Verdana", 8);

            MeasureBitmap = new Bitmap(1, 1);
            MeasureGraphics = Graphics.FromImage(MeasureBitmap);

            DrawRadialPath = new GraphicsPath();

            InvalidateCustimization();
        }

        protected override sealed void OnHandleCreated(EventArgs e)
        {
            if (DoneCreation)
                InitializeMessages();

            InvalidateCustimization();
            ColorHook();

            if (!(_LockWidth == 0))
                Width = _LockWidth;
            if (!(_LockHeight == 0))
                Height = _LockHeight;
            if (!_ControlMode)
                base.Dock = DockStyle.Fill;

            Transparent = _Transparent;
            if (_Transparent && _BackColor)
                BackColor = Color.Transparent;

            base.OnHandleCreated(e);
        }

        private bool DoneCreation;
        protected override sealed void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);

            if (Parent == null)
                return;
            _IsParentForm = Parent is Form;

            if (!_ControlMode)
            {
                InitializeMessages();

                if (_IsParentForm)
                {
                    ParentForm.FormBorderStyle = _BorderStyle;
                    ParentForm.TransparencyKey = _TransparencyKey;

                    if (!DesignMode)
                    {
                        ParentForm.Shown += FormShown;
                    }
                }

                Parent.BackColor = BackColor;
            }

            OnCreation();
            DoneCreation = true;
            InvalidateTimer();
        }

        #endregion

        private void DoAnimation(bool i)
        {
            OnAnimation();
            if (i)
                Invalidate();
        }

        protected override sealed void OnPaint(PaintEventArgs e)
        {
            if (Width == 0 || Height == 0)
                return;

            if (_Transparent && _ControlMode)
            {
                PaintHook(e);
                e.Graphics.DrawImage(B, 0, 0);
            }
            else
            {
                G = e.Graphics;
                PaintHook(e);
            }
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            ThemeShare.RemoveAnimationCallback(DoAnimation);
            base.OnHandleDestroyed(e);
        }

        private bool HasShown;
        private void FormShown(object sender, EventArgs e)
        {
            if (_ControlMode || HasShown)
                return;

            if (_StartPosition == FormStartPosition.CenterParent || _StartPosition == FormStartPosition.CenterScreen)
            {
                Rectangle SB = Screen.PrimaryScreen.Bounds;
                Rectangle CB = ParentForm.Bounds;
                ParentForm.Location = new Point(SB.Width / 2 - CB.Width / 2, SB.Height / 2 - CB.Width / 2);
            }

            HasShown = true;
        }


        #region " Size Handling "

        private Rectangle Frame;
        protected override sealed void OnSizeChanged(EventArgs e)
        {
            if (_Movable && !_ControlMode)
            {
                Frame = new Rectangle(7, 7, Width - 14, _Header - 7);
            }

            InvalidateBitmap();
            Invalidate();

            base.OnSizeChanged(e);
        }

        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            if (!(_LockWidth == 0))
                width = _LockWidth;
            if (!(_LockHeight == 0))
                height = _LockHeight;
            base.SetBoundsCore(x, y, width, height, specified);
        }

        #endregion

        #region " State Handling "

        protected MouseState State;
        private void SetState(MouseState current)
        {
            State = current;
            Invalidate();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (!(_IsParentForm && ParentForm.WindowState == FormWindowState.Maximized))
            {
                if (_Sizable && !_ControlMode)
                    InvalidateMouse();
            }

            base.OnMouseMove(e);
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            if (Enabled)
                SetState(MouseState.None);
            else
                SetState(MouseState.Block);
            base.OnEnabledChanged(e);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            SetState(MouseState.Over);
            base.OnMouseEnter(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            SetState(MouseState.Over);
            base.OnMouseUp(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            SetState(MouseState.None);

            if (GetChildAtPoint(PointToClient(MousePosition)) != null)
            {
                if (_Sizable && !_ControlMode)
                {
                    Cursor = Cursors.Default;
                    Previous = 0;
                }
            }

            base.OnMouseLeave(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
                SetState(MouseState.Down);

            if (!(_IsParentForm && ParentForm.WindowState == FormWindowState.Maximized || _ControlMode))
            {
                if (_Movable && Frame.Contains(e.Location))
                {
                    Capture = false;
                    WM_LMBUTTONDOWN = true;
                    DefWndProc(ref Messages[0]);
                }
                else if (_Sizable && !(Previous == 0))
                {
                    Capture = false;
                    WM_LMBUTTONDOWN = true;
                    DefWndProc(ref Messages[Previous]);
                }
            }

            base.OnMouseDown(e);
        }

        private bool WM_LMBUTTONDOWN;
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (WM_LMBUTTONDOWN && m.Msg == 513)
            {
                WM_LMBUTTONDOWN = false;

                SetState(MouseState.Over);
                if (!_SmartBounds)
                    return;

                if (IsParentMdi)
                {
                    CorrectBounds(new Rectangle(Point.Empty, Parent.Parent.Size));
                }
                else
                {
                    CorrectBounds(Screen.FromControl(Parent).WorkingArea);
                }
            }
        }

        private Point GetIndexPoint;
        private bool B1;
        private bool B2;
        private bool B3;
        private bool B4;
        private int GetIndex()
        {
            GetIndexPoint = PointToClient(MousePosition);
            B1 = GetIndexPoint.X < 7;
            B2 = GetIndexPoint.X > Width - 7;
            B3 = GetIndexPoint.Y < 7;
            B4 = GetIndexPoint.Y > Height - 7;

            if (B1 && B3)
                return 4;
            if (B1 && B4)
                return 7;
            if (B2 && B3)
                return 5;
            if (B2 && B4)
                return 8;
            if (B1)
                return 1;
            if (B2)
                return 2;
            if (B3)
                return 3;
            if (B4)
                return 6;
            return 0;
        }

        private int Current;
        private int Previous;
        private void InvalidateMouse()
        {
            Current = GetIndex();
            if (Current == Previous)
                return;

            Previous = Current;
            switch (Previous)
            {
                case 0:
                    Cursor = Cursors.Default;
                    break;
                case 1:
                case 2:
                    Cursor = Cursors.SizeWE;
                    break;
                case 3:
                case 6:
                    Cursor = Cursors.SizeNS;
                    break;
                case 4:
                case 8:
                    Cursor = Cursors.SizeNWSE;
                    break;
                case 5:
                case 7:
                    Cursor = Cursors.SizeNESW;
                    break;
            }
        }

        private Message[] Messages = new Message[9];
        private void InitializeMessages()
        {
            Messages[0] = Message.Create(Parent.Handle, 161, new IntPtr(2), IntPtr.Zero);
            for (int I = 1; I <= 8; I++)
            {
                Messages[I] = Message.Create(Parent.Handle, 161, new IntPtr(I + 9), IntPtr.Zero);
            }
        }

        private void CorrectBounds(Rectangle bounds)
        {
            if (Parent.Width > bounds.Width)
                Parent.Width = bounds.Width;
            if (Parent.Height > bounds.Height)
                Parent.Height = bounds.Height;

            int X = Parent.Location.X;
            int Y = Parent.Location.Y;

            if (X < bounds.X)
                X = bounds.X;
            if (Y < bounds.Y)
                Y = bounds.Y;

            int Width = bounds.X + bounds.Width;
            int Height = bounds.Y + bounds.Height;

            if (X + Parent.Width > Width)
                X = Width - Parent.Width;
            if (Y + Parent.Height > Height)
                Y = Height - Parent.Height;

            Parent.Location = new Point(X, Y);
        }

        #endregion
        
        #region " Base Properties "

        public override DockStyle Dock
        {
            get { return base.Dock; }
            set
            {
                if (!_ControlMode)
                    return;
                base.Dock = value;
            }
        }

        private bool _BackColor;
        [Category("Misc")]
        public override Color BackColor
        {
            get { return base.BackColor; }
            set
            {
                if (value == base.BackColor)
                    return;

                if (!IsHandleCreated && _ControlMode && value == Color.Transparent)
                {
                    _BackColor = true;
                    return;
                }

                base.BackColor = value;
                if (Parent != null)
                {
                    if (!_ControlMode)
                        Parent.BackColor = value;
                    ColorHook();
                }
            }
        }

        public override Size MinimumSize
        {
            get { return base.MinimumSize; }
            set
            {
                base.MinimumSize = value;
                if (Parent != null)
                    Parent.MinimumSize = value;
            }
        }

        public override Size MaximumSize
        {
            get { return base.MaximumSize; }
            set
            {
                base.MaximumSize = value;
                if (Parent != null)
                    Parent.MaximumSize = value;
            }
        }

        public override string Text
        {
            get { return base.Text; }
            set
            {
                base.Text = value;
                Invalidate();
            }
        }

        public override Font Font
        {
            get { return base.Font; }
            set
            {
                base.Font = value;
                Invalidate();
            }
        }

        //[Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        //public override Color ForeColor
        //{
        //    get { return Color.Empty; }
        //    set { }
        //}
        //[Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        //public override Image BackgroundImage
        //{
        //    get { return null; }
        //    set { }
        //}
        //[Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        //public override ImageLayout BackgroundImageLayout
        //{
        //    get { return ImageLayout.None; }
        //    set { }
        //}

        #endregion

        #region " Public Properties "

        private bool _SmartBounds = true;
        public bool SmartBounds
        {
            get { return _SmartBounds; }
            set { _SmartBounds = value; }
        }

        private bool _Movable = true;
        public bool Movable
        {
            get { return _Movable; }
            set { _Movable = value; }
        }

        private bool _Sizable = true;
        public bool Sizable
        {
            get { return _Sizable; }
            set { _Sizable = value; }
        }

        private Color _TransparencyKey;
        public Color TransparencyKey
        {
            get
            {
                if (_IsParentForm && !_ControlMode)
                    return ParentForm.TransparencyKey;
                else
                    return _TransparencyKey;
            }
            set
            {
                if (value == _TransparencyKey)
                    return;
                _TransparencyKey = value;

                if (_IsParentForm && !_ControlMode)
                {
                    ParentForm.TransparencyKey = value;
                    ColorHook();
                }
            }
        }

        private FormBorderStyle _BorderStyle;
        public FormBorderStyle BorderStyle
        {
            get
            {
                if (_IsParentForm && !_ControlMode)
                    return ParentForm.FormBorderStyle;
                else
                    return _BorderStyle;
            }
            set
            {
                _BorderStyle = value;

                if (_IsParentForm && !_ControlMode)
                {
                    ParentForm.FormBorderStyle = value;

                    if (!(value == FormBorderStyle.None))
                    {
                        Movable = false;
                        Sizable = false;
                    }
                }
            }
        }

        private FormStartPosition _StartPosition;
        public FormStartPosition StartPosition
        {
            get
            {
                if (_IsParentForm && !_ControlMode)
                    return ParentForm.StartPosition;
                else
                    return _StartPosition;
            }
            set
            {
                _StartPosition = value;

                if (_IsParentForm && !_ControlMode)
                {
                    ParentForm.StartPosition = value;
                }
            }
        }

        private bool _NoRounding;
        public bool NoRounding
        {
            get { return _NoRounding; }
            set
            {
                _NoRounding = value;
                Invalidate();
            }
        }

        private Image _Image;
        public Image Image
        {
            get { return _Image; }
            set
            {
                if (value == null)
                    _ImageSize = Size.Empty;
                else
                    _ImageSize = value.Size;

                _Image = value;
                Invalidate();
            }
        }

        private Dictionary<string, Color> Items = new Dictionary<string, Color>();
        public Bloom[] Colors
        {
            get
            {
                List<Bloom> T = new List<Bloom>();
                Dictionary<string, Color>.Enumerator E = Items.GetEnumerator();

                while (E.MoveNext())
                {
                    T.Add(new Bloom(E.Current.Key, E.Current.Value));
                }

                return T.ToArray();
            }
            set
            {
                foreach (Bloom B in value)
                {
                    if (Items.ContainsKey(B.Name))
                        Items[B.Name] = B.Value;
                }

                InvalidateCustimization();
                ColorHook();
                Invalidate();
            }
        }

        private string _Customization;
        public string Customization
        {
            get { return _Customization; }
            set
            {
                if (value == _Customization)
                    return;

                byte[] Data = null;
                Bloom[] Items = Colors;

                try
                {
                    Data = Convert.FromBase64String(value);
                    for (int I = 0; I <= Items.Length - 1; I++)
                    {
                        Items[I].Value = Color.FromArgb(BitConverter.ToInt32(Data, I * 4));
                    }
                }
                catch
                {
                    return;
                }

                _Customization = value;

                Colors = Items;
                ColorHook();
                Invalidate();
            }
        }

        private bool _Transparent;
        public bool Transparent
        {
            get { return _Transparent; }
            set
            {
                _Transparent = value;
                if (!(IsHandleCreated || _ControlMode))
                    return;

                if (!value && !(BackColor.A == 255))
                {
                    throw new Exception("Unable to change value to false while a transparent BackColor is in use.");
                }

                SetStyle(ControlStyles.Opaque, !value);
                SetStyle(ControlStyles.SupportsTransparentBackColor, value);

                InvalidateBitmap();
                Invalidate();
            }
        }

        #endregion

        #region " Private Properties "

        private Size _ImageSize;
        protected Size ImageSize
        {
            get { return _ImageSize; }
        }

        private bool _IsParentForm;
        protected bool IsParentForm
        {
            get { return _IsParentForm; }
        }

        protected bool IsParentMdi
        {
            get
            {
                if (Parent == null)
                    return false;
                return Parent.Parent != null;
            }
        }

        private int _LockWidth;
        protected int LockWidth
        {
            get { return _LockWidth; }
            set
            {
                _LockWidth = value;
                if (!(LockWidth == 0) && IsHandleCreated)
                    Width = LockWidth;
            }
        }

        private int _LockHeight;
        protected int LockHeight
        {
            get { return _LockHeight; }
            set
            {
                _LockHeight = value;
                if (!(LockHeight == 0) && IsHandleCreated)
                    Height = LockHeight;
            }
        }

        private int _Header = 24;
        protected int Header
        {
            get { return _Header; }
            set
            {
                _Header = value;

                if (!_ControlMode)
                {
                    Frame = new Rectangle(7, 7, Width - 14, value - 7);
                    Invalidate();
                }
            }
        }

        private bool _ControlMode;
        protected bool ControlMode
        {
            get { return _ControlMode; }
            set
            {
                _ControlMode = value;

                Transparent = _Transparent;
                if (_Transparent && _BackColor)
                    BackColor = Color.Transparent;

                InvalidateBitmap();
                Invalidate();
            }
        }

        private bool _IsAnimated;
        protected bool IsAnimated
        {
            get { return _IsAnimated; }
            set
            {
                _IsAnimated = value;
                InvalidateTimer();
            }
        }

        #endregion
        
        #region " Property Helpers "

        protected Pen GetPen(string name)
        {
            return new Pen(Items[name]);
        }
        protected Pen GetPen(string name, float width)
        {
            return new Pen(Items[name], width);
        }

        protected SolidBrush GetBrush(string name)
        {
            return new SolidBrush(Items[name]);
        }

        protected Color GetColor(string name)
        {
            return Items[name];
        }

        protected void SetColor(string name, Color value)
        {
            if (Items.ContainsKey(name))
                Items[name] = value;
            else
                Items.Add(name, value);
        }
        protected void SetColor(string name, byte r, byte g, byte b)
        {
            SetColor(name, Color.FromArgb(r, g, b));
        }
        protected void SetColor(string name, byte a, byte r, byte g, byte b)
        {
            SetColor(name, Color.FromArgb(a, r, g, b));
        }
        protected void SetColor(string name, byte a, Color value)
        {
            SetColor(name, Color.FromArgb(a, value));
        }

        private void InvalidateBitmap()
        {
            if (_Transparent && _ControlMode)
            {
                if (Width == 0 || Height == 0)
                    return;
                B = new Bitmap(Width, Height, PixelFormat.Format32bppPArgb);
                G = Graphics.FromImage(B);
            }
            else
            {
                G = null;
                B = null;
            }
        }

        private void InvalidateCustimization()
        {
            MemoryStream M = new MemoryStream(Items.Count * 4);

            foreach (Bloom B in Colors)
            {
                M.Write(BitConverter.GetBytes(B.Value.ToArgb()), 0, 4);
            }

            M.Close();
            _Customization = Convert.ToBase64String(M.ToArray());
        }

        private void InvalidateTimer()
        {
            if (DesignMode || !DoneCreation)
                return;

            if (_IsAnimated)
            {
                ThemeShare.AddAnimationCallback(DoAnimation);
            }
            else
            {
                ThemeShare.RemoveAnimationCallback(DoAnimation);
            }
        }

        #endregion
        
        #region " User Hooks "

        protected abstract void ColorHook();
        protected abstract void PaintHook(PaintEventArgs e);

        protected virtual void OnCreation()
        {
        }

        protected virtual void OnAnimation()
        {
        }

        #endregion
        
        #region " Offset "

        private Rectangle OffsetReturnRectangle;
        protected Rectangle Offset(Rectangle r, int amount)
        {
            OffsetReturnRectangle = new Rectangle(r.X + amount, r.Y + amount, r.Width - (amount * 2), r.Height - (amount * 2));
            return OffsetReturnRectangle;
        }

        private Size OffsetReturnSize;
        protected Size Offset(Size s, int amount)
        {
            OffsetReturnSize = new Size(s.Width + amount, s.Height + amount);
            return OffsetReturnSize;
        }

        private Point OffsetReturnPoint;
        protected Point Offset(Point p, int amount)
        {
            OffsetReturnPoint = new Point(p.X + amount, p.Y + amount);
            return OffsetReturnPoint;
        }

        #endregion

        #region " Center "


        private Point CenterReturn;
        protected Point Center(Rectangle p, Rectangle c)
        {
            CenterReturn = new Point((p.Width / 2 - c.Width / 2) + p.X + c.X, (p.Height / 2 - c.Height / 2) + p.Y + c.Y);
            return CenterReturn;
        }
        protected Point Center(Rectangle p, Size c)
        {
            CenterReturn = new Point((p.Width / 2 - c.Width / 2) + p.X, (p.Height / 2 - c.Height / 2) + p.Y);
            return CenterReturn;
        }

        protected Point Center(Rectangle child)
        {
            return Center(Width, Height, child.Width, child.Height);
        }
        protected Point Center(Size child)
        {
            return Center(Width, Height, child.Width, child.Height);
        }
        protected Point Center(int childWidth, int childHeight)
        {
            return Center(Width, Height, childWidth, childHeight);
        }

        protected Point Center(Size p, Size c)
        {
            return Center(p.Width, p.Height, c.Width, c.Height);
        }

        protected Point Center(int pWidth, int pHeight, int cWidth, int cHeight)
        {
            CenterReturn = new Point(pWidth / 2 - cWidth / 2, pHeight / 2 - cHeight / 2);
            return CenterReturn;
        }

        #endregion

        #region " Measure "

        private Bitmap MeasureBitmap;

        private Graphics MeasureGraphics;
        protected Size Measure()
        {
            lock (MeasureGraphics)
            {
                return MeasureGraphics.MeasureString(Text, Font, Width).ToSize();
            }
        }
        protected Size Measure(string text)
        {
            lock (MeasureGraphics)
            {
                return MeasureGraphics.MeasureString(text, Font, Width).ToSize();
            }
        }

        #endregion
        
        #region " DrawPixel "


        private SolidBrush DrawPixelBrush;
        protected void DrawPixel(Color c1, int x, int y)
        {
            if (_Transparent)
            {
                B.SetPixel(x, y, c1);
            }
            else
            {
                DrawPixelBrush = new SolidBrush(c1);
                G.FillRectangle(DrawPixelBrush, x, y, 1, 1);
            }
        }

        #endregion

        #region " DrawCorners "


        private SolidBrush DrawCornersBrush;
        protected void DrawCorners(Color c1, int offset)
        {
            DrawCorners(c1, 0, 0, Width, Height, offset);
        }
        protected void DrawCorners(Color c1, Rectangle r1, int offset)
        {
            DrawCorners(c1, r1.X, r1.Y, r1.Width, r1.Height, offset);
        }
        protected void DrawCorners(Color c1, int x, int y, int width, int height, int offset)
        {
            DrawCorners(c1, x + offset, y + offset, width - (offset * 2), height - (offset * 2));
        }

        protected void DrawCorners(Color c1)
        {
            DrawCorners(c1, 0, 0, Width, Height);
        }
        protected void DrawCorners(Color c1, Rectangle r1)
        {
            DrawCorners(c1, r1.X, r1.Y, r1.Width, r1.Height);
        }
        protected void DrawCorners(Color c1, int x, int y, int width, int height)
        {
            if (_NoRounding)
                return;

            if (_Transparent)
            {
                B.SetPixel(x, y, c1);
                B.SetPixel(x + (width - 1), y, c1);
                B.SetPixel(x, y + (height - 1), c1);
                B.SetPixel(x + (width - 1), y + (height - 1), c1);
            }
            else
            {
                DrawCornersBrush = new SolidBrush(c1);
                G.FillRectangle(DrawCornersBrush, x, y, 1, 1);
                G.FillRectangle(DrawCornersBrush, x + (width - 1), y, 1, 1);
                G.FillRectangle(DrawCornersBrush, x, y + (height - 1), 1, 1);
                G.FillRectangle(DrawCornersBrush, x + (width - 1), y + (height - 1), 1, 1);
            }
        }

        #endregion

        #region " DrawBorders "

        protected void DrawBorders(Pen p1, int offset)
        {
            DrawBorders(p1, 0, 0, Width, Height, offset);
        }
        protected void DrawBorders(Pen p1, Rectangle r, int offset)
        {
            DrawBorders(p1, r.X, r.Y, r.Width, r.Height, offset);
        }
        protected void DrawBorders(Pen p1, int x, int y, int width, int height, int offset)
        {
            DrawBorders(p1, x + offset, y + offset, width - (offset * 2), height - (offset * 2));
        }

        protected void DrawBorders(Pen p1)
        {
            DrawBorders(p1, 0, 0, Width, Height);
        }
        protected void DrawBorders(Pen p1, Rectangle r)
        {
            DrawBorders(p1, r.X, r.Y, r.Width, r.Height);
        }
        protected void DrawBorders(Pen p1, int x, int y, int width, int height)
        {
            G.DrawRectangle(p1, x, y, width - 1, height - 1);
        }

        #endregion

        #region " DrawText "

        private Point DrawTextPoint;

        private Size DrawTextSize;
        protected void DrawText(Brush b1, HorizontalAlignment a, int x, int y)
        {
            DrawText(b1, Text, a, x, y);
        }
        protected void DrawText(Brush b1, string text, HorizontalAlignment a, int x, int y)
        {
            if (text.Length == 0)
                return;

            DrawTextSize = Measure(text);
            DrawTextPoint = new Point(Width / 2 - DrawTextSize.Width / 2, Header / 2 - DrawTextSize.Height / 2);

            switch (a)
            {
                case HorizontalAlignment.Left:
                    G.DrawString(text, Font, b1, x, DrawTextPoint.Y + y);
                    break;
                case HorizontalAlignment.Center:
                    G.DrawString(text, Font, b1, DrawTextPoint.X + x, DrawTextPoint.Y + y);
                    break;
                case HorizontalAlignment.Right:
                    G.DrawString(text, Font, b1, Width - DrawTextSize.Width - x, DrawTextPoint.Y + y);
                    break;
            }
        }

        protected void DrawText(Brush b1, Point p1)
        {
            if (Text.Length == 0)
                return;
            G.DrawString(Text, Font, b1, p1);
        }
        protected void DrawText(Brush b1, int x, int y)
        {
            if (Text.Length == 0)
                return;
            G.DrawString(Text, Font, b1, x, y);
        }

        #endregion

        #region " DrawImage "


        private Point DrawImagePoint;
        protected void DrawImage(HorizontalAlignment a, int x, int y)
        {
            DrawImage(_Image, a, x, y);
        }
        protected void DrawImage(Image image, HorizontalAlignment a, int x, int y)
        {
            if (image == null)
                return;
            DrawImagePoint = new Point(Width / 2 - image.Width / 2, Header / 2 - image.Height / 2);

            switch (a)
            {
                case HorizontalAlignment.Left:
                    G.DrawImage(image, x, DrawImagePoint.Y + y, image.Width, image.Height);
                    break;
                case HorizontalAlignment.Center:
                    G.DrawImage(image, DrawImagePoint.X + x, DrawImagePoint.Y + y, image.Width, image.Height);
                    break;
                case HorizontalAlignment.Right:
                    G.DrawImage(image, Width - image.Width - x, DrawImagePoint.Y + y, image.Width, image.Height);
                    break;
            }
        }

        protected void DrawImage(Point p1)
        {
            DrawImage(_Image, p1.X, p1.Y);
        }
        protected void DrawImage(int x, int y)
        {
            DrawImage(_Image, x, y);
        }

        protected void DrawImage(Image image, Point p1)
        {
            DrawImage(image, p1.X, p1.Y);
        }
        protected void DrawImage(Image image, int x, int y)
        {
            if (image == null)
                return;
            G.DrawImage(image, x, y, image.Width, image.Height);
        }

        #endregion

        #region " DrawGradient "

        private LinearGradientBrush DrawGradientBrush;

        private Rectangle DrawGradientRectangle;
        protected void DrawGradient(ColorBlend blend, int x, int y, int width, int height)
        {
            DrawGradientRectangle = new Rectangle(x, y, width, height);
            DrawGradient(blend, DrawGradientRectangle);
        }
        protected void DrawGradient(ColorBlend blend, int x, int y, int width, int height, float angle)
        {
            DrawGradientRectangle = new Rectangle(x, y, width, height);
            DrawGradient(blend, DrawGradientRectangle, angle);
        }

        protected void DrawGradient(ColorBlend blend, Rectangle r)
        {
            DrawGradientBrush = new LinearGradientBrush(r, Color.Empty, Color.Empty, 90f);
            DrawGradientBrush.InterpolationColors = blend;
            G.FillRectangle(DrawGradientBrush, r);
        }
        protected void DrawGradient(ColorBlend blend, Rectangle r, float angle)
        {
            DrawGradientBrush = new LinearGradientBrush(r, Color.Empty, Color.Empty, angle);
            DrawGradientBrush.InterpolationColors = blend;
            G.FillRectangle(DrawGradientBrush, r);
        }


        protected void DrawGradient(Color c1, Color c2, int x, int y, int width, int height)
        {
            DrawGradientRectangle = new Rectangle(x, y, width, height);
            DrawGradient(c1, c2, DrawGradientRectangle);
        }
        protected void DrawGradient(Color c1, Color c2, int x, int y, int width, int height, float angle)
        {
            DrawGradientRectangle = new Rectangle(x, y, width, height);
            DrawGradient(c1, c2, DrawGradientRectangle, angle);
        }

        protected void DrawGradient(Color c1, Color c2, Rectangle r)
        {
            DrawGradientBrush = new LinearGradientBrush(r, c1, c2, 90f);
            G.FillRectangle(DrawGradientBrush, r);
        }
        protected void DrawGradient(Color c1, Color c2, Rectangle r, float angle)
        {
            DrawGradientBrush = new LinearGradientBrush(r, c1, c2, angle);
            G.FillRectangle(DrawGradientBrush, r);
        }

        #endregion

        #region " DrawRadial "

        private GraphicsPath DrawRadialPath;
        private PathGradientBrush DrawRadialBrush1;
        private LinearGradientBrush DrawRadialBrush2;

        private Rectangle DrawRadialRectangle;
        public void DrawRadial(ColorBlend blend, int x, int y, int width, int height)
        {
            DrawRadialRectangle = new Rectangle(x, y, width, height);
            DrawRadial(blend, DrawRadialRectangle, width / 2, height / 2);
        }
        public void DrawRadial(ColorBlend blend, int x, int y, int width, int height, Point center)
        {
            DrawRadialRectangle = new Rectangle(x, y, width, height);
            DrawRadial(blend, DrawRadialRectangle, center.X, center.Y);
        }
        public void DrawRadial(ColorBlend blend, int x, int y, int width, int height, int cx, int cy)
        {
            DrawRadialRectangle = new Rectangle(x, y, width, height);
            DrawRadial(blend, DrawRadialRectangle, cx, cy);
        }

        public void DrawRadial(ColorBlend blend, Rectangle r)
        {
            DrawRadial(blend, r, r.Width / 2, r.Height / 2);
        }
        public void DrawRadial(ColorBlend blend, Rectangle r, Point center)
        {
            DrawRadial(blend, r, center.X, center.Y);
        }
        public void DrawRadial(ColorBlend blend, Rectangle r, int cx, int cy)
        {
            DrawRadialPath.Reset();
            DrawRadialPath.AddEllipse(r.X, r.Y, r.Width - 1, r.Height - 1);

            DrawRadialBrush1 = new PathGradientBrush(DrawRadialPath);
            DrawRadialBrush1.CenterPoint = new Point(r.X + cx, r.Y + cy);
            DrawRadialBrush1.InterpolationColors = blend;

            if (G.SmoothingMode == SmoothingMode.AntiAlias)
            {
                G.FillEllipse(DrawRadialBrush1, r.X + 1, r.Y + 1, r.Width - 3, r.Height - 3);
            }
            else
            {
                G.FillEllipse(DrawRadialBrush1, r);
            }
        }


        protected void DrawRadial(Color c1, Color c2, int x, int y, int width, int height)
        {
            DrawRadialRectangle = new Rectangle(x, y, width, height);
            DrawRadial(c1, c2, DrawGradientRectangle);
        }
        protected void DrawRadial(Color c1, Color c2, int x, int y, int width, int height, float angle)
        {
            DrawRadialRectangle = new Rectangle(x, y, width, height);
            DrawRadial(c1, c2, DrawGradientRectangle, angle);
        }

        protected void DrawRadial(Color c1, Color c2, Rectangle r)
        {
            DrawRadialBrush2 = new LinearGradientBrush(r, c1, c2, 90f);
            G.FillRectangle(DrawGradientBrush, r);
        }
        protected void DrawRadial(Color c1, Color c2, Rectangle r, float angle)
        {
            DrawRadialBrush2 = new LinearGradientBrush(r, c1, c2, angle);
            G.FillEllipse(DrawGradientBrush, r);
        }

        #endregion

        #region " CreateRound "

        private GraphicsPath CreateRoundPath;

        private Rectangle CreateRoundRectangle;
        public GraphicsPath CreateRound(int x, int y, int width, int height, int slope)
        {
            CreateRoundRectangle = new Rectangle(x, y, width, height);
            return CreateRound(CreateRoundRectangle, slope);
        }

        public GraphicsPath CreateRound(Rectangle r, int slope)
        {
            CreateRoundPath = new GraphicsPath(FillMode.Winding);
            CreateRoundPath.AddArc(r.X, r.Y, slope, slope, 180f, 90f);
            CreateRoundPath.AddArc(r.Right - slope, r.Y, slope, slope, 270f, 90f);
            CreateRoundPath.AddArc(r.Right - slope, r.Bottom - slope, slope, slope, 0f, 90f);
            CreateRoundPath.AddArc(r.X, r.Bottom - slope, slope, slope, 90f, 90f);
            CreateRoundPath.CloseFigure();
            return CreateRoundPath;
        }

        #endregion

    }

    //---------------ThemeControl
    public abstract class ThemeControl : Control
    {


        #region " Initialization "

        protected Graphics G;

        protected Bitmap B;
        public ThemeControl()
        {
            SetStyle((ControlStyles)139270, true);

            _ImageSize = Size.Empty;
            Font = new Font("Verdana", 8);

            MeasureBitmap = new Bitmap(1, 1);
            MeasureGraphics = Graphics.FromImage(MeasureBitmap);

            DrawRadialPath = new GraphicsPath();

            InvalidateCustimization();
            //Remove?
        }

        protected override sealed void OnHandleCreated(EventArgs e)
        {
            InvalidateCustimization();
            ColorHook();

            if (!(_LockWidth == 0))
                Width = _LockWidth;
            if (!(_LockHeight == 0))
                Height = _LockHeight;

            Transparent = _Transparent;
            if (_Transparent && _BackColor)
                BackColor = Color.Transparent;

            base.OnHandleCreated(e);
        }

        private bool DoneCreation;
        protected override sealed void OnParentChanged(EventArgs e)
        {
            if (Parent != null)
            {
                OnCreation();
                DoneCreation = true;
                InvalidateTimer();
            }

            base.OnParentChanged(e);
        }

        #endregion

        private void DoAnimation(bool i)
        {
            OnAnimation();
            if (i)
                Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (Width == 0 || Height == 0)
                return;

            if (_Transparent)
            {
                PaintHook(e);
                e.Graphics.DrawImage(B, 0, 0);
            }
            else
            {
                G = e.Graphics;
                PaintHook(e);
            }
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            ThemeShare.RemoveAnimationCallback(DoAnimation);
            base.OnHandleDestroyed(e);
        }

        #region " Size Handling "

        protected override sealed void OnSizeChanged(EventArgs e)
        {
            if (_Transparent)
            {
                InvalidateBitmap();
            }

            Invalidate();
            base.OnSizeChanged(e);
        }

        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            if (!(_LockWidth == 0))
                width = _LockWidth;
            if (!(_LockHeight == 0))
                height = _LockHeight;
            base.SetBoundsCore(x, y, width, height, specified);
        }

        #endregion

        #region " State Handling "

        private bool InPosition;

        protected override void OnMouseEnter(EventArgs e)
        {
            InPosition = true;
            SetState(MouseState.Over);
            base.OnMouseEnter(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (InPosition)
                SetState(MouseState.Over);
            base.OnMouseUp(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
                SetState(MouseState.Down);
            base.OnMouseDown(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            InPosition = false;
            SetState(MouseState.None);
            base.OnMouseLeave(e);
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            if (Enabled)
                SetState(MouseState.None);
            else
                SetState(MouseState.Block);
            base.OnEnabledChanged(e);
        }

        protected MouseState State;
        private void SetState(MouseState current)
        {
            State = current;
            Invalidate();
        }

        #endregion


        #region " Base Properties "

        //[Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        //public override Color ForeColor
        //{
        //    get { return Color.Empty; }
        //    set { }
        //}
        //[Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        //public override Image BackgroundImage
        //{
        //    get { return null; }
        //    set { }
        //}
        //[Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        //public override ImageLayout BackgroundImageLayout
        //{
        //    get { return ImageLayout.None; }
        //    set { }
        //}

        public override string Text
        {
            get { return base.Text; }
            set
            {
                base.Text = value;
                Invalidate();
            }
        }
        public override Font Font
        {
            get { return base.Font; }
            set
            {
                base.Font = value;
                Invalidate();
            }
        }

        private bool _BackColor;
        [Category("Misc")]
        public override Color BackColor
        {
            get { return base.BackColor; }
            set
            {
                if (!IsHandleCreated && value == Color.Transparent)
                {
                    _BackColor = true;
                    return;
                }

                base.BackColor = value;
                if (Parent != null)
                    ColorHook();
            }
        }

        #endregion

        #region " Public Properties "

        private bool _NoRounding;
        public bool NoRounding
        {
            get { return _NoRounding; }
            set
            {
                _NoRounding = value;
                Invalidate();
            }
        }

        private Image _Image;
        public Image Image
        {
            get { return _Image; }
            set
            {
                if (value == null)
                {
                    _ImageSize = Size.Empty;
                }
                else
                {
                    _ImageSize = value.Size;
                }

                _Image = value;
                Invalidate();
            }
        }

        private bool _Transparent;
        public bool Transparent
        {
            get { return _Transparent; }
            set
            {
                _Transparent = value;
                if (!IsHandleCreated)
                    return;

                if (!value && !(BackColor.A == 255))
                {
                    throw new Exception("Unable to change value to false while a transparent BackColor is in use.");
                }

                SetStyle(ControlStyles.Opaque, !value);
                SetStyle(ControlStyles.SupportsTransparentBackColor, value);

                if (value)
                    InvalidateBitmap();
                else
                    B = null;
                Invalidate();
            }
        }

        private Dictionary<string, Color> Items = new Dictionary<string, Color>();
        public Bloom[] Colors
        {
            get
            {
                List<Bloom> T = new List<Bloom>();
                Dictionary<string, Color>.Enumerator E = Items.GetEnumerator();

                while (E.MoveNext())
                {
                    T.Add(new Bloom(E.Current.Key, E.Current.Value));
                }

                return T.ToArray();
            }
            set
            {
                foreach (Bloom B in value)
                {
                    if (Items.ContainsKey(B.Name))
                        Items[B.Name] = B.Value;
                }

                InvalidateCustimization();
                ColorHook();
                Invalidate();
            }
        }

        private string _Customization;
        public string Customization
        {
            get { return _Customization; }
            set
            {
                if (value == _Customization)
                    return;

                byte[] Data = null;
                Bloom[] Items = Colors;

                try
                {
                    Data = Convert.FromBase64String(value);
                    for (int I = 0; I <= Items.Length - 1; I++)
                    {
                        Items[I].Value = Color.FromArgb(BitConverter.ToInt32(Data, I * 4));
                    }
                }
                catch
                {
                    return;
                }

                _Customization = value;

                Colors = Items;
                ColorHook();
                Invalidate();
            }
        }

        #endregion

        #region " Private Properties "

        private Size _ImageSize;
        protected Size ImageSize
        {
            get { return _ImageSize; }
        }

        private int _LockWidth;
        protected int LockWidth
        {
            get { return _LockWidth; }
            set
            {
                _LockWidth = value;
                if (!(LockWidth == 0) && IsHandleCreated)
                    Width = LockWidth;
            }
        }

        private int _LockHeight;
        protected int LockHeight
        {
            get { return _LockHeight; }
            set
            {
                _LockHeight = value;
                if (!(LockHeight == 0) && IsHandleCreated)
                    Height = LockHeight;
            }
        }

        private bool _IsAnimated;
        protected bool IsAnimated
        {
            get { return _IsAnimated; }
            set
            {
                _IsAnimated = value;
                InvalidateTimer();
            }
        }

        #endregion
        
        #region " Property Helpers "

        protected Pen GetPen(string name)
        {
            return new Pen(Items[name]);
        }
        protected Pen GetPen(string name, float width)
        {
            return new Pen(Items[name], width);
        }

        protected SolidBrush GetBrush(string name)
        {
            return new SolidBrush(Items[name]);
        }

        protected Color GetColor(string name)
        {
            return Items[name];
        }

        protected void SetColor(string name, Color value)
        {
            if (Items.ContainsKey(name))
                Items[name] = value;
            else
                Items.Add(name, value);
        }
        protected void SetColor(string name, byte r, byte g, byte b)
        {
            SetColor(name, Color.FromArgb(r, g, b));
        }
        protected void SetColor(string name, byte a, byte r, byte g, byte b)
        {
            SetColor(name, Color.FromArgb(a, r, g, b));
        }
        protected void SetColor(string name, byte a, Color value)
        {
            SetColor(name, Color.FromArgb(a, value));
        }

        private void InvalidateBitmap()
        {
            if (Width == 0 || Height == 0)
                return;
            B = new Bitmap(Width, Height, PixelFormat.Format32bppPArgb);
            G = Graphics.FromImage(B);
        }

        private void InvalidateCustimization()
        {
            MemoryStream M = new MemoryStream(Items.Count * 4);

            foreach (Bloom B in Colors)
            {
                M.Write(BitConverter.GetBytes(B.Value.ToArgb()), 0, 4);
            }

            M.Close();
            _Customization = Convert.ToBase64String(M.ToArray());
        }

        private void InvalidateTimer()
        {
            if (DesignMode || !DoneCreation)
                return;

            if (_IsAnimated)
            {
                ThemeShare.AddAnimationCallback(DoAnimation);
            }
            else
            {
                ThemeShare.RemoveAnimationCallback(DoAnimation);
            }
        }
        #endregion
        
        #region " User Hooks "

        protected abstract void ColorHook();

        protected abstract void PaintHook(PaintEventArgs e);

        protected virtual void OnCreation()
        {
        }

        protected virtual void OnAnimation()
        {
        }

        #endregion
        
        #region " Offset "

        private Rectangle OffsetReturnRectangle;
        protected Rectangle Offset(Rectangle r, int amount)
        {
            OffsetReturnRectangle = new Rectangle(r.X + amount, r.Y + amount, r.Width - (amount * 2), r.Height - (amount * 2));
            return OffsetReturnRectangle;
        }

        private Size OffsetReturnSize;
        protected Size Offset(Size s, int amount)
        {
            OffsetReturnSize = new Size(s.Width + amount, s.Height + amount);
            return OffsetReturnSize;
        }

        private Point OffsetReturnPoint;
        protected Point Offset(Point p, int amount)
        {
            OffsetReturnPoint = new Point(p.X + amount, p.Y + amount);
            return OffsetReturnPoint;
        }

        #endregion

        #region " Center "


        private Point CenterReturn;
        protected Point Center(Rectangle p, Rectangle c)
        {
            CenterReturn = new Point((p.Width / 2 - c.Width / 2) + p.X + c.X, (p.Height / 2 - c.Height / 2) + p.Y + c.Y);
            return CenterReturn;
        }
        protected Point Center(Rectangle p, Size c)
        {
            CenterReturn = new Point((p.Width / 2 - c.Width / 2) + p.X, (p.Height / 2 - c.Height / 2) + p.Y);
            return CenterReturn;
        }

        protected Point Center(Rectangle child)
        {
            return Center(Width, Height, child.Width, child.Height);
        }
        protected Point Center(Size child)
        {
            return Center(Width, Height, child.Width, child.Height);
        }
        protected Point Center(int childWidth, int childHeight)
        {
            return Center(Width, Height, childWidth, childHeight);
        }

        protected Point Center(Size p, Size c)
        {
            return Center(p.Width, p.Height, c.Width, c.Height);
        }

        protected Point Center(int pWidth, int pHeight, int cWidth, int cHeight)
        {
            CenterReturn = new Point(pWidth / 2 - cWidth / 2, pHeight / 2 - cHeight / 2);
            return CenterReturn;
        }

        #endregion

        #region " Measure "

        private Bitmap MeasureBitmap;
        //TODO: Potential issues during multi-threading.
        private Graphics MeasureGraphics;

        protected Size Measure()
        {
            return MeasureGraphics.MeasureString(Text, Font, Width).ToSize();
        }
        protected Size Measure(string text)
        {
            return MeasureGraphics.MeasureString(text, Font, Width).ToSize();
        }

        #endregion
        
        #region " DrawPixel "


        private SolidBrush DrawPixelBrush;
        protected void DrawPixel(Color c1, int x, int y)
        {
            if (_Transparent)
            {
                B.SetPixel(x, y, c1);
            }
            else
            {
                DrawPixelBrush = new SolidBrush(c1);
                G.FillRectangle(DrawPixelBrush, x, y, 1, 1);
            }
        }

        #endregion

        #region " DrawCorners "


        private SolidBrush DrawCornersBrush;
        protected void DrawCorners(Color c1, int offset)
        {
            DrawCorners(c1, 0, 0, Width, Height, offset);
        }
        protected void DrawCorners(Color c1, Rectangle r1, int offset)
        {
            DrawCorners(c1, r1.X, r1.Y, r1.Width, r1.Height, offset);
        }
        protected void DrawCorners(Color c1, int x, int y, int width, int height, int offset)
        {
            DrawCorners(c1, x + offset, y + offset, width - (offset * 2), height - (offset * 2));
        }

        protected void DrawCorners(Color c1)
        {
            DrawCorners(c1, 0, 0, Width, Height);
        }
        protected void DrawCorners(Color c1, Rectangle r1)
        {
            DrawCorners(c1, r1.X, r1.Y, r1.Width, r1.Height);
        }
        protected void DrawCorners(Color c1, int x, int y, int width, int height)
        {
            if (_NoRounding)
                return;

            if (_Transparent)
            {
                B.SetPixel(x, y, c1);
                B.SetPixel(x + (width - 1), y, c1);
                B.SetPixel(x, y + (height - 1), c1);
                B.SetPixel(x + (width - 1), y + (height - 1), c1);
            }
            else
            {
                DrawCornersBrush = new SolidBrush(c1);
                G.FillRectangle(DrawCornersBrush, x, y, 1, 1);
                G.FillRectangle(DrawCornersBrush, x + (width - 1), y, 1, 1);
                G.FillRectangle(DrawCornersBrush, x, y + (height - 1), 1, 1);
                G.FillRectangle(DrawCornersBrush, x + (width - 1), y + (height - 1), 1, 1);
            }
        }

        #endregion

        #region " DrawBorders "

        protected void DrawBorders(Pen p1, int offset)
        {
            DrawBorders(p1, 0, 0, Width, Height, offset);
        }
        protected void DrawBorders(Pen p1, Rectangle r, int offset)
        {
            DrawBorders(p1, r.X, r.Y, r.Width, r.Height, offset);
        }
        protected void DrawBorders(Pen p1, int x, int y, int width, int height, int offset)
        {
            DrawBorders(p1, x + offset, y + offset, width - (offset * 2), height - (offset * 2));
        }

        protected void DrawBorders(Pen p1)
        {
            DrawBorders(p1, 0, 0, Width, Height);
        }
        protected void DrawBorders(Pen p1, Rectangle r)
        {
            DrawBorders(p1, r.X, r.Y, r.Width, r.Height);
        }
        protected void DrawBorders(Pen p1, int x, int y, int width, int height)
        {
            G.DrawRectangle(p1, x, y, width - 1, height - 1);
        }

        /// <summary>
        /// Draws the borders.
        /// </summary>
        /// <param name="p1">The p1.</param>
        /// <param name="p2">The p2.</param>
        /// <param name="rect">The rect.</param>
        protected void DrawBorders(Pen p1, Pen p2, Rectangle rect)
        {
            G.DrawRectangle(p1, rect.X, rect.Y, rect.Width - 1, rect.Height - 1);
            G.DrawRectangle(p2, rect.X + 1, rect.Y + 1, rect.Width - 3, rect.Height - 3);
        }
        #endregion

        #region " DrawText "

        private Point DrawTextPoint;

        private Size DrawTextSize;
        protected void DrawText(Brush b1, HorizontalAlignment a, int x, int y)
        {
            DrawText(b1, Text, a, x, y);
        }
        protected void DrawText(Brush b1, string text, HorizontalAlignment a, int x, int y)
        {
            if (text.Length == 0)
                return;

            DrawTextSize = Measure(text);
            DrawTextPoint = Center(DrawTextSize);

            switch (a)
            {
                case HorizontalAlignment.Left:
                    G.DrawString(text, Font, b1, x, DrawTextPoint.Y + y);
                    break;
                case HorizontalAlignment.Center:
                    G.DrawString(text, Font, b1, DrawTextPoint.X + x, DrawTextPoint.Y + y);
                    break;
                case HorizontalAlignment.Right:
                    G.DrawString(text, Font, b1, Width - DrawTextSize.Width - x, DrawTextPoint.Y + y);
                    break;
            }
        }

        protected void DrawText(Brush b1, Point p1)
        {
            if (Text.Length == 0)
                return;
            G.DrawString(Text, Font, b1, p1);
        }
        protected void DrawText(Brush b1, int x, int y)
        {
            if (Text.Length == 0)
                return;
            G.DrawString(Text, Font, b1, x, y);
        }

        /// <summary>
        /// Draws the text.
        /// </summary>
        /// <param name="a">a.</param>
        /// <param name="c">The c.</param>
        /// <param name="x">The x.</param>
        protected void DrawText(HorizontalAlignment a, Color c, int x)
        {
            DrawText(a, c, x, 0);
        }

        /// <summary>
        /// Draws the text.
        /// </summary>
        /// <param name="a">a.</param>
        /// <param name="c">The c.</param>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        protected void DrawText(HorizontalAlignment a, Color c, int x, int y)
        {
            if (string.IsNullOrEmpty(Text))
                return;
            Size _Size = G.MeasureString(Text, Font).ToSize();
            SolidBrush _Brush = new SolidBrush(c);

            switch (a)
            {
                case HorizontalAlignment.Left:
                    G.DrawString(Text, Font, _Brush, x, Height / 2 - _Size.Height / 2 + y);
                    break;
                case HorizontalAlignment.Right:
                    G.DrawString(Text, Font, _Brush, Width - _Size.Width - x, Height / 2 - _Size.Height / 2 + y);
                    break;
                case HorizontalAlignment.Center:
                    G.DrawString(Text, Font, _Brush, Width / 2 - _Size.Width / 2 + x, Height / 2 - _Size.Height / 2 + y);
                    break;
            }
        }


        #endregion

        #region " DrawImage "


        private Point DrawImagePoint;
        protected void DrawImage(HorizontalAlignment a, int x, int y)
        {
            DrawImage(_Image, a, x, y);
        }
        protected void DrawImage(Image image, HorizontalAlignment a, int x, int y)
        {
            if (image == null)
                return;
            DrawImagePoint = Center(image.Size);

            switch (a)
            {
                case HorizontalAlignment.Left:
                    G.DrawImage(image, x, DrawImagePoint.Y + y, image.Width, image.Height);
                    break;
                case HorizontalAlignment.Center:
                    G.DrawImage(image, DrawImagePoint.X + x, DrawImagePoint.Y + y, image.Width, image.Height);
                    break;
                case HorizontalAlignment.Right:
                    G.DrawImage(image, Width - image.Width - x, DrawImagePoint.Y + y, image.Width, image.Height);
                    break;
            }
        }

        protected void DrawImage(Point p1)
        {
            DrawImage(_Image, p1.X, p1.Y);
        }
        protected void DrawImage(int x, int y)
        {
            DrawImage(_Image, x, y);
        }

        protected void DrawImage(Image image, Point p1)
        {
            DrawImage(image, p1.X, p1.Y);
        }
        protected void DrawImage(Image image, int x, int y)
        {
            if (image == null)
                return;
            G.DrawImage(image, x, y, image.Width, image.Height);
        }

        #endregion

        #region " DrawGradient "

        private LinearGradientBrush DrawGradientBrush;

        private Rectangle DrawGradientRectangle;
        protected void DrawGradient(ColorBlend blend, int x, int y, int width, int height)
        {
            DrawGradientRectangle = new Rectangle(x, y, width, height);
            DrawGradient(blend, DrawGradientRectangle);
        }
        protected void DrawGradient(ColorBlend blend, int x, int y, int width, int height, float angle)
        {
            DrawGradientRectangle = new Rectangle(x, y, width, height);
            DrawGradient(blend, DrawGradientRectangle, angle);
        }

        protected void DrawGradient(ColorBlend blend, Rectangle r)
        {
            DrawGradientBrush = new LinearGradientBrush(r, Color.Empty, Color.Empty, 90f);
            DrawGradientBrush.InterpolationColors = blend;
            G.FillRectangle(DrawGradientBrush, r);
        }
        protected void DrawGradient(ColorBlend blend, Rectangle r, float angle)
        {
            DrawGradientBrush = new LinearGradientBrush(r, Color.Empty, Color.Empty, angle);
            DrawGradientBrush.InterpolationColors = blend;
            G.FillRectangle(DrawGradientBrush, r);
        }


        protected void DrawGradient(Color c1, Color c2, int x, int y, int width, int height)
        {
            DrawGradientRectangle = new Rectangle(x, y, width, height);
            DrawGradient(c1, c2, DrawGradientRectangle);
        }
        protected void DrawGradient(Color c1, Color c2, int x, int y, int width, int height, float angle)
        {
            DrawGradientRectangle = new Rectangle(x, y, width, height);
            DrawGradient(c1, c2, DrawGradientRectangle, angle);
        }

        protected void DrawGradient(Color c1, Color c2, Rectangle r)
        {
            DrawGradientBrush = new LinearGradientBrush(r, c1, c2, 90f);
            G.FillRectangle(DrawGradientBrush, r);
        }
        protected void DrawGradient(Color c1, Color c2, Rectangle r, float angle)
        {
            DrawGradientBrush = new LinearGradientBrush(r, c1, c2, angle);
            G.FillRectangle(DrawGradientBrush, r);
        }

        #endregion

        #region " DrawRadial "

        private GraphicsPath DrawRadialPath;
        private PathGradientBrush DrawRadialBrush1;
        private LinearGradientBrush DrawRadialBrush2;

        private Rectangle DrawRadialRectangle;
        public void DrawRadial(ColorBlend blend, int x, int y, int width, int height)
        {
            DrawRadialRectangle = new Rectangle(x, y, width, height);
            DrawRadial(blend, DrawRadialRectangle, width / 2, height / 2);
        }
        public void DrawRadial(ColorBlend blend, int x, int y, int width, int height, Point center)
        {
            DrawRadialRectangle = new Rectangle(x, y, width, height);
            DrawRadial(blend, DrawRadialRectangle, center.X, center.Y);
        }
        public void DrawRadial(ColorBlend blend, int x, int y, int width, int height, int cx, int cy)
        {
            DrawRadialRectangle = new Rectangle(x, y, width, height);
            DrawRadial(blend, DrawRadialRectangle, cx, cy);
        }

        public void DrawRadial(ColorBlend blend, Rectangle r)
        {
            DrawRadial(blend, r, r.Width / 2, r.Height / 2);
        }
        public void DrawRadial(ColorBlend blend, Rectangle r, Point center)
        {
            DrawRadial(blend, r, center.X, center.Y);
        }
        public void DrawRadial(ColorBlend blend, Rectangle r, int cx, int cy)
        {
            DrawRadialPath.Reset();
            DrawRadialPath.AddEllipse(r.X, r.Y, r.Width - 1, r.Height - 1);

            DrawRadialBrush1 = new PathGradientBrush(DrawRadialPath);
            DrawRadialBrush1.CenterPoint = new Point(r.X + cx, r.Y + cy);
            DrawRadialBrush1.InterpolationColors = blend;

            if (G.SmoothingMode == SmoothingMode.AntiAlias)
            {
                G.FillEllipse(DrawRadialBrush1, r.X + 1, r.Y + 1, r.Width - 3, r.Height - 3);
            }
            else
            {
                G.FillEllipse(DrawRadialBrush1, r);
            }
        }


        protected void DrawRadial(Color c1, Color c2, int x, int y, int width, int height)
        {
            DrawRadialRectangle = new Rectangle(x, y, width, height);
            DrawRadial(c1, c2, DrawRadialRectangle);
        }
        protected void DrawRadial(Color c1, Color c2, int x, int y, int width, int height, float angle)
        {
            DrawRadialRectangle = new Rectangle(x, y, width, height);
            DrawRadial(c1, c2, DrawRadialRectangle, angle);
        }

        protected void DrawRadial(Color c1, Color c2, Rectangle r)
        {
            DrawRadialBrush2 = new LinearGradientBrush(r, c1, c2, 90f);
            G.FillEllipse(DrawRadialBrush2, r);
        }
        protected void DrawRadial(Color c1, Color c2, Rectangle r, float angle)
        {
            DrawRadialBrush2 = new LinearGradientBrush(r, c1, c2, angle);
            G.FillEllipse(DrawRadialBrush2, r);
        }

        #endregion

        #region " CreateRound "

        private GraphicsPath CreateRoundPath;

        private Rectangle CreateRoundRectangle;
        public GraphicsPath CreateRound(int x, int y, int width, int height, int slope)
        {
            CreateRoundRectangle = new Rectangle(x, y, width, height);
            return CreateRound(CreateRoundRectangle, slope);
        }

        public GraphicsPath CreateRound(Rectangle r, int slope)
        {
            CreateRoundPath = new GraphicsPath(FillMode.Winding);
            CreateRoundPath.AddArc(r.X, r.Y, slope, slope, 180f, 90f);
            CreateRoundPath.AddArc(r.Right - slope, r.Y, slope, slope, 270f, 90f);
            CreateRoundPath.AddArc(r.Right - slope, r.Bottom - slope, slope, slope, 0f, 90f);
            CreateRoundPath.AddArc(r.X, r.Bottom - slope, slope, slope, 90f, 90f);
            CreateRoundPath.CloseFigure();
            return CreateRoundPath;
        }

        #endregion

    }
    
    static class ThemeShare
    {

        #region " Animation "

        private static int Frames;
        private static bool Invalidate;

        public static PrecisionTimer ThemeTimer = new PrecisionTimer();
        //1000 / 50 = 20 FPS
        private const int FPS = 50;

        private const int Rate = 10;
        public delegate void AnimationDelegate(bool invalidate);


        private static List<AnimationDelegate> Callbacks = new List<AnimationDelegate>();
        private static void HandleCallbacks(IntPtr state, bool reserve)
        {
            Invalidate = (Frames >= FPS);
            if (Invalidate)
                Frames = 0;

            lock (Callbacks)
            {
                for (int I = 0; I <= Callbacks.Count - 1; I++)
                {
                    Callbacks[I].Invoke(Invalidate);
                }
            }

            Frames += Rate;
        }

        private static void InvalidateThemeTimer()
        {
            if (Callbacks.Count == 0)
            {
                ThemeTimer.Delete();
            }
            else
            {
                ThemeTimer.Create(0, Rate, HandleCallbacks);
            }
        }

        public static void AddAnimationCallback(AnimationDelegate callback)
        {
            lock (Callbacks)
            {
                if (Callbacks.Contains(callback))
                    return;

                Callbacks.Add(callback);
                InvalidateThemeTimer();
            }
        }

        public static void RemoveAnimationCallback(AnimationDelegate callback)
        {
            lock (Callbacks)
            {
                if (!Callbacks.Contains(callback))
                    return;

                Callbacks.Remove(callback);
                InvalidateThemeTimer();
            }
        }

        #endregion

    }

    public enum MouseState : byte
    {
        None = 0,
        Over = 1,
        Down = 2,
        Block = 3
    }

    public struct Bloom
    {
        private Pen _Pen;
        public Pen Pen
        {
            get { return _Pen; }
            set { _Pen = value; }
        }

        private Brush _Brush;
        public Brush Brush
        {
            get { return _Brush; }
            set { _Brush = value; }
        }

        public string _Name;
        public string Name
        {
            get { return _Name; }
        }

        private Color _Value;
        public Color Value
        {
            get { return _Value; }
            set { _Value = value; }
        }

        public string ValueHex
        {
            get { return string.Concat("#", _Value.R.ToString("X2", null), _Value.G.ToString("X2", null), _Value.B.ToString("X2", null)); }
            set
            {
                try
                {
                    _Value = ColorTranslator.FromHtml(value);
                }
                catch
                {
                    return;
                }
            }
        }


        public Bloom(string name, Color value)
        {
            _Name = name;
            _Value = value;
            _Pen = new Pen(value);
            _Brush = new SolidBrush(value);
        }
    }

    //------------------
    //Creator: aeonhack
    //Site: elitevs.net
    //Created: 11/30/2011
    //Changed: 11/30/2011
    //Version: 1.0.0
    //------------------
    public class PrecisionTimer : IDisposable
    {

        private bool _Enabled;
        public bool Enabled
        {
            get { return _Enabled; }
        }

        private IntPtr Handle;

        private TimerDelegate TimerCallback;
        [DllImport("kernel32.dll", EntryPoint = "CreateTimerQueueTimer")]
        private static extern bool CreateTimerQueueTimer(ref IntPtr handle, IntPtr queue, TimerDelegate callback, IntPtr state, uint dueTime, uint period, uint flags);

        [DllImport("kernel32.dll", EntryPoint = "DeleteTimerQueueTimer")]
        private static extern bool DeleteTimerQueueTimer(IntPtr queue, IntPtr handle, IntPtr callback);

        public delegate void TimerDelegate(IntPtr r1, bool r2);

        public void Create(uint dueTime, uint period, TimerDelegate callback)
        {
            if (_Enabled)
                return;

            TimerCallback = callback;
            bool Success = CreateTimerQueueTimer(ref Handle, IntPtr.Zero, TimerCallback, IntPtr.Zero, dueTime, period, 0);

            if (!Success)
                ThrowNewException("CreateTimerQueueTimer");
            _Enabled = Success;
        }

        public void Delete()
        {
            if (!_Enabled)
                return;
            bool Success = DeleteTimerQueueTimer(IntPtr.Zero, Handle, IntPtr.Zero);

            if (!Success && !(Marshal.GetLastWin32Error() == 997))
            {
                ThrowNewException("DeleteTimerQueueTimer");
            }

            _Enabled = !Success;
        }

        private void ThrowNewException(string name)
        {
            throw new Exception(string.Format("{0} failed. Win32Error: {1}", name, Marshal.GetLastWin32Error()));
        }

        public void Dispose()
        {
            Delete();
        }
    }

    /// <summary>
    /// Class DesignFunctions.
    /// </summary>
    static class DesignFunctions
    {
        /// <summary>
        /// To the brush.
        /// </summary>
        /// <param name="A">a.</param>
        /// <param name="R">The r.</param>
        /// <param name="G">The g.</param>
        /// <param name="B">The b.</param>
        /// <returns>Brush.</returns>
        public static Brush ToBrush(int A, int R, int G, int B)
        {
            return ToBrush(Color.FromArgb(A, R, G, B));
        }
        /// <summary>
        /// To the brush.
        /// </summary>
        /// <param name="R">The r.</param>
        /// <param name="G">The g.</param>
        /// <param name="B">The b.</param>
        /// <returns>Brush.</returns>
        public static Brush ToBrush(int R, int G, int B)
        {
            return ToBrush(Color.FromArgb(R, G, B));
        }
        /// <summary>
        /// To the brush.
        /// </summary>
        /// <param name="A">a.</param>
        /// <param name="C">The c.</param>
        /// <returns>Brush.</returns>
        public static Brush ToBrush(int A, Color C)
        {
            return ToBrush(Color.FromArgb(A, C));
        }
        /// <summary>
        /// To the brush.
        /// </summary>
        /// <param name="Pen">The pen.</param>
        /// <returns>Brush.</returns>
        public static Brush ToBrush(Pen Pen)
        {
            return ToBrush(Pen.Color);
        }
        /// <summary>
        /// To the brush.
        /// </summary>
        /// <param name="Color">The color.</param>
        /// <returns>Brush.</returns>
        public static Brush ToBrush(Color Color)
        {
            return new SolidBrush(Color);
        }
        /// <summary>
        /// To the pen.
        /// </summary>
        /// <param name="A">a.</param>
        /// <param name="R">The r.</param>
        /// <param name="G">The g.</param>
        /// <param name="B">The b.</param>
        /// <returns>Pen.</returns>
        public static Pen ToPen(int A, int R, int G, int B)
        {
            return ToPen(Color.FromArgb(A, R, G, B));
        }
        /// <summary>
        /// To the pen.
        /// </summary>
        /// <param name="R">The r.</param>
        /// <param name="G">The g.</param>
        /// <param name="B">The b.</param>
        /// <returns>Pen.</returns>
        public static Pen ToPen(int R, int G, int B)
        {
            return ToPen(Color.FromArgb(R, G, B));
        }
        /// <summary>
        /// To the pen.
        /// </summary>
        /// <param name="A">a.</param>
        /// <param name="C">The c.</param>
        /// <returns>Pen.</returns>
        public static Pen ToPen(int A, Color C)
        {
            return ToPen(Color.FromArgb(A, C));
        }
        /// <summary>
        /// To the pen.
        /// </summary>
        /// <param name="Color">The color.</param>
        /// <returns>Pen.</returns>
        public static Pen ToPen(Color Color)
        {
            return ToPen(new SolidBrush(Color));
        }
        /// <summary>
        /// To the pen.
        /// </summary>
        /// <param name="Brush">The brush.</param>
        /// <returns>Pen.</returns>
        public static Pen ToPen(SolidBrush Brush)
        {
            return new Pen(Brush);
        }

        /// <summary>
        /// Class CornerStyle.
        /// </summary>
        public class CornerStyle
        {
            /// <summary>
            /// The top left
            /// </summary>
            public bool TopLeft;
            /// <summary>
            /// The top right
            /// </summary>
            public bool TopRight;
            /// <summary>
            /// The bottom left
            /// </summary>
            public bool BottomLeft;
            /// <summary>
            /// The bottom right
            /// </summary>
            public bool BottomRight;
        }

        /// <summary>
        /// Advs the rect.
        /// </summary>
        /// <param name="Rectangle">The rectangle.</param>
        /// <param name="CornerStyle">The corner style.</param>
        /// <param name="Curve">The curve.</param>
        /// <returns>GraphicsPath.</returns>
        public static GraphicsPath AdvRect(Rectangle Rectangle, CornerStyle CornerStyle, int Curve)
        {
            GraphicsPath functionReturnValue = default(GraphicsPath);
            functionReturnValue = new GraphicsPath();
            int ArcRectangleWidth = Curve * 2;

            if (CornerStyle.TopLeft)
            {
                functionReturnValue.AddArc(new Rectangle(Rectangle.X, Rectangle.Y, ArcRectangleWidth, ArcRectangleWidth), -180, 90);
            }
            else
            {
                functionReturnValue.AddLine(Rectangle.X, Rectangle.Y, Rectangle.X + ArcRectangleWidth, Rectangle.Y);
            }

            if (CornerStyle.TopRight)
            {
                functionReturnValue.AddArc(new Rectangle(Rectangle.Width - ArcRectangleWidth + Rectangle.X, Rectangle.Y, ArcRectangleWidth, ArcRectangleWidth), -90, 90);
            }
            else
            {
                functionReturnValue.AddLine(Rectangle.X + Rectangle.Width, Rectangle.Y, Rectangle.X + Rectangle.Width, Rectangle.Y + ArcRectangleWidth);
            }

            if (CornerStyle.BottomRight)
            {
                functionReturnValue.AddArc(new Rectangle(Rectangle.Width - ArcRectangleWidth + Rectangle.X, Rectangle.Height - ArcRectangleWidth + Rectangle.Y, ArcRectangleWidth, ArcRectangleWidth), 0, 90);
            }
            else
            {
                functionReturnValue.AddLine(Rectangle.X + Rectangle.Width, Rectangle.Y + Rectangle.Height, Rectangle.X + Rectangle.Width - ArcRectangleWidth, Rectangle.Y + Rectangle.Height);
            }

            if (CornerStyle.BottomLeft)
            {
                functionReturnValue.AddArc(new Rectangle(Rectangle.X, Rectangle.Height - ArcRectangleWidth + Rectangle.Y, ArcRectangleWidth, ArcRectangleWidth), 90, 90);
            }
            else
            {
                functionReturnValue.AddLine(Rectangle.X, Rectangle.Y + Rectangle.Height, Rectangle.X, Rectangle.Y + Rectangle.Height - ArcRectangleWidth);
            }

            functionReturnValue.CloseAllFigures();

            return functionReturnValue;
            return functionReturnValue;
        }

        /// <summary>
        /// Rounds the rect.
        /// </summary>
        /// <param name="Rectangle">The rectangle.</param>
        /// <param name="Curve">The curve.</param>
        /// <returns>GraphicsPath.</returns>
        public static GraphicsPath RoundRect(Rectangle Rectangle, int Curve)
        {
            GraphicsPath functionReturnValue = default(GraphicsPath);
            functionReturnValue = new GraphicsPath();
            int ArcRectangleWidth = Curve * 2;
            functionReturnValue.AddArc(new Rectangle(Rectangle.X, Rectangle.Y, ArcRectangleWidth, ArcRectangleWidth), -180, 90);
            functionReturnValue.AddArc(new Rectangle(Rectangle.Width - ArcRectangleWidth + Rectangle.X, Rectangle.Y, ArcRectangleWidth, ArcRectangleWidth), -90, 90);
            functionReturnValue.AddArc(new Rectangle(Rectangle.Width - ArcRectangleWidth + Rectangle.X, Rectangle.Height - ArcRectangleWidth + Rectangle.Y, ArcRectangleWidth, ArcRectangleWidth), 0, 90);
            functionReturnValue.AddArc(new Rectangle(Rectangle.X, Rectangle.Height - ArcRectangleWidth + Rectangle.Y, ArcRectangleWidth, ArcRectangleWidth), 90, 90);
            functionReturnValue.AddLine(new Point(Rectangle.X, Rectangle.Height - ArcRectangleWidth + Rectangle.Y), new Point(Rectangle.X, ArcRectangleWidth + Rectangle.Y));
            functionReturnValue.CloseAllFigures();
            return functionReturnValue;
            return functionReturnValue;
        }

        /// <summary>
        /// Rounds the rect.
        /// </summary>
        /// <param name="X">The x.</param>
        /// <param name="Y">The y.</param>
        /// <param name="Width">The width.</param>
        /// <param name="Height">The height.</param>
        /// <param name="Curve">The curve.</param>
        /// <returns>GraphicsPath.</returns>
        public static GraphicsPath RoundRect(int X, int Y, int Width, int Height, int Curve)
        {
            return RoundRect(new Rectangle(X, Y, Width, Height), Curve);
        }

        /// <summary>
        /// Class PillStyle.
        /// </summary>
        public class PillStyle
        {
            /// <summary>
            /// The left
            /// </summary>
            public bool Left;
            /// <summary>
            /// The right
            /// </summary>
            public bool Right;
        }

        #region Trying to add
        //public enum Styles { Left, Right }
        //private static Styles styles = Styles.Left;

        //public static GraphicsPath Pill(Rectangle Rectangle, bool left, bool right)
        //{
        //    GraphicsPath functionReturnValue = default(GraphicsPath);
        //    functionReturnValue = new GraphicsPath();

        //    DesignFunctions.PillStyle pillstyle = new DesignFunctions.PillStyle();

        //    switch (styles)
        //    {
        //        case Styles.Left:
        //            pillstyle.Left = true;
        //            if (pillstyle.Left)
        //            {
        //                functionReturnValue.AddArc(new Rectangle(Rectangle.X, Rectangle.Y, Rectangle.Height, Rectangle.Height), -270, 180);
        //            }
        //            else
        //            {
        //                functionReturnValue.AddLine(Rectangle.X, Rectangle.Y + Rectangle.Height, Rectangle.X, Rectangle.Y);
        //            }
        //            break;
        //        case Styles.Right:
        //            pillstyle.Right = true;
        //            if (pillstyle.Right)
        //            {
        //                functionReturnValue.AddArc(new Rectangle(Rectangle.X + Rectangle.Width - Rectangle.Height, Rectangle.Y, Rectangle.Height, Rectangle.Height), -90, 180);
        //            }
        //            else
        //            {
        //                functionReturnValue.AddLine(Rectangle.X + Rectangle.Width, Rectangle.Y, Rectangle.X + Rectangle.Width, Rectangle.Y + Rectangle.Height);
        //            }
        //            break;
        //        default:
        //            break;
        //    }

        //    functionReturnValue.CloseAllFigures();

        //    return functionReturnValue;
        //    return functionReturnValue;
        //} 
        #endregion

        /// <summary>
        /// Pills the specified rectangle.
        /// </summary>
        /// <param name="Rectangle">The rectangle.</param>
        /// <param name="PillStyle">The pill style.</param>
        /// <returns>GraphicsPath.</returns>
        public static GraphicsPath Pill(Rectangle Rectangle, PillStyle PillStyle)
        {
            GraphicsPath functionReturnValue = default(GraphicsPath);
            functionReturnValue = new GraphicsPath();

            if (PillStyle.Left)
            {
                functionReturnValue.AddArc(new Rectangle(Rectangle.X, Rectangle.Y, Rectangle.Height, Rectangle.Height), -270, 180);
            }
            else
            {
                functionReturnValue.AddLine(Rectangle.X, Rectangle.Y + Rectangle.Height, Rectangle.X, Rectangle.Y);
            }

            if (PillStyle.Right)
            {
                functionReturnValue.AddArc(new Rectangle(Rectangle.X + Rectangle.Width - Rectangle.Height, Rectangle.Y, Rectangle.Height, Rectangle.Height), -90, 180);
            }
            else
            {
                functionReturnValue.AddLine(Rectangle.X + Rectangle.Width, Rectangle.Y, Rectangle.X + Rectangle.Width, Rectangle.Y + Rectangle.Height);
            }

            functionReturnValue.CloseAllFigures();

            //return Pill(new Rectangle(Rectangle.X, Rectangle.Y, Rectangle.Width, Rectangle.Height), PillStyle);
            return functionReturnValue;
            //return functionReturnValue;
        }

        /// <summary>
        /// Pills the specified x.
        /// </summary>
        /// <param name="X">The x.</param>
        /// <param name="Y">The y.</param>
        /// <param name="Width">The width.</param>
        /// <param name="Height">The height.</param>
        /// <param name="PillStyle">The pill style.</param>
        /// <returns>System.Object.</returns>
        public static object Pill(int X, int Y, int Width, int Height, PillStyle PillStyle)
        {
            return Pill(new Rectangle(X, Y, Width, Height), PillStyle);
        }

    }

    static class Helper
    {

        static internal GraphicsPath RoundRect(Rectangle R, int radius)
        {
            GraphicsPath GP = new GraphicsPath();
            GP.AddArc(R.X, R.Y, radius, radius, 180, 90);
            GP.AddArc(R.Right - radius, R.Y, radius, radius, 270, 90);
            GP.AddArc(R.Right - radius, R.Bottom - radius, radius, radius, 0, 90);
            GP.AddArc(R.X, R.Bottom - radius, radius, radius, 90, 90);
            GP.CloseFigure();
            return GP;
        }

        static internal GraphicsPath RoundRect(Rectangle R, int UpperLeftCurve, int UpperRightCurve, int DownLeftCurve, int DownRightCurve)
        {
            //Curve = curve;
            //GraphicsPath P = new GraphicsPath();
            int UpperLeftCorner = UpperLeftCurve * 2;
            int UpperRightCorner = UpperRightCurve * 2;
            int DownLeftCorner = DownLeftCurve * 2;
            int DownRightCorner = DownRightCurve * 2;

            //P.AddArc(new Rectangle(R.X, R.Y, UpperLeftCorner, UpperLeftCorner), -180, 90);
            //P.AddArc(new Rectangle(R.Width - UpperRightCorner + R.X, R.Y, UpperRightCorner, UpperRightCorner), -90, 90);
            //P.AddArc(new Rectangle(R.Width - DownRightCorner + R.X, R.Height - DownRightCorner + R.Y, DownRightCorner, DownRightCorner), 0, 90);
            //P.AddArc(new Rectangle(R.X, R.Height - DownLeftCorner + R.Y, DownLeftCorner, DownLeftCorner), 90, 90);
            //P.CloseAllFigures();

            GraphicsPath GP = new GraphicsPath();
            GP.AddArc(R.X, R.Y, UpperLeftCorner, UpperLeftCorner, 180, 90);
            GP.AddArc(R.Right - UpperRightCorner, R.Y, UpperRightCorner, UpperRightCorner, 270, 90);
            GP.AddArc(R.Right - DownRightCorner, R.Bottom - DownRightCorner, DownRightCorner, DownRightCorner, 0, 90);
            GP.AddArc(R.X, R.Bottom - DownLeftCorner, DownLeftCorner, DownLeftCorner, 90, 90);
            GP.CloseFigure();
            return GP;
        }

        public static Color GreyColor(uint G)
        {
            //return Color.FromArgb(211, 211, 211);

            return SystemColors.ButtonFace;
        }

        public static void CenterString(Graphics G, string T, Font F, Color C, Rectangle R)
        {
            SizeF TS = G.MeasureString(T, F);

            using (SolidBrush B = new SolidBrush(C))
            {
                G.DrawString(T, F, B, new Point((int)R.Width / 2 - (int)(TS.Width / 2), (int)R.Height / 2 - (int)(TS.Height / 2)));
            }
        }

        public static void CenterStringTab(Graphics G, string text, Font font, Brush brush, Rectangle rect, bool shadow = false, int yOffset = 0)
        {
            SizeF textSize = G.MeasureString(text, font);
            int textX = rect.X + (int)(rect.Width / 2) - (int)(textSize.Width / 2);
            int textY = rect.Y + (int)(rect.Height / 2) - (int)(textSize.Height / 2) + yOffset;

            if (shadow)
                G.DrawString(text, font, Brushes.Black, textX + 1, textY + 1);
            G.DrawString(text, font, brush, textX, textY + 1);

        }


    }

    static class Draw
    {
        static internal StringFormat NearSF = new StringFormat
        {
            Alignment = StringAlignment.Near,
            LineAlignment = StringAlignment.Near
        };

        static internal GraphicsPath RoundRect(Rectangle R, int UpperLeftCurve, int UpperRightCurve, int DownLeftCurve, int DownRightCurve)
        {
            //Curve = curve;
            //GraphicsPath P = new GraphicsPath();
            int UpperLeftCorner = UpperLeftCurve * 2;
            int UpperRightCorner = UpperRightCurve * 2;
            int DownLeftCorner = DownLeftCurve * 2;
            int DownRightCorner = DownRightCurve * 2;

            //P.AddArc(new Rectangle(R.X, R.Y, UpperLeftCorner, UpperLeftCorner), -180, 90);
            //P.AddArc(new Rectangle(R.Width - UpperRightCorner + R.X, R.Y, UpperRightCorner, UpperRightCorner), -90, 90);
            //P.AddArc(new Rectangle(R.Width - DownRightCorner + R.X, R.Height - DownRightCorner + R.Y, DownRightCorner, DownRightCorner), 0, 90);
            //P.AddArc(new Rectangle(R.X, R.Height - DownLeftCorner + R.Y, DownLeftCorner, DownLeftCorner), 90, 90);
            //P.CloseAllFigures();

            GraphicsPath GP = new GraphicsPath();
            GP.AddArc(R.X, R.Y, UpperLeftCorner, UpperLeftCorner, 180, 90);
            GP.AddArc(R.Right - UpperRightCorner, R.Y, UpperRightCorner, UpperRightCorner, 270, 90);
            GP.AddArc(R.Right - DownRightCorner, R.Bottom - DownRightCorner, DownRightCorner, DownRightCorner, 0, 90);
            GP.AddArc(R.X, R.Bottom - DownLeftCorner, DownLeftCorner, DownLeftCorner, 90, 90);
            GP.CloseFigure();
            return GP;
        }


        public static GraphicsPath RoundRect(Rectangle Rectangle, int Curve)
        {
            GraphicsPath P = new GraphicsPath();
            int ArcRectangleWidth = Curve * 2;
            P.AddArc(new Rectangle(Rectangle.X, Rectangle.Y, ArcRectangleWidth, ArcRectangleWidth), -180, 90);
            P.AddArc(new Rectangle(Rectangle.Width - ArcRectangleWidth + Rectangle.X, Rectangle.Y, ArcRectangleWidth, ArcRectangleWidth), -90, 90);
            P.AddArc(new Rectangle(Rectangle.Width - ArcRectangleWidth + Rectangle.X, Rectangle.Height - ArcRectangleWidth + Rectangle.Y, ArcRectangleWidth, ArcRectangleWidth), 0, 90);
            P.AddArc(new Rectangle(Rectangle.X, Rectangle.Height - ArcRectangleWidth + Rectangle.Y, ArcRectangleWidth, ArcRectangleWidth), 90, 90);
            P.AddLine(new Point(Rectangle.X, Rectangle.Height - ArcRectangleWidth + Rectangle.Y), new Point(Rectangle.X, Curve + Rectangle.Y));
            return P;
        }

        public static GraphicsPath RoundRect(int X, int Y, int Width, int Height, int Curve)
        {
            Rectangle Rectangle = new Rectangle(X, Y, Width, Height);
            GraphicsPath P = new GraphicsPath();
            int ArcRectangleWidth = Curve * 2;
            P.AddArc(new Rectangle(Rectangle.X, Rectangle.Y, ArcRectangleWidth, ArcRectangleWidth), -180, 90);
            P.AddArc(new Rectangle(Rectangle.Width - ArcRectangleWidth + Rectangle.X, Rectangle.Y, ArcRectangleWidth, ArcRectangleWidth), -90, 90);
            P.AddArc(new Rectangle(Rectangle.Width - ArcRectangleWidth + Rectangle.X, Rectangle.Height - ArcRectangleWidth + Rectangle.Y, ArcRectangleWidth, ArcRectangleWidth), 0, 90);
            P.AddArc(new Rectangle(Rectangle.X, Rectangle.Height - ArcRectangleWidth + Rectangle.Y, ArcRectangleWidth, ArcRectangleWidth), 90, 90);
            P.AddLine(new Point(Rectangle.X, Rectangle.Height - ArcRectangleWidth + Rectangle.Y), new Point(Rectangle.X, Curve + Rectangle.Y));
            return P;
        }

        //Special Thanks to Aeonhack for RoundRect Functions... ;)
        public static GraphicsPath RoundRect(Rectangle rectangle, int curve, bool dummy = true)
        {
            GraphicsPath p = new GraphicsPath();
            int arcRectangleWidth = curve * 2;
            p.AddArc(new Rectangle(rectangle.X, rectangle.Y, arcRectangleWidth, arcRectangleWidth), -180, 90);
            p.AddArc(new Rectangle(rectangle.Width - arcRectangleWidth + rectangle.X, rectangle.Y, arcRectangleWidth, arcRectangleWidth), -90, 90);
            p.AddArc(new Rectangle(rectangle.Width - arcRectangleWidth + rectangle.X, rectangle.Height - arcRectangleWidth + rectangle.Y, arcRectangleWidth, arcRectangleWidth), 0, 90);
            p.AddArc(new Rectangle(rectangle.X, rectangle.Height - arcRectangleWidth + rectangle.Y, arcRectangleWidth, arcRectangleWidth), 90, 90);
            p.AddLine(new Point(rectangle.X, rectangle.Height - arcRectangleWidth + rectangle.Y), new Point(rectangle.X, curve + rectangle.Y));
            return p;
        }

        public static GraphicsPath RoundRect(int x, int y, int width, int height, int curve, bool dummy = true)
        {
            Rectangle rectangle = new Rectangle(x, y, width, height);
            GraphicsPath p = new GraphicsPath();
            int arcRectangleWidth = curve * 2;
            p.AddArc(new Rectangle(rectangle.X, rectangle.Y, arcRectangleWidth, arcRectangleWidth), -180, 90);
            p.AddArc(new Rectangle(rectangle.Width - arcRectangleWidth + rectangle.X, rectangle.Y, arcRectangleWidth, arcRectangleWidth), -90, 90);
            p.AddArc(new Rectangle(rectangle.Width - arcRectangleWidth + rectangle.X, rectangle.Height - arcRectangleWidth + rectangle.Y, arcRectangleWidth, arcRectangleWidth), 0, 90);
            p.AddArc(new Rectangle(rectangle.X, rectangle.Height - arcRectangleWidth + rectangle.Y, arcRectangleWidth, arcRectangleWidth), 90, 90);
            p.AddLine(new Point(rectangle.X, rectangle.Height - arcRectangleWidth + rectangle.Y), new Point(rectangle.X, curve + rectangle.Y));
            return p;
        }

        
        public static GraphicsPath CreateRound(int x, int y, int width, int height, int slope)
        {
            Rectangle CreateRoundRectangle = new Rectangle(x, y, width, height);
            return CreateRound(CreateRoundRectangle, slope);
        }

        public static GraphicsPath CreateRound(Rectangle r, int slope)
        {
            GraphicsPath CreateRoundPath = new GraphicsPath(FillMode.Winding);
            CreateRoundPath.AddArc(r.X, r.Y, slope, slope, 180f, 90f);
            CreateRoundPath.AddArc(r.Right - slope, r.Y, slope, slope, 270f, 90f);
            CreateRoundPath.AddArc(r.Right - slope, r.Bottom - slope, slope, slope, 0f, 90f);
            CreateRoundPath.AddArc(r.X, r.Bottom - slope, slope, slope, 90f, 90f);
            CreateRoundPath.CloseFigure();
            return CreateRoundPath;
        }

        #region "Functions"


        public static GraphicsPath RoundRec(Rectangle Rectangle, int Curve)
        {
            GraphicsPath P = new GraphicsPath();
            int ArcRectangleWidth = Curve * 2;
            P.AddArc(new Rectangle(Rectangle.X, Rectangle.Y, ArcRectangleWidth, ArcRectangleWidth), -180, 90);
            P.AddArc(new Rectangle(Rectangle.Width - ArcRectangleWidth + Rectangle.X, Rectangle.Y, ArcRectangleWidth, ArcRectangleWidth), -90, 90);
            P.AddArc(new Rectangle(Rectangle.Width - ArcRectangleWidth + Rectangle.X, Rectangle.Height - ArcRectangleWidth + Rectangle.Y, ArcRectangleWidth, ArcRectangleWidth), 0, 90);
            P.AddArc(new Rectangle(Rectangle.X, Rectangle.Height - ArcRectangleWidth + Rectangle.Y, ArcRectangleWidth, ArcRectangleWidth), 90, 90);
            P.AddLine(new Point(Rectangle.X, Rectangle.Height - ArcRectangleWidth + Rectangle.Y), new Point(Rectangle.X, Curve + Rectangle.Y));
            return P;
        }
        
        public static GraphicsPath RoundRect(dynamic x, dynamic y, dynamic w, dynamic h, dynamic r, bool TL = true, bool TR = true, bool BR = true, bool BL = true)
        {
            r = 0.3;

            GraphicsPath functionReturnValue = default(GraphicsPath);
            dynamic d = Math.Min(w, h) * r;
            dynamic xw = x + w;
            dynamic yh = y + h;
            functionReturnValue = new GraphicsPath();

            var _with1 = functionReturnValue;
            if (TL)
                _with1.AddArc(x, y, d, d, 180, 90);
            else
                _with1.AddLine(x, y, x, y);
            if (TR)
                _with1.AddArc(xw - d, y, d, d, 270, 90);
            else
                _with1.AddLine(xw, y, xw, y);
            if (BR)
                _with1.AddArc(xw - d, yh - d, d, d, 0, 90);
            else
                _with1.AddLine(xw, yh, xw, yh);
            if (BL)
                _with1.AddArc(x, yh - d, d, d, 90, 90);
            else
                _with1.AddLine(x, yh, x, yh);

            _with1.CloseFigure();
            return functionReturnValue;
        }


        #endregion

        public static Rectangle FullRectangle(Size S, bool Subtract)
        {
            if (Subtract)
            {
                return new Rectangle(0, 0, S.Width - 1, S.Height - 1);
            }
            else
            {
                return new Rectangle(0, 0, S.Width, S.Height);
            }
        }

        public static Color GreyColor(uint G)
        {
            //return Color.FromArgb(211, 211, 211);

            return SystemColors.ButtonFace;
        }

        public static void CenterString(Graphics G, string T, Font F, Color C, Rectangle R)
        {
            SizeF TS = G.MeasureString(T, F);

            using (SolidBrush B = new SolidBrush(C))
            {
                G.DrawString(T, F, B, new Point((int)R.Width / 2 - (int)(TS.Width / 2), (int)R.Height / 2 - (int)(TS.Height / 2)));
            }
        }
        // Get more free themes at ThemesVB.NET

        public static void FillRoundRect(Graphics G, Rectangle R, int Curve, Color C)
        {
            using (SolidBrush B = new SolidBrush(C))
            {
                G.FillPie(B, R.X, R.Y, Curve, Curve, 180, 90);
                G.FillPie(B, R.X + R.Width - Curve, R.Y, Curve, Curve, 270, 90);
                G.FillPie(B, R.X, R.Y + R.Height - Curve, Curve, Curve, 90, 90);
                G.FillPie(B, R.X + R.Width - Curve, R.Y + R.Height - Curve, Curve, Curve, 0, 90);
                G.FillRectangle(B, Convert.ToInt32(R.X + Curve / 2), R.Y, R.Width - Curve, Convert.ToInt32(Curve / 2));
                G.FillRectangle(B, R.X, Convert.ToInt32(R.Y + Curve / 2), R.Width, R.Height - Curve);
                G.FillRectangle(B, Convert.ToInt32(R.X + Curve / 2), Convert.ToInt32(R.Y + R.Height - Curve / 2), R.Width - Curve, Convert.ToInt32(Curve / 2));
            }

        }


        public static void DrawRoundRect(Graphics G, Rectangle R, int Curve, Color C)
        {
            using (Pen P = new Pen(C))
            {
                G.DrawArc(P, R.X, R.Y, Curve, Curve, 180, 90);
                G.DrawLine(P, Convert.ToInt32(R.X + Curve / 2), R.Y, Convert.ToInt32(R.X + R.Width - Curve / 2), R.Y);
                G.DrawArc(P, R.X + R.Width - Curve, R.Y, Curve, Curve, 270, 90);
                G.DrawLine(P, R.X, Convert.ToInt32(R.Y + Curve / 2), R.X, Convert.ToInt32(R.Y + R.Height - Curve / 2));
                G.DrawLine(P, Convert.ToInt32(R.X + R.Width), Convert.ToInt32(R.Y + Curve / 2), Convert.ToInt32(R.X + R.Width), Convert.ToInt32(R.Y + R.Height - Curve / 2));
                G.DrawLine(P, Convert.ToInt32(R.X + Curve / 2), Convert.ToInt32(R.Y + R.Height), Convert.ToInt32(R.X + R.Width - Curve / 2), Convert.ToInt32(R.Y + R.Height));
                G.DrawArc(P, R.X, R.Y + R.Height - Curve, Curve, Curve, 90, 90);
                G.DrawArc(P, R.X + R.Width - Curve, R.Y + R.Height - Curve, Curve, Curve, 0, 90);
            }

        }


        public static void CenterStringTab(Graphics G, string text, Font font, Brush brush, Rectangle rect, bool shadow = false, int yOffset = 0)
        {
            SizeF textSize = G.MeasureString(text, font);
            int textX = rect.X + (int)(rect.Width / 2) - (int)(textSize.Width / 2);
            int textY = rect.Y + (int)(rect.Height / 2) - (int)(textSize.Height / 2) + yOffset;

            if (shadow)
                G.DrawString(text, font, Brushes.Black, textX + 1, textY + 1);
            G.DrawString(text, font, brush, textX, textY + 1);

        }
        
        public static GraphicsPath TopRoundRect(Rectangle rect, int slope)
        {
            GraphicsPath gp = new GraphicsPath();
            int arcWidth = slope * 2;
            gp.AddArc(new Rectangle(rect.X, rect.Y, arcWidth, arcWidth), -180, 90);
            gp.AddArc(new Rectangle(rect.Width - arcWidth + rect.X, rect.Y, arcWidth, arcWidth), -90, 90);
            gp.AddLine(new Point(rect.X + rect.Width, rect.Y + arcWidth), new Point(rect.X + rect.Width, rect.Y + rect.Height));
            gp.AddLine(new Point(rect.X + rect.Width, rect.Y + rect.Height), new Point(rect.X, rect.Y + rect.Height));
            gp.AddLine(new Point(rect.X, rect.Y + rect.Height), new Point(rect.X, rect.Y + arcWidth));
            gp.CloseAllFigures();
            return gp;
        }

        public static GraphicsPath LeftRoundRect(Rectangle rect, int slope)
        {
            GraphicsPath gp = new GraphicsPath();
            int arcWidth = slope * 2;
            gp.AddArc(new Rectangle(rect.X, rect.Y, arcWidth, arcWidth), -180, 90);
            gp.AddLine(new Point(rect.X + arcWidth, rect.Y), new Point(rect.Width, rect.Y));
            gp.AddLine(new Point(rect.X + rect.Width, rect.Y), new Point(rect.X + rect.Width, rect.Y + rect.Height));
            gp.AddLine(new Point(rect.X + rect.Width, rect.Y + rect.Height), new Point(rect.X + arcWidth, rect.Y + rect.Height));
            gp.AddArc(new Rectangle(rect.X, rect.Height - arcWidth + rect.Y, arcWidth, arcWidth), 90, 90);
            gp.CloseAllFigures();
            return gp;
        }

        public static GraphicsPath RoundedRectangle(int x, int y, int width, int height, int cornerwidth, int PenWidth)
        {
            GraphicsPath p = new GraphicsPath();
            p.StartFigure();
            p.AddArc(new Rectangle(x, y, cornerwidth, cornerwidth), 180, 90);
            p.AddLine(cornerwidth, y, width - cornerwidth - PenWidth, y);

            p.AddArc(new Rectangle(width - cornerwidth - PenWidth, y, cornerwidth, cornerwidth), -90, 90);
            p.AddLine(width - PenWidth, cornerwidth, width - PenWidth, height - cornerwidth - PenWidth);

            p.AddArc(new Rectangle(width - cornerwidth - PenWidth, height - cornerwidth - PenWidth, cornerwidth, cornerwidth), 0, 90);
            p.AddLine(width - cornerwidth - PenWidth, height - PenWidth, cornerwidth, height - PenWidth);

            p.AddArc(new Rectangle(x, height - cornerwidth - PenWidth, cornerwidth, cornerwidth), 90, 90);
            p.CloseFigure();

            return p;
        }

        public static void DrawRoundedRectangle(Graphics g, Rectangle r, int d, Pen p)
        {
            SmoothingMode mode = g.SmoothingMode;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.DrawArc(p, r.X, r.Y, d, d, 180, 90);
            g.DrawLine(p, Convert.ToInt32(r.X + d / 2), r.Y, Convert.ToInt32(r.X + r.Width - d / 2), r.Y);
            g.DrawArc(p, r.X + r.Width - d, r.Y, d, d, 270, 90);
            g.DrawLine(p, r.X, Convert.ToInt32(r.Y + d / 2), r.X, Convert.ToInt32(r.Y + r.Height - d / 2));
            g.DrawLine(p, Convert.ToInt32(r.X + r.Width), Convert.ToInt32(r.Y + d / 2), Convert.ToInt32(r.X + r.Width), Convert.ToInt32(r.Y + r.Height - d / 2));
            g.DrawLine(p, Convert.ToInt32(r.X + d / 2), Convert.ToInt32(r.Y + r.Height), Convert.ToInt32(r.X + r.Width - d / 2), Convert.ToInt32(r.Y + r.Height));
            g.DrawArc(p, r.X, r.Y + r.Height - d, d, d, 90, 90);
            g.DrawArc(p, r.X + r.Width - d, r.Y + r.Height - d, d, d, 0, 90);
            g.SmoothingMode = mode;
        }

        //Special Thanks to Mephobia's for NoiseBrush Functions...
        public static TextureBrush NoiseBrush(Color[] colors)
        {
            Bitmap b = new Bitmap(128, 128);
            Random r = new Random(128);
            for (int x = 0; x <= b.Width - 1; x++)
            {
                for (int y = 0; y <= b.Height - 1; y++)
                {
                    b.SetPixel(x, y, colors[r.Next(colors.Length)]);
                }
            }
            TextureBrush T = new TextureBrush(b);
            b.Dispose();
            return T;
        }

        public static void Gradient(Graphics g, Color c1, Color c2, int x, int y, int width, int height)
        {
            Rectangle R = new Rectangle(x, y, width, height);
            using (LinearGradientBrush T = new LinearGradientBrush(R, c1, c2, LinearGradientMode.Vertical))
            {
                g.FillRectangle(T, R);
            }
        }

        public static void Gradient(Graphics g, Color c1, Color c2, Rectangle r)
        {
            using (LinearGradientBrush T = new LinearGradientBrush(r, c1, c2, LinearGradientMode.Vertical))
            {
                g.FillRectangle(T, r);
            }
        }

        public static void Blend(Graphics g, Color c1, Color c2, Color c3, float c, int d, int x, int y, int width, int height)
        {
            ColorBlend v = new ColorBlend(3);
            v.Colors = new Color[] {
            c1,
            c2,
            c3
        };
            v.Positions = new float[] {
            0,
            c,
            1
        };
            Rectangle R = new Rectangle(x, y, width, height);
            using (LinearGradientBrush T = new LinearGradientBrush(R, c1, c1, (LinearGradientMode)d))
            {
                T.InterpolationColors = v;
                g.FillRectangle(T, R);
            }
        }
        
        public static void BackGround(int width, int height, Graphics G)
        {
            Color P1 = Color.FromArgb(29, 25, 22);
            Color P2 = Color.FromArgb(35, 31, 28);

            for (int y = 0; y <= height; y += 4)
            {
                for (int x = 0; x <= width; x += 4)
                {
                    G.FillRectangle(new SolidBrush(P1), new Rectangle(x, y, 1, 1));
                    G.FillRectangle(new SolidBrush(P2), new Rectangle(x, y + 1, 1, 1));
                    try
                    {
                        G.FillRectangle(new SolidBrush(P1), new Rectangle(x + 2, y + 2, 1, 1));
                        G.FillRectangle(new SolidBrush(P2), new Rectangle(x + 2, y + 3, 1, 1));
                    }
                    catch
                    {
                    }
                }
            }
        }
        
        public static GraphicsPath TabControlRect(Rectangle rect, int slope)
        {
            GraphicsPath gp = new GraphicsPath();
            int arcWidth = slope * 2;
            gp.AddLine(new Point(rect.X, rect.Y), new Point(rect.X, rect.Y));
            gp.AddArc(new Rectangle(rect.Width - arcWidth + rect.X, rect.Y, arcWidth, arcWidth), -90, 90);
            gp.AddArc(new Rectangle(rect.Width - arcWidth + rect.X, rect.Height - arcWidth + rect.Y, arcWidth, arcWidth), 0, 90);
            gp.AddArc(new Rectangle(rect.X, rect.Height - arcWidth + rect.Y, arcWidth, arcWidth), 90, 90);
            gp.CloseAllFigures();
            return gp;
        }

        public static void ShadowedString(Graphics g, string s, Font font, Brush brush, Point pos)
        {
            g.DrawString(s, font, Brushes.Black, new Point(pos.X + 1, pos.Y + 1));
            g.DrawString(s, font, brush, pos);
        }

        public static Rectangle getSmallerRect(Rectangle rect, int value)
        {
            return new Rectangle(rect.X + value, rect.Y + value, rect.Width - (value * 2), rect.Height - (value * 2));
        }

        public static Color AlterColor(Color original, int amount = -20)
        {
            Color c = original;
            int a = amount;
            int r = 0;
            int g = 0;
            int b = 0;
            if (c.R + a < 0)
            {
                r = 0;
            }
            else if (c.R + a > 255)
            {
                r = 255;
            }
            else
            {
                r = c.R + a;
            }
            if (c.G + a < 0)
            {
                g = 0;
            }
            else if (c.G + a > 255)
            {
                g = 255;
            }
            else
            {
                g = c.G + a;
            }
            if (c.B + a < 0)
            {
                b = 0;
            }
            else if (c.B + a > 255)
            {
                b = 255;
            }
            else
            {
                b = c.B + a;
            }
            return Color.FromArgb(r, g, b);
        }

        static internal StringFormat CenterSF = new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center

        };

        static internal Color _FlatColor = Color.FromArgb(35, 168, 109);

        public static int RoundUp(double d)
        {
            if (d.ToString().Contains(","))
            {
                return Convert.ToInt32(d.ToString().Split(Convert.ToChar((",")), ((char)0))) + 1;
            }
            else
            {
                return Convert.ToInt32(d);
            }
        }

        public static Image CodeToImage(string Code)
        {
            return Image.FromStream(new System.IO.MemoryStream(Convert.FromBase64String(Code)));
        }

        public static SolidBrush GetBrush(Color c)
        {
            return new SolidBrush(c);
        }
        public static Pen GetPen(Color c)
        {
            return new Pen(new SolidBrush(c));
        }

        public static Brush ConvertToBrush(int R, int G, int B)
        {
            return new SolidBrush(Color.FromArgb(R, G, B));
        }

        public static Brush ConvertToBrush(Pen Pen)
        {
            return new SolidBrush(Pen.Color);
        }

        public static Brush ConvertToBrush(Color Color)
        {
            return new SolidBrush(Color);
        }

        public static Pen ConvertToPen(int R, int G, int B)
        {
            return new Pen(new SolidBrush(Color.FromArgb(R, G, B)));
        }

        public static Pen ConvertToPen(SolidBrush Brush)
        {
            return new Pen(Brush);
        }

        public static Pen ConvertToPen(Color Color)
        {
            return new Pen(new SolidBrush(Color));
        }

        public static void InnerGlow(Graphics G, Rectangle Rectangle, Color[] Colors)
        {
            int SubtractTwo = 1;
            int AddOne = 0;
            foreach (Color c_loopVariable in Colors)
            {
                Color c = c_loopVariable;
                G.DrawRectangle(new Pen(new SolidBrush(Color.FromArgb(c.R, c.B, c.G))), Rectangle.X + AddOne, Rectangle.Y + AddOne, Rectangle.Width - SubtractTwo, Rectangle.Height - SubtractTwo);
                SubtractTwo += 2;
                AddOne += 1;
            }
        }

        public static void InnerGlowRounded(Graphics G, Rectangle Rectangle, int Degree, Color[] Colors)
        {
            int SubtractTwo = 1;
            int AddOne = 0;
            foreach (Color c_loopVariable in Colors)
            {
                Color c = c_loopVariable;
                G.DrawPath(new Pen(new SolidBrush(Color.FromArgb(c.R, c.B, c.G))), Draw.RoundRect(Rectangle.X + AddOne, Rectangle.Y + AddOne, Rectangle.Width - SubtractTwo, Rectangle.Height - SubtractTwo, Degree));
                SubtractTwo += 2;
                AddOne += 1;
            }
        }

        public static Image ImageFromCode(ref String str)
        {
            byte[] imageBytes = Convert.FromBase64String(str);
            MemoryStream ms = new MemoryStream(imageBytes, 0, imageBytes.Length);
            ms.Write(imageBytes, 0, imageBytes.Length);
            Image i = Image.FromStream(ms, true);
            return i;
        }
        public static TextureBrush TiledTextureFromCode(string str)
        {
            return new TextureBrush(Draw.ImageFromCode(ref str), WrapMode.Tile);
        }

        public static GraphicsPath DrawArrow(int x, int y, bool flip)
        {
            GraphicsPath GP = new GraphicsPath();

            int W = 12;
            int H = 6;

            if (flip)
            {
                GP.AddLine(x + 1, y, x + W + 1, y);
                GP.AddLine(x + W, y, x + H, y + H - 1);
            }
            else
            {
                GP.AddLine(x, y + H, x + W, y + H);
                GP.AddLine(x + W, y + H, x + H, y);
            }

            GP.CloseFigure();
            return GP;
        }
    }


    #endregion

    public static class ColorConverter
    {
        public static int HexToRed(string HexString)
        {
            return HexToColor(HexString).R;
        }
        public static int HexToGreen(string HexString)
        {
            return HexToColor(HexString).G;
        }
        public static int HexToBlue(string HexString)
        {
            return HexToColor(HexString).B;
        }
        public static string ColorToHex(Color Color)
        {
            return string.Format("#{0}{1}{2}", Color.R.ToString("X2"), Color.G.ToString("X2"), Color.B.ToString("X2"));
        }
        public static string[] HexToRGB(string HexString)
        {
            Color tmpColor = ColorTranslator.FromHtml(HexString);
            string[] rgbArray = new string[4];
            rgbArray[0] = tmpColor.R.ToString();
            rgbArray[1] = tmpColor.G.ToString();
            rgbArray[2] = tmpColor.B.ToString();
            return rgbArray;
        }
        public static Color HexToColor(string HexString)
        {
            return ColorTranslator.FromHtml(HexString);
        }
    }

    public class ImageToCodeClass
    {
        public string ImageToCode(Bitmap Img)
        {
            return Convert.ToBase64String((byte[])System.ComponentModel.TypeDescriptor.GetConverter(Img).ConvertTo(Img, typeof(byte[])));
        }

        public Image CodeToImage(string Code)
        {
            return Image.FromStream(new System.IO.MemoryStream(Convert.FromBase64String(Code)));
        }
    }

}
