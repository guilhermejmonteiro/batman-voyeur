using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

public static class GifResources
{
    private static Random _random = new Random();

    // Method to add a GIF file path and its corresponding file identifier
    public static void AddGif(string gifFilePath, string fileId, string jsonFilePath)
    {
        Dictionary<string, string> gifs = LoadGifs(jsonFilePath);
        gifs.Add(gifFilePath, fileId);
        UpdateJsonFile(gifs, jsonFilePath);
    }

    // Method to get a random GIF file path
    public static string GetRandomGif(string jsonFilePath)
    {
        Dictionary<string, string> gifs = LoadGifs(jsonFilePath);

        if (gifs.Count == 0)
        {
            throw new InvalidOperationException("tem menos gif que agua no nordeste. bota mais ae");
        }

        int randomIndex = _random.Next(gifs.Count);
        return gifs.Keys.ElementAt(randomIndex);
    }

    // Method to get the file identifier associated with a GIF file path
    public static string GetFileId(string gifFilePath, string jsonFilePath)
    {
        Dictionary<string, string> gifs = LoadGifs(jsonFilePath);
        return gifs.ContainsKey(gifFilePath) ? gifs[gifFilePath] : null;
    }

    private static Dictionary<string, string> LoadGifs(string jsonFilePath)
    {
        if (File.Exists(jsonFilePath))
        {
            string json = File.ReadAllText(jsonFilePath);
            return JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
        }
        else
        {
            return new Dictionary<string, string>();
        }
    }

    private static void UpdateJsonFile(Dictionary<string, string> gifs, string jsonFilePath)
    {
        string json = JsonConvert.SerializeObject(gifs, Formatting.Indented);
        File.WriteAllText(jsonFilePath, json);
    }
}
