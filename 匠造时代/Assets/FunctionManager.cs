using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;






public class FunctionManager : MonoBehaviour
{
    public static FunctionManager instance; // ����ģʽ

    public Camera mainCamera; // �������������Ⱦ3D������
    public Camera uiCamera;   // UIר�����

    public Version gameVersion = new(0, 0, 0, 1); // ��Ϸ�汾��
    // ת���ַ�����gameVersiion.ToString()
    // ��������أ�av <= bv   av == bv

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // ���ֵ����ڳ����л�ʱ��������
        }
        else
        {
            Debug.LogWarning("FunctionManager ʵ���Ѵ��ڣ������ظ�ʵ����");
            Destroy(gameObject);
        }
    }


    public void ShowUIOnly()
    {
        // ���ô˷����л���ֻ��ʾUI

        // �����������Ĭ�ϲ����Ⱦ������3D������Default�㣩
        mainCamera.cullingMask = 0; // 0��ʾ����Ⱦ�κβ�

        // ȷ��UI������ò����ú��ʵ�Depth
        uiCamera.gameObject.SetActive(true);
        uiCamera.depth = mainCamera.depth + 1; // ȷ��UI�������Ⱦ
    }
    
    public void Show3DScene()
    {
        // ���ô˷����ָ�3D��Ⱦ

        // �ָ����������Ⱦͼ�㣨������ȾDefault���UI�㣩
        mainCamera.cullingMask = (1 << LayerMask.NameToLayer("Default")) | (1 << LayerMask.NameToLayer("UI"));
        uiCamera.depth = mainCamera.depth - 1;
    }

    public void DeleteAllUI()
    {
        // ɾ������UI���
        Canvas[] canvases = FindObjectsOfType<Canvas>();
        foreach (Canvas canvas in canvases)
        {
            Destroy(canvas.gameObject);
        }
    }

    public void LockMouse(bool status = true)
    {
        // ����(true)/ȡ������(false) ���
        Cursor.lockState = status ? CursorLockMode.Locked : CursorLockMode.Confined;
        Cursor.visible = !status;
    }

    public int GetRandomInt(System.Random random = null)
    {
        // ��ȡ��� Int ����
        random ??= new System.Random();
        return random.Next(2) == 0 ? random.Next(int.MinValue, 0) : random.Next(0, int.MaxValue);
    }
}