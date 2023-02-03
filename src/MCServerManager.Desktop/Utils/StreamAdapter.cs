namespace MCServerManager.Desktop.Utils;
public class StreamAdapter : Stream
{

	readonly Stream _stream;

	public StreamAdapter(Stream stream)
	{
		_stream = stream;
	}

	public override void Flush()
	{
		_stream.Flush();
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		return _stream.ReadAsync(buffer, offset, count).Result;
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		return _stream.Seek(offset, origin);
	}

	public override void SetLength(long value)
	{
		_stream.SetLength(value);
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		_stream.Write(buffer, offset, count);
	}

	public override bool CanRead => _stream.CanRead;

	public override bool CanSeek => _stream.CanSeek;

	public override bool CanWrite => _stream.CanWrite;

	public override long Length => _stream.Length;

	public override long Position
	{
		get => _stream.Position;
		set => _stream.Position = value;
	}
}