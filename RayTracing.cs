using System;

namespace OpenCIP
{
    public struct Vec3
    {
        public double X, Y, Z;

        public Vec3(double x, double y, double z) { X=x; Y=y; Z=z; }

        public static Vec3 operator +(Vec3 a, Vec3 b) { return new Vec3(a.X+b.X,a.Y+b.Y,a.Z+b.Z); }
        public static Vec3 operator -(Vec3 a, Vec3 b) { return new Vec3(a.X-b.X,a.Y-b.Y,a.Z-b.Z); }
        public static Vec3 operator -(Vec3 a)         { return new Vec3(-a.X,-a.Y,-a.Z); }
        public static Vec3 operator *(Vec3 a, double t){ return new Vec3(a.X*t,a.Y*t,a.Z*t); }
        public static Vec3 operator *(double t, Vec3 a){ return new Vec3(a.X*t,a.Y*t,a.Z*t); }
        public static Vec3 operator *(Vec3 a, Vec3 b)  { return new Vec3(a.X*b.X,a.Y*b.Y,a.Z*b.Z); }
        public static Vec3 operator /(Vec3 a, double t){ return new Vec3(a.X/t,a.Y/t,a.Z/t); }

        public double LenSq() { return X*X+Y*Y+Z*Z; }
        public double Len()   { return Math.Sqrt(LenSq()); }

        public Vec3 Norm()
        {
            double l = Len();
            return l > 1e-12 ? this/l : new Vec3(0,1,0);
        }

        public static double Dot(Vec3 a, Vec3 b)  { return a.X*b.X+a.Y*b.Y+a.Z*b.Z; }
        public static Vec3  Cross(Vec3 a, Vec3 b)
        {
            return new Vec3(a.Y*b.Z-a.Z*b.Y, a.Z*b.X-a.X*b.Z, a.X*b.Y-a.Y*b.X);
        }
        public static Vec3  Reflect(Vec3 v, Vec3 n){ return v - 2*Dot(v,n)*n; }

        public static readonly Vec3 Zero = new Vec3(0,0,0);
        public static readonly Vec3 Up   = new Vec3(0,1,0);
    }

    public struct Rayo
    {
        public Vec3 Origen;
        public Vec3 Dir;
        public Rayo(Vec3 o, Vec3 d) { Origen=o; Dir=d.Norm(); }
        public Vec3 En(double t)    { return Origen + Dir*t; }
    }

    public struct MaterialRT
    {
        public Vec3   Albedo;
        public double Especular;
        public double Reflexion;
        public double Rugosidad;
        public bool   EsEspejo;
        public bool   EsLuz;
        public Vec3   EmisionColor;
    }

    public struct InfoImpacto
    {
        public bool     Golpeo;
        public double   T;
        public Vec3     Punto;
        public Vec3     Normal;
        public MaterialRT Material;
    }

    public abstract class ObjetoRT
    {
        public MaterialRT Material;
        public abstract InfoImpacto Intersectar(Rayo r);
    }

    public class EsferaRT : ObjetoRT
    {
        public Vec3   Centro;
        public double Radio;

        public override InfoImpacto Intersectar(Rayo r)
        {
            var info = new InfoImpacto();
            Vec3 oc = r.Origen - Centro;
            double a = Vec3.Dot(r.Dir, r.Dir);
            double b = 2*Vec3.Dot(oc, r.Dir);
            double c = Vec3.Dot(oc,oc)-Radio*Radio;
            double d = b*b-4*a*c;
            if (d < 0) return info;
            double t = (-b-Math.Sqrt(d))/(2*a);
            if (t < 0.001) t = (-b+Math.Sqrt(d))/(2*a);
            if (t < 0.001) return info;
            info.Golpeo   = true;
            info.T        = t;
            info.Punto    = r.En(t);
            info.Normal   = ((info.Punto-Centro)/Radio).Norm();
            info.Material = Material;
            return info;
        }
    }

    public class PlanoRT : ObjetoRT
    {
        public Vec3   Normal;
        public double D;       // distancia desde origen
        public Vec3   Albedo2;

        public override InfoImpacto Intersectar(Rayo r)
        {
            var info = new InfoImpacto();
            double denom = Vec3.Dot(Normal, r.Dir);
            if (Math.Abs(denom) < 1e-6) return info;
            double t = -(Vec3.Dot(Normal, r.Origen)+D)/denom;
            if (t < 0.001) return info;

            info.Golpeo   = true;
            info.T        = t;
            info.Punto    = r.En(t);
            info.Normal   = denom < 0 ? Normal : -Normal;

            Vec3 p        = info.Punto;
            int xc        = (int)Math.Floor(p.X/2) & 1;
            int zc        = (int)Math.Floor(p.Z/2) & 1;
            var mat       = Material;
            if ((xc^zc) == 0) mat.Albedo = Albedo2;
            info.Material = mat;
            return info;
        }
    }

    public class CajaRT : ObjetoRT
    {
        public Vec3 Min;  // esquina mínima
        public Vec3 Max;  // esquina máxima
        public Vec3 Albedo2;

        public override InfoImpacto Intersectar(Rayo r)
        {
            var info = new InfoImpacto();
            double tMin = double.NegativeInfinity;
            double tMax = double.PositiveInfinity;
            Vec3 normalMin = Vec3.Zero;

            double[] orig = { r.Origen.X, r.Origen.Y, r.Origen.Z };
            double[] dir  = { r.Dir.X,    r.Dir.Y,    r.Dir.Z    };
            double[] bMin = { Min.X, Min.Y, Min.Z };
            double[] bMax = { Max.X, Max.Y, Max.Z };
            int[] axes    = { 0, 1, 2 };

            for (int i = 0; i < 3; i++)
            {
                double invD = Math.Abs(dir[i]) < 1e-12 ? 1e12 * Math.Sign(dir[i]+1e-15) : 1.0/dir[i];
                double t0 = (bMin[i] - orig[i]) * invD;
                double t1 = (bMax[i] - orig[i]) * invD;
                Vec3 n0 = Vec3.Zero, n1 = Vec3.Zero;
                if (i==0) { n0=new Vec3(-1,0,0); n1=new Vec3(1,0,0); }
                else if(i==1){ n0=new Vec3(0,-1,0); n1=new Vec3(0,1,0); }
                else          { n0=new Vec3(0,0,-1); n1=new Vec3(0,0,1); }
                if (invD < 0) { double tmp=t0; t0=t1; t1=tmp; Vec3 tn=n0; n0=n1; n1=tn; }
                if (t0 > tMin) { tMin = t0; normalMin = n0; }
                if (t1 < tMax)   tMax = t1;
                if (tMin > tMax) return info;
            }

            double tHit = tMin > 0.001 ? tMin : tMax;
            if (tHit < 0.001) return info;

            info.Golpeo   = true;
            info.T        = tHit;
            info.Punto    = r.En(tHit);
            info.Normal   = normalMin.Norm();
            info.Material = Material;
            return info;
        }
    }

    public class CilindroRT : ObjetoRT
    {
        public Vec3   Base;
        public double Radio;
        public double Altura;

        public override InfoImpacto Intersectar(Rayo r)
        {
            var info = new InfoImpacto();
            double ox = r.Origen.X - Base.X;
            double oz = r.Origen.Z - Base.Z;
            double dx = r.Dir.X, dz = r.Dir.Z;
            double a = dx*dx + dz*dz;
            if (a < 1e-10) return info;
            double b = 2*(ox*dx + oz*dz);
            double c = ox*ox + oz*oz - Radio*Radio;
            double disc = b*b - 4*a*c;
            if (disc < 0) return info;
            double sqrtD = Math.Sqrt(disc);
            double t1 = (-b-sqrtD)/(2*a);
            double t2 = (-b+sqrtD)/(2*a);
            double bestT = -1;
            Vec3   bestN = Vec3.Zero;

            for (int k = 0; k < 2; k++)
            {
                double t = k==0 ? t1 : t2;
                if (t < 0.001) continue;
                Vec3 p = r.En(t);
                if (p.Y >= Base.Y && p.Y <= Base.Y + Altura)
                {
                    if (bestT < 0) { bestT=t; bestN=new Vec3(p.X-Base.X,0,p.Z-Base.Z).Norm(); }
                }
            }
            for (int cap = 0; cap < 2; cap++)
            {
                double capY = cap==0 ? Base.Y : Base.Y+Altura;
                Vec3 capN   = cap==0 ? new Vec3(0,-1,0) : new Vec3(0,1,0);
                double denom = Vec3.Dot(capN, r.Dir);
                if (Math.Abs(denom) < 1e-8) continue;
                double t = (capY - r.Origen.Y) / r.Dir.Y;
                if (t < 0.001) continue;
                Vec3 p = r.En(t);
                double dx2=p.X-Base.X, dz2=p.Z-Base.Z;
                if (dx2*dx2+dz2*dz2 <= Radio*Radio)
                    if (bestT < 0 || t < bestT) { bestT=t; bestN=capN; }
            }

            if (bestT < 0) return info;
            info.Golpeo   = true;
            info.T        = bestT;
            info.Punto    = r.En(bestT);
            info.Normal   = bestN;
            info.Material = Material;
            return info;
        }
    }

    public class EsferaPlanetaRT : ObjetoRT
    {
        public Vec3   Centro;
        public double Radio;
        public Vec3   ColAgua, ColTierra, ColArena, ColNieve;
        public int    Semilla;

        public override InfoImpacto Intersectar(Rayo r)
        {
            var info = new InfoImpacto();
            Vec3 oc = r.Origen - Centro;
            double a = Vec3.Dot(r.Dir, r.Dir);
            double b = 2*Vec3.Dot(oc, r.Dir);
            double c = Vec3.Dot(oc,oc)-Radio*Radio;
            double d = b*b-4*a*c;
            if (d < 0) return info;
            double t = (-b-Math.Sqrt(d))/(2*a);
            if (t < 0.001) t = (-b+Math.Sqrt(d))/(2*a);
            if (t < 0.001) return info;

            Vec3 p      = r.En(t);
            Vec3 normal = ((p - Centro)/Radio).Norm();

            double lat = Math.Asin(Matematica.Clamp01(normal.Y*0.5+0.5)*2-1);
            double lon = Math.Atan2(normal.Z, normal.X);
            double u   = (lon / (Math.PI*2) + 0.5 + Semilla*0.01) % 1.0;
            double v   = lat / Math.PI + 0.5;

            double cont = Matematica.FBM(u*3.5+Semilla*0.007, v*3.5+Semilla*0.005, 6, 0.58);
            cont = (cont + 1) * 0.5;  // [0,1]

            double coast = Matematica.FBM(u*7+1.3, v*7+2.1, 4, 0.55)*0.15;
            cont += coast;

            double poleFactor = Math.Abs(normal.Y);
            double nieve = Matematica.Clamp01((poleFactor - 0.75) / 0.15);

            Vec3 col;
            if (nieve > 0.05)
                col = ColNieve * nieve + (cont > 0.52 ? ColTierra : ColAgua) * (1-nieve);
            else if (cont > 0.60)
                col = ColTierra;
            else if (cont > 0.52)
                col = ColArena;
            else
            {
                double depth = Matematica.Clamp01((0.52 - cont) / 0.3);
                col = ColAgua * (1 - depth*0.5);
            }

            if (cont > 0.60 && nieve < 0.5)
            {
                double mont = Matematica.FBM(u*12+3, v*12+5, 3, 0.5)*0.12;
                col = col * (0.88 + mont);
            }

            double nube = Matematica.WarpedFBM(u*4.5+0.7, v*4+1.2, 4, 0.52, 0.3);
            nube = Matematica.Clamp01((nube+1)*0.5 - 0.45) * 1.8;
            col = col*(1-nube*0.7) + new Vec3(0.95,0.97,1.0)*nube*0.7;

            var mat   = Material;
            mat.Albedo = col;
            mat.Especular = cont < 0.52 ? 0.55 : 0.06;
            mat.Reflexion = cont < 0.52 ? 0.25 : 0.01;

            info.Golpeo   = true;
            info.T        = t;
            info.Punto    = p;
            info.Normal   = normal;
            info.Material = mat;
            return info;
        }
    }

    public class EsferaLunaRT : ObjetoRT
    {
        public Vec3   Centro;
        public double Radio;
        public int    Semilla;

        public override InfoImpacto Intersectar(Rayo r)
        {
            var info = new InfoImpacto();
            Vec3 oc = r.Origen - Centro;
            double a = Vec3.Dot(r.Dir,r.Dir);
            double b = 2*Vec3.Dot(oc,r.Dir);
            double c = Vec3.Dot(oc,oc)-Radio*Radio;
            double d = b*b-4*a*c;
            if (d < 0) return info;
            double t = (-b-Math.Sqrt(d))/(2*a);
            if (t < 0.001) t = (-b+Math.Sqrt(d))/(2*a);
            if (t < 0.001) return info;

            Vec3 p      = r.En(t);
            Vec3 normal = ((p - Centro)/Radio).Norm();

            double u = Math.Atan2(normal.Z, normal.X) / (Math.PI*2) + 0.5 + Semilla*0.01;
            double v = Math.Asin(Matematica.Clamp01(normal.Y*0.5+0.5)*2-1) / Math.PI + 0.5;

            double base2 = Matematica.FBM(u*5+Semilla*0.01, v*5, 4, 0.6);
            base2 = (base2+1)*0.5;
            double gray = 0.55 + base2*0.2;

            double crater = 0;
            for (int ci=0; ci<6; ci++)
            {
                double cu = (Semilla*0.137*ci + ci*0.314) % 1.0;
                double cv = (Semilla*0.251*ci + ci*0.618) % 1.0;
                double dist2 = Math.Sqrt((u-cu)*(u-cu)*4 + (v-cv)*(v-cv)*4);
                double cr2 = 0.03 + (ci%3)*0.02;
                if (dist2 < cr2)
                {
                    double f = dist2/cr2;
                    crater = Math.Max(crater, (1-f)*0.25 - (f > 0.8 ? (f-0.8)*0.5 : 0));
                }
            }
            gray = Matematica.Clamp01(gray - crater);

            var mat   = Material;
            mat.Albedo = new Vec3(gray, gray*0.98, gray*0.95);
            mat.Especular = 0.03;

            info.Golpeo   = true;
            info.T        = t;
            info.Punto    = p;
            info.Normal   = normal;
            info.Material = mat;
            return info;
        }
    }

    public class TerrenoRM : ObjetoRT
    {
        public double Escala    = 0.18;   // frecuencia del ruido
        public double Amplitud  = 3.5;
        public double YOffset   = 0.0;    // desplazamiento vertical base
        public int    Semilla   = 0;
        public Vec3   ColorBajo;          // color valle/llanura
        public Vec3   ColorMedio;         // color ladera
        public Vec3   ColorAlto;

        private double Altura(double x, double z)
        {
            double nx = x * Escala + Semilla * 0.01;
            double nz = z * Escala + Semilla * 0.007;
            double v = 0, amp = 1, freq = 1, sum = 0;
            for (int i = 0; i < 5; i++)
            {
                v   += Matematica.Perlin(nx*freq, nz*freq) * amp;
                sum += amp;
                amp *= 0.5; freq *= 2.1;
            }
            return (v / sum) * Amplitud + YOffset;
        }

        public override InfoImpacto Intersectar(Rayo r)
        {
            var info = new InfoImpacto();
            const double tMin = 0.1, tMax = 120.0;
            const double paso0 = 0.25;

            double t = tMin;
            double yAnt = r.En(t).Y - Altura(r.En(t).X, r.En(t).Z);

            while (t < tMax)
            {
                double paso = paso0 * (1 + t * 0.04);  // paso adaptativo
                t += paso;
                Vec3   p  = r.En(t);
                double ySup = p.Y - Altura(p.X, p.Z);
                if (ySup < 0)  // cruzó la superficie
                {
                    double t0 = t - paso, t1 = t;
                    for (int k = 0; k < 8; k++)
                    {
                        double tm = (t0 + t1) * 0.5;
                        Vec3   pm = r.En(tm);
                        double ym = pm.Y - Altura(pm.X, pm.Z);
                        if (ym < 0) t1 = tm; else t0 = tm;
                    }
                    double tFin = (t0 + t1) * 0.5;
                    Vec3   pFin = r.En(tFin);

                    const double eps = 0.05;
                    double hL = Altura(pFin.X - eps, pFin.Z);
                    double hR = Altura(pFin.X + eps, pFin.Z);
                    double hD = Altura(pFin.X, pFin.Z - eps);
                    double hU = Altura(pFin.X, pFin.Z + eps);
                    Vec3 normal = new Vec3(hL - hR, 2*eps, hD - hU);
                    normal = normal.Norm();

                    double altNorm = Matematica.Clamp01(pFin.Y / Amplitud);
                    Vec3 albedo;
                    if (altNorm < 0.35)
                        albedo = ColorBajo * (1 - altNorm/0.35) + ColorMedio * (altNorm/0.35);
                    else if (altNorm < 0.75)
                    {
                        double f = (altNorm - 0.35) / 0.40;
                        albedo = ColorMedio * (1-f) + ColorAlto * f;
                    }
                    else
                        albedo = ColorAlto;

                    double tex = 0.85 + 0.15 * Matematica.Perlin(pFin.X*1.5, pFin.Z*1.5);
                    albedo = albedo * tex;

                    var mat = Material;
                    mat.Albedo = albedo;

                    info.Golpeo   = true;
                    info.T        = tFin;
                    info.Punto    = pFin;
                    info.Normal   = normal;
                    info.Material = mat;
                    return info;
                }
                yAnt = ySup;
            }
            return info;
        }
    }

    public class CuevaRM : ObjetoRT
    {
        public double RadioBase = 4.0;
        public int    Semilla   = 0;
        public Vec3   ColorPared;
        public Vec3   ColorTecho;
        public Vec3   ColorSuelo;

        private Vec3 CentroTunel(double z)
        {
            double ox = Math.Sin(z * 0.18 + Semilla * 0.05) * 1.8;
            double oy = Math.Cos(z * 0.12 + Semilla * 0.03) * 0.9 + 2.5;
            return new Vec3(ox, oy, z);
        }

        private double Ruido3D(double x, double y, double z)
        {
            return Matematica.Perlin(x*0.3 + y*0.17, z*0.3 + x*0.11)
                 + 0.5 * Matematica.Perlin(x*0.6 + z*0.23, y*0.6 + z*0.19)
                 + 0.25* Matematica.Perlin(x*1.1 + y*0.31, z*1.1 + x*0.27);
        }

        private double SDF(Vec3 p)
        {
            Vec3   c    = CentroTunel(p.Z);
            double dx   = p.X - c.X;
            double dy   = p.Y - c.Y;
            double dist = Math.Sqrt(dx*dx + dy*dy);
            double radio= RadioBase + Ruido3D(p.X, p.Y, p.Z) * 1.6;
            return dist - radio;
        }

        private Vec3 Gradiente(Vec3 p)
        {
            const double e = 0.04;
            return new Vec3(
                SDF(new Vec3(p.X+e, p.Y,   p.Z  )) - SDF(new Vec3(p.X-e, p.Y,   p.Z  )),
                SDF(new Vec3(p.X,   p.Y+e, p.Z  )) - SDF(new Vec3(p.X,   p.Y-e, p.Z  )),
                SDF(new Vec3(p.X,   p.Y,   p.Z+e)) - SDF(new Vec3(p.X,   p.Y,   p.Z-e))
            ).Norm();
        }

        public override InfoImpacto Intersectar(Rayo r)
        {
            var info  = new InfoImpacto();
            const double tMin = 0.2, tMax = 60.0, eps = 0.05;
            double t = tMin;

            for (int i = 0; i < 200 && t < tMax; i++)
            {
                Vec3   p   = r.En(t);
                double sdf = SDF(p);

                if (sdf > 0.0)
                {
                    double t0 = t - Math.Abs(sdf), t1 = t;
                    for (int k = 0; k < 6; k++)
                    {
                        double tm = (t0+t1)*0.5;
                        if (SDF(r.En(tm)) > 0) t1 = tm; else t0 = tm;
                    }
                    Vec3 pFin  = r.En((t0+t1)*0.5);
                    Vec3 grad  = Gradiente(pFin);
                    Vec3 normal= (-grad).Norm();

                    Vec3 centro = CentroTunel(pFin.Z);
                    double dy   = pFin.Y - centro.Y;
                    Vec3 albedo;
                    if (dy > RadioBase * 0.4)
                        albedo = ColorTecho;
                    else if (dy < -RadioBase * 0.4)
                        albedo = ColorSuelo;
                    else
                        albedo = ColorPared;

                    double tex = 0.8 + 0.2 * Matematica.Perlin(pFin.X*2.0, pFin.Z*2.0);
                    albedo = albedo * tex;

                    var mat   = Material;
                    mat.Albedo = albedo;

                    info.Golpeo   = true;
                    info.T        = (t0+t1)*0.5;
                    info.Punto    = pFin;
                    info.Normal   = normal;
                    info.Material = mat;
                    return info;
                }

                t += Math.Max(0.05, -sdf * 0.8);
            }
            return info;
        }
    }

    public class LuzRT
    {
        public Vec3   Pos;
        public Vec3   Color;
        public double Intensidad;
    }
}
