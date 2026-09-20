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
	public void SettingsDeserializationWithoutNewElementPreservesDefaults()
	{
		var xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?><SettingsCore xmlns=\"http://schemas.datacontract.org/2004/07/Monitorian.Core.Models\"><UsesLargeElements>true</UsesLargeElements></SettingsCore>";
		using var sr = new System.IO.StringReader(xml);
		using var xr = System.Xml.XmlReader.Create(sr);
		var serializer = new System.Runtime.Serialization.DataContractSerializer(typeof(SettingsCore));
		var loaded = (SettingsCore)serializer.ReadObject(xr);

		Assert.IsTrue(loaded.EnablesMiddleClickBlackout, "EnablesMiddleClickBlackout must default to true when missing from XML");
		Assert.IsTrue(loaded.EnablesHotKeys, "EnablesHotKeys must default to true when missing from XML");
		Assert.AreEqual(7, loaded.ScheduleDayHour, "ScheduleDayHour must default to 7 when missing from XML");
		Assert.AreEqual(20, loaded.ScheduleNightHour, "ScheduleNightHour must default to 20 when missing from XML");
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
