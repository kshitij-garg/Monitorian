# Monitorian 2.0 (v2.4.0-beta1) - Quick Access & Multi-HDR Beta Release

Monitorian 2.0 v2.4.0-beta1 is an experimental beta release addressing two of the most requested community issues from upstream `emoacht/Monitorian`: Quick Access flyout at cursor position across multi-monitor setups (Issue #238) and independent brightness control for identical multi-monitor HDR displays (Issue #756).

### 🌟 Key New Features & Fixes in Beta

- 🎯 **Quick Access at Cursor Position (Issue #238):**
  - Summon the Monitorian slider flyout directly under the mouse cursor anywhere across multi-monitor setups via the global shortcut **Win + Alt + M** or via the settings toggle.
  - Automatically calculates per-monitor DPI scaling based on cursor position (`VisualTreeHelperAddition.GetDpi`) and clamps the window within the active monitor's work area boundaries so it never overflows off-screen.
  - Configurable toggle under **INPUT & AUTOMATION** in the settings menu.

- 🖥️ **Multi-Monitor HDR Brightness Disambiguation (Issue #756):**
  - Resolves the issue where dual or triple identical HDR monitors (e.g. dual LG 27GP950 / UltraGear) adjust in tandem rather than independently.
  - Uses `DISPLAYCONFIG_DEVICE_INFO_GET_SOURCE_NAME` in `DisplayConfig.cs` to correlate Windows GDI device view names (`\\.\DISPLAY1`, `\\.\DISPLAY2`) with hardware paths.
  - Disambiguates identical display entries during enumeration and consumes matched candidates to assign unique `DisplayIdSet` target IDs per physical display.

- 🧪 **Automated Testing Suite (67/67 Tests Passing):**
  - Added `QuickAccessAndHdrTest.cs` covering cursor work area clamping, hotkey registration and event handling, settings deserialization defaults, and GDI view name parsing.
  - 100% test pass rate across all 67 unit tests.

---

### Downloads & Verification

No installer or administrative privileges required. Self-contained single-file binaries with all dependencies embedded:

| Asset | Description | Size | SHA-256 Checksum |
| :--- | :--- | :--- | :--- |
| **[Monitorian.exe](https://github.com/kshitij-garg/Monitorian-2.0/releases/download/v2.4.0-beta1/Monitorian.exe)** | True Single-File Standalone Executable | ~1.32 MB | `cab5148d3e510d05b2ba57ed2544c29f3b311b08119163a808d2f45ab6fae43d` |
| **[Monitorian-Portable.exe](https://github.com/kshitij-garg/Monitorian-2.0/releases/download/v2.4.0-beta1/Monitorian-Portable.exe)** | Zero-Config Portable Executable (local settings) | ~1.32 MB | `cab5148d3e510d05b2ba57ed2544c29f3b311b08119163a808d2f45ab6fae43d` |
| **[Monitorian-2.4.0-beta1.zip](https://github.com/kshitij-garg/Monitorian-2.0/releases/download/v2.4.0-beta1/Monitorian-2.4.0-beta1.zip)** | Complete Standard Release Archive (all 28 languages) | ~1.34 MB | `1a15cdce7efcb411e219a3e31bd275f248656350426369428df160f69ce5e617` |
| **[Monitorian-Portable-2.4.0-beta1.zip](https://github.com/kshitij-garg/Monitorian-2.0/releases/download/v2.4.0-beta1/Monitorian-Portable-2.4.0-beta1.zip)** | Complete Portable Release Archive (with portable.ini) | ~1.34 MB | `48c9c2f96477a605b74f1636eb9be647740f47ee31bbf5bdfaae14754302968f` |

Checksums manifest: `artifacts/release/SHA256SUMS.txt`
