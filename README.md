# 🎮 Nexus Omok Game

A classic Gomoku (Five in a Row) game built with .NET 8.0 and WPF for Windows Desktop.

![.NET Version](https://img.shields.io/badge/.NET-8.0-512BD4)
![Platform](https://img.shields.io/badge/Platform-Windows-0078D4)
![License](https://img.shields.io/badge/License-MIT-green)

## 📖 Overview

**Nexus Omok Game** is a traditional two-player board game where players take turns placing black and white stones on a 15x15 grid. The objective is to be the first to get exactly five stones in a row, either horizontally, vertically, or diagonally.

## ✨ Features

### Core Gameplay
- **15x15 Standard Board** - Classic Gomoku board size with clear grid lines
- **Two-Player Local Mode** - Play with friends or family on the same PC
- **Turn-Based System** - Black stone always starts first, automatic turn switching
- **Win Detection** - Automatically detects when exactly 5 stones are aligned
- **Draw Detection** - Recognizes when the board is full with no winner
- **Winning Visualization** - Red line highlights the winning five stones

### User Interface
- **Hover Preview** - Semi-transparent stone preview when hovering over valid positions
- **Current Turn Indicator** - Clear display of whose turn it is (⚫ Black / ⚪ White)
- **Status Messages** - Real-time game status updates at the bottom
- **New Game Button** - Easily restart the game at any time
- **Responsive Design** - Maintains proper proportions when resizing the window

### Visual Effects
- **Gradient Stones** - 3D-like effect on black and white stones
- **Star Points** - Traditional board markers (천원점) at key intersections
- **Sound Feedback** - System beep when placing stones
- **Smooth Animations** - Professional-looking UI transitions

## 🎯 Game Rules

1. **Board Size**: 15x15 grid (225 intersections)
2. **Starting Player**: Black stone always plays first
3. **Winning Condition**: Exactly 5 consecutive stones in a row
   - Horizontal, vertical, or diagonal alignment
   - **6 or more stones do not count as a win** (육목 금지)
4. **Simplified Rules**: No forbidden moves (쌍삼, 쌍사 금지 룰 미적용)
5. **Draw**: Game ends in a draw if the board is completely filled

## 🚀 Getting Started

### Prerequisites

- **Windows 10/11** (64-bit recommended)
- **.NET 8.0 SDK** or Runtime
- **Visual Studio 2022** (for development)
  - '.NET desktop development' workload required

### Installation

#### Option 1: Download Release (Coming Soon)
1. Download the latest release from the [Releases](../../releases) page
2. Extract the ZIP file
3. Run `Nexus-Omok-Game.exe`

#### Option 2: Build from Source
```bash
# Clone the repository
git clone https://github.com/yourusername/Nexus-Omok-Game.git
cd Nexus-Omok-Game

# Build the project
dotnet build

# Run the application
dotnet run
```

#### Option 3: Using Visual Studio
1. Open `Nexus-Omok-Game.sln` in Visual Studio 2022
2. Press `F5` or click the **Start** button to build and run

## 🎮 How to Play

1. **Launch the Game** - Start the application
2. **Make Your Move**
   - Hover your mouse over any empty intersection
   - See a preview of where your stone will be placed
   - Click to place your stone
3. **Win the Game** - Align exactly 5 stones in a row before your opponent
4. **Start Over** - Click the **🔄 New Game** button to play again

### Controls

| Action | Control |
|--------|---------|
| Place Stone | Left Mouse Click |
| Preview Position | Mouse Hover |
| New Game | Click "🔄 New Game" button |
| Confirm Restart | Yes/No dialog |

## 🏗️ Technical Stack

- **Framework**: .NET 8.0
- **UI Technology**: WPF (Windows Presentation Foundation)
- **Language**: C# 12.0
- **IDE**: Visual Studio 2022
- **Architecture**: MVVM-inspired pattern with code-behind

## 📁 Project Structure

```
Nexus-Omok-Game/
├── App.xaml                    # Application entry point
├── App.xaml.cs                 # Application logic
├── MainWindow.xaml             # Main game window UI
├── MainWindow.xaml.cs          # Game logic and event handlers
├── Nexus-Omok-Game.csproj     # Project configuration
├── PRD.md                      # Product Requirements Document
└── README.md                   # This file
```

## 🎨 UI Components

### Main Window Layout
- **Header** (Top)
  - Current turn indicator
  - New Game button
- **Game Board** (Center)
  - 15x15 grid canvas
  - Interactive stone placement
  - Hover effects
- **Status Bar** (Bottom)
  - Game status messages
  - Win/draw announcements

### Color Scheme
- **Board Background**: Wheat/Tan (#DEB887)
- **Board Border**: Saddle Brown (#8B4513)
- **Grid Lines**: Black
- **Black Stones**: Gradient from Dark Gray to Black
- **White Stones**: Gradient from White to Light Gray
- **Winning Line**: Red (70% opacity)

## 🔧 Configuration

### Game Constants
You can modify these constants in `MainWindow.xaml.cs`:

```csharp
private const int BOARD_SIZE = 15;        // Board dimensions (15x15)
private const double CELL_SIZE = 40.0;    // Size of each cell in pixels
private const double STONE_RADIUS = 15.0; // Radius of game stones
private const double BOARD_MARGIN = 20.0; // Margin around the board
```

## 🐛 Known Issues

- Sound may not play on some systems (gracefully handled)
- Window resizing maintains aspect ratio via Viewbox

## 🔮 Future Enhancements

- [ ] AI opponent (single-player mode)
- [ ] Game history and move replay
- [ ] Undo/Redo functionality
- [ ] Custom board themes
- [ ] Save/Load game state
- [ ] Online multiplayer support
- [ ] Tournament mode
- [ ] Move timer
- [ ] Statistics and leaderboards

## 📝 Development

### Building the Project
```bash
# Restore dependencies
dotnet restore

# Build in Debug mode
dotnet build

# Build in Release mode
dotnet build -c Release

# Run tests (if available)
dotnet test
```

### Code Style
- Follow C# naming conventions
- Use meaningful variable names
- Add XML documentation comments for public methods
- Keep methods focused and concise

## 🤝 Contributing

Contributions are welcome! Please follow these steps:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

**Enjoy playing Nexus Omok Game! 🎮⚫⚪**
