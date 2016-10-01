﻿using System;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;

namespace GameFrame.Controllers
{
    public class JoyStickButton : AbstractGamePadButton
    {
        public float ThumbstickTolerance = 0.35f;
        private readonly bool _leftStick;

        public JoyStickButton(bool leftStick = true, PlayerIndex player=PlayerIndex.One)
        {
            _leftStick = leftStick;
            Player = player;
        }

        public override void Update(GamePadState state)
        {
            var direction = _leftStick ? state.ThumbSticks.Left : state.ThumbSticks.Right;

            var absX = Math.Abs(direction.X);
            var absY = Math.Abs(direction.Y);
            if (absX > absY)
            {
                if (absX > ThumbstickTolerance)
                {
                    Button = direction.X > 0 ? Buttons.DPadRight : Buttons.DPadLeft;
                }
            }
            else if (absY > ThumbstickTolerance)
            {
                Button = direction.Y > 0 ? Buttons.DPadUp : Buttons.DPadDown;
            }
            else
            {
                Button = 0;
            }
        }
    }
}
