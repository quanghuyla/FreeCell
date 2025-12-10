# FreeCell Solitaire

## Game Description

FreeCell is a solitaire card game where all 52 cards are dealt face-up into 8 columns. The goal is to move all cards to the four foundation piles, organized by suit from Ace to King.

## Features Implemented

### Core Gameplay
- Full FreeCell rules implementation
- Drag and drop card movement, double click for automove.
- Valid move detection
- Win condition checking
- Move counter

### User Interface
- Main menu with Start Game, Theme Selection, and Rules
- Three color themes (Green, Blue, Red)
- Responsive UI layout
- Win screen display

### Polish
- Card sprite visuals for all 52 cards
- Smooth card animations
- Win celebration effect
- Undo/Redo functionality

## Technical Implementation

**Key Scripts:**
- `GameManager.cs` - Game state and setup
- `Card.cs` - Card properties and visuals
- `DragDrop.cs` - Card movement system
- `TableauColumn.cs`, `FreeCell.cs`, `Foundation.cs` - Container logic
- `CardSpriteManager.cs` - Sprite loading system
- `UIManager.cs` - UI updates and win screen
- `MainMenuManager.cs` - Menu navigation

**Architecture:**
- Singleton pattern for managers
- Interface-based container system (ICardContainer)
- Event-driven move validation
- Stack-based undo/redo system
