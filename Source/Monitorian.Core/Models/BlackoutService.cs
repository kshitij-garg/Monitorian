using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace Monitorian.Core.Models;

/// <summary>
/// Service to black out all monitors with instant full-screen overlay windows,
/// dismissing on any mouse movement, key press, or click.
/// </summary>
public static class BlackoutService
{
	#region Win32

	[DllImport("User32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool SetWindowPos(
		IntPtr hWnd,
		IntPtr hWndInsertAfter,
		int X,
		int Y,
		int cx,
		int cy,
		uint uFlags);

	[DllImport("User32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool GetCursorPos(out POINT lpPoint);

	[DllImport("User32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool SetForegroundWindow(IntPtr hWnd);

	[StructLayout(LayoutKind.Sequential)]
	private struct POINT
	{
		public int X;
		public int Y;
	}

	private static readonly IntPtr HWND_TOPMOST = new(-1);
	private const uint SWP_SHOWWINDOW = 0x0040;

	#endregion

	private static readonly List<Window> _windows = [];
	private static POINT _initialCursorPos;
	private static DateTime _activatedTime;
	private const double GracePeriodMs = 350;
	private const int MoveThresholdPx = 10;

	public static bool IsBlackoutActive => _windows.Count > 0;

	public static event EventHandler BlackoutChanged;

	public static void Toggle()
	{
		if (IsBlackoutActive)
		{
			Dismiss();
		}
		else
		{
			Show();
		}
	}

	public static void Show()
	{
		if (IsBlackoutActive)
			return;

		GetCursorPos(out _initialCursorPos);
		_activatedTime = DateTime.UtcNow;

		var screens = System.Windows.Forms.Screen.AllScreens;
		if (screens.Length == 0)
			return;

		Window primaryWindow = null;

		foreach (var screen in screens)
		{
			var window = new Window
			{
				WindowStyle = WindowStyle.None,
				ResizeMode = ResizeMode.NoResize,
				ShowInTaskbar = false,
				Topmost = true,
				Background = Brushes.Black,
				Cursor = Cursors.None,
				ShowActivated = screen.Primary,
				Left = screen.Bounds.Left,
				Top = screen.Bounds.Top,
				Width = screen.Bounds.Width,
				Height = screen.Bounds.Height
			};

			window.PreviewKeyDown += OnPreviewKeyDown;
			window.PreviewMouseDown += OnPreviewMouseDown;
			window.PreviewMouseWheel += OnPreviewMouseWheel;
			window.MouseMove += OnMouseMove;

			_windows.Add(window);

			window.Show();

			var hwnd = new WindowInteropHelper(window).Handle;
			SetWindowPos(
				hwnd,
				HWND_TOPMOST,
				screen.Bounds.X,
				screen.Bounds.Y,
				screen.Bounds.Width,
				screen.Bounds.Height,
				SWP_SHOWWINDOW);

			if (screen.Primary)
			{
				primaryWindow = window;
			}
		}

		if (primaryWindow is not null)
		{
			primaryWindow.Focus();
			SetForegroundWindow(new WindowInteropHelper(primaryWindow).Handle);
		}

		BlackoutChanged?.Invoke(null, EventArgs.Empty);
	}

	public static void Dismiss()
	{
		if (!IsBlackoutActive)
			return;

		var windowsToClose = _windows.ToArray();
		_windows.Clear();

		foreach (var window in windowsToClose)
		{
			try
			{
				window.PreviewKeyDown -= OnPreviewKeyDown;
				window.PreviewMouseDown -= OnPreviewMouseDown;
				window.PreviewMouseWheel -= OnPreviewMouseWheel;
				window.MouseMove -= OnMouseMove;
				window.Close();
			}
			catch
			{
			}
		}

		BlackoutChanged?.Invoke(null, EventArgs.Empty);
	}

	private static void OnPreviewKeyDown(object sender, KeyEventArgs e)
	{
		Dismiss();
		e.Handled = true;
	}

	private static void OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
	{
		if ((DateTime.UtcNow - _activatedTime).TotalMilliseconds < GracePeriodMs)
			return;

		Dismiss();
		e.Handled = true;
	}

	private static void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
	{
		if ((DateTime.UtcNow - _activatedTime).TotalMilliseconds < GracePeriodMs)
			return;

		Dismiss();
		e.Handled = true;
	}

	private static void OnMouseMove(object sender, MouseEventArgs e)
	{
		if ((DateTime.UtcNow - _activatedTime).TotalMilliseconds < GracePeriodMs)
		{
			GetCursorPos(out _initialCursorPos);
			return;
		}

		if (GetCursorPos(out var current))
		{
			var dx = Math.Abs(current.X - _initialCursorPos.X);
			var dy = Math.Abs(current.Y - _initialCursorPos.Y);
			if ((dx > MoveThresholdPx) || (dy > MoveThresholdPx))
			{
				Dismiss();
			}
		}
	}
}
