using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace OpenCIP
{
    public static class PostProcesadorIA
    {
        public static Bitmap Aplicar(Bitmap src, ContextoVisual ctx)
        {
            if (src == null) return src;
            bool esNeon   = ctx.Saturacion > 1.5;
            bool esSuave  = ctx.ModoSuave || ctx.Saturacion < 0.7;
            bool esCaos   = ctx.ModoCaos;

            if (esNeon)    return AplicarGlowNeon(src, ctx);
            if (esSuave)   return AplicarSuavizado(src, 2);
            if (esCaos)    return AplicarEdgeGlow(src, ctx);
            Bitmap tmp = AplicarSuavizado(src, 1);
            return AplicarContraste(tmp, 1.18);
        }

        public static Bitmap AplicarSuavizado(Bitmap src, int radio)
        {
            int w = src.Width, h = src.Height;
            Bitmap dst = new Bitmap(w, h, PixelFormat.Format24bppRgb);
            BitmapData sdSrc = src.LockBits(new Rectangle(0,0,w,h), ImageLockMode.ReadOnly,  PixelFormat.Format24bppRgb);
            BitmapData sdDst = dst.LockBits(new Rectangle(0,0,w,h), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            int stride = Math.Abs(sdSrc.Stride);
            byte[] pSrc = new byte[h*stride]; byte[] pDst = new byte[h*stride];
            Marshal.Copy(sdSrc.Scan0, pSrc, 0, pSrc.Length);
            int tam = radio*2+1; float norm = 1.0f/(tam*tam);
            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                float sr=0,sg=0,sb=0;
                for (int dy = -radio; dy <= radio; dy++)
                for (int dx = -radio; dx <= radio; dx++)
                {
                    int yy = Math.Max(0,Math.Min(h-1,y+dy));
                    int xx = Math.Max(0,Math.Min(w-1,x+dx));
                    int oo = yy*stride+xx*3;
                    sb += pSrc[oo]; sg += pSrc[oo+1]; sr += pSrc[oo+2];
                }
                int o = y*stride+x*3;
                pDst[o]=(byte)Math.Min(255,sb*norm); pDst[o+1]=(byte)Math.Min(255,sg*norm); pDst[o+2]=(byte)Math.Min(255,sr*norm);
            }
            Marshal.Copy(pDst, 0, sdDst.Scan0, pDst.Length);
            src.UnlockBits(sdSrc); dst.UnlockBits(sdDst);
            return dst;
        }

        private static Bitmap AplicarGlowNeon(Bitmap src, ContextoVisual ctx)
        {
            int w = src.Width, h = src.Height;
            Bitmap suave = AplicarSuavizado(src, 1);
            BitmapData sdS = suave.LockBits(new Rectangle(0,0,w,h), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            int stride = Math.Abs(sdS.Stride);
            byte[] pS = new byte[h*stride];
            Marshal.Copy(sdS.Scan0, pS, 0, pS.Length); suave.UnlockBits(sdS);

            Bitmap dst = new Bitmap(w, h, PixelFormat.Format24bppRgb);
            BitmapData sdD = dst.LockBits(new Rectangle(0,0,w,h), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            byte[] pD = new byte[h*stride];

            for (int y = 1; y < h-1; y++)
            for (int x = 1; x < w-1; x++)
            {
                int o = y*stride+x*3;
                float[] lum = new float[9]; int idx=0;
                for (int dy=-1;dy<=1;dy++) for (int dx=-1;dx<=1;dx++){
                    int oo=(y+dy)*stride+(x+dx)*3;
                    lum[idx++]=(pS[oo]*0.114f+pS[oo+1]*0.587f+pS[oo+2]*0.299f)/255f;
                }
                float gx=-lum[0]+lum[2]-2*lum[3]+2*lum[5]-lum[6]+lum[8];
                float gy=-lum[0]-2*lum[1]-lum[2]+lum[6]+2*lum[7]+lum[8];
                float mag=(float)Math.Min(1.0, Math.Sqrt(gx*gx+gy*gy)*2.5);
                float b_=pS[o]/255f, g_=pS[o+1]/255f, r_=pS[o+2]/255f;
                double hue=((double)x*360/w+(double)y*180/h+ctx.Semilla*0.01)%360;
                Color gc=Matematica.ColorHSV(hue,1.0,1.0);
                float gr=gc.R/255f, gg2=gc.G/255f, gb=gc.B/255f;
                float inv=1f-mag;
                pD[o]  =(byte)Math.Min(255,(b_*inv+gb*mag)*255);
                pD[o+1]=(byte)Math.Min(255,(g_*inv+gg2*mag)*255);
                pD[o+2]=(byte)Math.Min(255,(r_*inv+gr*mag)*255);
            }
            Marshal.Copy(pD, 0, sdD.Scan0, pD.Length); dst.UnlockBits(sdD);
            return dst;
        }

        private static Bitmap AplicarEdgeGlow(Bitmap src, ContextoVisual ctx)
        {
            int w = src.Width, h = src.Height;
            Bitmap suave = AplicarSuavizado(src, 1);
            BitmapData sdO = src.LockBits(new Rectangle(0,0,w,h), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            BitmapData sdS = suave.LockBits(new Rectangle(0,0,w,h), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            int stride = Math.Abs(sdO.Stride);
            byte[] pO=new byte[h*stride]; byte[] pSm=new byte[h*stride];
            Marshal.Copy(sdO.Scan0,pO,0,pO.Length); Marshal.Copy(sdS.Scan0,pSm,0,pSm.Length);
            src.UnlockBits(sdO); suave.UnlockBits(sdS);
            Bitmap dst = new Bitmap(w,h,PixelFormat.Format24bppRgb);
            BitmapData sdD = dst.LockBits(new Rectangle(0,0,w,h), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            byte[] pD=new byte[h*stride];
            Color eColor = ctx.Paleta.Count>0 ? ctx.Paleta[ctx.Paleta.Count-1] : Color.White;
            float er=eColor.R/255f, eg2=eColor.G/255f, eb=eColor.B/255f;
            for (int y=0;y<h;y++) for (int x=0;x<w;x++){
                int o=y*stride+x*3;
                float diff=(Math.Abs(pO[o]-pSm[o])+Math.Abs(pO[o+1]-pSm[o+1])+Math.Abs(pO[o+2]-pSm[o+2]))/384f;
                float edge=Math.Min(1f,diff*3f); float inv=1f-edge;
                pD[o]  =(byte)(pO[o]*inv  +eb*255*edge);
                pD[o+1]=(byte)(pO[o+1]*inv+eg2*255*edge);
                pD[o+2]=(byte)(pO[o+2]*inv+er*255*edge);
            }
            Marshal.Copy(pD,0,sdD.Scan0,pD.Length); dst.UnlockBits(sdD);
            return dst;
        }

        private static Bitmap AplicarContraste(Bitmap src, double factor)
        {
            int w=src.Width, h=src.Height;
            Bitmap dst = new Bitmap(w,h,PixelFormat.Format24bppRgb);
            BitmapData sdSrc=src.LockBits(new Rectangle(0,0,w,h),ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            BitmapData sdDst=dst.LockBits(new Rectangle(0,0,w,h),ImageLockMode.WriteOnly,PixelFormat.Format24bppRgb);
            int stride=Math.Abs(sdSrc.Stride);
            byte[] pS=new byte[h*stride]; byte[] pD=new byte[h*stride];
            Marshal.Copy(sdSrc.Scan0,pS,0,pS.Length);
            for (int i=0;i<pS.Length;i++) pD[i]=(byte)Math.Max(0,Math.Min(255,(int)(128+(pS[i]-128)*factor)));
            Marshal.Copy(pD,0,sdDst.Scan0,pD.Length);
            src.UnlockBits(sdSrc); dst.UnlockBits(sdDst);
            return dst;
        }
    }
}
