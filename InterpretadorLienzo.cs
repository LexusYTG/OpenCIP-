using System;
using System.Collections.Generic;
using System.Drawing;

namespace OpenCIP
{
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
}
