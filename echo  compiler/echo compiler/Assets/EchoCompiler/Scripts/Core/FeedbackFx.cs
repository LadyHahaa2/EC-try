using UnityEngine;

namespace EchoCompiler
{
    public sealed class FeedbackFx : MonoBehaviour
    {
        private ParticleSystem particles;

        public void Prepare(Transform parent)
        {
            if (particles != null) return;

            var go = new GameObject("GeneratedFeedbackParticles");
            go.transform.SetParent(parent, false);
            particles = go.AddComponent<ParticleSystem>();
            var main = particles.main;
            main.loop = false;
            main.playOnAwake = false;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startLifetime = 0.34f;
            main.startSpeed = 2.1f;
            main.startSize = 0.08f;
            main.maxParticles = 256;
            var emission = particles.emission;
            emission.enabled = false;
            var shape = particles.shape;
            shape.enabled = false;
            var renderer = particles.GetComponent<ParticleSystemRenderer>();
            renderer.material = RuntimeVisuals.NeonMaterial;
            renderer.sortingOrder = 8;
        }

        public void Burst(Vector3 position, Color color, int count = 14)
        {
            if (particles == null) Prepare(transform);
            var main = particles.main;
            main.startColor = color;
            var emit = new ParticleSystem.EmitParams { position = position, startColor = color };
            particles.Emit(emit, Mathf.Clamp(count, 1, 64));
        }
    }
}
