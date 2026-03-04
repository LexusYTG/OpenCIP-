using System;
using System.Collections.Generic;
using System.Drawing;

namespace OpenCIP
{
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
}
