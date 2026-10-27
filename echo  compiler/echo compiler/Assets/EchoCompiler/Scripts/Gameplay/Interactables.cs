using System.Collections.Generic;
using UnityEngine;

namespace EchoCompiler
{
    public interface IInteractable
    {
        string Id { get; }
        void Interact(ActorMarker actor);
    }

    public sealed class SwitchNode : MonoBehaviour, IInteractable
    {
        [SerializeField] private string id = "switch";
        [SerializeField] private int requiredActors = 1;
        private readonly HashSet<ActorMarker> occupants = new HashSet<ActorMarker>();
        private SpriteRenderer spriteRenderer;
        private Color inactiveColor = new Color(0.92f, 0.2f, 0.72f, 1f);
        private Color activeColor = new Color(0.2f, 0.95f, 0.9f, 1f);

        public string Id => id;
        public bool IsActive => occupants.Count >= requiredActors;

        public void Configure(string switchId, int actors = 1)
        {
            id = switchId;
            requiredActors = Mathf.Max(1, actors);
        }

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            var trigger = GetComponent<Collider2D>();
            if (trigger != null) trigger.isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var actor = other.GetComponentInParent<ActorMarker>();
            if (actor != null) occupants.Add(actor);
            RefreshVisual();
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            var actor = other.GetComponentInParent<ActorMarker>();
            if (actor != null) occupants.Remove(actor);
            RefreshVisual();
        }

        private void RefreshVisual()
        {
            if (spriteRenderer != null) spriteRenderer.color = IsActive ? activeColor : inactiveColor;
        }

        public void Interact(ActorMarker actor) { }
    }

    public sealed class Door : MonoBehaviour, IInteractable
    {
        [SerializeField] private string id = "door";
        [SerializeField] private string requiredSwitchId;
        private Collider2D blocker;
        private SpriteRenderer spriteRenderer;

        public string Id => id;
        public bool IsOpen { get; private set; }

        public void Configure(string doorId, string switchId)
        {
            id = doorId;
            requiredSwitchId = switchId;
        }

        public void SetOpen(bool open)
        {
            IsOpen = open;
            if (blocker != null) blocker.enabled = !open;
            if (spriteRenderer != null) spriteRenderer.color = open ? new Color(0.2f, 0.95f, 0.9f, 0.18f) : new Color(0.95f, 0.82f, 0.16f, 1f);
        }

        private void Awake()
        {
            blocker = GetComponent<Collider2D>() ?? gameObject.AddComponent<BoxCollider2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public bool Requires(string switchId) => string.Equals(requiredSwitchId, switchId, System.StringComparison.Ordinal);
        public void Interact(ActorMarker actor) { }
    }

    public sealed class SignalShard : MonoBehaviour, IInteractable
    {
        [SerializeField] private string id = "shard";
        public string Id => id;
        public bool Collected { get; private set; }
        private SpriteRenderer spriteRenderer;

        public void Configure(string shardId) { id = shardId; }

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            var trigger = GetComponent<Collider2D>();
            if (trigger != null) trigger.isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (Collected || other.GetComponentInParent<ActorMarker>() == null) return;
            Collected = true;
            if (spriteRenderer != null) spriteRenderer.enabled = false;
            var manager = other.GetComponentInParent<ActorMarker>().manager;
            if (manager != null) manager.NotifyShardCollected(this);
        }

        public void ResetShard()
        {
            Collected = false;
            if (spriteRenderer != null) spriteRenderer.enabled = true;
        }

        public void Interact(ActorMarker actor) { }
    }

    public sealed class Exit : MonoBehaviour, IInteractable
    {
        [SerializeField] private string id = "exit";
        public string Id => id;
        public void Configure(string exitId) { id = exitId; }

        private void Awake()
        {
            var trigger = GetComponent<Collider2D>();
            if (trigger != null) trigger.isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var actor = other.GetComponentInParent<ActorMarker>();
            if (actor != null && actor.manager != null) actor.manager.TryExit();
        }

        public void Interact(ActorMarker actor) { }
    }

    public sealed class LaserHazard : MonoBehaviour
    {
        [SerializeField] private Vector2 start;
        [SerializeField] private Vector2 end;
        [SerializeField] private float period = 2.4f;
        private float time;

        public void Configure(Vector2 from, Vector2 to, float seconds)
        {
            start = from;
            end = to;
            period = Mathf.Max(0.1f, seconds);
            transform.position = start;
        }

        private void Update()
        {
            time += Time.deltaTime;
            var t = Mathf.PingPong(time / period, 1f);
            transform.position = Vector2.Lerp(start, end, t);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var actor = other.GetComponentInParent<ActorMarker>();
            if (actor != null && actor.manager != null) actor.manager.FailRun("Laser contact");
        }
    }
}
