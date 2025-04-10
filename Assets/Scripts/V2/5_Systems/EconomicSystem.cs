using UnityEngine;
using V2.Entities;
using V2.Managers;

namespace V2.Systems
{
    public class EconomicSystem : MonoBehaviour
    {
        public static EconomicSystem Instance { get; private set; }
        
        public RegionEntity testRegion;

        [Header("Simple Economic Settings")]
        public float productivityFactor = 1.0f;
        public float laborElasticity = 0.5f;
        public float capitalElasticity = 0.5f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject); // optional safety
                return;
            }
            Instance = this;
        }

        private void OnEnable()
        {
            EventBus.Subscribe("TurnEnded", OnTurnEnded);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe("TurnEnded", OnTurnEnded);
        }

        private void OnTurnEnded(object _)
        {
            ProcessEconomicTick();
        }

        [ContextMenu("Run Economic Tick")]
        public void ManualTick()
        {
            ProcessEconomicTick();
        }

        public void ProcessEconomicTick()
        {
            if (testRegion == null)
            {
                Debug.LogWarning("TestRegion not assigned.");
                return;
            }

            float labor = testRegion.laborAvailable;
            float capital = testRegion.infrastructureLevel;

            // Simple Cobb-Douglas production
            float production = productivityFactor 
                * Mathf.Pow(labor, laborElasticity) 
                * Mathf.Pow(capital, capitalElasticity);

            testRegion.Production = Mathf.RoundToInt(production);
            testRegion.Wealth += testRegion.Production;

            Debug.Log($"[Tick] {testRegion.Name} | Labor: {labor}, Capital: {capital}, Production: {testRegion.Production}, Wealth: {testRegion.Wealth}");
        }
    }
}