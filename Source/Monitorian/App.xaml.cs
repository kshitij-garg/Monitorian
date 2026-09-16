using System;
using System.Diagnostics;
using System.Windows;

using Monitorian.Core;
using Monitorian.Core.Models;

namespace Monitorian;

public partial class App : Application
{
	private AppKeeper _keeper;
	private AppController _controller;

	protected override async void OnStartup(StartupEventArgs e)
	{
		base.OnStartup(e);

		try
		{
			_keeper = new AppKeeper();
			if (!await _keeper.StartAsync(e))
			{
				this.Shutdown(0); // This shutdown is expected behavior.
				return;
			}

			_controller = new AppController(_keeper);
			await _controller.InitiateAsync();
		}
		catch (Exception ex) when (AppKeeper.IsRecoverableException(ex))
		{
			TryReportException(ex);
			this.Shutdown(1);
		}
	}

	protected override void OnExit(ExitEventArgs e)
	{
		try
		{
			_controller?.End();
		}
		catch (Exception ex) when (AppKeeper.IsRecoverableException(ex))
		{
			TryReportException(ex);
		}
		finally
		{
			try
			{
				_keeper?.End();
			}
			catch (Exception ex) when (AppKeeper.IsRecoverableException(ex))
			{
				TryReportException(ex);
			}

			base.OnExit(e);
		}
	}

	private static void TryReportException(Exception exception)
	{
		try
		{
			Logger.SaveException(exception);
		}
		catch (Exception reportingException) when (AppKeeper.IsRecoverableException(reportingException))
		{
			Trace.WriteLine($"Failed to report application exception.{Environment.NewLine}{reportingException}");
		}
	}
}