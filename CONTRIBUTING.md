# How to contribute
It is highly encouraged that you contribute to this project.  Please [open an issue](https://github.com/josemesona/ManagedWimgApi/issues/new) first so that a
discussion can be had around the overall implementation.

Once your issue has been discussed, clone this repository and open the Solution File in Visual
Studio or the source code in your favorite editor.  To compile the code, build in Visual Studio
or run MSBuild.exe against src\dirs.proj.

# Unit tests
The Windows Imaging API (WimgApi) requires that operations run in an elevated context.  This means
that the unit tests must be run as Administrator as well.  Please launch Visual Studio as an
Administrator before running the unit tests.  The unit tests can also be run from an elevated
command-line through vstest.console.exe.

The unit tests use [NUnit](http://www.nunit.org/) as the framework and [Shouldly](http://docs.shouldly-lib.net/) for assertions.  Please consider developing
unit tests for any new functionality or to cover issues found.