# Contributing to Monitorian 2.0 Unofficial

> [!IMPORTANT]
> **This repository is the Monitorian 2.0 Unofficial fork.** 
> If you are looking for the original Monitorian repository or want to report an issue with the base software, please visit [emoacht/Monitorian](https://github.com/emoacht/Monitorian).
> Contributions here should specifically target the extended features (Portable Mode, Scroll OSD, Restore on Wake).

Thank you for your interest in contributing to this project.

## 1. General

- Read the [README](../README.md) before posting. Avoid asking questions that are already covered there.

- <ins>Be prepared to respond to any request</ins> after opening an issue, request or suggestion. In most cases, you will be requested to provide additional information.

- Do not send a comment by email. It will produce a lot of garbage and make it hard to read.

## 2. Issues

- Check the [system requirements](../README.md#system-requirements) and use the structured bug report form.

- Search [Issues](https://github.com/kshitij-garg/Monitorian-2.0/issues) to date. To search open and closed issues at once, remove `is:open` from default syntax in Filters box and use only `is:issue` syntax.

- Include all information necessary for a reader who has no knowledge about your monitor or other devices to understand and reproduce the issue.

- Attach relevant `exception.log`, `probe.log`, or `operation.log` files after removing sensitive information.

- An issue which includes no meaningful clue CAN BE CLOSED without review.

## 3. Feature requests or suggestions

- Do not open feature requests or suggestions in __Ideas__ of __Discussions__ in this project. This is for aggregating them in __Issues__.

- Search [Issues](https://github.com/kshitij-garg/Monitorian-2.0/issues) to date.

- This app's main focus and competence is brightness of monitors. This app uses DDC/CI for this purpose, and not vice versa. The importance for keeping focused cannot be overemphasized.

- <ins>Explain the necessity and added value of the feature.</ins> Describe how it would be useful in real-world scenarios or use cases. An existing feature of other apps is not a sufficient reason to include the same to this app.

- Include technical information to implement the feature as much as possible. A feature which is not technically feasible cannot be added anyways.

- A feature request or suggestion which is the same as already discussed CAN BE CLOSED without review.

## 4. Pull requests

### 4.1 PR for language

- Read [Community & Localization](../README.md#community--localization).
- Use phrases used by the OS as much as possible.
- Do not try to explain everything. You cannot create a flawless expression in a short sentence like menu item.
- Add or update localization tests when introducing a language.

### 4.2 PR for functionality

- <ins>Open an issue and get agreement for your proposed change before starting your work.</ins>
- Split the changes into dedicated commits based on their reasons and objectives. Avoid adding unrelated changes into a commit.
- It is recommended to use the latest features of C#, provided that they are supported in this project.
- Run the Debug and Release builds and the MSTest suite. Pull requests must pass Windows CI.
