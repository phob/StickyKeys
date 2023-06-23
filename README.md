# Sticky Keys Tool for Windows

## Overview

This tool allows you to programmatically enable Sticky Keys in Windows and ensure that the settings persist even after a system reboot.

## Features

- Enable Sticky Keys with specific settings.
- Persist Sticky Keys settings across system reboots by updating the registry.
- Basic error handling, including checks for proper registry access permissions.

## Usage

To use this tool, download the project and build it using a .NET compiler. The generated executable can be run from the command line.

Please ensure that you run the program with administrative privileges, as it needs to modify the Windows registry to persist Sticky Keys settings.

## Precautions

Please be aware that this tool modifies the Windows registry. Incorrect modifications to the registry can cause serious problems that may require you to reinstall your operating system. Always back up your registry before making changes.

## Future Improvements

- [ ] Provide a graphical user interface (GUI).
- [ ] Enhance error handling and logging.
- [ ] Include a 'restore' functionality that allows users to revert their settings from a backup file.
- [ ] Request necessary permissions programmatically or guide the user on how to grant these permissions.

## Contributions

Contributions are welcome! Please read the contributing guidelines before getting started.

## License

[MIT](https://choosealicense.com/licenses/mit/)
