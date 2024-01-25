#if UNITY_VISIONOS
using Unity.PolySpatial.InputDevices;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;
#endif
using UnityEngine;

public class VisionOSTouchInputManager : MonoBehaviour
{
 
    void OnEnable()
    {
#if UNITY_VISIONOS
        EnhancedTouchSupport.Enable();
#endif
    }

    void Update()
    {
#if UNITY_VISIONOS
        var activeTouches = Touch.activeTouches;

        if (activeTouches.Count > 0)
        {
            var primaryTouchData = EnhancedSpatialPointerSupport.GetPointerState(activeTouches[0]);
            if (activeTouches[0].phase == TouchPhase.Began)
            {
                var buttonObject = primaryTouchData.targetObject;
                if (buttonObject != null)
                {
                    if (buttonObject.TryGetComponent(out ButtonClick button))
                    {

                        button.Press();
                    }
                }
            }
        }
#endif
    }
}
