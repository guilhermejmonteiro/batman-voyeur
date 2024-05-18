using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace BatBot
{
    public static class GifResources
    {
        private static Random _random = new Random();

        public static void AddGif(string gifFileId, string jsonFilePath)
        {
            List<string> gifIds = LoadGifIds(jsonFilePath);
            gifIds.Add(gifFileId);
            UpdateJsonFile(gifIds, jsonFilePath);
        }

        public static string GetRandomGif(string jsonFilePath)
        {
            List<string> gifIds = LoadGifIds(jsonFilePath);

            if (gifIds.Count == 0)
            {
               return "nada";
            }
            else
            {
                int randomIndex = _random.Next(gifIds.Count);
                return gifIds[randomIndex];
            }
        }

        private static List<string> LoadGifIds(string jsonFilePath)
        {
            if (File.Exists(jsonFilePath))
            {
                string json = File.ReadAllText(jsonFilePath);
                return JsonConvert.DeserializeObject<List<string>>(json);
            }
            else
            {
                return new List<string>();
            }
        }

        private static void UpdateJsonFile(List<string> gifIds, string jsonFilePath)
        {
            string json = JsonConvert.SerializeObject(gifIds, Formatting.Indented);
            File.WriteAllText(jsonFilePath, json);
        }
    }
}
