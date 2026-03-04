using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

namespace OpenCIP
{
    public class GeneradorDibujo
    {
        private ContextoVisual _ctx;
        private Random         _rnd;

        private static readonly Color PAPEL_LAPIZ    = Color.FromArgb(248, 244, 236);
        private static readonly Color PAPEL_LAPICERA = Color.FromArgb(253, 251, 246);
        private static readonly Color PAPEL_CARBON   = Color.FromArgb(238, 233, 222);

        public GeneradorDibujo(ContextoVisual ctx)
        {
            _ctx = ctx;
            _rnd = new Random(ctx.Semilla);
        }

        // ═══════════════════════════════════════════════════════════════════════
        //  ENTRADA PRINCIPAL
        // ═══════════════════════════════════════════════════════════════════════
        public Bitmap Generar(int ancho, int alto, Action<int> progreso)
        {
            var bmp = new Bitmap(ancho, alto);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode      = SmoothingMode.AntiAlias;
                g.TextRenderingHint  = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                g.CompositingQuality = CompositingQuality.HighQuality;

                bool esInk    = _ctx.EstiloLapiz == "lapicera";
                bool esCarbon = _ctx.EstiloLapiz == "carbon";

                Color papel = esInk    ? PAPEL_LAPICERA
                            : esCarbon ? PAPEL_CARBON
                                       : PAPEL_LAPIZ;
                g.Clear(papel);

                progreso?.Invoke(3);
                DibujarTexturaPapel(g, ancho, alto);

                progreso?.Invoke(10);
                DibujarEscena(g, ancho, alto);

                progreso?.Invoke(88);
                if (esInk) DibujarVignetteInk(g, ancho, alto);
                DibujarGranoFinal(g, ancho, alto);

                progreso?.Invoke(100);
            }
            return bmp;
        }

        // ═══════════════════════════════════════════════════════════════════════
        //  TEXTURA DE PAPEL
        // ═══════════════════════════════════════════════════════════════════════
        private void DibujarTexturaPapel(Graphics g, int w, int h)
        {
            bool esInk = _ctx.EstiloLapiz == "lapicera";
            if (esInk)
            {
                // Papel fino: fibras horizontales muy sutiles
                for (int i = 0; i < w * 2; i++)
                {
                    int x = _rnd.Next(w), y = _rnd.Next(h);
                    int len = _rnd.Next(5, 28), a = _rnd.Next(3, 9);
                    using (var p = new Pen(Color.FromArgb(a, 125, 115, 105), 0.4f))
                        g.DrawLine(p, x, y, Math.Min(w - 1, x + len), y + _rnd.Next(-1, 2));
                }
            }
            else
            {
                // Grano tipo papel de boceto
                int granos = (int)(w * h * 0.003f);
                for (int i = 0; i < granos; i++)
                {
                    int x = _rnd.Next(w), y = _rnd.Next(h);
                    int sz = _rnd.Next(1, 3), a = _rnd.Next(6, 22);
                    using (var br = new SolidBrush(Color.FromArgb(a, 148, 138, 118)))
                        g.FillRectangle(br, x, y, sz, sz);
                }
                // Fibras largas de papel
                for (int i = 0; i < w; i++)
                {
                    int x = _rnd.Next(w), y = _rnd.Next(h);
                    int len = _rnd.Next(10, 45), a = _rnd.Next(4, 13);
                    using (var p = new Pen(Color.FromArgb(a, 158, 148, 128), 0.45f))
                        g.DrawLine(p, x, y, Math.Min(w - 1, x + len), y + _rnd.Next(-1, 2));
                }
            }
        }

        // ═══════════════════════════════════════════════════════════════════════
        //  ESCENA PRINCIPAL
        // ═══════════════════════════════════════════════════════════════════════
        private void DibujarEscena(Graphics g, int w, int h)
        {
            var pal = _ctx.PalabrasDetectadas;

            bool esMontana  = Contiene(pal, "montana","montaña","mountain","sierra","cumbre","pico","nevado","alps","andes");
            bool esOceano   = Contiene(pal, "oceano","ocean","mar","sea","playa","beach","lago","lake","rio","river");
            bool esBosque   = Contiene(pal, "bosque","forest","selva","jungle","arbol","tree","arboles");
            bool esCiudad   = Contiene(pal, "ciudad","city","edificio","building","arquitectura","skyline","urbano");
            bool esDesierto = Contiene(pal, "desierto","desert","arena","dunas","cactus");
            bool esFlores   = Contiene(pal, "flores","flowers","campo","meadow","prado","jardin","garden");
            bool esNoche    = Contiene(pal, "noche","night","nocturno","luna","moon") || _ctx.HoraDelDia == "noche";
            bool esAtardecer= Contiene(pal, "atardecer","sunset","amanecer","sunrise","ocaso");

            // Posición del horizonte según escena (regla de tercios)
            float hzRatio = esCiudad ? 0.60f : esOceano ? 0.50f : 0.555f;
            int hz = (int)(h * hzRatio);

            // ── Cielo (fondo, siempre primero) ──
            DibujarCielo(g, w, h, hz, esNoche, esAtardecer);

            // ── Sol / Luna ──
            if (!_ctx.SinSol && !esNoche)
                DibujarSol(g, w, hz, esAtardecer);
            else if (esNoche)
                DibujarLuna(g, w, hz);

            // ── Terreno ──
            if (esMontana)  DibujarMontanas(g, w, h, hz);
            if (esOceano)   DibujarAgua(g, w, h, hz);
            if (esBosque)   DibujarBosque(g, w, h, hz);
            if (esCiudad)   DibujarCiudad(g, w, h, hz);
            if (esDesierto) DibujarDesierto(g, w, h, hz);
            if (esFlores)   DibujarFlores(g, w, h, hz);

            // Paisaje por defecto: montañas + bosque
            if (!esMontana && !esOceano && !esBosque && !esCiudad && !esDesierto && !esFlores)
            {
                DibujarMontanas(g, w, h, hz);
                DibujarBosque(g, w, h, hz);
            }

            // ── Extras opcionales ──
            if (_ctx.ConIslas)    DibujarIslas(g, w, h, hz);
            if (_ctx.ConRocas)    DibujarRocas(g, w, h, hz);
            if (_ctx.ConPalmeras) DibujarPalmeras(g, w, h, hz);
            if (_ctx.ConLluvia)   DibujarLluvia(g, w, h);
            if (_ctx.ConNieve)    DibujarNieve(g, w, h);

            // ── Línea de horizonte (encima de todo el terreno) ──
            DibujarLineaHorizonte(g, w, h, hz);

            // ── Textura de suelo en primer plano ──
            DibujarSuelo(g, w, h, hz);
        }

        // ═══════════════════════════════════════════════════════════════════════
        //  CIELO
        // ═══════════════════════════════════════════════════════════════════════
        private void DibujarCielo(Graphics g, int w, int h, int hz, bool esNoche, bool esAtardecer)
        {
            if (esNoche)
                DibujarEstrellas(g, w, hz);
            else
                DibujarNubes(g, w, hz, esAtardecer);
        }

        // ─── Nubes con volumen ───────────────────────────────────────────────
        private void DibujarNubes(Graphics g, int w, int hz, bool esAtardecer)
        {
            bool ink = _ctx.EstiloLapiz == "lapicera";
            int nNubes = 2 + _rnd.Next(4);
            for (int ni = 0; ni < nNubes; ni++)
            {
                float cx = (float)(w * (0.05f + _rnd.NextDouble() * 0.85f));
                float cy = (float)(hz * (0.06f + _rnd.NextDouble() * 0.44f));
                float nW = 65 + (float)(_rnd.NextDouble() * 150);
                float nH = nW * (0.22f + (float)_rnd.NextDouble() * 0.20f);
                DibujarNube(g, cx, cy, nW, nH, ink, esAtardecer);
            }
        }

        private void DibujarNube(Graphics g, float cx, float cy, float nw, float nh,
                                  bool ink, bool esAtardecer)
        {
            float alfaContorno = esAtardecer ? 0.60f : 0.48f;
            Color tinta = ColorTinta(alfaContorno);

            // Contorno superior: serie de bultos irregulares
            int nBultos = 3 + _rnd.Next(3);
            var ptsTop = new List<PointF>();

            ptsTop.Add(new PointF(cx - nw * 0.5f, cy + nh * 0.25f));
            for (int b = 0; b <= nBultos; b++)
            {
                float t = (float)b / nBultos;
                float bx = cx - nw * 0.5f + t * nw;
                float amp = nh * (0.5f + (float)(_rnd.NextDouble() * 0.5f));
                float vx = (float)(_rnd.NextDouble() * nw * 0.10f - nw * 0.05f);
                ptsTop.Add(new PointF(bx + vx, cy - amp + (float)(_rnd.NextDouble() * nh * 0.18f)));
            }
            ptsTop.Add(new PointF(cx + nw * 0.5f, cy + nh * 0.25f));

            // Dibujar contorno superior como Bezier suave
            using (var p = new Pen(tinta, ink ? 1.0f : Grosor(0.75f)))
            {
                for (int i = 0; i < ptsTop.Count - 1; i++)
                {
                    if (ink) LineInk(g, p, ptsTop[i], ptsTop[i + 1]);
                    else     LineTremor(g, p, ptsTop[i], ptsTop[i + 1]);
                }
                // Base plana de la nube
                if (ink) LineInk(g, p, ptsTop[ptsTop.Count - 1], ptsTop[0]);
                else     LineTremor(g, p, ptsTop[ptsTop.Count - 1], ptsTop[0]);
            }

            // Sombreado interior inferior (gradiente de hatching)
            float shadY0 = cy + nh * 0.02f;
            int nShadLines = (int)(nh * 0.45f / (ink ? 4 : 6));
            for (int sl = 0; sl < nShadLines; sl++)
            {
                float t = (float)sl / Math.Max(1, nShadLines - 1);
                float y = shadY0 + sl * (ink ? 4 : 6);
                float ancho = nw * (0.55f + t * 0.35f);
                float x0 = cx - ancho * 0.5f + (float)(_rnd.NextDouble() * nw * 0.04f);
                float alfa = 0.10f + t * 0.09f;
                using (var p = new Pen(ColorTinta(alfa), ink ? 0.45f : 0.38f))
                    g.DrawLine(p, x0, y, x0 + ancho, y + (float)(_rnd.NextDouble() * 1.2f));
            }
        }

        // ─── Estrellas ───────────────────────────────────────────────────────
        private void DibujarEstrellas(Graphics g, int w, int hz)
        {
            int n = w / 18 + _rnd.Next(18);
            for (int i = 0; i < n; i++)
            {
                float sx = _rnd.Next(w);
                float sy = (float)(_rnd.NextDouble() * hz * 0.90f);
                float sz = 1.0f + (float)_rnd.NextDouble() * 2.8f;
                float intens = 0.28f + (float)_rnd.NextDouble() * 0.52f;
                using (var p = new Pen(ColorTinta(intens), sz * 0.45f))
                {
                    g.DrawLine(p, sx - sz, sy, sx + sz, sy);
                    g.DrawLine(p, sx, sy - sz, sx, sy + sz);
                    if (_rnd.NextDouble() > 0.55f)
                    {
                        float s2 = sz * 0.68f;
                        g.DrawLine(p, sx - s2, sy - s2, sx + s2, sy + s2);
                        g.DrawLine(p, sx + s2, sy - s2, sx - s2, sy + s2);
                    }
                }
            }
        }

        // ═══════════════════════════════════════════════════════════════════════
        //  HORIZONTE
        // ═══════════════════════════════════════════════════════════════════════
        private void DibujarLineaHorizonte(Graphics g, int w, int h, int hz)
        {
            bool ink = _ctx.EstiloLapiz == "lapicera";
            Color tinta = ColorTinta(0.68f);

            // Línea principal
            using (var p = new Pen(tinta, ink ? 1.55f : Grosor(1.0f)))
            {
                PointF prev = new PointF(0, hz + (float)(Matematica.Perlin(0, 0.5) * 4));
                int paso = ink ? 3 : 5;
                for (int x = paso; x <= w; x += paso)
                {
                    float dy = (float)(Matematica.Perlin(x * 0.0022, 0.5) * 5);
                    var next = new PointF(x, hz + dy);
                    if (ink) LineInk(g, p, prev, next);
                    else     LineTremor(g, p, prev, next);
                    prev = next;
                }
            }

            // Segunda línea paralela muy sutil
            using (var p = new Pen(ColorTinta(0.26f), ink ? 0.55f : Grosor(0.38f)))
            {
                int paso2 = ink ? 9 : 14;
                for (int x = 0; x < w - paso2; x += paso2)
                {
                    float dy1 = (float)(Matematica.Perlin(x * 0.0022, 0.5) * 5) + 2.2f;
                    float dy2 = (float)(Matematica.Perlin((x + paso2) * 0.0022, 0.5) * 5) + 2.2f;
                    var a = new PointF(x, hz + dy1);
                    var b = new PointF(x + paso2, hz + dy2);
                    if (ink) LineInk(g, p, a, b);
                    else g.DrawLine(p, a, b);
                }
            }
        }

        // ═══════════════════════════════════════════════════════════════════════
        //  SOL
        // ═══════════════════════════════════════════════════════════════════════
        private void DibujarSol(Graphics g, int w, int hz, bool esAtardecer)
        {
            bool ink = _ctx.EstiloLapiz == "lapicera";
            // Regla de tercios: sol en tercio derecho superior
            float posXRatio = esAtardecer
                ? 0.82f + (float)_rnd.NextDouble() * 0.12f
                : 0.66f + (float)_rnd.NextDouble() * 0.20f;
            int sx = (int)(w * posXRatio);
            int sy = esAtardecer
                ? (int)(hz * 0.78f + _rnd.NextDouble() * hz * 0.15f)
                : (int)(hz * (0.09f + _rnd.NextDouble() * 0.18f));
            int sr = (int)(w * (esAtardecer ? 0.052f : 0.040f));

            // Halos concéntricos
            if (!esAtardecer)
            {
                for (int ring = 4; ring >= 1; ring--)
                {
                    int rr = sr + ring * (ink ? 10 : 13);
                    float alfaHalo = 0.04f + ring * 0.025f;
                    using (var p = new Pen(ColorTinta(alfaHalo), ink ? 0.45f : Grosor(0.35f)))
                        g.DrawEllipse(p, sx - rr, sy - rr, rr * 2, rr * 2);
                }
            }

            // Círculo principal
            Color tintaSol = ColorTinta(ink ? 0.58f : 0.62f);
            using (var p = new Pen(tintaSol, ink ? 1.9f : Grosor(1.15f)))
                g.DrawEllipse(p, sx - sr, sy - sr, sr * 2, sr * 2);

            // Hatching interno del sol (círculo lleno con líneas)
            if (ink)
            {
                using (var p = new Pen(ColorTinta(0.16f), 0.48f))
                {
                    for (int yi = sy - sr + 3; yi < sy + sr - 2; yi += 4)
                    {
                        float hc = (float)Math.Sqrt(Math.Max(0.0, (double)sr * sr - (double)(yi - sy) * (yi - sy)));
                        if (hc < 1f) continue;
                        float jx = (float)(_rnd.NextDouble() - 0.5f);
                        g.DrawLine(p, sx - hc + jx, yi, sx + hc + jx, yi);
                    }
                }
            }

            // Rayos con variación de longitud y grosor
            int nRayos = ink ? 18 : 14;
            for (int i = 0; i < nRayos; i++)
            {
                double ang = i * Math.PI * 2 / nRayos;
                bool principal = i % 2 == 0;
                float rLen = sr + (principal ? (ink ? 24 : 20) : (ink ? 12 : 10)) + _rnd.Next(5);
                float r1 = sr + 4;
                float grosorR = principal ? (ink ? 1.35f : Grosor(1.0f)) : (ink ? 0.68f : Grosor(0.5f));
                float alfaR = principal ? 0.68f : 0.42f;

                var pa = new PointF(sx + (float)(Math.Cos(ang) * r1), sy + (float)(Math.Sin(ang) * r1));
                var pb = new PointF(sx + (float)(Math.Cos(ang) * rLen), sy + (float)(Math.Sin(ang) * rLen));
                using (var p = new Pen(ColorTinta(alfaR), grosorR))
                {
                    if (ink) LineInk(g, p, pa, pb);
                    else LineTremor(g, p, pa, pb);
                }
            }
        }

        // ═══════════════════════════════════════════════════════════════════════
        //  LUNA
        // ═══════════════════════════════════════════════════════════════════════
        private void DibujarLuna(Graphics g, int w, int hz)
        {
            bool ink = _ctx.EstiloLapiz == "lapicera";
            int mx = (int)(w * (0.62f + _rnd.NextDouble() * 0.22f));
            int my = (int)(hz * (0.07f + _rnd.NextDouble() * 0.18f));
            int mr = (int)(w * 0.038f);
            Color tinta = ColorTinta(0.68f);

            // Halo
            using (var p = new Pen(ColorTinta(0.07f), ink ? 0.5f : 0.4f))
                g.DrawEllipse(p, mx - mr - 9, my - mr - 9, (mr + 9) * 2, (mr + 9) * 2);
            using (var p = new Pen(ColorTinta(0.04f), ink ? 0.4f : 0.35f))
                g.DrawEllipse(p, mx - mr - 18, my - mr - 18, (mr + 18) * 2, (mr + 18) * 2);

            // Círculo luna
            using (var p = new Pen(tinta, ink ? 1.65f : Grosor(1.1f)))
                g.DrawEllipse(p, mx - mr, my - mr, mr * 2, mr * 2);

            // Sombra interior (fase creciente)
            int paso = ink ? 3 : 5;
            for (int yi = my - mr + 2; yi < my + mr; yi += paso)
            {
                float hc = (float)Math.Sqrt(Math.Max(0.0, (double)mr * mr - (double)(yi - my) * (yi - my)));
                if (hc < 1f) continue;
                float shadEnd = mx - hc * 0.32f;
                float shadStart = mx - hc;
                if (shadEnd <= shadStart) continue;
                using (var p = new Pen(ColorTinta(0.28f), ink ? 0.48f : Grosor(0.38f)))
                    g.DrawLine(p, shadStart, yi, shadEnd, yi);
            }
        }

        // ═══════════════════════════════════════════════════════════════════════
        //  MONTAÑAS  (perspectiva atmosférica real)
        // ═══════════════════════════════════════════════════════════════════════
        private void DibujarMontanas(Graphics g, int w, int h, int hz)
        {
            bool ink = _ctx.EstiloLapiz == "lapicera";
            int nPlanos = 3;

            for (int pl = 0; pl < nPlanos; pl++)
            {
                // 0 = plano más lejano (claro, fino), 2 = más cercano (oscuro, grueso)
                float prof = (float)pl / (nPlanos - 1);
                float alfaLinea   = ink ? (0.22f + prof * 0.50f) : (0.18f + prof * 0.42f);
                float grosorLinea = ink ? (0.65f + prof * 1.1f)  : Grosor(0.55f + prof * 0.65f);
                float altMaxRatio = 0.65f - pl * 0.12f;
                float baseY       = hz - pl * 6;
                Color tinta       = ColorTinta(alfaLinea);

                double seed = _ctx.Semilla * 0.001 + pl * 2.7;
                var pts = GenerarPerfilMontana(w, h, hz, baseY, altMaxRatio, seed, ink);

                // Perfil de la montaña
                using (var p = new Pen(tinta, grosorLinea))
                    DibujarPerfil(g, p, pts, ink);

                // Nieve en picos altos
                if (_ctx.ConNieve || pl == nPlanos - 1)
                    DibujarNevado(g, pts, hz, ink);

                // Hatching de ladera (solo planos con detalle)
                if (pl >= 1)
                    DibujarHatchingLadera(g, pts, hz, h, tinta, ink, pl);
            }
        }

        private List<PointF> GenerarPerfilMontana(int w, int h, int hz, float baseY,
                                                   float altMaxRatio, double seed, bool ink)
        {
            var pts = new List<PointF>();
            pts.Add(new PointF(-5, h));

            int paso = ink ? 2 : 3;
            for (int x = 0; x <= w; x += paso)
            {
                double nx = (double)x / w;
                // FBM de baja frecuencia para silueta base + alta para picos definidos
                double fbmBase = Matematica.FBM(nx * 2.8 + seed,       seed * 0.35, 6, 0.50);
                double fbmPico = Matematica.FBM(nx * 9.0 + seed * 1.9, seed * 0.85, 3, 0.62) * 0.20;
                double combined = (fbmBase + fbmPico + 1.0) * 0.5;
                // Máscara en seno para que la montaña no sea abrupta en extremos
                double mascara = Math.Sin(nx * Math.PI) * 0.65 + 0.35;
                float altMax = hz * altMaxRatio;
                float py = baseY - (float)(combined * mascara * altMax);
                py += (float)(_rnd.NextDouble() * (ink ? 0.8 : 1.8) - (ink ? 0.4 : 0.9));
                pts.Add(new PointF(x, Math.Max(7, py)));
            }

            pts.Add(new PointF(w + 5, h));
            return pts;
        }

        private void DibujarNevado(Graphics g, List<PointF> pts, int hz, bool ink)
        {
            float snowLine = hz * 0.38f;
            using (var p = new Pen(ColorTinta(ink ? 0.22f : 0.18f), ink ? 0.95f : Grosor(0.55f)))
            {
                for (int i = 1; i < pts.Count - 2; i++)
                {
                    if (pts[i].Y > snowLine) continue;
                    float sw = 10 + (snowLine - pts[i].Y) * 0.28f;
                    float jy = (float)(_rnd.NextDouble() * 3.5f);
                    float lineY = pts[i].Y + (snowLine - pts[i].Y) * 0.12f + jy;
                    var a = new PointF(pts[i].X - sw * 0.45f, lineY);
                    var b = new PointF(pts[i].X + sw * 0.45f, lineY);
                    if (ink) LineInk(g, p, a, b);
                    else g.DrawLine(p, a, b);
                }
            }
        }

        private void DibujarHatchingLadera(Graphics g, List<PointF> pts, int hz, int h,
                                            Color tintaBase, bool ink, int plano)
        {
            float alfa = ink ? (0.24f + plano * 0.11f) : (0.14f + plano * 0.09f);
            int pasoH = Math.Max(4, ink ? (9 - plano) : (14 - plano * 2));

            using (var ph = new Pen(ColorTinta(alfa), ink ? 0.50f : Grosor(0.38f)))
            {
                for (int i = 1; i < pts.Count - 2; i += 2)
                {
                    float px = pts[i].X;
                    float pyTop = pts[i].Y;
                    if (pyTop >= hz) continue;

                    // Pendiente local para orientar el hatching
                    float slope = (i + 2 < pts.Count - 1)
                        ? (pts[i + 2].Y - pts[i].Y) / Math.Max(1f, pts[i + 2].X - pts[i].X)
                        : 0f;
                    float dirX = slope > 0.1f ? 0.55f : slope < -0.1f ? -0.55f : 0f;

                    for (float y = pyTop + pasoH; y < hz; y += pasoH)
                    {
                        float prog = (y - pyTop) / (hz - pyTop);
                        float len = prog * 14f * (ink ? 1.0f : 0.65f);
                        float jx = (float)(_rnd.NextDouble() * 0.7f - 0.35f);
                        float endY = y + len * 0.45f;
                        if (endY > hz) { float t = (hz - y) / (endY - y); len *= t; endY = hz; }
                        var a = new PointF(px + jx, y);
                        var b = new PointF(px + len * dirX + jx, endY);
                        g.DrawLine(ph, a, b);
                    }
                }
            }

            // Crosshatch adicional en plano más cercano
            if (plano >= 2 && ink)
            {
                using (var ph2 = new Pen(ColorTinta(alfa * 0.65f), 0.42f))
                {
                    for (int i = 1; i < pts.Count - 2; i += 3)
                    {
                        float px = pts[i].X;
                        float pyTop = pts[i].Y;
                        if (pyTop >= hz) continue;
                        for (float y = pyTop + pasoH + 4; y < hz; y += pasoH + 4)
                        {
                            float prog = (y - pyTop) / (hz - pyTop);
                            float len = prog * 11f;
                            float jx = (float)(_rnd.NextDouble() * 0.7f - 0.35f);
                            float endY = y + len * 0.32f;
                            if (endY > hz) endY = hz;
                            g.DrawLine(ph2, px + jx, y, px - len + jx, endY);
                        }
                    }
                }
            }
        }

        // ═══════════════════════════════════════════════════════════════════════
        //  AGUA / OCÉANO
        // ═══════════════════════════════════════════════════════════════════════
        private void DibujarAgua(Graphics g, int w, int h, int hz)
        {
            bool ink = _ctx.EstiloLapiz == "lapicera";
            int zonaAgua = h - hz;
            int nLineas = zonaAgua / (ink ? 9 : 13);

            for (int i = 0; i < nLineas; i++)
            {
                float prof = (float)i / Math.Max(1, nLineas - 1);
                float yBase = hz + i * (ink ? 9 : 13) + (float)(_rnd.NextDouble() * 3);
                float grosorOnda = ink ? (0.48f + prof * 1.25f) : Grosor(0.48f + i * 0.05f);
                float alfaOnda = 0.18f + prof * 0.48f;

                float largo = w * (ink ? (0.10f + (float)_rnd.NextDouble() * 0.62f)
                                       : (0.22f + (float)_rnd.NextDouble() * 0.56f));
                float x0 = (float)(_rnd.NextDouble() * (w - largo));
                float curva = (float)(_rnd.NextDouble() * 5.5f - 2.75f);

                using (var p = new Pen(ColorTinta(alfaOnda), grosorOnda))
                using (var path = new GraphicsPath())
                {
                    if (ink)
                    {
                        float m1x = x0 + largo * 0.30f, m2x = x0 + largo * 0.70f;
                        float jy1 = (float)(_rnd.NextDouble() * 2.5f - 1.25f);
                        float jy2 = (float)(_rnd.NextDouble() * 2.5f - 1.25f);
                        path.AddBezier(x0, yBase, m1x, yBase + curva + jy1,
                                       m2x, yBase - curva + jy2, x0 + largo, yBase);
                    }
                    else
                    {
                        path.AddBezier(x0, yBase, x0 + largo * 0.33f, yBase + curva,
                                       x0 + largo * 0.66f, yBase - curva, x0 + largo, yBase);
                    }
                    g.DrawPath(p, path);
                }

                // Olas secundarias cortas
                if (i % 2 == 0 && largo > 45)
                {
                    int ns = 1 + _rnd.Next(3);
                    for (int s = 0; s < ns; s++)
                    {
                        float x2 = x0 + (float)(_rnd.NextDouble() * largo);
                        float l2 = largo * (0.04f + (float)_rnd.NextDouble() * 0.10f);
                        if (x2 + l2 > w) continue;
                        float yOfs = (float)(_rnd.NextDouble() * 4 - 2);
                        using (var p2 = new Pen(ColorTinta(alfaOnda * 0.52f), ink ? 0.42f : Grosor(0.32f)))
                            g.DrawLine(p2, x2, yBase + yOfs, x2 + l2, yBase + yOfs);
                    }
                }
            }

            // Destellos de luz en el agua
            int nDest = w / 38 + _rnd.Next(9);
            for (int d = 0; d < nDest; d++)
            {
                float dx = (float)(_rnd.NextDouble() * w);
                float dy = hz + (float)(_rnd.NextDouble() * zonaAgua * 0.62f);
                float dl = 5 + (float)(_rnd.NextDouble() * 14);
                using (var p = new Pen(ColorTinta(0.17f), ink ? 0.48f : Grosor(0.38f)))
                    g.DrawLine(p, dx, dy, dx + dl, dy + dl * 0.28f);
            }
        }

        // ═══════════════════════════════════════════════════════════════════════
        //  BOSQUE  (3 planos + árboles individuales)
        // ═══════════════════════════════════════════════════════════════════════
        private void DibujarBosque(Graphics g, int w, int h, int hz)
        {
            bool ink = _ctx.EstiloLapiz == "lapicera";

            for (int pl = 0; pl < 3; pl++)
            {
                float prof = (float)pl / 2;
                float yBase = hz + pl * (int)((h - hz) * 0.09f);
                float altMax = (h - hz) * (0.30f - pl * 0.06f);
                float alfaLinea = 0.18f + prof * 0.22f;
                Color tinta = ColorTinta(alfaLinea);

                double seed = _ctx.Semilla * 0.004 + pl * 4.8;
                double escala = 6.5 - pl * 1.5;
                int nPts = w / (ink ? 2 : 3);
                var pts = new List<PointF>();
                pts.Add(new PointF(-5, h));

                for (int xi = 0; xi <= nPts; xi++)
                {
                    double nx = (double)xi / nPts;
                    double n1 = Matematica.FBM(nx * escala + seed,         1.0 + pl, 5, 0.60);
                    double n2 = Matematica.FBM(nx * escala * 4 + seed + 8, 0.3,      3, 0.55) * 0.24;
                    double alt = Math.Max(0, (n1 + n2 + 1.0) * 0.5);
                    float py = yBase - (float)(altMax * alt);
                    float jy = (float)(_rnd.NextDouble() * (ink ? 1.3 : 2.8) - (ink ? 0.65 : 1.4));
                    pts.Add(new PointF((float)(nx * w), Math.Max(6, py + jy)));
                }
                pts.Add(new PointF(w + 5, h));

                using (var p = new Pen(tinta, Grosor(ink ? 0.82f : 0.68f)))
                    DibujarPerfil(g, p, pts, ink);

                if (pl >= 1)
                    DibujarHatchingBosque(g, pts, hz, h, ink, pl);
            }

            // Árboles individuales en primer plano (ordenados por profundidad)
            int nArboles = w / 48 + _rnd.Next(7);
            var lista = new List<(float x, float y)>();
            for (int i = 0; i < nArboles; i++)
                lista.Add((_rnd.Next(w), hz + (float)(_rnd.NextDouble() * (h - hz) * 0.52f)));
            lista.Sort((a, b) => a.y.CompareTo(b.y));

            foreach (var (ax, ay) in lista)
            {
                float pct = (ay - hz) / (h - hz);
                float alt  = (55 + pct * 85) * (0.72f + (float)_rnd.NextDouble() * 0.56f);
                float anch = alt * (0.36f + (float)_rnd.NextDouble() * 0.34f);
                if (_rnd.NextDouble() < 0.42f)
                    DibujarConifera(g, ax, ay, alt, anch * 0.52f);
                else
                    DibujarArbol(g, ax, ay, alt, anch);
            }
        }

        private void DibujarHatchingBosque(Graphics g, List<PointF> pts, int hz, int h,
                                            bool ink, int plano)
        {
            Color tintaH = ColorTinta(0.11f + plano * 0.055f);
            int pasoH = ink ? 7 : 11;
            using (var ph = new Pen(tintaH, ink ? 0.42f : Grosor(0.32f)))
            {
                for (int xi = 1; xi < pts.Count - 2; xi += 2)
                {
                    float px = pts[xi].X;
                    float pyTop = pts[xi].Y;
                    float maxY = Math.Min(hz + (h - hz) * 0.42f, h - 5);
                    for (float y = pyTop + 3; y < maxY; y += pasoH)
                    {
                        float jx = (float)(_rnd.NextDouble() * 1.2f - 0.6f);
                        float len = pasoH * (ink ? 0.65f : 0.55f);
                        float yy = y + (float)(_rnd.NextDouble() * 1.8f);
                        if (ink)
                            LineInk(g, ph, new PointF(px + jx, yy), new PointF(px + len + jx, yy + len * 0.42f));
                        else
                            g.DrawLine(ph, px + jx, yy, px + len + jx, yy);
                    }
                }
            }
        }

        // ─── Árbol con copa decidua ───────────────────────────────────────────
        private void DibujarArbol(Graphics g, float x, float baseY, float altura, float ancho)
        {
            bool ink = _ctx.EstiloLapiz == "lapicera";
            Color tinta = ColorTinta(0.72f);
            float troncoH = altura * 0.22f;
            float troncoW = Math.Max(3.5f, ancho * 0.095f);

            // Tronco
            using (var p = new Pen(tinta, ink ? 1.55f : Grosor(1.2f)))
            {
                float tipX = x + (float)(_rnd.NextDouble() * 3.5f - 1.75f);
                if (ink)
                {
                    LineInk(g, p, new PointF(x - troncoW * 0.5f, baseY),
                                  new PointF(tipX - troncoW * 0.18f, baseY - troncoH));
                    LineInk(g, p, new PointF(x + troncoW * 0.5f, baseY),
                                  new PointF(tipX + troncoW * 0.18f, baseY - troncoH));
                    // Marcas de corteza
                    int nCortes = (int)(troncoH / 8);
                    using (var pC = new Pen(ColorTinta(0.42f), 0.48f))
                        for (int ci = 1; ci < nCortes; ci++)
                        {
                            float cy = baseY - ci * 8;
                            float hh = troncoW * (0.48f - ci * 0.015f);
                            g.DrawLine(pC, x - hh + (float)(_rnd.NextDouble() * 1.5f), cy,
                                           x + hh - (float)(_rnd.NextDouble() * 1.5f),
                                           cy + (float)(_rnd.NextDouble() * 1.8f - 0.9f));
                        }
                }
                else
                {
                    LineTremor(g, p, new PointF(x - troncoW * 0.5f, baseY), new PointF(x, baseY - troncoH));
                    LineTremor(g, p, new PointF(x + troncoW * 0.5f, baseY), new PointF(x, baseY - troncoH));
                }
            }

            // Copa: deciduo (redonda) o conífera pequeña
            if (_rnd.NextDouble() > 0.38f)
                DibujarCopaRedonda(g, x, baseY - troncoH, altura - troncoH, ancho, tinta, ink);
            else
                DibujarCopaConos(g, x, baseY - troncoH, altura - troncoH, ancho, tinta, ink);
        }

        private void DibujarCopaRedonda(Graphics g, float x, float topY, float altCopa,
                                         float ancho, Color tinta, bool ink)
        {
            float copaY  = topY - altCopa * 0.43f;
            float rX     = ancho * 0.50f;
            float rY     = altCopa * 0.50f;
            int nSegs    = ink ? 28 : 20;

            var cPts = new List<PointF>();
            for (int ci = 0; ci < nSegs; ci++)
            {
                double ang = ci * Math.PI * 2 / nSegs;
                float jitter = ink ? (float)(_rnd.NextDouble() * 4.5f - 2.25f)
                                   : (float)(_rnd.NextDouble() * 7.5f - 3.75f);
                cPts.Add(new PointF(x + (float)(Math.Cos(ang) * rX) + jitter,
                                    copaY + (float)(Math.Sin(ang) * rY) + jitter * 0.45f));
            }

            using (var p = new Pen(tinta, ink ? 1.2f : Grosor(1.0f)))
                for (int ci = 0; ci < cPts.Count; ci++)
                {
                    var a = cPts[ci];
                    var b = cPts[(ci + 1) % cPts.Count];
                    if (ink) LineInk(g, p, a, b);
                    else g.DrawLine(p, a, b);
                }

            // Ramas visibles internas
            if (ink)
            {
                int nR = 3 + _rnd.Next(3);
                using (var pR = new Pen(ColorTinta(0.58f), 0.68f))
                    for (int r = 0; r < nR; r++)
                    {
                        double ang = _rnd.NextDouble() * Math.PI * 2;
                        var ramaEnd = new PointF(x + (float)(Math.Cos(ang) * rX * 0.62f),
                                                 copaY + (float)(Math.Sin(ang) * rY * 0.62f));
                        LineInk(g, pR, new PointF(x, copaY + rY * 0.28f), ramaEnd);
                    }
            }

            CrossHatch(g, x - rX, copaY - rY, rX * 2, rY * 2, ink ? 0.58f : 0.32f, 0.42f, ink);
        }

        private void DibujarCopaConos(Graphics g, float x, float topY, float altCopa,
                                       float ancho, Color tinta, bool ink)
        {
            int nN = 3 + _rnd.Next(2);
            float nivelH = altCopa / nN;
            using (var p = new Pen(tinta, ink ? 1.2f : Grosor(0.95f)))
            {
                for (int n = 0; n < nN; n++)
                {
                    float ty = topY - n * nivelH * 0.68f;
                    float tw = ancho * (1.0f - n * 0.28f) * 0.5f;
                    float tipY = ty - nivelH * 1.05f;
                    float jx1 = ink ? (float)(_rnd.NextDouble() * 2 - 1)   : (float)(_rnd.NextDouble() * 4 - 2);
                    float jx2 = ink ? (float)(_rnd.NextDouble() * 2 - 1)   : (float)(_rnd.NextDouble() * 4 - 2);
                    var izq = new PointF(x - tw + jx1, ty);
                    var der = new PointF(x + tw + jx2, ty);
                    var tip = new PointF(x + (ink ? (float)(_rnd.NextDouble() - 0.5f) : 0), tipY);
                    if (ink) { LineInk(g, p, izq, tip); LineInk(g, p, der, tip); LineInk(g, p, izq, der); }
                    else     { LineTremor(g, p, izq, tip); LineTremor(g, p, der, tip); LineTremor(g, p, izq, der); }
                    if (n > 0 && ink)
                        CrossHatch(g, x - tw, tipY, tw * 2, ty - tipY, 0.52f, 0.42f, false);
                }
            }
        }

        // ─── Conífera individual ─────────────────────────────────────────────
        private void DibujarConifera(Graphics g, float x, float baseY, float altura, float ancho)
        {
            bool ink = _ctx.EstiloLapiz == "lapicera";
            Color tinta = ColorTinta(0.72f);

            // Tronco corto
            using (var p = new Pen(ColorTinta(0.76f), Grosor(ink ? 0.88f : 0.78f)))
                if (ink) LineInk(g, p, new PointF(x, baseY), new PointF(x, baseY - altura * 0.16f));
                else     LineTremor(g, p, new PointF(x, baseY), new PointF(x, baseY - altura * 0.16f));

            int nN = 4 + _rnd.Next(2);
            for (int ni = 0; ni < nN; ni++)
            {
                float t = (float)ni / nN;
                float nivelY = baseY - altura * (0.14f + t * 0.78f);
                float nAnch  = ancho * (1.1f - t * 0.82f);
                float segH   = altura * 0.24f;

                var cima = new PointF(x + (float)(_rnd.NextDouble() * 1.2f - 0.6f), nivelY - segH);
                var izq  = new PointF(x - nAnch + (float)(_rnd.NextDouble() * 1.8f - 0.9f), nivelY + segH * 0.12f);
                var der  = new PointF(x + nAnch + (float)(_rnd.NextDouble() * 1.8f - 0.9f), nivelY + segH * 0.12f);

                using (var p = new Pen(tinta, Grosor(ink ? 1.0f : 0.85f)))
                {
                    if (ink) { LineInk(g, p, cima, izq); LineInk(g, p, cima, der); }
                    else     { LineTremor(g, p, cima, izq); LineTremor(g, p, cima, der); }

                    using (var pB = new Pen(ColorTinta(ink ? 0.52f : 0.42f), Grosor(0.48f)))
                        if (ink) LineInk(g, pB, izq, der);
                        else     LineTremor(g, pB, izq, der);
                }

                // Sombreado en lado izquierdo
                if (ni > 0)
                {
                    float paso = ink ? 4.5f : 6.5f;
                    using (var ps = new Pen(ColorTinta(0.28f + t * 0.14f), ink ? 0.45f : Grosor(0.35f)))
                        for (float ys = cima.Y + paso; ys < nivelY; ys += paso)
                        {
                            float prog = (ys - cima.Y) / (nivelY - cima.Y);
                            float xs = x - nAnch * prog * 0.78f;
                            g.DrawLine(ps, xs, ys, x + (float)(_rnd.NextDouble() * 1.5f - 0.75f), ys);
                        }
                }
            }
        }

        // ═══════════════════════════════════════════════════════════════════════
        //  CIUDAD  (skyline con perspectiva)
        // ═══════════════════════════════════════════════════════════════════════
        private void DibujarCiudad(Graphics g, int w, int h, int hz)
        {
            bool ink = _ctx.EstiloLapiz == "lapicera";
            int nEdif = w / 68 + _rnd.Next(4);
            Color tinta = ColorTinta(0.72f);

            var edifs = new List<(int bx, int by, int bw, int bh)>();
            for (int i = 0; i < nEdif; i++)
            {
                int bw2 = 28 + _rnd.Next(78);
                int bh2 = 55 + _rnd.Next(230);
                int bx2 = _rnd.Next(w - bw2);
                edifs.Add((bx2, hz - bh2, bw2, bh2));
            }
            edifs.Sort((a, b) => b.bh.CompareTo(a.bh));

            foreach (var (bx, by, bw2, bh2) in edifs)
            {
                float grosorE = ink ? 1.65f : Grosor(1.1f);

                // Silueta del edificio
                using (var p = new Pen(tinta, grosorE))
                {
                    if (ink)
                    {
                        LineInk(g, p, new PointF(bx, by + bh2), new PointF(bx, by));
                        LineInk(g, p, new PointF(bx, by),       new PointF(bx + bw2, by));
                        LineInk(g, p, new PointF(bx + bw2, by), new PointF(bx + bw2, by + bh2));
                    }
                    else g.DrawRectangle(p, bx, by, bw2, bh2);
                }

                // Sombra lateral izquierda
                if (ink)
                {
                    CrossHatch(g, bx, by, bw2 * 0.27f, bh2, 0.62f, 0.52f, false);
                    // Líneas de pisos
                    using (var pL = new Pen(ColorTinta(0.33f), 0.55f))
                    {
                        int nPisos = bh2 / 24;
                        for (int pi = 1; pi < nPisos; pi++)
                        {
                            float pyP = by + pi * 24;
                            LineInk(g, pL, new PointF(bx + 1, pyP), new PointF(bx + bw2 - 1, pyP), true);
                        }
                    }
                }

                // Ventanas
                DibujarVentanas(g, bx, by, bw2, bh2, tinta, ink);
            }
        }

        private void DibujarVentanas(Graphics g, int bx, int by, int bw, int bh,
                                      Color tinta, bool ink)
        {
            int vW = 9, vH = 13, padX = 7, padY = 9;
            int cols = Math.Max(1, (bw - padX * 2) / (vW + 7));
            int rows = Math.Max(1, (bh - padY * 2) / (vH + 9));
            int gX = cols > 1 ? (bw - padX * 2 - cols * vW) / (cols - 1) + vW : vW;
            int gY = rows > 1 ? (bh - padY * 2 - rows * vH) / (rows - 1) + vH : vH;

            for (int row = 0; row < rows; row++)
                for (int col = 0; col < cols; col++)
                {
                    int wx = bx + padX + col * gX;
                    int wy = by + padY + row * gY;
                    if (wx + vW > bx + bw - padX || wy + vH > by + bh - padY) continue;

                    bool encendida = _rnd.NextDouble() > 0.42f;
                    using (var pw = new Pen(ColorTinta(encendida ? 0.62f : 0.38f),
                                            ink ? 0.85f : Grosor(0.58f)))
                    {
                        if (ink)
                            LineInk(g, pw, new PointF(wx, wy), new PointF(wx + vW, wy + vH), true);
                        g.DrawRectangle(pw, wx, wy, vW, vH);
                    }
                    if (encendida && ink)
                        CrossHatch(g, wx + 1, wy + 1, vW - 2, vH - 2, 1.1f, 0.48f, false);
                }
        }

        // ═══════════════════════════════════════════════════════════════════════
        //  DESIERTO
        // ═══════════════════════════════════════════════════════════════════════
        private void DibujarDesierto(Graphics g, int w, int h, int hz)
        {
            bool ink = _ctx.EstiloLapiz == "lapicera";

            // (El sol lo maneja DibujarEscena respetando _ctx.SinSol)

            // Dunas con sombras de barlovento
            int nDunas = 3 + _rnd.Next(3);
            for (int d = 0; d < nDunas; d++)
            {
                double seed = _ctx.Semilla * 0.002 + d * 3.2;
                float dBaseY = hz + d * (int)((h - hz) * 0.19f);
                float altMax = (h - hz) * (0.22f - d * 0.022f);
                float alfa = 0.33f + d * 0.14f;
                Color tinta = ColorTinta(alfa);

                int nPts = w / 2;
                var pts = new List<PointF>();
                pts.Add(new PointF(0, h));
                for (int xi = 0; xi <= nPts; xi++)
                {
                    double nx = (double)xi / nPts;
                    double fbm = Matematica.FBM(nx * 1.4 + seed, seed * 0.5, 4, 0.52);
                    float py = dBaseY - (float)(fbm * 0.5 + 0.5) * altMax;
                    py += (float)(_rnd.NextDouble() * 1.1f - 0.55f);
                    pts.Add(new PointF((float)(nx * w), Math.Max(hz + 3, py)));
                }
                pts.Add(new PointF(w, h));

                using (var p = new Pen(tinta, Grosor(ink ? 1.1f : 0.85f)))
                    DibujarPerfil(g, p, pts, ink);

                // Sombra en vertiente empinada
                if (d < nDunas - 1)
                {
                    using (var ps = new Pen(ColorTinta(alfa * 0.40f), Grosor(0.38f)))
                    {
                        for (int xi = 2; xi < pts.Count - 3; xi++)
                        {
                            float slope = pts[xi + 1].Y - pts[xi - 1].Y;
                            if (slope >= -3) continue;
                            float px = pts[xi].X, pySurf = pts[xi].Y;
                            int nS = Math.Min(8, (int)(Math.Abs(slope) * 0.65f));
                            for (int si = 0; si < nS; si++)
                            {
                                float sy = pySurf + si * 9;
                                if (sy >= h) break;
                                g.DrawLine(ps, px + si * 3.5f, sy,
                                            Math.Min(w - 1, px + si * 3.5f + 20), sy + 5);
                            }
                        }
                    }
                }
            }

            // Cactus
            DibujarCactos(g, w, h, hz, ink);

            // Ondulación del horizonte
            using (var pH = new Pen(ColorTinta(0.54f), Grosor(ink ? 1.3f : 1.0f)))
            {
                for (int x = 0; x < w - 4; x += 4)
                {
                    float dy1 = (float)(Matematica.Perlin(x * 0.005, 0.75) * 4.2f);
                    float dy2 = (float)(Matematica.Perlin((x + 4) * 0.005, 0.75) * 4.2f);
                    if (ink) LineInk(g, pH, new PointF(x, hz + dy1), new PointF(x + 4, hz + dy2));
                    else g.DrawLine(pH, x, hz + dy1, x + 4, hz + dy2);
                }
            }
        }

        private void DibujarCactos(Graphics g, int w, int h, int hz, bool ink)
        {
            int nC = 2 + _rnd.Next(4);
            Color tintaC = ColorTinta(0.68f);
            for (int c = 0; c < nC; c++)
            {
                float cx = (float)(w * (0.06f + _rnd.NextDouble() * 0.88f));
                float cyB = hz + (float)(_rnd.NextDouble() * (h - hz) * 0.52f);
                float alt = 45 + (float)(_rnd.NextDouble() * 68);
                float aw  = alt * 0.16f;

                using (var p = new Pen(tintaC, Grosor(ink ? 1.5f : 1.2f)))
                {
                    // Tronco
                    if (ink) LineInk(g, p, new PointF(cx, cyB), new PointF(cx, cyB - alt));
                    else LineTremor(g, p, new PointF(cx, cyB), new PointF(cx, cyB - alt));

                    // Brazos
                    int nBrazos = 1 + _rnd.Next(3);
                    for (int br = 0; br < nBrazos; br++)
                    {
                        float lado = br % 2 == 0 ? 1 : -1;
                        float brazoY  = cyB - alt * (0.28f + (float)_rnd.NextDouble() * 0.40f);
                        float brazoLen = alt * (0.26f + (float)_rnd.NextDouble() * 0.28f);
                        float brazoEndX = cx + lado * aw * 3.2f;
                        if (ink) { LineInk(g, p, new PointF(cx, brazoY), new PointF(brazoEndX, brazoY));
                                   LineInk(g, p, new PointF(brazoEndX, brazoY), new PointF(brazoEndX, brazoY - brazoLen)); }
                        else { LineTremor(g, p, new PointF(cx, brazoY), new PointF(brazoEndX, brazoY));
                               LineTremor(g, p, new PointF(brazoEndX, brazoY), new PointF(brazoEndX, brazoY - brazoLen)); }
                    }
                    // Costillas
                    using (var pc = new Pen(ColorTinta(0.36f), Grosor(0.42f)))
                        for (float cxi = cx - aw; cxi <= cx + aw; cxi += aw)
                            g.DrawLine(pc, cxi, cyB, cxi, cyB - alt * 0.90f);
                }
            }
        }

        // ═══════════════════════════════════════════════════════════════════════
        //  FLORES / CAMPO
        // ═══════════════════════════════════════════════════════════════════════
        private void DibujarFlores(Graphics g, int w, int h, int hz)
        {
            bool ink = _ctx.EstiloLapiz == "lapicera";

            // Pasto base
            for (int i = 0; i < w * 2; i++)
            {
                float px = (float)(_rnd.NextDouble() * w);
                float py = hz + (float)(_rnd.NextDouble() * (h - hz));
                float lp = 5 + (float)(_rnd.NextDouble() * 13);
                float inc = (float)(_rnd.NextDouble() * 9 - 4.5f);
                using (var p = new Pen(ColorTinta(0.16f + (float)_rnd.NextDouble() * 0.18f), Grosor(0.32f)))
                    g.DrawLine(p, px, py, px + inc, py - lp);
            }

            // Flores con bezier + pétalos detallados
            int nFlores = w / 15 + _rnd.Next(20);
            var flores = new List<(float x, float y)>();
            for (int i = 0; i < nFlores; i++)
                flores.Add(((float)(_rnd.NextDouble() * w), hz + 8 + (float)(_rnd.NextDouble() * (h - hz - 8))));
            flores.Sort((a, b) => a.y.CompareTo(b.y));

            foreach (var (fx, fy) in flores)
            {
                float pct = (fy - hz) / (h - hz);
                float talloH = (16 + pct * 62) * (0.62f + (float)_rnd.NextDouble() * 0.76f);
                float pr     = (5 + pct * 17) * (0.52f + (float)_rnd.NextDouble() * 0.96f);
                Color tintaF = ColorTinta(0.46f + pct * 0.34f);
                float topX = fx + (float)(_rnd.NextDouble() * 14 - 7);
                float topY = fy - talloH;

                // Tallo bezier
                using (var p = new Pen(ColorTinta(0.33f + pct * 0.18f), Grosor(0.52f + pct * 0.48f)))
                using (var path = new GraphicsPath())
                {
                    float mx = fx + (topX - fx) * 0.5f + (float)(_rnd.NextDouble() * 11 - 5.5f);
                    path.AddBezier(fx, fy, mx, fy - talloH * 0.36f, topX, topY + talloH * 0.26f, topX, topY);
                    g.DrawPath(p, path);
                }

                // Hoja
                if (pct > 0.12f && _rnd.NextDouble() > 0.33f)
                {
                    float hojaY = fy - talloH * (0.22f + (float)_rnd.NextDouble() * 0.36f);
                    float hojaL = pr * (1.35f + (float)_rnd.NextDouble() * 0.75f);
                    float lado  = _rnd.NextDouble() > 0.5f ? 1 : -1;
                    float baseX = fx + (topX - fx) * ((hojaY - fy) / (topY - fy));
                    using (var p = new Pen(ColorTinta(0.36f + pct * 0.16f), Grosor(0.48f)))
                    using (var path = new GraphicsPath())
                    {
                        path.AddBezier(baseX, hojaY,
                                       baseX + lado * hojaL * 0.42f, hojaY - pr * 0.42f,
                                       baseX + lado * hojaL, hojaY,
                                       baseX, hojaY);
                        g.DrawPath(p, path);
                    }
                }

                // Pétalos
                int nPetalos = 5 + _rnd.Next(5);
                using (var p = new Pen(tintaF, Grosor(0.62f + pct * 0.42f)))
                    for (int pe = 0; pe < nPetalos; pe++)
                    {
                        double ang = pe * Math.PI * 2 / nPetalos + _rnd.NextDouble() * 0.22;
                        float pL = pr * (0.82f + (float)_rnd.NextDouble() * 0.48f);
                        if (ink && pct > 0.38f)
                        {
                            using (var path = new GraphicsPath())
                            {
                                path.AddBezier(topX, topY,
                                    topX + (float)(Math.Cos(ang - 0.42) * pL),
                                    topY + (float)(Math.Sin(ang - 0.42) * pL),
                                    topX + (float)(Math.Cos(ang + 0.42) * pL),
                                    topY + (float)(Math.Sin(ang + 0.42) * pL),
                                    topX, topY);
                                g.DrawPath(p, path);
                            }
                        }
                        else
                        {
                            float px2 = topX + (float)(Math.Cos(ang) * pr * 0.88f);
                            float py2 = topY + (float)(Math.Sin(ang) * pr * 0.62f);
                            float eW  = Math.Max(3.2f, pr * 0.52f);
                            float eH  = Math.Max(2.2f, pr * 0.38f);
                            g.DrawEllipse(p, px2 - eW * 0.5f, py2 - eH * 0.5f, eW, eH);
                        }
                    }

                // Centro
                float cR = Math.Max(2.2f, pr * 0.28f);
                using (var p = new Pen(ColorTinta(0.78f), Grosor(0.72f + pct * 0.42f)))
                    g.DrawEllipse(p, topX - cR, topY - cR, cR * 2, cR * 2);
                if (pct > 0.48f)
                    CrossHatch(g, topX - cR, topY - cR, cR * 2, cR * 2, 0.62f, 0.65f, false);
            }
        }

        // ═══════════════════════════════════════════════════════════════════════
        //  EXTRAS: ISLAS / ROCAS / PALMERAS
        // ═══════════════════════════════════════════════════════════════════════
        private void DibujarIslas(Graphics g, int w, int h, int hz)
        {
            int nI = 2 + _rnd.Next(3);
            Color tinta = ColorTinta(0.55f);
            for (int i = 0; i < nI; i++)
            {
                float cx = (float)(w * (0.1 + _rnd.NextDouble() * 0.8));
                float cy = hz - 8 - (float)(_rnd.NextDouble() * hz * 0.11f);
                float rw = 42 + (float)(_rnd.NextDouble() * 88);
                float rh = 14 + (float)(_rnd.NextDouble() * 22);
                using (var p = new Pen(tinta, Grosor(0.95f)))
                    g.DrawEllipse(p, cx - rw, cy - rh, rw * 2, rh * 2);
                DibujarHatching(g, (int)(cx - rw), (int)(cy - rh), (int)(rw * 2), (int)(rh * 2), 8, tinta, 0.25f);
                if (_rnd.NextDouble() > 0.32f)
                    DibujarArbol(g, cx, cy - rh + 2, 40 + (float)_rnd.NextDouble() * 24, 19);
            }
        }

        private void DibujarRocas(Graphics g, int w, int h, int hz)
        {
            bool ink = _ctx.EstiloLapiz == "lapicera";
            int nR = 3 + _rnd.Next(5);
            Color tinta = ColorTinta(0.65f);
            for (int i = 0; i < nR; i++)
            {
                float rx = _rnd.Next(w);
                float ry = hz + (float)(_rnd.NextDouble() * (h - hz) * 0.68f);
                int nPts = 5 + _rnd.Next(4);
                float rr = 16 + (float)(_rnd.NextDouble() * 30);
                var ptR = new PointF[nPts];
                for (int pi = 0; pi < nPts; pi++)
                {
                    double ang = pi * Math.PI * 2 / nPts + _rnd.NextDouble() * 0.28;
                    float rv = rr * (0.62f + (float)_rnd.NextDouble() * 0.72f);
                    ptR[pi] = new PointF(rx + (float)(Math.Cos(ang) * rv),
                                         ry + (float)(Math.Sin(ang) * rv * 0.52f));
                }
                using (var p = new Pen(tinta, Grosor(0.95f)))
                    for (int pi = 0; pi < ptR.Length; pi++)
                    {
                        var a = ptR[pi]; var b = ptR[(pi + 1) % ptR.Length];
                        if (ink) LineInk(g, p, a, b);
                        else     LineTremor(g, p, a, b);
                    }
                DibujarHatching(g, (int)(rx - rr), (int)(ry - rr * 0.52f),
                                 (int)(rr * 2), (int)(rr * 1.05f), 6, tinta, 0.20f);
            }
        }

        private void DibujarPalmeras(Graphics g, int w, int h, int hz)
        {
            bool ink = _ctx.EstiloLapiz == "lapicera";
            int n = 1 + _rnd.Next(4);
            Color tinta = ColorTinta(0.65f);
            for (int i = 0; i < n; i++)
            {
                float bx = _rnd.Next(w);
                float by = hz + (float)(_rnd.NextDouble() * (h - hz) * 0.22f);
                float alt = 95 + (float)(_rnd.NextDouble() * 68);
                float incl = (float)(_rnd.NextDouble() * 24 - 12);
                float topX = bx + incl, topY = by - alt;

                // Tronco bezier
                using (var p = new Pen(tinta, Grosor(1.62f)))
                using (var path = new GraphicsPath())
                {
                    path.AddBezier(bx, by, bx + incl * 0.22f, by - alt * 0.33f,
                                   bx + incl * 0.72f, by - alt * 0.75f, topX, topY);
                    g.DrawPath(p, path);
                }
                // Marcas de tronco
                using (var pM = new Pen(ColorTinta(0.42f), Grosor(0.48f)))
                    for (int m = 1; m < (int)(alt / 18); m++)
                    {
                        float tm = (float)m / (int)(alt / 18);
                        float mx2 = bx + (topX - bx) * tm;
                        float my2 = by + (topY - by) * tm;
                        g.DrawLine(pM, mx2 - 5, my2, mx2 + 5, my2 + 2);
                    }

                // Frondas
                int nF = 8 + _rnd.Next(5);
                for (int f = 0; f < nF; f++)
                {
                    double ang = f * Math.PI * 2 / nF - Math.PI / 2 + _rnd.NextDouble() * 0.28;
                    float len = 48 + (float)(_rnd.NextDouble() * 38);
                    float fEx = topX + (float)(Math.Cos(ang) * len);
                    float fEy = topY + (float)(Math.Sin(ang) * len * 0.52f);
                    using (var p = new Pen(tinta, Grosor(0.82f)))
                    using (var path = new GraphicsPath())
                    {
                        float midX = topX + (float)(Math.Cos(ang) * len * 0.48f) + (float)(_rnd.NextDouble() * 9 - 4.5f);
                        float midY = topY + (float)(Math.Sin(ang) * len * 0.42f) + (float)(_rnd.NextDouble() * 9 - 4.5f);
                        path.AddBezier(topX, topY, midX, midY, fEx, fEy, fEx, fEy);
                        g.DrawPath(p, path);
                    }
                    // Hojillas secundarias (solo ink)
                    if (ink)
                    {
                        int nHj = 4 + _rnd.Next(3);
                        for (int hj = 1; hj < nHj; hj++)
                        {
                            float tf = (float)hj / nHj;
                            float hx = topX + (fEx - topX) * tf;
                            float hy = topY + (fEy - topY) * tf;
                            double perpAng = ang + Math.PI / 2;
                            float hLen = 12 + (float)(_rnd.NextDouble() * 9);
                            for (int side = -1; side <= 1; side += 2)
                            {
                                using (var p = new Pen(ColorTinta(0.52f), 0.45f))
                                    g.DrawLine(p, hx, hy,
                                        hx + (float)(Math.Cos(perpAng) * side * hLen),
                                        hy + (float)(Math.Sin(perpAng) * side * hLen * 0.68f));
                            }
                        }
                    }
                }
            }
        }

        // ═══════════════════════════════════════════════════════════════════════
        //  SUELO (textura de primer plano)
        // ═══════════════════════════════════════════════════════════════════════
        private void DibujarSuelo(Graphics g, int w, int h, int hz)
        {
            bool ink = _ctx.EstiloLapiz == "lapicera";
            int nL = (h - hz) / (ink ? 18 : 26);
            for (int i = 0; i < nL; i++)
            {
                float prof = (float)i / Math.Max(1, nL - 1);
                float y    = hz + (h - hz) * (0.04f + prof * 0.92f);
                float largo = w * (0.12f + (float)_rnd.NextDouble() * 0.72f) * (0.35f + prof * 0.65f);
                float x0   = (float)(_rnd.NextDouble() * (w - largo));
                float alfa = 0.07f + prof * 0.13f;
                float dy   = (float)(_rnd.NextDouble() * 1.2f - 0.6f);
                using (var p = new Pen(ColorTinta(alfa), ink ? 0.42f : Grosor(0.32f)))
                    g.DrawLine(p, x0, y + dy, x0 + largo, y + dy);
            }
        }

        // ═══════════════════════════════════════════════════════════════════════
        //  LLUVIA / NIEVE
        // ═══════════════════════════════════════════════════════════════════════
        private void DibujarLluvia(Graphics g, int w, int h)
        {
            int nG = w / 5;
            for (int i = 0; i < nG; i++)
            {
                float rx = _rnd.Next(w), ry = _rnd.Next(h);
                float lp = 9 + (float)(_rnd.NextDouble() * 12);
                float ang = 0.12f + (float)(_rnd.NextDouble() * 0.07f);
                using (var p = new Pen(ColorTinta(0.16f), Grosor(0.38f)))
                    g.DrawLine(p, rx, ry, rx + ang * lp, ry + lp);
            }
        }

        private void DibujarNieve(Graphics g, int w, int h)
        {
            int nC = w / 7;
            for (int i = 0; i < nC; i++)
            {
                float cx = _rnd.Next(w), cy = _rnd.Next(h);
                float r = 2.5f + (float)(_rnd.NextDouble() * 3.5f);
                using (var p = new Pen(ColorTinta(0.24f), Grosor(0.42f)))
                {
                    g.DrawLine(p, cx - r, cy, cx + r, cy);
                    g.DrawLine(p, cx, cy - r, cx, cy + r);
                    g.DrawLine(p, cx - r * 0.7f, cy - r * 0.7f, cx + r * 0.7f, cy + r * 0.7f);
                    g.DrawLine(p, cx + r * 0.7f, cy - r * 0.7f, cx - r * 0.7f, cy + r * 0.7f);
                }
            }
        }

        // ═══════════════════════════════════════════════════════════════════════
        //  GRANO FINAL Y VIÑETA
        // ═══════════════════════════════════════════════════════════════════════
        private void DibujarGranoFinal(Graphics g, int w, int h)
        {
            int n = (int)(w * h * 0.0016f);
            for (int i = 0; i < n; i++)
            {
                int x = _rnd.Next(w), y = _rnd.Next(h), a = _rnd.Next(4, 15);
                using (var br = new SolidBrush(Color.FromArgb(a, 68, 62, 52)))
                    g.FillRectangle(br, x, y, 1, 1);
            }
            // Segundo pase más grueso
            for (int i = 0; i < n / 4; i++)
            {
                int x = _rnd.Next(w), y = _rnd.Next(h), a = _rnd.Next(3, 10);
                using (var br = new SolidBrush(Color.FromArgb(a, 88, 82, 68)))
                    g.FillRectangle(br, x, y, 2, 1);
            }
        }

        private void DibujarVignetteInk(Graphics g, int w, int h)
        {
            int margen = (int)(Math.Min(w, h) * 0.068f);
            for (int i = 0; i < margen; i++)
            {
                float t = 1.0f - (float)i / margen;
                int alfa = (int)(t * t * 24);
                if (alfa <= 0) break;
                using (var p = new Pen(Color.FromArgb(alfa, 16, 16, 26), 1))
                    g.DrawRectangle(p, i, i, w - i * 2 - 1, h - i * 2 - 1);
            }
        }

        // ═══════════════════════════════════════════════════════════════════════
        //  HELPERS GENERALES
        // ═══════════════════════════════════════════════════════════════════════
        private bool Contiene(List<string> lista, params string[] vals)
        {
            foreach (string v in vals)
                if (lista.Contains(v)) return true;
            return false;
        }

        private void DibujarPerfil(Graphics g, Pen p, List<PointF> pts, bool ink)
        {
            for (int i = 0; i < pts.Count - 1; i++)
            {
                if (ink) LineInk(g, p, pts[i], pts[i + 1]);
                else     LineTremor(g, p, pts[i], pts[i + 1]);
            }
        }

        private void DibujarHatching(Graphics g, int x, int y, int w, int h,
                                      int paso, Color tinta, float alfa)
        {
            Color hCol = Color.FromArgb(Math.Min(255, (int)(255 * alfa)), tinta.R, tinta.G, tinta.B);
            using (var p = new Pen(hCol, Grosor(0.42f)))
            {
                for (int xi = x; xi < x + w; xi += paso)
                    g.DrawLine(p, xi, y, xi, y + h);
                for (int yi = y; yi < y + h; yi += paso * 2)
                    g.DrawLine(p, x, yi, x + w, yi);
            }
        }

        private void CrossHatch(Graphics g, float x, float y, float w, float h,
                                  float densidad, float intensidad, bool doble = true)
        {
            bool ink = _ctx.EstiloLapiz == "lapicera";
            int paso = Math.Max(3, (int)(10f / densidad));
            Color tinta = ColorTinta(intensidad);

            using (var p = new Pen(tinta, ink ? Grosor(0.48f) : Grosor(0.38f)))
            {
                for (float d = -h; d < w; d += paso)
                {
                    float x1 = x + d, y1 = y;
                    float x2 = x + d + h, y2 = y + h;
                    if (x1 < x) { y1 += (x - x1); x1 = x; }
                    if (x2 > x + w) { y2 -= (x2 - (x + w)); x2 = x + w; }
                    if (y1 >= y + h || y2 <= y || x1 >= x + w || x2 <= x) continue;
                    float jx = (float)(_rnd.NextDouble() * 0.45f - 0.22f);
                    float jy = (float)(_rnd.NextDouble() * 0.45f - 0.22f);
                    g.DrawLine(p, x1 + jx, y1 + jy, x2 + jx, y2 + jy);
                }
            }

            if (doble && densidad > 0.62f)
            {
                int paso2 = Math.Max(4, (int)(12f / densidad));
                using (var p = new Pen(ColorTinta(intensidad * 1.05f), ink ? Grosor(0.43f) : Grosor(0.36f)))
                {
                    for (float d = 0; d < w + h; d += paso2)
                    {
                        float x1 = x + d, y1 = y;
                        float x2 = x + d - h, y2 = y + h;
                        if (x1 > x + w) { y1 += (x1 - (x + w)); x1 = x + w; }
                        if (x2 < x)     { y2 -= (x - x2);        x2 = x;     }
                        if (y1 >= y + h || y2 <= y) continue;
                        float jx = (float)(_rnd.NextDouble() * 0.45f - 0.22f);
                        g.DrawLine(p, x1 + jx, y1, x2 + jx, y2);
                    }
                }
            }
        }

        // ─── Línea con tremor orgánico (lápiz/carbón) ────────────────────────
        private void LineTremor(Graphics g, Pen p, PointF a, PointF b)
        {
            float dx = b.X - a.X, dy = b.Y - a.Y;
            float len = (float)Math.Sqrt(dx * dx + dy * dy);
            if (len < 2f) { g.DrawLine(p, a, b); return; }
            int segs = Math.Max(2, (int)(len / 10f));
            PointF prev = a;
            for (int i = 1; i <= segs; i++)
            {
                float t  = (float)i / segs;
                float jx = (float)(_rnd.NextDouble() * 2 - 1) * (len * 0.009f);
                float jy = (float)(_rnd.NextDouble() * 2 - 1) * (len * 0.009f);
                var next = new PointF(a.X + dx * t + jx, a.Y + dy * t + jy);
                g.DrawLine(p, prev, next);
                prev = next;
            }
        }

        // ─── Línea tipo tinta con variación de presión ───────────────────────
        private void LineInk(Graphics g, Pen p, PointF a, PointF b, bool firme = false)
        {
            if (_ctx.EstiloLapiz != "lapicera" || firme)
            {
                LineTremor(g, p, a, b); return;
            }
            float dx = b.X - a.X, dy = b.Y - a.Y;
            float len = (float)Math.Sqrt(dx * dx + dy * dy);
            if (len < 2f) { g.DrawLine(p, a, b); return; }
            int segs = Math.Max(3, (int)(len / 5f));
            PointF prev = a;
            for (int i = 1; i <= segs; i++)
            {
                float t = (float)i / segs;
                // Presión simulada: gruesa en el inicio y fin, fina en medio
                float presion = (float)Math.Sin(t * Math.PI) * 0.42f + 0.58f;
                float jx = (float)(_rnd.NextDouble() * 0.85f - 0.42f);
                float jy = (float)(_rnd.NextDouble() * 0.85f - 0.42f);
                var next = new PointF(a.X + dx * t + jx, a.Y + dy * t + jy);
                using (var pVar = new Pen(p.Color, p.Width * presion))
                    g.DrawLine(pVar, prev, next);
                prev = next;
            }
        }

        // ─── Color de tinta según estilo ─────────────────────────────────────
        private Color ColorTinta(float intensidad)
        {
            intensidad = Math.Max(0f, Math.Min(1f, intensidad));
            if (_ctx.EstiloLapiz == "lapicera")
            {
                // Tinta azul-negra con variación natural
                float v = (float)(_rnd.NextDouble() * 0.055f - 0.028f);
                int bv = (int)(5 + (1 - intensidad) * 34);
                int r  = Math.Max(0, Math.Min(255, bv + (int)(v * 32)));
                int gv = Math.Max(0, Math.Min(255, bv + (int)(v * 16)));
                int b  = Math.Max(0, Math.Min(255, bv + 24 + (int)(v * 26)));
                return Color.FromArgb(r, gv, b);
            }
            else if (_ctx.EstiloLapiz == "carbon")
            {
                // Carbón: negro cálido con textura
                int v = (int)(7 + (1 - intensidad) * 48);
                return Color.FromArgb(v, v, v);
            }
            else
            {
                // Lápiz grafito: gris cálido
                int v = (int)(52 + (1 - intensidad) * 115);
                return Color.FromArgb(v, v - 3, v + 7);
            }
        }

        // ─── Grosor según herramienta ─────────────────────────────────────────
        private float Grosor(float base2)
        {
            if (_ctx.EstiloLapiz == "lapicera") return base2 * 1.45f;
            if (_ctx.EstiloLapiz == "carbon")   return base2 * 2.20f;
            return base2;
        }
    }
}