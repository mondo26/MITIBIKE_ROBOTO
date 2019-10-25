using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstPersonCameraController : CameraBase
{
    /// <summary>
    /// 更新処理 (cameraBase Override)
    /// </summary>
    public override void Update()
    {
        // BaseCameraのUpdate実行
        base.Update();
    }

    /// <summary>
    /// 指定フレーム更新処理 (cameraBase Override)
    /// </summary>
    public override void FixedUpdate()
    {
        // BaseCameraのFixedUpdate実行
        base.FixedUpdate();
    }
}
