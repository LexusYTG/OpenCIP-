using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

namespace OpenCIP
{
    public class GeneradorAcuarela
    {
        private ContextoVisual _ctx;
        private Random _rnd;

        public GeneradorAcuarela(ContextoVisual ctx)
        {
            _ctx = ctx;
            _rnd = new Random(ctx.Semilla);
        }

        public Bitmap Generar(int ancho, int alto, Action<int> progreso)
        {
            Bitmap bmp = new Bitmap(ancho, alto, PixelFormat.Format24bppRgb);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode      = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.CompositingMode    = System.Drawing.Drawing2D.CompositingMode.SourceOver;
                g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;

                g.Clear(Color.FromArgb(253, 249, 243));
                if (progreso != null) progreso(3);

                string tema = DetectarTema();
                List<Color> pal = _ctx.Paleta.Count > 0 ? _ctx.Paleta : PaletaTema(tema);

                TexturaPapel(g, ancho, alto);
                if (progreso != null) progreso(8);

                switch (tema)
                {
                    case "oceano":   PintarOceano(g, ancho, alto, pal);    break;
                    case "montana":  PintarMontana(g, ancho, alto, pal);   break;
                    case "bosque":   PintarBosque(g, ancho, alto, pal);    break;
                    case "flores":   PintarFlores(g, ancho, alto, pal);    break;
                    case "ciudad":   PintarCiudad(g, ancho, alto, pal);    break;
                    case "desierto": PintarDesierto(g, ancho, alto, pal);  break;
                    case "noche":    PintarNoche(g, ancho, alto, pal);     break;
                    case "lago":     PintarLago(g, ancho, alto, pal);      break;
                    case "valle":    PintarValle(g, ancho, alto, pal);     break;
                    case "volcan":   PintarVolcan(g, ancho, alto, pal);    break;
                    case "playa":    PintarPlaya(g, ancho, alto, pal);     break;
                    default:         PintarAbstracto(g, ancho, alto, pal); break;
                }
                if (progreso != null) progreso(80);

                BordesHumedos(g, ancho, alto);
                GranoFinal(g, ancho, alto);
                if (progreso != null) progreso(96);
            }
            if (progreso != null) progreso(100);
            return bmp;
        }

        // ── Detección de tema ────────────────────────────────────────────────

        private string DetectarTema()
        {
            var pals = _ctx.PalabrasDetectadas;
            if (pals == null) return "abstracto";
            if (pals.Contains("bosque")||pals.Contains("forest")||pals.Contains("arbol")||pals.Contains("tree")) return "bosque";
            if (pals.Contains("montana")||pals.Contains("mountain")||pals.Contains("peak")) return "montana";
            if (pals.Contains("lago")||pals.Contains("lake")) return "lago";
            if (pals.Contains("playa")||pals.Contains("beach")) return "playa";
            if (pals.Contains("volcan")||pals.Contains("volcano")||pals.Contains("lava")) return "volcan";
            if (pals.Contains("valle")||pals.Contains("valley")) return "valle";
            if (pals.Contains("oceano")||pals.Contains("ocean")||pals.Contains("mar")||pals.Contains("sea")) return "oceano";
            if (pals.Contains("flores")||pals.Contains("flower")||pals.Contains("campo")) return "flores";
            if (pals.Contains("ciudad")||pals.Contains("city")) return "ciudad";
            if (pals.Contains("desierto")||pals.Contains("desert")) return "desierto";
            if (pals.Contains("noche")||pals.Contains("night")) return "noche";
            return "abstracto";
        }

        // ── Primitivas ───────────────────────────────────────────────────────

        private Color CA(Color c, int alfa)
            => Color.FromArgb(Clamp(alfa), c.R, c.G, c.B);

        private Color Varia(Color c, int rango)
            => Color.FromArgb(
                Clamp(c.R + _rnd.Next(-rango, rango + 1)),
                Clamp(c.G + _rnd.Next(-rango, rango + 1)),
                Clamp(c.B + _rnd.Next(-rango, rango + 1)));

        private int Clamp(int v) => v < 0 ? 0 : v > 255 ? 255 : v;
        private static float CF(float v) => v < 0 ? 0 : v > 1 ? 1 : v;

        private Color MixColor(Color a, Color b, float t)
        {
            t = CF(t);
            return Color.FromArgb(
                Clamp((int)(a.R * (1 - t) + b.R * t)),
                Clamp((int)(a.G * (1 - t) + b.G * t)),
                Clamp((int)(a.B * (1 - t) + b.B * t)));
        }

        /// <summary>
        /// Mancha de acuarela con bloom en borde.
        /// FIX: reducido el blanqueado de base (0.22→0.05) y el alpha del borde oscuro
        /// (0.90→0.55) para evitar la acumulación de blanco y los artefactos de borde.
        /// </summary>
        private void Mancha(Graphics g, float cx, float cy, float rx, float ry,
                            Color color, int alfa, float irregularidad = 0.25f)
        {
            if (alfa <= 0 || rx <= 0 || ry <= 0) return;
            int nPts = 20 + _rnd.Next(10);
            var pts = new PointF[nPts];
            for (int i = 0; i < nPts; i++)
            {
                double ang = i * Math.PI * 2 / nPts;
                // Una sola capa de ruido (era dos; dos capas multiplicadas
                // producían puntos extremos que rompían PathGradientBrush)
                double v1 = 1.0 + (_rnd.NextDouble() - 0.5) * irregularidad * 1.8;
                pts[i] = new PointF(
                    cx + (float)(Math.Cos(ang) * rx * v1),
                    cy + (float)(Math.Sin(ang) * ry * v1));
            }

            using (var path = new System.Drawing.Drawing2D.GraphicsPath())
            {
                path.AddClosedCurve(pts, 0.35f);

                // Capa 1: base con pigmento puro — sin blanqueado agresivo
                // Antes: MixColor(color, White, 0.22f) → acumulaba blanco
                using (var br = new SolidBrush(CA(color, (int)(alfa * 0.55))))
                    g.FillPath(br, path);

                // Capa 2: bloom en borde — borde más oscuro, centro más claro
                // FIX: alpha del borde bajado de 0.90 a 0.50 para evitar halos
                try
                {
                    using (var pgb = new System.Drawing.Drawing2D.PathGradientBrush(path))
                    {
                        pgb.CenterColor    = CA(MixColor(color, Color.White, 0.30f), (int)(alfa * 0.10));
                        pgb.SurroundColors = new[] { CA(MixColor(color, Color.Black, 0.15f), (int)(alfa * 0.50)) };
                        pgb.FocusScales    = new PointF(0.45f, 0.45f);
                        g.FillPath(pgb, path);
                    }
                }
                catch { /* path degenerado — base ya pintada, ok */ }
            }
        }

        /// <summary>
        /// Pincelada bezier con curvatura controlada.
        /// FIX: agregado parámetro 'curvatura' (0=plana, 1=muy curvada).
        /// El bug original usaba (x1-x0)*0.26 como brazo de control, lo que
        /// para una línea de ancho completo producía 200+px de desvío.
        /// Ahora el brazo es grosor*curvatura*2 — proporcional al grosor, no al largo.
        /// </summary>
        private void Pincelada(Graphics g, float x0, float y0, float x1, float y1,
                               float grosor, Color color, int alfa, float curvatura = 0.35f)
        {
            // Desplazamiento del punto medio: proporcional al grosor, no al largo
            float desvio = grosor * curvatura * 2.0f;
            float mx = (x0 + x1) / 2 + (float)(_rnd.NextDouble() - 0.5) * desvio;
            float my = (y0 + y1) / 2 + (float)(_rnd.NextDouble() - 0.5) * desvio;

            // Brazos del bezier: también proporcionales al grosor
            float brazo = grosor * curvatura * 1.2f;
            float dx = x1 - x0, dy = y1 - y0;
            float len = (float)Math.Sqrt(dx * dx + dy * dy);
            if (len < 0.001f) return;
            // perpendicular normalizado
            float px = -dy / len * brazo;
            float py =  dx / len * brazo;

            using (var path = new System.Drawing.Drawing2D.GraphicsPath())
            {
                path.AddBezier(
                    x0, y0,
                    mx + px, my + py,
                    mx - px, my - py,
                    x1, y1);

                float g2 = grosor * (0.85f + (float)(_rnd.NextDouble() * 0.30));
                using (var pen = new Pen(CA(color, alfa), g2))
                {
                    pen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
                    pen.EndCap   = System.Drawing.Drawing2D.LineCap.Round;
                    g.DrawPath(pen, path);
                }
            }
        }

        // ── Textura de papel ─────────────────────────────────────────────────

        private void TexturaPapel(Graphics g, int w, int h)
        {
            var rndP = new Random(_ctx.Semilla ^ 0x4444);
            for (int i = 0; i < 14; i++)
            {
                float cx = (float)(rndP.NextDouble() * w);
                float cy = (float)(rndP.NextDouble() * h);
                float rx = (float)(w * 0.09 + rndP.NextDouble() * w * 0.24);
                float ry = (float)(h * 0.07 + rndP.NextDouble() * h * 0.18);
                Mancha(g, cx, cy, rx, ry, Color.FromArgb(192, 175, 152), 4 + rndP.Next(7), 0.20f);
            }
        }

        private void GranoFinal(Graphics g, int w, int h)
        {
            var rndG = new Random(_ctx.Semilla ^ 0xBEEF);
            int n = (int)(w * h * 0.0024);
            for (int i = 0; i < n; i++)
            {
                int x = rndG.Next(w - 1), y = rndG.Next(h - 1);
                int a = rndG.Next(4, 20);
                using (var br = new SolidBrush(Color.FromArgb(a,
                    rndG.Next(82, 118), rndG.Next(72, 108), 78)))
                    g.FillRectangle(br, x, y, 1, 1);
            }
        }

        private void BordesHumedos(Graphics g, int w, int h)
        {
            using (var path = new System.Drawing.Drawing2D.GraphicsPath())
            {
                path.AddRectangle(new RectangleF(0, 0, w, h));
                path.AddEllipse(new RectangleF(w * 0.04f, h * 0.04f, w * 0.92f, h * 0.92f));
                using (var br = new System.Drawing.Drawing2D.PathGradientBrush(path))
                {
                    br.CenterColor    = Color.Transparent;
                    br.SurroundColors = new[] { Color.FromArgb(24, 115, 100, 84) };
                    g.FillRectangle(br, 0, 0, w, h);
                }
            }
        }

        // ── Paletas ──────────────────────────────────────────────────────────

        private List<Color> PaletaTema(string tema)
        {
            switch (tema)
            {
                case "oceano":   return new List<Color> { Color.FromArgb(12,82,158), Color.FromArgb(42,138,210), Color.FromArgb(158,205,238), Color.FromArgb(245,192,78), Color.FromArgb(255,148,55) };
                case "montana":  return new List<Color> { Color.FromArgb(62,82,122), Color.FromArgb(128,148,185), Color.FromArgb(205,215,232), Color.FromArgb(48,108,52), Color.FromArgb(235,240,248) };
                case "bosque":   return new List<Color> { Color.FromArgb(12,62,22), Color.FromArgb(32,108,38), Color.FromArgb(62,148,48), Color.FromArgb(108,82,28), Color.FromArgb(175,198,95) };
                case "flores":   return new List<Color> { Color.FromArgb(215,42,78), Color.FromArgb(255,172,22), Color.FromArgb(128,42,192), Color.FromArgb(38,152,68), Color.FromArgb(255,88,138) };
                case "desierto": return new List<Color> { Color.FromArgb(205,158,62), Color.FromArgb(225,128,42), Color.FromArgb(172,108,36), Color.FromArgb(245,212,132), Color.FromArgb(178,78,28) };
                case "noche":    return new List<Color> { Color.FromArgb(6,10,52), Color.FromArgb(22,32,108), Color.FromArgb(68,52,132), Color.FromArgb(192,172,232), Color.FromArgb(255,245,198) };
                case "lago":     return new List<Color> { Color.FromArgb(22,82,152), Color.FromArgb(68,142,205), Color.FromArgb(32,102,46), Color.FromArgb(192,218,238), Color.FromArgb(255,235,158) };
                case "valle":    return new List<Color> { Color.FromArgb(42,122,42), Color.FromArgb(72,152,62), Color.FromArgb(128,172,92), Color.FromArgb(88,142,205), Color.FromArgb(255,228,138) };
                case "volcan":   return new List<Color> { Color.FromArgb(192,42,6), Color.FromArgb(255,108,0), Color.FromArgb(32,28,22), Color.FromArgb(255,198,18), Color.FromArgb(158,22,4) };
                case "playa":    return new List<Color> { Color.FromArgb(0,142,172), Color.FromArgb(192,178,122), Color.FromArgb(68,172,192), Color.FromArgb(232,202,92), Color.FromArgb(0,98,138) };
                default:         return new List<Color> { Color.FromArgb(58,98,172), Color.FromArgb(192,68,52), Color.FromArgb(42,132,72), Color.FromArgb(212,162,52), Color.FromArgb(152,52,128) };
            }
        }

        // ── Helpers de escena ─────────────────────────────────────────────────

        private void PintarCieloAcuarela(Graphics g, int w, int hz,
                                          Color c1, Color c2, Color? c3 = null)
        {
            using (var br = new System.Drawing.Drawing2D.LinearGradientBrush(
                new PointF(0, 0), new PointF(0, hz), CA(c1, 215), CA(c2, 192)))
                g.FillRectangle(br, 0, 0, w, hz);

            if (c3.HasValue)
            {
                using (var br = new System.Drawing.Drawing2D.LinearGradientBrush(
                    new PointF(0, 0), new PointF(w, 0),
                    CA(c3.Value, 58), CA(Color.Transparent, 8)))
                    g.FillRectangle(br, 0, 0, w, hz);
            }

            var rndN = new Random(_ctx.Semilla ^ 0x1234);
            int nNubes = 5 + rndN.Next(7);
            for (int i = 0; i < nNubes; i++)
            {
                float cx = (float)(rndN.NextDouble() * w * 1.15 - w * 0.07);
                float cy = (float)(rndN.NextDouble() * hz * 0.68 + hz * 0.04);
                float rx = (float)(w * 0.06 + rndN.NextDouble() * w * 0.20);
                float ry = (float)(hz * 0.045 + rndN.NextDouble() * hz * 0.095);

                Mancha(g, cx + rx * 0.12f, cy + ry * 0.32f, rx * 0.82f, ry * 0.52f,
                    MixColor(c2, Color.FromArgb(148, 162, 195), 0.45f), 14 + rndN.Next(14), 0.45f);
                Mancha(g, cx, cy, rx, ry, Color.FromArgb(248, 251, 255), 22 + rndN.Next(24), 0.48f);
                Mancha(g, cx - rx * 0.14f, cy - ry * 0.22f, rx * 0.52f, ry * 0.38f,
                    Color.White, 16 + rndN.Next(16), 0.32f);
            }
        }

        private void PintarSolAcuarela(Graphics g, int w, int hz, Color cSol)
        {
            var rndS = new Random(_ctx.Semilla ^ 0x5678);
            float sx = (float)(w * (0.52 + rndS.NextDouble() * 0.32));
            float sy = (float)(hz * (0.09 + rndS.NextDouble() * 0.27));
            float sr = (float)(Math.Min(w, hz) * 0.052f);

            Mancha(g, sx, sy, sr * 5.5f, sr * 5.5f, cSol, 10, 0.32f);
            Mancha(g, sx, sy, sr * 3.2f, sr * 3.2f, cSol, 20, 0.26f);
            Mancha(g, sx, sy, sr * 1.7f, sr * 1.7f, MixColor(cSol, Color.White, 0.28f), 58, 0.16f);
            Mancha(g, sx, sy, sr, sr, Color.FromArgb(255, 252, 232), 205, 0.05f);
            for (int ri = 0; ri < 12; ri++)
            {
                double ang = ri * Math.PI * 2 / 12 + rndS.NextDouble() * 0.22;
                float rLen = sr * (2.2f + (float)(rndS.NextDouble() * 2.8f));
                using (var pen = new Pen(CA(cSol, 22 + rndS.Next(22)), 1.2f))
                    g.DrawLine(pen, sx, sy,
                        sx + (float)(Math.Cos(ang) * rLen),
                        sy + (float)(Math.Sin(ang) * rLen));
            }
        }

        private void PintarPinosAcuarela(Graphics g, int w, int h, int hz, Color cPino)
        {
            var rndP = new Random(_ctx.Semilla ^ 0x9999);
            int nPinos = w / 72 + rndP.Next(6);
            for (int i = 0; i < nPinos; i++)
            {
                float px  = (float)(rndP.NextDouble() * w);
                float py  = (float)(hz + rndP.NextDouble() * (h - hz) * 0.28);
                float alt = (float)(h * 0.09 + rndP.NextDouble() * h * 0.17);
                float an  = alt * 0.30f;
                Color cP  = Varia(cPino, 20);
                for (int ni = 0; ni < 4; ni++)
                {
                    float ty = py - alt * (0.16f + ni * 0.22f);
                    float tw = an * (1.12f - ni * 0.22f);
                    float th = alt * 0.26f;
                    Mancha(g, px, ty, tw * 0.82f, th * 0.5f, cP, 138 + ni * 14, 0.28f);
                }
                using (var pen = new Pen(CA(MixColor(cPino, Color.FromArgb(78, 48, 18), 0.52f), 162), 3.5f))
                {
                    pen.StartCap = pen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
                    g.DrawLine(pen, px, py, px, py - alt * 0.20f);
                }
            }
        }

        // ── Elementos reutilizables ───────────────────────────────────────────

        private void Pajaro(Graphics g, float x, float y, float escala, int alfa)
        {
            using (var pen = new Pen(CA(Color.FromArgb(28, 28, 32), alfa), escala * 1.1f))
            {
                pen.StartCap = pen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
                g.DrawBezier(pen, x - escala*6, y, x - escala*3, y - escala*2.5f, x - escala, y - escala, x, y);
                g.DrawBezier(pen, x, y, x + escala, y - escala, x + escala*3, y - escala*2.5f, x + escala*6, y);
            }
        }

        private void BandadaPajaros(Graphics g, int w, int hz, int n, Random rndB)
        {
            for (int i = 0; i < n; i++)
            {
                float bx  = (float)(rndB.NextDouble() * w * 0.85 + w * 0.07);
                float by  = (float)(rndB.NextDouble() * hz * 0.55 + hz * 0.08);
                float esc = 0.7f + (float)(rndB.NextDouble() * 0.9);
                Pajaro(g, bx, by, esc, 55 + rndB.Next(65));
            }
        }

        private void Bote(Graphics g, float cx, float cy, float esc, Color cBote, bool conVela)
        {
            var casco = new PointF[] {
                new PointF(cx - esc*8, cy), new PointF(cx - esc*6, cy + esc*3),
                new PointF(cx + esc*6, cy + esc*3), new PointF(cx + esc*8, cy)
            };
            using (var path = new System.Drawing.Drawing2D.GraphicsPath())
            {
                path.AddLines(casco); path.CloseFigure();
                using (var br = new SolidBrush(CA(cBote, 185))) g.FillPath(br, path);
                using (var pen = new Pen(CA(MixColor(cBote, Color.Black, 0.35f), 155), esc * 0.8f)) g.DrawPath(pen, path);
            }
            using (var pen = new Pen(CA(MixColor(cBote, Color.White, 0.28f), 115), esc * 0.5f))
                g.DrawLine(pen, cx - esc*8, cy, cx + esc*8, cy);
            if (conVela)
            {
                var vela = new PointF[] {
                    new PointF(cx - esc, cy), new PointF(cx - esc*0.5f, cy - esc*9),
                    new PointF(cx + esc*5.5f, cy - esc*1.5f)
                };
                using (var path = new System.Drawing.Drawing2D.GraphicsPath())
                {
                    path.AddLines(vela); path.CloseFigure();
                    using (var br = new SolidBrush(CA(Color.FromArgb(245, 238, 222), 185))) g.FillPath(br, path);
                    using (var pen = new Pen(CA(Color.FromArgb(88, 72, 52), 125), esc * 0.5f)) g.DrawPath(pen, path);
                }
                using (var pen = new Pen(CA(Color.FromArgb(88, 62, 32), 168), esc * 0.7f))
                    g.DrawLine(pen, cx - esc*0.5f, cy, cx - esc*0.5f, cy - esc*9.5f);
            }
        }

        private void Muelle(Graphics g, float x, float y, float largo, float ancho, Color col)
        {
            using (var br = new SolidBrush(CA(col, 172))) g.FillRectangle(br, x, y, largo, ancho);
            int nPil = (int)(largo / 22);
            for (int i = 0; i <= nPil; i++)
            {
                float px = x + i * largo / nPil;
                using (var pen = new Pen(CA(MixColor(col, Color.Black, 0.38f), 155), 3.5f))
                    g.DrawLine(pen, px, y + ancho, px, y + ancho + 18);
            }
        }

        private void ArbolCaducifolio(Graphics g, float x, float baseY, float alto, Color cTronco, Color cCopa)
        {
            float troncoH = alto * 0.38f;
            float troncoW = alto * 0.06f;
            using (var pen = new Pen(CA(cTronco, 192), troncoW))
            {
                pen.StartCap = pen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
                g.DrawLine(pen, x, baseY, x + (float)(_rnd.NextDouble() - 0.5) * alto * 0.12f, baseY - troncoH);
            }
            float ramaY = baseY - troncoH;
            for (int r = 0; r < 3; r++)
            {
                double ang = -Math.PI / 2 + (r - 1) * Math.PI * 0.28;
                float  rx  = x + (float)(Math.Cos(ang) * alto * 0.22);
                float  ry  = ramaY + (float)(Math.Sin(ang) * alto * 0.08) - alto * 0.1f;
                using (var pen = new Pen(CA(cTronco, 152), troncoW * 0.55f))
                {
                    pen.StartCap = pen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
                    g.DrawLine(pen, x, ramaY, rx, ry);
                }
            }
            float copaR  = alto * 0.34f;
            int nManchas = 4 + _rnd.Next(3);
            for (int m = 0; m < nManchas; m++)
            {
                double ang = m * Math.PI * 2 / nManchas + _rnd.NextDouble() * 0.5;
                float  ox  = (float)(Math.Cos(ang) * copaR * 0.32);
                float  oy  = (float)(Math.Sin(ang) * copaR * 0.22);
                float  cr  = copaR * (0.62f + (float)(_rnd.NextDouble() * 0.28));
                Mancha(g, x + ox, ramaY - copaR * 0.45f + oy, cr, cr * 0.78f, Varia(cCopa, 18), 145 + _rnd.Next(38), 0.32f);
            }
        }

        private void MuroPiedra(Graphics g, float x, float y, float largo, float alto, Color cPiedra)
        {
            using (var br = new SolidBrush(CA(cPiedra, 162))) g.FillRectangle(br, x, y, largo, alto);
            var rndM = new Random(_ctx.Semilla ^ 0xA1B2);
            int nFilas = (int)(alto / 9);
            for (int fi = 1; fi < nFilas; fi++)
            {
                float fy = y + fi * alto / nFilas;
                using (var pen = new Pen(CA(MixColor(cPiedra, Color.Black, 0.28f), 68), 1f))
                    g.DrawLine(pen, x, fy, x + largo, fy);
            }
            for (int fi = 0; fi < nFilas; fi++)
            {
                float fy  = y + fi * alto / nFilas;
                float fyN = y + (fi + 1) * alto / nFilas;
                int   nV  = (int)(largo / 22);
                float off = fi % 2 == 0 ? 11 : 0;
                for (int vi = 0; vi <= nV; vi++)
                {
                    float vx = x + off + vi * largo / nV;
                    using (var pen = new Pen(CA(MixColor(cPiedra, Color.Black, 0.22f), 48), 1f))
                        g.DrawLine(pen, vx, fy, vx, fyN);
                }
            }
            using (var pen = new Pen(CA(MixColor(cPiedra, Color.Black, 0.42f), 85), 1.5f))
                g.DrawLine(pen, x, y, x + largo, y);
        }

        private void BandaNiebla(Graphics g, float x, float y, float ancho, float alto, int alfa)
        {
            using (var br = new System.Drawing.Drawing2D.LinearGradientBrush(
                new PointF(x, y), new PointF(x, y + alto),
                CA(Color.FromArgb(238, 242, 248), alfa), Color.Transparent))
                g.FillRectangle(br, x, y, ancho, alto);
        }

        // ── OCÉANO ────────────────────────────────────────────────────────────

        private void PintarOceano(Graphics g, int w, int h, List<Color> pal)
        {
            int   hz    = (int)(h * 0.40);
            Color cHor  = pal.Count > 4 ? pal[4] : Color.FromArgb(255, 138, 48);
            Color cAgua1 = pal[0];
            Color cAgua2 = pal.Count > 1 ? pal[1] : Color.FromArgb(42, 138, 215);

            PintarCieloAcuarela(g, w, hz, cHor, Color.FromArgb(158, 212, 248), Color.FromArgb(255, 218, 98));

            // Banda horizonte cálida
            using (var br = new System.Drawing.Drawing2D.LinearGradientBrush(
                new PointF(0, hz - h * 0.04f), new PointF(0, hz + h * 0.08f),
                CA(MixColor(cHor, Color.White, 0.55f), 75), Color.Transparent))
                g.FillRectangle(br, 0, hz - h * 0.04f, w, h * 0.12f);

            // Fondo agua
            using (var br = new System.Drawing.Drawing2D.LinearGradientBrush(
                new PointF(0, hz), new PointF(0, h),
                CA(MixColor(cAgua1, cHor, 0.35f), 225), CA(cAgua1, 245)))
                g.FillRectangle(br, 0, hz, w, h - hz);

            // Olas planas
            int nCapas = 28 + _rnd.Next(10);
            for (int i = 0; i < nCapas; i++)
            {
                float pct    = (float)i / nCapas;
                float y      = hz + pct * (h - hz) + (float)(_rnd.NextDouble() - 0.5) * 5f;
                Color cOla   = MixColor(MixColor(cAgua1, cHor, (1 - pct) * 0.25f), cAgua2, pct);
                float grosor = 2.5f + pct * 22 + (float)(_rnd.NextDouble() * 7);
                int   alfa   = (int)(30 + pct * 58 + _rnd.NextDouble() * 28);
                Pincelada(g, (float)(_rnd.NextDouble() * w * 0.08), y,
                    w - (float)(_rnd.NextDouble() * w * 0.08), y,
                    grosor, Varia(cOla, 16), alfa, 0.05f);
            }

            // Espuma plana
            for (int i = 0; i < 18; i++)
            {
                float pct = (float)_rnd.NextDouble();
                float y   = hz + pct * (h - hz) * 0.55f;
                float x0  = (float)(_rnd.NextDouble() * w * 0.35);
                float x1  = x0 + (float)(w * (0.12 + _rnd.NextDouble() * 0.42));
                Pincelada(g, x0, y, x1, y, 2 + (float)(_rnd.NextDouble() * 5),
                    Color.FromArgb(232, 244, 254), 48 + _rnd.Next(52), 0.07f);
                Pincelada(g, x0 + 4, y + 5, x1 + 4, y + 5, 1.5f,
                    MixColor(cAgua2, Color.Black, 0.20f), 18 + _rnd.Next(16), 0.05f);
            }

            // Reflejo del sol
            float solX = (float)(w * 0.68);
            for (int i = 0; i < 22; i++)
            {
                float ry  = hz + i * (float)(h - hz) / 22f;
                float rw  = 8 + i * 7 + (float)(_rnd.NextDouble() * 24);
                float rxO = (float)(_rnd.NextDouble() * 12 - 6);
                Mancha(g, solX + rxO, ry, rw * 0.5f, 3f,
                    MixColor(cHor, Color.White, 0.60f), Math.Max(4, 68 - i * 3), 0.22f);
            }

            PintarSolAcuarela(g, w, hz, cHor);

            // Costa lejana (siempre presente)
            var rndOc = new Random(_ctx.Semilla ^ 0xABCD);
            var ptsC  = new List<PointF> { new PointF(-5, hz + 10) };
            int nC    = w / 9;
            for (int xi = 0; xi <= nC; xi++)
            {
                double nx = (double)xi / nC;
                double n  = Matematica.FBM(nx * 3.2 + _ctx.Semilla * 0.002, 0.28, 3, 0.52);
                float  py = hz - (float)((n + 1) * 0.5 * hz * 0.10);
                ptsC.Add(new PointF((float)(nx * w), py));
            }
            ptsC.Add(new PointF(w + 5, hz + 10));
            using (var path = new System.Drawing.Drawing2D.GraphicsPath())
            {
                path.AddCurve(ptsC.ToArray(), 0.28f);
                path.AddLine(w + 5, hz + 10, -5, hz + 10);
                path.CloseFigure();
                using (var br = new SolidBrush(CA(Color.FromArgb(52, 46, 40), 62)))
                    g.FillPath(br, path);
            }

            // Faro en la costa
            float faroX   = (float)(w * 0.22);
            float faroY   = hz - (float)(hz * 0.08);
            float faroAlt = (float)(hz * 0.12);
            using (var br = new SolidBrush(CA(Color.FromArgb(235, 235, 235), 172)))
                g.FillRectangle(br, faroX - 4, faroY - faroAlt, 8, faroAlt);
            using (var br = new SolidBrush(CA(Color.FromArgb(185, 45, 35), 162)))
                g.FillRectangle(br, faroX - 5, faroY - faroAlt * 1.12f, 10, faroAlt * 0.15f);
            Mancha(g, faroX, faroY - faroAlt * 1.18f, 8, 5, Color.FromArgb(255, 245, 168), 172, 0.18f);

            // Velero (punto focal)
            float boteX = (float)(w * 0.62);
            float boteY = hz + (h - hz) * 0.18f;
            Bote(g, boteX, boteY, 1.1f + (float)(_rnd.NextDouble() * 0.4), Color.FromArgb(82, 60, 38), true);
            // Reflejo del velero en el agua
            using (var br = new SolidBrush(CA(Color.FromArgb(52, 42, 28), 32)))
                g.FillRectangle(br, boteX - 14, boteY + 2, 28, 8);

            // Gaviotas
            BandadaPajaros(g, w, hz, 6 + _rnd.Next(5), new Random(_ctx.Semilla ^ 0xB1D5));
        }

        // ── MONTAÑA ───────────────────────────────────────────────────────────

        private void PintarMontana(Graphics g, int w, int h, List<Color> pal)
        {
            int   hz    = (int)(h * 0.43);
            Color cNieve = pal.Count > 4 ? pal[4] : Color.FromArgb(235, 242, 252);
            Color cPino  = pal.Count > 3 ? pal[3] : Color.FromArgb(18, 62, 26);

            PintarCieloAcuarela(g, w, hz,
                Color.FromArgb(195, 218, 248), Color.FromArgb(162, 192, 228),
                Color.FromArgb(228, 198, 162));

            Color[] colMont = {
                Color.FromArgb(202, 212, 235), Color.FromArgb(155, 168, 202),
                Color.FromArgb(98,  118, 158), Color.FromArgb(68,  88,  128), pal[0]
            };
            var rndM = new Random(_ctx.Semilla);
            for (int pl = 0; pl < 5; pl++)
            {
                double tS     = _ctx.Semilla * 0.006 + pl * 8.5;
                double altMax = 0.11 + pl * 0.072;
                int    yBase  = hz + (int)(h * 0.012 * pl);
                int    nPts   = w / 3;
                var    pts    = new List<PointF> { new PointF(-5, h) };
                for (int xi = 0; xi <= nPts; xi++)
                {
                    double nx  = (double)xi / nPts;
                    double n1  = Matematica.FBM(nx * 2.8 + tS, 0.5, 6, 0.60);
                    double n2  = Matematica.FBM(nx * 8.8 + tS + 4.1, 0.3, 3, 0.52) * 0.16;
                    double alt = Math.Pow(((n1 + n2) + 1) * 0.5, 0.68);
                    float  py  = yBase - (float)(h * altMax * alt);
                    pts.Add(new PointF((float)(nx * w), Math.Max(5, py + (float)(rndM.NextDouble() * 2.2 - 1.1))));
                }
                pts.Add(new PointF(w + 5, h));
                using (var path = new System.Drawing.Drawing2D.GraphicsPath())
                {
                    path.AddLines(pts.ToArray()); path.CloseFigure();
                    using (var br = new SolidBrush(CA(Varia(colMont[pl], 12), 88 + pl * 30)))
                        g.FillPath(br, path);
                    if (pl >= 3)
                    {
                        float nivelNieve = yBase - (float)(h * altMax * 0.44f);
                        var ptN = new List<PointF> { new PointF(-5, nivelNieve) };
                        for (int xi = 0; xi <= nPts; xi++)
                        {
                            double nx  = (double)xi / nPts;
                            double n1  = Matematica.FBM(nx * 2.8 + tS, 0.5, 6, 0.60);
                            double n2  = Matematica.FBM(nx * 8.8 + tS + 4.1, 0.3, 3, 0.52) * 0.16;
                            double alt = Math.Pow(((n1 + n2) + 1) * 0.5, 0.68);
                            float  py  = yBase - (float)(h * altMax * alt);
                            if (py < nivelNieve) ptN.Add(new PointF((float)(nx * w), Math.Max(5, py)));
                        }
                        ptN.Add(new PointF(w + 5, nivelNieve));
                        if (ptN.Count > 4)
                        {
                            using (var pathN = new System.Drawing.Drawing2D.GraphicsPath())
                            {
                                pathN.AddLines(ptN.ToArray()); pathN.CloseFigure();
                                using (var brN = new SolidBrush(CA(cNieve, 162 + pl * 18)))
                                    g.FillPath(brN, pathN);
                                // Sombra azulada cara norte
                                using (var brS2 = new SolidBrush(CA(Color.FromArgb(148, 172, 215), 25)))
                                    g.FillPath(brS2, pathN);
                            }
                        }
                    }
                    using (var brS = new SolidBrush(CA(Color.FromArgb(28, 38, 68), 16 + pl * 7)))
                        g.FillPath(brS, path);
                }
            }

            // Cascada (tercio izquierdo)
            float cascX  = (float)(w * 0.28);
            float cascY1 = hz + (h - hz) * 0.05f;
            float cascY2 = hz + (h - hz) * 0.42f;
            for (int ci = 0; ci < 4; ci++)
            {
                float ox = (float)(rndM.NextDouble() * 8 - 4);
                using (var pen = new Pen(CA(Color.FromArgb(200, 220, 240), 28 + ci * 10), 3.5f - ci * 0.5f))
                {
                    pen.StartCap = pen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
                    g.DrawLine(pen, cascX + ox, cascY1, cascX + ox * 1.8f, cascY2);
                }
            }
            Mancha(g, cascX, cascY2 + 6, 18, 8, Color.FromArgb(215, 232, 248), 92, 0.42f);
            Mancha(g, cascX, cascY2 + 4, 9,  5, Color.White, 130, 0.28f);
            BandaNiebla(g, cascX - 28, cascY2 - 5, 56, 22, 32);

            PintarPinosAcuarela(g, w, h, hz, cPino);

            // Prado alpino con flores
            int yPrado = hz + (int)((h - hz) * 0.62f);
            using (var br = new System.Drawing.Drawing2D.LinearGradientBrush(
                new PointF(0, yPrado), new PointF(0, h),
                CA(Color.FromArgb(68, 128, 52), 195), CA(Color.FromArgb(42, 95, 32), 225)))
                g.FillRectangle(br, 0, yPrado, w, h - yPrado);

            var rndF = new Random(_ctx.Semilla ^ 0xF1F1);
            Color[] cFlores = { Color.FromArgb(255, 215, 32), Color.FromArgb(255, 255, 255),
                                  Color.FromArgb(228, 68, 115), Color.FromArgb(155, 215, 255) };
            for (int i = 0; i < 265; i++)
            {
                float fx  = (float)(rndF.NextDouble() * w);
                float fy  = yPrado + (float)(rndF.NextDouble() * (h - yPrado) * 0.82);
                float pct = (fy - yPrado) / (h - yPrado);
                Mancha(g, fx, fy, 2 + pct * 8, 2 + pct * 7,
                    Varia(cFlores[rndF.Next(cFlores.Length)], 22), (int)(52 + pct * 88), 0.34f);
            }

            // Cabaña alpina
            float cabX = (float)(w * 0.65);
            float cabY = yPrado + (h - yPrado) * 0.18f;
            float cabW = 38, cabH = 24;
            using (var br = new SolidBrush(CA(Color.FromArgb(198, 175, 142), 185)))
                g.FillRectangle(br, cabX, cabY, cabW, cabH);
            var techoPts = new PointF[] {
                new PointF(cabX - 4, cabY),
                new PointF(cabX + cabW / 2, cabY - cabH * 0.65f),
                new PointF(cabX + cabW + 4, cabY)
            };
            using (var path = new System.Drawing.Drawing2D.GraphicsPath())
            {
                path.AddLines(techoPts); path.CloseFigure();
                using (var br = new SolidBrush(CA(Color.FromArgb(88, 52, 32), 192)))
                    g.FillPath(br, path);
            }
            using (var br = new SolidBrush(CA(Color.FromArgb(255, 242, 185), 158)))
                g.FillRectangle(br, cabX + 8, cabY + 8, 9, 9);
            // Humo
            for (int s = 0; s < 4; s++)
            {
                float sy = cabY - cabH * 0.65f - s * 8;
                Mancha(g, cabX + cabW * 0.55f + s * 2, sy, 5 + s * 2, 4 + s,
                    Color.FromArgb(168, 162, 158), 12 + s * 6, 0.55f);
            }

            PintarSolAcuarela(g, w, hz, Color.FromArgb(255, 222, 152));
            BandadaPajaros(g, w, hz, 4 + _rnd.Next(4), new Random(_ctx.Semilla ^ 0xA2B3));
        }

        // ── BOSQUE ────────────────────────────────────────────────────────────

        private void PintarBosque(Graphics g, int w, int h, List<Color> pal)
        {
            int hz = (int)(h * 0.42);
            PintarCieloAcuarela(g, w, hz, Color.FromArgb(152, 205, 238), Color.FromArgb(205, 232, 212));

            using (var br = new System.Drawing.Drawing2D.LinearGradientBrush(
                new PointF(0, hz), new PointF(0, h),
                CA(MixColor(pal[0], Color.FromArgb(98, 158, 38), 0.28f), 218),
                CA(MixColor(pal[0], Color.Black, 0.42f), 240)))
                g.FillRectangle(br, 0, hz, w, h - hz);

            // Hojarasca
            var rndH = new Random(_ctx.Semilla ^ 0xD171);
            for (int i = 0; i < 55; i++)
            {
                float lx  = (float)(rndH.NextDouble() * w);
                float ly  = hz + (float)(rndH.NextDouble() * (h - hz) * 0.9);
                float pct = (ly - hz) / (float)(h - hz);
                Color cHj = rndH.NextDouble() > 0.5 ? Color.FromArgb(115, 82, 22) : Color.FromArgb(82, 118, 28);
                Mancha(g, lx, ly, 5 + pct * 14, 3 + pct * 8, Varia(cHj, 25), (int)(32 + pct * 52), 0.48f);
            }

            var rndB = new Random(_ctx.Semilla ^ 0xF001);
            for (int i = 0; i < 22; i++)
            {
                float lx = (float)(rndB.NextDouble() * w);
                float ly = hz + (float)(rndB.NextDouble() * (h - hz));
                Mancha(g, lx, ly, 18 + (float)(rndB.NextDouble() * 48), 8 + (float)(rndB.NextDouble() * 18),
                    Color.FromArgb(205, 210, 88), 25 + rndB.Next(35), 0.55f);
            }

            double[] escPlano  = { 12.0, 8.2, 5.2, 3.4 };
            int[]    yOffPlano = { 0, (int)(h * 0.052), (int)(h * 0.105), (int)(h * 0.165) };
            Color[]  colPlano  = {
                MixColor(pal[0], Color.Black, 0.52f), pal[0],
                pal.Count > 1 ? pal[1] : Color.FromArgb(32, 112, 40),
                pal.Count > 2 ? pal[2] : Color.FromArgb(62, 152, 50)
            };
            var rndT = new Random(_ctx.Semilla);
            for (int pl = 0; pl < 4; pl++)
            {
                double tS     = _ctx.Semilla * 0.003 + pl * 9.3;
                double esc    = escPlano[pl];
                int    yB     = hz + yOffPlano[pl];
                double altMax = 0.28 + pl * 0.062;
                int    nPts   = w / 2;
                var    pts    = new List<PointF> { new PointF(-10, h) };
                for (int xi = 0; xi <= nPts; xi++)
                {
                    double nx  = (double)xi / nPts;
                    double nA  = Matematica.FBM(nx * esc + tS, 1.0 + pl, 5, 0.62);
                    double nD  = Matematica.FBM(nx * esc * 4.2 + tS + 7, 0.5, 3, 0.55) * 0.21;
                    double alt = ((nA + nD) + 1) * 0.5;
                    pts.Add(new PointF((float)(nx * w),
                        Math.Max(5, yB - (float)(h * altMax * alt) + (float)(rndT.NextDouble() * 4 - 2))));
                }
                pts.Add(new PointF(w + 10, h));
                using (var path = new System.Drawing.Drawing2D.GraphicsPath())
                {
                    path.AddCurve(pts.ToArray(), 0.3f);
                    path.AddLine(w + 10, h, -10, h); path.CloseFigure();
                    using (var br = new SolidBrush(CA(Varia(colPlano[pl], 14), 142 + pl * 22)))
                        g.FillPath(br, path);
                    if (pl >= 2)
                    {
                        using (var br2 = new SolidBrush(CA(MixColor(colPlano[pl], Color.FromArgb(178, 222, 78), 0.18f), 38)))
                            g.FillPath(br2, path);
                    }
                }
            }

            for (int r = 0; r < 8; r++)
            {
                float lx  = (float)(rndT.NextDouble() * w);
                float lw2 = 12 + (float)(rndT.NextDouble() * 38);
                var ptR = new PointF[] {
                    new PointF(lx - lw2*0.5f, 0), new PointF(lx + lw2*0.5f, 0),
                    new PointF(lx + lw2*2.8f, h), new PointF(lx - lw2*2.8f, h)
                };
                using (var br = new System.Drawing.Drawing2D.LinearGradientBrush(
                    new PointF(lx, 0), new PointF(lx, h * 0.70f),
                    CA(Color.FromArgb(255, 248, 195), 22 + rndT.Next(22)), Color.Transparent))
                    g.FillPolygon(br, ptR);
            }

            // Sendero
            int nS = 24;
            var pS1 = new PointF[nS]; var pS2 = new PointF[nS];
            for (int i = 0; i < nS; i++)
            {
                double t = (double)i / (nS - 1);
                float  sy = hz + (float)(t * (h - hz));
                float  hw = (float)(w * 0.025 + t * w * 0.10);
                float  cx = w * 0.5f + (float)(Math.Sin(t * Math.PI * 1.8 + _ctx.Semilla * 0.005) * w * 0.08);
                pS1[i] = new PointF(cx - hw, sy);
                pS2[i] = new PointF(cx + hw, sy);
            }
            using (var path = new System.Drawing.Drawing2D.GraphicsPath())
            {
                path.AddCurve(pS1, 0.4f);
                for (int i = nS - 1; i >= 0; i--) path.AddLine(pS2[i], i > 0 ? pS2[i-1] : pS1[0]);
                path.CloseFigure();
                using (var br = new SolidBrush(CA(Color.FromArgb(148, 118, 72), 85))) g.FillPath(br, path);
                using (var br2 = new SolidBrush(CA(Color.FromArgb(178, 152, 98), 28))) g.FillPath(br2, path);
            }

            // Troncos primer plano
            Color cTronco = Color.FromArgb(62, 42, 22);
            for (int side = 0; side < 2; side++)
            {
                float[] txA = side == 0 ? new float[] { w * 0.04f, w * 0.13f, w * 0.08f }
                                        : new float[] { w * 0.92f, w * 0.83f, w * 0.88f };
                float[] twA = { 14, 10, 7 };
                for (int ti = 0; ti < 3; ti++)
                {
                    float tx = txA[ti] + (float)(_rnd.NextDouble() * 6 - 3);
                    using (var pen = new Pen(CA(Varia(cTronco, 18), 185), twA[ti]))
                    {
                        pen.StartCap = pen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
                        g.DrawLine(pen, tx, h, tx + (float)(_rnd.NextDouble() * 8 - 4), hz + (h - hz) * 0.1f);
                    }
                    for (int l = 0; l < 4; l++)
                    {
                        float ly = hz + (h - hz) * (0.2f + l * 0.18f);
                        Mancha(g, tx, ly, twA[ti] * 0.8f, twA[ti] * 0.4f, Color.FromArgb(88, 122, 42), 40, 0.45f);
                    }
                }
            }

            // Hongos
            var rndHong = new Random(_ctx.Semilla ^ 0x4F4F);
            for (int i = 0; i < 8; i++)
            {
                float hx = (float)(rndHong.NextDouble() * w * 0.8 + w * 0.1);
                float hy = hz + (h - hz) * 0.72f + (float)(rndHong.NextDouble() * (h - hz) * 0.22);
                float hr = 4 + (float)(rndHong.NextDouble() * 8);
                Color cS = rndHong.NextDouble() > 0.5 ? Color.FromArgb(215, 52, 28) : Color.FromArgb(215, 162, 28);
                Mancha(g, hx, hy - hr, hr, hr * 0.55f, cS, 162, 0.22f);
                for (int p = 0; p < 3; p++)
                    Mancha(g, hx + (float)(rndHong.NextDouble() * hr - hr * 0.5f),
                        hy - hr * (0.6f + (float)(rndHong.NextDouble() * 0.3f)),
                        hr * 0.15f, hr * 0.12f, Color.White, 152, 0.14f);
                using (var pen = new Pen(CA(Color.FromArgb(235, 222, 198), 162), hr * 0.35f))
                    g.DrawLine(pen, hx, hy, hx, hy - hr * 0.55f);
            }
        }

        private void PintarFlores(Graphics g, int w, int h, List<Color> pal)
        {
            int hz = (int)(h * 0.37);
            PintarCieloAcuarela(g, w, hz, Color.FromArgb(172, 215, 255), Color.FromArgb(220, 238, 255));
            PintarSolAcuarela(g, w, hz, Color.FromArgb(255, 238, 118));

            // Pasto: degradado único sin acumulación de blanco
            using (var br = new System.Drawing.Drawing2D.LinearGradientBrush(
                new PointF(0, hz), new PointF(0, h),
                CA(Color.FromArgb(52, 148, 42), 215), CA(Color.FromArgb(28, 85, 18), 240)))
                g.FillRectangle(br, 0, hz, w, h - hz);

            Color[] florCols = pal.Count >= 4 ? pal.ToArray() : new Color[] {
                Color.FromArgb(218, 42, 78), Color.FromArgb(255, 178, 25),
                Color.FromArgb(128, 40, 195), Color.FromArgb(255, 82, 142),
                Color.FromArgb(52, 178, 255), Color.FromArgb(255, 122, 38)
            };
            var rndF = new Random(_ctx.Semilla);

            for (int i = 0; i < 600; i++)
            {
                float fx  = (float)(rndF.NextDouble() * w);
                float fy  = hz + (float)(rndF.NextDouble() * (h - hz) * 0.70);
                float pct = (fy - hz) / (h - hz);
                Mancha(g, fx, fy, 1.8f + pct * 5, 1.8f + pct * 4,
                    Varia(florCols[rndF.Next(florCols.Length)], 28), (int)(38 + pct * 70), 0.42f);
            }
            for (int i = 0; i < 210; i++)
            {
                float fx  = (float)(rndF.NextDouble() * w);
                float fy  = hz + (float)(rndF.NextDouble() * (h - hz) * 0.85);
                float pct = (fy - hz) / (h - hz);
                float rs  = 3.5f + pct * 10;
                Color cf  = Varia(florCols[rndF.Next(florCols.Length)], 22);
                Mancha(g, fx, fy, rs, rs * 0.88f, cf, (int)(60 + pct * 88), 0.38f);
                Mancha(g, fx, fy, rs * 0.25f, rs * 0.25f, Color.FromArgb(255, 210, 0), (int)(115 + pct * 60), 0.10f);
            }
            for (int i = 0; i < 60; i++)
            {
                float fx  = (float)(rndF.NextDouble() * w);
                float fy  = hz + (h - hz) * 0.38f + (float)(rndF.NextDouble() * (h - hz) * 0.55);
                float pct = (fy - hz) / (h - hz);
                Color cf  = Varia(florCols[rndF.Next(florCols.Length)], 18);
                float rs  = 7.5f + pct * 20;
                int nPet  = 5 + rndF.Next(4);
                for (int pi = 0; pi < nPet; pi++)
                {
                    double ang = pi * Math.PI * 2 / nPet + rndF.NextDouble() * 0.28;
                    Mancha(g, fx + (float)(Math.Cos(ang) * rs * 0.72), fy + (float)(Math.Sin(ang) * rs * 0.55),
                        rs * 0.48f, rs * 0.38f, cf, (int)(78 + pct * 108), 0.36f);
                }
                Mancha(g, fx, fy, rs * 0.26f, rs * 0.26f, Color.FromArgb(255, 210, 0), (int)(142 + pct * 68), 0.10f);
                Mancha(g, fx, fy, rs * 0.12f, rs * 0.12f, Color.FromArgb(128, 78, 18), 185, 0.07f);
            }
            for (int i = 0; i < 85; i++)
            {
                float fx   = (float)(rndF.NextDouble() * w);
                float fy   = hz + (h - hz) * 0.5f + (float)(rndF.NextDouble() * (h - hz) * 0.45f);
                float fLen = 14 + (float)(rndF.NextDouble() * 30);
                using (var pen = new Pen(CA(Color.FromArgb(42, 118, 32), 72 + rndF.Next(58)), 1.4f))
                    g.DrawLine(pen, fx, fy, fx + (float)(rndF.NextDouble() * 7 - 3.5), fy + fLen);
            }

            // Árbol solitario (punto focal)
            ArbolCaducifolio(g, (float)(w * 0.22), hz + (h - hz) * 0.55f, (h - hz) * 0.65f,
                Color.FromArgb(72, 48, 22), MixColor(pal[0], Color.FromArgb(32, 128, 22), 0.35f));

            // Muro de piedra en primer plano
            float muroY = hz + (h - hz) * 0.74f;
            MuroPiedra(g, 0, muroY, w * 0.38f, 18, Color.FromArgb(148, 138, 118));
            MuroPiedra(g, w * 0.62f, muroY, w * 0.38f, 18, Color.FromArgb(148, 138, 118));

            // Mariposas
            var rndM = new Random(_ctx.Semilla ^ 0xB1B1);
            for (int i = 0; i < 12; i++)
            {
                float bx  = (float)(rndM.NextDouble() * w * 0.85 + w * 0.07);
                float by  = hz + (float)(rndM.NextDouble() * (h - hz) * 0.65);
                float br2 = 3.5f + (float)(rndM.NextDouble() * 5);
                Color cb  = Varia(florCols[rndM.Next(florCols.Length)], 18);
                Mancha(g, bx - br2 * 0.6f, by, br2, br2 * 0.65f, cb, 145, 0.22f);
                Mancha(g, bx + br2 * 0.6f, by, br2, br2 * 0.65f, cb, 145, 0.22f);
                using (var pen = new Pen(CA(Color.FromArgb(28, 22, 14), 162), 1.2f))
                    g.DrawLine(pen, bx, by - br2 * 0.6f, bx, by + br2 * 0.6f);
            }

            BandadaPajaros(g, w, hz, 3 + _rnd.Next(3), new Random(_ctx.Semilla ^ 0xC2D3));
        }

        private void PintarDesierto(Graphics g, int w, int h, List<Color> pal)
        {
            int hz = (int)(h * 0.35);
            PintarCieloAcuarela(g, w, hz, Color.FromArgb(245, 178, 58), Color.FromArgb(255, 222, 128),
                Color.FromArgb(255, 128, 38));
            PintarSolAcuarela(g, w, hz, Color.FromArgb(255, 225, 78));

            Color cDuna1  = pal[0];
            Color cDuna2  = pal.Count > 2 ? pal[2] : Color.FromArgb(172, 108, 36);
            Color cSombra = MixColor(cDuna2, Color.FromArgb(78, 38, 8), 0.48f);
            var   rndD    = new Random(_ctx.Semilla);

            // Mesa/butte (punto focal)
            float mesaX = (float)(w * 0.68);
            float mesaW = (float)(w * 0.18);
            float mesaH = (float)(hz * 0.52);
            float mesaY = hz - mesaH;
            var ptsMesa = new PointF[] {
                new PointF(mesaX - mesaW * 0.52f, hz), new PointF(mesaX - mesaW * 0.48f, mesaY + mesaH * 0.08f),
                new PointF(mesaX - mesaW * 0.42f, mesaY), new PointF(mesaX + mesaW * 0.42f, mesaY),
                new PointF(mesaX + mesaW * 0.48f, mesaY + mesaH * 0.08f), new PointF(mesaX + mesaW * 0.52f, hz)
            };
            using (var path = new System.Drawing.Drawing2D.GraphicsPath())
            {
                path.AddLines(ptsMesa); path.CloseFigure();
                using (var br = new SolidBrush(CA(MixColor(cDuna2, Color.FromArgb(148, 62, 22), 0.55f), 175)))
                    g.FillPath(br, path);
                using (var brL = new System.Drawing.Drawing2D.LinearGradientBrush(
                    new PointF(mesaX - mesaW * 0.42f, mesaY), new PointF(mesaX + mesaW * 0.42f, mesaY),
                    CA(MixColor(cDuna2, Color.White, 0.28f), 42), CA(MixColor(cDuna2, Color.Black, 0.25f), 32)))
                    g.FillPath(brL, path);
            }
            for (int est = 0; est < 4; est++)
            {
                float ey = mesaY + mesaH * (0.18f + est * 0.18f);
                using (var pen = new Pen(CA(MixColor(cDuna1, cDuna2, (float)est / 3), 52), 1.8f))
                    g.DrawLine(pen, mesaX - mesaW * 0.46f, ey, mesaX + mesaW * 0.46f, ey);
            }

            // Dunas
            for (int duna = 0; duna < 6; duna++)
            {
                double tS   = _ctx.Semilla * 0.005 + duna * 4.3;
                int    yB   = hz + (int)((h - hz) * (0.07 + duna * 0.158));
                int    nPts = w / 3;
                var    pts  = new List<PointF> { new PointF(-10, h) };
                for (int xi = 0; xi <= nPts; xi++)
                {
                    double nx = (double)xi / nPts;
                    double n  = Matematica.FBM(nx * 2.2 + tS, 0.5, 4, 0.58);
                    float py  = yB - (float)((n + 1) * 0.5 * (h - hz) * 0.26);
                    pts.Add(new PointF((float)(nx * w), Math.Max(hz, py)));
                }
                pts.Add(new PointF(w + 10, h));
                using (var path = new System.Drawing.Drawing2D.GraphicsPath())
                {
                    path.AddCurve(pts.ToArray(), 0.45f);
                    path.AddLine(w + 10, h, -10, h); path.CloseFigure();
                    Color cD = MixColor(cDuna1, cDuna2, (float)duna / 5);
                    using (var br = new SolidBrush(CA(Varia(cD, 18), 152 + duna * 14))) g.FillPath(br, path);
                    using (var br2 = new SolidBrush(CA(cSombra, 38 + duna * 7))) g.FillPath(br2, path);
                    var ptsC = new PointF[pts.Count - 2];
                    for (int xi = 1; xi < pts.Count - 1; xi++) ptsC[xi-1] = new PointF(pts[xi].X, pts[xi].Y - 2.5f);
                    using (var pen = new Pen(CA(MixColor(cDuna1, Color.White, 0.52f), 62), 2f))
                        g.DrawCurve(pen, ptsC, 0.45f);
                }
            }

            // Espejismo
            using (var br = new System.Drawing.Drawing2D.LinearGradientBrush(
                new PointF(0, hz + 2), new PointF(0, hz + (h - hz) * 0.12f),
                CA(Color.FromArgb(88, 152, 215), 30), Color.Transparent))
                g.FillRectangle(br, w * 0.2f, hz + 2, w * 0.6f, (h - hz) * 0.10f);

            PintarCactus(g, w, h, hz, MixColor(cDuna2, Color.Black, 0.65f));

            // Caravana de camellos lejana
            float carX = (float)(w * 0.42);
            float carY = hz + (h - hz) * 0.08f;
            for (int c = 0; c < 3; c++)
            {
                float cx2 = carX + c * 22;
                float cy2 = carY + (float)(_rnd.NextDouble() * 4);
                using (var br = new SolidBrush(CA(Color.FromArgb(52, 38, 22), 112)))
                {
                    g.FillEllipse(br, cx2 - 7, cy2 - 5, 14, 8);
                    g.FillEllipse(br, cx2 - 3, cy2 - 9, 7, 6);
                }
                using (var pen = new Pen(CA(Color.FromArgb(52, 38, 22), 112), 2.2f))
                    g.DrawLine(pen, cx2 + 4, cy2 - 4, cx2 + 6, cy2 - 9);
                for (int p = 0; p < 4; p++)
                {
                    float px2 = cx2 - 6 + p * 4;
                    using (var pen = new Pen(CA(Color.FromArgb(42, 28, 12), 105), 1.2f))
                        g.DrawLine(pen, px2, cy2 + 3, px2 + (float)(_rnd.NextDouble() * 2 - 1), cy2 + 8);
                }
            }

            // Calima
            using (var br = new System.Drawing.Drawing2D.LinearGradientBrush(
                new PointF(0, hz - h * 0.05f), new PointF(0, hz + h * 0.13f),
                CA(Color.FromArgb(255, 215, 118), 52), Color.Transparent))
                g.FillRectangle(br, 0, hz - h * 0.05f, w, h * 0.18f);
        }

        private void PintarCactus(Graphics g, int w, int h, int hz, Color cCactus)
        {
            var rndC = new Random(_ctx.Semilla ^ 0xCACA);
            int nCact = 3 + rndC.Next(5);
            for (int i = 0; i < nCact; i++)
            {
                float cx   = (float)(rndC.NextDouble() * w);
                float cy   = hz + (float)(rndC.NextDouble() * (h - hz) * 0.45);
                float alt  = (float)(h * 0.07 + rndC.NextDouble() * h * 0.13);
                float gros = 4.5f + (float)(rndC.NextDouble() * 6);

                using (var pen = new Pen(CA(cCactus, 172), gros))
                    g.DrawLine(pen, cx, cy, cx, cy - alt);

                int nBrazos = 1 + rndC.Next(3);
                for (int b = 0; b < nBrazos; b++)
                {
                    float bY   = cy - alt * (0.32f + (float)(rndC.NextDouble() * 0.42));
                    float bDir = rndC.NextDouble() > 0.5 ? 1f : -1f;
                    float bLen = alt * (0.28f + (float)(rndC.NextDouble() * 0.38));
                    float bUp  = alt * 0.22f;
                    using (var pen = new Pen(CA(cCactus, 155), gros * 0.68f))
                    {
                        g.DrawLine(pen, cx, bY, cx + bDir * bLen, bY);
                        g.DrawLine(pen, cx + bDir * bLen, bY, cx + bDir * bLen, bY - bUp);
                    }
                }
            }
        }

        // ── CIUDAD ────────────────────────────────────────────────────────────

        private void PintarCiudad(Graphics g, int w, int h, List<Color> pal)
        {
            int hz = (int)(h * 0.44);
            PintarCieloAcuarela(g, w, hz, Color.FromArgb(152, 192, 238), Color.FromArgb(198, 218, 248),
                Color.FromArgb(255, 198, 138));

            // Asfalto mojado
            Color cAsf = Color.FromArgb(58, 58, 68);
            using (var br = new System.Drawing.Drawing2D.LinearGradientBrush(
                new PointF(0, hz), new PointF(0, h),
                CA(cAsf, 210), CA(MixColor(cAsf, Color.Black, 0.30f), 240)))
                g.FillRectangle(br, 0, hz, w, h - hz);
            // Línea de carril
            int nGuiones = w / 38;
            for (int gi = 0; gi < nGuiones; gi++)
            {
                if (gi % 2 == 0)
                    using (var pen = new Pen(CA(Color.FromArgb(215, 205, 155), 48), 2f))
                        g.DrawLine(pen, gi * 38, hz + (h - hz) * 0.55f, gi * 38 + 18, hz + (h - hz) * 0.55f);
            }

            var rndC = new Random(_ctx.Semilla);
            int nEdif = 10 + rndC.Next(6);

            // Edificios lejanos
            for (int i = 0; i < nEdif * 2; i++)
            {
                int bw = 14 + rndC.Next(38), bh = 35 + rndC.Next(85), bx = rndC.Next(w - bw);
                Color cBase = MixColor(pal.Count > 0 ? pal[rndC.Next(pal.Count)] : Color.Gray,
                    Color.FromArgb(135, 148, 185), 0.65f);
                using (var br = new SolidBrush(CA(cBase, 48 + rndC.Next(35))))
                    g.FillRectangle(br, bx, hz - bh, bw, bh);
            }

            // Edificios con personalidad
            for (int i = 0; i < nEdif; i++)
            {
                int bw = 28 + rndC.Next(68), bh = 55 + rndC.Next(175), bx = rndC.Next(w - bw);
                Color cBase = pal.Count > 0 ? Varia(pal[rndC.Next(pal.Count)], 22) : Color.FromArgb(95, 112, 155);

                using (var br = new SolidBrush(CA(cBase, 95 + rndC.Next(55))))
                    g.FillRectangle(br, bx, hz - bh, bw, bh);
                using (var lbr = new System.Drawing.Drawing2D.LinearGradientBrush(
                    new PointF(bx, hz - bh), new PointF(bx + bw, hz - bh),
                    CA(Color.White, 25), CA(MixColor(cBase, Color.Black, 0.20f), 18)))
                    g.FillRectangle(lbr, bx, hz - bh, bw, bh);

                // Retranqueo superior
                if (bh > 80 && rndC.NextDouble() > 0.4f)
                {
                    int rb = (int)(bw * (0.15f + rndC.NextDouble() * 0.2f));
                    int rh = (int)(bh * (0.22f + rndC.NextDouble() * 0.18f));
                    using (var br = new SolidBrush(CA(Varia(cBase, 15), 105)))
                        g.FillRectangle(br, bx + rb, hz - bh, bw - rb * 2, rh);
                }

                // Ventanas
                if (bw > 24)
                {
                    for (int vy = 0; vy < bh / 14; vy++)
                    for (int vx2 = 0; vx2 < bw / 12; vx2++)
                    {
                        int wx = bx + 3 + vx2 * 12, wy = hz - bh + 5 + vy * 14;
                        if (wx + 7 > bx + bw - 2 || wy + 9 > hz - 1) continue;
                        bool enc = rndC.NextDouble() > 0.55f;
                        Color cV = enc ? Color.FromArgb(195, 225, 255) : Color.FromArgb(62, 82, 118);
                        using (var bv = new SolidBrush(CA(cV, enc ? 145 : 60)))
                            g.FillRectangle(bv, wx, wy, 7, 9);
                    }
                }

                // Corona: antena, tanque de agua o terraza
                double ct = rndC.NextDouble();
                if (ct < 0.30f)
                {
                    float ax = bx + bw / 2f;
                    using (var pen = new Pen(CA(Color.FromArgb(148, 148, 162), 162), 2f))
                        g.DrawLine(pen, ax, hz - bh, ax, hz - bh - 18 - rndC.Next(22));
                }
                else if (ct < 0.58f)
                {
                    float tx = bx + bw * 0.3f, ty = hz - bh - 12;
                    using (var br = new SolidBrush(CA(Color.FromArgb(92, 72, 48), 158)))
                        g.FillEllipse(br, tx, ty, bw * 0.40f, 12);
                    for (int s = 0; s < 3; s++)
                        using (var pen = new Pen(CA(Color.FromArgb(72, 52, 32), 142), 2f))
                            g.DrawLine(pen, tx + s * bw * 0.17f + 4, hz - bh, tx + s * bw * 0.17f, ty + 10);
                }
                else
                {
                    using (var br = new SolidBrush(CA(MixColor(cBase, Color.FromArgb(22, 38, 68), 0.38f), 102)))
                        g.FillRectangle(br, bx, hz - bh - 8, bw, 8);
                }

                // Reflejo en el asfalto
                int refH = Math.Min(bh / 4, (h - hz) / 2);
                using (var rbr = new System.Drawing.Drawing2D.LinearGradientBrush(
                    new PointF(0, hz), new PointF(0, hz + refH),
                    CA(cBase, 40), Color.Transparent))
                    g.FillRectangle(rbr, bx, hz, bw, refH);
            }

            // Grúa de construcción
            if (_rnd.NextDouble() > 0.45f)
            {
                float gx = (float)(w * 0.72), gy = hz - (float)(h * 0.28);
                using (var pen = new Pen(CA(Color.FromArgb(225, 168, 22), 145), 3f))
                {
                    g.DrawLine(pen, gx, hz, gx, gy);
                    g.DrawLine(pen, gx - w * 0.08f, gy, gx + w * 0.05f, gy);
                    g.DrawLine(pen, gx - w * 0.08f, gy, gx - w * 0.08f, gy + 15);
                }
                using (var pen = new Pen(CA(Color.FromArgb(88, 88, 98), 112), 1f))
                    g.DrawLine(pen, gx + w * 0.03f, gy, gx + w * 0.03f, gy + 28);
            }

            // Faroles de calle
            int nFaroles = 4 + _rnd.Next(3);
            for (int fi = 0; fi < nFaroles; fi++)
            {
                float fx = (float)(fi * w / (nFaroles - 1.0));
                using (var pen = new Pen(CA(Color.FromArgb(52, 52, 62), 185), 2.5f))
                    g.DrawLine(pen, fx, hz, fx, hz + (h - hz) * 0.32f);
                using (var pen = new Pen(CA(Color.FromArgb(52, 52, 62), 165), 1.8f))
                    g.DrawLine(pen, fx, hz, fx + 10, hz - 8);
            }

            BandadaPajaros(g, w, hz, 3 + _rnd.Next(4), new Random(_ctx.Semilla ^ 0xC8D9));
        }

        // ── ABSTRACTO ─────────────────────────────────────────────────────────

        private void PintarAbstracto(Graphics g, int w, int h, List<Color> pal)
        {
            var rndA = new Random(_ctx.Semilla);
            using (var br = new SolidBrush(CA(MixColor(pal[0], pal[pal.Count > 1 ? 1 : 0], 0.5f), 38)))
                g.FillRectangle(br, 0, 0, w, h);

            for (int capa = 0; capa < 5; capa++)
            {
                int n = 6 + capa * 11;
                for (int i = 0; i < n; i++)
                {
                    float cx = (float)(rndA.NextDouble() * w);
                    float cy = (float)(rndA.NextDouble() * h);
                    float rx = (float)(w * (0.038 + rndA.NextDouble() * 0.22 / (capa + 1)));
                    float ry = (float)(h * (0.032 + rndA.NextDouble() * 0.20 / (capa + 1)));
                    Mancha(g, cx, cy, rx, ry, Varia(pal[rndA.Next(pal.Count)], 22),
                        33 + capa * 18 + rndA.Next(55), 0.32f);
                }
            }
            for (int i = 0; i < 45; i++)
            {
                float x0 = (float)(rndA.NextDouble() * w);
                float y0 = (float)(rndA.NextDouble() * h);
                float x1 = x0 + (float)((rndA.NextDouble() - 0.5) * w * 0.42);
                float y1 = y0 + (float)((rndA.NextDouble() - 0.5) * h * 0.42);
                Pincelada(g, x0, y0, x1, y1, 2 + (float)(rndA.NextDouble() * 12),
                    Varia(pal[rndA.Next(pal.Count)], 22), 28 + rndA.Next(58));
            }
        }

        // ── NOCHE ─────────────────────────────────────────────────────────────

        private void PintarNoche(Graphics g, int w, int h, List<Color> pal)
        {
            int   hz      = (int)(h * 0.45);
            Color cCielo1 = pal[0];
            Color cCielo2 = pal.Count > 1 ? pal[1] : Color.FromArgb(22, 32, 108);
            Color cAcento = pal.Count > 2 ? pal[2] : Color.FromArgb(65, 50, 132);

            using (var br = new System.Drawing.Drawing2D.LinearGradientBrush(
                new PointF(0, 0), new PointF(0, hz),
                CA(cCielo1, 255), CA(MixColor(cCielo2, Color.FromArgb(48, 38, 118), 0.52f), 238)))
                g.FillRectangle(br, 0, 0, w, hz);
            using (var br = new System.Drawing.Drawing2D.LinearGradientBrush(
                new PointF(0, hz - hz * 0.28f), new PointF(0, hz),
                Color.Transparent, CA(Color.FromArgb(78, 52, 128), 48)))
                g.FillRectangle(br, 0, hz - hz * 0.28f, w, hz * 0.28f);

            var rndN = new Random(_ctx.Semilla);

            for (int pass = 0; pass < 2; pass++)
            {
                int nVM = pass == 0 ? 160 : 90;
                for (int i = 0; i < nVM; i++)
                {
                    float bx  = (float)(rndN.NextDouble() * w * 1.6 - w * 0.3);
                    float byR = (float)(rndN.NextDouble() * hz * 0.82 + hz * 0.02);
                    float by  = byR + (bx - w * 0.5f) * 0.38f;
                    if (by < 2 || by > hz - 2) continue;
                    float brx = pass == 0
                        ? (float)(w * 0.014 + rndN.NextDouble() * w * 0.038)
                        : (float)(w * 0.007 + rndN.NextDouble() * w * 0.016);
                    Mancha(g, bx, by, brx, brx * 0.48f,
                        pass == 0 ? Color.FromArgb(152, 158, 215) : Color.FromArgb(198, 205, 255),
                        4 + rndN.Next(pass == 0 ? 10 : 20), 0.62f);
                }
            }

            for (int i = 0; i < 580; i++)
            {
                float  sx = (float)(rndN.NextDouble() * w);
                float  sy = (float)(rndN.NextDouble() * hz * 0.97f);
                double ch = rndN.NextDouble();
                float  sr; int sa;
                if      (ch < 0.048) { sr = 2.8f; sa = 188 + rndN.Next(65); }
                else if (ch < 0.18)  { sr = 1.6f; sa = 138 + rndN.Next(90); }
                else                 { sr = 0.8f; sa =  88 + rndN.Next(132); }

                if (sr > 1.5f) Mancha(g, sx, sy, sr * 2.8f, sr * 2.8f, cAcento, 22, 0.28f);
                if (sr > 2.0f) Mancha(g, sx, sy, sr * 1.5f, sr * 1.5f, Color.FromArgb(198, 212, 255), 42, 0.18f);
                Mancha(g, sx, sy, sr, sr, Color.FromArgb(242, 240, 255), sa, 0.07f);

                if (sr > 2.0f)
                {
                    using (var pen = new Pen(CA(Color.FromArgb(218, 228, 255), sa / 4), 0.8f))
                    {
                        g.DrawLine(pen, sx - sr * 4, sy, sx + sr * 4, sy);
                        g.DrawLine(pen, sx, sy - sr * 4, sx, sy + sr * 4);
                    }
                }
            }

            float lx = (float)(w * 0.70f);
            float ly = (float)(hz * 0.17f);
            float lr = (float)(Math.Min(w, hz) * 0.053f);
            for (int ri = 5; ri >= 0; ri--)
            {
                Mancha(g, lx, ly, lr * (1 + ri * 1.1f) * 2.4f, lr * (1 + ri * 1.1f) * 2.4f,
                    MixColor(Color.FromArgb(188, 202, 255), cAcento, ri * 0.11f),
                    11 + ri * 5, 0.42f);
            }
            Mancha(g, lx, ly, lr * 1.12f, lr * 1.12f, Color.FromArgb(240, 246, 228), 222, 0.06f);
            for (int c = 0; c < 6; c++)
            {
                float ca2 = (float)(rndN.NextDouble() * Math.PI * 2);
                float cd  = (float)(rndN.NextDouble() * lr * 0.62f);
                float cr  = (float)(lr * (0.07 + rndN.NextDouble() * 0.18));
                Mancha(g, lx + (float)(Math.Cos(ca2) * cd), ly + (float)(Math.Sin(ca2) * cd),
                    cr, cr * 0.88f, Color.FromArgb(208, 215, 198), 32, 0.22f);
            }

            int yTerr    = hz - (int)(hz * 0.10f);
            int nPtsTerr = w / 4;
            var ptsTerr  = new List<PointF>();
            ptsTerr.Add(new PointF(-5, h));
            for (int xi = 0; xi <= nPtsTerr; xi++)
            {
                double nx = (double)xi / nPtsTerr;
                double n  = Matematica.FBM(nx * 4.5 + _ctx.Semilla * 0.004, 0.5, 5, 0.60);
                float  py = yTerr - (float)((n + 1) * 0.5 * hz * 0.28);
                ptsTerr.Add(new PointF((float)(nx * w), Math.Max(5, py)));
            }
            ptsTerr.Add(new PointF(w + 5, h));
            using (var path = new System.Drawing.Drawing2D.GraphicsPath())
            {
                path.AddCurve(ptsTerr.ToArray(), 0.3f);
                path.AddLine(w + 5, h, -5, h);
                path.CloseFigure();
                using (var br = new SolidBrush(CA(MixColor(cCielo1, Color.Black, 0.52f), 250)))
                    g.FillPath(br, path);
                using (var br2 = new SolidBrush(CA(Color.FromArgb(48, 52, 108), 28)))
                    g.FillPath(br2, path);
            }

            int yAgua = (int)(hz + (h - hz) * 0.07f);
            using (var br = new System.Drawing.Drawing2D.LinearGradientBrush(
                new PointF(0, yAgua), new PointF(0, h),
                CA(MixColor(cCielo1, cCielo2, 0.5f), 218),
                CA(MixColor(cCielo1, Color.Black, 0.65f), 235)))
                g.FillRectangle(br, 0, yAgua, w, h - yAgua);

            for (int i = 0; i < 18; i++)
            {
                float ry  = yAgua + i * (float)(h - yAgua) / 18f;
                float rw2 = 5 + i * 8.5f + (float)(rndN.NextDouble() * 24);
                float rxO = lx + (float)(rndN.NextDouble() * 15 - 7.5f);
                Mancha(g, rxO, ry, rw2 * 0.5f, 3.4f, Color.FromArgb(242, 244, 212),
                    Math.Max(3, 92 - i * 5), 0.30f);
            }

            for (int i = 0; i < 70; i++)
            {
                float sx = (float)(rndN.NextDouble() * w);
                float sy = yAgua + (float)(rndN.NextDouble() * (h - yAgua));
                Mancha(g, sx, sy, 1.2f, 0.7f, Color.FromArgb(198, 202, 242), 32 + rndN.Next(42), 0.20f);
            }
        }

        // ── LAGO ──────────────────────────────────────────────────────────────

        private void PintarLago(Graphics g, int w, int h, List<Color> pal)
        {
            int   hz    = (int)(h * 0.42);
            Color cCielo = pal.Count > 3 ? pal[3] : Color.FromArgb(192, 222, 242);
            Color cVeg   = pal.Count > 2 ? pal[2] : Color.FromArgb(32, 105, 46);
            Color cAgua  = pal[0];

            PintarCieloAcuarela(g, w, hz, cCielo, Color.FromArgb(228, 244, 255), Color.FromArgb(255, 238, 172));
            PintarSolAcuarela(g, w, hz, Color.FromArgb(255, 240, 172));

            using (var br = new System.Drawing.Drawing2D.LinearGradientBrush(
                new PointF(0, hz), new PointF(0, h),
                CA(cVeg, 195), CA(MixColor(cVeg, Color.FromArgb(18, 62, 16), 0.52f), 218)))
                g.FillRectangle(br, 0, hz, w, h - hz);

            float lakeCX = w * 0.5f, lakeCY = hz + (h - hz) * 0.42f;
            float lakeRX = w * 0.35f, lakeRY = (h - hz) * 0.28f;

            int nPL = 32; var ptLago = new PointF[nPL];
            for (int i = 0; i < nPL; i++)
            {
                double ang  = i * Math.PI * 2 / nPL;
                double varR = 1.0 + Matematica.FBM(ang * 2.5 + _ctx.Semilla * 0.012, 0.5, 3, 0.52) * 0.22;
                ptLago[i] = new PointF(
                    lakeCX + (float)(Math.Cos(ang) * lakeRX * varR),
                    lakeCY + (float)(Math.Sin(ang) * lakeRY * varR));
            }
            using (var pathLago = new System.Drawing.Drawing2D.GraphicsPath())
            {
                pathLago.AddClosedCurve(ptLago, 0.42f);
                using (var br = new SolidBrush(CA(cAgua, 215))) g.FillPath(br, pathLago);
                using (var br2 = new SolidBrush(CA(MixColor(cCielo, cAgua, 0.35f), 65))) g.FillPath(br2, pathLago);
                try
                {
                    using (var pgb = new System.Drawing.Drawing2D.PathGradientBrush(pathLago))
                    {
                        pgb.CenterColor    = CA(MixColor(cAgua, Color.White, 0.22f), 38);
                        pgb.SurroundColors = new[] { CA(MixColor(cAgua, Color.Black, 0.18f), 82) };
                        g.FillPath(pgb, pathLago);
                    }
                }
                catch { }
            }

            // Niebla matinal sobre el agua
            BandaNiebla(g, lakeCX - lakeRX * 0.85f, lakeCY - lakeRY * 0.28f, lakeRX * 1.7f, lakeRY * 0.55f, 30);

            for (int i = 1; i <= 7; i++)
            {
                float sc = (float)i / 7 * 0.88f;
                using (var pen = new Pen(CA(Color.FromArgb(172, 215, 248), 14 + i * 6), 0.9f))
                    g.DrawEllipse(pen, lakeCX - lakeRX * sc, lakeCY - lakeRY * sc, lakeRX * sc * 2, lakeRY * sc * 2);
            }

            PintarPinosAcuarela(g, w, h, hz, cVeg);

            // Árbol caducifolio en la orilla
            ArbolCaducifolio(g, lakeCX - lakeRX * 0.88f, lakeCY + lakeRY * 0.55f, (h - hz) * 0.55f,
                Color.FromArgb(58, 38, 18), MixColor(cVeg, Color.FromArgb(42, 158, 38), 0.28f));

            // Muelle y bote
            float muelleX = lakeCX - lakeRX * 0.35f;
            float muelleY = lakeCY - lakeRY * 0.15f;
            Muelle(g, muelleX - 12, muelleY, 65, 8, Color.FromArgb(132, 98, 58));
            Bote(g, muelleX + 52, muelleY + 6, 0.85f, Color.FromArgb(122, 78, 38), false);

            // Nenúfares
            var rndNen = new Random(_ctx.Semilla ^ 0x3333);
            for (int i = 0; i < 22; i++)
            {
                double ang  = rndNen.NextDouble() * Math.PI * 2;
                double dist = rndNen.NextDouble() * 0.72;
                float  nx   = lakeCX + (float)(Math.Cos(ang) * lakeRX * dist * 0.82f);
                float  ny   = lakeCY + (float)(Math.Sin(ang) * lakeRY * dist * 0.82f);
                float  nr   = 3.5f + (float)(rndNen.NextDouble() * 7.5);
                Mancha(g, nx, ny, nr, nr * 0.52f, Color.FromArgb(38, 128, 52), 128, 0.24f);
                if (rndNen.NextDouble() > 0.42)
                    Mancha(g, nx, ny, nr * 0.40f, nr * 0.40f, Color.FromArgb(255, 168, 198), 172, 0.14f);
            }

            using (var br = new SolidBrush(CA(MixColor(cVeg, cAgua, 0.48f), 45)))
                g.FillRectangle(br, 0, (int)(lakeCY - lakeRY * 0.82), w, (int)(lakeRY * 0.50f));

            BandadaPajaros(g, w, hz, 3 + _rnd.Next(4), new Random(_ctx.Semilla ^ 0xD4E5));
        }

        private void PintarValle(Graphics g, int w, int h, List<Color> pal)
        {
            int   hz    = (int)(h * 0.40);
            Color cV1   = pal.Count > 0 ? pal[0] : Color.FromArgb(42, 122, 42);
            Color cV2   = pal.Count > 1 ? pal[1] : Color.FromArgb(72, 152, 62);
            Color cAgua = pal.Count > 3 ? pal[3] : Color.FromArgb(78, 148, 212);

            PintarCieloAcuarela(g, w, hz, Color.FromArgb(148, 195, 252), Color.FromArgb(212, 232, 255),
                Color.FromArgb(255, 212, 152));
            PintarSolAcuarela(g, w, hz, Color.FromArgb(255, 235, 155));
            var rndV = new Random(_ctx.Semilla);

            // Montañas lejanas
            {
                var ptsM = new List<PointF> { new PointF(-5, h) };
                int nPts = w / 4;
                for (int xi = 0; xi <= nPts; xi++)
                {
                    double nx = (double)xi / nPts;
                    double n  = Matematica.FBM(nx * 2.8 + _ctx.Semilla * 0.005, 0.5, 6, 0.60);
                    ptsM.Add(new PointF((float)(nx * w), Math.Max(5, hz - (float)((n + 1) * 0.5 * hz * 0.55))));
                }
                ptsM.Add(new PointF(w + 5, h));
                using (var path = new System.Drawing.Drawing2D.GraphicsPath())
                {
                    path.AddCurve(ptsM.ToArray(), 0.35f);
                    path.AddLine(w + 5, h, -5, h); path.CloseFigure();
                    using (var br = new SolidBrush(CA(Color.FromArgb(168, 188, 218), 162))) g.FillPath(br, path);
                    using (var br2 = new SolidBrush(CA(Color.White, 25))) g.FillPath(br2, path);
                }
            }

            // Colinas con textura de surcos en campo lejano
            Color[] cColinas = { MixColor(cV1, Color.FromArgb(28, 78, 24), 0.42f), cV1, cV2 };
            for (int pl = 0; pl < 3; pl++)
            {
                double tS  = _ctx.Semilla * 0.003 + pl * 5.7;
                int    yB  = hz + (int)((h - hz) * (0.04 + pl * 0.15));
                int    nP  = w / 4;
                var    pts = new List<PointF> { new PointF(-5, h) };
                for (int xi = 0; xi <= nP; xi++)
                {
                    double nx = (double)xi / nP;
                    double n  = Matematica.FBM(nx * 3.2 + tS, 0.5, 4, 0.56);
                    pts.Add(new PointF((float)(nx * w), Math.Max(hz - 12, yB - (float)((n + 1) * 0.5 * (h - hz) * 0.30))));
                }
                pts.Add(new PointF(w + 5, h));
                using (var path = new System.Drawing.Drawing2D.GraphicsPath())
                {
                    path.AddCurve(pts.ToArray(), 0.42f);
                    path.AddLine(w + 5, h, -5, h); path.CloseFigure();
                    using (var br = new SolidBrush(CA(Varia(cColinas[pl], 12), 172 + pl * 22)))
                        g.FillPath(br, path);
                    if (pl == 0) // Surcos en campo lejano
                    {
                        for (int s = 0; s < 12; s++)
                        {
                            float sy = yB - (float)((h - hz) * 0.18 * s / 12);
                            using (var pen = new Pen(CA(MixColor(cColinas[0], Color.Black, 0.15f), 20), 1f))
                                g.DrawLine(pen, 0, sy, w, sy);
                        }
                    }
                }
            }

            // Río serpenteante con orillas vegetadas
            int nRioPts = 28;
            var ptsRio  = new PointF[nRioPts];
            for (int i = 0; i < nRioPts; i++)
            {
                double t = (double)i / (nRioPts - 1);
                float  rx = (float)(w * 0.5 + Math.Sin(t * Math.PI * 3.8 + _ctx.Semilla * 0.008) * w * 0.24);
                ptsRio[i] = new PointF(rx, hz + (float)(t * (h - hz)));
            }
            for (int side = -1; side <= 1; side += 2)
            {
                var ptsOr = new PointF[nRioPts];
                for (int i = 0; i < nRioPts; i++)
                    ptsOr[i] = new PointF(ptsRio[i].X + side * (12 + i * 0.8f), ptsRio[i].Y);
                using (var pen = new Pen(CA(MixColor(cV1, Color.FromArgb(28, 88, 22), 0.55f), 60), 12f))
                {
                    pen.StartCap = pen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
                    g.DrawCurve(pen, ptsOr, 0.5f);
                }
            }
            for (int thick = 4; thick >= 1; thick--)
            {
                Color cRio = thick <= 2 ? MixColor(cAgua, Color.White, 0.28f) : cAgua;
                using (var pen = new Pen(CA(cRio, 42 + thick * 25), thick * 5 + 2))
                {
                    pen.StartCap = pen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
                    pen.LineJoin = System.Drawing.Drawing2D.LineJoin.Round;
                    g.DrawCurve(pen, ptsRio, 0.52f);
                }
            }
            // Destellos en el río
            var rndRi = new Random(_ctx.Semilla ^ 0xF8F8);
            for (int d = 0; d < 10; d++)
            {
                int idx = rndRi.Next(nRioPts - 3) + 1;
                Mancha(g, ptsRio[idx].X, ptsRio[idx].Y, 6 + (float)(rndRi.NextDouble() * 10), 2.5f,
                    Color.FromArgb(215, 235, 252), 62, 0.22f);
            }

            PintarPinosAcuarela(g, w / 3, h, hz + (h - hz) / 4, cV1);
            PintarPinosAcuarela(g, w, h, hz + (h - hz) / 4, cV1);

            // Granja (punto focal)
            float granjaX = (float)(w * 0.72), granjaY = hz + (h - hz) * 0.32f;
            float granW = 52, granH = 36;
            using (var br = new SolidBrush(CA(Color.FromArgb(172, 38, 28), 185)))
                g.FillRectangle(br, granjaX, granjaY, granW, granH);
            var techoP = new PointF[] {
                new PointF(granjaX - 3, granjaY),
                new PointF(granjaX + granW / 2, granjaY - granH * 0.55f),
                new PointF(granjaX + granW + 3, granjaY)
            };
            using (var path = new System.Drawing.Drawing2D.GraphicsPath())
            {
                path.AddLines(techoP); path.CloseFigure();
                using (var br = new SolidBrush(CA(Color.FromArgb(98, 22, 12), 192))) g.FillPath(br, path);
            }
            using (var br = new SolidBrush(CA(Color.FromArgb(205, 195, 165), 175)))
                g.FillRectangle(br, granjaX + granW + 4, granjaY + granH * 0.25f, 14, granH * 0.75f);
            using (var br = new SolidBrush(CA(Color.FromArgb(178, 165, 128), 165)))
                g.FillEllipse(br, granjaX + granW + 2, granjaY + granH * 0.18f, 18, 12);
            MuroPiedra(g, granjaX - 20, granjaY + granH, granW + 50, 14, Color.FromArgb(145, 132, 108));

            // Flores silvestres
            var rndF = new Random(_ctx.Semilla ^ 0x7777);
            Color[] cfls = { Color.FromArgb(255, 225, 52), Color.FromArgb(215, 52, 88),
                              Color.FromArgb(255, 255, 98), Color.FromArgb(198, 98, 225) };
            for (int i = 0; i < 155; i++)
            {
                float fx  = (float)(rndF.NextDouble() * w);
                float fy  = hz + (float)(rndF.NextDouble() * (h - hz));
                float pct = (fy - hz) / (h - hz);
                Mancha(g, fx, fy, 2.2f + pct * 9f, 2.2f + pct * 8f,
                    Varia(cfls[rndF.Next(cfls.Length)], 25), 68 + (int)(pct * 105), 0.36f);
            }

            BandadaPajaros(g, w, hz, 4 + _rnd.Next(4), new Random(_ctx.Semilla ^ 0xE5F6));
        }

        private void PintarVolcan(Graphics g, int w, int h, List<Color> pal)
        {
            int   hz    = (int)(h * 0.44);
            Color cLava1 = pal[0];
            Color cLava2 = pal.Count > 1 ? pal[1] : Color.FromArgb(255, 115, 0);

            // Cielo oscuro y dramático
            using (var br = new System.Drawing.Drawing2D.LinearGradientBrush(
                new PointF(0, 0), new PointF(0, hz),
                CA(Color.FromArgb(8, 4, 2), 252), CA(Color.FromArgb(92, 35, 10), 235)))
                g.FillRectangle(br, 0, 0, w, hz);

            // Resplandor del volcán en el cielo
            Mancha(g, w / 2f, hz * 0.95f, w * 0.28f, hz * 0.45f, cLava2, 45, 0.58f);
            Mancha(g, w / 2f, hz * 0.88f, w * 0.12f, hz * 0.28f, MixColor(cLava2, Color.White, 0.32f), 30, 0.46f);

            var rndV = new Random(_ctx.Semilla);

            // Columna pirocástica: manchas de humo grises superpuestas
            for (int i = 0; i < 55; i++)
            {
                float cx = (float)(w * 0.5 + (rndV.NextDouble() - 0.5) * w * 0.18 * (i / 55.0 + 0.3));
                float cy = (float)(hz * 0.10 + rndV.NextDouble() * hz * 0.70);
                float cr = (float)(w * 0.04 + rndV.NextDouble() * w * 0.16);
                int   a  = 15 + rndV.Next(40);
                Color cH = MixColor(Color.FromArgb(28, 22, 18), Color.FromArgb(95, 82, 72), (float)(rndV.NextDouble()));
                Mancha(g, cx, cy, cr, cr * 0.62f, cH, a, 0.58f);
            }
            // Núcleo brillante de la columna
            for (int i = 0; i < 22; i++)
            {
                float cx = (float)(w * 0.5 + (rndV.NextDouble() - 0.5) * w * 0.06);
                float cy = (float)(hz * 0.08 + rndV.NextDouble() * hz * 0.50);
                float cr = (float)(w * 0.015 + rndV.NextDouble() * w * 0.06);
                Mancha(g, cx, cy, cr, cr * 0.55f, cLava2, 18 + rndV.Next(28), 0.42f);
            }

            // Cono volcánico
            int volcX = w / 2, volcBase = h;
            int volcW = (int)(w * 0.60);
            var ptsCono = new PointF[] {
                new PointF(volcX - volcW / 2f, volcBase),
                new PointF(volcX - (int)(volcW * 0.09f), hz + 2),
                new PointF(volcX + (int)(volcW * 0.09f), hz + 2),
                new PointF(volcX + volcW / 2f, volcBase)
            };
            using (var path = new System.Drawing.Drawing2D.GraphicsPath())
            {
                path.AddLines(ptsCono); path.CloseFigure();
                using (var br = new SolidBrush(CA(Color.FromArgb(16, 11, 7), 242))) g.FillPath(br, path);
                try
                {
                    using (var pgb = new System.Drawing.Drawing2D.PathGradientBrush(path))
                    {
                        pgb.CenterPoint    = new PointF(volcX, hz + (h - hz) * 0.4f);
                        pgb.CenterColor    = CA(Color.FromArgb(58, 38, 22), 112);
                        pgb.SurroundColors = new[] { CA(Color.FromArgb(7, 4, 2), 0) };
                        g.FillPath(pgb, path);
                    }
                }
                catch { }
            }

            // Suelo de lava solidificada
            using (var br = new SolidBrush(CA(Color.FromArgb(20, 14, 9), 192)))
                g.FillRectangle(br, 0, hz, w, h - hz);

            // Ríos de lava con glow (capas: glow exterior → núcleo brillante)
            for (int ri = 0; ri < 7; ri++)
            {
                float rxOff = (float)((rndV.NextDouble() - 0.5) * volcW * 0.55);
                int   nLP   = 22;
                var   ptsLv = new PointF[nLP];
                for (int pi = 0; pi < nLP; pi++)
                {
                    double t  = (double)pi / (nLP - 1);
                    float  lx = volcX + rxOff + (float)(Math.Sin(t * Math.PI * 5.5 + ri * 1.3) * 18);
                    ptsLv[pi] = new PointF(lx, hz + 2 + (float)(t * (h - hz) * 0.82));
                }
                // Glow naranja ancho
                using (var pen = new Pen(CA(MixColor(cLava2, Color.FromArgb(255, 58, 0), 0.52f), 32), 20))
                { pen.StartCap = pen.EndCap = System.Drawing.Drawing2D.LineCap.Round; g.DrawCurve(pen, ptsLv, 0.4f); }
                // Cuerpo naranja-rojo
                using (var pen = new Pen(CA(cLava2, 82), 8))
                { pen.StartCap = pen.EndCap = System.Drawing.Drawing2D.LineCap.Round; g.DrawCurve(pen, ptsLv, 0.4f); }
                // Núcleo brillante
                using (var pen = new Pen(CA(MixColor(cLava2, Color.White, 0.38f), 145 + rndV.Next(52)), 3.8f))
                { pen.StartCap = pen.EndCap = System.Drawing.Drawing2D.LineCap.Round; g.DrawCurve(pen, ptsLv, 0.4f); }
            }

            // Charcos de lava
            for (int i = 0; i < 14; i++)
            {
                float lx  = (float)(rndV.NextDouble() * w);
                float ly  = hz + (float)((0.42 + rndV.NextDouble() * 0.52) * (h - hz));
                float lrx = (float)(w * 0.022 + rndV.NextDouble() * w * 0.082);
                float lry = (float)((h - hz) * 0.012 + rndV.NextDouble() * (h - hz) * 0.048);
                Mancha(g, lx, ly, lrx * 2.2f, lry * 2.2f, cLava2, 32, 0.42f);
                Mancha(g, lx, ly, lrx, lry, MixColor(cLava2, Color.White, 0.22f), 162 + rndV.Next(72), 0.34f);
            }

            // Lluvia de piroclastos (puntos brillantes cayendo)
            for (int i = 0; i < 45; i++)
            {
                float px = (float)(w * 0.35 + rndV.NextDouble() * w * 0.30);
                float py = (float)(rndV.NextDouble() * hz * 0.85);
                float ps = 1.5f + (float)(rndV.NextDouble() * 3.5f);
                Color cp = rndV.NextDouble() > 0.5 ? cLava1 : cLava2;
                Mancha(g, px, py, ps, ps * 0.65f, MixColor(cp, Color.White, 0.35f), 88 + rndV.Next(88), 0.15f);
                using (var pen = new Pen(CA(cp, 55 + rndV.Next(55)), 1f))
                    g.DrawLine(pen, px, py, px + (float)(rndV.NextDouble() * 6 - 3), py + ps * 3);
            }

            // Cráter oscuro con borde incandescente
            Mancha(g, volcX, hz + 2, volcW * 0.09f, volcW * 0.035f, Color.FromArgb(6, 4, 2), 235, 0.12f);
            Mancha(g, volcX, hz + 2, volcW * 0.10f, volcW * 0.040f, cLava2, 55, 0.35f);
        }

        private void PintarPlaya(Graphics g, int w, int h, List<Color> pal)
        {
            int   hz     = (int)(h * 0.37);
            int   hzAgua = (int)(hz + (h - hz) * 0.32f);
            Color cAgua  = pal.Count > 0 ? pal[0] : Color.FromArgb(0, 152, 198);
            Color cArena = pal.Count > 1 ? pal[1] : Color.FromArgb(218, 198, 138);

            PintarCieloAcuarela(g, w, hz, Color.FromArgb(52, 172, 248), Color.FromArgb(128, 212, 255),
                Color.FromArgb(178, 228, 255));
            PintarSolAcuarela(g, w, hz, Color.FromArgb(255, 235, 92));

            Color[] cAguas = {
                Color.FromArgb(0,  205, 238), Color.FromArgb(0, 172, 215),
                cAgua, MixColor(cAgua, Color.FromArgb(0, 58, 128), 0.42f)
            };
            for (int band = 0; band < 4; band++)
            {
                float yB = hz + band * (hzAgua - hz) / 4f;
                using (var br = new SolidBrush(CA(cAguas[band], 182 + band * 12)))
                    g.FillRectangle(br, 0, (int)yB, w, (int)((hzAgua - hz) / 4f) + 2);
            }

            var rndP = new Random(_ctx.Semilla);
            for (int i = 0; i < 14; i++)
            {
                float pct   = (float)i / 14;
                float yOla  = hz + pct * (hzAgua - hz);
                float oGros = 2 + (1 - pct) * 8.5f;
                float x0    = (float)(rndP.NextDouble() * w * 0.17);
                float x1    = w - (float)(rndP.NextDouble() * w * 0.17);
                Pincelada(g, x0, yOla, x1, yOla, oGros + 2, cAguas[Math.Min((int)(pct * 4), 3)], 52 + (int)(pct * 42), 0.07f);
                Pincelada(g, x0, yOla, x1, yOla, oGros * 0.58f, Color.FromArgb(235, 250, 255), 52 + rndP.Next(52), 0.06f);
            }

            for (int layer = 0; layer < 3; layer++)
            {
                Color cA = MixColor(cArena, layer == 0 ? Color.FromArgb(178, 150, 86) : Color.White, layer * 0.11f);
                using (var br = new System.Drawing.Drawing2D.LinearGradientBrush(
                    new PointF(0, hzAgua + layer * 5), new PointF(0, h),
                    CA(MixColor(cA, cAgua, 0.22f), 188 - layer * 12), CA(cA, 225 - layer * 12)))
                    g.FillRectangle(br, 0, hzAgua + layer * 5, w, h - hzAgua);
            }

            // Texturas de arena
            for (int i = 0; i < 45; i++)
            {
                float ax = (float)(rndP.NextDouble() * w);
                float ay = hzAgua + (float)(rndP.NextDouble() * (h - hzAgua));
                Mancha(g, ax, ay, 16 + (float)(rndP.NextDouble() * 40), 3.5f + (float)(rndP.NextDouble() * 8),
                    MixColor(cArena, Color.FromArgb(152, 126, 56), 0.38f), 20 + rndP.Next(26), 0.42f);
            }
            for (int i = 0; i < 25; i++)
            {
                float cx = (float)(rndP.NextDouble() * w);
                float cy = hzAgua + (float)(rndP.NextDouble() * (h - hzAgua) * 0.22);
                Mancha(g, cx, cy, 1.8f + (float)(rndP.NextDouble() * 5), 1.2f + (float)(rndP.NextDouble() * 3),
                    MixColor(Color.White, cArena, 0.28f), 138 + rndP.Next(82), 0.18f);
            }

            // Conchas en la orilla
            var rndC = new Random(_ctx.Semilla ^ 0xC0C0);
            for (int i = 0; i < 14; i++)
            {
                float sx  = (float)(rndC.NextDouble() * w);
                float sy  = hzAgua + (float)(rndC.NextDouble() * (h - hzAgua) * 0.15);
                float sr  = 2.5f + (float)(rndC.NextDouble() * 5);
                Color cSh = rndC.NextDouble() > 0.5 ? Color.FromArgb(242, 218, 192) : Color.FromArgb(215, 165, 140);
                Mancha(g, sx, sy, sr, sr * 0.65f, cSh, 168, 0.18f);
                Mancha(g, sx, sy, sr * 0.4f, sr * 0.25f, Color.FromArgb(255, 240, 225), 145, 0.08f);
            }

            // Sombrilla de playa (punto focal)
            float somX = (float)(w * 0.62), somY = hzAgua + (h - hzAgua) * 0.28f;
            float somR = (float)(Math.Min(w, h) * 0.055f);
            Color cSom = Color.FromArgb(215, 52, 38);
            // Franjas de la sombrilla
            for (int s = 0; s < 6; s++)
            {
                double a1 = s * Math.PI / 3, a2 = (s + 0.5) * Math.PI / 3;
                var ptsSom = new PointF[] {
                    new PointF(somX, somY - somR * 0.12f),
                    new PointF(somX + (float)(Math.Cos(a1) * somR), somY - somR + (float)(Math.Sin(a1) * somR * 0.28f)),
                    new PointF(somX + (float)(Math.Cos(a2) * somR), somY - somR + (float)(Math.Sin(a2) * somR * 0.28f))
                };
                using (var path = new System.Drawing.Drawing2D.GraphicsPath())
                {
                    path.AddLines(ptsSom); path.CloseFigure();
                    Color cFranja = s % 2 == 0 ? cSom : Color.FromArgb(255, 245, 245);
                    using (var br = new SolidBrush(CA(cFranja, 175))) g.FillPath(br, path);
                }
            }
            // Borde y palo
            Mancha(g, somX, somY - somR * 0.65f, somR * 1.05f, somR * 0.38f, cSom, 48, 0.28f);
            using (var pen = new Pen(CA(Color.FromArgb(118, 88, 42), 178), 2.8f))
                g.DrawLine(pen, somX + 2, somY - somR * 0.12f, somX + 5, somY + somR * 0.85f);
            // Toalla
            using (var br = new SolidBrush(CA(Color.FromArgb(52, 148, 215), 88)))
                g.FillRectangle(br, somX - 18, somY + somR * 0.72f, 38, 12);

            // Bote en el mar
            Bote(g, (float)(w * 0.38), hz + (h - hz) * 0.15f, 0.9f, Color.FromArgb(72, 52, 28), true);

            PintarPalmerasAcuarela(g, w, h, hzAgua, pal);

            // Gaviotas
            BandadaPajaros(g, w, hz, 5 + _rnd.Next(5), new Random(_ctx.Semilla ^ 0xA8B9));
        }

        private void PintarPalmerasAcuarela(Graphics g, int w, int h, int hz, List<Color> pal)
        {
            var   rndP    = new Random(_ctx.Semilla ^ 0x2222);
            int   nPalms  = 2 + rndP.Next(4);
            Color cTronco = Color.FromArgb(115, 80, 36);
            Color cHoja   = Color.FromArgb(32, 126, 46);

            for (int i = 0; i < nPalms; i++)
            {
                float px     = (float)(w * (0.04 + rndP.NextDouble() * 0.92));
                float pyBase = hz + (float)(rndP.NextDouble() * (h - hz) * 0.12);
                float alt    = (float)(h * 0.17 + rndP.NextDouble() * h * 0.14);
                float incl   = (float)((rndP.NextDouble() - 0.5) * 0.46);

                int nTronco   = 10;
                var ptsTronco = new PointF[nTronco];
                for (int ti = 0; ti < nTronco; ti++)
                {
                    float t    = (float)ti / (nTronco - 1);
                    float curv = (float)(Math.Sin(t * Math.PI * 0.5) * alt * incl * 1.2);
                    ptsTronco[ti] = new PointF(px + curv, pyBase - t * alt);
                }

                for (int ti = 0; ti < nTronco - 1; ti++)
                {
                    float gros = 8.5f - ti * 0.5f - i * 0.8f;
                    using (var pen = new Pen(CA(Varia(cTronco, 12), 195), Math.Max(2, gros)))
                    {
                        pen.StartCap = pen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
                        g.DrawLine(pen, ptsTronco[ti], ptsTronco[ti + 1]);
                    }
                    if (ti % 2 == 0)
                        using (var pen = new Pen(CA(MixColor(cTronco, Color.Black, 0.28f), 78), Math.Max(1f, gros + 1)))
                            g.DrawLine(pen, ptsTronco[ti].X - 2, ptsTronco[ti].Y,
                                           ptsTronco[ti].X + 2, ptsTronco[ti].Y);
                }

                float topX  = ptsTronco[nTronco - 1].X;
                float topY  = ptsTronco[nTronco - 1].Y;
                int   nHojas = 7 + rndP.Next(5);
                for (int hi = 0; hi < nHojas; hi++)
                {
                    double ang = hi * Math.PI * 2 / nHojas + rndP.NextDouble() * 0.58 - 0.28;
                    float  hLen = (float)(alt * 0.52 + rndP.NextDouble() * alt * 0.22);
                    float  hEx  = topX + (float)(Math.Cos(ang) * hLen);
                    float  hEy  = topY + (float)(Math.Sin(ang) * hLen * 0.56) - hLen * 0.12f;
                    float  mX   = topX + (float)(Math.Cos(ang) * hLen * 0.48);
                    float  mY   = topY + (float)(Math.Sin(ang) * hLen * 0.28) - hLen * 0.16f;

                    Color cH = Varia(cHoja, 22);
                    using (var pen = new Pen(CA(cH, 162 + rndP.Next(62)), 4.2f - hi * 0.15f))
                    {
                        pen.StartCap = pen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
                        using (var path = new System.Drawing.Drawing2D.GraphicsPath())
                        {
                            path.AddBezier(topX, topY, mX, mY - 17, hEx - 11, hEy - 11, hEx, hEy);
                            g.DrawPath(pen, path);
                        }
                    }
                    for (int sj = 0; sj < 4; sj++)
                    {
                        float  t2   = 0.28f + sj * 0.18f;
                        float  sjx  = topX + (float)(Math.Cos(ang) * hLen * t2);
                        float  sjy  = topY + (float)(Math.Sin(ang) * hLen * t2 * 0.58) - hLen * t2 * 0.12f;
                        float  sjL  = hLen * 0.20f;
                        double sjA  = ang + Math.PI * 0.42 * (sj % 2 == 0 ? 1 : -1);
                        using (var pen = new Pen(CA(cH, 108 + rndP.Next(52)), 1.8f))
                        {
                            pen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
                            g.DrawLine(pen, sjx, sjy,
                                sjx + (float)(Math.Cos(sjA) * sjL),
                                sjy + (float)(Math.Sin(sjA) * sjL * 0.58));
                        }
                    }
                }
            }
        }
    }
}