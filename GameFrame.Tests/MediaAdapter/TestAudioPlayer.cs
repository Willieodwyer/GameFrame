﻿using System;
using GameFrame.Content;
using GameFrame.MediaAdapter;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Screens;
using MonoGame.Extended.ViewportAdapters;

namespace GameFrame.Tests.MediaAdapter
{
    [TestClass]
    public class TestAudioPlayer : GameFrameGame
    {
        private  GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private readonly ScreenComponent _screenComponent;

        public TestAudioPlayer()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            base.LoadContent();
        }

        [TestMethod]
        public void PlayWav()
        {
            IAudioPlayer player = new AudioAdapter();
            player.Play("wav", "BirabutoKingdom");
            player.Resume();
            player.Pause();
            player.Dispose();
        }

        [TestMethod]
        public void PlayMp3()
        {
            IAudioPlayer player = new AudioAdapter();
            player.Play("mp3", "piano");
            player.Resume();
            player.Pause();
            player.Dispose();
        }
    }
}