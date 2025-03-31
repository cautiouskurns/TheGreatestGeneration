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
| **RegionEconomyComponent** | Manages economic health of a region | Implemented |
| **RegionPopulationComponent** | Manages demographics and labor allocation | Implemented |
| **InfrastructureComponent** | Tracks infrastructure levels by category | Planned |
| **EconomicCycleComponent** | Tracks cycle effects on region performance | Planned |

#### Systems (Logic Processors)

| System | Purpose | Status |
|--------|---------|--------|
| **EconomicSystem** | Calculates production, consumption, and wealth changes | Implemented |
| **MapSystem** | Manages spatial relationships and territory | Implemented |
| **TradeSystem** | Handles resource movement between regions | Implemented |
| **ProjectSystem** | Processes ongoing projects and applies effects | Partial |
| **EconomicCycleSystem** | Advances economic cycles and applies modifiers | Planned |
| **ResourceVisualizationSystem** | Visualizes resources on the map | Implemented |

### 3. State Management System

The new state management system tracks gameplay state and history:

| Component | Purpose | Status |
|-----------|---------|--------|
| **GameStateManager** | Central manager for game state tracking | Implemented |
| **EconomyState** | Tracks economic cycle phase and resource conditions | Implemented |
| **DiplomacyState** | Tracks relations with other nations | Implemented |
| **PlayerHistoryState** | Records player decisions and event history | Implemented |
| **RegionState** | Tracks summary state of regions (satisfaction, etc.) | Implemented |

### 4. Dialogue and Event System

The dialogue and event system handles narrative events and player choices:

| Component | Purpose | Status |
|-----------|---------|--------|
| **EventDialogueManager** | Handles display and processing of dialogue | Implemented |
| **SimpleDialogueEvent** | Data structure for events with dialogue and choices | Implemented |
| **DialogueLine** | Single line of dialogue with variable processing | Implemented |
| **DialogueChoice** | Player choice with conditional visibility | Implemented |
| **DialogueOutcome** | Effects of player choices on game state | Implemented |

### 5. MVC Pattern (User Interface)

The MVC pattern handles player interaction and visualization:

#### Models

| Model | Purpose | Status |
|-------|---------|--------|
| **MapModel** | Central data model managing all regions | Implemented |
| **NationModel** | Manages nations and their aggregated properties | Implemented |
| **EconomicCycleModel** | Tracks current cycle phase and effects | Planned |
| **ResourceTradeModel** | Models resource flow and prices | Implemented |

#### Views

| View | Purpose | Status |
|------|---------|--------|
| **MapView** | Renders the game map with region visualizations | Implemented |
| **RegionInfoUI** | Displays detailed region information panel | Implemented |
| **NationDashboardUI** | Shows nation-level statistics | Implemented |
| **AIActionDisplay** | Displays AI actions with visual feedback | Implemented |
| **GameStateDisplay** | Shows current game state information | Implemented |
| **ResourceLayerToggle** | Controls visualization of resources | Implemented |
| **EconomicDashboard** | Shows economic indicators and cycle info | Planned |

#### Controllers

| Controller | Purpose | Status |
|------------|---------|--------|
| **MapController** | Handles map interaction and selection | Implemented |
| **CameraController** | Manages map navigation and zoom | Implemented |
| **AIController** | Manages AI nation decision-making | Implemented |
| **ProjectController** | Handles project selection and placement | Planned |

### 6. Event System (Communication Layer)

The EventBus provides a central communication mechanism for decoupled systems:

| Key Event | Triggered By | Subscribed By |
|-----------|--------------|---------------|
| **RegionSelected** | MapController | RegionInfoUI, MapView, TradeSystem |
| **RegionUpdated** | RegionEntity | MapView, RegionInfoUI, ResourceVisualizationSystem |
| **RegionCreated** | MapModel | EconomicSystem, MapView |
| **RegionEntitiesReady** | MapModel | TradeSystem, ResourceVisualizationSystem |
| **TurnEnded** | TurnManager | EconomicSystem, MapModel, TradeSystem |
| **TurnStarted** | TurnManager | Various Systems |
| **TurnProcessed** | TurnManager | UI Components |
| **PlayerTurnEnded** | TurnManager | AIController |
| **AITurnsCompleted** | AIController | TurnManager |
| **EconomicSystemReady** | EconomicSystem | MapView |
| **NationUpdated** | NationEntity | NationDashboardUI |
| **NationSelected** | MapController | NationDashboardUI |
| **NationModelUpdated** | NationModel | UI Components |
| **DialogueEnded** | EventDialogueManager | GameStateDisplay |
| **EconomicCycleChanged** | EconomicCycleSystem | Multiple Systems |

## Implementation Progress

### Completed Features

1. **Core Game Loop**
   - ✅ ScriptableObjects for nations, regions, and terrain
   - ✅ Basic region and map visualization with terrain coloring
   - ✅ Turn-based gameplay with EventBus communication
   - ✅ Procedural map generation with terrain effects
   - ✅ Camera controls with pan and zoom
   - ✅ Auto-simulation feature for rapid testing

2. **Economic Simulation**
   - ✅ Resource component with production/consumption rates
   - ✅ Terrain effects on resource production
   - ✅ Basic product recipes and conversion
   - ✅ Economic cycles data structure
   - ✅ Wealth and production tracking
   - ✅ Resource visualization on map

3. **Nation Management**
   - ✅ Nation entities with region grouping
   - ✅ Nation dashboard UI
   - ✅ Aggregated national statistics
   - ✅ Nation templates for procedural generation
   - ✅ AI nation decision making

4. **Trade System**
   - ✅ Inter-region resource trading
   - ✅ Trade route visualization
   - ✅ Trade calculations based on supply and demand
   - ✅ Trade transaction recording and reporting

5. **UI Systems**
   - ✅ Region information panel with detailed stats
   - ✅ Visual feedback for economic changes
   - ✅ AI action display with animated notifications
   - ✅ Turn management UI
   - ✅ Resource toggles and visualization

6. **State and Dialogue Systems**
   - ✅ Game state tracking framework
   - ✅ Dialogue system with choices
   - ✅ Event-based narrative framework
   - ✅ State variables in dialogue text
   - ✅ Choice outcomes affecting game state

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
   - ⏳ Enhanced production and consumption charts
   - ⏳ Trend analysis for resources

### Planned Features

1. **Economic Cycles**
   - 📋 Full cycle phase transitions
   - 📋 Phase-specific modifiers to production and consumption
   - 📋 Visual indicators of current cycle phase
   - 📋 Strategic opportunities in different phases

2. **Advanced Diplomacy**
   - 📋 Inter-nation relations
   - 📋 Diplomatic events and crises
   - 📋 Alliance and rivalry systems
   - 📋 Trade agreements

3. **Infrastructure System**
   - 📋 Infrastructure categories and levels
   - 📋 Infrastructure effects on production
   - 📋 Building and upgrading mechanics
   - 📋 Maintenance costs

## Technical Implementation Details

### State Management

The game now has a robust state management system through the `GameStateManager` class:

```csharp
public class GameStateManager : MonoBehaviour
{
    // Singleton pattern for easy access
    public static GameStateManager Instance { get; private set; }
    
    // Core state objects
    public EconomyState Economy { get; private set; } = new EconomyState();
    public DiplomacyState Diplomacy { get; private set; } = new DiplomacyState();
    public PlayerHistoryState History { get; private set; } = new PlayerHistoryState();
    public Dictionary<string, RegionState> RegionStates { get; private set; } = new Dictionary<string, RegionState>();
    
    // Generic state data storage
    private Dictionary<string, object> gameStateData = new Dictionary<string, object>();
    
    // Gameplay tracking
    private int currentTurn = 0;
}
```

This system tracks the current economic phase, diplomatic relations, player history, and region satisfaction levels, providing a foundation for narrative events and gameplay progression.

### Dialogue System

The dialogue system provides a flexible framework for narrative events:

```csharp
public class EventDialogueManager : MonoBehaviour
{
    // UI References
    public GameObject dialoguePanel;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI dialogueText;
    public Button continueButton;
    public GameObject choicesContainer;
    
    // Dialogue state tracking
    private string[] currentDialogueLines;
    private int currentLineIndex;
    private SimpleDialogueEvent currentEvent;
    
    // Core methods
    public void ShowDialogue(string title, string[] lines);
    public void ShowDialogueWithChoices(string title, string message, string[] choiceTexts);
    public void ShowDialogueEvent(SimpleDialogueEvent dialogueEvent);
}
```

The dialogue system integrates with the state system to provide variable text and conditional choices, as well as outcomes that affect the game state.

### Region Components System

The region component architecture has been expanded:

```
┌─────────────────────────────────┐
│ RegionEntity                   │
├─────────────────────────────────┤
│ - regionName: string           │
│ - ownerNationName: string      │
│ - regionColor: Color           │
│ - terrainType: TerrainTypeDataSO│
│ - resources: ResourceComponent  │
│ - productionComponent: ProductionComponent │
│ - economy: RegionEconomyComponent │
│ - population: RegionPopulationComponent │
├─────────────────────────────────┤
│ + ProcessTurn()                │
│ + UpdateEconomy(int, int)      │
│ + ResetChangeFlags()           │
│ + GetDescription()             │
│ + GetDetailedDescription()     │
└─────────────────────────────────┘
```

This enhanced component-based approach allows for more specialized behavior while maintaining a clean interface.

### Resource Visualization System

The new `ScreenSpaceResourceVisualization` system provides dynamic, screen-space visualization of resources:

```csharp
public class ScreenSpaceResourceVisualization : MonoBehaviour
{
    // Configuration
    public bool showResourceIcons = true;
    public float iconSize = 0.3f;
    public float iconSpacing = 25f;
    public int maxIconsPerRegion = 3;
    
    // Resource tracking
    private Dictionary<string, Sprite> resourceIconMap = new Dictionary<string, Sprite>();
    private Dictionary<string, List<GameObject>> regionResourceVisualizers = 
        new Dictionary<string, List<GameObject>>();
        
    // Core methods
    public void ToggleResourceIcons();
    private void UpdateResourceVisualizers(RegionEntity region);
    private GameObject CreateResourceVisualizer(string resourceName, float amount, int index, int totalCount, Transform parent, Vector3 regionPosition);
}
```

This system provides a toggleable layer of resource icons that show the most important resources in each region, updating dynamically as resources change.

### Trade System

The trade system has been fully implemented with multiple components:

```csharp
public class TradeSystem : MonoBehaviour
{
    // Configuration
    public int tradeRadius = 2;
    public float tradeEfficiency = 0.8f;
    public int maxTradingPartnersPerRegion = 3;
    
    // Component references
    private TradeCalculator calculator;
    private TradeVisualizer visualizer;
    private TradeRecorder recorder;
    
    // Core methods
    private void ProcessTrade(object _);
    public List<TradeInfo> GetRecentImports(string regionName);
    public List<TradeInfo> GetRecentExports(string regionName);
}
```

This system calculates trades between regions based on resource surpluses and deficits, visualizes trade routes with curved lines, and records trade history for UI display.

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
  │    ├── Maps/            - Saved terrain maps
  │    └── Test/            - Test data for development
  ├── Scripts/
  │    ├── 1_Core/          - Core managers
  │    │    ├── GameManager.cs
  │    │    ├── TurnManager.cs
  │    │    ├── GameStateManager.cs
  │    │    └── EventDialogueManager.cs
  │    ├── 2_Data/          - Data structures and models
  │    │    ├── ScriptableObjects/
  │    │    └── Models/
  │    ├── 3_Entities/      - Entity classes
  │    │    ├── RegionEntity.cs
  │    │    └── NationEntity.cs
  │    ├── 4_Components/    - Component classes
  │    │    ├── RegionComponents/
  │    │    │    ├── ResourceComponent.cs
  │    │    │    ├── ProductionComponent.cs
  │    │    │    ├── RegionEconomyComponent.cs
  │    │    │    └── RegionPopulationComponent.cs
  │    ├── 5_Systems/       - System classes
  │    │    ├── EconomicSystem.cs
  │    │    ├── MapSystem.cs
  │    │    ├── TradeSystems/
  │    │    │    ├── TradeSystem.cs
  │    │    │    └── zSubSystems/
  │    │    └── ResourceVisualizationSystem.cs
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
- Complete economic cycle phase transitions
- Add more event content for game state testing
- Enhance resource visualization with trends
- Implement infrastructure component
- Improve AI decision-making with resource consideration

### Medium Term (Next 1-2 Months)
- Complete project system implementation
- Add random events triggered by game state
- Enhance diplomatic relations system
- Create tutorial content 
- Implement player feedback system

### Long Term (3-6 Months)
- Add roguelike run structure with progression
- Implement generational shifts
- Add tech progression
- Expand diplomacy system
- Create narrative event chains
- Implement historical progression

## Trade System Design

The trade system has evolved from the initial concept into a multi-component implementation:

### TradeCalculator

Handles the logic for determining which regions should trade with each other:

```csharp
public class TradeCalculator
{
    // Configuration
    private Dictionary<string, RegionEntity> regions;
    private float tradeEfficiency;
    private int maxTradingPartnersPerRegion;
    private float tradeRadius;
    
    // Core methods
    public List<TradeTransaction> CalculateTrades();
    private Dictionary<string, float> CalculateDeficits(RegionEntity region);
    private List<RegionEntity> FindTradingPartners(RegionEntity region, string resourceName);
    private (float, float) CalculateTradeAmount(RegionEntity exporter, string resourceName, float deficitAmount);
}
```

### TradeVisualizer

Handles the visual representation of trade routes:

```csharp
public class TradeVisualizer
{
    // Configuration
    private Color importColor;
    private Color exportColor;
    private float tradeLineWidth;
    private float minimumTradeVolumeToShow;
    
    // Line rendering
    private List<GameObject> activeTradeLines = new List<GameObject>();
    
    // Core methods
    public void ShowTradeLine(RegionEntity from, RegionEntity to, Color color, float tradeAmount);
    private Vector3 CalculateQuadraticBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2);
}
```

### TradeRecorder

Tracks trade history for regions:

```csharp
public class TradeRecorder
{
    // Trade history
    private Dictionary<string, List<TradeInfo>> recentImports;
    private Dictionary<string, List<TradeInfo>> recentExports;
    private Dictionary<string, int> regionTradeVolume;
    
    // Core methods
    public void RecordTrade(TradeTransaction trade);
    public List<TradeInfo> GetRecentImports(string regionName);
    public List<TradeInfo> GetRecentExports(string regionName);
}
```

### TradeTransaction

Represents a single trade between regions:

```csharp
public class TradeTransaction
{
    public RegionEntity Exporter { get; set; }
    public RegionEntity Importer { get; set; }
    public string ResourceName { get; set; }
    public float Amount { get; set; }
    public float ReceivedAmount { get; set; }
    
    public void Execute();
}
```

## Dialogue and Event System Design

The dialogue system provides a narrative framework integrated with game state:

### SimpleDialogueEvent

Contains all data for a narrative event:

```csharp
public class SimpleDialogueEvent
{
    public string id;
    public string title;
    public string description;
    public List<DialogueLine> lines;
    public List<DialogueChoice> choices;
    
    // Optional conditions
    public string requiredEconomicPhase;
    public string requiredResourceShortage;
}
```

### DialogueLine

Represents a single line of dialogue with variable processing:

```csharp
public class DialogueLine
{
    public string text;
    public string speakerName;
    
    // Process state variables in text
    public string GetProcessedText(GameStateManager stateManager);
}
```

### DialogueChoice

Player choice with conditional visibility and outcomes:

```csharp
public class DialogueChoice
{
    public string text;
    public List<DialogueOutcome> outcomes;
    
    // Optional condition
    public bool hasCondition;
    public string requiredState;
    public float requiredValue;
}
```

### DialogueOutcome

Effects of player choices on game state:

```csharp
public class DialogueOutcome
{
    public enum OutcomeType
    {
        AddResource,
        RemoveResource,
        ChangeRelation,
        ChangeSatisfaction,
        SetEconomicPhase,
        RecordDecision
    }
    
    public OutcomeType type;
    public string targetId;
    public float value;
    public string description;
}
```

## Conclusion

The Economic Cycles project has made significant progress in implementing its core architecture. The modular design has proven effective, allowing systems to be developed and tested independently while maintaining integration through the EventBus.

The addition of the state management and dialogue systems provides a foundation for narrative elements and persistent game state tracking. The trade system has been fully implemented, enabling resource movement between regions. The resource visualization system enhances the player's understanding of the economic situation.

Moving forward, the focus will be on:
1. Completing the economic cycle implementation
2. Enhancing the production chain complexity
3. Adding more narrative content through the dialogue system
4. Developing the roguelike meta-progression

These enhancements will build upon the solid architectural foundation to create a deep, engaging economic strategy experience with high replayability.