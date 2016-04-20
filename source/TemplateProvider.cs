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

		string Name { get; }
	}

	internal class CompatibilityTemplateProvider : ITemplateProvider
	{
		private readonly ITemplateSetLoader _loader;

		public CompatibilityTemplateProvider(ITemplateSetLoader loader, string setname)
		{
			_loader = loader;
			Name = setname;
		}

		public ITemplateSet Load(string culture)
		{
			return _loader.Load(culture, Name);
		}

		public string Name { get; private set; }
	}
}