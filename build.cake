// --- Arguments --------------------
var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
// ----------------------------------

var solutionDir = "./src/";
var solution = "./src/build-demo.sln";
var projectName = "build-demo";

var artifactsDir  = "./artifacts";
var mergedArtifact = artifactsDir + "/merged/build-demo-console.exe";
var testingDir = artifactsDir + "/tests";

// --- Nuget ------------------------
var nugetBinary = "./tools/nuget.exe";
var nugetPackageDir = "./artifacts/nuget";
var nugetSources = new List<String>(){"http://nuget.netclean.com/nuget", "https://www.nuget.org/api/v2"};
// ----------------------------------


// --- Helper functions--------------
void CreateOrCleanDirectory(string dir){

    if (!DirectoryExists(dir))
    {
        CreateDirectory(dir);
    }
    else {
        CleanDirectories(dir);
    }
}

void RunUnitTests(string testDir) {

    NUnitSettings settings = new NUnitSettings();
    settings.AppDomainUsage = NUnitAppDomainUsage.None;

    NUnit(testDir + "/*.test.dll", settings);
}

// -----------------------------------

Task("Clean")
    .Does(() =>
{
    var parsedSolution = ParseSolution(solution);
    foreach(var project in parsedSolution.Projects)
    {
        var projectBuildDir = project.Path.GetDirectory().ToString() + "/bin";
        Information(@"Cleaning {0}", projectBuildDir);
        CleanDirectory(projectBuildDir);
    }

    CreateOrCleanDirectory(artifactsDir);
});

Task("Restore-NuGet-Packages")
    .IsDependentOn("Clean")
    .Does(() =>
{
    NuGetRestoreSettings settings = new NuGetRestoreSettings();
    settings.ToolPath = nugetBinary;
    settings.Source = nugetSources;
    settings.NoCache = true;

    NuGetRestore(solution, settings);
});


Task("Build")
    .IsDependentOn("Restore-NuGet-Packages")
    .Does(() =>
{
    if(IsRunningOnUnix()) {
        XBuild(solution, new XBuildSettings()
            .UseToolVersion(XBuildToolVersion.NET40)
            .SetVerbosity(Verbosity.Minimal)
            .SetConfiguration(configuration));
    }
    else {
        MSBuild(solution, new MSBuildSettings()
            .UseToolVersion(MSBuildToolVersion.NET45)
            .SetVerbosity(Verbosity.Minimal)
            .SetConfiguration(configuration));
    }
});

Task("Collect-Files")
    .IsDependentOn("Build")
    .Does(() =>
{
        var outputDir = artifactsDir + "/"+ projectName;
        CreateOrCleanDirectory(outputDir);
        CopyFiles("./src/" + projectName + "/bin/" + configuration + "/*", outputDir);

        var testDir = artifactsDir + "/tests";
        CreateOrCleanDirectory(testDir);


        CopyFiles("./src/" + projectName + ".test/bin/" + configuration + "/*", testDir);
        var dirsWithData = GetDirectories("./src/" + projectName + ".test/bin/" + configuration + "/*");

});

Task("Merge")
    .IsDependentOn("Collect-Files")
    .Does(() =>
{
    var assemblyPaths = GetFiles(artifactsDir + "/" +  projectName + "/*.dll");
    ILRepack(
        mergedArtifact,
        artifactsDir + "/" + projectName + "/build-demo.exe",
        assemblyPaths,
        new ILRepackSettings { Internalize = true }
        );
});

Task("Run-Unit-Tests")
    .IsDependentOn("Merge")
    .Does(() =>
{
    RunUnitTests(testingDir);
});

Task("Default")
    .IsDependentOn("Run-Unit-Tests");

RunTarget(target);