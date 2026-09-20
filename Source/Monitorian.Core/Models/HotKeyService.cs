using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace Monitorian.Core.Models;

/// <summary>
/// Service to register and manage global keyboard shortcuts (Win+Alt+Up/Down for brightness, Win+Alt+B for blackout).
/// </summary>
public class HotKeyService : IDisposable
{
	#region Win32

	[DllImport("user32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

	[DllImport("user32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

	private const uint MOD_ALT = 0x0001;
	private const uint MOD_WIN = 0x0008;
	private const uint MOD_NOREPEAT = 0x4000;

	private const uint VK_UP = 0x26;
	private const uint VK_DOWN = 0x28;
	private const uint VK_B = 0x42;

	private const int WM_HOTKEY = 0x0312;

	public const int HotKeyBrightnessUpId = 1001;
	public const int HotKeyBrightnessDownId = 1002;
	public const int HotKeyBlackoutToggleId = 1003;

	#endregion

	private HwndSource _source;
	private IntPtr _handle;
	private readonly HashSet<int> _registeredIds = [];
	private bool _isDisposed;

	public event Action BrightnessUpRequested;
	public event Action BrightnessDownRequested;
	public event Action BlackoutToggleRequested;

	public bool IsRegistered => _registeredIds.Count > 0;

	public bool Register()
	{
		if (_isDisposed)
			return false;

		EnsureWindow();

		if (_handle == IntPtr.Zero)
			return false;

		// Win + Alt + Up
		RegisterSingle(HotKeyBrightnessUpId, MOD_WIN | MOD_ALT | MOD_NOREPEAT, VK_UP);

		// Win + Alt + Down
		RegisterSingle(HotKeyBrightnessDownId, MOD_WIN | MOD_ALT | MOD_NOREPEAT, VK_DOWN);

		// Win + Alt + B
		RegisterSingle(HotKeyBlackoutToggleId, MOD_WIN | MOD_ALT | MOD_NOREPEAT, VK_B);

		return IsRegistered;
	}

	private bool RegisterSingle(int id, uint modifiers, uint vk)
	{
		if (_registeredIds.Contains(id))
			return true;

		if (RegisterHotKey(_handle, id, modifiers, vk))
		{
			_registeredIds.Add(id);
			return true;
		}

		return false;
	}

	public void Unregister()
	{
		if (_handle == IntPtr.Zero || _registeredIds.Count == 0)
			return;

		foreach (var id in _registeredIds)
		{
			try
			{
				UnregisterHotKey(_handle, id);
			}
			catch
			{
			}
		}

		_registeredIds.Clear();
	}

	private void EnsureWindow()
	{
		if (_source is not null)
			return;

		var parameters = new HwndSourceParameters("MonitorianHotKeySink")
		{
			Width = 0,
			Height = 0,
			WindowStyle = 0,
			HwndSourceHook = HwndHook
		};

		_source = new HwndSource(parameters);
		_handle = _source.Handle;
	}

	private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
	{
		if (msg == WM_HOTKEY)
		{
			var id = wParam.ToInt32();
			switch (id)
			{
				case HotKeyBrightnessUpId:
					BrightnessUpRequested?.Invoke();
					handled = true;
					break;

				case HotKeyBrightnessDownId:
					BrightnessDownRequested?.Invoke();
					handled = true;
					break;

				case HotKeyBlackoutToggleId:
					BlackoutToggleRequested?.Invoke();
					handled = true;
					break;
			}
		}

		return IntPtr.Zero;
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
			Unregister();

			if (_source is not null)
			{
				_source.RemoveHook(HwndHook);
				_source.Dispose();
				_source = null;
				_handle = IntPtr.Zero;
			}
		}

		_isDisposed = true;
	}
}
