﻿using System;
using Demos.TopDownRpg;
using Microsoft.Xna.Framework;

namespace Demos.Screens
{
    public class MainMenuScreen : MenuScreen
    {
        private readonly Game _game;

        public MainMenuScreen(IServiceProvider serviceProvider, Game game)
            : base(serviceProvider)
        {
            _game = game;
        }

        public override void LoadContent()
        {
            base.LoadContent();

            AddMenuItem("New Game", Show<TopDownRpgScene>);
            AddMenuItem("Load Game", Show<LoadGameScreen>);
            AddMenuItem("Options", Show<OptionsScreen>);
            AddMenuItem("Exit", _game.Exit);
        }
    }
}