# Building and releasing on GitHub

## 1. Run the GitHub build

Every push to `master` starts the **Windows CI** workflow automatically.

1. Open the repository's **Actions** tab.
2. Select **Windows CI**.
3. Open the run for the target commit.
4. Confirm that both the `Debug` and `Release` jobs pass.

You can also select **Run workflow** to rebuild the current `master` branch
without creating another commit.

## 2. Download and test the build

At the bottom of the successful workflow run, download the
`Monitorian-release-<commit>` artifact. It contains:

- `Monitorian.exe`
- `Monitorian-Portable.exe`
- standard and portable ZIP archives
- `SHA256SUMS.txt`

Extract the artifact and test both executables on Windows 10 and Windows 11.
At minimum, verify monitor discovery, brightness and contrast changes, tray
scrolling, wake restoration, language switching, settings persistence, and the
documented CLI commands.

Hardware-dependent and interactive tray behavior cannot be fully validated by
the hosted GitHub runner.

## 3. Publish the release

After testing the artifact:

1. Open **Releases** and select **Draft a new release**.
2. Create a tag matching the application version, for example `v2.3.0`, from
   the tested commit.
3. Set the title to `Monitorian 2.0 v2.3.0`.
4. Copy the matching section from `CHANGELOG.md` into the release notes.
5. Upload the two executables, two ZIP archives, and `SHA256SUMS.txt`.
6. Mark it as the latest release and publish.
7. Verify every public download and compare its SHA-256 hash with
   `SHA256SUMS.txt`.

Do not publish a release from a failed or untested workflow run.

## Local equivalent

Maintainers with Visual Studio 2022 Build Tools can produce the same versioned
archives locally:

```powershell
.\build-release.ps1 -Version "2.3.0"
```
