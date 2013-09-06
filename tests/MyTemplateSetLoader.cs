using System;
using System.Resources;
using NUnit.Framework;
using VersionOne.Localization;

namespace VersionOne.Tests.LocalizationTests
{
	internal class MyTemplateSetLoader : ITemplateSetLoader
	{
		public ITemplateSet Load(string culture, string setname)
		{
			string resourcename = string.Format("Strings.{0}.{1}.txt", culture, setname);
			if (!ResourceLoader.Exists(resourcename, GetType()))
				return null;
			System.IO.TextReader reader = ResourceLoader.LoadClassText(resourcename, GetType());
			return new TextTemplateSet(reader);
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
