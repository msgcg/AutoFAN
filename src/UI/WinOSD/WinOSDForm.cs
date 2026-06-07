using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Windows.Forms;

namespace WinOSD
{
    internal class WinOSDForm : OSDNativeForm
    {
        public static Rectangle ScreenRect => Screen.PrimaryScreen.Bounds;
        public static Font DefaultFont { get; set; } = new Font("Arial", 22f, FontStyle.Bold);

        #region Variables
        private SolidBrush _brush;
        private System.Drawing.StringFormat _stringFormat;
        
        private System.Windows.Forms.Timer _viewClock;
        private Font _textFont;
        private System.Collections.Generic.List<FanCtrl.OSDTextElement> _elements;
        private AnimateMode _mode;
        private uint _time;
        private GraphicsPath _gp;
        #endregion

        #region Public Methods
        /// <summary>
        /// Show given text on OSD-window
        /// </summary>
        /// <param name="pt">Top-left corner of text in screen coordinates</param>
        /// <param name="alpha">Transparency of text</param>
        /// <param name="textColor">Color of text</param>
        /// <param name="textFont">Font of text</param>
        /// <param name="showTimeMSec">How long text will be remain on screen, in millisecond</param>
        /// <param name="mode">Effect to be applied. Work only if <c>time</c> greater than 0</param>
        /// <param name="time">Time, in milliseconds, for effect playing. If this equal to 0 <c>mode</c> ignored and text showed at once</param>
        /// <param name="text">Text to display</param>
        public void Show(System.Collections.Generic.List<FanCtrl.OSDTextElement> elements, Point pt, byte alpha, int showTimeMSec, AnimateMode mode, uint time)
        {
            if (this._viewClock != null)
            {
                _viewClock.Stop();
                _viewClock.Dispose();
            }
            this._elements = elements;
            this._mode = mode;
            this._time = time;
            SizeF textArea;

            if (this._stringFormat == null)
                _stringFormat = DefaultStringFormat;

            textArea = MeasureElements(elements);

            base.Location = pt;
            base.Alpha = alpha;
            base.Size = new Size(Math.Max(10, (int)Math.Ceiling(textArea.Width) + 10), Math.Max(10, (int)Math.Ceiling(textArea.Height) + 10));
            if (time > 0)
                base.ShowAnimate(mode, time);
            else
                base.Show();

            if (showTimeMSec > 0)
            {
                _viewClock = new System.Windows.Forms.Timer();
                _viewClock.Tick += viewTimer;
                _viewClock.Interval = showTimeMSec;
                _viewClock.Start();
            }
        }


        private static StringFormat DefaultStringFormat => new StringFormat()
        {
            Alignment = StringAlignment.Near,
            LineAlignment = StringAlignment.Near,
            Trimming = StringTrimming.EllipsisWord
        };

        public static SizeF MeasureElements(System.Collections.Generic.List<FanCtrl.OSDTextElement> elements)
        {
            SizeF textArea = new SizeF(0, 0);
            using (Bitmap bm = new Bitmap(250, 50))
            using (Graphics fx = Graphics.FromImage(bm))
            {
                float currentY = 0;
                foreach (var el in elements)
                {
                    using (Font f = new Font("Consolas", el.FontSize, FontStyle.Bold))
                    {
                        var s = fx.MeasureString(el.Text, f, ScreenRect.Width, DefaultStringFormat);
                        if (s.Width > textArea.Width) textArea.Width = s.Width;
                        currentY += s.Height;
                    }
                }
                textArea.Height = currentY;
            }
            return textArea;
        }
        #endregion

        #region Overrided Drawing & Path Creation
        protected override void PerformPaint(PaintEventArgs e)
        {
            if(base.Handle == IntPtr.Zero)
                return;
            Graphics g = e.Graphics;
            if(this._gp != null)
                this._gp.Dispose();
            
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            float currentY = 0;
            if (this._elements != null)
            {
                foreach (var el in this._elements)
                {
                    using (Font f = new Font("Consolas", el.FontSize, FontStyle.Bold))
                    using (Brush b = new SolidBrush(el.Color))
                    using (Brush shadowBrush = new SolidBrush(Color.Black))
                    {
                        var s = g.MeasureString(el.Text, f, base.Bound.Width, DefaultStringFormat);
                        RectangleF rect = new RectangleF(0, currentY, base.Bound.Width, s.Height);
                        RectangleF shadowRect = new RectangleF(2, currentY + 2, base.Bound.Width, s.Height);
                        
                        g.DrawString(el.Text, f, shadowBrush, shadowRect, DefaultStringFormat);
                        g.DrawString(el.Text, f, b, rect, DefaultStringFormat);
                        currentY += s.Height;
                    }
                }
            }
        }
        #endregion 

        #region Timers
        protected void viewTimer(object sender, System.EventArgs e)
        {
            this._viewClock.Stop();
            this._viewClock.Dispose();
            if(this._time > 0)
                this.HideAnimate(this._mode, this._time);
            this.Close();
        }
        #endregion
    }
}


