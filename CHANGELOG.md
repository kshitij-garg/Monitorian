# Changelog

Notable changes are documented here. This project follows semantic versioning
where practical.

## 2.3.0 - 2026-09-16

### Added

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
- Corrected a WPF scrollbar style target mismatch that crashed the application
  whenever the tray settings menu was opened.
- Made satellite-resource packaging independent of `SolutionDir`.
- Replaced the static build badge with the real workflow status.
- Removed an unsupported OSD setting claim from the README.

## 2.2.0 - 2026-09-16

- Added the in-app language selector and reorganized settings menu.
- Added Hindi, Bengali, Marathi, Telugu, and Tamil localizations.
- Added portable single-file release packaging.

For older changes, see the Git history and GitHub releases.
