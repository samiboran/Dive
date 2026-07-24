using System;
using System.IO;
using UnityEngine;

/// <summary>
/// Jenerik JSON save/load — Application.persistentDataPath altına yazar.
/// Tasarım değeri olmayan altyapı işi; stash dışında ileride ayarlar/istatistik
/// için de kullanılabilir.
/// </summary>
public static class SaveSystem
{
    private static string GetPath(string fileName) =>
        Path.Combine(Application.persistentDataPath, fileName);

    public static void Save<T>(string fileName, T data)
    {
        try
        {
            string json = JsonUtility.ToJson(data, prettyPrint: true);
            File.WriteAllText(GetPath(fileName), json);
            Debug.Log($"[SaveSystem] Kaydedildi: {GetPath(fileName)}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveSystem] Kayıt hatası: {e.Message}");
        }
    }

    public static T Load<T>(string fileName) where T : new()
    {
        try
        {
            string path = GetPath(fileName);
            if (!File.Exists(path)) return new T();

            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<T>(json) ?? new T();
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveSystem] Yükleme hatası: {e.Message}");
            return new T();
        }
    }

    public static bool Exists(string fileName) => File.Exists(GetPath(fileName));

    public static void Delete(string fileName)
    {
        string path = GetPath(fileName);
        if (File.Exists(path)) File.Delete(path);
    }
}
