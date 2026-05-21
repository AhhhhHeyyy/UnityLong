using System.Collections.Generic;
using UnityEngine;

public class JoyconIK : MonoBehaviour {
	
    // Joycon必要變數
	private List<Joycon> joycons;
    public float[] stick;
    public Vector3 gyro;
    public Vector3 accel;
    public int jc_ind = 0; // 0左 1右
    public Quaternion orientation;

    public Joycon j;
    // ===== IK控制 =====
    [Header("IK Target")]
    public Transform handTarget; // 右手或左手Target
    public Transform bodyRoot; // 角色身體中心(胸口/頭/角色根物件)

    [Header("位置設定")]
    private bool isLeftHand = false;
    public float sideOffset = 0.25f;
    public float forwardOffset = 0f;
    public float heightOffset = 0f;

    [Header("加速度影響")]
    public float accelMovePower = 0.15f;

    [Header("平滑")]
    public float moveSmooth = 50f;
    public float rotSmooth = 50f;

    [Header("旋轉修正")]
    public Vector3 rotationOffset = Vector3.zero;

    void Start ()
    {
        gyro = new Vector3(0, 0, 0);
        accel = new Vector3(0, 0, 0);

        // get the public Joycon array attached to the JoyconManager in scene
        joycons = JoyconManager.Instance.j;
		if (joycons.Count < jc_ind+1)
        {
			Destroy(gameObject);
		}

        if (jc_ind == 0)
        { 
            isLeftHand = true;
        }

    }

    void Update ()
    {
		if (joycons.Count > 0)
        {
			j = joycons [jc_ind];

            // 重設位置SL
            if (j.GetButtonDown(Joycon.Button.SL))
            {
				j.Recenter ();
			}
            // 重設位置SR
            if (j.GetButtonDown(Joycon.Button.SR))
            {
                j.Recenter();
            }

            stick = j.GetStick();
            gyro = j.GetGyro();
            accel = j.GetAccel();

            orientation = j.GetVector();
            gameObject.transform.rotation = orientation;

            // IK控制
            if (handTarget != null)
            {
                UpdateIKTarget();
            }
        }
    }

    void UpdateIKTarget()
    {
        Quaternion joyRot = bodyRoot.rotation * orientation;

        // 左手鏡像修正
        if (isLeftHand)
        {
            joyRot = new Quaternion(
                joyRot.x,
                joyRot.y,
                joyRot.z,
                joyRot.w
            );
        }

        // 額外角度修正
        joyRot *= Quaternion.Euler(rotationOffset);

        // 位置基準點
        Vector3 basePos =
            bodyRoot.position +
            bodyRoot.right * (isLeftHand ? -sideOffset : sideOffset) +
            bodyRoot.forward * forwardOffset +
            bodyRoot.up * heightOffset;

        // Joy-Con朝向（讓手的位置跟著抬手方向改變）
        Vector3 handDir = joyRot * Vector3.forward;

        // 抬手 / 指向位置偏移（會讓手肘開始彎曲）
        Vector3 aimOffset = handDir * 0.35f;

        // 加速度讓手往前伸 / 揮動感
        Vector3 accelOffset =
            bodyRoot.forward * accel.magnitude * accelMovePower;

        // 最終位置
        Vector3 targetPos = basePos + aimOffset + accelOffset;

        // 平滑移動
        handTarget.position = Vector3.Lerp(
            handTarget.position,
            targetPos,
            Time.deltaTime * moveSmooth
        );

        // 平滑旋轉
        handTarget.rotation = Quaternion.Slerp(
            handTarget.rotation,
            joyRot,
            Time.deltaTime * rotSmooth
        );
    }
}