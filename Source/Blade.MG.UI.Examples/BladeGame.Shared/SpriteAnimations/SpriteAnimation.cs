using System;
using System.Collections.Generic;
using BladeGame.Sprites;

namespace BladeGame.SpriteAnimations
{
    public class SpriteAnimation
    {
        public static readonly TimeSpan FPS_30 = TimeSpan.FromMilliseconds(1000f / 30f);
        public static readonly TimeSpan FPS_24 = TimeSpan.FromMilliseconds(1000f / 24f);
        public static readonly TimeSpan FPS_20 = TimeSpan.FromMilliseconds(1000f / 20f);
        public static readonly TimeSpan FPS_15 = TimeSpan.FromMilliseconds(1000f / 15f);
        public static readonly TimeSpan FPS_10 = TimeSpan.FromMilliseconds(1000f / 10f);
        public static readonly TimeSpan FPS_5 = TimeSpan.FromMilliseconds(1000f / 5f);
        public static readonly TimeSpan FPS_2 = TimeSpan.FromMilliseconds(1000f / 2f);
        public static readonly TimeSpan FPS_1 = TimeSpan.FromMilliseconds(1000f / 1f);

        public enum AnimationState
        {
            Stopped, Running, Paused, Finished
        }

        private Dictionary<string, SpriteAnimationSequence> sequences = new Dictionary<string, SpriteAnimationSequence>();
        private Dictionary<string, StoryBoard> storyboards = new Dictionary<string, StoryBoard>();

        private AnimationState currentState = AnimationState.Stopped;

        // Track StoryBoard
        private StoryBoard currentStoryBoard;
        private int storyBoardHasRepeated;
        public int numberOfTimesItemRepeated = 0;

        // Track StoryBoard Item
        private int currentItemIndex = 0;
        private StoryBoardItemBase currentItem;

        // Track Sequence
        int currentFrame = 0;
        private DateTime lastFrameTime = DateTime.Now;

        public SpriteAnimation()
        {

        }

        public void dispose()
        {
            currentState = AnimationState.Stopped;

            // Dispose of all Sequences
            foreach (SpriteAnimationSequence sequence in sequences.Values)
            {
                sequence.Dispose();
            }

            sequences.Clear();
            storyboards.Clear();
        }

        public void addSequence(SpriteAnimationSequence sequence)
        {
            sequences.Add(sequence.SequenceName, sequence);
        }

        public void addStoryBoard(StoryBoard storyBoard)
        {
            storyboards.Add(storyBoard.storyBoardName, storyBoard);
        }

        public StoryBoard getActiveStoryBoard()
        {
            return currentStoryBoard;
        }

        public String getActiveStoryBoardName()
        {
            if (currentStoryBoard == null)
            {
                return "";
            }

            return currentStoryBoard.storyBoardName;
        }

        public void startAnimation(String storyBoardName)
        {
            StoryBoard storyBoard;
            if (!storyboards.TryGetValue(storyBoardName, out storyBoard))
            {
                //Log.d("Animation", "StoryBoard not found '" + storyBoardName + "'");
            }

            startAnimation(storyBoard);
        }

        public void startAnimation(StoryBoard storyBoard)
        {
            // Set the storyboard
            currentStoryBoard = storyBoard;
            if (storyBoard == null)
            {
                return;
            }

            resetStoryBoard();
            lastFrameTime = DateTime.MinValue;

            currentState = AnimationState.Running;
        }

        public void stopAnimation()
        {
            currentState = AnimationState.Stopped;
        }

        public void pauseAnimation()
        {
            if (currentState == AnimationState.Running)
            {
                currentState = AnimationState.Paused;
            }
        }

        public void resumeAnimation()
        {
            if (currentState == AnimationState.Paused)
            {
                lastFrameTime = DateTime.Now;
                currentState = AnimationState.Running;
            }
        }

        public AnimationState getAnimationState()
        {
            return currentState;
        }

        public SpriteTexture2D getFrame()
        {
            return getFrame(1);
        }

        private SpriteTexture2D getFrame(int depth)
        {

            // Make sure we haven't entered a loop (assume a depth of 10 means trouble)
            if (depth >= 10)
            {
                return null;
            }

            // Make sure we have a StoryBoard
            if (currentStoryBoard == null)
            {
                //Log.d("Animation", "currentStoryBoard is null");
                return null;
            }

            // Check if the current storyboard item is a redirect
            if (currentItem is StoryBoardItemRedirect)
            {
                // Redirect to new storyboard
                StoryBoardItemRedirect item = (StoryBoardItemRedirect)currentItem;
                string newStoryBoardName = item.NewStoryBoard;

                if (!storyboards.TryGetValue(newStoryBoardName, out currentStoryBoard))
                {
                    currentStoryBoard = null;
                }

                resetStoryBoard();

                return getFrame(++depth);
            }

            // Get the current StoryBoard item
            // Fetch the Sequence for this StoryBoard item
            StoryBoardItem storyBoardItem = (StoryBoardItem)currentItem;
            SpriteAnimationSequence sequence;
            if (!sequences.TryGetValue(storyBoardItem.sequenceName, out sequence))
            {
                sequence = null;
            }

            // Make sure we have a frame to return
            if (sequence == null || sequence.frames.Count == 0)
            {
                return null;
            }

            // If the animation is not running just return the last frame we used
            if (currentState != AnimationState.Running)
            {
                return getCurrentFrame(sequence);
            }

            DateTime currentTime = DateTime.Now;
            if (lastFrameTime == DateTime.MinValue)
            {
                lastFrameTime = currentTime;
                resetSequence(sequence, storyBoardItem.direction);
            }
            float delta = (float)(currentTime - lastFrameTime).TotalMilliseconds;

            while (delta > sequence.FrameDelay.TotalMilliseconds)
            {
                delta -= (float)sequence.FrameDelay.TotalMilliseconds;
                lastFrameTime = currentTime;

                // Move to the Next Frame in the Current Sequence
                if (!moveToNextFrame(sequence, storyBoardItem.direction))
                {
                    // We've run out of frames, so move to the next StoryBoard Item
                    if (!moveToNextItem())
                    {
                        // No more StoryBoard Items
                        currentState = AnimationState.Finished;
                        return null;
                    }

                    // storyBoardItem = currentItem;
                    // sequence = sequences.get(storyBoardItem.sequenceName);
                    // resetSequence(sequence, storyBoardItem.direction);
                    return getFrame(++depth);
                }
            }

            return getCurrentFrame(sequence);
        }

        private void resetStoryBoard()
        {
            // Reset Storyboard
            storyBoardHasRepeated = 0;

            // Reset Current Storyboard Item
            currentItem = (currentStoryBoard.storyBoardItems.Count > 0) ? currentStoryBoard.storyBoardItems[0] : null;
            currentItemIndex = 0;

            // (currentItem.direction == SequenceDirection.Forward) ? 0 : (storyBoardItems.size() - 1);
        }

        private bool mustRepeatStoryBoard()
        {
            if (currentStoryBoard.numberOfTimesToRepeat == -1 || currentStoryBoard.numberOfTimesToRepeat <= storyBoardHasRepeated)
            {
                return true;
            }

            return false;
        }

        private bool moveToNextItem()
        {
            StoryBoardItem storyBoardItem = (StoryBoardItem)currentStoryBoard.storyBoardItems[currentItemIndex];
            numberOfTimesItemRepeated++;

            if (mustRepeatStoryBoardItem(storyBoardItem))
            {
                // Just continue with this StoryBoard Item
                return true;
            }

            // Move to the next StoryBoard Item
            currentItemIndex++;
            if (currentItemIndex >= currentStoryBoard.storyBoardItems.Count)
            {
                currentItemIndex = 0;
                storyBoardHasRepeated++;

                if (!mustRepeatStoryBoard())
                {
                    return false;
                }
            }

            currentItem = currentStoryBoard.storyBoardItems[currentItemIndex];
            resetStoryBoardItem();

            // Reset the Sequence
            if (currentItem is StoryBoardItem)
            {
                SpriteAnimationSequence sequence;
                if (!sequences.TryGetValue(storyBoardItem.sequenceName, out sequence))
                {
                    sequence = null;
                }
                resetSequence(sequence, storyBoardItem.direction);
            }

            return true;
        }

        public bool mustRepeatStoryBoardItem(StoryBoardItem storyBoardItem)
        {
            if (storyBoardItem.numberOfTimesToRepeat == -1 || numberOfTimesItemRepeated <= storyBoardItem.numberOfTimesToRepeat)
            {
                return true;
            }

            return false;
        }

        public void resetStoryBoardItem()
        {
            numberOfTimesItemRepeated = 0;
        }

        public void resetSequence(SpriteAnimationSequence sequence, SequenceDirection direction)
        {
            if (direction == SequenceDirection.Forward)
            {
                currentFrame = 0;
            }
            else
            {
                currentFrame = sequence.frames.Count - 1;
            }
        }

        public SpriteTexture2D getCurrentFrame(SpriteAnimationSequence sequence)
        {
            return sequence.frames[currentFrame];
        }

        public bool moveToNextFrame(SpriteAnimationSequence sequence, SequenceDirection direction)
        {
            if (direction == SequenceDirection.Forward)
            {
                currentFrame++;

                if (currentFrame >= sequence.frames.Count)
                {
                    currentFrame = 0;
                    return false;
                }
            }
            else
            {
                currentFrame--;

                if (currentFrame < 0)
                {
                    currentFrame = sequence.frames.Count - 1;
                    return false;
                }
            }

            return true;
        }

    }
}