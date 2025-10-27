using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class PlayerData
{
    public int level;
    public string playerName;
}

public class SaveManager : MonoBehaviour
{
    public static SaveManager instance; // ����ģʽ

    private string savePath;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // ���ֵ����ڳ����л�ʱ��������
        }
        else
        {
            Debug.LogWarning("SaveManagerʵ���Ѵ��ڣ������ظ�ʵ����");
            Destroy(gameObject);
        }
    }



    void Start() { }

    public void SaveGame(PlayerData data)
    {
        string json = JsonUtility.ToJson(data, true); // true��ʾ��ʽ�����׶���
        if (savePath == null)
        {
            savePath = Path.Combine(Application.persistentDataPath, "playerData.json");
        }
        File.WriteAllText(savePath, json);
        Debug.Log($"��������ѱ�����: {savePath}");
    }

    public PlayerData LoadGame()
    {
        if (savePath == null)
        {
            savePath = Path.Combine(Application.persistentDataPath, "playerData.json");
        }

        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            return JsonUtility.FromJson<PlayerData>(json);
        }
        return null;
    }

    // �����Ǵ浵��صĹ��߷�����

    // ���ش浵��Ŀ¼��persistentDataPath/saves��
    public string GetSavesRoot()
    {
        return Path.Combine(Application.persistentDataPath, "saves");
    }

    // ����ָ���浵��Ŀ¼��persistentDataPath/saves/{saveName}��
    public string GetSaveDirectory(string saveName)
    {
        return Path.Combine(GetSavesRoot(), saveName);
    }

    // ����ָ���浵��icon·����Ĭ���ļ��� icon.png��
    public string GetSaveIconPath(string saveName)
    {
        return Path.Combine(GetSaveDirectory(saveName), "icon.png");
    }

    // �Ӵ��̼���Texture2D��֧��PNG/JPG��
    public Texture2D LoadTextureFromFile(string path)
    {
        if (string.IsNullOrEmpty(path) || !File.Exists(path)) return null;
        try
        {
            byte[] bytes = File.ReadAllBytes(path);
            Texture2D tex = new Texture2D(2, 2);
            if (tex.LoadImage(bytes))
            {
                return tex;
            }
            else
            {
                Destroy(tex);
                return null;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"����ͼƬʧ��: {path} , {ex.Message}");
            return null;
        }
    }

    // ��ݷ��������ݴ浵��������icon
    public Texture2D LoadSaveIcon(string saveName)
    {
        string iconPath = GetSaveIconPath(saveName);
        return LoadTextureFromFile(iconPath);
    }
}