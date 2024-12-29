# Contributing to NeatShift

Hey there! Thanks for considering contributing to NeatShift. Whether you're fixing bugs, adding features, or improving documentation, your help makes NeatShift better for everyone.

## Branch Strategy

We use a two-branch system:
- `main` - stable branch for releases
- `develop` - branch for active development and contributions

Always create your changes in `develop`, not `main`!

## Quick Start

1. Fork the repo
2. Clone your fork: `git clone https://github.com/your-username/NeatShift.git`
3. Create your feature branch from `develop`: `git checkout develop && git checkout -b cool-new-feature`
4. Make your changes
5. Test your changes
6. Commit: `git commit -m 'Add some feature'`
7. Push: `git push origin cool-new-feature`
8. Open a Pull Request targeting the `develop` branch

## Development Setup

1. Make sure you have .NET 6.0 SDK installed
2. Open the solution in Visual Studio 2022 or your preferred IDE
3. Restore NuGet packages
4. Build and run!

## What Can I Work On?

- Check out our [open issues](https://github.com/BytexGrid/NeatShift/issues)
- Look for issues tagged with `good-first-issue` if you're just starting
- Feel free to propose new features through issues

## Code Style

We keep it simple:
- Use standard C# naming conventions
- Keep methods focused and small
- Add comments for non-obvious code
- Include XML documentation for public APIs

## Making Changes

1. **Always Branch from Develop**: Your changes should be based on the `develop` branch
2. **Keep Changes Small**: Smaller PRs are easier to review
3. **Write Good Commit Messages**: Explain what and why, not how
4. **Update Documentation**: If you change functionality, update the docs
5. **Add Tests**: New features should include tests
6. **Test Your Changes**: Make sure everything still works!

## Pull Request Process

1. Make sure your PR targets the `develop` branch
2. Update the README.md if needed
3. Make sure your code builds clean without warnings
4. Write a good PR description explaining:
   - What you changed
   - Why you changed it
   - How to test it
5. Link any related issues

## Need Help?

- Check out our [documentation](https://github.com/BytexGrid/NeatShift/wiki)
- Create an issue with your question
- Join our discussions

## Bug Reports

Found a bug? Please include:
- Steps to reproduce
- What you expected to happen
- What actually happened
- Screenshots if relevant
- Your environment (OS, .NET version, etc.)

## Feature Requests

Have an idea? Great! Create an issue and:
- Explain the feature
- Why it would be useful
- How it should work
- Any implementation ideas you have

## Code Review Process

1. At least one maintainer will review your PR
2. We might suggest changes
3. Once approved, it will be merged into `develop`
4. Changes in `develop` will be periodically merged to `main` for releases

Thanks for contributing to NeatShift! 🚀
