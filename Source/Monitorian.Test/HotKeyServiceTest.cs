using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Monitorian.Core.Models;

namespace Monitorian.Test;

[TestClass]
public class HotKeyServiceTest
{
	private static void RunInSta(Action action)
	{
		Exception thrown = null;
		var thread = new Thread(() =>
		{
			try
			{
				action();
			}
			catch (Exception ex)
			{
				thrown = ex;
			}
		});
		thread.SetApartmentState(ApartmentState.STA);
		thread.Start();
		thread.Join();

		if (thrown is not null)
			throw thrown;
	}

	[TestMethod]
	public void SettingsDefaultHotKeysIsEnabled()
	{
		var settings = new SettingsCore();
		Assert.IsTrue(settings.EnablesHotKeys);
	}

	[TestMethod]
	public void HotKeyServiceLifecycle()
	{
		RunInSta(() =>
		{
			using var service = new HotKeyService();
			Assert.IsFalse(service.IsRegistered);

			// Calling Register attempts Win32 registration
			// In CI/headless or desktop environments, it should either succeed or fail gracefully without throwing.
			var registered = service.Register();
			if (registered)
			{
				Assert.IsTrue(service.IsRegistered);
				service.Unregister();
				Assert.IsFalse(service.IsRegistered);
			}
		});
	}
}
