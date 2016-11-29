﻿using System;
using System.Collections.Generic;
using Demos.TopDownRpg.Entities;
using GameFrame;
using GameFrame.Common;
using GameFrame.Controllers;
using GameFrame.Controllers.Click;
using GameFrame.Controllers.Click.TouchScreen;
using GameFrame.Controllers.GamePad;
using GameFrame.Controllers.KeyBoard;
using GameFrame.Controllers.SmartButton;
using GameFrame.Ink;
using GameFrame.Movers;
using GameFrame.ServiceLocator;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using MonoGame.Extended;

namespace Demos.TopDownRpg.GameModes
{
    public delegate void ClickEvent(Point p);
    public abstract class AbstractRpgGameMode : IGameMode
    {
        public List<IUpdate> UpdateList { get; }
        public StoryDialogBox DialogBox { get; set; }
        public ClickEvent ClickEvent { get; set; }
        public EventHandler InteractEvent { get; set; }

        protected AbstractRpgGameMode()
        {
            UpdateList = new List<IUpdate>();
            var clickController = new ClickController();
            clickController.MouseControl.OnPressedEvent += (state, mouseState) =>
            {
                if (!DialogBox.Interact(mouseState.Position))
                {
                    ClickEvent?.Invoke(mouseState.Position);
                }
            };
            var moveGesture = new SmartGesture(GestureType.Tap);
            moveGesture.GestureEvent += gesture =>
            {
                var position = gesture.Position.ToPoint();
                if (!DialogBox.Interact(position))
                {
                    ClickEvent?.Invoke(position);
                }
            };
            clickController.TouchScreenControl.AddSmartGesture(moveGesture);
            UpdateList.Add(clickController);
        }

        public void AddInteractionController()
        {
            var controller = new SmartController();
            var buttonsCreated = StaticServiceLocator.ContainsService<List<IButtonAble>>();
            if (!buttonsCreated)
            {
                var interactButton = new List<IButtonAble>
                {
                    new KeyButton(Keys.E),
                    new GamePadButton(Buttons.A)
                };
                StaticServiceLocator.AddService(interactButton);
            }
            var buttons = StaticServiceLocator.GetService<List<IButtonAble>>();
            var smartButton = new CompositeSmartButton(buttons)
            {
                OnButtonJustPressed = (sender, args) =>
                {
                    if (!DialogBox.Interact())
                    {
                        InteractEvent?.Invoke(this, null);
                    }
                }
            };
            controller.AddButton(smartButton);
            UpdateList.Add(controller);
        }

        public void Update(GameTime gameTime)
        {
            foreach (var toUpdate in UpdateList)
            {
                toUpdate.Update(gameTime);
            }
        }

        public abstract void Dispose();

        public abstract void Draw(SpriteBatch spriteBatch);
    }
}