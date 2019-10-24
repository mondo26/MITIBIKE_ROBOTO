using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/******************************************************************
 * * カメラベースクラス　(MonoBehaviourから継承)
 * ****************************************************************/
public class CameraBase : MonoBehaviour
{
    [SerializeField, Header("カメラが回転する速度")]
    protected float rotateSpeed;
    protected float MIN_ANGLE;                                         // Angleの最小値
    protected float MAX_ANGLE;                                         // Angleの最大値
    protected float yaw, pitch;                                        // 横縦の回転量を格納

    /// <summary>
    /// コンストラクタ
    /// </summary>
    public virtual void Awake()
    {
        this.MIN_ANGLE = 0;
        this.MAX_ANGLE = 0;
    }

    /// <summary>
    /// 毎フレーム更新処理
    /// </summary>
    public virtual void Update()
    {
        CameraInput();
    }

    /// <summary>
    /// 指定フレーム更新
    /// </summary>
    public virtual void FixedUpdate()
    {
        CameraRotate();
    }

    /// <summary>
    /// 入力処理
    /// </summary>
    public virtual void CameraInput()
    {
        this.yaw += Input.GetAxis("Horizontal_Camera") * Time.deltaTime * rotateSpeed;           // 横回転入力
        this.pitch += Input.GetAxis("Vertical_Camera") * Time.deltaTime * rotateSpeed;           // 縦回転入力
    }

    /// <summary>
    /// 回転処理
    /// </summary>
    public virtual void CameraRotate()
    {
        this.pitch = Mathf.Clamp(pitch, MIN_ANGLE, MAX_ANGLE);                       // 縦回転角度制限する
        transform.eulerAngles = new Vector3(-pitch, -yaw, 0.0f);                     // 回転の実行
    }
}