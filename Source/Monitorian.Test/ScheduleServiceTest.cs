using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Monitorian.Core.Models;

namespace Monitorian.Test;

[TestClass]
public class ScheduleServiceTest
{
	[TestMethod]
	public void SettingsDefaultScheduleValues()
	{
		var settings = new SettingsCore();
		Assert.IsFalse(settings.EnablesSchedule);
		Assert.AreEqual(7, settings.ScheduleDayHour);
		Assert.AreEqual(20, settings.ScheduleNightHour);
		Assert.AreEqual(80, settings.ScheduleDayBrightness);
		Assert.AreEqual(30, settings.ScheduleNightBrightness);
	}

	[TestMethod]
	public void SettingsScheduleHourClamping()
	{
		var settings = new SettingsCore
		{
			ScheduleDayHour = -5,
			ScheduleNightHour = 35,
			ScheduleDayBrightness = -10,
			ScheduleNightBrightness = 150
		};

		Assert.AreEqual(0, settings.ScheduleDayHour);
		Assert.AreEqual(23, settings.ScheduleNightHour);
		Assert.AreEqual(0, settings.ScheduleDayBrightness);
		Assert.AreEqual(100, settings.ScheduleNightBrightness);
	}

	[TestMethod]
	public void DeterminePeriodStandardDay()
	{
		// Day starts at 7, Night starts at 20.
		// 10:00 AM should be Day.
		var time = new DateTime(2026, 9, 20, 10, 0, 0);
		var period = ScheduleService.DeterminePeriod(time, 7, 20);
		Assert.AreEqual(SchedulePeriod.Day, period);
	}

	[TestMethod]
	public void DeterminePeriodStandardNight()
	{
		// 22:00 (10:00 PM) should be Night.
		var time = new DateTime(2026, 9, 20, 22, 0, 0);
		var period = ScheduleService.DeterminePeriod(time, 7, 20);
		Assert.AreEqual(SchedulePeriod.Night, period);
	}

	[TestMethod]
	public void DeterminePeriodEarlyMorning()
	{
		// 03:00 AM should be Night.
		var time = new DateTime(2026, 9, 20, 3, 0, 0);
		var period = ScheduleService.DeterminePeriod(time, 7, 20);
		Assert.AreEqual(SchedulePeriod.Night, period);
	}

	[TestMethod]
	public void DeterminePeriodExactBoundaries()
	{
		// 07:00 should transition into Day
		var timeDay = new DateTime(2026, 9, 20, 7, 0, 0);
		Assert.AreEqual(SchedulePeriod.Day, ScheduleService.DeterminePeriod(timeDay, 7, 20));

		// 20:00 should transition into Night
		var timeNight = new DateTime(2026, 9, 20, 20, 0, 0);
		Assert.AreEqual(SchedulePeriod.Night, ScheduleService.DeterminePeriod(timeNight, 7, 20));
	}

	[TestMethod]
	public void DeterminePeriodInvertedHours()
	{
		// Inverted: Day starts at 20, Night starts at 7.
		var timeNight = new DateTime(2026, 9, 20, 12, 0, 0);
		Assert.AreEqual(SchedulePeriod.Night, ScheduleService.DeterminePeriod(timeNight, 20, 7));

		var timeDay = new DateTime(2026, 9, 20, 21, 0, 0);
		Assert.AreEqual(SchedulePeriod.Day, ScheduleService.DeterminePeriod(timeDay, 20, 7));
	}

	[TestMethod]
	public void DeterminePeriodIdenticalHours()
	{
		var time = new DateTime(2026, 9, 20, 12, 0, 0);
		Assert.AreEqual(SchedulePeriod.Day, ScheduleService.DeterminePeriod(time, 12, 12));
	}
}
