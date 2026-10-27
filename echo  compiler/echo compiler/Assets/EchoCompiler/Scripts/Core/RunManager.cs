using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EchoCompiler
{
    public sealed class RunManager : MonoBehaviour
    {
        public static RunManager Instance { get; private set; }

        [Header("Recording")]
        [SerializeField] private float recordingSeconds = 8f;
        [SerializeField] private float moveSpeed = 3.5f;
        [SerializeField] private int maxEchoes = 3;

        private readonly List<EchoAgent> echoes = new List<EchoAgent>(3);
        private RecordingBuffer recording;
        private InputRouter input;
        private LevelLoader loader;
        private HUDController hud;
        private EchoPool echoPool;
        private Rigidbody2D playerBody;
        private ActorMarker playerMarker;
        private SpriteRenderer playerRenderer;
        private Vector2 recordingStart;
        private float recordingTime;
        private float levelTime;
        private bool resetting;
        private int levelIndex;
        private int collectedShards;
        private AudioSource audioSource;
        private AudioClip beepClip;
        private GameStateMachine stateMachine;
        private FeedbackFx feedback;
        private float shakeTime;
        private float shakeAmplitude;

        public EchoGameState State => stateMachine == null ? EchoGameState.Menu : stateMachine.Current;
        public LevelDefinition CurrentLevel => loader == null ? null : loader.CurrentDefinition;
        public int LevelIndex => levelIndex;
        public int EchoCount => echoes.Count;
        public int MaxEchoes => maxEchoes;
        public float RecordingProgress => recordingSeconds <= 0f ? 0f : Mathf.Clamp01(recordingTime / recordingSeconds);
        public float LevelTime => levelTime;
        public int CollectedShards => collectedShards;
        public int ShardTotal => loader == null ? 0 : loader.Shards.Count;
        public float BestTime => CurrentLevel == null ? 0f : PlayerPrefs.GetFloat(BestTimeKey(CurrentLevel.levelId), 0f);
        public Material NeonMaterial => RuntimeVisuals.NeonMaterial;
        public Transform PlayerTransform => playerBody == null ? null : playerBody.transform;

        public void Configure(InputRouter inputRouter, LevelLoader levelLoader, HUDController hudController)
        {
            input = inputRouter;
            loader = levelLoader;
            hud = hudController;
            echoPool = GetComponent<EchoPool>() ?? gameObject.AddComponent<EchoPool>();
            feedback = GetComponent<FeedbackFx>() ?? gameObject.AddComponent<FeedbackFx>();
            feedback.Prepare(transform);
            stateMachine = stateMachine ?? new GameStateMachine();
            if (hud != null) hud.Bind(levelLoader);
            recording = new RecordingBuffer(RecordingCapacity());
            maxEchoes = Mathf.Clamp(maxEchoes, 1, 3);
            EnsureAudio();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            stateMachine = new GameStateMachine();
            DontDestroyOnLoad(gameObject);
        }

        private float FixedStep => Mathf.Max(Time.fixedDeltaTime, 0.0001f);

        private int RecordingCapacity()
        {
            return Mathf.Max(1, Mathf.CeilToInt(recordingSeconds / FixedStep) + 1);
        }

        private void SetState(EchoGameState next)
        {
            if (stateMachine == null) stateMachine = new GameStateMachine();
            stateMachine.Set(next);
        }

        private void Update()
        {
            if (input == null || loader == null) return;
            if (input.PausePressed && State != EchoGameState.Menu && State != EchoGameState.Success && State != EchoGameState.Fail)
            {
                TogglePause();
                return;
            }

            if (State == EchoGameState.Paused) return;
            if (input.ResetPressed && State != EchoGameState.Menu)
            {
                ResetRoom();
                return;
            }
            if (State == EchoGameState.Recording && input.SubmitPressed) CommitEcho();
            if (State == EchoGameState.Success && (input.SubmitPressed || Input.GetKeyDown(KeyCode.Return))) NextLevel();
            if (State == EchoGameState.Fail && (input.SubmitPressed || Input.GetKeyDown(KeyCode.Return))) ResetRoom();
            if (State == EchoGameState.Recording && levelTime >= CurrentLevel.timeLimit) FailRun("Time limit reached");
            if (hud != null) hud.Refresh(this);
        }

        private void FixedUpdate()
        {
            if (State != EchoGameState.Recording || playerBody == null || input == null || resetting) return;
            var movement = input.Move;
            playerBody.MovePosition(playerBody.position + movement * moveSpeed * Time.fixedDeltaTime);
            recordingTime += Time.fixedDeltaTime;
            levelTime += Time.fixedDeltaTime;
            recording.Sample(recordingTime, movement, input.InteractPressed);
            if (recordingTime >= recordingSeconds) CommitEcho();
        }

        public void StartNewRun()
        {
            levelIndex = 0;
            Time.timeScale = 1f;
            LoadLevel(levelIndex);
        }

        public void LoadLevel(int index)
        {
            levelIndex = Mathf.Clamp(index, 0, 2);
            if (echoPool != null) echoPool.ReleaseAll();
            var definition = loader.GetDefinition(levelIndex);
            loader.Build(definition, this);
            if (echoPool != null) echoPool.Prepare(loader.RoomRoot, this);
            CreatePlayer(definition.playerSpawn);
            collectedShards = 0;
            echoes.Clear();
            levelTime = 0f;
            BeginRecording();
            if (hud != null) hud.ShowRoom(definition.displayName);
        }

        private void CreatePlayer(Vector2 position)
        {
            if (playerBody != null) Destroy(playerBody.gameObject);
            var go = RuntimeVisuals.CreateBlock(loader.RoomRoot, "Player", position, new Vector2(0.72f, 0.72f), new Color(0.2f, 0.7f, 1f, 1f), 5);
            playerRenderer = go.GetComponent<SpriteRenderer>();
            playerBody = go.AddComponent<Rigidbody2D>();
            playerBody.gravityScale = 0f;
            playerBody.freezeRotation = true;
            playerBody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            playerBody.interpolation = RigidbodyInterpolation2D.Interpolate;
            var collider = go.AddComponent<CircleCollider2D>();
            collider.radius = 0.46f;
            playerMarker = go.AddComponent<ActorMarker>();
            playerMarker.manager = this;
            playerMarker.SetEcho(false);
            var camera = Camera.main;
            if (camera != null) camera.transform.position = new Vector3(0f, 0f, -10f);
        }

        private void BeginRecording()
        {
            if (recording == null || recording.Capacity != RecordingCapacity()) recording = new RecordingBuffer(RecordingCapacity());
            recording.Begin();
            recordingStart = playerBody == null ? Vector2.zero : playerBody.position;
            recordingTime = 0f;
            SetState(EchoGameState.Recording);
        }

        public void CommitEcho()
        {
            if (resetting || State != EchoGameState.Recording) return;
            SetState(EchoGameState.Replay);
            var frames = recording.CopyFrames();
            if (frames.Length == 0) frames = new[] { new InputFrame(0f, Vector2.zero, false) };
            if (echoes.Count < maxEchoes) SpawnEcho(frames, recordingStart);
            recording.Clear();
            PlayBeep(660f);
            PlayFeedback(recordingStart, new Color(1f, 0.3f, 0.86f), 0.08f, 0.04f);
            BeginRecording();
        }

        private void SpawnEcho(InputFrame[] frames, Vector2 start)
        {
            var colors = new[] { new Color(1f, 0.3f, 0.86f), new Color(1f, 0.82f, 0.16f), new Color(0.25f, 0.9f, 1f) };
            var color = colors[echoes.Count % colors.Length];
            var agent = echoPool == null ? null : echoPool.Acquire(frames, start, color);
            if (agent == null)
            {
                var go = RuntimeVisuals.CreateBlock(loader.RoomRoot, "Echo_" + echoes.Count, start, new Vector2(0.68f, 0.68f), color, 4);
                var body = go.AddComponent<Rigidbody2D>();
                body.gravityScale = 0f;
                body.freezeRotation = true;
                body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
                body.interpolation = RigidbodyInterpolation2D.Interpolate;
                var collider = go.AddComponent<CircleCollider2D>();
                collider.radius = 0.44f;
                go.AddComponent<LineRenderer>();
                agent = go.AddComponent<EchoAgent>();
                agent.Initialize(frames, start, color, this);
            }
            echoes.Add(agent);
        }

        public void NotifyShardCollected(SignalShard shard)
        {
            collectedShards++;
            PlayBeep(880f);
            PlayFeedback(shard == null ? PlayerTransform.position : shard.transform.position, new Color(1f, 0.86f, 0.1f), 0.12f, 0.06f);
        }

        public void TryExit()
        {
            if (State != EchoGameState.Recording || loader == null || !loader.AllSwitchesActive()) return;
            if (collectedShards < loader.Shards.Count) return;
            SetState(EchoGameState.Success);
            SaveBestTime();
            PlayBeep(990f);
            PlayFeedback(PlayerTransform == null ? Vector3.zero : PlayerTransform.position, new Color(0.2f, 0.95f, 0.9f), 0.24f, 0.12f);
            if (hud != null) hud.ShowResult(true, "Room clear. Press Space for next room.");
        }

        public void FailRun(string reason)
        {
            if (resetting || State == EchoGameState.Success || State == EchoGameState.Fail) return;
            SetState(EchoGameState.Fail);
            PlayBeep(160f);
            PlayFeedback(PlayerTransform == null ? Vector3.zero : PlayerTransform.position, new Color(1f, 0.18f, 0.32f), 0.3f, 0.16f);
            if (hud != null) hud.ShowResult(false, reason + ". Press Space to retry.");
            StartCoroutine(ResetAfterFailure());
        }

        private IEnumerator ResetAfterFailure()
        {
            resetting = true;
            yield return new WaitForSecondsRealtime(0.45f);
            Time.timeScale = 1f;
            yield return null;
            LoadLevel(levelIndex);
            resetting = false;
        }

        public void ResetRoom()
        {
            if (loader == null || CurrentLevel == null || resetting) return;
            StartCoroutine(ResetRoomRoutine());
        }

        private IEnumerator ResetRoomRoutine()
        {
            resetting = true;
            Time.timeScale = 1f;
            yield return null;
            LoadLevel(levelIndex);
            resetting = false;
        }

        public void NextLevel()
        {
            if (levelIndex >= 2)
            {
                if (hud != null) hud.ShowResult(true, "All three rooms complete. Press R to replay.");
                return;
            }
            LoadLevel(levelIndex + 1);
        }

        public void TogglePause()
        {
            if (State == EchoGameState.Paused)
            {
                Time.timeScale = 1f;
                SetState(EchoGameState.Recording);
                if (hud != null) hud.ShowPause(false);
            }
            else if (State == EchoGameState.Recording)
            {
                Time.timeScale = 0f;
                SetState(EchoGameState.Paused);
                if (hud != null) hud.ShowPause(true);
            }
        }

        private void EnsureAudio()
        {
            if (audioSource == null) audioSource = gameObject.GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
            if (beepClip != null) return;
            const int rate = 44100;
            const int samples = 4410;
            beepClip = AudioClip.Create("GeneratedBeep", samples, 1, rate, false);
            var data = new float[samples];
            for (var i = 0; i < samples; i++) data[i] = Mathf.Sin(2f * Mathf.PI * 440f * i / rate) * Mathf.Exp(-5f * i / (float)samples);
            beepClip.SetData(data, 0);
        }

        private void PlayBeep(float pitch)
        {
            if (audioSource == null || beepClip == null) return;
            audioSource.pitch = Mathf.Clamp(pitch / 440f, 0.25f, 2.5f);
            audioSource.PlayOneShot(beepClip, 0.18f);
        }

        private static string BestTimeKey(string levelId)
        {
            return "EchoCompiler.BestTime." + levelId;
        }

        private void SaveBestTime()
        {
            if (CurrentLevel == null) return;
            var key = BestTimeKey(CurrentLevel.levelId);
            var previous = PlayerPrefs.GetFloat(key, 0f);
            if (previous <= 0f || levelTime < previous)
            {
                PlayerPrefs.SetFloat(key, levelTime);
                PlayerPrefs.Save();
                Debug.Log(string.Format("Echo level complete: {0} in {1:0.00}s (best)", CurrentLevel.levelId, levelTime));
            }
        }

        private void PlayFeedback(Vector3 position, Color color, float duration, float amplitude)
        {
            if (feedback != null) feedback.Burst(position, color);
            shakeTime = Mathf.Max(shakeTime, duration);
            shakeAmplitude = Mathf.Max(shakeAmplitude, amplitude);
        }

        private void LateUpdate()
        {
            if (loader == null || playerBody == null) return;
            loader.UpdateDoors();
            if (Camera.main != null)
            {
                var target = new Vector3(playerBody.position.x, playerBody.position.y, -10f);
                Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, target, 4f * Time.unscaledDeltaTime);
                if (shakeTime > 0f)
                {
                    var scale = shakeTime / 0.3f;
                    var offset = Random.insideUnitCircle * shakeAmplitude * Mathf.Clamp01(scale);
                    Camera.main.transform.position += new Vector3(offset.x, offset.y, 0f);
                    shakeTime -= Time.unscaledDeltaTime;
                    if (shakeTime <= 0f) shakeAmplitude = 0f;
                }
            }
        }
    }
}
