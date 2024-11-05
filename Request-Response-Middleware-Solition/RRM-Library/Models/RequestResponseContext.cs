namespace RRM_Library.Models
{
    public class RequestResponseContext
    {
        internal readonly HttpContext _context;
        public RequestResponseContext(HttpContext context)
        {
            this._context = context;
        }

        public string? Request { get; set; }
        public string? Response { get; set; }
        public int? RequestLength => Request?.Length;
        public int? ResponseLength => Response?.Length;
        [JsonIgnore]
        public TimeSpan RequestTime { get; set; }
        public Uri Url => BuildUrl();

        // Build the URL from the request
        internal Uri BuildUrl()
        {
            return new Uri($"{_context.Request.Scheme}://{_context.Request.Host}{_context.Request.Path}{_context.Request.QueryString}", UriKind.RelativeOrAbsolute);
        }

        // Format the request time
        public string FormatedRequestTime => RequestTime == TimeSpan.Zero ? "00:00.000" : RequestTime.ToString(@"mm\:ss\.fff");
    }
}
