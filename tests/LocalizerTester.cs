using NUnit.Framework;
using VersionOne.Localization;

namespace VersionOne.Tests.LocalizationTests
{
	public class LocalizerTester
	{
		[Test] public void UnresolvedSelf ()
		{
			Localizer loc = new Localizer(null);
			Assert.AreEqual("bogus", loc.Resolve("bogus"));
		}

		[Test] public void SimpleTranslate ()
		{
			Localizer loc = new Localizer(null);
			loc.Add("tag1", "This is the tag 1 string");
			Assert.AreEqual("This is the tag 1 string", loc.Resolve("tag1"));
		}

		[Test] public void SimpleTemplate ()
		{
			Localizer loc = new Localizer(null);
			loc.Add("Plural", "{1}s");
			Assert.AreEqual("Storys", loc.Resolve("Plural'Story"));
		}

		[Test] public void PluralTemplate ()
		{
			Localizer loc = new Localizer(null);
			loc.Add("Plural", "{1}s");
			Assert.AreEqual("AssetTypes", loc.Resolve("Plural'AssetType'Story"));
		}

		[Test] public void PluralAssocTemplate ()
		{
			Localizer loc = new Localizer(null);
			loc.Add("Plural", "{1}s");
			loc.Add("Plural'AssetType", "{Plural'{1'2}'}");
			loc.Add("AssetType'Fronat", "Memo");
			Assert.AreEqual("Memos", loc.Resolve("Plural'AssetType'Fronat"));
		}

		[Test] public void PluralNestTemplate ()
		{
			Localizer loc = new Localizer(null);
			loc.Add("Plural", "{1}s");
			loc.Add("Plural'AssetType", "{Plural'{1'2}'x}");
			loc.Add("AssetType'Story", "Backlog Item");
			Assert.AreEqual("Backlog Items", loc.Resolve("Plural'AssetType'Story"));
		}

		[Test] public void CascadedTemplate ()
		{
			Localizer loc = new Localizer(null);
			loc.Add("Plural", "{1}s");
			loc.Add("Report2", "Report {Plural'1}/{Plural'2}");
			Assert.AreEqual("Report Scopes/Storys", loc.Resolve("Report2'Scope'Story"));
		}

		[Test] public void TemplateOverride ()
		{
			Localizer loc = new Localizer(null);
			loc.Add("Plural", "{1}s");
			loc.Add("Plural'Story", "Stories");
			Assert.AreEqual("Scopes", loc.Resolve("Plural'Scope"));
			Assert.AreEqual("Stories", loc.Resolve("Plural'Story"));
		}

		[Test] public void ComplexTemplate ()
		{
			Localizer loc = new Localizer(null);
			loc.Add("Plural", "{1}s");
			loc.Add("Plural'Story", "Stories");
			loc.Add("Report2", "Report {Plural'1}/{Plural'2}");
			loc.Add("Scope", "Project");
			Assert.AreEqual("Report Projects/Stories", loc.Resolve("Report2'Scope'Story"));
		}

		[Test] public void ComplexTemplateOverride ()
		{
			Localizer loc = new Localizer(null);
			loc.Add("Plural", "{1}s");
			loc.Add("Report2", "Report {Plural'1}/{Plural'2}");
			loc.Add("Plural'Story", "Stories");
			loc.Add("Scope", "Project");
			loc.Add("Report2'Member", "Report: User by {2}");
			Assert.AreEqual("Report Projects/Stories", loc.Resolve("Report2'Scope'Story"));
			Assert.AreEqual("Report: User by Story", loc.Resolve("Report2'Member'Story"));
			Assert.AreEqual("Report: User by Project", loc.Resolve("Report2'Member'Scope"));
		}

		[Test] public void AlwaysExpand ()
		{
			Localizer loc = new Localizer(null);
			loc.Add("Add'Story", "Add {Story}");
			Assert.AreEqual("Add Story", loc.Resolve("Add'Story"));
		}

		[Test] public void AlwaysExpandCascaded ()
		{
			Localizer loc = new Localizer(null);
			loc.Add("Add'Story", "Add {Story}");
			loc.Add("Story", "Backlog Item");
			Assert.AreEqual("Add Backlog Item", loc.Resolve("Add'Story"));
		}

		[Test] public void AlwaysExpandSubstitution ()
		{
			Localizer loc = new Localizer(null);
			loc.Add("Add", "Add {1}");
			loc.Add("Story", "Backlog Item");
			loc.Add("Backlog Item", "bogus");
			Assert.AreEqual("Add Backlog Item", loc.Resolve("Add'Story"));
		}

		[Test] public void NoInfiniteRecursion ()
		{
			Localizer loc = new Localizer(null);
			loc.Add("bogus", "recursive {0} recursive");
			Assert.AreEqual("recursive bogus recursive", loc.Resolve("bogus"));
		}

		[Test] public void DoubleBrace ()
		{
			Localizer loc = new Localizer(null);
			loc.Add("open", "open {{ brace");
			loc.Add("close", "close }} brace");
			Assert.AreEqual("open { brace", loc.Resolve("open"));
			Assert.AreEqual("close } brace", loc.Resolve("close"));
		}

		[Test] public void MatchingDoubleBrace ()
		{
			Localizer loc = new Localizer(null);
			loc.Add("match", "open {{and}} close");
			loc.Add("and", "xyz");
			Assert.AreEqual("open {and} close", loc.Resolve("match"));
		}

		[Test] public void BracesNest ()
		{
			Localizer loc = new Localizer(null);
			loc.Add("match", "open {{{and}}} close");
			loc.Add("and", "xyz");
			Assert.AreEqual("open {xyz} close", loc.Resolve("match"));
		}

		[Test] public void ConditionalSpaceNo ()
		{
			Localizer loc = new Localizer(null);
			loc.Add("A", "x {1}");
			loc.Add("B", "y");
			Assert.AreEqual("x", loc.Resolve("A"));
		}

		[Test] public void ConditionalSpaceYes ()
		{
			Localizer loc = new Localizer(null);
			loc.Add("A", "x {1}");
			loc.Add("B", "y");
			Assert.AreEqual("x y", loc.Resolve("A'B"));
		}

		[Test] public void DetermineKeys()
		{
			ListAssert.AreEqual(new[] { "A", "B" }, Localizer.DetermineKeys("blah $[A] blah $[B] blah", "$[", "]"));
		}

		[Test] public void EmptyBraces()
		{
			Localizer loc = new Localizer(null);
			loc.Add("A", "{}");
			Assert.AreEqual("", loc.Resolve("A"));
		}
	}
}
