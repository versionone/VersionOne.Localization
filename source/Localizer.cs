using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace VersionOne.Localization
{
	public interface ILocalizerResolver
	{
		string Resolve(string tag);

		string Signature { get; }

		TemplateStack GetTemplateStack();
	}

	public class TemplateStack
	{
		public IDictionary<string, string> Templates { get; set; }
		public TemplateStack FallbackStack { get; set; }
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
		private string _signature;
		public Localizer (Localizer fallback)
		{
			_fallback = fallback;
		}

		public TemplateStack GetTemplateStack()
		{
			Dictionary<string, string> templates = new Dictionary<string, string>();
			foreach (DictionaryEntry entry in _templates)
			{
				templates[(string) entry.Key] = (string) entry.Value;
			}

			return new TemplateStack {Templates = templates, FallbackStack = _fallback != null ? _fallback.GetTemplateStack() : null};
		}

		static readonly byte[] _valueSeparator = new byte[] { 0x01 };
		static readonly byte[] _recordSeparator = new byte[] { 0x02 };

		public string Signature
		{
			get
			{
				return _signature ?? (_signature = Convert.ToBase64String(ComputeSignature()));
			}
		}

		private byte[] ComputeSignature()
		{
			using (SHA1CryptoServiceProvider sha1 = new SHA1CryptoServiceProvider())
			{
				foreach (DictionaryEntry entry in _templates)
				{
					var keyBytes = Encoding.UTF8.GetBytes((string) entry.Key);
					var valueBytes = Encoding.UTF8.GetBytes((string) entry.Value);
					sha1.TransformBlock(keyBytes, 0, keyBytes.Length, keyBytes, 0);
					sha1.TransformBlock(_valueSeparator, 0, _valueSeparator.Length, _valueSeparator, 0);
					sha1.TransformBlock(valueBytes, 0, valueBytes.Length, valueBytes, 0);
					sha1.TransformBlock(_recordSeparator, 0, _recordSeparator.Length, _recordSeparator, 0);
				}

				var fallbackSignatureBytes = _fallback != null ? _fallback.ComputeSignature() : new byte[0];
				sha1.TransformFinalBlock(fallbackSignatureBytes, 0, fallbackSignatureBytes.Length);

				return sha1.Hash;
			}
		}

		public void Add (string tag, string translation)
		{
			_templates[tag] = translation;
			_resolved.Clear();
			_signature = null;
		}

		public void Remove (string tag)
		{
			_templates.Remove(tag);
			_resolved.Clear();
			_signature = null;
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