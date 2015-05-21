# msbuild-front
A simple stupid way to share front end assets between msbuild projects

## What does it do?

It force you to follow two conventions
- your front end stuff are in folder called 'app' at the root of your project
- you split your files into 'modules' so that the 'app' folder contains only module folders

Then, msbuild-front will automatically create symbolic links to the modules of your dependencies right in your 'app' folder.
Done!
You can use them as if they were in your project (link html, css, images with relative paths)

## How to install and use it?

- `nuget install msbuild-front`
- build from Visual Studio or `msbuild` on the command line

## Contact

You can contact me at manitra (at) manitra (dot) net
