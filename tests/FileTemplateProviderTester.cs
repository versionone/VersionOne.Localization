﻿using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace VersionOne.Localization.Tests
{
	public class FileTemplateProviderTester
	{
		private IEnumerable<Template> SetupTemplates(Encoding encoding)
		{
			using (var writer = new StreamWriter("no.Test.txt", false, encoding))
				writer.WriteLine("løsningsforslag=solutions");

			return LoadTemplates().ToList();
		}

		private IEnumerable<Template> LoadTemplates()
		{
			var loader = new FileTemplateProvider(".", "Test");
			using (var set = loader.Load("no"))
				yield return set.GetNextTemplate();
		}

		[Test] public void BOMless_UTF8_Works()
		{
			var templates = SetupTemplates(new UTF8Encoding(false));
			Assert.IsNotEmpty(templates);
			Assert.AreEqual("løsningsforslag", templates.First().Tag);
		}

		[Test] public void UTF8_Works()
		{
			var templates = SetupTemplates(new UTF8Encoding(true));
			Assert.IsNotEmpty(templates);
			Assert.AreEqual("løsningsforslag", templates.First().Tag);
		}

		[ExpectedException(typeof(DecoderFallbackException))]
		[Test] public void ANSI_Throws()
		{
			var templates = SetupTemplates(Encoding.GetEncoding(CultureInfo.GetCultureInfo("no").TextInfo.ANSICodePage));
			Assert.IsEmpty(templates);
		}
	}
}
