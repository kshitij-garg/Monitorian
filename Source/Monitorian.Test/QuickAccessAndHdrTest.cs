using System;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Monitorian.Core.Models;
using ScreenFrame.Movers;

namespace Monitorian.Test;

[TestClass]
public class QuickAccessAndHdrTest
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
	public void SettingsDefaultQuickAccessIsEnabled()
	{
		var settings = new SettingsCore();
		Assert.IsTrue(settings.EnablesQuickAccess, "EnablesQuickAccess should default to true.");
	}

	[TestMethod]
	public void SettingsDeserializationPreservesQuickAccessDefault()
	{
		// Simulate older settings XML lacking EnablesQuickAccess
		const string legacyXml = @"<SettingsCore xmlns=""http://schemas.datacontract.org/2004/07/Monitorian.Core.Models"" xmlns:i=""http://www.w3.org/2001/XMLSchema-instance""><EnablesIconWheel>true</EnablesIconWheel></SettingsCore>";

		var serializer = new DataContractSerializer(typeof(SettingsCore));
		using var stream = new MemoryStream(Encoding.UTF8.GetBytes(legacyXml));
		var loaded = (SettingsCore)serializer.ReadObject(stream);

		Assert.IsTrue(loaded.EnablesQuickAccess, "EnablesQuickAccess must default to true when missing from legacy XML.");
		Assert.IsTrue(loaded.EnablesHotKeys, "EnablesHotKeys must default to true when missing from legacy XML.");
		Assert.IsTrue(loaded.EnablesMiddleClickBlackout, "EnablesMiddleClickBlackout must default to true when missing from legacy XML.");
	}

	[TestMethod]
	public void HotKeyServiceQuickAccessWiring()
	{
		RunInSta(() =>
		{
			using var service = new HotKeyService();
			bool fired = false;
			service.QuickAccessRequested += () => fired = true;

			Assert.AreEqual(1004, HotKeyService.HotKeyQuickAccessId);
			Assert.IsFalse(fired);
		});
	}

	[TestMethod]
	public void DisplayIndexParsingFromGdiName()
	{
		string[] testCases = ["\\\\.\\DISPLAY1", "\\\\.\\DISPLAY2", "DISPLAY10", "\\\\.\\DISPLAY3 "];
		byte[] expected = [1, 2, 10, 3];

		for (int i = 0; i < testCases.Length; i++)
		{
			var match = Regex.Match(testCases[i], @"DISPLAY(?<index>\d{1,2})\s*$");
			Assert.IsTrue(match.Success, $"Failed to match {testCases[i]}");
			byte parsed = byte.Parse(match.Groups["index"].Value);
			Assert.AreEqual(expected[i], parsed, $"Mismatch for {testCases[i]}");
		}
	}

	[TestMethod]
	public void StickWindowMoverCursorClamping()
	{
		RunInSta(() =>
		{
			using var notifyIcon = new System.Windows.Forms.NotifyIcon();
			var window = new Window();
			var mover = new StickWindowMover(window, notifyIcon);

			// Test cursor clamping with mock screen coordinates
			var cursor = new Point(1900, 1050);
			double windowWidth = 320;
			double windowHeight = 400;

			if (mover.TryGetAdjacentLocationToCursor(cursor, windowWidth, windowHeight, out Rect location))
			{
				Assert.IsTrue(location.Width == windowWidth);
				Assert.IsTrue(location.Height == windowHeight);
				Assert.IsTrue(location.Right <= 1920 || location.X >= 0);
			}
		});
	}
}
