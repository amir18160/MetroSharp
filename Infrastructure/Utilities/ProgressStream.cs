using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Utilities
{
    public class ProgressStream : Stream
    {
        private readonly Stream _baseStream;
        private readonly Action<long, long> _progressCallback;
        private long _totalBytesRead;

        public ProgressStream(Stream baseStream, Action<long, long> progressCallback)
        {
            _baseStream = baseStream;
            _progressCallback = progressCallback;
            _totalBytesRead = 0;
        }

        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            int bytesRead = await _baseStream.ReadAsync(buffer, cancellationToken);
            _totalBytesRead += bytesRead;
            _progressCallback?.Invoke(_totalBytesRead, _baseStream.Length);
            return bytesRead;
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return ReadAsync(buffer.AsMemory(offset, count), cancellationToken).AsTask();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int bytesRead = _baseStream.Read(buffer, offset, count);
            _totalBytesRead += bytesRead;
            _progressCallback?.Invoke(_totalBytesRead, _baseStream.Length);
            return bytesRead;
        }

        public override bool CanRead => _baseStream.CanRead;
        public override bool CanSeek => _baseStream.CanSeek;
        public override bool CanWrite => false;
        public override long Length => _baseStream.Length;

        public override long Position
        {
            get => _baseStream.Position;
            set => _baseStream.Position = value;
        }

        public override void Flush() => _baseStream.Flush();
        public override long Seek(long offset, SeekOrigin origin) => _baseStream.Seek(offset, origin);
        public override void SetLength(long value) => _baseStream.SetLength(value);
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }

}