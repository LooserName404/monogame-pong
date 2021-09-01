using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace MonoPong
{
    public enum PadType { One, Two, Three, Four, Keyboard, Mouse, AI }
    public enum Button { Left = -1, Right = 1, Click = 2, None = 0 }

    public class PlayerPad
    {
        public int Y
        {
            get;
            private set;
        }

        public readonly PadType Type;

        private int _oldY;

        private static int _oldX;
        private static int _oldDirection;
        private static bool _latch;

        public PlayerPad(PadType padType)
        {
            Type = padType;
            var mouse = Mouse.GetState();
            _oldY = mouse.Y;
        }

        public static (PadType, Button) SelectionPoll()
        {
            var dirLatch = _oldDirection;
            int dir;
            bool buttonLatch = _latch;
            bool clicked;

            // Check the gamepads
            for (int i = 0; i < 4; i++)
            {
                var pad = GamePad.GetState(i);
                // if it's not connected, who cares
                if (!pad.IsConnected) continue;

                // We latch the select button because you have to hit select to get to the controller menu
                clicked = pad.IsButtonDown(Buttons.A);
                _latch = clicked;
                if (clicked & !buttonLatch) return ((PadType) i, Button.Click);

                // Direction is latched so when unselecting you don't full go to the other
                dir = pad.IsButtonDown(Buttons.DPadLeft) | pad.IsButtonDown(Buttons.LeftThumbstickLeft) ? -1 :
                    pad.IsButtonDown(Buttons.DPadRight) | pad.IsButtonDown(Buttons.LeftThumbstickRight) ? 1 : 0;
                _oldDirection = dir;
                if (dir != 0) return ((PadType) i, dir != dirLatch ? (Button) dir : 0);
            }

            // do the same for the keyboard
            var keyboard = Keyboard.GetState();
            clicked = keyboard.IsKeyDown(Keys.Space);
            _latch = clicked;
            if (clicked & !buttonLatch) return (PadType.Keyboard, Button.Click);
            dir = keyboard.IsKeyDown(Keys.Left) ? -1 :
                keyboard.IsKeyDown(Keys.Right) ? 1 : 0;
            _oldDirection = dir;
            if (dir != 0) return (PadType.Keyboard, dir != dirLatch ? (Button) dir : 0);

            // and mouse
            var mouse = Mouse.GetState();
            var x = mouse.X;
            clicked = mouse.LeftButton == ButtonState.Pressed;
            _latch = clicked;
            if (clicked & !buttonLatch) return (PadType.Mouse, Button.Click);
            var newX = x - _oldX;
            _oldX = x;
            dir = Math.Sign(newX);
            _oldDirection = dir;
            if (dir != 0) return (PadType.Mouse, dir != dirLatch ? (Button) dir : 0);

            // nothing input, so return a pretty blank tuple.
            return (PadType.AI, 0);
        }

        private int GetPad(PlayerIndex padType)
        {
            var state = GamePad.GetState(padType);
            if (!state.IsConnected) return 0;
            var dpad = state.IsButtonDown(Buttons.DPadUp) ? -1 : state.IsButtonDown(Buttons.DPadDown) ? 1 : 0;
            if (dpad != 0) return dpad * 16;
            var stick = state.ThumbSticks.Left.Y;
            return (int)(stick * 16);
        }

        private int GetKeyboard()
        {
            var state = Keyboard.GetState();
            var dpad = state.IsKeyDown(Keys.Up) ? -1 : state.IsKeyDown(Keys.Down) ? 1 : 0;
            return dpad * 16;
        }

        private int GetMouse()
        {
            var state = Mouse.GetState();
            var newY = state.Y - _oldY;
            _oldY = state.Y;
            return newY;
        }

        public void Poll()
        {
            Y = Type switch
            {
                PadType.One => GetPad((PlayerIndex)Type),
                PadType.Two => GetPad((PlayerIndex)Type),
                PadType.Three => GetPad((PlayerIndex)Type),
                PadType.Four => GetPad((PlayerIndex)Type),
                PadType.Keyboard => GetKeyboard(),
                PadType.Mouse => GetMouse(),
                _ => 0
            };
        }
    }
}
