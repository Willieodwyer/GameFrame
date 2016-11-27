﻿using Demos.TopDownRpg.GameModes;
using GameFrame;
using GameFrame.Ink;
using Microsoft.Xna.Framework;

namespace Demos.TopDownRpg.Entities
{
    public class NorthDesertGuard : AbsractBattleEntity
    {
        private GameFrameStory _gameStory;
        private readonly string _flagName;
        public NorthDesertGuard(GameModeController gameModeController, string flag, Vector2 startPosition, Vector2 endPosition) : base(gameModeController, flag, startPosition, endPosition)
        {
            _flagName = flag;
            Name = "Guard";
            SpriteSheet = "1";
        }

        public override GameFrameStory Interact()
        {
            var learnedFight = GameFlags.GetVariable<bool>("learned_fight");
            var scriptName = learnedFight ? "north_guard_post_fight.ink" : "north_guard_pre_fight.ink";
            _gameStory = ReadStory(scriptName);
            _gameStory.ChoosePathString("dialog");
            CompleteEvent completeEvent = win =>
            {
                _gameStory.SetVariableState("to_move", true);
            };
            ReadStory(_gameStory, completeEvent);
            return _gameStory;
        }

        public override void CompleteInteract()
        {
            if (!AlreadyMoved)
            {
                var toMove = _gameStory.GetVariableState<int>("to_move") == 1;
                if (toMove)
                {
                    MoveDelegate?.Invoke(this, new Point(24, 35));
                    GameFlags.SetVariable(_flagName, true);
                    AlreadyMoved = true;
                }
            }
        }
    }
}
