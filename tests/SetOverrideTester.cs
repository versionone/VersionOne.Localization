using System.Collections.Generic;
using NUnit.Framework;
using VersionOne.Localization;

namespace VersionOne.Tests.LocalizationTests
{
	public class SetOverrideTesterBase
	{
		protected Localizer loc;

		[SetUp] public void Setup ()
		{
			LocalizationManager mgr = new LocalizationManager("en", new MyTemplateSetLoader(), null, "Base");
			loc = mgr.DefaultLocalizer;
		}

		[Test] public void NotFound ()
		{
			Assert.AreEqual("bogus", loc.Resolve("bogus"));
		}

		[Test] public void Comment ()
		{
			Assert.AreEqual("Comment", loc.Resolve("Comment"));
		}

		[Test] public void Multiline ()
		{
			Assert.AreEqual("First line\nSecond line", loc.Resolve("LongText"));
		}

		[Test] public void Plural ()
		{
			Assert.AreEqual("Projects", loc.Resolve("Plural'Project"));
		}

		[Test] public void PluralStory ()
		{
			Assert.AreEqual("Stories", loc.Resolve("Plural'Story"));
		}

		[Test] public void TemplateExpansion ()
		{
			Assert.AreEqual("Delete Story", loc.Resolve("Command'Delete'Story"));
		}

		[Test] public void Fixed ()
		{
			Assert.AreEqual("Never Change", loc.Resolve("Fixed"));
		}

		[Test] public void TagWithEqualSigns ()
		{
			Assert.AreEqual("Equal-sign free", loc.Resolve("This=has=equal=signs"));
		}
	}

	public class SetOverrideTesterScrum
	{
		protected Localizer loc;

		[SetUp] public void Setup ()
		{
			LocalizationManager mgr = new LocalizationManager("en", new MyTemplateSetLoader(), null, "Base", "Scrum");
			loc = mgr.DefaultLocalizer;
		}

		[Test] public void SomeThingsDontChange ()
		{
			Assert.AreEqual("Projects", loc.Resolve("Plural'Project"));
		}

		[Test] public void Story ()
		{
			Assert.AreEqual("Backlog Item", loc.Resolve("Story"));
		}

		[Test] public void PluralStory ()
		{
			Assert.AreEqual("Backlog Items", loc.Resolve("Plural'Story"));
		}

		[Test] public void TemplateExpansion ()
		{
			Assert.AreEqual("Delete Backlog Item", loc.Resolve("Command'Delete'Story"));
		}

		[Test] public void Fixed ()
		{
			Assert.AreEqual("Never Change", loc.Resolve("Fixed"));
		}
	}

	public class SetOverrideTesterCustom
	{
		protected Localizer loc;

		[SetUp] public void Setup ()
		{
			LocalizationManager mgr = new LocalizationManager("en", new MyTemplateSetLoader(), null, "Base", "Scrum", "Custom");
			loc = mgr.DefaultLocalizer;
		}

		[Test] public void SomeThingsStillDontChange ()
		{
			Assert.AreEqual("Projects", loc.Resolve("Plural'Project"));
		}

		[Test] public void Story ()
		{
			Assert.AreEqual("PBI", loc.Resolve("Story"));
		}

		[Test] public void PluralStory ()
		{
			Assert.AreEqual("PBIs", loc.Resolve("Plural'Story"));
		}

		[Test] public void TemplateExpansion ()
		{
			Assert.AreEqual("Remove PBIs", loc.Resolve("Command'Delete'Story"));
		}

		[Test] public void Fixed ()
		{
			Assert.AreEqual("Never Change", loc.Resolve("Fixed"));
		}
	}

	public class SetGlobalOverridesTester
	{
		protected LocalizationManager mgr;
		protected Localizer loc;

		[SetUp]
		public void Setup()
		{
			IDictionary<string, string> overrides = new Dictionary<string, string>();
			overrides.Add("Story", "Bone");
			overrides.Add("DEF", "fed");

			mgr = new LocalizationManager("en", new MyTemplateSetLoader(), overrides, "Base", "Scrum", "Custom");
			loc = mgr.DefaultLocalizer;
		}

		[Test]
		public void SomeThingsStillDontChange()
		{
			Assert.AreEqual("Projects", loc.Resolve("Plural'Project"));
		}

		[Test]
		public void Story()
		{
			Assert.AreEqual("Bone", loc.Resolve("Story"));
		}

		[Test]
		public void PluralStory()
		{
			Assert.AreEqual("Bones", loc.Resolve("Plural'Story"));
		}

		[Test]
		public void TemplateExpansion()
		{
			Assert.AreEqual("Remove Bones", loc.Resolve("Command'Delete'Story"));
		}

		[Test]
		public void Fixed()
		{
			Assert.AreEqual("Never Change", loc.Resolve("Fixed"));
		}

		[Test]
		public void MoreGlobalOverrides()
		{
			Assert.AreEqual("ABC fed GHI", loc.Resolve("XYZ"));
		}

		[Test]
		public void CultureFallbackToDefault()
		{
			Assert.AreEqual("fed", mgr.GetLocalizer("fr").Resolve("DEF"));
		}

		[Test]
		public void CultureOverridesDefault()
		{
			Assert.AreEqual("PQR", mgr.GetLocalizer("fr-FR").Resolve("DEF"));
		}

		[Test]
		public void NonExistantCultureFallbackToDefault()
		{
			Assert.AreEqual("ABC fed GHI", mgr.GetLocalizer("ru").Resolve("XYZ"));
		}
	}
}
