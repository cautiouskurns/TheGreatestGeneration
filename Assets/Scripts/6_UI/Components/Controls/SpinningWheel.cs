/// CLASS PURPOSE:
/// SpinningWheel applies continuous rotation to a GameObject to create a simple
/// spinning animation effect, typically used for decorative UI or loading indicators.
///
/// CORE RESPONSIBILITIES:
/// - Rotate the GameObject around the Z-axis every frame based on a speed multiplier
///
/// KEY COLLABORATORS:
/// - Unity Engine: Uses MonoBehaviour lifecycle and transform updates
///
/// CURRENT ARCHITECTURE NOTES:
/// - Applies rotation in Update using Time.deltaTime for frame-independent speed
///
/// REFACTORING SUGGESTIONS:
/// - Allow axis configuration to enable multi-axis spinning
/// - Expose rotation direction and enable/disable control via public methods
///
/// EXTENSION OPPORTUNITIES:
/// - Add easing or acceleration effects
/// - Integrate with UI state changes (e.g., only spin while loading)
/// - Support dynamic speed adjustments or event-based triggers
using UnityEngine;

public class SpinningWheel : MonoBehaviour
{
    public float rotationSpeed = 300f; // degrees per second
    
    void Update()
    {
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
    }
}