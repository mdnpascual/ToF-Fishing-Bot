// Code From: https://stackoverflow.com/a/66400120/1862452
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media.Media3D;

namespace ToF_Fishing_Bot
{
    public class Lens_Form : Form
    {
        private readonly Timer timer;
        private Bitmap scrBmp;
        private Graphics scrGrp;
        private bool mouseDown;

        public Lens_Form() : base()
        {
            SetStyle(
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.Opaque |
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint, true);
            UpdateStyles();

            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            TopMost = true;
            Width = 150;
            Height = 150;

            timer = new Timer() { Interval = 55, Enabled = true };
            timer.Tick += (s, e) => Invalidate();
        }

        public int ZoomFactor { get; set; } = 2;
        public bool HideCursor { get; set; } = true;
        public bool AutoClose { get; set; } = true;
        public bool NearestNeighborInterpolation { get; set; }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            var gp = new GraphicsPath();
            gp.AddEllipse(0, 0, Width, Height);
            Region = new Region(gp);

            CopyScreen();
            SetLocation();

            Capture = true;
            mouseDown = true;

            Cursor = Cursors.Cross;
            if (HideCursor) Cursor.Hide();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button == MouseButtons.Left)
            {
                mouseDown = true;
                Cursor = Cursors.Default;
                if (HideCursor) Cursor.Hide();
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            mouseDown = false;
            if (HideCursor) Cursor.Show();
            if (AutoClose) Dispose();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.KeyCode == Keys.Escape) Dispose();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (mouseDown) SetLocation();
            else CopyScreen();

            var pos = Cursor.Position;
            var cr = RectangleToScreen(ClientRectangle);
            var dY = cr.Top - Top;
            var dX = cr.Left - Left;

            e.Graphics.TranslateTransform(Width / 2, Height / 2);
            e.Graphics.ScaleTransform(ZoomFactor, ZoomFactor);
            e.Graphics.TranslateTransform(-pos.X - dX, -pos.Y - dY);
            e.Graphics.Clear(BackColor);

            if (NearestNeighborInterpolation)
            {
                e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
                e.Graphics.PixelOffsetMode = PixelOffsetMode.Half;
            }

            if (scrBmp != null) e.Graphics.DrawImage(scrBmp, 0, 0);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                timer.Dispose();
                scrBmp?.Dispose();
                scrGrp?.Dispose();
            }
            base.Dispose(disposing);
        }

        private void CopyScreen()
        {
            if (scrBmp == null)
            {
                var sz = Screen.FromControl(this).Bounds.Size;

                scrBmp = new Bitmap(sz.Width, sz.Height);
                scrGrp = Graphics.FromImage(scrBmp);
            }

            scrGrp.CopyFromScreen(Point.Empty, Point.Empty, scrBmp.Size);
        }

        private void SetLocation()
        {
            var p = Cursor.Position;

            Left = p.X - Width / 2;
            Top = p.Y - Height / 2;
        }
    }
}
