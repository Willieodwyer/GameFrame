﻿using Demos.TopDownRpg.SpeedState;
using GameFrame.Ink;
using GameFrame.Movers;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace Demos.TopDownRpg
{
    public class Entity : BaseMovable
    {
        [JsonProperty("name")] public string Name;

        [JsonProperty("sprite-sheet")] public string SpriteSheet;

        [JsonProperty("script")] public string Script;
        
        public SpeedContext SpeedContext;
        public override float Speed => SpeedContext.Speed;

        public Entity()
        {
            SpeedContext = new SpeedContext(4);
        }

        public virtual GameFrameStory Interact()
        {
            var storyText = StoryImporter.ReadStory(Script);
            var story = new GameFrameStory(storyText);
            return story;
        }

        public virtual void CompleteInteract()
        {
            
        }
    }
}
