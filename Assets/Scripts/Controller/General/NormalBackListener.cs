using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalBackListener : MonoBehaviour
{
    void Update()
    {
        //监听返回按钮，所有的场景都需要添加该脚本
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            BackHandler._instance.PopScene();
        }
    }

    public void OnClickBackBtn()
    {
        BackHandler._instance.PopScene();
    }
}