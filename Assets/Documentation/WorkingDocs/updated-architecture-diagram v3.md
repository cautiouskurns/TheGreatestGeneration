# Economic Cycles: Updated Architecture Diagram v2

## Core Architecture

```mermaid
graph TD
    %% Core Managers
    subgraph "Core Managers"
        GM[GameManager]
        TM[TurnManager]
        EM[EventBus]
    end

    %% Data Layer
    subgraph "Data Layer"
        SO[ScriptableObjects]
        subgraph "Models"
            MM[MapModel]
            ECM[EconomicCycleModel]
            NM[NationModel]
        end
    end

    %% Entities
    subgraph "Entities"
        RE[RegionEntity]
        NE[NationEntity]
        PE[ProjectEntity]
    end

    %% Components
    subgraph "Components"
        RC[ResourceComponent]
        PC[ProductionComponent]
        IC[InfrastructureComponent]
        POPC[PopulationComponent]
    end

    %% Systems
    subgraph "Systems"
        ECS[EconomicSystem]
        MS[MapSystem]
        PS[ProjectSystem]
        RTS[ResourceTradeSystem]
        ECS2[EconomicCycleSystem]
    end

    %% UI
    subgraph "User Interface"
        subgraph "Views"
            MV[MapView]
            RV[RegionView]
            EV[EconomicView]
            RDV[ResourceDashboardView]
        end
        subgraph "UI Components"
            RP[RegionPanel]
            RIU[RegionInfoUI]
            ED[EconomicDashboard]
            AAD[AIActionDisplay]
            NDU[NationDashboardUI]
        end
    end

    %% Controllers
    subgraph "Controllers"
        MC[MapController]
        CC[CameraController]
        PC2[ProjectController]
        AIC[AIController]
    end

    %% Utils
    subgraph "Utilities"
        MG[MapGenerator]
        RCH[RegionClickHandler]
        NG[NoiseGenerator]
        TG[TerrainGenerator]
        MDV[MapDebugVisualizer]
    end

    %% Major Relationships
    GM --> MM
    GM --> NM
    GM --> TM
    MM --> RE
    MM --> NM
    NM --> NE
    RE --> NE
    MM -.-> EM
    NM -.-> EM
    RE --> RC
    RE --> PC
    RE --> IC
    RE --> POPC
    
    MV --> SO
    MC -.-> EM
    MC --> GM
    
    MS -.-> EM
    MS --> MV
    
    ECS -.-> EM
    ECS --> RE
    ECS --> RC
    
    TM -.-> EM
    
    RCH -.-> EM
    
    EM -.-> MS
    EM -.-> ECS
    EM -.-> RIU
    EM -.-> MV
    EM -.-> ECS2
    EM -.-> NDU
    
    RIU --> RE
    RIU --> RC
    
    NE --> RE
    
    ECS2 --> RC
    ECS2 --> PC
    
    AIC -.-> EM
    AIC --> AAD
    
    NDU --> NE
    
    %% Resource System Connections
    RC --> RTS
    RC --> ECS
    ECS2 --> RC
    PC --> RC
    
    %% Added utility connections
    MG --> TG
    TG --> NG
    MG --> MM
    
    %% Legend
    classDef core fill:#ff9900,stroke:#333,stroke-width:2px
    classDef data fill:#3498db,stroke:#333,stroke-width:2px
    classDef entities fill:#2ecc71,stroke:#333,stroke-width:2px
    classDef components fill:#1abc9c,stroke:#333,stroke-width:2px
    classDef systems fill:#9b59b6,stroke:#333,stroke-width:2px
    classDef ui fill:#e74c3c,stroke:#333,stroke-width:2px
    classDef controllers fill:#f1c40f,stroke:#333,stroke-width:2px
    classDef events fill:#34495e,stroke:#333,stroke-width:2px
    classDef utils fill:#95a5a6,stroke:#333,stroke-width:2px
    
    class GM,TM,EM core
    class SO,MM,ECM,NM data
    class RE,NE,PE entities
    class RC,PC,IC,POPC components
    class ECS,MS,PS,RTS,ECS2 systems
    class MV,RV,EV,RDV,RP,RIU,ED,AAD,NDU ui
    class MC,CC,PC2,AIC controllers
    class MG,RCH,NG,TG,MDV utils
```

## Data Flow - Turn Processing

```mermaid
sequenceDiagram
    participant User
    participant TM as TurnManager
    participant EM as EventBus
    participant MM as MapModel
    participant NM as NationModel
    participant RE as RegionEntity
    participant RC as ResourceComponent
    participant PC as ProductionComponent
    participant ECS as EconomicSystem
    participant ECS2 as EconomicCycleSystem
    participant UI as UI Components
    
    User->>TM: Click "End Turn"
    TM->>EM: Trigger "TurnEnded"
    EM->>MM: Notify of "TurnEnded"
    MM->>RE: ProcessTurn() for each region
    RE->>RC: ProcessTurn(wealth, regionSize)
    RC->>PC: ProcessProduction()
    PC-->>RC: Update resources based on recipes
    RC-->>RE: Update resource values
    RE->>EM: Trigger "RegionUpdated"
    EM->>ECS: Notify of "TurnEnded"
    ECS->>RE: Process economic changes
    MM->>NM: ProcessTurn()
    NM->>NM: UpdateAllNations()
    NM->>EM: Trigger "NationModelUpdated"
    EM->>TM: Trigger "PlayerTurnEnded"
    TM->>AIC: Notify of "PlayerTurnEnded"
    AIC->>EM: Trigger "AITurnsCompleted"
    EM->>TM: Trigger "TurnProcessed"
    EM->>UI: Notify of updates
    UI-->>User: Update visual display
```

## Resource System Flow

```mermaid
sequenceDiagram
    participant RE as RegionEntity
    participant RC as ResourceComponent
    participant PC as ProductionComponent
    participant ECS as EconomicSystem
    participant ECS2 as EconomicCycleSystem
    
    RE->>RC: Request resource calculation
    RC->>RC: CalculateBaseProduction()
    RC->>RC: CalculateConsumption(wealth, size)
    RC->>PC: Get active recipes
    PC->>PC: CanProduceRecipe() check
    RC->>PC: Get production rates
    PC-->>RC: Return production values
    RC->>RC: Apply production and consumption
    RC-->>RE: Update resource stockpiles
    RE->>ECS: Report resource changes
    PC->>PC: Process recipes
    PC->>RC: RemoveResource() for inputs
    PC->>RC: AddResource() for outputs
    ECS->>RE: Apply economic effects
```

## Components Relationship

```mermaid
graph TD
    subgraph "RegionEntity"
        RE[RegionEntity]
        RC[ResourceComponent]
        PC[ProductionComponent]
        IC[InfrastructureComponent]
        POPC[PopulationComponent]
    end
    
    RE --> RC
    RE --> PC
    RE --> IC
    RE --> POPC
    
    RC <--> PC
    IC --> PC
    POPC --> PC
    POPC --> RC
    
    classDef entity fill:#2ecc71,stroke:#333,stroke-width:2px
    classDef component fill:#1abc9c,stroke:#333,stroke-width:2px
    
    class RE entity
    class RC,PC,IC,POPC component
```

## Nation Hierarchy

```mermaid
graph TD
    subgraph "Nation Management"
        NM[NationModel]
        NE[NationEntity]
        RE1[RegionEntity 1]
        RE2[RegionEntity 2]
        RE3[RegionEntity 3]
        AR[Aggregated Resources]
        NDU[NationDashboardUI]
    end
    
    NM --> NE
    NE --> RE1
    NE --> RE2
    NE --> RE3
    NE --> AR
    NE -.-> NDU
    
    classDef model fill:#3498db,stroke:#333,stroke-width:2px
    classDef entity fill:#2ecc71,stroke:#333,stroke-width:2px
    classDef data fill:#1abc9c,stroke:#333,stroke-width:2px
    classDef ui fill:#e74c3c,stroke:#333,stroke-width:2px
    
    class NM model
    class NE entity
    class RE1,RE2,RE3 entity
    class AR data
    class NDU ui
```

## Resource State Transitions

```mermaid
stateDiagram-v2
    [*] --> Production
    
    Production --> Storage: Resources Created
    Storage --> Consumption: Resources Used
    Storage --> Trading: Resources Exported
    Storage --> Processing: Recipe Input
    Processing --> Storage: Recipe Output
    Trading --> Storage: Resources Imported
    
    Consumption --> [*]
    
    note right of Production
        Affected by:
        - Terrain type
        - Infrastructure level
        - Economic cycle phase
        - Labor allocation
    end note
    
    note right of Storage
        Limited by:
        - Storage infrastructure
        - Resource type
    end note
    
    note right of Consumption
        Driven by:
        - Population needs
        - Infrastructure maintenance
        - Production requirements
        - Wealth factor
        - Size factor
    end note
    
    note right of Processing
        Implemented by:
        - ProductionComponent
        - Recipe system
        - Resource transformation
    end note
    
    note right of Trading
        Enabled by:
        - Transportation infrastructure
        - Trade agreements
        - Market conditions
    end note
```

## Folder Structure (Updated)

```mermaid
graph TD
    subgraph "Assets"
        Prefabs --> Map
        Prefabs --> UI
        Prefabs --> Effects
        
        ScriptableObjects --> Nations
        ScriptableObjects --> EconomicCycles
        ScriptableObjects --> Projects
        ScriptableObjects --> Regions
        ScriptableObjects --> Resources
        ScriptableObjects --> TerrainTypes
        ScriptableObjects --> Maps
        
        Resources --> Primary
        Resources --> Secondary
        Resources --> Tertiary
        Resources --> Abstract
        Resources --> Balance
        
        Scenes --> MainGame
        
        Scripts --> Core
        Scripts --> Data
        Scripts --> Entities
        Scripts --> Components
        Scripts --> Systems
        Scripts --> UI_Scripts[UI]
        Scripts --> Controllers
        Scripts --> Managers
        Scripts --> Utils
        
        Core --> GameManager
        Core --> TurnManager
        
        Data --> ScriptableObjects_Script[ScriptableObjects]
        Data --> Models
        
        ScriptableObjects_Script --> MapDataSO
        ScriptableObjects_Script --> ResourceDataSO
        ScriptableObjects_Script --> TerrainTypeDataSO
        ScriptableObjects_Script --> NationDataSO
        ScriptableObjects_Script --> ProjectDataSO
        ScriptableObjects_Script --> EconomicCycleDataSO
        
        Models --> MapModel
        Models --> NationModel
        
        Entities --> RegionEntity
        Entities --> NationEntity
        
        Components --> ResourceComponent
        Components --> ProductionComponent
        
        Systems --> EconomicSystem
        Systems --> MapSystem
        
        UI_Scripts --> Views
        UI_Scripts --> UI_Components[Components]
        
        Views --> MapView
        UI_Components --> RegionInfoUI
        UI_Components --> AIActionDisplay
        UI_Components --> NationDashboardUI
        
        Controllers --> MapController
        Controllers --> CameraController
        Controllers --> AIController
        
        Managers --> EventBus
        
        Utils --> Helpers
        Utils --> Extensions
        
        Helpers --> RegionClickHandler
        Helpers --> MapGenerator
        Helpers --> NoiseGenerator
        Helpers --> TerrainGenerator
        Helpers --> MapDebugVisualizer
    end
```

## Resource System Design

```mermaid
classDiagram
    class ResourceComponent {
        -Dictionary~string,float~ resources
        -Dictionary~string,float~ productionRates
        -Dictionary~string,float~ consumptionRates
        -Dictionary~string,ResourceDataSO~ resourceDefinitions
        -float baseConsumptionFactor
        -float wealthConsumptionMultiplier
        -float sizeConsumptionMultiplier
        +ProcessTurn(wealth, size)
        +AddResource(string, float)
        +RemoveResource(string, float)
        +GetResourceAmount(string)
        +CalculateBaseProduction()
        +CalculateConsumption(wealth, size)
        +GetConsumptionSatisfaction()
        +GetOverallSatisfaction()
        +HasConsumptionNeeds()
        +LoadResourceDefinitions(ResourceDataSO[])
    }
    
    class RegionEntity {
        +string regionName
        +int wealth
        +int production
        +string ownerNationName
        +Color regionColor
        +TerrainTypeDataSO terrainType
        +bool hasChangedThisTurn
        +int wealthDelta
        +int productionDelta
        +ResourceComponent resources
        +ProductionComponent productionComponent
        +UpdateEconomy(int, int)
        +ResetChangeFlags()
        +GetDescription()
        +GetDetailedDescription()
    }
    
    class ProductionComponent {
        -List~string~ activeRecipes
        -float productionEfficiency
        -Dictionary~string,float~ recipeProgress
        +ProcessProduction()
        +CanProduceRecipe(recipe)
        +ProduceRecipe(recipe)
        +ActivateRecipe(string)
        +DeactivateRecipe(string)
        +SetProductionEfficiency(float)
        +GetRecipeProgress(string)
        +GetActiveRecipes()
        +GetAvailableRecipes()
    }
    
    class ResourceDataSO {
        +string resourceName
        +ResourceType resourceType
        +ResourceCategory category
        +float baseValue
        +float marketVolatility
        +float maxStoragePerCapacity
        +float perishRate
        +float transportFactor
        +float weightPerUnit
        +bool isRawResource
        +ResourceProductionRecipe[] productionRecipes
    }
    
    class ResourceProductionRecipe {
        +string recipeName
        +ResourceInput[] inputs
        +float outputAmount
        +float productionTime
        +string requiredInfrastructureType
        +int minimumInfrastructureLevel
    }
    
    class NationEntity {
        +string nationName
        +Color nationColor
        +List~RegionEntity~ regions
        -int totalWealth
        -int totalProduction
        -Dictionary~string,float~ aggregatedResources
        -Dictionary~string,float~ aggregatedProduction
        -Dictionary~string,float~ aggregatedConsumption
        +AddRegion(RegionEntity)
        +RemoveRegion(RegionEntity)
        +UpdateAggregatedData()
        +GetTotalWealth()
        +GetTotalProduction()
        +GetAggregatedResources()
        +GetAggregatedProduction()
        +GetAggregatedConsumption()
        +GetResourceBalance()
        +GetNationSummary()
    }
    
    class NationModel {
        -Dictionary~string,NationEntity~ nations
        -NationEntity selectedNation
        +RegisterRegion(RegionEntity)
        +GetNation(string)
        +GetAllNations()
        +SelectNation(string)
        +GetSelectedNation()
        +UpdateAllNations()
        +ProcessTurn()
        +GetGlobalResourceBalance()
    }
    
    RegionEntity "1" --> "1" ResourceComponent
    RegionEntity "1" --> "1" ProductionComponent
    ResourceComponent ..> ResourceDataSO
    ProductionComponent ..> ResourceProductionRecipe
    ResourceDataSO *-- ResourceProductionRecipe
    NationEntity "1" o-- "many" RegionEntity
    NationModel "1" o-- "many" NationEntity
```

## AI Controller and System

```mermaid
classDiagram
    class AIController {
        +float turnDelay
        +GameObject aiThinkingIndicator
        +AIActionDisplay actionDisplay
        -GameManager gameManager
        -MapModel mapModel
        +OnPlayerTurnEnded(object)
        -ProcessAITurns()
        -ProcessNationTurn(Nation)
        -SimulateResourceAllocation(Nation)
        -SimulateProjectSelection(Nation)
    }
    
    class AIActionDisplay {
        +TextMeshProUGUI actionText
        +float messageDuration
        +int maxMessages
        -Queue~string~ messageQueue
        +DisplayAction(string, Color)
        -UpdateDisplayText()
        -ClearAfterDelay()
    }
    
    class TurnManager {
        -bool isPlayerTurn
        +EndTurn()
        -OnAITurnsCompleted(object)
    }
    
    AIController --> AIActionDisplay
    AIController ..> TurnManager
    AIController --> GameManager
```

## Map Generation System

```mermaid
classDiagram
    class MapGenerator {
        -int width
        -int height
        -int nationCount
        -int regionsPerNation
        -float elevationScale
        -float moistureScale
        -int seed
        -TerrainTypeDataSO[,] terrainMap
        -int[,] nationMap
        -Color[] nationColors
        -Dictionary~string,TerrainTypeDataSO~ terrainTypes
        -NationTemplate[] nationTemplates
        +SetTerrainTypes(Dictionary)
        +SetNationTemplates(NationTemplate[])
        +SetTerrainParameters(float, float)
        +GenerateMap()
        -GenerateNationColors()
        -GenerateTerrainMap()
        -GenerateNations()
        -ExpandNations()
        -ExpandNationsWithTemplates()
    }
    
    class NationTemplate {
        +string name
        +Color color
        +Vector2 centerPosition
        +float expansionRadius
        +bool isLandlocked
        +bool isMountainous
        +float forestPreference
        +float desertPreference
        +float plainsPreference
        +float wealthMultiplier
        +float productionMultiplier
    }
    
    class NoiseGenerator {
        +GenerateNoiseMap(width, height, scale, octaves, persistence, lacunarity, seed, offset)
        +GenerateElevationMap(width, height, elevationScale, seed)
        +GenerateMoistureMap(width, height, moistureScale, seed)
        +DetermineTerrainType(elevation, moisture, terrainTypes)
        +GenerateTerrainMap(width, height, elevationScale, moistureScale, terrainTypes, seed)
    }
    
    class TerrainGenerator {
        -int width
        -int height
        -int seed
        -float elevationScale
        -float moistureScale
        -Dictionary~string,TerrainTypeDataSO~ terrainTypes
        +SetTerrainParameters(float, float)
        +SetTerrainTypes(Dictionary)
        +GenerateTerrainMap()
    }
    
    class MapDebugVisualizer {
        +bool showElevationMap
        +bool showMoistureMap
        +bool showTerrainMap
        +bool showNationMap
        +float elevationScale
        +float moistureScale
        +int seed
        +int mapWidth
        +int mapHeight
        +TerrainTypeDataSO[] terrainTypes
        +GameObject mapContainer
        +GameObject debugTilePrefab
        -float[,] elevationMap
        -float[,] moistureMap
        -TerrainTypeDataSO[,] terrainMap
        -int[,] nationMap
        -GameObject[,] mapTiles
        -Dictionary~string,TerrainTypeDataSO~ terrainTypeDict
        +GenerateAndVisualize()
        -ClearMapVisualization()
        +RegenerateMap()
        +SaveCurrentMapToAsset()
    }
    
    MapGenerator ..> NoiseGenerator
    MapGenerator ..> TerrainGenerator
    MapGenerator --> NationTemplate
    TerrainGenerator ..> NoiseGenerator
    MapDebugVisualizer ..> NoiseGenerator
```

## Implementation Status

| Component | Status | Description |
|-----------|--------|-------------|
| Core Managers | ✅ Implemented | GameManager, TurnManager, EventBus |
| Map Generation | ✅ Implemented | Terrain and nation procedural generation |
| Region Management | ✅ Implemented | Basic region functionality with terrain |
| Resource System | ✅ Implemented | Resource tracking with production/consumption |
| Production System | ✅ Implemented | Recipe-based resource transformation |
| Nation System | ✅ Implemented | Nation entities, aggregation, UI |
| Economic Cycles | 🟨 Partial | Framework ready, implementation pending |
| AI Nations | ✅ Implemented | Basic AI turns with decision simulation |
| UI Elements | ✅ Implemented | Map view, region info, nation dashboard |
| Project System | 🟨 Partial | Data structure ready, interaction pending |
| Population System | 📝 Planned | Data structure defined, not implemented |
| Infrastructure | 📝 Planned | Data structure defined, not implemented |

## Event System Communication

| Event Name | Triggered By | Subscribed By | Status |
|------------|--------------|---------------|--------|
| RegionCreated | MapModel | EconomicSystem | ✅ |
| RegionUpdated | RegionEntity | MapView, RegionInfoUI | ✅ |
| RegionSelected | MapController | RegionInfoUI, MapView | ✅ |
| TurnEnded | TurnManager | EconomicSystem, MapModel | ✅ |
| TurnStarted | TurnManager | Various Systems | ✅ |
| TurnProcessed | TurnManager | UI Components | ✅ |
| PlayerTurnEnded | TurnManager | AIController | ✅ |
| AITurnsCompleted | AIController | TurnManager | ✅ |
| EconomicSystemReady | EconomicSystem | MapView | ✅ |
| NationUpdated | NationEntity | NationDashboardUI | ✅ |
| NationSelected | NationModel | NationDashboardUI | ✅ |
| NationModelUpdated | NationModel | UI Components | ✅ |
| RegionClicked | RegionClickHandler | MapController | ✅ |
| EconomicCycleChanged | EconomicCycleSystem | Multiple Systems | 🟨 |