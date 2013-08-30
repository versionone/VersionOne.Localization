using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace VersionOne.Localization
{
	public interface ILocalizerResolver
	{
		string Resolve(string tag);
	}

	public class Localizer : ILocalizerResolver
	{
		private const string SeparatorString = "\'";
		private const char SeparatorChar = '\'';
		private const char LeftBraceChar = '{';
		private const char RightBraceChar = '}';
		private const string LeftBracePlaceholderString = "\x2";
		private const char LeftBracePlaceholderChar = '\x2';
		private const string RightBracePlaceholderString = "\x3";
		private const char RightBracePlaceholderChar = '\x3';
		private static readonly Regex rxEmbeddedTag = new Regex(@"\{([^{}]*)\}", RegexOptions.Compiled);
		private static readonly Regex rxDoubleLeftBrace = new Regex(@"\{\{", RegexOptions.Compiled);
		private static readonly Regex rxDoubleRightBrace = new Regex(@"\}\}", RegexOptions.RightToLeft | RegexOptions.Compiled);

		private static string StripLiteralBraces (string input)
		{
			return rxDoubleRightBrace.Replace(rxDoubleLeftBrace.Replace(input, LeftBracePlaceholderString), RightBracePlaceholderString);
		}
		private static string ReplaceLiteralBraces (string input)
		{
			StringBuilder s = new StringBuilder(input);
			s.Replace(LeftBracePlaceholderChar, LeftBraceChar).Replace(RightBracePlaceholderChar, RightBraceChar);
			return s.ToString();
		}


		private readonly IDictionary _templates = new Hashtable {{"", ""}};
		private readonly IDictionary _resolved = Hashtable.Synchronized(new Hashtable());
		private readonly Localizer _fallback;

		public Localizer (Localizer fallback)
		{
			_fallback = fallback;
		}

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

		public void Add (string tag, string translation)
		{
			_templates[tag] = translation;
			_resolved.Clear();
		}

		public void Remove (string tag)
		{
			_templates.Remove(tag);
			_resolved.Clear();
		}

		public string Resolve (string tag)
		{
			return Resolve(tag, null);
		}

		private string Resolve(string tag, Stack<string> resolvestack)
		{
			string translation = LookupCached(tag);
			if (translation != null) return translation;

			if (resolvestack == null)
				resolvestack = new Stack<string>();

			resolvestack.Push(tag);
			string resolution = ResolveTemplate(tag, resolvestack);
			resolvestack.Pop();

			return resolution;
		}

		private string LookupCached (string tag)
		{
			return (string) _resolved[tag];
		}

		private string ResolveTemplate (string tag, Stack<string> resolvestack)
		{
			string template = FindTemplate(tag);
			if (template == null) return Cache(tag, tag.Trim());
			string translation = ExpandTemplate(template, tag, resolvestack).Trim();
			return Cache(tag, translation);
		}

		private string Cache (string tag, string translation)
		{
			return (string) (_resolved[tag] = translation);
		}

		private string FindTemplate (string templateTag)
		{
			string tag = templateTag;
			string template;
			while ( (template = LookupTemplate(tag)) == null)
			{
				int dotindex = tag.LastIndexOf(SeparatorChar);
				if (dotindex < 0) break;
				tag = tag.Substring(0, dotindex);
			}
			if (template == null && _fallback != null)
				template = _fallback.FindTemplate(templateTag);
			return template;
		}

		private string LookupTemplate (string tag)
		{
			return (string) _templates[tag];
		}

		private string ExpandTemplate (string template, string tag, Stack<string> resolvestack)
		{
			string strippedtemplate = StripLiteralBraces(template);
			string[] tagparts = tag.Split(SeparatorChar);
			return ReplaceLiteralBraces(ExpandTemplate(strippedtemplate, tagparts, resolvestack));
		}

		private string ExpandTemplate (string template, string[] tagparts, Stack<string> resolvestack)
		{
			StringBuilder expanded = new StringBuilder(template);
			for (;;)
			{
				MatchCollection matches = rxEmbeddedTag.Matches(expanded.ToString());
				if (matches.Count == 0) break;

				for (int i = matches.Count-1; i >= 0; --i)
				{
					Match m = matches[i];
					string embeddedtag = m.Groups[1].Value;
					string replacement = ResolveEmbedded(embeddedtag, tagparts, resolvestack);
					expanded.Remove(m.Index, m.Length);
					expanded.Insert(m.Index, replacement);
				}
			}
			return expanded.ToString();
		}

		private string ResolveEmbedded (string embeddedtag, string[] tagparts, Stack<string> resolvestack)
		{
			string tag = Unembed(embeddedtag, tagparts);
			return resolvestack.Contains(tag) ? tag : Resolve(tag, resolvestack);
		}

		private static string Unembed (string embeddedtag, string[] tagparts)
		{
			string[] embeds = embeddedtag.Split(SeparatorChar);
			for (int i = 0; i < embeds.Length; ++i)
			{
				int index;
				if (int.TryParse(embeds[i], out index))
				{
					embeds[i] = tagparts.Length > index ? tagparts[index] : string.Empty;
				}
			}
			return string.Join(SeparatorString, embeds);
		}
	}
}