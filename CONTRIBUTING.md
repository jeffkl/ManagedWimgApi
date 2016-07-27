# How to contribute
It is highly encouraged that you contribute to this project.  Please open an issue first so that a
discussion can be had around the overall implementation.

Once your issue has been discussed, clone this repository and open the Solution File in Visual
Studio or the source code in your favorite editor.  To compile the code, build in Visual Studio
or run MSBuild.exe against src\dirs.proj.

# Unit tests
The Windows Imaging API (WimgApi) requires that operations run in an elevated context.  This means
that the unit tests must be run as Administrator as well.  Please launch Visual Studio as an
Administrator before running the unit tests.  The unit tests can also be run from an elevated
command-line through vstest.console.exe.

The unit tests use NUnit as the framework and Shouldly for assertions.  Please consider developing
unit tests for any new functionality or to cover issues found.