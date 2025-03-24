# Economic Cycles: Updated Architecture Diagram

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
    end

    %% Major Relationships
    GM --> MM
    GM --> TM
    MM --> RE
    MM --> NE
    MM -.-> EM
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
    
    RIU --> RE
    RIU --> RC
    
    NE --> RE
    
    ECS2 --> RC
    ECS2 --> PC
    
    AIC -.-> EM
    AIC --> AAD
    
    %% Resource System Connections
    RC --> RTS
    RC --> ECS
    ECS2 --> RC
    
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
    class SO,MM,ECM data
    class RE,NE,PE entities
    class RC,PC,IC,POPC components
    class ECS,MS,PS,RTS,ECS2 systems
    class MV,RV,EV,RDV,RP,RIU,ED,AAD ui
    class MC,CC,PC2,AIC controllers
    class MG,RCH,NG,TG utils
```

## Data Flow - Turn Processing

```mermaid
sequenceDiagram
    participant User
    participant TM as TurnManager
    participant EM as EventBus
    participant MM as MapModel
    participant RE as RegionEntity
    participant RC as ResourceComponent
    participant ECS as EconomicSystem
    participant ECS2 as EconomicCycleSystem
    participant UI as UI Components
    
    User->>TM: Click "End Turn"
    TM->>EM: Trigger "TurnEnded"
    EM->>MM: Notify of "TurnEnded"
    MM->>RE: ProcessTurn() for each region
    RE->>RC: ProcessTurn()
    RC-->>RE: Update resource values
    RE->>EM: Trigger "RegionUpdated"
    EM->>ECS: Notify of "TurnEnded"
    ECS->>RE: Process economic changes
    EM->>ECS2: Notify of "TurnEnded"
    ECS2->>ECS2: Update cycle phase if needed
    ECS2->>EM: Trigger "EconomicCycleChanged" if phase changed
    EM->>UI: Notify of "RegionUpdated"
    UI-->>User: Update visual display
    EM->>TM: Trigger "TurnProcessed"
    TM->>EM: Trigger "AITurnsCompleted"
    EM->>TM: Begin next player turn
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
    RC->>PC: Get production rates
    PC-->>RC: Return production values
    RC->>ECS2: Get cycle modifiers
    ECS2-->>RC: Return current modifiers
    RC->>RC: Apply production and consumption
    RC-->>RE: Update resource stockpiles
    RE->>ECS: Report resource changes
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

## Resource State Transitions

```mermaid
stateDiagram-v2
    [*] --> Production
    
    Production --> Storage: Resources Created
    Storage --> Consumption: Resources Used
    Storage --> Trading: Resources Exported
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
        
        Models --> MapModel
        
        Entities --> RegionEntity
        Entities --> NationEntity
        
        Components --> ResourceComponent
        Components --> ProductionComponent
        Components --> InfrastructureComponent
        
        Systems --> EconomicSystem
        Systems --> MapSystem
        Systems --> EconomicCycleSystem
        
        UI_Scripts --> Views
        UI_Scripts --> UI_Components[Components]
        
        Views --> MapView
        UI_Components --> RegionInfoUI
        UI_Components --> AIActionDisplay
        
        Controllers --> MapController
        Controllers --> CameraController
        Controllers --> AIController
        
        Managers --> EventBus
        
        Utils --> Helpers
        
        Helpers --> RegionClickHandler
        Helpers --> MapGenerator
        Helpers --> NoiseGenerator
    end
```

## Implementation Status

| Component | Status | Description |
|-----------|--------|-------------|
| Core Managers | Implemented | GameManager, TurnManager, EventBus |
| Map Generation | Implemented | Terrain and nation procedural generation |
| Region Management | Implemented | Basic region functionality with terrain |
| Resource System | In Progress | Basic resource tracking, needs production chains |
| Economic Cycles | Planned | Framework ready, implementation pending |
| Production Chains | Planned | Basic structure defined, needs implementation |
| AI Nations | Initial | Basic AI turns with placeholder actions |
| UI Elements | Partial | Map view and region info implemented, resource dashboard needed |

## Resource System Design

```mermaid
classDiagram
    class ResourceComponent {
        -Dictionary~string,float~ resources
        -Dictionary~string,float~ productionRates
        -Dictionary~string,float~ consumptionRates
        +ProcessTurn()
        +AddResource(string, float)
        +RemoveResource(string, float)
        +GetResourceAmount(string)
        +CalculateBaseProduction()
    }
    
    class RegionEntity {
        +string regionName
        +int wealth
        +int production
        +TerrainTypeDataSO terrainType
        +ResourceComponent resources
        +UpdateEconomy(int, int)
        +GetDescription()
        +GetDetailedDescription()
    }
    
    class ProductionComponent {
        -Dictionary~string,float~ baseProductionRates
        -Dictionary~string,float~ productionMultipliers
        +CalculateProductionRate(string)
        +ProcessRecipes(ResourceComponent)
    }
    
    class ResourceDataSO {
        +string resourceName
        +ResourceType resourceType
        +float baseValue
        +bool isRawResource
        +ResourceProductionRecipe[] productionRecipes
    }
    
    class EconomicCycleSystem {
        +enum CyclePhase
        -CyclePhase currentPhase
        -Dictionary~string,float~ sectorModifiers
        +GetPhaseModifiers(string)
        +AdvanceCycle()
    }
    
    RegionEntity "1" --> "1" ResourceComponent
    RegionEntity "1" --> "0..1" ProductionComponent
    ResourceComponent ..> ResourceDataSO
    ProductionComponent ..> ResourceDataSO
    EconomicCycleSystem ..> ResourceComponent
```

## Next Implementation Priorities

1. Complete ResourceComponent integration
2. Implement basic ProductionComponent
3. Create EconomicCycleSystem with phase-based modifiers
4. Enhance UI to display resources and production chains
5. Develop region specialization mechanics
