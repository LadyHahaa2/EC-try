using System.Collections.Generic;
using UnityEngine;

namespace EchoCompiler
{
    public sealed class LevelLoader : MonoBehaviour
    {
        private readonly List<SwitchNode> switches = new List<SwitchNode>();
        private readonly List<Door> doors = new List<Door>();
        private readonly List<SignalShard> shards = new List<SignalShard>();
        private readonly List<LaserHazard> lasers = new List<LaserHazard>();
        private Transform roomRoot;
        private LevelDefinition[] definitions = new LevelDefinition[0];
        private bool ownsCurrentDefinition;
        private LevelDefinition generatedDefinition;

        public LevelDefinition CurrentDefinition { get; private set; }
        public IReadOnlyList<SwitchNode> Switches => switches;
        public IReadOnlyList<Door> Doors => doors;
        public IReadOnlyList<SignalShard> Shards => shards;
        public IReadOnlyList<LaserHazard> Lasers => lasers;
        public Transform RoomRoot => roomRoot;

        public void ConfigureDefinitions(LevelDefinition[] source)
        {
            definitions = source ?? new LevelDefinition[0];
        }

        public LevelDefinition GetDefinition(int index)
        {
            if (definitions != null && index >= 0 && index < definitions.Length && definitions[index] != null)
            {
                return definitions[index];
            }

            var fallback = CreateDefaultDefinition(index);
            generatedDefinition = fallback;
            return fallback;
        }

        public LevelDefinition CreateDefaultDefinition(int index)
        {
            var level = ScriptableObject.CreateInstance<LevelDefinition>();
            level.levelId = "level_0" + (index + 1);
            level.displayName = index == 0 ? "First Echo" : index == 1 ? "Two Nodes" : "Beat Hazard";
            level.roomSize = new Vector2(18f, 10f);
            level.timeLimit = index == 2 ? 90f : 75f;
            level.echoLimit = 3;
            level.playerSpawn = new Vector2(-6.4f, -2.8f);
            level.exitPosition = new Vector2(6.4f, 2.8f);
            level.walls = new LevelDefinition.WallDefinition[0];
            if (index == 0)
            {
                level.switches = new[] { Point("switch_alpha", -1.1f, -0.4f) };
                level.doors = new[] { Door("door_alpha", 2.8f, -0.4f, "switch_alpha") };
                level.shards = new LevelDefinition.PointDefinition[0];
                level.lasers = new LevelDefinition.LaserDefinition[0];
            }
            else if (index == 1)
            {
                level.displayName = "Maze Relay";
                level.playerSpawn = new Vector2(-7f, -3f);
                level.exitPosition = new Vector2(8.2f, 4.1f);
                level.switches = new[] { Point("switch_alpha", -6.5f, 2.6f), Point("switch_beta", -2.4f, -2f) };
                level.doors = new[] { Door("door_alpha", 2.8f, 0f, "switch_alpha") };
                level.shards = new[] { Point("shard_signal", 4.1f, 2.5f) };
                level.walls = new[]
                {
                    Wall("maze_west_spine", -4.7f, 0f, 0.6f, 7.2f),
                    Wall("maze_lower_run", -1.3f, -3.5f, 6.2f, 0.6f),
                    Wall("maze_center_spine", 0.4f, 1.2f, 0.6f, 5.2f),
                    Wall("maze_upper_run", 4.4f, 3.4f, 6.4f, 0.6f),
                    Wall("maze_east_spine", 5.2f, -1.5f, 0.6f, 4.2f),
                    Wall("maze_lower_turn", 6.6f, -3.3f, 3.6f, 0.6f)
                };
            }
            else
            {
                level.switches = new[] { Point("switch_alpha", -1.4f, 0f) };
                level.doors = new[] { Door("door_alpha", 2.7f, 0f, "switch_alpha") };
                level.shards = new[] { Point("shard_signal", 0.4f, 2.1f) };
                level.lasers = new[]
                {
                    new LevelDefinition.LaserDefinition { id = "laser_beat", start = new Vector2(-0.1f, -2.2f), end = new Vector2(-0.1f, 2.2f), period = 2.1f }
                };
            }
            return level;
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

        public void Build(LevelDefinition definition, RunManager manager)
        {
            var incomingGenerated = definition != null && definition == generatedDefinition;
            if (incomingGenerated) generatedDefinition = null;
            Clear();
            CurrentDefinition = definition;
            ownsCurrentDefinition = incomingGenerated;
            generatedDefinition = null;
            roomRoot = new GameObject("RuntimeRoom_" + definition.levelId).transform;
            BuildBackdrop(definition);
            BuildWalls(definition);

            var switchDefinitions = definition.switches ?? new LevelDefinition.PointDefinition[0];
            var doorDefinitions = definition.doors ?? new LevelDefinition.DoorDefinition[0];
            var shardDefinitions = definition.shards ?? new LevelDefinition.PointDefinition[0];
            var laserDefinitions = definition.lasers ?? new LevelDefinition.LaserDefinition[0];
            var wallDefinitions = definition.walls ?? new LevelDefinition.WallDefinition[0];

            for (var i = 0; i < wallDefinitions.Length; i++)
            {
                var wall = wallDefinitions[i];
                RuntimeVisuals.CreateColliderBlock(roomRoot, "MazeWall_" + wall.id, wall.position, wall.size, new Color(0.14f, 0.22f, 0.33f, 1f));
            }

            for (var i = 0; i < switchDefinitions.Length; i++)
            {
                var point = switchDefinitions[i];
                var go = RuntimeVisuals.CreateBlock(roomRoot, "Switch_" + point.id, point.position, new Vector2(1.1f, 1.1f), new Color(0.92f, 0.2f, 0.72f, 1f), 2);
                var switchCollider = go.AddComponent<CircleCollider2D>();
                switchCollider.isTrigger = true;
                var node = go.AddComponent<SwitchNode>();
                node.Configure(point.id, 1);
                switches.Add(node);
            }

            for (var i = 0; i < doorDefinitions.Length; i++)
            {
                var point = doorDefinitions[i];
                var go = RuntimeVisuals.CreateColliderBlock(roomRoot, "Door_" + point.id, point.position, new Vector2(0.65f, 3.1f), new Color(0.95f, 0.82f, 0.16f, 1f));
                var door = go.AddComponent<Door>();
                var switchId = string.IsNullOrWhiteSpace(point.requiredSwitchId)
                    ? (switches.Count > 0 ? switches[Mathf.Min(i, switches.Count - 1)].Id : string.Empty)
                    : point.requiredSwitchId;
                door.Configure(point.id, switchId);
                doors.Add(door);
            }

            for (var i = 0; i < shardDefinitions.Length; i++)
            {
                var point = shardDefinitions[i];
                var go = RuntimeVisuals.CreateBlock(roomRoot, "SignalShard_" + point.id, point.position, new Vector2(0.48f, 0.48f), new Color(1f, 0.86f, 0.1f, 1f), 3);
                go.transform.Rotate(0f, 0f, 45f);
                var shardCollider = go.AddComponent<CircleCollider2D>();
                shardCollider.isTrigger = true;
                var shard = go.AddComponent<SignalShard>();
                shard.Configure(point.id);
                shards.Add(shard);
            }

            for (var i = 0; i < laserDefinitions.Length; i++)
            {
                var info = laserDefinitions[i];
                var go = RuntimeVisuals.CreateColliderBlock(roomRoot, "Laser_" + info.id, info.start, new Vector2(0.22f, 2.3f), new Color(1f, 0.18f, 0.32f, 0.95f), true, 4);
                var laser = go.AddComponent<LaserHazard>();
                laser.Configure(info.start, info.end, info.period);
                lasers.Add(laser);
            }

            var exit = RuntimeVisuals.CreateBlock(roomRoot, "Exit_" + definition.levelId, definition.exitPosition, new Vector2(1.4f, 1.4f), new Color(0.2f, 0.95f, 0.9f, 0.9f), 1);
            var exitCollider = exit.AddComponent<BoxCollider2D>();
            exitCollider.isTrigger = true;
            exit.AddComponent<Exit>().Configure("exit_" + definition.levelId);

            UpdateDoors();
        }

        public void UpdateDoors()
        {
            for (var i = 0; i < doors.Count; i++)
            {
                var open = false;
                for (var s = 0; s < switches.Count; s++)
                {
                    if (doors[i].Requires(switches[s].Id)) open = switches[s].IsActive;
                }
                doors[i].SetOpen(open);
            }
        }

        public bool AllSwitchesActive()
        {
            for (var i = 0; i < switches.Count; i++) if (!switches[i].IsActive) return false;
            return true;
        }

        public void Clear()
        {
            switches.Clear();
            doors.Clear();
            shards.Clear();
            lasers.Clear();
            if (roomRoot != null) Destroy(roomRoot.gameObject);
            roomRoot = null;
            if (ownsCurrentDefinition && CurrentDefinition != null)
            {
                if (Application.isPlaying) Destroy(CurrentDefinition);
                else DestroyImmediate(CurrentDefinition);
            }
            if (generatedDefinition != null && generatedDefinition != CurrentDefinition)
            {
                if (Application.isPlaying) Destroy(generatedDefinition);
                else DestroyImmediate(generatedDefinition);
            }
            CurrentDefinition = null;
            ownsCurrentDefinition = false;
            generatedDefinition = null;
        }

        private void BuildBackdrop(LevelDefinition definition)
        {
            var background = RuntimeVisuals.CreateBlock(roomRoot, "Background", Vector2.zero, definition.roomSize, new Color(0.035f, 0.05f, 0.1f, 1f), -10);
            background.transform.position = new Vector3(0f, 0f, 1f);
            var border = new GameObject("RoomOutline").AddComponent<LineRenderer>();
            border.transform.SetParent(roomRoot, false);
            border.material = RuntimeVisuals.NeonMaterial;
            RuntimeVisuals.AddOutline(border, definition.roomSize, new Color(0.2f, 0.95f, 0.9f, 0.45f));
        }

        private void BuildWalls(LevelDefinition definition)
        {
            var w = definition.roomSize.x;
            var h = definition.roomSize.y;
            var wallColor = new Color(0.11f, 0.16f, 0.25f, 1f);
            RuntimeVisuals.CreateColliderBlock(roomRoot, "Wall_N", new Vector2(0f, h * 0.5f + 0.3f), new Vector2(w + 1f, 0.6f), wallColor);
            RuntimeVisuals.CreateColliderBlock(roomRoot, "Wall_S", new Vector2(0f, -h * 0.5f - 0.3f), new Vector2(w + 1f, 0.6f), wallColor);
            RuntimeVisuals.CreateColliderBlock(roomRoot, "Wall_W", new Vector2(-w * 0.5f - 0.3f, 0f), new Vector2(0.6f, h + 1f), wallColor);
            RuntimeVisuals.CreateColliderBlock(roomRoot, "Wall_E", new Vector2(w * 0.5f + 0.3f, 0f), new Vector2(0.6f, h + 1f), wallColor);
        }
    }
}
