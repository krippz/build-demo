# build-demo
A simple demo of a self-contained build.

This repo has two build scripts:
- build.sh for Linux and OS X
- buils.ps1 for Windows, don't forget to `Set-ExecutionPolicy Unrestricted`

Clone this repo down to your machine `cd` to the `build-demo` folder or what you prefered to call it, and type `build.sh` or `build.ps1`


### What it does
The repo contains two projects, one console app and a test for that app
The console app has two dependencies in for of nuget packages, these are ment to merged into the final console app.

The build will do the following:
- Get NuGet.exe
- Get build dependencies, Cake, ILRepack, NUnit.Runners
- Build
- Copy to artifacts folder
- ILRepack
- Run tests
