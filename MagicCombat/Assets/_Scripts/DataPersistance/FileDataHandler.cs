using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class FileDataHandler
{
    string dataDirPath = "";
    string dataFileName = "";

    bool useEncryption = false;

    readonly string codeWord = "potblackchilli";
    public FileDataHandler(string dataDirPath, string dataFileName, bool useEncryption)
    {
        this.dataDirPath = dataDirPath;
        this.dataFileName = dataFileName;
        this.useEncryption = useEncryption;
    }

    public GameData Load()
    {
        // using Path.Combine to account for different OS's having different path seperators
        string fullPath = Path.Combine(dataDirPath, dataFileName);

        GameData loadedData = null;

        if (File.Exists(fullPath))
        {
            try
            {
                // Load serialised data from file
                string dataToLoad = "";
                using (FileStream stream = new FileStream(fullPath, FileMode.Open)) // Using FileMode.Open to read from the file
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        dataToLoad = reader.ReadToEnd();
                    }
                }

                // If we are decrypting the data
                if (useEncryption)
                {
                    dataToLoad = EncryptDecrypt(dataToLoad);
                }


                // Deserialise the data from Json back into C# object
                loadedData = JsonUtility.FromJson<GameData>(dataToLoad);

            }
            catch (Exception e)
            {
                Debug.LogError("Error occoured when trying to load data from file: " + fullPath + "/n" + e);
            }
        }
        return loadedData;
    }

    public void Save(GameData data)
    {
        // using Path.Combine to account for different OS's having different path seperators
        string fullPath = Path.Combine(dataDirPath, dataFileName);
        // Never really used try catch blocks before
        try
        {
            // Create directory path of the file to be written to if it doesn't already exist
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

            // Serialise C# game data object into Json, true peram to make it readable
            string dataToStore = JsonUtility.ToJson(data, true);

            // If we are encrypting the data
            if (useEncryption)
            {
                dataToStore = EncryptDecrypt(dataToStore);
            }

            // Write serialised data to the file
            using (FileStream stream = new FileStream(fullPath, FileMode.Create)) // Using a 'Using' block to ensure connection to the file is closed once done reading or writing to it
            {
                using(StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write(dataToStore);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error occoured when trying to save data to file: " + fullPath + "/n" + e);
        }
    }
    string EncryptDecrypt(string data)
    {
        string modifiedData = "";

        for(int i = 0; i < data.Length; i++)
        {
            modifiedData += (char)(data[i] ^ codeWord[i % codeWord.Length]); // Using XOR operator to decrypt/encrypt the data using the codeword
        }
        return modifiedData;
    } 
}
