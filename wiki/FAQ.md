# Frequently Asked Questions (FAQ)

## General Questions

### What is NeatShift?
NeatShift is a Windows application that helps you organize files and folders while maintaining their accessibility through symbolic links.

### Is NeatShift free?
Yes, NeatShift is free software licensed under the GNU General Public License v3.0. You can use, modify, and distribute it freely under the terms of the GPL.

### What operating systems are supported?
Windows 7 SP1 or later, with Windows 10/11 recommended for best experience.

## Installation

### Why do I see a SmartScreen warning?
This is normal for new applications without an expensive code signing certificate. The warning appears because Windows doesn't recognize our publisher yet. You can safely proceed by clicking "More info" → "Run anyway".

### Do I need to install anything?
No, NeatShift is portable and doesn't require installation. Just download and run.

### Why does it need administrator privileges?
Administrator rights are required to:
- Create symbolic links (Windows security requirement)
- Create system restore points
- Move protected files

## Features

### What is a symbolic link?
A symbolic link is a special file that points to another file or folder. It acts like a shortcut but works at a system level, making it seamless for applications.

### Will moving files break my programs?
No! That's the whole point of NeatShift. When files are moved, symbolic links are created in their original location, so programs continue to work normally.

### Can I undo operations?
Yes, in two ways:
1. Use the system restore point created before operations
2. Delete symbolic links and move files back manually

### Is there a file size limit?
No, but larger files take longer to move. The speed depends on your disk performance.

## Troubleshooting

### The "Move" button is disabled
Check:
1. You have selected a destination
2. You have added files to move
3. No operation is currently in progress

### "Access Denied" errors
Ensure:
1. You're running as administrator
2. Files aren't in use by other programs
3. You have permissions for both source and destination

### Symbolic link creation failed
Common causes:
1. Not running as administrator
2. Target path too long
3. Invalid destination

## Security

### Is it safe to use?
Yes! NeatShift is:
1. Open source - code can be reviewed
2. Signed with Sigstore
3. Creates restore points
4. Doesn't collect data

### How can I verify the download?
See our [Security & Verification](Security-and-Verification) guide for instructions on verifying Sigstore signatures.

## Support

### Where can I report bugs?
Use our [GitHub Issues](https://github.com/BytexGrid/NeatShift/issues) page.

### How do I request features?
1. Check existing issues first
2. Create a new issue with the "enhancement" label
3. Describe your feature request in detail

### Where can I get help?
1. Check this FAQ
2. Read the [Usage Guide](Usage-Guide)
3. Create a GitHub issue
4. Contact support through GitHub 