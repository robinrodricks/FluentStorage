# Release Notes

#### 5.0.1
 - Fix : Implementation of `LocalDiskMessenger.StartProcessorAsync` (issue [#14](https://github.com/robinrodricks/FluentStorage/issues/14))(`netstandard2.1` / `net6.0` and above

#### 5.0.0
 - New: Introducing the FluentStorage set of libraries created from Storage.NET
 - New: Added SFTP provider using [SSH.NET](https://github.com/sshnet/SSH.NET)
 - Fix: FTP provider [FluentFTP](https://github.com/robinrodricks/FluentFTP) updated to v44
 - Fix: AWS Nugets bumped to latest versions as of Jan 2023
 - Fix: All nuget packages now target `netstandard2.0`,`netstandard2.1`,`net50`,`net60`
 - Change: Refactored package structure to simplify naming scheme
 - Change: Refactored entire codebase to simplify code organization
 - New: Documentation wiki created with one page per provider