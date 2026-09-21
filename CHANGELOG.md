# Changelog

Notable changes are documented here. This project follows semantic versioning
where practical.

## 2.4.0-beta1 - 2026-09-21

### Added

- **Quick Access at Cursor Position (Issue #238):** Summon the Monitorian slider flyout directly under the mouse cursor anywhere across multi-monitor desktops using the global shortcut `Win + Alt + M` or settings toggle, eliminating the need to move cursor to the notification tray.
- **Automated Unit Tests:** Added `QuickAccessAndHdrTest.cs` validating cursor edge-clamping, hotkey wiring, settings deserialization defaults, and GDI index parsing.

### Fixed

- **Multi-Monitor HDR Brightness Disambiguation (Issue #756):** Resolved tandem brightness adjustment on dual/triple identical HDR monitors (e.g. LG UltraGear). Corrected DisplayConfig path mapping using GDI source device names and guaranteed unique `(AdapterId, TargetId)` `DisplayIdSet` assignments per monitor.

## 2.3.0 - 2026-09-20

### Added

- Global keyboard shortcuts (`Win+Alt+Up`/`Down` for brightness adjustments with OSD feedback, `Win+Alt+B` for instant blackout).
- Energy-efficient scheduled day/night brightness transitions with 0% continuous background CPU overhead.
- Middle-click tray icon screen blackout: Instant pitch-black multi-monitor overlay for privacy and resting displays; restores immediately on any key press, mouse click, or mouse movement.
- Configurable toggles for global hotkeys, scheduled brightness, and middle-click blackout in the settings menu under Input & Automation.
- Comprehensive competitive benchmark and SWOT analysis in docs/BENCHMARK_AND_SWOT.md.
- Windows CI for Debug and Release builds, unit tests, localization validation,
  packaging checks, and startup/CLI smoke tests.
- Versioned GitHub build artifacts with standard and portable ZIP archives and
  SHA-256 checksums.
- Automated CodeQL scanning and Dependabot updates.
- Structured bug and feature request forms, a pull request template, security
  policy, and code of conduct.
- CLI support for monitor targeting, relative brightness adjustments, and
  contrast get/set commands.
- Automated CLI parser and localization tests.
- Maintainer documentation for building and publishing releases from GitHub.

### Fixed

- Prevented a window lifecycle race when changing the menu language.
- Fixed middle-click tray icon blackout on Windows 11 by introducing multi-layered detection (WPF overlay window, WinForms events, and low-level shell callback message interception in WndProc) with 500ms deduplication.
- Fixed settings deserialization default value loss in DataContractSerializer using [OnDeserializing] callback so upgrades with pre-existing settings files retain true default states.
- Corrected a WPF scrollbar style target mismatch that crashed the application
  whenever the tray settings menu was opened.
- Added a global exception policy that logs and recovers from non-fatal WPF
  Dispatcher and unobserved task exceptions, plus guarded startup and shutdown.
- Hardened `ProductInfo` reflection with executing-assembly fallbacks and null-safe
  attribute queries to prevent `NullReferenceException` in headless or test runners.
- Made satellite-resource packaging independent of `SolutionDir`.
- Replaced the static build badge with the real workflow status.
- Removed an unsupported OSD setting claim from the README.

## 2.2.0 - 2026-09-16

- Added the in-app language selector and reorganized settings menu.
- Added Hindi, Bengali, Marathi, Telugu, and Tamil localizations.
- Added portable single-file release packaging.

For older changes, see the Git history and GitHub releases.
