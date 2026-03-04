using System;
using System.Collections.Generic;
using System.Drawing;

namespace OpenCIP
{
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
}
