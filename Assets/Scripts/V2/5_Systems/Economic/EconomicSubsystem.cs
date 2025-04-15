using V2.Entities;
using UnityEngine;

namespace V2.Systems.Economic
{
    /// <summary>
    /// Base abstract class for all economic subsystems
    /// </summary>
    public abstract class EconomicSubsystem
    {
        protected EconomicSystem economicSystem;

        public EconomicSubsystem(EconomicSystem system)
        {
            economicSystem = system;
        }

        /// <summary>
        /// Process the subsystem's logic for a given region
        /// </summary>
        public abstract void Process(RegionEntity region);
    }
}