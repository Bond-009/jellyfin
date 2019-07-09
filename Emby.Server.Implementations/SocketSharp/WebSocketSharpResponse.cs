using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Emby.Server.Implementations.SocketSharp
{
    public class WebSocketSharpResponse : IResponse
    {
        private readonly ILogger _logger;

        public WebSocketSharpResponse(ILogger logger, HttpResponse response)
        {
            _logger = logger;
            OriginalResponse = response;
        }

        public HttpResponse OriginalResponse { get; }

        public int StatusCode
        {
            get => OriginalResponse.StatusCode;
            set => OriginalResponse.StatusCode = value;
        }

        public string StatusDescription { get; set; }

        public string ContentType
        {
            get => OriginalResponse.ContentType;
            set => OriginalResponse.ContentType = value;
        }

        public void AddHeader(string name, string value)
        {
            if (string.Equals(name, "Content-Type", StringComparison.OrdinalIgnoreCase))
            {
                ContentType = value;
                return;
            }

            OriginalResponse.Headers.Add(name, value);
        }

        public void Redirect(string url)
        {
            OriginalResponse.Redirect(url);
        }

        public Stream OutputStream => OriginalResponse.Body;

        public bool SendChunked { get; set; }

        public async Task TransmitFile(
            string path,
            long offset,
            long count,
            FileShare fileShare,
            IStreamHelper streamHelper,
            CancellationToken cancellationToken)
        {
            // use non-async filestream along with read due to https://github.com/dotnet/corefx/issues/6039
            var fileOptions = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? FileOptions.SequentialScan
                : FileOptions.SequentialScan | FileOptions.Asynchronous;

            using (var fs = new FileStream(
                path,
                FileMode.Open,
                FileAccess.Read,
                fileShare,
                StreamDefaults.DefaultFileStreamBufferSize,
                fileOptions))
            {
                if (offset > 0)
                {
                    fs.Position = offset;
                }

                if (count > 0)
                {
                    await streamHelper.CopyToAsync(fs, OutputStream, count, cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    await fs.CopyToAsync(OutputStream, StreamDefaults.DefaultCopyToBufferSize, cancellationToken).ConfigureAwait(false);
                }
            }
        }
    }
}
