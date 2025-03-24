# Economic Cycles: Updated Architecture Document

## Overview

This document outlines the architecture for Economic Cycles, a roguelike grand strategy game focused on economic management, regional development, and adaptation to cyclical economic patterns. The architecture combines elements of Entity Component System (ECS) for game logic, Model-View-Controller (MVC) for UI interactions, and an event-driven communication system to maintain loose coupling between systems.

## Core Architecture Philosophy

- **Modular Systems**: Each system is self-contained and communicates through events
- **Data-Driven Design**: Game parameters and configurations stored in ScriptableObjects for easy tuning
- **Loose Coupling**: Systems interact without direct dependencies via the EventBus
- **Scalable Structure**: Foundation supports future additions without major refactoring
- **Playable First Approach**: Focus on getting core mechanics working before adding complexity

## Architecture Components

### 1. Data Layer (ScriptableObjects)

ScriptableObjects store static configuration data, making it easy to modify and tune game parameters:

| ScriptableObject | Purpose | Status |
|------------------|---------|--------|
| **NationDataSO** | Base templates for nations with starting values | Implemented |
| **TerrainTypeDataSO** | Terrain types with economic modifiers | Implemented |
| **RegionTypeDataSO** | Region specializations and characteristics | Implemented |
| **ProjectDataSO** | Infrastructure projects with costs and effects | Implemented |
| **EconomicCycleDataSO** | Configuration for cycle phases and effects | Implemented |
| **ResourceDataSO** | Resource types, values, and production chains | Implemented |
| **TerrainMapDataSO** | Saved terrain layouts for map generation | Implemented |
| **ResourceBalanceDataSO** | Balance settings for the economic system | Implemented |

### 2. Entity Component System (Game Logic)

The ECS pattern handles the dynamic simulation aspects:

#### Entities (Domain Objects)

| Entity | Purpose | Status |
|--------|---------|--------|
| **RegionEntity** | Represents a geographic area with economic data | Implemented |
| **NationEntity** | Groups regions into political units | Planned |
| **ProjectEntity** | Represents in-progress infrastructure development | Planned |

#### Components (Data Containers)

| Component | Purpose | Status |
|-----------|---------|--------|
| **ResourceComponent** | Manages resource quantities and production/consumption | Implemented |
| **ProductionComponent** | Handles production efficiency and conversion | Planned |
| **InfrastructureComponent** | Tracks infrastructure levels by category | Planned |
| **PopulationComponent** | Manages demographics and labor allocation | Planned |
| **EconomicCycleComponent** | Tracks cycle effects on region performance | Planned |

#### Systems (Logic Processors)

| System | Purpose | Status |
|--------|---------|--------|
| **EconomicSystem** | Calculates production, consumption, and wealth changes | Implemented |
| **MapSystem** | Manages spatial relationships and territory | Implemented |
| **ProjectSystem** | Processes ongoing projects and applies effects | Planned |
| **EconomicCycleSystem** | Advances economic cycles and applies modifiers | Planned |
| **ResourceTradeSystem** | Handles resource movement between regions | Planned |

### 3. MVC Pattern (User Interface)

The MVC pattern handles player interaction and visualization:

#### Models

| Model | Purpose | Status |
|-------|---------|--------|
| **MapModel** | Central data model managing all regions | Implemented |
| **EconomicCycleModel** | Tracks current cycle phase and effects | Planned |
| **ResourceTradeModel** | Models resource flow and prices | Planned |

#### Views

| View | Purpose | Status |
|------|---------|--------|
| **MapView** | Renders the game map with region visualizations | Implemented |
| **RegionInfoUI** | Displays detailed region information panel | Implemented |
| **EconomicDashboard** | Shows national indicators and cycle info | Planned |
| **ResourceDashboardView** | Displays resource production/consumption | Planned |

#### Controllers

| Controller | Purpose | Status |
|------------|---------|--------|
| **MapController** | Handles map interaction and selection | Implemented |
| **CameraController** | Manages map navigation and zoom | Implemented |
| **AIController** | Manages AI nation decision-making | Implemented |
| **ProjectController** | Handles project selection and placement | Planned |

### 4. Event System (Communication Layer)

The EventBus provides a central communication mechanism for decoupled systems:

| Key Event | Triggered By | Subscribed By |
|-----------|--------------|---------------|
| **RegionSelected** | MapController | RegionInfoUI, MapView |
| **RegionUpdated** | RegionEntity | MapView, RegionInfoUI |
| **TurnEnded** | TurnManager | EconomicSystem, MapModel |
| **PlayerTurnEnded** | TurnManager | AIController |
| **AITurnsCompleted** | AIController | TurnManager |
| **EconomicCycleChanged** | EconomicCycleSystem | Multiple Systems |
| **RegionCreated** | MapModel | EconomicSystem, MapView |

## Implementation Plan

### Phase 1: Core Game Loop (Completed)
- ✅ ScriptableObjects for nations, regions, and terrain
- ✅ Basic region and map visualization
- ✅ Turn-based gameplay with EventBus communication
- ✅ Procedural map generation with terrain effects
- ✅ Camera controls and basic UI

### Phase 2: Economic Simulation (In Progress)
- ✅ Resource component with production/consumption
- ✅ Terrain effects on resource production
- ✅ Basic economic cycles framework
- ⏳ Project system for region development
- ⏳ Resource visualization in UI

### Phase 3: Strategic Depth (Planned)
- Production chains and resource transformation
- Infrastructure development with compounding effects
- Trade networks and inter-region resource movement
- Population system with class dynamics
- Advanced AI decision-making

### Phase 4: Roguelike Elements (Future)
- Run structure with meta-progression
- Generational shifts between runs
- Random events and crisis management
- Unlockable starting conditions and policies
- Historical era transitions

## Resource System Design

The resource system is the cornerstone of the economic simulation:

### Resource Components
- **Storage**: Tracks current quantities of resources
- **Production**: Calculates base resource generation per turn
- **Consumption**: Determines resource usage per turn
- **Trade**: Handles resource import/export (future)

### Resource Categories
- **Primary**: Raw materials harvested directly (Food, Wood, Minerals)
- **Secondary**: Products requiring processing (Iron, Textiles, Building Materials)
- **Tertiary**: Complex goods requiring multiple inputs (Fine Food, Luxury Goods)
- **Abstract**: Non-physical resources (Wealth, Research, Culture)

### Production Chain Logic
```
Primary Resources → Secondary Resources → Tertiary Resources
     ↓                    ↓                     ↓
Terrain Type       Infrastructure Level      Population
Modifiers          Efficiency Bonuses        Consumption
     ↓                    ↓                     ↓
Economic Cycle    Production Recipes      Market Demand
Effects           Resource Requirements    Price Effects
```

## Economic Cycle System

The economic cycle system provides strategic depth through time-based phases:

### Cycle Phases
1. **Expansion**: High growth, increasing prices, good for investment
2. **Peak**: Maximum productivity, stable prices, resource shortages begin
3. **Contraction**: Reduced output, falling prices, projects become costlier
4. **Recovery**: Stabilization, bargain prices, preparation for next expansion

### Phase Effects
- Sector-specific production modifiers
- Resource consumption changes
- Project costs and completion time adjustments
- Population growth and migration effects

## Project Structure

The folder structure follows a logical organization that separates concerns:

```
Assets/
  ├── Prefabs/
  │    ├── Map/
  │    ├── UI/
  │    └── Effects/
  ├── ScriptableObjects/
  │    ├── Nations/
  │    ├── Regions/
  │    ├── Projects/
  │    ├── Resources/
  │    ├── TerrainTypes/
  │    ├── EconomicCycles/
  │    └── Maps/
  ├── Scripts/
  │    ├── 1_Core/
  │    │    ├── GameManager.cs
  │    │    └── TurnManager.cs
  │    ├── 2_Data/
  │    │    ├── ScriptableObjects/
  │    │    └── Models/
  │    ├── 3_Entities/
  │    │    ├── RegionEntity.cs
  │    │    └── NationEntity.cs
  │    ├── 4_Components/
  │    │    ├── ResourceComponent.cs
  │    │    └── ProductionComponent.cs
  │    ├── 5_Systems/
  │    │    ├── EconomicSystem.cs
  │    │    ├── MapSystem.cs
  │    │    └── EconomicCycleSystem.cs
  │    ├── 6_UI/
  │    │    ├── Views/
  │    │    └── Components/
  │    ├── 7_Controllers/
  │    │    ├── MapController.cs
  │    │    ├── CameraController.cs
  │    │    └── AIController.cs
  │    ├── 8_Managers/
  │    │    └── EventBus.cs
  │    └── 9_Utils/
  │         ├── Helpers/
  │         └── Extensions/
  └── Scenes/
       └── MainGame.unity
```

## Technical Implementation Details

### EventBus Implementation

The current EventBus uses a static dictionary of delegates to allow systems to communicate without direct references:

```csharp
// Simplified example
public static class EventBus
{
    private static Dictionary<string, Action<object>> eventDictionary = 
        new Dictionary<string, Action<object>>();
    
    public static void Subscribe(string eventName, Action<object> listener) { ... }
    public static void Unsubscribe(string eventName, Action<object> listener) { ... }
    public static void Trigger(string eventName, object data = null) { ... }
}
```

### Procedural Generation

The map generation system uses:

1. **Noise Generation**: Perlin noise creates natural-looking terrain
2. **Terrain Assignment**: Elevation and moisture determine terrain types
3. **Nation Placement**: Strategic placement of nations with expansion logic
4. **Region Creation**: Regions within nations with terrain-based properties

### Resource Component

The ResourceComponent currently handles:

1. Resource storage with quantities
2. Base production and consumption rates
3. Terrain-based production modifiers
4. Turn-by-turn resource processing

## Roadmap & Next Steps

1. **Short Term (Next 1-2 Weeks)**
   - Complete ResourceComponent with production chains
   - Implement infrastructure levels with compounding effects
   - Add project creation and completion system
   - Enhance UI to show more economic details

2. **Medium Term (Next Month)**
   - Implement full economic cycle system with phase transitions
   - Add trade mechanics between regions
   - Create population system with consumption needs
   - Implement basic random events

3. **Long Term (2-3 Months)**
   - Add roguelike run structure
   - Implement meta-progression between runs
   - Add multiple victory conditions
   - Create narrative events and decision points

## Conclusion

The Economic Cycles architecture provides a solid foundation for the game's core economic simulation while maintaining flexibility for future enhancements. The modular design using an event-driven approach allows for incremental development and testing, with each system isolated enough to evolve independently yet connected through the central EventBus.

The current implementation focuses on establishing the core gameplay loop and resource management systems. As development progresses, additional layers of strategic depth will be added through economic cycles, production chains, infrastructure development, and roguelike meta-progression.
