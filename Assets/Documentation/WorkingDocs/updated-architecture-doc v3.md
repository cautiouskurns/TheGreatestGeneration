# Economic Cycles: Updated Architecture Document (2025)

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
| **NationTemplate** | Templates for procedural nation generation | Implemented |

### 2. Entity Component System (Game Logic)

The ECS pattern handles the dynamic simulation aspects:

#### Entities (Domain Objects)

| Entity | Purpose | Status |
|--------|---------|--------|
| **RegionEntity** | Represents a geographic area with economic data | Implemented |
| **NationEntity** | Groups regions into political units | Implemented |
| **ProjectEntity** | Represents in-progress infrastructure development | Partial |

#### Components (Data Containers)

| Component | Purpose | Status |
|-----------|---------|--------|
| **ResourceComponent** | Manages resource quantities and production/consumption | Implemented |
| **ProductionComponent** | Handles production efficiency and conversion | Implemented |
| **InfrastructureComponent** | Tracks infrastructure levels by category | Planned |
| **PopulationComponent** | Manages demographics and labor allocation | Planned |
| **EconomicCycleComponent** | Tracks cycle effects on region performance | Planned |

#### Systems (Logic Processors)

| System | Purpose | Status |
|--------|---------|--------|
| **EconomicSystem** | Calculates production, consumption, and wealth changes | Implemented |
| **MapSystem** | Manages spatial relationships and territory | Implemented |
| **ProjectSystem** | Processes ongoing projects and applies effects | Partial |
| **EconomicCycleSystem** | Advances economic cycles and applies modifiers | Planned |
| **ResourceTradeSystem** | Handles resource movement between regions | Planned |

### 3. MVC Pattern (User Interface)

The MVC pattern handles player interaction and visualization:

#### Models

| Model | Purpose | Status |
|-------|---------|--------|
| **MapModel** | Central data model managing all regions | Implemented |
| **NationModel** | Manages nations and their aggregated properties | Implemented |
| **EconomicCycleModel** | Tracks current cycle phase and effects | Planned |
| **ResourceTradeModel** | Models resource flow and prices | Planned |

#### Views

| View | Purpose | Status |
|------|---------|--------|
| **MapView** | Renders the game map with region visualizations | Implemented |
| **RegionInfoUI** | Displays detailed region information panel | Implemented |
| **NationDashboardUI** | Shows nation-level statistics | Implemented |
| **AIActionDisplay** | Displays AI actions with visual feedback | Implemented |
| **EconomicDashboard** | Shows economic indicators and cycle info | Planned |
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
| **RegionCreated** | MapModel | EconomicSystem, MapView |
| **TurnEnded** | TurnManager | EconomicSystem, MapModel |
| **TurnStarted** | TurnManager | Various Systems |
| **TurnProcessed** | TurnManager | UI Components |
| **PlayerTurnEnded** | TurnManager | AIController |
| **AITurnsCompleted** | AIController | TurnManager |
| **EconomicSystemReady** | EconomicSystem | MapView |
| **NationUpdated** | NationEntity | NationDashboardUI |
| **NationSelected** | MapController | NationDashboardUI |
| **NationModelUpdated** | NationModel | UI Components |
| **EconomicCycleChanged** | EconomicCycleSystem | Multiple Systems |

## Implementation Progress

### Completed Features

1. **Core Game Loop**
   - ✅ ScriptableObjects for nations, regions, and terrain
   - ✅ Basic region and map visualization with terrain coloring
   - ✅ Turn-based gameplay with EventBus communication
   - ✅ Procedural map generation with terrain effects
   - ✅ Camera controls with pan and zoom

2. **Economic Simulation**
   - ✅ Resource component with production/consumption rates
   - ✅ Terrain effects on resource production
   - ✅ Basic product recipes and conversion
   - ✅ Economic cycles data structure
   - ✅ Wealth and production tracking

3. **Nation Management**
   - ✅ Nation entities with region grouping
   - ✅ Nation dashboard UI
   - ✅ Aggregated national statistics
   - ✅ Nation templates for procedural generation
   - ✅ AI nation decision making

4. **UI Systems**
   - ✅ Region information panel with detailed stats
   - ✅ Visual feedback for economic changes
   - ✅ AI action display with animated notifications
   - ✅ Turn management UI

### In-Progress Features

1. **Production Chains**
   - ⏳ Recipe system for resource transformation (Basic implemented)
   - ⏳ Advanced production dependencies
   - ⏳ Production efficiency modifiers

2. **Project System**
   - ⏳ Project creation and selection interface
   - ⏳ Project progress tracking
   - ⏳ Project completion effects

3. **Resource Visualization**
   - ⏳ Resource flow diagrams
   - ⏳ Production and consumption charts
   - ⏳ Trend analysis for resources

### Planned Features

1. **Economic Cycles**
   - 📋 Full cycle phase transitions
   - 📋 Phase-specific modifiers to production and consumption
   - 📋 Visual indicators of current cycle phase
   - 📋 Strategic opportunities in different phases

2. **Trade System**
   - 📋 Inter-region resource movement
   - 📋 Regional trade specialization
   - 📋 Trade routes and infrastructure
   - 📋 Market price fluctuations

3. **Population System**
   - 📋 Population growth and migration
   - 📋 Social classes with different consumption needs
   - 📋 Population satisfaction effects
   - 📋 Labor allocation to sectors

## Technical Implementation Details

### Resource System

The `ResourceComponent` has been significantly expanded:

```
┌─────────────────────────────────────┐
│ ResourceComponent                   │
├─────────────────────────────────────┤
│ - resources: Dictionary<string,float>│
│ - productionRates: Dictionary        │
│ - consumptionRates: Dictionary       │
│ - resourceDefinitions: Dictionary    │
├─────────────────────────────────────┤
│ + AddResource(string, float)         │
│ + RemoveResource(string, float)      │
│ + GetResourceAmount(string)          │
│ + ProcessTurn(wealth, size)          │
│ + CalculateBaseProduction()          │
│ + CalculateConsumption(wealth, size) │
│ + GetConsumptionSatisfaction()       │
│ + GetOverallSatisfaction()           │
└─────────────────────────────────────┘
```

Key features include:
- Resource storage with dynamic quantities
- Production and consumption rates calculated based on multiple factors
- Consumption affected by region wealth and size
- Resource satisfaction tracking
- Category-based consumption scaling
- Integration with terrain types for production bonuses

### Production System

The `ProductionComponent` manages resource transformation:

```
┌─────────────────────────────────────┐
│ ProductionComponent                 │
├─────────────────────────────────────┤
│ - activeRecipes: List<string>       │
│ - productionEfficiency: float       │
│ - recipeProgress: Dictionary         │
├─────────────────────────────────────┤
│ + ProcessProduction()               │
│ + CanProduceRecipe(recipe)          │
│ + ActivateRecipe(string)            │
│ + DeactivateRecipe(string)          │
│ + GetRecipeProgress(string)         │
│ + GetAvailableRecipes()             │
└─────────────────────────────────────┘
```

Key features include:
- Recipe-based resource transformation
- Multi-turn production tracking
- Production efficiency modifiers
- Resource consumption for production
- Recipe activation/deactivation

### Nation System

The `NationEntity` and `NationModel` classes track nation-level data:

```
┌─────────────────────────────────────┐
│ NationEntity                        │
├─────────────────────────────────────┤
│ - nationName: string                │
│ - nationColor: Color                │
│ - regions: List<RegionEntity>       │
│ - aggregatedResources: Dictionary    │
├─────────────────────────────────────┤
│ + AddRegion(RegionEntity)           │
│ + RemoveRegion(RegionEntity)        │
│ + UpdateAggregatedData()            │
│ + GetResourceBalance()              │
│ + GetNationSummary()                │
└─────────────────────────────────────┘
```

Key features include:
- Multiple regions belonging to nations
- Aggregated statistics across all regions
- Resource balance calculations
- National summaries for UI display

### EventBus System

The `EventBus` provides decoupled communication:

```csharp
public static class EventBus
{
    private static Dictionary<string, Action<object>> eventDictionary = 
        new Dictionary<string, Action<object>>();
    
    public static void Subscribe(string eventName, Action<object> listener)
    {
        // Add listener to event
    }
    
    public static void Unsubscribe(string eventName, Action<object> listener)
    {
        // Remove listener from event
    }
    
    public static void Trigger(string eventName, object data = null)
    {
        // Notify all listeners of the event
    }
}
```

The EventBus system has proven effective in maintaining loose coupling between systems.

### Map Generation

The map generation system has been expanded to include:

1. **Noise Generation**: Perlin noise creates natural-looking terrain
2. **Terrain Assignment**: Elevation and moisture determine terrain types
3. **Nation Placement**: Strategic placement with templates controlling position and expansion
4. **Nation Expansion**: Cellular automata-style expansion with terrain preferences
5. **Region Creation**: Regions within nations with terrain-based resources

## Folder Structure Status

The project follows the planned folder structure:

```
Assets/
  ├── Prefabs/
  │    ├── Map/             - Contains MapTile and other map-related prefabs
  │    ├── UI/              - UI element prefabs like RegionInfoPanel
  │    └── Effects/         - Visual effect prefabs
  ├── ScriptableObjects/
  │    ├── Nations/         - Nation data configurations
  │    ├── Regions/         - Region type definitions
  │    ├── Projects/        - Project definitions
  │    ├── Resources/       - Resource type configurations
  │    │    ├── Primary/    - Raw resource definitions
  │    │    ├── Secondary/  - Processed resource definitions
  │    │    ├── Tertiary/   - Complex resource definitions
  │    │    ├── Abstract/   - Non-physical resource definitions
  │    │    └── Balance/    - Resource balance configurations
  │    ├── TerrainTypes/    - Terrain type definitions
  │    ├── EconomicCycles/  - Economic cycle configurations
  │    └── Maps/            - Saved terrain maps
  ├── Scripts/
  │    ├── 1_Core/          - Core managers
  │    │    ├── GameManager.cs
  │    │    └── TurnManager.cs
  │    ├── 2_Data/          - Data structures and models
  │    │    ├── ScriptableObjects/
  │    │    └── Models/
  │    ├── 3_Entities/      - Entity classes
  │    │    ├── RegionEntity.cs
  │    │    └── NationEntity.cs
  │    ├── 4_Components/    - Component classes
  │    │    ├── ResourceComponent.cs
  │    │    └── ProductionComponent.cs
  │    ├── 5_Systems/       - System classes
  │    │    ├── EconomicSystem.cs
  │    │    └── MapSystem.cs
  │    ├── 6_UI/            - UI classes
  │    │    ├── Views/
  │    │    └── Components/
  │    ├── 7_Controllers/   - Controller classes
  │    │    ├── MapController.cs
  │    │    ├── CameraController.cs
  │    │    └── AIController.cs
  │    ├── 8_Managers/      - Manager classes
  │    │    └── EventBus.cs
  │    └── 9_Utils/         - Utility classes
  │         ├── Helpers/
  │         └── Extensions/
  └── Scenes/
       └── MainGame.unity   - Main game scene
```

## Roadmap (Updated)

### Short Term (Next 2-3 Weeks)
- Complete the project system implementation
- Add more resource production chains
- Enhance the resource UI with visual graphs
- Implement infrastructure components
- Improve AI decision-making with resource consideration

### Medium Term (Next 1-2 Months)
- Implement full economic cycle system
- Add inter-region trade
- Create basic population system
- Add more terrain effects on production
- Implement random events system

### Long Term (3-6 Months)
- Add roguelike run structure
- Implement generational shifts
- Expand tech progression
- Add diplomacy system
- Create narrative event chains
- Implement historical progression

## Resource Production Chain Design

The implementation of resource production chains has evolved from the initial design:

### Recipe System

Resources now follow a recipe-based production system:

```
┌─────────────────────────┐
│ ResourceProductionRecipe│
├─────────────────────────┤
│ - recipeName            │
│ - inputs (resources)    │
│ - outputAmount          │
│ - productionTime        │
│ - infrastructureReqs    │
└─────────────────────────┘
```

Each recipe:
- Has a unique name (e.g., "Basic Iron Smelting")
- Requires specific input resources (e.g., Iron Ore + Coal)
- Produces a specific output resource (e.g., Iron)
- May take multiple turns to complete
- May require specific infrastructure levels

### Resource Categories

Resources are now fully categorized:

1. **Primary Resources**: Direct extraction from terrain (implemented)
   - Examples: Crops, Coal, Iron Ore
   - Directly affected by terrain type
   - Simple production chains

2. **Secondary Resources**: First level processing (implemented)
   - Examples: Iron, Bread
   - Requires primary resources as inputs
   - Affected by infrastructure

3. **Tertiary Resources**: Complex goods (partially implemented)
   - Examples: Fine Food
   - Requires secondary resources
   - Higher value but more complex chains

4. **Abstract Resources**: Non-physical (planned)
   - Examples: Research, Culture, Military Power
   - Generated by specialized infrastructure
   - Used for advanced gameplay systems

### Production Efficiency Factors

The production system now considers multiple factors:
- Terrain type bonuses/penalties
- Resource availability
- Infrastructure levels (planned)
- Economic cycle phase (planned)
- Population skills (planned)

## AI Improvement Plans

The AI system has been enhanced with several features:

### AI Decision Making

AI nations now:
- Make simulated economic decisions
- Display notifications of their actions
- Process turns automatically when player ends turn
- Have visual feedback showing their activity

### Planned AI Improvements

1. **Strategic Resource Management**
   - AI prioritizes resources based on needs
   - Specializes regions based on terrain
   - Builds appropriate production chains

2. **Competitive AI Behavior**
   - AI reacts to player actions
   - Competes for limited resources
   - Forms alliances or rivalries

3. **Adaptive Strategy**
   - Adjusts to economic cycle phases
   - Responds to crises and opportunities
   - Leverages nation-specific strengths

## Visualization Improvements

The map and UI visualization system has been enhanced:

1. **Terrain-Based Visuals**
   - Regions colored based on terrain type
   - Terrain effects clearly displayed
   - Interactive region selection

2. **Economic Feedback**
   - Visual indicators for resource changes
   - Animation for value changes
   - Color-coded status indicators

3. **Nation Dashboard**
   - Nation-level summary statistics
   - Resource balance visualization
   - Territory overview

## Test and Debug Tools

Several testing tools have been implemented:

1. **MapDebugVisualizer**
   - Visualizes terrain generation
   - Shows elevation and moisture maps
   - Allows saving terrain configurations

2. **AI Action Display**
   - Shows AI decisions in real-time
   - Color-codes actions by nation
   - Provides feedback on AI thinking

3. **Resource Monitoring**
   - Tracks resource flows
   - Shows production and consumption
   - Highlights shortages and surpluses

## Conclusion

The Economic Cycles project has made significant progress in implementing its core architecture. The modularity of the design has proven effective, allowing systems to be developed and tested independently while maintaining integration through the EventBus.

The resource and production systems now form a strong foundation for the economic simulation. Terrain effects and nation management systems provide strategic depth. The AI system offers dynamic opposition to the player.

Moving forward, the focus will be on:
1. Completing the economic cycle implementation
2. Enhancing the production chain complexity
3. Adding population and infrastructure systems
4. Developing the roguelike meta-progression

These enhancements will build upon the solid architectural foundation to create a deep, engaging economic strategy experience with high replayability.