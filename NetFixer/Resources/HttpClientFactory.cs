namespace NetFixer.Resources
{
    public static class HttpClientFactory
    {
        public static HttpClient Create()
        {
            return new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(10)
            };
        }
    }
}
