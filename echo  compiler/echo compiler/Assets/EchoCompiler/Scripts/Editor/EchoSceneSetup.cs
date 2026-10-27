#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EchoCompiler.Editor
{
    public static class EchoSceneSetup
    {
        [MenuItem("Tools/Echo/Import TextMeshPro Resources")]
        public static void ImportTextResources()
        {
            TMPro.TMP_PackageResourceImporter.ImportResources(true, false, false);
            AssetDatabase.Refresh();
            Debug.Log("Imported TextMeshPro Essential Resources for the generated HUD.");
        }

        [MenuItem("Tools/Echo/Create Demo Scene")]
        public static void CreateDemoScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            var camera = Camera.main;
            if (camera != null)
            {
                camera.orthographic = true;
                camera.orthographicSize = 6f;
                camera.backgroundColor = new Color(0.015f, 0.025f, 0.06f, 1f);
                camera.transform.position = new Vector3(0f, 0f, -10f);
            }

            var bootstrap = new GameObject("EchoCompilerBootstrap");
            var bootstrapComponent = bootstrap.AddComponent<EchoCompilerBootstrap>();
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/SampleScene.unity");

            EnsureFolder("Assets/EchoCompiler/Data");
            bootstrapComponent.levelDefinitions = new[]
            {
                CreateDefinition("Level_01.asset", 0),
                CreateDefinition("Level_02.asset", 1),
                CreateDefinition("Level_03.asset", 2)
            };

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene("Assets/Scenes/SampleScene.unity", true)
            };
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeGameObject = bootstrap;
            Debug.Log("Echo Compiler demo scene and three LevelDefinition assets created.");
        }

        private static void EnsureFolder(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                var parent = path.Substring(0, path.LastIndexOf('/'));
                var name = path.Substring(path.LastIndexOf('/') + 1);
                AssetDatabase.CreateFolder(parent, name);
            }
        }

        private static LevelDefinition CreateDefinition(string filename, int index)
        {
            var path = "Assets/EchoCompiler/Data/" + filename;
            var definition = AssetDatabase.LoadAssetAtPath<LevelDefinition>(path);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<LevelDefinition>();
                AssetDatabase.CreateAsset(definition, path);
            }

            definition.levelId = "level_0" + (index + 1);
            definition.displayName = index == 0 ? "First Echo" : index == 1 ? "Maze Relay" : "Beat Hazard";
            definition.roomSize = new Vector2(18f, 10f);
            definition.timeLimit = index == 2 ? 90f : 75f;
            definition.echoLimit = 3;
            definition.playerSpawn = index == 1 ? new Vector2(-7f, -3f) : new Vector2(-6.4f, -2.8f);
            definition.exitPosition = index == 1 ? new Vector2(8.2f, 4.1f) : new Vector2(6.4f, 2.8f);
            definition.switches = index == 0
                ? new[] { Point("switch_alpha", -1.1f, -0.4f) }
                : index == 1
                    ? new[] { Point("switch_alpha", -6.5f, 2.6f), Point("switch_beta", -2.4f, -2f) }
                    : new[] { Point("switch_alpha", -1.4f, 0f) };
            definition.doors = new[] { Door("door_alpha", 2.8f, index == 0 ? -0.4f : 0f, "switch_alpha") };
            definition.shards = index == 0
                ? new LevelDefinition.PointDefinition[0]
                : index == 1
                    ? new[] { Point("shard_signal", 4.1f, 2.5f) }
                    : new[] { Point("shard_signal", 0.5f, 2.1f) };
            definition.walls = index == 1
                ? new[]
                {
                    Wall("maze_west_spine", -4.7f, 0f, 0.6f, 7.2f),
                    Wall("maze_lower_run", -1.3f, -3.5f, 6.2f, 0.6f),
                    Wall("maze_center_spine", 0.4f, 1.2f, 0.6f, 5.2f),
                    Wall("maze_upper_run", 4.4f, 3.4f, 6.4f, 0.6f),
                    Wall("maze_east_spine", 5.2f, -1.5f, 0.6f, 4.2f),
                    Wall("maze_lower_turn", 6.6f, -3.3f, 3.6f, 0.6f)
                }
                : new LevelDefinition.WallDefinition[0];
            definition.lasers = index == 2
                ? new[] { new LevelDefinition.LaserDefinition { id = "laser_beat", start = new Vector2(-0.1f, -2.2f), end = new Vector2(-0.1f, 2.2f), period = 2.1f } }
                : new LevelDefinition.LaserDefinition[0];
            EditorUtility.SetDirty(definition);
            return definition;
        }

        private static LevelDefinition.PointDefinition Point(string id, float x, float y)
        {
            return new LevelDefinition.PointDefinition { id = id, position = new Vector2(x, y) };
        }

        private static LevelDefinition.DoorDefinition Door(string id, float x, float y, string requiredSwitchId)
        {
            return new LevelDefinition.DoorDefinition
            {
                id = id,
                position = new Vector2(x, y),
                requiredSwitchId = requiredSwitchId
            };
        }

        private static LevelDefinition.WallDefinition Wall(string id, float x, float y, float width, float height)
        {
            return new LevelDefinition.WallDefinition
            {
                id = id,
                position = new Vector2(x, y),
                size = new Vector2(width, height)
            };
        }
    }
}
#endif
