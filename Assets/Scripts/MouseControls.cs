using UnityEngine;
using UnityEngine.Events;

public class MouseControls : MonoBehaviour
{
    public UnityEvent<Vector3> OnClick;
    
    void Update()
    {
        if (Input.GetMouseButtonDown(0)) {
            Ray mouseRay = Camera.main.ScreenPointToRay( Input.mousePosition );
            if (Physics.Raycast( mouseRay, out RaycastHit hitInfo )) {
                Vector3 clickWorldPosition = hitInfo.point;
                OnClick.Invoke(clickWorldPosition);
            }
        }
    }
}
