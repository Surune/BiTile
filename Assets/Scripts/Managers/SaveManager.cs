using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public static class SaveManager
{
    private const string SaveFileName = "save.sav";
    private const string SaveDirectoryName = "SavesDir";
    private const string EncryptionKey = "BiTile";
    private static readonly byte[] EncryptionIv =
    {
        0x42, 0x69, 0x54, 0x69, 0x6C, 0x65, 0x53, 0x61,
        0x76, 0x65, 0x46, 0x69, 0x6C, 0x65, 0x30, 0x31
    };

    private static SaveData data;

    private static string SaveDir => Path.GetFullPath(Path.Combine(Application.dataPath, "..", SaveDirectoryName));
    private static string SavePath => Path.Combine(SaveDir, SaveFileName);

    public static int GetClearedStageCount(Definitions.GameMode mode, int chapterId)
    {
        var index = GetChapterProgressIndex(mode, chapterId);
        return index < 0 ? 0 : Data.chapterProgresses[index].clearedStageCount;
    }

    public static void SetClearedStageCount(Definitions.GameMode mode, int chapterId, int clearedStageCount)
    {
        var index = GetChapterProgressIndex(mode, chapterId);
        if (index < 0)
        {
            AddChapterProgress(mode, chapterId).clearedStageCount = clearedStageCount;
        }
        else
        {
            Data.chapterProgresses[index].clearedStageCount = clearedStageCount;
        }

        Save();
    }

    public static bool IsChapterCleared(Definitions.GameMode mode, int chapterId, int stageCount)
    {
        return GetClearedStageCount(mode, chapterId) == stageCount;
    }

    public static bool IsHardModeUnlocked()
    {
        return Data.hardModeUnlocked;
    }

    public static void UnlockHardMode()
    {
        Data.hardModeUnlocked = true;
        Save();
    }

    public static bool HasStar(Definitions.GameMode mode, int chapterId, int stage)
    {
        var index = GetChapterProgressIndex(mode, chapterId);
        return index >= 0 && Data.chapterProgresses[index].starredStages.Contains(stage);
    }

    public static bool UnlockStar(Definitions.GameMode mode, int chapterId, int stage)
    {
        var index = GetChapterProgressIndex(mode, chapterId);
        if (index < 0)
        {
            AddChapterProgress(mode, chapterId).starredStages.Add(stage);
        }
        else
        {
            var starredStages = Data.chapterProgresses[index].starredStages;
            if (starredStages.Contains(stage))
            {
                return false;
            }

            starredStages.Add(stage);
        }

        Save();
        return true;
    }

    public static void Reset()
    {
        data = new SaveData();
        Save();
    }

    private static SaveData Data
    {
        get
        {
            data ??= Load();
            return data;
        }
    }

    private static SaveData Load()
    {
        if (File.Exists(SavePath))
        {
            var encryptedText = File.ReadAllText(SavePath);
            var json = Decrypt(encryptedText);
            return JsonUtility.FromJson<SaveData>(json);
        }

        data = new SaveData();
        Save();
        return data;
    }

    private static void Save()
    {
        Directory.CreateDirectory(SaveDir);
        var json = JsonUtility.ToJson(data);
        var encryptedText = Encrypt(json);
        File.WriteAllText(SavePath, encryptedText);
    }

    private static ChapterProgressData AddChapterProgress(Definitions.GameMode mode, int chapterId)
    {
        var chapterProgress = new ChapterProgressData
        {
            mode = mode,
            chapterId = chapterId
        };
        Data.chapterProgresses.Add(chapterProgress);
        return chapterProgress;
    }

    private static int GetChapterProgressIndex(Definitions.GameMode mode, int chapterId)
    {
        for (var i = 0; i < Data.chapterProgresses.Count; i++)
        {
            var chapterProgress = Data.chapterProgresses[i];
            if (chapterProgress.mode == mode && chapterProgress.chapterId == chapterId)
            {
                return i;
            }
        }

        return -1;
    }

    private static string Encrypt(string plainText)
    {
        using var aes = Aes.Create();
        aes.Key = CreateKey();
        aes.IV = EncryptionIv;

        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        using var memoryStream = new MemoryStream();
        using var cryptoStream = new CryptoStream(memoryStream, aes.CreateEncryptor(), CryptoStreamMode.Write);
        cryptoStream.Write(plainBytes, 0, plainBytes.Length);
        cryptoStream.FlushFinalBlock();

        return Convert.ToBase64String(memoryStream.ToArray());
    }

    private static string Decrypt(string encryptedText)
    {
        using var aes = Aes.Create();
        aes.Key = CreateKey();
        aes.IV = EncryptionIv;

        var encryptedBytes = Convert.FromBase64String(encryptedText);
        using var memoryStream = new MemoryStream(encryptedBytes);
        using var cryptoStream = new CryptoStream(memoryStream, aes.CreateDecryptor(), CryptoStreamMode.Read);
        using var reader = new StreamReader(cryptoStream, Encoding.UTF8);
        return reader.ReadToEnd();
    }

    private static byte[] CreateKey()
    {
        using var sha256 = SHA256.Create();
        return sha256.ComputeHash(Encoding.UTF8.GetBytes(EncryptionKey));
    }
}
