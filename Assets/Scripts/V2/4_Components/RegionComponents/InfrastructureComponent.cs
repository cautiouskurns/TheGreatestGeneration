using UnityEngine;

namespace V2.Components
{
    public class InfrastructureComponent
    {
        public int Level { get; private set; } = 1;
        private float maintenanceCost = 0.5f;

        public InfrastructureComponent()
        {
            // Initialize with default values
        }

        public void Upgrade()
        {
            Level++;
            maintenanceCost = Level * 0.5f;
            Debug.Log($"Infrastructure upgraded to level {Level}");
        }

        public float GetMaintenanceCost()
        {
            return maintenanceCost;
        }
    }
}