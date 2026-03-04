# 🎨 OpenCIP – Open CPU Image Painter
[![License: GPL v3](https://img.shields.io/badge/License-GPLv3-blue.svg)](https://www.gnu.org/licenses/gpl-3.0)
[![C#](https://img.shields.io/badge/C%23-9.0-239120.svg)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![.NET](https://img.shields.io/badge/.NET-6.x-green.svg)](https://dotnet.microsoft.com/)
[![Zero Dependencies](https://img.shields.io/badge/Dependencies-None-brightgreen.svg)]()

## Descripción
OpenCIP (CPU Image Painter) es un generador procedural de imágenes basado en CPU, escrito en C#. Integra algoritmos como ruido Perlin, fractales Mandelbrot, fluidos turbulentos, diagramas Voronoi, ondas de interferencia, nebulosas espaciales, plasma caótico, mundos voxel (estilo Minecraft), redes neuronales CPPN, escenas 3D con raytracing básico, simulaciones de acuarela y dibujos a lápiz/lapicera/carbón. Interpreta prompts textuales para crear arte abstracto, paisajes, entornos 3D y más. Incluye modos "IA" autónomos para selección algorítmica inteligente y post-procesado refinado.

Ideal para experimentación creativa, fondos de pantalla, arte digital o prototipos visuales sin GPU. Ejecuta en paralelo vía CPU para optimización.

## Para qué sirve
- Crear imágenes procedurales: paisajes (océano, bosque, montaña), abstractos (fractales, plasma), 3D (terrenos, planetas, ciudades), acuarelas o dibujos estilizados.
- Explorar temas (espacio, fuego, nieve, atardeceres) con modificadores (oscuro, caótico, suave, simétrico).
- Modos avanzados: lienzo multicapa, render progresivo (3D/redes neuronales), post-proceso "IA".
- Aplicaciones educativas: estudiar algoritmos gráficos/procedimentales.
- Exportación en PNG/JPG/BMP para diseño, juegos o visuales.

## Cómo se usa
1. **Requisitos**: Windows + .NET 6.0.
2. **Compilación**: Abre el proyecto en Visual Studio o compila con `dotnet build` (requiere csproj configurado para .NET 6.0).
3. **Ejecución**: Interfaz con panel izquierdo (prompt, ejemplos, semilla, escala, algoritmos manuales) y canvas derecho.
   - **Prompt**: Español/inglés, e.g., "IA lienzo oceano violeta" (modo IA para océano violeta). "IA" activa autonomía. Ejemplos clickeables integrados.
   - **Semilla**: Fija (0-999999) para reproducibilidad.
   - **Escala**: 0.1x-4.0x vía trackbar.
   - **Algoritmos manuales**: Checkboxes para override (AUTO default).
   - **Generar**: Botón/Enter; muestra progreso, cancela si necesario.
   - **Guardar**: Exporta imagen.
   - **Aleatorio/Limpiar**: Prompt random o borrar input.
4. **Consejos**: Combina colores (rojo, azul), temas (bosque, fractal) y mods (caótico). Para 3D: "terreno 3d verde". Progresivo actualiza en vivo. Tags detectados y resumen visual ayudan.

## Ejemplos de Prompts Creativos
- "IA lienzo bosque encantado niebla violeta arcoiris": Paisaje místico con niebla y arcoíris.
- "fractal mandelbrot neon caotico azul rosa": Fractal vibrante neon caótico.
- "acuarela ciudad nocturna lluvia neon": Acuarela urbana lluviosa con luces neon.
- "lapiz desierto alien 3d oscuro estrellas": Dibujo lápiz de desierto extraterrestre 3D.
- "IA plasma fuego turbulento rojo amarillo intenso": Abstracto plasma ígneo caótico.
- "minecraft voxel selva lluviosa palmeras": Mundo bloqueado selvático.
- "red neural cppn abstracto simetrico pastel suave": Patrón neuronal simétrico suave.

Experimenta variaciones para resultados únicos; combina algoritmos/colores/mods.

## Explicaciones de Diseño
Diseño minimalista y accesible: UI oscura (BG_DARK #121216, ACENTO #00BEF0) para confort visual. Panel izquierdo agrupa inputs lógicos (prompt arriba, controles abajo); canvas derecho ocupa espacio principal. Botones planos, etiquetas claras y ejemplos clickeables aceleran onboarding. Modo "IA" interpreta prompts dinámicamente, seleccionando algoritmos/paletas para no-técnicos. Progreso (barra, updates en vivo) y cancelación mejoran UX en tareas intensivas. Enfoque en intuición: tags/resumen visual feedback inmediato. Escalabilidad: enums/diccionarios permiten expansiones fáciles sin refactor.

## Arquitectura y Organización
Arquitectura modular multi-archivo con namespaces (OpenCIP para core, TSFG para UI principal). Componentes clave:
- **ContextoVisual.cs**: Clase central con propiedades (algoritmos, pesos, paleta), flags (modos caos/suave/oscuro) y enums (escenas, entornos 3D).
- **BancoPalabras.cs**: Diccionarios para mapping palabras → colores/temas/modificadores; delegados aplican cambios a ContextoVisual.
- **InterpretadorPrompt.cs / InterpretadorIA.cs / InterpretadorLienzo.cs**: Parsers de prompts; detectan keywords, configuran ContextoVisual vía mappings.
- **MotorOpenCIP.cs**: Renderizador principal; usa Tasks/Parallel.For para CPU paralela. Métodos modulares por algoritmo (e.g., RenderPerlin, Render3D).
- **UI.cs / MainForm.cs / MainForm.Designer.cs**: WinForms UI; maneja eventos, progresivo vía callbacks, GDI+ para dibujo.
- **PostProcesadorIA.cs**: Refina imágenes post-render con filtros/mezclas.
- **Generadores específicos**: Archivos como `Generadores.cs`, `GeneradorMundoVoxel.cs`, `GeneradorEscena3D.cs`, `GeneradorDibujo.cs`, `GeneradorAcuarela.cs`, `GeneradorLienzo.cs` para algoritmos dedicados (Perlin, Voxel, 3D, Dibujo, Acuarela, Lienzo).
- **Utilidades**: `Matematica.cs` (funciones matemáticas, Perlin, FBM), `Enums.cs` (tipos como AlgoritmoBase, TipoEscena), `RayTracing.cs` (estructuras para raytracing), `RedNeuronal.cs` (redes CPPN).

Organización: Archivos por componente (clases/funciones relacionadas); enums centralizados para tipos. Estado reutilizable en variables como _ultimoCtx. Extensible: agregar algoritmos vía enums y clases nuevas.

## Licencia
GNU General Public License v3 (GPL-3.0). Usa/modifica/distribuye libremente con atribución y misma licencia. Sin garantías. Ver [LICENSE](https://www.gnu.org/licenses/gpl-3.0.en.html).

## Autor y Desarrollador
- **Autor**: © Turing Software
- **Desarrollador**: LexusYTG

## Detalles Técnicos
- **Lenguaje**: C# 9.0
- **Dependencias**: .NET 6.0 (System.Drawing, System.Windows.Forms, Tasks).
- **Características**: Paralelismo Tasks, GDI+ render, enums para tipos (algoritmos, escenas, pinceles), RNG reproducible.
- **Limitaciones**: CPU-intensivo en alta resolución; sin GPU. Código multi-archivo para modularidad.
Contribuciones bienvenidas. Reporta issues en repo.
