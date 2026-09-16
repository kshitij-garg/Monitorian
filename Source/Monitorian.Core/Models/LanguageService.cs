using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Threading;

using Monitorian.Core.Properties;

namespace Monitorian.Core.Models;

public class LanguageItem
{
	public string Code { get; }
	public string DisplayName { get; }

	public LanguageItem(string code, string displayName)
	{
		Code = code;
		DisplayName = displayName;
	}

	public override string ToString() => DisplayName;
}

public class LanguageService
{
	public static IReadOnlyCollection<string> Options => [Option];

	private const string Option = "/lang";

	public static IReadOnlyList<LanguageItem> SupportedLanguages { get; } =
	[
		new(null, "System Default"),
		new("en", "English"),
		new("hi", "हिन्दी (Hindi)"),
		new("bn", "বাংলা (Bengali)"),
		new("mr", "मराठी (Marathi)"),
		new("te", "తెలుగు (Telugu)"),
		new("ta", "தமிழ் (Tamil)"),
		new("ar", "العربية (Arabic)"),
		new("ca", "Català (Catalan)"),
		new("zh-Hans", "简体中文 (Chinese Simplified)"),
		new("zh-Hant", "繁體中文 (Chinese Traditional)"),
		new("nl-NL", "Nederlands (Dutch)"),
		new("fr", "Français (French)"),
		new("de", "Deutsch (German)"),
		new("el-GR", "Ελληνικά (Greek)"),
		new("it", "Italiano (Italian)"),
		new("ja-JP", "日本語 (Japanese)"),
		new("ko-KR", "한국어 (Korean)"),
		new("fa-IR", "فارسی (Persian)"),
		new("pl-PL", "Polski (Polish)"),
		new("pt-BR", "Português (Portuguese)"),
		new("ro", "Română (Romanian)"),
		new("ru-RU", "Русский (Russian)"),
		new("sq", "Shqip (Albanian)"),
		new("sl", "Slovenščina (Slovenian)"),
		new("es", "Español (Spanish)"),
		new("sv-SE", "Svenska (Swedish)"),
		new("tr-TR", "Türkçe (Turkish)"),
		new("uk-UA", "Українська (Ukrainian)"),
		new("vi-VN", "Tiếng Việt (Vietnamese)")
	];

	private static CultureInfo _currentCulture;

	public static IReadOnlyDictionary<string, string> ResourceDictionary => _resourceDictionary ??= BuildResourceDictionary();
	private static Dictionary<string, string> _resourceDictionary;

	private static Dictionary<string, string> BuildResourceDictionary()
	{
		var culture = _currentCulture ?? CultureInfo.CurrentUICulture;

		// Go back up culture hierarchy. 
		while (culture != CultureInfo.InvariantCulture)
		{
			var resourceSet = new ResourceManager(typeof(Resources)).GetResourceSet(culture, true, false);
			if (resourceSet is not null)
			{
				return resourceSet.Cast<DictionaryEntry>()
					.Where(x => x.Key is string)
					.ToDictionary(x => (string)x.Key, x => x.Value?.ToString());
			}

			culture = culture.Parent;
		}
		return new Dictionary<string, string>();
	}

	public static bool IsResourceRightToLeft => ((_currentCulture ?? CultureInfo.CurrentUICulture).TextInfo.IsRightToLeft is true);

	/// <summary>
	/// Switches default and current thread's culture.
	/// </summary>
	/// <param name="cultureName">Culture code or null for default/CLI</param>
	/// <returns>True if successfully switched to a specific culture</returns>
	public static bool SwitchDefault(string cultureName = null)
	{
		var culture = GetSpecifiedCulture(cultureName);

		_currentCulture = culture;
		_resourceDictionary = null;

		if (culture is not null)
		{
			CultureInfo.DefaultThreadCurrentCulture = culture;
			CultureInfo.DefaultThreadCurrentUICulture = culture;
			Thread.CurrentThread.CurrentCulture = culture;
			Thread.CurrentThread.CurrentUICulture = culture;
			return true;
		}
		else
		{
			CultureInfo.DefaultThreadCurrentCulture = null;
			CultureInfo.DefaultThreadCurrentUICulture = null;
			Thread.CurrentThread.CurrentCulture = CultureInfo.InstalledUICulture;
			Thread.CurrentThread.CurrentUICulture = CultureInfo.InstalledUICulture;
			return false;
		}
	}

	/// <summary>
	/// Switches current thread's culture.
	/// </summary>
	/// <returns>True if current culture is non-null</returns>
	public static bool Switch()
	{
		if (_currentCulture is not null)
		{
			Thread.CurrentThread.CurrentCulture = _currentCulture;
			Thread.CurrentThread.CurrentUICulture = _currentCulture;
			return true;
		}
		return false;
	}

	private static CultureInfo GetSpecifiedCulture(string cultureName)
	{
		var arguments = AppKeeper.StandardArguments;
		if (arguments is not null)
		{
			int i = 0;
			while (i < arguments.Count - 1)
			{
				if (string.Equals(arguments[i], Option, StringComparison.OrdinalIgnoreCase))
				{
					var cliCultureName = arguments[i + 1];
					var match = CultureInfo.GetCultures(CultureTypes.AllCultures)
						.FirstOrDefault(x => string.Equals(x.Name, cliCultureName, StringComparison.OrdinalIgnoreCase));
					if (match is not null)
						return match;
				}
				i++;
			}
		}

		if (!string.IsNullOrWhiteSpace(cultureName))
		{
			return CultureInfo.GetCultures(CultureTypes.AllCultures)
				.FirstOrDefault(x => string.Equals(x.Name, cultureName, StringComparison.OrdinalIgnoreCase));
		}

		return null;
	}
}