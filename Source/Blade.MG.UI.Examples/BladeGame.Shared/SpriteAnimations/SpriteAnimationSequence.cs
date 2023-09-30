using System;
using System.Collections.Generic;
using BladeGame.Sprites;

namespace BladeGame.SpriteAnimations
{
    public class SpriteAnimationSequence
    {

        public List<SpriteTexture2D> frames { get; set; }  = new List<SpriteTexture2D>();
        public string SequenceName { get; set; }
        public TimeSpan FrameDelay { get; set; }

        public SpriteAnimationSequence(string sequenceName)
        {
            this.SequenceName = sequenceName;
            this.FrameDelay = SpriteAnimation.FPS_30;
        }

        public SpriteAnimationSequence(string sequenceName, TimeSpan frameDelay)
        {
            this.SequenceName = sequenceName;
            this.FrameDelay = frameDelay;
        }

        public SpriteAnimationSequence(string sequenceName, TimeSpan frameDelay, List<SpriteTexture2D> frames)
        {
            this.SequenceName = sequenceName;
            this.FrameDelay = frameDelay;
            this.frames.AddRange(frames);
        }

        public void Dispose()
        {
            //// Dispose of all frames
            //foreach (Frame frame in frames)
            //{
            //    if (!frame.frameBitmap.isRecycled())
            //    {
            //        frame.frameBitmap.recycle();
            //    }
            //    frame.frameBitmap = null;
            //}

            frames.Clear();
        }

        public List<SpriteTexture2D> GetFrames()
        {
            return frames;
        }

        public void SetFrames(List<SpriteTexture2D> frames)
        {
            this.frames.Clear();
            this.frames.AddRange(frames);
        }

        public void AddFrames(List<SpriteTexture2D> frames)
        {
            this.frames.AddRange(frames);
        }

        public void AddFrame(SpriteTexture2D frame)
        {
            this.frames.Add(frame);
        }

    }
}
