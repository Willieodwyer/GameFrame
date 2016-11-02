﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameFrame.Content;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;

namespace GameFrame.MediaAdapter
{
    public class AudioPlayer : IAudioPlayer
    {
        ContentManager _content;
        private SoundEffect _effect;
        private SoundEffectInstance _instance;


        public virtual void Play(string audioType, string fileName)
        {
            if (audioType.Equals("wav", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine("AudioPlayer::play(): " + fileName);
                    _content = ContentManagerFactory.RequestContentManager();
                    _effect = _content.Load<SoundEffect>(fileName);
                    _instance = _effect.CreateInstance();
                    _instance.Play();
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
            }
            else
            {
                Debug.WriteLine("Invalid media. " + audioType + " format not supported");
            }
        }

        public virtual void Pause()
        {
            _instance?.Pause();
            Debug.WriteLine("AudioPLayer::pause()");
        }

        public virtual void Resume()
        {
            Debug.WriteLine("AudioPLayer::resume()");
        }
    }
}
