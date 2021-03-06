using System.IO;
using System.Text;

namespace VersionOne.Localization
{
	public class FileTemplateSetLoader : ITemplateSetLoader
	{
		private readonly string _path;

		public FileTemplateSetLoader (string path)
		{
			_path = path;
		}

		public ITemplateSet Load(string culture, string setname)
		{
			string filename = Path.Combine(_path, string.Format("{0}.{1}.txt", culture, setname));
			if (!File.Exists(filename))
				return null;
			TextReader reader = new StreamReader(filename, new UTF8Encoding(false, true));
			return new TextTemplateSet(reader);
		}
	}
}
