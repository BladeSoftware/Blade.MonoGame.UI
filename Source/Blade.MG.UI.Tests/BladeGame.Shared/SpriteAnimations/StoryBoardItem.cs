
namespace BladeGame.SpriteAnimations
{
    public class StoryBoardItem : StoryBoardItemBase
    {
        public string sequenceName;
        public int numberOfTimesToRepeat;
        public SequenceDirection direction;

        public StoryBoardItem(string sequenceName, int numberOfTimesToRepeat, SequenceDirection direction)
        {
            this.sequenceName = sequenceName;
            this.numberOfTimesToRepeat = numberOfTimesToRepeat;
            this.direction = direction;
        }

    }
}
