using System;
using System.Drawing;

namespace OpenCIP
{
    public static class Generadores
    {
        public static double Perlin(double nx, double ny, ContextoVisual ctx)
        {
            double e = ctx.ModoSuave ? 0.4 : 0.5;
            double v;
            if (ctx.ModoCaos)
            {
                double warpFuerza = ctx.Intensidad * 0.8;
                v = Matematica.WarpedFBM(nx*ctx.Escala, ny*ctx.Escala, 7, e, warpFuerza);
            }
            else if (ctx.ModoSuave)
            {
                v = Matematica.FBM(nx*ctx.Escala, ny*ctx.Escala, 6, e);
            }
            else
            {
                double v1 = Matematica.FBM(nx*ctx.Escala, ny*ctx.Escala, 5, e);
                double v2 = Matematica.Turbulencia(nx*ctx.Escala+3.7, ny*ctx.Escala+1.3, 4);
                v = v1*0.7 + v2*0.3;
            }
            return Matematica.Clamp01((v+1)/2 * ctx.Intensidad);
        }

        public static double Fractal(double nx, double ny, ContextoVisual ctx)
        {
            bool esJulia = ctx.ModoCaos;
            double zx, zy, cx, cy;
            double orbitaMin = double.MaxValue;

            if (esJulia)
            {
                double t = (ctx.Semilla % 1000) / 1000.0;
                cx = -0.7 + t * 0.3;
                cy = 0.27015 + t * 0.1;
                zx = nx * 3.5 * ctx.Escala - 1.75;
                zy = ny * 3.5 * ctx.Escala - 1.75;
            }
            else
            {
                zx = nx * 3.5 * ctx.Escala - 2.5;
                zy = ny * 2.0 * ctx.Escala - 1.0;
                cx = zx; cy = zy;
            }

            double zx2 = zx*zx, zy2 = zy*zy;
            int i = 0;
            while (zx2+zy2 < 256.0 && i < ctx.Iteraciones)
            {
                double orbDist = Math.Sqrt(zx2+zy2);
                if (orbDist < orbitaMin) orbitaMin = orbDist;

                zy = 2*zx*zy + cy;
                zx = zx2-zy2 + cx;
                zx2 = zx*zx; zy2 = zy*zy;
                i++;
            }

            if (i == ctx.Iteraciones)
            {
                double interior = Math.Sin(orbitaMin * 8.0 * ctx.Complejidad) * 0.5 + 0.5;
                return Matematica.Clamp01(interior * 0.15 * ctx.Intensidad);
            }

            double log2 = Math.Log(Math.Sqrt(zx2+zy2)) / Math.Log(2.0);
            double suave = i + 1.0 - Math.Log(log2) / Math.Log(2.0);
            double v = suave / ctx.Iteraciones;

            double v1 = Math.Sin(v * Math.PI * 6  * ctx.Complejidad + orbitaMin * 2) * 0.5 + 0.5;
            double v2 = Math.Cos(v * Math.PI * 14 * ctx.Complejidad) * 0.5 + 0.5;
            v = (v1 * 0.6 + v2 * 0.4) * v;

            return Matematica.Clamp01(v * ctx.Intensidad);
        }

        public static double Fluido(double nx, double ny, ContextoVisual ctx)
        {
            double t    = ctx.Semilla * 0.001;
            double esc  = ctx.Escala;

            double eps    = 0.01;
            double p00    = Matematica.FBM(nx*esc+t, ny*esc,     4, 0.5);
            double p10    = Matematica.FBM(nx*esc+t+eps, ny*esc, 4, 0.5);
            double p01    = Matematica.FBM(nx*esc+t, ny*esc+eps, 4, 0.5);
            double curlX  = (p01 - p00) / eps;
            double curlY  = -(p10 - p00) / eps;

            double fuerza = ctx.Intensidad * 0.2;
            double fx = nx + curlX * fuerza;
            double fy = ny + curlY * fuerza;

            double r = Matematica.FBM(fx*2, fy*2, ctx.ModoCaos ? 7 : 5, 0.55);

            if (ctx.ModoCaos)
            {
                double r2 = Matematica.Turbulencia(fx*3+2.1, fy*3+3.8, 5);
                r = r * 0.6 + r2 * 0.4;
            }

            return Matematica.Clamp01((r+1)/2);
        }

        public static double Geometrico(double nx, double ny, ContextoVisual ctx)
        {
            double cx = nx-0.5, cy = ny-0.5;
            double dist = Math.Sqrt(cx*cx+cy*cy);
            double ang  = Math.Atan2(cy, cx);
            int segs    = 3+(ctx.Semilla%9);
            double escF = ctx.Escala*ctx.Complejidad;

            double espiral = Math.Sin((ang - Math.Log(dist+0.001)*2) * segs * ctx.Intensidad);

            double radial = Math.Sin(dist*20*escF) * Math.Cos(ang*segs*2);

            double onda = Math.Sin(cx*15*escF+cy*11*escF);

            double v = espiral*0.4 + radial*0.4 + onda*0.2;

            if (ctx.ModoSimetrico)
            {
                double vsim = Math.Sin(dist*6*escF+ang*segs)*Math.Cos(dist*8*escF);
                v = (v+vsim)/2;
            }
            return Matematica.Clamp01((v+1)/2);
        }

        public static double Voronoi(double nx, double ny, ContextoVisual ctx)
        {
            var rnd = new Random(ctx.Semilla);
            int N   = 25+(int)(ctx.Complejidad*12);
            double[] px = new double[N];
            double[] py = new double[N];
            for (int i = 0; i < N; i++)
            {
                px[i] = rnd.NextDouble();
                py[i] = rnd.NextDouble();
            }

            double min1=double.MaxValue, min2=double.MaxValue;
            for (int i = 0; i < N; i++)
            {
                double dx = nx-px[i], dy = ny-py[i];
                double d  = Math.Sqrt(dx*dx+dy*dy)*ctx.Escala;
                if (d < min1) { min2=min1; min1=d; }
                else if (d < min2) min2=d;
            }

            double borde = (min2-min1)*ctx.Intensidad*3;

            double tex = Matematica.FBM(nx*ctx.Escala*2, ny*ctx.Escala*2, 3, 0.5)*0.3;
            return Matematica.Clamp01(borde*0.7+tex*0.3);
        }

        public static double Onda(double nx, double ny, ContextoVisual ctx)
        {
            var rnd  = new Random(ctx.Semilla);
            int nOndas = 5+(int)(ctx.Complejidad*4);
            double acc  = 0;

            for (int i = 0; i < nOndas; i++)
            {
                double ox   = rnd.NextDouble();
                double oy   = rnd.NextDouble();
                double freq = (0.8+rnd.NextDouble()*3)*ctx.Escala;
                double fase = rnd.NextDouble()*Math.PI*2;

                double dx = nx-ox, dy = ny-oy;
                double r  = Math.Sqrt(dx*dx+dy*dy);
                double ondaCirc = Math.Sin(r*freq*15+fase);

                double angOnd = rnd.NextDouble()*Math.PI*2;
                double ondaLin = Math.Cos((nx*Math.Cos(angOnd)+ny*Math.Sin(angOnd))*freq*10+fase);

                acc += (ondaCirc+ondaLin)*0.5;
            }
            acc /= nOndas;
            if (ctx.ModoSuave) acc = Math.Tanh(acc*2.5);

            return Matematica.Clamp01((acc+1)/2*ctx.Intensidad);
        }

        public static double Nebulosa(double nx, double ny, ContextoVisual ctx)
        {
            double e = ctx.Escala;

            double n1 = Matematica.WarpedFBM(nx*e*2.0, ny*e*2.0, 6, 0.6, 0.5);

            double n2 = Matematica.Turbulencia(nx*e*3.5+1.7, ny*e*3.5+0.8, 5);

            double n3 = Matematica.FBM(nx*e*8+n1*2, ny*e*8+n2*2, 4, 0.4);

            double estrellas = Math.Max(0, n3-0.6)*5;

            double v = n1*0.5+n2*0.3+(n3+1)*0.5*0.15+estrellas*0.05;
            return Matematica.Clamp01(v*ctx.Intensidad);
        }

        public static double Plasma(double nx, double ny, ContextoVisual ctx)
        {
            double e = ctx.Escala * ctx.Complejidad;
            double t = ctx.Semilla * 0.00123;
            int variante = ctx.Semilla % 4;

            double v;
            switch (variante)
            {
                case 0:
                    v = Math.Sin(nx*e*10 + Math.Sin(ny*e*7 + t))
                      + Math.Cos(ny*e*8  + Math.Cos(nx*e*9 + t*0.7))
                      + Math.Sin((nx+ny)*e*6)
                      + Math.Cos((nx-ny)*e*4 + t*0.5);
                    v /= 4.0;
                    break;
                case 1:
                    double r1 = Math.Sqrt((nx-0.5)*(nx-0.5)+(ny-0.5)*(ny-0.5));
                    double a1 = Math.Atan2(ny-0.5, nx-0.5);
                    v = Math.Sin(r1*e*20+t*2)
                      + Math.Cos(a1*5+r1*e*10)
                      + Math.Sin(nx*e*8+ny*e*12+t);
                    v /= 3.0;
                    break;
                case 2:
                    v = Math.Sin(nx*e*12 + ny*e*8  + t)
                      * Math.Cos(nx*e*5  - ny*e*9  + t*0.8)
                      + Math.Sin((nx-0.3)*e*16) * Math.Sin((ny-0.7)*e*14+t);
                    break;
                default:
                    double cx = nx-0.5, cy2 = ny-0.5;
                    double r2  = Math.Sqrt(cx*cx+cy2*cy2)*e;
                    v = Math.Sin(r2*15+t)
                      + Math.Cos(nx*e*10+t*0.5)
                      + Math.Sin(ny*e*10+t*0.3)
                      + Math.Cos((nx+ny)*e*7);
                    v /= 4.0;
                    break;
            }

            if (ctx.ModoCaos)
            {
                double r = Math.Sqrt((nx-0.5)*(nx-0.5)+(ny-0.5)*(ny-0.5));
                v += Math.Sin(r*e*20+t)*0.5;
                v += Math.Cos(nx*e*13)*Math.Sin(ny*e*11+t)*0.3;
            }

            return Matematica.Clamp01((v*0.5+0.5)*ctx.Intensidad);
        }
    }
}
