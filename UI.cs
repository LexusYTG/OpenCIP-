using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Threading.Tasks;

namespace OpenCIP
{
    // ══════════════════════════════════════════════════════════════════════
    //  PALETA GLOBAL
    // ══════════════════════════════════════════════════════════════════════
    internal static class Pal
    {
        public static readonly Color Base      = Color.FromArgb(11, 11, 15);
        public static readonly Color Surface   = Color.FromArgb(17, 17, 23);
        public static readonly Color Raised    = Color.FromArgb(23, 23, 32);
        public static readonly Color Overlay   = Color.FromArgb(30, 30, 42);
        public static readonly Color Border    = Color.FromArgb(42, 42, 58);
        public static readonly Color BorderHi  = Color.FromArgb(65, 65, 90);

        public static readonly Color Cyan      = Color.FromArgb(0,   210, 255);
        public static readonly Color Magenta   = Color.FromArgb(200,  50, 255);
        public static readonly Color Amber     = Color.FromArgb(255, 165,   0);
        public static readonly Color Emerald   = Color.FromArgb(0,   210, 120);
        public static readonly Color Coral     = Color.FromArgb(255,  80,  80);

        public static readonly Color TextPrimary   = Color.FromArgb(225, 225, 235);
        public static readonly Color TextSecondary = Color.FromArgb(130, 130, 155);
        public static readonly Color TextMuted     = Color.FromArgb(68,  68,  90);

        public static Color Alpha(Color c, int a) => Color.FromArgb(a, c.R, c.G, c.B);
        public static Color Lerp(Color a, Color b, float t) => Color.FromArgb(
            (int)(a.R + (b.R - a.R) * t),
            (int)(a.G + (b.G - a.G) * t),
            (int)(a.B + (b.B - a.B) * t));
    }

    // ══════════════════════════════════════════════════════════════════════
    //  GFX HELPERS
    // ══════════════════════════════════════════════════════════════════════
    internal static class Gfx
    {
        public static GraphicsPath Pill(Rectangle r, int radius)
        {
            var p = new GraphicsPath();
            int d = radius * 2;
            p.AddArc(r.X,             r.Y,              d, d, 180, 90);
            p.AddArc(r.Right - d,     r.Y,              d, d, 270, 90);
            p.AddArc(r.Right - d,     r.Bottom - d,     d, d,   0, 90);
            p.AddArc(r.X,             r.Bottom - d,     d, d,  90, 90);
            p.CloseFigure();
            return p;
        }

        public static void FillPill(Graphics g, Brush b, Rectangle r, int radius)
        { using var p = Pill(r, radius); g.FillPath(b, p); }

        public static void DrawPill(Graphics g, Pen pen, Rectangle r, int radius)
        { using var p = Pill(r, radius); g.DrawPath(pen, p); }

        public static void Glow(Graphics g, Rectangle r, Color col, int radius, int passes = 3)
        {
            for (int i = passes; i >= 1; i--)
            {
                var inf = Rectangle.Inflate(r, i * 2, i * 2);
                int alpha = 40 / i;
                using var pen = new Pen(Color.FromArgb(alpha, col), 2f);
                DrawPill(g, pen, inf, radius + i * 2);
            }
        }

        public static void SetHQ(Graphics g)
        {
            g.SmoothingMode         = SmoothingMode.AntiAlias;
            g.TextRenderingHint     = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            g.InterpolationMode     = InterpolationMode.HighQualityBicubic;
            g.CompositingQuality    = CompositingQuality.HighQuality;
        }
    }

    // ══════════════════════════════════════════════════════════════════════
    //  NEON LABEL — texto con halo luminoso
    // ══════════════════════════════════════════════════════════════════════
    internal class NeonLabel : Control
    {
        public Color GlowColor { get; set; } = Pal.Cyan;
        public float GlowStrength { get; set; } = 0.6f;
        public ContentAlignment TextAlign { get; set; } = ContentAlignment.MiddleLeft;

        public NeonLabel()
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
            BackColor = Color.Transparent;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Gfx.SetHQ(e.Graphics);
            if (string.IsNullOrEmpty(Text)) return;

            SizeF sz = e.Graphics.MeasureString(Text, Font);
            float tx = TextAlign switch
            {
                ContentAlignment.MiddleCenter => (Width - sz.Width) / 2f,
                ContentAlignment.MiddleRight  => Width - sz.Width - 2,
                _                             => 2f
            };
            float ty = (Height - sz.Height) / 2f;

            // Halo
            int glowAlpha = (int)(GlowStrength * 90);
            for (int i = 3; i >= 1; i--)
            {
                using var glowBrush = new SolidBrush(Color.FromArgb(glowAlpha / i, GlowColor));
                e.Graphics.DrawString(Text, Font, glowBrush, tx - i, ty);
                e.Graphics.DrawString(Text, Font, glowBrush, tx + i, ty);
                e.Graphics.DrawString(Text, Font, glowBrush, tx, ty - i);
                e.Graphics.DrawString(Text, Font, glowBrush, tx, ty + i);
            }
            // Texto principal
            using var mainBrush = new SolidBrush(ForeColor);
            e.Graphics.DrawString(Text, Font, mainBrush, tx, ty);
        }
    }

    // ══════════════════════════════════════════════════════════════════════
    //  TAB STRIP — tabs horizontales custom
    // ══════════════════════════════════════════════════════════════════════
    internal class TabStrip : Control
    {
        private readonly string[] _tabs;
        private int    _selected = 0;
        private int    _hovered  = -1;
        private float  _animPos  = 0f;
        private float  _animTarget = 0f;
        // FIX: was declared as _timer but referenced as _anim throughout — unified to _anim
        private readonly System.Windows.Forms.Timer _anim;

        public Color AccentColor { get; set; } = Pal.Cyan;
        public event EventHandler<int>? TabChanged;
        public int SelectedIndex => _selected;

        public TabStrip(string[] tabs)
        {
            _tabs = tabs;
            Height = 38;
            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
            Cursor = Cursors.Hand;

            // FIX: use fully-qualified System.Windows.Forms.Timer to avoid ambiguity with System.Threading.Timer
            _anim = new System.Windows.Forms.Timer { Interval = 12 };
            _anim.Tick += delegate
            {
                _animPos += (_animTarget - _animPos) * 0.22f;
                if (Math.Abs(_animPos - _animTarget) < 0.5f) { _animPos = _animTarget; _anim.Stop(); }
                Invalidate();
            };
        }

        private float TabWidth => Width / (float)_tabs.Length;

        protected override void OnMouseMove(MouseEventArgs e)
        {
            int h = (int)(e.X / TabWidth);
            if (h != _hovered) { _hovered = h; Invalidate(); }
        }
        protected override void OnMouseLeave(EventArgs e) { _hovered = -1; Invalidate(); }
        protected override void OnMouseClick(MouseEventArgs e)
        {
            int idx = (int)(e.X / TabWidth);
            if (idx < 0 || idx >= _tabs.Length) return;
            _selected = idx;
            _animTarget = _selected * TabWidth;
            _anim.Start();
            TabChanged?.Invoke(this, _selected);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            Gfx.SetHQ(g);
            g.Clear(Pal.Surface);

            // Fondo de tabs
            for (int i = 0; i < _tabs.Length; i++)
            {
                var rc = new RectangleF(i * TabWidth, 0, TabWidth, Height);
                if (i == _hovered && i != _selected)
                    g.FillRectangle(new SolidBrush(Pal.Alpha(AccentColor, 12)), rc);
            }

            // Slider animado
            var sliderRc = new RectangleF(_animPos + 2, Height - 3, TabWidth - 4, 2);
            using var sliderBrush = new LinearGradientBrush(
                new RectangleF(_animPos, Height - 3, TabWidth, 3),
                AccentColor, Pal.Alpha(AccentColor, 80), LinearGradientMode.Horizontal);
            g.FillRectangle(sliderBrush, sliderRc);

            // Glow del slider
            using var glowPen = new Pen(Pal.Alpha(AccentColor, 40), 4f);
            g.DrawLine(glowPen, _animPos + 2, Height - 2, _animPos + TabWidth - 2, Height - 2);

            // Textos
            using var fnt = new Font("Segoe UI Semibold", 8.5f, FontStyle.Bold);
            for (int i = 0; i < _tabs.Length; i++)
            {
                bool sel = i == _selected;
                var rc  = new RectangleF(i * TabWidth, 0, TabWidth, Height - 4);
                var col = sel ? AccentColor : (i == _hovered ? Pal.TextSecondary : Pal.TextMuted);
                using var tb = new SolidBrush(col);
                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString(_tabs[i].ToUpper(), fnt, tb, rc, sf);
            }

            // Línea inferior separadora
            using var sepPen = new Pen(Pal.Border, 1);
            g.DrawLine(sepPen, 0, Height - 1, Width, Height - 1);

            // Divisores verticales entre tabs
            for (int i = 1; i < _tabs.Length; i++)
            {
                int dx = (int)(i * TabWidth);
                using var dvPen = new Pen(Pal.Border, 1);
                g.DrawLine(dvPen, dx, 8, dx, Height - 8);
            }
        }

        protected override void Dispose(bool disposing) { if (disposing) _anim?.Dispose(); base.Dispose(disposing); }
    }

    // ══════════════════════════════════════════════════════════════════════
    //  ICON BUTTON — botón cuadrado con ícono SVG-style dibujado
    // ══════════════════════════════════════════════════════════════════════
    internal class IconButton : Control
    {
        public enum IconType { Dice, Clear, Save, Cancel }

        public IconType Icon { get; set; }
        public Color    AccentColor { get; set; } = Pal.Cyan;

        private bool  _hover  = false;
        private float _hoverT = 0f;
        public readonly System.Windows.Forms.Timer _anim;

        public IconButton()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
            Cursor = Cursors.Hand;
            Size   = new Size(38, 38);

            // FIX: use fully-qualified type to avoid Timer ambiguity
            _anim = new System.Windows.Forms.Timer { Interval = 12 };
            _anim.Tick += delegate
            {
                _hoverT += _hover ? 0.18f : -0.18f;
                _hoverT  = Math.Max(0f, Math.Min(1f, _hoverT));
                Invalidate();
                if ((_hover && _hoverT >= 1f) || (!_hover && _hoverT <= 0f)) _anim.Stop();
            };
        }

        protected override void OnMouseEnter(EventArgs e) { _hover = true;  _anim.Start(); }
        protected override void OnMouseLeave(EventArgs e) { _hover = false; _anim.Start(); }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            Gfx.SetHQ(g);

            var rc = new Rectangle(1, 1, Width - 3, Height - 3);
            Color bg = Pal.Lerp(Pal.Overlay, Pal.Alpha(AccentColor, 40), _hoverT);

            using (var sb = new SolidBrush(bg)) Gfx.FillPill(g, sb, rc, 7);
            using (var pen = new Pen(Pal.Lerp(Pal.Border, AccentColor, _hoverT * 0.7f), 1f))
                Gfx.DrawPill(g, pen, rc, 7);

            Color iconCol = Pal.Lerp(Pal.TextSecondary, AccentColor, _hoverT);
            using var ipen = new Pen(iconCol, 1.8f) { StartCap = LineCap.Round, EndCap = LineCap.Round };
            float cx = Width / 2f, cy = Height / 2f;

            switch (Icon)
            {
                case IconType.Dice:
                    // Dado
                    g.DrawRectangle(ipen, cx - 7, cy - 7, 14, 14);
                    void dot(float x, float y) {
                        using var db = new SolidBrush(iconCol);
                        g.FillEllipse(db, x - 1.5f, y - 1.5f, 3f, 3f);
                    }
                    dot(cx - 3.5f, cy - 3.5f); dot(cx + 3.5f, cy - 3.5f);
                    dot(cx, cy); dot(cx - 3.5f, cy + 3.5f); dot(cx + 3.5f, cy + 3.5f);
                    break;
                case IconType.Clear:
                    g.DrawLine(ipen, cx - 6, cy - 6, cx + 6, cy + 6);
                    g.DrawLine(ipen, cx + 6, cy - 6, cx - 6, cy + 6);
                    break;
                case IconType.Save:
                    g.DrawRectangle(ipen, cx - 7, cy - 7, 14, 14);
                    g.DrawLine(ipen, cx - 4, cy - 7, cx - 4, cy - 2);
                    g.DrawLine(ipen, cx + 4, cy - 7, cx + 4, cy - 2);
                    g.DrawLine(ipen, cx - 4, cy - 2, cx + 4, cy - 2);
                    g.DrawRectangle(ipen, cx - 5, cy + 1, 10, 5);
                    break;
                case IconType.Cancel:
                    var crc = Rectangle.Inflate(rc, -6, -6);
                    Gfx.DrawPill(g, ipen, crc, 999);
                    g.DrawLine(ipen, cx - 4, cy - 4, cx + 4, cy + 4);
                    g.DrawLine(ipen, cx + 4, cy - 4, cx - 4, cy + 4);
                    break;
            }
        }

        protected override void Dispose(bool disposing) { if (disposing) _anim?.Dispose(); base.Dispose(disposing); }
    }

    // ══════════════════════════════════════════════════════════════════════
    //  PRIMARY BUTTON — botón principal con pulse animado
    // ══════════════════════════════════════════════════════════════════════
    internal class PrimaryButton : Control
    {
        public Color  AccentColor { get; set; } = Pal.Cyan;
        public string SubText     { get; set; } = "";

        private bool  _hover  = false;
        private bool  _pulse  = false;
        private float _hoverT = 0f;
        private float _pulseT = 0f;
        public readonly System.Windows.Forms.Timer _animHover;
        public readonly System.Windows.Forms.Timer _animPulse;

        public PrimaryButton()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
            Cursor = Cursors.Hand;
            Height = 46;

            // FIX: use fully-qualified type to avoid Timer ambiguity
            _animHover = new System.Windows.Forms.Timer { Interval = 12 };
            _animHover.Tick += delegate
            {
                _hoverT += _hover ? 0.15f : -0.15f;
                _hoverT  = Math.Max(0f, Math.Min(1f, _hoverT));
                Invalidate();
                if ((_hover && _hoverT >= 1f) || (!_hover && _hoverT <= 0f)) _animHover.Stop();
            };

            _animPulse = new System.Windows.Forms.Timer { Interval = 16 };
            _animPulse.Tick += delegate
            {
                _pulseT = (_pulseT + 0.04f) % 1f;
                Invalidate();
            };
        }

        public void StartPulse() { _pulse = true;  _animPulse.Start(); }
        public void StopPulse()  { _pulse = false; _animPulse.Stop(); _pulseT = 0f; Invalidate(); }

        protected override void OnMouseEnter(EventArgs e) { _hover = true;  _animHover.Start(); }
        protected override void OnMouseLeave(EventArgs e) { _hover = false; _animHover.Start(); }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            Gfx.SetHQ(g);

            var rc = new Rectangle(0, 0, Width - 1, Height - 1);

            // Pulse ring
            if (_pulse)
            {
                float s = (float)Math.Sin(_pulseT * Math.PI);
                int pAlpha = (int)(s * 45);
                var prc = Rectangle.Inflate(rc, (int)(s * 4), (int)(s * 4));
                using var pPen = new Pen(Color.FromArgb(pAlpha, AccentColor), 2f);
                Gfx.DrawPill(g, pPen, prc, 8 + (int)(s * 4));
            }

            // Fondo con gradiente
            Color c1 = Pal.Lerp(
                Color.FromArgb(AccentColor.R / 5, AccentColor.G / 5, AccentColor.B / 5),
                Color.FromArgb(AccentColor.R / 3, AccentColor.G / 3, AccentColor.B / 3),
                _hoverT);
            Color c2 = Pal.Lerp(Pal.Overlay, Pal.Alpha(AccentColor, 60), _hoverT);

            using var lgb = new LinearGradientBrush(rc, c2, c1, LinearGradientMode.Vertical);
            Gfx.FillPill(g, lgb, rc, 8);

            // Borde
            float borderAlpha = 100 + _hoverT * 155;
            using var borderPen = new Pen(Color.FromArgb((int)borderAlpha, AccentColor), 1f);
            Gfx.DrawPill(g, borderPen, rc, 8);

            // Glow exterior
            if (_hoverT > 0.1f)
                Gfx.Glow(g, rc, AccentColor, 8, (int)(2 + _hoverT * 2));

            // Texto principal
            using var fMain = new Font("Segoe UI", 11f, FontStyle.Bold);
            using var tMain = new SolidBrush(Enabled ? Pal.TextPrimary : Pal.TextMuted);
            string displayText = string.IsNullOrEmpty(SubText) ? Text : Text;
            SizeF  szMain = g.MeasureString(displayText, fMain);
            float  ty = string.IsNullOrEmpty(SubText)
                ? (Height - szMain.Height) / 2f
                : Height / 2f - szMain.Height - 1;

            g.DrawString(displayText, fMain, tMain, (Width - szMain.Width) / 2f, ty);

            // Subtexto
            if (!string.IsNullOrEmpty(SubText))
            {
                using var fSub = new Font("Segoe UI", 7.5f);
                using var tSub = new SolidBrush(Pal.Alpha(AccentColor, 180));
                SizeF szSub = g.MeasureString(SubText, fSub);
                g.DrawString(SubText, fSub, tSub, (Width - szSub.Width) / 2f, Height / 2f + 2);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) { _animHover?.Dispose(); _animPulse?.Dispose(); }
            base.Dispose(disposing);
        }
    }

    // ══════════════════════════════════════════════════════════════════════
    //  PANEL TAGS
    // ══════════════════════════════════════════════════════════════════════
    public class PanelTags : Control
    {
        private readonly List<string> _tags = new();
        private readonly List<Color>  _cols = new();

        private static readonly Color[] PALETTE = {
            Pal.Cyan, Pal.Magenta, Pal.Amber, Pal.Emerald, Pal.Coral,
            Color.FromArgb(100, 180, 255)
        };

        public PanelTags()
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
            BackColor = Color.Transparent;
        }

        public void SetTags(IEnumerable<string> tags)
        {
            _tags.Clear(); _cols.Clear();
            int i = 0;
            foreach (string t in tags)
            { _tags.Add(t); _cols.Add(PALETTE[i++ % PALETTE.Length]); }
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Gfx.SetHQ(e.Graphics);
            var g = e.Graphics;
            g.Clear(Color.FromArgb(0, 0, 0, 0));

            using var f = new Font("Segoe UI", 8f);
            int x = 0, y = 0, maxH = 0;

            for (int i = 0; i < _tags.Count; i++)
            {
                SizeF sz = g.MeasureString(_tags[i], f);
                int w = (int)sz.Width + 16, h = (int)sz.Height + 8;
                if (x + w > Width && x > 0) { x = 0; y += maxH + 5; maxH = 0; }

                var rc = new Rectangle(x, y, w, h);
                using (var sb = new SolidBrush(Pal.Alpha(_cols[i], 28)))
                    Gfx.FillPill(g, sb, rc, 5);
                using (var pen = new Pen(Pal.Alpha(_cols[i], 110), 1f))
                    Gfx.DrawPill(g, pen, rc, 5);
                using (var tb = new SolidBrush(Pal.Alpha(_cols[i], 220)))
                    g.DrawString(_tags[i], f, tb, x + 8, y + 4);

                x += w + 5; maxH = Math.Max(maxH, h);
            }
        }
    }

    // ══════════════════════════════════════════════════════════════════════
    //  SLIDER CUSTOM — trackbar con estética cohesiva
    // ══════════════════════════════════════════════════════════════════════
    internal class SlimSlider : Control
    {
        public int Minimum { get; set; } = 0;
        public int Maximum { get; set; } = 100;
        private int _value = 50;
        public int Value
        {
            get => _value;
            set { _value = Math.Max(Minimum, Math.Min(Maximum, value)); Invalidate(); Scroll?.Invoke(this, EventArgs.Empty); }
        }
        public Color TrackColor  { get; set; } = Pal.Cyan;
        public event EventHandler? Scroll;

        private bool _dragging = false;

        public SlimSlider()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
            Height = 28;
            Cursor = Cursors.Hand;
        }

        private float Ratio => (float)(_value - Minimum) / (Maximum - Minimum);
        private int ThumbX => (int)(12 + Ratio * (Width - 24));

        protected override void OnMouseDown(MouseEventArgs e)
        { _dragging = true; UpdateFromMouse(e.X); }
        protected override void OnMouseMove(MouseEventArgs e)
        { if (_dragging) UpdateFromMouse(e.X); }
        protected override void OnMouseUp(MouseEventArgs e)
        { _dragging = false; }

        private void UpdateFromMouse(int mx)
        {
            float ratio = Math.Max(0f, Math.Min(1f, (mx - 12f) / (Width - 24f)));
            Value = Minimum + (int)(ratio * (Maximum - Minimum));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            Gfx.SetHQ(g);
            g.Clear(Color.Transparent);

            int cy = Height / 2, tx = ThumbX;

            // Track base
            var trackRc = new Rectangle(10, cy - 3, Width - 20, 5);
            using (var sb = new SolidBrush(Pal.Border)) Gfx.FillPill(g, sb, trackRc, 3);

            // Track fill
            var fillRc = new Rectangle(10, cy - 3, tx - 10, 5);
            if (fillRc.Width > 0)
            {
                using var lgb = new LinearGradientBrush(
                    new Rectangle(10, cy - 3, Width - 20, 5),
                    Pal.Alpha(TrackColor, 180), TrackColor, LinearGradientMode.Horizontal);
                Gfx.FillPill(g, lgb, fillRc, 3);
            }

            // Thumb
            int r = 8;
            var thumbRc = new Rectangle(tx - r, cy - r, r * 2, r * 2);
            using (var sb = new SolidBrush(Pal.Overlay)) g.FillEllipse(sb, thumbRc);
            using (var sb = new SolidBrush(TrackColor)) g.FillEllipse(sb,
                new Rectangle(tx - 5, cy - 5, 10, 10));
            using (var pen = new Pen(Pal.Alpha(TrackColor, 80), 2f)) g.DrawEllipse(pen,
                new Rectangle(tx - r, cy - r, r * 2, r * 2));

            // Glow del thumb
            using var gpen = new Pen(Pal.Alpha(TrackColor, 40), 4f);
            g.DrawEllipse(gpen, new Rectangle(tx - r - 2, cy - r - 2, r * 2 + 4, r * 2 + 4));
        }
    }

    // ══════════════════════════════════════════════════════════════════════
    //  PROGRESS BAR CUSTOM
    // ══════════════════════════════════════════════════════════════════════
    internal class NeonProgressBar : Control
    {
        private int _value = 0;
        public int Value
        {
            get => _value;
            set { _value = Math.Max(0, Math.Min(100, value)); Invalidate(); }
        }
        public Color BarColor { get; set; } = Pal.Cyan;

        public NeonProgressBar()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
            Height = 4;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Pal.Surface);

            if (_value <= 0) return;
            float w = Width * _value / 100f;

            // Glow
            using var glowPen = new Pen(Pal.Alpha(BarColor, 50), 8f);
            g.DrawLine(glowPen, 0, Height / 2f, w, Height / 2f);

            // Barra
            using var lgb = new LinearGradientBrush(
                new RectangleF(0, 0, Math.Max(1, w), Height),
                Pal.Alpha(BarColor, 180), BarColor, LinearGradientMode.Horizontal);
            g.FillRectangle(lgb, 0, 0, w, Height);

            // Brillo
            using var whitePen = new Pen(Pal.Alpha(Color.White, 80), 1f);
            g.DrawLine(whitePen, 0, 0, w, 0);
        }
    }

    // ══════════════════════════════════════════════════════════════════════
    //  VENTANA PRINCIPAL
    // ══════════════════════════════════════════════════════════════════════
    public class VentanaPrincipal : Form
    {
        // Controles
        private TextBox        txtPrompt = null!;
        private PrimaryButton  btnGenerar = null!;
        private IconButton     btnAleatorio = null!, btnLimpiar = null!;
        private PrimaryButton  btnCancelar = null!;
        private PrimaryButton  btnGuardar = null!;
        private PictureBox     canvas = null!;
        private NeonLabel      lblTitulo = null!, lblModoActual = null!;
        private Label          lblEstado = null!, lblResumen = null!, lblZoomVal = null!;
        private PanelTags      panelTags = null!;
        private NeonProgressBar barraProgreso = null!;
        private NumericUpDown  numSemilla = null!;
        private CheckBox       chkSemillaFija = null!;
        private SlimSlider     trackZoom = null!;
        private CheckBox[]     _chkAlgos = null!;
        private PrimaryButton  _btnAutoAlgo = null!;
        private TabStrip       tabStrip = null!;
        private Panel[]        tabPages = null!;
        private bool           _modoManualAlgo = false;

        // Estado
        private Bitmap?        _imagenActual;
        private ContextoVisual? _ultimoCtx;
        private bool          _generando = false;
        private bool          _cancelar  = false;

        // Layout
        private const int PANEL_W = 380;

        public VentanaPrincipal()
        {
            IniciarInterfaz();
        }

        private void IniciarInterfaz()
        {
            Text           = "OpenCIP 2.5  –  CPU Image Painter";
            Size           = new Size(1280, 720);
            MinimumSize    = new Size(600, 480);
            StartPosition  = FormStartPosition.CenterScreen;
            BackColor      = Pal.Base;
            ForeColor      = Pal.TextPrimary;
            DoubleBuffered = true;

            // ── Canvas ──────────────────────────────────────────────────────
            canvas = new PictureBox
            {
                BackColor = Pal.Base,
                SizeMode  = PictureBoxSizeMode.Zoom,
                Dock      = DockStyle.Fill
            };
            canvas.Paint += OnCanvasPaint;

            // ── Panel lateral ────────────────────────────────────────────────
            var panelIzq = new Panel
            {
                BackColor = Pal.Surface,
                Dock      = DockStyle.Left,
                Width     = PANEL_W,
            };
            panelIzq.Paint += delegate(object? s, PaintEventArgs ev)
            {
                // Borde derecho con gradiente vertical
                var rc = new Rectangle(PANEL_W - 1, 0, 1, panelIzq.Height);
                using var lgb = new LinearGradientBrush(
                    new Rectangle(PANEL_W - 2, 0, 2, Math.Max(1, panelIzq.Height)),
                    Color.Transparent, Pal.Alpha(Pal.Cyan, 50), LinearGradientMode.Vertical);
                ev.Graphics.FillRectangle(lgb, rc);
            };

            // ── Header del panel ─────────────────────────────────────────────
            var header = new Panel
            {
                Location  = new Point(0, 0),
                Size      = new Size(PANEL_W, 64),
                BackColor = Pal.Raised,
            };
            header.Paint += delegate(object? s, PaintEventArgs ev)
            {
                ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                // Línea inferior
                using var lgb = new LinearGradientBrush(
                    new Rectangle(0, 63, PANEL_W, 1),
                    Pal.Alpha(Pal.Cyan, 60), Color.Transparent, LinearGradientMode.Horizontal);
                ev.Graphics.FillRectangle(lgb, 0, 63, PANEL_W, 1);
                // Punto decorativo
                using var sb = new SolidBrush(Pal.Cyan);
                ev.Graphics.FillEllipse(sb, PANEL_W - 18, 14, 6, 6);
                using var glowPen = new Pen(Pal.Alpha(Pal.Cyan, 60), 3f);
                ev.Graphics.DrawEllipse(glowPen, PANEL_W - 20, 12, 10, 10);
            };

            lblTitulo = new NeonLabel
            {
                Text      = "OpenCIP",
                Font      = new Font("Segoe UI Black", 20f, FontStyle.Bold),
                ForeColor = Pal.Cyan,
                GlowColor = Pal.Cyan,
                GlowStrength = 0.4f,
                Location  = new Point(16, 10),
                Size      = new Size(200, 40),
            };

            var lblVer = new Label
            {
                Text      = "2.5",
                Font      = new Font("Segoe UI", 11f),
                ForeColor = Pal.Magenta,
                Location  = new Point(132, 18),
                AutoSize  = true,
                BackColor = Color.Transparent,
            };

            var lblSub = new Label
            {
                Text      = "CPU Image Painter",
                Font      = new Font("Segoe UI", 8f),
                ForeColor = Pal.TextMuted,
                Location  = new Point(17, 44),
                AutoSize  = true,
                BackColor = Color.Transparent,
            };

            header.Controls.AddRange(new Control[] { lblTitulo, lblVer, lblSub });

            // ── Tab strip ────────────────────────────────────────────────────
            tabStrip = new TabStrip(new[] { "Generar", "Ejemplos", "Avanzado" })
            {
                Location     = new Point(0, 64),
                Size         = new Size(PANEL_W, 38),
                AccentColor  = Pal.Cyan,
            };
            tabStrip.TabChanged += OnTabChanged;

            // ── Tab pages ────────────────────────────────────────────────────
            tabPages = new Panel[3];
            for (int i = 0; i < 3; i++)
            {
                tabPages[i] = new Panel
                {
                    Location  = new Point(0, 102),
                    Size      = new Size(PANEL_W, panelIzq.Height - 102 - 56),
                    BackColor = Color.Transparent,
                    Visible   = i == 0,
                    AutoScroll = true,
                };
            }

            // ── Tab 0: GENERAR ───────────────────────────────────────────────
            BuildTabGenerar(tabPages[0]);

            // ── Tab 1: EJEMPLOS ──────────────────────────────────────────────
            BuildTabEjemplos(tabPages[1]);

            // ── Tab 2: AVANZADO ──────────────────────────────────────────────
            BuildTabAvanzado(tabPages[2]);

            // ── Barra de estado ──────────────────────────────────────────────
            var statusBar = new Panel
            {
                Dock      = DockStyle.Bottom,
                Height    = 56,
                BackColor = Pal.Raised,
            };
            statusBar.Paint += delegate(object? s, PaintEventArgs ev)
            {
                using var topPen = new Pen(Pal.Border, 1);
                ev.Graphics.DrawLine(topPen, 0, 0, statusBar.Width, 0);
            };

            barraProgreso = new NeonProgressBar
            {
                Location  = new Point(0, 0),
                Size      = new Size(PANEL_W, 4),
                BarColor  = Pal.Cyan,
            };

            lblModoActual = new NeonLabel
            {
                Text         = "Listo",
                Font         = new Font("Segoe UI", 8.5f, FontStyle.Italic),
                ForeColor    = Pal.Emerald,
                GlowColor    = Pal.Emerald,
                GlowStrength = 0.25f,
                Location     = new Point(16, 10),
                Size         = new Size(PANEL_W - 20, 18),
            };

            lblEstado = new Label
            {
                Text      = "Esperando prompt…",
                Font      = new Font("Segoe UI", 8f),
                ForeColor = Pal.TextMuted,
                Location  = new Point(16, 30),
                Size      = new Size(PANEL_W - 20, 18),
                BackColor = Color.Transparent,
            };

            statusBar.Controls.AddRange(new Control[]
                { barraProgreso, lblModoActual, lblEstado });

            panelIzq.Controls.Add(header);
            panelIzq.Controls.Add(tabStrip);
            panelIzq.Controls.AddRange(tabPages);
            panelIzq.Controls.Add(statusBar);
            panelIzq.Resize += delegate { ResizeTabPages(panelIzq, statusBar); };

            Controls.Add(canvas);
            Controls.Add(panelIzq);
        }

        private void ResizeTabPages(Panel panelIzq, Panel statusBar)
        {
            int availH = panelIzq.Height - 102 - statusBar.Height;
            foreach (var tp in tabPages)
                tp.Size = new Size(PANEL_W, Math.Max(10, availH));
            statusBar.Location = new Point(0, panelIzq.Height - statusBar.Height);
        }

        // ── Tab 0: Generar ──────────────────────────────────────────────────
        private void BuildTabGenerar(Panel tab)
        {
            int y = 16;
            const int PAD = 16;
            const int W   = 348;

            // Label prompt
            var lblP = Mk.Label("Describe la imagen que querés crear:", 8.5f, Pal.TextSecondary,
                new Point(PAD, y));
            tab.Controls.Add(lblP); y += 20;

            // Textarea con borde custom
            var promptWrap = new Panel
            {
                Location  = new Point(PAD, y),
                Size      = new Size(W, 90),
                BackColor = Pal.Overlay,
            };
            promptWrap.Paint += delegate(object? s, PaintEventArgs ev)
            {
                ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                bool focused = txtPrompt?.Focused ?? false;
                Color bc = focused ? Pal.Alpha(Pal.Cyan, 160) : Pal.Border;
                using var pen = new Pen(bc, focused ? 1.5f : 1f);
                Gfx.DrawPill(ev.Graphics, pen,
                    new Rectangle(0, 0, promptWrap.Width - 1, promptWrap.Height - 1), 8);
                if (focused)
                {
                    using var gPen = new Pen(Pal.Alpha(Pal.Cyan, 35), 4f);
                    Gfx.DrawPill(ev.Graphics, gPen,
                        new Rectangle(-2, -2, promptWrap.Width + 3, promptWrap.Height + 3), 10);
                }
            };

            txtPrompt = new TextBox
            {
                Location    = new Point(8, 8),
                Size        = new Size(W - 18, 74),
                Multiline   = true,
                BackColor   = Pal.Overlay,
                ForeColor   = Pal.TextPrimary,
                BorderStyle = BorderStyle.None,
                Font        = new Font("Segoe UI", 10.5f),
                ScrollBars  = ScrollBars.Vertical,
                Text        = "IA lienzo sunset violeta oceano",
            };
            txtPrompt.KeyDown  += OnPromptKeyDown;
            txtPrompt.GotFocus  += delegate { promptWrap.Invalidate(); };
            txtPrompt.LostFocus += delegate { promptWrap.Invalidate(); };
            promptWrap.Controls.Add(txtPrompt);
            tab.Controls.Add(promptWrap); y += 98;

            // Hint
            var lblHint = Mk.Label("«IA …» activa modo autónomo  ·  Enter para generar", 7.5f,
                Pal.TextMuted, new Point(PAD, y));
            tab.Controls.Add(lblHint); y += 20;

            // Botón GENERAR + iconos
            btnGenerar = new PrimaryButton
            {
                Text        = "GENERAR",
                SubText     = "Enter  ·  Ctrl+G",
                AccentColor = Pal.Cyan,
                Location    = new Point(PAD, y),
                Size        = new Size(W - 84, 46),
            };
            btnGenerar.Click += OnGenerarClick;

            btnAleatorio = new IconButton
            {
                Icon        = IconButton.IconType.Dice,
                AccentColor = Pal.Amber,
                Location    = new Point(PAD + W - 80, y),
                Size        = new Size(36, 46),
            };
            btnAleatorio.Click += OnAleatorioClick;

            btnLimpiar = new IconButton
            {
                Icon        = IconButton.IconType.Clear,
                AccentColor = Pal.Coral,
                Location    = new Point(PAD + W - 40, y),
                Size        = new Size(36, 46),
            };
            btnLimpiar.Click += OnLimpiarClick;

            tab.Controls.AddRange(new Control[] { btnGenerar, btnAleatorio, btnLimpiar });
            y += 52;

            btnCancelar = new PrimaryButton
            {
                Text        = "CANCELAR",
                AccentColor = Pal.Coral,
                Location    = new Point(PAD, y - 52),
                Size        = new Size(W - 84, 46),
                Visible     = false,
            };
            btnCancelar.Click += OnCancelarClick;
            tab.Controls.Add(btnCancelar);

            // Divider
            tab.Controls.Add(Mk.Sep(PAD, y, W)); y += 14;

            // Semilla
            var lblSem = Mk.Label("SEMILLA", 7f, Pal.TextMuted, new Point(PAD, y));
            lblSem.Font = new Font("Segoe UI Semibold", 7f, FontStyle.Bold);
            tab.Controls.Add(lblSem);

            chkSemillaFija = new CheckBox
            {
                Text      = "Fijar",
                Location  = new Point(PAD + 200, y - 1),
                Size      = new Size(55, 18),
                ForeColor = Pal.TextMuted,
                BackColor = Color.Transparent,
                Font      = new Font("Segoe UI", 8f),
            };
            tab.Controls.Add(chkSemillaFija);
            y += 18;

            numSemilla = new NumericUpDown
            {
                Location  = new Point(PAD, y),
                Size      = new Size(160, 28),
                Minimum   = 0, Maximum = 999999, Value = 42,
                BackColor = Pal.Overlay, ForeColor = Pal.TextPrimary,
                Font      = new Font("Segoe UI", 10f),
                BorderStyle = BorderStyle.None,
            };

            var btnNewSeed = new Button
            {
                Text      = "↺  Nueva",
                Location  = new Point(PAD + 166, y),
                Size      = new Size(82, 28),
                BackColor = Pal.Overlay,
                ForeColor = Pal.TextSecondary,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 8.5f),
                Cursor    = Cursors.Hand,
            };
            btnNewSeed.FlatAppearance.BorderColor = Pal.Border;
            btnNewSeed.Click += delegate { numSemilla.Value = new Random().Next(1, 999999); };
            tab.Controls.AddRange(new Control[] { numSemilla, btnNewSeed });
            y += 36;

            tab.Controls.Add(Mk.Sep(PAD, y, W)); y += 14;

            // Zoom
            var lblZ = Mk.Label("ESCALA DE RENDERIZADO", 7f, Pal.TextMuted, new Point(PAD, y));
            lblZ.Font = new Font("Segoe UI Semibold", 7f, FontStyle.Bold);
            lblZoomVal = new Label
            {
                Text      = "1.0×",
                Font      = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Pal.Cyan,
                Location  = new Point(PAD + W - 36, y),
                Size      = new Size(36, 16),
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleRight,
            };
            tab.Controls.AddRange(new Control[] { lblZ, lblZoomVal });
            y += 18;

            trackZoom = new SlimSlider
            {
                Location   = new Point(PAD, y),
                Size       = new Size(W, 28),
                Minimum    = 1, Maximum = 40, Value = 10,
                TrackColor = Pal.Cyan,
            };
            trackZoom.Scroll += OnZoomScroll;
            tab.Controls.Add(trackZoom); y += 36;

            tab.Controls.Add(Mk.Sep(PAD, y, W)); y += 14;

            // Guardar
            btnGuardar = new PrimaryButton
            {
                Text        = "Guardar imagen",
                SubText     = "PNG · JPG · BMP",
                AccentColor = Pal.Emerald,
                Location    = new Point(PAD, y),
                Size        = new Size(W, 46),
                Enabled     = false,
            };
            btnGuardar.Click += OnGuardarClick;
            tab.Controls.Add(btnGuardar); y += 54;

            tab.Controls.Add(Mk.Sep(PAD, y, W)); y += 14;

            // Tags
            var lblT = Mk.Label("PALABRAS INTERPRETADAS", 7f, Pal.TextMuted, new Point(PAD, y));
            lblT.Font = new Font("Segoe UI Semibold", 7f, FontStyle.Bold);
            tab.Controls.Add(lblT); y += 18;

            panelTags = new PanelTags
            {
                Location = new Point(PAD, y),
                Size     = new Size(W, 58),
            };
            tab.Controls.Add(panelTags); y += 64;

            lblResumen = new Label
            {
                Text      = "",
                Font      = new Font("Segoe UI", 8f, FontStyle.Italic),
                ForeColor = Pal.TextMuted,
                Location  = new Point(PAD, y),
                Size      = new Size(W, 36),
                BackColor = Color.Transparent,
            };
            tab.Controls.Add(lblResumen);
        }

        // ── Tab 1: Ejemplos ─────────────────────────────────────────────────
        private void BuildTabEjemplos(Panel tab)
        {
            string[][] cats = {
                new[]{ "LIENZO", "IA lienzo oceano","IA lienzo bosque niebla","IA lienzo flores primavera",
                    "IA lienzo montana nevada","IA lienzo desierto dunas","IA lienzo noche luna",
                    "IA lienzo selva tropical","IA lienzo tundra aurora","IA lienzo volcan oscuro",
                    "IA lienzo lago montaña","IA lienzo pantano niebla","IA lienzo playa caribe","IA lienzo cañon rojizo" },
                new[]{ "3D ENTORNOS", "terreno 3d verde","planeta 3d espacio oscuro","superficie alien 3d oscuro",
                    "cueva 3d neon oscuro","ciudad 3d oscuro neon","canon 3d rojizo",
                    "oceano 3d tranquilo","nebulosa 3d morado cyan","tormenta 3d rayos","esferas 3d metalico" },
                new[]{ "ACUARELA", "acuarela bosque verde","acuarela flores pastel",
                    "acuarela oceano azul","acuarela montana nieve","acuarela noche violeta" },
                new[]{ "ALGORITMOS", "IA fractal neon oscuro","IA nebulosa purpura","fractal julia ia caos",
                    "plasma oscuro ia neon","voronoi celular ia","fluido turbulento ia",
                    "onda interferencia ia","minecraft bloques verde","red neural cppn ia","geometrico simetrico ia" },
                new[]{ "DIBUJO", "lapiz montana atardecer","lapiz bosque niebla","lapiz oceano sin sol",
                    "lapiz ciudad noche","lapicera montana nevada","lapicera bosque lluvia",
                    "carbon paisaje dramatico","sketch oceano islas" },
            };
            Color[] catColors = { Pal.Emerald, Pal.Cyan, Pal.Amber, Pal.Magenta, Color.FromArgb(160,120,80) };

            var scroll = new Panel
            {
                Dock       = DockStyle.Fill,
                AutoScroll = true,
                BackColor  = Color.Transparent,
            };
            tab.Controls.Add(scroll);

            int y = 12;
            for (int ci = 0; ci < cats.Length; ci++)
            {
                string[] cat = cats[ci];
                var lblCat = new Label
                {
                    Text      = cat[0],
                    Font      = new Font("Segoe UI Semibold", 7.5f, FontStyle.Bold),
                    ForeColor = catColors[ci],
                    Location  = new Point(14, y),
                    AutoSize  = true,
                    BackColor = Color.Transparent,
                };
                scroll.Controls.Add(lblCat); y += 20;

                int col = 0, bw = 166, bh = 26, gap = 4;
                foreach (string ej in cat.Skip(1))
                {
                    string cap = ej;
                    var btn = new Button
                    {
                        Text      = cap,
                        Size      = new Size(bw, bh),
                        Location  = new Point(14 + col * (bw + gap), y),
                        BackColor = Pal.Overlay,
                        ForeColor = Color.FromArgb(160, 160, 185),
                        FlatStyle = FlatStyle.Flat,
                        Font      = new Font("Segoe UI", 7.5f),
                        Cursor    = Cursors.Hand,
                        Tag       = cap,
                        TextAlign = ContentAlignment.MiddleLeft,
                        Padding   = new Padding(5, 0, 0, 0),
                    };
                    btn.FlatAppearance.BorderColor           = Pal.Border;
                    btn.FlatAppearance.MouseOverBackColor    = Color.FromArgb(38, 38, 55);
                    btn.FlatAppearance.BorderSize            = 1;
                    btn.Click += OnEjemploClick;
                    scroll.Controls.Add(btn);

                    col++;
                    if (col >= 2) { col = 0; y += bh + gap; }
                }
                if (col != 0) y += bh + gap;
                y += 10;
            }
        }

        // ── Tab 2: Avanzado ─────────────────────────────────────────────────
        private void BuildTabAvanzado(Panel tab)
        {
            int y = 14;
            const int PAD = 14, W = 350;

            var lblAlgoTit = Mk.Label("OVERRIDE DE ALGORITMO", 7f, Pal.TextMuted, new Point(PAD, y));
            lblAlgoTit.Font = new Font("Segoe UI Semibold", 7f, FontStyle.Bold);
            tab.Controls.Add(lblAlgoTit);

            _btnAutoAlgo = new PrimaryButton
            {
                Text        = "AUTO",
                AccentColor = Pal.Emerald,
                Location    = new Point(PAD + 230, y - 2),
                Size        = new Size(110, 24),
            };
            _btnAutoAlgo.Click += OnAutoAlgoClick;
            tab.Controls.Add(_btnAutoAlgo);
            y += 22;

            var lblHint = Mk.Label("Seleccionar override ignora el prompt para elegir algoritmo", 7.5f,
                Pal.TextMuted, new Point(PAD, y));
            tab.Controls.Add(lblHint); y += 18;

            string[] algoNombres = {
                "Perlin","Fractal","Fluido","Geométrico","Voronoi",
                "Onda","Nebulosa","Plasma","Voxel","RedNeural","3D","Acuarela","Lápiz"
            };
            _chkAlgos = new CheckBox[algoNombres.Length];

            var panAlgos = new Panel
            {
                Location  = new Point(PAD, y),
                Size      = new Size(W, 160),
                BackColor = Pal.Raised,
            };
            panAlgos.Paint += delegate(object? s, PaintEventArgs ev)
            {
                using var pen = new Pen(Pal.Border, 1);
                Gfx.DrawPill(ev.Graphics, pen,
                    new Rectangle(0, 0, panAlgos.Width - 1, panAlgos.Height - 1), 6);
            };

            for (int ai = 0; ai < algoNombres.Length; ai++)
            {
                _chkAlgos[ai] = new CheckBox
                {
                    Text      = algoNombres[ai],
                    Location  = new Point(8 + ai % 2 * 172, 4 + ai / 2 * 26),
                    Size      = new Size(166, 24),
                    ForeColor = Color.FromArgb(175, 175, 200),
                    BackColor = Color.Transparent,
                    Font      = new Font("Segoe UI", 8.5f),
                    Tag       = (AlgoritmoBase)ai,
                };
                _chkAlgos[ai].CheckedChanged += OnAlgoCheckChanged;
                panAlgos.Controls.Add(_chkAlgos[ai]);
            }
            tab.Controls.Add(panAlgos); y += 168;
        }

        // ── Helpers de construcción ─────────────────────────────────────────
        private static class Mk
        {
            public static Label Label(string text, float size, Color color, Point loc)
            {
                return new Label
                {
                    Text      = text,
                    Font      = new Font("Segoe UI", size),
                    ForeColor = color,
                    Location  = loc,
                    AutoSize  = true,
                    BackColor = Color.Transparent,
                };
            }
            public static Panel Sep(int x, int y, int w)
            {
                return new Panel { Location = new Point(x, y), Size = new Size(w, 1), BackColor = Pal.Border };
            }
        }

        // ══════════════════════════════════════════════════════════════════
        //  EVENTOS
        // ══════════════════════════════════════════════════════════════════

        private void OnTabChanged(object? sender, int idx)
        {
            for (int i = 0; i < tabPages.Length; i++)
                tabPages[i].Visible = i == idx;
        }

        private void OnAlgoCheckChanged(object? sender, EventArgs e)
        {
            _modoManualAlgo = _chkAlgos.Any(c => c.Checked);
            _btnAutoAlgo.AccentColor = _modoManualAlgo ? Pal.Amber : Pal.Emerald;
            _btnAutoAlgo.Text        = _modoManualAlgo ? "MANUAL" : "AUTO";
            _btnAutoAlgo.Invalidate();
        }

        private void OnAutoAlgoClick(object? sender, EventArgs e)
        {
            foreach (var c in _chkAlgos) c.Checked = false;
            _modoManualAlgo            = false;
            _btnAutoAlgo.AccentColor   = Pal.Emerald;
            _btnAutoAlgo.Text          = "AUTO";
            _btnAutoAlgo.Invalidate();
        }

        private void OnZoomScroll(object? sender, EventArgs e)
        {
            lblZoomVal.Text = (trackZoom.Value / 10.0).ToString("0.0") + "×";
        }

        private void OnEjemploClick(object? sender, EventArgs e)
        {
            if (sender is Button btn)
            {
                txtPrompt.Text = btn.Tag?.ToString() ?? "";
                tabStrip.GetType()
                    .GetField("_selected", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.SetValue(tabStrip, 0);
                OnTabChanged(null, 0);
                tabStrip.Invalidate();
                IniciarGeneracion();
            }
        }

        private void OnPromptKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && !e.Shift)
            { e.SuppressKeyPress = true; IniciarGeneracion(); }
        }

        private void OnGenerarClick(object? sender, EventArgs e) => IniciarGeneracion();
        private void OnCancelarClick(object? sender, EventArgs e) { _cancelar = true; lblEstado.Text = "Cancelando…"; }

        private void OnAleatorioClick(object? sender, EventArgs e)
        {
            string[] temas = {
                "IA lienzo sunset naranja dorado","IA lienzo bosque verde niebla ia",
                "IA lienzo flores primavera pastel","IA lienzo montana nevada oscuro",
                "IA lienzo desierto calido dunas","IA lienzo noche luna estrellas",
                "IA lienzo oceano violeta ia","acuarela flores rojo amarillo",
                "acuarela bosque verde azul ia","acuarela nocturno violeta ia",
                "terreno 3d verde oscuro","planeta 3d espacio oscuro neon",
                "cueva 3d neon oscuro cian","ciudad 3d oscuro neon violeta",
                "IA fractal 3d neon oscuro","IA espacio nebulosa purpura",
                "fractal julia ia neon caos","plasma oscuro ia caos neon",
                "minecraft mundo bloques verde","IA render 3d esfera metalico"
            };
            txtPrompt.Text = temas[new Random().Next(temas.Length)];
            IniciarGeneracion();
        }

        private void OnLimpiarClick(object? sender, EventArgs e) { txtPrompt.Text = ""; txtPrompt.Focus(); }

        private void OnGuardarClick(object? sender, EventArgs e)
        {
            if (_imagenActual == null) return;
            using var dlg = new SaveFileDialog
            {
                Filter           = "PNG (*.png)|*.png|JPEG (*.jpg)|*.jpg|BMP (*.bmp)|*.bmp",
                FileName         = $"OpenCIP_{numSemilla.Value}_{DateTime.Now:yyyyMMdd_HHmmss}.png",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
            };
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                ImageFormat fmt = Path.GetExtension(dlg.FileName).ToLower() switch
                {
                    ".jpg" => ImageFormat.Jpeg,
                    ".bmp" => ImageFormat.Bmp,
                    _      => ImageFormat.Png,
                };
                _imagenActual.Save(dlg.FileName, fmt);
                lblEstado.Text = $"Guardado: {Path.GetFileName(dlg.FileName)}";
                lblModoActual.ForeColor    = Pal.Emerald;
                lblModoActual.GlowColor    = Pal.Emerald;
                lblModoActual.Text         = "✓  Archivo guardado";
            }
        }

        private void OnCanvasPaint(object? sender, PaintEventArgs e)
        {
            if (canvas.Image == null) DibujarPlaceholder(e.Graphics, canvas.ClientRectangle);
        }

        // ══════════════════════════════════════════════════════════════════
        //  GENERACIÓN
        // ══════════════════════════════════════════════════════════════════
        private async void IniciarGeneracion()
        {
            if (_generando) return;
            _generando = true;
            _cancelar  = false;

            string prompt  = txtPrompt.Text.Trim();
            int    semilla = chkSemillaFija.Checked
                ? (int)numSemilla.Value
                : new Random().Next(1, 999999);
            numSemilla.Value = semilla;

            _ultimoCtx        = InterpretadorPrompt.Interpretar(prompt, semilla);
            _ultimoCtx.Escala *= trackZoom.Value / 10.0;

            if (_modoManualAlgo)
            {
                var algos  = new List<AlgoritmoBase>();
                var pesos  = new List<float>();
                for (int i = 0; i < _chkAlgos.Length; i++)
                    if (_chkAlgos[i].Checked) { algos.Add((AlgoritmoBase)i); pesos.Add(1f); }

                if (algos.Count > 0)
                {
                    _ultimoCtx.ModoLienzo      = false;
                    _ultimoCtx.Algoritmos       = algos;
                    _ultimoCtx.PesosAlgoritmos  = pesos;
                    float tot = pesos.Sum();
                    for (int i = 0; i < pesos.Count; i++) pesos[i] /= tot;
                    bool s3D = algos.Count == 1 && algos[0] == AlgoritmoBase.Escena3D;
                    bool sRN = algos.Count == 1 && algos[0] == AlgoritmoBase.RedNeuralCPPN;
                    _ultimoCtx.EsProgresivo = s3D || sRN;
                }
            }

            // Modo label
            string modoStr = _ultimoCtx switch
            {
                { ModoLienzo: true }  => "Lienzo IA" + (_ultimoCtx.LienzoPostProceso ? " + post-proceso" : ""),
                _ when _ultimoCtx.Algoritmos.Contains(AlgoritmoBase.Acuarela)        => "Acuarela",
                _ when _ultimoCtx.Algoritmos.Contains(AlgoritmoBase.DibujoLapiz)     =>
                    _ultimoCtx.EstiloLapiz switch {
                        "lapicera" => "Lapicera", "carbon" => "Carbón", _ => "Lápiz" },
                _ when _ultimoCtx.Algoritmos.Contains(AlgoritmoBase.RedNeuralCPPN)   => "Red Neural CPPN",
                _ when _ultimoCtx.Algoritmos.Contains(AlgoritmoBase.Escena3D)        => "Escena 3D",
                _ when _ultimoCtx.Algoritmos.Contains(AlgoritmoBase.MundoVoxelMinecraft) => "Voxel Minecraft",
                _ when _ultimoCtx.ModoIAInicio                                        => "IA Autónoma",
                _                                                                     => "Estándar (paralelo CPU)",
            };

            lblModoActual.Text       = modoStr;
            lblModoActual.ForeColor  = Pal.Amber;
            lblModoActual.GlowColor  = Pal.Amber;

            panelTags.SetTags(_ultimoCtx.PalabrasDetectadas.Take(20));
            lblResumen.Text = string.IsNullOrWhiteSpace(_ultimoCtx.ResumenVisual)
                ? "" : "→ " + _ultimoCtx.ResumenVisual.Trim();

            // UI estado generando
            btnGenerar.Visible   = false;
            btnCancelar.Visible  = true;
            btnGuardar.Enabled   = false;
            barraProgreso.Value  = 0;
            btnGenerar.StopPulse();

            string promptCorto = prompt.Length > 48 ? prompt[..48] + "…" : prompt;
            lblEstado.Text = $"«{promptCorto}»";

            var prog = new Progress<int>(p =>
            {
                if (IsDisposed) return;
                try { Invoke((Action)(() =>
                {
                    barraProgreso.Value = Math.Min(100, p);
                    lblEstado.Text      = $"Generando… {p}%";
                })); } catch { }
            });

            Action<Bitmap> actualizarCanvas = bmp =>
            {
                if (IsDisposed) return;
                try { BeginInvoke((Action)(() =>
                {
                    if (!IsDisposed) { canvas.Image = bmp; canvas.Refresh(); }
                })); } catch { }
            };

            try
            {
                int w = Math.Max(canvas.Width,  1280);
                int h = Math.Max(canvas.Height, 720);
                if (_ultimoCtx.Algoritmos.Contains(AlgoritmoBase.Escena3D) ||
                    _ultimoCtx.Algoritmos.Contains(AlgoritmoBase.RedNeuralCPPN))
                    { w = Math.Min(w, 1280); h = Math.Min(h, 720); }

                ContextoVisual ctx = _ultimoCtx;
                bool cancelRef     = false;

                _imagenActual = _ultimoCtx.EsProgresivo
                    ? await Task.Run(() => MotorOpenCIP.RenderizarProgresivo(w, h, ctx, prog, actualizarCanvas, ref cancelRef))
                    : await Task.Run(() => MotorOpenCIP.Renderizar(w, h, ctx, prog));

                canvas.Image = _imagenActual;

                if (_ultimoCtx.ModoIAPostProceso && _imagenActual != null)
                {
                    lblEstado.Text = "Post-procesando con IA…";
                    var clon = new Bitmap(_imagenActual);
                    canvas.Image = null; canvas.Refresh();
                    var result   = await Task.Run(() => PostProcesadorIA.Aplicar(clon, _ultimoCtx));
                    clon.Dispose();
                    _imagenActual = result;
                    canvas.Image  = _imagenActual;
                }

                btnGuardar.Enabled = true;

                string algoStr = string.Join("+",
                    _ultimoCtx.Algoritmos.Select(a => a.ToString()[..Math.Min(5, a.ToString().Length)]));

                lblModoActual.Text      = $"✓  {modoStr}";
                lblModoActual.ForeColor = Pal.Emerald;
                lblModoActual.GlowColor = Pal.Emerald;
                lblEstado.Text          = $"{w}×{h}px  ·  semilla {semilla}  ·  [{algoStr}]";
                barraProgreso.Value     = 100;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al generar:\n" + ex.Message, "OpenCIP",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblModoActual.Text      = "✕  Error";
                lblModoActual.ForeColor = Pal.Coral;
                lblModoActual.GlowColor = Pal.Coral;
                lblEstado.Text          = ex.Message[..Math.Min(60, ex.Message.Length)];
            }
            finally
            {
                _generando          = false;
                _cancelar           = false;
                btnGenerar.Visible  = true;
                btnCancelar.Visible = false;
            }
        }

        // ══════════════════════════════════════════════════════════════════
        //  PLACEHOLDER CANVAS
        // ══════════════════════════════════════════════════════════════════
        private void DibujarPlaceholder(Graphics g, Rectangle r)
        {
            Gfx.SetHQ(g);
            g.Clear(Pal.Base);

            // Grid puntillada sutil
            for (int x = 0; x < r.Width; x += 40)
                for (int y = 0; y < r.Height; y += 40)
                {
                    using var sb = new SolidBrush(Color.FromArgb(18, 18, 26));
                    g.FillEllipse(sb, x - 1, y - 1, 2, 2);
                }

            // Glow central
            float cx = r.Width / 2f, cy = r.Height / 2f;
            using (var gp = new GraphicsPath())
            {
                gp.AddEllipse(cx - 500, cy - 350, 1000, 700);
                using var pgb = new PathGradientBrush(gp)
                {
                    CenterColor    = Color.FromArgb(14, Pal.Cyan.R, Pal.Cyan.G, Pal.Cyan.B),
                    SurroundColors = new[] { Color.Transparent },
                };
                g.FillPath(pgb, gp);
            }

            // Título principal con sombra
            using (var fT = new Font("Segoe UI Black", 58, FontStyle.Bold))
            {
                const string title = "OpenCIP";
                SizeF szT = g.MeasureString(title, fT);
                float tx  = cx - szT.Width  / 2f;
                float ty  = cy - szT.Height / 2f - 22;

                // Halo
                for (int i = 4; i >= 1; i--)
                    using (var hb = new SolidBrush(Color.FromArgb(12 * (5 - i), Pal.Cyan)))
                    {
                        g.DrawString(title, fT, hb, tx - i, ty);
                        g.DrawString(title, fT, hb, tx + i, ty);
                        g.DrawString(title, fT, hb, tx, ty - i);
                        g.DrawString(title, fT, hb, tx, ty + i);
                    }
                using (var mb = new SolidBrush(Pal.Cyan))
                    g.DrawString(title, fT, mb, tx, ty);

                // "2.0" en magenta
                using (var fV = new Font("Segoe UI", 30, FontStyle.Regular))
                using (var vb = new SolidBrush(Pal.Magenta))
                {
                    SizeF szV = g.MeasureString("2.5", fV);
                    g.DrawString("2.5", fV, vb, tx + szT.Width + 6, ty + 18);
                }

                // Subtítulo
                using (var fS = new Font("Segoe UI", 14))
                using (var sb = new SolidBrush(Color.FromArgb(50, 50, 72)))
                {
                    string sub = "Escribe un prompt y presiona GENERAR";
                    SizeF szS  = g.MeasureString(sub, fS);
                    g.DrawString(sub, fS, sb, cx - szS.Width / 2f, cy + szT.Height / 2f - 14);
                }
            }

            // Líneas de escaneo decorativas
            using (var scanPen = new Pen(Color.FromArgb(6, 255, 255, 255), 1))
            {
                for (int y = 0; y < r.Height; y += 4)
                    g.DrawLine(scanPen, 0, y, r.Width, y);
            }

            // Esquinas CRT-style
            void Corner(int x, int y, int sx, int sy)
            {
                using var pen = new Pen(Color.FromArgb(50, Pal.Cyan.R, Pal.Cyan.G, Pal.Cyan.B), 1.5f);
                g.DrawLine(pen, x, y, x + sx * 28, y);
                g.DrawLine(pen, x, y, x, y + sy * 28);
                using var dotBrush = new SolidBrush(Pal.Alpha(Pal.Cyan, 80));
                g.FillEllipse(dotBrush, x - 2, y - 2, 4, 4);
            }
            Corner(28, 28, 1, 1); Corner(r.Width - 28, 28, -1, 1);
            Corner(28, r.Height - 28, 1, -1); Corner(r.Width - 28, r.Height - 28, -1, -1);
        }
    }

    // ══════════════════════════════════════════════════════════════════════
    //  PROGRAM
    // ══════════════════════════════════════════════════════════════════════
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new VentanaPrincipal());
        }
    }
}