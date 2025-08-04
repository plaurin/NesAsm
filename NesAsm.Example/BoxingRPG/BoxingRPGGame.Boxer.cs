using NesAsm.Emulator;

namespace NesAsm.Example.BoxingRPG;

public static partial class BoxingRPGGame
{
    public static class Boxer
    {
        public static IBoxerState IdleState = new BoxerIdleState();
        public static IBoxerState JumpLeftState = new BoxerJumpLeftState();
        public static IBoxerState JumpRightState = new BoxerJumpRightState();

        public static IBoxerState CurrentState { get; private set; } = IdleState;

        public static int X;
        public static int Y;
        public static int AnimFramesLeft;

        public static void Update()
        {
            if (AnimFramesLeft > 0) AnimFramesLeft--;

            CurrentState.Update();

            DrawMetaTile(15, 22, BoxerPaletteIndex, CurrentState.Tiles);
        }

        internal static void ToState(IBoxerState targetState)
        {
            CurrentState.Exit();
            CurrentState = targetState;
            targetState.Enter();
        }

        public interface IBoxerState : IState
        {
            byte[,] Tiles { get; }
        }

        public class BoxerIdleState : IBoxerState
        {
            public byte[,] Tiles => BoxerIdleTiles;

            public void Update()
            {
                if (InputManager.LeftJustDoubleTap) ToState(JumpLeftState);
                if (InputManager.RightJustDoubleTap) ToState(JumpRightState);

                if (InputManager.Up) WindowY -= 1;
                if (InputManager.Down) WindowY += 1;

                if (InputManager.Left) X -= 1;
                if (InputManager.Right) X += 1;
            }
        }

        public abstract class BoxerJumpBaseState(int DeltaX) : IBoxerState
        {
            public abstract byte[,] Tiles { get; }

            public void Enter()
            {
                AnimFramesLeft = 30;
            }

            public void Exit()
            {
                Y = 0;
            }

            public void Update()
            {
                if (AnimFramesLeft == 0) ToState(IdleState);

                X += DeltaX;
                if (AnimFramesLeft > 15 && AnimFramesLeft % 2 == 0) Y += 1;
                if (AnimFramesLeft < 15 && AnimFramesLeft % 2 == 0) Y -= 1;
            }
        }

        public class BoxerJumpLeftState() : BoxerJumpBaseState(-2)
        {
            public override byte[,] Tiles => BoxerJumpLeftTiles;
        }

        public class BoxerJumpRightState() : BoxerJumpBaseState(2)
        {
            public override byte[,] Tiles => BoxerJumpRightTiles;
        }
    }
}