# Echo Compiler

Echo Compiler is a small Unity 2022.3 LTS top-down time-cooperation puzzle prototype. Move with WASD, record up to eight seconds, press Space to compile the recording into a translucent Echo, then use up to three past selves to hold switches, collect signal shards and reach the exit.

## Run

Open the Unity project at `echo  compiler/echo compiler` with Unity `2022.3.62f3c1` and open `Assets/Scenes/SampleScene.unity`. The runtime bootstrap builds the room from procedural sprites, so no external art or audio package is required. If the scene has been reset, use `Tools/Echo/Create Demo Scene` once in the editor.

The project imports Unity's bundled TextMeshPro Essential Resources for the generated HUD. No third-party art or audio package is used.

Controls: WASD or arrow keys move; Space commits the current recording; R resets the room; Escape or P pauses and resumes without losing the current recording; Space/Enter advances after a successful room or retries after failure. Level 2 is a data-driven maze with echo-operated switches.

## Core loop

1. Enter a room and move while the fixed-step recorder samples input at 50 Hz.
2. Press Space to spawn an Echo that replays the captured frames from the recording start position.
3. Coordinate the player and Echo actors on pressure switches, through doors and around hazards.
4. Collect required signal shards, reach the exit and advance to the next room.

## Architecture

`RunManager` owns the game state, room timer, recording commits, Echo lifetime, reset and success/fail transitions through `GameStateMachine`. `InputRouter` is the single input boundary and supports keyboard/arrow movement plus legacy joystick buttons as a compatibility fallback; the project remains configured for Unity's legacy input handler. `RecordingBuffer` derives its capacity from the actual `Time.fixedDeltaTime`, so an 8-second recording is not truncated when the physics step changes. `EchoAgent` replays frames in `FixedUpdate`. `LevelLoader` reads the ordered `LevelDefinition` assets assigned on `EchoCompilerBootstrap` and only uses the documented layouts as a missing-asset fallback. Doors bind to explicit switch IDs. `SwitchNode`, `Door`, `SignalShard`, `Exit` and `LaserHazard` communicate through small interaction boundaries. `HUDController` and `ReplayTimelineUI` update a stable Canvas without rebuilding the hierarchy every frame.

## Editor tool

Open `Tools/Echo/Level Validator`, select a `LevelDefinition`, then press `Validate`. The validator reports null arrays, empty or duplicate IDs, out-of-bounds points, overlapping spawn/exit positions, invalid time limits, invalid Echo limits and duplicate level IDs. `Generate Test Room` creates a bootstrap object in the active scene. `Tools/Echo/Create Demo Scene` creates the sample scene and three data assets.

## Tests and performance

`Assets/EchoCompiler/Tests/EchoCompilerAutomatedTests.cs` contains 8 EditMode and 3 PlayMode Unity Test Runner checks for recording, state, level data, explicit door binding and runtime object creation. The legacy ContextMenu smoke scripts remain available for quick manual checks. The HUD exposes FPS, GC/frame, batches and managed memory through `ProfilerRecorder`; the runtime is designed to avoid per-frame Canvas hierarchy changes. Exact 1080p/60 FPS, GC allocation, batches and memory values remain **TO MEASURE** in the Unity Profiler on the target machine.

Validation record from this workspace: Unity `2022.3.62f3c1` batch compilation passed with no C# errors; the Windows player build completed successfully at 82,889,620 bytes. A headless player startup produced no managed exceptions or missing-glyph warnings, and a normal windowed player startup rendered the first room and generated HUD. Shader "unsupported on this GPU" lines in the headless log are expected from the `-nographics` null device and are not performance measurements.

## Known limitations

The input path uses Unity's legacy input handler because the blank project is configured for it; `InputRouter` keeps the boundary small enough to swap to the Input System package later. Replay determinism is based on fixed-step input frames and can vary slightly across machines; a production build can switch to position snapshots and interpolation if cross-machine determinism is required. The demo intentionally omits leaderboard, random seeds and trajectory export from the MVP. Profiler measurements and a recorded 60-90 second demo video are still release-material tasks, not code-completeness claims.

## Interview notes

- Situation/Task: needed a small, explainable 2D prototype without art dependencies. Action: built a procedural runtime room and a bounded fixed-step recording pipeline. Result: three escalating puzzle rooms can be replayed from one Unity scene.
- Situation/Task: repeated replay bugs were hard to diagnose. Action: isolated input, buffer, playback and reset responsibilities behind `RunManager`, `RecordingBuffer` and `EchoAgent`. Result: a single reset path clears actors, timers and coroutines.
- Situation/Task: editor setup errors slowed iteration. Action: added `EchoLevelValidatorWindow` with configuration checks and test-room generation. Result: invalid IDs, bounds and parameters are surfaced before play mode.

## 90-second demo script

"Echo Compiler turns a player's movement into a reusable teammate. I start in the first room, route to the pressure plate, press Space, and let the Echo hold the plate while I cross the door. The second room demonstrates two recordings and a signal shard. The third adds a moving laser: touching the player or any Echo resets the whole run safely. The project is deliberately asset-free; the same data-driven loader builds all rooms, and the validator catches bad definitions before runtime."
