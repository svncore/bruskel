# Mac App Store readiness

Bruskel keeps its Mac App Store permissions explicit in
`installer/macos/Bruskel.AppStore.entitlements.plist`.

The Avalonia UI uses the platform file picker for opening and saving documents. This is
important for App Sandbox: the user grants access to a selected file or folder through the
system panel instead of Bruskel receiving unrestricted filesystem access.

Before submitting a build, the release job must still:

1. sign the app with the Mac App Store distribution certificate;
2. embed the entitlements while signing every nested native framework and helper;
3. package the signed app as an installer package (`.pkg`), not a DMG;
4. validate the package with Apple's submission tooling and upload it to App Store Connect.

The DMG workflow remains the direct-distribution path and should continue using Developer ID
signing and notarization separately.
