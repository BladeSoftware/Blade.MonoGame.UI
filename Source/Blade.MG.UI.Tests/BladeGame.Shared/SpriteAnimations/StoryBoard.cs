using System.Collections.Generic;

namespace BladeGame.SpriteAnimations
{
    public enum SequenceDirection
    {
        Forward, Reverse
    }

    public class StoryBoard
    {

        public string storyBoardName;
        public List<StoryBoardItemBase> storyBoardItems = new List<StoryBoardItemBase>();
        public int numberOfTimesToRepeat = 0; // 0 = Play Once, 1/2/3.. = Repeat 1/2/3 times, -1 = Repeat Until Stopped

        public StoryBoard(string storyBoardName)
        {
            this.storyBoardName = storyBoardName;
        }

        public void addItem(string sequenceName, int numberOfTimesToRepeat)
        {
            StoryBoardItem item = new StoryBoardItem(sequenceName, numberOfTimesToRepeat, SequenceDirection.Forward);
            storyBoardItems.Add(item);
        }

        public void addItem(string sequenceName, int numberOfTimesToRepeat, SequenceDirection direction)
        {
            StoryBoardItem item = new StoryBoardItem(sequenceName, numberOfTimesToRepeat, direction);
            storyBoardItems.Add(item);
        }

        public void addItem(SpriteAnimationSequence sequence, int numberOfTimesToRepeat)
        {
            StoryBoardItem item = new StoryBoardItem(sequence.SequenceName, numberOfTimesToRepeat, SequenceDirection.Forward);
            storyBoardItems.Add(item);
        }

        public void addItem(SpriteAnimationSequence sequence, int numberOfTimesToRepeat, SequenceDirection direction)
        {
            StoryBoardItem item = new StoryBoardItem(sequence.SequenceName, numberOfTimesToRepeat, direction);
            storyBoardItems.Add(item);
        }

        public void addRedirect(string newStoryBoard)
        {
            StoryBoardItemRedirect item = new StoryBoardItemRedirect(newStoryBoard);
            storyBoardItems.Add(item);
        }
    }
}