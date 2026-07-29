using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public static class SaveManager
{
    private const string SaveFileName = "save.sav";
    private const string SaveDirectoryName = "SavesDir";
    private const string EncryptionKey = "BiTile.Save.File.v1";
    private static readonly byte[] EncryptionIv =
    {
        0x42, 0x69, 0x54, 0x69, 0x6C, 0x65, 0x53, 0x61,
        0x76, 0x65, 0x46, 0x69, 0x6C, 0x65, 0x30, 0x31
    };

    private static SaveData data;

    private static string InstallDirectory => Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
    private static string SaveDirectory => Path.Combine(InstallDirectory, SaveDirectoryName);
    private static string SavePath => Path.Combine(SaveDirectory, SaveFileName);

    public static int GetLastUnlockedStage(Definitions.GameMode mode, int chapter)
    {
        return mode == Definitions.GameMode.Normal
            ? Data.lastUnlockedStage
            : GetHardClearedStageCount(chapter) + 1;
    }

    public static void SetLastUnlockedStage(Definitions.GameMode mode, int chapter, int stage)
    {
        if (mode == Definitions.GameMode.Normal)
        {
            Data.lastUnlockedStage = stage;
        }
        else
        {
            SetHardClearedStageCount(chapter, stage - 1);
        }

        Save();
    }

    public static bool IsHardModeUnlocked()
    {
        return Data.normalClearedChapterMask != 0;
    }

    public static bool IsNormalChapterCleared(int chapter)
    {
        return (Data.normalClearedChapterMask & 1 << (chapter - 1)) != 0;
    }

    public static void CompleteNormalChapter(int chapter)
    {
        Data.normalClearedChapterMask |= 1 << (chapter - 1);
        Save();
    }

    public static bool HasStar(Definitions.GameMode mode, int progressStage)
    {
        return GetStarredProgressStages(mode).Contains(progressStage);
    }

    public static bool UnlockStar(Definitions.GameMode mode, int progressStage)
    {
        var starredProgressStages = GetStarredProgressStages(mode);
        if (starredProgressStages.Contains(progressStage))
        {
            return false;
        }

        starredProgressStages.Add(progressStage);
        Save();
        return true;
    }

    public static bool IsModeCleared(Definitions.GameMode mode)
    {
        return mode == Definitions.GameMode.Normal
            ? Data.normalModeCleared
            : Data.hardModeCleared;
    }

    public static void CompleteMode(Definitions.GameMode mode)
    {
        if (mode == Definitions.GameMode.Normal)
        {
            Data.normalModeCleared = true;
        }
        else
        {
            Data.hardModeCleared = true;
        }

        Save();
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
            if (data == null)
            {
                data = Load();
            }

            return data;
        }
    }

    private static SaveData Load()
    {
        if (!File.Exists(SavePath))
        {
            return CreateDefaultSaveData();
        }

        var encryptedText = File.ReadAllText(SavePath);
        var json = Decrypt(encryptedText);
        return JsonUtility.FromJson<SaveData>(json);
    }

    private static SaveData CreateDefaultSaveData()
    {
        var saveData = new SaveData();
        data = saveData;
        Save();
        return saveData;
    }

    private static void Save()
    {
        Directory.CreateDirectory(SaveDirectory);
        var json = JsonUtility.ToJson(data);
        var encryptedText = Encrypt(json);
        File.WriteAllText(SavePath, encryptedText);
    }

    private static List<int> GetStarredProgressStages(Definitions.GameMode mode)
    {
        return mode == Definitions.GameMode.Normal
            ? Data.starredProgressStages
            : Data.hardStarredProgressStages;
    }

    private static int GetHardClearedStageCount(int chapter)
    {
        return chapter switch
        {
            1 => Data.hardChapter1ClearedStageCount,
            2 => Data.hardChapter2ClearedStageCount,
            3 => Data.hardChapter3ClearedStageCount,
            4 => Data.hardChapter4ClearedStageCount,
            5 => Data.hardChapter5ClearedStageCount,
            6 => Data.hardChapter6ClearedStageCount,
            7 => Data.hardChapter7ClearedStageCount,
            _ => throw new ArgumentOutOfRangeException(nameof(chapter), chapter, null)
        };
    }

    private static void SetHardClearedStageCount(int chapter, int clearedStageCount)
    {
        switch (chapter)
        {
            case 1:
                Data.hardChapter1ClearedStageCount = clearedStageCount;
                break;
            case 2:
                Data.hardChapter2ClearedStageCount = clearedStageCount;
                break;
            case 3:
                Data.hardChapter3ClearedStageCount = clearedStageCount;
                break;
            case 4:
                Data.hardChapter4ClearedStageCount = clearedStageCount;
                break;
            case 5:
                Data.hardChapter5ClearedStageCount = clearedStageCount;
                break;
            case 6:
                Data.hardChapter6ClearedStageCount = clearedStageCount;
                break;
            case 7:
                Data.hardChapter7ClearedStageCount = clearedStageCount;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(chapter), chapter, null);
        }
    }

    private static string Encrypt(string plainText)
    {
        using (var aes = Aes.Create())
        {
            aes.Key = CreateKey();
            aes.IV = EncryptionIv;

            var plainBytes = Encoding.UTF8.GetBytes(plainText);
            using (var memoryStream = new MemoryStream())
            using (var cryptoStream = new CryptoStream(memoryStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
            {
                cryptoStream.Write(plainBytes, 0, plainBytes.Length);
                cryptoStream.FlushFinalBlock();
                return Convert.ToBase64String(memoryStream.ToArray());
            }
        }
    }

    private static string Decrypt(string encryptedText)
    {
        using (var aes = Aes.Create())
        {
            aes.Key = CreateKey();
            aes.IV = EncryptionIv;

            var encryptedBytes = Convert.FromBase64String(encryptedText);
            using (var memoryStream = new MemoryStream(encryptedBytes))
            using (var cryptoStream = new CryptoStream(memoryStream, aes.CreateDecryptor(), CryptoStreamMode.Read))
            using (var reader = new StreamReader(cryptoStream, Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }
    }

    private static byte[] CreateKey()
    {
        using (var sha256 = SHA256.Create())
        {
            return sha256.ComputeHash(Encoding.UTF8.GetBytes(EncryptionKey));
        }
    }
}
