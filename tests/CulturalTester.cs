using System.Globalization;
using NUnit.Framework;

namespace VersionOne.Localization.Tests
{
	public class CulturalTester
	{
		private LocalizationManager mgr;

		[TestFixtureSetUp] public void Setup ()
		{
			mgr = new LocalizationManager("en", new MyTemplateSetLoader(), null, "Base", "Scrum", "Custom");
		}

		[Test] public void LoadInvariantCulture ()
		{
			Localizer loc = mgr.GetLocalizer(CultureInfo.InvariantCulture);
			Assert.IsNotNull(loc);
			Assert.AreEqual(mgr.GetLocalizer("en"), loc);
		}

		[Test] public void LoadDefaultCulture ()
		{
			Localizer loc = mgr.GetLocalizer("en");
			Assert.IsNotNull(loc);
			Assert.AreEqual(mgr.GetLocalizer("en"), loc);
		}

		[Test] public void LoadNonexistNeutralCulture ()
		{
			Localizer loc = mgr.GetLocalizer("de");
			Assert.IsNotNull(loc);
			Assert.AreEqual(mgr.GetLocalizer("en"), loc);
		}

		[Test] public void LoadNonexistSpecificCulture ()
		{
			Localizer loc = mgr.GetLocalizer("de-AT");
			Assert.IsNotNull(loc);
			Assert.AreEqual(mgr.GetLocalizer("en"), loc);
		}

		[Test] public void LoadNonexistDefaultSpecificCulture ()
		{
			Localizer loc = mgr.GetLocalizer("en-ZW");
			Assert.IsNotNull(loc);
			Assert.AreEqual(mgr.GetLocalizer("en"), loc);
		}

		[Test] public void LoadNonexistGenericSpecificCulture ()
		{
			Localizer loc = mgr.GetLocalizer("fr-CH");
			Assert.IsNotNull(loc);
			Assert.AreEqual(mgr.GetLocalizer("fr"), loc);
		}

		[Test] public void LoadSpecificCulture ()
		{
			Localizer loc = mgr.GetLocalizer("fr-FR");
			Assert.IsNotNull(loc);
		}

		[Test] public void DefaultFromSpecific ()
		{
			Assert.AreEqual("Default English", mgr.GetLocalizer("fr-FR").Resolve("TestDefault"));
		}

		[Test] public void NeutralFromSpecific ()
		{
			Assert.AreEqual("Neutral French", mgr.GetLocalizer("fr-FR").Resolve("TestNeutral"));
		}

		[Test] public void SpecificFromSpecific ()
		{
			Assert.AreEqual("Specific Fance French", mgr.GetLocalizer("fr-FR").Resolve("TestSpecific"));
		}

		[Test] public void DefaultFromNeutral ()
		{
			Assert.AreEqual("Default English", mgr.GetLocalizer("fr").Resolve("TestDefault"));
		}

		[Test] public void NeutralFromNeutral ()
		{
			Assert.AreEqual("Neutral French", mgr.GetLocalizer("fr").Resolve("TestNeutral"));
		}

		[Test] public void SpecificFromNeutral ()
		{
			Assert.AreEqual("Specific French", mgr.GetLocalizer("fr").Resolve("TestSpecific"));
		}

		[Test] public void DefaultFromDefault ()
		{
			Assert.AreEqual("Default English", mgr.GetLocalizer("en").Resolve("TestDefault"));
		}

		[Test] public void NeutralFromDefault ()
		{
			Assert.AreEqual("Neutral English", mgr.GetLocalizer("en").Resolve("TestNeutral"));
		}

		[Test] public void SpecificFromDefault ()
		{
			Assert.AreEqual("Specific English", mgr.GetLocalizer("en").Resolve("TestSpecific"));
		}

		[Test] public void DefaultFromUS ()
		{
			Assert.AreEqual("Default English", mgr.GetLocalizer("en-US").Resolve("TestDefault"));
		}

		[Test] public void NeutralFromUS ()
		{
			Assert.AreEqual("Neutral English", mgr.GetLocalizer("en-US").Resolve("TestNeutral"));
		}

		[Test] public void SpecificFromUS ()
		{
			Assert.AreEqual("Specific American English", mgr.GetLocalizer("en-US").Resolve("TestSpecific"));
		}

		[Test] public void FromMissingNeutralCulture ()
		{
			Assert.AreEqual("Neutral English", mgr.GetLocalizer("de").Resolve("TestNeutral"));
		}

		[Test] public void FromMissingNeutralSpecificCulture ()
		{
			Assert.AreEqual("Specific English", mgr.GetLocalizer("de-AT").Resolve("TestSpecific"));
		}

		[Test] public void FromMissingSpecificCulture ()
		{
			Assert.AreEqual("Specific French", mgr.GetLocalizer("fr-CH").Resolve("TestSpecific"));
		}

		[Test] public void Resolving()
		{
			/*
			       | Base | Scrum | Custom
			 ------+----------------------
			 en    |  1   |   2   |   3
			 fr    |  4   |   5   |   6
			 fr-SW |  7   |   8   |   9
			 
			 *  Localizer always resolves from right to left, bottom to top
			 */
			Assert.AreEqual("ABC PQR GHI", mgr.GetLocalizer("fr-FR").Resolve("XYZ"));
		}

		[Test] public void TemplateExpansion()
		{
			Assert.AreEqual("Remove PBIs", mgr.GetLocalizer("fr-FR").Resolve("Command'Delete'Story"));
		}
	}
}
