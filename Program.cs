// ============================================================
//  OpenCIP – Open CPU Image Painter  +  MUNDO VOXEL MINECRAFT
//  Monolítico: todo en Program.cs  |  Target: .NET 4.x / C# 5.0
//  MEJORADO: Todos los algoritmos optimizados
// ============================================================
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OpenCIP
{
    // ═══════════════════════════════════════════════════
    //  ENUMERACIONES Y TIPOS BASE
    // ═══════════════════════════════════════════════════

    public enum AlgoritmoBase
    {
        RuidoPerlin,
        FractalMandelbrot,
        FluidoTurbulento,
        GeometricoSimetrico,
        VoronoiCelular,
        OndaInterferencia,
        NebulosaEspacial,
        PlasmaCaos,
        MundoVoxelMinecraft,
        RuidoSimplex,           // NUEVO
        DominioWarping,         // NUEVO
        Multifractal,           // NUEVO
        TurbulenciaRizos,       // NUEVO
        PatronesCelulares,      // NUEVO
        RayMarching2D,          // NUEVO
        SuperficiesImplicitas   // NUEVO
    }

    public enum ModoFusion
    {
        Normal, Multiplicar, Pantalla, Superponer, Diferencia, Luminosidad
    }

    // ═══════════════════════════════════════════════════
    //  CONTEXTO VISUAL
    // ═══════════════════════════════════════════════════

    public class ContextoVisual
    {
        public List<AlgoritmoBase> Algoritmos { get; set; }
        public List<float> PesosAlgoritmos { get; set; }
        public List<Color> Paleta { get; set; }
        public double Escala { get; set; }
        public double Intensidad { get; set; }
        public double Complejidad { get; set; }
        public int Semilla { get; set; }
        public bool ModoCaos { get; set; }
        public bool ModoSuave { get; set; }
        public bool ModoOscuro { get; set; }
        public bool ModoRetro { get; set; }
        public bool ModoSimetrico { get; set; }
        public int Iteraciones { get; set; }
        public double Saturacion { get; set; }
        public List<string> PalabrasDetectadas { get; set; }
        public string ResumenVisual { get; set; }
        public double TiempoAnimacion { get; set; } // Para efectos temporales

        public ContextoVisual()
        {
            Algoritmos = new List<AlgoritmoBase>();
            PesosAlgoritmos = new List<float>();
            Paleta = new List<Color>();
            Escala = 1.0;
            Intensidad = 1.0;
            Complejidad = 1.0;
            Semilla = new Random().Next();
            ModoCaos = false;
            ModoSuave = false;
            ModoOscuro = false;
            ModoRetro = false;
            ModoSimetrico = false;
            Iteraciones = 150;
            Saturacion = 1.0;
            PalabrasDetectadas = new List<string>();
            ResumenVisual = "";
            TiempoAnimacion = 0.0;
        }
    }

    // ═══════════════════════════════════════════════════
    //  BANCO DE PALABRAS CLAVE (ES + EN)  +  MINECRAFT
    // ═══════════════════════════════════════════════════

    public static class BancoPalabras
    {
        public static readonly Dictionary<string, Color[]> MapaColores = new Dictionary<string, Color[]>(StringComparer.OrdinalIgnoreCase)
        {
            {"rojo",    new[]{ Color.DarkRed,       Color.Red,          Color.OrangeRed  }},
            {"red",     new[]{ Color.DarkRed,       Color.Red,          Color.OrangeRed  }},
            {"azul",    new[]{ Color.DarkBlue,      Color.RoyalBlue,    Color.CornflowerBlue }},
            {"blue",    new[]{ Color.DarkBlue,      Color.RoyalBlue,    Color.CornflowerBlue }},
            {"verde",   new[]{ Color.DarkGreen,     Color.LimeGreen,    Color.MediumSeaGreen }},
            {"green",   new[]{ Color.DarkGreen,     Color.LimeGreen,    Color.MediumSeaGreen }},
            {"amarillo",new[]{ Color.DarkGoldenrod, Color.Gold,         Color.Yellow     }},
            {"yellow",  new[]{ Color.DarkGoldenrod, Color.Gold,         Color.Yellow     }},
            {"naranja", new[]{ Color.DarkOrange,    Color.Orange,       Color.Coral      }},
            {"orange",  new[]{ Color.DarkOrange,    Color.Orange,       Color.Coral      }},
            {"morado",  new[]{ Color.Indigo,        Color.DarkViolet,   Color.MediumPurple}},
            {"purple",  new[]{ Color.Indigo,        Color.DarkViolet,   Color.MediumPurple}},
            {"violeta", new[]{ Color.DarkViolet,    Color.BlueViolet,   Color.Violet     }},
            {"violet",  new[]{ Color.DarkViolet,    Color.BlueViolet,   Color.Violet     }},
            {"rosa",    new[]{ Color.HotPink,       Color.DeepPink,     Color.LightPink  }},
            {"pink",    new[]{ Color.HotPink,       Color.DeepPink,     Color.LightPink  }},
            {"blanco",  new[]{ Color.White,         Color.WhiteSmoke,   Color.GhostWhite }},
            {"white",   new[]{ Color.White,         Color.WhiteSmoke,   Color.GhostWhite }},
            {"negro",   new[]{ Color.Black,         Color.FromArgb(15,15,15), Color.FromArgb(30,30,30) }},
            {"black",   new[]{ Color.Black,         Color.FromArgb(15,15,15), Color.FromArgb(30,30,30) }},
            {"dorado",  new[]{ Color.Goldenrod,     Color.Gold,         Color.DarkGoldenrod }},
            {"gold",    new[]{ Color.Goldenrod,     Color.Gold,         Color.DarkGoldenrod }},
            {"cian",    new[]{ Color.DarkCyan,      Color.Cyan,         Color.LightCyan  }},
            {"cyan",    new[]{ Color.DarkCyan,      Color.Cyan,         Color.LightCyan  }},
            {"turquesa",new[]{ Color.Teal,          Color.Turquoise,    Color.LightSeaGreen}},
            {"turquoise",new[]{ Color.Teal,         Color.Turquoise,    Color.LightSeaGreen}},
            {"plateado",new[]{ Color.Silver,        Color.LightGray,    Color.DarkGray   }},
            {"silver",  new[]{ Color.Silver,        Color.LightGray,    Color.DarkGray   }},
            {"marron",  new[]{ Color.SaddleBrown,   Color.Peru,         Color.BurlyWood  }},
            {"brown",   new[]{ Color.SaddleBrown,   Color.Peru,         Color.BurlyWood  }},
            {"gris",    new[]{ Color.DimGray,       Color.Gray,         Color.LightGray  }},
            {"gray",    new[]{ Color.DimGray,       Color.Gray,         Color.LightGray  }},
            {"lima",    new[]{ Color.Lime,          Color.LimeGreen,    Color.YellowGreen}},
            {"lime",    new[]{ Color.Lime,          Color.LimeGreen,    Color.YellowGreen}},
            {"indigo",  new[]{ Color.Indigo,        Color.MidnightBlue, Color.Navy       }},
            {"coral",   new[]{ Color.Coral,         Color.LightCoral,   Color.Salmon     }},
            {"esmeralda",new[]{ Color.DarkGreen,    Color.SeaGreen,     Color.MediumSeaGreen}},
            {"emerald", new[]{ Color.DarkGreen,     Color.SeaGreen,     Color.MediumSeaGreen}},
        };

        public static readonly Dictionary<string, Action<ContextoVisual>> MapaTemas = new Dictionary<string, Action<ContextoVisual>>(StringComparer.OrdinalIgnoreCase)
        {
            // Naturaleza
            {"bosque", ctx => {
                AgregarAlgo(ctx, AlgoritmoBase.RuidoPerlin, 0.4f);
                AgregarAlgo(ctx, AlgoritmoBase.VoronoiCelular, 0.3f);
                AgregarAlgo(ctx, AlgoritmoBase.Multifractal, 0.3f);
                ctx.Paleta.AddRange(new[]{Color.DarkGreen, Color.ForestGreen, Color.SaddleBrown, Color.LightYellow});
                ctx.ModoSuave = true; ctx.Escala = 2.5; ctx.ResumenVisual += "bosque ";
            }},
            {"forest", ctx => {
                AgregarAlgo(ctx, AlgoritmoBase.RuidoPerlin, 0.4f);
                AgregarAlgo(ctx, AlgoritmoBase.VoronoiCelular, 0.3f);
                AgregarAlgo(ctx, AlgoritmoBase.Multifractal, 0.3f);
                ctx.Paleta.AddRange(new[]{Color.DarkGreen, Color.ForestGreen, Color.SaddleBrown, Color.LightYellow});
                ctx.ModoSuave = true; ctx.Escala = 2.5; ctx.ResumenVisual += "forest ";
            }},
            {"ocean", ctx => {
                AgregarAlgo(ctx, AlgoritmoBase.OndaInterferencia, 0.3f);
                AgregarAlgo(ctx, AlgoritmoBase.FluidoTurbulento, 0.4f);
                AgregarAlgo(ctx, AlgoritmoBase.TurbulenciaRizos, 0.3f);
                ctx.Paleta.AddRange(new[]{Color.Navy, Color.RoyalBlue, Color.DeepSkyBlue, Color.Aquamarine, Color.White});
                ctx.ModoSuave = true; ctx.Escala = 1.8; ctx.ResumenVisual += "oceano ";
            }},
            {"oceano", ctx => {
                AgregarAlgo(ctx, AlgoritmoBase.OndaInterferencia, 0.3f);
                AgregarAlgo(ctx, AlgoritmoBase.FluidoTurbulento, 0.4f);
                AgregarAlgo(ctx, AlgoritmoBase.TurbulenciaRizos, 0.3f);
                ctx.Paleta.AddRange(new[]{Color.Navy, Color.RoyalBlue, Color.DeepSkyBlue, Color.Aquamarine, Color.White});
                ctx.ModoSuave = true; ctx.Escala = 1.8; ctx.ResumenVisual += "oceano ";
            }},
            {"mar", ctx => {
                AgregarAlgo(ctx, AlgoritmoBase.OndaInterferencia, 0.4f);
                AgregarAlgo(ctx, AlgoritmoBase.FluidoTurbulento, 0.3f);
                AgregarAlgo(ctx, AlgoritmoBase.DominioWarping, 0.3f);
                ctx.Paleta.AddRange(new[]{Color.Navy, Color.SteelBlue, Color.CadetBlue, Color.White});
                ctx.ModoSuave = true; ctx.Escala = 2.0; ctx.ResumenVisual += "mar ";
            }},
            {"sea", ctx => {
                AgregarAlgo(ctx, AlgoritmoBase.OndaInterferencia, 0.4f);
                AgregarAlgo(ctx, AlgoritmoBase.FluidoTurbulento, 0.3f);
                AgregarAlgo(ctx, AlgoritmoBase.DominioWarping, 0.3f);
                ctx.Paleta.AddRange(new[]{Color.Navy, Color.SteelBlue, Color.CadetBlue, Color.White});
                ctx.ModoSuave = true; ctx.Escala = 2.0; ctx.ResumenVisual += "sea ";
            }},
            {"fuego", ctx => {
                AgregarAlgo(ctx, AlgoritmoBase.FluidoTurbulento, 0.4f);
                AgregarAlgo(ctx, AlgoritmoBase.PlasmaCaos, 0.3f);
                AgregarAlgo(ctx, AlgoritmoBase.TurbulenciaRizos, 0.3f);
                ctx.Paleta.AddRange(new[]{Color.DarkRed, Color.OrangeRed, Color.Orange, Color.Yellow, Color.White});
                ctx.ModoCaos = true; ctx.Intensidad = 1.4; ctx.ResumenVisual += "fuego ";
            }},
            {"fire", ctx => {
                AgregarAlgo(ctx, AlgoritmoBase.FluidoTurbulento, 0.4f);
                AgregarAlgo(ctx, AlgoritmoBase.PlasmaCaos, 0.3f);
                AgregarAlgo(ctx, AlgoritmoBase.TurbulenciaRizos, 0.3f);
                ctx.Paleta.AddRange(new[]{Color.DarkRed, Color.OrangeRed, Color.Orange, Color.Yellow, Color.White});
                ctx.ModoCaos = true; ctx.Intensidad = 1.4; ctx.ResumenVisual += "fire ";
            }},
            {"espacio", ctx => {
                AgregarAlgo(ctx, AlgoritmoBase.NebulosaEspacial, 0.5f);
                AgregarAlgo(ctx, AlgoritmoBase.VoronoiCelular, 0.2f);
                AgregarAlgo(ctx, AlgoritmoBase.RuidoSimplex, 0.3f);
                ctx.Paleta.AddRange(new[]{Color.Black, Color.Indigo, Color.DarkViolet, Color.MidnightBlue, Color.White});
                ctx.ModoOscuro = true; ctx.Complejidad = 1.5; ctx.ResumenVisual += "espacio ";
            }},
            {"space", ctx => {
                AgregarAlgo(ctx, AlgoritmoBase.NebulosaEspacial, 0.5f);
                AgregarAlgo(ctx, AlgoritmoBase.VoronoiCelular, 0.2f);
                AgregarAlgo(ctx, AlgoritmoBase.RuidoSimplex, 0.3f);
                ctx.Paleta.AddRange(new[]{Color.Black, Color.Indigo, Color.DarkViolet, Color.MidnightBlue, Color.White});
                ctx.ModoOscuro = true; ctx.Complejidad = 1.5; ctx.ResumenVisual += "space ";
            }},
            {"galaxia", ctx => {
                AgregarAlgo(ctx, AlgoritmoBase.NebulosaEspacial, 0.5f);
                AgregarAlgo(ctx, AlgoritmoBase.GeometricoSimetrico, 0.2f);
                AgregarAlgo(ctx, AlgoritmoBase.Multifractal, 0.3f);
                ctx.Paleta.AddRange(new[]{Color.Black, Color.Navy, Color.DarkViolet, Color.DeepSkyBlue, Color.White});
                ctx.ModoOscuro = true; ctx.ModoSimetrico = true; ctx.Complejidad = 2.0; ctx.ResumenVisual += "galaxia ";
            }},
            {"galaxy", ctx => {
                AgregarAlgo(ctx, AlgoritmoBase.NebulosaEspacial, 0.5f);
                AgregarAlgo(ctx, AlgoritmoBase.GeometricoSimetrico, 0.2f);
                AgregarAlgo(ctx, AlgoritmoBase.Multifractal, 0.3f);
                ctx.Paleta.AddRange(new[]{Color.Black, Color.Navy, Color.DarkViolet, Color.DeepSkyBlue, Color.White});
                ctx.ModoOscuro = true; ctx.ModoSimetrico = true; ctx.Complejidad = 2.0; ctx.ResumenVisual += "galaxy ";
            }},
            {"cristal", ctx => {
                AgregarAlgo(ctx, AlgoritmoBase.VoronoiCelular, 0.4f);
                AgregarAlgo(ctx, AlgoritmoBase.FractalMandelbrot, 0.3f);
                AgregarAlgo(ctx, AlgoritmoBase.RayMarching2D, 0.3f);
                ctx.Paleta.AddRange(new[]{Color.LightCyan, Color.Cyan, Color.LightBlue, Color.White, Color.Silver});
                ctx.ModoSimetrico = true; ctx.Complejidad = 1.8; ctx.ResumenVisual += "cristal ";
            }},
            {"crystal", ctx => {
                AgregarAlgo(ctx, AlgoritmoBase.VoronoiCelular, 0.4f);
                AgregarAlgo(ctx, AlgoritmoBase.FractalMandelbrot, 0.3f);
                AgregarAlgo(ctx, AlgoritmoBase.RayMarching2D, 0.3f);
                ctx.Paleta.AddRange(new[]{Color.LightCyan, Color.Cyan, Color.LightBlue, Color.White, Color.Silver});
                ctx.ModoSimetrico = true; ctx.Complejidad = 1.8; ctx.ResumenVisual += "crystal ";
            }},
            {"fractal", ctx => {
                AgregarAlgo(ctx, AlgoritmoBase.FractalMandelbrot, 0.6f);
                AgregarAlgo(ctx, AlgoritmoBase.Multifractal, 0.4f);
                ctx.Iteraciones = 300; ctx.Complejidad = 2.0; ctx.ResumenVisual += "fractal ";
            }},
            {"mandelbrot", ctx => {
                AgregarAlgo(ctx, AlgoritmoBase.FractalMandelbrot, 0.7f);
                AgregarAlgo(ctx, AlgoritmoBase.Multifractal, 0.3f);
                ctx.Iteraciones = 500; ctx.Complejidad = 3.0; ctx.ResumenVisual += "mandelbrot ";
            }},
            {"nube", ctx => {
                AgregarAlgo(ctx, AlgoritmoBase.RuidoPerlin, 0.5f);
                AgregarAlgo(ctx, AlgoritmoBase.RuidoSimplex, 0.5f);
                ctx.Paleta.AddRange(new[]{Color.White, Color.LightGray, Color.LightSteelBlue, Color.LightBlue});
                ctx.ModoSuave = true; ctx.Escala = 1.5; ctx.ResumenVisual += "nubes ";
            }},
            {"cloud", ctx => {
                AgregarAlgo(ctx, AlgoritmoBase.RuidoPerlin, 0.5f);
                AgregarAlgo(ctx, AlgoritmoBase.RuidoSimplex, 0.5f);
                ctx.Paleta.AddRange(new[]{Color.White, Color.LightGray, Color.LightSteelBlue, Color.LightBlue});
                ctx.ModoSuave = true; ctx.Escala = 1.5; ctx.ResumenVisual += "clouds ";
            }},
            {"niebla", ctx => {
                AgregarAlgo(ctx, AlgoritmoBase.RuidoPerlin, 0.4f);
                AgregarAlgo(ctx, AlgoritmoBase.DominioWarping, 0.6f);
                ctx.Paleta.AddRange(new[]{Color.DimGray, Color.Gray, Color.LightGray, Color.White});
                ctx.ModoSuave = true; ctx.Escala = 1.2; ctx.Intensidad = 0.6; ctx.ResumenVisual += "niebla ";
            }},
            {"fog", ctx => {
                AgregarAlgo(ctx, AlgoritmoBase.RuidoPerlin, 0.4f);
                AgregarAlgo(ctx, AlgoritmoBase.DominioWarping, 0.6f);
                ctx.Paleta.AddRange(new[]{Color.DimGray, Color.Gray, Color.LightGray, Color.White});
                ctx.ModoSuave = true; ctx.Escala = 1.2; ctx.Intensidad = 0.6; ctx.ResumenVisual += "fog ";
            }},
            {"lava", ctx => {
                AgregarAlgo(ctx, AlgoritmoBase.VoronoiCelular, 0.3f);
                AgregarAlgo(ctx, AlgoritmoBase.FluidoTurbulento, 0.4f);
                AgregarAlgo(ctx, AlgoritmoBase.TurbulenciaRizos, 0.3f);
                ctx.Paleta.AddRange(new[]{Color.Black, Color.DarkRed, Color.OrangeRed, Color.Orange});
                ctx.ModoCaos = true; ctx.Intensidad = 1.3; ctx.ResumenVisual += "lava ";
            }},
            {"plasma", ctx => {
                AgregarAlgo(ctx, AlgoritmoBase.PlasmaCaos, 0.4f);
                AgregarAlgo(ctx, AlgoritmoBase.OndaInterferencia, 0.3f);
                AgregarAlgo(ctx, AlgoritmoBase.DominioWarping, 0.3f);
                ctx.Paleta.AddRange(new[]{Color.DarkViolet, Color.Cyan, Color.DeepPink, Color.Yellow});
                ctx.ModoCaos = true; ctx.Intensidad = 1.5; ctx.Saturacion = 1.8; ctx.ResumenVisual += "plasma ";
            }},
            {"mandala", ctx => {
                AgregarAlgo(ctx, AlgoritmoBase.GeometricoSimetrico, 0.5f);
                AgregarAlgo(ctx, AlgoritmoBase.OndaInterferencia, 0.3f);
                AgregarAlgo(ctx, AlgoritmoBase.PatronesCelulares, 0.2f);
                ctx.ModoSimetrico = true; ctx.Complejidad = 2.0; ctx.ResumenVisual += "mandala ";
            }},
            {"geometrico", ctx => {
                AgregarAlgo(ctx, AlgoritmoBase.GeometricoSimetrico, 0.6f);
                AgregarAlgo(ctx, AlgoritmoBase.SuperficiesImplicitas, 0.4f);
                ctx.ModoSimetrico = true; ctx.ResumenVisual += "geometrico ";
            }},
            {"geometric", ctx => {
                AgregarAlgo(ctx, AlgoritmoBase.GeometricoSimetrico, 0.6f);
                AgregarAlgo(ctx, AlgoritmoBase.SuperficiesImplicitas, 0.4f);
                ctx.ModoSimetrico = true; ctx.ResumenVisual += "geometric ";
            }},
            {"psicodelico", ctx => {
                AgregarAlgo(ctx, AlgoritmoBase.PlasmaCaos, 0.3f);
                AgregarAlgo(ctx, AlgoritmoBase.OndaInterferencia, 0.3f);
                AgregarAlgo(ctx, AlgoritmoBase.DominioWarping, 0.4f);
                ctx.ModoCaos = true; ctx.Saturacion = 2.0; ctx.Intensidad = 1.5; ctx.ResumenVisual += "psicodelico ";
            }},
            {"psychedelic", ctx => {
                AgregarAlgo(ctx, AlgoritmoBase.PlasmaCaos, 0.3f);
                AgregarAlgo(ctx, AlgoritmoBase.OndaInterferencia, 0.3f);
                AgregarAlgo(ctx, AlgoritmoBase.DominioWarping, 0.4f);
                ctx.ModoCaos = true; ctx.Saturacion = 2.0; ctx.Intensidad = 1.5; ctx.ResumenVisual += "psychedelic ";
            }},
            {"nieve", ctx => {
                AgregarAlgo(ctx, AlgoritmoBase.VoronoiCelular, 0.3f);
                AgregarAlgo(ctx, AlgoritmoBase.RuidoPerlin, 0.4f);
                AgregarAlgo(ctx, AlgoritmoBase.Multifractal, 0.3f);
                ctx.Paleta.AddRange(new[]{Color.White, Color.AliceBlue, Color.LightBlue, Color.LightCyan});
                ctx.ModoSuave = true; ctx.Intensidad = 0.7; ctx.ResumenVisual += "nieve ";
            }},
            {"snow", ctx => {
                AgregarAlgo(ctx, AlgoritmoBase.VoronoiCelular, 0.3f);
                AgregarAlgo(ctx, AlgoritmoBase.RuidoPerlin, 0.4f);
                AgregarAlgo(ctx, AlgoritmoBase.Multifractal, 0.3f);
                ctx.Paleta.AddRange(new[]{Color.White, Color.AliceBlue, Color.LightBlue, Color.LightCyan});
                ctx.ModoSuave = true; ctx.Intensidad = 0.7; ctx.ResumenVisual += "snow ";
            }},
            {"atardecer", ctx => {
                AgregarAlgo(ctx, AlgoritmoBase.OndaInterferencia, 0.3f);
                AgregarAlgo(ctx, AlgoritmoBase.RuidoPerlin, 0.4f);
                AgregarAlgo(ctx, AlgoritmoBase.TurbulenciaRizos, 0.3f);
                ctx.Paleta.AddRange(new[]{Color.DarkRed, Color.OrangeRed, Color.Orange, Color.Gold, Color.DarkBlue, Color.Indigo});
                ctx.ModoSuave = true; ctx.Intensidad = 1.1; ctx.ResumenVisual += "atardecer ";
            }},
            {"sunset", ctx => {
                AgregarAlgo(ctx, AlgoritmoBase.OndaInterferencia, 0.3f);
                AgregarAlgo(ctx, AlgoritmoBase.RuidoPerlin, 0.4f);
                AgregarAlgo(ctx, AlgoritmoBase.TurbulenciaRizos, 0.3f);
                ctx.Paleta.AddRange(new[]{Color.DarkRed, Color.OrangeRed, Color.Orange, Color.Gold, Color.DarkBlue, Color.Indigo});
                ctx.ModoSuave = true; ctx.Intensidad = 1.1; ctx.ResumenVisual += "sunset ";
            }},
            {"amanecer", ctx => {
                AgregarAlgo(ctx, AlgoritmoBase.RuidoPerlin, 0.5f);
                AgregarAlgo(ctx, AlgoritmoBase.OndaInterferencia, 0.3f);
                AgregarAlgo(ctx, AlgoritmoBase.DominioWarping, 0.2f);
                ctx.Paleta.AddRange(new[]{Color.Pink, Color.LightCoral, Color.Gold, Color.LightYellow, Color.LightBlue});
                ctx.ModoSuave = true; ctx.Intensidad = 0.9; ctx.ResumenVisual += "amanecer ";
            }},
            {"sunrise", ctx => {
                AgregarAlgo(ctx, AlgoritmoBase.RuidoPerlin, 0.5f);
                AgregarAlgo(ctx, AlgoritmoBase.OndaInterferencia, 0.3f);
                AgregarAlgo(ctx, AlgoritmoBase.DominioWarping, 0.2f);
                ctx.Paleta.AddRange(new[]{Color.Pink, Color.LightCoral, Color.Gold, Color.LightYellow, Color.LightBlue});
                ctx.ModoSuave = true; ctx.Intensidad = 0.9; ctx.ResumenVisual += "sunrise ";
            }},
            {"onda", ctx => {
                AgregarAlgo(ctx, AlgoritmoBase.OndaInterferencia, 0.6f);
                AgregarAlgo(ctx, AlgoritmoBase.TurbulenciaRizos, 0.4f);
                ctx.Escala = 2.0; ctx.ResumenVisual += "ondas ";
            }},
            {"wave", ctx => {
                AgregarAlgo(ctx, AlgoritmoBase.OndaInterferencia, 0.6f);
                AgregarAlgo(ctx, AlgoritmoBase.TurbulenciaRizos, 0.4f);
                ctx.Escala = 2.0; ctx.ResumenVisual += "waves ";
            }},
            {"retro", ctx => {
                ctx.ModoRetro = true; ctx.Saturacion = 0.7; ctx.ResumenVisual += "retro ";
            }},
            {"vintage", ctx => {
                ctx.ModoRetro = true; ctx.Saturacion = 0.5; ctx.ResumenVisual += "vintage ";
            }},
            {"abstracto", ctx => {
                AgregarAlgo(ctx, AlgoritmoBase.PlasmaCaos, 0.3f);
                AgregarAlgo(ctx, AlgoritmoBase.GeometricoSimetrico, 0.3f);
                AgregarAlgo(ctx, AlgoritmoBase.DominioWarping, 0.4f);
                ctx.Complejidad = 1.5; ctx.ResumenVisual += "abstracto ";
            }},
            {"abstract", ctx => {
                AgregarAlgo(ctx, AlgoritmoBase.PlasmaCaos, 0.3f);
                AgregarAlgo(ctx, AlgoritmoBase.GeometricoSimetrico, 0.3f);
                AgregarAlgo(ctx, AlgoritmoBase.DominioWarping, 0.4f);
                ctx.Complejidad = 1.5; ctx.ResumenVisual += "abstract ";
            }},
            // NUEVOS TEMAS
            {"simplex", ctx => {
                AgregarAlgo(ctx, AlgoritmoBase.RuidoSimplex, 1.0f);
                ctx.ResumenVisual += "simplex ";
            }},
            {"warp", ctx => {
                AgregarAlgo(ctx, AlgoritmoBase.DominioWarping, 1.0f);
                ctx.ResumenVisual += "warp ";
            }},
            {"deformar", ctx => {
                AgregarAlgo(ctx, AlgoritmoBase.DominioWarping, 1.0f);
                ctx.ResumenVisual += "deformar ";
            }},
            {"multifractal", ctx => {
                AgregarAlgo(ctx, AlgoritmoBase.Multifractal, 1.0f);
                ctx.ResumenVisual += "multifractal ";
            }},
            {"rizos", ctx => {
                AgregarAlgo(ctx, AlgoritmoBase.TurbulenciaRizos, 1.0f);
                ctx.ResumenVisual += "rizos ";
            }},
            {"curl", ctx => {
                AgregarAlgo(ctx, AlgoritmoBase.TurbulenciaRizos, 1.0f);
                ctx.ResumenVisual += "curl ";
            }},
            {"celular", ctx => {
                AgregarAlgo(ctx, AlgoritmoBase.PatronesCelulares, 1.0f);
                ctx.ResumenVisual += "celular ";
            }},
            {"cellular", ctx => {
                AgregarAlgo(ctx, AlgoritmoBase.PatronesCelulares, 1.0f);
                ctx.ResumenVisual += "cellular ";
            }},
            {"raymarching", ctx => {
                AgregarAlgo(ctx, AlgoritmoBase.RayMarching2D, 1.0f);
                ctx.ResumenVisual += "raymarching ";
            }},
            {"implicito", ctx => {
                AgregarAlgo(ctx, AlgoritmoBase.SuperficiesImplicitas, 1.0f);
                ctx.ResumenVisual += "implicito ";
            }},
            {"implicit", ctx => {
                AgregarAlgo(ctx, AlgoritmoBase.SuperficiesImplicitas, 1.0f);
                ctx.ResumenVisual += "implicit ";
            }},
            // MINECRAFT / VOXEL
            {"minecraft", ctx => {
                AgregarAlgo(ctx, AlgoritmoBase.MundoVoxelMinecraft, 1.0f);
                ctx.ResumenVisual += "minecraft ";
            }},
            {"voxel", ctx => {
                AgregarAlgo(ctx, AlgoritmoBase.MundoVoxelMinecraft, 1.0f);
                ctx.ResumenVisual += "voxel ";
            }},
            {"bloques", ctx => {
                AgregarAlgo(ctx, AlgoritmoBase.MundoVoxelMinecraft, 1.0f);
                ctx.ResumenVisual += "bloques ";
            }},
            {"blocks", ctx => {
                AgregarAlgo(ctx, AlgoritmoBase.MundoVoxelMinecraft, 1.0f);
                ctx.ResumenVisual += "blocks ";
            }},
            {"isometrico", ctx => {
                AgregarAlgo(ctx, AlgoritmoBase.MundoVoxelMinecraft, 1.0f);
                ctx.ResumenVisual += "isometrico ";
            }},
            {"isometric", ctx => {
                AgregarAlgo(ctx, AlgoritmoBase.MundoVoxelMinecraft, 1.0f);
                ctx.ResumenVisual += "isometric ";
            }},
            {"mundo", ctx => {
                AgregarAlgo(ctx, AlgoritmoBase.MundoVoxelMinecraft, 1.0f);
                ctx.ResumenVisual += "mundo ";
            }},
            {"world", ctx => {
                AgregarAlgo(ctx, AlgoritmoBase.MundoVoxelMinecraft, 1.0f);
                ctx.ResumenVisual += "world ";
            }},
        };

        public static readonly Dictionary<string, Action<ContextoVisual>> MapaModificadores = new Dictionary<string, Action<ContextoVisual>>(StringComparer.OrdinalIgnoreCase)
        {
            {"oscuro",    ctx => { ctx.ModoOscuro = true;  ctx.Intensidad *= 0.7; }},
            {"dark",      ctx => { ctx.ModoOscuro = true;  ctx.Intensidad *= 0.7; }},
            {"brillante", ctx => { ctx.Intensidad *= 1.4;  ctx.Saturacion *= 1.3; }},
            {"bright",    ctx => { ctx.Intensidad *= 1.4;  ctx.Saturacion *= 1.3; }},
            {"suave",     ctx => { ctx.ModoSuave  = true;  ctx.Intensidad *= 0.8; }},
            {"soft",      ctx => { ctx.ModoSuave  = true;  ctx.Intensidad *= 0.8; }},
            {"intenso",   ctx => { ctx.Intensidad *= 1.5;  ctx.Saturacion *= 1.4; }},
            {"intense",   ctx => { ctx.Intensidad *= 1.5;  ctx.Saturacion *= 1.4; }},
            {"caotico",   ctx => { ctx.ModoCaos   = true;  ctx.Complejidad *= 1.5; }},
            {"chaotic",   ctx => { ctx.ModoCaos   = true;  ctx.Complejidad *= 1.5; }},
            {"simetrico", ctx => { ctx.ModoSimetrico = true; }},
            {"symmetric", ctx => { ctx.ModoSimetrico = true; }},
            {"complejo",  ctx => { ctx.Complejidad *= 2.0; ctx.Iteraciones += 100; }},
            {"complex",   ctx => { ctx.Complejidad *= 2.0; ctx.Iteraciones += 100; }},
            {"simple",    ctx => { ctx.Complejidad *= 0.5; ctx.Escala *= 0.7; }},
            {"grande",    ctx => { ctx.Escala *= 0.5; }},
            {"big",       ctx => { ctx.Escala *= 0.5; }},
            {"pequeño",   ctx => { ctx.Escala *= 2.0; }},
            {"small",     ctx => { ctx.Escala *= 2.0; }},
            {"calido",    ctx => { ctx.Paleta.AddRange(new[]{Color.OrangeRed, Color.Orange, Color.Gold}); }},
            {"warm",      ctx => { ctx.Paleta.AddRange(new[]{Color.OrangeRed, Color.Orange, Color.Gold}); }},
            {"frio",      ctx => { ctx.Paleta.AddRange(new[]{Color.DarkBlue, Color.CornflowerBlue, Color.Cyan}); }},
            {"cold",      ctx => { ctx.Paleta.AddRange(new[]{Color.DarkBlue, Color.CornflowerBlue, Color.Cyan}); }},
            {"cool",      ctx => { ctx.Paleta.AddRange(new[]{Color.DarkBlue, Color.CornflowerBlue, Color.Cyan}); }},
            {"misterioso",ctx => { ctx.ModoOscuro = true; ctx.Paleta.AddRange(new[]{Color.Indigo, Color.DarkViolet}); }},
            {"mysterious",ctx => { ctx.ModoOscuro = true; ctx.Paleta.AddRange(new[]{Color.Indigo, Color.DarkViolet}); }},
            {"energetico",ctx => { ctx.Intensidad *= 1.6; ctx.Saturacion *= 1.5; ctx.ModoCaos = true; }},
            {"energetic", ctx => { ctx.Intensidad *= 1.6; ctx.Saturacion *= 1.5; ctx.ModoCaos = true; }},
            {"tranquilo", ctx => { ctx.ModoSuave = true; ctx.Intensidad *= 0.7; ctx.Saturacion *= 0.8; }},
            {"peaceful",  ctx => { ctx.ModoSuave = true; ctx.Intensidad *= 0.7; ctx.Saturacion *= 0.8; }},
            {"calm",      ctx => { ctx.ModoSuave = true; ctx.Intensidad *= 0.7; ctx.Saturacion *= 0.8; }},
            {"pastel",    ctx => { ctx.Saturacion *= 0.5; ctx.Intensidad *= 1.2; ctx.ModoSuave = true; }},
            {"neon",      ctx => { ctx.Saturacion *= 2.5; ctx.Intensidad *= 1.3; ctx.ModoOscuro = true; }},
            {"luz",       ctx => { ctx.Intensidad *= 1.3; }},
            {"light",     ctx => { ctx.Intensidad *= 1.3; }},
            {"sombra",    ctx => { ctx.ModoOscuro = true; ctx.Intensidad *= 0.8; }},
            {"shadow",    ctx => { ctx.ModoOscuro = true; ctx.Intensidad *= 0.8; }},
            {"detallado", ctx => { ctx.Complejidad *= 1.8; ctx.Iteraciones += 200; }},
            {"detailed",  ctx => { ctx.Complejidad *= 1.8; ctx.Iteraciones += 200; }},
            {"suavizado", ctx => { ctx.ModoSuave = true; ctx.Complejidad *= 0.8; }},
            {"smooth",    ctx => { ctx.ModoSuave = true; ctx.Complejidad *= 0.8; }},
        };

        private static void AgregarAlgo(ContextoVisual ctx, AlgoritmoBase algo, float peso)
        {
            int idx = ctx.Algoritmos.IndexOf(algo);
            if (idx >= 0) ctx.PesosAlgoritmos[idx] += peso;
            else { ctx.Algoritmos.Add(algo); ctx.PesosAlgoritmos.Add(peso); }
        }
    }

    // ═══════════════════════════════════════════════════
    //  INTERPRETADOR DE PROMPT
    // ═══════════════════════════════════════════════════

    public static class InterpretadorPrompt
    {
        public static ContextoVisual Interpretar(string prompt, int semilla)
        {
            var ctx = new ContextoVisual();
            if (semilla >= 0) ctx.Semilla = semilla;

            if (string.IsNullOrWhiteSpace(prompt))
            {
                ctx.Algoritmos.Add(AlgoritmoBase.RuidoPerlin);
                ctx.PesosAlgoritmos.Add(1.0f);
                ctx.ResumenVisual = "aleatorio";
                return ctx;
            }

            var tokens = prompt.ToLower()
                .Split(new char[]{' ',',','.',':',';','!','?','-','_','/','\t','\n'}, StringSplitOptions.RemoveEmptyEntries)
                .ToList();

            ctx.PalabrasDetectadas.AddRange(tokens);

            foreach (var token in tokens)
            {
                if (BancoPalabras.MapaTemas.ContainsKey(token))
                    BancoPalabras.MapaTemas[token](ctx);
            }

            foreach (var token in tokens)
            {
                if (BancoPalabras.MapaColores.ContainsKey(token))
                    ctx.Paleta.AddRange(BancoPalabras.MapaColores[token]);
            }

            foreach (var token in tokens)
            {
                if (BancoPalabras.MapaModificadores.ContainsKey(token))
                    BancoPalabras.MapaModificadores[token](ctx);
            }

            if (ctx.Algoritmos.Count == 0)
            {
                ctx.Algoritmos.Add(AlgoritmoBase.RuidoPerlin);
                ctx.PesosAlgoritmos.Add(1.0f);
                ctx.ResumenVisual = "perlin base";
            }

            if (ctx.Paleta.Count == 0)
                AsignarPaletaPorDefecto(ctx);

            float totalPeso = 0;
            foreach (var p in ctx.PesosAlgoritmos) totalPeso += p;
            for (int i = 0; i < ctx.PesosAlgoritmos.Count; i++)
                ctx.PesosAlgoritmos[i] /= totalPeso;

            ctx.Escala     = Math.Max(0.2, Math.Min(5.0, ctx.Escala));
            ctx.Intensidad = Math.Max(0.3, Math.Min(2.5, ctx.Intensidad));
            ctx.Saturacion = Math.Max(0.1, Math.Min(3.0, ctx.Saturacion));
            ctx.Iteraciones= Math.Max(50, Math.Min(800, ctx.Iteraciones));

            return ctx;
        }

        private static void AsignarPaletaPorDefecto(ContextoVisual ctx)
        {
            if (ctx.ModoOscuro)
                ctx.Paleta.AddRange(new[]{Color.Black, Color.DarkSlateBlue, Color.DarkViolet, Color.MidnightBlue});
            else if (ctx.ModoSuave)
                ctx.Paleta.AddRange(new[]{Color.LightBlue, Color.LightGreen, Color.LightYellow, Color.LavenderBlush});
            else
                ctx.Paleta.AddRange(new[]{Color.DarkBlue, Color.MediumBlue, Color.DodgerBlue, Color.Orange, Color.White});
        }
    }

    // ═══════════════════════════════════════════════════
    //  MOTOR MATEMÁTICO MEJORADO
    // ═══════════════════════════════════════════════════

    public static class Matematica
    {
        private static int[] _perm = new int[512];
        private static bool _init = false;
        private static int _semilla;
        private static double[] _grad3 = new double[] {
            1,1,0, -1,1,0, 1,-1,0, -1,-1,0,
            1,0,1, -1,0,1, 1,0,-1, -1,0,-1,
            0,1,1, 0,-1,1, 0,1,-1, 0,-1,-1
        };

        public static void InicializarSemilla(int semilla)
        {
            _semilla = semilla;
            var rnd = new Random(semilla);
            var p = Enumerable.Range(0, 256).ToArray();
            for (int i = 255; i > 0; i--) { int j = rnd.Next(i+1); int t = p[i]; p[i] = p[j]; p[j] = t; }
            for (int i = 0; i < 512; i++) _perm[i] = p[i & 255];
            _init = true;
        }

        private static double Fade(double t)
        {
            return t * t * t * (t * (t * 6 - 15) + 10);
        }
        
        private static double FadeSuperior(double t)
        {
            // Curva de interpolación más suave (6t^5 - 15t^4 + 10t^3)
            return t * t * t * (t * (t * 6 - 15) + 10);
        }

        private static double Lerp(double t, double a, double b)
        {
            return a + t * (b - a);
        }
        
        private static double LerpCoseno(double t, double a, double b)
        {
            double ft = t * Math.PI;
            double f = (1 - Math.Cos(ft)) * 0.5;
            return a * (1 - f) + b * f;
        }

        private static double Grad(int h, double x, double y)
        {
            h &= 15;
            double u = h < 8 ? x : y;
            double v = h < 4 ? y : (h == 12 || h == 14) ? x : 0;
            return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
        }

        public static double Perlin(double x, double y)
        {
            if (!_init) InicializarSemilla(42);
            int X = (int)Math.Floor(x) & 255, Y = (int)Math.Floor(y) & 255;
            x -= Math.Floor(x); y -= Math.Floor(y);
            double u = Fade(x), v = Fade(y);
            int A = _perm[X]+Y, B = _perm[X+1]+Y;
            return Lerp(v, Lerp(u, Grad(_perm[A],x,y), Grad(_perm[B],x-1,y)),
                           Lerp(u, Grad(_perm[A+1],x,y-1), Grad(_perm[B+1],x-1,y-1)));
        }
        
        // Ruido Simplex 2D mejorado
        public static double Simplex(double x, double y)
        {
            if (!_init) InicializarSemilla(42);
            
            double F2 = 0.5 * (Math.Sqrt(3.0) - 1.0);
			double G2 = (3.0 - Math.Sqrt(3.0)) / 6.0;
            
            double s = (x + y) * F2;
            int i = (int)Math.Floor(x + s);
            int j = (int)Math.Floor(y + s);
            double t = (i + j) * G2;
            double X0 = i - t;
            double Y0 = j - t;
            double x0 = x - X0;
            double y0 = y - Y0;
            
            int i1, j1;
            if (x0 > y0) { i1 = 1; j1 = 0; }
            else { i1 = 0; j1 = 1; }
            
            double x1 = x0 - i1 + G2;
            double y1 = y0 - j1 + G2;
            double x2 = x0 - 1.0 + 2.0 * G2;
            double y2 = y0 - 1.0 + 2.0 * G2;
            
            int ii = i & 255;
            int jj = j & 255;
            
            double n0 = 0, n1 = 0, n2 = 0;
            
            double t0 = 0.5 - x0 * x0 - y0 * y0;
            if (t0 >= 0) {
                t0 *= t0;
                int gi0 = _perm[ii + _perm[jj]] % 12;
                n0 = t0 * t0 * (x0 * _grad3[gi0 * 3] + y0 * _grad3[gi0 * 3 + 1]);
            }
            
            double t1 = 0.5 - x1 * x1 - y1 * y1;
            if (t1 >= 0) {
                t1 *= t1;
                int gi1 = _perm[ii + i1 + _perm[jj + j1]] % 12;
                n1 = t1 * t1 * (x1 * _grad3[gi1 * 3] + y1 * _grad3[gi1 * 3 + 1]);
            }
            
            double t2 = 0.5 - x2 * x2 - y2 * y2;
            if (t2 >= 0) {
                t2 *= t2;
                int gi2 = _perm[ii + 1 + _perm[jj + 1]] % 12;
                n2 = t2 * t2 * (x2 * _grad3[gi2 * 3] + y2 * _grad3[gi2 * 3 + 1]);
            }
            
            return 70.0 * (n0 + n1 + n2);
        }

        public static double FBM(double x, double y, int octavas, double persistencia)
        {
            double total = 0, amp = 1, freq = 1, max = 0;
            for (int i = 0; i < octavas; i++)
            {
                total += Perlin(x * freq, y * freq) * amp;
                max += amp; amp *= persistencia; freq *= 2;
            }
            return total / max;
        }
        
        public static double FBMSimplex(double x, double y, int octavas, double persistencia)
        {
            double total = 0, amp = 1, freq = 1, max = 0;
            for (int i = 0; i < octavas; i++)
            {
                total += Simplex(x * freq, y * freq) * amp;
                max += amp; amp *= persistencia; freq *= 2;
            }
            return total / max;
        }
        
        // Turbulencia con rizos (Curl Noise approximation)
        public static double TurbulenciaRizos(double x, double y, int octavas, double persistencia, double tiempo)
        {
            double dx = 0, dy = 0;
            double amp = 1, freq = 1;
            
            for (int i = 0; i < octavas; i++)
            {
                double n = Simplex(x * freq + tiempo, y * freq);
                double angle = n * Math.PI * 2;
                dx += Math.Cos(angle) * amp;
                dy += Math.Sin(angle) * amp;
                amp *= persistencia;
                freq *= 2;
            }
            
            return Simplex(x + dx, y + dy);
        }
        
        // Dominio Warping - distorsión del espacio
        public static double DominioWarp(double x, double y, double warpAmount, int octavas)
        {
            double qx = FBM(x + 0.0, y + 0.0, octavas, 0.5);
            double qy = FBM(x + 5.2, y + 1.3, octavas, 0.5);
            
            double rx = FBM(x + warpAmount * qx + 1.7, y + warpAmount * qy + 9.2, octavas, 0.5);
            double ry = FBM(x + warpAmount * qx + 8.3, y + warpAmount * qy + 2.8, octavas, 0.5);
            
            return FBM(x + warpAmount * rx, y + warpAmount * ry, octavas, 0.5);
        }

        public static double Clamp01(double v)
        {
            return Math.Max(0, Math.Min(1, v));
        }
        
        public static double Clamp(double v, double min, double max)
        {
            return Math.Max(min, Math.Min(max, v));
        }
        
        public static double Smoothstep(double edge0, double edge1, double x)
        {
            double t = Clamp((x - edge0) / (edge1 - edge0), 0.0, 1.0);
            return t * t * (3.0 - 2.0 * t);
        }

        public static double Map(double v, double inMin, double inMax, double outMin, double outMax)
        {
            return outMin + (v - inMin) / (inMax - inMin) * (outMax - outMin);
        }

        public static Color Lerp(Color a, Color b, double t)
        {
            t = Clamp01(t);
            return Color.FromArgb(
                (int)(a.A + (b.A - a.A) * t),
                (int)(a.R + (b.R - a.R) * t),
                (int)(a.G + (b.G - a.G) * t),
                (int)(a.B + (b.B - a.B) * t)
            );
        }

        public static Color MutiStop(List<Color> paleta, double t)
        {
            if (paleta.Count == 0) return Color.Black;
            if (paleta.Count == 1) return paleta[0];
            t = Clamp01(t);
            double seg = t * (paleta.Count - 1);
            int idx = (int)seg;
            if (idx >= paleta.Count - 1) return paleta[paleta.Count - 1];
            return Lerp(paleta[idx], paleta[idx + 1], seg - idx);
        }

        public static Color AjustarSaturacion(Color c, double sat)
        {
            double r = c.R / 255.0, g = c.G / 255.0, b = c.B / 255.0;
            double gris = r * 0.299 + g * 0.587 + b * 0.114;
            r = Clamp01(gris + (r - gris) * sat);
            g = Clamp01(gris + (g - gris) * sat);
            b = Clamp01(gris + (b - gris) * sat);
            return Color.FromArgb((int)(r*255), (int)(g*255), (int)(b*255));
        }

        public static Color ColorHSV(double h, double s, double v)
        {
            s = Clamp01(s); v = Clamp01(v); h = h % 360; if (h < 0) h += 360;
            double c = v * s, x = c * (1 - Math.Abs((h / 60) % 2 - 1)), m = v - c;
            double r, g, b;
            if      (h < 60)  { r=c; g=x; b=0; }
            else if (h < 120) { r=x; g=c; b=0; }
            else if (h < 180) { r=0; g=c; b=x; }
            else if (h < 240) { r=0; g=x; b=c; }
            else if (h < 300) { r=x; g=0; b=c; }
            else              { r=c; g=0; b=x; }
            return Color.FromArgb((int)((r+m)*255),(int)((g+m)*255),(int)((b+m)*255));
        }
        
        // Distancia a segmento de línea
        public static double DistanciaSegmento(double px, double py, double x1, double y1, double x2, double y2)
        {
            double dx = x2 - x1;
            double dy = y2 - y1;
            double l2 = dx * dx + dy * dy;
            if (l2 == 0) return Math.Sqrt((px - x1) * (px - x1) + (py - y1) * (py - y1));
            double t = Clamp(((px - x1) * dx + (py - y1) * dy) / l2, 0, 1);
            double projX = x1 + t * dx;
            double projY = y1 + t * dy;
            return Math.Sqrt((px - projX) * (px - projX) + (py - projY) * (py - projY));
        }
        
        // Distancia a círculo
        public static double DistanciaCirculo(double px, double py, double cx, double cy, double r)
        {
            return Math.Abs(Math.Sqrt((px - cx) * (px - cx) + (py - cy) * (py - cy)) - r);
        }
        
        // Rotación 2D
        public static void Rotar(double x, double y, double angulo, out double rx, out double ry)
        {
            double cos = Math.Cos(angulo);
            double sin = Math.Sin(angulo);
            rx = x * cos - y * sin;
            ry = x * sin + y * cos;
        }
    }

    // ═══════════════════════════════════════════════════
    //  GENERADORES POR ALGORITMO MEJORADOS
    // ═══════════════════════════════════════════════════

    public static class Generadores
    {
        // Perlin mejorado con FBM y opciones de suavizado
        public static double Perlin(double nx, double ny, ContextoVisual ctx)
        {
            double e = ctx.ModoSuave ? 0.4 : 0.5;
            int octavas = ctx.ModoCaos ? 8 : 5;
            double v = Matematica.FBM(nx * ctx.Escala, ny * ctx.Escala, octavas, e);
            
            // Añadir detalle de alta frecuencia
            if (ctx.Complejidad > 1.5)
            {
                double detalle = Matematica.Perlin(nx * ctx.Escala * 4, ny * ctx.Escala * 4) * 0.1;
                v += detalle;
            }
            
            return Matematica.Clamp01((v + 1) / 2);
        }

        // Fractal Mandelbrot mejorado con suavizado y coloreado por curvatura
        public static double Fractal(double nx, double ny, ContextoVisual ctx)
        {
            double zoom = ctx.Escala * 2.0;
            double zx = nx * 3.5 * zoom - 2.5;
            double zy = ny * 2.0 * zoom - 1.0;
            double cx = zx, cy = zy;
            
            double zx2 = 0, zy2 = 0;
            int i = 0;
            
            while (zx2 + zy2 < 4.0 && i < ctx.Iteraciones)
            {
                zy = 2.0 * zx * zy + cy;
                zx = zx2 - zy2 + cx;
                zx2 = zx * zx;
                zy2 = zy * zy;
                i++;
            }
            
            if (i == ctx.Iteraciones) return 0;
            
            // Suavizado logarítmico mejorado
            double logZn = Math.Log(zx2 + zy2) / 2.0;
            double nu = Math.Log(logZn / Math.Log(2.0)) / Math.Log(2.0);
            double smooth = i + 1.0 - nu;
            
            double valor = smooth / ctx.Iteraciones * ctx.Intensidad;
            
            // Añadir textura orbital
            if (ctx.ModoCaos)
            {
                double orbita = Math.Sin(smooth * 0.1) * Math.Cos(smooth * 0.15) * 0.1;
                valor += orbita;
            }
            
            return Matematica.Clamp01(valor);
        }

        // Fluido turbulento mejorado con advección y múltiples escalas
        public static double Fluido(double nx, double ny, ContextoVisual ctx)
        {
            double t = ctx.Semilla * 0.001;
            double escala = ctx.Escala;
            
            // Campo vectorial mejorado
            double v1 = Matematica.FBM(nx * escala + t, ny * escala, 4, 0.5);
            double v2 = Matematica.FBM(nx * escala + 5.2, ny * escala + 1.3 + t, 4, 0.5);
            double v3 = Matematica.FBM(nx * escala * 2.0 - t * 0.5, ny * escala * 2.0, 3, 0.5);
            
            double ang = v1 * Math.PI * 4 + v3 * Math.PI;
            double rad = (v2 + 1) / 2;
            
            // Advección del campo
            double fx = nx + Math.Cos(ang) * rad * 0.2 * ctx.Intensidad;
            double fy = ny + Math.Sin(ang) * rad * 0.2 * ctx.Intensidad;
            
            // Turbulencia multi-escala
            double r1 = Matematica.FBM(fx * 2, fy * 2, 4, 0.5);
            double r2 = Matematica.FBM(fx * 4 + r1, fy * 4, 2, 0.5) * 0.5;
            
            double resultado = (r1 + r2) / 1.5;
            return Matematica.Clamp01((resultado + 1) / 2);
        }

        // Geométrico simétrico mejorado con más variantes
        public static double Geometrico(double nx, double ny, ContextoVisual ctx)
        {
            double cx = nx - 0.5, cy = ny - 0.5;
            double dist = Math.Sqrt(cx*cx + cy*cy);
            double ang  = Math.Atan2(cy, cx);
            
            int segs = 3 + (ctx.Semilla % 9);
            if (ctx.ModoCaos) segs = 5 + (ctx.Semilla % 15);
            
            double segAng = Math.PI * 2 / segs;
            double angRel = ang % segAng;
            double escF = ctx.Escala * ctx.Complejidad;
            
            // Múltiples armónicos
            double v1 = Math.Sin(angRel * escF * 8);
            double v2 = Math.Cos(dist * 20 * escF);
            double v3 = Math.Sin(ang * segs + dist * 10);
            
            double v = (v1 + v2 * 0.5 + v3 * 0.25) / 1.75;
            
            if (ctx.ModoSimetrico)
            {
                double sim = Math.Sin(dist * 15 * escF) * Math.Cos(ang * segs);
                v = (v + sim) / 2;
            }
            
            // Añadir detalle radial
            if (ctx.Complejidad > 1.2)
            {
                v += Math.Sin(dist * 50 * escF) * 0.1 * (1.0 - dist * 2.0);
            }
            
            return Matematica.Clamp01((v + 1) / 2);
        }

        // Voronoi mejorado con distancia al segundo vecino (F2-F1) y colores
        public static double Voronoi(double nx, double ny, ContextoVisual ctx)
        {
            var rnd = new Random(ctx.Semilla);
            int N = 15 + (int)(ctx.Complejidad * 15);
            double escala = ctx.Escala;
            
            // Grid-based acceleration
            int gridSize = (int)Math.Sqrt(N) + 1;
            var puntos = new List<double[]>();
            
            for (int i = 0; i < N; i++) 
            { 
                puntos.Add(new double[] { rnd.NextDouble(), rnd.NextDouble() });
            }
            
            double min1 = double.MaxValue, min2 = double.MaxValue;
            double nearestX = 0, nearestY = 0;
            
            for (int i = 0; i < N; i++)
            {
                double dx = nx - puntos[i][0];
                double dy = ny - puntos[i][1];
                
                // Distancia euclidiana con escala
                double d = Math.Sqrt(dx*dx + dy*dy) * escala;
                
                // Distancia Manhattan alternativa
                if (ctx.ModoCaos)
                {
                    d = (Math.Abs(dx) + Math.Abs(dy)) * escala * 0.7;
                }
                
                if (d < min1) 
                { 
                    min2 = min1; 
                    min1 = d; 
                    nearestX = puntos[i][0];
                    nearestY = puntos[i][1];
                }
                else if (d < min2) 
                { 
                    min2 = d; 
                }
            }
            
            // F2 - F1 para bordes de celda
            double v = (min2 - min1) * ctx.Intensidad * 2.0;
            
            // Añadir gradiente de distancia al centro
            if (ctx.ModoSuave)
            {
                double centro = 1.0 - Math.Exp(-min1 * 3.0);
                v = v * 0.7 + centro * 0.3;
            }
            
            return Matematica.Clamp01(v);
        }

        // Ondas de interferencia mejoradas con múltiples fuentes y fases
        public static double Onda(double nx, double ny, ContextoVisual ctx)
        {
            var rnd = new Random(ctx.Semilla);
            int ondas = 3 + (int)(ctx.Complejidad * 4);
            double acc = 0;
            double escala = ctx.Escala;
            
            for (int i = 0; i < ondas; i++)
            {
                double ox = rnd.NextDouble();
                double oy = rnd.NextDouble();
                double freq = (0.5 + rnd.NextDouble() * 2.0) * escala;
                double fase = rnd.NextDouble() * Math.PI * 2;
                double amp = 1.0 - (i / (double)ondas) * 0.5; // Amplitud decreciente
                
                double dx = nx - ox;
                double dy = ny - oy;
                double dist = Math.Sqrt(dx*dx + dy*dy);
                
                // Onda circular con decaimiento
                double onda = Math.Sin(dist * freq * 10.0 + fase) * amp;
                
                // Interferencia constructiva/destructiva
                acc += onda;
            }
            
            acc /= ondas;
            
            // Normalización suave
            if (ctx.ModoSuave) 
                acc = Math.Tanh(acc * 2.0);
            
            return Matematica.Clamp01((acc + 1) / 2 * ctx.Intensidad);
        }

        // Nebulosa mejorada con múltiples octavas de FBM y coloración
        public static double Nebulosa(double nx, double ny, ContextoVisual ctx)
        {
            double e  = ctx.Escala;
            double intensidad = ctx.Intensidad;
            
            // Base de nube grande
            double n1 = Matematica.FBM(nx * e * 1.0, ny * e * 1.0, 6, 0.6);
            
            // Detalle medio
            double n2 = Matematica.FBM(nx * e * 2.0 + n1 * 0.5, ny * e * 2.0 + n1 * 0.3, 5, 0.5);
            
            // Detalle fino
            double n3 = Matematica.FBM(nx * e * 4.0 + n2, ny * e * 4.0 + n2, 4, 0.4);
            
            // Estructura filamentosa
            double filamentos = Math.Abs(Matematica.Simplex(nx * e * 8.0, ny * e * 8.0));
            filamentos = Math.Pow(filamentos, 2.0);
            
            // Combinación no lineal
            double v = n3 * 0.5 + n2 * 0.3 + n1 * 0.2;
            v = v + filamentos * 0.2 * intensidad;
            
            // Contraste adaptativo
            if (ctx.ModoOscuro)
            {
                v = Math.Pow(Matematica.Clamp01((v + 1) / 2), 2.0) * intensidad;
            }
            else
            {
                v = Matematica.Clamp01((v + 1) / 2) * intensidad;
            }
            
            return Matematica.Clamp01(v);
        }

        // Plasma caótico mejorado con más variaciones
        public static double Plasma(double nx, double ny, ContextoVisual ctx)
        {
            double e = ctx.Escala * ctx.Complejidad;
            double t = ctx.Semilla * 0.01;
            
            // Funciones de distorsión
            double s1 = Math.Sin(nx * e * 5.0 + t);
            double c1 = Math.Cos(ny * e * 5.0 + t);
            double s2 = Math.Sin((nx + ny) * e * 3.0);
            double c2 = Math.Cos((nx - ny) * e * 4.0);
            
            // Interferencia compleja
            double v = s1 + c1 + s2 + c2;
            
            // Añadir no linealidades
            v += Math.Sin(Math.Sqrt(nx*nx + ny*ny) * e * 8.0) * 0.5;
            v += Math.Cos(nx * ny * e * e * 10.0) * 0.3;
            
            if (ctx.ModoCaos)
            {
                // Caos adicional con retroalimentación
                double ruido = Matematica.Simplex(nx * e * 2.0, ny * e * 2.0);
                v += Math.Sin(ruido * Math.PI * 4.0) * 0.4;
            }
            
            return Matematica.Clamp01((v / 4.5 + 0.5) * ctx.Intensidad);
        }
        
        // NUEVO: Ruido Simplex mejorado
        public static double SimplexMejorado(double nx, double ny, ContextoVisual ctx)
        {
            double escala = ctx.Escala;
            int octavas = ctx.ModoCaos ? 6 : 4;
            double persistencia = ctx.ModoSuave ? 0.4 : 0.5;
            
            double v = Matematica.FBMSimplex(nx * escala, ny * escala, octavas, persistencia);
            
            // Derivadas para efecto de relieve
            if (ctx.Complejidad > 1.3)
            {
                double delta = 0.01;
                double dx = Matematica.Simplex((nx + delta) * escala, ny * escala) - 
                           Matematica.Simplex((nx - delta) * escala, ny * escala);
                double dy = Matematica.Simplex(nx * escala, (ny + delta) * escala) - 
                           Matematica.Simplex(nx * escala, (ny - delta) * escala);
                double gradiente = Math.Sqrt(dx*dx + dy*dy) * 2.0;
                v = v * 0.8 + gradiente * 0.2;
            }
            
            return Matematica.Clamp01((v + 1) / 2);
        }
        
        // NUEVO: Dominio Warping avanzado
        public static double Warping(double nx, double ny, ContextoVisual ctx)
        {
            double cantidad = ctx.Intensidad * 0.5;
            int octavas = (int)(ctx.Complejidad * 3.0) + 2;
            
            double v = Matematica.DominioWarp(nx * ctx.Escala, ny * ctx.Escala, cantidad, octavas);
            
            // Segunda pasada de warping para más distorsión
            if (ctx.ModoCaos)
            {
                v = Matematica.DominioWarp(nx * ctx.Escala + v * 0.1, ny * ctx.Escala + v * 0.1, cantidad * 0.5, 3);
            }
            
            return Matematica.Clamp01((v + 1) / 2);
        }
        
        // NUEVO: Multifractal heterogéneo
        public static double Multifractal(double nx, double ny, ContextoVisual ctx)
        {
            double x = nx * ctx.Escala;
            double y = ny * ctx.Escala;
            int octavas = (int)(ctx.Complejidad * 4.0) + 2;
            
            double valor = 1.0;
            double amp = 1.0;
            double freq = 1.0;
            double peso = 1.0;
            
            for (int i = 0; i < octavas; i++)
            {
                double signal = Matematica.Perlin(x * freq, y * freq);
                signal = Math.Abs(signal); // Valor absoluto para terrenos rugosos
                signal = signal * signal;  // Cuadrado para acentuar picos
                
                signal *= peso;
                peso = signal * 0.5; // Heterogeneidad: peso depende del valor anterior
                
                valor += signal * amp;
                amp *= 0.5;
                freq *= 2.0;
            }
            
            return Matematica.Clamp01(valor / 2.0);
        }
        
        // NUEVO: Turbulencia con rizos (Curl-like)
        public static double Rizos(double nx, double ny, ContextoVisual ctx)
        {
            double escala = ctx.Escala;
            double tiempo = ctx.TiempoAnimacion;
            int octavas = (int)(ctx.Complejidad * 3.0) + 2;
            
            double v = Matematica.TurbulenciaRizos(nx * escala, ny * escala, octavas, 0.5, tiempo);
            
            // Normalizar y aplicar intensidad
            v = (v + 1) / 2;
            
            // Acentuar contraste
            if (ctx.ModoCaos)
            {
                v = Math.Pow(v, 1.5);
            }
            
            return Matematica.Clamp01(v * ctx.Intensidad);
        }
        
        // NUEVO: Patrones celulares reacción-difusión
        public static double Celular(double nx, double ny, ContextoVisual ctx)
        {
            var rnd = new Random(ctx.Semilla);
            int celdasX = (int)(5 * ctx.Complejidad) + 3;
            int celdasY = (int)(5 * ctx.Complejidad) + 3;
            
            double cellW = 1.0 / celdasX;
            double cellH = 1.0 / celdasY;
            
            int cx = (int)(nx * celdasX);
            int cy = (int)(ny * celdasY);
            
            double localX = (nx - cx * cellW) / cellW;
            double localY = (ny - cy * cellH) / cellH;
            
            // Patrón de reacción-difusión simplificado (Gray-Scott-like)
            double u = 1.0;
            double v = 0.0;
            
            // Semilla inicial basada en posición
            if ((cx + cy) % 3 == 0 && localX > 0.4 && localX < 0.6 && localY > 0.4 && localY < 0.6)
            {
                v = 0.5 + rnd.NextDouble() * 0.5;
            }
            
            // Iteraciones de difusión
            int iter = (int)(ctx.Iteraciones / 10.0) + 5;
            double du = 0.0, dv = 0.0;
            
            for (int i = 0; i < iter; i++)
            {
                double lapU = -4 * u;
                double lapV = -4 * v;
                
                // Laplaciano simplificado
                lapU += (Math.Sin(localX * Math.PI * 2) + Math.Cos(localY * Math.PI * 2)) * 0.5;
                lapV += (Math.Cos(localX * Math.PI * 2) + Math.Sin(localY * Math.PI * 2)) * 0.5;
                
                double f = 0.055;
                double k = 0.062;
                double ru = 0.2;
                double rv = 0.1;
                
                du = ru * lapU - u * v * v + f * (1.0 - u);
                dv = rv * lapV + u * v * v - (f + k) * v;
                
                u += du * 0.1;
                v += dv * 0.1;
                
                u = Matematica.Clamp01(u);
                v = Matematica.Clamp01(v);
            }
            
            return v * ctx.Intensidad;
        }
        
        // NUEVO: Ray Marching 2D simplificado
        public static double RayMarching(double nx, double ny, ContextoVisual ctx)
        {
            double cx = nx - 0.5;
            double cy = ny - 0.5;
            
            // Cámara
            double roX = 0.5;
            double roY = 0.5;
            double roZ = -1.0;
            
            // Dirección del rayo
            double rdX = cx * 2.0;
            double rdY = cy * 2.0;
            double rdZ = 1.0;
            
            // Normalizar
            double len = Math.Sqrt(rdX*rdX + rdY*rdY + rdZ*rdZ);
            rdX /= len; rdY /= len; rdZ /= len;
            
            double t = 0.0;
            double densidad = 0.0;
            int pasos = (int)(ctx.Iteraciones / 5.0) + 10;
            
            for (int i = 0; i < pasos && t < 5.0; i++)
            {
                double pX = roX + rdX * t;
                double pY = roY + rdY * t;
                double pZ = roZ + rdZ * t;
                
                // SDF de escena: esfera + ruido
                double sphere = Math.Sqrt(pX*pX + pY*pY + pZ*pZ) - 0.5;
                double ruido = Matematica.Simplex(pX * ctx.Escala, pY * ctx.Escala) * 0.1;
                double d = sphere + ruido;
                
                if (d < 0.01)
                {
                    densidad += 0.1 * (1.0 - t / 5.0);
                }
                
                t += Math.Max(0.01, Math.Abs(d));
            }
            
            return Matematica.Clamp01(densidad * ctx.Intensidad);
        }
        
        // NUEVO: Superficies implícitas
        public static double Implicitas(double nx, double ny, ContextoVisual ctx)
        {
            double x = (nx - 0.5) * ctx.Escala * 4.0;
            double y = (ny - 0.5) * ctx.Escala * 4.0;
            
            // Metaballs
            var rnd = new Random(ctx.Semilla);
            int numBalls = (int)(ctx.Complejidad * 5.0) + 3;
            double suma = 0.0;
            
            for (int i = 0; i < numBalls; i++)
            {
                double bx = (rnd.NextDouble() - 0.5) * 3.0;
                double by = (rnd.NextDouble() - 0.5) * 3.0;
                double r = 0.3 + rnd.NextDouble() * 0.4;
                
                double dx = x - bx;
                double dy = y - by;
                double d2 = dx*dx + dy*dy;
                
                suma += (r * r) / d2;
            }
            
            double v = Matematica.Clamp01(suma / 2.0);
            
            // Añadir detalle de ruido
            double detalle = Matematica.Simplex(x * 2.0, y * 2.0) * 0.1;
            v += detalle;
            
            return Matematica.Clamp01(v * ctx.Intensidad);
        }
    }

    // ═══════════════════════════════════════════════════
    //  GENERADOR MUNDO VOXEL MINECRAFT MEJORADO
    // ═══════════════════════════════════════════════════

    public class GeneradorMundoVoxel
    {
        private ContextoVisual ctx;
        private Random rnd;
        private int[,] alturaMapa;
        private int[,] tipoBloque;
        private int[,] bioma;
        private int chunksX, chunksZ;
        private double[,] temperatura;
        private double[,] humedad;
        
        // Paleta Minecraft expandida
        private Color colorAgua = Color.FromArgb(64, 164, 255);
        private Color colorArena = Color.FromArgb(219, 211, 160);
        private Color colorTierra = Color.FromArgb(134, 96, 67);
        private Color colorPiedra = Color.FromArgb(128, 128, 128);
        private Color colorNieve = Color.FromArgb(255, 255, 255);
        private Color colorPasto = Color.FromArgb(92, 164, 68);
        private Color colorPastoSeco = Color.FromArgb(150, 160, 80);
        private Color colorMadera = Color.FromArgb(184, 148, 95);
        private Color colorHoja = Color.FromArgb(50, 110, 40);
        private Color colorHojaSeca = Color.FromArgb(180, 140, 50);
        private Color colorCactus = Color.FromArgb(20, 140, 20);
        private Color colorLadrillo = Color.FromArgb(180, 99, 86);
        private Color colorObsidiana = Color.FromArgb(15, 11, 22);
        private Color colorDiamante = Color.FromArgb(100, 219, 237);
        private Color colorOro = Color.FromArgb(255, 215, 0);
        private Color colorHielo = Color.FromArgb(200, 240, 255);
        private Color colorLava = Color.FromArgb(255, 90, 0);
        private Color colorMusgo = Color.FromArgb(80, 120, 60);

        public GeneradorMundoVoxel(ContextoVisual contexto)
        {
            ctx = contexto;
            rnd = new Random(ctx.Semilla);
            chunksX = 20;
            chunksZ = 20;
        }

        public Bitmap Generar(int anchoImg, int altoImg)
        {
            Bitmap bmp = new Bitmap(anchoImg, altoImg, PixelFormat.Format24bppRgb);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                // Gradiente de cielo mejorado
                DibujarCielo(g, anchoImg, altoImg);
                
                GenerarMapaAltura();
                GenerarBiomas();
                DibujarMundoVoxel(g, anchoImg, altoImg);
                DibujarNubes(g, anchoImg, altoImg);
                DibujarSol(g, anchoImg, altoImg);
                
                // Efecto atmosférico
                if (ctx.ModoSuave)
                {
                    DibujarNiebla(g, anchoImg, altoImg);
                }
            }
            return bmp;
        }
        
        private void DibujarCielo(Graphics g, int ancho, int alto)
        {
            Color arriba = ctx.ModoOscuro ? Color.FromArgb(10, 10, 30) : Color.FromArgb(135, 206, 235);
            Color abajo = ctx.ModoOscuro ? Color.FromArgb(30, 30, 60) : Color.FromArgb(200, 230, 255);
            
            using (Brush brush = new LinearGradientBrush(
                new Rectangle(0, 0, ancho, alto), arriba, abajo, 90f))
            {
                g.FillRectangle(brush, 0, 0, ancho, alto);
            }
            
            // Estrellas si es oscuro
            if (ctx.ModoOscuro)
            {
                for (int i = 0; i < 100; i++)
                {
                    int x = rnd.Next(ancho);
                    int y = rnd.Next(alto / 2);
                    int size = rnd.Next(1, 3);
                    g.FillEllipse(Brushes.White, x, y, size, size);
                }
            }
        }

        private void GenerarMapaAltura()
        {
            alturaMapa = new int[chunksX, chunksZ];
            tipoBloque = new int[chunksX, chunksZ];
            bioma = new int[chunksX, chunksZ];
            temperatura = new double[chunksX, chunksZ];
            humedad = new double[chunksX, chunksZ];
            
            // Generar temperatura y humedad para biomas
            for (int x = 0; x < chunksX; x++)
            {
                for (int z = 0; z < chunksZ; z++)
                {
                    double nx = x * 0.1;
                    double nz = z * 0.1;
                    temperatura[x, z] = Matematica.FBM(nx + 100, nz, 2, 0.5);
                    humedad[x, z] = Matematica.FBM(nx, nz + 200, 2, 0.5);
                }
            }
            
            for (int x = 0; x < chunksX; x++)
            {
                for (int z = 0; z < chunksZ; z++)
                {
                    double nx = x * 0.3 * ctx.Escala + rnd.NextDouble() * 0.1;
                    double nz = z * 0.3 * ctx.Escala + rnd.NextDouble() * 0.1;
                    
                    // Altura base con múltiples octavas
                    double altura = Matematica.FBM(nx, nz, 4, 0.5);
                    altura += Matematica.FBM(nx * 2, nz * 2, 2, 0.5) * 0.5;
                    
                    int alturaNormalizada = (int)((altura + 1) * 4 * ctx.Intensidad) + 2;
                    alturaNormalizada = Math.Max(1, Math.Min(14, alturaNormalizada));
                    alturaMapa[x, z] = alturaNormalizada;
                    
                    // Determinar bioma
                    double temp = temperatura[x, z];
                    double hum = humedad[x, z];
                    
                    if (temp < -0.3) bioma[x, z] = 0; // Nieve
                    else if (temp > 0.4 && hum < -0.2) bioma[x, z] = 1; // Desierto
                    else if (hum > 0.3) bioma[x, z] = 2; // Bosque
                    else bioma[x, z] = 3; // Llanura
                    
                    // Tipo de bloque según altura y bioma
                    if (alturaNormalizada < 3) tipoBloque[x, z] = 0; // Agua
                    else if (alturaNormalizada < 5) 
                    {
                        tipoBloque[x, z] = bioma[x, z] == 1 ? 1 : 1; // Arena o grava
                    }
                    else if (alturaNormalizada < 10) 
                    {
                        tipoBloque[x, z] = bioma[x, z] == 0 ? 4 : (bioma[x, z] == 1 ? 5 : 2); // Nieve, pasto seco, pasto
                    }
                    else if (alturaNormalizada < 13) tipoBloque[x, z] = 3; // Piedra
                    else tipoBloque[x, z] = 4; // Nieve/pico
                }
            }
            
            // Vegetación y estructuras mejoradas
            for (int x = 2; x < chunksX - 2; x++)
            {
                for (int z = 2; z < chunksZ - 2; z++)
                {
                    int tipo = tipoBloque[x, z];
                    int bio = bioma[x, z];
                    
                    // Árboles según bioma
                    if ((tipo == 2 || tipo == 5) && rnd.NextDouble() < 0.12)
                    {
                        tipoBloque[x, z] = 10; // Árbol
                    }
                    // Cactus en desierto
                    else if (bio == 1 && tipo == 1 && rnd.NextDouble() < 0.08)
                    {
                        tipoBloque[x, z] = 11; // Cactus
                    }
                    // Obsidiana en cuevas bajas
                    else if (tipo == 3 && rnd.NextDouble() < 0.03)
                    {
                        tipoBloque[x, z] = 12; // Obsidiana
                    }
                    // Diamante expuesto
                    else if (tipo == 2 && rnd.NextDouble() < 0.015)
                    {
                        tipoBloque[x, z] = 13; // Diamante
                    }
                    // Oro
                    else if (tipo == 3 && rnd.NextDouble() < 0.02)
                    {
                        tipoBloque[x, z] = 14; // Oro
                    }
                    // Lava subterránea
                    else if (tipo == 0 && rnd.NextDouble() < 0.05 && alturaMapa[x,z] < 2)
                    {
                        tipoBloque[x, z] = 15; // Lava
                    }
                }
            }
        }
        
        private void GenerarBiomas()
        {
            // Post-procesamiento de biomas para transiciones suaves
            for (int x = 1; x < chunksX - 1; x++)
            {
                for (int z = 1; z < chunksZ - 1; z++)
                {
                    // Suavizar bordes de bioma
                    if (bioma[x,z] != bioma[x+1,z] || bioma[x,z] != bioma[x,z+1])
                    {
                        if (rnd.NextDouble() < 0.3)
                        {
                            // Zona de transición
                            if (tipoBloque[x,z] == 2) tipoBloque[x,z] = 5; // Pasto seco
                        }
                    }
                }
            }
        }

        private void DibujarMundoVoxel(Graphics g, int anchoImg, int altoImg)
        {
            int tileWidth = 24;
            int tileHeight = 12;
            int offsetX = anchoImg / 2;
            int offsetY = altoImg / 3;
            
            // Sombra ambiental
            Color sombraAmbiental = ctx.ModoOscuro ? Color.FromArgb(50, 0, 0, 0) : Color.FromArgb(30, 0, 0, 0);
            
            // Painter's algorithm: de atrás adelante y de arriba abajo
            for (int z = 0; z < chunksZ; z++)
            {
                for (int x = chunksX - 1; x >= 0; x--)
                {
                    int isoX = offsetX + (x - z) * tileWidth / 2;
                    int isoY = offsetY + (x + z) * tileHeight / 2;
                    
                    int altura = alturaMapa[x, z];
                    int tipo = tipoBloque[x, z];
                    int bio = bioma[x, z];
                    
                    // Columna de bloques
                    for (int h = 0; h < altura; h++)
                    {
                        int drawY = isoY - h * tileHeight;
                        Color colorBloque = ObtenerColorBloque(tipo, h, altura, bio);
                        
                        // Sombra según profundidad
                        if (h < altura - 1)
                        {
                            colorBloque = Oscurecer(colorBloque, 10);
                        }
                        
                        DibujarBloque(g, isoX, drawY, tileWidth, tileHeight, colorBloque);
                    }
                    
                    // Agua con transparencia
                    if (altura < 3)
                    {
                        for (int h = altura; h < 3; h++)
                        {
                            int drawY = isoY - h * tileHeight;
                            DibujarBloqueTransparente(g, isoX, drawY, tileWidth, tileHeight, colorAgua, 160);
                        }
                    }
                    
                    // Vegetación y especiales
                    int ySuperficie = isoY - altura * tileHeight;
                    if (tipo == 10) DibujarArbol(g, isoX, ySuperficie, tileWidth, tileHeight, bio);
                    else if (tipo == 11) DibujarCactus(g, isoX, ySuperficie, tileWidth, tileHeight);
                    else if (tipo == 12) DibujarBloque(g, isoX, ySuperficie, tileWidth, tileHeight, colorObsidiana);
                    else if (tipo == 13) DibujarBloque(g, isoX, ySuperficie, tileWidth, tileHeight, colorDiamante);
                    else if (tipo == 14) DibujarBloque(g, isoX, ySuperficie - tileHeight, tileWidth, tileHeight, colorOro);
                    else if (tipo == 15) DibujarBloque(g, isoX, ySuperficie, tileWidth, tileHeight, colorLava);
                }
            }
        }

        private Color ObtenerColorBloque(int tipoSuperficie, int nivel, int alturaTotal, int bioma)
        {
            bool esSuperficie = (nivel == alturaTotal - 1);
            if (!esSuperficie)
            {
                if (nivel > alturaTotal - 4) return colorTierra;
                return colorPiedra;
            }
            
            switch (tipoSuperficie)
            {
                case 0: return colorArena;
                case 1: return colorArena;
                case 2: return bioma == 2 ? colorMusgo : colorPasto;
                case 3: return colorPiedra;
                case 4: return colorNieve;
                case 5: return colorPastoSeco;
                default: return colorTierra;
            }
        }

        private void DibujarBloque(Graphics g, int x, int y, int w, int h, Color color)
        {
            Point[] superior = new Point[]
            {
                new Point(x, y - h/2),
                new Point(x + w/2, y - h),
                new Point(x + w, y - h/2),
                new Point(x + w/2, y)
            };
            
            Point[] izquierda = new Point[]
            {
                new Point(x, y - h/2),
                new Point(x + w/2, y),
                new Point(x + w/2, y + h),
                new Point(x, y + h/2)
            };
            
            Point[] derecha = new Point[]
            {
                new Point(x + w/2, y),
                new Point(x + w, y - h/2),
                new Point(x + w, y + h/2),
                new Point(x + w/2, y + h)
            };
            
            Color colorSuperior = Aclarar(color, 25);
            Color colorIzquierda = color;
            Color colorDerecha = Oscurecer(color, 30);
            
            using (Brush brushSup = new SolidBrush(colorSuperior))
            using (Brush brushIzq = new SolidBrush(colorIzquierda))
            using (Brush brushDer = new SolidBrush(colorDerecha))
            {
                g.FillPolygon(brushSup, superior);
                g.FillPolygon(brushIzq, izquierda);
                g.FillPolygon(brushDer, derecha);
            }
            
            // Borde sutil
            using (Pen pen = new Pen(Oscurecer(color, 50), 1))
            {
                g.DrawPolygon(pen, superior);
                g.DrawPolygon(pen, izquierda);
                g.DrawPolygon(pen, derecha);
            }
        }

        private void DibujarBloqueTransparente(Graphics g, int x, int y, int w, int h, Color color, int alpha)
        {
            Color transSuperior = Color.FromArgb(alpha, Aclarar(color, 20));
            Color transIzquierda = Color.FromArgb(alpha, color);
            Color transDerecha = Color.FromArgb(alpha, Oscurecer(color, 30));
            
            Point[] superior = new Point[]
            {
                new Point(x, y - h/2),
                new Point(x + w/2, y - h),
                new Point(x + w, y - h/2),
                new Point(x + w/2, y)
            };
            
            Point[] izquierda = new Point[]
            {
                new Point(x, y - h/2),
                new Point(x + w/2, y),
                new Point(x + w/2, y + h),
                new Point(x, y + h/2)
            };
            
            Point[] derecha = new Point[]
            {
                new Point(x + w/2, y),
                new Point(x + w, y - h/2),
                new Point(x + w, y + h/2),
                new Point(x + w/2, y + h)
            };
            
            using (Brush brushSup = new SolidBrush(transSuperior))
            using (Brush brushIzq = new SolidBrush(transIzquierda))
            using (Brush brushDer = new SolidBrush(transDerecha))
            {
                g.FillPolygon(brushSup, superior);
                g.FillPolygon(brushIzq, izquierda);
                g.FillPolygon(brushDer, derecha);
            }
        }

        private void DibujarArbol(Graphics g, int x, int y, int w, int h, int bioma)
        {
            Color madera = colorMadera;
            Color hojas = bioma == 2 ? colorHoja : (bioma == 1 ? colorHojaSeca : colorHoja);
            
            // Tronco más alto
            int alturaTronco = 2 + rnd.Next(2);
            for (int i = 0; i < alturaTronco; i++)
            {
                DibujarBloque(g, x, y - i * h, w, h, madera);
            }
            
            // Copa del árbol más detallada
            int copaY = y - alturaTronco * h;
            int copaSize = 1 + rnd.Next(2);
            
            for (int cx = -copaSize; cx <= copaSize; cx++)
            {
                for (int cz = -copaSize; cz <= copaSize; cz++)
                {
                    if (Math.Abs(cx) + Math.Abs(cz) <= copaSize + 1 && rnd.NextDouble() > 0.2)
                    {
                        int dx = cx * w / 2;
                        int dz = cz * w / 4;
                        DibujarBloque(g, x + dx - dz, copaY - Math.Abs(cx) * h / 2 - Math.Abs(cz) * h / 4, w, h, hojas);
                    }
                }
            }
        }

        private void DibujarCactus(Graphics g, int x, int y, int w, int h)
        {
            int alturaCactus = 2 + rnd.Next(3);
            for (int i = 0; i < alturaCactus; i++)
            {
                DibujarBloque(g, x, y - i * h, w, h, colorCactus);
            }
            
            // Brazos del cactus
            if (alturaCactus > 2 && rnd.NextDouble() < 0.5)
            {
                int brazoAltura = rnd.Next(alturaCactus - 1);
                int lado = rnd.Next(2) == 0 ? -1 : 1;
                DibujarBloque(g, x + lado * w / 2, y - brazoAltura * h, w, h, colorCactus);
                if (rnd.NextDouble() < 0.3)
                {
                    DibujarBloque(g, x + lado * w / 2, y - (brazoAltura + 1) * h, w, h, colorCactus);
                }
            }
        }

        private void DibujarNubes(Graphics g, int anchoImg, int altoImg)
        {
            int numNubes = (int)(8 * ctx.Intensidad);
            
            for (int i = 0; i < numNubes; i++)
            {
                int nx = rnd.Next(anchoImg);
                int ny = rnd.Next(altoImg / 4);
                int tamaño = 40 + rnd.Next(80);
                
                // Nubes más realistas con múltiples elipses
                using (Brush brush = new SolidBrush(Color.FromArgb(180, 255, 255, 255)))
                using (Brush brushSombra = new SolidBrush(Color.FromArgb(100, 200, 200, 220)))
                {
                    // Cuerpo principal
                    g.FillEllipse(brush, nx, ny, tamaño, tamaño / 2);
                    
                    // Bultos
                    g.FillEllipse(brush, nx + tamaño/4, ny - tamaño/6, tamaño/2, tamaño/3);
                    g.FillEllipse(brushSombra, nx + tamaño/3, ny + tamaño/4, tamaño/3, tamaño/4);
                    
                    if (rnd.NextDouble() < 0.5)
                    {
                        g.FillEllipse(brush, nx - tamaño/6, ny + tamaño/8, tamaño/3, tamaño/4);
                    }
                }
            }
        }

        private void DibujarSol(Graphics g, int anchoImg, int altoImg)
        {
            int solX = anchoImg - 120;
            int solY = 80;
            
            // Sol con glow
            for (int r = 60; r > 0; r -= 3)
            {
                int alpha = (int)(50 * (1.0 - r / 60.0));
                int amarillo = 255;
                int rojo = 255 - (int)((60 - r) * 2.5);
                using (Brush brush = new SolidBrush(Color.FromArgb(alpha, 255, Math.Max(100, rojo), 0)))
                {
                    g.FillEllipse(brush, solX - r, solY - r, r * 2, r * 2);
                }
            }
            
            // Núcleo del sol
            for (int r = 25; r > 0; r -= 2)
            {
                int amarillo = 255;
                int rojo = 255 - (25 - r) * 3;
                using (Brush brush = new SolidBrush(Color.FromArgb(255, 255, Math.Max(0, rojo), 50)))
                {
                    g.FillEllipse(brush, solX - r, solY - r, r * 2, r * 2);
                }
            }
        }
        
        private void DibujarNiebla(Graphics g, int anchoImg, int altoImg)
        {
            using (Brush brush = new SolidBrush(Color.FromArgb(40, 200, 220, 240)))
            {
                g.FillRectangle(brush, 0, altoImg / 2, anchoImg, altoImg / 2);
            }
        }

        private Color Aclarar(Color c, int porcentaje)
        {
            int r = Math.Min(255, c.R + (255 - c.R) * porcentaje / 100);
            int g = Math.Min(255, c.G + (255 - c.G) * porcentaje / 100);
            int b = Math.Min(255, c.B + (255 - c.B) * porcentaje / 100);
            return Color.FromArgb(r, g, b);
        }

        private Color Oscurecer(Color c, int porcentaje)
        {
            int r = c.R * (100 - porcentaje) / 100;
            int g = c.G * (100 - porcentaje) / 100;
            int b = c.B * (100 - porcentaje) / 100;
            return Color.FromArgb(r, g, b);
        }
    }

    // ═══════════════════════════════════════════════════
    //  MOTOR PRINCIPAL DE RENDERIZADO MEJORADO
    // ═══════════════════════════════════════════════════

    public static class MotorOpenCIP
    {
        public static Bitmap Renderizar(int ancho, int alto, ContextoVisual ctx, IProgress<int> progreso)
        {
            // Caso especial para Minecraft
            if (ctx.Algoritmos.Count == 1 && ctx.Algoritmos[0] == AlgoritmoBase.MundoVoxelMinecraft)
            {
                var genVoxel = new GeneradorMundoVoxel(ctx);
                if (progreso != null) progreso.Report(50);
                var result = genVoxel.Generar(ancho, alto);
                if (progreso != null) progreso.Report(100);
                return result;
            }

            Matematica.InicializarSemilla(ctx.Semilla);

            var bmp = new Bitmap(ancho, alto, PixelFormat.Format24bppRgb);
            var rect = new Rectangle(0, 0, ancho, alto);
            var bmpData = bmp.LockBits(rect, ImageLockMode.WriteOnly, bmp.PixelFormat);
            int stride = Math.Abs(bmpData.Stride);
            var pixels = new byte[stride * alto];

            var paleta = ctx.Paleta.Count > 0 ? ctx.Paleta : new List<Color> { Color.Black, Color.White };
            int procesados = 0;
            object lok = new object();

            Parallel.For(0, alto, y =>
            {
                double ny = (double)y / alto;
                double sy = ctx.ModoSimetrico ? ((ny < 0.5) ? ny * 2 : (1 - ny) * 2) : ny;

                for (int x = 0; x < ancho; x++)
                {
                    double nx = (double)x / ancho;
                    double sx = ctx.ModoSimetrico ? ((nx < 0.5) ? nx * 2 : (1 - nx) * 2) : nx;

                    double valor = 0;
                    double pesoTotal = 0;
                    
                    for (int a = 0; a < ctx.Algoritmos.Count; a++)
                    {
                        double v = EjecutarAlgoritmo(ctx.Algoritmos[a], sx, sy, ctx);
                        valor += v * ctx.PesosAlgoritmos[a];
                        pesoTotal += ctx.PesosAlgoritmos[a];
                    }
                    
                    if (pesoTotal > 0)
                        valor /= pesoTotal;
                        
                    valor = Matematica.Clamp01(valor);

                    if (ctx.ModoOscuro) 
                    {
                        // Curva de gamma para oscurecer
                        valor = Math.Pow(valor, 1.5);
                    }

                    Color c = Matematica.MutiStop(paleta, valor);

                    if (Math.Abs(ctx.Saturacion - 1.0) > 0.01)
                        c = Matematica.AjustarSaturacion(c, ctx.Saturacion);

                    if (ctx.ModoRetro) c = CuantizarRetro(c, 6);
                    
                    // Dithering para suavizar bandas de color
                    if (ctx.ModoSuave && ctx.ModoRetro)
                    {
                        c = AplicarDithering(c, x, y);
                    }

                    int off = y * stride + x * 3;
                    pixels[off]     = c.B;
                    pixels[off + 1] = c.G;
                    pixels[off + 2] = c.R;
                }

                lock (lok)
                {
                    procesados++;
                    if (progreso != null && procesados % 20 == 0)
                        progreso.Report(procesados * 100 / alto);
                }
            });

            Marshal.Copy(pixels, 0, bmpData.Scan0, pixels.Length);
            bmp.UnlockBits(bmpData);
            return bmp;
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
                case AlgoritmoBase.RuidoSimplex:        return Generadores.SimplexMejorado(nx, ny, ctx);
                case AlgoritmoBase.DominioWarping:      return Generadores.Warping(nx, ny, ctx);
                case AlgoritmoBase.Multifractal:        return Generadores.Multifractal(nx, ny, ctx);
                case AlgoritmoBase.TurbulenciaRizos:    return Generadores.Rizos(nx, ny, ctx);
                case AlgoritmoBase.PatronesCelulares:   return Generadores.Celular(nx, ny, ctx);
                case AlgoritmoBase.RayMarching2D:       return Generadores.RayMarching(nx, ny, ctx);
                case AlgoritmoBase.SuperficiesImplicitas: return Generadores.Implicitas(nx, ny, ctx);
                default: return 0;
            }
        }

        private static Color CuantizarRetro(Color c, int niveles)
        {
            double step = 255.0 / (niveles - 1);
            int r = (int)(Math.Round(c.R / step) * step);
            int g = (int)(Math.Round(c.G / step) * step);
            int b = (int)(Math.Round(c.B / step) * step);
            return Color.FromArgb(Math.Min(255, r), Math.Min(255, g), Math.Min(255, b));
        }
        
        private static Color AplicarDithering(Color c, int x, int y)
        {
            // Matriz de Bayer 2x2 simplificada
            int[,] bayer = new int[,] { {0, 2}, {3, 1} };
            int umbral = bayer[x % 2, y % 2];
            
            int r = Math.Min(255, c.R + umbral);
            int g = Math.Min(255, c.G + umbral);
            int b = Math.Min(255, c.B + umbral);
            
            return Color.FromArgb(r, g, b);
        }
    }

    // ═══════════════════════════════════════════════════
    //  CONTROL: PANEL DE CHIPS DE TAGS
    // ═══════════════════════════════════════════════════

    public class PanelTags : Control
    {
        private List<string> _tags = new List<string>();
        private List<Color> _coloresTags = new List<Color>();
        private static readonly Color[] PALETA_CHIPS = {
            Color.FromArgb(0,150,200), Color.FromArgb(180,60,200),
            Color.FromArgb(200,100,0), Color.FromArgb(0,160,100),
            Color.FromArgb(160,30,30), Color.FromArgb(80,80,200)
        };

        public PanelTags() { SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true); }

        public void SetTags(IEnumerable<string> tags)
        {
            _tags.Clear(); _coloresTags.Clear();
            int idx = 0;
            foreach (var t in tags) {
                _tags.Add(t);
                _coloresTags.Add(PALETA_CHIPS[idx % PALETA_CHIPS.Length]);
                idx++;
            }
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(Color.FromArgb(28, 28, 33));
            var g = e.Graphics; 
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var font = new Font("Segoe UI", 8f, FontStyle.Regular);
            int x = 4, y = 4, maxH = 0;
            for (int i = 0; i < _tags.Count; i++)
            {
                var sz = g.MeasureString(_tags[i], font);
                int w = (int)sz.Width + 14, h = (int)sz.Height + 8;
                if (x + w > Width - 4 && x > 4) { x = 4; y += maxH + 4; maxH = 0; }
                var rc = new Rectangle(x, y, w, h);
                using (var br = new SolidBrush(Color.FromArgb(60, _coloresTags[i].R, _coloresTags[i].G, _coloresTags[i].B)))
                    GraphicsExt.FillRoundedRect(g, br, rc, 4);
                using (var pen = new Pen(Color.FromArgb(180, _coloresTags[i].R, _coloresTags[i].G, _coloresTags[i].B), 1))
                    GraphicsExt.DrawRoundedRect(g, pen, rc, 4);
                g.DrawString(_tags[i], font, Brushes.White, x + 7, y + 4);
                x += w + 4; maxH = Math.Max(maxH, h);
            }
            font.Dispose();
        }
    }

    public static class GraphicsExt
    {
        public static void FillRoundedRect(Graphics g, Brush br, Rectangle r, int radius)
        {
            using (var path = CrearRounded(r, radius)) g.FillPath(br, path);
        }
        public static void DrawRoundedRect(Graphics g, Pen pen, Rectangle r, int radius)
        {
            using (var path = CrearRounded(r, radius)) g.DrawPath(pen, path);
        }
        private static System.Drawing.Drawing2D.GraphicsPath CrearRounded(Rectangle r, int rad)
        {
            var p = new System.Drawing.Drawing2D.GraphicsPath();
            p.AddArc(r.X, r.Y, rad*2, rad*2, 180, 90);
            p.AddArc(r.Right-rad*2, r.Y, rad*2, rad*2, 270, 90);
            p.AddArc(r.Right-rad*2, r.Bottom-rad*2, rad*2, rad*2, 0, 90);
            p.AddArc(r.X, r.Bottom-rad*2, rad*2, rad*2, 90, 90);
            p.CloseFigure(); return p;
        }
    }

    // ═══════════════════════════════════════════════════
    //  VENTANA PRINCIPAL MEJORADA
    // ═══════════════════════════════════════════════════

    public class VentanaPrincipal : Form
    {
        private TextBox txtPrompt;
        private Button  btnGenerar, btnGuardar, btnAleatorio, btnLimpiar;
        private PictureBox canvas;
        private Label   lblTitulo, lblInstruccion, lblInterpretado, lblEstado;
        private PanelTags panelTags;
        private ProgressBar barraProgreso;
        private NumericUpDown numSemilla;
        private CheckBox chkSemillaFija;
        private Panel   panelIzq, panelDer, panelPrompt, panelInfo;
        private Label   lblResumen;
        private TrackBar trackZoom;
        private Label   lblZoomVal;

        private Bitmap       _imagenActual;
        private ContextoVisual _ultimoCtx;
        private bool         _generando = false;
        private static readonly Color BG_DARK   = Color.FromArgb(18, 18, 22);
        private static readonly Color BG_PANEL  = Color.FromArgb(28, 28, 35);
        private static readonly Color BG_INPUT  = Color.FromArgb(40, 40, 50);
        private static readonly Color ACENTO    = Color.FromArgb(0, 190, 240);
        private static readonly Color TEXTO     = Color.FromArgb(220, 220, 230);
        private static readonly Color GRIS      = Color.FromArgb(100, 100, 115);

        public VentanaPrincipal()
        {
            InitComponent();
        }

        private void InitComponent()
        {
            this.Text            = "OpenCIP – Open CPU Image Painter";
            this.Size            = new Size(1200, 780);
            this.MinimumSize     = new Size(900, 620);
            this.StartPosition   = FormStartPosition.CenterScreen;
            this.BackColor       = BG_DARK;
            this.ForeColor       = TEXTO;
            this.DoubleBuffered  = true;

            panelIzq = new Panel {
                BackColor = BG_PANEL,
                Dock = DockStyle.Left,
                Width = 340,
                Padding = new Padding(0)
            };

            lblTitulo = new Label {
                Text = "OpenCIP",
                Font = new Font("Segoe UI", 22, FontStyle.Bold),
                ForeColor = ACENTO,
                Location = new Point(20, 18),
                AutoSize = true
            };

            var lblSubtitulo = new Label {
                Text = "Open CPU Image Painter",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),
                ForeColor = GRIS,
                Location = new Point(22, 52),
                AutoSize = true
            };

            var sep1 = CrearSeparador(20, 74, 300);

            lblInstruccion = new Label {
                Text = "Describe la imagen que quieres generar:",
                Font = new Font("Segoe UI", 9f, FontStyle.Regular),
                ForeColor = TEXTO,
                Location = new Point(20, 86),
                AutoSize = true
            };

            txtPrompt = new TextBox {
                Location    = new Point(20, 108),
                Size        = new Size(300, 90),
                Multiline   = true,
                BackColor   = BG_INPUT,
                ForeColor   = TEXTO,
                BorderStyle = BorderStyle.FixedSingle,
                Font        = new Font("Segoe UI", 10f),
                ScrollBars  = ScrollBars.Vertical,
                Text        = "espacio oscuro con nebulosa purpura y estrellas"
            };
            txtPrompt.KeyDown += new KeyEventHandler((s, e) => { 
                if (e.KeyCode == Keys.Enter && !e.Shift) { 
                    e.SuppressKeyPress = true; 
                    IniciarGeneracion(); 
                } 
            });

            var lblEjemplos = new Label {
                Text = "Ejemplos rapidos:",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),
                ForeColor = GRIS,
                Location = new Point(20, 205),
                AutoSize = true
            };

            var panelEjemplos = new FlowLayoutPanel {
                Location  = new Point(20, 222),
                Size      = new Size(300, 80),
                BackColor = Color.Transparent,
                Padding   = new Padding(0),
                Margin    = new Padding(0)
            };
            
            var ejemplos = new[] {
                "fuego intenso naranja",
                "bosque verde suave",
                "ocean azul tranquilo",
                "fractal mandelbrot oscuro",
                "plasma psicodelico neon",
                "mandala geometrico dorado",
                "minecraft mundo bloques",
                "voxel isometrico verde",
                "simplex warp detallado",
                "multifractal terreno",
                "celular patrones organico",
                "raymarching 3d abstracto"
            };
            
            foreach (var ej in ejemplos)
            {
                string captura = ej;
                var btn = new Button {
                    Text      = captura,
                    AutoSize  = false,
                    Size      = new Size(140, 24),
                    Margin    = new Padding(0, 0, 4, 4),
                    BackColor = BG_INPUT,
                    ForeColor = GRIS,
                    FlatStyle = FlatStyle.Flat,
                    Font      = new Font("Segoe UI", 7.5f),
                    Cursor    = Cursors.Hand
                };
                btn.FlatAppearance.BorderColor = Color.FromArgb(55, 55, 65);
                btn.Click += new EventHandler((s, e) => { txtPrompt.Text = captura; IniciarGeneracion(); });
                panelEjemplos.Controls.Add(btn);
            }

            var sep2 = CrearSeparador(20, 308, 300);

            var lblSemilla = new Label {
                Text = "Semilla:",
                Font = new Font("Segoe UI", 9f),
                ForeColor = TEXTO,
                Location = new Point(20, 320),
                AutoSize = true
            };

            chkSemillaFija = new CheckBox {
                Text      = "Fijar semilla",
                Location  = new Point(165, 318),
                Size      = new Size(110, 22),
                ForeColor = GRIS,
                BackColor = Color.Transparent,
                Font      = new Font("Segoe UI", 8.5f),
                Checked   = false
            };

            numSemilla = new NumericUpDown {
                Location  = new Point(20, 340),
                Size      = new Size(255, 26),
                Minimum   = 0,
                Maximum   = 999999,
                Value     = 42,
                BackColor = BG_INPUT,
                ForeColor = TEXTO,
                Font      = new Font("Segoe UI", 10f)
            };

            var lblZoom = new Label {
                Text = "Zoom / Escala:",
                Font = new Font("Segoe UI", 9f),
                ForeColor = TEXTO,
                Location = new Point(20, 376),
                AutoSize = true
            };

            lblZoomVal = new Label {
                Text = "1.0x",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = ACENTO,
                Location = new Point(275, 376),
                AutoSize = true
            };

            trackZoom = new TrackBar {
                Location = new Point(20, 392),
                Size     = new Size(300, 36),
                Minimum  = 1,
                Maximum  = 40,
                Value    = 10,
                TickFrequency = 5,
                BackColor = BG_PANEL
            };
            trackZoom.Scroll += new EventHandler((s, e) => lblZoomVal.Text = (trackZoom.Value / 10.0).ToString("0.0") + "x");

            var sep3 = CrearSeparador(20, 432, 300);

            btnGenerar = CrearBoton("GENERAR  (Enter)", new Point(20, 444), new Size(300, 42),
                Color.FromArgb(0, 130, 180), Color.White, true);
            btnGenerar.Click += new EventHandler((s, e) => IniciarGeneracion());

            btnAleatorio = CrearBoton("Aleatorio", new Point(20, 495), new Size(145, 32),
                Color.FromArgb(50, 50, 65), TEXTO, false);
            btnAleatorio.Click += new EventHandler((s, e) => AleatorioClick());

            btnLimpiar = CrearBoton("Limpiar", new Point(175, 495), new Size(145, 32),
                Color.FromArgb(50, 50, 65), TEXTO, false);
            btnLimpiar.Click += new EventHandler((s, e) => { txtPrompt.Text = ""; txtPrompt.Focus(); });

            btnGuardar = CrearBoton("Guardar imagen", new Point(20, 536), new Size(300, 32),
                Color.FromArgb(40, 40, 55), GRIS, false);
            btnGuardar.Enabled = false;
            btnGuardar.Click += new EventHandler((s, e) => GuardarImagen());

            var sep4 = CrearSeparador(20, 577, 300);

            var lblTagsTitulo = new Label {
                Text = "Interpretado como:",
                Font = new Font("Segoe UI", 9f),
                ForeColor = GRIS,
                Location = new Point(20, 588),
                AutoSize = true
            };

            panelTags = new PanelTags {
                Location = new Point(20, 607),
                Size     = new Size(300, 72),
            };

            lblResumen = new Label {
                Text      = "",
                Font      = new Font("Segoe UI", 8.5f, FontStyle.Italic),
                ForeColor = Color.FromArgb(140, 140, 160),
                Location  = new Point(20, 682),
                Size      = new Size(300, 40),
                AutoSize  = false
            };

            var panelEstado = new Panel {
                Dock      = DockStyle.Bottom,
                Height    = 26,
                BackColor = Color.FromArgb(22, 22, 28)
            };

            barraProgreso = new ProgressBar {
                Location  = new Point(0, 0),
                Size      = new Size(200, 26),
                Style     = ProgressBarStyle.Continuous,
                BackColor = Color.FromArgb(30, 30, 38)
            };

            lblEstado = new Label {
                Text      = "  Listo – escribe un prompt y presiona GENERAR",
                Dock      = DockStyle.Fill,
                ForeColor = GRIS,
                Font      = new Font("Segoe UI", 8.5f),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding   = new Padding(8, 0, 0, 0)
            };

            panelEstado.Controls.Add(lblEstado);
            panelEstado.Controls.Add(barraProgreso);

            canvas = new PictureBox {
                BackColor = Color.FromArgb(12, 12, 16),
                SizeMode  = PictureBoxSizeMode.Zoom,
                Anchor    = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                Location  = new Point(340, 0),
                Size      = new Size(this.ClientSize.Width - 340, this.ClientSize.Height - 26)
            };

            canvas.Paint += new PaintEventHandler((s, e) => {
                if (canvas.Image == null) DibujarPlaceholder(e.Graphics, canvas.ClientRectangle);
            });

            panelIzq.Controls.AddRange(new Control[] {
                lblTitulo, lblSubtitulo, sep1,
                lblInstruccion, txtPrompt, lblEjemplos, panelEjemplos,
                sep2, lblSemilla, chkSemillaFija, numSemilla,
                lblZoom, lblZoomVal, trackZoom,
                sep3, btnGenerar, btnAleatorio, btnLimpiar, btnGuardar,
                sep4, lblTagsTitulo, panelTags, lblResumen
            });

            this.Controls.Add(panelIzq);
            this.Controls.Add(canvas);
            this.Controls.Add(panelEstado);

            this.Resize += new EventHandler((s, e) => canvas.Size = new Size(
                Math.Max(100, this.ClientSize.Width - 340),
                Math.Max(100, this.ClientSize.Height - 26)));
        }

        private Panel CrearSeparador(int x, int y, int w)
        {
            return new Panel { Location = new Point(x, y), Size = new Size(w, 1), BackColor = Color.FromArgb(50, 50, 60) };
        }

        private Button CrearBoton(string texto, Point loc, Size sz, Color bg, Color fg, bool bold)
        {
            var b = new Button {
                Text      = texto,
                Location  = loc,
                Size      = sz,
                BackColor = bg,
                ForeColor = fg,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", bold ? 10.5f : 9f, bold ? FontStyle.Bold : FontStyle.Regular),
                Cursor    = Cursors.Hand
            };
            b.FlatAppearance.BorderSize  = 0;
            b.FlatAppearance.MouseOverBackColor = Color.FromArgb(Math.Min(255, bg.R+20), Math.Min(255, bg.G+20), Math.Min(255, bg.B+20));
            return b;
        }

        private void DibujarPlaceholder(Graphics g, Rectangle r)
        {
            g.Clear(Color.FromArgb(12, 12, 16));
            string msg = "Escribe un prompt y presiona GENERAR\npara que OpenCIP pinte tu imagen";
            var font = new Font("Segoe UI", 14, FontStyle.Regular);
            var sz   = g.MeasureString(msg, font, r.Width);
            var pt   = new PointF((r.Width - sz.Width) / 2f, (r.Height - sz.Height) / 2f);
            g.DrawString(msg, font, new SolidBrush(Color.FromArgb(55, 55, 65)), pt);
            font.Dispose();

            using (var pen = new Pen(Color.FromArgb(25, 25, 30), 1))
            {
                for (int x = 0; x < r.Width; x += 40) g.DrawLine(pen, x, 0, x, r.Height);
                for (int y = 0; y < r.Height; y += 40) g.DrawLine(pen, 0, y, r.Width, y);
            }
        }

        private async void IniciarGeneracion()
        {
            if (_generando) return;
            _generando = true;

            string prompt = txtPrompt.Text.Trim();
            int semilla = chkSemillaFija.Checked ? (int)numSemilla.Value : new Random().Next(1, 999999);
            numSemilla.Value = semilla;

            _ultimoCtx = InterpretadorPrompt.Interpretar(prompt, semilla);
            _ultimoCtx.Escala *= trackZoom.Value / 10.0;

            var tagsDisplay = new List<string>(_ultimoCtx.PalabrasDetectadas.Take(20));
            panelTags.SetTags(tagsDisplay);
            
            string resumen = _ultimoCtx.ResumenVisual.Trim();
            if (resumen.Length == 0) resumen = "patron general";
            lblResumen.Text = "-> " + resumen + "  |  algos: " + string.Join("+", _ultimoCtx.Algoritmos.Select(a => a.ToString().Substring(0, Math.Min(5, a.ToString().Length))).ToArray());

            btnGenerar.Enabled = false;
            btnGenerar.Text    = "Generando...";
            btnGuardar.Enabled = false;
            barraProgreso.Value = 0;
            
            string promptCorto = prompt.Length > 40 ? prompt.Substring(0, 40) + "..." : prompt;
            lblEstado.Text = string.Format("  Interpretando «{0}»...", promptCorto);

            var prog = new Progress<int>(p => {
                if (this.IsDisposed) return;
                try {
                    this.Invoke(new Action(() => {
                        barraProgreso.Value = Math.Min(100, p);
                        lblEstado.Text = string.Format("  Generando... {0}%", p);
                    }));
                } catch { }
            });

            try
            {
                int w = Math.Max(canvas.Width, 512);
                int h = Math.Max(canvas.Height, 512);

                var ctx = _ultimoCtx;
                _imagenActual = await Task.Run(() => MotorOpenCIP.Renderizar(w, h, ctx, prog));

                canvas.Image   = _imagenActual;
                btnGuardar.Enabled = true;
                
                string resumenFinal = _ultimoCtx.ResumenVisual.Trim();
                if (resumenFinal.Length == 0) resumenFinal = "perlin base";
                lblEstado.Text = string.Format(
                    "  Imagen generada  {0}x{1}  |  semilla: {2}  |  {3}",
                    w, h, semilla, resumenFinal);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al generar:\n" + ex.Message, "OpenCIP", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblEstado.Text = "  Error al generar";
            }
            finally
            {
                _generando = false;
                btnGenerar.Enabled = true;
                btnGenerar.Text    = "GENERAR  (Enter)";
                barraProgreso.Value = 100;
            }
        }

        private void AleatorioClick()
        {
            var temas = new[] {
                "oceano azul tranquilo", "bosque verde oscuro", "galaxia purpura espacio",
                "fuego intenso naranja rojo", "fractal mandelbrot oscuro",
                "cristal cian simetrico", "plasma psicodelico neon",
                "mandala dorado geometrico", "nebulosa violeta espacio oscuro",
                "atardecer naranja rojo oscuro", "nieve azul suave",
                "lava oscura rojo intenso", "onda azul verde interference",
                "voronoi cristal frio", "sunset calm warm orange",
                "minecraft mundo bloques verde",
                "voxel isometrico desierto",
                "simplex warp detallado",
                "multifractal terreno erosionado",
                "celular patrones organicos",
                "raymarching esferas abstracto",
                "rizos turbulencia fluido"
            };
            var rnd = new Random();
            txtPrompt.Text = temas[rnd.Next(temas.Length)];
            IniciarGeneracion();
        }

        private void GuardarImagen()
        {
            if (_imagenActual == null) return;
            using (var dlg = new SaveFileDialog())
            {
                dlg.Filter     = "PNG (*.png)|*.png|JPEG (*.jpg)|*.jpg|BMP (*.bmp)|*.bmp";
                dlg.FileName   = string.Format("OpenCIP_{0}_{1}.png",
                    numSemilla.Value, DateTime.Now.ToString("yyyyMMdd_HHmmss"));
                dlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    ImageFormat fmt = ImageFormat.Png;
                    string ext = Path.GetExtension(dlg.FileName).ToLower();
                    if (ext == ".jpg") fmt = ImageFormat.Jpeg;
                    else if (ext == ".bmp") fmt = ImageFormat.Bmp;
                    _imagenActual.Save(dlg.FileName, fmt);
                    lblEstado.Text = "  Guardado: " + Path.GetFileName(dlg.FileName);
                }
            }
        }
    }

    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new VentanaPrincipal());
        }
    }
}