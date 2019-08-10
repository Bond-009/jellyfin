using System;
using System.Buffers;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.Services;
using Microsoft.Net.Http.Headers;

namespace Emby.Server.Implementations.HttpServer
{
    public class RangeRequestWriter : IAsyncStreamWriter, IHttpResult
    {
        /// <summary>
        /// Gets or sets the source stream.
        /// </summary>
        /// <value>The source stream.</value>
        private Stream _sourceStream;
        private string _rangeHeader;
        private bool _isHeadRequest;

        private long _rangeStart;
        private long _rangeEnd;
        private long _rangeLength;
        private long _totalContentLength;

        /// <summary>
        /// The _requested ranges.
        /// </summary>
        private List<KeyValuePair<long, long?>> _requestedRanges;

        /// <summary>
        /// Initializes a new instance of the <see cref="RangeRequestWriter" /> class.
        /// </summary>
        /// <param name="rangeHeader">The range header.</param>
        /// <param name="contentLength">The content length.</param>
        /// <param name="source">The source.</param>
        /// <param name="contentType">Type of the content.</param>
        /// <param name="isHeadRequest">if set to <c>true</c> [is head request].</param>
        public RangeRequestWriter(
            string rangeHeader,
            long contentLength,
            Stream source,
            string contentType,
            bool isHeadRequest)
        {
            if (string.IsNullOrEmpty(contentType))
            {
                throw new ArgumentNullException(nameof(contentType));
            }

            _rangeHeader = rangeHeader;
            _sourceStream = source;
            _isHeadRequest = isHeadRequest;

            ContentType = contentType;
            Headers[HeaderNames.ContentType] = contentType;
            Headers[HeaderNames.AcceptRanges] = "bytes";
            StatusCode = HttpStatusCode.PartialContent;

            SetRangeValues(contentLength);
        }

        public Action OnComplete { get; set; }

        /// <summary>
        /// Gets the additional HTTP Headers.
        /// </summary>
        /// <value>The headers.</value>
        public IDictionary<string, string> Headers { get; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets the requested ranges.
        /// </summary>
        /// <value>The requested ranges.</value>
        protected List<KeyValuePair<long, long?>> RequestedRanges
        {
            get
            {
                if (_requestedRanges == null)
                {
                    _requestedRanges = new List<KeyValuePair<long, long?>>();

                    // Example: bytes=0-,32-63
                    var ranges = _rangeHeader.Split('=')[1].Split(',');

                    foreach (var range in ranges)
                    {
                        var vals = range.Split('-');

                        long start = 0;
                        long? end = null;

                        if (!string.IsNullOrEmpty(vals[0]))
                        {
                            start = long.Parse(vals[0], CultureInfo.InvariantCulture);
                        }

                        if (!string.IsNullOrEmpty(vals[1]))
                        {
                            end = long.Parse(vals[1], CultureInfo.InvariantCulture);
                        }

                        _requestedRanges.Add(new KeyValuePair<long, long?>(start, end));
                    }
                }

                return _requestedRanges;
            }
        }

        public string ContentType { get; set; }

        public IRequest RequestContext { get; set; }

        public object Response { get; set; }

        public int Status { get; set; }

        public HttpStatusCode StatusCode
        {
            get => (HttpStatusCode)Status;
            set => Status = (int)value;
        }

        public async Task WriteToAsync(Stream responseStream, CancellationToken cancellationToken)
        {
            try
            {
                // Headers only
                if (_isHeadRequest)
                {
                    return;
                }

                using (var source = _sourceStream)
                {
                    // If the requested range is "0-", we can optimize by just doing a stream copy
                    if (_rangeEnd >= _totalContentLength - 1)
                    {
                        await source.CopyToAsync(responseStream, StreamDefaults.CopyToBufferSize, cancellationToken).ConfigureAwait(false);
                    }
                    else
                    {
                        await CopyToInternalAsync(source, responseStream, _rangeLength, cancellationToken).ConfigureAwait(false);
                    }
                }
            }
            finally
            {
                OnComplete?.Invoke();
            }
        }

        /// <summary>
        /// Sets the range values.
        /// </summary>
        private void SetRangeValues(long contentLength)
        {
            var requestedRange = RequestedRanges[0];

            _totalContentLength = contentLength;

            // If the requested range is "0-", we can optimize by just doing a stream copy
            if (!requestedRange.Value.HasValue)
            {
                _rangeEnd = _totalContentLength - 1;
            }
            else
            {
                _rangeEnd = requestedRange.Value.Value;
            }

            _rangeStart = requestedRange.Key;
            _rangeLength = 1 + _rangeEnd - _rangeStart;

            Headers[HeaderNames.ContentLength] = _rangeLength.ToString(CultureInfo.InvariantCulture);
            Headers[HeaderNames.ContentRange] = $"bytes {_rangeStart}-{_rangeEnd}/{_totalContentLength}";

            if (_rangeStart > 0 && _sourceStream.CanSeek)
            {
                _sourceStream.Position = _rangeStart;
            }
        }

        private static async Task CopyToInternalAsync(Stream source, Stream destination, long copyLength, CancellationToken cancellationToken)
        {
            byte[] buffer = ArrayPool<byte>.Shared.Rent(StreamDefaults.CopyToBufferSize);
            try
            {
                int bytesRead;
                while ((bytesRead = await source.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false)) != 0)
                {
                    if (bytesRead == 0)
                    {
                        break;
                    }

                    var bytesToCopy = Math.Min(bytesRead, copyLength);

                    await destination.WriteAsync(buffer, 0, Convert.ToInt32(bytesToCopy), cancellationToken).ConfigureAwait(false);

                    copyLength -= bytesToCopy;

                    if (copyLength <= 0)
                    {
                        break;
                    }
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }
    }
}
