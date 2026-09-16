using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Monitorian.Core.Models;

namespace Monitorian.Core.ViewModels;

public class MenuWindowViewModel : ViewModelBase
{
	private readonly AppControllerCore _controller;
	public SettingsCore Settings => _controller.Settings;

	public MenuWindowViewModel(AppControllerCore controller)
	{
		this._controller = controller ?? throw new ArgumentNullException(nameof(controller));

		CleanLicense();
	}

	#region License

	private const string LicenseFileName = "License.txt";
	private static bool _licenseFileExists = true; // Default

	public void OpenLicense()
	{
		Task.Run(() =>
		{
			try
			{
				if (!string.IsNullOrEmpty(ProductInfo.LicenseUrl))
				{
					Process.Start(new ProcessStartInfo(ProductInfo.LicenseUrl) { UseShellExecute = true });
					return;
				}
			}
			catch
			{
			}

			var licenseFileBody = DocumentService.ReadEmbeddedFile(LicenseFileName);
			var licenseFileHtml = DocumentService.BuildHtml(LicenseFileName, ProductInfo.Product, licenseFileBody);

			(_licenseFileExists, var licenseFilePath) = TempService.SaveFile(LicenseFileName, "html", licenseFileHtml);
			if (!_licenseFileExists)
				return;

			Process.Start(licenseFilePath);
		});
	}

	private void CleanLicense()
	{
		if (!_licenseFileExists)
			return;

		Task.Run(() => _licenseFileExists = TempService.DeleteFile(LicenseFileName, "html", TimeSpan.FromHours(1)));
	}

	#endregion

	#region Startup

	public bool CanRegister => _controller.StartupAgent.CanRegister();

	public bool IsRegistered
	{
		get
		{
			if (!_isRegistered.HasValue)
			{
				_isRegistered = _controller.StartupAgent.IsRegistered();
			}
			return _isRegistered.GetValueOrDefault();
		}
		set
		{
			if (_isRegistered == value)
				return;

			if (value)
			{
				_controller.StartupAgent.Register();
			}
			else
			{
				_controller.StartupAgent.Unregister();
			}
			_isRegistered = value;
			OnPropertyChanged();
		}
	}
	private bool? _isRegistered;

	#endregion

	#region Accent color

	public bool IsAccentColorSupported => _controller.WindowPainter.IsAccentColorSupported;

	#endregion

	#region Language

	public IReadOnlyList<LanguageItem> Languages => LanguageService.SupportedLanguages;

	public LanguageItem SelectedLanguage
	{
		get
		{
			var code = Settings.SelectedCulture;
			return Languages.FirstOrDefault(x => string.Equals(x.Code, code, StringComparison.OrdinalIgnoreCase))
				?? Languages[0];
		}
		set
		{
			if (value is null)
				return;

			var currentCode = Settings.SelectedCulture;
			if (string.Equals(currentCode, value.Code, StringComparison.OrdinalIgnoreCase))
				return;

			Settings.SelectedCulture = value.Code;
			LanguageService.SwitchDefault(value.Code);
			OnPropertyChanged();

			LanguageChanged?.Invoke(this, EventArgs.Empty);
		}
	}

	public event EventHandler LanguageChanged;

	#endregion

	public event EventHandler CloseAppRequested;

	/// <summary>
	/// Closes this application.
	/// </summary>
	public void CloseApp() => CloseAppRequested?.Invoke(this, EventArgs.Empty);

	#region IDisposable

	private bool _isDisposed = false;

	protected override void Dispose(bool disposing)
	{
		if (_isDisposed)
			return;

		if (disposing)
		{
			CloseAppRequested = null;
			LanguageChanged = null;
		}

		_isDisposed = true;

		base.Dispose(disposing);
	}

	#endregion
}