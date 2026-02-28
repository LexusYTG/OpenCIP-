# 🎨 OpenCIP – Open CPU Image Painter

[![License: GPL v3](https://img.shields.io/badge/License-GPLv3-blue.svg)](https://www.gnu.org/licenses/gpl-3.0)
[![.NET Framework](https://img.shields.io/badge/.NET-4.x-green.svg)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-5.0-239120.svg)](https://docs.microsoft.com/en-us/dotnet/csharp/)

> Generador de imágenes algorítmicas procedural con estilo Minecraft voxel y múltiples algoritmos de ruido matemático.

![OpenCIP Logo](https://via.placeholder.com/800x400/0a0a0f/00bef0?text=OpenCIP+-+Generación+Procedural)

---

## ✨ Características

### 🖼️ Algoritmos de Generación
- **Ruido Perlin** – Generación orgánica de terrenos y texturas
- **Ruido Simplex** – Variante mejorada del Perlin, más eficiente
- **Fractal Mandelbrot** – Exploración del conjunto de Mandelbrot con suavizado
- **Fluido Turbulento** – Simulación de fluidos con advección vectorial
- **Voronoi Celular** – Diagramas de Voronoi con distancia F2-F1
- **Nebulosa Espacial** – Generación de nebulosas con múltiples octavas
- **Plasma Caótico** – Patrones psicodélicos e interferencias
- **Dominio Warping** – Distorsión espacial no lineal
- **Multifractal** – Terrenos heterogéneos realistas
- **Turbulencia con Rizos** – Aproximación a Curl Noise
- **Patrones Celulares** – Simulación de reacción-difusión (Gray-Scott)
- **Ray Marching 2D** – Renderizado de distancias firmes
- **Superficies Implícitas** – Metaballs y campos escalares

### 🎮 Modo Mundo Voxel (Minecraft)
- Generación isométrica de mundos tipo Minecraft
- Múltiples biomas: Nieve, Desierto, Bosque, Llanura
- Vegetación procedural: Árboles, Cactus, Cuevas
- Minerales: Diamante, Oro, Obsidiana, Lava
- Sistema de iluminación con sombras y transparencias
- Cielo dinámico con sol y nubes

### 🎨 Características Visuales
- **Paleta de colores** automática según temas
- **Modos de renderizado**: Oscuro, Suave, Caótico, Simétrico, Retro
- **Ajustes en tiempo real**: Saturación, Intensidad, Escala, Complejidad
- **Dithering** Bayer para suavizado de bandas
- **Cuantización retro** con niveles configurables

---

## 🚀 Uso Rápido

### Prompts Soportados
Escribe en español o inglés para generar imágenes:

```bash
# Naturaleza
"bosque verde oscuro"
"ocean azul tranquilo" 
"fuego intenso naranja"
"nieve azul suave"

# Abstracto
"fractal mandelbrot oscuro"
"plasma psicodelico neon"
"mandala geometrico dorado"

# Voxel/Minecraft
"minecraft mundo bloques"
"voxel isometrico desierto"

# Técnicos
"simplex warp detallado"
"multifractal terreno erosionado"
"raymarching esferas abstracto"
Modificadores
Añade calificadores para ajustar el resultado:
Table
Copy
Modificador	Efecto
oscuro / dark	Reduce intensidad, modo nocturno
brillante / bright	Aumenta saturación e intensidad
suave / soft	Modo suavizado, menos contraste
intenso / intense	Máxima saturación y caos
caotico / chaotic	Aumenta complejidad aleatoria
simetrico / symmetric	Simetría especular
retro / vintage	Paleta limitada, estilo 8-bit
neon	Saturación extrema, fondo oscuro
🛠️ Compilación
Requisitos
.NET Framework 4.x o superior
Visual Studio 2015+ / MSBuild
Windows 7/8/10/11
Compilar desde código fuente
bash
Copy
# Clonar repositorio
git clone https://github.com/turing-software/opencip.git
cd opencip

# Compilar con MSBuild
msbuild OpenCIP.csproj /p:Configuration=Release

# O con Visual Studio
devenv OpenCIP.sln /Build Release
Ejecutar
bash
Copy
# Desde la carpeta bin/Release
./OpenCIP.exe
📋 Estructura del Proyecto
plain
Copy
OpenCIP/
├── Program.cs              # Código fuente monolítico principal
├── Algoritmos/             # (En Program.cs)
│   ├── Ruido Perlin/Simplex
│   ├── Fractales
│   ├── Fluidos
│   ├── Voronoi
│   └── ...
├── Generadores/            # Implementaciones específicas
│   ├── GeneradorMundoVoxel # Motor Minecraft isométrico
│   └── MotorOpenCIP        # Renderizador principal
├── UI/
│   ├── VentanaPrincipal    # Interfaz WinForms
│   ├── PanelTags          # Chips de interpretación
│   └── Controles personalizados
└── Recursos/
    └── Paletas de colores predefinidas
🔧 API Interna
ContextoVisual
Clase principal de configuración de generación:
csharp
Copy
var ctx = new ContextoVisual {
    Algoritmos = new List<AlgoritmoBase> { 
        AlgoritmoBase.RuidoPerlin,
        AlgoritmoBase.VoronoiCelular 
    },
    PesosAlgoritmos = new List<float> { 0.6f, 0.4f },
    Paleta = new List<Color> { Color.DarkBlue, Color.Cyan, Color.White },
    Escala = 2.5,
    Intensidad = 1.2,
    Complejidad = 1.5,
    Semilla = 12345,
    ModoOscuro = true,
    Iteraciones = 200
};
Generar Imagen Programáticamente
csharp
Copy
// Interpretar prompt
var ctx = InterpretadorPrompt.Interpretar("espacio nebulosa purpura", -1);

// Renderizar
Bitmap imagen = MotorOpenCIP.Renderizar(1024, 1024, ctx, null);

// Guardar
imagen.Save("output.png", ImageFormat.Png);
📝 Licencia
GNU General Public License v3.0
Copyright (c) 2024 Turing Software / LexusYTG
Este programa es software libre: puedes redistribuirlo y/o modificarlo bajo los términos de la Licencia Pública General de GNU publicada por la Free Software Foundation, ya sea la versión 3 de la Licencia, o (a tu elección) cualquier versión posterior.
Este programa se distribuye con la esperanza de que sea útil, pero SIN NINGUNA GARANTÍA; incluso sin la garantía implícita de COMERCIABILIDAD o APTITUD PARA UN PROPÓSITO PARTICULAR. Consulta la Licencia Pública General de GNU para más detalles.
Deberías haber recibido una copia de la Licencia Pública General de GNU junto con este programa. Si no es así, visita https://www.gnu.org/licenses/.
👥 Autores
Turing Software – Arquitectura y algoritmos principales
LexusYTG – Implementación de motores de ruido y voxel
🤝 Contribuciones
Las contribuciones son bienvenidas. Por favor:
Fork el repositorio
Crea una rama (git checkout -b feature/nueva-caracteristica)
Commit tus cambios (git commit -am 'Añadir nueva característica')
Push a la rama (git push origin feature/nueva-caracteristica)
Abre un Pull Request
📸 Galería de Ejemplos
Table
Copy
Prompt	Descripción
galaxia purpura espacio oscuro	Nebulosa con estrellas y polvo cósmico
minecraft bosque isometrico	Mundo voxel con árboles y relieve
fractal mandelbrot zoom	Exploración profunda del conjunto
plasma psicodelico neon	Colores vibrantes tipo ácido
oceano ondas tranquilas	Agua realista con reflejos
🔗 Enlaces
Repositorio GitHub
Reportar Issues
GPL v3
Documentación .NET
<p align="center">
  <i>Generado con ❤️ y muchas matemáticas por Turing Software</i><br>
  <sub>OpenCIP v1.0 – La CPU es tu lienzo</sub>
</p>
