using System;
using System.Globalization;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Monitorian.Core.Models;
using Monitorian.Core.Properties;

namespace Monitorian.Test;

[TestClass]
public class LanguageServiceTest
{
	[TestCleanup]
	public void ResetCulture()
	{
		LanguageService.SwitchDefault();
	}

	[TestMethod]
	public void SupportedLanguageCodesAreValidAndUnique()
	{
		var languages = LanguageService.SupportedLanguages;
		var codes = languages.Select(x => x.Code).ToArray();

		Assert.IsTrue(languages.Count > 1);
		Assert.IsNull(languages[0].Code);
		Assert.AreEqual(codes.Length, codes.Distinct(StringComparer.OrdinalIgnoreCase).Count());

		foreach (var code in codes.Where(x => x is not null))
		{
			Assert.IsTrue(
				string.Equals(code, CultureInfo.GetCultureInfo(code).Name, StringComparison.OrdinalIgnoreCase),
				code);
		}
	}

	[TestMethod]
	public void ResourcesLoadForEverySupportedLanguage()
	{
		foreach (var language in LanguageService.SupportedLanguages.Where(x => x.Code is not null))
		{
			Assert.IsTrue(LanguageService.SwitchDefault(language.Code), language.Code);

			var culture = CultureInfo.GetCultureInfo(language.Code);
			var resourceSet = Resources.ResourceManager.GetResourceSet(culture, true, true);

			Assert.IsNotNull(resourceSet, language.Code);
			Assert.IsFalse(string.IsNullOrWhiteSpace(Resources.Close), language.Code);

			if (!string.Equals(language.Code, "en", StringComparison.OrdinalIgnoreCase))
			{
				Assert.IsTrue(LanguageService.ResourceDictionary.Count > 0, language.Code);
			}
		}
	}
}
