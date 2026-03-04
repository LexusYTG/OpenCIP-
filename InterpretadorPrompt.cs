using System;
using System.Collections.Generic;
using System.Drawing;

namespace OpenCIP
{
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
}
