namespace Blade.MG.UI.Models
{
    public class Style<T>
    {
        public string Property { get; private set; }
        //public Range[] Ranges { get; private set; }
        public string[] Parts { get; private set; }

        public Style(string property)
        {
            this.Property = property;

            int j = 0;
            int index = 0;

            int count = property.Count(p => p == '.');

            Parts = new string[count + 1];
            Parts[0] = property;

            while ((index = property.IndexOf('.', index)) >= 0)
            {
                index++;
                Parts[++j] = Property[index..];
            }

            //Ranges = new Range[count + 1];
            //Ranges[0] = new Range(0, Property.Length);

            //while ((index = property.IndexOf('.', index + 1)) >= 0)
            //{
            //    Ranges[++j] = new Range(index + 1, Property.Length);
            //}

            // var s = Property.AsSpan()[Ranges[0]];
        }
    }
}
