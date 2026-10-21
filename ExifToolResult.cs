namespace ExifToolUI
{
    internal class ExifToolResult
    {
        public List<KeyValuePair<string, string>> Tags { get; set; }
        public string Error { get; set; }

        public ExifToolResult(List<KeyValuePair<string, string>> tags, string error) => (Tags, Error) = (tags, error);
    }
}
