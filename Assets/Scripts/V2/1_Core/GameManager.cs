using UnityEngine;
using V2.Entities;
using V2.Managers;

namespace V2.Core
{
    public class GameManager : MonoBehaviour
    {
        public RegionEntity testRegion;

        private void Start()
        {
            testRegion = new RegionEntity("Test Region", 100, 50);
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
            testRegion.ProcessTurn();
            Debug.Log(testRegion.GetSummary());
        }
    }
}
