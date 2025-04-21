# Map and Dialogue System Integration Setup

This document outlines how to set up the Map visualization with region selection and integrate it with the Economic and Dialogue systems.

## Scene Hierarchy Setup

Create a scene with the following hierarchy structure:

```
GameManager
├── EconomicSystem
│   └── DialogueEventManager
├── UI
│   ├── DialoguePanel
│   │   ├── TitleText (TMP)
│   │   ├── DescriptionText (TMP)
│   │   └── ChoicesContainer
│   └── HUD
├── Map
│   ├── MapCamera
│   └── RegionsContainer
```

## Step 1: Create the Region Prefab

1. Create a new empty GameObject and name it "RegionPrefab"
2. Add the following components:
   - `RectTransform` (if using Canvas-based UI) or a simple `Transform` (if using WorldSpace)
   - `Image` component for the region shape
   - `Button` component for click handling
   - `RegionView` script

3. Create a child GameObject for the selection indicator:
   - Name it "SelectionIndicator"
   - Add an `Image` component with a highlight sprite or color
   - Set it inactive by default

4. Create child GameObjects for the text displays:
   - "RegionName" with a TextMeshPro component
   - "WealthText" with a TextMeshPro component
   - "PopulationText" with a TextMeshPro component

5. Create a child GameObject for status indicators:
   - Name it "StatusIndicators" 
   - Add three UI Image components set to "Filled" for satisfaction, wealth, and production
   - Add the `RegionStatusIndicator` script and link the image components

6. Configure the Button component:
   - Add an OnClick event that calls `RegionView.OnClick()`

7. Save this as a prefab

## Step 2: Set up the Map System

1. Create an empty GameObject named "Map"
2. Add the `MapManager` script
3. Create a child GameObject named "MapCamera"
   - Add a Camera component
   - Set it to Orthographic
   - Position it to look down on the map
4. Create a child GameObject named "RegionsContainer"
5. Add the `RegionManager` script to the Map GameObject
6. Link the references:
   - Set the `regionPrefab` to your RegionPrefab asset
   - Set the `mapContainer` to the RegionsContainer Transform

## Step 3: Set up the Dialogue UI

1. Create a Canvas GameObject for your UI
2. Create a Panel named "DialoguePanel" 
   - Set it to be centered
   - Add a background image
3. Add the following child objects:
   - "TitleText" with TextMeshPro
   - "DescriptionText" with TextMeshPro
   - "ChoicesContainer" (empty GameObject)
4. Create a choice button prefab:
   - Create a Button with TextMeshPro
   - Save it as a prefab
5. Add the `DialogueUIManager` script to your DialoguePanel
6. Link the references:
   - Set `dialoguePanel` to the DialoguePanel GameObject
   - Set `titleText` and `descriptionText` to their respective TextMeshPro components
   - Set `choicesContainer` to the ChoicesContainer Transform
   - Set `choiceButtonPrefab` to your choice button prefab

## Step 4: Connect Systems in Play Mode

1. Enter Play Mode
2. Verify that regions appear in your map
3. Click on a region to select it
4. Confirm that the DialogueSystem triggers events
5. Test dialogue choices to ensure they affect the selected region

## Troubleshooting

- If regions don't appear, check that the RegionManager script is properly instantiating the prefab
- If dialogues don't show up, verify that the EventBus is correctly sending messages
- If economic effects don't apply, check that the RegionEntity is being updated

This is a minimal setup - you can expand each section with additional UI elements or visuals as needed.