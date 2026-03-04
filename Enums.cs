using System;

namespace OpenCIP
{
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
}
