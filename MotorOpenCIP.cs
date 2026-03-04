using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace OpenCIP
{
    public static class MotorOpenCIP
    {
        public static Bitmap Renderizar(int ancho, int alto, ContextoVisual ctx, IProgress<int> progreso)
        {
            if (ctx.ModoLienzo)
            {
                var gen = new GeneradorLienzo(ctx);
                Action<int> repL = delegate(int p) { if (progreso != null) progreso.Report(p); };
                return gen.Generar(ancho, alto, repL);
            }

            if (ctx.Algoritmos.Count == 1 && ctx.Algoritmos[0] == AlgoritmoBase.Acuarela)
            {
                var gen = new GeneradorAcuarela(ctx);
                Action<int> repA = delegate(int p) { if (progreso != null) progreso.Report(p); };
                return gen.Generar(ancho, alto, repA);
            }

            if (ctx.Algoritmos.Count == 1 && ctx.Algoritmos[0] == AlgoritmoBase.DibujoLapiz)
            {
                var gen = new GeneradorDibujo(ctx);
                Action<int> repD = delegate(int p) { if (progreso != null) progreso.Report(p); };
                return gen.Generar(ancho, alto, repD);
            }

            if (ctx.Algoritmos.Count == 1)
            {
                if (ctx.Algoritmos[0] == AlgoritmoBase.MundoVoxelMinecraft)
                {
                    var gen = new GeneradorMundoVoxel(ctx);
                    if (progreso != null) progreso.Report(50);
                    Bitmap r = gen.Generar(ancho, alto);
                    if (progreso != null) progreso.Report(100);
                    return r;
                }
            }

            Matematica.InicializarSemilla(ctx.Semilla);

            Bitmap bmp = new Bitmap(ancho, alto, PixelFormat.Format24bppRgb);
            Rectangle rect = new Rectangle(0,0,ancho,alto);
            BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.WriteOnly, bmp.PixelFormat);
            int stride = Math.Abs(bmpData.Stride);
            byte[] pixels = new byte[stride*alto];

            List<Color> paleta = ctx.Paleta.Count > 0 ? ctx.Paleta : new List<Color>{ Color.Black, Color.White };
            int procesados = 0;
            object lok = new object();

            Parallel.For(0, alto, delegate(int y)
            {
                double ny = (double)y/alto;
                double sy = ctx.ModoSimetrico ? (ny<0.5 ? ny*2:(1-ny)*2) : ny;

                for (int x = 0; x < ancho; x++)
                {
                    double nx = (double)x/ancho;
                    double sx = ctx.ModoSimetrico ? (nx<0.5 ? nx*2:(1-nx)*2) : nx;

                    double valor = 0;
                    for (int a = 0; a < ctx.Algoritmos.Count; a++)
                    {
                        double v = EjecutarAlgoritmo(ctx.Algoritmos[a], sx, sy, ctx);
                        valor += v*ctx.PesosAlgoritmos[a];
                    }
                    valor = Matematica.Clamp01(valor);

                    if (ctx.ModoOscuro) valor = valor*valor;

                    Color c = Matematica.MutiStop(paleta, valor);
                    if (Math.Abs(ctx.Saturacion-1.0) > 0.01)
                        c = Matematica.AjustarSaturacion(c, ctx.Saturacion);
                    if (ctx.ModoRetro) c = CuantizarRetro(c, 6);

                    int off = y*stride+x*3;
                    pixels[off]   = c.B;
                    pixels[off+1] = c.G;
                    pixels[off+2] = c.R;
                }

                lock (lok)
                {
                    procesados++;
                    if (progreso != null && procesados%20 == 0)
                        progreso.Report(procesados*100/alto);
                }
            });

            Marshal.Copy(pixels, 0, bmpData.Scan0, pixels.Length);
            bmp.UnlockBits(bmpData);
            return bmp;
        }

        public static Bitmap RenderizarProgresivo(int ancho, int alto, ContextoVisual ctx,
            IProgress<int> progreso,
            Action<Bitmap>  actualizarCanvas,
            ref bool cancelar)
        {
            Action<int> rep = delegate(int p) {
                if (progreso != null) progreso.Report(p);
            };

            if (ctx.Algoritmos.Count == 1 && ctx.Algoritmos[0] == AlgoritmoBase.RedNeuralCPPN)
            {
                var gen = new GeneradorRedNeuronal(ctx);
                return gen.GenerarProgresivo(ancho, alto, rep, actualizarCanvas, ref cancelar);
            }

            if (ctx.Algoritmos.Count == 1 && ctx.Algoritmos[0] == AlgoritmoBase.Escena3D)
            {
                Matematica.InicializarSemilla(ctx.Semilla);
                var gen = new GeneradorEscena3D(ctx);
                return gen.GenerarProgresivo(ancho, alto, rep, actualizarCanvas, ref cancelar);
            }

            return Renderizar(ancho, alto, ctx, progreso);
        }

        private static double EjecutarAlgoritmo(AlgoritmoBase algo, double nx, double ny, ContextoVisual ctx)
        {
            switch (algo)
            {
                case AlgoritmoBase.RuidoPerlin:         return Generadores.Perlin(nx, ny, ctx);
                case AlgoritmoBase.FractalMandelbrot:   return Generadores.Fractal(nx, ny, ctx);
                case AlgoritmoBase.FluidoTurbulento:    return Generadores.Fluido(nx, ny, ctx);
                case AlgoritmoBase.GeometricoSimetrico: return Generadores.Geometrico(nx, ny, ctx);
                case AlgoritmoBase.VoronoiCelular:      return Generadores.Voronoi(nx, ny, ctx);
                case AlgoritmoBase.OndaInterferencia:   return Generadores.Onda(nx, ny, ctx);
                case AlgoritmoBase.NebulosaEspacial:    return Generadores.Nebulosa(nx, ny, ctx);
                case AlgoritmoBase.PlasmaCaos:          return Generadores.Plasma(nx, ny, ctx);
                case AlgoritmoBase.MundoVoxelMinecraft: return 0;
                case AlgoritmoBase.RedNeuralCPPN:       return 0;
                case AlgoritmoBase.Escena3D:            return 0;
                case AlgoritmoBase.DibujoLapiz:         return 0;
                default: return 0;
            }
        }

        private static Color CuantizarRetro(Color c, int niveles)
        {
            double step = 255.0/(niveles-1);
            int r = (int)(Math.Round(c.R/step)*step);
            int g = (int)(Math.Round(c.G/step)*step);
            int b = (int)(Math.Round(c.B/step)*step);
            return Color.FromArgb(Math.Min(255,r),Math.Min(255,g),Math.Min(255,b));
        }
    }
}
