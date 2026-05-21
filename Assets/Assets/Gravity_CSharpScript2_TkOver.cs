using UnityEngine;
using System.Collections.Generic;

public class MaxStyleGravityPro : MonoBehaviour
{
    public enum GravityType { Planar, Spherical }

    [Header("基礎設定")]
    public GravityType type = GravityType.Planar;
    public float strength = 9.81f;
    public float range = 50f;

    [Header("物理接管邏輯")]
    [Tooltip("開啟：離開區域後保持無重力狀態。關閉：離開區域後恢復 Unity 原生重力。")]
    public bool maintainWeightlessOnExit = false;

    [Header("過濾設定")]
    public LayerMask affectedLayers = -1;
    public List<Rigidbody> targetObjects = new List<Rigidbody>();
    public bool useIncludeListOnly = false;

    // 用來追蹤目前正在受影響的物件，以便在離開時處理恢復
    private HashSet<Rigidbody> currentlyAffected = new HashSet<Rigidbody>();

    void FixedUpdate()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, range, affectedLayers);
        HashSet<Rigidbody> hitRigidbodies = new HashSet<Rigidbody>();

        foreach (var col in colliders)
        {
            Rigidbody rb = col.attachedRigidbody;
            if (rb == null || rb.isKinematic) continue;

            // 過濾邏輯
            bool isInList = targetObjects.Contains(rb);
            if (useIncludeListOnly && !isInList) continue;
            if (!useIncludeListOnly && isInList && targetObjects.Count > 0) continue;

            hitRigidbodies.Add(rb);
            
            // 進入/持續在區域內：關閉原生重力並施加力場
            rb.useGravity = false;
            ApplyGravity(rb);
            
            if (!currentlyAffected.Contains(rb))
            {
                currentlyAffected.Add(rb);
            }
        }

        // 處理離開區域的物件
        List<Rigidbody> toRemove = new List<Rigidbody>();
        foreach (var rb in currentlyAffected)
        {
            if (!hitRigidbodies.Contains(rb))
            {
                // 物件離開了範圍
                ExitEffect(rb);
                toRemove.Add(rb);
            }
        }

        foreach (var rb in toRemove)
        {
            currentlyAffected.Remove(rb);
        }
    }

    void ApplyGravity(Rigidbody rb)
    {
        Vector3 forceDirection = (type == GravityType.Planar) 
            ? -transform.up 
            : (transform.position - rb.position).normalized;

        rb.AddForce(forceDirection * strength, ForceMode.Acceleration);
    }

    void ExitEffect(Rigidbody rb)
    {
        if (rb == null) return;

        if (maintainWeightlessOnExit)
        {
            // 開啟時：保持無重力，物件會沿慣性飄走
            rb.useGravity = false;
        }
        else
        {
            // 預設（關閉）：恢復 Unity 原始重力
            rb.useGravity = true;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = maintainWeightlessOnExit ? Color.cyan : Color.yellow;
        if (type == GravityType.Spherical)
            Gizmos.DrawWireSphere(transform.position, range);
        else
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(range, 0.1f, range));
        }
    }
}