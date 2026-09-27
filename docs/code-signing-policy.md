# Code signing policy

Project maintainer [CyanQX](https://github.com/CyanQX) maintains the VoxMate source code and build scripts. The maintainer reviews outside contributions, checks the origin of release builds, and approves each release signing request. The [privacy policy](privacy.md) describes how the app handles local recordings, text, and history.

An application has been submitted for the free service: **Free code signing provided by [SignPath.io](https://signpath.io), certificate by [SignPath Foundation](https://signpath.org)**. SignPath Foundation has not approved VoxMate, and no VoxMate release has been signed with its certificate. Installers released before approval are clearly labeled unsigned.

Signing is limited to official releases built from this project's public source code and build scripts. Release files must be produced by a GitHub-hosted build, retain verifiable product and version metadata, and receive a new SHA-256 checksum after signing. This project's signing identity will not be used to sign third-party whisper.cpp, llama.cpp, or other upstream binaries.

At present, [CyanQX](https://github.com/CyanQX) serves as the author and committer, reviewer of outside contributions, and approver of every release signing request. Team roles and review rules will be updated in the public repository if more maintainers join. Maintainers must use multi-factor authentication for both the repository and signing service accounts. Private keys, tokens, and passwords must never be committed to the repository.
