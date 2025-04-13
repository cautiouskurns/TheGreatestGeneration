using UnityEngine;

namespace V2.Components
{
    public class PopulationComponent
    {
        public int LaborAvailable { get; private set; } = 100;
        public float Satisfaction { get; private set; } = 1.0f;

        public void UpdateSatisfaction(float value)
        {
            Satisfaction = Mathf.Clamp01(value);
            Debug.Log($"Population satisfaction updated: {Satisfaction:F2}");
        }

        public void UpdateLabor(int change)
        {
            LaborAvailable = Mathf.Max(0, LaborAvailable + change);
            Debug.Log($"Labor available updated: {LaborAvailable}");
        }
    }
}