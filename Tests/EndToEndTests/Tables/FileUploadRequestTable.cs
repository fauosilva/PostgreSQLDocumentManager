namespace EndToEndTests.Tables
{
    public class FileUploadRequestTable
    {
        public string FileName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string MimeType { get; set; } = string.Empty;


        public MultipartFormDataContent ToRequest()
        {
            var streamContent = new StreamContent(new MemoryStream(File.ReadAllBytes(FileName)));
            streamContent.Headers.Add("Content-Type", MimeType);
            streamContent.Headers.Add("Content-Disposition", $"form-data; name=\"{Name}\"; filename=\"{FileName}\"");

            var nameContent = new StringContent(Name);
            nameContent.Headers.Add("Content-Disposition", $"form-data; name=\"Name\"");
            var descriptionContent = new StringContent(Description + new Random().Next().ToString());
            descriptionContent.Headers.Add("Content-Disposition", $"form-data; name=\"Description\"");
            var categoryContent = new StringContent(Category);
            categoryContent.Headers.Add("Content-Disposition", $"form-data; name=\"Category\"");

            var multiPartFormData = new MultipartFormDataContent("------")
            {
                nameContent,
                descriptionContent,
                categoryContent,
                streamContent
            };

            return multiPartFormData;
        }
    }
}
