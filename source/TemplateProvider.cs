using System;
using System.IO;
using System.Text;

namespace VersionOne.Localization
{
	/*
	 * Replacement for the ITemplateSetLoader to allow for an enumeration of individual source endpoints
	 * that can be mixed in any order between files and database tables.
	 */
	public interface ITemplateProvider
	{
		ITemplateSet Load(string culture);
	}

	internal class CompatibilityTemplateProvider : ITemplateProvider
	{
		private readonly ITemplateSetLoader _loader;
		private readonly string _setname;

		public CompatibilityTemplateProvider(ITemplateSetLoader loader, string setname)
		{
			_loader = loader;
			_setname = setname;
		}

		public ITemplateSet Load(string culture)
		{
			try 
			{
				return _loader.Load(culture, _setname);
			}
			catch (Exception e)
			{
				throw new TemplateSetLoadException(culture, _setname, e);
			}
		}

		private class TemplateSetLoadException : ApplicationException
		{
			public TemplateSetLoadException(string culture, string name, Exception inner)
				: base(string.Format("Faied to load \"{1}\" template set for \"{0}\" culture.", culture, name), inner) { }
		}
	}
}