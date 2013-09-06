using System.IO;
using NUnit.Framework;

namespace VersionOne.Localization.Tests
{
	internal class MyTemplateSetLoader : ITemplateSetLoader
	{
		public ITemplateSet Load(string culture, string setname)
		{
			var classtype = GetType();
			string resourcename = string.Format("{2}.Strings.{0}.{1}.txt", culture, setname, classtype.Namespace);
			return classtype.Assembly.GetManifestResourceInfo(resourcename) == null ? null :
				new TextTemplateSet(new StreamReader(classtype.Assembly.GetManifestResourceStream(resourcename)));
		}
	}

	public class MyTemplateSetTester
	{
		[Test] public void GetNonExistTemplateSet ()
		{
			ITemplateSetLoader loader = new MyTemplateSetLoader();
			using (ITemplateSet templates = loader.Load("af", "Base"))
				Assert.IsNull(templates);
		}
		[Test] public void GeExistTemplateSet ()
		{
			ITemplateSetLoader loader = new MyTemplateSetLoader();
			using (ITemplateSet templates = loader.Load("en", "Base"))
				Assert.IsNotNull(templates);
		}
		[Test] public void GetEmptyTemplateSet ()
		{
			ITemplateSetLoader loader = new MyTemplateSetLoader();
			using (ITemplateSet templates = loader.Load("en", "Empty"))
				Assert.IsNull(templates.GetNextTemplate());
		}
		[Test] public void GetNonEmptyTemplateSet ()
		{
			ITemplateSetLoader loader = new MyTemplateSetLoader();
			using (ITemplateSet templates = loader.Load("en", "Base"))
				Assert.AreEqual("LongText", templates.GetNextTemplate().Tag);
		}
	}
}
