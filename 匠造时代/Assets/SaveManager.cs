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
    public static SaveManager instance; // 单例模式

    private string savePath;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // 保持单例在场景切换时不被销毁
        }
        else
        {
            Debug.LogWarning("SaveManager实例已存在，销毁重复实例。");
            Destroy(gameObject);
        }
    }



    void Start() { }

    public void SaveGame(PlayerData data)
    {
        string json = JsonUtility.ToJson(data, true); // true表示格式化（易读）
        if (savePath == null)
        {
            savePath = Path.Combine(Application.persistentDataPath, "playerData.json");
        }
        File.WriteAllText(savePath, json);
        Debug.Log($"玩家数据已保存至: {savePath}");
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

    // 以下是存档相关的工具方法：

    // 返回存档根目录（persistentDataPath/saves）
    public string GetSavesRoot()
    {
        return Path.Combine(Application.persistentDataPath, "saves");
    }

    // 返回指定存档的目录（persistentDataPath/saves/{saveName}）
    public string GetSaveDirectory(string saveName)
    {
        return Path.Combine(GetSavesRoot(), saveName);
    }

    // 返回指定存档的icon路径（默认文件名 icon.png）
    public string GetSaveIconPath(string saveName)
    {
        return Path.Combine(GetSaveDirectory(saveName), "icon.png");
    }

    // 从磁盘加载Texture2D（支持PNG/JPG）
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
            Debug.LogWarning($"加载图片失败: {path} , {ex.Message}");
            return null;
        }
    }

    // 便捷方法：根据存档名加载其icon
    public Texture2D LoadSaveIcon(string saveName)
    {
        string iconPath = GetSaveIconPath(saveName);
        return LoadTextureFromFile(iconPath);
    }
}