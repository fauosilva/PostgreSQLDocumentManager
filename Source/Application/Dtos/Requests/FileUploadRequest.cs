namespace ApplicationCore.Dtos.Requests
{
    //Todo: Improve validation to use proper validation model
    public class FileUploadRequest
    {
        public string? Name { get; private set; }
        public string? Description { get; private set; }
        public string? Category { get; private set; }

        public bool IsValid()
        {
            return !string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(Description) && !string.IsNullOrEmpty(Category);
        }

        public void AddMetadata(string key, string value)
        {
            if (string.Equals(key, nameof(Name), StringComparison.InvariantCultureIgnoreCase))
            {
                Name = value;
            }

            if (string.Equals(key, nameof(Description), StringComparison.InvariantCultureIgnoreCase))
            {
                Description = value;
            }

            if (string.Equals(key, nameof(Category), StringComparison.InvariantCultureIgnoreCase))
            {
                Category = value;
            }
        }

        public string GetKeyName()
        {
            return DateTime.UtcNow.ToString("yyyyMMddHHmmssfff") + Name;
        }
    }
}
