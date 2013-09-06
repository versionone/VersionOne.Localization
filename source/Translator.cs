using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace VersionOne.Localization
{
	public class Translator
	{
		public static string Translate(string input, ILocalizerResolver resolver, string startTag, string endTag)
		{
			StringBuilder builder = new StringBuilder();
			using (StringReader reader = new StringReader(input))
			using (StringWriter writer = new StringWriter(builder))
				Translate(reader, writer, resolver, startTag, endTag);
			return builder.ToString();
		}

		public static void Translate(TextReader input, TextWriter output, ILocalizerResolver resolver, string startTag, string endTag)
		{
			if (startTag.Length != 2)
				throw new ApplicationException("Start Tag must be 2 characters");
			if (endTag.Length != 1)
				throw new ApplicationException("End Tag must be 1 character");

			char[] buffer = new char[64];

			for (;;)
			{
				int c = input.Read();
				if (c == -1)
					break;
				if (c != startTag[0] || input.Peek() != startTag[1])
					output.Write((char)c);
				else
				{
					input.Read(); //consume the 2nd char of the startTag

					//build the word we want to resolve
					//if we hit EOF then it's just like we hit the endTag
					// if we hit the endTag, resolve the word and write it to output
					int pos = 0;
					for (; ; )
					{
						c = input.Read();
						if (c == -1 || c == endTag[0]) //end of file is just like endTag or end tag found
							break;

						if (pos == buffer.Length)
							Array.Resize(ref buffer, buffer.Length * 2);
						buffer[pos++] = (char)c;
					}

					//resolve the word and write it to the output
					string word = new string(buffer, 0, pos);
					string result = resolver.Resolve(word);
					output.Write(result);
				}
			}
		}

		public static IEnumerable<string> DetermineKeys(string input, string startTag, string endTag)
		{
			using (StringReader sr = new StringReader(input))
				return DetermineKeys(sr, startTag, endTag);
		}

		private static IEnumerable<string> DetermineKeys(TextReader input, string startTag, string endTag)
		{
			if (startTag.Length != 2)
				throw new ApplicationException("Start Tag must be 2 characters");
			if (endTag.Length != 1)
				throw new ApplicationException("End Tag must be 1 character");

			IList<string> results = new List<string>();

			char[] buffer = new char[64];

			for (; ; )
			{
				int c = input.Read();
				if (c == -1)
					break;
				if (c == startTag[0] && input.Peek() == startTag[1])
				{
					input.Read(); //consume the 2nd char of the startTag

					//build the word we want to resolve
					//if we hit EOF then it's just like we hit the endTag
					// if we hit the endTag, add the word to the result list
					int pos = 0;
					for (;;)
					{
						c = input.Read();
						if (c == -1 || c == endTag[0]) //end of file is just like endTag or end tag found
							break;

						if (pos == buffer.Length)
							Array.Resize(ref buffer, buffer.Length * 2);
						buffer[pos++] = (char)c;
					}

					//add the word to the results list of found keys
					results.Add(new string(buffer, 0, pos));
				}
			}

			return results;
		}
	}
}