using UnityEngine;

public class ObjectTransformer : MonoBehaviour
{
    [Header("--- 共通設定 ---")]
    [Range(5f, 30f)] public float duration = 10f; // 運作總時間 (已改為 5~30 秒)
    public AnimationCurve easeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // 緩動曲線
    public bool pingPong = true; // 是否來回反轉
    public bool repeat = true;   // 是否重複循環

    [Header("--- 旋轉設定 ---")]
    public bool enableRotation = true;
    public Vector3 rotationAxis = Vector3.up; // 旋轉軸向
    public float totalRotationAngle = 360f;   // 總時間內旋轉的角度

    [Header("--- 位移設定 ---")]
    public bool enableMovement = true;
    public Vector3 startPosition;
    public Vector3 endPosition;
    [Min(1)] public int movementCycles = 1; // 總時間內要完成的循環次數

    private float timer = 0f;
    private bool forward = true; // 用於控制 Repeat 模式下的 PingPong 方向

    void Update()
    {
        UpdateTimer();
        
        // 1. 計算總時間的標準進度 (0 到 1)
        float totalProgress = timer / duration;
        
        // 處理 Repeat 模式下的 PingPong 翻轉（影響旋轉與位移的大方向）
        if (!forward)
        {
            totalProgress = 1f - totalProgress;
        }

        // 2. 執行旋轉 (旋轉依然跟隨總時間曲線)
        if (enableRotation)
        {
            float rotationT = easeCurve.Evaluate(totalProgress);
            ApplyRotation(rotationT);
        }

        // 3. 執行位移 (以總時間為基礎，切分出 movementCycles 個子循環)
        if (enableMovement)
        {
            ApplyMovement(totalProgress);
        }
    }

    private void UpdateTimer()
    {
        timer += Time.deltaTime;

        if (timer >= duration)
        {
            if (repeat)
            {
                timer = 0f;
                if (pingPong) forward = !forward; // 大循環的來回切換
            }
            else
            {
                timer = duration; // 停止在終點
            }
        }
    }

    private void ApplyRotation(float t)
    {
        float currentAngle = t * totalRotationAngle;
        transform.localRotation = Quaternion.AngleAxis(currentAngle, rotationAxis);
    }

    private void ApplyMovement(float totalProgress)
    {
        // 將總進度 (0~1) 乘以次數，例如 3 次就會變成 0.0 ~ 3.0
        float scaledProgress = totalProgress * movementCycles;
        
        // 取小數點部分，得到當前小週期的進度 (0.0 ~ 1.0)
        float cycleProgress = scaledProgress % 1.0f;
        
        // 邊界修正：當剛好到達 1.0 且總進度結束時，避免取餘數變成 0
        if (cycleProgress == 0f && totalProgress >= 1f)
        {
            cycleProgress = 1f;
        }

        float moveT = 0f;

        if (pingPong)
        {
            // 如果勾選 pingPong，一個 Cycle 包含來回
            // 0.0 ~ 0.5 代表去程 (乘以 2 變 0~1)；0.5 ~ 1.0 代表回程 (變 1~0)
            if (cycleProgress < 0.5f)
            {
                float localT = cycleProgress * 2f;
                moveT = easeCurve.Evaluate(localT); // 套用 Ease 曲線
            }
            else
            {
                float localT = (cycleProgress - 0.5f) * 2f;
                moveT = easeCurve.Evaluate(1f - localT); // 曲線反轉回來
            }
        }
        else
        {
            // 如果沒有勾選 pingPong，一個 Cycle 就是單向從 A 到 B，然後瞬間回到 A 重複
            moveT = easeCurve.Evaluate(cycleProgress);
        }

        // 根據計算出來的 moveT 進行平滑插值位移
        transform.localPosition = Vector3.Lerp(startPosition, endPosition, moveT);
    }

    // 在 Inspector 點擊右鍵組件可快速設定當前位置為起始點或終點
    [ContextMenu("Set Current Position as Start")]
    void SetStart() => startPosition = transform.localPosition;

    [ContextMenu("Set Current Position as End")]
    void SetEnd() => endPosition = transform.localPosition;
}