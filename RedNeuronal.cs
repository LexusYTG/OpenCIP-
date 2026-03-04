using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace OpenCIP
{
    public class RedNeuronal256
    {
        private const int N_IN  = 4;
        private const int N_H1  = 256;
        private const int N_H2  = 256;
        private const int N_OUT = 3;    // R, G, B

        private double[,] W1;   // [N_IN,  N_H1]
        private double[]  b1;   // [N_H1]
        private int[]     act1;

        private double[,] W2;   // [N_H1, N_H2]
        private double[]  b2;   // [N_H2]
        private int[]     act2;

        private double[,] W3;   // [N_H2, N_OUT]
        private double[]  b3;   // [N_OUT]

        private double escala;

        public void Inicializar(int semilla, ContextoVisual ctx)
        {
            escala = ctx.Escala * ctx.Complejidad;
            var rnd = new Random(semilla);

            W1   = new double[N_IN,  N_H1];
            b1   = new double[N_H1];
            act1 = new int[N_H1];

            W2   = new double[N_H1, N_H2];
            b2   = new double[N_H2];
            act2 = new int[N_H2];

            W3   = new double[N_H2, N_OUT];
            b3   = new double[N_OUT];

            double xL1 = Math.Sqrt(2.0 / N_IN);
            double xL2 = Math.Sqrt(2.0 / N_H1);
            double xL3 = Math.Sqrt(2.0 / N_H2);

            for (int j = 0; j < N_H1; j++)
            {
                b1[j]   = (rnd.NextDouble()*2-1) * 0.05;
                act1[j] = rnd.Next(8);
                for (int i = 0; i < N_IN; i++)
                    W1[i,j] = (rnd.NextDouble()*2-1) * xL1 * ctx.Intensidad;
            }

            for (int j = 0; j < N_H2; j++)
            {
                b2[j]   = (rnd.NextDouble()*2-1) * 0.05;
                act2[j] = rnd.Next(8);
                for (int i = 0; i < N_H1; i++)
                    W2[i,j] = (rnd.NextDouble()*2-1) * xL2;
            }

            for (int j = 0; j < N_OUT; j++)
            {
                b3[j] = (rnd.NextDouble()*2-1) * 0.05;
                for (int i = 0; i < N_H2; i++)
                    W3[i,j] = (rnd.NextDouble()*2-1) * xL3;
            }

            if (ctx.Paleta.Count > 0)
            {
                Color col = ctx.Paleta[0];
                b3[0] += (col.R/255.0 - 0.5) * 0.6;
                b3[1] += (col.G/255.0 - 0.5) * 0.6;
                b3[2] += (col.B/255.0 - 0.5) * 0.6;
            }
        }

        public void Evaluar(double nx, double ny, out double ro, out double go, out double bo)
        {
            double cx  = nx*2-1;
            double cy  = ny*2-1;
            double rad = Math.Sqrt(cx*cx+cy*cy);
            double ang = Math.Atan2(cy,cx)/Math.PI;

            double[] inp = new double[]{ cx*escala, cy*escala, rad*escala, ang };

            double[] h1 = new double[N_H1];
            for (int j = 0; j < N_H1; j++)
            {
                double s = b1[j];
                for (int i = 0; i < N_IN; i++) s += W1[i,j]*inp[i];
                h1[j] = Activar(s, act1[j]);
            }

            double[] h2 = new double[N_H2];
            for (int j = 0; j < N_H2; j++)
            {
                double s = b2[j];
                for (int i = 0; i < N_H1; i++) s += W2[i,j]*h1[i];
                h2[j] = Activar(s, act2[j]);
            }

            double sr=b3[0], sg=b3[1], sb=b3[2];
            for (int i = 0; i < N_H2; i++)
            {
                sr += W3[i,0]*h2[i];
                sg += W3[i,1]*h2[i];
                sb += W3[i,2]*h2[i];
            }

            ro = Matematica.Clamp01((Math.Tanh(sr)+1)/2);
            go = Matematica.Clamp01((Math.Tanh(sg)+1)/2);
            bo = Matematica.Clamp01((Math.Tanh(sb)+1)/2);
        }

        private double Activar(double x, int tipo)
        {
            switch (tipo)
            {
                case 0: return Math.Sin(x);
                case 1: return Math.Cos(x);
                case 2: return Math.Tanh(x);
                case 3: return Math.Exp(-x*x*0.5);           // Gaussiana
                case 4: return Math.Abs(Math.Sin(x));
                case 5: return 1.0/(1.0+Math.Exp(-x));       // Sigmoide
                case 6: return Math.Max(0, x);                // ReLU
                case 7: return Math.Log(1.0+Math.Exp(x));    // Softplus
                default: return Math.Sin(x);
            }
        }
    }

    public class GeneradorRedNeuronal
    {
        private RedNeuronal256 _red;
        private ContextoVisual  _ctx;

        public GeneradorRedNeuronal(ContextoVisual ctx)
        {
            _ctx = ctx;
            _red = new RedNeuronal256();
            _red.Inicializar(ctx.Semilla, ctx);
        }

        public Bitmap GenerarProgresivo(int ancho, int alto,
            Action<int> reportarProgreso,
            Action<Bitmap> actualizarCanvas,
            ref bool cancelar)
        {
            int stride3 = ancho*3;
            byte[] buf  = new byte[alto*stride3];

            Bitmap bmpParcial = new Bitmap(ancho, alto, PixelFormat.Format24bppRgb);

            for (int y = 0; y < alto; y++)
            {
                if (cancelar) break;

                double ny = (double)y/alto;
                double sy = _ctx.ModoSimetrico ? (ny<0.5 ? ny*2:(1-ny)*2) : ny;

                for (int x = 0; x < ancho; x++)
                {
                    double nx = (double)x/ancho;
                    double sx = _ctx.ModoSimetrico ? (nx<0.5 ? nx*2:(1-nx)*2) : nx;

                    double ro, go, bo;
                    _red.Evaluar(sx, sy, out ro, out go, out bo);

                    if (Math.Abs(_ctx.Saturacion-1.0) > 0.01)
                    {
                        Color c = Color.FromArgb((int)(ro*255),(int)(go*255),(int)(bo*255));
                        c = Matematica.AjustarSaturacion(c, _ctx.Saturacion);
                        ro = c.R/255.0; go = c.G/255.0; bo = c.B/255.0;
                    }
                    if (_ctx.ModoOscuro) { ro*=ro; go*=go; bo*=bo; }

                    int off = y*stride3+x*3;
                    buf[off]   = (byte)(bo*255);
                    buf[off+1] = (byte)(go*255);
                    buf[off+2] = (byte)(ro*255);
                }

                if ((y % 6 == 0 || y == alto-1) && actualizarCanvas != null)
                {
                    int lineasCopiadas = (y+1)*stride3;
                    var bmpData = bmpParcial.LockBits(
                        new Rectangle(0,0,ancho,alto), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
                    Marshal.Copy(buf, 0, bmpData.Scan0, lineasCopiadas);
                    bmpParcial.UnlockBits(bmpData);
                    actualizarCanvas(new Bitmap(bmpParcial));
                }

                if (reportarProgreso != null)
                    reportarProgreso(y*100/alto);
            }

            Bitmap bmpFinal = new Bitmap(ancho, alto, PixelFormat.Format24bppRgb);
            var bdFinal = bmpFinal.LockBits(
                new Rectangle(0,0,ancho,alto), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            Marshal.Copy(buf, 0, bdFinal.Scan0, buf.Length);
            bmpFinal.UnlockBits(bdFinal);
            return bmpFinal;
        }
    }
}
