using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace OpenCIP
{
    public class GeneradorMundoVoxel
    {
        private ContextoVisual _ctx;
        private Random _rnd;
        private int[,] _alturaMapa;
        private int[,] _tipoBloque;
        private int _chunksX = 32, _chunksZ = 32;

        private Color _colorAgua      = Color.FromArgb(64,164,255);
        private Color _colorArena     = Color.FromArgb(219,211,160);
        private Color _colorTierra    = Color.FromArgb(134,96,67);
        private Color _colorPiedra    = Color.FromArgb(128,128,128);
        private Color _colorNieve     = Color.FromArgb(255,255,255);
        private Color _colorPasto     = Color.FromArgb(92,164,68);
        private Color _colorMadera    = Color.FromArgb(184,148,95);
        private Color _colorHoja      = Color.FromArgb(50,110,40);
        private Color _colorCactus    = Color.FromArgb(20,140,20);
        private Color _colorObsidiana = Color.FromArgb(15,11,22);
        private Color _colorDiamante  = Color.FromArgb(100,219,237);

        public GeneradorMundoVoxel(ContextoVisual ctx)
        {
            _ctx = ctx;
            _rnd = new Random(ctx.Semilla);
        }

        public Bitmap Generar(int anchoImg, int altoImg)
        {
            Bitmap bmp = new Bitmap(anchoImg, altoImg, PixelFormat.Format24bppRgb);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.FromArgb(135,206,235));
                GenerarMapaAltura();
                DibujarMundoVoxel(g, anchoImg, altoImg);
                DibujarNubes(g, anchoImg, altoImg);
                DibujarSol(g, anchoImg, altoImg);
            }
            return bmp;
        }

        private void GenerarMapaAltura()
        {
            _alturaMapa = new int[_chunksX, _chunksZ];
            _tipoBloque = new int[_chunksX, _chunksZ];

            for (int x = 0; x < _chunksX; x++)
            for (int z = 0; z < _chunksZ; z++)
            {
                double nx = x*0.3*_ctx.Escala;
                double nz = z*0.3*_ctx.Escala;
                double alt = Matematica.FBM(nx, nz, 4, 0.5);
                int aN = (int)((alt+1)*4*_ctx.Intensidad)+2;
                _alturaMapa[x,z] = Math.Max(1,Math.Min(12,aN));
                if      (aN < 3)  _tipoBloque[x,z] = 0;
                else if (aN < 5)  _tipoBloque[x,z] = 1;
                else if (aN < 8)  _tipoBloque[x,z] = 2;
                else if (aN < 11) _tipoBloque[x,z] = 3;
                else              _tipoBloque[x,z] = 4;
            }

            for (int x = 1; x < _chunksX-1; x++)
            for (int z = 1; z < _chunksZ-1; z++)
            {
                if (_tipoBloque[x,z]==2 && _rnd.NextDouble()<0.15) _tipoBloque[x,z]=10;
                else if (_tipoBloque[x,z]==1 && _rnd.NextDouble()<0.05) _tipoBloque[x,z]=11;
                else if (_tipoBloque[x,z]==3 && _rnd.NextDouble()<0.02) _tipoBloque[x,z]=12;
                else if (_tipoBloque[x,z]==2 && _rnd.NextDouble()<0.01) _tipoBloque[x,z]=13;
            }
        }

        private void DibujarMundoVoxel(Graphics g, int anchoImg, int altoImg)
        {
            int tW=32, tH=16;
            int offX = anchoImg/2-(8*tW);
            int offY = altoImg/2;

            for (int z = 0; z < _chunksZ; z++)
            for (int x = _chunksX-1; x >= 0; x--)
            {
                int isoX = offX+(x-z)*tW/2;
                int isoY = offY+(x+z)*tH/2;
                int alt  = _alturaMapa[x,z];
                int tipo = _tipoBloque[x,z];

                for (int h = 0; h < alt; h++)
                {
                    Color cb = ObtenerColorBloque(tipo, h, alt);
                    DibujarBloque(g, isoX, isoY-h*tH, tW, tH, cb);
                }

                if (alt < 3)
                for (int h = alt; h < 3; h++)
                    DibujarBloqueAlfa(g, isoX, isoY-h*tH, tW, tH, _colorAgua, 180);

                if (tipo==10) DibujarArbol(g, isoX, isoY-alt*tH, tW, tH);
                if (tipo==11) DibujarCactus(g, isoX, isoY-alt*tH, tW, tH);
                if (tipo==12) DibujarBloque(g, isoX, isoY-alt*tH, tW, tH, _colorObsidiana);
                if (tipo==13) DibujarBloque(g, isoX, isoY-alt*tH, tW, tH, _colorDiamante);
            }
        }

        private Color ObtenerColorBloque(int tipo, int nivel, int alto)
        {
            bool esSuper = (nivel==alto-1);
            if (!esSuper) return nivel>alto-4 ? _colorTierra : _colorPiedra;
            switch (tipo)
            {
                case 0: case 1: return _colorArena;
                case 2: return _colorPasto;
                case 3: return _colorPiedra;
                case 4: return _colorNieve;
                default: return _colorTierra;
            }
        }

        private void DibujarBloque(Graphics g, int x, int y, int w, int h, Color color)
        {
            Point[] sup = { new Point(x,y-h/2), new Point(x+w/2,y-h), new Point(x+w,y-h/2), new Point(x+w/2,y) };
            Point[] izq = { new Point(x,y-h/2), new Point(x+w/2,y), new Point(x+w/2,y+h), new Point(x,y+h/2) };
            Point[] der = { new Point(x+w/2,y), new Point(x+w,y-h/2), new Point(x+w,y+h/2), new Point(x+w/2,y+h) };

            Color cs = Aclarar(color,20), ci = Oscurecer(color,20), cd = Oscurecer(color,40);
            using (Brush bS=new SolidBrush(cs)) g.FillPolygon(bS, sup);
            using (Brush bI=new SolidBrush(ci)) g.FillPolygon(bI, izq);
            using (Brush bD=new SolidBrush(cd)) g.FillPolygon(bD, der);
            using (Pen pen=new Pen(Oscurecer(color,60),1))
            { g.DrawPolygon(pen,sup); g.DrawPolygon(pen,izq); g.DrawPolygon(pen,der); }
        }

        private void DibujarBloqueAlfa(Graphics g, int x, int y, int w, int h, Color color, int alfa)
        {
            Point[] sup = { new Point(x,y-h/2), new Point(x+w/2,y-h), new Point(x+w,y-h/2), new Point(x+w/2,y) };
            Point[] izq = { new Point(x,y-h/2), new Point(x+w/2,y), new Point(x+w/2,y+h), new Point(x,y+h/2) };
            Point[] der = { new Point(x+w/2,y), new Point(x+w,y-h/2), new Point(x+w,y+h/2), new Point(x+w/2,y+h) };
            using (Brush bS=new SolidBrush(Color.FromArgb(alfa,Aclarar(color,20)))) g.FillPolygon(bS,sup);
            using (Brush bI=new SolidBrush(Color.FromArgb(alfa,Oscurecer(color,20)))) g.FillPolygon(bI,izq);
            using (Brush bD=new SolidBrush(Color.FromArgb(alfa,Oscurecer(color,40)))) g.FillPolygon(bD,der);
        }

        private void DibujarArbol(Graphics g, int x, int y, int w, int h)
        {
            for (int i = 0; i < 2; i++) DibujarBloque(g, x, y-i*h, w, h, _colorMadera);
            int cy = y-2*h;
            DibujarBloque(g,x,cy,w,h,_colorHoja);
            DibujarBloque(g,x-w/2,cy-h/2,w,h,_colorHoja);
            DibujarBloque(g,x+w/2,cy-h/2,w,h,_colorHoja);
            DibujarBloque(g,x,cy-h,w,h,_colorHoja);
        }

        private void DibujarCactus(Graphics g, int x, int y, int w, int h)
        {
            int alt = 2+_rnd.Next(2);
            for (int i = 0; i < alt; i++) DibujarBloque(g, x, y-i*h, w, h, _colorCactus);
        }

        private void DibujarNubes(Graphics g, int anchoImg, int altoImg)
        {
            int n = (int)(5*_ctx.Intensidad);
            for (int i = 0; i < n; i++)
            {
                int nx=_rnd.Next(anchoImg), ny=_rnd.Next(altoImg/3), t=30+_rnd.Next(50);
                using (Brush b=new SolidBrush(Color.FromArgb(200,255,255,255)))
                {
                    g.FillEllipse(b,nx,ny,t,t/2);
                    g.FillEllipse(b,nx+t/3,ny-10,(int)(t*0.8),(int)(t*0.4));
                    g.FillEllipse(b,nx-t/4,ny+5,(int)(t*0.6),(int)(t*0.3));
                }
            }
        }

        private void DibujarSol(Graphics g, int anchoImg, int altoImg)
        {
            int sx=anchoImg-100, sy=50;
            for (int r = 40; r > 0; r-=2)
            {
                int rojo=255-(40-r)*3;
                using (Brush b=new SolidBrush(Color.FromArgb(255,255,Math.Max(0,rojo),0)))
                    g.FillEllipse(b,sx-r,sy-r,r*2,r*2);
            }
        }

        private Color Aclarar(Color c, int pct)
        {
            return Color.FromArgb(
                Math.Min(255,c.R+(255-c.R)*pct/100),
                Math.Min(255,c.G+(255-c.G)*pct/100),
                Math.Min(255,c.B+(255-c.B)*pct/100));
        }
        private Color Oscurecer(Color c, int pct)
        {
            return Color.FromArgb(
                c.R*(100-pct)/100,
                c.G*(100-pct)/100,
                c.B*(100-pct)/100);
        }
    }
}
