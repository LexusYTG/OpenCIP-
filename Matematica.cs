using System;
using System.Collections.Generic;
using System.Drawing;

namespace OpenCIP
{
    public static class Matematica
    {
        private static int[]  _perm  = new int[1024];
        private static bool   _init  = false;

        public static void InicializarSemilla(int semilla)
        {
            var rnd = new Random(semilla);
            int[] p = new int[256];
            for (int i = 0; i < 256; i++) p[i] = i;
            for (int i = 255; i > 0; i--)
            {
                int j = rnd.Next(i + 1);
                int t = p[i]; p[i] = p[j]; p[j] = t;
            }
            for (int i = 0; i < 1024; i++) _perm[i] = p[i & 255];
            _init = true;
        }

        private static double Fade(double t)  { return t*t*t*(t*(t*6-15)+10); }
        private static double Lerp(double t, double a, double b) { return a+t*(b-a); }
        private static double Grad(int h, double x, double y)
        {
            h &= 15;
            double u = h < 8 ? x : y;
            double v = h < 4 ? y : (h==12||h==14) ? x : 0;
            return ((h&1)==0?u:-u)+((h&2)==0?v:-v);
        }

        public static double Perlin(double x, double y)
        {
            if (!_init) InicializarSemilla(42);
            int X = (int)Math.Floor(x)&255;
            int Y = (int)Math.Floor(y)&255;
            x -= Math.Floor(x); y -= Math.Floor(y);
            double u = Fade(x), v = Fade(y);
            int A = _perm[X]+Y, B = _perm[X+1]+Y;
            return Lerp(v,
                Lerp(u, Grad(_perm[A],x,y),   Grad(_perm[B],x-1,y)),
                Lerp(u, Grad(_perm[A+1],x,y-1),Grad(_perm[B+1],x-1,y-1)));
        }

        public static double FBM(double x, double y, int octavas, double persistencia)
        {
            double total=0, amp=1, freq=1, max=0;
            for (int i = 0; i < octavas; i++)
            {
                total += Perlin(x*freq, y*freq)*amp;
                max+=amp; amp*=persistencia; freq*=2;
            }
            return total/max;
        }

        public static double WarpedFBM(double x, double y, int octavas, double pers, double fuerza)
        {
            double wx = FBM(x+0.0, y+0.0, octavas, pers) * fuerza;
            double wy = FBM(x+5.2, y+1.3, octavas, pers) * fuerza;
            return FBM(x+wx, y+wy, octavas, pers);
        }

        public static double Turbulencia(double x, double y, int octavas)
        {
            double total=0, amp=1, freq=1, max=0;
            for (int i = 0; i < octavas; i++)
            {
                total += Math.Abs(Perlin(x*freq, y*freq))*amp;
                max+=amp; amp*=0.5; freq*=2;
            }
            return total/max;
        }

        public static double Clamp01(double v)  { return Math.Max(0, Math.Min(1, v)); }
        public static double Clamp(double v, double mn, double mx) { return Math.Max(mn, Math.Min(mx, v)); }

        public static double Map(double v, double inMin, double inMax, double outMin, double outMax)
        {
            return outMin+(v-inMin)/(inMax-inMin)*(outMax-outMin);
        }

        public static double SuavMin(double a, double b, double k)
        {
            double h = Clamp01(0.5 + 0.5*(b-a)/k);
            return Lerp(h, b, a) - k*h*(1.0-h);
        }

        public static Color LerpColor(Color a, Color b, double t)
        {
            t = Clamp01(t);
            return Color.FromArgb(
                (int)(a.A+(b.A-a.A)*t),
                (int)(a.R+(b.R-a.R)*t),
                (int)(a.G+(b.G-a.G)*t),
                (int)(a.B+(b.B-a.B)*t));
        }

        public static Color MutiStop(List<Color> paleta, double t)
        {
            if (paleta.Count == 0) return Color.Black;
            if (paleta.Count == 1) return paleta[0];
            t = Clamp01(t);
            double seg = t*(paleta.Count-1);
            int idx = (int)seg;
            if (idx >= paleta.Count-1) return paleta[paleta.Count-1];
            return LerpColor(paleta[idx], paleta[idx+1], seg-idx);
        }

        public static Color AjustarSaturacion(Color c, double sat)
        {
            double r = c.R/255.0, g = c.G/255.0, b = c.B/255.0;
            double gris = r*0.299+g*0.587+b*0.114;
            r = Clamp01(gris+(r-gris)*sat);
            g = Clamp01(gris+(g-gris)*sat);
            b = Clamp01(gris+(b-gris)*sat);
            return Color.FromArgb((int)(r*255),(int)(g*255),(int)(b*255));
        }

        public static Color ColorHSV(double h, double s, double v)
        {
            s=Clamp01(s); v=Clamp01(v); h=h%360; if(h<0) h+=360;
            double c=v*s, x=c*(1-Math.Abs((h/60)%2-1)), m=v-c;
            double r,g,b;
            if      (h<60)  {r=c;g=x;b=0;}
            else if (h<120) {r=x;g=c;b=0;}
            else if (h<180) {r=0;g=c;b=x;}
            else if (h<240) {r=0;g=x;b=c;}
            else if (h<300) {r=x;g=0;b=c;}
            else            {r=c;g=0;b=x;}
            return Color.FromArgb((int)((r+m)*255),(int)((g+m)*255),(int)((b+m)*255));
        }

        public static Color MezclarColores(Color a, Color b, ModoFusion modo, double t)
        {
            t = Clamp01(t);
            double ra=a.R/255.0, ga=a.G/255.0, ba_=a.B/255.0;
            double rb=b.R/255.0, gb=b.G/255.0, bb_=b.B/255.0;
            double ro,go,bo;
            switch (modo)
            {
                case ModoFusion.Multiplicar:
                    ro=ra*rb; go=ga*gb; bo=ba_*bb_; break;
                case ModoFusion.Pantalla:
                    ro=1-(1-ra)*(1-rb); go=1-(1-ga)*(1-gb); bo=1-(1-ba_)*(1-bb_); break;
                case ModoFusion.Diferencia:
                    ro=Math.Abs(ra-rb); go=Math.Abs(ga-gb); bo=Math.Abs(ba_-bb_); break;
                default:
                    ro=rb; go=gb; bo=bb_; break;
            }
            return Color.FromArgb(
                (int)((ra+(ro-ra)*t)*255),
                (int)((ga+(go-ga)*t)*255),
                (int)((ba_+(bo-ba_)*t)*255));
        }
    }
}
