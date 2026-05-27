using UnityEngine;

/// <summary>
/// 網頁按鈕按下(0x03) → 從 aimPoint 發射 Raycast，
/// 射線路徑上第一個有 DraggableObject 的物件才會被抓取，其餘全部穿透。
/// 持有期間物件跟著 lockTarget 位置移動。
/// 按鈕放開(0x04) → 放開物件。
/// </summary>
public class WebRtcSelectController : MonoBehaviour
{
    [Header("瞄準點")]
    [Tooltip("Raycast 從這裡射出（方向參考用，本身不會被命中）")]
    public Transform aimPoint;

    [Tooltip("射線固定朝向此物件；持有時物件跟著此 Transform 移動")]
    public Transform lockTarget;

    [Header("抓取設定")]
    public float dragForce   = 20f;
    public float dragDamping = 5f;
    public LayerMask draggableLayer = ~0;

    [Header("射線視覺化")]
    public float rayLength = 100f;
    public Color rayColor  = Color.red;

    private Rigidbody  targetBody;
    private Vector3    holdOffset;
    private bool       _isHolding;
    private LineRenderer _line;

    void Awake()
    {
        _line = gameObject.AddComponent<LineRenderer>();
        _line.positionCount = 2;
        _line.startWidth    = 0.02f;
        _line.endWidth      = 0.02f;
        _line.material      = new Material(Shader.Find("Sprites/Default"));
        _line.startColor    = rayColor;
        _line.endColor      = rayColor;
        _line.useWorldSpace = true;
    }

    void OnEnable()
    {
        SensorEvents.OnGrabPressed  += OnPressed;
        SensorEvents.OnGrabReleased += Release;
    }

    void OnDisable()
    {
        SensorEvents.OnGrabPressed  -= OnPressed;
        SensorEvents.OnGrabReleased -= Release;
    }

    private void OnPressed() => _isHolding = true;

    void Update()
    {
        if (aimPoint != null)
        {
            Vector3 start = aimPoint.position;
            Vector3 end   = start + GetDir() * rayLength;
            _line.SetPosition(0, start);
            _line.SetPosition(1, end);
            _line.enabled = true;
        }
        else
        {
            _line.enabled = false;
        }

        if (!_isHolding || aimPoint == null) return;

        Vector3 dir = GetDir();

        if (targetBody != null) return;

        // 取得射線上所有命中，依距離排序，找第一個有 DraggableObject 的
        RaycastHit[] hits = Physics.RaycastAll(aimPoint.position, dir, 100f, draggableLayer);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (RaycastHit hit in hits)
        {
            // 跳過 aimPoint 與 lockTarget 自身的 collider
            if (hit.collider.transform.IsChildOf(aimPoint.root)) continue;
            if (lockTarget != null && hit.collider.transform.IsChildOf(lockTarget.root)) continue;

            DraggableObject draggable = hit.collider.GetComponentInParent<DraggableObject>();
            if (draggable == null) continue;

            // 往上找父物件的 Rigidbody（移動整個父物件）
            Rigidbody rb = draggable.GetComponentInParent<Rigidbody>();
            if (rb == null)
            {
                Debug.LogWarning($"[Grab] {hit.collider.name} 有 DraggableObject 但父層缺少 Rigidbody");
                continue;
            }

            Debug.Log($"[Grab] 命中: {hit.collider.name}，距離: {hit.distance:F2}");
            targetBody = rb;

            // 記錄抓取當下 lockTarget（有則用，無則用 aimPoint）到物件的偏移
            Transform anchor = lockTarget != null ? lockTarget : aimPoint;
            holdOffset = rb.position - anchor.position;

            if (!rb.isKinematic)
            {
                rb.useGravity     = false;
                rb.linearDamping  = dragDamping;
                rb.angularDamping = dragDamping;
            }
            break;
        }
    }

    void FixedUpdate()
    {
        if (targetBody == null) return;

        Transform anchor = lockTarget != null ? lockTarget : aimPoint;
        if (anchor == null) return;

        // 目標位置 = anchor 當前位置 + 抓取時的偏移 → 物件跟著 lockTarget 移動
        Vector3 target = anchor.position + holdOffset;

        if (targetBody.isKinematic)
            targetBody.MovePosition(target);
        else
            targetBody.linearVelocity = (target - targetBody.position) * dragForce;
    }

    private void Release()
    {
        _isHolding = false;
        if (targetBody == null) return;

        if (!targetBody.isKinematic)
        {
            targetBody.useGravity     = true;
            targetBody.linearDamping  = 0.05f;
            targetBody.angularDamping = 0.05f;
        }
        targetBody = null;
    }

    private Vector3 GetDir() => lockTarget != null
        ? (lockTarget.position - aimPoint.position).normalized
        : aimPoint.forward;
}
