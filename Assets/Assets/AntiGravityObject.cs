using UnityEngine;

public class AntiGravityObject : MonoBehaviour
{
    public float antiGravityForce = 9.81f; // 反重力力量，預設值為地球重力加速度
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("AntiGravityObject requires a Rigidbody component!");
            enabled = false; // 如果沒有Rigidbody，禁用腳本
        }
    }

    void FixedUpdate()
    {
        // FixedUpdate 適用於物理運算
        if (rb != null)
        {
            // 施加一個與重力方向相反的力
            // rb.mass * Physics.gravity.magnitude 可以抵消重力
            // antiGravityForce 可以調整額外的浮力
            rb.AddForce(Vector3.up * (rb.mass * Physics.gravity.magnitude + antiGravityForce), ForceMode.Force);
        }
    }

    // 可以新增一個方法來切換反重力狀態
    public void SetAntiGravity(bool enable)
    {
        if (rb != null)
        {
            if (enable)
            {
                // 確保重力開啟，這樣我們可以透過施力來抵消它
                rb.useGravity = true;
                // 在 FixedUpdate 中會持續施加反重力力
            }
            else
            {
                // 關閉反重力，讓物體恢復正常重力
                rb.useGravity = true; // 或設置為 false 如果你不希望它受到重力影響
            }
        }
    }
}
