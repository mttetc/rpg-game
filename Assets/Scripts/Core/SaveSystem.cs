using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Player;

namespace Core
{
    public class SaveSystem : MonoBehaviour
    {
        private static string savePath => Path.Combine(Application.persistentDataPath, "save.dat");

        public static void SaveGame()
        {
            try
            {
                BinaryFormatter formatter = new BinaryFormatter();
                using (FileStream stream = new FileStream(savePath, FileMode.Create))
                {
                    var player = GameObject.FindGameObjectWithTag("Player");
                    if (player != null)
                    {
                        var playerHealth = player.GetComponent<PlayerHealth>();
                        // Save game data here
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to save game: {e.Message}");
            }
        }

        public static void LoadGame()
        {
            if (File.Exists(savePath))
            {
                try
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    using (FileStream stream = new FileStream(savePath, FileMode.Open))
                    {
                        // Load game data here
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Failed to load game: {e.Message}");
                }
            }
        }
    }
} 