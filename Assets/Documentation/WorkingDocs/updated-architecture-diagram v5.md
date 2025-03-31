# Economic Cycles: Updated Architecture Diagram v5

## Core Architecture

```mermaid
graph TD
    %% Core Managers
    subgraph "Core Managers"
        GM[GameManager]
        TM[TurnManager]
        EM[EventBus]
        GSM[GameStateManager]
        EDM[EventDialogueManager]
    end

    %% Data Layer
    subgraph "Data Layer"
        SO[ScriptableObjects]
        subgraph "Models"
            MM[MapModel]
            ECM[EconomicCycleModel]
            NM[NationModel]
        end
        subgraph "State Models"
            ES[EconomyState]
            DS[DiplomacyState]
            PHS[PlayerHistoryState]
            RS[RegionState]
        end
        subgraph "Dialogue Models"
            SDE[SimpleDialogueEvent]
            DL[DialogueLine]
            DC[DialogueChoice]
            DO[DialogueOutcome]
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
        RVS[ResourceVisualizationSystem]
    end

    %% Trade System Components
    subgraph "Trade System Components"
        TC[TradeCalculator]
        TV[TradeVisualizer]
        TR[TradeRecorder]
        TT[TradeTransaction]
        TI[TradeInfo]
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
            GSD[GameStateDisplay]
            RLT[ResourceLayerToggle]
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
        DST[DialogueSystemTester]
        GSD2[GameStateDebugger]
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
    TM --> GSM
    
    RCH -.-> EM
    
    EM -.-> MS
    EM -.-> ECS
    EM -.-> RIU
    EM -.-> MV
    EM -.-> ECS2
    EM -.-> NDU
    EM -.-> GSD
    
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
    
    GSM --> ES
    GSM --> DS 
    GSM --> PHS
    GSM --> RS
    
    EDM --> SDE
    SDE --> DL
    SDE --> DC
    DC --> DO
    EDM --> GSM
    
    RVS -.-> EM
    RVS --> RE
    RLT --> RVS
    
    %% Trade System Connections
    RTS --> TC
    RTS --> TV
    RTS --> TR
    TR --> TI
    TC --> TT
    RTS --> GM
    RTS -.-> EM
    
    %% Added utility connections
    MG --> TG
    TG --> NG
    MG --> MM
    
    DST --> EDM
    DST --> GSM
    GSD2 --> GSM
    
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
    classDef trade fill:#8e44ad,stroke:#333,stroke-width:2px
    classDef state fill:#16a085,stroke:#333,stroke-width:2px
    classDef dialogue fill:#d35400,stroke:#333,stroke-width:2px
    
    class GM,TM,EM,GSM,EDM core
    class SO,MM,ECM,NM data
    class ES,DS,PHS,RS state
    class SDE,DL,DC,DO dialogue
    class RE,NE,PE entities
    class RC,PC,REC,POPC,IC components
    class ECS,MS,PS,RTS,ECS2,RVS systems
    class MV,RV,EV,RDV,RP,RIU,ED,AAD,NDU,GSD,RLT ui
    class MC,CC,PC2,AIC controllers
    class MG,RCH,NG,TG,MDV,DST,GSD2 utils
    class TC,TV,TR,TT,TI trade
```

## Data Flow - Turn Processing

```mermaid
sequenceDiagram
    participant User
    participant TM as TurnManager
    participant EM as EventBus
    participant GSM as GameStateManager
    participant MM as MapModel
    participant NM as NationModel
    participant RTS as TradeSystem
    participant RE as RegionEntity
    participant RC as ResourceComponent
    participant PC as ProductionComponent
    participant REC as RegionEconomyComponent
    participant POPC as RegionPopulationComponent
    participant ECS as EconomicSystem
    participant AIC as AIController
    participant UI as UI Components
    
    User->>TM: Click "End Turn"
    TM->>GSM: IncrementTurn()
    TM->>GSM: SyncWithGameSystems()
    TM->>EM: Trigger "TurnEnded"
    EM->>MM: Notify of "TurnEnded"
    MM->>RE: ProcessTurn() for each region
    RE->>RC: CalculateProduction()
    RC->>POPC: Get labor allocation
    RE->>RC: CalculateDemand()
    RC->>REC: Get wealth multiplier
    RE->>RC: ProcessTurn(wealth, regionSize)
    RC->>PC: ProcessProduction()
    PC-->>RC: Update resources based on recipes
    RC-->>RE: Update resource values
    RE->>POPC: UpdateSatisfaction(needsSatisfaction)
    POPC->>POPC: UpdatePopulation()
    POPC->>REC: Apply satisfaction effects
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

## State & Dialogue System

```mermaid
graph TD
    subgraph "State Management System"
        GSM[GameStateManager]
        ES[EconomyState]
        DS[DiplomacyState]
        PHS[PlayerHistoryState]
        RS[RegionState]
    end
    
    subgraph "Dialogue System"
        EDM[EventDialogueManager]
        SDE[SimpleDialogueEvent]
        DL[DialogueLine]
        DC[DialogueChoice]
        DO[DialogueOutcome]
    end
    
    GSM --> ES
    GSM --> DS
    GSM --> PHS
    GSM --> RS
    
    EDM --> SDE
    SDE --> DL
    SDE --> DC
    DC --> DO
    
    EDM --> GSM
    
    SDE -.-> ES
    DC -.-> ES
    DO --> GSM
    
    TM[TurnManager] --> GSM
    RIU[RegionInfoUI] -.-> GSM
    GSD[GameStateDisplay] --> GSM
    
    classDef core fill:#ff9900,stroke:#333,stroke-width:2px
    classDef state fill:#16a085,stroke:#333,stroke-width:2px
    classDef dialogue fill:#d35400,stroke:#333,stroke-width:2px
    classDef ui fill:#e74c3c,stroke:#333,stroke-width:2px
    
    class GSM core
    class EDM core
    class ES,DS,PHS,RS state
    class SDE,DL,DC,DO dialogue
    class RIU,GSD ui
    class TM core
```

## Resource Visualization System

```mermaid
graph TD
    subgraph "Resource Visualization"
        RVS[ResourceVisualizationSystem]
        RLT[ResourceLayerToggle]
        RM[ResourceIconMapping]
    end
    
    subgraph "Region & Resources"
        RE[RegionEntity]
        RC[ResourceComponent]
    end
    
    RE --> RC
    RVS --> RE
    RVS --> RC
    RLT --> RVS
    RM --> RVS
    
    RVS -.-> EM[EventBus]
    
    classDef system fill:#9b59b6,stroke:#333,stroke-width:2px
    classDef component fill:#1abc9c,stroke:#333,stroke-width:2px
    classDef entity fill:#2ecc71,stroke:#333,stroke-width:2px
    classDef ui fill:#e74c3c,stroke:#333,stroke-width:2px
    
    class RVS system
    class RLT ui
    class RM component
    class RE entity
    class RC component
    class EM core
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
        Core --> GameStateManager
        Core --> EventDialogueManager
        
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
        Models --> EconomyState
        Models --> DiplomacyState
        Models --> PlayerHistoryState
        Models --> RegionState
        Models --> SimpleDialogueEvent
        
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
        Systems --> ResourceVisualizationSystem
        
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
        Components --> GameStateDisplay
        Components --> ResourceLayerToggle
        
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
        Helpers --> DialogueSystemTester
        Helpers --> GameStateDebugger
    end
```
