# Economic Cycles: Architecture Diagram

## Core Architecture

```mermaid
graph TD
    %% Core Managers
    subgraph "Core Managers"
        GM[GameManager]
        TM[TurnManager]
    end

    %% Data Layer
    subgraph "Data Layer"
        SO[ScriptableObjects]
        subgraph "Models"
            MM[MapModel]
            EM[EconomicModel]
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
        EC[EconomicComponent]
        VC[VisualComponent]
    end

    %% Systems
    subgraph "Systems"
        ES[EconomicSystem]
        MS[MapSystem]
        PS[ProjectSystem]
    end

    %% UI
    subgraph "User Interface"
        subgraph "Views"
            MV[MapView]
            RV[RegionView]
            EV[EconomicView]
        end
        subgraph "UI Components"
            RP[RegionPanel]
            RIU[RegionInfoUI]
            ED[EconomicDashboard]
        end
    end

    %% Controllers
    subgraph "Controllers"
        MC[MapController]
        CC[CameraController]
        PC[ProjectController]
    end

    %% Communication
    subgraph "Event Bus"
        EB[EventBus]
    end

    %% Utils
    subgraph "Utilities"
        MG[MapGenerator]
        RCH[RegionClickHandler]
        EF[EconomicFormulas]
    end

    %% Major Relationships
    GM --> MM
    GM --> TM
    MM --> RE
    MM --> NE
    MM -.-> EB
    RE --> RC
    RE --> EC
    
    MV --> SO
    MC -.-> EB
    MC --> GM
    
    MS -.-> EB
    MS --> MV
    
    ES -.-> EB
    ES --> RE
    
    TM -.-> EB
    
    RCH -.-> EB
    
    EB -.-> MS
    EB -.-> ES
    EB -.-> RIU
    EB -.-> MV
    
    RIU --> RE
    
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
    
    class GM,TM core
    class SO,MM,EM data
    class RE,NE,PE entities
    class RC,EC,VC components
    class ES,MS,PS systems
    class MV,RV,EV,RP,RIU,ED ui
    class MC,CC,PC controllers
    class EB events
    class MG,RCH,EF utils
```

## Data Flow

```mermaid
sequenceDiagram
    participant User
    participant RCH as RegionClickHandler
    participant MC as MapController
    participant GM as GameManager
    participant MM as MapModel
    participant EB as EventBus
    participant RE as RegionEntity
    participant MV as MapView
    participant RIU as RegionInfoUI
    participant TM as TurnManager
    participant ES as EconomicSystem
    
    %% Region Selection Flow
    User->>RCH: Click on Region
    RCH->>EB: Trigger "RegionClicked"
    EB->>MC: Notify of "RegionClicked"
    MC->>GM: SelectRegion(name)
    GM->>MM: SelectRegion(name)
    MM->>EB: Trigger "RegionSelected"
    EB->>MV: Notify of "RegionSelected"
    EB->>RIU: Notify of "RegionSelected"
    MV->>MV: Highlight selected region
    RIU->>RIU: Update info panel
    
    %% Turn Processing Flow
    User->>TM: Click "End Turn"
    TM->>EB: Trigger "TurnEnded"
    EB->>ES: Notify of "TurnEnded"
    EB->>MM: Notify of "TurnEnded"
    ES->>RE: UpdateEconomy()
    RE->>EB: Trigger "RegionUpdated"
    EB->>MV: Notify of "RegionUpdated"
    MV->>MV: Update visual representation
    note over MV: Show economy changes
```

## Event System

```mermaid
graph LR
    subgraph "Events"
        E1["RegionClicked"]
        E2["RegionSelected"]
        E3["RegionUpdated"]
        E4["RegionCreated"]
        E5["RegionEntitiesReady"]
        E6["TurnEnded"]
        E7["TurnProcessed"]
        E8["EconomicSystemReady"]
        E9["MapModelTurnProcessed"]
    end
    
    subgraph "Publishers"
        P1["RegionClickHandler"]
        P2["MapModel"]
        P3["RegionEntity"]
        P4["TurnManager"]
        P5["EconomicSystem"]
    end
    
    subgraph "Subscribers"
        S1["MapController"]
        S2["MapView"]
        S3["RegionInfoUI"]
        S4["MapSystem"]
        S5["EconomicSystem"]
        S6["MapModel"]
    end
    
    P1-->E1
    P2-->E2
    P2-->E5
    P2-->E9
    P3-->E3
    P4-->E6
    P5-->E8
    
    E1-->S1
    E2-->S2
    E2-->S3
    E2-->S4
    E3-->S2
    E3-->S4
    E5-->S2
    E6-->S5
    E6-->S6
    E8-->S2
```

## Component Dependencies

```mermaid
graph TD
    subgraph "Core Systems"
        GM[GameManager] --> MM[MapModel]
        GM --> MV[MapView]
        TM[TurnManager] --> EB[EventBus]
    end
    
    subgraph "Data Flow"
        MM --> RE[RegionEntity]
        MM --> NE[NationEntity]
        MM -.-> EB
        RE -.-> EB
    end
    
    subgraph "Input Handling"
        RCH[RegionClickHandler] -.-> EB
        EB --> MC[MapController]
        MC --> GM
    end
    
    subgraph "UI Updates"
        EB --> MV
        EB --> RIU[RegionInfoUI]
        MV --> RO[Region GameObjects]
    end
    
    subgraph "Turn Processing"
        EB --> ES[EconomicSystem]
        ES --> RE
    end
    
    %% External Dependencies
    SO[ScriptableObjects] --> GM
    SO --> MM
    SO --> MV
    
    %% Legend
    classDef core fill:#ff9900,stroke:#333,stroke-width:2px
    classDef data fill:#3498db,stroke:#333,stroke-width:2px
    classDef input fill:#2ecc71,stroke:#333,stroke-width:2px
    classDef ui fill:#e74c3c,stroke:#333,stroke-width:2px
    classDef processing fill:#9b59b6,stroke:#333,stroke-width:2px
    classDef external fill:#95a5a6,stroke:#333,stroke-width:2px
    
    class GM,TM core
    class MM,RE,NE data
    class RCH,MC input
    class MV,RIU,RO ui
    class ES,EB processing
    class SO external
```

## Folder Structure and Asset Organization

```mermaid
graph TD
    subgraph "Assets"
        Prefabs --> Map
        Prefabs --> UI
        
        ScriptableObjects --> Nations
        ScriptableObjects --> EconomicCycles
        ScriptableObjects --> Projects
        ScriptableObjects --> Regions
        ScriptableObjects --> Resources
        
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
        
        Models --> MapModel
        
        Entities --> RegionEntity
        
        Systems --> EconomicSystem
        Systems --> MapSystem
        
        UI_Scripts --> Views
        UI_Scripts --> UI_Components[Components]
        
        Views --> MapView
        UI_Components --> RegionInfoUI
        
        Controllers --> MapController
        Controllers --> CameraController
        
        Managers --> EventBus
        
        Utils --> Helpers
        
        Helpers --> RegionClickHandler
        Helpers --> MapGenerator
    end
```

This architecture diagram provides a comprehensive overview of the Economic Cycles project structure, showing the relationships between different components and the data flow through the system. The event-based communication model is clearly illustrated, showing how different parts of the system interact without direct references.
