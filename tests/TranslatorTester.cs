using NUnit.Framework;

namespace VersionOne.Localization.Tests
{
	public class TranslatorTester
	{
		[Test] public void DetermineKeys()
		{
			Assert.That(Translator.DetermineKeys("blah $[A] blah $[B] blah", "$[", "]"), Is.EqualTo(new[] { "A", "B" }));
		}
	}
}