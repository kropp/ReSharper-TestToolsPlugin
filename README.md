# ReSharper TestTools

This plugin adds following actions:

* Generate test fixture stub for given class
* Ignore/unignore test from Unit Test Session
* Mark test with category from Unit Test Session (with suggests based on categories used in solution)
* Run any method w/o parameters as test

Only NUnit and ReSharper 8.0 are supported.

## Install ##

Open ReSharper - Extension Manager&hellip; and find TestTools in ReSharper Gallery.

## Build ##

To build the source, you need the [ReSharper 8.0 SDK](http://confluence.jetbrains.com/display/ReSharper/ReSharper+8+EAP) installed.

Simply build solution using VS 2012. (Projects targets .NET 4.0, so it should work in VS 2010 as well)

## Contributing ##

Feel free to [raise issues on GitHub](https://github.com/kropp/ReSharper-TestTools/issues), or [fork the project](http://help.github.com/fork-a-repo/) and [send a pull request](http://help.github.com/send-pull-requests/).