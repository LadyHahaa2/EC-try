#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace EchoCompiler.Editor
{
    public sealed class EchoLevelValidatorWindow : EditorWindow
    {
        private LevelDefinition definition;
        private Vector2 scroll;
        private readonly List<string> messages = new List<string>();

        [MenuItem("Tools/Echo/Level Validator")]
        public static void Open()
        {
            var window = GetWindow<EchoLevelValidatorWindow>("Echo Level Validator");
            window.minSize = new Vector2(500f, 420f);
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Echo Compiler / Level Validator", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Checks null references, duplicate IDs, spatial overlap and recording bounds before a room is used.", MessageType.Info);
            definition = (LevelDefinition)EditorGUILayout.ObjectField("Level Definition", definition, typeof(LevelDefinition), false);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Validate", GUILayout.Height(30f))) Validate();
                if (GUILayout.Button("Generate Test Room", GUILayout.Height(30f))) GenerateTestRoom();
            }

            scroll = EditorGUILayout.BeginScrollView(scroll);
            for (var i = 0; i < messages.Count; i++)
            {
                var isError = messages[i].StartsWith("ERROR");
                EditorGUILayout.HelpBox(messages[i], isError ? MessageType.Error : MessageType.Info);
            }
            EditorGUILayout.EndScrollView();
        }

        private void Validate()
        {
            messages.Clear();
            if (definition == null)
            {
                messages.Add("ERROR: Select a LevelDefinition first.");
                Repaint();
                return;
            }

            if (string.IsNullOrWhiteSpace(definition.levelId)) messages.Add("ERROR: levelId is empty.");
            if (definition.roomSize.x <= 0f || definition.roomSize.y <= 0f) messages.Add("ERROR: roomSize must be positive.");
            if (definition.timeLimit <= 0f) messages.Add("ERROR: timeLimit must be greater than zero.");
            if (definition.echoLimit < 0 || definition.echoLimit > 3) messages.Add("ERROR: echoLimit must be between 0 and 3.");
            if (Vector2.Distance(definition.playerSpawn, definition.exitPosition) < 0.8f) messages.Add("ERROR: playerSpawn and exitPosition overlap.");

            var ids = new HashSet<string>();
            CheckIds("switch", definition.switches, ids);
            CheckDoorIds(definition.doors, ids, definition.switches);
            CheckIds("shard", definition.shards, ids);
            CheckWalls(definition.walls, ids);
            if (definition.lasers != null)
            {
                for (var i = 0; i < definition.lasers.Length; i++)
                {
                    if (!ids.Add(definition.lasers[i].id)) messages.Add("ERROR: duplicate ID " + definition.lasers[i].id + ".");
                    if (definition.lasers[i].period <= 0f) messages.Add("ERROR: laser " + definition.lasers[i].id + " has invalid period.");
                }
            }

            var guids = AssetDatabase.FindAssets("t:LevelDefinition");
            for (var i = 0; i < guids.Length; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                var other = AssetDatabase.LoadAssetAtPath<LevelDefinition>(path);
                if (other != null && other != definition && other.levelId == definition.levelId) messages.Add("ERROR: duplicate levelId in " + path + ".");
            }
            if (messages.Count == 0) messages.Add("OK: no blocking issues found.");
            Repaint();
        }

        private void CheckIds(string label, LevelDefinition.PointDefinition[] points, HashSet<string> ids)
        {
            if (points == null)
            {
                messages.Add("ERROR: " + label + " list is null.");
                return;
            }
            for (var i = 0; i < points.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(points[i].id)) messages.Add("ERROR: " + label + " " + i + " has an empty ID.");
                else if (!ids.Add(points[i].id)) messages.Add("ERROR: duplicate ID " + points[i].id + ".");
                if (definition.roomSize.x > 0f && Mathf.Abs(points[i].position.x) > definition.roomSize.x * 0.5f) messages.Add("ERROR: " + points[i].id + " is outside room bounds.");
                if (definition.roomSize.y > 0f && Mathf.Abs(points[i].position.y) > definition.roomSize.y * 0.5f) messages.Add("ERROR: " + points[i].id + " is outside room bounds.");
            }
        }

        private void CheckDoorIds(LevelDefinition.DoorDefinition[] doors, HashSet<string> ids, LevelDefinition.PointDefinition[] switches)
        {
            if (doors == null)
            {
                messages.Add("ERROR: door list is null.");
                return;
            }

            var switchIds = new HashSet<string>();
            if (switches != null)
            {
                for (var i = 0; i < switches.Length; i++) switchIds.Add(switches[i].id);
            }

            for (var i = 0; i < doors.Length; i++)
            {
                var door = doors[i];
                if (string.IsNullOrWhiteSpace(door.id)) messages.Add("ERROR: door " + i + " has an empty ID.");
                else if (!ids.Add(door.id)) messages.Add("ERROR: duplicate ID " + door.id + ".");
                if (definition.roomSize.x > 0f && Mathf.Abs(door.position.x) > definition.roomSize.x * 0.5f) messages.Add("ERROR: " + door.id + " is outside room bounds.");
                if (definition.roomSize.y > 0f && Mathf.Abs(door.position.y) > definition.roomSize.y * 0.5f) messages.Add("ERROR: " + door.id + " is outside room bounds.");
                if (!string.IsNullOrWhiteSpace(door.requiredSwitchId) && !switchIds.Contains(door.requiredSwitchId))
                {
                    messages.Add("ERROR: door " + door.id + " references missing switch " + door.requiredSwitchId + ".");
                }
            }
        }

        private void CheckWalls(LevelDefinition.WallDefinition[] walls, HashSet<string> ids)
        {
            if (walls == null) return;
            for (var i = 0; i < walls.Length; i++)
            {
                var wall = walls[i];
                if (string.IsNullOrWhiteSpace(wall.id)) messages.Add("ERROR: wall " + i + " has an empty ID.");
                else if (!ids.Add(wall.id)) messages.Add("ERROR: duplicate ID " + wall.id + ".");
                if (wall.size.x <= 0f || wall.size.y <= 0f) messages.Add("ERROR: wall " + wall.id + " has invalid size.");
                if (definition.roomSize.x > 0f && Mathf.Abs(wall.position.x) + wall.size.x * 0.5f > definition.roomSize.x * 0.5f)
                    messages.Add("ERROR: wall " + wall.id + " exceeds horizontal room bounds.");
                if (definition.roomSize.y > 0f && Mathf.Abs(wall.position.y) + wall.size.y * 0.5f > definition.roomSize.y * 0.5f)
                    messages.Add("ERROR: wall " + wall.id + " exceeds vertical room bounds.");
            }
        }

        private void GenerateTestRoom()
        {
            if (definition == null)
            {
                messages.Add("ERROR: Select a LevelDefinition before generating.");
                return;
            }
            var room = new GameObject("GeneratedTestRoom_" + definition.levelId);
            room.AddComponent<EchoCompilerBootstrap>();
            Undo.RegisterCreatedObjectUndo(room, "Generate Echo Test Room");
            Selection.activeGameObject = room;
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            messages.Add("OK: generated a bootstrap test room in the active scene.");
            Repaint();
        }
    }
}
#endif
