using UnityEngine;
using V2.Managers;

namespace V2.Core
{
    public class TurnManager : MonoBehaviour
    {
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Debug.Log("Turn ended");
                EventBus.Trigger("TurnEnded");
            }
        }
    }
}