using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Monitorian.Core.Models;

namespace Monitorian.Test;

[TestClass]
public class BlackoutServiceTest
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
	public void SettingsDefaultMiddleClickBlackoutIsEnabled()
	{
		var settings = new SettingsCore();
		Assert.IsTrue(settings.EnablesMiddleClickBlackout);
	}

	[TestMethod]
	public void BlackoutServiceStartsInactive()
	{
		Assert.IsFalse(BlackoutService.IsBlackoutActive);
	}

	[TestMethod]
	public void BlackoutServiceStartAndDismiss()
	{
		RunInSta(() =>
		{
			try
			{
				BlackoutService.Dismiss();
				Assert.IsFalse(BlackoutService.IsBlackoutActive);

				BlackoutService.Show();
				Assert.IsTrue(BlackoutService.IsBlackoutActive);

				BlackoutService.Dismiss();
				Assert.IsFalse(BlackoutService.IsBlackoutActive);
			}
			finally
			{
				BlackoutService.Dismiss();
			}
		});
	}

	[TestMethod]
	public void BlackoutServiceToggle()
	{
		RunInSta(() =>
		{
			try
			{
				BlackoutService.Dismiss();
				Assert.IsFalse(BlackoutService.IsBlackoutActive);

				BlackoutService.Toggle();
				Assert.IsTrue(BlackoutService.IsBlackoutActive);

				BlackoutService.Toggle();
				Assert.IsFalse(BlackoutService.IsBlackoutActive);
			}
			finally
			{
				BlackoutService.Dismiss();
			}
		});
	}
}
