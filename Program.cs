using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OpenCIP
{
	using TipoEntorno3D = OpenCIP.GeneradorLienzo.TipoEntorno3D;
	using Matematica = OpenCIP.GeneradorLienzo.Matematica;
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
        RedNeuralCPPN,
        Escena3D,
        Acuarela,
        DibujoLapiz
    }

    public enum TipoEscena
    {
        Oceano,
        Bosque,
        CampoFlores,
        Montana,
        Desierto,
        Noche,
        Paisaje3D,
        SelvaLluviosa,
        Tundra,
        Volcan,
        Lago,
        Pantano,
        Valle,
        Playa,
        CañonRocoso,
    }

    public enum TipoPincel
    {
        Redondo,
        Plano,
        Abanico,
        Detalle,
        Esponja,
        CerdaCerda,
        CuchilloPaleta,
        PlumaChinesa,
        Rollito,
        Difuminador,
    }

    public enum ModoFusion
    {
        Normal, Multiplicar, Pantalla, Superponer, Diferencia, Luminosidad
    }

    // ═══════════════════════════════════════════════════

    public class ContextoVisual
    {
        public List<AlgoritmoBase> Algoritmos        { get; set; }
        public List<float>         PesosAlgoritmos   { get; set; }
        public List<Color>         Paleta            { get; set; }
        public double              Escala            { get; set; }
        public double              Intensidad        { get; set; }
        public double              Complejidad       { get; set; }
        public int                 Semilla           { get; set; }
        public bool                ModoCaos          { get; set; }
        public bool                ModoSuave         { get; set; }
        public bool                ModoOscuro        { get; set; }
        public bool                ModoRetro         { get; set; }
        public bool                ModoSimetrico     { get; set; }
        public int                 Iteraciones       { get; set; }
        public double              Saturacion        { get; set; }
        public List<string>        PalabrasDetectadas{ get; set; }
        public string              ResumenVisual     { get; set; }
        public bool                EsProgresivo      { get; set; }
        public bool                ModoIAInicio      { get; set; }
        public bool                ModoIAPostProceso { get; set; }
        public bool                ModoLienzo        { get; set; }
        public bool                LienzoPostProceso { get; set; }
        public TipoEscena          EscenaLienzo      { get; set; }
        public TipoEntorno3D       Entorno3D         { get; set; }

        public bool   SinSol         { get; set; }
        public bool   ConIslas       { get; set; }
        public bool   ConLluvia      { get; set; }
        public bool   ConNieve       { get; set; }
        public bool   ConNiebla      { get; set; }
        public bool   ConArcoIris    { get; set; }
        public bool   ConPalmeras    { get; set; }
        public bool   ConRocas       { get; set; }
        public bool   ConTormenta    { get; set; }
        public bool   ConEstrellas   { get; set; }
        public string HoraDelDia     { get; set; }
        public string EstiloLapiz    { get; set; }

        public ContextoVisual()
        {
            Algoritmos         = new List<AlgoritmoBase>();
            PesosAlgoritmos    = new List<float>();
            Paleta             = new List<Color>();
            Escala             = 1.0;
            Intensidad         = 1.0;
            Complejidad        = 1.0;
            Semilla            = new Random().Next();
            ModoCaos           = false;
            ModoSuave          = false;
            ModoOscuro         = false;
            ModoRetro          = false;
            ModoSimetrico      = false;
            Iteraciones        = 150;
            Saturacion         = 1.0;
            PalabrasDetectadas = new List<string>();
            ResumenVisual      = "";
            EsProgresivo       = false;
            ModoIAInicio       = false;
            ModoIAPostProceso  = false;
            ModoLienzo         = false;
            LienzoPostProceso  = false;
            EscenaLienzo       = TipoEscena.Oceano;
            Entorno3D          = TipoEntorno3D.Esferas;
            SinSol             = false;
            ConIslas           = false;
            ConLluvia          = false;
            ConNieve           = false;
            ConNiebla          = false;
            ConArcoIris        = false;
            ConPalmeras        = false;
            ConRocas           = false;
            ConTormenta        = false;
            ConEstrellas       = false;
            HoraDelDia         = "dia";
            EstiloLapiz        = "lapiz";
        }
    }

    // ═══════════════════════════════════════════════════

    public static class BancoPalabras
    {
        public static readonly Dictionary<string, Color[]> MapaColores =
            new Dictionary<string, Color[]>(StringComparer.OrdinalIgnoreCase)
        {
            {"rojo",     new[]{ Color.DarkRed,       Color.Red,          Color.OrangeRed      }},
            {"red",      new[]{ Color.DarkRed,       Color.Red,          Color.OrangeRed      }},
            {"azul",     new[]{ Color.DarkBlue,      Color.RoyalBlue,    Color.CornflowerBlue }},
            {"blue",     new[]{ Color.DarkBlue,      Color.RoyalBlue,    Color.CornflowerBlue }},
            {"verde",    new[]{ Color.DarkGreen,     Color.LimeGreen,    Color.MediumSeaGreen }},
            {"green",    new[]{ Color.DarkGreen,     Color.LimeGreen,    Color.MediumSeaGreen }},
            {"amarillo", new[]{ Color.DarkGoldenrod, Color.Gold,         Color.Yellow         }},
            {"yellow",   new[]{ Color.DarkGoldenrod, Color.Gold,         Color.Yellow         }},
            {"naranja",  new[]{ Color.DarkOrange,    Color.Orange,       Color.Coral          }},
            {"orange",   new[]{ Color.DarkOrange,    Color.Orange,       Color.Coral          }},
            {"morado",   new[]{ Color.Indigo,        Color.DarkViolet,   Color.MediumPurple   }},
            {"purple",   new[]{ Color.Indigo,        Color.DarkViolet,   Color.MediumPurple   }},
            {"violeta",  new[]{ Color.DarkViolet,    Color.BlueViolet,   Color.Violet         }},
            {"violet",   new[]{ Color.DarkViolet,    Color.BlueViolet,   Color.Violet         }},
            {"rosa",     new[]{ Color.HotPink,       Color.DeepPink,     Color.LightPink      }},
            {"pink",     new[]{ Color.HotPink,       Color.DeepPink,     Color.LightPink      }},
            {"blanco",   new[]{ Color.White,         Color.WhiteSmoke,   Color.GhostWhite     }},
            {"white",    new[]{ Color.White,         Color.WhiteSmoke,   Color.GhostWhite     }},
            {"negro",    new[]{ Color.Black, Color.FromArgb(15,15,15), Color.FromArgb(30,30,30) }},
            {"black",    new[]{ Color.Black, Color.FromArgb(15,15,15), Color.FromArgb(30,30,30) }},
            {"dorado",   new[]{ Color.Goldenrod,     Color.Gold,         Color.DarkGoldenrod  }},
            {"gold",     new[]{ Color.Goldenrod,     Color.Gold,         Color.DarkGoldenrod  }},
            {"cian",     new[]{ Color.DarkCyan,      Color.Cyan,         Color.LightCyan      }},
            {"cyan",     new[]{ Color.DarkCyan,      Color.Cyan,         Color.LightCyan      }},
            {"turquesa", new[]{ Color.Teal,          Color.Turquoise,    Color.LightSeaGreen  }},
            {"turquoise",new[]{ Color.Teal,          Color.Turquoise,    Color.LightSeaGreen  }},
            {"plateado", new[]{ Color.Silver,        Color.LightGray,    Color.Gainsboro      }},
            {"silver",   new[]{ Color.Silver,        Color.LightGray,    Color.Gainsboro      }},
        };

        public static readonly Dictionary<string, Action<ContextoVisual>> MapaTemas =
            new Dictionary<string, Action<ContextoVisual>>(StringComparer.OrdinalIgnoreCase)
        {
            {"bosque",  delegate(ContextoVisual ctx) {
                AgregarAlgo(ctx, AlgoritmoBase.RuidoPerlin, 0.7f);
                AgregarAlgo(ctx, AlgoritmoBase.VoronoiCelular, 0.3f);
                ctx.Paleta.AddRange(new[]{Color.DarkGreen,Color.ForestGreen,Color.SaddleBrown,Color.LightYellow});
                ctx.ModoSuave=true; ctx.Escala=2.5; ctx.ResumenVisual+="bosque ";
            }},
            {"forest",  delegate(ContextoVisual ctx) {
                AgregarAlgo(ctx, AlgoritmoBase.RuidoPerlin, 0.7f);
                AgregarAlgo(ctx, AlgoritmoBase.VoronoiCelular, 0.3f);
                ctx.Paleta.AddRange(new[]{Color.DarkGreen,Color.ForestGreen,Color.SaddleBrown,Color.LightYellow});
                ctx.ModoSuave=true; ctx.Escala=2.5; ctx.ResumenVisual+="forest ";
            }},
            {"ocean",   delegate(ContextoVisual ctx) {
                AgregarAlgo(ctx, AlgoritmoBase.OndaInterferencia, 0.5f);
                AgregarAlgo(ctx, AlgoritmoBase.FluidoTurbulento, 0.5f);
                ctx.Paleta.AddRange(new[]{Color.Navy,Color.RoyalBlue,Color.DeepSkyBlue,Color.Aquamarine,Color.White});
                ctx.ModoSuave=true; ctx.Escala=1.8; ctx.ResumenVisual+="oceano ";
            }},
            {"oceano",  delegate(ContextoVisual ctx) {
                AgregarAlgo(ctx, AlgoritmoBase.OndaInterferencia, 0.5f);
                AgregarAlgo(ctx, AlgoritmoBase.FluidoTurbulento, 0.5f);
                ctx.Paleta.AddRange(new[]{Color.Navy,Color.RoyalBlue,Color.DeepSkyBlue,Color.Aquamarine,Color.White});
                ctx.ModoSuave=true; ctx.Escala=1.8; ctx.ResumenVisual+="oceano ";
            }},
            {"mar",     delegate(ContextoVisual ctx) {
                AgregarAlgo(ctx, AlgoritmoBase.OndaInterferencia, 0.6f);
                AgregarAlgo(ctx, AlgoritmoBase.FluidoTurbulento, 0.4f);
                ctx.Paleta.AddRange(new[]{Color.Navy,Color.SteelBlue,Color.CadetBlue,Color.White});
                ctx.ModoSuave=true; ctx.Escala=2.0; ctx.ResumenVisual+="mar ";
            }},
            {"sea",     delegate(ContextoVisual ctx) {
                AgregarAlgo(ctx, AlgoritmoBase.OndaInterferencia, 0.6f);
                AgregarAlgo(ctx, AlgoritmoBase.FluidoTurbulento, 0.4f);
                ctx.Paleta.AddRange(new[]{Color.Navy,Color.SteelBlue,Color.CadetBlue,Color.White});
                ctx.ModoSuave=true; ctx.Escala=2.0; ctx.ResumenVisual+="sea ";
            }},
            {"fuego",   delegate(ContextoVisual ctx) {
                AgregarAlgo(ctx, AlgoritmoBase.FluidoTurbulento, 0.6f);
                AgregarAlgo(ctx, AlgoritmoBase.PlasmaCaos, 0.4f);
                ctx.Paleta.AddRange(new[]{Color.DarkRed,Color.OrangeRed,Color.Orange,Color.Yellow,Color.White});
                ctx.ModoCaos=true; ctx.Intensidad=1.4; ctx.ResumenVisual+="fuego ";
            }},
            {"fire",    delegate(ContextoVisual ctx) {
                AgregarAlgo(ctx, AlgoritmoBase.FluidoTurbulento, 0.6f);
                AgregarAlgo(ctx, AlgoritmoBase.PlasmaCaos, 0.4f);
                ctx.Paleta.AddRange(new[]{Color.DarkRed,Color.OrangeRed,Color.Orange,Color.Yellow,Color.White});
                ctx.ModoCaos=true; ctx.Intensidad=1.4; ctx.ResumenVisual+="fire ";
            }},
            {"espacio", delegate(ContextoVisual ctx) {
                AgregarAlgo(ctx, AlgoritmoBase.NebulosaEspacial, 0.7f);
                AgregarAlgo(ctx, AlgoritmoBase.VoronoiCelular, 0.3f);
                ctx.Paleta.AddRange(new[]{Color.Black,Color.Indigo,Color.DarkViolet,Color.MidnightBlue,Color.White});
                ctx.ModoOscuro=true; ctx.Complejidad=1.5; ctx.ResumenVisual+="espacio ";
            }},
            {"space",   delegate(ContextoVisual ctx) {
                AgregarAlgo(ctx, AlgoritmoBase.NebulosaEspacial, 0.7f);
                AgregarAlgo(ctx, AlgoritmoBase.VoronoiCelular, 0.3f);
                ctx.Paleta.AddRange(new[]{Color.Black,Color.Indigo,Color.DarkViolet,Color.MidnightBlue,Color.White});
                ctx.ModoOscuro=true; ctx.Complejidad=1.5; ctx.ResumenVisual+="space ";
            }},
            {"galaxia", delegate(ContextoVisual ctx) {
                AgregarAlgo(ctx, AlgoritmoBase.NebulosaEspacial, 0.8f);
                AgregarAlgo(ctx, AlgoritmoBase.GeometricoSimetrico, 0.2f);
                ctx.Paleta.AddRange(new[]{Color.Black,Color.Navy,Color.DarkViolet,Color.DeepSkyBlue,Color.White});
                ctx.ModoOscuro=true; ctx.ModoSimetrico=true; ctx.Complejidad=2.0; ctx.ResumenVisual+="galaxia ";
            }},
            {"galaxy",  delegate(ContextoVisual ctx) {
                AgregarAlgo(ctx, AlgoritmoBase.NebulosaEspacial, 0.8f);
                AgregarAlgo(ctx, AlgoritmoBase.GeometricoSimetrico, 0.2f);
                ctx.Paleta.AddRange(new[]{Color.Black,Color.Navy,Color.DarkViolet,Color.DeepSkyBlue,Color.White});
                ctx.ModoOscuro=true; ctx.ModoSimetrico=true; ctx.Complejidad=2.0; ctx.ResumenVisual+="galaxy ";
            }},
            {"cristal", delegate(ContextoVisual ctx) {
                AgregarAlgo(ctx, AlgoritmoBase.VoronoiCelular, 0.6f);
                AgregarAlgo(ctx, AlgoritmoBase.FractalMandelbrot, 0.4f);
                ctx.Paleta.AddRange(new[]{Color.LightCyan,Color.Cyan,Color.LightBlue,Color.White,Color.Silver});
                ctx.ModoSimetrico=true; ctx.Complejidad=1.8; ctx.ResumenVisual+="cristal ";
            }},
            {"crystal", delegate(ContextoVisual ctx) {
                AgregarAlgo(ctx, AlgoritmoBase.VoronoiCelular, 0.6f);
                AgregarAlgo(ctx, AlgoritmoBase.FractalMandelbrot, 0.4f);
                ctx.Paleta.AddRange(new[]{Color.LightCyan,Color.Cyan,Color.LightBlue,Color.White,Color.Silver});
                ctx.ModoSimetrico=true; ctx.Complejidad=1.8; ctx.ResumenVisual+="crystal ";
            }},
            {"fractal", delegate(ContextoVisual ctx) {
                AgregarAlgo(ctx, AlgoritmoBase.FractalMandelbrot, 1.0f);
                ctx.Iteraciones=300; ctx.Complejidad=2.0; ctx.ResumenVisual+="fractal ";
            }},
            {"mandelbrot", delegate(ContextoVisual ctx) {
                AgregarAlgo(ctx, AlgoritmoBase.FractalMandelbrot, 1.0f);
                ctx.Iteraciones=500; ctx.Complejidad=3.0; ctx.ResumenVisual+="mandelbrot ";
            }},
            {"julia",   delegate(ContextoVisual ctx) {
                AgregarAlgo(ctx, AlgoritmoBase.FractalMandelbrot, 1.0f);
                ctx.Iteraciones=400; ctx.Complejidad=2.5; ctx.ModoCaos=true; ctx.ResumenVisual+="julia ";
            }},
            {"nube",    delegate(ContextoVisual ctx) {
                AgregarAlgo(ctx, AlgoritmoBase.RuidoPerlin, 1.0f);
                ctx.Paleta.AddRange(new[]{Color.White,Color.LightGray,Color.LightSteelBlue,Color.LightBlue});
                ctx.ModoSuave=true; ctx.Escala=1.5; ctx.ResumenVisual+="nubes ";
            }},
            {"cloud",   delegate(ContextoVisual ctx) {
                AgregarAlgo(ctx, AlgoritmoBase.RuidoPerlin, 1.0f);
                ctx.Paleta.AddRange(new[]{Color.White,Color.LightGray,Color.LightSteelBlue,Color.LightBlue});
                ctx.ModoSuave=true; ctx.Escala=1.5; ctx.ResumenVisual+="clouds ";
            }},
            {"niebla",  delegate(ContextoVisual ctx) {
                AgregarAlgo(ctx, AlgoritmoBase.RuidoPerlin, 1.0f);
                ctx.Paleta.AddRange(new[]{Color.DimGray,Color.Gray,Color.LightGray,Color.White});
                ctx.ModoSuave=true; ctx.Escala=1.2; ctx.Intensidad=0.6; ctx.ResumenVisual+="niebla ";
            }},
            {"fog",     delegate(ContextoVisual ctx) {
                AgregarAlgo(ctx, AlgoritmoBase.RuidoPerlin, 1.0f);
                ctx.Paleta.AddRange(new[]{Color.DimGray,Color.Gray,Color.LightGray,Color.White});
                ctx.ModoSuave=true; ctx.Escala=1.2; ctx.Intensidad=0.6; ctx.ResumenVisual+="fog ";
            }},
            {"lava",    delegate(ContextoVisual ctx) {
                AgregarAlgo(ctx, AlgoritmoBase.VoronoiCelular, 0.5f);
                AgregarAlgo(ctx, AlgoritmoBase.FluidoTurbulento, 0.5f);
                ctx.Paleta.AddRange(new[]{Color.Black,Color.DarkRed,Color.OrangeRed,Color.Orange});
                ctx.ModoCaos=true; ctx.Intensidad=1.3; ctx.ResumenVisual+="lava ";
            }},
            {"plasma",  delegate(ContextoVisual ctx) {
                AgregarAlgo(ctx, AlgoritmoBase.PlasmaCaos, 0.7f);
                AgregarAlgo(ctx, AlgoritmoBase.OndaInterferencia, 0.3f);
                ctx.Paleta.AddRange(new[]{Color.DarkViolet,Color.Cyan,Color.DeepPink,Color.Yellow});
                ctx.ModoCaos=true; ctx.Intensidad=1.5; ctx.Saturacion=1.8; ctx.ResumenVisual+="plasma ";
            }},
            {"mandala", delegate(ContextoVisual ctx) {
                AgregarAlgo(ctx, AlgoritmoBase.GeometricoSimetrico, 0.7f);
                AgregarAlgo(ctx, AlgoritmoBase.OndaInterferencia, 0.3f);
                ctx.ModoSimetrico=true; ctx.Complejidad=2.0; ctx.ResumenVisual+="mandala ";
            }},
            {"geometrico", delegate(ContextoVisual ctx) {
                AgregarAlgo(ctx, AlgoritmoBase.GeometricoSimetrico, 1.0f);
                ctx.ModoSimetrico=true; ctx.ResumenVisual+="geometrico ";
            }},
            {"geometric",  delegate(ContextoVisual ctx) {
                AgregarAlgo(ctx, AlgoritmoBase.GeometricoSimetrico, 1.0f);
                ctx.ModoSimetrico=true; ctx.ResumenVisual+="geometric ";
            }},
            {"psicodelico", delegate(ContextoVisual ctx) {
                AgregarAlgo(ctx, AlgoritmoBase.PlasmaCaos, 0.5f);
                AgregarAlgo(ctx, AlgoritmoBase.OndaInterferencia, 0.5f);
                ctx.ModoCaos=true; ctx.Saturacion=2.0; ctx.Intensidad=1.5; ctx.ResumenVisual+="psicodelico ";
            }},
            {"psychedelic", delegate(ContextoVisual ctx) {
                AgregarAlgo(ctx, AlgoritmoBase.PlasmaCaos, 0.5f);
                AgregarAlgo(ctx, AlgoritmoBase.OndaInterferencia, 0.5f);
                ctx.ModoCaos=true; ctx.Saturacion=2.0; ctx.Intensidad=1.5; ctx.ResumenVisual+="psychedelic ";
            }},
            {"nieve",   delegate(ContextoVisual ctx) {
                AgregarAlgo(ctx, AlgoritmoBase.VoronoiCelular, 0.6f);
                AgregarAlgo(ctx, AlgoritmoBase.RuidoPerlin, 0.4f);
                ctx.Paleta.AddRange(new[]{Color.White,Color.AliceBlue,Color.LightBlue,Color.LightCyan});
                ctx.ModoSuave=true; ctx.Intensidad=0.7; ctx.ResumenVisual+="nieve ";
            }},
            {"snow",    delegate(ContextoVisual ctx) {
                AgregarAlgo(ctx, AlgoritmoBase.VoronoiCelular, 0.6f);
                AgregarAlgo(ctx, AlgoritmoBase.RuidoPerlin, 0.4f);
                ctx.Paleta.AddRange(new[]{Color.White,Color.AliceBlue,Color.LightBlue,Color.LightCyan});
                ctx.ModoSuave=true; ctx.Intensidad=0.7; ctx.ResumenVisual+="snow ";
            }},
            {"atardecer", delegate(ContextoVisual ctx) {
                AgregarAlgo(ctx, AlgoritmoBase.OndaInterferencia, 0.4f);
                AgregarAlgo(ctx, AlgoritmoBase.RuidoPerlin, 0.6f);
                ctx.Paleta.AddRange(new[]{Color.DarkRed,Color.OrangeRed,Color.Orange,Color.Gold,Color.DarkBlue,Color.Indigo});
                ctx.ModoSuave=true; ctx.Intensidad=1.1; ctx.ResumenVisual+="atardecer ";
            }},
            {"sunset",  delegate(ContextoVisual ctx) {
                AgregarAlgo(ctx, AlgoritmoBase.OndaInterferencia, 0.4f);
                AgregarAlgo(ctx, AlgoritmoBase.RuidoPerlin, 0.6f);
                ctx.Paleta.AddRange(new[]{Color.DarkRed,Color.OrangeRed,Color.Orange,Color.Gold,Color.DarkBlue,Color.Indigo});
                ctx.ModoSuave=true; ctx.Intensidad=1.1; ctx.ResumenVisual+="sunset ";
            }},
            {"amanecer", delegate(ContextoVisual ctx) {
                AgregarAlgo(ctx, AlgoritmoBase.RuidoPerlin, 0.7f);
                AgregarAlgo(ctx, AlgoritmoBase.OndaInterferencia, 0.3f);
                ctx.Paleta.AddRange(new[]{Color.Pink,Color.LightCoral,Color.Gold,Color.LightYellow,Color.LightBlue});
                ctx.ModoSuave=true; ctx.Intensidad=0.9; ctx.ResumenVisual+="amanecer ";
            }},
            {"sunrise", delegate(ContextoVisual ctx) {
                AgregarAlgo(ctx, AlgoritmoBase.RuidoPerlin, 0.7f);
                AgregarAlgo(ctx, AlgoritmoBase.OndaInterferencia, 0.3f);
                ctx.Paleta.AddRange(new[]{Color.Pink,Color.LightCoral,Color.Gold,Color.LightYellow,Color.LightBlue});
                ctx.ModoSuave=true; ctx.Intensidad=0.9; ctx.ResumenVisual+="sunrise ";
            }},
            {"onda",    delegate(ContextoVisual ctx) {
                AgregarAlgo(ctx, AlgoritmoBase.OndaInterferencia, 1.0f);
                ctx.Escala=2.0; ctx.ResumenVisual+="ondas ";
            }},
            {"wave",    delegate(ContextoVisual ctx) {
                AgregarAlgo(ctx, AlgoritmoBase.OndaInterferencia, 1.0f);
                ctx.Escala=2.0; ctx.ResumenVisual+="waves ";
            }},
            {"retro",   delegate(ContextoVisual ctx) {
                ctx.ModoRetro=true; ctx.Saturacion=0.7; ctx.ResumenVisual+="retro ";
            }},
            {"vintage", delegate(ContextoVisual ctx) {
                ctx.ModoRetro=true; ctx.Saturacion=0.5; ctx.ResumenVisual+="vintage ";
            }},
            {"abstracto", delegate(ContextoVisual ctx) {
                AgregarAlgo(ctx, AlgoritmoBase.PlasmaCaos, 0.5f);
                AgregarAlgo(ctx, AlgoritmoBase.GeometricoSimetrico, 0.5f);
                ctx.Complejidad=1.5; ctx.ResumenVisual+="abstracto ";
            }},
            {"abstract",  delegate(ContextoVisual ctx) {
                AgregarAlgo(ctx, AlgoritmoBase.PlasmaCaos, 0.5f);
                AgregarAlgo(ctx, AlgoritmoBase.GeometricoSimetrico, 0.5f);
                ctx.Complejidad=1.5; ctx.ResumenVisual+="abstract ";
            }},
            {"minecraft",  delegate(ContextoVisual ctx) {
                AgregarAlgo(ctx, AlgoritmoBase.MundoVoxelMinecraft, 1.0f);
                ctx.ResumenVisual+="minecraft ";
            }},
            {"voxel",      delegate(ContextoVisual ctx) {
                AgregarAlgo(ctx, AlgoritmoBase.MundoVoxelMinecraft, 1.0f);
                ctx.ResumenVisual+="voxel ";
            }},
            {"bloques",    delegate(ContextoVisual ctx) {
                AgregarAlgo(ctx, AlgoritmoBase.MundoVoxelMinecraft, 1.0f);
                ctx.ResumenVisual+="bloques ";
            }},
            {"blocks",     delegate(ContextoVisual ctx) {
                AgregarAlgo(ctx, AlgoritmoBase.MundoVoxelMinecraft, 1.0f);
                ctx.ResumenVisual+="blocks ";
            }},
            {"isometrico", delegate(ContextoVisual ctx) {
                AgregarAlgo(ctx, AlgoritmoBase.MundoVoxelMinecraft, 1.0f);
                ctx.ResumenVisual+="isometrico ";
            }},
            {"isometric",  delegate(ContextoVisual ctx) {
                AgregarAlgo(ctx, AlgoritmoBase.MundoVoxelMinecraft, 1.0f);
                ctx.ResumenVisual+="isometric ";
            }},
            {"mundo",      delegate(ContextoVisual ctx) {
                AgregarAlgo(ctx, AlgoritmoBase.MundoVoxelMinecraft, 1.0f);
                ctx.ResumenVisual+="mundo ";
            }},
            {"world",      delegate(ContextoVisual ctx) {
                AgregarAlgo(ctx, AlgoritmoBase.MundoVoxelMinecraft, 1.0f);
                ctx.ResumenVisual+="world ";
            }},
            {"neural",     delegate(ContextoVisual ctx) {
                AgregarAlgo(ctx, AlgoritmoBase.RedNeuralCPPN, 1.0f);
                ctx.EsProgresivo=true; ctx.ResumenVisual+="neural ";
            }},
            {"neuronal",   delegate(ContextoVisual ctx) {
                AgregarAlgo(ctx, AlgoritmoBase.RedNeuralCPPN, 1.0f);
                ctx.EsProgresivo=true; ctx.ResumenVisual+="neuronal ";
            }},
            {"cppn",       delegate(ContextoVisual ctx) {
                AgregarAlgo(ctx, AlgoritmoBase.RedNeuralCPPN, 1.0f);
                ctx.EsProgresivo=true; ctx.ResumenVisual+="cppn ";
            }},
            {"ia",         delegate(ContextoVisual ctx) {
                AgregarAlgo(ctx, AlgoritmoBase.RedNeuralCPPN, 1.0f);
                ctx.EsProgresivo=true; ctx.ResumenVisual+="ia ";
            }},
            {"ai",         delegate(ContextoVisual ctx) {
                AgregarAlgo(ctx, AlgoritmoBase.RedNeuralCPPN, 1.0f);
                ctx.EsProgresivo=true; ctx.ResumenVisual+="ai ";
            }},
            {"pintar",     delegate(ContextoVisual ctx) {
                AgregarAlgo(ctx, AlgoritmoBase.RedNeuralCPPN, 1.0f);
                ctx.EsProgresivo=true; ctx.ResumenVisual+="pintar ";
            }},
            {"paint",      delegate(ContextoVisual ctx) {
                AgregarAlgo(ctx, AlgoritmoBase.RedNeuralCPPN, 1.0f);
                ctx.EsProgresivo=true; ctx.ResumenVisual+="paint ";
            }},
            {"3d",         delegate(ContextoVisual ctx) {
                AgregarAlgo(ctx, AlgoritmoBase.Escena3D, 1.0f);
                ctx.EsProgresivo=true; ctx.ResumenVisual+="3d ";
            }},
            {"escena",     delegate(ContextoVisual ctx) {
                AgregarAlgo(ctx, AlgoritmoBase.Escena3D, 1.0f);
                ctx.EsProgresivo=true; ctx.ResumenVisual+="escena3D ";
            }},
            {"scene",      delegate(ContextoVisual ctx) {
                AgregarAlgo(ctx, AlgoritmoBase.Escena3D, 1.0f);
                ctx.EsProgresivo=true; ctx.ResumenVisual+="scene3D ";
            }},
            {"raytracer",  delegate(ContextoVisual ctx) {
                AgregarAlgo(ctx, AlgoritmoBase.Escena3D, 1.0f);
                ctx.EsProgresivo=true; ctx.ResumenVisual+="raytracer ";
            }},
            {"esfera",     delegate(ContextoVisual ctx) {
                AgregarAlgo(ctx, AlgoritmoBase.Escena3D, 1.0f);
                ctx.EsProgresivo=true; ctx.ResumenVisual+="esferas3D ";
            }},
            {"sphere",     delegate(ContextoVisual ctx) {
                AgregarAlgo(ctx, AlgoritmoBase.Escena3D, 1.0f);
                ctx.EsProgresivo=true; ctx.ResumenVisual+="spheres3D ";
            }},
            {"render",     delegate(ContextoVisual ctx) {
                AgregarAlgo(ctx, AlgoritmoBase.Escena3D, 1.0f);
                ctx.EsProgresivo=true; ctx.ResumenVisual+="render3D ";
            }},
            {"terreno",    delegate(ContextoVisual ctx) {
                AgregarAlgo(ctx, AlgoritmoBase.Escena3D, 1.0f);
                ctx.EsProgresivo=true; ctx.Entorno3D=TipoEntorno3D.Terreno; ctx.ResumenVisual+="terreno3D ";
            }},
            {"terrain",    delegate(ContextoVisual ctx) {
                AgregarAlgo(ctx, AlgoritmoBase.Escena3D, 1.0f);
                ctx.EsProgresivo=true; ctx.Entorno3D=TipoEntorno3D.Terreno; ctx.ResumenVisual+="terrain3D ";
            }},
            {"planeta",    delegate(ContextoVisual ctx) {
                AgregarAlgo(ctx, AlgoritmoBase.Escena3D, 1.0f);
                ctx.EsProgresivo=true; ctx.Entorno3D=TipoEntorno3D.Planeta; ctx.ResumenVisual+="planeta3D ";
            }},
            {"planet",     delegate(ContextoVisual ctx) {
                AgregarAlgo(ctx, AlgoritmoBase.Escena3D, 1.0f);
                ctx.EsProgresivo=true; ctx.Entorno3D=TipoEntorno3D.Planeta; ctx.ResumenVisual+="planet3D ";
            }},
            {"cueva",      delegate(ContextoVisual ctx) {
                AgregarAlgo(ctx, AlgoritmoBase.Escena3D, 1.0f);
                ctx.EsProgresivo=true; ctx.Entorno3D=TipoEntorno3D.Cueva; ctx.ResumenVisual+="cueva3D ";
            }},
            {"cave",       delegate(ContextoVisual ctx) {
                AgregarAlgo(ctx, AlgoritmoBase.Escena3D, 1.0f);
                ctx.EsProgresivo=true; ctx.Entorno3D=TipoEntorno3D.Cueva; ctx.ResumenVisual+="cave3D ";
            }},
            {"ciudad",     delegate(ContextoVisual ctx) {
                AgregarAlgo(ctx, AlgoritmoBase.Escena3D, 1.0f);
                ctx.EsProgresivo=true; ctx.Entorno3D=TipoEntorno3D.Ciudad; ctx.ResumenVisual+="ciudad3D ";
            }},
            {"city",       delegate(ContextoVisual ctx) {
                AgregarAlgo(ctx, AlgoritmoBase.Escena3D, 1.0f);
                ctx.EsProgresivo=true; ctx.Entorno3D=TipoEntorno3D.Ciudad; ctx.ResumenVisual+="city3D ";
            }},
            {"superficie", delegate(ContextoVisual ctx) {
                AgregarAlgo(ctx, AlgoritmoBase.Escena3D, 1.0f);
                ctx.EsProgresivo=true; ctx.Entorno3D=TipoEntorno3D.SuperficiePlanetaria; ctx.ResumenVisual+="superficie3D ";
            }},
            {"alien",      delegate(ContextoVisual ctx) {
                AgregarAlgo(ctx, AlgoritmoBase.Escena3D, 1.0f);
                ctx.EsProgresivo=true; ctx.Entorno3D=TipoEntorno3D.SuperficiePlanetaria; ctx.ResumenVisual+="alien3D ";
            }},
            {"nebulosa3d", delegate(ContextoVisual ctx) {
                AgregarAlgo(ctx, AlgoritmoBase.Escena3D, 1.0f);
                ctx.EsProgresivo=true; ctx.Entorno3D=TipoEntorno3D.Nebulosa3D; ctx.ResumenVisual+="nebulosa3D ";
            }},
            {"canon",      delegate(ContextoVisual ctx) {
                AgregarAlgo(ctx, AlgoritmoBase.Escena3D, 1.0f);
                ctx.EsProgresivo=true; ctx.Entorno3D=TipoEntorno3D.Canon; ctx.ResumenVisual+="canon3D ";
            }},
            {"cañon",      delegate(ContextoVisual ctx) {
                AgregarAlgo(ctx, AlgoritmoBase.Escena3D, 1.0f);
                ctx.EsProgresivo=true; ctx.Entorno3D=TipoEntorno3D.Canon; ctx.ResumenVisual+="cañon3D ";
            }},
            {"canyon",     delegate(ContextoVisual ctx) {
                AgregarAlgo(ctx, AlgoritmoBase.Escena3D, 1.0f);
                ctx.EsProgresivo=true; ctx.Entorno3D=TipoEntorno3D.Canon; ctx.ResumenVisual+="canyon3D ";
            }},
            {"tormenta",   delegate(ContextoVisual ctx) {
                AgregarAlgo(ctx, AlgoritmoBase.Escena3D, 1.0f);
                ctx.EsProgresivo=true; ctx.Entorno3D=TipoEntorno3D.Tormenta; ctx.ResumenVisual+="tormenta3D ";
            }},
            {"storm",      delegate(ContextoVisual ctx) {
                AgregarAlgo(ctx, AlgoritmoBase.Escena3D, 1.0f);
                ctx.EsProgresivo=true; ctx.Entorno3D=TipoEntorno3D.Tormenta; ctx.ResumenVisual+="storm3D ";
            }},
            {"oceano3d",   delegate(ContextoVisual ctx) {
                AgregarAlgo(ctx, AlgoritmoBase.Escena3D, 1.0f);
                ctx.EsProgresivo=true; ctx.Entorno3D=TipoEntorno3D.Oceano3D; ctx.ResumenVisual+="oceano3D ";
            }},
            {"acuarela",   delegate(ContextoVisual ctx) {
                AgregarAlgo(ctx, AlgoritmoBase.Acuarela, 1.0f);
                ctx.ResumenVisual+="acuarela ";
            }},
            {"watercolor", delegate(ContextoVisual ctx) {
                AgregarAlgo(ctx, AlgoritmoBase.Acuarela, 1.0f);
                ctx.ResumenVisual+="watercolor ";
            }},
            {"acuarelas",  delegate(ContextoVisual ctx) {
                AgregarAlgo(ctx, AlgoritmoBase.Acuarela, 1.0f);
                ctx.ResumenVisual+="acuarelas ";
            }},
            {"pintura",    delegate(ContextoVisual ctx) {
                AgregarAlgo(ctx, AlgoritmoBase.Acuarela, 1.0f);
                ctx.ResumenVisual+="pintura ";
            }},
            {"lapiz",      delegate(ContextoVisual ctx) {
                AgregarAlgo(ctx, AlgoritmoBase.DibujoLapiz, 1.0f);
                ctx.EstiloLapiz="lapiz"; ctx.ResumenVisual+="lápiz ";
            }},
            {"pencil",     delegate(ContextoVisual ctx) {
                AgregarAlgo(ctx, AlgoritmoBase.DibujoLapiz, 1.0f);
                ctx.EstiloLapiz="lapiz"; ctx.ResumenVisual+="pencil ";
            }},
            {"lapicera",   delegate(ContextoVisual ctx) {
                AgregarAlgo(ctx, AlgoritmoBase.DibujoLapiz, 1.0f);
                ctx.EstiloLapiz="lapicera"; ctx.ResumenVisual+="lapicera ";
            }},
            {"boligrafo",  delegate(ContextoVisual ctx) {
                AgregarAlgo(ctx, AlgoritmoBase.DibujoLapiz, 1.0f);
                ctx.EstiloLapiz="lapicera"; ctx.ResumenVisual+="bolígrafo ";
            }},
            {"pen",        delegate(ContextoVisual ctx) {
                AgregarAlgo(ctx, AlgoritmoBase.DibujoLapiz, 1.0f);
                ctx.EstiloLapiz="lapicera"; ctx.ResumenVisual+="pen ";
            }},
            {"sketch",     delegate(ContextoVisual ctx) {
                AgregarAlgo(ctx, AlgoritmoBase.DibujoLapiz, 1.0f);
                ctx.EstiloLapiz="lapiz"; ctx.ResumenVisual+="sketch ";
            }},
            {"dibujo",     delegate(ContextoVisual ctx) {
                AgregarAlgo(ctx, AlgoritmoBase.DibujoLapiz, 1.0f);
                ctx.EstiloLapiz="lapiz"; ctx.ResumenVisual+="dibujo ";
            }},
            {"boceto",     delegate(ContextoVisual ctx) {
                AgregarAlgo(ctx, AlgoritmoBase.DibujoLapiz, 1.0f);
                ctx.EstiloLapiz="lapiz"; ctx.ResumenVisual+="boceto ";
            }},
            {"carbon",     delegate(ContextoVisual ctx) {
                AgregarAlgo(ctx, AlgoritmoBase.DibujoLapiz, 1.0f);
                ctx.EstiloLapiz="carbon"; ctx.ModoOscuro=true; ctx.ResumenVisual+="carbón ";
            }},
            {"charcoal",   delegate(ContextoVisual ctx) {
                AgregarAlgo(ctx, AlgoritmoBase.DibujoLapiz, 1.0f);
                ctx.EstiloLapiz="carbon"; ctx.ModoOscuro=true; ctx.ResumenVisual+="charcoal ";
            }},
        };

        public static readonly Dictionary<string, Action<ContextoVisual>> MapaModificadores =
            new Dictionary<string, Action<ContextoVisual>>(StringComparer.OrdinalIgnoreCase)
        {
            {"oscuro",     delegate(ContextoVisual ctx){ ctx.ModoOscuro=true;   ctx.Intensidad*=0.7; }},
            {"dark",       delegate(ContextoVisual ctx){ ctx.ModoOscuro=true;   ctx.Intensidad*=0.7; }},
            {"brillante",  delegate(ContextoVisual ctx){ ctx.Intensidad*=1.4;   ctx.Saturacion*=1.3; }},
            {"bright",     delegate(ContextoVisual ctx){ ctx.Intensidad*=1.4;   ctx.Saturacion*=1.3; }},
            {"suave",      delegate(ContextoVisual ctx){ ctx.ModoSuave=true;    ctx.Intensidad*=0.8; }},
            {"soft",       delegate(ContextoVisual ctx){ ctx.ModoSuave=true;    ctx.Intensidad*=0.8; }},
            {"intenso",    delegate(ContextoVisual ctx){ ctx.Intensidad*=1.5;   ctx.Saturacion*=1.4; }},
            {"intense",    delegate(ContextoVisual ctx){ ctx.Intensidad*=1.5;   ctx.Saturacion*=1.4; }},
            {"caotico",    delegate(ContextoVisual ctx){ ctx.ModoCaos=true;     ctx.Complejidad*=1.5;}},
            {"chaotic",    delegate(ContextoVisual ctx){ ctx.ModoCaos=true;     ctx.Complejidad*=1.5;}},
            {"simetrico",  delegate(ContextoVisual ctx){ ctx.ModoSimetrico=true; }},
            {"symmetric",  delegate(ContextoVisual ctx){ ctx.ModoSimetrico=true; }},
            {"complejo",   delegate(ContextoVisual ctx){ ctx.Complejidad*=2.0; ctx.Iteraciones+=100; }},
            {"complex",    delegate(ContextoVisual ctx){ ctx.Complejidad*=2.0; ctx.Iteraciones+=100; }},
            {"simple",     delegate(ContextoVisual ctx){ ctx.Complejidad*=0.5; ctx.Escala*=0.7; }},
            {"grande",     delegate(ContextoVisual ctx){ ctx.Escala*=0.5; }},
            {"big",        delegate(ContextoVisual ctx){ ctx.Escala*=0.5; }},
            {"pequeño",    delegate(ContextoVisual ctx){ ctx.Escala*=2.0; }},
            {"small",      delegate(ContextoVisual ctx){ ctx.Escala*=2.0; }},
            {"calido",     delegate(ContextoVisual ctx){ ctx.Paleta.AddRange(new[]{Color.OrangeRed,Color.Orange,Color.Gold}); }},
            {"warm",       delegate(ContextoVisual ctx){ ctx.Paleta.AddRange(new[]{Color.OrangeRed,Color.Orange,Color.Gold}); }},
            {"frio",       delegate(ContextoVisual ctx){ ctx.Paleta.AddRange(new[]{Color.DarkBlue,Color.CornflowerBlue,Color.Cyan}); }},
            {"cold",       delegate(ContextoVisual ctx){ ctx.Paleta.AddRange(new[]{Color.DarkBlue,Color.CornflowerBlue,Color.Cyan}); }},
            {"cool",       delegate(ContextoVisual ctx){ ctx.Paleta.AddRange(new[]{Color.DarkBlue,Color.CornflowerBlue,Color.Cyan}); }},
            {"misterioso", delegate(ContextoVisual ctx){ ctx.ModoOscuro=true; ctx.Paleta.AddRange(new[]{Color.Indigo,Color.DarkViolet}); }},
            {"mysterious", delegate(ContextoVisual ctx){ ctx.ModoOscuro=true; ctx.Paleta.AddRange(new[]{Color.Indigo,Color.DarkViolet}); }},
            {"energetico", delegate(ContextoVisual ctx){ ctx.Intensidad*=1.6; ctx.Saturacion*=1.5; ctx.ModoCaos=true; }},
            {"energetic",  delegate(ContextoVisual ctx){ ctx.Intensidad*=1.6; ctx.Saturacion*=1.5; ctx.ModoCaos=true; }},
            {"tranquilo",  delegate(ContextoVisual ctx){ ctx.ModoSuave=true; ctx.Intensidad*=0.7; ctx.Saturacion*=0.8; }},
            {"peaceful",   delegate(ContextoVisual ctx){ ctx.ModoSuave=true; ctx.Intensidad*=0.7; ctx.Saturacion*=0.8; }},
            {"calm",       delegate(ContextoVisual ctx){ ctx.ModoSuave=true; ctx.Intensidad*=0.7; ctx.Saturacion*=0.8; }},
            {"pastel",     delegate(ContextoVisual ctx){ ctx.Saturacion*=0.5; ctx.Intensidad*=1.2; ctx.ModoSuave=true; }},
            {"neon",       delegate(ContextoVisual ctx){ ctx.Saturacion*=2.5; ctx.Intensidad*=1.3; ctx.ModoOscuro=true; }},
            {"luz",        delegate(ContextoVisual ctx){ ctx.Intensidad*=1.3; }},
            {"light",      delegate(ContextoVisual ctx){ ctx.Intensidad*=1.3; }},
            {"sombra",     delegate(ContextoVisual ctx){ ctx.ModoOscuro=true; ctx.Intensidad*=0.8; }},
            {"shadow",     delegate(ContextoVisual ctx){ ctx.ModoOscuro=true; ctx.Intensidad*=0.8; }},
            {"metalico",   delegate(ContextoVisual ctx){ ctx.Saturacion*=0.3; ctx.Intensidad*=1.2; }},
            {"metallic",   delegate(ContextoVisual ctx){ ctx.Saturacion*=0.3; ctx.Intensidad*=1.2; }},
            {"cristalino", delegate(ContextoVisual ctx){ ctx.ModoSimetrico=true; ctx.Complejidad*=1.5; }},
            {"atardecer",  delegate(ContextoVisual ctx){ ctx.HoraDelDia="atardecer"; ctx.Paleta.AddRange(new[]{Color.FromArgb(255,100,20),Color.FromArgb(255,180,60),Color.FromArgb(180,60,120),Color.FromArgb(80,30,80)}); }},
            {"sunset",     delegate(ContextoVisual ctx){ ctx.HoraDelDia="atardecer"; ctx.Paleta.AddRange(new[]{Color.FromArgb(255,100,20),Color.FromArgb(255,180,60),Color.FromArgb(180,60,120),Color.FromArgb(80,30,80)}); }},
            {"amanecer",   delegate(ContextoVisual ctx){ ctx.HoraDelDia="amanecer";  ctx.Paleta.AddRange(new[]{Color.FromArgb(255,160,80),Color.FromArgb(200,220,255),Color.FromArgb(255,200,120)}); }},
            {"sunrise",    delegate(ContextoVisual ctx){ ctx.HoraDelDia="amanecer";  ctx.Paleta.AddRange(new[]{Color.FromArgb(255,160,80),Color.FromArgb(200,220,255),Color.FromArgb(255,200,120)}); }},
            {"mediodia",   delegate(ContextoVisual ctx){ ctx.HoraDelDia="mediodia";  ctx.Intensidad*=1.2; }},
            {"noon",       delegate(ContextoVisual ctx){ ctx.HoraDelDia="mediodia";  ctx.Intensidad*=1.2; }},
            {"nocturno",   delegate(ContextoVisual ctx){ ctx.HoraDelDia="noche"; ctx.ModoOscuro=true; ctx.ConEstrellas=true; }},
            {"night",      delegate(ContextoVisual ctx){ ctx.HoraDelDia="noche"; ctx.ModoOscuro=true; ctx.ConEstrellas=true; }},
            {"lluvia",     delegate(ContextoVisual ctx){ ctx.ConLluvia=true;  ctx.ModoSuave=true; ctx.Saturacion*=0.85; }},
            {"rain",       delegate(ContextoVisual ctx){ ctx.ConLluvia=true;  ctx.ModoSuave=true; ctx.Saturacion*=0.85; }},
            {"lloviendo",  delegate(ContextoVisual ctx){ ctx.ConLluvia=true;  ctx.ModoSuave=true; }},
            {"nevada",     delegate(ContextoVisual ctx){ ctx.ConNieve=true;   ctx.Paleta.AddRange(new[]{Color.White,Color.AliceBlue,Color.LightCyan}); }},
            {"nieve",      delegate(ContextoVisual ctx){ ctx.ConNieve=true;   ctx.Paleta.AddRange(new[]{Color.White,Color.AliceBlue,Color.LightCyan}); }},
            {"snow",       delegate(ContextoVisual ctx){ ctx.ConNieve=true;   ctx.Paleta.AddRange(new[]{Color.White,Color.AliceBlue,Color.LightCyan}); }},
            {"niebla",     delegate(ContextoVisual ctx){ ctx.ConNiebla=true;  ctx.ModoSuave=true; ctx.Saturacion*=0.7; }},
            {"fog",        delegate(ContextoVisual ctx){ ctx.ConNiebla=true;  ctx.ModoSuave=true; ctx.Saturacion*=0.7; }},
            {"mist",       delegate(ContextoVisual ctx){ ctx.ConNiebla=true;  ctx.ModoSuave=true; }},
            {"neblina",    delegate(ContextoVisual ctx){ ctx.ConNiebla=true;  ctx.ModoSuave=true; }},
            {"arcoiris",   delegate(ContextoVisual ctx){ ctx.ConArcoIris=true; }},
            {"rainbow",    delegate(ContextoVisual ctx){ ctx.ConArcoIris=true; }},
            {"arco",       delegate(ContextoVisual ctx){ ctx.ConArcoIris=true; }},
            {"palmeras",   delegate(ContextoVisual ctx){ ctx.ConPalmeras=true; }},
            {"palmera",    delegate(ContextoVisual ctx){ ctx.ConPalmeras=true; }},
            {"palm",       delegate(ContextoVisual ctx){ ctx.ConPalmeras=true; }},
            {"palms",      delegate(ContextoVisual ctx){ ctx.ConPalmeras=true; }},
            {"rocas",      delegate(ContextoVisual ctx){ ctx.ConRocas=true; }},
            {"rocks",      delegate(ContextoVisual ctx){ ctx.ConRocas=true; }},
            {"piedras",    delegate(ContextoVisual ctx){ ctx.ConRocas=true; }},
            {"tormenta",   delegate(ContextoVisual ctx){ ctx.ConTormenta=true; ctx.ModoOscuro=true; ctx.Paleta.AddRange(new[]{Color.FromArgb(40,40,60),Color.FromArgb(80,80,120),Color.DarkGray}); }},
            {"storm",      delegate(ContextoVisual ctx){ ctx.ConTormenta=true; ctx.ModoOscuro=true; }},
            {"islas",      delegate(ContextoVisual ctx){ ctx.ConIslas=true; }},
            {"isla",       delegate(ContextoVisual ctx){ ctx.ConIslas=true; }},
            {"island",     delegate(ContextoVisual ctx){ ctx.ConIslas=true; }},
            {"islands",    delegate(ContextoVisual ctx){ ctx.ConIslas=true; }},
            {"tropical",   delegate(ContextoVisual ctx){ ctx.ConPalmeras=true; ctx.Paleta.AddRange(new[]{Color.FromArgb(0,180,140),Color.FromArgb(0,210,180),Color.SandyBrown}); }},
            {"alpino",     delegate(ContextoVisual ctx){ ctx.ConNieve=true; ctx.ConRocas=true; ctx.Saturacion*=0.9; }},
            {"alpine",     delegate(ContextoVisual ctx){ ctx.ConNieve=true; ctx.ConRocas=true; }},
            {"costero",    delegate(ContextoVisual ctx){ ctx.ConIslas=true; ctx.ConRocas=true; ctx.ConPalmeras=true; }},
            {"coastal",    delegate(ContextoVisual ctx){ ctx.ConIslas=true; ctx.ConRocas=true; }},
            {"seco",       delegate(ContextoVisual ctx){ ctx.Saturacion*=0.7; ctx.Intensidad*=1.1; }},
            {"dry",        delegate(ContextoVisual ctx){ ctx.Saturacion*=0.7; }},
            {"humedo",     delegate(ContextoVisual ctx){ ctx.ConNiebla=true; ctx.Saturacion*=1.15; }},
            {"humid",      delegate(ContextoVisual ctx){ ctx.ConNiebla=true; ctx.Saturacion*=1.15; }},
            {"despejado",  delegate(ContextoVisual ctx){ ctx.ModoSuave=false; ctx.Intensidad*=1.1; }},
            {"clear",      delegate(ContextoVisual ctx){ ctx.ModoSuave=false; ctx.Intensidad*=1.1; }},
            {"arido",      delegate(ContextoVisual ctx){ ctx.Saturacion*=0.6; ctx.Paleta.AddRange(new[]{Color.SaddleBrown,Color.Peru,Color.Khaki}); }},
            {"arid",       delegate(ContextoVisual ctx){ ctx.Saturacion*=0.6; ctx.Paleta.AddRange(new[]{Color.SaddleBrown,Color.Peru,Color.Khaki}); }},
            {"epico",      delegate(ContextoVisual ctx){ ctx.Intensidad*=1.5; ctx.Saturacion*=1.4; ctx.Complejidad*=1.3; }},
            {"epic",       delegate(ContextoVisual ctx){ ctx.Intensidad*=1.5; ctx.Saturacion*=1.4; ctx.Complejidad*=1.3; }},
            {"dramatico",  delegate(ContextoVisual ctx){ ctx.Intensidad*=1.4; ctx.ModoOscuro=true; ctx.Saturacion*=1.3; }},
            {"dramatic",   delegate(ContextoVisual ctx){ ctx.Intensidad*=1.4; ctx.ModoOscuro=true; ctx.Saturacion*=1.3; }},
            {"vintage",    delegate(ContextoVisual ctx){ ctx.Saturacion*=0.6; ctx.ModoRetro=true; ctx.Paleta.AddRange(new[]{Color.SaddleBrown,Color.Peru,Color.Tan}); }},
            {"retro",      delegate(ContextoVisual ctx){ ctx.ModoRetro=true; ctx.Saturacion*=0.7; }},
            {"minimalista",delegate(ContextoVisual ctx){ ctx.Complejidad*=0.4; ctx.ModoSuave=true; }},
            {"minimal",    delegate(ContextoVisual ctx){ ctx.Complejidad*=0.4; ctx.ModoSuave=true; }},
        };

        private static void AgregarAlgo(ContextoVisual ctx, AlgoritmoBase algo, float peso)
        {
            int idx = ctx.Algoritmos.IndexOf(algo);
            if (idx >= 0) ctx.PesosAlgoritmos[idx] += peso;
            else { ctx.Algoritmos.Add(algo); ctx.PesosAlgoritmos.Add(peso); }
        }
    }

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

            string promptTrim = prompt.Trim();
            bool iaLienzo = promptTrim.StartsWith("IA lienzo", StringComparison.OrdinalIgnoreCase);
            if (iaLienzo)
            {
                string resto = promptTrim.Length > 9 ? promptTrim.Substring(9).Trim() : "";
                return InterpretadorLienzo.Interpretar(resto, semilla);
            }

            bool iaInicio = promptTrim.StartsWith("IA ", StringComparison.OrdinalIgnoreCase)
                         || string.Equals(promptTrim, "IA", StringComparison.OrdinalIgnoreCase);
            if (iaInicio)
            {
                string parametros = promptTrim.Length > 2 ? promptTrim.Substring(2).Trim() : "";
                return InterpretadorIA.InterpretarConIA(parametros, semilla);
            }

            Char[] separadores = new char[]{' ',',','.',';',':','!','?','-','_','/','\t','\n'};
            var tokens = new List<string>(
                prompt.ToLower().Split(separadores, StringSplitOptions.RemoveEmptyEntries));

            bool iaEnMedio = tokens.Contains("ia");
            if (iaEnMedio) tokens.Remove("ia");

            ctx.PalabrasDetectadas.AddRange(tokens);

            foreach (string token in tokens)
            {
                if (BancoPalabras.MapaTemas.ContainsKey(token))
                    BancoPalabras.MapaTemas[token](ctx);
            }
            foreach (string token in tokens)
            {
                if (BancoPalabras.MapaColores.ContainsKey(token))
                    ctx.Paleta.AddRange(BancoPalabras.MapaColores[token]);
            }
            foreach (string token in tokens)
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

            // Prioridad exclusiva: DibujoLapiz y Acuarela no se mezclan con otros algoritmos.
            // El tema (oceano, bosque, etc.) ya está en PalabrasDetectadas y lo usa el generador.
            if (ctx.Algoritmos.Contains(AlgoritmoBase.DibujoLapiz))
            {
                ctx.Algoritmos.Clear();
                ctx.PesosAlgoritmos.Clear();
                ctx.Algoritmos.Add(AlgoritmoBase.DibujoLapiz);
                ctx.PesosAlgoritmos.Add(1.0f);
            }
            else if (ctx.Algoritmos.Contains(AlgoritmoBase.Acuarela))
            {
                ctx.Algoritmos.Clear();
                ctx.PesosAlgoritmos.Clear();
                ctx.Algoritmos.Add(AlgoritmoBase.Acuarela);
                ctx.PesosAlgoritmos.Add(1.0f);
            }

            if (ctx.Paleta.Count == 0) AsignarPaletaPorDefecto(ctx);

            float totalPeso = 0;
            foreach (float p in ctx.PesosAlgoritmos) totalPeso += p;
            for (int i = 0; i < ctx.PesosAlgoritmos.Count; i++)
                ctx.PesosAlgoritmos[i] /= totalPeso;

            ctx.Escala     = Math.Max(0.2, Math.Min(5.0, ctx.Escala));
            ctx.Intensidad = Math.Max(0.3, Math.Min(2.5, ctx.Intensidad));
            ctx.Saturacion = Math.Max(0.1, Math.Min(3.0, ctx.Saturacion));
            ctx.Iteraciones= Math.Max(50,  Math.Min(800, ctx.Iteraciones));

            if (iaEnMedio) ctx.ModoIAPostProceso = true;

            return ctx;
        }

        private static void AsignarPaletaPorDefecto(ContextoVisual ctx)
        {
            if (ctx.ModoOscuro)
                ctx.Paleta.AddRange(new[]{ Color.Black, Color.DarkSlateBlue, Color.DarkViolet, Color.MidnightBlue });
            else if (ctx.ModoSuave)
                ctx.Paleta.AddRange(new[]{ Color.LightBlue, Color.LightGreen, Color.LightYellow, Color.LavenderBlush });
            else
                ctx.Paleta.AddRange(new[]{ Color.DarkBlue, Color.MediumBlue, Color.DodgerBlue, Color.Orange, Color.White });
        }
    }

    // ═══════════════════════════════════════════════════

    public static class InterpretadorIA
    {
        private static readonly Dictionary<string, float[]> _semMap =
            new Dictionary<string, float[]>(StringComparer.OrdinalIgnoreCase)
        {
            // índices: 0=Perlin 1=Fractal 2=Fluido 3=Geometrico 4=Voronoi 5=Onda 6=Nebulosa 7=Plasma 8=Voxel 9=RedNeural 10=Escena3D 11=Acuarela 12=DibujoLapiz
            {"fractal",    new float[]{0,6,0,0,0,0,0,1,0,1,0,0,0}},
            {"mandelbrot", new float[]{0,7,0,0,0,0,0,0,0,0,0,0,0}},
            {"julia",      new float[]{0,6,0,0,0,0,0,0,0,1,0,0,0}},
            {"sierpinski", new float[]{0,4,0,3,0,0,0,0,0,0,0,0,0}},
            {"3d",         new float[]{0,0,0,0,0,0,1,0,0,0,7,0,0}},
            {"render",     new float[]{0,0,0,0,0,0,0,0,0,0,7,0,0}},
            {"escena",     new float[]{0,0,0,0,0,0,0,0,0,0,6,0,0}},
            {"esfera",     new float[]{0,0,0,0,0,0,0,0,0,0,6,0,0}},
            {"scene",      new float[]{0,0,0,0,0,0,0,0,0,0,6,0,0}},
            {"sphere",     new float[]{0,0,0,0,0,0,0,0,0,0,6,0,0}},
            {"raytracing", new float[]{0,0,0,0,0,0,0,0,0,0,7,0,0}},
            {"neural",     new float[]{0,0,0,0,0,0,0,0,0,7,0,0,0}},
            {"neuronal",   new float[]{0,0,0,0,0,0,0,0,0,7,0,0,0}},
            {"cppn",       new float[]{0,0,0,0,0,0,0,0,0,7,0,0,0}},
            {"red",        new float[]{0,0,0,0,0,0,0,0,0,4,0,0,0}},
            {"espacio",    new float[]{0,0,0,0,1,0,6,1,0,0,1,0,0}},
            {"space",      new float[]{0,0,0,0,1,0,6,1,0,0,1,0,0}},
            {"nebulosa",   new float[]{0,0,0,0,0,0,7,0,0,0,0,0,0}},
            {"galaxia",    new float[]{0,0,0,0,1,0,6,0,0,0,2,0,0}},
            {"cosmos",     new float[]{0,0,0,0,0,0,6,1,0,1,2,0,0}},
            {"fuego",      new float[]{0,0,6,0,0,0,0,2,0,0,0,0,0}},
            {"fire",       new float[]{0,0,6,0,0,0,0,2,0,0,0,0,0}},
            {"lava",       new float[]{0,0,5,0,2,0,0,1,0,0,0,0,0}},
            {"smoke",      new float[]{3,0,4,0,0,0,1,0,0,0,0,0,0}},
            {"plasma",     new float[]{0,0,1,0,0,2,0,7,0,0,0,0,0}},
            {"psicodelico",new float[]{0,0,0,0,0,3,0,6,0,0,0,0,0}},
            {"psychedelic",new float[]{0,0,0,0,0,3,0,6,0,0,0,0,0}},
            {"cristal",    new float[]{0,2,0,1,5,1,0,0,0,0,0,0,0}},
            {"crystal",    new float[]{0,2,0,1,5,1,0,0,0,0,0,0,0}},
            {"onda",       new float[]{0,0,1,0,0,7,0,1,0,0,0,0,0}},
            {"wave",       new float[]{0,0,1,0,0,7,0,1,0,0,0,0,0}},
            {"geometrico", new float[]{0,0,0,7,0,1,0,0,0,0,0,0,0}},
            {"mandala",    new float[]{0,1,0,6,0,2,0,0,0,0,0,0,0}},
            {"bosque",     new float[]{5,0,1,0,3,0,0,0,0,0,0,0,0}},
            {"ocean",      new float[]{2,0,3,0,0,5,0,0,0,0,0,0,0}},
            {"oceano",     new float[]{2,0,3,0,0,5,0,0,0,0,0,0,0}},
            {"nube",       new float[]{5,0,0,0,0,1,1,0,0,0,0,0,0}},
            {"abstracto",  new float[]{1,1,1,1,1,1,1,1,0,1,0,0,0}},
            {"abstract",   new float[]{1,1,1,1,1,1,1,1,0,1,0,0,0}},
            {"voronoi",    new float[]{0,0,0,0,7,0,0,0,0,0,0,0,0}},
            {"celular",    new float[]{0,0,0,0,6,0,0,0,0,0,0,0,0}},
            {"perlin",     new float[]{7,0,0,0,0,0,0,0,0,0,0,0,0}},
            {"ruido",      new float[]{6,0,0,0,0,0,0,0,0,0,0,0,0}},
            {"turbulencia",new float[]{1,0,6,0,0,0,0,1,0,0,0,0,0}},
            {"planeta",    new float[]{0,0,0,0,0,0,2,0,0,0,7,0,0}},
            {"terreno",    new float[]{3,0,0,0,0,0,0,0,0,0,6,0,0}},
            {"terrain",    new float[]{3,0,0,0,0,0,0,0,0,0,6,0,0}},
            {"superficie", new float[]{2,0,0,0,0,0,0,0,0,0,7,0,0}},
            {"alien",      new float[]{0,0,0,0,0,0,2,0,0,0,7,0,0}},
            {"canon",      new float[]{2,0,0,0,0,0,0,0,0,0,6,0,0}},
            {"canyon",     new float[]{2,0,0,0,0,0,0,0,0,0,6,0,0}},
            {"tormenta",   new float[]{0,0,4,0,0,0,1,0,0,0,5,0,0}},
            {"storm",      new float[]{0,0,4,0,0,0,1,0,0,0,5,0,0}},
            {"ciudad",     new float[]{0,0,0,2,0,0,0,0,0,0,6,0,0}},
            {"city",       new float[]{0,0,0,2,0,0,0,0,0,0,6,0,0}},
            {"flores",     new float[]{3,0,0,0,2,0,0,0,0,2,0,0,0}},
            {"flowers",    new float[]{3,0,0,0,2,0,0,0,0,2,0,0,0}},
            {"noche",      new float[]{1,0,0,0,0,1,6,0,0,0,1,0,0}},
            {"night",      new float[]{1,0,0,0,0,1,6,0,0,0,1,0,0}},
            {"aurora",     new float[]{1,0,0,0,0,2,4,0,0,0,1,0,0}},
            {"volcan",     new float[]{0,0,5,0,0,0,0,3,0,0,0,0,0}},
            {"volcano",    new float[]{0,0,5,0,0,0,0,3,0,0,0,0,0}},
            {"acuarela",   new float[]{0,0,0,0,0,0,0,0,0,0,0,7,0}},
            {"watercolor", new float[]{0,0,0,0,0,0,0,0,0,0,0,7,0}},
            {"pintura",    new float[]{0,0,0,0,0,0,0,0,0,0,0,5,2}},
            {"lapiz",      new float[]{0,0,0,0,0,0,0,0,0,0,0,0,7}},
            {"pencil",     new float[]{0,0,0,0,0,0,0,0,0,0,0,0,7}},
            {"lapicera",   new float[]{0,0,0,0,0,0,0,0,0,0,0,0,7}},
            {"pen",        new float[]{0,0,0,0,0,0,0,0,0,0,0,0,7}},
            {"sketch",     new float[]{0,0,0,0,0,0,0,0,0,0,0,0,6}},
            {"dibujo",     new float[]{0,0,0,0,0,0,0,0,0,0,0,0,6}},
            {"boceto",     new float[]{0,0,0,0,0,0,0,0,0,0,0,0,6}},
            {"carbon",     new float[]{0,0,0,0,0,0,0,0,0,0,0,0,6}},
            {"charcoal",   new float[]{0,0,0,0,0,0,0,0,0,0,0,0,6}},
            {"linea",      new float[]{0,0,0,3,0,2,0,0,0,0,0,0,4}},
            {"line",       new float[]{0,0,0,3,0,2,0,0,0,0,0,0,4}},
            {"contorno",   new float[]{0,0,0,2,0,0,0,0,0,0,0,0,5}},
            {"outline",    new float[]{0,0,0,2,0,0,0,0,0,0,0,0,5}},
            {"portrait",   new float[]{0,0,0,0,0,0,0,0,0,3,0,0,4}},
            {"retrato",    new float[]{0,0,0,0,0,0,0,0,0,3,0,0,4}},
            {"paisaje",    new float[]{4,0,0,0,0,0,0,0,0,0,0,1,2}},
            {"landscape",  new float[]{4,0,0,0,0,0,0,0,0,0,0,1,2}},
            {"arquitectura",new float[]{0,0,0,5,0,0,0,0,0,0,2,0,3}},
            {"building",   new float[]{0,0,0,4,0,0,0,0,0,0,3,0,2}},
            {"naturaleza", new float[]{5,0,1,0,2,0,0,0,0,0,0,1,1}},
            {"nature",     new float[]{5,0,1,0,2,0,0,0,0,0,0,1,1}},
            {"animal",     new float[]{3,0,0,0,2,0,0,0,0,2,0,0,2}},
            {"animals",    new float[]{3,0,0,0,2,0,0,0,0,2,0,0,2}},
            {"arbol",      new float[]{4,0,0,0,2,0,0,0,0,0,0,0,3}},
            {"tree",       new float[]{4,0,0,0,2,0,0,0,0,0,0,0,3}},
            {"montaña",    new float[]{4,0,0,0,0,0,0,0,0,0,0,0,3}},
            {"mountain",   new float[]{4,0,0,0,0,0,0,0,0,0,0,0,3}},
            {"desierto",   new float[]{3,0,1,0,0,0,0,1,0,0,0,0,2}},
            {"desert",     new float[]{3,0,1,0,0,0,0,1,0,0,0,0,2}},
            {"lluvia",     new float[]{2,0,3,0,0,3,0,0,0,0,0,2,1}},
            {"rain",       new float[]{2,0,3,0,0,3,0,0,0,0,0,2,1}},
            {"minecraft",  new float[]{0,0,0,0,0,0,0,0,7,0,0,0,0}},
            {"voxel",      new float[]{0,0,0,0,0,0,0,0,7,0,0,0,0}},
            {"bloques",    new float[]{0,0,0,2,0,0,0,0,6,0,0,0,0}},
        };

        public static ContextoVisual InterpretarConIA(string parametros, int semilla)
        {
            var ctx = new ContextoVisual();
            if (semilla >= 0) ctx.Semilla = semilla;
            ctx.ModoIAInicio = true;

            float[] votos = new float[13];

            Char[] sep = new char[]{' ',',','.',';',':','!','?','-','_','/','\t','\n'};
            var tokens = string.IsNullOrWhiteSpace(parametros)
                ? new List<string>()
                : new List<string>(parametros.ToLower().Split(sep, StringSplitOptions.RemoveEmptyEntries));

            ctx.PalabrasDetectadas.AddRange(tokens);

            foreach (string t in tokens)
            {
                if (BancoPalabras.MapaColores.ContainsKey(t))
                    ctx.Paleta.AddRange(BancoPalabras.MapaColores[t]);
                if (BancoPalabras.MapaModificadores.ContainsKey(t))
                    BancoPalabras.MapaModificadores[t](ctx);
            }

            foreach (string t in tokens)
            {
                if (_semMap.ContainsKey(t))
                {
                    float[] v = _semMap[t];
                    for (int i = 0; i < Math.Min(v.Length, votos.Length); i++)
                        votos[i] += v[i];
                }
            }

            float totalVotos = 0;
            foreach (float f in votos) totalVotos += f;
            if (totalVotos < 0.5f)
            {
                var rndSel = new Random(semilla);
                votos[rndSel.Next(11)] += (float)(rndSel.NextDouble()*4+2);
                votos[rndSel.Next(11)] += (float)(rndSel.NextDouble()*3+1);
                votos[rndSel.Next(11)] += (float)(rndSel.NextDouble()*2);
            }

            votos[(int)AlgoritmoBase.MundoVoxelMinecraft] = 0;

            float maxVoto = 0;
            foreach (float f in votos) if (f > maxVoto) maxVoto = f;
            float umbral = maxVoto * 0.30f;

            for (int i = 0; i < votos.Length; i++)
            {
                if (votos[i] >= umbral && votos[i] > 0)
                {
                    ctx.Algoritmos.Add((AlgoritmoBase)i);
                    ctx.PesosAlgoritmos.Add(votos[i]);
                }
            }

            if (ctx.Algoritmos.Count == 0)
            {
                ctx.Algoritmos.Add(AlgoritmoBase.RedNeuralCPPN);
                ctx.PesosAlgoritmos.Add(1.0f);
            }

            if (ctx.Algoritmos.Contains(AlgoritmoBase.Escena3D) ||
                ctx.Algoritmos.Contains(AlgoritmoBase.RedNeuralCPPN))
                ctx.EsProgresivo = true;

            // Prioridad exclusiva: si hay DibujoLapiz o Acuarela, se descartan los demás.
            if (ctx.Algoritmos.Contains(AlgoritmoBase.DibujoLapiz))
            {
                ctx.Algoritmos.Clear();
                ctx.PesosAlgoritmos.Clear();
                ctx.Algoritmos.Add(AlgoritmoBase.DibujoLapiz);
                ctx.PesosAlgoritmos.Add(1.0f);
                ctx.EsProgresivo = false;
                // Detectar estilo desde tokens
                foreach (string t in tokens)
                    if (BancoPalabras.MapaTemas.ContainsKey(t))
                        BancoPalabras.MapaTemas[t](ctx);
            }
            else if (ctx.Algoritmos.Contains(AlgoritmoBase.Acuarela))
            {
                ctx.Algoritmos.Clear();
                ctx.PesosAlgoritmos.Clear();
                ctx.Algoritmos.Add(AlgoritmoBase.Acuarela);
                ctx.PesosAlgoritmos.Add(1.0f);
                ctx.EsProgresivo = false;
            }

            if (ctx.Paleta.Count == 0)
            {
                var rp = new Random(semilla ^ 0x5A3C);
                ctx.Paleta.Add(Matematica.ColorHSV(rp.NextDouble()*360, 0.9, 0.9));
                ctx.Paleta.Add(Matematica.ColorHSV(rp.NextDouble()*360, 0.8, 0.85));
                ctx.Paleta.Add(Matematica.ColorHSV(rp.NextDouble()*360, 1.0, 1.0));
                ctx.Paleta.Add(Color.Black);
            }

            float total = 0;
            foreach (float p in ctx.PesosAlgoritmos) total += p;
            for (int i = 0; i < ctx.PesosAlgoritmos.Count; i++)
                ctx.PesosAlgoritmos[i] /= total;

            ctx.Escala     = Math.Max(0.2, Math.Min(5.0, ctx.Escala));
            ctx.Intensidad = Math.Max(0.3, Math.Min(2.5, ctx.Intensidad));
            ctx.Saturacion = Math.Max(0.1, Math.Min(3.0, ctx.Saturacion));
            ctx.Iteraciones= Math.Max(50,  Math.Min(800, ctx.Iteraciones));

            string algs = string.Join("+", ctx.Algoritmos.ConvertAll(delegate(AlgoritmoBase a){ return a.ToString(); }).ToArray());
            ctx.ResumenVisual = "[IA] " + algs + " «" + parametros + "»";
            return ctx;
        }
    }

    // ═══════════════════════════════════════════════════

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
            Bitmap dst = new Bitmap(w, h, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            BitmapData sdSrc = src.LockBits(new Rectangle(0,0,w,h), ImageLockMode.ReadOnly,  System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            BitmapData sdDst = dst.LockBits(new Rectangle(0,0,w,h), ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
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
            BitmapData sdS = suave.LockBits(new Rectangle(0,0,w,h), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            int stride = Math.Abs(sdS.Stride);
            byte[] pS = new byte[h*stride];
            Marshal.Copy(sdS.Scan0, pS, 0, pS.Length); suave.UnlockBits(sdS);

            Bitmap dst = new Bitmap(w, h, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            BitmapData sdD = dst.LockBits(new Rectangle(0,0,w,h), ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
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
            BitmapData sdO = src.LockBits(new Rectangle(0,0,w,h), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            BitmapData sdS = suave.LockBits(new Rectangle(0,0,w,h), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            int stride = Math.Abs(sdO.Stride);
            byte[] pO=new byte[h*stride]; byte[] pSm=new byte[h*stride];
            Marshal.Copy(sdO.Scan0,pO,0,pO.Length); Marshal.Copy(sdS.Scan0,pSm,0,pSm.Length);
            src.UnlockBits(sdO); suave.UnlockBits(sdS);
            Bitmap dst = new Bitmap(w,h,System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            BitmapData sdD = dst.LockBits(new Rectangle(0,0,w,h), ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
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
            Bitmap dst = new Bitmap(w,h,System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            BitmapData sdSrc=src.LockBits(new Rectangle(0,0,w,h),ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            BitmapData sdDst=dst.LockBits(new Rectangle(0,0,w,h),ImageLockMode.WriteOnly,System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            int stride=Math.Abs(sdSrc.Stride);
            byte[] pS=new byte[h*stride]; byte[] pD=new byte[h*stride];
            Marshal.Copy(sdSrc.Scan0,pS,0,pS.Length);
            for (int i=0;i<pS.Length;i++) pD[i]=(byte)Math.Max(0,Math.Min(255,(int)(128+(pS[i]-128)*factor)));
            Marshal.Copy(pD,0,sdDst.Scan0,pD.Length);
            src.UnlockBits(sdSrc); dst.UnlockBits(sdDst);
            return dst;
        }
    }

    // ═══════════════════════════════════════════════════

    public static class InterpretadorLienzo
    {
        private static readonly Dictionary<string,TipoEscena> _mapaTipo =
            new Dictionary<string,TipoEscena>(StringComparer.OrdinalIgnoreCase)
        {
            {"bosque",TipoEscena.Bosque},     {"forest",TipoEscena.Bosque},
            {"selva", TipoEscena.SelvaLluviosa},{"jungle",TipoEscena.SelvaLluviosa},
            {"rainforest",TipoEscena.SelvaLluviosa},
            {"arboleda",TipoEscena.Bosque},   {"arbol", TipoEscena.Bosque},
            {"trees",TipoEscena.Bosque},
            {"flores",TipoEscena.CampoFlores},{"flowers",TipoEscena.CampoFlores},
            {"campo", TipoEscena.CampoFlores},{"meadow",TipoEscena.CampoFlores},
            {"prado", TipoEscena.CampoFlores},{"floral",TipoEscena.CampoFlores},
            {"primavera",TipoEscena.CampoFlores},{"spring",TipoEscena.CampoFlores},
            {"montana",TipoEscena.Montana},   {"montaña",TipoEscena.Montana},
            {"mountain",TipoEscena.Montana},  {"nevado",TipoEscena.Montana},
            {"sierra", TipoEscena.Montana},   {"picos",TipoEscena.Montana},
            {"cumbre", TipoEscena.Montana},   {"glaciar",TipoEscena.Montana},
            {"desierto",TipoEscena.Desierto}, {"desert",TipoEscena.Desierto},
            {"arena",  TipoEscena.Desierto},  {"dunas",TipoEscena.Desierto},
            {"sahara", TipoEscena.Desierto},  {"calido",TipoEscena.Desierto},
            {"noche",  TipoEscena.Noche},     {"night",TipoEscena.Noche},
            {"luna",   TipoEscena.Noche},     {"moon",TipoEscena.Noche},
            {"nocturno",TipoEscena.Noche},    {"stars",TipoEscena.Noche},
            {"estrellas",TipoEscena.Noche},
            {"paisaje",TipoEscena.Paisaje3D}, {"landscape",TipoEscena.Paisaje3D},
            {"terreno",TipoEscena.Paisaje3D}, {"terrain",TipoEscena.Paisaje3D},
            {"planeta",TipoEscena.Paisaje3D}, {"planet",TipoEscena.Paisaje3D},
            {"volcan", TipoEscena.Volcan},    {"volcano",TipoEscena.Volcan},
            {"lava",   TipoEscena.Volcan},    {"erupcion",TipoEscena.Volcan},
            {"lago",   TipoEscena.Lago},      {"lake",TipoEscena.Lago},
            {"laguna", TipoEscena.Lago},      {"estanque",TipoEscena.Lago},
            {"pantano",TipoEscena.Pantano},   {"swamp",TipoEscena.Pantano},
            {"ciénaga",TipoEscena.Pantano},   {"marsh",TipoEscena.Pantano},
            {"valle",  TipoEscena.Valle},     {"valley",TipoEscena.Valle},
            {"rio",    TipoEscena.Valle},     {"river",TipoEscena.Valle},
            {"playa",  TipoEscena.Playa},     {"beach",TipoEscena.Playa},
            {"costa",  TipoEscena.Playa},
            {"caribe", TipoEscena.Playa},     {"island",TipoEscena.Playa},
            {"tundra", TipoEscena.Tundra},    {"artico",TipoEscena.Tundra},
            {"arctic", TipoEscena.Tundra},    {"aurora",TipoEscena.Tundra},
            {"polar",  TipoEscena.Tundra},    {"hielo",TipoEscena.Tundra},
            {"canon",  TipoEscena.CañonRocoso},{"cañon",TipoEscena.CañonRocoso},
            {"canyon", TipoEscena.CañonRocoso},{"barranco",TipoEscena.CañonRocoso},
            {"superficie",TipoEscena.Paisaje3D},
        };

        public static ContextoVisual Interpretar(string descripcion, int semilla)
        {
            var ctx = new ContextoVisual();
            if (semilla >= 0) ctx.Semilla = semilla;
            ctx.ModoLienzo   = true;
            ctx.EscenaLienzo = TipoEscena.Oceano;

            Char[] sep = new char[]{' ',',','.',';',':','!','?','-','_','/','\t','\n'};
            var tokens = string.IsNullOrWhiteSpace(descripcion)
                ? new List<string>()
                : new List<string>(descripcion.ToLower().Split(sep, StringSplitOptions.RemoveEmptyEntries));

            if (tokens.Count > 0 && tokens[tokens.Count-1] == "ia")
            {
                ctx.LienzoPostProceso = true;
                ctx.ModoIAPostProceso = true;
                tokens.RemoveAt(tokens.Count-1);
            }

            ctx.PalabrasDetectadas.AddRange(tokens);

            // Detectar negaciones: "sin X" / "without X" / "no X"
            var negados = new System.Collections.Generic.HashSet<string>();
            for (int i = 0; i < tokens.Count - 1; i++)
            {
                string t = tokens[i];
                if (t == "sin" || t == "without" || t == "no" || t == "ningún" || t == "ningun")
                    negados.Add(tokens[i+1]);
            }

            // Aplicar negaciones a campos
            if (negados.Contains("sol") || negados.Contains("sun"))      ctx.SinSol = true;
            if (negados.Contains("lluvia") || negados.Contains("rain"))   ctx.ConLluvia = false;
            if (negados.Contains("niebla") || negados.Contains("fog"))    ctx.ConNiebla = false;
            if (negados.Contains("nieve") || negados.Contains("snow"))    ctx.ConNieve  = false;
            if (negados.Contains("nubes") || negados.Contains("clouds"))  ctx.ConTormenta = false;

            foreach (string t in tokens)
                if (_mapaTipo.ContainsKey(t)) { ctx.EscenaLienzo = _mapaTipo[t]; break; }

            foreach (string t in tokens)
            {
                if (negados.Contains(t)) continue;
                if (BancoPalabras.MapaColores.ContainsKey(t))
                    ctx.Paleta.AddRange(BancoPalabras.MapaColores[t]);
                if (BancoPalabras.MapaModificadores.ContainsKey(t))
                    BancoPalabras.MapaModificadores[t](ctx);
                if (BancoPalabras.MapaTemas.ContainsKey(t))
                    BancoPalabras.MapaTemas[t](ctx);
            }

            ctx.Escala     = Math.Max(0.5, Math.Min(3.0, ctx.Escala));
            ctx.Intensidad = Math.Max(0.5, Math.Min(2.0, ctx.Intensidad));
            ctx.Saturacion = Math.Max(0.3, Math.Min(2.5, ctx.Saturacion));

            string desc2 = string.Join(" ", tokens.ToArray());
            ctx.ResumenVisual = string.Format("[Lienzo:{0}] {1}{2}",
                ctx.EscenaLienzo, desc2.Length > 0 ? desc2 : "escena",
                ctx.LienzoPostProceso ? " +IA" : "");
            return ctx;
        }
    }

    // ═══════════════════════════════════════════════════

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

    // ═══════════════════════════════════════════════════

    public class GeneradorAcuarela
    {
        private ContextoVisual _ctx;
        private Random _rnd;

        public GeneradorAcuarela(ContextoVisual ctx)
        {
            _ctx = ctx;
            _rnd = new Random(ctx.Semilla);
        }

        public Bitmap Generar(int ancho, int alto, Action<int> progreso)
        {
            Bitmap bmp = new Bitmap(ancho, alto, PixelFormat.Format24bppRgb);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode     = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.CompositingMode   = System.Drawing.Drawing2D.CompositingMode.SourceOver;
                g.CompositingQuality= System.Drawing.Drawing2D.CompositingQuality.HighQuality;

                // Fondo papel acuarela: blanco cálido
                g.Clear(Color.FromArgb(252, 248, 242));

                if (progreso!=null) progreso(3);

                // Detectar tema
                string tema = "abstracto";
                var pals = _ctx.PalabrasDetectadas;
                if (pals!=null)
                {
                    if (pals.Contains("bosque")||pals.Contains("forest")||pals.Contains("arbol")||pals.Contains("tree")) tema="bosque";
                    else if (pals.Contains("montana")||pals.Contains("mountain")||pals.Contains("peak")) tema="montana";
                    else if (pals.Contains("lago")||pals.Contains("lake")) tema="lago";
                    else if (pals.Contains("playa")||pals.Contains("beach")) tema="playa";
                    else if (pals.Contains("volcan")||pals.Contains("volcano")||pals.Contains("lava")) tema="volcan";
                    else if (pals.Contains("valle")||pals.Contains("valley")) tema="valle";
                    else if (pals.Contains("oceano")||pals.Contains("ocean")||pals.Contains("mar")||pals.Contains("sea")) tema="oceano";
                    else if (pals.Contains("flores")||pals.Contains("flower")||pals.Contains("campo")) tema="flores";
                    else if (pals.Contains("ciudad")||pals.Contains("city")) tema="ciudad";
                    else if (pals.Contains("desierto")||pals.Contains("desert")) tema="desierto";
                    else if (pals.Contains("noche")||pals.Contains("night")) tema="noche";
                }

                List<Color> pal = _ctx.Paleta.Count > 0 ? _ctx.Paleta
                    : PaletaTema(tema);

                // Textura de papel: granulado suave
                TexturaPapel(g, ancho, alto);
                if (progreso!=null) progreso(8);

                // Capas de pintura según tema
                switch (tema)
                {
                    case "oceano":   PintarOceano(g, ancho, alto, pal);   break;
                    case "montana":  PintarMontana(g, ancho, alto, pal);  break;
                    case "bosque":   PintarBosque(g, ancho, alto, pal);   break;
                    case "flores":   PintarFlores(g, ancho, alto, pal);   break;
                    case "ciudad":   PintarCiudad(g, ancho, alto, pal);   break;
                    case "desierto": PintarDesierto(g, ancho, alto, pal); break;
                    case "noche":    PintarNoche(g, ancho, alto, pal);    break;
                    case "lago":     PintarLago(g, ancho, alto, pal);     break;
                    case "valle":    PintarValle(g, ancho, alto, pal);    break;
                    case "volcan":   PintarVolcan(g, ancho, alto, pal);   break;
                    case "playa":    PintarPlaya(g, ancho, alto, pal);    break;
                    default:         PintarAbstracto(g, ancho, alto, pal); break;
                }
                if (progreso!=null) progreso(80);

                // Post: textura de acuarela y bordes húmedos
                BordesHumedos(g, ancho, alto);
                GranoFinal(g, ancho, alto);
                if (progreso!=null) progreso(96);
            }
            if (progreso!=null) progreso(100);
            return bmp;
        }

        // ── Utilidades de pincel ────────────────────────────────────────────

        private Color CA(Color c, int alfa)
        {
            return Color.FromArgb(Math.Min(255, alfa), c.R, c.G, c.B);
        }

        private Color Varia(Color c, int rango)
        {
            int r=Clamp(c.R+_rnd.Next(-rango,rango+1));
            int gv=Clamp(c.G+_rnd.Next(-rango,rango+1));
            int b=Clamp(c.B+_rnd.Next(-rango,rango+1));
            return Color.FromArgb(r,gv,b);
        }
        private int Clamp(int v){return v<0?0:v>255?255:v;}
        private static float CF(float v){return v<0?0:v>1?1:v;}

        // Mancha de acuarela: elipse con borde irregular y transparencia
        private void Mancha(Graphics g, float cx, float cy, float rx, float ry,
                            Color color, int alfa, float irregularidad=0.25f)
        {
            int nPts = 18 + _rnd.Next(8);
            var pts = new PointF[nPts];
            for (int i=0; i<nPts; i++)
            {
                double ang = i * Math.PI*2/nPts;
                double varR = 1.0 + (_rnd.NextDouble()-0.5)*irregularidad*2;
                float px = cx + (float)(Math.Cos(ang)*rx*varR);
                float py = cy + (float)(Math.Sin(ang)*ry*varR);
                pts[i] = new PointF(px, py);
            }
            using (var path = new System.Drawing.Drawing2D.GraphicsPath())
            {
                path.AddClosedCurve(pts, 0.4f);
                using (var br = new SolidBrush(CA(color, alfa)))
                    g.FillPath(br, path);
            }
        }

        // Pincelada tipo acuarela: bezier suave con variación de grosor
        private void Pincelada(Graphics g, float x0, float y0, float x1, float y1,
                               float grosor, Color color, int alfa)
        {
            float mx=(x0+x1)/2 + (float)(_rnd.NextDouble()-0.5)*grosor*2;
            float my=(y0+y1)/2 + (float)(_rnd.NextDouble()-0.5)*grosor*2;
            using (var path = new System.Drawing.Drawing2D.GraphicsPath())
            {
                path.AddBezier(x0,y0, mx-(y1-y0)*0.3f, my+(x1-x0)*0.3f,
                                       mx+(y1-y0)*0.3f, my-(x1-x0)*0.3f, x1,y1);
                using (var pen = new Pen(CA(color, alfa), grosor))
                {
                    pen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
                    pen.EndCap   = System.Drawing.Drawing2D.LineCap.Round;
                    g.DrawPath(pen, path);
                }
            }
        }

        // ── Textura de papel ────────────────────────────────────────────────

        private void TexturaPapel(Graphics g, int w, int h)
        {
            // Manchas de humedad sutil
            int nManchas = 6 + _rnd.Next(5);
            for (int i=0; i<nManchas; i++)
            {
                float cx=(float)(_rnd.NextDouble()*w);
                float cy=(float)(_rnd.NextDouble()*h);
                float rx=(float)(w*0.12 + _rnd.NextDouble()*w*0.18);
                float ry=(float)(h*0.10 + _rnd.NextDouble()*h*0.14);
                Mancha(g, cx, cy, rx, ry, Color.FromArgb(200,185,165), 8+_rnd.Next(8), 0.3f);
            }
        }

        private void GranoFinal(Graphics g, int w, int h)
        {
            // Grano de papel: puntos semitransparentes rápidos con SolidBrush
            var rndG = new Random(_ctx.Semilla^0xBEEF);
            int nGrains = (int)(w * h * 0.0018);
            for (int i = 0; i < nGrains; i++)
            {
                int x = rndG.Next(w-1), y = rndG.Next(h-1);
                int alfa = rndG.Next(5, 20);
                using (var br = new SolidBrush(Color.FromArgb(alfa, 110, 100, 90)))
                    g.FillRectangle(br, x, y, 1, 1);
            }
        }

        private void BordesHumedos(Graphics g, int w, int h)
        {
            // Bordes húmedos: acumulación de pigmento en bordes de manchas
            // Simulado con viñeta muy sutil
            using (var path = new System.Drawing.Drawing2D.GraphicsPath())
            {
                path.AddRectangle(new RectangleF(0,0,w,h));
                var ellR = new RectangleF(w*0.05f,h*0.05f,w*0.9f,h*0.9f);
                path.AddEllipse(ellR);
                using (var br = new System.Drawing.Drawing2D.PathGradientBrush(path))
                {
                    br.CenterColor = Color.Transparent;
                    br.SurroundColors = new Color[]{ Color.FromArgb(18, 140, 120, 100) };
                    g.FillRectangle(br, 0, 0, w, h);
                }
            }
        }

        // ── Paletas por tema ────────────────────────────────────────────────

        private List<Color> PaletaTema(string tema)
        {
            switch(tema)
            {
                case "oceano":  return new List<Color>{ Color.FromArgb(30,100,180), Color.FromArgb(60,160,220), Color.FromArgb(180,210,240), Color.FromArgb(240,200,100) };
                case "montana": return new List<Color>{ Color.FromArgb(80,100,140), Color.FromArgb(160,180,200), Color.FromArgb(220,225,235), Color.FromArgb(60,120,60) };
                case "bosque":  return new List<Color>{ Color.FromArgb(20,80,30), Color.FromArgb(50,130,50), Color.FromArgb(80,160,60), Color.FromArgb(130,100,40) };
                case "flores":  return new List<Color>{ Color.FromArgb(220,60,90), Color.FromArgb(255,180,30), Color.FromArgb(140,60,200), Color.FromArgb(50,160,80) };
                case "desierto":return new List<Color>{ Color.FromArgb(210,170,80), Color.FromArgb(230,140,50), Color.FromArgb(180,120,40), Color.FromArgb(250,220,140) };
                case "noche":   return new List<Color>{ Color.FromArgb(10,15,60), Color.FromArgb(30,40,120), Color.FromArgb(80,60,140), Color.FromArgb(200,180,240) };
                case "lago":    return new List<Color>{ Color.FromArgb(30,90,160), Color.FromArgb(80,150,210), Color.FromArgb(40,110,50), Color.FromArgb(200,220,240) };
                case "valle":   return new List<Color>{ Color.FromArgb(50,130,50), Color.FromArgb(80,160,70), Color.FromArgb(140,180,100), Color.FromArgb(100,150,210) };
                case "volcan":  return new List<Color>{ Color.FromArgb(200,50,10), Color.FromArgb(255,120,0), Color.FromArgb(40,40,40), Color.FromArgb(180,30,5) };
                case "playa":   return new List<Color>{ Color.FromArgb(0,150,180), Color.FromArgb(200,185,130), Color.FromArgb(80,180,200), Color.FromArgb(240,210,100) };
                default:        return new List<Color>{ Color.FromArgb(70,110,180), Color.FromArgb(200,80,60), Color.FromArgb(50,140,80), Color.FromArgb(220,170,60) };
            }
        }

        // ── Pintores temáticos ──────────────────────────────────────────────

        private void PintarOceano(Graphics g, int w, int h, List<Color> pal)
        {
            int hz = (int)(h*0.42);
            // Cielo: degradado con manchas de nube
            var colCielo1 = pal.Count>3 ? pal[3] : Color.FromArgb(240,200,100);
            var colCielo2 = Color.FromArgb(180,210,240);
            PintarCieloAcuarela(g, w, hz, colCielo1, colCielo2);

            // Agua: capas horizontales de azul
            var colAgua1 = pal[0]; var colAgua2 = pal.Count>1?pal[1]:Color.FromArgb(60,160,220);
            int nCapas = 18 + _rnd.Next(10);
            for (int i=0; i<nCapas; i++)
            {
                float y = hz + i*(float)(h-hz)/nCapas + (float)(_rnd.NextDouble()-0.5)*8;
                float pct = (float)i/nCapas;
                Color cOla = MixColor(colAgua1, colAgua2, pct);
                float grosor = (4+pct*20) + (float)_rnd.NextDouble()*8;
                int alfa = (int)(40+pct*55+_rnd.NextDouble()*25);
                float x0=(float)(_rnd.NextDouble()*w*0.15);
                float x1=(float)(w-_rnd.NextDouble()*w*0.15);
                Pincelada(g, x0,y, x1,y, grosor, Varia(cOla,18), alfa);
            }
            // Espuma: blanco horizontal
            for (int i=0; i<8; i++)
            {
                float y=hz+(float)(_rnd.NextDouble()*(h-hz)*0.4);
                float x0=(float)(_rnd.NextDouble()*w*0.3); float x1=(float)(w*0.5+_rnd.NextDouble()*w*0.5);
                Pincelada(g,x0,y,x1,y,3+_rnd.Next(5),Color.FromArgb(220,235,245),55+_rnd.Next(35));
            }
            // Sol/reflejo
            PintarSolAcuarela(g, w, hz, colCielo1);
        }

        private void PintarMontana(Graphics g, int w, int h, List<Color> pal)
        {
            int hz = (int)(h*0.45);
            var colCielo = pal.Count>1?pal[1]:Color.FromArgb(160,180,200);
            PintarCieloAcuarela(g, w, hz, colCielo, Color.FromArgb(210,220,235));

            // Montañas: 4 planos con silueta FBM
            Color[] colMont = { Color.FromArgb(160,175,200), Color.FromArgb(110,130,160),
                                Color.FromArgb(80,100,130),  pal[0] };
            var rndM = new Random(_ctx.Semilla);
            for (int pl=0; pl<4; pl++)
            {
                double tS = _ctx.Semilla*0.005+pl*6.7;
                double altMax = 0.20+pl*0.08;
                int yBase = hz - (int)(h*0.02*pl);
                int nPts = w/4;
                var pts = new List<PointF>();
                pts.Add(new PointF(0, h));
                for (int xi=0; xi<=nPts; xi++)
                {
                    double nx=(double)xi/nPts;
                    double n1=Matematica.FBM(nx*3.0+tS, 0.5, 5, 0.58);
                    double n2=Matematica.FBM(nx*7.0+tS+3, 0.3, 3, 0.52)*0.25;
                    double alt=((n1+n2)+1)*0.5;
                    float py = yBase - (float)(h*altMax*alt);
                    float px = (float)(nx*w);
                    float jy = (float)(rndM.NextDouble()*3-1.5);
                    pts.Add(new PointF(px, Math.Max(5,py+jy)));
                }
                pts.Add(new PointF(w, h));
                using (var path = new System.Drawing.Drawing2D.GraphicsPath())
                {
                    path.AddLines(pts.ToArray());
                    path.CloseFigure();
                    int alfa = 110 + pl*28;
                    using (var br = new SolidBrush(CA(Varia(colMont[pl],15), alfa)))
                        g.FillPath(br, path);
                    // Nieve en la cima
                    if (pl>=2)
                    {
                        using (var br2 = new SolidBrush(Color.FromArgb(45, 240,242,248)))
                            g.FillPath(br2, path);
                    }
                }
            }
            // Pinos al pie
            PintarPinosAcuarela(g, w, h, hz, pal.Count>2?pal[2]:Color.FromArgb(20,70,30));
        }

        private void PintarBosque(Graphics g, int w, int h, List<Color> pal)
        {
            int hz = (int)(h*0.42);
            // Cielo entre los árboles: azul/verde difuso
            PintarCieloAcuarela(g, w, hz, Color.FromArgb(160,200,230), Color.FromArgb(200,220,200));

            // Suelo con degradado
            using (var br = new System.Drawing.Drawing2D.LinearGradientBrush(
                new PointF(0,hz), new PointF(0,h),
                CA(pal.Count>0?pal[0]:Color.FromArgb(20,80,30), 200),
                CA(pal.Count>0?MixColor(pal[0],Color.Black,0.3f):Color.FromArgb(10,40,10), 220)))
                g.FillRectangle(br, 0, hz, w, h-hz);

            // Árboles: 3 planos de siluetas de copa
            double[] escPlano = {10.0, 6.5, 4.0};
            int[]    yOffPlano= {0, (int)(h*0.06), (int)(h*0.12)};
            Color[]  colPlano = {
                Color.FromArgb(40,100,40), Color.FromArgb(20,70,25), pal[0]
            };
            var rndT = new Random(_ctx.Semilla);
            for (int pl=0; pl<3; pl++)
            {
                double tS=_ctx.Semilla*0.003+pl*7.3;
                double esc=escPlano[pl];
                int yB=hz+yOffPlano[pl];
                double altMax=0.22+pl*0.08;
                int nPts=w/3;
                var pts=new List<PointF>();
                pts.Add(new PointF(-10,h));
                for (int xi=0; xi<=nPts; xi++)
                {
                    double nx=(double)xi/nPts;
                    double nA=Matematica.FBM(nx*esc+tS,1.0+pl,4,0.60);
                    double nD=Matematica.FBM(nx*esc*3.5+tS+5,0.5,3,0.55)*0.28;
                    double alt=((nA+nD)+1)*0.5;
                    float py=yB-(float)(h*altMax*alt);
                    float jy=(float)(rndT.NextDouble()*4-2);
                    pts.Add(new PointF((float)(nx*w),Math.Max(5,py+jy)));
                }
                pts.Add(new PointF(w+10,h));
                using (var path = new System.Drawing.Drawing2D.GraphicsPath())
                {
                    path.AddCurve(pts.ToArray(), 0.3f);
                    path.AddLine(w+10,h,-10,h);
                    path.CloseFigure();
                    int alfa=150+pl*25;
                    using (var br = new SolidBrush(CA(Varia(colPlano[pl],12),alfa)))
                        g.FillPath(br, path);
                }
            }
            // Luz solar filtrada
            using (var br = new SolidBrush(Color.FromArgb(25,255,240,180)))
                g.FillRectangle(br, 0, 0, w, h);
        }

        private void PintarFlores(Graphics g, int w, int h, List<Color> pal)
        {
            int hz = (int)(h*0.40);
            // Cielo
            PintarCieloAcuarela(g, w, hz, Color.FromArgb(180,215,255), Color.FromArgb(220,235,255));

            // Pasto base
            using (var br = new System.Drawing.Drawing2D.LinearGradientBrush(
                new PointF(0,hz), new PointF(0,h),
                Color.FromArgb(180, 80,160,60), Color.FromArgb(220, 40,110,30)))
                g.FillRectangle(br, 0, hz, w, h-hz);

            // Flores en múltiples capas — más pequeñas al fondo, grandes adelante
            Color[] florCols = pal.Count>=4 ? pal.ToArray() : new Color[]{
                Color.FromArgb(220,60,90), Color.FromArgb(255,180,30),
                Color.FromArgb(140,60,200), Color.FromArgb(255,80,150),
                Color.FromArgb(60,180,255), Color.FromArgb(255,120,40)
            };
            var rndF = new Random(_ctx.Semilla);
            // Fondo: flores pequeñas densas
            for (int i=0; i<400; i++)
            {
                float fx=(float)(rndF.NextDouble()*w);
                float fy=(float)(hz + rndF.NextDouble()*(h-hz)*0.6);
                float pct=(fy-hz)/(h-hz);
                float rx=(float)(3+pct*6); float ry=(float)(3+pct*5);
                Color cf=florCols[rndF.Next(florCols.Length)];
                int alfa=(int)(50+pct*80);
                Mancha(g,fx,fy,rx,ry,cf,alfa,0.4f);
            }
            // Primer plano: flores grandes con detalle
            for (int i=0; i<80; i++)
            {
                float fx=(float)(rndF.NextDouble()*w);
                float fy=(float)(hz + (h-hz)*0.4 + rndF.NextDouble()*(h-hz)*0.55);
                float pct=(fy-hz)/(h-hz);
                float rx=(float)(8+pct*18); float ry=(float)(7+pct*14);
                Color cf=florCols[rndF.Next(florCols.Length)];
                Mancha(g,fx,fy,rx,ry,cf,(int)(80+pct*120),0.45f);
                // Centro amarillo
                Mancha(g,fx,fy,rx*0.28f,ry*0.28f,Color.FromArgb(255,220,30),(int)(120+pct*80),0.2f);
            }
        }

        private void PintarDesierto(Graphics g, int w, int h, List<Color> pal)
        {
            int hz=(int)(h*0.38);
            PintarCieloAcuarela(g,w,hz,pal.Count>1?pal[1]:Color.FromArgb(230,180,80),Color.FromArgb(250,220,140));

            // Dunas: formas sinuosas con degradado
            Color cDuna1=pal[0]; Color cDuna2=pal.Count>2?pal[2]:Color.FromArgb(180,120,40);
            var rndD=new Random(_ctx.Semilla);
            for (int duna=0; duna<5; duna++)
            {
                double tS=_ctx.Semilla*0.004+duna*3.1;
                int yB=hz+(int)((h-hz)*(0.1+duna*0.18));
                int nPts=w/3;
                var pts=new List<PointF>();
                pts.Add(new PointF(-10,h));
                for (int xi=0; xi<=nPts; xi++)
                {
                    double nx=(double)xi/nPts;
                    double n=Matematica.FBM(nx*2.5+tS,0.5,4,0.55);
                    float py=yB-(float)((n+1)*0.5*(h-hz)*0.22);
                    pts.Add(new PointF((float)(nx*w),Math.Max(hz,py)));
                }
                pts.Add(new PointF(w+10,h));
                using (var path=new System.Drawing.Drawing2D.GraphicsPath())
                {
                    path.AddCurve(pts.ToArray(),0.4f);
                    path.AddLine(w+10,h,-10,h);
                    path.CloseFigure();
                    Color cD=MixColor(cDuna1,cDuna2,(float)duna/4);
                    using (var br=new SolidBrush(CA(Varia(cD,15),160+duna*15)))
                        g.FillPath(br,path);
                }
            }
            PintarSolAcuarela(g,w,hz,pal.Count>1?pal[1]:Color.FromArgb(255,200,60));
        }

        private void PintarCiudad(Graphics g, int w, int h, List<Color> pal)
        {
            int hz=(int)(h*0.45);
            PintarCieloAcuarela(g,w,hz,Color.FromArgb(160,190,230),Color.FromArgb(200,215,240));
            // Edificios: rectángulos con acuarela
            var rndC=new Random(_ctx.Semilla);
            int nEdif=10+rndC.Next(8);
            for (int i=0; i<nEdif; i++)
            {
                int bw=30+rndC.Next(80); int bh=60+rndC.Next(200);
                int bx=rndC.Next(w-bw); int by=hz-bh;
                Color cE=pal.Count>0?Varia(pal[rndC.Next(pal.Count)],20):Color.FromArgb(100,120,160);
                using (var br=new SolidBrush(CA(cE,100+rndC.Next(60))))
                    g.FillRectangle(br,bx,by,bw,bh);
                // Ventanas
                for (int vy=0; vy<bh/20; vy++) for (int vx=0; vx<bw/16; vx++)
                {
                    if (bx+6+vx*16+10>bx+bw-4) continue;
                    bool enc=rndC.NextDouble()>0.4;
                    Color cV=enc?Color.FromArgb(255,240,180):Color.FromArgb(80,100,140);
                    using (var br=new SolidBrush(CA(cV,enc?130:80)))
                        g.FillRectangle(br,bx+6+vx*16,by+8+vy*20,8,11);
                }
            }
        }

        private void PintarAbstracto(Graphics g, int w, int h, List<Color> pal)
        {
            var rndA=new Random(_ctx.Semilla);
            // Capas de manchas grandes a pequeñas
            for (int capa=0; capa<4; capa++)
            {
                int n=8+capa*12;
                for (int i=0; i<n; i++)
                {
                    float cx=(float)(rndA.NextDouble()*w);
                    float cy=(float)(rndA.NextDouble()*h);
                    float rx=(float)(w*(0.05+rndA.NextDouble()*0.20/(capa+1)));
                    float ry=(float)(h*(0.04+rndA.NextDouble()*0.18/(capa+1)));
                    Color c=pal[rndA.Next(pal.Count)];
                    int alfa=40+capa*20+rndA.Next(50);
                    Mancha(g,cx,cy,rx,ry,Varia(c,20),alfa,0.35f);
                }
            }
        }

        // ── Helpers de escena ───────────────────────────────────────────────

        private void PintarCieloAcuarela(Graphics g, int w, int hz, Color c1, Color c2)
        {
            using (var br=new System.Drawing.Drawing2D.LinearGradientBrush(
                new PointF(0,0), new PointF(0,hz), CA(c1,200), CA(c2,180)))
                g.FillRectangle(br,0,0,w,hz);
            // Nubes difusas
            var rndN=new Random(_ctx.Semilla^0x1234);
            int nNubes=4+rndN.Next(5);
            for (int i=0; i<nNubes; i++)
            {
                float cx=(float)(rndN.NextDouble()*w);
                float cy=(float)(rndN.NextDouble()*hz*0.7);
                float rx=(float)(w*0.06+rndN.NextDouble()*w*0.14);
                float ry=(float)(hz*0.04+rndN.NextDouble()*hz*0.08);
                Mancha(g,cx,cy,rx,ry,Color.FromArgb(245,248,252),35+rndN.Next(35),0.5f);
            }
        }

        private void PintarSolAcuarela(Graphics g, int w, int hz, Color cSol)
        {
            var rndS=new Random(_ctx.Semilla^0x5678);
            float sx=(float)(w*(0.55+rndS.NextDouble()*0.3));
            float sy=(float)(hz*(0.12+rndS.NextDouble()*0.25));
            float sr=(float)(Math.Min(w,hz)*0.055);
            // Glow
            Mancha(g,sx,sy,sr*3.5f,sr*3.5f,cSol,20,0.4f);
            Mancha(g,sx,sy,sr*2.0f,sr*2.0f,cSol,35,0.3f);
            // Disco solar
            Mancha(g,sx,sy,sr,sr,Color.FromArgb(255,252,240),180,0.08f);
        }

        private void PintarPinosAcuarela(Graphics g, int w, int h, int hz, Color cPino)
        {
            var rndP=new Random(_ctx.Semilla^0x9999);
            int nPinos=w/80+rndP.Next(5);
            for (int i=0; i<nPinos; i++)
            {
                float px=(float)(rndP.NextDouble()*w);
                float py=(float)(hz+rndP.NextDouble()*(h-hz)*0.2);
                float alt=(float)(h*0.12+rndP.NextDouble()*h*0.15);
                float an=alt*0.35f;
                // Silueta de pino: triángulos apilados
                for (int ni=0; ni<3; ni++)
                {
                    float ty=py-alt*0.2f-ni*alt*0.28f;
                    float tw=an*(1.0f-ni*0.25f);
                    var triPts=new PointF[]{ new PointF(px,ty-alt*0.3f), new PointF(px-tw,ty), new PointF(px+tw,ty) };
                    using (var br=new SolidBrush(CA(Varia(cPino,15),150)))
                        g.FillPolygon(br,triPts);
                }
            }
        }

        private void PintarNoche(Graphics g, int w, int h, List<Color> pal)
        {
            int hz = (int)(h * 0.45);
            // Cielo nocturno con degradado
            using (var br = new System.Drawing.Drawing2D.LinearGradientBrush(
                new PointF(0,0), new PointF(0,hz),
                CA(pal[0], 245), CA(pal.Count>1?pal[1]:Color.FromArgb(30,40,120), 225)))
                g.FillRectangle(br, 0, 0, w, hz);

            // Estrellas en capas: muchas pequeñas + pocas grandes con halo
            var rndN = new Random(_ctx.Semilla);
            for (int i = 0; i < 400; i++)
            {
                float sx = (float)(rndN.NextDouble() * w);
                float sy = (float)(rndN.NextDouble() * hz * 0.95f);
                int sa = 100 + rndN.Next(155);
                float sr = rndN.NextDouble() < 0.08 ? 2.5f : 1f;
                if (sr > 2) Mancha(g, sx, sy, sr*2, sr*2, Color.FromArgb(220,220,255), 30, 0.3f);
                Mancha(g, sx, sy, sr, sr, Color.FromArgb(240, 238, 255), sa, 0.1f);
            }

            // Via láctea: banda difusa diagonal
            for (int i = 0; i < 80; i++)
            {
                float bx = (float)(rndN.NextDouble() * w);
                float by = (float)(rndN.NextDouble() * hz * 0.7f + hz * 0.05f);
                float brx = (float)(w * 0.03 + rndN.NextDouble() * w * 0.05);
                float bry = (float)(hz * 0.02 + rndN.NextDouble() * hz * 0.04);
                Mancha(g, bx, by, brx, bry, Color.FromArgb(180, 190, 255), 8 + rndN.Next(12), 0.6f);
            }

            // Luna con halo
            float lx = (float)(w * 0.72f), ly = (float)(hz * 0.18f);
            float lr = (float)(Math.Min(w, hz) * 0.048f);
            Mancha(g, lx, ly, lr * 4, lr * 4, Color.FromArgb(200, 210, 255), 18, 0.5f);
            Mancha(g, lx, ly, lr * 2, lr * 2, Color.FromArgb(220, 225, 255), 45, 0.2f);
            Mancha(g, lx, ly, lr, lr, Color.FromArgb(245, 248, 230), 210, 0.08f);

            // Terreno oscuro con silueta de árboles/montañas
            Color cTierra = pal[0];
            int yTerr = hz - (int)(hz * 0.08f);
            int nPtsTerr = w / 5;
            var ptsTerr = new List<PointF>();
            ptsTerr.Add(new PointF(-5, h));
            for (int xi = 0; xi <= nPtsTerr; xi++)
            {
                double nx = (double)xi / nPtsTerr;
                double n = Matematica.FBM(nx * 4.0 + _ctx.Semilla * 0.003, 0.5, 4, 0.58);
                float py = yTerr - (float)((n + 1) * 0.5 * hz * 0.22);
                ptsTerr.Add(new PointF((float)(nx * w), Math.Max(5, py)));
            }
            ptsTerr.Add(new PointF(w + 5, h));
            using (var path = new System.Drawing.Drawing2D.GraphicsPath())
            {
                path.AddCurve(ptsTerr.ToArray(), 0.3f);
                path.AddLine(w + 5, h, -5, h);
                path.CloseFigure();
                using (var br = new SolidBrush(CA(MixColor(cTierra, Color.Black, 0.4f), 240)))
                    g.FillPath(br, path);
            }

            // Reflejo de luna en agua oscura al pie
            int yAgua = (int)(hz + (h - hz) * 0.1f);
            using (var br = new System.Drawing.Drawing2D.LinearGradientBrush(
                new PointF(0, yAgua), new PointF(0, h),
                CA(MixColor(pal[0], pal.Count > 1 ? pal[1] : Color.FromArgb(10, 30, 80), 0.5f), 200),
                CA(MixColor(pal[0], Color.Black, 0.6f), 220)))
                g.FillRectangle(br, 0, yAgua, w, h - yAgua);
            // Franja de reflejo de luna sobre el agua
            for (int i = 0; i < 12; i++)
            {
                float ry = yAgua + i * (float)(h - yAgua) / 12f;
                float rw2 = (8 + i * 6) + (float)(rndN.NextDouble() * 20);
                float rx = lx + (float)(rndN.NextDouble() * 10 - 5);
                float al = Math.Max(0, 80 - i * 5);
                using (var br = new SolidBrush(Color.FromArgb((int)al, 240, 240, 210)))
                    g.FillEllipse(br, rx - rw2 / 2, ry, rw2, 4);
            }
        }

        private void PintarLago(Graphics g, int w, int h, List<Color> pal)
        {
            int hz = (int)(h * 0.42);
            // Cielo claro con matices suaves
            PintarCieloAcuarela(g, w, hz, pal.Count > 3 ? pal[3] : Color.FromArgb(200, 220, 240),
                                           Color.FromArgb(230, 240, 252));

            // Vegetación perimetral (orilla)
            Color cVeg = pal.Count > 2 ? pal[2] : Color.FromArgb(40, 110, 50);
            using (var br = new System.Drawing.Drawing2D.LinearGradientBrush(
                new PointF(0, hz), new PointF(0, h),
                CA(cVeg, 180), CA(MixColor(cVeg, Color.FromArgb(30, 70, 20), 0.5f), 200)))
                g.FillRectangle(br, 0, hz, w, h - hz);

            // Lago: elipse central con reflejo de cielo
            float lakeCX = w * 0.5f, lakeCY = hz + (h - hz) * 0.38f;
            float lakeRX = w * 0.32f, lakeRY = (h - hz) * 0.26f;
            var rndL = new Random(_ctx.Semilla);

            // Agua con varias capas de transparencia
            Color cAgua = pal[0];
            for (int capa = 3; capa >= 0; capa--)
            {
                float scale = 1.0f - capa * 0.08f;
                int alfa = 60 + capa * 35;
                Mancha(g, lakeCX, lakeCY, lakeRX * scale, lakeRY * scale,
                       MixColor(cAgua, Color.FromArgb(180, 210, 240), capa * 0.2f), alfa, 0.18f);
            }

            // Reflejo especular del cielo en el agua
            Color cReflejo = pal.Count > 3 ? pal[3] : Color.FromArgb(200, 220, 240);
            Mancha(g, lakeCX, lakeCY - lakeRY * 0.2f, lakeRX * 0.55f, lakeRY * 0.35f, cReflejo, 45, 0.3f);

            // Ondas concéntricas sutiles
            for (int i = 1; i <= 4; i++)
            {
                float scale = (float)i / 4;
                using (var pen = new Pen(CA(Color.FromArgb(180, 210, 240), 25 + i * 8), 1.2f))
                    g.DrawEllipse(pen, lakeCX - lakeRX * scale, lakeCY - lakeRY * scale,
                                  lakeRX * scale * 2, lakeRY * scale * 2);
            }

            // Árboles en la orilla
            PintarPinosAcuarela(g, w, h, hz, cVeg);

            // Lirios/nenúfares en el agua
            var rndNen = new Random(_ctx.Semilla ^ 0x3333);
            for (int i = 0; i < 12; i++)
            {
                float angle = (float)(rndNen.NextDouble() * Math.PI * 2);
                float dist = (float)(rndNen.NextDouble() * 0.75);
                float nx = lakeCX + (float)(Math.Cos(angle) * lakeRX * dist);
                float ny = lakeCY + (float)(Math.Sin(angle) * lakeRY * dist);
                float nr = 4 + (float)(rndNen.NextDouble() * 6);
                Mancha(g, nx, ny, nr, nr * 0.55f, Color.FromArgb(50, 140, 60), 120, 0.3f);
                if (rndNen.NextDouble() > 0.5f)
                    Mancha(g, nx, ny, nr * 0.5f, nr * 0.5f, Color.FromArgb(255, 180, 200), 160, 0.2f);
            }

            PintarSolAcuarela(g, w, hz, Color.FromArgb(255, 240, 180));
        }

        private void PintarValle(Graphics g, int w, int h, List<Color> pal)
        {
            int hz = (int)(h * 0.42);
            PintarCieloAcuarela(g, w, hz, Color.FromArgb(150, 195, 245), Color.FromArgb(210, 230, 252));

            // Montañas al fondo (lejanas, desaturadas)
            Color cMontLejos = Color.FromArgb(170, 185, 210);
            var rndV = new Random(_ctx.Semilla);
            int nPts = w / 4;
            var ptsM = new List<PointF>();
            ptsM.Add(new PointF(-5, h));
            for (int xi = 0; xi <= nPts; xi++)
            {
                double nx = (double)xi / nPts;
                double n = Matematica.FBM(nx * 2.5 + _ctx.Semilla * 0.004, 0.5, 5, 0.58);
                float py = hz - (float)((n + 1) * 0.5 * hz * 0.45);
                ptsM.Add(new PointF((float)(nx * w), Math.Max(5, py)));
            }
            ptsM.Add(new PointF(w + 5, h));
            using (var path = new System.Drawing.Drawing2D.GraphicsPath())
            {
                path.AddCurve(ptsM.ToArray(), 0.3f);
                path.AddLine(w + 5, h, -5, h);
                path.CloseFigure();
                using (var br = new SolidBrush(CA(cMontLejos, 170)))
                    g.FillPath(br, path);
            }

            // Valle verde — suelo con colinas suaves
            Color cValle = pal.Count > 0 ? pal[0] : Color.FromArgb(60, 140, 50);
            Color cValle2 = pal.Count > 1 ? pal[1] : Color.FromArgb(90, 170, 70);
            using (var br = new System.Drawing.Drawing2D.LinearGradientBrush(
                new PointF(0, hz), new PointF(0, h),
                CA(cValle2, 190), CA(cValle, 220)))
                g.FillRectangle(br, 0, hz, w, h - hz);

            // Río serpenteante por el centro
            Color cRio = Color.FromArgb(80, 150, 210);
            int nRioPts = 20;
            var ptsRio = new PointF[nRioPts];
            for (int i = 0; i < nRioPts; i++)
            {
                double t = (double)i / (nRioPts - 1);
                float rx = (float)(w * 0.5 + Math.Sin(t * Math.PI * 3 + _ctx.Semilla * 0.01) * w * 0.18);
                float ry = hz + (float)(t * (h - hz));
                ptsRio[i] = new PointF(rx, ry);
            }
            for (int thick = 3; thick >= 1; thick--)
            {
                using (var pen = new Pen(CA(cRio, 60 + thick * 30), thick * 5))
                {
                    pen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
                    pen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
                    pen.LineJoin = System.Drawing.Drawing2D.LineJoin.Round;
                    g.DrawCurve(pen, ptsRio, 0.5f);
                }
            }

            // Bosque a ambos lados del valle
            PintarPinosAcuarela(g, w / 3, h, hz, cValle);
            PintarPinosAcuarela(g, w, h, hz, cValle);

            // Flores en el prado
            var rndF = new Random(_ctx.Semilla ^ 0x7777);
            Color[] cfls = { Color.FromArgb(255, 220, 60), Color.FromArgb(220, 60, 90), Color.FromArgb(255, 255, 100) };
            for (int i = 0; i < 120; i++)
            {
                float fx = (float)(rndF.NextDouble() * w);
                float fy = hz + (float)(rndF.NextDouble() * (h - hz));
                float pct = (fy - hz) / (h - hz);
                Mancha(g, fx, fy, 3 + pct * 8, 3 + pct * 7, cfls[rndF.Next(cfls.Length)], 80 + (int)(pct * 100), 0.4f);
            }
            PintarSolAcuarela(g, w, hz, Color.FromArgb(255, 235, 160));
        }

        private void PintarVolcan(Graphics g, int w, int h, List<Color> pal)
        {
            int hz = (int)(h * 0.45);
            // Cielo oscuro / anaranjado
            using (var br = new System.Drawing.Drawing2D.LinearGradientBrush(
                new PointF(0, 0), new PointF(0, hz),
                Color.FromArgb(230, 20, 10, 5), Color.FromArgb(220, 80, 30, 10)))
                g.FillRectangle(br, 0, 0, w, hz);

            // Humo: manchas oscuras en el cielo
            var rndV = new Random(_ctx.Semilla);
            for (int i = 0; i < 25; i++)
            {
                float cx = (float)(w * 0.35 + rndV.NextDouble() * w * 0.3);
                float cy = (float)(rndV.NextDouble() * hz * 0.7);
                Mancha(g, cx, cy, (float)(w * 0.05 + rndV.NextDouble() * w * 0.12),
                       (float)(hz * 0.04 + rndV.NextDouble() * hz * 0.1),
                       Color.FromArgb(30, 25, 20), 25 + rndV.Next(40), 0.5f);
            }

            // Volcán: cono central con silueta
            Color cVolcan = Color.FromArgb(20, 15, 10);
            int volcX = w / 2, volcBase = h;
            int volcW = (int)(w * 0.55), volcH = (int)(h * 0.75);
            var ptsCono = new PointF[] {
                new PointF(volcX - volcW / 2, volcBase),
                new PointF(volcX - (int)(volcW * 0.08), hz + 2),
                new PointF(volcX + (int)(volcW * 0.08), hz + 2),
                new PointF(volcX + volcW / 2, volcBase)
            };
            using (var path = new System.Drawing.Drawing2D.GraphicsPath())
            {
                path.AddLines(ptsCono);
                path.CloseFigure();
                using (var br = new SolidBrush(CA(cVolcan, 230)))
                    g.FillPath(br, path);
            }

            // Lava: ríos naranjas en el cono
            Color cLava1 = pal[0]; Color cLava2 = pal.Count > 1 ? pal[1] : Color.FromArgb(255, 120, 0);
            for (int ri = 0; ri < 5; ri++)
            {
                float rxOff = (float)((rndV.NextDouble() - 0.5) * volcW * 0.5);
                int nLP = 15;
                var ptsLava = new PointF[nLP];
                for (int pi = 0; pi < nLP; pi++)
                {
                    double t = (double)pi / (nLP - 1);
                    float lx = volcX + rxOff + (float)(Math.Sin(t * Math.PI * 4 + ri) * 15);
                    float ly = hz + (float)(t * (h - hz) * 0.75);
                    ptsLava[pi] = new PointF(lx, ly);
                }
                for (int thick = 2; thick >= 0; thick--)
                {
                    Color cl = MixColor(cLava2, cLava1, (float)thick / 2);
                    using (var pen = new Pen(CA(cl, 120 + thick * 50), thick * 4 + 3))
                        g.DrawCurve(pen, ptsLava, 0.4f);
                }
            }

            // Lava en el suelo: charcos
            for (int i = 0; i < 8; i++)
            {
                float lx = (float)(rndV.NextDouble() * w);
                float ly = hz + (float)((0.5 + rndV.NextDouble() * 0.5) * (h - hz));
                Mancha(g, lx, ly, (float)(w * 0.03 + rndV.NextDouble() * w * 0.07),
                       (float)((h - hz) * 0.02 + rndV.NextDouble() * (h - hz) * 0.05),
                       cLava2, 140 + rndV.Next(80), 0.4f);
            }

            // Ceniza: suelo oscuro
            using (var br = new SolidBrush(CA(Color.FromArgb(25, 20, 15), 180)))
                g.FillRectangle(br, 0, hz, w, h - hz);
        }

        private void PintarPlaya(Graphics g, int w, int h, List<Color> pal)
        {
            int hz = (int)(h * 0.40);
            int hzAgua = (int)(hz + (h - hz) * 0.35f); // donde empieza la arena

            // Cielo tropical
            Color cCielo1 = Color.FromArgb(60, 170, 240), cCielo2 = Color.FromArgb(140, 210, 255);
            PintarCieloAcuarela(g, w, hz, cCielo1, cCielo2);
            PintarSolAcuarela(g, w, hz, Color.FromArgb(255, 230, 100));

            // Agua: azul/turquesa con capas
            Color cAgua = pal.Count > 0 ? pal[0] : Color.FromArgb(0, 160, 200);
            using (var br = new System.Drawing.Drawing2D.LinearGradientBrush(
                new PointF(0, hz), new PointF(0, hzAgua),
                CA(cAgua, 200), CA(Color.FromArgb(80, 200, 210), 180)))
                g.FillRectangle(br, 0, hz, w, hzAgua - hz);

            // Olas: líneas blancas
            var rndP = new Random(_ctx.Semilla);
            for (int i = 0; i < 8; i++)
            {
                float yOla = hz + i * (float)(hzAgua - hz) / 8f;
                float x0 = (float)(rndP.NextDouble() * w * 0.2);
                float x1 = w - (float)(rndP.NextDouble() * w * 0.2);
                Pincelada(g, x0, yOla, x1, yOla, 4 + i, Color.FromArgb(230, 245, 252), 50 + i * 6);
            }

            // Arena: degradado cálido
            Color cArena = pal.Count > 1 ? pal[1] : Color.FromArgb(220, 200, 140);
            using (var br = new System.Drawing.Drawing2D.LinearGradientBrush(
                new PointF(0, hzAgua), new PointF(0, h),
                CA(cArena, 200), CA(MixColor(cArena, Color.FromArgb(180, 155, 90), 0.4f), 230)))
                g.FillRectangle(br, 0, hzAgua, w, h - hzAgua);

            // Textura de arena: manchas de sombra
            for (int i = 0; i < 30; i++)
            {
                float ax = (float)(rndP.NextDouble() * w);
                float ay = hzAgua + (float)(rndP.NextDouble() * (h - hzAgua));
                Mancha(g, ax, ay, 20 + (float)(rndP.NextDouble() * 40), 5 + (float)(rndP.NextDouble() * 10),
                       MixColor(cArena, Color.FromArgb(160, 130, 60), 0.3f), 30 + rndP.Next(30), 0.4f);
            }

            // Palmeras
            PintarPalmerasAcuarela(g, w, h, hzAgua, pal);
        }

        private void PintarPalmerasAcuarela(Graphics g, int w, int h, int hz, List<Color> pal)
        {
            var rndP = new Random(_ctx.Semilla ^ 0x2222);
            int nPalms = 2 + rndP.Next(4);
            Color cTronco = Color.FromArgb(120, 85, 40);
            Color cHoja = Color.FromArgb(40, 130, 50);

            for (int i = 0; i < nPalms; i++)
            {
                float px = (float)(w * (0.05 + rndP.NextDouble() * 0.9));
                float pyBase = hz + (float)(rndP.NextDouble() * (h - hz) * 0.15);
                float alt = (float)(h * 0.18 + rndP.NextDouble() * h * 0.12);
                float incl = (float)((rndP.NextDouble() - 0.5) * 0.4); // inclinación

                // Tronco curvado
                int nTronco = 8;
                var ptsTronco = new PointF[nTronco];
                for (int ti = 0; ti < nTronco; ti++)
                {
                    float t = (float)ti / (nTronco - 1);
                    float curva = (float)(Math.Sin(t * Math.PI * 0.5) * alt * incl);
                    ptsTronco[ti] = new PointF(px + curva, pyBase - t * alt);
                }
                using (var pen = new Pen(CA(cTronco, 180), 8 - i))
                    g.DrawCurve(pen, ptsTronco, 0.3f);

                // Hojas desde la cima
                float topX = ptsTronco[nTronco - 1].X;
                float topY = ptsTronco[nTronco - 1].Y;
                int nHojas = 6 + rndP.Next(4);
                for (int hi = 0; hi < nHojas; hi++)
                {
                    double ang = hi * Math.PI * 2 / nHojas + rndP.NextDouble() * 0.5;
                    float hLen = (float)(alt * 0.45 + rndP.NextDouble() * alt * 0.2);
                    float hEndX = topX + (float)(Math.Cos(ang) * hLen);
                    float hEndY = topY + (float)(Math.Sin(ang) * hLen * 0.6) - hLen * 0.1f;
                    using (var pen = new Pen(CA(cHoja, 150 + rndP.Next(60)), 4))
                    {
                        pen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
                        pen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
                        using (var path = new System.Drawing.Drawing2D.GraphicsPath())
                        {
                            float mX = topX + (float)(Math.Cos(ang) * hLen * 0.5);
                            float mY = topY + (float)(Math.Sin(ang) * hLen * 0.3) - hLen * 0.15f;
                            path.AddBezier(topX, topY, mX, mY - 15, hEndX - 10, hEndY - 10, hEndX, hEndY);
                            g.DrawPath(pen, path);
                        }
                    }
                }
            }
        }

        private Color MixColor(Color a, Color b, float t)
        {
            t=CF(t);
            return Color.FromArgb(
                Clamp((int)(a.R*(1-t)+b.R*t)),
                Clamp((int)(a.G*(1-t)+b.G*t)),
                Clamp((int)(a.B*(1-t)+b.B*t)));
        }
    }



    // ═══════════════════════════════════════════════════

    public enum TipoEntorno3D
    {
        Esferas,
        Terreno,
        Planeta,
        Cueva,               // cueva con estalactitas/estalagmitas
        Ciudad,
        SuperficiePlanetaria,
        Nebulosa3D,
        Canon,
        Tormenta,
        Oceano3D,
    }

    // ═══════════════════════════════════════════════════

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

    // ═══════════════════════════════════════════════════

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

    // ═══════════════════════════════════════════════════

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

    // ═══════════════════════════════════════════════════

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

    // ═══════════════════════════════════════════════════

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

    public class GeneradorEscena3D
    {
        private List<ObjetoRT> _objetos = new List<ObjetoRT>();
        private List<LuzRT>    _luces   = new List<LuzRT>();
        private Vec3 _camPos, _camLookAt;
        private Vec3 _cielo1, _cielo2;
        private ContextoVisual _ctx;
        private Random _rnd;

        public GeneradorEscena3D(ContextoVisual ctx)
        {
            _ctx = ctx;
            _rnd = new Random(ctx.Semilla);
            ConstruirEscena();
        }

        private Vec3 ColorToVec(Color c, double brillo)
        {
            return new Vec3(c.R/255.0*brillo, c.G/255.0*brillo, c.B/255.0*brillo);
        }

        private void ConstruirEscena()
        {
            switch (_ctx.Entorno3D)
            {
                case TipoEntorno3D.Terreno:           ConstruirTerreno();           return;
                case TipoEntorno3D.Planeta:           ConstruirPlaneta();           return;
                case TipoEntorno3D.Cueva:             ConstruirCueva();             return;
                case TipoEntorno3D.Ciudad:            ConstruirCiudad();            return;
                case TipoEntorno3D.SuperficiePlanetaria: ConstruirSuperficiePlanetaria(); return;
                case TipoEntorno3D.Nebulosa3D:        ConstruirNebulosa3D();        return;
                case TipoEntorno3D.Canon:             ConstruirCanon3D();           return;
                case TipoEntorno3D.Tormenta:          ConstruirTormenta3D();        return;
                case TipoEntorno3D.Oceano3D:          ConstruirOceano3D();          return;
                default:                              ConstruirEsferas();           return;
            }
        }

        private void ConstruirEsferas()
        {
            List<Color> paleta = _ctx.Paleta;
            if (paleta.Count == 0)
            {
                paleta = new List<Color>();
                paleta.Add(Color.CornflowerBlue);
                paleta.Add(Color.Coral);
                paleta.Add(Color.Gold);
                paleta.Add(Color.MediumSeaGreen);
                paleta.Add(Color.Orchid);
            }

            if (_ctx.ModoOscuro)
            {
                _cielo1 = new Vec3(0.01, 0.01, 0.05);
                _cielo2 = new Vec3(0.0,  0.0,  0.0);
            }
            else
            {
                _cielo1 = new Vec3(0.5, 0.7, 1.0);
                _cielo2 = new Vec3(0.1, 0.3, 0.6);
            }

            _camPos    = new Vec3(0, 4, -12);
            _camLookAt = new Vec3(0, 0, 0);

            Color colPiso1 = _ctx.ModoOscuro ? Color.FromArgb(20,20,20) : Color.White;
            Color colPiso2 = _ctx.ModoOscuro ? Color.FromArgb(40,40,60) : Color.LightGray;

            var piso = new PlanoRT
            {
                Normal  = new Vec3(0, 1, 0),
                D       = 0,
                Albedo2 = ColorToVec(colPiso2, 0.8)
            };
            piso.Material = new MaterialRT
            {
                Albedo    = ColorToVec(colPiso1, 0.9),
                Especular = 0.1,
                Reflexion = _ctx.ModoOscuro ? 0.3 : 0.15,
                Rugosidad = 0.8
            };
            _objetos.Add(piso);

            int numEsferas = 4 + (int)(_ctx.Complejidad * 2) + (_ctx.ModoCaos ? 3 : 0);
            numEsferas = Math.Min(numEsferas, 10);

            double[] xPos  = { -4, 0, 4, -2, 2, -5, 5, -1, 1, 3 };
            double[] radio = { 1.2, 1.5, 1.2, 0.9, 0.9, 0.7, 0.7, 0.6, 0.8, 1.0 };

            for (int i = 0; i < numEsferas; i++)
            {
                Color colBase = paleta[i % paleta.Count];
                bool esEspejo = (i % 5 == 1);
                bool esMetal  = (i % 4 == 2);

                double yr = radio[i % radio.Length];
                double xr = xPos[i % xPos.Length] + (_rnd.NextDouble()-0.5)*0.5;
                double zr = (_rnd.NextDouble()-0.5)*3;

                var esfera = new EsferaRT
                {
                    Centro = new Vec3(xr, yr, zr),
                    Radio  = yr
                };

                esfera.Material = new MaterialRT
                {
                    Albedo    = ColorToVec(colBase, _ctx.Intensidad),
                    Especular = esEspejo ? 0.95 : (esMetal ? 0.7 : 0.1),
                    Reflexion = esEspejo ? 0.9  : (esMetal ? 0.5 : 0.05),
                    Rugosidad = esEspejo ? 0.02 : (esMetal ? 0.1 : 0.8),
                    EsEspejo  = esEspejo
                };
                _objetos.Add(esfera);

                if (_ctx.ModoCaos && i < 4)
                {
                    for (int k = 0; k < 3; k++)
                    {
                        double ang = k*Math.PI*2/3+i;
                        double ox  = xr + Math.Cos(ang)*1.8;
                        double oz  = zr + Math.Sin(ang)*1.8;
                        Color colSat = paleta[(i+k+1)%paleta.Count];
                        var sat = new EsferaRT
                        {
                            Centro = new Vec3(ox, 0.3, oz),
                            Radio  = 0.3
                        };
                        sat.Material = new MaterialRT
                        {
                            Albedo    = ColorToVec(colSat, _ctx.Intensidad*1.2),
                            Especular = 0.6,
                            Reflexion = 0.3
                        };
                        _objetos.Add(sat);
                    }
                }
            }

            if (_ctx.ModoOscuro)
            {
                for (int i = 0; i < 3; i++)
                {
                    Color colLuz = paleta[i % paleta.Count];
                    double ang   = i * Math.PI * 2 / 3;
                    var eLuz = new EsferaRT
                    {
                        Centro = new Vec3(Math.Cos(ang)*5, 3, Math.Sin(ang)*5),
                        Radio  = 0.5
                    };
                    eLuz.Material = new MaterialRT
                    {
                        EsLuz        = true,
                        EmisionColor = ColorToVec(colLuz, 4.0),
                        Albedo       = ColorToVec(colLuz, 1.0)
                    };
                    _objetos.Add(eLuz);
                }
            }

            Vec3 colLuzPrinc = _ctx.ModoOscuro
                ? new Vec3(0.3, 0.4, 0.6)
                : new Vec3(1.0, 0.95, 0.9);

            _luces.Add(new LuzRT { Pos=new Vec3(5,10,-5), Color=colLuzPrinc, Intensidad=1.0 });
            _luces.Add(new LuzRT { Pos=new Vec3(-8,6,3),  Color=new Vec3(0.5,0.5,1.0), Intensidad=0.4 });
        }

        private void ConstruirTerreno()
        {
            List<Color> pal = _ctx.Paleta.Count>0 ? _ctx.Paleta
                : new List<Color>{ Color.ForestGreen, Color.RosyBrown, Color.Snow, Color.SteelBlue };

            bool esDia = !_ctx.ModoOscuro;
            _cielo1 = esDia ? new Vec3(0.45,0.62,0.85) : new Vec3(0.02,0.05,0.12);
            _cielo2 = esDia ? new Vec3(0.15,0.30,0.55) : new Vec3(0.0, 0.0, 0.02);

            _camPos    = new Vec3(0, 3.5, -12);
            _camLookAt = new Vec3(0, 1.8, 10);

            Matematica.InicializarSemilla(_ctx.Semilla);
            var terreno = new TerrenoRM
            {
                Escala    = 0.14 + _ctx.Complejidad * 0.06,
                Amplitud  = 3.5  + _ctx.Intensidad  * 2.0,
                YOffset   = 0.0,
                Semilla   = _ctx.Semilla,
                ColorBajo  = ColorToVec(pal.Count>0 ? pal[0] : Color.ForestGreen, 0.85),
                ColorMedio = ColorToVec(pal.Count>1 ? pal[1] : Color.RosyBrown,   0.80),
                ColorAlto  = ColorToVec(pal.Count>2 ? pal[2] : Color.Snow,        0.95),
            };
            terreno.Material = new MaterialRT { Especular=0.04, Rugosidad=0.96 };
            _objetos.Add(terreno);

            var agua = new PlanoRT { Normal=new Vec3(0,1,0), D=-0.15, Albedo2=new Vec3(0.08,0.22,0.55) };
            agua.Material = new MaterialRT {
                Albedo    = new Vec3(0.08,0.22,0.55),
                Especular = 0.75, Reflexion = 0.55, Rugosidad = 0.04
            };
            _objetos.Add(agua);

            if (!esDia)
            {
                var rndS = new Random(_ctx.Semilla ^ 0xABCD);
                for (int i=0; i<15; i++)
                {
                    double ang = rndS.NextDouble()*Math.PI*2;
                    double pit = rndS.NextDouble()*Math.PI*0.4;
                    double rs  = 60 + rndS.NextDouble()*30;
                    var st = new EsferaRT {
                        Centro = new Vec3(Math.Cos(pit)*Math.Cos(ang)*rs, Math.Sin(pit)*rs+5, Math.Cos(pit)*Math.Sin(ang)*rs),
                        Radio  = 0.2+rndS.NextDouble()*0.4
                    };
                    double br = 0.6 + rndS.NextDouble()*1.4;
                    st.Material = new MaterialRT { EsLuz=true, EmisionColor=new Vec3(br,br*0.95,br*0.9) };
                    _objetos.Add(st);
                }
            }

            Vec3 luzCol = esDia ? new Vec3(1.0,0.95,0.85) : new Vec3(0.2,0.25,0.45);
            _luces.Add(new LuzRT { Pos=new Vec3(8,20,-8),   Color=luzCol,                Intensidad=esDia?1.2:0.4 });
            _luces.Add(new LuzRT { Pos=new Vec3(-10,8,5),   Color=new Vec3(0.4,0.5,0.8), Intensidad=0.3 });
        }

        private void ConstruirPlaneta()
        {
            List<Color> pal = _ctx.Paleta.Count>0 ? _ctx.Paleta
                : new List<Color>{ Color.FromArgb(30,100,200), Color.FromArgb(60,160,60), Color.FromArgb(220,200,140), Color.FloralWhite };

            _cielo1 = new Vec3(0.0, 0.0, 0.0);
            _cielo2 = new Vec3(0.0, 0.0, 0.0);
            _camPos    = new Vec3(0, 2, -22);
            _camLookAt = new Vec3(0, 0, 0);

            var planeta = new EsferaPlanetaRT {
                Centro = new Vec3(0, 0, 0),
                Radio  = 6.0,
                ColAgua   = ColorToVec(pal[0], 1.0),                          // océanos
                ColTierra = ColorToVec(pal.Count>1?pal[1]:Color.ForestGreen, 1.0), // continentes
                ColArena  = ColorToVec(pal.Count>2?pal[2]:Color.SandyBrown, 0.9),  // desiertos/costas
                ColNieve  = new Vec3(0.95,0.97,1.0),                           // polos
                Semilla   = _ctx.Semilla
            };
            planeta.Material = new MaterialRT { Especular=0.12, Rugosidad=0.8 };
            _objetos.Add(planeta);

            var atmos = new EsferaRT { Centro=new Vec3(0,0,0), Radio=6.5 };
            Color colAtm = pal.Count>3 ? pal[3] : Color.CornflowerBlue;
            atmos.Material = new MaterialRT {
                EsLuz=true, EmisionColor=ColorToVec(colAtm, 0.08),
                Albedo=Vec3.Zero
            };
            _objetos.Add(atmos);

            var luna = new EsferaLunaRT { Centro=new Vec3(10,3,10), Radio=1.5, Semilla=_ctx.Semilla^0xFF };
            luna.Material = new MaterialRT { Especular=0.04, Rugosidad=0.96 };
            _objetos.Add(luna);

            if (_ctx.ModoCaos || _ctx.ModoOscuro)
            {
                int nSeg = 60;
                for (int i=0;i<nSeg;i++)
                {
                    double ang = i*Math.PI*2/nSeg;
                    double r2  = 8.5 + Math.Sin(i*7.3)*0.4;
                    double x   = Math.Cos(ang)*r2;
                    double z   = Math.Sin(ang)*r2;
                    var seg = new CajaRT {
                        Min=new Vec3(x-0.25, -0.06, z-0.25),
                        Max=new Vec3(x+0.25,  0.06, z+0.25)
                    };
                    double br2 = 0.55+0.15*Matematica.Perlin(i*0.2+_ctx.Semilla*0.01,i*0.15);
                    seg.Material = new MaterialRT { Albedo=new Vec3(br2,br2*0.9,br2*0.7), Especular=0.15, Reflexion=0.1 };
                    _objetos.Add(seg);
                }
            }

            var rndS = new Random(_ctx.Semilla^0x1234);
            for (int i=0;i<80;i++) {
                double t=rndS.NextDouble()*Math.PI*2, p=rndS.NextDouble()*Math.PI-Math.PI*0.5;
                double rs=60+rndS.NextDouble()*30;
                var star = new EsferaRT {
                    Centro=new Vec3(Math.Cos(p)*Math.Cos(t)*rs, Math.Sin(p)*rs, Math.Cos(p)*Math.Sin(t)*rs),
                    Radio=0.15+rndS.NextDouble()*0.35
                };
                double br=0.7+rndS.NextDouble()*1.5;
                double hue=rndS.NextDouble();
                Vec3 starCol = hue<0.25 ? new Vec3(br*0.8,br*0.9,br) :
                               hue<0.5  ? new Vec3(br,br,br) :
                               hue<0.75 ? new Vec3(br,br*0.95,br*0.7) :
                                          new Vec3(br,br*0.6,br*0.5);
                star.Material = new MaterialRT { EsLuz=true, EmisionColor=starCol };
                _objetos.Add(star);
            }

            _luces.Add(new LuzRT{ Pos=new Vec3(30,15,-20), Color=new Vec3(1.0,0.97,0.90), Intensidad=1.5 });
            _luces.Add(new LuzRT{ Pos=new Vec3(-25,5,10),  Color=new Vec3(0.08,0.10,0.30), Intensidad=0.15 });
        }

        private void ConstruirCueva()
        {
            List<Color> pal = _ctx.Paleta.Count>0 ? _ctx.Paleta
                : new List<Color>{ Color.FromArgb(80,60,40), Color.FromArgb(60,40,25), Color.FromArgb(40,80,120) };

            _cielo1 = new Vec3(0.0, 0.0, 0.0);
            _cielo2 = new Vec3(0.0, 0.0, 0.0);

            _camPos    = new Vec3(0, 2.5, -2);
            _camLookAt = new Vec3(0, 2.5, 10);

            Matematica.InicializarSemilla(_ctx.Semilla);
            var cueva = new CuevaRM
            {
                RadioBase  = 3.5 + _ctx.Complejidad * 1.5,
                Semilla    = _ctx.Semilla,
                ColorPared = ColorToVec(pal[0],               0.75),
                ColorTecho = ColorToVec(pal.Count>1?pal[1]:pal[0], 0.65),
                ColorSuelo = ColorToVec(pal.Count>2?pal[2]:pal[0], 0.70),
            };
            cueva.Material = new MaterialRT { Especular=0.06, Rugosidad=0.94 };
            _objetos.Add(cueva);

            var suelo = new PlanoRT { Normal=new Vec3(0,1,0), D=0, Albedo2=ColorToVec(pal[0],0.5) };
            suelo.Material = new MaterialRT {
                Albedo    = ColorToVec(pal[0], 0.6),
                Reflexion = 0.18, Especular = 0.35, Rugosidad = 0.4
            };
            _objetos.Add(suelo);

            var charco = new PlanoRT { Normal=new Vec3(0,1,0), D=-0.05, Albedo2=new Vec3(0.05,0.1,0.25) };
            charco.Material = new MaterialRT {
                Albedo=new Vec3(0.05,0.1,0.28), Reflexion=0.55, Especular=0.65, Rugosidad=0.03
            };
            _objetos.Add(charco);

            Color[] colLuces = { Color.OrangeRed, Color.Cyan, Color.DodgerBlue, Color.LimeGreen };
            for (int i=0; i<4; i++)
            {
                double ang = i*Math.PI*2/4;
                double zL  = 3 + i*4.0;
                double xL  = Math.Sin(ang + i*0.7) * 2.5;
                _luces.Add(new LuzRT {
                    Pos=new Vec3(xL, 1.5, zL),
                    Color=ColorToVec(colLuces[i], 1.0),
                    Intensidad = 1.0 + _ctx.Intensidad * 0.5
                });
                var eLuz = new EsferaRT { Centro=new Vec3(xL, 1.5, zL), Radio=0.18 };
                eLuz.Material = new MaterialRT {
                    EsLuz=true,
                    EmisionColor=ColorToVec(colLuces[i], 3.5)
                };
                _objetos.Add(eLuz);
            }

            _luces.Add(new LuzRT { Pos=new Vec3(0,5,5), Color=new Vec3(0.15,0.12,0.1), Intensidad=0.2 });
        }

        private void ConstruirCiudad()
        {
            List<Color> pal = _ctx.Paleta.Count>0 ? _ctx.Paleta
                : new List<Color>{ Color.FromArgb(30,40,60), Color.FromArgb(0,200,255), Color.FromArgb(255,100,0), Color.FromArgb(80,80,120) };
            _cielo1 = _ctx.ModoOscuro ? new Vec3(0.01,0.02,0.05) : new Vec3(0.2,0.4,0.7);
            _cielo2 = _ctx.ModoOscuro ? new Vec3(0,0,0)          : new Vec3(0.05,0.15,0.35);
            _camPos    = new Vec3(0, 5, -18);
            _camLookAt = new Vec3(0, 8, 0);

            var piso = new PlanoRT { Normal=new Vec3(0,1,0), D=0, Albedo2=ColorToVec(pal[0],0.3) };
            piso.Material = new MaterialRT { Albedo=ColorToVec(pal[0],0.4), Reflexion=0.45, Especular=0.75, Rugosidad=0.08 };
            _objetos.Add(piso);

            int[] gridX = {-10,-6,-2,2,6,10,-8,-4,0,4,8};
            int[] gridZ = {0,4,8,12,16};
            foreach (int gx in gridX)
            foreach (int gz in gridZ)
            {
                double h   = 4 + _rnd.NextDouble()*18;
                double hw  = 1.0 + _rnd.NextDouble()*1.2;
                double hd  = 1.0 + _rnd.NextDouble()*1.2;
                Color cEdif= pal[_rnd.Next(pal.Count)];
                bool esVidrio = _rnd.NextDouble() < 0.4;

                var torre = new CajaRT {
                    Min = new Vec3(gx-hw, 0, gz-hd),
                    Max = new Vec3(gx+hw, h, gz+hd)
                };
                torre.Material = new MaterialRT {
                    Albedo    = ColorToVec(cEdif, esVidrio?0.5:0.8),
                    Especular = esVidrio?0.95:0.25,
                    Reflexion = esVidrio?0.75:0.08,
                    Rugosidad = esVidrio?0.02:0.5
                };
                _objetos.Add(torre);

                if (h > 8)
                {
                    double hTop=h*0.25, wTop=hw*0.55;
                    var remate = new CajaRT {
                        Min=new Vec3(gx-wTop, h, gz-wTop),
                        Max=new Vec3(gx+wTop, h+hTop, gz+wTop)
                    };
                    remate.Material = new MaterialRT {
                        Albedo=ColorToVec(pal[(_rnd.Next(pal.Count)+1)%pal.Count],0.7),
                        Especular=0.6, Reflexion=0.3, Rugosidad=0.1
                    };
                    _objetos.Add(remate);
                }

                if (_rnd.NextDouble() < 0.35 && h > 10)
                {
                    var antena = new CilindroRT {
                        Base   = new Vec3(gx, h + (h>8?h*0.25:0), gz),
                        Radio  = 0.06,
                        Altura = 2.0 + _rnd.NextDouble()*3.0
                    };
                    antena.Material = new MaterialRT { Albedo=new Vec3(0.6,0.6,0.7), Especular=0.7, Reflexion=0.3 };
                    _objetos.Add(antena);
                }

                if (_ctx.ModoOscuro)
                {
                    double yTop2 = h + (h>8?h*0.25:0);
                    var lTop = new EsferaRT { Centro=new Vec3(gx, yTop2+0.3, gz), Radio=0.18 };
                    Color cLuz = _rnd.NextDouble()<0.5 ? Color.Red : Color.White;
                    lTop.Material = new MaterialRT { EsLuz=true, EmisionColor=ColorToVec(cLuz,2.5) };
                    _objetos.Add(lTop);
                }
            }

            if (_ctx.ModoOscuro)
            {
                for (int i=0;i<8;i++)
                {
                    double lx=(_rnd.NextDouble()-0.5)*22;
                    double lz=_rnd.NextDouble()*16+2;
                    _luces.Add(new LuzRT{Pos=new Vec3(lx,6,lz),Color=new Vec3(1.0,0.85,0.55),Intensidad=0.6});
                }
            }

            Vec3 luzPrinc = _ctx.ModoOscuro ? new Vec3(0.15,0.2,0.4) : new Vec3(0.9,0.9,1.0);
            _luces.Add(new LuzRT{ Pos=new Vec3(0,40,-15), Color=luzPrinc, Intensidad=_ctx.ModoOscuro?0.3:1.0 });
            _luces.Add(new LuzRT{ Pos=new Vec3(-20,15,5), Color=new Vec3(0.3,0.5,1.0), Intensidad=0.4 });
        }

        private void ConstruirSuperficiePlanetaria()
        {
            List<Color> pal = _ctx.Paleta.Count>0 ? _ctx.Paleta
                : new List<Color>{ Color.FromArgb(180,90,50), Color.FromArgb(140,70,35), Color.FromArgb(210,160,100) };

            bool esDia = !_ctx.ModoOscuro;
            _cielo1 = esDia ? new Vec3(0.55,0.28,0.12) : new Vec3(0.01,0.01,0.03);
            _cielo2 = esDia ? new Vec3(0.70,0.40,0.20) : new Vec3(0.0, 0.0,  0.0);

            _camPos    = new Vec3(0, 3.0, -12);
            _camLookAt = new Vec3(0, 1.5, 10);

            Matematica.InicializarSemilla(_ctx.Semilla);
            var terreno = new TerrenoRM
            {
                Escala   = 0.12 + _ctx.Complejidad * 0.05,
                Amplitud = 4.0  + _ctx.Intensidad  * 2.5,
                YOffset  = 0.0,
                Semilla  = _ctx.Semilla + 7,
                ColorBajo  = ColorToVec(pal[0],                   0.80),  // llanuras
                ColorMedio = ColorToVec(pal.Count>1?pal[1]:pal[0], 0.75), // laderas oscuras
                ColorAlto  = ColorToVec(pal.Count>2?pal[2]:pal[0], 0.90), // polvo cima
            };
            terreno.Material = new MaterialRT { Especular=0.03, Rugosidad=0.98 };
            _objetos.Add(terreno);

            var planeta2 = new EsferaRT { Centro=new Vec3(25,8,40), Radio=7 };
            Color cP2 = pal.Count>2 ? pal[2] : Color.CornflowerBlue;
            planeta2.Material = new MaterialRT { Albedo=ColorToVec(cP2,0.9), Especular=0.15, Reflexion=0.05 };
            _objetos.Add(planeta2);

            if (!esDia)
            {
                var rndS = new Random(_ctx.Semilla ^ 0x5E7A);
                for (int i=0; i<20; i++)
                {
                    double t2 = rndS.NextDouble()*Math.PI*2;
                    double p  = rndS.NextDouble()*Math.PI*0.45;
                    double rs = 55+rndS.NextDouble()*30;
                    var star  = new EsferaRT {
                        Centro=new Vec3(Math.Cos(p)*Math.Cos(t2)*rs, Math.Sin(p)*rs+3, Math.Cos(p)*Math.Sin(t2)*rs),
                        Radio=0.2+rndS.NextDouble()*0.45
                    };
                    double br=0.5+rndS.NextDouble()*1.5;
                    star.Material=new MaterialRT{EsLuz=true,EmisionColor=new Vec3(br,br*0.9,br*0.85)};
                    _objetos.Add(star);
                }
                var luna = new EsferaRT { Centro=new Vec3(-10,8,30), Radio=1.2 };
                luna.Material=new MaterialRT{Albedo=new Vec3(0.5,0.45,0.4),Especular=0.04};
                _objetos.Add(luna);
            }

            Vec3 luzS = esDia ? new Vec3(1.0,0.85,0.70) : new Vec3(0.04,0.04,0.08);
            _luces.Add(new LuzRT{Pos=new Vec3(20,40,-15), Color=luzS,                       Intensidad=esDia?1.2:0.3});
            _luces.Add(new LuzRT{Pos=new Vec3(-20,12,5),  Color=new Vec3(0.5,0.35,0.25),    Intensidad=0.20});
        }

        private void ConstruirNebulosa3D()
        {
            _cielo1 = new Vec3(0,0,0);
            _cielo2 = new Vec3(0,0,0);
            _camPos    = new Vec3(0, 0, -18);
            _camLookAt = new Vec3(0, 0, 0);

            List<Color> pal = _ctx.Paleta.Count>0 ? _ctx.Paleta
                : new List<Color>{ Color.Magenta, Color.Cyan, Color.DeepSkyBlue, Color.MediumPurple };

            for (int i=0;i<20;i++) {
                double t2=_rnd.NextDouble()*Math.PI*2, p=_rnd.NextDouble()*Math.PI;
                double rs=3+_rnd.NextDouble()*8;
                Color cN=pal[_rnd.Next(pal.Count)];
                var nube = new EsferaRT{Centro=new Vec3(Math.Cos(t2)*rs,Math.Sin(p)*rs*0.6,Math.Sin(t2)*rs*0.8),Radio=2+_rnd.NextDouble()*4};
                nube.Material=new MaterialRT{EsLuz=true,EmisionColor=ColorToVec(cN,0.15+_rnd.NextDouble()*0.25)};
                _objetos.Add(nube);
            }
            for (int i=0;i<5;i++) {
                double t2=_rnd.NextDouble()*Math.PI*2, p=_rnd.NextDouble()*2-1;
                var str = new EsferaRT{Centro=new Vec3(Math.Cos(t2)*p*3,p*2,Math.Sin(t2)*p*3),Radio=0.15+_rnd.NextDouble()*0.4};
                double br=3+_rnd.NextDouble()*5;
                Color cSt=pal[_rnd.Next(pal.Count)];
                str.Material=new MaterialRT{EsLuz=true,EmisionColor=ColorToVec(cSt,br)};
                _objetos.Add(str);
            }
            for (int i=0;i<30;i++) {
                double t2=_rnd.NextDouble()*Math.PI*2, p=_rnd.NextDouble()*Math.PI-Math.PI/2;
                double r2=30+_rnd.NextDouble()*20;
                var fStar=new EsferaRT{Centro=new Vec3(Math.Cos(p)*Math.Cos(t2)*r2,Math.Sin(p)*r2,Math.Cos(p)*Math.Sin(t2)*r2),Radio=0.1+_rnd.NextDouble()*0.2};
                double br2=0.5+_rnd.NextDouble()*1.5;
                fStar.Material=new MaterialRT{EsLuz=true,EmisionColor=new Vec3(br2,br2*0.95,br2*0.9)};
                _objetos.Add(fStar);
            }
            _luces.Add(new LuzRT{Pos=new Vec3(0,0,-5),Color=new Vec3(1,1,1),Intensidad=0.5});
        }

        private void ConstruirCanon3D()
        {
            List<Color> pal = _ctx.Paleta.Count>0 ? _ctx.Paleta
                : new List<Color>{ Color.FromArgb(180,80,40), Color.FromArgb(220,140,60), Color.FromArgb(100,60,30) };

            _cielo1 = _ctx.ModoOscuro ? new Vec3(0.01,0.02,0.06) : new Vec3(0.4,0.6,0.9);
            _cielo2 = _ctx.ModoOscuro ? new Vec3(0,0,0)          : new Vec3(0.7,0.85,1.0);
            _camPos    = new Vec3(0, 2, -8);
            _camLookAt = new Vec3(0, 3, 8);

            var suelo=new PlanoRT{Normal=new Vec3(0,1,0),D=0,Albedo2=ColorToVec(pal[pal.Count-1],0.6)};
            suelo.Material=new MaterialRT{Albedo=ColorToVec(pal[0],0.8),Especular=0.05,Rugosidad=0.98};
            _objetos.Add(suelo);

            var rio=new PlanoRT{Normal=new Vec3(0,1,0),D=-0.1,Albedo2=new Vec3(0.1,0.2,0.5)};
            rio.Material=new MaterialRT{Albedo=new Vec3(0.1,0.25,0.6),Especular=0.65,Reflexion=0.45,Rugosidad=0.05};
            _objetos.Add(rio);

            Matematica.InicializarSemilla(_ctx.Semilla);
            for (int lado = 0; lado < 2; lado++)
            {
                double xBase = lado==0 ? -5.5 : 4.5;
                double xDir  = lado==0 ? -1.0 :  1.0;

                double[] estratoH   = { 0.0, 1.2, 2.6, 4.2, 6.0, 8.0, 10.2 };
                double[] estratoAncho = { 2.5, 2.1, 2.8, 2.0, 2.6, 2.3, 1.8 };

                for (int seg=0; seg<6; seg++)
                {
                    double zStart = seg*3.0 - 2.0;
                    for (int est=0; est<6; est++)
                    {
                        double yBot = estratoH[est];
                        double yTop = estratoH[est+1];
                        double front = Matematica.Perlin(seg*0.4+est*0.7+_ctx.Semilla*0.003, est*0.5+seg*0.3)*0.8;
                        double anchoEst = estratoAncho[est] + front*0.6;
                        double xFront = xBase + xDir*(anchoEst);

                        int palIdx = est % pal.Count;
                        double nCol = Matematica.Perlin(est*0.3+seg*0.2, _ctx.Semilla*0.001);
                        Color cEst  = pal[palIdx];

                        var bloque = new CajaRT {
                            Min = new Vec3(Math.Min(xBase,xFront), yBot, zStart),
                            Max = new Vec3(Math.Max(xBase,xFront), yTop, zStart+3.2)
                        };
                        bloque.Material = new MaterialRT {
                            Albedo    = ColorToVec(cEst, 0.7+nCol*0.2),
                            Especular = 0.04,
                            Rugosidad = 0.97
                        };
                        _objetos.Add(bloque);
                    }
                }
            }

            for (int i=0;i<8;i++)
            {
                double rx=(_rnd.NextDouble()-0.5)*6, rz=_rnd.NextDouble()*12+1;
                double rh=0.3+_rnd.NextDouble()*0.8;
                var roca = new CajaRT {
                    Min=new Vec3(rx-rh*0.8, 0, rz-rh*0.6),
                    Max=new Vec3(rx+rh*0.8, rh, rz+rh*0.6)
                };
                roca.Material=new MaterialRT{Albedo=ColorToVec(pal[0],0.65),Especular=0.03,Rugosidad=0.99};
                _objetos.Add(roca);
            }

            Vec3 luzCol = _ctx.ModoOscuro ? new Vec3(0.1,0.15,0.3) : new Vec3(1.0,0.9,0.7);
            _luces.Add(new LuzRT{Pos=new Vec3(8,25,0),Color=luzCol,Intensidad=1.3});
            _luces.Add(new LuzRT{Pos=new Vec3(-5,8,10),Color=new Vec3(0.4,0.55,0.8),Intensidad=0.3});
        }

        private void ConstruirTormenta3D()
        {
            _cielo1 = new Vec3(0.08,0.08,0.12);
            _cielo2 = new Vec3(0.02,0.02,0.04);
            _camPos    = new Vec3(0, 5, -15);
            _camLookAt = new Vec3(0, 8, 0);

            List<Color> pal = _ctx.Paleta.Count>0 ? _ctx.Paleta
                : new List<Color>{ Color.FromArgb(40,45,55), Color.FromArgb(20,30,80), Color.FromArgb(200,200,255) };

            for (int i=0;i<25;i++) {
                double t2=_rnd.NextDouble()*Math.PI*2;
                double rx=(_rnd.NextDouble()-0.5)*20, ry=5+_rnd.NextDouble()*6, rz=_rnd.NextDouble()*15;
                double rad=2+_rnd.NextDouble()*5;
                Color cN=pal[_rnd.Next(pal.Count)];
                var nub=new EsferaRT{Centro=new Vec3(rx,ry,rz),Radio=rad};
                nub.Material=new MaterialRT{Albedo=ColorToVec(cN,0.6),Especular=0.02,Rugosidad=1.0};
                _objetos.Add(nub);
            }
            var rayo=new EsferaRT{Centro=new Vec3((_rnd.NextDouble()-0.5)*6,3,4),Radio=0.15};
            rayo.Material=new MaterialRT{EsLuz=true,EmisionColor=new Vec3(5,5,8)};
            _objetos.Add(rayo);
            var suelo=new PlanoRT{Normal=new Vec3(0,1,0),D=0,Albedo2=new Vec3(0.05,0.06,0.1)};
            suelo.Material=new MaterialRT{Albedo=new Vec3(0.08,0.09,0.12),Reflexion=0.6,Especular=0.8,Rugosidad=0.05};
            _objetos.Add(suelo);

            _luces.Add(new LuzRT{Pos=new Vec3(0,20,0),Color=new Vec3(0.5,0.55,0.8),Intensidad=0.8});
            _luces.Add(new LuzRT{Pos=new Vec3(3,3,4), Color=new Vec3(0.7,0.7,1.0),Intensidad=2.0});  // rayo
        }

        private void ConstruirOceano3D()
        {
            bool esDia = !_ctx.ModoOscuro;
            _cielo1 = esDia ? new Vec3(0.3,0.55,0.9) : new Vec3(0.02,0.04,0.12);
            _cielo2 = esDia ? new Vec3(0.7,0.85,1.0) : new Vec3(0.01,0.02,0.06);
            _camPos    = new Vec3(0, 3, -12);
            _camLookAt = new Vec3(0, 1.5, 5);

            List<Color> pal = _ctx.Paleta.Count>0 ? _ctx.Paleta
                : new List<Color>{ Color.FromArgb(10,80,180), Color.FromArgb(0,160,200), Color.FromArgb(255,255,255) };

            var oceano=new PlanoRT{Normal=new Vec3(0,1,0),D=0,Albedo2=ColorToVec(pal.Count>1?pal[1]:Color.Cyan,0.6)};
            oceano.Material=new MaterialRT{Albedo=ColorToVec(pal[0],0.5),Especular=0.85,Reflexion=0.7,Rugosidad=0.02};
            _objetos.Add(oceano);

            for (int i=0;i<20;i++) {
                double wx=(_rnd.NextDouble()-0.5)*25, wz=_rnd.NextDouble()*20;
                double wr=0.3+_rnd.NextDouble()*0.8;
                var ola=new EsferaRT{Centro=new Vec3(wx,wr*0.5,wz),Radio=wr};
                Color cOla=pal.Count>2?pal[2]:Color.White;
                ola.Material=new MaterialRT{Albedo=ColorToVec(cOla,0.9),Especular=0.6,Reflexion=0.3,Rugosidad=0.1};
                _objetos.Add(ola);
            }
            if (_ctx.ModoSuave) {
                var isla=new EsferaRT{Centro=new Vec3(8,0.5,18),Radio=2};
                isla.Material=new MaterialRT{Albedo=new Vec3(0.3,0.6,0.2),Especular=0.05,Rugosidad=0.95};
                _objetos.Add(isla);
            }
            Color cSol = esDia ? Color.Gold : Color.WhiteSmoke;
            var sol=new EsferaRT{Centro=new Vec3(8,10,25),Radio=esDia?2.0:1.2};
            sol.Material=new MaterialRT{EsLuz=true,EmisionColor=ColorToVec(cSol,esDia?4:2)};
            _objetos.Add(sol);

            _luces.Add(new LuzRT{Pos=new Vec3(8,15,25),Color=esDia?new Vec3(1.0,0.97,0.85):new Vec3(0.4,0.5,0.7),Intensidad=esDia?1.2:0.6});
            _luces.Add(new LuzRT{Pos=new Vec3(-10,8,-5),Color=new Vec3(0.3,0.5,0.8),Intensidad=0.3});
        }

        public Bitmap GenerarProgresivo(int ancho, int alto,
            Action<int> reportarProgreso,
            Action<Bitmap> actualizarCanvas,
            ref bool cancelar)
        {
            Vec3 fwd  = (_camLookAt - _camPos).Norm();
            Vec3 right = Vec3.Cross(fwd, Vec3.Up).Norm();
            Vec3 up    = Vec3.Cross(right, fwd).Norm();
            double fov = 60.0 * Math.PI / 180.0;
            double h   = Math.Tan(fov/2);
            double w   = h * ancho / alto;

            int stride3 = ancho*3;
            byte[] buf  = new byte[alto*stride3];

            Bitmap bmpParcial = new Bitmap(ancho, alto, PixelFormat.Format24bppRgb);

            int spp = _ctx.ModoSuave ? 4 : 2;

            for (int y = 0; y < alto; y++)
            {
                if (cancelar) break;

                for (int x = 0; x < ancho; x++)
                {
                    double tr=0, tg=0, tb=0;

                    for (int s = 0; s < spp; s++)
                    {
                        double jx = (s%2==0)?0.25:0.75;
                        double jy = (s<2)?0.25:0.75;

                        double u = (x+jx) / ancho * 2 - 1;
                        double v = 1 - (y+jy) / alto * 2;
                        Vec3 d   = (fwd + right*(u*w) + up*(v*h)).Norm();
                        var rayo = new Rayo(_camPos, d);

                        Vec3 col = Trazar(rayo, 3);
                        tr += col.X; tg += col.Y; tb += col.Z;
                    }

                    tr/=spp; tg/=spp; tb/=spp;

                    tr = Math.Sqrt(Matematica.Clamp01(tr));
                    tg = Math.Sqrt(Matematica.Clamp01(tg));
                    tb = Math.Sqrt(Matematica.Clamp01(tb));

                    int off    = y*stride3+x*3;
                    buf[off]   = (byte)(tb*255);
                    buf[off+1] = (byte)(tg*255);
                    buf[off+2] = (byte)(tr*255);
                }

                if ((y%4 == 0 || y==alto-1) && actualizarCanvas != null)
                {
                    int lineas = (y+1)*stride3;
                    var bd = bmpParcial.LockBits(
                        new Rectangle(0,0,ancho,alto), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
                    Marshal.Copy(buf, 0, bd.Scan0, lineas);
                    bmpParcial.UnlockBits(bd);
                    actualizarCanvas(new Bitmap(bmpParcial));
                }

                if (reportarProgreso != null)
                    reportarProgreso(y*100/alto);
            }

            Bitmap bmpFinal = new Bitmap(ancho, alto, PixelFormat.Format24bppRgb);
            var bdF = bmpFinal.LockBits(
                new Rectangle(0,0,ancho,alto), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            Marshal.Copy(buf, 0, bdF.Scan0, buf.Length);
            bmpFinal.UnlockBits(bdF);
            return bmpFinal;
        }

        private Vec3 Trazar(Rayo r, int profundidad)
        {
            if (profundidad <= 0) return Vec3.Zero;

            var mejor = new InfoImpacto();
            mejor.T   = double.MaxValue;

            foreach (ObjetoRT obj in _objetos)
            {
                InfoImpacto info = obj.Intersectar(r);
                if (info.Golpeo && info.T < mejor.T)
                    mejor = info;
            }

            if (!mejor.Golpeo) return ColorCielo(r.Dir);

            MaterialRT mat = mejor.Material;

            if (mat.EsLuz) return mat.EmisionColor;

            Vec3 punto  = mejor.Punto;
            Vec3 normal = mejor.Normal;

            Vec3 colorFinal = mat.Albedo * 0.08; // ambiental

            foreach (LuzRT luz in _luces)
            {
                Vec3 L       = (luz.Pos - punto).Norm();
                double lDist = (luz.Pos - punto).Len();

                Rayo rSombra    = new Rayo(punto + normal*0.001, L);
                bool enSombra   = false;
                foreach (ObjetoRT obj in _objetos)
                {
                    InfoImpacto si = obj.Intersectar(rSombra);
                    if (si.Golpeo && si.T < lDist && !si.Material.EsLuz)
                    {
                        enSombra = true;
                        break;
                    }
                }
                if (enSombra) continue;

                double nDotL = Math.Max(0, Vec3.Dot(normal, L));
                double atten = 1.0/(1.0+lDist*lDist*0.02);
                colorFinal = colorFinal + mat.Albedo * luz.Color * (nDotL * atten * luz.Intensidad);

                Vec3 V    = (-r.Dir).Norm();
                Vec3 H    = (L+V).Norm();
                double spec = Math.Pow(Math.Max(0, Vec3.Dot(normal, H)), 64*mat.Especular+1);
                double specF = mat.Especular * atten * luz.Intensidad;
                colorFinal = colorFinal + luz.Color * (spec * specF);
            }

            if (mat.Reflexion > 0.01 && profundidad > 1)
            {
                Vec3 refDir     = Vec3.Reflect(r.Dir, normal);
                if (mat.Rugosidad > 0.05)
                {
                    var rnd = new Random((int)(punto.X*1000+punto.Y*2000+punto.Z*3000));
                    refDir = (refDir + new Vec3(
                        (rnd.NextDouble()-0.5)*mat.Rugosidad,
                        (rnd.NextDouble()-0.5)*mat.Rugosidad,
                        (rnd.NextDouble()-0.5)*mat.Rugosidad)).Norm();
                }
                Rayo rRef  = new Rayo(punto + normal*0.001, refDir);
                Vec3 cRef  = Trazar(rRef, profundidad-1);
                colorFinal = colorFinal*(1-mat.Reflexion) + cRef*mat.Reflexion;
            }

            return colorFinal;
        }

        private Vec3 ColorCielo(Vec3 dir)
        {
            double t = (dir.Y+1)*0.5;
            return _cielo2*(1-t) + _cielo1*t;
        }
    }

    // ═══════════════════════════════════════════════════

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

    // ═══════════════════════════════════════════════════

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

    // ═══════════════════════════════════════════════════

    public class GeneradorDibujo
    {
        private ContextoVisual _ctx;
        private Random         _rnd;

        private static readonly Color PAPEL_LAPIZ    = Color.FromArgb(245, 242, 235);
        private static readonly Color PAPEL_LAPICERA = Color.FromArgb(250, 248, 244);
        private static readonly Color PAPEL_CARBON   = Color.FromArgb(235, 230, 220);

        public GeneradorDibujo(ContextoVisual ctx)
        {
            _ctx = ctx;
            _rnd = new Random(ctx.Semilla);
        }

        public Bitmap Generar(int ancho, int alto, Action<int> progreso)
        {
            Bitmap bmp = new Bitmap(ancho, alto);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode     = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

                bool esLapicera = _ctx.EstiloLapiz == "lapicera";
                bool esCarbon   = _ctx.EstiloLapiz == "carbon";

                Color papel = esLapicera ? Color.FromArgb(252, 252, 250)
                            : esCarbon   ? PAPEL_CARBON
                            : PAPEL_LAPIZ;
                g.Clear(papel);

                if (progreso != null) progreso(5);

                if (esLapicera)
                    DibujarTexturaPapelFino(g, ancho, alto);
                else
                    DibujarTexturaPapel(g, ancho, alto);

                if (progreso != null) progreso(15);
                DibujarEscena(g, ancho, alto);
                if (progreso != null) progreso(85);

                if (esLapicera)
                    DibujarVignetteInk(g, ancho, alto);

                DibujarGrano(g, ancho, alto);
                if (progreso != null) progreso(100);
            }
            return bmp;
        }

        private void DibujarTexturaPapelFino(Graphics g, int w, int h)
        {
            // Papel de dibujo técnico: fibras sutiles y microirregularidades
            int nFibras = w * 3;
            for (int i = 0; i < nFibras; i++)
            {
                int x  = _rnd.Next(w);
                int y  = _rnd.Next(h);
                int len = _rnd.Next(3, 18);
                int alfa = _rnd.Next(3, 12);
                using (Pen p = new Pen(Color.FromArgb(alfa, 140, 130, 120), 0.5f))
                    g.DrawLine(p, x, y, x + len, y + _rnd.Next(-1, 2));
            }
        }

        private void DibujarTexturaPapel(Graphics g, int w, int h)
        {
            int granos = (int)(w * h * 0.004);
            for (int i = 0; i < granos; i++)
            {
                int x   = _rnd.Next(w);
                int y   = _rnd.Next(h);
                int sz  = _rnd.Next(1, 3);
                int alfa = _rnd.Next(8, 28);
                using (SolidBrush br = new SolidBrush(Color.FromArgb(alfa, 160, 150, 130)))
                    g.FillRectangle(br, x, y, sz, sz);
            }
        }

        private void DibujarEscena(Graphics g, int w, int h)
        {
            var palabras = _ctx.PalabrasDetectadas;
            bool tieneHorizonte = true;
            int hz = (int)(h * 0.55);

            bool esMontana   = Contiene(palabras, "montana","montaña","mountain","sierra","cumbre","pico","nevado");
            bool esOceano    = Contiene(palabras, "oceano","ocean","mar","sea","playa","beach","lago","lake");
            bool esBosque    = Contiene(palabras, "bosque","forest","selva","jungle","arbol","tree");
            bool esCiudad    = Contiene(palabras, "ciudad","city","edificio","building","arquitectura");
            bool esDesiero   = Contiene(palabras, "desierto","desert","arena","dunas");
            bool esFlores    = Contiene(palabras, "flores","flowers","campo","meadow","prado");
            bool esNoche     = Contiene(palabras, "noche","night","nocturno") || _ctx.HoraDelDia == "noche";
            bool esPaisaje   = !esMontana && !esOceano && !esBosque && !esCiudad && !esFlores;

            DibujarHorizonte(g, w, h, hz);

            if (esNoche)   DibujarEstrellasDibujo(g, w, hz);
            if (esMontana) DibujarMontanasDibujo(g, w, h, hz);
            if (esOceano)  DibujarAguaDibujo(g, w, h, hz);
            if (esBosque)  DibujarBosqueDibujo(g, w, h, hz);
            if (esCiudad)  DibujarCiudadDibujo(g, w, h, hz);
            if (esDesiero) DibujarDesiertoSketch(g, w, h, hz);
            if (esFlores)  DibujarFloresDibujo(g, w, h, hz);
            if (esPaisaje && !esMontana && !esOceano && !esBosque)
            {
                DibujarMontanasDibujo(g, w, h, hz);
                DibujarBosqueDibujo(g, w, h, hz);
            }

            if (_ctx.ConIslas)   DibujarIslasDibujo(g, w, h, hz);
            if (_ctx.ConRocas)   DibujarRocasDibujo(g, w, h, hz);
            if (_ctx.ConPalmeras) DibujarPalmerasDibujo(g, w, h, hz);
            if (_ctx.ConLluvia)  DibujarLluvia(g, w, h);
            if (_ctx.ConNieve)   DibujarNieveSketch(g, w, h);

            if (!_ctx.SinSol && !esNoche)
                DibujarSolDibujo(g, w, hz);
            else if (esNoche)
                DibujarLunaDibujo(g, w, hz);
        }

        private bool Contiene(List<string> lista, params string[] vals)
        {
            foreach (string v in vals)
                if (lista.Contains(v)) return true;
            return false;
        }

        private void DibujarHorizonte(Graphics g, int w, int h, int hz)
        {
            Color tinta = ColorTinta(0.7f);
            float grosor = _ctx.EstiloLapiz == "lapicera" ? 1.8f : Grosor(1.2f);
            using (Pen p = new Pen(tinta, grosor))
            {
                float ondaH = hz;
                if (_ctx.EstiloLapiz == "lapicera")
                {
                    // Horizonte de tinta: línea continua con presión variable
                    var pts = new List<PointF>();
                    for (int x = 0; x <= w; x += 4)
                    {
                        float dy = (float)(Matematica.Perlin(x * 0.002, 0.5) * 4);
                        pts.Add(new PointF(x, ondaH + dy));
                    }
                    for (int i = 0; i < pts.Count - 1; i++)
                        LineInk(g, p, pts[i], pts[i+1]);
                }
                else
                {
                    for (int x = 0; x < w; x += 3)
                    {
                        float dy = (float)(Matematica.Perlin(x * 0.003, 0.5) * 6);
                        g.DrawLine(p, x, ondaH + dy, Math.Min(w-1, x+3), ondaH + dy);
                    }
                }
            }
        }

        private void DibujarSolDibujo(Graphics g, int w, int hz)
        {
            bool ink = _ctx.EstiloLapiz == "lapicera";
            int sx = (int)(w * (0.65 + _rnd.NextDouble() * 0.2));
            int sy = (int)(hz * (0.12 + _rnd.NextDouble() * 0.18));
            int sr = (int)(w * 0.045);
            Color tinta = ColorTinta(0.6f);

            if (ink)
            {
                // Sol de tinta: círculo limpio con rayos de longitud variable y presión
                using (Pen p = new Pen(tinta, 2.0f))
                    g.DrawEllipse(p, sx-sr, sy-sr, sr*2, sr*2);
                // Segunda línea ligeramente desplazada (efecto doble trazo de tinta)
                using (Pen p2 = new Pen(ColorTinta(0.3f), 0.6f))
                    g.DrawEllipse(p2, sx-sr+1, sy-sr+1, sr*2-2, sr*2-2);

                int nRayos = 16;
                for (int i = 0; i < nRayos; i++)
                {
                    double ang = i * Math.PI * 2 / nRayos;
                    float lenRayo = (i % 2 == 0) ? sr + 18 + _rnd.Next(6) : sr + 9 + _rnd.Next(4);
                    float r1 = sr + 3;
                    PointF pa = new PointF(sx + (float)(Math.Cos(ang)*r1),
                                           sy + (float)(Math.Sin(ang)*r1));
                    PointF pb = new PointF(sx + (float)(Math.Cos(ang)*lenRayo),
                                           sy + (float)(Math.Sin(ang)*lenRayo));
                    using (Pen p = new Pen(tinta, i % 2 == 0 ? 1.4f : 0.9f))
                        LineInk(g, p, pa, pb);
                }
            }
            else
            {
                using (Pen p = new Pen(tinta, Grosor(1.0f)))
                    g.DrawEllipse(p, sx-sr, sy-sr, sr*2, sr*2);
                for (int i = 0; i < 12; i++)
                {
                    double ang = i * Math.PI / 6;
                    float r1 = sr + 6, r2 = sr + 14 + _rnd.Next(4);
                    g.DrawLine(new Pen(tinta, Grosor(0.7f)),
                        sx + (float)(Math.Cos(ang)*r1), sy + (float)(Math.Sin(ang)*r1),
                        sx + (float)(Math.Cos(ang)*r2), sy + (float)(Math.Sin(ang)*r2));
                }
            }
        }

        private void DibujarLunaDibujo(Graphics g, int w, int hz)
        {
            int mx = (int)(w * (0.7 + _rnd.NextDouble() * 0.15));
            int my = (int)(hz * (0.1 + _rnd.NextDouble() * 0.15));
            int mr = (int)(w * 0.04);
            Color tinta = ColorTinta(0.7f);
            using (Pen p = new Pen(tinta, Grosor(1.1f)))
                g.DrawArc(p, mx-mr, my-mr, mr*2, mr*2, 200, 320);
            DibujarHatching(g, mx-mr, my-mr, mr*2, mr*2, 4, tinta, 0.3f);
        }

        private void DibujarEstrellasDibujo(Graphics g, int w, int hz)
        {
            Color tinta = ColorTinta(0.5f);
            int n = w / 25 + _rnd.Next(10);
            for (int i = 0; i < n; i++)
            {
                float sx = _rnd.Next(w);
                float sy = _rnd.Next(hz - 10);
                float sz = 1.0f + (float)_rnd.NextDouble() * 2;
                using (Pen p = new Pen(tinta, sz * 0.5f))
                {
                    g.DrawLine(p, sx-sz, sy, sx+sz, sy);
                    g.DrawLine(p, sx, sy-sz, sx, sy+sz);
                }
            }
        }

        private void DibujarMontanasDibujo(Graphics g, int w, int h, int hz)
        {
            bool ink = _ctx.EstiloLapiz == "lapicera";
            int nPlanos = 3;
            for (int pl = 0; pl < nPlanos; pl++)
            {
                float factorProf = 1.0f - pl * 0.28f;
                float baseY = hz + pl * 10;
                float alpha = ink ? (0.55f + pl * 0.15f) : (0.3f + pl * 0.25f);
                Color tinta = ColorTinta(alpha);
                float grosor = ink ? (1.0f + pl * 0.4f) : Grosor(0.8f + pl * 0.3f);

                double seed = _ctx.Semilla * 0.001 + pl * 1.7;
                var pts = new List<PointF>();
                pts.Add(new PointF(0, h));
                int nPicos = 4 + pl;
                for (int i = 0; i <= w; i += w / (nPicos * 4))
                {
                    double nx = (double)i / w;
                    double ny = Matematica.FBM(nx * 2.5 + seed, seed * 0.3, 5, 0.55);
                    float pY = baseY - (float)(ny * 0.5 + 0.5) * (hz * (0.55f + pl * 0.05f) * factorProf);
                    pY += ink ? (float)(_rnd.NextDouble() * 1.5 - 0.75) : (float)(_rnd.NextDouble() * 3 - 1.5);
                    pts.Add(new PointF(i, Math.Max(5, pY)));
                }
                pts.Add(new PointF(w, h));

                using (Pen p = new Pen(tinta, grosor))
                {
                    for (int i = 0; i < pts.Count - 1; i++)
                    {
                        if (ink) LineInk(g, p, pts[i], pts[i+1]);
                        else     LineTremor(g, p, pts[i], pts[i+1]);
                    }
                }

                if (_ctx.ConNieve && pl == nPlanos - 1)
                {
                    using (Pen pN = new Pen(ColorTinta(0.3f), ink ? 1.2f : Grosor(0.6f)))
                        for (int i = 1; i < pts.Count - 2; i++)
                            if (pts[i].Y < hz * 0.5f)
                            {
                                g.DrawLine(pN, pts[i].X - 4, pts[i].Y + 6, pts[i].X + 4, pts[i].Y + 6);
                                g.DrawLine(pN, pts[i].X, pts[i].Y + 2, pts[i].X - 5, pts[i].Y + 9);
                            }
                }

                if (ink)
                    CrossHatchMontanaInk(g, pts, hz, pl);
                else
                    DibujarHatchingMontana(g, pts, hz, tinta, pl);
            }
        }

        private void CrossHatchMontanaInk(Graphics g, List<PointF> perfil, int hz, int plano)
        {
            if (plano < 1) return;
            // Crosshatching diagonal en las laderas de la montaña
            float densidad = 0.5f + plano * 0.35f;
            int paso = Math.Max(5, (int)(10 / densidad));
            Color tinta = ColorTinta(0.5f + plano * 0.15f);

            // Líneas diagonales a 45° dentro del perfil
            using (Pen p = new Pen(tinta, 0.7f))
            {
                for (int xi = 0; xi < perfil.Count - 2; xi++)
                {
                    float px0 = perfil[xi].X;
                    float perfilY = perfil[xi].Y;
                    for (float yo = perfilY + 4; yo < hz; yo += paso)
                    {
                        float x1 = px0;
                        float y1 = yo;
                        float x2 = px0 + (yo - perfilY) * 0.7f;
                        float y2 = yo + (yo - perfilY) * 0.4f;
                        if (y2 > hz) { float t = (hz - y1)/(y2-y1); x2 = x1+(x2-x1)*t; y2 = hz; }
                        float jx = (float)(_rnd.NextDouble() * 0.8 - 0.4);
                        g.DrawLine(p, x1+jx, y1, x2+jx, y2);
                    }
                }
            }
            // Segunda capa cruzada para sombra más densa en planos cercanos
            if (plano >= 2)
            {
                int paso2 = paso + 3;
                using (Pen p = new Pen(ColorTinta(0.65f), 0.6f))
                {
                    for (int xi = 0; xi < perfil.Count - 2; xi++)
                    {
                        float px0    = perfil[xi].X;
                        float perfilY = perfil[xi].Y;
                        for (float yo = perfilY + 6; yo < hz; yo += paso2)
                        {
                            float x1 = px0, y1 = yo;
                            float x2 = px0 - (yo - perfilY) * 0.5f;
                            float y2 = yo + (yo - perfilY) * 0.3f;
                            if (y2 > hz) { float t = (hz-y1)/(y2-y1); x2=x1+(x2-x1)*t; y2=hz; }
                            float jx = (float)(_rnd.NextDouble() * 0.8 - 0.4);
                            g.DrawLine(p, x1+jx, y1, x2+jx, y2);
                        }
                    }
                }
            }
        }

        private void DibujarHatchingMontana(Graphics g, List<PointF> perfil, int hz, Color tinta, int plano)
        {
            if (plano < 1) return;
            int paso = 10 + plano * 4;
            Color hCol = Color.FromArgb((int)(tinta.A * 0.35f), tinta.R, tinta.G, tinta.B);
            using (Pen p = new Pen(hCol, Grosor(0.5f)))
            {
                for (int xi = 0; xi < perfil.Count - 2; xi++)
                {
                    float perfilY = perfil[xi].Y;
                    float px = perfil[xi].X;
                    for (float y = perfilY; y < hz; y += paso)
                    {
                        float xOff = (float)(_rnd.NextDouble() * 3 - 1.5);
                        g.DrawLine(p, px + xOff, y, px + paso * 0.7f + xOff, y + paso * 0.4f);
                    }
                }
            }
        }

        private void DibujarAguaDibujo(Graphics g, int w, int h, int hz)
        {
            bool ink = _ctx.EstiloLapiz == "lapicera";
            int nLineas = (h - hz) / (ink ? 10 : 14);
            for (int i = 0; i < nLineas; i++)
            {
                float yBase = hz + i * (ink ? 10 : 14) + (float)(_rnd.NextDouble() * 3);
                float largo  = w * (ink ? (0.15f + (float)_rnd.NextDouble() * 0.55f)
                                        : (0.3f + (float)_rnd.NextDouble() * 0.5f));
                float x0    = (float)(_rnd.NextDouble() * (w - largo));
                float curva = (float)(_rnd.NextDouble() * 4 - 2);
                float profundidad = (float)i / nLineas;

                // Las ondas más cercanas son más gruesas y oscuras
                float grosorOnda = ink ? (0.6f + profundidad * 1.0f) : Grosor(0.6f + i * 0.05f);
                Color tintaOnda  = ColorTinta(ink ? (0.35f + profundidad * 0.35f)
                                                   : (0.25f + i * 0.01f));

                using (Pen p = new Pen(tintaOnda, grosorOnda))
                using (var path = new System.Drawing.Drawing2D.GraphicsPath())
                {
                    if (ink)
                    {
                        // Ondas de tinta: beziers con micro-variación de presión
                        float mid1X = x0 + largo * 0.25f;
                        float mid2X = x0 + largo * 0.75f;
                        float jy1   = (float)(_rnd.NextDouble() * 2 - 1);
                        float jy2   = (float)(_rnd.NextDouble() * 2 - 1);
                        path.AddBezier(x0, yBase, mid1X, yBase + curva + jy1,
                                       mid2X, yBase - curva + jy2, x0 + largo, yBase);
                    }
                    else
                    {
                        path.AddBezier(x0, yBase, x0 + largo * 0.33f, yBase + curva,
                                       x0 + largo * 0.66f, yBase - curva, x0 + largo, yBase);
                    }
                    g.DrawPath(p, path);
                }

                // Tinta: agregar líneas secundarias cortas para textura de agua
                if (ink && i % 3 == 0 && largo > 60)
                {
                    float x2nd = x0 + largo * 0.3f + (float)(_rnd.NextDouble() * largo * 0.4f);
                    float len2 = largo * (0.05f + (float)_rnd.NextDouble() * 0.12f);
                    using (Pen p2 = new Pen(ColorTinta(0.25f + profundidad * 0.2f), 0.6f))
                        g.DrawLine(p2, x2nd, yBase + 4, x2nd + len2, yBase + 4);
                }
            }
        }

        private void DibujarBosqueDibujo(Graphics g, int w, int h, int hz)
        {
            bool ink = _ctx.EstiloLapiz == "lapicera";

            // Masa forestal al fondo: silueta de treeline con FBM
            int nPlanos = 3;
            for (int pl = 0; pl < nPlanos; pl++)
            {
                double tS = _ctx.Semilla * 0.003 + pl * 5.5;
                double esc = 8.0 - pl * 1.8;
                float yBase = hz + pl * (int)((h - hz) * 0.12f);
                float altMax = (h - hz) * (0.28f - pl * 0.06f);
                float alphaLine = 0.25f + pl * 0.18f;
                Color tinta = ColorTinta(alphaLine);

                // Perfil de copas usando FBM
                int nPts = w / 2;
                var pts = new List<PointF>();
                pts.Add(new PointF(-5, h));
                for (int xi = 0; xi <= nPts; xi++)
                {
                    double nx = (double)xi / nPts;
                    double n1 = Matematica.FBM(nx * esc + tS, 1.0 + pl, 4, 0.60);
                    double n2 = Matematica.FBM(nx * esc * 3.5 + tS + 5, 0.5, 3, 0.55) * 0.28;
                    double alt = ((n1 + n2) + 1) * 0.5;
                    float py = yBase - (float)(altMax * alt);
                    float jy = (float)(_rnd.NextDouble() * (ink ? 1.5 : 3) - (ink ? 0.75 : 1.5));
                    pts.Add(new PointF((float)(nx * w), Math.Max(5, py + jy)));
                }
                pts.Add(new PointF(w + 5, h));

                // Línea de silueta
                using (Pen p = new Pen(tinta, Grosor(ink ? 0.9f : 0.7f)))
                    for (int i = 0; i < pts.Count - 1; i++)
                    {
                        if (ink) LineInk(g, p, pts[i], pts[i + 1]);
                        else LineTremor(g, p, pts[i], pts[i + 1]);
                    }

                // Hatching interior (masa forestal)
                if (pl >= 1)
                {
                    Color tintaH = ColorTinta(alphaLine * 0.5f);
                    using (Pen ph = new Pen(tintaH, Grosor(0.4f)))
                    {
                        int pasoH = ink ? 8 : 12;
                        for (int xi = 0; xi < pts.Count - 2; xi++)
                        {
                            float px0 = pts[xi].X, pyTop = pts[xi].Y;
                            for (float y = pyTop + 4; y < hz + (h - hz) * 0.4f; y += pasoH)
                            {
                                float yy = y + (float)(_rnd.NextDouble() * 3);
                                float len = pasoH * 0.6f;
                                if (ink) LineInk(g, ph, new PointF(px0, yy), new PointF(px0 + len, yy + len * 0.5f));
                                else g.DrawLine(ph, px0, yy, px0 + len, yy);
                            }
                        }
                    }
                }
            }

            // Árboles individuales en primer plano (variados)
            int nArboles = w / 55 + _rnd.Next(5);
            // Ordenar por Y para simular profundidad (atrás primero)
            var posArboles = new List<float[]>();
            for (int i = 0; i < nArboles; i++)
            {
                float x = _rnd.Next(w);
                float bY = hz + (float)(_rnd.NextDouble() * (h - hz) * 0.5);
                posArboles.Add(new float[] { x, bY });
            }
            posArboles.Sort(delegate(float[] a, float[] b2) { return a[1].CompareTo(b2[1]); });

            foreach (float[] pos in posArboles)
            {
                float x = pos[0], bY = pos[1];
                float pct = (bY - hz) / (h - hz);
                float alt = (50 + pct * 70) * (0.8f + (float)_rnd.NextDouble() * 0.4f);
                float anch = alt * (0.35f + (float)_rnd.NextDouble() * 0.35f);

                // 40% coníferas, 60% frondosas
                bool conifera = _rnd.NextDouble() < 0.4;
                if (conifera)
                    DibujarConifera(g, x, bY, alt, anch * 0.6f);
                else
                    DibujarArbol(g, x, bY, alt, anch);
            }
        }

        private void DibujarConifera(Graphics g, float x, float baseY, float altura, float ancho)
        {
            bool ink = _ctx.EstiloLapiz == "lapicera";
            Color tinta = ColorTinta(0.72f);
            float topY = baseY - altura;

            // Tronco recto delgado
            using (Pen p = new Pen(ColorTinta(0.8f), Grosor(ink ? 1.0f : 0.9f)))
            {
                if (ink) LineInk(g, p, new PointF(x, baseY), new PointF(x, topY));
                else LineTremor(g, p, new PointF(x, baseY), new PointF(x, topY));
            }

            // 3-4 niveles de ramas triangulares escalonadas
            int nNiveles = 3 + _rnd.Next(2);
            for (int ni = 0; ni < nNiveles; ni++)
            {
                float t = (float)ni / nNiveles;
                float nivelY = baseY - altura * (0.2f + t * 0.7f);
                float nivelAnch = ancho * (1.0f - t * 0.65f);
                float nivelAltoSeg = altura * 0.22f;

                PointF cima = new PointF(x, nivelY - nivelAltoSeg);
                PointF izq = new PointF(x - nivelAnch, nivelY + nivelAltoSeg * 0.3f);
                PointF der = new PointF(x + nivelAnch, nivelY + nivelAltoSeg * 0.3f);

                using (Pen p = new Pen(tinta, Grosor(ink ? 1.1f : 0.8f)))
                {
                    if (ink)
                    {
                        LineInk(g, p, cima, izq); LineInk(g, p, cima, der);
                        LineInk(g, p, izq, der);
                    }
                    else
                    {
                        LineTremor(g, p, cima, izq); LineTremor(g, p, cima, der);
                        LineTremor(g, p, izq, der);
                    }
                }
                // Ramitas desde los bordes
                if (ni >= 1)
                {
                    using (Pen pr = new Pen(ColorTinta(0.55f), Grosor(0.5f)))
                        for (int ri = -1; ri <= 1; ri += 2)
                        {
                            float ramX = x + ri * nivelAnch * 0.5f;
                            float ramY = nivelY;
                            float ramEndX = ramX + ri * nivelAnch * 0.3f;
                            float ramEndY = ramY + nivelAltoSeg * 0.25f;
                            g.DrawLine(pr, ramX, ramY, ramEndX, ramEndY);
                        }
                }
            }
        }

        private void DibujarArbol(Graphics g, float x, float baseY, float altura, float ancho)
        {
            bool ink    = _ctx.EstiloLapiz == "lapicera";
            Color tinta = ColorTinta(0.7f);
            float troncoH = altura * 0.25f;
            float troncoW = ancho * 0.12f;

            using (Pen p = new Pen(tinta, ink ? 1.6f : Grosor(1.2f)))
            {
                if (ink)
                {
                    LineInk(g, p, new PointF(x - troncoW * 0.5f, baseY), new PointF(x - troncoW * 0.3f, baseY - troncoH));
                    LineInk(g, p, new PointF(x + troncoW * 0.5f, baseY), new PointF(x + troncoW * 0.3f, baseY - troncoH));
                    // Textura de corteza en el tronco
                    int nCortes = (int)(troncoH / 8);
                    using (Pen pC = new Pen(ColorTinta(0.6f), 0.6f))
                        for (int ci = 1; ci < nCortes; ci++)
                        {
                            float cy = baseY - ci * 8;
                            float cx1 = x - troncoW * 0.4f + (float)(_rnd.NextDouble() * 3);
                            float cx2 = x + troncoW * 0.4f - (float)(_rnd.NextDouble() * 3);
                            g.DrawLine(pC, cx1, cy, cx2, cy + (float)(_rnd.NextDouble() * 3 - 1.5));
                        }
                }
                else
                {
                    LineTremor(g, p, new PointF(x, baseY), new PointF(x, baseY - troncoH));
                    LineTremor(g, p, new PointF(x - troncoW, baseY), new PointF(x, baseY - troncoH));
                    LineTremor(g, p, new PointF(x + troncoW, baseY), new PointF(x, baseY - troncoH));
                }
            }

            bool esConifero = _rnd.NextDouble() > 0.45;
            if (esConifero)
            {
                int nNiveles = 3 + _rnd.Next(2);
                float nivelH = (altura - troncoH) / nNiveles;
                using (Pen p = new Pen(tinta, ink ? 1.3f : Grosor(1.0f)))
                {
                    for (int n = 0; n < nNiveles; n++)
                    {
                        float ty = baseY - troncoH - n * nivelH;
                        float tw = ancho * (1.0f - n * 0.25f) * 0.5f;
                        float tipY = ty - nivelH * 1.1f;
                        if (ink)
                        {
                            LineInk(g, p, new PointF(x - tw, ty), new PointF(x, tipY));
                            LineInk(g, p, new PointF(x + tw, ty), new PointF(x, tipY));
                            LineInk(g, p, new PointF(x - tw, ty), new PointF(x + tw, ty));
                            // Interior con hatching diagonal
                            CrossHatch(g, x - tw, tipY, tw * 2, ty - tipY, 0.6f, 0.55f, false);
                        }
                        else
                        {
                            LineTremor(g, p, new PointF(x - tw, ty), new PointF(x, tipY));
                            LineTremor(g, p, new PointF(x + tw, ty), new PointF(x, tipY));
                            LineTremor(g, p, new PointF(x - tw, ty), new PointF(x + tw, ty));
                        }
                    }
                }
            }
            else
            {
                float copaY  = baseY - troncoH - altura * 0.4f;
                float copaRX = ancho * 0.5f;
                float copaRY = altura * 0.45f;
                using (Pen p = new Pen(tinta, ink ? 1.3f : Grosor(1.0f)))
                {
                    int nSegs = ink ? 24 : 18;
                    for (int ci = 0; ci < nSegs; ci++)
                    {
                        double a1 = ci * Math.PI * 2 / nSegs;
                        double a2 = (ci+1) * Math.PI * 2 / nSegs;
                        float jx = ink ? (float)(_rnd.NextDouble() * 2 - 1) : (float)(_rnd.NextDouble() * 4 - 2);
                        float jy = ink ? (float)(_rnd.NextDouble() * 2 - 1) : (float)(_rnd.NextDouble() * 4 - 2);
                        PointF pa = new PointF(x + (float)(Math.Cos(a1)*copaRX) + jx,
                                               copaY + (float)(Math.Sin(a1)*copaRY) + jy);
                        PointF pb = new PointF(x + (float)(Math.Cos(a2)*copaRX) + jx,
                                               copaY + (float)(Math.Sin(a2)*copaRY) + jy);
                        if (ink) LineInk(g, p, pa, pb);
                        else     g.DrawLine(p, pa, pb);
                    }
                }
                // Crosshatching en la copa para volumen
                CrossHatch(g, x - copaRX, copaY - copaRY, copaRX*2, copaRY*2,
                           ink ? 0.7f : 0.4f, 0.5f, ink);
            }
        }

        private void DibujarCiudadDibujo(Graphics g, int w, int h, int hz)
        {
            bool ink = _ctx.EstiloLapiz == "lapicera";
            int nEdif = w / 80 + _rnd.Next(3);
            Color tinta = ColorTinta(0.75f);

            // Ordenar por tamaño (los altos al fondo)
            var edificios = new List<int[]>();
            for (int i = 0; i < nEdif; i++)
            {
                int bw = 35 + _rnd.Next(70);
                int bh = 70 + _rnd.Next(200);
                int bx = _rnd.Next(w - bw);
                edificios.Add(new int[]{ bx, hz - bh, bw, bh });
            }
            edificios.Sort(delegate(int[] a, int[] b2) { return b2[3].CompareTo(a[3]); });

            foreach (int[] edif in edificios)
            {
                int bx = edif[0], by = edif[1], bw = edif[2], bh = edif[3];
                float grosorEdif = ink ? 1.8f : Grosor(1.2f);

                using (Pen p = new Pen(tinta, grosorEdif))
                {
                    if (ink)
                    {
                        LineInk(g, p, new PointF(bx, by + bh), new PointF(bx, by));
                        LineInk(g, p, new PointF(bx, by), new PointF(bx + bw, by));
                        LineInk(g, p, new PointF(bx + bw, by), new PointF(bx + bw, by + bh));
                        LineInk(g, p, new PointF(bx, by + bh), new PointF(bx + bw, by + bh));
                    }
                    else
                    {
                        g.DrawRectangle(p, bx, by, bw, bh);
                    }
                }

                // Crosshatching en la fachada (lado izquierdo en sombra)
                if (ink)
                    CrossHatch(g, bx, by, bw * 0.3f, bh, 0.55f, 0.6f, false);

                // Ventanas con detalles de tinta
                int nVentH = bh / 22, nVentW = bw / 18;
                for (int vy = 0; vy < nVentH; vy++)
                    for (int vx = 0; vx < nVentW; vx++)
                    {
                        int wx2 = bx + 6 + vx * 18;
                        int wy  = by + 8 + vy * 22;
                        if (wx2 + 10 > bx + bw - 4) continue;
                        bool encendida = _rnd.NextDouble() > (ink ? 0.45 : 0.35);
                        using (Pen pw = new Pen(ColorTinta(ink ? 0.65f : 0.7f),
                                                ink ? 1.0f : Grosor(0.7f)))
                        {
                            if (ink)
                                LineInk(g, pw, new PointF(wx2, wy), new PointF(wx2 + 8, wy + 12), true);
                            g.DrawRectangle(pw, wx2, wy, 8, 12);
                        }
                        if (encendida)
                            CrossHatch(g, wx2, wy, 8, 12, ink ? 1.2f : 0.8f,
                                       ink ? 0.55f : 0.4f, false);
                    }

                // Detalles arquitectónicos en tinta: cornisa, base
                if (ink)
                {
                    using (Pen pD = new Pen(ColorTinta(0.7f), 1.0f))
                    {
                        LineInk(g, pD, new PointF(bx - 2, by + 2), new PointF(bx + bw + 2, by + 2), true);
                        LineInk(g, pD, new PointF(bx - 2, by + bh - 4), new PointF(bx + bw + 2, by + bh - 4), true);
                    }
                }
            }
        }

        private void DibujarDesiertoSketch(Graphics g, int w, int h, int hz)
        {
            bool ink = _ctx.EstiloLapiz == "lapicera";

            // Cielo: rayado horizontal muy suave para dar sensación de calor
            Color tintaCielo = ColorTinta(0.15f);
            int nRayosCielo = hz / 18;
            for (int i = 0; i < nRayosCielo; i++)
            {
                float y = i * 18 + (float)(_rnd.NextDouble() * 8);
                float largo = w * (0.2f + (float)_rnd.NextDouble() * 0.6f);
                float x0 = (float)(_rnd.NextDouble() * (w - largo));
                using (Pen p = new Pen(tintaCielo, Grosor(0.4f)))
                    g.DrawLine(p, x0, y, x0 + largo, y);
            }

            // Sol en el cielo
            if (!_ctx.SinSol) DibujarSolDibujo(g, w, hz);

            // Dunas: perfiles FBM suaves con muchos puntos
            int nDunas = 4 + _rnd.Next(3);
            for (int d = 0; d < nDunas; d++)
            {
                double seed = _ctx.Semilla * 0.002 + d * 2.3;
                float dBaseY = hz + d * (int)((h - hz) * 0.22f);
                float altMax = (h - hz) * (0.18f - d * 0.02f);
                float alphaFade = 0.4f + d * 0.15f;
                Color tinta = ColorTinta(alphaFade);

                // Perfil de la duna con muchos puntos (evita electrocardiograma)
                int nPts = w / 3;
                var pts = new List<PointF>();
                pts.Add(new PointF(0, h));
                for (int xi = 0; xi <= nPts; xi++)
                {
                    double nx = (double)xi / nPts;
                    // FBM de baja frecuencia: formas de duna suave
                    double fbm = Matematica.FBM(nx * 1.8 + seed, seed * 0.4, 4, 0.55);
                    float py = dBaseY - (float)(fbm * 0.5 + 0.5) * altMax;
                    float jy = (float)(_rnd.NextDouble() * 1.5 - 0.75);
                    pts.Add(new PointF((float)(nx * w), Math.Max(hz + 2, py + jy)));
                }
                pts.Add(new PointF(w, h));

                // Dibujar silueta
                using (Pen p = new Pen(tinta, Grosor(ink ? 1.2f : 0.9f)))
                    for (int pi = 0; pi < pts.Count - 1; pi++)
                    {
                        if (ink) LineInk(g, p, pts[pi], pts[pi + 1]);
                        else LineTremor(g, p, pts[pi], pts[pi + 1]);
                    }

                // Sombra en las laderas de la duna (lado derecho)
                if (d < nDunas - 1)
                {
                    for (int xi = 1; xi < pts.Count - 2; xi++)
                    {
                        float slope = pts[xi + 1].Y - pts[xi].Y;
                        if (slope < -2) // ladera descendente = lado soleado, sombra al otro
                        {
                            float px = pts[xi].X;
                            float pySurf = pts[xi].Y;
                            int nSombra = (int)(Math.Abs(slope) * 0.8);
                            Color cSombra = ColorTinta(alphaFade * 0.45f);
                            using (Pen ps = new Pen(cSombra, Grosor(0.4f)))
                                for (int si = 0; si < nSombra; si++)
                                {
                                    float sy = pySurf + si * 7;
                                    if (sy >= h) break;
                                    float sx0 = px + si * 3;
                                    float sx1 = px + (float)(w / (nPts * 1.5));
                                    g.DrawLine(ps, sx0, sy, Math.Min(w - 1, sx1), sy + 3);
                                }
                        }
                    }
                }
            }

            // Cactos: elemento icónico del desierto
            int nCactos = 2 + _rnd.Next(4);
            Color tintaCacto = ColorTinta(0.7f);
            for (int c = 0; c < nCactos; c++)
            {
                float cx = (float)(w * (0.08 + _rnd.NextDouble() * 0.84));
                float cyBase = hz + (float)(_rnd.NextDouble() * (h - hz) * 0.5f);
                float altCacto = 40 + (float)_rnd.NextDouble() * 60;
                float anchCacto = altCacto * 0.18f;

                using (Pen p = new Pen(tintaCacto, Grosor(ink ? 1.4f : 1.1f)))
                {
                    // Tronco central
                    if (ink) LineInk(g, p, new PointF(cx, cyBase), new PointF(cx, cyBase - altCacto));
                    else LineTremor(g, p, new PointF(cx, cyBase), new PointF(cx, cyBase - altCacto));

                    // Brazos (1-2 por cacto)
                    int nBrazos = 1 + _rnd.Next(2);
                    for (int br = 0; br < nBrazos; br++)
                    {
                        float lado = br % 2 == 0 ? 1 : -1;
                        float brazoY = cyBase - altCacto * (0.35f + (float)_rnd.NextDouble() * 0.35f);
                        float brazoLen = altCacto * (0.3f + (float)_rnd.NextDouble() * 0.25f);
                        float brazoX = cx + lado * anchCacto * 3;

                        // Brazo horizontal
                        if (ink) LineInk(g, p, new PointF(cx, brazoY), new PointF(brazoX, brazoY));
                        else LineTremor(g, p, new PointF(cx, brazoY), new PointF(brazoX, brazoY));
                        // Brazo vertical hacia arriba
                        if (ink) LineInk(g, p, new PointF(brazoX, brazoY), new PointF(brazoX, brazoY - brazoLen));
                        else LineTremor(g, p, new PointF(brazoX, brazoY), new PointF(brazoX, brazoY - brazoLen));
                    }

                    // Textura del cacto: costillas verticales
                    using (Pen pc = new Pen(ColorTinta(0.4f), Grosor(0.5f)))
                        for (float cxi = cx - anchCacto; cxi <= cx + anchCacto; cxi += anchCacto * 0.6f)
                            g.DrawLine(pc, cxi, cyBase, cxi, cyBase - altCacto * 0.95f);
                }
            }

            // Horizonte de calor: línea ondulada en el límite del suelo
            using (Pen pH = new Pen(ColorTinta(0.55f), Grosor(ink ? 1.5f : 1.1f)))
            {
                for (int x = 0; x < w - 4; x += 4)
                {
                    float dy1 = (float)(Matematica.Perlin(x * 0.004, 0.7) * 5);
                    float dy2 = (float)(Matematica.Perlin((x + 4) * 0.004, 0.7) * 5);
                    if (ink) LineInk(g, pH, new PointF(x, hz + dy1), new PointF(x + 4, hz + dy2));
                    else g.DrawLine(pH, x, hz + dy1, x + 4, hz + dy2);
                }
            }
        }

        private void DibujarFloresDibujo(Graphics g, int w, int h, int hz)
        {
            bool ink = _ctx.EstiloLapiz == "lapicera";

            // Fondo: pasto con líneas verticales cortas
            Color tintaPasto = ColorTinta(0.3f);
            int nPastos = w * 2;
            for (int i = 0; i < nPastos; i++)
            {
                float px = (float)(_rnd.NextDouble() * w);
                float py = hz + (float)(_rnd.NextDouble() * (h - hz));
                float largoP = 4 + (float)(_rnd.NextDouble() * 10);
                float incP = (float)(_rnd.NextDouble() * 6 - 3);
                using (Pen p = new Pen(tintaPasto, Grosor(0.4f)))
                    g.DrawLine(p, px, py, px + incP, py - largoP);
            }

            // Flores: distribuidas en todo el campo con perspectiva (más grandes adelante)
            int nFlores = w / 18 + _rnd.Next(15);
            // Ordenar por Y para pintar primero las del fondo (más pequeñas)
            var flores = new List<float[]>();
            for (int i = 0; i < nFlores; i++)
            {
                float fx = (float)(_rnd.NextDouble() * w);
                float fy = hz + 5 + (float)(_rnd.NextDouble() * (h - hz - 5));
                flores.Add(new float[] { fx, fy });
            }
            flores.Sort(delegate(float[] a, float[] b2) { return a[1].CompareTo(b2[1]); });

            foreach (float[] flor in flores)
            {
                float fx = flor[0], fy = flor[1];
                float pct = (fy - hz) / (h - hz);   // 0=fondo 1=primer plano

                // Tamaño proporcional a la profundidad
                float talloH  = (12 + pct * 55) * (0.7f + (float)_rnd.NextDouble() * 0.6f);
                float pr      = (4 + pct * 14) * (0.6f + (float)_rnd.NextDouble() * 0.8f);
                float grosorT = Grosor(0.6f + pct * 0.6f);
                Color tintaFlor = ColorTinta(0.5f + pct * 0.3f);

                // Tallo curvo con hoja
                float topX = fx + (float)(_rnd.NextDouble() * 10 - 5);
                float topY = fy - talloH;
                using (Pen p = new Pen(tintaFlor, grosorT))
                using (var path = new System.Drawing.Drawing2D.GraphicsPath())
                {
                    float mx = fx + (topX - fx) * 0.5f + (float)(_rnd.NextDouble() * 8 - 4);
                    path.AddBezier(fx, fy, mx, fy - talloH * 0.4f, topX, topY + talloH * 0.3f, topX, topY);
                    if (ink) { /* bezier directo */ g.DrawPath(p, path); }
                    else g.DrawPath(p, path);
                }

                // Hoja a media altura del tallo
                if (pct > 0.2f && _rnd.NextDouble() > 0.4)
                {
                    float hojaY = fy - talloH * (0.3f + (float)_rnd.NextDouble() * 0.3f);
                    float hojaL = pr * (1.2f + (float)_rnd.NextDouble());
                    float lado  = _rnd.NextDouble() > 0.5 ? 1 : -1;
                    using (Pen p = new Pen(ColorTinta(0.4f + pct * 0.2f), Grosor(0.5f)))
                    using (var path = new System.Drawing.Drawing2D.GraphicsPath())
                    {
                        path.AddBezier(fx, hojaY, fx + lado * hojaL * 0.5f, hojaY - pr * 0.4f,
                                       fx + lado * hojaL, hojaY, fx, hojaY);
                        g.DrawPath(p, path);
                    }
                }

                // Pétalos en la cabeza
                int nPetalos = 5 + _rnd.Next(4);
                float cabezaY = topY;
                using (Pen p = new Pen(tintaFlor, Grosor(0.7f + pct * 0.4f)))
                {
                    for (int pe = 0; pe < nPetalos; pe++)
                    {
                        double ang = pe * Math.PI * 2 / nPetalos + _rnd.NextDouble() * 0.3;
                        float petalLx = pr * (0.8f + (float)_rnd.NextDouble() * 0.5f);
                        float petalLy = petalLx * 0.65f;
                        float px2 = topX + (float)(Math.Cos(ang) * pr * 0.9);
                        float py2 = cabezaY + (float)(Math.Sin(ang) * pr * 0.65f);

                        // Pétalo como elipse pequeña o curva bezier
                        if (ink && pct > 0.5f)
                        {
                            using (var path = new System.Drawing.Drawing2D.GraphicsPath())
                            {
                                path.AddBezier(topX, cabezaY,
                                    topX + (float)(Math.Cos(ang - 0.4) * petalLx),
                                    cabezaY + (float)(Math.Sin(ang - 0.4) * petalLy),
                                    topX + (float)(Math.Cos(ang + 0.4) * petalLx),
                                    cabezaY + (float)(Math.Sin(ang + 0.4) * petalLy),
                                    topX, cabezaY);
                                g.DrawPath(p, path);
                            }
                        }
                        else
                        {
                            float eW = Math.Max(3, pr * 0.55f), eH = Math.Max(2, pr * 0.40f);
                            g.DrawEllipse(p, px2 - eW / 2, py2 - eH / 2, eW, eH);
                        }
                    }
                }

                // Centro de la flor
                float cR = Math.Max(2, pr * 0.28f);
                using (Pen p = new Pen(ColorTinta(0.8f), Grosor(0.8f + pct * 0.5f)))
                    g.DrawEllipse(p, topX - cR, cabezaY - cR, cR * 2, cR * 2);

                // En primer plano: punto de detalle en el centro
                if (pct > 0.6f)
                    CrossHatch(g, topX - cR, cabezaY - cR, cR * 2, cR * 2, 0.6f, 0.7f, false);
            }
        }

        private void DibujarIslasDibujo(Graphics g, int w, int h, int hz)
        {
            int nIslas = 2 + _rnd.Next(3);
            Color tinta = ColorTinta(0.55f);
            for (int i = 0; i < nIslas; i++)
            {
                float cx = (float)(w * (0.1 + _rnd.NextDouble() * 0.8));
                float cy = hz - 5 - (float)(_rnd.NextDouble() * (hz * 0.15));
                float rw = 40 + (float)(_rnd.NextDouble() * 80);
                float rh = 12 + (float)(_rnd.NextDouble() * 20);
                using (Pen p = new Pen(tinta, Grosor(1.0f)))
                    g.DrawEllipse(p, cx - rw, cy - rh, rw*2, rh*2);
                DibujarHatching(g, (int)(cx-rw), (int)(cy-rh), (int)(rw*2), (int)(rh*2), 7, tinta, 0.3f);
                if (_rnd.NextDouble() > 0.4)
                    DibujarArbol(g, cx, cy - rh, 35 + (float)_rnd.NextDouble()*20, 16);
            }
        }

        private void DibujarRocasDibujo(Graphics g, int w, int h, int hz)
        {
            int nRocas = 3 + _rnd.Next(5);
            Color tinta = ColorTinta(0.65f);
            for (int i = 0; i < nRocas; i++)
            {
                float rx = _rnd.Next(w);
                float ry = hz + (float)(_rnd.NextDouble() * (h - hz) * 0.7);
                int nPts = 5 + _rnd.Next(4);
                var ptR = new PointF[nPts];
                float rr = 15 + (float)(_rnd.NextDouble() * 25);
                for (int pi = 0; pi < nPts; pi++)
                {
                    double ang = pi * Math.PI * 2 / nPts;
                    float rVar = rr * (0.7f + (float)_rnd.NextDouble() * 0.6f);
                    ptR[pi] = new PointF(rx + (float)(Math.Cos(ang)*rVar), ry + (float)(Math.Sin(ang)*rVar*0.6));
                }
                using (Pen p = new Pen(tinta, Grosor(1.0f)))
                    g.DrawPolygon(p, ptR);
                DibujarHatching(g, (int)(rx-rr), (int)(ry-rr*0.6), (int)(rr*2), (int)(rr*1.2), 6, tinta, 0.25f);
            }
        }

        private void DibujarPalmerasDibujo(Graphics g, int w, int h, int hz)
        {
            int n = 1 + _rnd.Next(3);
            Color tinta = ColorTinta(0.65f);
            for (int i = 0; i < n; i++)
            {
                float bx = _rnd.Next(w);
                float by = hz + (float)(_rnd.NextDouble() * (h - hz) * 0.3);
                float alt = 90 + (float)(_rnd.NextDouble() * 60);
                float incl = (float)(_rnd.NextDouble() * 20 - 10);
                float topX = bx + incl, topY = by - alt;

                using (Pen p = new Pen(tinta, Grosor(1.5f)))
                {
                    using (var path = new System.Drawing.Drawing2D.GraphicsPath())
                    {
                        path.AddBezier(bx, by, bx + incl*0.3f, by - alt*0.4f,
                                       bx + incl*0.7f, by - alt*0.75f, topX, topY);
                        g.DrawPath(p, path);
                    }
                }

                int nFrondas = 7 + _rnd.Next(4);
                for (int f = 0; f < nFrondas; f++)
                {
                    double ang = f * Math.PI * 2 / nFrondas - Math.PI/2;
                    float len = 40 + (float)(_rnd.NextDouble() * 30);
                    float fEx = topX + (float)(Math.Cos(ang) * len);
                    float fEy = topY + (float)(Math.Sin(ang) * len * 0.5);
                    using (Pen p = new Pen(tinta, Grosor(0.9f)))
                    using (var path = new System.Drawing.Drawing2D.GraphicsPath())
                    {
                        float midX = topX + (float)(Math.Cos(ang) * len * 0.5f) + (float)(_rnd.NextDouble()*6-3);
                        float midY = topY + (float)(Math.Sin(ang) * len * 0.4f) + (float)(_rnd.NextDouble()*6-3);
                        path.AddBezier(topX, topY, midX, midY, fEx, fEy, fEx, fEy);
                        g.DrawPath(p, path);
                    }
                }
            }
        }

        private void DibujarLluvia(Graphics g, int w, int h)
        {
            Color tinta = ColorTinta(0.2f);
            int nGotas = w / 6;
            float angLluvia = 0.15f;
            for (int i = 0; i < nGotas; i++)
            {
                float rx = _rnd.Next(w);
                float ry = _rnd.Next(h);
                float largo = 8 + (float)(_rnd.NextDouble() * 10);
                using (Pen p = new Pen(tinta, Grosor(0.5f)))
                    g.DrawLine(p, rx, ry, rx + angLluvia*largo, ry + largo);
            }
        }

        private void DibujarNieveSketch(Graphics g, int w, int h)
        {
            Color tinta = ColorTinta(0.3f);
            int nCopos = w / 8;
            for (int i = 0; i < nCopos; i++)
            {
                float cx = _rnd.Next(w);
                float cy = _rnd.Next(h);
                float r  = 2 + (float)(_rnd.NextDouble() * 3);
                using (Pen p = new Pen(tinta, Grosor(0.5f)))
                {
                    g.DrawLine(p, cx-r, cy, cx+r, cy);
                    g.DrawLine(p, cx, cy-r, cx, cy+r);
                    g.DrawLine(p, cx-r*0.7f, cy-r*0.7f, cx+r*0.7f, cy+r*0.7f);
                    g.DrawLine(p, cx+r*0.7f, cy-r*0.7f, cx-r*0.7f, cy+r*0.7f);
                }
            }
        }

        private void DibujarHatching(Graphics g, int x, int y, int w, int h, int paso, Color tinta, float alfa)
        {
            Color hCol = Color.FromArgb(Math.Min(255,(int)(255*alfa)), tinta.R, tinta.G, tinta.B);
            using (Pen p = new Pen(hCol, Grosor(0.5f)))
            {
                for (int xi = x; xi < x + w; xi += paso)
                    g.DrawLine(p, xi, y, xi, y + h);
                for (int yi = y; yi < y + h; yi += paso * 2)
                    g.DrawLine(p, x, yi, x + w, yi);
            }
        }

        private void DibujarGrano(Graphics g, int w, int h)
        {
            int n = (int)(w * h * 0.002);
            for (int i = 0; i < n; i++)
            {
                int x    = _rnd.Next(w);
                int y    = _rnd.Next(h);
                int alfa = _rnd.Next(5, 18);
                using (SolidBrush br = new SolidBrush(Color.FromArgb(alfa, 80, 70, 60)))
                    g.FillRectangle(br, x, y, 1, 1);
            }
        }

        private void LineTremor(Graphics g, Pen p, PointF a, PointF b)
        {
            float dx = b.X - a.X, dy = b.Y - a.Y;
            float len = (float)Math.Sqrt(dx*dx + dy*dy);
            if (len < 2) { g.DrawLine(p, a, b); return; }
            int segs = Math.Max(2, (int)(len / 12));
            PointF prev = a;
            for (int i = 1; i <= segs; i++)
            {
                float t  = (float)i / segs;
                float jx = (float)(_rnd.NextDouble() * 2 - 1) * (len * 0.012f);
                float jy = (float)(_rnd.NextDouble() * 2 - 1) * (len * 0.012f);
                PointF next = new PointF(a.X + dx*t + jx, a.Y + dy*t + jy);
                g.DrawLine(p, prev, next);
                prev = next;
            }
        }

        private Color ColorTinta(float intensidad)
        {
            if (_ctx.EstiloLapiz == "lapicera")
            {
                // Tinta azul-negra real: oscura pero con ligero azul/violeta
                // Variación de saturación simulando tinta real (no perfectamente uniforme)
                float var = (float)(_rnd.NextDouble() * 0.08 - 0.04);
                int baseV  = (int)(8  + (1 - intensidad) * 28);
                int r = Math.Max(0, baseV + (int)(var * 40));
                int gv = Math.Max(0, baseV + (int)(var * 20));
                int b = Math.Max(0, Math.Min(255, baseV + 18 + (int)(var * 30)));
                return Color.FromArgb(r, gv, b);
            }
            else if (_ctx.EstiloLapiz == "carbon")
            {
                int v = (int)(10 + (1-intensidad) * 40);
                return Color.FromArgb(v, v, v);
            }
            else
            {
                int v = (int)(60 + (1-intensidad) * 100);
                return Color.FromArgb(v, v-5, v+5);
            }
        }

        private float Grosor(float base2)
        {
            if (_ctx.EstiloLapiz == "lapicera") return base2 * 1.5f;
            if (_ctx.EstiloLapiz == "carbon")   return base2 * 2.2f;
            return base2;
        }

        // Crosshatching diagonal real para lapicera (45° y 135°)
        private void CrossHatch(Graphics g, float x, float y, float w, float h,
                                 float densidad, float intensidad, bool doble = true)
        {
            if (_ctx.EstiloLapiz != "lapicera")
            {
                DibujarHatching(g, (int)x, (int)y, (int)w, (int)h, (int)(8/densidad), ColorTinta(intensidad), intensidad * 0.4f);
                return;
            }
            int paso = Math.Max(3, (int)(9 / densidad));
            Color tinta = ColorTinta(intensidad);
            // Líneas a 45°
            using (Pen p = new Pen(tinta, Grosor(0.55f)))
            {
                float diag = w + h;
                for (float d = -h; d < w; d += paso)
                {
                    float x1 = x + d, y1 = y;
                    float x2 = x + d + h, y2 = y + h;
                    // Clip a la región
                    if (x1 < x) { y1 += (x - x1); x1 = x; }
                    if (x2 > x+w) { y2 -= (x2 - (x+w)); x2 = x+w; }
                    if (y1 >= y + h || y2 <= y || x1 >= x+w || x2 <= x) continue;
                    // Microtemblor sutil para tinta
                    float jx = (float)(_rnd.NextDouble() * 0.6 - 0.3);
                    float jy = (float)(_rnd.NextDouble() * 0.6 - 0.3);
                    g.DrawLine(p, x1 + jx, y1 + jy, x2 + jx, y2 + jy);
                }
            }
            // Líneas a 135° (cruzadas) para sombras más densas
            if (doble && densidad > 0.7f)
            {
                using (Pen p = new Pen(ColorTinta(intensidad * 1.1f), Grosor(0.5f)))
                {
                    int paso2 = Math.Max(4, (int)(11 / densidad));
                    for (float d = 0; d < w + h; d += paso2)
                    {
                        float x1 = x + d, y1 = y;
                        float x2 = x + d - h, y2 = y + h;
                        if (x1 > x+w) { y1 += (x1-(x+w)); x1 = x+w; }
                        if (x2 < x)   { y2 -= (x - x2);   x2 = x;   }
                        if (y1 >= y+h || y2 <= y) continue;
                        float jx = (float)(_rnd.NextDouble() * 0.6 - 0.3);
                        g.DrawLine(p, x1+jx, y1, x2+jx, y2);
                    }
                }
            }
        }

        // Línea de tinta: más firme que el lápiz, presión variable
        private void LineInk(Graphics g, Pen p, PointF a, PointF b, bool firme = false)
        {
            if (_ctx.EstiloLapiz != "lapicera" || firme)
            {
                LineTremor(g, p, a, b);
                return;
            }
            float dx = b.X - a.X, dy = b.Y - a.Y;
            float len = (float)Math.Sqrt(dx*dx + dy*dy);
            if (len < 2) { g.DrawLine(p, a, b); return; }
            // Tinta: muchos más segmentos, temblor muy pequeño, pero presión variable
            int segs = Math.Max(3, (int)(len / 6));
            PointF prev = a;
            for (int i = 1; i <= segs; i++)
            {
                float t  = (float)i / segs;
                // Presión variable: más delgado al inicio y final del trazo
                float presion = (float)Math.Sin(t * Math.PI) * 0.4f + 0.6f;
                float jx = (float)(_rnd.NextDouble() * 1.0 - 0.5);
                float jy = (float)(_rnd.NextDouble() * 1.0 - 0.5);
                PointF next = new PointF(a.X + dx*t + jx, a.Y + dy*t + jy);
                using (Pen pVar = new Pen(p.Color, p.Width * presion))
                    g.DrawLine(pVar, prev, next);
                prev = next;
            }
        }

        private void DibujarVignetteInk(Graphics g, int w, int h)
        {
            // Viñeta sutil en bordes para lapicera — sensación de encuadre artístico
            int margen = (int)(Math.Min(w, h) * 0.06);
            for (int i = 0; i < margen; i++)
            {
                int alfa = (int)((1.0 - (double)i/margen) * 18);
                if (alfa <= 0) break;
                using (Pen p = new Pen(Color.FromArgb(alfa, 20, 20, 30), 1))
                {
                    g.DrawRectangle(p, i, i, w - i*2 - 1, h - i*2 - 1);
                }
            }
        }
    }

    // ═══════════════════════════════════════════════════

    public class PanelTags : Control
    {
        private List<string> _tags = new List<string>();
        private List<Color>  _cols = new List<Color>();

        private static readonly Color[] CHIPS = {
            Color.FromArgb(0,150,200), Color.FromArgb(180,60,200),
            Color.FromArgb(200,100,0), Color.FromArgb(0,160,100),
            Color.FromArgb(160,30,30), Color.FromArgb(80,80,200)
        };

        public PanelTags()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer|ControlStyles.ResizeRedraw|ControlStyles.UserPaint, true);
        }

        public void SetTags(IEnumerable<string> tags)
        {
            _tags.Clear(); _cols.Clear();
            int i = 0;
            foreach (string t in tags)
            {
                _tags.Add(t);
                _cols.Add(CHIPS[i%CHIPS.Length]);
                i++;
            }
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(Color.FromArgb(28,28,33));
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Font font = new Font("Segoe UI", 8f, FontStyle.Regular);
            int x=4, y=4, maxH=0;
            for (int i = 0; i < _tags.Count; i++)
            {
                SizeF sz = g.MeasureString(_tags[i], font);
                int w = (int)sz.Width+14, h = (int)sz.Height+8;
                if (x+w > Width-4 && x > 4) { x=4; y+=maxH+4; maxH=0; }
                Rectangle rc = new Rectangle(x, y, w, h);
                using (Brush br=new SolidBrush(Color.FromArgb(60,_cols[i].R,_cols[i].G,_cols[i].B)))
                    GfxExt.FillRounded(g, br, rc, 4);
                using (Pen pen=new Pen(Color.FromArgb(180,_cols[i].R,_cols[i].G,_cols[i].B),1))
                    GfxExt.DrawRounded(g, pen, rc, 4);
                g.DrawString(_tags[i], font, Brushes.White, x+7, y+4);
                x+=w+4; maxH=Math.Max(maxH,h);
            }
            font.Dispose();
        }
    }

    public static class GfxExt
    {
        public static void FillRounded(Graphics g, Brush br, Rectangle r, int rad)
        {
            using (var p = RoundedPath(r,rad)) g.FillPath(br, p);
        }
        public static void DrawRounded(Graphics g, Pen pen, Rectangle r, int rad)
        {
            using (var p = RoundedPath(r,rad)) g.DrawPath(pen, p);
        }
        private static GraphicsPath RoundedPath(Rectangle r, int rad)
        {
            var p = new GraphicsPath();
            p.AddArc(r.X,          r.Y,          rad*2, rad*2, 180, 90);
            p.AddArc(r.Right-rad*2,r.Y,          rad*2, rad*2, 270, 90);
            p.AddArc(r.Right-rad*2,r.Bottom-rad*2,rad*2,rad*2,   0, 90);
            p.AddArc(r.X,          r.Bottom-rad*2,rad*2,rad*2,  90, 90);
            p.CloseFigure();
            return p;
        }
    }

    // ═══════════════════════════════════════════════════

    public class VentanaPrincipal : Form
    {
        private TextBox         txtPrompt;
        private Button          btnGenerar, btnGuardar, btnAleatorio, btnLimpiar, btnCancelar;
        private PictureBox      canvas;
        private Label           lblTitulo, lblInstruccion, lblEstado, lblResumen, lblZoomVal;
        private PanelTags       panelTags;
        private ProgressBar     barraProgreso;
        private NumericUpDown   numSemilla;
        private CheckBox        chkSemillaFija;
        private Panel           panelIzq;
        private TrackBar        trackZoom;
        private Label           lblModoActual;
        private CheckBox[]      _chkAlgos;
        private Button          _btnAutoAlgo;
        private bool            _modoManualAlgo = false;

        private Bitmap          _imagenActual;
        private ContextoVisual  _ultimoCtx;
        private bool            _generando = false;
        private bool            _cancelar  = false;

        private static readonly Color BG_DARK  = Color.FromArgb(18,18,22);
        private static readonly Color BG_PANEL = Color.FromArgb(28,28,35);
        private static readonly Color BG_INPUT = Color.FromArgb(40,40,50);
        private static readonly Color ACENTO   = Color.FromArgb(0,190,240);
        private static readonly Color TEXTO    = Color.FromArgb(220,220,230);
        private static readonly Color GRIS     = Color.FromArgb(100,100,115);
        private static readonly Color VERDE    = Color.FromArgb(0,200,100);
        private static readonly Color NARANJA  = Color.FromArgb(230,140,0);

        public VentanaPrincipal()
        {
            IniciarInterfaz();
        }

        private void IniciarInterfaz()
        {
            Text           = "OpenCIP 2.0";
            Size           = new Size(1340, 860);
            MinimumSize    = new Size(900, 650);
            StartPosition  = FormStartPosition.CenterScreen;
            BackColor      = BG_DARK;
            ForeColor      = TEXTO;
            DoubleBuffered = true;

            panelIzq = new Panel
            {
                BackColor = BG_PANEL,
                Dock      = DockStyle.Left,
                Width     = 358,
                Padding   = new Padding(0)
            };

            var panelScroll = new Panel
            {
                Dock        = DockStyle.Fill,
                AutoScroll  = true,
                BackColor   = Color.Transparent,
                Padding     = new Padding(0),
                AutoScrollMinSize = new Size(318, 1060)
            };

            lblTitulo = new Label
            {
                Text      = "OpenCIP 2.0",
                Font      = new Font("Segoe UI", 22, FontStyle.Bold),
                ForeColor = ACENTO,
                Location  = new Point(18, 14),
                AutoSize  = true
            };

            var lblSub = new Label
            {
                Text      = "CPU Image Painter  ·  15 Escenas  ·  10 Entornos 3D  ·  Acuarela",
                Font      = new Font("Segoe UI", 7.5f),
                ForeColor = GRIS,
                Location  = new Point(16, 50),
                AutoSize  = true
            };

            var sep1 = Separador(20, 68, 320);

            lblInstruccion = new Label
            {
                Text      = "Prompt  (ES / EN)  —  «IA …» activa modo autónomo",
                Font      = new Font("Segoe UI", 8.5f),
                ForeColor = TEXTO,
                Location  = new Point(16, 78),
                AutoSize  = true
            };

            txtPrompt = new TextBox
            {
                Location    = new Point(20, 97),
                Size      = new Size(300, 76),
                Multiline   = true,
                BackColor   = BG_INPUT,
                ForeColor   = TEXTO,
                BorderStyle = BorderStyle.FixedSingle,
                Font        = new Font("Segoe UI", 10f),
                ScrollBars  = ScrollBars.Vertical,
                Text        = "IA lienzo sunset violeta oceano",
                Anchor      = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            txtPrompt.KeyDown += new KeyEventHandler(OnPromptKeyDown);

            var lblEj = new Label
            {
                Text      = "Ejemplos rápidos:",
                Font      = new Font("Segoe UI", 8.5f),
                ForeColor = GRIS,
                Location  = new Point(16, 182),
                AutoSize  = true
            };

            string[][] catNombres = new string[][] {
                new string[]{ "LIENZO",
                    "IA lienzo oceano",
                    "IA lienzo bosque niebla",
                    "IA lienzo flores primavera",
                    "IA lienzo montana nevada",
                    "IA lienzo desierto dunas",
                    "IA lienzo noche luna",
                    "IA lienzo selva tropical",
                    "IA lienzo tundra aurora",
                    "IA lienzo volcan oscuro",
                    "IA lienzo lago montaña",
                    "IA lienzo pantano niebla",
                    "IA lienzo valle verde rio",
                    "IA lienzo playa caribe",
                    "IA lienzo cañon rojizo",
                },
                new string[]{ "3D",
                    "terreno 3d verde",
                    "planeta 3d espacio oscuro",
                    "superficie alien 3d oscuro",
                    "cueva 3d neon oscuro",
                    "ciudad 3d oscuro neon",
                    "canon 3d rojizo",
                    "oceano 3d tranquilo",
                    "nebulosa 3d morado cyan",
                    "tormenta 3d rayos",
                    "esferas 3d metalico",
                },
                new string[]{ "ACUARELA",
                    "acuarela bosque verde",
                    "acuarela flores pastel",
                    "acuarela oceano azul",
                    "acuarela montana nieve",
                    "acuarela noche violeta",
                },
                new string[]{ "ALGORITMOS",
                    "IA fractal neon oscuro",
                    "IA nebulosa purpura",
                    "fractal julia ia caos",
                    "plasma oscuro ia neon",
                    "voronoi celular ia",
                    "fluido turbulento ia",
                    "onda interferencia ia",
                    "minecraft bloques verde",
                    "red neural cppn ia",
                    "geometrico simetrico ia",
                },
                new string[]{ "DIBUJO",
                    "lapiz montana atardecer",
                    "lapiz bosque niebla",
                    "lapiz oceano sin sol",
                    "lapiz ciudad noche",
                    "lapiz flores campo",
                    "lapicera montana nevada",
                    "lapicera bosque lluvia",
                    "lapicera ciudad oscuro",
                    "boceto desierto arido",
                    "carbon paisaje dramatico",
                    "sketch oceano islas",
                    "dibujo selva tropical",
                },
            };
            Color[] catColores = new Color[] {
                Color.FromArgb(0,180,120),
                Color.FromArgb(0,160,230),
                Color.FromArgb(220,120,0),
                Color.FromArgb(180,60,200),
                Color.FromArgb(120,90,50),
            };

            var panEjScroll = new Panel
            {
                Location    = new Point(20, 198),
                Size      = new Size(300, 270),
                AutoScroll  = true,
                BackColor   = Color.Transparent,
                Anchor      = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            int gyOffset = 0;
            int colW = 152, colGap = 4;

            for (int ci = 0; ci < catNombres.Length; ci++)
            {
                string[] catArr = catNombres[ci];
                var lblCat = new Label
                {
                    Text      = catArr[0],
                    Font      = new Font("Segoe UI", 7f, FontStyle.Bold),
                    ForeColor = catColores[ci],
                    Location  = new Point(0, gyOffset),
                    AutoSize  = true
                };
                panEjScroll.Controls.Add(lblCat);
                gyOffset += 18;

                int col = 0;
                foreach (string ej in catArr)
                {
                    if (ej == catArr[0]) continue;
                    string cap = ej;
                    int bx = col * (colW + colGap);
                    var btn = new Button
                    {
                        Text      = cap,
                        AutoSize  = false,
                        Size      = new Size(colW, 22),
                        Location  = new Point(bx, gyOffset),
                        BackColor = BG_INPUT,
                        ForeColor = GRIS,
                        FlatStyle = FlatStyle.Flat,
                        Font      = new Font("Segoe UI", 7f),
                        Cursor    = Cursors.Hand,
                        Tag       = cap,
                        TextAlign = ContentAlignment.MiddleLeft,
                        Padding   = new Padding(4, 0, 0, 0)
                    };
                    btn.FlatAppearance.BorderColor = Color.FromArgb(50, 50, 62);
                    btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(55, 55, 70);
                    btn.Click += new EventHandler(OnEjemploClick);
                    panEjScroll.Controls.Add(btn);

                    col++;
                    if (col >= 2) { col = 0; gyOffset += 24; }
                }
                if (col != 0) gyOffset += 24;
                gyOffset += 6;
            }

            var sep2 = Separador(20, 476, 320);

            var lblSem = new Label
            {
                Text      = "Semilla",
                Font      = new Font("Segoe UI", 9f),
                ForeColor = TEXTO,
                Location  = new Point(16, 484),
                AutoSize  = true
            };
            chkSemillaFija = new CheckBox
            {
                Text      = "Fijar",
                Location  = new Point(222, 482),
                Size      = new Size(60, 22),
                ForeColor = GRIS,
                BackColor = Color.Transparent,
                Font      = new Font("Segoe UI", 8.5f),
                Anchor    = AnchorStyles.Top | AnchorStyles.Right
            };
            numSemilla = new NumericUpDown
            {
                Location  = new Point(16, 502),
                Size      = new Size(218, 26),
                Minimum   = 0, Maximum = 999999, Value = 42,
                BackColor = BG_INPUT, ForeColor = TEXTO,
                Font      = new Font("Segoe UI", 10f),
                Anchor    = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            var lblZoom = new Label
            {
                Text      = "Escala:",
                Font      = new Font("Segoe UI", 9f),
                ForeColor = TEXTO,
                Location  = new Point(16, 538),
                AutoSize  = true
            };
            lblZoomVal = new Label
            {
                Text      = "1.0x",
                Font      = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = ACENTO,
                Location  = new Point(282, 538),
                AutoSize  = true,
                Anchor    = AnchorStyles.Top | AnchorStyles.Right
            };
            trackZoom = new TrackBar
            {
                Location      = new Point(16, 554),
                Size      = new Size(300, 34),
                Minimum       = 1, Maximum = 40, Value = 10,
                TickFrequency = 5,
                BackColor     = BG_PANEL,
                Anchor        = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            trackZoom.Scroll += new EventHandler(OnZoomScroll);

            // ── Panel de selección manual de algoritmos ───────────────────────
            var sep3 = Separador(20, 592, 320);

            var lblAlgoTit = new Label
            {
                Text      = "Algoritmo (manual – override del prompt):",
                Font      = new Font("Segoe UI", 8.5f),
                ForeColor = GRIS,
                Location  = new Point(16, 598),
                Size      = new Size(220, 18),
                AutoSize  = false
            };

            _btnAutoAlgo = new Button
            {
                Text      = "AUTO",
                Location  = new Point(246, 595),
                Size      = new Size(60, 20),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0,130,80),
                ForeColor = Color.White,
                Font      = new Font("Segoe UI", 7.5f, FontStyle.Bold)
            };
            _btnAutoAlgo.FlatAppearance.BorderSize = 0;
            _btnAutoAlgo.Click += new EventHandler(OnAutoAlgoClick);

            string[] algoNombres = new string[]{
                "Perlin", "Fractal", "Fluido", "Geométrico", "Voronoi",
                "Onda", "Nebulosa", "Plasma", "Voxel", "RedNeural",
                "3D", "Acuarela", "Lápiz"
            };
            _chkAlgos = new CheckBox[algoNombres.Length];

            var panAlgos = new Panel
            {
                Location    = new Point(16, 620),
                Size        = new Size(300, 110),
                BackColor   = Color.FromArgb(22, 22, 30),
                AutoScroll  = true,
                BorderStyle = BorderStyle.FixedSingle,
                AutoScrollMinSize = new Size(280, algoNombres.Length / 2 * 22 + 4)
            };

            for (int ai = 0; ai < algoNombres.Length; ai++)
            {
                var chk = new CheckBox
                {
                    Text      = algoNombres[ai],
                    Location  = new Point(4 + (ai % 2) * 144, 2 + (ai / 2) * 22),
                    Size      = new Size(140, 20),
                    ForeColor = Color.FromArgb(180, 180, 200),
                    BackColor = Color.Transparent,
                    Font      = new Font("Segoe UI", 8.5f),
                    Tag       = (AlgoritmoBase)ai
                };
                chk.CheckedChanged += new EventHandler(OnAlgoCheckChanged);
                _chkAlgos[ai] = chk;
                panAlgos.Controls.Add(chk);
            }
            // 110px panel height = suficiente para ~4 filas visibles, el resto con scroll
            int algoShift = 120;

            lblModoActual = new Label
            {
                Text      = "Modo: Estándar (paralelo)",
                Font      = new Font("Segoe UI", 8f, FontStyle.Italic),
                ForeColor = VERDE,
                Location  = new Point(16, 598 + algoShift),
                Size      = new Size(300, 18),
                Anchor    = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            btnGenerar = Boton("GENERAR  (Enter)", new Point(16, 620 + algoShift), new Size(300, 44),
                Color.FromArgb(0, 120, 170), Color.White, true);
            btnGenerar.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            btnGenerar.Click += new EventHandler(OnGenerarClick);

            btnCancelar = Boton("Cancelar", new Point(16, 780), new Size(300, 44),
                Color.FromArgb(150, 40, 40), Color.White, true);
            btnCancelar.Anchor  = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            btnCancelar.Visible = false;
            btnCancelar.Click  += new EventHandler(OnCancelarClick);

            btnAleatorio = Boton("Aleatorio", new Point(16, 834), new Size(144, 30),
                Color.FromArgb(50, 50, 65), TEXTO, false);
            btnAleatorio.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            btnAleatorio.Click += new EventHandler(OnAleatorioClick);

            btnLimpiar = Boton("Limpiar", new Point(172, 834), new Size(144, 30),
                Color.FromArgb(50, 50, 65), TEXTO, false);
            btnLimpiar.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnLimpiar.Click += new EventHandler(OnLimpiarClick);

            btnGuardar = Boton("Guardar  (PNG · JPG · BMP)", new Point(16, 874), new Size(300, 30),
                Color.FromArgb(40, 40, 55), GRIS, false);
            btnGuardar.Anchor  = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            btnGuardar.Enabled = false;
            btnGuardar.Click  += new EventHandler(OnGuardarClick);

            var sep4 = Separador(20, 912, 320);

            var lblTagsTit = new Label
            {
                Text      = "Palabras interpretadas:",
                Font      = new Font("Segoe UI", 8.5f),
                ForeColor = GRIS,
                Location  = new Point(16, 918),
                AutoSize  = true
            };
            panelTags = new PanelTags
            {
                Location = new Point(16, 935),
                Size      = new Size(300, 54),
                Anchor   = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };
            lblResumen = new Label
            {
                Text      = "",
                Font      = new Font("Segoe UI", 8f, FontStyle.Italic),
                ForeColor = Color.FromArgb(130, 130, 155),
                Location  = new Point(16, 992),
                Size      = new Size(300, 36),
                AutoSize  = false,
                Anchor    = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            var panEstado = new Panel
            {
                Dock      = DockStyle.Bottom,
                Height    = 26,
                BackColor = Color.FromArgb(22, 22, 28)
            };
            barraProgreso = new ProgressBar
            {
                Location  = new Point(0, 0),
                Size      = new Size(200, 26),
                Style     = ProgressBarStyle.Continuous,
                BackColor = Color.FromArgb(30, 30, 38)
            };
            lblEstado = new Label
            {
                Text      = "  Listo",
                Dock      = DockStyle.Fill,
                ForeColor = GRIS,
                Font      = new Font("Segoe UI", 8.5f),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding   = new Padding(8, 0, 0, 0)
            };
            panEstado.Controls.Add(lblEstado);
            panEstado.Controls.Add(barraProgreso);

            canvas = new PictureBox
            {
                BackColor = Color.FromArgb(12, 12, 16),
                SizeMode  = PictureBoxSizeMode.Zoom,
                Dock      = DockStyle.Fill
            };
            canvas.Paint += new PaintEventHandler(OnCanvasPaint);

            panelScroll.Controls.AddRange(new Control[]
            {
                lblTitulo, lblSub, sep1,
                lblInstruccion, txtPrompt, lblEj, panEjScroll,
                sep2, lblSem, chkSemillaFija, numSemilla,
                lblZoom, lblZoomVal, trackZoom,
                sep3, lblAlgoTit, _btnAutoAlgo, panAlgos,
                lblModoActual,
                btnGenerar, btnCancelar,
                btnAleatorio, btnLimpiar, btnGuardar,
                sep4, lblTagsTit, panelTags, lblResumen
            });

            panelIzq.Controls.Add(panelScroll);

            Controls.Add(canvas);
            Controls.Add(panelIzq);
            Controls.Add(panEstado);

            Resize += new EventHandler(OnFormResize);
        }


        private void OnFormResize(object sender, EventArgs e)
        {
        }

        private void OnAlgoCheckChanged(object sender, EventArgs e)
        {
            bool alguno = false;
            foreach (CheckBox chk in _chkAlgos)
                if (chk.Checked) { alguno = true; break; }
            _modoManualAlgo = alguno;
            _btnAutoAlgo.BackColor = alguno
                ? Color.FromArgb(170, 60, 0)
                : Color.FromArgb(0, 130, 80);
            _btnAutoAlgo.Text = alguno ? "MANUAL" : "AUTO";
        }

        private void OnAutoAlgoClick(object sender, EventArgs e)
        {
            foreach (CheckBox chk in _chkAlgos)
                chk.Checked = false;
            _modoManualAlgo = false;
            _btnAutoAlgo.BackColor = Color.FromArgb(0, 130, 80);
            _btnAutoAlgo.Text = "AUTO";
        }

        private void OnZoomScroll(object sender, EventArgs e)
        {
            lblZoomVal.Text = (trackZoom.Value/10.0).ToString("0.0") + "x";
        }

        private void OnEjemploClick(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            if (btn != null)
            {
                txtPrompt.Text = btn.Tag.ToString();
                IniciarGeneracion();
            }
        }

        private void OnPromptKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && !e.Shift)
            {
                e.SuppressKeyPress = true;
                IniciarGeneracion();
            }
        }

        private void OnGenerarClick(object sender, EventArgs e)
        {
            IniciarGeneracion();
        }

        private void OnCancelarClick(object sender, EventArgs e)
        {
            _cancelar = true;
            lblEstado.Text = "  Cancelando...";
        }

        private void OnAleatorioClick(object sender, EventArgs e)
        {
            string[] temas = new string[]
            {
                "IA lienzo sunset naranja dorado",
                "IA lienzo bosque verde niebla ia",
                "IA lienzo flores primavera pastel",
                "IA lienzo montana nevada oscuro",
                "IA lienzo desierto calido dunas",
                "IA lienzo noche luna estrellas",
                "IA lienzo oceano violeta ia",
                "acuarela flores rojo amarillo",
                "acuarela bosque verde azul ia",
                "acuarela nocturno violeta ia",
                "terreno 3d verde oscuro",
                "planeta 3d espacio oscuro neon",
                "cueva 3d neon oscuro cian",
                "ciudad 3d oscuro neon violeta",
                "IA fractal 3d neon oscuro",
                "IA espacio nebulosa purpura",
                "fractal julia ia neon caos",
                "plasma oscuro ia caos neon",
                "minecraft mundo bloques verde",
                "IA render 3d esfera metalico"
            };
            var rnd = new Random();
            txtPrompt.Text = temas[rnd.Next(temas.Length)];
            IniciarGeneracion();
        }

        private void OnLimpiarClick(object sender, EventArgs e)
        {
            txtPrompt.Text = "";
            txtPrompt.Focus();
        }

        private void OnGuardarClick(object sender, EventArgs e)
        {
            if (_imagenActual == null) return;
            using (SaveFileDialog dlg = new SaveFileDialog())
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

        private void OnCanvasPaint(object sender, PaintEventArgs e)
        {
            if (canvas.Image == null) DibujarPlaceholder(e.Graphics, canvas.ClientRectangle);
        }


        private async void IniciarGeneracion()
        {
            if (_generando) return;
            _generando = true;
            _cancelar  = false;

            string prompt  = txtPrompt.Text.Trim();
            int    semilla = chkSemillaFija.Checked
                ? (int)numSemilla.Value
                : new Random().Next(1, 999999);
            numSemilla.Value = semilla;

            _ultimoCtx        = InterpretadorPrompt.Interpretar(prompt, semilla);
            _ultimoCtx.Escala *= trackZoom.Value/10.0;

            // Override manual: si el usuario seleccionó algoritmos manualmente,
            // reemplazar completamente lo que decidió el intérprete.
            if (_modoManualAlgo)
            {
                var algosSeleccionados  = new List<AlgoritmoBase>();
                var pesosSeleccionados  = new List<float>();
                for (int ai = 0; ai < _chkAlgos.Length; ai++)
                {
                    if (_chkAlgos[ai].Checked)
                    {
                        algosSeleccionados.Add((AlgoritmoBase)ai);
                        pesosSeleccionados.Add(1.0f);
                    }
                }
                if (algosSeleccionados.Count > 0)
                {
                    _ultimoCtx.ModoLienzo    = false;
                    _ultimoCtx.Algoritmos    = algosSeleccionados;
                    _ultimoCtx.PesosAlgoritmos = pesosSeleccionados;
                    // Normalizar pesos
                    float totalP = 0;
                    foreach (float p in _ultimoCtx.PesosAlgoritmos) totalP += p;
                    for (int pi = 0; pi < _ultimoCtx.PesosAlgoritmos.Count; pi++)
                        _ultimoCtx.PesosAlgoritmos[pi] /= totalP;
                    // Modo progresivo solo si 3D o RedNeural seleccionados solos
                    bool solo3D  = algosSeleccionados.Count == 1 && algosSeleccionados[0] == AlgoritmoBase.Escena3D;
                    bool soloRN  = algosSeleccionados.Count == 1 && algosSeleccionados[0] == AlgoritmoBase.RedNeuralCPPN;
                    bool soloLapiz   = algosSeleccionados.Count == 1 && algosSeleccionados[0] == AlgoritmoBase.DibujoLapiz;
                    bool soloAcuarela = algosSeleccionados.Count == 1 && algosSeleccionados[0] == AlgoritmoBase.Acuarela;
                    _ultimoCtx.EsProgresivo = solo3D || soloRN;
                    if (solo3D) _ultimoCtx.EsProgresivo = true;
                    if (soloRN) _ultimoCtx.EsProgresivo = true;
                }
            }

            bool esProgresivo = _ultimoCtx.EsProgresivo;

            if (_ultimoCtx.ModoLienzo)
                lblModoActual.Text = "Modo: Lienzo IA – composición por capas" +
                    (_ultimoCtx.LienzoPostProceso ? " + post-proceso IA" : "");
            else if (_ultimoCtx.Algoritmos.Contains(AlgoritmoBase.Acuarela))
                lblModoActual.Text = "Modo: Acuarela – pinceles + papel texturizado";
            else if (_ultimoCtx.Algoritmos.Contains(AlgoritmoBase.DibujoLapiz))
                lblModoActual.Text = "Modo: Dibujo " + (_ultimoCtx.EstiloLapiz == "lapicera" ? "Lapicera – tinta + contornos" : _ultimoCtx.EstiloLapiz == "carbon" ? "Carbón – trazos gruesos oscuros" : "Lápiz – grafito + hatching");
            else if (_ultimoCtx.ModoIAInicio)
                lblModoActual.Text = "Modo: IA Autónoma – selección inteligente de algoritmos";
            else if (_ultimoCtx.ModoIAPostProceso)
                lblModoActual.Text = "Modo: Post-proceso IA – generación + refinado automático";
            else if (_ultimoCtx.Algoritmos.Contains(AlgoritmoBase.RedNeuralCPPN))
                lblModoActual.Text = "Modo: Red Neural CPPN 256 – pintura en tiempo real";
            else if (_ultimoCtx.Algoritmos.Contains(AlgoritmoBase.Escena3D))
                lblModoActual.Text = "Modo: Escena 3D Raytracer – scanline en tiempo real";
            else if (_ultimoCtx.Algoritmos.Contains(AlgoritmoBase.MundoVoxelMinecraft))
                lblModoActual.Text = "Modo: Mundo Voxel Minecraft";
            else
                lblModoActual.Text = "Modo: Estándar (paralelo CPU)";

            panelTags.SetTags(_ultimoCtx.PalabrasDetectadas.Take(20) as IEnumerable<string>
                ?? new List<string>());

            string resumen = _ultimoCtx.ResumenVisual.Trim();
            if (resumen.Length == 0) resumen = "patrón general";
            lblResumen.Text = "→ " + resumen;

            btnGenerar.Visible  = false;
            btnCancelar.Visible = true;
            btnGuardar.Enabled  = false;
            barraProgreso.Value = 0;

            string promptCorto = prompt.Length > 45 ? prompt.Substring(0,45)+"..." : prompt;
            lblEstado.Text = string.Format("  «{0}» – preparando...", promptCorto);

            var prog = new Progress<int>(delegate(int p) {
                if (IsDisposed) return;
                try {
                    Invoke(new Action(delegate() {
                        barraProgreso.Value = Math.Min(100,p);
                        lblEstado.Text = string.Format("  Generando... {0}%", p);
                    }));
                } catch { }
            });

            Action<Bitmap> actualizarCanvas = delegate(Bitmap bmp) {
                if (IsDisposed) return;
                try {
                    BeginInvoke(new Action(delegate() {
                        if (!IsDisposed)
                        {
                            canvas.Image = bmp;
                            canvas.Refresh();
                        }
                    }));
                } catch { }
            };

            try
            {
                int w = Math.Max(canvas.Width,  1280);
                int h = Math.Max(canvas.Height, 720);

                if (_ultimoCtx.Algoritmos.Contains(AlgoritmoBase.Escena3D))
                {
                    w = Math.Min(w, 1280);
                    h = Math.Min(h, 720);
                }
                else if (_ultimoCtx.Algoritmos.Contains(AlgoritmoBase.RedNeuralCPPN))
                {
                    w = Math.Min(w, 1280);
                    h = Math.Min(h, 720);
                }

                ContextoVisual ctx = _ultimoCtx;
                bool cancelRef     = false;

                if (esProgresivo)
                {
                    _imagenActual = await Task.Run(delegate() {
                        return MotorOpenCIP.RenderizarProgresivo(w, h, ctx, prog, actualizarCanvas, ref cancelRef);
                    });
                }
                else
                {
                    _imagenActual = await Task.Run(delegate() {
                        return MotorOpenCIP.Renderizar(w, h, ctx, prog);
                    });
                }

                canvas.Image = _imagenActual;

                if (_ultimoCtx.ModoIAPostProceso && _imagenActual != null)
                {
                    lblEstado.Text = "  Post-procesando con IA...";

                    Bitmap clonar = new Bitmap(_imagenActual);
                    canvas.Image = null;
                    canvas.Refresh();

                    Bitmap resultado = await Task.Run(delegate() {
                        return PostProcesadorIA.Aplicar(clonar, _ultimoCtx);
                    });

                    clonar.Dispose();
                    _imagenActual = resultado;
                    canvas.Image  = _imagenActual;
                }

                btnGuardar.Enabled= true;

                string algoStr = string.Join("+",
                    _ultimoCtx.Algoritmos
                        .Select(delegate(AlgoritmoBase a) { return a.ToString().Substring(0,Math.Min(6,a.ToString().Length)); })
                        .ToArray());

                lblEstado.Text = string.Format("  OK – {0}×{1} px  |  semilla:{2}  |  [{3}]",
                    w, h, semilla, algoStr);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al generar:\n"+ex.Message, "OpenCIP",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblEstado.Text = "  Error al generar";
            }
            finally
            {
                _generando          = false;
                _cancelar           = false;
                btnGenerar.Visible  = true;
                btnCancelar.Visible = false;
                barraProgreso.Value = 100;
            }
        }


        private void DibujarPlaceholder(Graphics g, Rectangle r)
        {
            g.SmoothingMode     = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
            g.Clear(Color.FromArgb(12, 12, 16));

            using (Pen pen = new Pen(Color.FromArgb(20, 20, 26), 1))
            {
                for (int x = 0; x < r.Width;  x += 44) g.DrawLine(pen, x, 0, x, r.Height);
                for (int y = 0; y < r.Height; y += 44) g.DrawLine(pen, 0, y, r.Width, y);
            }

            using (Font fTitle = new Font("Segoe UI", 48, FontStyle.Bold))
            using (Font fSub   = new Font("Segoe UI", 13, FontStyle.Regular))
            using (SolidBrush bTitle = new SolidBrush(Color.FromArgb(0, 190, 240)))
            using (SolidBrush bSub   = new SolidBrush(Color.FromArgb(55, 55, 70)))
            {
                string title = "OpenCIP 2.0";
                string sub   = "Escribe un prompt y presiona GENERAR";

                SizeF szT = g.MeasureString(title, fTitle);
                SizeF szS = g.MeasureString(sub,   fSub);

                float cx = r.Width  / 2f;
                float cy = r.Height / 2f;

                g.DrawString(title, fTitle, bTitle, cx - szT.Width/2f, cy - szT.Height/2f - 20);
                g.DrawString(sub,   fSub,   bSub,   cx - szS.Width/2f, cy + szT.Height/2f - 8);
            }
        }

        private Panel Separador(int x, int y, int w)
        {
            return new Panel { Location=new Point(x,y), Size=new Size(w,1), BackColor=Color.FromArgb(50,50,60) };
        }

        private Button Boton(string texto, Point loc, Size sz, Color bg, Color fg, bool bold)
        {
            var b = new Button
            {
                Text=      texto,
                Location=  loc,
                Size=      sz,
                BackColor= bg,
                ForeColor= fg,
                FlatStyle= FlatStyle.Flat,
                Font=      new Font("Segoe UI", bold?10.5f:9f, bold?FontStyle.Bold:FontStyle.Regular),
                Cursor=    Cursors.Hand
            };
            b.FlatAppearance.BorderSize = 0;
            b.FlatAppearance.MouseOverBackColor = Color.FromArgb(
                Math.Min(255,bg.R+25),
                Math.Min(255,bg.G+25),
                Math.Min(255,bg.B+25));
            return b;
        }
    }

    // ═══════════════════════════════════════════════════

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
}