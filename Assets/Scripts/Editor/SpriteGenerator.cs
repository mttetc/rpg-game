using UnityEngine;
using UnityEditor;
using System.IO;

public class SpriteGenerator
{
    public static void GenerateSprites()
    {
        // Ensure directories exist
        CreateDirectoryIfNotExists("Assets/Sprites");

        // Generate sprites if they don't exist
        if (!File.Exists("Assets/Sprites/PlayerSprite.png"))
        {
            CreateSprite("PlayerSprite", Color.blue, new Vector2(32, 32));
        }

        if (!File.Exists("Assets/Sprites/EnemySprite.png"))
        {
            CreateSprite("EnemySprite", Color.red, new Vector2(32, 32));
        }

        if (!File.Exists("Assets/Sprites/GroundSprite.png"))
        {
            CreateGroundSprite();
        }

        AssetDatabase.Refresh();
    }

    private static void CreateGroundSprite()
    {
        int width = 32;
        int height = 32;
        Texture2D texture = new Texture2D(width, height);

        // Create a grass-like pattern
        Color grassColor = new Color(0.4f, 0.8f, 0.3f);
        Color darkGrassColor = new Color(0.3f, 0.7f, 0.2f);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Create a subtle pattern
                if ((x + y) % 4 == 0)
                {
                    texture.SetPixel(x, y, darkGrassColor);
                }
                else
                {
                    texture.SetPixel(x, y, grassColor);
                }
            }
        }

        texture.Apply();

        // Save texture as PNG
        byte[] bytes = texture.EncodeToPNG();
        string path = "Assets/Sprites/GroundSprite.png";
        File.WriteAllBytes(path, bytes);

        // Set texture import settings
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = 32;
            importer.filterMode = FilterMode.Point;
            importer.wrapMode = TextureWrapMode.Repeat;
            importer.SaveAndReimport();
        }
    }

    private static void CreateDirectoryIfNotExists(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            string parentPath = Path.GetDirectoryName(path);
            string folderName = Path.GetFileName(path);
            AssetDatabase.CreateFolder(parentPath, folderName);
        }
    }

    private static void CreateSprite(string name, Color color, Vector2 size)
    {
        // Create texture
        int width = (int)size.x;
        int height = (int)size.y;
        Texture2D texture = new Texture2D(width, height);

        // Fill texture with color
        Color[] pixels = new Color[width * height];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = color;
        }
        texture.SetPixels(pixels);
        texture.Apply();

        // Save texture as PNG
        byte[] bytes = texture.EncodeToPNG();
        string path = $"Assets/Sprites/{name}.png";
        File.WriteAllBytes(path, bytes);

        // Set texture import settings
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = 32;
            importer.filterMode = FilterMode.Point;
            importer.SaveAndReimport();
        }
    }
} 