using System;
using System.Windows.Threading;

namespace Monitorian.Core.Models;

public enum SchedulePeriod
{
	None = 0,
	Day,
	Night
}

/// <summary>
/// Service to automatically adjust monitor brightness based on scheduled daytime and nighttime hours.
/// </summary>
public class ScheduleService : IDisposable
{
	private readonly DispatcherTimer _timer;
	private bool _isDisposed;

	public SchedulePeriod CurrentPeriod { get; private set; } = SchedulePeriod.None;
	public bool IsRunning => _timer.IsEnabled;

	public event Action<SchedulePeriod, int> PeriodChanged;

	public ScheduleService()
	{
		_timer = new DispatcherTimer
		{
			Interval = TimeSpan.FromMinutes(1)
		};
		_timer.Tick += (_, _) => OnTick();
	}

	private SettingsCore _currentSettings;

	public void Start(SettingsCore settings)
	{
		if (_isDisposed)
			return;

		_currentSettings = settings ?? throw new ArgumentNullException(nameof(settings));
		_timer.Stop();

		Evaluate(_currentSettings, force: true);
		_timer.Start();
	}

	public void Stop()
	{
		_timer.Stop();
		CurrentPeriod = SchedulePeriod.None;
	}

	private void OnTick()
	{
		if (_currentSettings is { EnablesSchedule: true })
		{
			Evaluate(_currentSettings, force: false);
		}
		else
		{
			Stop();
		}
	}

	public void Evaluate(SettingsCore settings, bool force)
	{
		if (settings is null || !settings.EnablesSchedule)
			return;

		var newPeriod = DeterminePeriod(DateTime.Now, settings.ScheduleDayHour, settings.ScheduleNightHour);
		if (force || newPeriod != CurrentPeriod)
		{
			CurrentPeriod = newPeriod;
			var targetBrightness = (newPeriod == SchedulePeriod.Day)
				? settings.ScheduleDayBrightness
				: settings.ScheduleNightBrightness;

			PeriodChanged?.Invoke(newPeriod, targetBrightness);
		}
	}

	/// <summary>
	/// Determines whether a given time falls within the daytime or nighttime schedule.
	/// </summary>
	public static SchedulePeriod DeterminePeriod(DateTime time, int dayHour, int nightHour)
	{
		dayHour = Math.Max(0, Math.Min(23, dayHour));
		nightHour = Math.Max(0, Math.Min(23, nightHour));

		int currentHour = time.Hour;

		if (dayHour < nightHour)
		{
			return (currentHour >= dayHour && currentHour < nightHour)
				? SchedulePeriod.Day
				: SchedulePeriod.Night;
		}
		if (dayHour > nightHour)
		{
			return (currentHour >= nightHour && currentHour < dayHour)
				? SchedulePeriod.Night
				: SchedulePeriod.Day;
		}

		return SchedulePeriod.Day;
	}

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (_isDisposed)
			return;

		if (disposing)
		{
			_timer.Stop();
			_currentSettings = null;
		}

		_isDisposed = true;
	}
}
