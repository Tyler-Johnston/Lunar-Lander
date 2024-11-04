using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Runtime.Serialization.Json;

namespace CS5410.HighScores
{
    public static class HighScoreManager
    {
        public static List<HighScore> HighScores { get; private set; } = new List<HighScore>();

        static HighScoreManager()
        {
            LoadHighScores();
        }

        private static void LoadHighScores()
        {
            using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (storage.FileExists("HighScores.json"))
                {
                    using (IsolatedStorageFileStream fs = storage.OpenFile("HighScores.json", FileMode.Open))
                    {
                        if (fs.Length > 0)
                        {
                            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(List<HighScore>));
                            HighScores = (List<HighScore>)serializer.ReadObject(fs);
                        }
                    }
                }
            }
        }

        public static void SaveHighScores()
        {
            using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (IsolatedStorageFileStream fs = storage.CreateFile("HighScores.json"))
                {
                    DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(List<HighScore>));
                    serializer.WriteObject(fs, HighScores);
                }
            }
        }

        public static void AddHighScore(HighScore highScore)
        {
            HighScores.Add(highScore);
            HighScores.Sort((hs1, hs2) => hs2.Score.CompareTo(hs1.Score));
            if (HighScores.Count > 10)
            {
                HighScores.RemoveRange(10, HighScores.Count - 10);
            }
            SaveHighScores();
        }
    }
}
