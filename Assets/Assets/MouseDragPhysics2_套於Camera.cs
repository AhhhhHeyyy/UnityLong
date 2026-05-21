using UnityEngine;

public class MouseDragPhysics : MonoBehaviour
{
    [Header("Dragging Settings")]
    public float dragForce = 20f;       
    public float dragDamping = 5f;     
    public LayerMask draggableLayer = ~0; // 預設為選取所有圖層

    private Rigidbody targetBody;       
    private float selectionDistance;    
    private Vector3 dragOffset;         

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TryGrabObject();
        }

        if (Input.GetMouseButtonUp(0))
        {
            ReleaseObject();
        }
    }

    void FixedUpdate()
    {
        if (targetBody != null)
        {
            DragObject();
        }
    }

    void TryGrabObject()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100f, draggableLayer))
        {
            if (hit.rigidbody != null)
            {
                targetBody = hit.rigidbody;
                
                // 核心修正：固定點擊時的距離，防止物件往相機衝
                selectionDistance = hit.distance;

                // 核心修正：記錄世界空間下的偏移量（物件中心 - 點擊點）
                // 這樣拖拽時會維持抓取那一刻的相對位置，不會瞬移
                dragOffset = targetBody.position - hit.point;

                if (!targetBody.isKinematic)
                {
                    targetBody.useGravity = false;
                    // Unity 2022.3+ 使用 linearDamping, 舊版請改為 drag
                    targetBody.linearDamping = dragDamping; 
                    targetBody.angularDamping = dragDamping;
                }
            }
        }
    }

    void DragObject()
    {
        // 根據初始距離計算滑鼠在世界空間的位置
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(
            Input.mousePosition.x, 
            Input.mousePosition.y, 
            selectionDistance
        ));

        // 目標位置 = 當前滑鼠世界位置 + 初始偏移量
        Vector3 targetPosition = mouseWorldPos + dragOffset;

        if (targetBody.isKinematic)
        {
            targetBody.MovePosition(targetPosition);
        }
        else
        {
            // 計算物體目前位置到目標位置的向量
            Vector3 forceDirection = targetPosition - targetBody.position;
            // 使用速度賦值，手感最紮實且不會偏移
            targetBody.linearVelocity = forceDirection * dragForce;
        }
    }

    void ReleaseObject()
    {
        if (targetBody != null)
        {
            if (!targetBody.isKinematic)
            {
                targetBody.useGravity = true;
                targetBody.linearDamping = 0.05f; 
                targetBody.angularDamping = 0.05f;
            }
            targetBody = null;
        }
    }
}