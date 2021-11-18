namespace MockHttp
{
	internal class CanSeekMemoryStream : MemoryStream
	{
		public CanSeekMemoryStream(byte[] buffer, bool isSeekable)
			: base(buffer, 0, buffer.Length)
		{
			CanSeek = isSeekable;
		}

		public override bool CanSeek
		{
			get;
		}
	}
}
