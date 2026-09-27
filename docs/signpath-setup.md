# SignPath Foundation signing integration

The current [v0.1.0 installer](https://github.com/CyanQX/VoxMate-Windows/releases/tag/v0.1.0) is unsigned. An application for free SignPath Foundation signing has been submitted and is awaiting review. Only after SignPath Foundation approves the project and the certificate and project are configured can this repository's GitHub Actions workflow produce a trusted signed installer. SignPath alone decides whether to approve the application.

## Prepared files

- [Code signing policy](code-signing-policy.md): public signing scope, maintainer roles, and privacy information.
- [Installer workflow](../.github/workflows/package.yml): builds on a GitHub-hosted Windows runner, stores the unsigned EXE as an Actions artifact, and optionally submits it to SignPath.
- [Artifact Configuration](../signing/signpath-artifact-configuration.xml): restricts signing to `VoxMate-Setup-${version}-win-x64.exe` and enforces the product name, product version, file version, and company name.

## Configuration after approval

Before configuration, maintainers must enable GitHub two-factor authentication using the [official GitHub instructions](https://docs.github.com/en/authentication/securing-your-account-with-two-factor-authentication-2fa/configuring-two-factor-authentication) and enable multi-factor authentication for their SignPath accounts.

1. Follow SignPath's approval notice to activate the organization and VoxMate project, and add GitHub.com as a Trusted Build System. If the signing policy requires GitHub audit logs or source code policy verification, install the SignPath GitHub App as directed and grant only the required access to this repository.
2. Create an Artifact Configuration in the SignPath project using the contents of `signing/signpath-artifact-configuration.xml`, and record its slug. Configure the SignPath Foundation certificate and a Signing Policy requiring manual approval of every signing request. Use the organization and project values supplied by SignPath.
3. In GitHub repository Settings → Secrets and variables → Actions, create the secret `SIGNPATH_API_TOKEN` and the variables `SIGNPATH_ORGANIZATION_ID`, `SIGNPATH_PROJECT_SLUG`, `SIGNPATH_SIGNING_POLICY_SLUG`, and `SIGNPATH_ARTIFACT_CONFIGURATION_SLUG`. Give the token only the permissions required to submit signing requests. Never commit it.
4. In GitHub Actions → **Build Windows installer** → Run workflow, choose `main` and set `sign_with_signpath` to `true`. The SignPath approver checks the source, build record, and version before manually approving the request. The workflow then verifies the Authenticode status, publisher, and version and computes the signed file's SHA-256 checksum.
5. Download the `VoxMate-signed-installer` Actions artifact. Recheck its Authenticode signature and SHA-256 checksum, then attach it to the matching GitHub Release with a clear signing status and a link to the code signing policy. An unsigned installer's checksum does not apply to the signed file.

The workflow defaults to `sign_with_signpath=false`, so it can verify the build before approval, but the output remains unsigned. The source repository's `main` branch contains no EXE or DLL files, models, or signing keys.

SignPath Foundation requires multi-factor authentication for all maintainers on GitHub and SignPath. It also requires verifiable project reputation for downloadable executable software. A new project's application may be rejected; download counts, user feedback, and media coverage must never be fabricated.

References: [SignPath Foundation conditions](https://signpath.org/terms.html), [GitHub Trusted Build System](https://docs.signpath.io/trusted-build-systems/github), and [Artifact Configuration syntax](https://docs.signpath.io/artifact-configuration/syntax).
