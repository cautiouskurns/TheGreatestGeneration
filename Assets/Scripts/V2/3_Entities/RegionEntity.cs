using UnityEngine;
using V2.Components;
using V2.Managers;
using System.Collections.Generic;
using System.Linq;

namespace V2.Entities
{
    /// <summary>
    /// CLASS PURPOSE:
    /// RegionEntity represents a single economic unit in the simulation. It encapsulates basic
    /// economic state (wealth, production) and delegates resource and production behavior
    /// to internal components.
    ///
    /// CORE RESPONSIBILITIES:
    /// - Maintain core simulation variables: wealth and production
    /// - Delegate resource generation and production processing to internal components
    /// - React to global turn-based events via `ProcessTurn`
    /// - Emit region updates using the EventBus
    ///
    /// KEY COLLABORATORS:
    /// - ResourceComponent: Manages raw resource generation and storage
    /// - ProductionComponent: Simulates transformation of resources into production output
    /// - EventBus: Sends notifications such as "RegionUpdated" for UI or other systems
    ///
    /// CURRENT ARCHITECTURE NOTES:
    /// - Designed for simplicity and testability in early-stage prototypes
    /// - Encapsulates internal systems to ensure isolation and modular growth
    /// - No dependencies on visual/UI systems to remain headless and simulation-driven
    ///
    /// REFACTORING SUGGESTIONS:
    /// - Introduce a RegionConfig or RegionTemplate for dynamic instantiation
    /// - Add support for multiple economic subsystems (infrastructure, policies, traits)
    /// - Split economic logic into strategies for adjustable rulesets
    ///
    /// EXTENSION OPPORTUNITIES:
    /// - Add meta-data like region size, terrain, owner nation, or name localization
    /// - Expose computed economic metrics like satisfaction, balance sheets, or risk levels
    /// - Support event history or logs for replay/debugging purposes
    /// </summary>
    public class RegionEntity
    {
        public string Name { get;  set; }
        public int Wealth { get;  set; }
        public int Production { get;  set; }
        public int laborAvailable { get;  set; }
        public int infrastructureLevel { get;  set; }

        private ResourceComponent resources;
        private ProductionComponent productionComponent;

        private List<int> wealthHistory = new();
        private List<int> productionHistory = new();
        private const int maxHistoryLength = 50;

        public RegionEntity(string name, int initialWealth, int initialProduction)
        {
            Name = name;
            Wealth = initialWealth;
            Production = initialProduction;
            laborAvailable = 100;
            infrastructureLevel = 1;
            resources = new ResourceComponent();
            productionComponent = new ProductionComponent(resources);
        }

        /// <summary>
        /// Called on each simulation turn. Triggers core economic logic.
        /// </summary>
        public void ProcessTurn()
        {
            Debug.Log($"[Region: {Name}] Processing Turn...");
            resources.GenerateResources();
            productionComponent.ProcessProduction();
            Wealth += 5;
            Production += 10;
            Debug.Log($"[Region: {Name}] Wealth: {Wealth}, Production: {Production}");

            wealthHistory.Add(Wealth);
            productionHistory.Add(Production);

            if (wealthHistory.Count > maxHistoryLength)
            {
                wealthHistory.RemoveAt(0);
                productionHistory.RemoveAt(0);
            }

            //PrintAsciiGraph();
            EventBus.Trigger("RegionUpdated", this);
        }

        /// <summary>
        /// Provides a string summary of the region's current state.
        /// </summary>
        public string GetSummary()
        {
            return $"[{Name}] Wealth: {Wealth}, Production: {Production}, Resources: {resources.GetResourceOverview()}";
        }

    private void PrintAsciiGraph()
    {
        int graphHeight = 10;
        int width = Mathf.Min(wealthHistory.Count, maxHistoryLength);
        int maxWealth = wealthHistory.DefaultIfEmpty(1).Max();
        int maxProduction = productionHistory.DefaultIfEmpty(1).Max();

        Debug.Log($"\nRegion: {Name} — ASCII Graphs");

        // Generate Wealth Graph
        Debug.Log("\nWealth:");
        for (int y = 0; y < graphHeight; y++)
        {
            float threshold = 1f - (float)y / (graphHeight - 1);
            int yLabelValue = Mathf.RoundToInt(threshold * maxWealth);
            string line = $"{yLabelValue.ToString().PadLeft(4)} |";

            // Create a full-width empty space first
            char[] graphLine = new char[maxHistoryLength];
            for (int i = 0; i < maxHistoryLength; i++) {
                graphLine[i] = ' ';
            }
            
            // Fill from the right side (most recent data on the right)
            for (int i = 0; i < width; i++)
            {
                int dataIndex = wealthHistory.Count - width + i;
                int graphIndex = maxHistoryLength - width + i;
                char symbol = (float)wealthHistory[dataIndex] / maxWealth >= threshold ? '█' : ' ';
                graphLine[graphIndex] = symbol;
            }
            
            line += new string(graphLine);
            Debug.Log(line);
        }

        // Create right-aligned x-axis
        string wealthAxis = "     +" + new string('-', maxHistoryLength);
        
        // X-axis labels, right-aligned
        char[] xLabels = new char[maxHistoryLength];
        for (int i = 0; i < maxHistoryLength; i++) {
            xLabels[i] = '.';
        }
        // Add markers every 10 ticks
        for (int i = 0; i < maxHistoryLength; i += 10) {
            if (i < xLabels.Length) xLabels[i] = '|';
        }
        string wealthXLabels = "      " + new string(xLabels);
        
        Debug.Log(wealthAxis);
        Debug.Log(wealthXLabels);

        // Generate Production Graph
        Debug.Log("\nProduction:");
        for (int y = 0; y < graphHeight; y++)
        {
            float threshold = 1f - (float)y / (graphHeight - 1);
            int yLabelValue = Mathf.RoundToInt(threshold * maxProduction);
            string line = $"{yLabelValue.ToString().PadLeft(4)} |";

            // Create a full-width empty space first
            char[] graphLine = new char[maxHistoryLength];
            for (int i = 0; i < maxHistoryLength; i++) {
                graphLine[i] = ' ';
            }
            
            // Fill from the right side (most recent data on the right)
            for (int i = 0; i < width; i++)
            {
                int dataIndex = productionHistory.Count - width + i;
                int graphIndex = maxHistoryLength - width + i;
                char symbol = (float)productionHistory[dataIndex] / maxProduction >= threshold ? '█' : ' ';
                graphLine[graphIndex] = symbol;
            }
            
            line += new string(graphLine);
            Debug.Log(line);
        }

        // Create right-aligned x-axis for production
        string prodAxis = "     +" + new string('-', maxHistoryLength);
        
        // X-axis labels, consistent with the wealth graph
        string prodXLabels = "      " + new string(xLabels);
        
        Debug.Log(prodAxis);
        Debug.Log(prodXLabels);

        Debug.Log($"[Tick {wealthHistory.Count}] Wealth: {Wealth}, Production: {Production}");
    }
    }
}