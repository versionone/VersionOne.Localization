using System;
using System.Collections.Generic;

namespace VersionOne.Localization
{
	public interface ITemplateProvider
	{
		ITemplateSet Load(string culture);
	}

	public class CompatibilityTemplateProvider : ITemplateProvider
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
			return _loader.Load(culture, _setname);
		}
	}

	public class FileTemplateProvider : ITemplateProvider
	{
		public ITemplateSet Load(string culture)
		{
			throw new NotImplementedException();
		}
	}

	public class TableTemplateProvider : ITemplateProvider
	{
		public ITemplateSet Load(string culture)
		{
			throw new NotImplementedException();
		}
	}
}