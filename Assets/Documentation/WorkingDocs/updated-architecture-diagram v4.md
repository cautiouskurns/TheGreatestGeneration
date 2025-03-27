# Economic Cycles: Updated Architecture Diagram v3

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
        REC[RegionEconomyComponent]
        POPC[RegionPopulationComponent]
        IC[InfrastructureComponent]
    end

    %% Systems
    subgraph "Systems"
        ECS[EconomicSystem]
        MS[MapSystem]
        PS[ProjectSystem]
        RTS[TradeSystem]
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
        TCL[TradeCalculator]
        TVZ[TradeVisualizer]
        TRC[TradeRecorder]
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
    RE --> REC
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
    RIU --> RTS
    
    NE --> RE
    
    ECS2 --> RC
    ECS2 --> PC
    
    AIC -.-> EM
    AIC --> AAD
    AIC --> GM
    
    NDU --> NE
    
    %% Resource System Connections
    RC --> RTS
    RC --> ECS
    ECS2 --> RC
    PC --> RC
    
    %% Trade System Connections
    RTS --> TCL
    RTS --> TVZ
    RTS --> TRC
    RTS --> GM
    
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
    class RC,PC,REC,POPC,IC components
    class ECS,MS,PS,RTS,ECS2 systems
    class MV,RV,EV,RDV,RP,RIU,ED,AAD,NDU ui
    class MC,CC,PC2,AIC controllers
    class MG,RCH,NG,TG,MDV,TCL,TVZ,TRC utils
```

## Data Flow - Turn Processing

```mermaid
sequenceDiagram
    participant User
    participant TM as TurnManager
    participant EM as EventBus
    participant MM as MapModel
    participant NM as NationModel
    participant RTS as TradeSystem
    participant RE as RegionEntity
    participant RC as ResourceComponent
    participant PC as ProductionComponent
    participant ECS as EconomicSystem
    participant AIC as AIController
    participant UI as UI Components
    
    User->>TM: Click "End Turn"
    TM->>EM: Trigger "TurnEnded"
    EM->>MM: Notify of "TurnEnded"
    MM->>RE: ProcessTurn() for each region
    RE->>RC: CalculateProduction()
    RE->>RC: CalculateDemand()
    RE->>RC: ProcessTurn(wealth, regionSize)
    RC->>PC: ProcessProduction()
    PC-->>RC: Update resources based on recipes
    RC-->>RE: Update resource values
    RE->>EM: Trigger "RegionUpdated"
    EM->>ECS: Notify of "TurnEnded"
    ECS->>RE: Process economic changes
    EM->>RTS: Notify of "TurnEnded"
    RTS->>RTS: CalculateTrades()
    RTS->>RE: Execute trades between regions
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
    participant REC as RegionEconomyComponent
    participant POPC as RegionPopulationComponent
    participant RC as ResourceComponent
    participant PC as ProductionComponent
    participant TS as TradeSystem
    
    RE->>RC: CalculateProduction()
    RC->>POPC: Get labor allocation
    POPC-->>RC: Return labor allocation
    RC->>RC: Apply terrain & efficiency modifiers
    
    RE->>RC: CalculateDemand()
    RC->>POPC: Get population size
    POPC-->>RC: Return population size
    RC->>RC: Calculate per capita needs
    RC->>REC: Get wealth multiplier
    REC-->>RC: Return wealth effect on consumption
    
    RE->>RC: ProcessTurn(wealth, regionSize)
    RC->>PC: ProcessProduction()
    PC->>PC: Process active recipes
    PC->>RC: RemoveResource() for inputs
    PC->>RC: AddResource() for outputs
    RC->>RC: Apply production and consumption
    
    TS->>RC: Query resource surpluses/deficits
    RC-->>TS: Return resource data
    TS->>TS: Calculate possible trades
    TS->>RC: Execute trade transactions
    RC-->>RE: Update resource stockpiles
    
    RC->>POPC: Update satisfaction
    POPC->>REC: Apply satisfaction effects
    REC->>RE: Update economy values
    RE->>RE: Trigger RegionUpdated event
```

## Components Relationship

```mermaid
graph TD
    subgraph "RegionEntity"
        RE[RegionEntity]
        RC[ResourceComponent]
        PC[ProductionComponent] 
        REC[RegionEconomyComponent]
        POPC[RegionPopulationComponent]
    end
    
    RE --> RC
    RE --> PC
    RE --> REC
    RE --> POPC
    
    RC <--> PC
    REC --> PC
    POPC --> PC
    POPC --> RC
    RC --> REC
    
    classDef entity fill:#2ecc71,stroke:#333,stroke-width:2px
    classDef component fill:#1abc9c,stroke:#333,stroke-width:2px
    
    class RE entity
    class RC,PC,REC,POPC component
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
        - Population size
    end note
    
    note right of Storage
        Limited by:
        - Storage infrastructure
        - Resource type
        - Perishable goods decay
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
        - Efficiency modifiers
    end note
    
    note right of Trading
        Enabled by:
        - TradeSystem components
        - Resource surpluses/deficits
        - Trade efficiency
        - Partner limitations
        - Distance factors
    end note
```

## Trade System Structure

```mermaid
graph TD
    subgraph "Trade System"
        TS[TradeSystem]
        TC[TradeCalculator]
        TR[TradeRecorder]
        TV[TradeVisualizer]
        TT[TradeTransaction]
        TI[TradeInfo]
    end
    
    TS --> TC
    TS --> TR
    TS --> TV
    TC --> TT
    TR --> TI
    TV --> TT
    
    RE1[RegionEntity 1] <--> TS
    RE2[RegionEntity 2] <--> TS
    
    TS --> GM[GameManager]
    RIU[RegionInfoUI] --> TS
    
    classDef system fill:#9b59b6,stroke:#333,stroke-width:2px
    classDef component fill:#1abc9c,stroke:#333,stroke-width:2px
    classDef entity fill:#2ecc71,stroke:#333,stroke-width:2px
    classDef ui fill:#e74c3c,stroke:#333,stroke-width:2px
    
    class TS system
    class TC,TR,TV,TT,TI component
    class RE1,RE2 entity
    class RIU ui
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
        ScriptableObjects --> Test
        
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
        ScriptableObjects_Script --> TerrainMapDataSO
        
        Models --> MapModel
        Models --> NationModel
        
        Entities --> RegionEntity
        Entities --> NationEntity
        
        Components --> RegionComponents
        
        RegionComponents --> ResourceComponent
        RegionComponents --> ProductionComponent
        RegionComponents --> RegionEconomyComponent
        RegionComponents --> RegionPopulationComponent
        
        Systems --> EconomicSystem
        Systems --> MapSystem
        Systems --> TradeSystems
        
        TradeSystems --> TradeSystem
        TradeSystems --> zSubSystems
        
        zSubSystems --> TradeCalculator
        zSubSystems --> TradeRecorder
        zSubSystems --> TradeVisualizer
        zSubSystems --> TradeTransaction
        zSubSystems --> TradeInfo
        
        UI_Scripts --> Views
        UI_Scripts --> Components
        
        Views --> MapView
        Components --> RegionInfoUI
        Components --> AIActionDisplay
        Components --> NationDashboardUI
        Components --> SpinningWheel
        
        Controllers --> MapController
        Controllers --> CameraController
        Controllers --> AIController
        
        Managers --> EventBus
        
        Utils --> Helpers
        Utils --> Extensions
        
        Helpers --> RegionClickHandler
        Helpers --> RegionBehavior
        Helpers --> MapGenerator
        Helpers --> MapManager
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
        +CalculateProduction()
        +CalculateDemand()
        +ProcessTurn(wealth, size)
        +AddResource(string, float)
        +RemoveResource(string, float)
        +GetResourceAmount(string)
        +GetAllResources()
        +GetAllProductionRates()
        +GetAllConsumptionRates()
        +GetConsumptionSatisfaction()
        +GetOverallSatisfaction()
        +LoadResourceDefinitions(ResourceDataSO[])
    }
    
    class RegionEntity {
        +string regionName
        +string ownerNationName
        +Color regionColor
        +TerrainTypeDataSO terrainType
        +bool hasChangedThisTurn
        +float landProductivity
        +ResourceComponent resources
        +ProductionComponent productionComponent
        +RegionEconomyComponent economy
        +RegionPopulationComponent population
        +ProcessTurn()
        +UpdateEconomy(int, int)
        +ResetChangeFlags()
        +GetDescription()
        +GetDetailedDescription()
    }
    
    class RegionEconomyComponent {
        +int wealth
        +int production
        +float productionEfficiency
        +float capitalInvestment
        +int wealthDelta
        +int productionDelta
        +ApplyChanges(int, int)
        +ApplySatisfactionEffects(float)
        +AdjustCapitalInvestment(float)
        +ResetChangeFlags()
        +CalculateBaseWealthChange()
        +CalculateBaseProductionChange()
        +CalculateResourceEffect(Dictionary)
    }
    
    class RegionPopulationComponent {
        +int laborAvailable
        +float satisfaction
        +Dictionary~string,float~ laborAllocation
        +UpdateSatisfaction(Dictionary)
        +UpdatePopulation()
        +GetLaborAllocation(string)
        +SetLaborAllocation(string, float)
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
    RegionEntity "1" --> "1" RegionEconomyComponent
    RegionEntity "1" --> "1" RegionPopulationComponent
    ResourceComponent ..> ResourceDataSO
    ProductionComponent ..> ResourceProductionRecipe
    ResourceDataSO *-- ResourceProductionRecipe
    NationEntity "1" o-- "many" RegionEntity
    NationModel "1" o-- "many" NationEntity
```

## Trade System Design

```mermaid
classDiagram
    class TradeSystem {
        +int tradeRadius
        +float tradeEfficiency
        +int maxTradingPartnersPerRegion
        +bool showTradeLines
        +Color importColor
        +Color exportColor
        +TradeCalculator calculator
        +TradeVisualizer visualizer
        +TradeRecorder recorder
        +ProcessTrade(object)
        +GetRecentImports(string)
        +GetRecentExports(string)
        +GetRegionTradeColor(string)
        +ClearSelectedRegion()
        +DebugTradeSystem()
    }
    
    class TradeCalculator {
        -Dictionary~string,RegionEntity~ regions
        -float tradeEfficiency
        -int maxTradingPartnersPerRegion
        -float tradeRadius
        -Dictionary~string,HashSet~string~~ tradingPartnersByRegion
        +CalculateTrades()
        -CalculateDeficits(RegionEntity)
        -FindTradingPartners(RegionEntity, string)
        -CalculateTradeAmount(RegionEntity, string, float)
        -CalculateSurplus(RegionEntity, string)
        -VerifyPartnerLimits()
        +GetTradingPartnersCount()
    }
    
    class TradeTransaction {
        +RegionEntity Exporter
        +RegionEntity Importer
        +string ResourceName
        +float Amount
        +float ReceivedAmount
        +Execute()
    }
    
    class TradeVisualizer {
        -Color importColor
        -Color exportColor
        -float tradeLineWidth
        -float tradeLineDuration
        -float minimumTradeVolumeToShow
        -bool showTradeLines
        -float curveHeight
        -int curveSegments
        -List~GameObject~ activeTradeLines
        +ShowTradeLine(RegionEntity, RegionEntity, Color, float)
        -CalculateQuadraticBezierPoint(float, Vector3, Vector3, Vector3)
        -AddDirectionIndicator(Vector3, Vector3, Vector3, Color, float)
        +ClearTradeLines()
    }
    
    class TradeRecorder {
        -Dictionary~string, List~TradeInfo~~ recentImports
        -Dictionary~string, List~TradeInfo~~ recentExports
        -Dictionary~string,int~ regionTradeVolume
        +RecordTrade(TradeTransaction)
        -RecordImport(string, string, string, float)
        -RecordExport(string, string, string, float)
        -IncrementTradeVolume(string)
        +GetRecentImports(string)
        +GetRecentExports(string)
        +GetAllExports()
        +GetAllImports()
        +GetTradeVolume(string)
        +GetRegionTradeColor(string, Color, Color)
        +ClearTradeData()
    }
    
    class TradeInfo {
        +string partnerName
        +string resourceName
        +float amount
    }
    
    TradeSystem --> TradeCalculator
    TradeSystem --> TradeVisualizer
    TradeSystem --> TradeRecorder
    TradeCalculator ..> TradeTransaction
    TradeRecorder ..> TradeInfo
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
        -ProcessNationTurn(NationData)
        -SimulateResourceAllocation(NationData)
        -SimulateProjectSelection(NationData)
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
    
    class SpinningWheel {
        +float rotationSpeed
        +Update()
    }
    
    AIController --> AIActionDisplay
    AIController ..> TurnManager
    AIController --> GameManager
    AIController --> SpinningWheel
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
        -PlaceNationSeeds()
        -ExpandNations()
        -ExpandNationsWithTemplates()
        -CreateMapData()
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
        -OnGUI()
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
| Map Generation | ✅ Implemented | Terrain and nation procedural generation with templates |
| Region Management | ✅ Implemented | Region entities with component-based architecture |
| Resource System | ✅ Implemented | Resource production, consumption, and tracking |
| Production System | ✅ Implemented | Recipe-based resource transformation |
| Population System | ✅ Implemented | Labor allocation, satisfaction tracking |
| Economy System | ✅ Implemented | Wealth and production management with modifiers |
| Nation System | ✅ Implemented | Nation entities, aggregation, UI |
| Trade System | ✅ Implemented | Inter-region resource trading with visualization |
| AI Nations | ✅ Implemented | AI decision simulation with visual feedback |
| UI Elements | ✅ Implemented | Map view, region info, nation dashboard, trade display |
| Project System | 🟨 Partial | Data structure ready, interaction pending |
| Economic Cycles | 🟨 Partial | Data structure ready, implementation pending |
| Infrastructure | 🟨 Partial | Data structure defined, implementation pending |

## Event System Communication

| Event Name | Triggered By | Subscribed By | Status |
|------------|--------------|---------------|--------|
| RegionCreated | MapModel | EconomicSystem | ✅ |
| RegionEntitiesReady | MapModel | TradeSystem | ✅ |
| RegionUpdated | RegionEntity | MapView, RegionInfoUI | ✅ |
| RegionSelected | MapController | RegionInfoUI, MapView, TradeSystem | ✅ |
| RegionClicked | RegionClickHandler | MapController | ✅ |
| TurnEnded | TurnManager | EconomicSystem, MapModel, TradeSystem | ✅ |
| TurnStarted | TurnManager | Various Systems | ✅ |
| TurnProcessed | TurnManager | UI Components | ✅ |
| PlayerTurnEnded | TurnManager | AIController | ✅ |
| AITurnsCompleted | AIController | TurnManager | ✅ |
| EconomicSystemReady | EconomicSystem | MapView | ✅ |
| NationUpdated | NationEntity | NationDashboardUI | ✅ |
| NationSelected | NationModel | NationDashboardUI | ✅ |
| NationModelUpdated | NationModel | UI Components | ✅ |
| EconomicCycleChanged | EconomicCycleSystem | Multiple Systems | 🟨 |