using System;
using System.Text;

namespace FontStashSharp
{
	internal ref struct TextSource
	{
		public ReadOnlySpan<char> StringSpan;
		public StringSegment StringText;
		public StringBuilder StringBuilderText;
		private int Position;

		public TextSource(string text)
		{
			StringText = new StringSegment(text);
			StringBuilderText = null;
			StringSpan = null;
			Position = 0;
		}

		public TextSource(StringSegment text)
		{
			StringText = text;
			StringBuilderText = null;
			StringSpan = null;
			Position = 0;
		}

		public TextSource(StringBuilder text)
		{
			StringText = new StringSegment();
			StringBuilderText = text;
			StringSpan = null;
			Position = 0;
		}

		public TextSource(ReadOnlySpan<char> text)
		{
			StringSpan = text;
			StringText = new StringSegment("");
			StringBuilderText = null;
			Position = 0;
		}

		public bool IsNull => StringText.IsNullOrEmpty && StringBuilderText == null && StringSpan == null;

		public bool GetNextCodepoint(out int result)
		{
			result = 0;

			if (!StringText.IsNullOrEmpty)
			{
				if (Position >= StringText.Length)
				{
					return false;
				}

				result = char.ConvertToUtf32(StringText.String, StringText.Offset + Position);
				Position += char.IsSurrogatePair(StringText.String, StringText.Offset + Position) ? 2 : 1;
				return true;
			}

			if (StringBuilderText != null)
			{
				if (Position >= StringBuilderText.Length)
				{
					return false;
				}

				result = StringBuilderConvertToUtf32(StringBuilderText, Position);
				Position += StringBuilderIsSurrogatePair(StringBuilderText, Position) ? 2 : 1;
				return true;
			}

			if (StringSpan != null)
			{
				if (Position >= StringSpan.Length)
				{
					return false;
				}

				result = SpanConvertToUtf32(in StringSpan, Position);
				Position += SpanIsSurrogatePair(in StringSpan, Position) ? 2 : 1;
				return true;
			}

			return false;
		}

		public void Reset()
		{
			Position = 0;
		}

		private static bool StringBuilderIsSurrogatePair(StringBuilder sb, int index)
		{
			if (index + 1 < sb.Length)
				return char.IsSurrogatePair(sb[index], sb[index + 1]);
			return false;
		}

		private static int StringBuilderConvertToUtf32(StringBuilder sb, int index)
		{
			if (!char.IsHighSurrogate(sb[index]))
				return sb[index];

			return char.ConvertToUtf32(sb[index], sb[index + 1]);
		}
		private static bool SpanIsSurrogatePair(in ReadOnlySpan<char> span, int index)
		{
			if (index + 1 < span.Length)
				return char.IsSurrogatePair(span[index], span[index + 1]);
			return false;
		}
		private static int SpanConvertToUtf32(in ReadOnlySpan<char> span, int index)
		{
			if (!char.IsHighSurrogate(span[index]))
				return span[index];
			return char.ConvertToUtf32(span[index], span[index + 1]);
		}

		public static int CalculateLength(string text)
		{
			if (string.IsNullOrEmpty(text))
			{
				return 0;
			}

			var pos = 0;
			var result = 0;
			while(pos < text.Length)
			{
				pos += char.IsSurrogatePair(text, pos) ? 2 : 1;
				++result;
			}

			return result;
		}
	}
}
