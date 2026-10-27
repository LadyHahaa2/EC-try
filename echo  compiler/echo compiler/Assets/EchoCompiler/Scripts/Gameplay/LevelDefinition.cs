using System;
using UnityEngine;

namespace EchoCompiler
{
    [CreateAssetMenu(fileName = "LevelDefinition", menuName = "Echo Compiler/Level Definition")]
    public sealed class LevelDefinition : ScriptableObject
    {
        [Serializable]
        public struct PointDefinition
        {
            public string id;
            public Vector2 position;
        }

        [Serializable]
        public struct DoorDefinition
        {
            public string id;
            public Vector2 position;
            public string requiredSwitchId;
        }

        [Serializable]
        public struct LaserDefinition
        {
            public string id;
            public Vector2 start;
            public Vector2 end;
            public float period;
        }

        [Serializable]
        public struct WallDefinition
        {
            public string id;
            public Vector2 position;
            public Vector2 size;
        }

        public string levelId = "level_01";
        public string displayName = "Echo Room";
        public Vector2 roomSize = new Vector2(18f, 10f);
        public Vector2 playerSpawn = new Vector2(-6f, -2f);
        public Vector2 exitPosition = new Vector2(6f, 2f);
        [Min(1f)] public float timeLimit = 90f;
        [Range(0, 3)] public int echoLimit = 3;
        public PointDefinition[] switches = new PointDefinition[0];
        public DoorDefinition[] doors = new DoorDefinition[0];
        public PointDefinition[] shards = new PointDefinition[0];
        public LaserDefinition[] lasers = new LaserDefinition[0];
        public WallDefinition[] walls = new WallDefinition[0];

        public bool HasSwitches => switches != null && switches.Length > 0;
    }
}
