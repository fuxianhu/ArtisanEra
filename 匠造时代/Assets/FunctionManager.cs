using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;






public class FunctionManager : MonoBehaviour
{
    public static FunctionManager instance; // 单例模式

    public Camera mainCamera; // 主相机（用于渲染3D场景）
    public Camera uiCamera;   // UI专用相机

    public Version gameVersion = new(0, 0, 0, 1); // 游戏版本号
    // 转换字符串：gameVersiion.ToString()
    // 运算符重载：av <= bv   av == bv

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // 保持单例在场景切换时不被销毁
        }
        else
        {
            Debug.LogWarning("FunctionManager 实例已存在，销毁重复实例。");
            Destroy(gameObject);
        }
    }


    public void ShowUIOnly()
    {
        // 调用此方法切换到只显示UI

        // 禁用主相机对默认层的渲染（假设3D物体在Default层）
        mainCamera.cullingMask = 0; // 0表示不渲染任何层

        // 确保UI相机启用并设置合适的Depth
        uiCamera.gameObject.SetActive(true);
        uiCamera.depth = mainCamera.depth + 1; // 确保UI相机后渲染
    }
    
    public void Show3DScene()
    {
        // 调用此方法恢复3D渲染

        // 恢复主相机的渲染图层（例如渲染Default层和UI层）
        mainCamera.cullingMask = (1 << LayerMask.NameToLayer("Default")) | (1 << LayerMask.NameToLayer("UI"));
        uiCamera.depth = mainCamera.depth - 1;
    }

    public void DeleteAllUI()
    {
        // 删除所有UI组件
        Canvas[] canvases = FindObjectsOfType<Canvas>();
        foreach (Canvas canvas in canvases)
        {
            Destroy(canvas.gameObject);
        }
    }

    public void LockMouse(bool status = true)
    {
        // 锁定(true)/取消锁定(false) 鼠标
        Cursor.lockState = status ? CursorLockMode.Locked : CursorLockMode.Confined;
        Cursor.visible = !status;
    }

    public int GetRandomInt(System.Random random = null)
    {
        // 获取随机 Int 整数
        random ??= new System.Random();
        return random.Next(2) == 0 ? random.Next(int.MinValue, 0) : random.Next(0, int.MaxValue);
    }
}