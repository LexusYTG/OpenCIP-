using System;
using System.Collections.Generic;
using System.Drawing;

namespace OpenCIP
{
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
}
