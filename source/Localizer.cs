using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace VersionOne.Localization
{
	public interface ILocalizerResolver
	{
		string Resolve(string tag);

		Dictionary<string, string> GetTemplateDictionary();
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

		public Dictionary<string, string> GetTemplateDictionary()
		{
			Dictionary<string, string> merged = new Dictionary<string, string>();
			if (_fallback != null)
			{
				_fallback.GetTemplateDictionary().ToList().ForEach(
					kvPair => merged.Add(kvPair.Key, kvPair.Value));
			}
			foreach (DictionaryEntry entry in _templates)
			{
				merged[(string) entry.Key] = (string) entry.Value;
			}
			return merged;
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