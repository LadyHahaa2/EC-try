using System.Collections.Generic;
using UnityEngine;

namespace EchoCompiler
{
    public sealed class EchoPool : MonoBehaviour
    {
        private readonly Queue<EchoAgent> available = new Queue<EchoAgent>(3);
        private readonly List<EchoAgent> active = new List<EchoAgent>(3);
        private Transform parent;
        private Transform poolRoot;
        private RunManager owner;

        public void Prepare(Transform roomParent, RunManager manager)
        {
            ReleaseAll();
            parent = roomParent;
            owner = manager;
            if (poolRoot == null)
            {
                poolRoot = new GameObject("EchoPoolRoot").transform;
                poolRoot.SetParent(transform, false);
            }
            PruneAvailable();
            var colors = new[] { new Color(1f, 0.3f, 0.86f), new Color(1f, 0.82f, 0.16f), new Color(0.25f, 0.9f, 1f) };
            for (var i = available.Count; i < 3; i++)
            {
                var go = RuntimeVisuals.CreateBlock(poolRoot, "EchoPool_" + i, Vector2.zero, new Vector2(0.68f, 0.68f), colors[i], 4);
                go.SetActive(false);
                var body = go.AddComponent<Rigidbody2D>();
                body.gravityScale = 0f;
                body.freezeRotation = true;
                body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
                body.interpolation = RigidbodyInterpolation2D.Interpolate;
                var collider = go.AddComponent<CircleCollider2D>();
                collider.radius = 0.44f;
                go.AddComponent<LineRenderer>();
                available.Enqueue(go.AddComponent<EchoAgent>());
            }
        }

        public EchoAgent Acquire(InputFrame[] frames, Vector3 start, Color color)
        {
            if (available.Count == 0 || parent == null) return null;
            var agent = available.Dequeue();
            agent.transform.SetParent(parent, false);
            agent.gameObject.SetActive(true);
            agent.Initialize(frames, start, color, owner);
            active.Add(agent);
            return agent;
        }

        public void ReleaseAll()
        {
            for (var i = 0; i < active.Count; i++)
            {
                if (active[i] != null)
                {
                    active[i].gameObject.SetActive(false);
                    if (poolRoot != null) active[i].transform.SetParent(poolRoot, false);
                    available.Enqueue(active[i]);
                }
            }
            active.Clear();
            PruneAvailable();
        }

        private void PruneAvailable()
        {
            if (available.Count == 0) return;
            var valid = new Queue<EchoAgent>(available.Count);
            while (available.Count > 0)
            {
                var agent = available.Dequeue();
                if (agent != null) valid.Enqueue(agent);
            }
            while (valid.Count > 0) available.Enqueue(valid.Dequeue());
        }
    }
}
