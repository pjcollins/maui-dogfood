.NET MAUI Dogfooder
===============

## ⛭ Configuration

 * Clone or download this repository into a parent folder at `$HOME/maui-previews`.
 * Configure a classic GitHub PAT with full "repo" scope, and enable SSO for the xamarin org.
    * Save this token in a file named `github.token` in a `tokens` folder in your dogfood root folder (e.g. `maui-previews/tokens`).
 * For internal shipping feeds, you will need to generate a [dnceng PAT][0] with "Packaging" -> "Read" scope.
    * Save your user name and token in a file named `dnceng.token` in a `tokens` folder in your dogfood root folder (e.g. `maui-previewer/tokens`). The file should look like this:
        ```
        foo@bar.com
        token
        ```

Your directory tree will end up looking like this:

```
maui-previews
├── _cache
├── _logs
├── _temp
├── dotnet
├── maui-dogfood
└── tokens
    └── github.token
    └── dnceng.token
```

[0]: https://dev.azure.com/dnceng/_usersSettings/tokens


## 🦴 How to Dogfood

 * Visit the .NET Release Tracker to find the .NET SDK artifacts and feed for the release you want to dogfood.
 * Download the appropriate .NET SDK installer from 'Published artifacts' -> 'shipping' -> 'assets' -> 'Sdk' -> '$version' -> 'dotnet-sdk-$version-$platform.[pkg|msi]'
 * Run the tool/app, set the path to the preview SDK you downloaded, and select the Android, MaciOS, and Maui commits you want to install.
 * Create a template, build it, and run it using the `./maui-previews/dotnet/dotnet` tool.

#### VS Code

##### macOS

 * Edit `~/.zprofile` (or your preferred shell profile) to prefix $PATH with this dotnet install path:
   ```
   # Use maui-previews dotnet
   export PATH="$HOME/maui-previews/dotnet:$PATH"
   ```
 * Open VS Code from the command line with `code .` in your test project folder.
