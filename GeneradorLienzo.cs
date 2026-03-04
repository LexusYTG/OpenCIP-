using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace OpenCIP
{
    public class GeneradorLienzo
    {
        private ContextoVisual _ctx;
        private Random _rnd;

        private Color _cieloAlto, _cieloMedio, _cieloHorizonte;
        private Color _sueloBase, _sueloDetalle;
        private Color _colorSol,  _colorGlow;
        private int   _solX, _solY, _solRadio;

        public GeneradorLienzo(ContextoVisual ctx)
        {
            _ctx = ctx;
            _rnd = new Random(ctx.Semilla);
            ConfigurarPaleta();
        }

        private void ConfigurarPaleta()
        {
            List<Color> pal = _ctx.Paleta;

            switch (_ctx.EscenaLienzo)
            {
                case TipoEscena.Bosque:
                    _cieloAlto      = Color.FromArgb(100,150,220);
                    _cieloMedio     = Color.FromArgb(160,200,240);
                    _cieloHorizonte = Color.FromArgb(200,220,200);
                    _sueloBase      = Color.FromArgb(30, 80, 20);
                    _sueloDetalle   = Color.FromArgb(20, 55, 10);
                    _colorSol       = Color.FromArgb(255,240,150);
                    _colorGlow      = Color.FromArgb(255,220,100);
                    break;
                case TipoEscena.CampoFlores:
                    _cieloAlto      = Color.FromArgb(100,180,255);
                    _cieloMedio     = Color.FromArgb(160,210,255);
                    _cieloHorizonte = Color.FromArgb(210,230,255);
                    _sueloBase      = Color.FromArgb(60,130,30);
                    _sueloDetalle   = Color.FromArgb(200,60,80);
                    _colorSol       = Color.FromArgb(255,245,100);
                    _colorGlow      = Color.FromArgb(255,230,140);
                    break;
                case TipoEscena.Montana:
                    _cieloAlto      = Color.FromArgb(40,80,160);
                    _cieloMedio     = Color.FromArgb(100,140,220);
                    _cieloHorizonte = Color.FromArgb(180,200,230);
                    _sueloBase      = Color.FromArgb(80,90,100);
                    _sueloDetalle   = Color.FromArgb(230,235,240);
                    _colorSol       = Color.FromArgb(255,250,220);
                    _colorGlow      = Color.FromArgb(255,230,180);
                    break;
                case TipoEscena.Desierto:
                    _cieloAlto      = Color.FromArgb(200,140,60);
                    _cieloMedio     = Color.FromArgb(230,180,80);
                    _cieloHorizonte = Color.FromArgb(250,220,120);
                    _sueloBase      = Color.FromArgb(210,170,80);
                    _sueloDetalle   = Color.FromArgb(180,130,50);
                    _colorSol       = Color.FromArgb(255,240,80);
                    _colorGlow      = Color.FromArgb(255,160,30);
                    break;
                case TipoEscena.Noche:
                    _cieloAlto      = Color.FromArgb(5,5,25);
                    _cieloMedio     = Color.FromArgb(10,10,50);
                    _cieloHorizonte = Color.FromArgb(20,20,80);
                    _sueloBase      = Color.FromArgb(10,20,30);
                    _sueloDetalle   = Color.FromArgb(5,10,20);
                    _colorSol       = Color.FromArgb(240,240,220);  // luna
                    _colorGlow      = Color.FromArgb(100,100,180);
                    break;
                case TipoEscena.Paisaje3D:
                    _cieloAlto      = Color.FromArgb(60,100,180);
                    _cieloMedio     = Color.FromArgb(100,150,220);
                    _cieloHorizonte = Color.FromArgb(160,190,240);
                    _sueloBase      = Color.FromArgb(50,80,40);
                    _sueloDetalle   = Color.FromArgb(100,120,80);
                    _colorSol       = Color.FromArgb(255,245,180);
                    _colorGlow      = Color.FromArgb(255,200,100);
                    break;
                case TipoEscena.SelvaLluviosa:
                    _cieloAlto      = Color.FromArgb(60,100,50);
                    _cieloMedio     = Color.FromArgb(100,140,80);
                    _cieloHorizonte = Color.FromArgb(140,170,100);
                    _sueloBase      = Color.FromArgb(20,60,15);
                    _sueloDetalle   = Color.FromArgb(10,40,8);
                    _colorSol       = Color.FromArgb(220,255,150);
                    _colorGlow      = Color.FromArgb(180,220,80);
                    break;
                case TipoEscena.Tundra:
                    _cieloAlto      = Color.FromArgb(10,15,40);
                    _cieloMedio     = Color.FromArgb(20,40,80);
                    _cieloHorizonte = Color.FromArgb(30,80,60);
                    _sueloBase      = Color.FromArgb(200,220,240);
                    _sueloDetalle   = Color.FromArgb(160,185,210);
                    _colorSol       = Color.FromArgb(80,255,120);   // aurora verde
                    _colorGlow      = Color.FromArgb(120,80,200);   // aurora violeta
                    break;
                case TipoEscena.Volcan:
                    _cieloAlto      = Color.FromArgb(30,10,10);
                    _cieloMedio     = Color.FromArgb(80,20,5);
                    _cieloHorizonte = Color.FromArgb(180,60,10);
                    _sueloBase      = Color.FromArgb(20,10,5);
                    _sueloDetalle   = Color.FromArgb(200,80,0);
                    _colorSol       = Color.FromArgb(255,120,0);
                    _colorGlow      = Color.FromArgb(255,50,0);
                    break;
                case TipoEscena.Lago:
                    _cieloAlto      = Color.FromArgb(80,120,200);
                    _cieloMedio     = Color.FromArgb(130,170,230);
                    _cieloHorizonte = Color.FromArgb(190,215,245);
                    _sueloBase      = Color.FromArgb(20,80,130);
                    _sueloDetalle   = Color.FromArgb(10,50,90);
                    _colorSol       = Color.FromArgb(255,250,200);
                    _colorGlow      = Color.FromArgb(255,220,140);
                    break;
                case TipoEscena.Pantano:
                    _cieloAlto      = Color.FromArgb(40,55,35);
                    _cieloMedio     = Color.FromArgb(70,85,55);
                    _cieloHorizonte = Color.FromArgb(110,120,80);
                    _sueloBase      = Color.FromArgb(25,40,15);
                    _sueloDetalle   = Color.FromArgb(40,65,25);
                    _colorSol       = Color.FromArgb(180,220,80);
                    _colorGlow      = Color.FromArgb(100,160,30);
                    break;
                case TipoEscena.Valle:
                    _cieloAlto      = Color.FromArgb(100,160,230);
                    _cieloMedio     = Color.FromArgb(150,195,245);
                    _cieloHorizonte = Color.FromArgb(200,225,250);
                    _sueloBase      = Color.FromArgb(50,120,40);
                    _sueloDetalle   = Color.FromArgb(80,150,60);
                    _colorSol       = Color.FromArgb(255,245,170);
                    _colorGlow      = Color.FromArgb(255,215,100);
                    break;
                case TipoEscena.Playa:
                    _cieloAlto      = Color.FromArgb(50,140,230);
                    _cieloMedio     = Color.FromArgb(100,185,245);
                    _cieloHorizonte = Color.FromArgb(170,215,250);
                    _sueloBase      = Color.FromArgb(0,160,170);
                    _sueloDetalle   = Color.FromArgb(220,200,140);
                    _colorSol       = Color.FromArgb(255,240,100);
                    _colorGlow      = Color.FromArgb(255,200,60);
                    break;
                case TipoEscena.CañonRocoso:
                    _cieloAlto      = Color.FromArgb(100,150,220);
                    _cieloMedio     = Color.FromArgb(160,195,235);
                    _cieloHorizonte = Color.FromArgb(210,220,230);
                    _sueloBase      = Color.FromArgb(160,80,40);
                    _sueloDetalle   = Color.FromArgb(120,55,25);
                    _colorSol       = Color.FromArgb(255,220,120);
                    _colorGlow      = Color.FromArgb(255,170,50);
                    break;
                default: // Oceano / atardecer
                    _cieloAlto      = Color.FromArgb(30, 20, 80);
                    _cieloMedio     = Color.FromArgb(120,50,140);
                    _cieloHorizonte = Color.FromArgb(240,110,40);
                    _sueloBase      = Color.FromArgb(10,35,90);
                    _sueloDetalle   = Color.FromArgb(20,80,180);
                    _colorSol       = Color.FromArgb(255,245,80);
                    _colorGlow      = Color.FromArgb(255,150,20);
                    break;
            }

            if (pal.Count >= 1) _cieloAlto      = pal[0];
            if (pal.Count >= 2) _cieloHorizonte  = pal[1];
            if (pal.Count >= 3) _sueloBase        = pal[2];

            if (_ctx.ModoOscuro)
            {
                _cieloAlto      = OscC(_cieloAlto,      45);
                _cieloHorizonte = OscC(_cieloHorizonte, 35);
                _colorGlow      = Color.FromArgb(80,60,160);
            }
        }

        public Bitmap Generar(int ancho, int alto, Action<int> progreso)
        {
            Matematica.InicializarSemilla(_ctx.Semilla);

            float[] bufR = new float[ancho*alto];
            float[] bufG = new float[ancho*alto];
            float[] bufB = new float[ancho*alto];

            double hFrac = 0.44;
            switch (_ctx.EscenaLienzo)
            {
                case TipoEscena.Montana:    hFrac = 0.48; break;
                case TipoEscena.Paisaje3D:  hFrac = 0.45; break;
                case TipoEscena.Noche:      hFrac = 0.42; break;
                case TipoEscena.Desierto:   hFrac = 0.38; break;
            }
            int horizonte = (int)(alto * (hFrac + (_rnd.NextDouble()-0.5)*0.05));

            _solRadio = Math.Max(18, Math.Min(ancho,alto)/10);
            _solX     = (int)(ancho * (0.2 + _rnd.NextDouble()*0.6));
            _solY     = (_ctx.EscenaLienzo == TipoEscena.Noche)
                ? (int)(alto*0.08 + _rnd.NextDouble()*alto*0.12)
                : (int)(alto*0.10 + _rnd.NextDouble()*alto*0.18);

            CapaCielo(bufR, bufG, bufB, ancho, alto, horizonte); if (progreso!=null) progreso(12);

            bool tieneSol = _ctx.EscenaLienzo != TipoEscena.SelvaLluviosa
                         && _ctx.EscenaLienzo != TipoEscena.Pantano
                         && _ctx.EscenaLienzo != TipoEscena.CañonRocoso;
            if (tieneSol)
                CapaSol(bufR, bufG, bufB, ancho, alto);
            if (progreso!=null) progreso(22);

            switch (_ctx.EscenaLienzo)
            {
                case TipoEscena.Oceano:
                    CapaOceano(bufR,bufG,bufB,ancho,alto,horizonte);      if (progreso!=null) progreso(50);
                    CapaReflejo(bufR,bufG,bufB,ancho,alto,horizonte);     if (progreso!=null) progreso(70);
                    CapaNubes(bufR,bufG,bufB,ancho,alto,horizonte);       if (progreso!=null) progreso(88);
                    break;
                case TipoEscena.Bosque:
                    CapaSuelo(bufR,bufG,bufB,ancho,alto,horizonte);       if (progreso!=null) progreso(40);
                    CapaBosque(bufR,bufG,bufB,ancho,alto,horizonte);      if (progreso!=null) progreso(70);
                    CapaNiebla(bufR,bufG,bufB,ancho,alto,horizonte,0.4f); if (progreso!=null) progreso(82);
                    CapaNubes(bufR,bufG,bufB,ancho,alto,horizonte);       if (progreso!=null) progreso(90);
                    break;
                case TipoEscena.CampoFlores:
                    CapaSuelo(bufR,bufG,bufB,ancho,alto,horizonte);       if (progreso!=null) progreso(40);
                    CapaCampoFlores(bufR,bufG,bufB,ancho,alto,horizonte); if (progreso!=null) progreso(72);
                    CapaNubes(bufR,bufG,bufB,ancho,alto,horizonte);       if (progreso!=null) progreso(90);
                    break;
                case TipoEscena.Montana:
                    CapaMontanas(bufR,bufG,bufB,ancho,alto,horizonte);    if (progreso!=null) progreso(60);
                    CapaNiebla(bufR,bufG,bufB,ancho,alto,horizonte,0.3f); if (progreso!=null) progreso(74);
                    CapaNubes(bufR,bufG,bufB,ancho,alto,horizonte);       if (progreso!=null) progreso(88);
                    break;
                case TipoEscena.Desierto:
                    CapaDesierto(bufR,bufG,bufB,ancho,alto,horizonte);    if (progreso!=null) progreso(65);
                    CapaCalima(bufR,bufG,bufB,ancho,alto,horizonte);      if (progreso!=null) progreso(82);
                    break;
                case TipoEscena.Noche:
                    CapaEstrellas(bufR,bufG,bufB,ancho,alto,horizonte);   if (progreso!=null) progreso(35);
                    CapaSuelo(bufR,bufG,bufB,ancho,alto,horizonte);       if (progreso!=null) progreso(50);
                    CapaNiebla(bufR,bufG,bufB,ancho,alto,horizonte,0.5f); if (progreso!=null) progreso(68);
                    CapaNubes(bufR,bufG,bufB,ancho,alto,horizonte);       if (progreso!=null) progreso(85);
                    break;
                case TipoEscena.Paisaje3D:
                    CapaTerrenoPerspectiva(bufR,bufG,bufB,ancho,alto,horizonte); if (progreso!=null) progreso(65);
                    CapaNieblaAtmosferica(bufR,bufG,bufB,ancho,alto,horizonte);  if (progreso!=null) progreso(80);
                    CapaNubes(bufR,bufG,bufB,ancho,alto,horizonte);              if (progreso!=null) progreso(90);
                    break;
                case TipoEscena.SelvaLluviosa:
                    CapaSuelo(bufR,bufG,bufB,ancho,alto,horizonte);             if (progreso!=null) progreso(30);
                    CapaBosque(bufR,bufG,bufB,ancho,alto,horizonte);            if (progreso!=null) progreso(52);
                    CapaSelvaExtra(bufR,bufG,bufB,ancho,alto,horizonte);        if (progreso!=null) progreso(70);
                    CapaNiebla(bufR,bufG,bufB,ancho,alto,horizonte,0.65f);      if (progreso!=null) progreso(83);
                    CapaNubes(bufR,bufG,bufB,ancho,alto,horizonte);             if (progreso!=null) progreso(91);
                    break;
                case TipoEscena.Tundra:
                    CapaSuelo(bufR,bufG,bufB,ancho,alto,horizonte);             if (progreso!=null) progreso(28);
                    CapaAurora(bufR,bufG,bufB,ancho,alto,horizonte);            if (progreso!=null) progreso(55);
                    CapaEstrellas(bufR,bufG,bufB,ancho,alto,horizonte);         if (progreso!=null) progreso(65);
                    CapaNiebla(bufR,bufG,bufB,ancho,alto,horizonte,0.3f);       if (progreso!=null) progreso(82);
                    break;
                case TipoEscena.Volcan:
                    CapaSuelo(bufR,bufG,bufB,ancho,alto,horizonte);             if (progreso!=null) progreso(25);
                    CapaMontanas(bufR,bufG,bufB,ancho,alto,horizonte);          if (progreso!=null) progreso(45);
                    CapaLava(bufR,bufG,bufB,ancho,alto,horizonte);              if (progreso!=null) progreso(65);
                    CapaCeniza(bufR,bufG,bufB,ancho,alto,horizonte);            if (progreso!=null) progreso(82);
                    break;
                case TipoEscena.Lago:
                    CapaSuelo(bufR,bufG,bufB,ancho,alto,horizonte);             if (progreso!=null) progreso(28);
                    CapaLago(bufR,bufG,bufB,ancho,alto,horizonte);              if (progreso!=null) progreso(55);
                    CapaReflejo(bufR,bufG,bufB,ancho,alto,horizonte);           if (progreso!=null) progreso(70);
                    CapaNiebla(bufR,bufG,bufB,ancho,alto,horizonte,0.35f);      if (progreso!=null) progreso(82);
                    CapaNubes(bufR,bufG,bufB,ancho,alto,horizonte);             if (progreso!=null) progreso(90);
                    break;
                case TipoEscena.Pantano:
                    CapaSuelo(bufR,bufG,bufB,ancho,alto,horizonte);             if (progreso!=null) progreso(28);
                    CapaBosque(bufR,bufG,bufB,ancho,alto,horizonte);            if (progreso!=null) progreso(50);
                    CapaNiebla(bufR,bufG,bufB,ancho,alto,horizonte,0.70f);      if (progreso!=null) progreso(70);
                    CapaLucesGas(bufR,bufG,bufB,ancho,alto,horizonte);          if (progreso!=null) progreso(85);
                    break;
                case TipoEscena.Valle:
                    CapaSuelo(bufR,bufG,bufB,ancho,alto,horizonte);             if (progreso!=null) progreso(28);
                    CapaRio(bufR,bufG,bufB,ancho,alto,horizonte);               if (progreso!=null) progreso(50);
                    CapaBosque(bufR,bufG,bufB,ancho,alto,horizonte);            if (progreso!=null) progreso(65);
                    CapaNieblaAtmosferica(bufR,bufG,bufB,ancho,alto,horizonte); if (progreso!=null) progreso(80);
                    CapaNubes(bufR,bufG,bufB,ancho,alto,horizonte);             if (progreso!=null) progreso(90);
                    break;
                case TipoEscena.Playa:
                    CapaSuelo(bufR,bufG,bufB,ancho,alto,horizonte);             if (progreso!=null) progreso(25);
                    CapaOceano(bufR,bufG,bufB,ancho,alto,horizonte);            if (progreso!=null) progreso(45);
                    CapaArena(bufR,bufG,bufB,ancho,alto,horizonte);             if (progreso!=null) progreso(62);
                    CapaReflejo(bufR,bufG,bufB,ancho,alto,horizonte);           if (progreso!=null) progreso(75);
                    CapaNubes(bufR,bufG,bufB,ancho,alto,horizonte);             if (progreso!=null) progreso(90);
                    break;
                case TipoEscena.CañonRocoso:
                    CapaSuelo(bufR,bufG,bufB,ancho,alto,horizonte);             if (progreso!=null) progreso(25);
                    CapaCanon(bufR,bufG,bufB,ancho,alto,horizonte);             if (progreso!=null) progreso(55);
                    CapaCalima(bufR,bufG,bufB,ancho,alto,horizonte);            if (progreso!=null) progreso(75);
                    CapaNubes(bufR,bufG,bufB,ancho,alto,horizonte);             if (progreso!=null) progreso(90);
                    break;
                default:
                    CapaOceano(bufR,bufG,bufB,ancho,alto,horizonte);
                    CapaReflejo(bufR,bufG,bufB,ancho,alto,horizonte);
                    CapaNubes(bufR,bufG,bufB,ancho,alto,horizonte);
                    break;
            }

            Bitmap bmp = new Bitmap(ancho, alto, PixelFormat.Format24bppRgb);
            var bd = bmp.LockBits(new Rectangle(0,0,ancho,alto), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            int stride = Math.Abs(bd.Stride);
            byte[] pix = new byte[alto*stride];
            for (int y=0;y<alto;y++) for (int x=0;x<ancho;x++)
            {
                int i=y*ancho+x, off=y*stride+x*3;
                pix[off]  =(byte)(C1(bufB[i])*255);
                pix[off+1]=(byte)(C1(bufG[i])*255);
                pix[off+2]=(byte)(C1(bufR[i])*255);
            }
            Marshal.Copy(pix, 0, bd.Scan0, pix.Length);
            bmp.UnlockBits(bd);
            if (progreso!=null) progreso(100);
            return bmp;
        }


        private void CapaCielo(float[] r, float[] g, float[] b, int w, int h, int hz)
        {
            float r0=_cieloAlto.R/255f,      g0=_cieloAlto.G/255f,      b0=_cieloAlto.B/255f;
            float r1=_cieloMedio.R/255f,     g1=_cieloMedio.G/255f,     b1=_cieloMedio.B/255f;
            float r2=_cieloHorizonte.R/255f, g2=_cieloHorizonte.G/255f, b2=_cieloHorizonte.B/255f;
            for (int y=0;y<hz;y++)
            {
                double t  = (double)y/hz;
                double tc = Math.Pow(t, 0.6);
                double n  = Matematica.FBM((double)1.8/w+1.1, (double)y/h*3+0.4, 3, 0.5)*0.07;
                tc = Matematica.Clamp01(tc+n);
                float ra,ga,ba;
                if (tc < 0.5) { double u=tc*2; ra=(float)(r0*(1-u)+r1*u); ga=(float)(g0*(1-u)+g1*u); ba=(float)(b0*(1-u)+b1*u); }
                else          { double u=tc*2-1; ra=(float)(r1*(1-u)+r2*u); ga=(float)(g1*(1-u)+g2*u); ba=(float)(b1*(1-u)+b2*u); }
                for (int x=0;x<w;x++)
                {
                    double nx=(double)x/w;
                    double dn=Matematica.FBM(nx*2.2, (double)y/h*2.5+0.9, 3, 0.5)*0.04;
                    int idx=y*w+x;
                    r[idx]=C1(ra+(float)dn); g[idx]=C1(ga+(float)dn*0.5f); b[idx]=C1(ba);
                }
            }
        }

        private void CapaSol(float[] r, float[] g, float[] b, int w, int h)
        {
            float rs=_colorSol.R/255f, gs=_colorSol.G/255f, bs=_colorSol.B/255f;
            float rg=_colorGlow.R/255f, gg=_colorGlow.G/255f, bg=_colorGlow.B/255f;
            int glR = _solRadio*5;
            bool esLuna = _ctx.EscenaLienzo == TipoEscena.Noche;
            int yMin=Math.Max(0,_solY-glR), yMax=Math.Min(h-1,_solY+glR);
            int xMin=Math.Max(0,_solX-glR), xMax=Math.Min(w-1,_solX+glR);
            for (int y=yMin;y<=yMax;y++) for (int x=xMin;x<=xMax;x++)
            {
                double dx=x-_solX, dy=y-_solY, dist=Math.Sqrt(dx*dx+dy*dy);
                if (dist<=_solRadio)
                {
                    double t=dist/_solRadio; int idx=y*w+x;
                    if (esLuna) { double c=0.92*(1-t)+0.78*t; r[idx]=(float)c; g[idx]=(float)(c*0.97); b[idx]=(float)c; }
                    else        { r[idx]=(float)(1.0*(1-t)+rs*t); g[idx]=(float)(0.99*(1-t)+gs*t); b[idx]=(float)(0.80*(1-t)+bs*t); }
                }
                else if (dist<=glR)
                {
                    double t=(dist-_solRadio)/(glR-_solRadio);
                    double glow=Math.Pow(1-t, esLuna?3.0:2.2)*0.85;
                    int idx=y*w+x;
                    r[idx]=C1((float)(r[idx]*(1-glow)+rg*glow));
                    g[idx]=C1((float)(g[idx]*(1-glow)+gg*glow));
                    b[idx]=C1((float)(b[idx]*(1-glow)+bg*glow));
                }
            }
        }

        private void CapaNubes(float[] r, float[] g, float[] b, int w, int h, int hz)
        {
            double esc=_ctx.Escala*2.0;
            for (int y=0;y<hz;y++)
            {
                double yRel=(double)y/hz;
                double dens=Math.Pow(yRel,1.5)*0.85; if (dens<0.06) continue;
                for (int x=0;x<w;x++)
                {
                    double nx=(double)x/w, ny=(double)y/h;
                    double n1=Matematica.WarpedFBM(nx*esc+1.7, ny*esc*2.5+0.3, 5, 0.55, 0.35);
                    double nube=(n1+1)*0.5;
                    double umbral=0.60-dens*0.14; if (nube<=umbral) continue;
                    double alpha=Matematica.Clamp01((nube-umbral)/(1-umbral))*dens*0.72;
                    double tH=Math.Pow(yRel,1.1);
                    float cr=C1((float)(0.96*(1-tH)+_cieloHorizonte.R/255f*tH*1.1));
                    float cg=C1((float)(0.93*(1-tH)+_cieloHorizonte.G/255f*tH*0.85));
                    float cb=C1((float)(1.00*(1-tH)+_cieloHorizonte.B/255f*tH*0.5));
                    int idx=y*w+x;
                    r[idx]=C1((float)(r[idx]*(1-alpha)+cr*alpha));
                    g[idx]=C1((float)(g[idx]*(1-alpha)+cg*alpha));
                    b[idx]=C1((float)(b[idx]*(1-alpha)+cb*alpha));
                }
            }
        }

        private void CapaSuelo(float[] r, float[] g, float[] b, int w, int h, int hz)
        {
            float r0=_cieloHorizonte.R/255f, g0=_cieloHorizonte.G/255f, b0=_cieloHorizonte.B/255f;
            float r1=_sueloBase.R/255f, g1=_sueloBase.G/255f, b1=_sueloBase.B/255f;
            double esc=_ctx.Escala*2.5, tS=_ctx.Semilla*0.003;
            for (int y=hz;y<h;y++)
            {
                double yR=(double)(y-hz)/(h-hz);
                for (int x=0;x<w;x++)
                {
                    double nx=(double)x/w;
                    double n=Matematica.FBM(nx*esc+tS, yR*esc*0.8, 4, 0.55);
                    double blend=Matematica.Clamp01(Math.Pow(yR,0.5)+(n+1)*0.5*0.18-0.08);
                    int idx=y*w+x;
                    r[idx]=(float)(r0*(1-blend)+r1*blend);
                    g[idx]=(float)(g0*(1-blend)+g1*blend);
                    b[idx]=(float)(b0*(1-blend)+b1*blend);
                }
            }
            int mez=Math.Max(4,hz/12);
            for (int y=hz-mez;y<hz;y++) { if(y<0)continue;
                double t=(double)(y-(hz-mez))/mez; int yA=Math.Min(h-1,hz+mez);
                for (int x=0;x<w;x++) { double a=t*0.4; int ic=y*w+x, ia=yA*w+x;
                    r[ic]=C1((float)(r[ic]*(1-a)+r[ia]*a));
                    g[ic]=C1((float)(g[ic]*(1-a)+g[ia]*a));
                    b[ic]=C1((float)(b[ic]*(1-a)+b[ia]*a)); }
            }
        }

        private void CapaNiebla(float[] r, float[] g, float[] b, int w, int h, int hz, float opacidad)
        {
            float fr=_cieloHorizonte.R/255f*1.1f, fg=_cieloHorizonte.G/255f*1.1f, fb=_cieloHorizonte.B/255f*1.15f;
            fr=C1(fr); fg=C1(fg); fb=C1(fb);
            int bandaH=(int)(h*0.12);
            for (int y=hz-bandaH/2; y<hz+bandaH; y++)
            {
                if (y<0||y>=h) continue;
                double yR=Math.Abs(y-hz)/(double)bandaH;
                double alpha=Math.Max(0, (1-yR*yR)*opacidad*0.8);
                for (int x=0;x<w;x++)
                {
                    double nx=(double)x/w;
                    double n=Matematica.FBM(nx*3.5+(double)y/h*2.0, _ctx.Semilla*0.001, 3, 0.5);
                    double fa=Matematica.Clamp01(alpha*(0.7+n*0.3));
                    int idx=y*w+x;
                    r[idx]=C1((float)(r[idx]*(1-fa)+fr*fa));
                    g[idx]=C1((float)(g[idx]*(1-fa)+fg*fa));
                    b[idx]=C1((float)(b[idx]*(1-fa)+fb*fa));
                }
            }
        }


        private void CapaOceano(float[] r, float[] g, float[] b, int w, int h, int hz)
        {
            float rh=_cieloHorizonte.R/255f, gh=_cieloHorizonte.G/255f, bh=_cieloHorizonte.B/255f;
            float rf=_sueloBase.R/255f, gf=_sueloBase.G/255f, bf=_sueloBase.B/255f;
            double esc=_ctx.Escala*1.6, tS=_ctx.Semilla*0.0027, eps=0.01;
            for (int y=hz;y<h;y++)
            {
                double yR=(double)(y-hz)/(h-hz);
                for (int x=0;x<w;x++)
                {
                    double nx=(double)x/w;
                    double p00=Matematica.FBM(nx*esc+tS, yR*esc*0.6, 4, 0.5);
                    double p10=Matematica.FBM(nx*esc+tS+eps, yR*esc*0.6, 4, 0.5);
                    double p01=Matematica.FBM(nx*esc+tS, yR*esc*0.6+eps, 4, 0.5);
                    double wave=Matematica.FBM((nx+(p01-p00)/eps*0.12)*2.8, (yR-(p10-p00)/eps*0.12)*1.8+tS*0.6, 5, 0.55);
                    double blend=Matematica.Clamp01(Math.Pow(yR,0.7)+(wave+1)*0.5*0.22-0.10);
                    int idx=y*w+x;
                    r[idx]=(float)(rh*(1-blend)+rf*blend);
                    g[idx]=(float)(gh*(1-blend)+gf*blend);
                    b[idx]=(float)(bh*(1-blend)+bf*blend);
                }
            }
            FranjaHorizonte(r,g,b,w,h,hz);
        }

        private void CapaReflejo(float[] r, float[] g, float[] b, int w, int h, int hz)
        {
            float rs=_colorSol.R/255f, gs=_colorSol.G/255f, bs=_colorSol.B/255f;
            for (int y=hz;y<h;y++)
            {
                double yR=(double)(y-hz)/(h-hz);
                double anchoRef=_solRadio*(0.6+yR*3.0);
                double onda=Matematica.FBM((double)y/h*4, _ctx.Semilla*0.002, 3, 0.5)*anchoRef*0.35;
                for (int x=0;x<w;x++)
                {
                    double dx=Math.Abs(x-(_solX+onda)); if (dx>anchoRef*2.5) continue;
                    double inten=Math.Max(0,1.0-(dx/anchoRef)*(dx/anchoRef))*(1-yR*0.65)*0.80;
                    double sc=Matematica.Perlin((double)x/w*10+1.3,(double)y/h*8+_ctx.Semilla*0.01);
                    inten=Matematica.Clamp01(inten*(0.65+sc*0.35));
                    int idx=y*w+x;
                    r[idx]=C1((float)(r[idx]*(1-inten)+rs*inten));
                    g[idx]=C1((float)(g[idx]*(1-inten)+gs*inten));
                    b[idx]=C1((float)(b[idx]*(1-inten)+bs*inten));
                }
            }
            int nRays=10+_rnd.Next(10);
            for (int i=0;i<nRays;i++)
            {
                double ang=Math.PI*(-0.5+(_rnd.NextDouble()-0.5)*0.65);
                double rayI=_ctx.Intensidad*0.13*(0.35+_rnd.NextDouble()*0.65);
                float rrr=rs*0.55f, rgr=gs*0.45f, rbr=bs*0.25f;
                for (int y=_solY;y<h;y++)
                {
                    double dist=y-_solY, xR=_solX+Math.Tan(ang)*dist;
                    double halfW=0.015*dist+_solRadio*0.4;
                    int x0=Math.Max(0,(int)(xR-halfW-2)), x1=Math.Min(w-1,(int)(xR+halfW+2));
                    for (int x=x0;x<=x1;x++)
                    {
                        double ddx=Math.Abs(x-xR); if(ddx>halfW) continue;
                        double fade=Matematica.Clamp01((1-ddx/halfW)*(1-dist/(h-_solY+1))*rayI);
                        int idx=y*w+x;
                        r[idx]=C1(r[idx]+(float)fade*rrr);
                        g[idx]=C1(g[idx]+(float)fade*rgr);
                        b[idx]=C1(b[idx]+(float)fade*rbr);
                    }
                }
            }
        }

        private void CapaBosque(float[] r, float[] g, float[] b, int w, int h, int hz)
        {
            // Fondo verde degradado primero
            float rS=_sueloBase.R/255f, gS=_sueloBase.G/255f, bS=_sueloBase.B/255f;
            float rH=_cieloHorizonte.R/255f, gH=_cieloHorizonte.G/255f, bH=_cieloHorizonte.B/255f;
            for (int y=hz; y<h; y++)
            {
                double yR=(double)(y-hz)/(h-hz);
                for (int x=0; x<w; x++)
                {
                    int idx=y*w+x;
                    r[idx]=Lerp3f(rH,rS,(float)yR);
                    g[idx]=Lerp3f(gH,gS,(float)yR);
                    b[idx]=Lerp3f(bH,bS,(float)yR);
                }
            }

            // 3 planos de árboles: lejos=pequeños/claros, cerca=grandes/oscuros
            double[] escPlano = { 12.0, 8.0, 5.0 };
            float[]  alturaPct= { 0.22f, 0.32f, 0.44f };
            float[]  oscuro   = { 0.55f, 0.40f, 0.25f };
            int[]    yBaseOff = { 0, (int)(h*0.05), (int)(h*0.10) };

            for (int plano=0; plano<3; plano++)
            {
                double tS  = _ctx.Semilla*0.003 + plano*7.3;
                double esc = escPlano[plano];
                int    yB  = hz + yBaseOff[plano];
                float  osc = oscuro[plano];
                float  altMax = alturaPct[plano];

                // Mezcla color de árbol segun plano
                float rT = Lerp3f(rH, rS*osc, (float)plano/2);
                float gT = Lerp3f(gH, gS*osc + 0.05f, (float)plano/2);
                float bT = Lerp3f(bH, bS*osc, (float)plano/2);

                // Calcular silueta de copas: FBM de alta frecuencia para bordes orgánicos
                int[] yTops = new int[w];
                for (int x=0; x<w; x++)
                {
                    double nx=(double)x/w;
                    // FBM rápido para la silueta base
                    double nBase = Matematica.FBM(nx*esc+tS, 1.0+plano, 4, 0.60);
                    // Detalle fino de bordes de copa
                    double nDet  = Matematica.FBM(nx*esc*3.5+tS+5, 0.5, 3, 0.55)*0.3;
                    double alt   = ((nBase+nDet)+1)*0.5;
                    yTops[x] = yB - (int)(h*altMax*alt);
                }

                // Suavizado local para que la silueta fluya (sin picos aislados)
                int vent = Math.Max(1, w/200);
                int[] ySmooth = new int[w];
                for (int x=0; x<w; x++)
                {
                    int sum=0, cnt=0;
                    for (int k=-vent; k<=vent; k++)
                    { int xx=x+k; if(xx>=0&&xx<w){sum+=yTops[xx];cnt++;} }
                    ySmooth[x]=sum/cnt;
                }

                // Pintar árbol: copa redondeada arriba, tronco oscuro abajo
                for (int x=0; x<w; x++)
                {
                    double nx=(double)x/w;
                    int yTop=ySmooth[x];
                    int yBot=Math.Min(h-1, yB+(int)(h*0.03));

                    for (int y=yTop; y<=yBot; y++)
                    {
                        if (y<0||y>=h) continue;
                        double yR=(double)(y-yTop)/Math.Max(1,yBot-yTop);
                        // Parte alta = copa, parte baja = tronco/suelo
                        float osc2 = yR > 0.7f ? osc*0.6f : osc;
                        // Variación de textura foliar
                        double tex = Matematica.Perlin(nx*esc*4+tS+2.1, (double)y/h*6)*0.12;
                        float fr2 = C1(rT*osc2+(float)tex*0.08f);
                        float fg2 = C1(gT*osc2+(float)tex*0.14f);
                        float fb2 = C1(bT*osc2+(float)tex*0.06f);

                        // Niebla atmósferia en planos lejanos
                        float atmF = plano<1 ? 0.35f : plano<2 ? 0.18f : 0f;
                        fr2=Lerp3f(fr2,rH,atmF); fg2=Lerp3f(fg2,gH,atmF); fb2=Lerp3f(fb2,bH,atmF);

                        int idx=y*w+x;
                        r[idx]=C1(fr2); g[idx]=C1(fg2); b[idx]=C1(fb2);
                    }
                }
            }
        }

        private void CapaCampoFlores(float[] r, float[] g, float[] b, int w, int h, int hz)
        {
            // 1. Pasto base — degradado verde desde horizonte
            float rg=_sueloBase.R/255f, gg=_sueloBase.G/255f, bg=_sueloBase.B/255f;
            float rH=_cieloHorizonte.R/255f, gH=_cieloHorizonte.G/255f, bH=_cieloHorizonte.B/255f;
            double escH=_ctx.Escala*3.5, tS=_ctx.Semilla*0.004;
            for (int y=hz; y<h; y++)
            {
                double yR=(double)(y-hz)/(h-hz);
                for (int x=0; x<w; x++)
                {
                    double nx=(double)x/w;
                    double n=Matematica.FBM(nx*escH+tS, yR*escH*0.6+tS, 4, 0.55);
                    double blend = Matematica.Clamp01(Math.Pow(yR,0.4) + (n+1)*0.5*0.25 - 0.1);
                    int idx=y*w+x;
                    r[idx]=Lerp3f(rH,C1(rg+(float)(n*0.04)),  (float)blend);
                    g[idx]=Lerp3f(gH,C1(gg+(float)(n*0.06)),  (float)blend);
                    b[idx]=Lerp3f(bH,C1(bg+(float)(n*0.02)),  (float)blend);
                }
            }
            FranjaHorizonte(r,g,b,w,h,hz);

            // 2. Flores — múltiples capas de colores cubriendo todo el campo
            Color[] floresPal = {
                Color.FromArgb(220,60,90),   // rojo/rosa
                Color.FromArgb(255,180,30),  // amarillo
                Color.FromArgb(140,60,200),  // violeta
                Color.FromArgb(255,255,80),  // amarillo claro
                Color.FromArgb(60,160,255),  // azul
                Color.FromArgb(255,120,160), // rosa
                Color.FromArgb(255,80,40),   // naranja
            };
            for (int pi=0; pi<Math.Min(_ctx.Paleta.Count, floresPal.Length); pi++)
                floresPal[pi]=_ctx.Paleta[pi];

            // Capas de flores con escalas distintas — crean profundidad
            double[] escFlor = { 18.0, 28.0, 42.0 };
            float[]  radFlor = { 0.012f, 0.007f, 0.004f };   // radio en fracción del alto
            float[]  alfaFlor= { 0.85f,  0.80f,  0.75f };

            for (int capa=0; capa<3; capa++)
            {
                double escF = escFlor[capa];
                double tSeed= _ctx.Semilla*0.007 + capa*3.7;
                int    radioBase = Math.Max(1, (int)(radFlor[capa]*h));

                for (int ci=0; ci<floresPal.Length; ci++)
                {
                    Color fc = floresPal[ci];
                    float fR=fc.R/255f, fG=fc.G/255f, fB=fc.B/255f;
                    double tC = tSeed + ci*1.33;

                    for (int y=hz; y<h; y++)
                    {
                        double yR=(double)(y-hz)/(h-hz);
                        // Flores más grandes y densas en primer plano
                        double densBase = 0.20 + yR*0.50;
                        int radio = Math.Max(1, (int)(radioBase*(0.4+yR*0.8)));

                        for (int x=0; x<w; x+=Math.Max(1,radio/2))
                        {
                            double nx=(double)x/w;
                            double noiseF = Matematica.Perlin(nx*escF+tC, yR*escF*0.8+tC*1.3);
                            double dens   = Matematica.Clamp01((noiseF+1)*0.5*densBase);
                            if (dens < 0.42) continue;

                            double alpha = Matematica.Clamp01((dens-0.42)*4.5)*alfaFlor[capa];

                            // Dibujar círculo de flor
                            for (int dy=-radio; dy<=radio; dy++)
                            for (int dx=-radio; dx<=radio; dx++)
                            {
                                int xx=x+dx, yy=y+dy;
                                if (xx<0||xx>=w||yy<hz||yy>=h) continue;
                                double dd=Math.Sqrt(dx*dx+dy*dy); if(dd>radio) continue;
                                double a2=alpha*(1-dd/radio*0.6);
                                int ii=yy*w+xx;
                                r[ii]=C1((float)(r[ii]*(1-a2)+fR*a2));
                                g[ii]=C1((float)(g[ii]*(1-a2)+fG*a2));
                                b[ii]=C1((float)(b[ii]*(1-a2)+fB*a2));
                            }
                        }
                    }
                }
            }
        }

        private void CapaMontanas(float[] r, float[] g, float[] b, int w, int h, int hz)
        {
            float[] planoEsc    = { 0.8f, 1.4f, 2.2f, 3.5f, 5.0f };
            float[] planoAltura = { 0.55f, 0.45f, 0.36f, 0.28f, 0.20f };
            float[] planoYOff   = { 0.08f, 0.04f, 0.0f, -0.03f, -0.06f };

            for (int pl=0; pl<5; pl++)
            {
                double tS    = _ctx.Semilla*0.005 + pl*6.7;
                double escM  = _ctx.Escala*planoEsc[pl];
                int yBase    = hz - (int)(h*planoYOff[pl]);
                float fade   = (float)pl/4.0f;   // 0=lejano 4=cercano

                float rM = Lerp3f(_cieloHorizonte.R/255f, _sueloBase.R/255f*0.80f, fade);
                float gM = Lerp3f(_cieloHorizonte.G/255f, _sueloBase.G/255f*0.75f, fade);
                float bM = Lerp3f(_cieloHorizonte.B/255f, _sueloBase.B/255f*0.85f, fade);

                float rR = rM * (0.65f + fade*0.15f);
                float gR = gM * (0.60f + fade*0.15f);
                float bR = bM * (0.55f + fade*0.15f);

                // PRIMERO: Calcular todas las alturas del plano
                int[] yTops = new int[w];
                for (int x=0; x<w; x++)
                {
                    double nx=(double)x/w;
                    double n1=Matematica.FBM(nx*escM+tS,        0.5, 6, 0.62);
                    double n2=Matematica.FBM(nx*escM*1.9+tS+3,  0.3, 4, 0.55)*0.30;
                    double n3=Matematica.FBM(nx*escM*4.1+tS+7,  0.1, 3, 0.50)*0.12;
                    double altNorm=((n1+n2+n3)+1)*0.5;
                    int altPx=(int)(h*planoAltura[pl]*altNorm);
                    yTops[x]=yBase-altPx;
                }

                // SEGUNDO: Suavizar las alturas (ventana de 3 píxeles)
                int[] ySmooth = new int[w];
                for (int x=0; x<w; x++)
                {
                    int sum=0, cnt=0;
                    for (int k=-1; k<=1; k++)
                    {
                        int xx=x+k;
                        if(xx>=0 && xx<w){sum+=yTops[xx]; cnt++;}
                    }
                    ySmooth[x]=sum/cnt;
                }

                // TERCERO: Pintar usando las alturas suavizadas
                for (int x=0; x<w; x++)
                {
                    double nx=(double)x/w;
                    int yTop=ySmooth[x];

                    double nx2=((double)x+1)/w;
                    // Pendiente lateral para sombra (comparar con pixel vecino)
                    int yTopN = x+1 < w ? ySmooth[x+1] : yTop;
                    double slope = (double)(yTop - yTopN) / Math.Max(1, h * planoAltura[pl]);
                    float sombraLat = (float)Matematica.Clamp01(-slope * 2.5) * 0.50f;

                    for (int y=yTop; y<h; y++)
                    {
                        if (y<0||y>=h) continue;
                        double yR=(double)(y-yTop)/Math.Max(1,yBase-yTop);

                        double nieveThresh = pl>=2 ? 0.12 : 0.08;
                        double nieveA = pl>=1 ? Matematica.Clamp01((nieveThresh-yR)/nieveThresh*0.95) : 0;

                        double texR = Matematica.FBM(nx*escM*6+tS+1.5, yR*4+pl, 3, 0.50)*0.18;
                        double texV = Matematica.FBM(nx*escM*8+tS+4.2, yR*6+pl, 2, 0.50)*0.10;

                        float vegA = 0;
                        if (pl>=3 && yR>0.2 && yR<0.65)
                        {
                            double vegN=Matematica.Perlin(nx*escM*12+tS+8, yR*8);
                            vegA=(float)Matematica.Clamp01((vegN+0.3)*1.5)*0.70f;
                        }
                        float rV=0.15f, gV=0.38f, bV=0.12f;

                        float fr2 = C1(rR*(1-sombraLat) + (float)texR + (float)texV*0.4f);
                        float fg2 = C1(gR*(1-sombraLat) + (float)texR*0.6f + (float)texV*0.5f);
                        float fb2 = C1(bR*(1-sombraLat) + (float)texV*0.8f);

                        fr2=Lerp3f(fr2, rV, vegA);
                        fg2=Lerp3f(fg2, gV, vegA);
                        fb2=Lerp3f(fb2, bV, vegA);

                        fr2=Lerp3f(fr2, 0.96f, (float)nieveA);
                        fg2=Lerp3f(fg2, 0.97f, (float)nieveA);
                        fb2=Lerp3f(fb2, 1.00f, (float)nieveA);

                        float atmF=(float)(pl<2 ? (2-pl)*0.35 : 0);
                        fr2=Lerp3f(fr2, _cieloHorizonte.R/255f, atmF);
                        fg2=Lerp3f(fg2, _cieloHorizonte.G/255f, atmF);
                        fb2=Lerp3f(fb2, _cieloHorizonte.B/255f, atmF);

                        int idx=y*w+x;
                        r[idx]=C1(fr2); g[idx]=C1(fg2); b[idx]=C1(fb2);
                    }
                }
            }
            FranjaHorizonte(r,g,b,w,h,hz);
        }
        
        private void CapaDesierto(float[] r, float[] g, float[] b, int w, int h, int hz)
        {
            float r0=_cieloHorizonte.R/255f, g0=_cieloHorizonte.G/255f, b0=_cieloHorizonte.B/255f;
            float r1=_sueloBase.R/255f,      g1=_sueloBase.G/255f,      b1=_sueloBase.B/255f;
            float r2=_sueloDetalle.R/255f,   g2=_sueloDetalle.G/255f,   b2=_sueloDetalle.B/255f;
            double escD=_ctx.Escala*2.0, tS=_ctx.Semilla*0.006;
            for (int y=hz;y<h;y++)
            {
                double yR=(double)(y-hz)/(h-hz);
                for (int x=0;x<w;x++)
                {
                    double nx=(double)x/w;
                    double duna1=Matematica.FBM(nx*escD+tS, yR*0.4, 5, 0.58);
                    double duna2=Matematica.FBM(nx*escD*2.5+tS+2, yR*0.8+0.5, 3, 0.52);
                    double duna=(duna1*0.7+duna2*0.3+1)*0.5;
                    double shadow=Math.Max(0, Matematica.FBM(nx*escD*1.8+tS+4, yR*0.5+0.3, 4, 0.6))*0.4;

                    double blend=Matematica.Clamp01(Math.Pow(yR,0.6)+shadow*0.3);
                    int idx=y*w+x;
                    float ri=Lerp3f(r0,r1,(float)blend)+((float)duna-0.5f)*0.08f;
                    float gi=Lerp3f(g0,g1,(float)blend)+((float)duna-0.5f)*0.06f;
                    float bi=Lerp3f(b0,b1,(float)blend);
                    ri=C1(ri-(float)shadow*0.12f); gi=C1(gi-(float)shadow*0.10f);
                    r[idx]=ri; g[idx]=gi; b[idx]=C1(bi);
                }
            }
            FranjaHorizonte(r,g,b,w,h,hz);
        }

        private void CapaCalima(float[] r, float[] g, float[] b, int w, int h, int hz)
        {
            float fR=_cieloHorizonte.R/255f*1.15f, fG=_cieloHorizonte.G/255f*1.05f, fB=_cieloHorizonte.B/255f*0.85f;
            fR=C1(fR); fG=C1(fG);
            int banda=(int)(h*0.18);
            for (int y=hz-banda/3; y<hz+banda; y++)
            {
                if (y<0||y>=h) continue;
                double yR=Math.Abs(y-hz)/(double)banda;
                double alpha=Math.Max(0,(1-yR*yR)*0.55);
                for (int x=0;x<w;x++)
                {
                    double n=Matematica.FBM((double)x/w*4+_ctx.Semilla*0.002,(double)y/h*2,2,0.5);
                    double fa=Matematica.Clamp01(alpha*(0.65+n*0.35));
                    int idx=y*w+x;
                    r[idx]=C1((float)(r[idx]*(1-fa)+fR*fa));
                    g[idx]=C1((float)(g[idx]*(1-fa)+fG*fa));
                    b[idx]=C1((float)(b[idx]*(1-fa)+fB*fa));
                }
            }
        }

        private void CapaEstrellas(float[] r, float[] g, float[] b, int w, int h, int hz)
        {
            for (int y=0;y<hz;y++) for (int x=0;x<w;x++)
            {
                double nx=(double)x/w, ny=(double)y/h;
                double via=Matematica.FBM(nx*1.8+2.3, ny*4.0+0.7, 5, 0.6)*0.5+0.5;
                double band=Math.Exp(-Math.Pow((ny-0.35)*6,2));
                double v=via*band*0.25;
                if (v<0.02) continue;
                int idx=y*w+x;
                r[idx]=C1(r[idx]+(float)v*0.7f);
                g[idx]=C1(g[idx]+(float)v*0.75f);
                b[idx]=C1(b[idx]+(float)v);
            }
            var rndS=new Random(_ctx.Semilla^0xABCD);
            int nStars=(int)(w*hz*0.0004);
            for (int i=0;i<nStars;i++)
            {
                int sx=rndS.Next(w), sy=rndS.Next(hz);
                double bright=0.5+rndS.NextDouble()*0.5;
                double size=rndS.NextDouble(); // <0.9=punto, resto=grande
                float sv=(float)bright;
                if (size<0.90) { if(sy*w+sx<w*hz) { r[sy*w+sx]=C1(r[sy*w+sx]+sv); g[sy*w+sx]=C1(g[sy*w+sx]+sv); b[sy*w+sx]=C1(b[sy*w+sx]+sv); } }
                else { // estrella con difuminado
                    for (int dy=-1;dy<=1;dy++) for (int dx=-1;dx<=1;dx++)
                    { int yy=sy+dy, xx=sx+dx; if(yy<0||yy>=hz||xx<0||xx>=w) continue;
                      float sv2=sv*(float)(1-Math.Sqrt(dx*dx+dy*dy)*0.55);
                      int ii=yy*w+xx; r[ii]=C1(r[ii]+sv2); g[ii]=C1(g[ii]+sv2); b[ii]=C1(b[ii]+sv2); }
                }
            }
        }

        private void CapaTerrenoPerspectiva(float[] r, float[] g, float[] b, int w, int h, int hz)
        {
            float rH=_cieloHorizonte.R/255f, gH=_cieloHorizonte.G/255f, bH=_cieloHorizonte.B/255f;
            double escT=_ctx.Escala*1.8, tS=_ctx.Semilla*0.004;

            double camX=0.5, camZ=0.0;
            double camHeight=0.28;
            double pitchAngle=0.15;  // inclinación hacia abajo

            for (int x=0;x<w;x++)
            {
                double rayAngle=((double)x/w-0.5)*1.3; // campo visual horizontal
                double dirX=Math.Cos(rayAngle), dirZ=Math.Sin(rayAngle)+1.5;

                for (int y=hz;y<h;y++)
                {
                    double rowAngle=pitchAngle+((double)(y-hz)/(h-hz))*0.8;
                    if (rowAngle<0.01) continue;

                    double dist=camHeight/rowAngle;
                    double tx=camX+dirX*dist*0.3;
                    double tz=camZ+dirZ*dist;

                    double terrHeight=Matematica.WarpedFBM(tx*escT+tS, tz*escT*0.5+tS, 6, 0.58, 0.4);
                    terrHeight=(terrHeight+1)*0.5;

                    float rt, gt2, bt;
                    if (terrHeight < 0.35) { rt=_sueloBase.R/255f*0.9f; gt2=_sueloBase.G/255f; bt=_sueloBase.B/255f*0.8f; }
                    else if (terrHeight < 0.65) { rt=0.45f; gt2=0.40f; bt=0.35f; }
                    else { float sn=(float)(terrHeight-0.65)/0.35f; rt=Lerp3f(0.55f,0.97f,sn); gt2=Lerp3f(0.50f,0.98f,sn); bt=Lerp3f(0.45f,1.0f,sn); }

                    double fog=Matematica.Clamp01(1-dist*0.05);
                    rt=Lerp3f(rH,rt,(float)fog); gt2=Lerp3f(gH,gt2,(float)fog); bt=Lerp3f(bH,bt,(float)fog);

                    double shadowN=Matematica.FBM(tx*escT+tS+5, tz*escT*0.5+tS+5, 3, 0.5);
                    float shadow=C1((float)(1-Math.Max(0,shadowN)*0.3));
                    int idx=y*w+x;
                    r[idx]=C1(rt*shadow); g[idx]=C1(gt2*shadow); b[idx]=C1(bt);
                }
            }
            FranjaHorizonte(r,g,b,w,h,hz);
        }

        private void CapaNieblaAtmosferica(float[] r, float[] g, float[] b, int w, int h, int hz)
        {
            float fR=_cieloHorizonte.R/255f, fG=_cieloHorizonte.G/255f, fB=_cieloHorizonte.B/255f;
            for (int y=hz;y<h;y++)
            {
                double yR=(double)(y-hz)/(h-hz);
                double fogStr=Math.Pow(1-yR,3)*0.55;
                if (fogStr<0.02) continue;
                for (int x=0;x<w;x++)
                {
                    int idx=y*w+x;
                    r[idx]=C1((float)(r[idx]*(1-fogStr)+fR*fogStr));
                    g[idx]=C1((float)(g[idx]*(1-fogStr)+fG*fogStr));
                    b[idx]=C1((float)(b[idx]*(1-fogStr)+fB*fogStr));
                }
            }
        }

        private void FranjaHorizonte(float[] r, float[] g, float[] b, int w, int h, int hz)
        {
            int mez=Math.Max(4,hz/12);
            for (int y=hz-mez;y<hz;y++) { if(y<0) continue;
                double t=(double)(y-(hz-mez))/mez; int yA=Math.Min(h-1,hz+mez);
                for (int x=0;x<w;x++) {
                    double a=t*0.40; int ic=y*w+x, ia=yA*w+x;
                    r[ic]=C1((float)(r[ic]*(1-a)+r[ia]*a));
                    g[ic]=C1((float)(g[ic]*(1-a)+g[ia]*a));
                    b[ic]=C1((float)(b[ic]*(1-a)+b[ia]*a)); } }
        }

        private static float C1(float v)       { return v<0?0:v>1?1:v; }
        private static float Lerp3f(float a, float b, float t) { return a+(b-a)*(t<0?0:t>1?1:t); }
        private static Color OscC(Color c, int p) { return Color.FromArgb(c.R*(100-p)/100, c.G*(100-p)/100, c.B*(100-p)/100); }


        private void CapaSelvaExtra(float[] r, float[] g, float[] b, int w, int h, int hz)
        {
            double esc = _ctx.Escala * 3.0;
            double ts  = _ctx.Semilla * 0.0013;
            float r0 = _sueloBase.R/255f, g0 = _sueloBase.G/255f, b0 = _sueloBase.B/255f;
            for (int y = hz; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                double nx = (double)x/w, ny = (double)(y-hz)/(h-hz);
                double n1 = Matematica.WarpedFBM(nx*esc+ts+2.1, ny*esc+0.5, 5, 0.6, 0.45);
                double n2 = Matematica.FBM(nx*esc*2+7, ny*esc*1.5+3, 4, 0.5);
                double planta = ((n1+n2)/2.0+1)*0.5;
                if (planta < 0.52) continue;
                double alpha = Matematica.Clamp01((planta-0.52)/0.30) * 0.55;
                float rv = C1(r0 * 0.7f);
                float gv = C1(g0 * 1.1f + 0.1f);
                float bv = C1(b0 * 0.8f + 0.05f);
                int idx = y*w+x;
                r[idx] = C1((float)(r[idx]*(1-alpha) + rv*alpha));
                g[idx] = C1((float)(g[idx]*(1-alpha) + gv*alpha));
                b[idx] = C1((float)(b[idx]*(1-alpha) + bv*alpha));
            }
            for (int x = 0; x < w; x++)
            {
                double nx = (double)x/w;
                double n = Matematica.Perlin(nx*15 + ts, 0.5);
                if (n < 0.2) continue;
                double lianaH = (n-0.2)/0.6 * (h-hz)*0.5;
                for (int y = hz; y < hz+(int)lianaH && y < h; y++)
                {
                    int idx = y*w+x;
                    float alpha = C1((float)((y-hz)/lianaH*0.5));
                    r[idx] = C1(r[idx]*(1-alpha) + r0*0.5f*alpha);
                    g[idx] = C1(g[idx]*(1-alpha) + g0*1.2f*alpha);
                    b[idx] = C1(b[idx]*(1-alpha) + b0*0.6f*alpha);
                }
            }
        }

        private void CapaAurora(float[] r, float[] g, float[] b, int w, int h, int hz)
        {
            double ts = _ctx.Semilla * 0.0017;
            for (int y = 0; y < hz; y++)
            {
                double yR = (double)y/hz;
                double intensidad = Math.Pow(yR, 0.7) * 0.8;
                if (intensidad < 0.02) continue;
                for (int x = 0; x < w; x++)
                {
                    double nx = (double)x/w;
                    double onda1 = Math.Sin(nx*6.28*3 + ts*0.7 + yR*4)*0.5+0.5;
                    double onda2 = Math.Sin(nx*6.28*5 + ts*1.3 + yR*3)*0.5+0.5;
                    double n = Matematica.FBM(nx*4+ts, yR*2, 3, 0.5)*0.5+0.5;
                    double aurora = n * (onda1*0.6+onda2*0.4) * intensidad;
                    aurora = Matematica.Clamp01(aurora);
                    int idx = y*w+x;
                    float ra = (float)(_colorGlow.R/255.0 * aurora * 0.5);
                    float ga = (float)(_colorSol.R/255.0 * aurora * 0.9);   // verde del sol
                    float ba = (float)(_colorGlow.B/255.0 * aurora * 0.8);
                    r[idx] = C1(r[idx] + ra);
                    g[idx] = C1(g[idx] + ga);
                    b[idx] = C1(b[idx] + ba);
                }
            }
        }

        private void CapaLava(float[] r, float[] g, float[] b, int w, int h, int hz)
        {
            double esc = _ctx.Escala * 4.0;
            double ts  = _ctx.Semilla * 0.0019;
            for (int y = hz; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                double nx = (double)x/w, ny = (double)(y-hz)/(h-hz);
                double n1 = Matematica.WarpedFBM(nx*esc+ts, ny*esc*0.7, 5, 0.55, 0.5);
                double lava = (n1+1)*0.5;
                if (lava < 0.60) continue;
                double alpha = Matematica.Clamp01((lava-0.60)/0.25) * 0.9;
                double t2 = Matematica.Clamp01((lava-0.60)/0.40);
                float rl = (float)(1.0*alpha);
                float gl = (float)((0.3+t2*0.4)*alpha);
                float bl = (float)(t2*0.05*alpha);
                int idx = y*w+x;
                r[idx] = C1((float)(r[idx]*(1-alpha) + rl));
                g[idx] = C1((float)(g[idx]*(1-alpha) + gl));
                b[idx] = C1((float)(b[idx]*(1-alpha) + bl));
            }
        }

        private void CapaCeniza(float[] r, float[] g, float[] b, int w, int h, int hz)
        {
            double esc = _ctx.Escala * 2.5;
            double ts  = _ctx.Semilla * 0.0011;
            for (int y = 0; y < hz; y++)
            {
                double yR = (double)y/hz;
                double densidad = Math.Pow(1.0-yR, 1.2) * 0.7;
                if (densidad < 0.05) continue;
                for (int x = 0; x < w; x++)
                {
                    double nx = (double)x/w;
                    double n = Matematica.WarpedFBM(nx*esc+ts+3, (1-yR)*esc*1.5, 4, 0.55, 0.35);
                    double ceniza = (n+1)*0.5 * densidad;
                    if (ceniza < 0.12) continue;
                    double alpha = Matematica.Clamp01((ceniza-0.12)/0.50) * 0.65;
                    int idx = y*w+x;
                    float rc = 0.25f, gc = 0.18f, bc = 0.14f;
                    r[idx] = C1((float)(r[idx]*(1-alpha) + rc*alpha));
                    g[idx] = C1((float)(g[idx]*(1-alpha) + gc*alpha));
                    b[idx] = C1((float)(b[idx]*(1-alpha) + bc*alpha));
                }
            }
        }

        private void CapaLago(float[] r, float[] g, float[] b, int w, int h, int hz)
        {
            float r0=_sueloBase.R/255f, g0=_sueloBase.G/255f, b0=_sueloBase.B/255f;
            float rH=_cieloHorizonte.R/255f, gH=_cieloHorizonte.G/255f, bH=_cieloHorizonte.B/255f;
            double esc=_ctx.Escala*2.0, ts=_ctx.Semilla*0.0013;

            // Orilla/prado alrededor del lago — base verde-marrón
            for (int y=hz; y<h; y++)
            for (int x=0; x<w; x++)
            {
                double nx=(double)x/w, ny=(double)(y-hz)/(h-hz);
                double n=Matematica.FBM(nx*esc*2+ts, ny*esc+ts, 3, 0.5)*0.08;
                double yR=ny;
                int idx=y*w+x;
                float rOr=Lerp3f(rH, _sueloDetalle.R/255f, (float)yR);
                float gOr=Lerp3f(gH, _sueloDetalle.G/255f, (float)yR);
                float bOr=Lerp3f(bH, _sueloDetalle.B/255f, (float)yR);
                r[idx]=C1(rOr+(float)n); g[idx]=C1(gOr+(float)n*1.2f); b[idx]=C1(bOr);
            }

            // Lago: forma elíptica irregular con borde ruidoso
            // Centro del lago en el tercio superior del suelo
            double lakeCX = 0.5, lakeCY = 0.28;   // centro en coords relativas al suelo
            double lakeRX = 0.30 + (_rnd.NextDouble()-0.5)*0.06;  // semi-eje X
            double lakeRY = 0.18 + (_rnd.NextDouble()-0.5)*0.04;  // semi-eje Y

            for (int y=hz; y<h; y++)
            for (int x=0; x<w; x++)
            {
                double nx=(double)x/w, ny=(double)(y-hz)/(h-hz);
                double dx2=(nx-lakeCX)/lakeRX, dy2=(ny-lakeCY)/lakeRY;
                // Distancia elíptica normalizada
                double distElip=Math.Sqrt(dx2*dx2+dy2*dy2);
                // Bordes orgánicos con ruido
                double borde=Matematica.FBM(nx*esc*3+ts+1.5, ny*esc*2+ts, 3, 0.5)*0.18;
                double esLago=Matematica.Clamp01(1.0-(distElip-borde));
                if (esLago < 0.02) continue;

                // Color del agua: azul profundo en el centro, más claro en bordes
                double prof=Matematica.Clamp01(1.0-distElip*0.8);
                double onda=Matematica.FBM(nx*esc*4+ts, ny*esc*3+ts*0.5, 3, 0.5)*0.04;
                float rA=Lerp3f(rH*0.9f, r0*0.35f, (float)prof);
                float gA=Lerp3f(gH*0.95f, g0*0.55f, (float)prof);
                float bA=Lerp3f(bH, C1(b0*1.15f), (float)prof);
                rA=C1(rA+(float)onda*0.5f); gA=C1(gA+(float)onda*0.6f); bA=C1(bA+(float)onda);

                float alpha=(float)Matematica.Clamp01(esLago*2.0)*0.92f;
                int idx=y*w+x;
                r[idx]=C1(r[idx]*(1-alpha)+rA*alpha);
                g[idx]=C1(g[idx]*(1-alpha)+gA*alpha);
                b[idx]=C1(b[idx]*(1-alpha)+bA*alpha);
            }

            // Vegetación en la orilla del lago (juncos/arbustos)
            var rndL=new Random(_ctx.Semilla^0x4444);
            int nJuncos=w/30;
            float rJ=_sueloBase.R/255f*0.35f, gJ=_sueloBase.G/255f*0.9f, bJ=_sueloBase.B/255f*0.25f;
            for (int j=0; j<nJuncos; j++)
            {
                double jx=rndL.NextDouble();
                double jy=lakeCY+(rndL.NextDouble()-0.5)*lakeRY*2.4;
                int jxi=(int)(jx*w), jyi=hz+(int)(jy*(h-hz));
                int altJ=8+rndL.Next(14), anchJ=3+rndL.Next(5);
                for (int dy=0; dy<altJ; dy++)
                for (int dx=-anchJ/2; dx<=anchJ/2; dx++)
                {
                    int yy=jyi-dy, xx=jxi+dx;
                    if (yy<hz||yy>=h||xx<0||xx>=w) continue;
                    float a=(float)(1-Math.Abs(dx)*2.0/anchJ)*0.7f;
                    int idx=yy*w+xx;
                    r[idx]=C1(r[idx]*(1-a)+rJ*a);
                    g[idx]=C1(g[idx]*(1-a)+gJ*a);
                    b[idx]=C1(b[idx]*(1-a)+bJ*a);
                }
            }
        }

        private void CapaLucesGas(float[] r, float[] g, float[] b, int w, int h, int hz)
        {
            var rndL = new Random(_ctx.Semilla ^ 0xABCD);
            int nLuces = 8 + (int)(_ctx.Intensidad * 5);
            for (int k = 0; k < nLuces; k++)
            {
                double lx = rndL.NextDouble();
                double ly = 0.05 + rndL.NextDouble() * 0.6;  // en el suelo/pantano
                double lr = 0.015 + rndL.NextDouble() * 0.04;
                float[] colGas = {
                    (float)(0.3 + rndL.NextDouble()*0.5),
                    (float)(0.8 + rndL.NextDouble()*0.2),
                    (float)(0.2 + rndL.NextDouble()*0.4)
                };
                int yBase = hz + (int)(ly*(h-hz));
                int xBase = (int)(lx*w);
                int radioP = (int)(lr*Math.Min(w,h));
                for (int dy = -radioP*4; dy <= radioP*4; dy++)
                for (int dx = -radioP*4; dx <= radioP*4; dx++)
                {
                    int y = yBase+dy, x = xBase+dx;
                    if (y<hz||y>=h||x<0||x>=w) continue;
                    double d = Math.Sqrt(dx*dx+dy*dy);
                    double glow = Math.Pow(Math.Max(0, 1.0 - d/(radioP*4)), 2.5) * 0.55;
                    if (glow < 0.01) continue;
                    int idx = y*w+x;
                    r[idx] = C1((float)(r[idx]*(1-glow) + colGas[0]*glow));
                    g[idx] = C1((float)(g[idx]*(1-glow) + colGas[1]*glow));
                    b[idx] = C1((float)(b[idx]*(1-glow) + colGas[2]*glow));
                }
            }
        }

        private void CapaRio(float[] r, float[] g, float[] b, int w, int h, int hz)
        {
            double ts = _ctx.Semilla * 0.002;
            float rA = _sueloBase.R/255f*0.4f;
            float gA = _sueloBase.G/255f*0.6f;
            float bA = _sueloBase.B/255f*1.2f;
            for (int y = hz; y < h; y++)
            {
                double ny = (double)(y-hz)/(h-hz);
                double cx = 0.5 + Math.Sin(ny*3.5+ts)*0.2 + Matematica.FBM(ny*4+ts+5, 0.5, 3, 0.5)*0.1;
                double anchoRio = 0.04 + ny*0.06;
                for (int x = 0; x < w; x++)
                {
                    double nx = (double)x/w;
                    double dist = Math.Abs(nx-cx)/anchoRio;
                    if (dist > 1.2) continue;
                    double alpha = Matematica.Clamp01(1.0-dist) * 0.85;
                    int idx = y*w+x;
                    r[idx] = C1((float)(r[idx]*(1-alpha) + rA*alpha));
                    g[idx] = C1((float)(g[idx]*(1-alpha) + gA*alpha));
                    b[idx] = C1((float)(b[idx]*(1-alpha) + bA*alpha));
                }
            }
        }

        private void CapaArena(float[] r, float[] g, float[] b, int w, int h, int hz)
        {
            double esc = _ctx.Escala*3.0, ts = _ctx.Semilla*0.0017;
            float rS = 0.86f, gS = 0.78f, bS = 0.54f;   // color arena
            int limArena = hz + (h-hz)/3;
            for (int y = hz; y < limArena && y < h; y++)
            for (int x = 0; x < w; x++)
            {
                double nx=(double)x/w, ny=(double)(y-hz)/(limArena-hz);
                double n = Matematica.FBM(nx*esc+ts, ny*esc*0.5, 3, 0.5);
                double alpha = Math.Pow(ny, 0.4) * 0.90;
                alpha += n*0.08;
                alpha = Matematica.Clamp01(alpha);
                int idx = y*w+x;
                r[idx] = C1((float)(r[idx]*(1-alpha) + rS*alpha));
                g[idx] = C1((float)(g[idx]*(1-alpha) + gS*alpha));
                b[idx] = C1((float)(b[idx]*(1-alpha) + bS*alpha));
            }
        }

        private void CapaCanon(float[] r, float[] g, float[] b, int w, int h, int hz)
        {
            double esc = _ctx.Escala*2.0, ts = _ctx.Semilla*0.0013;
            float rC=_sueloBase.R/255f, gC=_sueloBase.G/255f, bC=_sueloBase.B/255f;
            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                double nx=(double)x/w, ny=(double)y/h;
                double izq = nx * 2.5;         // distancia desde izquierda
                double der = (1-nx) * 2.5;     // distancia desde derecha
                double pared = Math.Min(izq, der);
                double apertura = 0.5 + ny*0.4 + Matematica.FBM(ny*esc+ts, nx*esc*0.3, 3, 0.5)*0.15;
                double alphaP = Matematica.Clamp01(1.0 - pared/apertura);
                if (alphaP < 0.02) continue;
                double estrato = Math.Sin(ny*30*_ctx.Complejidad)*0.5+0.5;
                double n = Matematica.FBM(nx*esc+ts, ny*esc, 4, 0.5)*0.3;
                double tonoR = rC*(0.8+estrato*0.4+n);
                double tonoG = gC*(0.6+estrato*0.2+n*0.5);
                double tonoB = bC*(0.4+estrato*0.1+n*0.3);
                int idx = y*w+x;
                r[idx] = C1((float)(r[idx]*(1-alphaP) + tonoR*alphaP));
                g[idx] = C1((float)(g[idx]*(1-alphaP) + tonoG*alphaP));
                b[idx] = C1((float)(b[idx]*(1-alphaP) + tonoB*alphaP));
            }
            float rG = _colorGlow.R/255f, gG = _colorGlow.G/255f, bG = _colorGlow.B/255f;
            for (int y = 0; y < hz/2 && y < h; y++)
            for (int x = 0; x < w; x++)
            {
                double nx=(double)x/w, ny=(double)y/h;
                double golpe = Math.Abs(nx-0.8);
                double alpha2 = Matematica.Clamp01(1.0-golpe*8) * (1.0-ny/0.5) * 0.35;
                if (alpha2 < 0.01) continue;
                int idx = y*w+x;
                r[idx] = C1((float)(r[idx]*(1-alpha2) + rG*alpha2));
                g[idx] = C1((float)(g[idx]*(1-alpha2) + gG*alpha2));
                b[idx] = C1((float)(b[idx]*(1-alpha2) + bG*alpha2));
            }
        }
    }
}
