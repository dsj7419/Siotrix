namespace GynBot.Extensions
{
    #region Using

    using System;
    using System.IO;
    using System.Net.Http;
    using System.Threading.Tasks;

    #endregion

    public static class HttpClientExtensions
    {
        #region Public Methods

        public static async Task DownloadAsync(this HttpClient client, Uri requestUri, string filename)
        {
            using (client = new HttpClient())
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, requestUri))
                {
                    using (
                        Stream contentStream = await (await client.SendAsync(request)).Content.ReadAsStreamAsync(),
                               stream = new FileStream
                                   (filename, FileMode.Create, FileAccess.Write, FileShare.None, 3145728, true))
                    {
                        await contentStream.CopyToAsync(stream).ConfigureAwait(false);
                    }
                }
            }
        }

        #endregion Public Methods
    }
}