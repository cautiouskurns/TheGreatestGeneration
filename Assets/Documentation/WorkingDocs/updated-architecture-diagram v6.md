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
            TS[TurnState]
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
        RD[ResourceDependency]
    end

    %% Systems
    subgraph "Systems"
        ECS[EconomicSystem]
        EES[EnhancedEconomicSystem]
        MS[MapSystem]
        PS[ProjectSystem]
        RTS[TradeSystem]
        ECS2[EconomicCycleSystem]
        RVS[ResourceVisualizationSystem]
        RM[ResourceMarket]
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
            MOT[MapOverlayToggle]
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
        GSI[GameSystemInitializer]
        MGF[MapGenerationFactory]
        RI[ResourceInitializer]
        ASM[AutoSimulationManager]
        EESD[EnhancedEconomicSystemDebug]
    end

    %% Major Relationships
    GM --> MM
    GM --> NM
    GM --> TM
    GM --> GSI
    GM --> MGF
    GM --> RI
    GM --> EES
    
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
    PC --> RD
    
    MV --> SO
    MC -.-> EM
    MC --> GM
    
    MS -.-> EM
    MS --> MV
    
    ECS -.-> EM
    ECS --> RE
    ECS --> RC
    
    EES --> MM
    EES --> RM
    EES --> SO
    EES --> GSM
    EESD --> EES
    
    TM -.-> EM
    TM --> GSM
    TM --> ASM
    ASM --> TS
    
    RCH -.-> EM
    
    EM -.-> MS
    EM -.-> ECS
    EM -.-> RIU
    EM -.-> MV
    EM -.-> ECS2
    EM -.-> NDU
    EM -.-> GSD
    EM -.-> EES
    
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
    
    MV --> MOT
    MOT --> MV
    
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
    
    %% Resource Market connections
    RM -.-> EM
    
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
    class ES,DS,PHS,RS,TS state
    class SDE,DL,DC,DO dialogue
    class RE,NE,PE entities
    class RC,PC,REC,POPC,IC,RD components
    class ECS,MS,PS,RTS,ECS2,RVS,EES,RM systems
    class MV,RV,EV,RDV,RP,RIU,ED,AAD,NDU,GSD,RLT,MOT ui
    class MC,CC,PC2,AIC controllers
    class MG,RCH,NG,TG,MDV,DST,GSD2,GSI,MGF,RI,ASM,EESD utils
    class TC,TV,TR,TT,TI trade

    sequenceDiagram
    participant User
    participant TM as TurnManager
    participant EM as EventBus
    participant GSM as GameStateManager
    participant MM as MapModel
    participant NM as NationModel
    participant EES as EnhancedEconomicSystem
    participant RM as ResourceMarket
    participant RTS as TradeSystem
    participant RE as RegionEntity
    participant RC as ResourceComponent
    participant PC as ProductionComponent
    participant REC as RegionEconomyComponent
    participant POPC as RegionPopulationComponent
    participant AIC as AIController
    participant UI as UI Components
    
    User->>TM: Click "End Turn"
    TM->>GSM: IncrementTurn()
    TM->>GSM: SyncWithGameSystems()
    TM->>EM: Trigger "TurnEnded"
    
    %% Region Processing
    EM->>MM: Notify of "TurnEnded"
    MM->>RE: ProcessTurn() for each region
    RE->>RC: CalculateProduction()
    RC->>POPC: Get labor allocation
    POPC-->>RC: Return labor allocation
    RE->>RC: CalculateDemand()
    RC->>REC: Get wealth multiplier
    REC-->>RC: Return multiplier
    RE->>RC: ProcessTurn(wealth, regionSize)
    RC->>PC: ProcessProduction()
    PC-->>RC: Update resources based on recipes
    RC-->>RE: Update resource values
    RE->>POPC: UpdateSatisfaction(needsSatisfaction)
    POPC->>POPC: UpdatePopulation()
    POPC->>REC: Apply satisfaction effects
    RE->>EM: Trigger "RegionUpdated"
    
    %% Enhanced Economic System
    EM->>EES: Notify of "TurnEnded"
    EES->>EES: ProcessSupplyAndDemand()
    EES->>EES: ProcessProduction()
    EES->>EES: ProcessInfrastructure()
    EES->>EES: ProcessPopulationConsumption()
    EES->>EES: ProcessEconomicCycle()
    EES->>EES: ProcessPriceVolatility()
    EES->>RM: Update resource prices
    RM->>EM: Trigger "ResourcePricesUpdated"
    EES->>EM: Trigger "EnhancedEconomicProcessed"
    
    %% Trade System Processing
    EM->>RTS: Notify of "TurnEnded"
    RTS->>TC: CalculateTrades()
    TC-->>RTS: Return trade transactions
    RTS->>TT: Execute() for each transaction
    TT->>RE: Transfer resources between regions
    RTS->>TR: RecordTrade() for each transaction
    RTS->>TV: Visualize trades
    
    %% Nation and Turn Progression
    MM->>NM: ProcessTurn()
    NM->>NM: UpdateAllNations()
    NM->>EM: Trigger "NationModelUpdated"
    EM->>TM: Trigger "PlayerTurnEnded"
    TM->>AIC: Notify of "PlayerTurnEnded"
    AIC->>AIC: ProcessAITurns()
    AIC->>EM: Trigger "AITurnsCompleted"
    EM->>TM: Trigger "TurnProcessed"
    EM->>UI: Notify of updates
    UI-->>User: Update visual display


    sequenceDiagram
    participant RE as RegionEntity
    participant REC as RegionEconomyComponent
    participant POPC as RegionPopulationComponent
    participant RC as ResourceComponent
    participant PC as ProductionComponent
    participant RM as ResourceMarket
    participant TS as TradeSystem
    
    %% Production Calculation
    RE->>RC: CalculateProduction()
    RC->>POPC: Get labor allocation
    POPC-->>RC: Return labor allocation
    RC->>RC: Apply terrain & efficiency modifiers
    RC->>RC: Apply economic cycle modifiers
    
    %% Demand Calculation
    RE->>RC: CalculateDemand()
    RC->>POPC: Get population size
    POPC-->>RC: Return population size
    RC->>RM: Get market prices
    RM-->>RC: Return current prices
    RC->>RC: Calculate price elasticity
    RC->>REC: Get wealth multiplier
    REC-->>RC: Return wealth effect on consumption
    RC->>RC: Calculate final consumption rates
    
    %% Processing
    RE->>RC: ProcessTurn(wealth, regionSize)
    RC->>PC: ProcessProduction()
    PC->>PC: Check recipe dependencies
    PC->>PC: Apply production efficiency
    PC->>RC: RemoveResource() for inputs
    PC->>RC: AddResource() for outputs
    RC->>RC: Apply production and consumption
    
    %% Trade
    TS->>RC: Query for resource surpluses/deficits
    RC-->>TS: Return resource data
    TS->>TS: Calculate possible trades
    TS->>TS: Apply trade efficiency
    TS->>TS: Apply distance factors
    TS->>RC: Execute trade transactions
    RC-->>RE: Update resource stockpiles
    
    %% Satisfaction & Updates
    RC->>POPC: Update satisfaction
    POPC->>POPC: Calculate unmet needs impact
    POPC->>REC: Apply satisfaction effects
    REC->>RE: Update economy values
    RE->>RE: Trigger RegionUpdated event

    graph TD
    subgraph "Enhanced Economic System"
        EES[EnhancedEconomicSystem]
        EESO[EnhancedEconomicModelSO]
        EESD[EnhancedEconomicSystemDebug]
    end
    
    subgraph "Economic Model Configuration"
        SDR[SupplyDemandRules]
        PTR[ProductionTransformationRules]
        IR[InfrastructureImpactRules]
        PCR[PopulationConsumptionRules]
        ECR[EconomicCycleRules]
        PVR[PriceVolatilityRules]
    end
    
    subgraph "Economic Processing Methods"
        PSD[ProcessSupplyAndDemand]
        PP[ProcessProduction]
        PI[ProcessInfrastructure]
        PPC[ProcessPopulationConsumption]
        PEC[ProcessEconomicCycle]
        PPV[ProcessPriceVolatility]
        DES[DebugEconomicState]
    end
    
    EES --> EESO
    EESO --> SDR
    EESO --> PTR
    EESO --> IR
    EESO --> PCR
    EESO --> ECR
    EESO --> PVR
    
    EES --> PSD
    EES --> PP
    EES --> PI
    EES --> PPC
    EES --> PEC
    EES --> PPV
    EES --> DES
    
    EESD --> EES
    EESD --> DES
    
    EES --> MM[MapModel]
    EES --> RM[ResourceMarket]
    EES --> GSM[GameStateManager]
    
    %% Class styling
    classDef system fill:#9b59b6,stroke:#333,stroke-width:2px
    classDef config fill:#3498db,stroke:#333,stroke-width:2px
    classDef method fill:#1abc9c,stroke:#333,stroke-width:2px
    classDef debug fill:#95a5a6,stroke:#333,stroke-width:2px
    
    class EES system
    class EESO config
    class SDR,PTR,IR,PCR,ECR,PVR config
    class PSD,PP,PI,PPC,PEC,PPV,DES method
    class EESD debug

    graph TD
    subgraph "Trade System Architecture"
        TS[TradeSystem]
        subgraph "Core Components"
            TC[TradeCalculator]
            TR[TradeRecorder]
            TV[TradeVisualizer]
        end
        subgraph "Data Structures"
            TT[TradeTransaction]
            TI[TradeInfo]
        end
        subgraph "Configuration"
            TSC[TradeSystemConfig]
        end
        subgraph "Methods"
            PT[ProcessTrade]
            CT[CalculateTrades]
            RTL[RefreshTradeLines]
            SRTL[ShowRegionTradeLines]
            DTS[DebugTradeSystem]
        end
    end
    
    %% Main dependencies
    TS --> TC
    TS --> TR
    TS --> TV
    TS --> PT
    
    TC --> CT
    TC --> TT
    
    TV --> RTL
    TV --> SRTL
    
    TR --> TI
    
    TS --> TSC
    
    %% External connections
    TS -.-> EM[EventBus]
    TS --> GM[GameManager]
    TS -.-> RE[RegionEntity]
    
    %% Class styling
    classDef system fill:#9b59b6,stroke:#333,stroke-width:2px
    classDef component fill:#1abc9c,stroke:#333,stroke-width:2px
    classDef data fill:#3498db,stroke:#333,stroke-width:2px
    classDef method fill:#95a5a6,stroke:#333,stroke-width:2px
    classDef config fill:#f1c40f,stroke:#333,stroke-width:2px
    
    class TS system
    class TC,TR,TV component
    class TT,TI data
    class PT,CT,RTL,SRTL,DTS method
    class TSC config

    graph TD
    subgraph "Resource Market System"
        RM[ResourceMarket]
        subgraph "Data Structures"
            BP[basePrices]
            CP[currentPrices]
            PH[priceHistory]
        end
        subgraph "Methods"
            UP[UpdatePrices]
            GPS[CalculateGlobalSupply]
            GPD[CalculateGlobalDemand]
            AP[AdjustPrice]
            GCP[GetCurrentPrice]
            GPH[GetPriceHistory]
            GPR[GetPriceRatio]
            UDI[UpdateDebugInfo]
        end
    end
    
    %% Main connections
    RM --> BP
    RM --> CP
    RM --> PH
    
    RM --> UP
    RM --> GPS
    RM --> GPD
    RM --> AP
    RM --> GCP
    RM --> GPH
    RM --> GPR
    RM --> UDI
    
    %% External connections
    RM -.-> EM[EventBus]
    RM -.-> EES[EnhancedEconomicSystem]
    RM -.-> UI[UI Components]
    
    %% Class styling
    classDef system fill:#9b59b6,stroke:#333,stroke-width:2px
    classDef data fill:#3498db,stroke:#333,stroke-width:2px
    classDef method fill:#1abc9c,stroke:#333,stroke-width:2px
    
    class RM system
    class BP,CP,PH data
    class UP,GPS,GPD,AP,GCP,GPH,GPR,UDI method

    graph TD
    subgraph "RegionEntity with Components"
        RE[RegionEntity]
        subgraph "Core Properties"
            ID[regionName]
            ON[ownerNationName]
            RC1[regionColor]
            TT[terrainType]
        end
        subgraph "Components"
            REC[RegionEconomyComponent]
            POPC[RegionPopulationComponent]
            RC2[ResourceComponent]
            PC[ProductionComponent]
        end
        subgraph "Methods"
            PT[ProcessTurn]
            UE[UpdateEconomy]
            GD[GetDescription]
            GDD[GetDetailedDescription]
            CRB[CalculateResourceBalance]
            RCF[ResetChangeFlags]
        end
    end
    
    %% Main connections
    RE --> ID
    RE --> ON
    RE --> RC1
    RE --> TT
    
    RE --> REC
    RE --> POPC
    RE --> RC2
    RE --> PC
    
    RE --> PT
    RE --> UE
    RE --> GD
    RE --> GDD
    RE --> CRB
    RE --> RCF
    
    %% Component relationships
    PC --> RC2
    REC --> POPC
    PC --> REC
    
    %% External connections
    RE -.-> EM[EventBus]
    RE --> NE[NationEntity]
    
    %% Class styling
    classDef entity fill:#2ecc71,stroke:#333,stroke-width:2px
    classDef property fill:#3498db,stroke:#333,stroke-width:2px
    classDef component fill:#1abc9c,stroke:#333,stroke-width:2px
    classDef method fill:#95a5a6,stroke:#333,stroke-width:2px
    
    class RE entity
    class ID,ON,RC1,TT property
    class REC,POPC,RC2,PC component
    class PT,UE,GD,GDD,CRB,RCF method

    sequenceDiagram
    participant GM as GameManager
    participant MGF as MapGenerationFactory
    participant GSI as GameSystemInitializer
    participant RI as ResourceInitializer
    participant MM as MapModel
    participant NM as NationModel
    participant RTS as TradeSystem
    participant EES as EnhancedEconomicSystem
    
    %% Initialize Components
    GM->>GM: InitializeComponents()
    GM->>MGF: Create instance
    GM->>GSI: Create instance
    GM->>RI: Create instance
    
    %% Generate Map Data
    GM->>MGF: CreateMap(config, terrainTypes, nationTemplates)
    MGF->>MGF: CreateMapGenerator(config)
    MGF->>MGF: ConfigureGenerator(generator, config, terrainTypes)
    MGF-->>GM: Return MapDataSO
    
    %% Initialize Game Systems
    GM->>MGF: CreateTerrainDictionary()
    MGF-->>GM: Return terrainTypes dictionary
    
    GM->>GSI: InitializeGameSystems(mapData, terrainTypes)
    GSI->>MM: Create MapModel(mapData, terrainTypes)
    GSI->>NM: Create NationModel(mapData)
    GSI->>GSI: RegisterRegionsWithNations(mapModel, nationModel)
    GSI->>RTS: InitializeTradeSystem(mapModel)
    GSI->>GSI: EnsureGameStateManager()
    GSI-->>GM: Return GameSystemsConfig
    
    %% Store references
    GM->>GM: Store mapModel
    GM->>GM: Store nationModel
    GM->>GM: Store tradeSystem
    
    %% Initialize Resources
    GM->>RI: InitializeRegionResources(mapModel.GetAllRegions(), availableResources)
    RI->>RI: Load resource definitions for each region
    RI->>RI: Activate default recipes
    RI->>RI: Create initial resource imbalances
    
    %% Initialize Enhanced Economic System
    GM->>GM: Find or create EnhancedEconomicSystem
    GM->>EES: Configure with economic model
    
    %% Apply Global Settings
    GM->>GM: ApplyGlobalSettings()




    graph TD
    subgraph "Game State Management"
        GSM[GameStateManager]
        subgraph "State Components"
            ES[EconomyState]
            DS[DiplomacyState]
            PHS[PlayerHistoryState]
            RS[RegionState]
        end
        subgraph "State Methods"
            GS[GetState]
            SS[SetState]
            GCT[GetCurrentTurn]
            IT[IncrementTurn]
            SWG[SyncWithGameSystems]
            IRS[IsResourceInShortage]
            GNR[GetNationRelation]
            GRS[GetRegionSatisfaction]
            GECP[GetCurrentEconomicCyclePhase]
        end
    end
    
    subgraph "Dialogue System"
        EDM[EventDialogueManager]
        subgraph "Dialogue Components"
            SDE[SimpleDialogueEvent]
            DL[DialogueLine]
            DC[DialogueChoice]
            DO[DialogueOutcome]
        end
        subgraph "Dialogue Methods"
            SD[ShowDialogue]
            SDW[ShowDialogueWithChoices]
            SDE2[ShowDialogueEvent]
            DCL[DisplayCurrentLine]
            DC2[DisplayChoices]
            SEC[ShowEventChoices]
            CCC[CheckChoiceCondition]
            PO[ProcessOutcome]
            PT[ProcessTextWithStateVariables]
        end
    end
    
    %% Main connections
    GSM --> ES
    GSM --> DS
    GSM --> PHS
    GSM --> RS
    
    GSM --> GS
    GSM --> SS
    GSM --> GCT
    GSM --> IT
    GSM --> SWG
    GSM --> IRS
    GSM --> GNR
    GSM --> GRS
    GSM --> GECP
    
    EDM --> SDE
    SDE --> DL
    SDE --> DC
    DC --> DO
    
    EDM --> SD
    EDM --> SDW
    EDM --> SDE2
    EDM --> DCL
    EDM --> DC2
    EDM --> SEC
    EDM --> CCC
    EDM --> PO
    EDM --> PT
    
    %% Cross-system connections
    EDM --> GSM
    SDE --> ES
    DC --> ES
    DO --> GSM
    
    %% External connections
    GSM -.-> TM[TurnManager]
    GSM -.-> GM[GameManager]
    EDM -.-> EM[EventBus]
    
    %% Class styling
    classDef core fill:#ff9900,stroke:#333,stroke-width:2px
    classDef state fill:#16a085,stroke:#333,stroke-width:2px
    classDef dialogue fill:#d35400,stroke:#333,stroke-width:2px
    classDef method fill:#95a5a6,stroke:#333,stroke-width:2px
    
    class GSM,EDM core
    class ES,DS,PHS,RS state
    class SDE,DL,DC,DO dialogue
    class GS,SS,GCT,IT,SWG,IRS,GNR,GRS,GECP,SD,SDW,SDE2,DCL,DC2,SEC,CCC,PO,PT method



    graph TD
    subgraph "Turn Management"
        TM[TurnManager]
        TS[TurnState]
        ASM[AutoSimulationManager]
        TMC[TurnManagerConfigSO]
    end
    
    subgraph "TurnState Properties"
        IPT[IsPlayerTurn]
        IASE[IsAutoSimulationEnabled]
        CT[CurrentTurn]
        CAT[CurrentAutoTurn]
        DAS[DefaultAutoSimulation]
    end
    
    subgraph "TurnManager Methods"
        ET[EndTurn]
        PT[ProcessTurnEnd]
        OATC[OnAITurnsCompleted]
        TAS[ToggleAutoSimulation]
        OASTC[OnAutoSimulationToggleChanged]
        UUI[UpdateUIForAutoSimulation]
        USS[UpdateSimulationStatus]
        EGS[EnsureGameStateManager]
    end
    
    subgraph "AutoSimulation Methods"
        SAS[StartAutoSimulation]
        SAS2[StopAutoSimulation]
        ASR[AutoSimulationRoutine]
    end
    
    %% Main connections
    TM --> TS
    TM --> ASM
    TM --> TMC
    
    TS --> IPT
    TS --> IASE
    TS --> CT
    TS --> CAT
    TS --> DAS
    
    TM --> ET
    TM --> PT
    TM --> OATC
    TM --> TAS
    TM --> OASTC
    TM --> UUI
    TM --> USS
    TM --> EGS
    
    ASM --> SAS
    ASM --> SAS2
    ASM --> ASR
    
    %% External connections
    TM -.-> EM[EventBus]
    TM --> GSM[GameStateManager]
    
    %% Class styling
    classDef core fill:#ff9900,stroke:#333,stroke-width:2px
    classDef state fill:#16a085,stroke:#333,stroke-width:2px
    classDef config fill:#3498db,stroke:#333,stroke-width:2px
    classDef method fill:#95a5a6,stroke:#333,stroke-width:2px
    classDef component fill:#1abc9c,stroke:#333,stroke-width:2px
    
    class TM core
    class TS state
    class TMC config
    class ET,PT,OATC,TAS,OASTC,UUI,USS,EGS method
    class ASM component
    class SAS,SAS2,ASR method



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
        ScriptableObjects --> Game
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
        
        ScriptableObjects_Script --> Map[Map SOs]
        ScriptableObjects_Script --> Economy[Economy SOs] 
        ScriptableObjects_Script --> Game[Game SOs]
        
        Map --> MapDataSO
        Map --> TerrainTypeDataSO
        Map --> RegionTypeDataSO
        Map --> NationDataSO
        Map --> TerrainMapDataSO
        Map --> RegionMapDataSO
        
        Economy --> ResourceDataSO
        Economy --> ProjectDataSO
        Economy --> EconomicCycleDataSO
        Economy --> ResourceBalanceDataSO
        Economy --> EnhancedEconomicModelSO
        Economy --> InfrastructureTypeDataSO
        
        Game --> GameConfigurationSO
        Game --> TurnManagerConfigSO
        
        Models --> MapModel
        Models --> NationModel
        Models --> States[State Models]
        Models --> SimpleDialogueEvent