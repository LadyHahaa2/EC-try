using System;

namespace EchoCompiler
{
    public sealed class GameStateMachine
    {
        public EchoGameState Current { get; private set; } = EchoGameState.Menu;
        public event Action<EchoGameState, EchoGameState> Changed;

        public void Set(EchoGameState next)
        {
            if (Current == next) return;
            var previous = Current;
            Current = next;
            Changed?.Invoke(previous, next);
        }
    }
}
