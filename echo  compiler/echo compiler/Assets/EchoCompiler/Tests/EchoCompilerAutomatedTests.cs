using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace EchoCompiler.Tests
{
    public sealed class EchoCompilerAutomatedTests
    {
        [Test] public void RecordingBufferCapsAtCapacity()
        {
            var buffer = new RecordingBuffer(4);
            buffer.Begin();
            for (var i = 0; i < 8; i++) buffer.Sample(i * 0.02f, Vector2.right, false);
            Assert.That(buffer.Count, Is.EqualTo(4));
        }

        [Test] public void RecordingBufferCopyIsIndependent()
        {
            var buffer = new RecordingBuffer(4);
            buffer.Begin();
            buffer.Sample(0.1f, Vector2.up, true);
            var copy = buffer.CopyFrames();
            buffer.Clear();
            Assert.That(copy.Length, Is.EqualTo(1));
            Assert.That(copy[0].interact, Is.True);
        }

        [Test] public void RecordingBufferClearResetsCount()
        {
            var buffer = new RecordingBuffer(2);
            buffer.Begin();
            buffer.Sample(0f, Vector2.zero, false);
            buffer.Clear();
            Assert.That(buffer.Count, Is.Zero);
        }

        [Test] public void InputFrameKeepsPayload()
        {
            var frame = new InputFrame(0.2f, new Vector2(0.5f, -0.5f), true);
            Assert.That(frame.time, Is.EqualTo(0.2f).Within(0.0001f));
            Assert.That(frame.move, Is.EqualTo(new Vector2(0.5f, -0.5f)));
            Assert.That(frame.interact, Is.True);
        }

        [Test] public void GameStateMachineEmitsTransitions()
        {
            var machine = new GameStateMachine();
            var changed = 0;
            machine.Changed += (_, __) => changed++;
            machine.Set(EchoGameState.Recording);
            machine.Set(EchoGameState.Recording);
            Assert.That(machine.Current, Is.EqualTo(EchoGameState.Recording));
            Assert.That(changed, Is.EqualTo(1));
        }

        [Test] public void DefaultLevelsHaveUniqueIds()
        {
            var loader = new GameObject("TestLoader").AddComponent<LevelLoader>();
            var first = loader.CreateDefaultDefinition(0);
            var second = loader.CreateDefaultDefinition(1);
            var third = loader.CreateDefaultDefinition(2);
            Assert.That(first.levelId, Is.Not.EqualTo(second.levelId));
            Assert.That(second.levelId, Is.Not.EqualTo(third.levelId));
            Object.DestroyImmediate(loader.gameObject);
            Object.DestroyImmediate(first);
            Object.DestroyImmediate(second);
            Object.DestroyImmediate(third);
        }

        [Test] public void DefaultLevelsStayWithinMvpBounds()
        {
            var loader = new GameObject("TestLoader").AddComponent<LevelLoader>();
            for (var i = 0; i < 3; i++)
            {
                var level = loader.CreateDefaultDefinition(i);
                Assert.That(level.roomSize.x, Is.GreaterThan(0f));
                Assert.That(level.roomSize.y, Is.GreaterThan(0f));
                Assert.That(level.echoLimit, Is.InRange(0, 3));
                Assert.That(level.timeLimit, Is.GreaterThan(0f));
                Object.DestroyImmediate(level);
            }
            Object.DestroyImmediate(loader.gameObject);
        }

        [Test] public void DoorDefinitionsDeclareExplicitSwitchBinding()
        {
            var loader = new GameObject("TestLoader").AddComponent<LevelLoader>();
            var level = loader.CreateDefaultDefinition(1);
            Assert.That(level.doors, Is.Not.Null.And.Not.Empty);
            Assert.That(level.doors[0].requiredSwitchId, Is.EqualTo("switch_alpha"));
            Object.DestroyImmediate(loader.gameObject);
            Object.DestroyImmediate(level);
        }

        [Test] public void MazeLevelDefinesNavigableWallLayout()
        {
            var loader = new GameObject("TestLoader").AddComponent<LevelLoader>();
            var level = loader.CreateDefaultDefinition(1);
            Assert.That(level.displayName, Is.EqualTo("Maze Relay"));
            Assert.That(level.walls, Has.Length.EqualTo(6));
            Assert.That(level.walls[0].size.x, Is.GreaterThan(0f));
            Assert.That(level.walls[0].size.y, Is.GreaterThan(0f));
            Object.DestroyImmediate(loader.gameObject);
            Object.DestroyImmediate(level);
        }

        [UnityTest]
        public IEnumerator RuntimeTypesCanBeCreatedInPlayMode()
        {
            var go = new GameObject("PlayModeInput");
            var router = go.AddComponent<InputRouter>();
            yield return null;
            Assert.That(router, Is.Not.Null);
            Cleanup(go);
        }

        [UnityTest]
        public IEnumerator RuntimeStateMachineStartsAtMenu()
        {
            var go = new GameObject("PlayModeManager");
            var manager = go.AddComponent<RunManager>();
            yield return null;
            Assert.That(manager.State, Is.EqualTo(EchoGameState.Menu));
            Cleanup(go);
        }

        [UnityTest]
        public IEnumerator RuntimeVisualsCreatesConfiguredSprite()
        {
            var root = new GameObject("PlayModeVisualRoot");
            var block = RuntimeVisuals.CreateBlock(root.transform, "Block", Vector2.zero, Vector2.one, Color.white);
            yield return null;
            Assert.That(block.GetComponent<SpriteRenderer>().sprite, Is.Not.Null);
            Cleanup(root);
        }

        private static void Cleanup(Object target)
        {
            if (target == null) return;
            if (Application.isPlaying) Object.Destroy(target);
            else Object.DestroyImmediate(target);
        }
    }
}
