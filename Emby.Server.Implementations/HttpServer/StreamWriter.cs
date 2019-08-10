using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.Services;
using Microsoft.Net.Http.Headers;

namespace Emby.Server.Implementations.HttpServer
{
    /// <summary>
    /// Class StreamWriter
    /// </summary>
    public class StreamWriter : IAsyncStreamWriter, IHasHeaders
    {
        /// <summary>
        /// Gets or sets the source stream.
        /// </summary>
        /// <value>The source stream.</value>
        private Stream _sourceStream;

        private byte[] _sourceBytes;

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamWriter" /> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="contentType">Type of the content.</param>
        /// <param name="logger">The logger.</param>
        public StreamWriter(Stream source, string contentType)
        {
            if (string.IsNullOrEmpty(contentType))
            {
                throw new ArgumentNullException(nameof(contentType));
            }

            _sourceStream = source;

            Headers["Content-Type"] = contentType;

            if (source.CanSeek)
            {
                Headers[HeaderNames.ContentLength] = source.Length.ToString(CultureInfo.InvariantCulture);
            }

            Headers[HeaderNames.ContentType] = contentType;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamWriter"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="contentType">Type of the content.</param>
        public StreamWriter(byte[] source, string contentType, int contentLength)
        {
            if (string.IsNullOrEmpty(contentType))
            {
                throw new ArgumentNullException(nameof(contentType));
            }

            _sourceBytes = source;

            Headers[HeaderNames.ContentLength] = contentLength.ToString(CultureInfo.InvariantCulture);
            Headers[HeaderNames.ContentType] = contentType;
        }

        /// <summary>
        /// Gets the options.
        /// </summary>
        /// <value>The options.</value>
        public IDictionary<string, string> Headers { get; } = new Dictionary<string, string>();

        public Action OnComplete { get; set; }
        public Action OnError { get; set; }

        public async Task WriteToAsync(Stream responseStream, CancellationToken cancellationToken)
        {
            try
            {
                var bytes = _sourceBytes;

                if (bytes != null)
                {
                    await responseStream.WriteAsync(bytes, 0, bytes.Length, cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    using (var src = _sourceStream)
                    {
                        await src.CopyToAsync(responseStream, StreamDefaults.CopyToBufferSize, cancellationToken).ConfigureAwait(false);
                    }
                }
            }
            catch
            {
                OnError?.Invoke();

                throw;
            }
            finally
            {
                OnComplete?.Invoke();
            }
        }
    }
}
