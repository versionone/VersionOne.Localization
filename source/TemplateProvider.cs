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

	public class CompatibilityTemplateProvider : ITemplateProvider
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

	public class FileTemplateProvider : ITemplateProvider
	{
		private readonly string _path;

		public FileTemplateProvider(string path, string name)
		{
			_path = path;
			Name = name;
		}

		public ITemplateSet Load(string culture)
		{
			var filename = Path.Combine(_path, string.Format("{0}.{1}.txt", culture, Name));
			if (!File.Exists(filename))
				return null;
			TextReader reader = new StreamReader(filename, new UTF8Encoding(false, true));
			return new TextTemplateSet(reader);
		}

		public string Name { get; private set; }
	}
}