# Othello Sample Game

This sample demonstrates how to build a complete turn-based multiplayer game using the Midnite Oil Software Multiplayer framework.

## What's Included

- **Complete Othello/Reversi game implementation** with full multiplayer support
- **Turn-based multiplayer gameplay** synchronized across all clients
- **Network synchronized game board** with 8x8 grid
- **Player chip color assignment** - random assignment of Black/White
- **Move validation and chip flipping logic** - all 8 directions
- **Game UI** with turn indicators, chip counts, and game controls
- **Pass and resign functionality** for edge cases
- **Rematch system** to play again without leaving the session

## Requirements

- Unity 6000.0 or later
- Midnite Oil Software Core package installed
- Netcode for GameObjects (2.0.0+)
- Unity Gaming Services configured (Authentication, Relay, Lobby)
- Unity Input System

## Installation

1. Import this sample from the Package Manager
2. Ensure the Core package is already installed
3. Configure your Unity Gaming Services project
4. Add scenes to Build Settings in this order:
   - Bootstrapper (from Multiplayer package)
   - Main Menu (from Multiplayer package)
   - Othello (from this sample)

## Components Overview

### Core Scripts

- **OthelloGameManager.cs** - Extends `GameManager` for Othello-specific logic
  - Manages player chip colors using `NetworkList`
  - Tracks player pass state
  - Handles turn transitions and game over conditions
  
- **OthelloBoard.cs** - Board representation and game logic
  - 8x8 grid of cells
  - Move validation (checks all 8 directions for surrounded chips)
  - Input handling with raycasting
  - Network synchronization via RPCs
  
- **OthelloPlayer.cs** - Player representation extending `NetworkPlayer`

- **Cell.cs** - Individual board cell component
  - Position tracking (X, Y)
  - Chip reference
  - Visual highlighting for valid/invalid moves
  
- **Chip.cs** - Game piece with flip animation
  - Color property (Black/White)
  - Flip animation
  
- **GameUI.cs** - User interface controller
  - Turn indicator
  - Chip count display
  - Pass/Resign buttons
  - Game over screen with results

### Events

- **ChipDroppedEvent.cs** - Raised when a chip is placed on the board

### Assets

- **Models**: Board and disc 3D models
- **Materials**: Board texture, chip materials, highlight materials
- **Textures**: Wood texture, board texture, disc sprites
- **Prefabs**: Cell, Chip, OthelloPlayer

## How to Use

1. **Play the Main Menu scene** in the editor
2. **Sign in** using anonymous authentication or username/password
3. **Create a lobby** or join an existing one
4. **Wait for 2 players** to join (required for Othello)
5. **Start the game** (host only) to load the Othello scene
6. **Play Othello**:
   - Hover over cells to see valid moves
   - Click to place your chip
   - Red highlight = invalid move
   - Green highlight = chips that will be flipped
   - Pass if you have no legal moves
   - Game ends when board is full or both players pass

## Code Structure

The sample follows the Midnite Oil Software coding standards:
- Private fields prefixed with underscore: `_boardSize`
- Public properties in PascalCase: `CurrentPlayer`
- Use of `var` for obvious types
- Early-exit guard clauses
- `Logwin.Log()` for debug logging with `_enableDebugLog` flag
- Event-driven architecture using `EventBus`

## Extending the Sample

This sample serves as a template for building other turn-based multiplayer games. Consider:
- Adapting the board size and rules for Chess, Checkers, Go
- Adding AI opponent for single-player mode
- Implementing move hints or tutorial system
- Adding chat or emote system
- Creating tournament or ranked modes

## Troubleshooting

**Chips not appearing**: Ensure all clients have joined before starting the game

**Invalid moves not highlighted**: Check that raycasting layers are set correctly

**Game not starting**: Verify Unity Gaming Services are configured and you're signed in

**Disconnection issues**: Check your Relay allocation and network connectivity

For more help, see the main package documentation.
