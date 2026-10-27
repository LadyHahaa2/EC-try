using UnityEngine;

namespace EchoCompiler
{
    public sealed class EchoAgent : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 3.5f;
        private InputFrame[] frames = new InputFrame[0];
        private int frameIndex;
        private Rigidbody2D body;
        private LineRenderer trail;
        private float trailTimer;
        private ActorMarker marker;

        public int FrameCount => frames == null ? 0 : frames.Length;
        public int FrameIndex => frameIndex;

        public void Initialize(InputFrame[] source, Vector3 startPosition, Color color, RunManager owner)
        {
            frames = source ?? new InputFrame[0];
            frameIndex = 0;
            transform.position = startPosition;
            body = GetComponent<Rigidbody2D>() ?? gameObject.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.freezeRotation = true;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            marker = GetComponent<ActorMarker>() ?? gameObject.AddComponent<ActorMarker>();
            marker.manager = owner;
            marker.SetEcho(true);
            var renderer = GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                renderer.color = new Color(color.r, color.g, color.b, 0.55f);
                renderer.sortingOrder = 3;
            }

            trail = GetComponent<LineRenderer>();
            if (trail == null)
            {
                gameObject.AddComponent<LineRenderer>();
                trail = GetComponent<LineRenderer>();
            }
            if (trail == null) return;
            trail.positionCount = 0;
            trail.startWidth = 0.045f;
            trail.endWidth = 0.01f;
            trail.material = owner != null ? owner.NeonMaterial : null;
            trail.startColor = new Color(color.r, color.g, color.b, 0.8f);
            trail.endColor = new Color(color.r, color.g, color.b, 0.08f);
            trailTimer = 0f;
        }

        public void ResetPlayback(Vector3 position)
        {
            frameIndex = 0;
            transform.position = position;
            if (trail != null) trail.positionCount = 0;
        }

        private void FixedUpdate()
        {
            if (frames == null || frameIndex >= frames.Length || body == null) return;
            var frame = frames[frameIndex++];
            var delta = frame.move * moveSpeed * Time.fixedDeltaTime;
            body.MovePosition(body.position + delta);
            trailTimer += Time.fixedDeltaTime;
            if (trail != null && trailTimer >= 0.08f)
            {
                trailTimer = 0f;
                trail.positionCount = Mathf.Min(trail.positionCount + 1, 96);
                trail.SetPosition(trail.positionCount - 1, transform.position);
            }
        }
    }
}
