namespace ApplicationCore.Dtos.Responses
{
    public record DownloadDocumentResponse
    {
        public DownloadDocumentResponse(Stream filestream, string name, string contenType)
        {
            Filestream = filestream;
            Name = name;
            ContentType = contenType;
        }

        public Stream Filestream { get; set; }
        public string Name { get; set; }
        public string ContentType { get; set; }
    }
}
