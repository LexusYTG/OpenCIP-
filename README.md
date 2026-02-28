# 🎨 OpenCIP – Open CPU Image Painter

[![License: GPL v3](https://img.shields.io/badge/License-GPLv3-blue.svg)](https://www.gnu.org/licenses/gpl-3.0)
[![C#](https://img.shields.io/badge/C%23-5.0-239120.svg)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![.NET](https://img.shields.io/badge/.NET-4.x-green.svg)](https://dotnet.microsoft.com/)
[![Zero Dependencies](https://img.shields.io/badge/Dependencies-None-brightgreen.svg)]()

> Generador de imágenes algorítmicas procedural con estilo Minecraft voxel y múltiples algoritmos de ruido matemático. **Código 100% nativo, sin dependencias externas.**

---

## ✨ Características

### 🖼️ Algoritmos de Generación (16 motores matemáticos)
- **Ruido Perlin** – Generación orgánica de terrenos y texturas
- **Ruido Simplex** – Variante mejorada del Perlin, más eficiente  
- **Fractal Mandelbrot** – Exploración del conjunto de Mandelbrot con suavizado logarítmico
- **Fluido Turbulento** – Simulación de fluidos con advección vectorial multi-escala
- **Geométrico Simétrico** – Patrones mandala con múltiples armónicos
- **Voronoi Celular** – Diagramas de Voronoi con distancia F2-F1 y bordes de celda
- **Onda Interferencia** – Superposición de ondas circulares con decaimiento
- **Nebulosa Espacial** – Nubes cósmicas con filamentos y FBM multi-octava
- **Plasma Caótico** – Funciones de distorsión trigonométricas no lineales
- **Dominio Warping** – Distorsión espacial recursiva del espacio de muestreo
- **Multifractal** – Terrenos heterogéneos con pesos adaptativos por octava
- **Turbulencia Rizos** – Aproximación a Curl Noise con desplazamiento angular
- **Patrones Celulares** – Simulación de reacción-difusión tipo Gray-Scott
- **Ray Marching 2D** – Renderizado de distancias firmes con SDF
- **Superficies Implícitas** – Metaballs y campos escalares acumulativos
- **Mundo Voxel Minecraft** – Motor isométrico completo con biomas y vegetación

### 🎮 Motor Voxel (Minecraft)
- Generación isométrica de mundos 20x20 chunks
- 5 biomas: Nieve, Desierto, Bosque, Llanura, Zonas de transición
- Sistema de altura con Perlin FBM multi-octava
- Vegetación: Árboles con copa procedural, Cactus con brazos aleatorios
- Minerales: Diamante, Oro, Obsidiana, Lava subterránea
- Iluminación con 3 caras por bloque (superior aclarada, lateral derecha oscurecida)
- Agua semi-transparente con múltiples niveles
- Cielo dinámico: gradiente atmosférico, sol con glow, nubes volumétricas, estrellas nocturnas

### 🎨 Pipeline de Renderizado
- **Fusión multi-algoritmo** con pesos normalizados
- **Paleta automática** por temas (ES/EN) o colores explícitos
- **Modos**: Oscuro (gamma 1.5), Suave (dithering Bayer), Caótico (ruido aditivo), Simétrico (espejo XY), Retro (cuantización 6 niveles)
- **Ajustes**: Saturación HSV, Intensidad multiplicativa, Escala de muestreo, Complejidad iterativa
- **Procesamiento paralelo** con reporte de progreso thread-safe

---

## 🚀 Uso Rápido

### Prompts (Español / English)
El parser detecta automáticamente idioma y compone algoritmos:
Naturaleza:
"bosque verde oscuro" / "forest green dark"
"ocean azul tranquilo" / "ocean blue calm"
"fuego intenso naranja" / "fire intense orange"
"nieve azul suave" / "snow blue soft"
"lava oscura rojo intenso" / "lava dark red intense"
Abstracto:
"fractal mandelbrot oscuro" / "mandelbrot fractal dark"
"plasma psicodelico neon" / "plasma psychedelic neon"
"mandala geometrico dorado" / "mandala geometric gold"
"cristal cian simetrico" / "crystal cyan symmetric"
Técnico:
"simplex warp detallado" / "simplex warp detailed"
"multifractal terreno erosionado" / "multifractal terrain eroded"
"celular patrones organicos" / "cellular patterns organic"
"raymarching esferas abstracto" / "raymarching spheres abstract"
"rizos turbulencia fluido" / "curl turbulence fluid"
Minecraft / Voxel:
"minecraft mundo bloques verde" / "minecraft world blocks green"
"voxel isometrico desierto" / "voxel isometric desert"
plain
Copy

### Modificadores Componibles
Se acumulan al prompt base:

| Modificador | Efecto matemático |
|-------------|-------------------|
| `oscuro` / `dark` | `Intensidad *= 0.7`, curva gamma 1.5 |
| `brillante` / `bright` | `Intensidad *= 1.4`, `Saturacion *= 1.3` |
| `suave` / `soft` / `calm` | `ModoSuave = true`, `Intensidad *= 0.8`, dithering activado |
| `intenso` / `intense` | `Intensidad *= 1.5`, `Saturacion *= 1.4` |
| `caotico` / `chaotic` | `ModoCaos = true`, `Complejidad *= 1.5`, octavas +3 |
| `simetrico` / `symmetric` | Espejo XY: `sx = nx < 0.5 ? nx*2 : (1-nx)*2` |
| `complejo` / `complex` | `Complejidad *= 2.0`, `Iteraciones += 100` |
| `simple` | `Complejidad *= 0.5`, `Escala *= 0.7` |
| `grande` / `big` | `Escala *= 0.5` (zoom out) |
| `pequeño` / `small` | `Escala *= 2.0` (zoom in) |
| `retro` / `vintage` | Cuantización a 6 niveles por canal |
| `neon` | `Saturacion *= 2.5`, forzar `ModoOscuro = true` |
| `detallado` / `detailed` | `Complejidad *= 1.8`, `Iteraciones += 200` |

---

## 🛠️ Compilación

### Requisitos Mínimos
- **.NET Framework 4.0+** (cualquier versión)
- **Compilador C# 5.0** compatible
- **Windows** (usa System.Windows.Forms y GDI+)

### Opciones de Compilación

#### #Develop / SharpDevelop (recomendado por el autor)
Archivo → Nuevo → Proyecto → Aplicación Windows Forms
Copiar Program.cs al proyecto
F9 (Compilar) o Ctrl+Shift+B
plain
Copy

#### csc.exe (compilador de línea de comandos de .NET)
```batch
:: Framework 4.x
%WINDIR%\Microsoft.NET\Framework\v4.0.30319\csc.exe ^
  /target:winexe ^
  /out:OpenCIP.exe ^
  /reference:System.dll ^
  /reference:System.Drawing.dll ^
  /reference:System.Windows.Forms.dll ^
  Program.cs

:: O simplemente (inferencia automática de referencias)
csc /target:winexe /out:OpenCIP.exe Program.cs
MSBuild (sin Visual Studio)
batch
Copy
:: Crear archivo OpenCIP.csproj mínimo o usar existente
msbuild OpenCIP.csproj /p:Configuration=Release /p:TargetFrameworkVersion=v4.0
Notas de Compatibilidad
Sin NuGet, sin DLLs externos, sin instaladores
Funciona en Windows XP SP3 con .NET 4.0 Client Profile
No requiere privilegios de administrador
Single-file deployment: solo OpenCIP.exe
📋 Arquitectura del Código
Estructura Monolítica (Single File)
plain
Copy
Program.cs (único archivo)
├── Enumeraciones
│   └── AlgoritmoBase (16 algoritmos)
│   └── ModoFusion (6 modos de mezcla)
│
├── ContextoVisual
│   ├── Configuración de pipeline (algoritmos, pesos, paleta)
│   └── Parámetros de renderizado (escala, intensidad, semilla)
│
├── BancoPalabras (Parser semántico)
│   ├── MapaColores: 22 colores con 3 tonos cada uno
│   ├── MapaTemas: 32 temas con composición de algoritmos
│   └── MapaModificadores: 24 modificadores de contexto
│
├── InterpretadorPrompt
│   └── Tokenización + aplicación de reglas semánticas
│
├── Matematica (Motor numérico)
│   ├── Perlin Noise (clásico, gradientes pseudo-aleatorios)
│   ├── Simplex Noise (implem. propia, gradientes 3D proyectados)
│   ├── FBM (Fractal Brownian Motion)
│   ├── Turbulencia con Rizos (Curl approximation)
│   ├── Dominio Warping (dominio distorsionado recursivo)
│   └── Utilidades: Lerp, Clamp, Smoothstep, HSV, etc.
│
├── Generadores (16 implementaciones)
│   ├── Perlin: FBM 5-8 octavas + detalle de alta frecuencia
│   ├── Fractal: Mandelbrot con suavizado logarítmico de iteraciones
│   ├── Fluido: Campo vectorial con advección y turbulencia multi-escala
│   ├── Geometrico: Armónicos angulares con simetría configurable
│   ├── Voronoi: Grid acceleration, distancia Manhattan/Euclidiana
│   ├── Onda: Superposición de fuentes con fase y decaimiento
│   ├── Nebulosa: FBM 3 capas + filamentos Simplex
│   ├── Plasma: Distorsión trigonométrica no lineal
│   ├── SimplexMejorado: FBM Simplex + gradientes para relieve
│   ├── Warping: Dominio warp doble pasada (caos adicional)
│   ├── Multifractal: Heterogeneidad con pesos por valor previo
│   ├── Rizos: Desplazamiento angular acumulativo
│   ├── Celular: Gray-Scott simplificado con difusión iterativa
│   ├── RayMarching: SDF esfera + ruido, marcha paso variable
│   ├── Implicitas: Metaballs acumulativos con detalle Simplex
│   └── (MundoVoxel es clase separada, no generador de píxeles)
│
├── GeneradorMundoVoxel
│   ├── Mapa de altura: Perlin FBM 4 octavas
│   ├── Biomas: Temperatura y humedad con FBM
│   ├── Post-procesamiento: Transiciones suaves entre biomas
│   ├── Renderizado isométrico: Proyección dimétrica 2:1
│   ├── Dibujado de bloques: 3 caras con iluminación diferencial
│   ├── Vegetación: Árboles recursivos, cactus con brazos
│   └── Efectos: Cielo degradado, sol, nubes, niebla atmosférica
│
├── MotorOpenCIP (Orquestador)
│   ├── Selección de ruta: Voxel vs Algorítmico
│   ├── Paralelización: Parallel.For por filas de píxeles
│   ├── Mezcla ponderada: Suma normalizada de algoritmos
│   ├── Post-procesado: Saturación, cuantización retro, dithering
│   └── Bloc de bloqueo: LockBits para acceso directo a memoria
│
├── PanelTags (Control personalizado)
│   └── Chips redondeados con paleta cíclica
│
└── VentanaPrincipal (UI WinForms)
    ├── Layout: Panel izquierdo fijo, canvas derecho adaptable
    ├── Controles: Prompt multilínea, ejemplos rápidos, semilla, zoom
    ├── Progreso: IProgress<T> con marshaling al UI thread
    └── Guardado: PNG/JPEG/BMP con nombre timestamped
🔧 API Programática
Uso Mínimo
csharp
Copy
using OpenCIP;

// 1. Interpretar prompt
var ctx = InterpretadorPrompt.Interpretar("espacio nebulosa purpura", semilla: -1);

// 2. Renderizar
Bitmap bmp = MotorOpenCIP.Renderizar(1024, 1024, ctx, progreso: null);

// 3. Usar
bmp.Save("output.png");
Configuración Manual Avanzada
csharp
Copy
var ctx = new ContextoVisual {
    // Pipeline algorítmico
    Algoritmos = new List<AlgoritmoBase> { 
        AlgoritmoBase.NebulosaEspacial,
        AlgoritmoBase.VoronoiCelular 
    },
    PesosAlgoritmos = new List<float> { 0.7f, 0.3f },
    
    // Paleta personalizada
    Paleta = new List<Color> { 
        Color.Black, 
        Color.FromArgb(75, 0, 130),   // Indigo
        Color.FromArgb(138, 43, 226), // BlueViolet  
        Color.White 
    },
    
    // Parámetros matemáticos
    Escala = 2.5,        // Frecuencia de muestreo
    Intensidad = 1.2,    // Multiplicador de amplitud
    Complejidad = 1.5,   // Afecta octavas e iteraciones
    Iteraciones = 200,   // Máximo para fractales/raymarching
    
    // Semilla determinística
    Semilla = 12345,
    
    // Modos de renderizado
    ModoOscuro = true,
    ModoSuave = false,
    ModoCaos = false,
    ModoSimetrico = true,
    ModoRetro = false,
    
    // Post-procesado
    Saturacion = 1.3     // 1.0 = neutral, >1 = saturar, <1 = desaturar
};
Progreso Asíncrono
csharp
Copy
var progreso = new Progress<int>(pct => {
    barraProgreso.Value = pct;
    lblEstado.Text = $"Generando... {pct}%";
});

// En background thread
await Task.Run(() => {
    var bmp = MotorOpenCIP.Renderizar(1920, 1080, ctx, progreso);
    // Marshal al UI thread para asignar a PictureBox
});
📝 Licencia
plain
Copy
OpenCIP – Open CPU Image Painter
Copyright (C) 2024  Turing Software / LexusYTG

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <https://www.gnu.org/licenses/>.
Autores:
Turing Software – Arquitectura, algoritmos de ruido, motor de fusión
LexusYTG – Implementación C# 5.0, motor voxel, UI WinForms, optimizaciones de renderizado
🐛 Notas Técnicas / Troubleshooting
Table
Copy
Síntoma	Causa	Solución
Imagen negra	ModoOscuro + paleta oscura	Añadir color blanco a paleta o reducir gamma
Rendimiento lento	Resolución alta + Iteraciones altas	Reducir a 512x512 o bajar Complejidad
Bandas de color	ModoRetro sin dithering	Activar ModoSuave para dithering Bayer
Mundo voxel plano	Escala muy bajo en zoom	Ajustar trackZoom a 1.0x-2.0x
Colores invertidos	Saturación negativa	Verificar Saturacion > 0.1
🎯 Roadmap / TODO
[ ] Soporte para animaciones (secuencias de TiempoAnimacion)
[ ] Exportación a heightmap RAW
[ ] Más biomas: Jungla, Pantano, Mesa, Tundra
[ ] Algoritmo de erosión hidráulica para terrenos
[ ] Shaders de post-procesado (bloom, SSAO aproximado)
<p align="center">
  <b>OpenCIP v1.0</b><br>
  <i>Código nativo. Cero dependencias. Matemáticas puras.</i><br>
  <sub>Hecho en SharpDevelop con C# 5.0</sub>
</p>
