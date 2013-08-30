using System;

namespace VersionOne.Localization
{
	public interface ITemplateSetLoader
	{
		ITemplateSet Load (string culture, string setname);
	}

	public interface ITemplateSet : IDisposable
	{
		Template GetNextTemplate ();
	}

	public class Template
	{
		public readonly string Tag;
		public readonly string Translation;

		public Template (string tag, string translation)
		{
			Tag = tag;
			Translation = translation;
		}
	}

}