using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace OpenCIP
{
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

        // ─── utilidades ─────────────────────────────────────────────────────────

        private Vec3 CV(Color c, double b = 1.0) =>
            new Vec3(c.R / 255.0 * b, c.G / 255.0 * b, c.B / 255.0 * b);

        private Vec3 Lerp(Vec3 a, Vec3 b, double t) => a * (1 - t) + b * t;

        /// Devuelve una cámara variada dentro de un abanico definido por los parámetros
        private void CamaraVariada(Vec3 basePosMin, Vec3 basePosMax,
                                    Vec3 lookatMin, Vec3 lookatMax)
        {
            double t = (_rnd.NextDouble() * 0.6 + 0.2); // evitar extremos
            _camPos    = Lerp(basePosMin, basePosMax, t);
            _camLookAt = Lerp(lookatMin,  lookatMax,  1 - t);
        }

        /// Estrella de fondo lejos en la esfera celeste
        private void Estrella(Vec3 dir, double radio, double br, Vec3 col)
        {
            var s = new EsferaRT { Centro = dir.Norm() * (55 + radio * 10), Radio = radio };
            s.Material = new MaterialRT { EsLuz = true, EmisionColor = col * br };
            _objetos.Add(s);
        }

        private void CieloEstrellado(int n, Random rndS, double brMin = 0.5, double brMax = 2.2)
        {
            for (int i = 0; i < n; i++)
            {
                double t2 = rndS.NextDouble() * Math.PI * 2;
                double p  = rndS.NextDouble() * Math.PI - Math.PI / 2;
                double r  = 55 + rndS.NextDouble() * 30;
                double br = brMin + rndS.NextDouble() * (brMax - brMin);
                double hue = rndS.NextDouble();
                Vec3 col = hue < 0.25 ? new Vec3(br * 0.80, br * 0.88, br)
                         : hue < 0.50 ? new Vec3(br, br, br)
                         : hue < 0.75 ? new Vec3(br, br * 0.93, br * 0.65)
                         :              new Vec3(br, br * 0.55, br * 0.45);
                var s = new EsferaRT {
                    Centro = new Vec3(Math.Cos(p) * Math.Cos(t2) * r,
                                      Math.Sin(p) * r,
                                      Math.Cos(p) * Math.Sin(t2) * r),
                    Radio = 0.12 + rndS.NextDouble() * 0.35
                };
                s.Material = new MaterialRT { EsLuz = true, EmisionColor = col };
                _objetos.Add(s);
            }
        }

        // ── árbol compuesto: tronco cilíndrico + copa multicapa ───────────────
        private void AgregarArbol(double x, double z, double escala,
                                   Vec3 colTronco, Vec3 colCopa)
        {
            double h = escala * (0.9 + _rnd.NextDouble() * 0.4);
            double r = escala * 0.11;

            // tronco
            var tronco = new CilindroRT {
                Base   = new Vec3(x, 0,       z),
                Radio  = r,
                Altura = h * 0.55
            };
            tronco.Material = new MaterialRT {
                Albedo    = colTronco,
                Especular = 0.02,
                Rugosidad = 0.98
            };
            _objetos.Add(tronco);

            // copa: 3 esferas escalonadas para dar volumen, no una sola pelota
            double[] capas = { 0.55, 0.75, 0.90 };
            double[] radios = { escala * 0.55, escala * 0.45, escala * 0.28 };
            for (int k = 0; k < 3; k++)
            {
                double jx = (_rnd.NextDouble() - 0.5) * r * 2;
                double jz = (_rnd.NextDouble() - 0.5) * r * 2;
                var copa = new EsferaRT {
                    Centro = new Vec3(x + jx, h * capas[k], z + jz),
                    Radio  = radios[k] * (0.85 + _rnd.NextDouble() * 0.3)
                };
                copa.Material = new MaterialRT {
                    Albedo    = colCopa * (0.75 + k * 0.1),
                    Especular = 0.04,
                    Rugosidad = 0.92
                };
                _objetos.Add(copa);
            }
        }

        // ── roca aleatoria: caja ligeramente no-cuadrada + esfera encima ──────
        private void AgregarRoca(double x, double y, double z,
                                  double escala, Vec3 col)
        {
            double lx = escala * (0.6 + _rnd.NextDouble() * 0.8);
            double ly = escala * (0.4 + _rnd.NextDouble() * 0.5);
            double lz = escala * (0.6 + _rnd.NextDouble() * 0.7);
            var base_ = new CajaRT {
                Min = new Vec3(x - lx, y,        z - lz),
                Max = new Vec3(x + lx, y + ly,   z + lz)
            };
            base_.Material = new MaterialRT {
                Albedo    = col * (0.7 + _rnd.NextDouble() * 0.25),
                Especular = 0.05,
                Rugosidad = 0.97
            };
            _objetos.Add(base_);
            // remate redondeado
            var tope = new EsferaRT {
                Centro = new Vec3(x + (_rnd.NextDouble() - 0.5) * lx * 0.5,
                                  y + ly * 0.7,
                                  z + (_rnd.NextDouble() - 0.5) * lz * 0.5),
                Radio  = Math.Min(lx, lz) * 0.65
            };
            tope.Material = base_.Material;
            _objetos.Add(tope);
        }

        // ── nube coherente: esfera madre + satélites adheridos ───────────────
        //   Resultado: masa nubosa orgánica, no "sopa de esferas"
        private void AgregarNube(double cx, double cy, double cz,
                                  double radio, Vec3 col)
        {
            // cuerpo central alargado (2 esferas solapadas)
            double stretch = 1.6 + _rnd.NextDouble() * 1.0;
            for (int k = 0; k < 2; k++)
            {
                double ox = (k - 0.5) * radio * stretch * 0.55;
                var s = new EsferaRT {
                    Centro = new Vec3(cx + ox, cy, cz),
                    Radio  = radio * (1.0 - k * 0.2)
                };
                s.Material = new MaterialRT {
                    Albedo    = col * (0.90 + k * 0.05),
                    Especular = 0.01,
                    Rugosidad = 1.0
                };
                _objetos.Add(s);
            }
            // protuberancias menores en borde superior → silueta orgánica
            int bumps = 3 + _rnd.Next(3);
            for (int b = 0; b < bumps; b++)
            {
                double ang = b * Math.PI * 2 / bumps + _rnd.NextDouble() * 0.8;
                double br2 = radio * (0.4 + _rnd.NextDouble() * 0.35);
                double bx  = cx + Math.Cos(ang) * radio * stretch * 0.45;
                double by  = cy + radio * (0.3 + _rnd.NextDouble() * 0.45);
                double bz  = cz + Math.Sin(ang) * radio * 0.25;
                var bump = new EsferaRT {
                    Centro = new Vec3(bx, by, bz),
                    Radio  = br2
                };
                bump.Material = new MaterialRT {
                    Albedo    = col * 0.95,
                    Especular = 0.01,
                    Rugosidad = 1.0
                };
                _objetos.Add(bump);
            }
        }

        // ─── despacho de escenas ────────────────────────────────────────────────

        private void ConstruirEscena()
        {
            switch (_ctx.Entorno3D)
            {
                case TipoEntorno3D.Terreno:              ConstruirTerreno();              return;
                case TipoEntorno3D.Planeta:              ConstruirPlaneta();              return;
                case TipoEntorno3D.Cueva:                ConstruirCueva();                return;
                case TipoEntorno3D.Ciudad:               ConstruirCiudad();               return;
                case TipoEntorno3D.SuperficiePlanetaria: ConstruirSuperficiePlanetaria(); return;
                case TipoEntorno3D.Nebulosa3D:           ConstruirNebulosa3D();           return;
                case TipoEntorno3D.Canon:                ConstruirCanon3D();              return;
                case TipoEntorno3D.Tormenta:             ConstruirTormenta3D();           return;
                case TipoEntorno3D.Oceano3D:             ConstruirOceano3D();             return;
                default:                                 ConstruirEsferas();              return;
            }
        }

        // ═══════════════════════════════════════════════════════════════════════
        // ESCENA: Esferas (galería de materiales)
        // ═══════════════════════════════════════════════════════════════════════
        private void ConstruirEsferas()
        {
            List<Color> paleta = _ctx.Paleta.Count > 0 ? _ctx.Paleta
                : new List<Color> { Color.CornflowerBlue, Color.Coral, Color.Gold,
                                    Color.MediumSeaGreen, Color.Orchid };

            _cielo1 = _ctx.ModoOscuro ? new Vec3(0.01, 0.01, 0.05) : new Vec3(0.5, 0.7, 1.0);
            _cielo2 = _ctx.ModoOscuro ? new Vec3(0.0,  0.0,  0.0)  : new Vec3(0.1, 0.3, 0.6);

            // Cámara variada: ligeramente elevada, con algo de lateral
            double camAng = (_rnd.NextDouble() - 0.5) * 0.6;
            double camDist = 10 + _rnd.NextDouble() * 4;
            _camPos    = new Vec3(Math.Sin(camAng) * camDist, 3 + _rnd.NextDouble() * 2, -camDist);
            _camLookAt = new Vec3(0, 1, 0);

            // Suelo — checker solo si modo claro
            Color colPiso1 = _ctx.ModoOscuro ? Color.FromArgb(20, 20, 20) : Color.White;
            Color colPiso2 = _ctx.ModoOscuro ? Color.FromArgb(40, 40, 60) : Color.LightGray;
            var piso = new PlanoRT { Normal = new Vec3(0, 1, 0), D = 0,
                                     Albedo2 = CV(colPiso2, 0.8) };
            piso.Material = new MaterialRT {
                Albedo    = CV(colPiso1, 0.9),
                Especular = 0.1,
                Reflexion = _ctx.ModoOscuro ? 0.3 : 0.15,
                Rugosidad = 0.8
            };
            _objetos.Add(piso);

            int numEsferas = Math.Min(4 + (int)(_ctx.Complejidad * 2) + (_ctx.ModoCaos ? 3 : 0), 10);
            double[] xPos  = { -4, 0, 4, -2, 2, -5, 5, -1, 1, 3 };
            double[] radios = { 1.2, 1.5, 1.2, 0.9, 0.9, 0.7, 0.7, 0.6, 0.8, 1.0 };

            for (int i = 0; i < numEsferas; i++)
            {
                Color colBase = paleta[i % paleta.Count];
                bool esEspejo = (i % 5 == 1);
                bool esMetal  = (i % 4 == 2);
                double yr = radios[i % radios.Length];
                double xr = xPos[i % xPos.Length] + (_rnd.NextDouble() - 0.5) * 0.5;
                double zr = (_rnd.NextDouble() - 0.5) * 3;

                var e = new EsferaRT { Centro = new Vec3(xr, yr, zr), Radio = yr };
                e.Material = new MaterialRT {
                    Albedo    = CV(colBase, _ctx.Intensidad),
                    Especular = esEspejo ? 0.95 : (esMetal ? 0.7 : 0.1),
                    Reflexion = esEspejo ? 0.9  : (esMetal ? 0.5 : 0.05),
                    Rugosidad = esEspejo ? 0.02 : (esMetal ? 0.1 : 0.8),
                    EsEspejo  = esEspejo
                };
                _objetos.Add(e);
            }

            if (_ctx.ModoOscuro)
            {
                for (int i = 0; i < 3; i++)
                {
                    Color cL = paleta[i % paleta.Count];
                    double ang = i * Math.PI * 2 / 3;
                    var eLuz = new EsferaRT {
                        Centro = new Vec3(Math.Cos(ang) * 5, 3, Math.Sin(ang) * 5),
                        Radio  = 0.5
                    };
                    eLuz.Material = new MaterialRT {
                        EsLuz = true, EmisionColor = CV(cL, 4.0), Albedo = CV(cL, 1.0)
                    };
                    _objetos.Add(eLuz);
                }
            }

            Vec3 cLP = _ctx.ModoOscuro ? new Vec3(0.3, 0.4, 0.6) : new Vec3(1.0, 0.95, 0.9);
            _luces.Add(new LuzRT { Pos = new Vec3(5, 10, -5),  Color = cLP,                    Intensidad = 1.0 });
            _luces.Add(new LuzRT { Pos = new Vec3(-8, 6, 3),   Color = new Vec3(0.5, 0.5, 1.0), Intensidad = 0.4 });
        }

        // ═══════════════════════════════════════════════════════════════════════
        // ESCENA: Terreno — vegetación, rocas, variedad de cámara
        // ═══════════════════════════════════════════════════════════════════════
        private void ConstruirTerreno()
        {
            List<Color> pal = _ctx.Paleta.Count > 0 ? _ctx.Paleta
                : new List<Color> { Color.ForestGreen, Color.RosyBrown, Color.Snow, Color.SteelBlue };

            bool esDia = !_ctx.ModoOscuro;
            _cielo1 = esDia ? new Vec3(0.45, 0.62, 0.85) : new Vec3(0.02, 0.05, 0.12);
            _cielo2 = esDia ? new Vec3(0.15, 0.30, 0.55) : new Vec3(0.0,  0.0,  0.02);

            // Cámara variada: posición lateral y altura distintas cada seed
            double camX  = (_rnd.NextDouble() - 0.5) * 8;
            double camY  = 2.5 + _rnd.NextDouble() * 3.5;
            double camZ  = -10 - _rnd.NextDouble() * 4;
            double lookZ =  8  + _rnd.NextDouble() * 6;
            _camPos    = new Vec3(camX, camY, camZ);
            _camLookAt = new Vec3(camX * 0.3, 1.5 + _rnd.NextDouble(), lookZ);

            Matematica.InicializarSemilla(_ctx.Semilla);
            var terreno = new TerrenoRM {
                Escala   = 0.14 + _ctx.Complejidad * 0.06,
                Amplitud = 3.5  + _ctx.Intensidad  * 2.0,
                YOffset  = 0.0,
                Semilla  = _ctx.Semilla,
                ColorBajo  = CV(pal.Count > 0 ? pal[0] : Color.ForestGreen, 0.85),
                ColorMedio = CV(pal.Count > 1 ? pal[1] : Color.RosyBrown,   0.80),
                ColorAlto  = CV(pal.Count > 2 ? pal[2] : Color.Snow,        0.95),
            };
            terreno.Material = new MaterialRT { Especular = 0.04, Rugosidad = 0.96 };
            _objetos.Add(terreno);

            // Agua solo en zonas bajas — plano ligeramente bajo el nivel 0
            var agua = new PlanoRT { Normal = new Vec3(0, 1, 0), D = -0.3,
                                     Albedo2 = new Vec3(0.08, 0.22, 0.55) };
            agua.Material = new MaterialRT {
                Albedo    = new Vec3(0.07, 0.20, 0.52),
                Especular = 0.80,
                Reflexion = 0.60,
                Rugosidad = 0.03
            };
            _objetos.Add(agua);

            // Árboles distribuidos orgánicamente
            Vec3 cTronco = new Vec3(0.32, 0.20, 0.10);
            Vec3 cCopa   = CV(pal[0], 0.70);
            int nArboles = 8 + (int)(_ctx.Complejidad * 5);
            var rndA = new Random(_ctx.Semilla ^ 0xBEEF);
            for (int i = 0; i < nArboles; i++)
            {
                double tx = (rndA.NextDouble() - 0.5) * 20;
                double tz =  rndA.NextDouble() * 18 + 2;
                double sc = 0.8 + rndA.NextDouble() * 1.0;
                AgregarArbol(tx, tz, sc, cTronco, cCopa);
            }

            // Rocas sueltas
            var rndR = new Random(_ctx.Semilla ^ 0xCAFE);
            for (int i = 0; i < 12; i++)
            {
                double rx = (rndR.NextDouble() - 0.5) * 22;
                double rz =  rndR.NextDouble() * 20;
                AgregarRoca(rx, 0, rz, 0.3 + rndR.NextDouble() * 0.6,
                            CV(pal.Count > 1 ? pal[1] : Color.RosyBrown, 0.6));
            }

            // Noche: estrellas
            if (!esDia)
            {
                var rndS = new Random(_ctx.Semilla ^ 0xABCD);
                CieloEstrellado(20, rndS);
            }
            else
            {
                // Nubes en el cielo (3-5 nubes bien colocadas)
                int nNubes = 3 + _rnd.Next(3);
                for (int n = 0; n < nNubes; n++)
                {
                    double nx = (_rnd.NextDouble() - 0.5) * 30;
                    double nz =  10 + _rnd.NextDouble() * 25;
                    double ny =  8  + _rnd.NextDouble() * 4;
                    double nr =  2  + _rnd.NextDouble() * 2;
                    AgregarNube(nx, ny, nz, nr, new Vec3(0.92, 0.93, 0.95));
                }
            }

            Vec3 luzCol = esDia ? new Vec3(1.0, 0.95, 0.85) : new Vec3(0.15, 0.20, 0.40);
            _luces.Add(new LuzRT { Pos = new Vec3(8, 20, -8),  Color = luzCol,                Intensidad = esDia ? 1.2 : 0.35 });
            _luces.Add(new LuzRT { Pos = new Vec3(-10, 8, 5), Color = new Vec3(0.4, 0.5, 0.8), Intensidad = 0.25 });
        }

        // ═══════════════════════════════════════════════════════════════════════
        // ESCENA: Planeta desde el espacio
        // ═══════════════════════════════════════════════════════════════════════
        private void ConstruirPlaneta()
        {
            List<Color> pal = _ctx.Paleta.Count > 0 ? _ctx.Paleta
                : new List<Color> { Color.FromArgb(30, 100, 200), Color.FromArgb(60, 160, 60),
                                    Color.FromArgb(220, 200, 140), Color.FloralWhite };

            _cielo1 = Vec3.Zero;
            _cielo2 = Vec3.Zero;

            // Cámara: ángulo de vista variado — no siempre frontal
            double camAng = (_rnd.NextDouble() - 0.5) * 1.2;
            double camEl  = (_rnd.NextDouble() - 0.3) * 0.6;
            double camD   = 18 + _rnd.NextDouble() * 6;
            _camPos    = new Vec3(Math.Sin(camAng) * camD, Math.Sin(camEl) * camD * 0.5, -Math.Cos(camAng) * camD);
            _camLookAt = new Vec3(0, 0, 0);

            var planeta = new EsferaPlanetaRT {
                Centro    = new Vec3(0, 0, 0),
                Radio     = 6.0,
                ColAgua   = CV(pal[0], 1.0),
                ColTierra = CV(pal.Count > 1 ? pal[1] : Color.ForestGreen, 1.0),
                ColArena  = CV(pal.Count > 2 ? pal[2] : Color.SandyBrown,  0.9),
                ColNieve  = new Vec3(0.95, 0.97, 1.0),
                Semilla   = _ctx.Semilla
            };
            planeta.Material = new MaterialRT { Especular = 0.12, Rugosidad = 0.8 };
            _objetos.Add(planeta);

            // Capa de atmósfera tenue (emisiva muy suave)
            var atmos = new EsferaRT { Centro = Vec3.Zero, Radio = 6.55 };
            Color cAtm = pal.Count > 3 ? pal[3] : Color.CornflowerBlue;
            atmos.Material = new MaterialRT { EsLuz = true, EmisionColor = CV(cAtm, 0.07), Albedo = Vec3.Zero };
            _objetos.Add(atmos);

            // Luna con superficie rugosa
            var luna = new EsferaLunaRT { Centro = new Vec3(10, 3, 10), Radio = 1.5, Semilla = _ctx.Semilla ^ 0xFF };
            luna.Material = new MaterialRT { Especular = 0.04, Rugosidad = 0.96 };
            _objetos.Add(luna);

            // Anillos opcionales (Caos o Modo oscuro)
            if (_ctx.ModoCaos || _ctx.ModoOscuro)
            {
                int nSeg = 60;
                for (int i = 0; i < nSeg; i++)
                {
                    double ang = i * Math.PI * 2 / nSeg;
                    double r2  = 8.5 + Math.Sin(i * 7.3) * 0.4;
                    double x   = Math.Cos(ang) * r2;
                    double z   = Math.Sin(ang) * r2;
                    var seg = new CajaRT {
                        Min = new Vec3(x - 0.22, -0.05, z - 0.22),
                        Max = new Vec3(x + 0.22,  0.05, z + 0.22)
                    };
                    double br = 0.5 + 0.15 * Matematica.Perlin(i * 0.2 + _ctx.Semilla * 0.01, i * 0.15);
                    seg.Material = new MaterialRT {
                        Albedo    = new Vec3(br, br * 0.88, br * 0.68),
                        Especular = 0.15,
                        Reflexion = 0.1
                    };
                    _objetos.Add(seg);
                }
            }

            // Cielo estrellado denso
            var rndS = new Random(_ctx.Semilla ^ 0x1234);
            CieloEstrellado(100, rndS, 0.5, 2.0);

            _luces.Add(new LuzRT { Pos = new Vec3(30, 15, -20), Color = new Vec3(1.0, 0.97, 0.90), Intensidad = 1.5 });
            _luces.Add(new LuzRT { Pos = new Vec3(-25, 5,  10), Color = new Vec3(0.06, 0.08, 0.28), Intensidad = 0.12 });
        }

        // ═══════════════════════════════════════════════════════════════════════
        // ESCENA: Cueva — luz de antorcha, estalactitas, charco
        // ═══════════════════════════════════════════════════════════════════════
        private void ConstruirCueva()
        {
            List<Color> pal = _ctx.Paleta.Count > 0 ? _ctx.Paleta
                : new List<Color> { Color.FromArgb(80, 60, 40), Color.FromArgb(60, 40, 25),
                                    Color.FromArgb(40, 80, 120) };

            _cielo1 = Vec3.Zero;
            _cielo2 = Vec3.Zero;

            // Cámara: pequeña variación de altura y lateral
            double cX = (_rnd.NextDouble() - 0.5) * 1.0;
            double cY = 1.8 + _rnd.NextDouble() * 1.4;
            _camPos    = new Vec3(cX, cY, -2);
            _camLookAt = new Vec3(cX * 0.3, cY + 0.5, 12);

            Matematica.InicializarSemilla(_ctx.Semilla);
            var cueva = new CuevaRM {
                RadioBase  = 3.5 + _ctx.Complejidad * 1.5,
                Semilla    = _ctx.Semilla,
                ColorPared = CV(pal[0],               0.75),
                ColorTecho = CV(pal.Count > 1 ? pal[1] : pal[0], 0.65),
                ColorSuelo = CV(pal.Count > 2 ? pal[2] : pal[0], 0.70),
            };
            cueva.Material = new MaterialRT { Especular = 0.06, Rugosidad = 0.94 };
            _objetos.Add(cueva);

            // Suelo de roca
            var suelo = new PlanoRT { Normal = new Vec3(0, 1, 0), D = 0, Albedo2 = CV(pal[0], 0.5) };
            suelo.Material = new MaterialRT {
                Albedo    = CV(pal[0], 0.6),
                Reflexion = 0.08,
                Especular = 0.15,
                Rugosidad = 0.9
            };
            _objetos.Add(suelo);

            // Charco reflectante localizado (no cubre toda la cueva)
            var charco = new PlanoRT { Normal = new Vec3(0, 1, 0), D = -0.04, Albedo2 = new Vec3(0.04, 0.09, 0.22) };
            charco.Material = new MaterialRT {
                Albedo = new Vec3(0.04, 0.10, 0.25), Reflexion = 0.60, Especular = 0.70, Rugosidad = 0.02
            };
            _objetos.Add(charco);

            // Estalactitas: cilindros delgados colgando del techo, variados
            var rndE = new Random(_ctx.Semilla ^ 0xD00D);
            for (int i = 0; i < 12; i++)
            {
                double ex = (rndE.NextDouble() - 0.5) * 5;
                double ez =  2 + rndE.NextDouble() * 10;
                double eal = 0.5 + rndE.NextDouble() * 1.8;
                double er  = 0.04 + rndE.NextDouble() * 0.08;
                double techoY = 3.0 + Matematica.Perlin(ex * 0.3, ez * 0.3) * 1.5;
                var est = new CilindroRT {
                    Base   = new Vec3(ex, techoY - eal, ez),
                    Radio  = er,
                    Altura = eal
                };
                est.Material = new MaterialRT {
                    Albedo    = CV(pal.Count > 1 ? pal[1] : pal[0], 0.55 + rndE.NextDouble() * 0.2),
                    Especular = 0.08,
                    Rugosidad = 0.93
                };
                _objetos.Add(est);
            }

            // Antorchas con cono de luz cálida
            Color[] colLuces = { Color.OrangeRed, Color.Coral, Color.Gold, Color.DodgerBlue };
            for (int i = 0; i < 4; i++)
            {
                double ang = i * Math.PI * 2 / 4 + 0.4;
                double zL  = 3 + i * 3.5;
                double xL  = Math.Sin(ang + i * 0.6) * 2.2;
                double yL  = 1.2 + _rnd.NextDouble() * 0.4;

                _luces.Add(new LuzRT {
                    Pos        = new Vec3(xL, yL, zL),
                    Color      = CV(colLuces[i], 1.0),
                    Intensidad = 1.2 + _ctx.Intensidad * 0.4
                });
                var eLuz = new EsferaRT { Centro = new Vec3(xL, yL, zL), Radio = 0.14 };
                eLuz.Material = new MaterialRT {
                    EsLuz = true, EmisionColor = CV(colLuces[i], 4.0)
                };
                _objetos.Add(eLuz);

                // mástil de la antorcha
                var mastil = new CilindroRT {
                    Base   = new Vec3(xL, 0, zL),
                    Radio  = 0.04,
                    Altura = yL - 0.1
                };
                mastil.Material = new MaterialRT {
                    Albedo = new Vec3(0.25, 0.16, 0.08), Especular = 0.03, Rugosidad = 0.98
                };
                _objetos.Add(mastil);
            }

            // Luz ambiental mínima de fondo
            _luces.Add(new LuzRT { Pos = new Vec3(0, 6, 6), Color = new Vec3(0.08, 0.07, 0.06), Intensidad = 0.15 });
        }

        // ═══════════════════════════════════════════════════════════════════════
        // ESCENA: Ciudad — rascacielos con retranqueos, calles, vida nocturna
        // ═══════════════════════════════════════════════════════════════════════
        private void ConstruirCiudad()
        {
            List<Color> pal = _ctx.Paleta.Count > 0 ? _ctx.Paleta
                : new List<Color> { Color.FromArgb(30, 40, 60), Color.FromArgb(0, 180, 240),
                                    Color.FromArgb(255, 100, 0), Color.FromArgb(80, 90, 130) };

            _cielo1 = _ctx.ModoOscuro ? new Vec3(0.01, 0.02, 0.06) : new Vec3(0.22, 0.40, 0.72);
            _cielo2 = _ctx.ModoOscuro ? new Vec3(0.0,  0.0,  0.01) : new Vec3(0.55, 0.72, 0.90);

            // Cámara: variada en altura y ángulo lateral
            double cAng  = (_rnd.NextDouble() - 0.5) * 0.8;
            double cDist = 15 + _rnd.NextDouble() * 6;
            double cElev = 4  + _rnd.NextDouble() * 6;
            _camPos    = new Vec3(Math.Sin(cAng) * cDist, cElev, -Math.Cos(cAng) * cDist);
            _camLookAt = new Vec3(0, cElev * 0.6, 4);

            // Suelo — asfalto con reflejo húmedo en noche
            var piso = new PlanoRT { Normal = new Vec3(0, 1, 0), D = 0, Albedo2 = CV(pal[0], 0.25) };
            piso.Material = new MaterialRT {
                Albedo    = CV(pal[0], 0.35),
                Reflexion = _ctx.ModoOscuro ? 0.55 : 0.20,
                Especular = _ctx.ModoOscuro ? 0.80 : 0.30,
                Rugosidad = _ctx.ModoOscuro ? 0.05 : 0.35
            };
            _objetos.Add(piso);

            // Grid de edificios
            int[] gridX = { -10, -6, -2, 2, 6, 10, -8, -4, 0, 4, 8 };
            int[] gridZ = { 0, 4, 8, 12, 16 };

            foreach (int gx in gridX)
            foreach (int gz in gridZ)
            {
                ConstruirEdificio(gx, gz, pal);
            }

            // Estrellas nocturnas
            if (_ctx.ModoOscuro)
            {
                CieloEstrellado(25, new Random(_ctx.Semilla ^ 0xC17A));

                // Luces de calle en intervalos
                for (int i = 0; i < 10; i++)
                {
                    double lx = (_rnd.NextDouble() - 0.5) * 24;
                    double lz =  _rnd.NextDouble() * 16 + 1;
                    _luces.Add(new LuzRT { Pos = new Vec3(lx, 5, lz),
                                           Color = new Vec3(1.0, 0.82, 0.48), Intensidad = 0.55 });
                }
            }
            else
            {
                // Nubes sobre la ciudad
                for (int n = 0; n < 4; n++)
                {
                    double nx = (_rnd.NextDouble() - 0.5) * 35;
                    double nz = 10 + _rnd.NextDouble() * 20;
                    AgregarNube(nx, 28 + _rnd.NextDouble() * 8, nz, 3 + _rnd.NextDouble() * 2,
                                new Vec3(0.88, 0.90, 0.94));
                }
            }

            Vec3 luzP = _ctx.ModoOscuro ? new Vec3(0.10, 0.14, 0.35) : new Vec3(0.90, 0.92, 1.0);
            _luces.Add(new LuzRT { Pos = new Vec3(0, 45, -20), Color = luzP, Intensidad = _ctx.ModoOscuro ? 0.25 : 1.0 });
            _luces.Add(new LuzRT { Pos = new Vec3(-20, 15, 5), Color = new Vec3(0.3, 0.5, 1.0), Intensidad = 0.35 });
        }

        /// Construye un edificio con hasta 4 cuerpos escalonados + detalles
        private void ConstruirEdificio(double cx, double cz, List<Color> pal)
        {
            double hTotal = 5 + _rnd.NextDouble() * 20;
            double bw     = 1.0 + _rnd.NextDouble() * 1.3;  // half-width base
            double bd     = 1.0 + _rnd.NextDouble() * 1.3;  // half-depth base
            Color  cEdif  = pal[_rnd.Next(pal.Count)];
            bool   esVid  = _rnd.NextDouble() < 0.4;        // fachada vidriada

            // Número de retranqueos según altura
            int nCuerpos = hTotal > 12 ? (hTotal > 18 ? 4 : 3) : 2;
            double yAcum = 0;

            for (int c = 0; c < nCuerpos; c++)
            {
                double fracH = c == 0 ? 0.50 : (c == 1 ? 0.28 : (c == 2 ? 0.15 : 0.07));
                double ch    = hTotal * fracH;
                double shrink = c * 0.22;
                double fw = bw * (1 - shrink);
                double fd = bd * (1 - shrink);

                // pequeño offset lateral para romper la monotonía
                double offX = c > 0 ? (_rnd.NextDouble() - 0.5) * bw * 0.3 : 0;
                double offZ = c > 0 ? (_rnd.NextDouble() - 0.5) * bd * 0.3 : 0;

                var cuerpo = new CajaRT {
                    Min = new Vec3(cx - fw + offX, yAcum,      cz - fd + offZ),
                    Max = new Vec3(cx + fw + offX, yAcum + ch, cz + fd + offZ)
                };
                // cada cuerpo puede ser de material distinto
                bool vidActual = esVid && (c % 2 == 0);
                cuerpo.Material = new MaterialRT {
                    Albedo    = CV(cEdif, vidActual ? 0.45 : 0.80),
                    Especular = vidActual ? 0.92 : 0.22,
                    Reflexion = vidActual ? 0.72 : 0.07,
                    Rugosidad = vidActual ? 0.02 : 0.55
                };
                _objetos.Add(cuerpo);

                // Franjas de ventanas como emisores tenues en modo noche
                if (_ctx.ModoOscuro && c == 0 && hTotal > 6)
                {
                    int nPlantas = (int)(ch / 1.0);
                    for (int p = 0; p < nPlantas; p += 2)
                    {
                        double yW = yAcum + 0.55 + p * 1.0;
                        var win = new CajaRT {
                            Min = new Vec3(cx - fw - 0.01 + offX, yW,        cz - fd + offZ),
                            Max = new Vec3(cx - fw + 0.01 + offX, yW + 0.55, cz + fd + offZ)
                        };
                        bool encendida = _rnd.NextDouble() < 0.65;
                        Color cW = encendida ? (_rnd.NextDouble() < 0.5 ? Color.LightYellow : Color.LightCyan)
                                              : Color.FromArgb(5, 8, 12);
                        win.Material = new MaterialRT {
                            EsLuz = encendida, EmisionColor = CV(cW, encendida ? 1.2 : 0.0),
                            Albedo = CV(cW, 0.8)
                        };
                        _objetos.Add(win);
                    }
                }

                yAcum += ch;
            }

            // Corona del edificio: tanque de agua, flecha, o dome
            double coronaType = _rnd.NextDouble();
            if (coronaType < 0.33 && hTotal > 8) // Tanque de agua
            {
                var tanque = new CilindroRT {
                    Base   = new Vec3(cx - bw * 0.15, yAcum, cz - bd * 0.15),
                    Radio  = bw * 0.25,
                    Altura = hTotal * 0.06
                };
                tanque.Material = new MaterialRT {
                    Albedo = new Vec3(0.45, 0.38, 0.28), Especular = 0.08, Rugosidad = 0.9
                };
                _objetos.Add(tanque);
            }
            else if (coronaType < 0.66 && hTotal > 12) // Antena delgada
            {
                var ant = new CilindroRT {
                    Base   = new Vec3(cx, yAcum, cz),
                    Radio  = 0.05,
                    Altura = 2.0 + _rnd.NextDouble() * 3.0
                };
                ant.Material = new MaterialRT { Albedo = new Vec3(0.55, 0.55, 0.6), Especular = 0.7 };
                _objetos.Add(ant);
                if (_ctx.ModoOscuro)
                {
                    var baliza = new EsferaRT { Centro = new Vec3(cx, yAcum + ant.Altura, cz), Radio = 0.12 };
                    baliza.Material = new MaterialRT { EsLuz = true, EmisionColor = new Vec3(3, 0.2, 0.2) };
                    _objetos.Add(baliza);
                }
            }
            else // Dome / remate piramidal
            {
                var dome = new EsferaRT {
                    Centro = new Vec3(cx, yAcum + bw * 0.3, cz),
                    Radio  = Math.Min(bw, bd) * 0.45
                };
                dome.Material = new MaterialRT {
                    Albedo = CV(pal[(_rnd.Next(pal.Count) + 1) % pal.Count], 0.7),
                    Especular = 0.5, Reflexion = 0.25, Rugosidad = 0.15
                };
                _objetos.Add(dome);
            }
        }

        // ═══════════════════════════════════════════════════════════════════════
        // ESCENA: Superficie Planetaria — marciana/lunar/alienígena
        // ═══════════════════════════════════════════════════════════════════════
        private void ConstruirSuperficiePlanetaria()
        {
            List<Color> pal = _ctx.Paleta.Count > 0 ? _ctx.Paleta
                : new List<Color> { Color.FromArgb(180, 90, 50), Color.FromArgb(140, 70, 35),
                                    Color.FromArgb(210, 160, 100) };

            bool esDia = !_ctx.ModoOscuro;
            _cielo1 = esDia ? new Vec3(0.55, 0.28, 0.12) : new Vec3(0.01, 0.01, 0.03);
            _cielo2 = esDia ? new Vec3(0.70, 0.42, 0.20) : new Vec3(0.0,  0.0,  0.0);

            // Cámara variada: más baja o más alta, lateral
            double camX = (_rnd.NextDouble() - 0.5) * 6;
            double camY = 2.0 + _rnd.NextDouble() * 3.0;
            _camPos    = new Vec3(camX, camY, -10 - _rnd.NextDouble() * 3);
            _camLookAt = new Vec3(camX * 0.2, 1.0, 10);

            Matematica.InicializarSemilla(_ctx.Semilla);
            var terreno = new TerrenoRM {
                Escala   = 0.12 + _ctx.Complejidad * 0.05,
                Amplitud = 4.0  + _ctx.Intensidad  * 2.5,
                YOffset  = 0.0,
                Semilla  = _ctx.Semilla + 7,
                ColorBajo  = CV(pal[0],                   0.80),
                ColorMedio = CV(pal.Count > 1 ? pal[1] : pal[0], 0.72),
                ColorAlto  = CV(pal.Count > 2 ? pal[2] : pal[0], 0.88),
            };
            terreno.Material = new MaterialRT { Especular = 0.03, Rugosidad = 0.98 };
            _objetos.Add(terreno);

            // Rocas alienígenas — más densas y grandes
            var rndR = new Random(_ctx.Semilla ^ 0xA7B3);
            for (int i = 0; i < 18; i++)
            {
                double rx = (rndR.NextDouble() - 0.5) * 24;
                double rz =  rndR.NextDouble() * 22;
                AgregarRoca(rx, 0, rz, 0.4 + rndR.NextDouble() * 1.4,
                            CV(pal[rndR.Next(pal.Count)], 0.65));
            }

            // Planeta/luna visible en el cielo
            var planeta2 = new EsferaRT { Centro = new Vec3(28, 10, 45), Radio = 8 };
            Color cP2 = pal.Count > 2 ? pal[2] : Color.CornflowerBlue;
            planeta2.Material = new MaterialRT { Albedo = CV(cP2, 0.85), Especular = 0.12, Reflexion = 0.04 };
            _objetos.Add(planeta2);

            if (!esDia)
            {
                var rndS = new Random(_ctx.Semilla ^ 0x5E7A);
                CieloEstrellado(30, rndS);
                var lunaS = new EsferaRT { Centro = new Vec3(-15, 10, 35), Radio = 1.4 };
                lunaS.Material = new MaterialRT { Albedo = new Vec3(0.48, 0.42, 0.38), Especular = 0.04 };
                _objetos.Add(lunaS);
            }

            Vec3 luzS = esDia ? new Vec3(1.0, 0.82, 0.68) : new Vec3(0.04, 0.04, 0.08);
            _luces.Add(new LuzRT { Pos = new Vec3(20, 40, -15), Color = luzS,                      Intensidad = esDia ? 1.2 : 0.25 });
            _luces.Add(new LuzRT { Pos = new Vec3(-20, 12, 5),  Color = new Vec3(0.5, 0.32, 0.22), Intensidad = 0.18 });
        }

        // ═══════════════════════════════════════════════════════════════════════
        // ESCENA: Nebulosa 3D — pilares, filamentos, núcleos brillantes
        // ═══════════════════════════════════════════════════════════════════════
        private void ConstruirNebulosa3D()
        {
            _cielo1 = Vec3.Zero;
            _cielo2 = Vec3.Zero;
            _camPos    = new Vec3(0, 0, -20);
            _camLookAt = new Vec3(0, 0, 0);

            List<Color> pal = _ctx.Paleta.Count > 0 ? _ctx.Paleta
                : new List<Color> { Color.Magenta, Color.Cyan, Color.DeepSkyBlue, Color.MediumPurple };

            // Pilares de polvo — columnas cilíndricas alargadas con esferas encima
            for (int p = 0; p < 4; p++)
            {
                double pAng = p * Math.PI * 2 / 4 + _rnd.NextDouble() * 0.4;
                double pr   = 3 + _rnd.NextDouble() * 4;
                double px   = Math.Cos(pAng) * pr;
                double pz   = Math.Sin(pAng) * pr * 0.6;
                double ph   = 5 + _rnd.NextDouble() * 6;
                Color cPil  = pal[p % pal.Count];

                // Cuerpo del pilar (cilindro)
                var pilar = new CilindroRT {
                    Base   = new Vec3(px, -3, pz),
                    Radio  = 0.6 + _rnd.NextDouble() * 0.8,
                    Altura = ph
                };
                pilar.Material = new MaterialRT {
                    EsLuz = true, EmisionColor = CV(cPil, 0.08 + _rnd.NextDouble() * 0.08)
                };
                _objetos.Add(pilar);

                // Cabeza luminosa del pilar
                var cabeza = new EsferaRT {
                    Centro = new Vec3(px, -3 + ph, pz),
                    Radio  = 0.9 + _rnd.NextDouble() * 1.2
                };
                cabeza.Material = new MaterialRT {
                    EsLuz = true, EmisionColor = CV(cPil, 0.25 + _rnd.NextDouble() * 0.3)
                };
                _objetos.Add(cabeza);
            }

            // Nubes de gas en capas elípticas
            for (int i = 0; i < 14; i++)
            {
                double t2 = _rnd.NextDouble() * Math.PI * 2;
                double el = (_rnd.NextDouble() - 0.5) * Math.PI * 0.5;
                double rs = 2 + _rnd.NextDouble() * 9;
                Color cN  = pal[_rnd.Next(pal.Count)];
                double nr = 2 + _rnd.NextDouble() * 4;
                double nx = Math.Cos(el) * Math.Cos(t2) * rs;
                double ny = Math.Sin(el) * rs * 0.5;
                double nz = Math.Cos(el) * Math.Sin(t2) * rs * 0.7;
                AgregarNube(nx, ny, nz, nr, CV(cN, 0.0)); // material propio abajo
                // reemplazar material de la última nube por emisivo
                // (las esferas ya se añadieron; ajustamos las últimas)
                // Solución: construir directamente en vez de usar AgregarNube aquí
            }

            // Nubes de gas gaseosas directamente (emisivas)
            for (int i = 0; i < 10; i++)
            {
                double t2 = _rnd.NextDouble() * Math.PI * 2;
                double rs = 1.5 + _rnd.NextDouble() * 7;
                Color cN  = pal[_rnd.Next(pal.Count)];
                double em = 0.06 + _rnd.NextDouble() * 0.14;
                var nub = new EsferaRT {
                    Centro = new Vec3(Math.Cos(t2) * rs,
                                      (_rnd.NextDouble() - 0.5) * rs * 0.5,
                                      Math.Sin(t2) * rs * 0.65),
                    Radio  = 2 + _rnd.NextDouble() * 4
                };
                nub.Material = new MaterialRT { EsLuz = true, EmisionColor = CV(cN, em) };
                _objetos.Add(nub);
            }

            // Estrellas jóvenes brillantes en el centro
            for (int i = 0; i < 7; i++)
            {
                double sr = _rnd.NextDouble() * 2;
                double st = _rnd.NextDouble() * Math.PI * 2;
                Color cSt = pal[_rnd.Next(pal.Count)];
                var star = new EsferaRT {
                    Centro = new Vec3(Math.Cos(st) * sr, (_rnd.NextDouble() - 0.5) * 2, Math.Sin(st) * sr),
                    Radio  = 0.10 + _rnd.NextDouble() * 0.25
                };
                star.Material = new MaterialRT { EsLuz = true, EmisionColor = CV(cSt, 5 + _rnd.NextDouble() * 6) };
                _objetos.Add(star);
            }

            // Estrellas de fondo
            CieloEstrellado(40, new Random(_ctx.Semilla ^ 0xBB11), 0.4, 1.8);

            _luces.Add(new LuzRT { Pos = new Vec3(0, 0, -6), Color = new Vec3(0.8, 0.8, 1.0), Intensidad = 0.3 });
        }

        // ═══════════════════════════════════════════════════════════════════════
        // ESCENA: Cañón — estratos, voladizos, arco, río
        // ═══════════════════════════════════════════════════════════════════════
        private void ConstruirCanon3D()
        {
            List<Color> pal = _ctx.Paleta.Count > 0 ? _ctx.Paleta
                : new List<Color> { Color.FromArgb(180, 80, 40), Color.FromArgb(220, 140, 60),
                                    Color.FromArgb(140, 55, 30), Color.FromArgb(200, 170, 100) };

            _cielo1 = _ctx.ModoOscuro ? new Vec3(0.02, 0.04, 0.12) : new Vec3(0.42, 0.65, 0.92);
            _cielo2 = _ctx.ModoOscuro ? new Vec3(0.0,  0.0,  0.0)  : new Vec3(0.68, 0.84, 1.00);

            // Cámara: dentro del cañón, mirando hacia dentro, con variación
            double camY  = 1.5 + _rnd.NextDouble() * 2.5;
            double camX  = (_rnd.NextDouble() - 0.5) * 2.5;
            _camPos    = new Vec3(camX, camY, -6 - _rnd.NextDouble() * 3);
            _camLookAt = new Vec3(0, camY + 1.5, 10);

            // Suelo arenoso
            var suelo = new PlanoRT { Normal = new Vec3(0, 1, 0), D = 0, Albedo2 = CV(pal[pal.Count - 1], 0.55) };
            suelo.Material = new MaterialRT { Albedo = CV(pal[0], 0.78), Especular = 0.04, Rugosidad = 0.98 };
            _objetos.Add(suelo);

            // Río estrecho en el fondo del cañón
            var rio = new PlanoRT { Normal = new Vec3(0, 1, 0), D = -0.08, Albedo2 = new Vec3(0.08, 0.20, 0.50) };
            rio.Material = new MaterialRT {
                Albedo    = new Vec3(0.06, 0.18, 0.52),
                Especular = 0.70,
                Reflexion = 0.55,
                Rugosidad = 0.04
            };
            _objetos.Add(rio);

            // Paredes del cañón — múltiples estratos horizontales por lado
            Matematica.InicializarSemilla(_ctx.Semilla);

            // Alturas y colores de estratos geológicos
            double[] estratoY    = { 0.0, 1.3, 2.9, 4.7, 6.8, 9.2, 11.8, 14.0 };
            int[]    estratoPal  = { 0, 2, 1, 3, 0, 2, 1, 3 };

            for (int lado = 0; lado < 2; lado++)
            {
                double xDir  = lado == 0 ? -1.0 : 1.0;
                double xBase = lado == 0 ? -2.0 : 2.0;  // borde interior del cañón

                for (int seg = 0; seg < 7; seg++)  // segmentos a lo largo del cañón
                {
                    double zS = seg * 3.0 - 3.0;
                    double zE = zS + 3.2;

                    for (int est = 0; est < estratoY.Length - 1; est++)
                    {
                        double yBot = estratoY[est];
                        double yTop = estratoY[est + 1];

                        // Variación de ancho por estrato — crea voladizos naturales
                        double noiseF = Matematica.Perlin(seg * 0.45 + est * 0.8 + _ctx.Semilla * 0.003, est * 0.6);
                        double anchoExtra = noiseF * 1.2;
                        double xOuter = xBase + xDir * (3.5 + est * 0.2 + anchoExtra);

                        int pi = estratoPal[est % estratoPal.Length];
                        double nCol = Matematica.Perlin(est * 0.25 + seg * 0.18, _ctx.Semilla * 0.001);

                        var bloque = new CajaRT {
                            Min = new Vec3(Math.Min(xBase, xOuter) - (lado == 0 ? 0.05 : 0), yBot, zS),
                            Max = new Vec3(Math.Max(xBase, xOuter) + (lado == 1 ? 0.05 : 0), yTop, zE)
                        };
                        bloque.Material = new MaterialRT {
                            Albedo    = CV(pal[pi % pal.Count], 0.68 + nCol * 0.22),
                            Especular = 0.035,
                            Rugosidad = 0.97
                        };
                        _objetos.Add(bloque);
                    }
                }
            }

            // Arco natural — par de pilares con una caja superior conectando
            double archX = (_rnd.NextDouble() - 0.5) * 1.5;
            double archZ = 8 + _rnd.NextDouble() * 4;
            double archH = 3.5 + _rnd.NextDouble() * 2.5;
            double archW = 0.5 + _rnd.NextDouble() * 0.4;

            for (int side = 0; side < 2; side++) // pilares izquierda y derecha
            {
                double px = archX + (side == 0 ? -1.8 : 1.8);
                var pilar = new CajaRT {
                    Min = new Vec3(px - archW, 0, archZ - archW),
                    Max = new Vec3(px + archW, archH, archZ + archW)
                };
                pilar.Material = new MaterialRT { Albedo = CV(pal[1], 0.75), Especular = 0.03, Rugosidad = 0.98 };
                _objetos.Add(pilar);
            }
            var dintel = new CajaRT { // dintel del arco
                Min = new Vec3(archX - 2.3, archH, archZ - archW * 1.1),
                Max = new Vec3(archX + 2.3, archH + archW * 2, archZ + archW * 1.1)
            };
            dintel.Material = new MaterialRT { Albedo = CV(pal[2], 0.72), Especular = 0.03, Rugosidad = 0.98 };
            _objetos.Add(dintel);

            // Rocas sueltas en el suelo del cañón
            for (int i = 0; i < 15; i++)
            {
                double rx = (_rnd.NextDouble() - 0.5) * 3.5;
                double rz =  _rnd.NextDouble() * 16;
                AgregarRoca(rx, 0, rz, 0.2 + _rnd.NextDouble() * 0.7, CV(pal[0], 0.60));
            }

            // Estrellas o nubes
            if (_ctx.ModoOscuro)
                CieloEstrellado(15, new Random(_ctx.Semilla ^ 0xCA11));
            else
            {
                for (int n = 0; n < 3; n++)
                    AgregarNube((_rnd.NextDouble() - 0.5) * 10, 18 + _rnd.NextDouble() * 5,
                                5 + _rnd.NextDouble() * 12, 2.5 + _rnd.NextDouble() * 1.5,
                                new Vec3(0.92, 0.93, 0.96));
            }

            Vec3 luzCol = _ctx.ModoOscuro ? new Vec3(0.10, 0.14, 0.30) : new Vec3(1.0, 0.90, 0.70);
            _luces.Add(new LuzRT { Pos = new Vec3(6, 28, -2),  Color = luzCol,                   Intensidad = 1.3 });
            _luces.Add(new LuzRT { Pos = new Vec3(-5, 8, 12), Color = new Vec3(0.42, 0.55, 0.80), Intensidad = 0.28 });
        }

        // ═══════════════════════════════════════════════════════════════════════
        // ESCENA: Tormenta — nubes coherentes, rayo, suelo inundado
        // ═══════════════════════════════════════════════════════════════════════
        private void ConstruirTormenta3D()
        {
            _cielo1 = new Vec3(0.06, 0.06, 0.10);
            _cielo2 = new Vec3(0.02, 0.02, 0.04);

            // Cámara variada — a veces más baja mirando el horizonte
            double camY = 2 + _rnd.NextDouble() * 5;
            double camX = (_rnd.NextDouble() - 0.5) * 4;
            _camPos    = new Vec3(camX, camY, -14 - _rnd.NextDouble() * 4);
            _camLookAt = new Vec3(0, 7, 0);

            List<Color> pal = _ctx.Paleta.Count > 0 ? _ctx.Paleta
                : new List<Color> { Color.FromArgb(50, 55, 70), Color.FromArgb(25, 35, 90),
                                    Color.FromArgb(210, 215, 255) };

            // Suelo inundado reflectante — con algo de altura para simular charcos
            var suelo = new PlanoRT { Normal = new Vec3(0, 1, 0), D = 0, Albedo2 = new Vec3(0.04, 0.05, 0.09) };
            suelo.Material = new MaterialRT {
                Albedo    = new Vec3(0.06, 0.07, 0.11),
                Reflexion = 0.70,
                Especular = 0.85,
                Rugosidad = 0.04
            };
            _objetos.Add(suelo);

            // Nubes tormentosas — usando AgregarNube para tener cuerpos coherentes
            // Organizadas en varias capas de altura
            int nNubes = 8 + (int)(_ctx.Complejidad * 4);
            for (int i = 0; i < nNubes; i++)
            {
                double cx = (_rnd.NextDouble() - 0.5) * 28;
                double cz =  _rnd.NextDouble() * 25;
                double cy =  6 + _rnd.NextDouble() * 7;
                double nr =  2.5 + _rnd.NextDouble() * 4.5;
                // Tonos grises muy oscuros con tinte azulado
                double gr = 0.18 + _rnd.NextDouble() * 0.18;
                Vec3 colNube = new Vec3(gr, gr * 0.95, gr * 1.08);
                AgregarNube(cx, cy, cz, nr, colNube);
            }

            // Rayo como cilindro delgado + esfera en la base (impacto)
            double rayoX = (_rnd.NextDouble() - 0.5) * 5;
            double rayoZ =  4 + _rnd.NextDouble() * 6;
            double rayoH =  6 + _rnd.NextDouble() * 4;
            var rayoCil = new CilindroRT {
                Base   = new Vec3(rayoX, 0, rayoZ),
                Radio  = 0.04,
                Altura = rayoH
            };
            rayoCil.Material = new MaterialRT { EsLuz = true, EmisionColor = new Vec3(5.5, 5.5, 8.0) };
            _objetos.Add(rayoCil);

            var impacto = new EsferaRT { Centro = new Vec3(rayoX, 0.2, rayoZ), Radio = 0.35 };
            impacto.Material = new MaterialRT { EsLuz = true, EmisionColor = new Vec3(4, 4, 7) };
            _objetos.Add(impacto);

            // Lluvia implícita — cilindros muy finos y largos
            var rndLl = new Random(_ctx.Semilla ^ 0xBA17);
            for (int i = 0; i < 20; i++)
            {
                double lx = (rndLl.NextDouble() - 0.5) * 20;
                double lz =  rndLl.NextDouble() * 20;
                double ly =  2 + rndLl.NextDouble() * 5;
                var gota = new CilindroRT {
                    Base   = new Vec3(lx, ly - 0.8, lz),
                    Radio  = 0.015,
                    Altura = 0.8
                };
                gota.Material = new MaterialRT {
                    Albedo = new Vec3(0.55, 0.60, 0.75), Especular = 0.4, Reflexion = 0.2, Rugosidad = 0.1
                };
                _objetos.Add(gota);
            }

            _luces.Add(new LuzRT { Pos = new Vec3(0, 22, 0),         Color = new Vec3(0.40, 0.44, 0.70), Intensidad = 0.7 });
            _luces.Add(new LuzRT { Pos = new Vec3(rayoX, 4, rayoZ),   Color = new Vec3(0.65, 0.65, 1.0),  Intensidad = 2.5 }); // destello rayo
        }

        // ═══════════════════════════════════════════════════════════════════════
        // ESCENA: Océano — olas volumétricas, isla, sol/luna
        // ═══════════════════════════════════════════════════════════════════════
        private void ConstruirOceano3D()
        {
            bool esDia = !_ctx.ModoOscuro;
            _cielo1 = esDia ? new Vec3(0.28, 0.52, 0.88) : new Vec3(0.02, 0.04, 0.12);
            _cielo2 = esDia ? new Vec3(0.68, 0.84, 1.00) : new Vec3(0.01, 0.02, 0.06);

            // Cámara variada — a veces casi a nivel del mar, a veces más elevada
            double camY  = 1.5 + _rnd.NextDouble() * 4.0;
            double camAng = (_rnd.NextDouble() - 0.5) * 0.8;
            double camD  = 10 + _rnd.NextDouble() * 4;
            _camPos    = new Vec3(Math.Sin(camAng) * camD, camY, -camD);
            _camLookAt = new Vec3(0, camY * 0.3, 5);

            List<Color> pal = _ctx.Paleta.Count > 0 ? _ctx.Paleta
                : new List<Color> { Color.FromArgb(10, 75, 175), Color.FromArgb(0, 155, 195),
                                    Color.FromArgb(240, 248, 255) };

            // Superficie del océano
            var oceano = new PlanoRT { Normal = new Vec3(0, 1, 0), D = 0,
                                        Albedo2 = CV(pal.Count > 1 ? pal[1] : Color.Cyan, 0.55) };
            oceano.Material = new MaterialRT {
                Albedo    = CV(pal[0], 0.50),
                Especular = 0.88,
                Reflexion = 0.72,
                Rugosidad = 0.02
            };
            _objetos.Add(oceano);

            // Olas — esferas grandes y achatadas (radio > altura efectiva) para dar
            // sensación de cresta, no de pelotas flotando
            var rndO = new Random(_ctx.Semilla ^ 0x0CEA);
            for (int i = 0; i < 25; i++)
            {
                double wx = (rndO.NextDouble() - 0.5) * 30;
                double wz =  rndO.NextDouble() * 28;
                double wr = 0.8 + rndO.NextDouble() * 2.0;   // radio amplio
                double wh = wr * 0.25;                         // muy achatado
                // simulamos con una esfera grande ligeramente hundida
                var ola = new EsferaRT {
                    Centro = new Vec3(wx, -wr + wh, wz),
                    Radio  = wr
                };
                double blnc = 0.82 + rndO.NextDouble() * 0.14;
                ola.Material = new MaterialRT {
                    Albedo    = new Vec3(blnc, blnc, blnc + 0.04),
                    Especular = 0.65,
                    Reflexion = 0.35,
                    Rugosidad = 0.08
                };
                _objetos.Add(ola);
            }

            // Isla con playa, interior verde y palma
            if (esDia || _ctx.ModoSuave)
            {
                // Base de la isla (roca/arena)
                var islaBase = new EsferaRT { Centro = new Vec3(8, -1.8, 18), Radio = 4.0 };
                islaBase.Material = new MaterialRT { Albedo = new Vec3(0.78, 0.68, 0.45), Especular = 0.04, Rugosidad = 0.97 };
                _objetos.Add(islaBase);

                // Interior vegetal
                var islaVerde = new EsferaRT { Centro = new Vec3(7.5, 0.3, 17.5), Radio = 2.2 };
                islaVerde.Material = new MaterialRT { Albedo = new Vec3(0.22, 0.55, 0.20), Especular = 0.05, Rugosidad = 0.95 };
                _objetos.Add(islaVerde);

                // Palmera
                AgregarArbol(8, 18, 1.5,
                             new Vec3(0.40, 0.28, 0.12),
                             new Vec3(0.20, 0.55, 0.18));
            }

            // Rocas emergentes
            for (int i = 0; i < 6; i++)
            {
                double rx = (rndO.NextDouble() - 0.5) * 22;
                double rz =  rndO.NextDouble() * 22;
                AgregarRoca(rx, -0.2, rz, 0.4 + rndO.NextDouble() * 0.8,
                             new Vec3(0.35, 0.33, 0.30));
            }

            // Sol o luna
            Color cSol = esDia ? Color.Gold : Color.WhiteSmoke;
            double solR = esDia ? 2.2 : 1.3;
            double solBr = esDia ? 5.0 : 2.2;
            var sol = new EsferaRT { Centro = new Vec3(12, 12, 35), Radio = solR };
            sol.Material = new MaterialRT { EsLuz = true, EmisionColor = CV(cSol, solBr) };
            _objetos.Add(sol);

            // Nubes bajas sobre el horizonte
            if (esDia)
            {
                for (int n = 0; n < 5; n++)
                {
                    double nx = (_rnd.NextDouble() - 0.5) * 30;
                    double nz = 15 + _rnd.NextDouble() * 20;
                    AgregarNube(nx, 5 + _rnd.NextDouble() * 4, nz,
                                2.5 + _rnd.NextDouble() * 2.0, new Vec3(0.90, 0.92, 0.96));
                }
            }
            else
            {
                CieloEstrellado(20, new Random(_ctx.Semilla ^ 0x0CEA2));
            }

            Vec3 luzPrinc = esDia ? new Vec3(1.0, 0.96, 0.84) : new Vec3(0.30, 0.40, 0.65);
            _luces.Add(new LuzRT { Pos = new Vec3(12, 15, 35), Color = luzPrinc,                   Intensidad = esDia ? 1.3 : 0.55 });
            _luces.Add(new LuzRT { Pos = new Vec3(-12, 8, -5), Color = new Vec3(0.28, 0.48, 0.80), Intensidad = 0.28 });
        }

        // ═══════════════════════════════════════════════════════════════════════
        // RENDER — sin cambios en la lógica de trazado
        // ═══════════════════════════════════════════════════════════════════════
        public Bitmap GenerarProgresivo(int ancho, int alto,
            Action<int> reportarProgreso,
            Action<Bitmap> actualizarCanvas,
            ref bool cancelar)
        {
            Vec3 fwd   = (_camLookAt - _camPos).Norm();
            Vec3 right = Vec3.Cross(fwd, Vec3.Up).Norm();
            Vec3 up    = Vec3.Cross(right, fwd).Norm();
            double fov = 60.0 * Math.PI / 180.0;
            double h   = Math.Tan(fov / 2);
            double w   = h * ancho / alto;

            int    stride3 = ancho * 3;
            byte[] buf     = new byte[alto * stride3];
            Bitmap bmpParcial = new Bitmap(ancho, alto, PixelFormat.Format24bppRgb);

            int spp = _ctx.ModoSuave ? 4 : 2;

            for (int y = 0; y < alto; y++)
            {
                if (cancelar) break;

                for (int x = 0; x < ancho; x++)
                {
                    double tr = 0, tg = 0, tb = 0;

                    for (int s = 0; s < spp; s++)
                    {
                        double jx = (s % 2 == 0) ? 0.25 : 0.75;
                        double jy = (s < 2) ? 0.25 : 0.75;
                        double u  = (x + jx) / ancho * 2 - 1;
                        double v  = 1 - (y + jy) / alto * 2;
                        Vec3   d  = (fwd + right * (u * w) + up * (v * h)).Norm();
                        Vec3  col = Trazar(new Rayo(_camPos, d), 3);
                        tr += col.X; tg += col.Y; tb += col.Z;
                    }

                    tr /= spp; tg /= spp; tb /= spp;
                    tr = Math.Sqrt(Matematica.Clamp01(tr));
                    tg = Math.Sqrt(Matematica.Clamp01(tg));
                    tb = Math.Sqrt(Matematica.Clamp01(tb));

                    int off    = y * stride3 + x * 3;
                    buf[off]   = (byte)(tb * 255);
                    buf[off+1] = (byte)(tg * 255);
                    buf[off+2] = (byte)(tr * 255);
                }

                if ((y % 4 == 0 || y == alto - 1) && actualizarCanvas != null)
                {
                    var bd = bmpParcial.LockBits(
                        new Rectangle(0, 0, ancho, alto), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
                    Marshal.Copy(buf, 0, bd.Scan0, (y + 1) * stride3);
                    bmpParcial.UnlockBits(bd);
                    actualizarCanvas(new Bitmap(bmpParcial));
                }

                reportarProgreso?.Invoke(y * 100 / alto);
            }

            Bitmap bmpFinal = new Bitmap(ancho, alto, PixelFormat.Format24bppRgb);
            var bdF = bmpFinal.LockBits(
                new Rectangle(0, 0, ancho, alto), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            Marshal.Copy(buf, 0, bdF.Scan0, buf.Length);
            bmpFinal.UnlockBits(bdF);
            return bmpFinal;
        }

        private Vec3 Trazar(Rayo r, int profundidad)
        {
            if (profundidad <= 0) return Vec3.Zero;

            var mejor = new InfoImpacto { T = double.MaxValue };

            foreach (ObjetoRT obj in _objetos)
            {
                InfoImpacto info = obj.Intersectar(r);
                if (info.Golpeo && info.T < mejor.T)
                    mejor = info;
            }

            if (!mejor.Golpeo) return ColorCielo(r.Dir);

            MaterialRT mat    = mejor.Material;
            if (mat.EsLuz)   return mat.EmisionColor;

            Vec3 punto  = mejor.Punto;
            Vec3 normal = mejor.Normal;

            Vec3 colorFinal = mat.Albedo * 0.08; // ambiental mínimo

            foreach (LuzRT luz in _luces)
            {
                Vec3   L     = (luz.Pos - punto).Norm();
                double lDist = (luz.Pos - punto).Len();

                Rayo rSombra  = new Rayo(punto + normal * 0.001, L);
                bool enSombra = false;
                foreach (ObjetoRT obj in _objetos)
                {
                    InfoImpacto si = obj.Intersectar(rSombra);
                    if (si.Golpeo && si.T < lDist && !si.Material.EsLuz)
                    { enSombra = true; break; }
                }
                if (enSombra) continue;

                double nDotL = Math.Max(0, Vec3.Dot(normal, L));
                double atten = 1.0 / (1.0 + lDist * lDist * 0.02);
                colorFinal = colorFinal + mat.Albedo * luz.Color * (nDotL * atten * luz.Intensidad);

                Vec3   V    = (-r.Dir).Norm();
                Vec3   H    = (L + V).Norm();
                double spec = Math.Pow(Math.Max(0, Vec3.Dot(normal, H)), 64 * mat.Especular + 1);
                colorFinal  = colorFinal + luz.Color * (spec * mat.Especular * atten * luz.Intensidad);
            }

            if (mat.Reflexion > 0.01 && profundidad > 1)
            {
                Vec3 refDir = Vec3.Reflect(r.Dir, normal);
                if (mat.Rugosidad > 0.05)
                {
                    var rnd2 = new Random((int)(punto.X * 1000 + punto.Y * 2000 + punto.Z * 3000));
                    refDir = (refDir + new Vec3(
                        (rnd2.NextDouble() - 0.5) * mat.Rugosidad,
                        (rnd2.NextDouble() - 0.5) * mat.Rugosidad,
                        (rnd2.NextDouble() - 0.5) * mat.Rugosidad)).Norm();
                }
                Vec3 cRef  = Trazar(new Rayo(punto + normal * 0.001, refDir), profundidad - 1);
                colorFinal = colorFinal * (1 - mat.Reflexion) + cRef * mat.Reflexion;
            }

            return colorFinal;
        }

        private Vec3 ColorCielo(Vec3 dir)
        {
            double t = (dir.Y + 1) * 0.5;
            return _cielo2 * (1 - t) + _cielo1 * t;
        }
    }
}