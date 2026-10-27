using System;
using UnityEngine;

namespace EchoCompiler
{
    [Serializable]
    public struct InputFrame
    {
        public float time;
        public Vector2 move;
        public bool interact;

        public InputFrame(float time, Vector2 move, bool interact)
        {
            this.time = time;
            this.move = move;
            this.interact = interact;
        }
    }

    public sealed class InputRouter : MonoBehaviour
    {
        [SerializeField] private float deadZone = 0.05f;

        public Vector2 Move
        {
            get
            {
                var value = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
                if (value == Vector2.zero)
                {
                    value = new Vector2(
                        (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow) ? 1f : 0f) - (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) ? 1f : 0f),
                        (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) ? 1f : 0f) - (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow) ? 1f : 0f));
                }
                if (value.sqrMagnitude > 1f) value.Normalize();
                return value.sqrMagnitude < deadZone * deadZone ? Vector2.zero : value;
            }
        }

        public bool SubmitPressed => Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.JoystickButton0);
        public bool ResetPressed => Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.JoystickButton1);
        public bool PausePressed => Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.JoystickButton7);
        public bool InteractPressed => Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.JoystickButton2);
    }

    [Serializable]
    public sealed class RecordingBuffer
    {
        [SerializeField] private int capacitySamples = 480;
        private InputFrame[] frames;
        private int count;

        public int Count => count;
        public int Capacity => capacitySamples;

        public RecordingBuffer(int capacitySamples = 480)
        {
            this.capacitySamples = Mathf.Max(1, capacitySamples);
            frames = new InputFrame[this.capacitySamples];
        }

        public void Begin()
        {
            count = 0;
            if (frames == null || frames.Length != capacitySamples) frames = new InputFrame[capacitySamples];
        }

        public void Sample(float time, Vector2 move, bool interact)
        {
            if (count >= capacitySamples) return;
            frames[count++] = new InputFrame(time, move, interact);
        }

        public InputFrame[] CopyFrames()
        {
            var copy = new InputFrame[count];
            Array.Copy(frames, copy, count);
            return copy;
        }

        public void Clear()
        {
            count = 0;
        }
    }

    public enum EchoGameState
    {
        Menu,
        Playing,
        Recording,
        Replay,
        Success,
        Fail,
        Paused
    }

    public sealed class ActorMarker : MonoBehaviour
    {
        public bool isEcho;
        public RunManager manager;

        public void SetEcho(bool value)
        {
            isEcho = value;
            gameObject.layer = LayerMask.NameToLayer("Default");
        }
    }
}
