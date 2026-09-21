# Monitorian 2.0 (v2.3.0) - High Stability, Global Hotkeys & Automation Release

Monitorian 2.0 v2.3.0 is a premier release bringing system-wide keyboard shortcuts, automated day/night brightness transitions, instant multi-monitor blackout, Windows 11 system tray compatibility hardening, and complete resolution of runtime crashes.

### 🌟 Key New Features & Capabilities

- ⌨️ **Global Keyboard Shortcuts (HotKeyService):**
  - **Win + Alt + Up**: Increment monitor brightness by 5% with real-time acrylic OSD feedback.
  - **Win + Alt + Down**: Decrement monitor brightness by 5% with OSD feedback.
  - **Win + Alt + B**: Toggle multi-monitor pitch-black screen blackout instantly.
  - Uses an isolated message-only window sink (HWND_MESSAGE) with MOD_NOREPEAT to eliminate conflict and prevent key-repeat flooding.
  - Configurable toggle under **INPUT & AUTOMATION** in the settings menu.

- 🌓 **Automated Scheduled Day/Night Mode (ScheduleService):**
  - Smoothly transitions monitors between configurable daytime (default: 80% at 07:00) and nighttime (default: 30% at 20:00) brightness levels.
  - Ultra-efficient minute-boundary scheduler with 0% continuous background CPU burn.
  - Preserves interim manual slider adjustments until the next scheduled period change.

- 🌑 **Middle-Click Instant Screen Blackout (BlackoutService):**
  - Middle-clicking the notification tray icon instantly covers all connected displays with full-screen pitch-black overlays for OLED standby, cinema mode, or privacy.
  - Restores displays seamlessly on any key press, mouse click, or mouse movement.
  - Engineered with multi-layered detection (WPF overlay window, WinForms mouse events, and low-level Win32 shell message interception in WndProc) to ensure 100% reliability on Windows 11 taskbars.
  - Integrated 500ms deduplication and 16px motion threshold to prevent sensor jitter from prematurely dismissing the blackout.

- ⚡ **Free Native Command-Line Interface (CLI):**
  - Monitor targeting by name or device instance ID (e.g., Monitorian.exe /set "Dell U2720Q" 75).
  - Relative adjustments (e.g., Monitorian.exe /set +10, /set -15).
  - Full contrast inspection and control (/get contrast, /set contrast 60).

### 🛡️ Critical Fixes & Stability

- **Settings Deserialization Default Value Preservation:** Added [OnDeserializing] and SetDefaultValues() in SettingsCore. When upgrading with pre-existing settings.xml configurations, missing tags retain their true default states (EnablesMiddleClickBlackout = true, EnablesHotKeys = true) rather than being overwritten to false by DataContractSerializer.
- **Startup & Foreground Hardening:** Integrated WindowHelper.EnsureForegroundWindow (AttachThreadInput + SetForegroundWindow) and increased prevention threshold to eliminate startup focus lockout when launched or forwarded from Windows Explorer.
- **Process Robustness:** Enforced ShutdownMode="OnExplicitShutdown" to guarantee that background service windows and auxiliary dialogs never trigger premature application termination.
- **Tray Menu Crash Fixed:** Eliminated the WPF XamlParseException (PlainScrollBarStyle on ScrollViewer) ensuring smooth, crash-free settings menu navigation.
- **Language Switch Stability:** Resolved window lifecycle races during dynamic runtime culture switching across all 28 supported languages.
- **Future-Proofing & Testing:** Added automated unit tests bringing the MSTest suite to **62/62 passing tests**.

---

### Downloads & Verification

No installer or administrative privileges required. Self-contained with all DLL dependencies embedded via Costura.Fody:

| Asset | Description | Size | SHA-256 Checksum |
| :--- | :--- | :--- | :--- |
| **[Monitorian.exe](https://github.com/kshitij-garg/Monitorian-2.0/releases/download/v2.3.0/Monitorian.exe)** | True Single-File Standalone Executable | ~1.3 MB | 4A2E26B26025B0A6299046AED0958B6D058096194BB237E9A9867537FFF8E6C7 |
| **[Monitorian-Portable.exe](https://github.com/kshitij-garg/Monitorian-2.0/releases/download/v2.3.0/Monitorian-Portable.exe)** | Zero-Config Portable Executable (local settings) | ~1.3 MB | 4A2E26B26025B0A6299046AED0958B6D058096194BB237E9A9867537FFF8E6C7 |
| **[Monitorian-2.3.0.zip](https://github.com/kshitij-garg/Monitorian-2.0/releases/download/v2.3.0/Monitorian-2.3.0.zip)** | Complete Standard Release Archive (all 28 languages) | ~1.3 MB | DBA3B143A8723D5B05204B178F371E2E2191FEDC62A80D5C8CD5028AE60E9B4E |
| **[Monitorian-Portable-2.3.0.zip](https://github.com/kshitij-garg/Monitorian-2.0/releases/download/v2.3.0/Monitorian-Portable-2.3.0.zip)** | Complete Portable Release Archive (with portable.ini) | ~1.3 MB | D7D00BB93FDD0E36F6EC5F1DAC2584ACF02614C9F6E9540DBF73E9153FEFFF39 |
| **[SHA256SUMS.txt](https://github.com/kshitij-garg/Monitorian-2.0/releases/download/v2.3.0/SHA256SUMS.txt)** | Official SHA-256 Checksum Manifest | Text | Verification signature |
