using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using Core;
using Player;
using Combat;
using CameraSystem;
using UI;

public class TestSceneSetup : EditorWindow
{
    [MenuItem("Tools/Setup Test Scene")]
    public static void SetupScene()
    {
        if (!EditorUtility.DisplayDialog("Setup Test Scene", 
            "This will create a new test scene. Any unsaved changes in the current scene will be lost. Continue?", 
            "Yes", "No"))
        {
            return;
        }

        try
        {
            // Generate required sprites first
            SpriteGenerator.GenerateSprites();

            // Create layers
            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty layers = tagManager.FindProperty("layers");
            layers.GetArrayElementAtIndex(8).stringValue = "Player";
            layers.GetArrayElementAtIndex(9).stringValue = "Enemy";
            layers.GetArrayElementAtIndex(10).stringValue = "Environment";
            tagManager.ApplyModifiedProperties();

            // Create scene
            var scene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.DefaultGameObjects);
            
            // Setup GameManager
            var gameManager = new GameObject("GameManager");
            gameManager.AddComponent<GameManager>();
            
            // Setup Player
            var player = new GameObject("Player");
            player.layer = LayerMask.NameToLayer("Player");
            player.tag = "Player";
            player.transform.position = new Vector3(0, 0, 0);
            
            var playerSprite = new GameObject("Sprite");
            playerSprite.transform.SetParent(player.transform);
            var spriteRenderer = playerSprite.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/PlayerSprite.png");
            spriteRenderer.sortingOrder = 1;
            
            var playerCollider = player.AddComponent<BoxCollider2D>();
            playerCollider.size = new Vector2(0.8f, 0.8f);
            playerCollider.isTrigger = false;
            
            var playerRb = player.AddComponent<Rigidbody2D>();
            playerRb.gravityScale = 0f;
            playerRb.constraints = RigidbodyConstraints2D.FreezeRotation;
            playerRb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            playerRb.interpolation = RigidbodyInterpolation2D.Interpolate;
            playerRb.linearDamping = 5f;
            
            player.AddComponent<PlayerController>();
            player.AddComponent<PlayerHealth>();

            // Create map layout
            CreateMapLayout();

            // Add enemies
            CreateEnemy(new Vector3(5, 5, 0), "Enemy1");
            CreateEnemy(new Vector3(-5, -5, 0), "Enemy2");

            // Setup Camera
            UnityEngine.Camera.main.gameObject.AddComponent<CameraFollow>();
            
            // Setup UI
            var canvas = new GameObject("Canvas");
            var canvasComponent = canvas.AddComponent<Canvas>();
            canvasComponent.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.AddComponent<CanvasScaler>();
            canvas.AddComponent<GraphicRaycaster>();
            
            // Add TextMeshPro essentials
            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            
            var uiManager = canvas.AddComponent<UIManager>();
            
            // Ensure the _Project/Scenes folder exists
            if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
                AssetDatabase.CreateFolder("Assets", "Scenes");

            // Save scene
            string scenePath = "Assets/Scenes/TestScene.unity";
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene, scenePath);
            
            Debug.Log("Test scene setup completed successfully!");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to setup test scene: {e.Message}");
            EditorUtility.DisplayDialog("Error", 
                "Failed to setup test scene. Check the console for details.", 
                "OK");
        }
    }

    // Add validation for the menu item
    [MenuItem("Tools/Setup Test Scene", true)]
    private static bool ValidateSetupScene()
    {
        // Always allow scene setup since we generate sprites if they don't exist
        return true;
    }

    private static void CreateWall(Vector3 position, Vector3 scale, string name, Color color)
    {
        var wall = new GameObject(name);
        wall.layer = LayerMask.NameToLayer("Environment");
        wall.transform.position = position;
        
        var wallSprite = new GameObject("Sprite");
        wallSprite.transform.SetParent(wall.transform);
        var wallRenderer = wallSprite.AddComponent<SpriteRenderer>();
        wallRenderer.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/PlayerSprite.png");
        wallRenderer.color = color;
        wallSprite.transform.localScale = scale;
        
        var wallCollider = wall.AddComponent<BoxCollider2D>();
        wallCollider.size = new Vector2(1, 1);
        
        wall.isStatic = true;
    }

    private static void CreateFloorTiles()
    {
        var floorParent = new GameObject("FloorTiles");
        floorParent.transform.position = Vector3.zero;
        
        for (int x = -14; x <= 14; x++)
        {
            for (int y = -9; y <= 9; y++)
            {
                var tile = new GameObject($"Tile_{x}_{y}");
                tile.transform.SetParent(floorParent.transform);
                tile.transform.position = new Vector3(x, y, 0);
                
                var tileSprite = tile.AddComponent<SpriteRenderer>();
                tileSprite.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/GroundSprite.png");
                tileSprite.color = new Color(
                    Random.Range(0.3f, 0.4f),
                    Random.Range(0.6f, 0.7f),
                    Random.Range(0.3f, 0.4f)
                );
                tileSprite.sortingOrder = -1;
            }
        }
    }

    private static void CreatePath(Vector3 position, Vector2 scale, Color color, string name, int sortingOrder = 0)
    {
        var path = new GameObject(name);
        path.transform.position = position;
        
        var pathSprite = path.AddComponent<SpriteRenderer>();
        pathSprite.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/GroundSprite.png");
        pathSprite.color = color;
        pathSprite.sortingOrder = sortingOrder;
        path.transform.localScale = new Vector3(scale.x, scale.y, 1);
    }

    private static void CreateDecoration(Vector3 position, Color color, string name, Vector2? scale = null, int sortingOrder = 0)
    {
        var decoration = new GameObject(name);
        decoration.layer = LayerMask.NameToLayer("Environment");
        decoration.transform.position = position;
        
        var sprite = decoration.AddComponent<SpriteRenderer>();
        sprite.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/PlayerSprite.png");
        sprite.color = color;
        sprite.sortingOrder = sortingOrder;
        
        if (scale.HasValue)
        {
            decoration.transform.localScale = new Vector3(scale.Value.x, scale.Value.y, 1);
        }
        
        var collider = decoration.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(1, 1);
        
        decoration.isStatic = true;
    }

    private static void CreateEnemy(Vector3 position, string name)
    {
        var enemy = new GameObject(name);
        enemy.layer = LayerMask.NameToLayer("Enemy");
        enemy.transform.position = position;
        
        var enemySprite = new GameObject("Sprite");
        enemySprite.transform.SetParent(enemy.transform);
        var enemySpriteRenderer = enemySprite.AddComponent<SpriteRenderer>();
        enemySpriteRenderer.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/EnemySprite.png");
        
        var rb = enemy.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.simulated = true;
        
        var collider = enemy.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = new Vector2(0.8f, 0.8f);
        
        enemy.AddComponent<Enemy>();
        
        Debug.Log($"Created enemy: {name} on layer: {enemy.layer}");
    }

    private static void CreateMapLayout()
    {
        // Create ground/floor
        var ground = new GameObject("Ground");
        var groundSprite = ground.AddComponent<SpriteRenderer>();
        groundSprite.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/GroundSprite.png");
        groundSprite.drawMode = SpriteDrawMode.Tiled;
        groundSprite.size = new Vector2(30, 20);
        groundSprite.sortingOrder = -2;
        ground.transform.position = Vector3.zero;

        // Create path with darker color
        CreatePath(Vector3.zero, new Vector2(4, 15), new Color(0.6f, 0.5f, 0.3f), "MainPath", -1);
        CreatePath(Vector3.zero, new Vector2(15, 4), new Color(0.6f, 0.5f, 0.3f), "CrossPath", -1);

        // Create trees with better visuals
        float[] treePositions = { -8, -4, 4, 8 };
        foreach (float x in treePositions)
        {
            foreach (float y in treePositions)
            {
                if (Mathf.Abs(x) + Mathf.Abs(y) > 8)
                {
                    CreateTree(new Vector3(x, y, 0));
                }
            }
        }

        CreateBorderWalls();
    }

    private static void CreateTree(Vector3 position)
    {
        var tree = new GameObject($"Tree_{position.x}_{position.y}");
        tree.transform.position = position;
        
        // Tree trunk (brown)
        var trunk = new GameObject("Trunk");
        trunk.transform.SetParent(tree.transform);
        var trunkSprite = trunk.AddComponent<SpriteRenderer>();
        trunkSprite.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/PlayerSprite.png");
        trunkSprite.color = new Color(0.5f, 0.3f, 0.2f);
        trunk.transform.localScale = new Vector3(0.5f, 1f, 1);
        trunkSprite.sortingOrder = 0;

        // Tree top (green)
        var top = new GameObject("Top");
        top.transform.SetParent(tree.transform);
        top.transform.localPosition = new Vector3(0, 0.7f, 0);
        var topSprite = top.AddComponent<SpriteRenderer>();
        topSprite.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/PlayerSprite.png");
        topSprite.color = new Color(0.2f, 0.6f, 0.2f);
        top.transform.localScale = new Vector3(1.2f, 1.2f, 1);
        topSprite.sortingOrder = 1;

        // Add collider to base
        var collider = tree.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(0.5f, 0.8f);
        collider.offset = new Vector2(0, 0.4f);

        tree.layer = LayerMask.NameToLayer("Environment");
    }

    private static void CreateBorderWalls()
    {
        // Create invisible walls to contain the player
        float mapWidth = 12f;
        float mapHeight = 8f;

        CreateInvisibleWall(new Vector3(-mapWidth, 0, 0), new Vector2(1, mapHeight * 2), "LeftWall");
        CreateInvisibleWall(new Vector3(mapWidth, 0, 0), new Vector2(1, mapHeight * 2), "RightWall");
        CreateInvisibleWall(new Vector3(0, mapHeight, 0), new Vector2(mapWidth * 2, 1), "TopWall");
        CreateInvisibleWall(new Vector3(0, -mapHeight, 0), new Vector2(mapWidth * 2, 1), "BottomWall");
    }

    private static void CreateInvisibleWall(Vector3 position, Vector2 size, string name)
    {
        var wall = new GameObject(name);
        wall.transform.position = position;
        wall.layer = LayerMask.NameToLayer("Environment");

        var collider = wall.AddComponent<BoxCollider2D>();
        collider.size = size;

        wall.isStatic = true;
    }
} 