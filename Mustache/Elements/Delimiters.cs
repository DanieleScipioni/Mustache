namespace Mustache.Elements
{
    public class Delimiters : Tag
    {
        public readonly string OpenDelimiter;
        public readonly string CloseDelimiter;

        public Delimiters(string key, string openDelimiter, string closeDelimiter) : base(key)
        {
            OpenDelimiter = openDelimiter;
            CloseDelimiter = closeDelimiter;
        }

        internal override void Accept(IElementRenderer renderer)
        {
            renderer.Render(this);
        }
    }
}