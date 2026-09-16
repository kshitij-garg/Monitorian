using Microsoft.VisualStudio.TestTools.UnitTesting;

using Monitorian.Core.Models;

namespace Monitorian.Test;

[TestClass]
public class CommandLineServiceTest
{
	[TestMethod]
	public void ParseGetBrightness()
	{
		Assert.IsTrue(CommandLineService.TryParse(["/get"], out var command, out _));
		Assert.AreEqual(CommandLineAction.Get, command.Action);
		Assert.AreEqual(CommandLineMetric.Brightness, command.Metric);
		Assert.IsNull(command.Target);
	}

	[TestMethod]
	public void ParseGetContrastForMonitor()
	{
		Assert.IsTrue(CommandLineService.TryParse(
			["/get", "contrast", "Dell", "U2720Q"],
			out var command,
			out _));

		Assert.AreEqual(CommandLineMetric.Contrast, command.Metric);
		Assert.AreEqual("Dell U2720Q", command.Target);
	}

	[TestMethod]
	public void ParseAbsoluteSetForMonitor()
	{
		Assert.IsTrue(CommandLineService.TryParse(
			["/set", "Dell U2720Q", "75"],
			out var command,
			out _));

		Assert.AreEqual(CommandLineAction.Set, command.Action);
		Assert.AreEqual("Dell U2720Q", command.Target);
		Assert.AreEqual(75, command.Value);
		Assert.IsFalse(command.IsRelative);
	}

	[TestMethod]
	public void ParseRelativeSet()
	{
		Assert.IsTrue(CommandLineService.TryParse(["/set", "-15"], out var command, out _));
		Assert.AreEqual(-15, command.Value);
		Assert.IsTrue(command.IsRelative);
	}

	[TestMethod]
	public void ParseContrastSet()
	{
		Assert.IsTrue(CommandLineService.TryParse(
			["/set", "contrast", "LG UltraGear", "55"],
			out var command,
			out _));

		Assert.AreEqual(CommandLineMetric.Contrast, command.Metric);
		Assert.AreEqual("LG UltraGear", command.Target);
		Assert.AreEqual(55, command.Value);
	}

	[TestMethod]
	public void RejectInvalidAbsoluteValue()
	{
		Assert.IsFalse(CommandLineService.TryParse(["/set", "101"], out _, out var error));
		Assert.IsFalse(string.IsNullOrWhiteSpace(error));
	}
}
