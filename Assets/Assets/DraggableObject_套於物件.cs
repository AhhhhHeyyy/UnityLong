using UnityEngine;

public class DraggableObject : MonoBehaviour
{
    private Rigidbody rb;
    private float mZCoord;
    private Vector3 mOffset;

    void Start() {
        rb = GetComponent<Rigidbody>();
    }

    void OnMouseDown() {
        mZCoord = Camera.main.WorldToScreenPoint(gameObject.transform.position).z;
        mOffset = gameObject.transform.position - GetMouseWorldPos();
        
        // 抓取時關閉重力避免衝突
        if(rb) rb.useGravity = false;
    }

    private Vector3 GetMouseWorldPos() {
        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = mZCoord;
        return Camera.main.ScreenToWorldPoint(mousePoint);
    }

    void OnMouseDrag() {
        if(rb) {
            rb.MovePosition(GetMouseWorldPos() + mOffset);
        }
    }

    void OnMouseUp() {
        if(rb) rb.useGravity = true;
    }
}
