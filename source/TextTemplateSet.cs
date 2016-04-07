using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace VersionOne.Localization
{
	public class TextTemplateSet : ITemplateSet, IEnumerator<Template>
	{
		private TextReader _reader;

		public TextTemplateSet (TextReader reader)
		{
			_reader = reader;
		}

		private static string NextLine (TextReader reader)
		{
			string line = reader.ReadLine();
			if (line != null)
				while (line.EndsWith("_"))
				{
					string nextline = reader.ReadLine();
					if (nextline == null)
						nextline = string.Empty;
					line = line.Substring(0, line.Length-1) + "\n" + nextline;
				}
			return line;
		}
		private static Template NextTemplate (TextReader reader)
		{
			string line;
			while ((line = NextLine(reader)) != null)
			{
				if (line.StartsWith(";")) continue;

				int eqindex = line.IndexOf('=');
				while (eqindex >= 0 && eqindex+1 < line.Length && line[eqindex+1] == '=')
					eqindex = line.IndexOf('=', eqindex+2);
				if (eqindex < 0) continue;

				string tag = line.Substring(0, eqindex).Replace("==", "=");
				string translation = line.Substring(eqindex+1);
				return new Template(tag, translation);
			}
			return null;
		}

		public Template GetNextTemplate ()
		{
			if (_reader == null) return null;
			Template t = NextTemplate(_reader);
			if (t == null)
				Dispose();
			return t;
		}

		public void Dispose ()
		{
			if (_reader != null)
			{
				((IDisposable)_reader).Dispose();
				_reader = null;
			}
		}

		public bool MoveNext()
		{
			var nextTemplate = GetNextTemplate();
			
			if (nextTemplate == null) return false;
			
			Current = nextTemplate;
			return true;
		}

		public void Reset()
		{
			var reader = _reader as StreamReader;

			if (reader == null) return;

			var streamReader = reader;
			streamReader.BaseStream.Position = 0;
			streamReader.DiscardBufferedData();
		}

		public Template Current { get; private set; }

		object IEnumerator.Current
		{
			get { return Current; }
		}
	}
}
