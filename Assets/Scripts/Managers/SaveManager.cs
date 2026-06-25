using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public static class SaveManager
{
    private const string SaveFileName = "save.dat";
    private const string EncryptionKey = "BiTile.Save.File.v1";
    private static readonly byte[] EncryptionIv =
    {
        0x42, 0x69, 0x54, 0x69, 0x6C, 0x65, 0x53, 0x61,
        0x76, 0x65, 0x46, 0x69, 0x6C, 0x65, 0x30, 0x31
    };

    private static SaveData data;

    private static string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);

    public static int LastUnlockedStage
    {
        get => Data.lastUnlockedStage;
        set
        {
            Data.lastUnlockedStage = value;
            Save();
        }
    }

    public static int TileSkinIndex
    {
        get => Data.tileSkinIndex;
        set
        {
            Data.tileSkinIndex = value;
            Save();
        }
    }

    public static void Reset()
    {
        data = new SaveData();
        Save();
    }

    public static void CompleteAllStages()
    {
        data = new SaveData
        {
            lastUnlockedStage = PuzzleStageRepository.TotalStageCount
        };
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
        Directory.CreateDirectory(Application.persistentDataPath);
        var json = JsonUtility.ToJson(data);
        var encryptedText = Encrypt(json);
        File.WriteAllText(SavePath, encryptedText);
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
