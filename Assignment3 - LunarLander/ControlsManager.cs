using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Runtime.Serialization.Json;
using Microsoft.Xna.Framework.Input;

namespace CS5410
{
    public static class ControlsManager
    {
        public static Dictionary<string, Keys> Controls { get; private set; } = new Dictionary<string, Keys>();

        static ControlsManager()
        {
            LoadControls();
        }

        private static void LoadControls()
        {
            using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (storage.FileExists("Controls.json"))
                {
                    using (IsolatedStorageFileStream fs = storage.OpenFile("Controls.json", FileMode.Open))
                    {
                        try
                        {
                            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(Dictionary<string, Keys>));
                            Controls = (Dictionary<string, Keys>)serializer.ReadObject(fs);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"Error loading controls: {e.Message}");
                        }
                    }
                }
                else
                {
                    SetDefaultControls();
                }
            }
        }

        public static void SaveControls()
        {
            using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (IsolatedStorageFileStream fs = storage.CreateFile("Controls.json"))
                {
                    DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(Dictionary<string, Keys>));
                    serializer.WriteObject(fs, Controls);
                }
            }
        }

        private static void SetDefaultControls()
        {
            Controls["Thrust"] = Keys.Up;
            Controls["RotateLeft"] = Keys.Left;
            Controls["RotateRight"] = Keys.Right;
            SaveControls();
        }

        public static void SetControl(string action, Keys key)
        {
            if (Controls.ContainsKey(action))
            {
                Controls[action] = key;
            }
            else
            {
                Controls.Add(action, key);
            }
            SaveControls();
        }
    }
}
