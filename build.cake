#tool nuget:https://www.myget.org/F/nunit/api/v3/index.json?package=NUnit.ConsoleRunner&version=3.9.0-dev-03938

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

//////////////////////////////////////////////////////////////////////
// SET ERROR LEVELS
//////////////////////////////////////////////////////////////////////

var ErrorDetail = new List<string>();

//////////////////////////////////////////////////////////////////////
// SET PACKAGE VERSION
//////////////////////////////////////////////////////////////////////

var version = "1.0.0";
var modifier = "";

var isAppveyor = BuildSystem.IsRunningOnAppVeyor;
var dbgSuffix = configuration == "Debug" ? "-dbg" : "";
var packageVersion = version + modifier + dbgSuffix;
var suffix = string.Empty;

//////////////////////////////////////////////////////////////////////
// SUPPORTED FRAMEWORKS
//////////////////////////////////////////////////////////////////////

var AllFrameworks = new string[]
{
    "netstandard2.0"
};

//////////////////////////////////////////////////////////////////////
// DEFINE RUN CONSTANTS
//////////////////////////////////////////////////////////////////////

var PROJECT_DIR = Context.Environment.WorkingDirectory.FullPath + "/";

Information("Project dir {0}", PROJECT_DIR);

var ARTIFACTS_DIR = PROJECT_DIR + "artefacts/";
var BIN_DIR = PROJECT_DIR + "bin/" + configuration + "/";
var SDK_BIN_DIR = PROJECT_DIR + "commercetools.NET/bin/" + configuration + "/";
var TEST_BIN_DIR = PROJECT_DIR + "commercetools.NET.Tests/bin/" + configuration + "/";
var IMAGE_DIR = PROJECT_DIR + "images/";

var SOLUTION_FILE = "./commercetools.NET.sln";

var TEST_PROJECT_DIR = PROJECT_DIR + "commercetools.NET.Tests/";
var SDK_PROJECT_DIR = PROJECT_DIR + "commercetools.NET/";

// Test Runners
var NUNITLITE_RUNNER_DLL = "nunitlite-runner.dll";

// Test Assemblies
var SDK_TESTS = "commercetools.NET.tests.dll";

// Packages
var ZIP_PACKAGE = ARTIFACTS_DIR + "commercetools.NET-" + packageVersion + ".zip";

//Crate a artifacts directory
var artifactsDirectory = Directory("./artifacts");

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////

Setup(context =>
{
    if (BuildSystem.IsRunningOnAppVeyor)
    {
        var tag = AppVeyor.Environment.Repository.Tag;

        if (tag.IsTag)
        {
            packageVersion = tag.Name;
        }
        else
        {
            var buildNumber = AppVeyor.Environment.Build.Number.ToString("00000");
            var branch = AppVeyor.Environment.Repository.Branch;
            var isPullRequest = AppVeyor.Environment.PullRequest.IsPullRequest;

            if (branch == "master" && !isPullRequest)
            {
                packageVersion = version + "-dev-" + buildNumber + dbgSuffix;
            }
            else
            {
                var suffix = "-ci-" + buildNumber + dbgSuffix;

                if (isPullRequest)
                    suffix += "-pr-" + AppVeyor.Environment.PullRequest.Number;
                else if (AppVeyor.Environment.Repository.Branch.StartsWith("release", StringComparison.OrdinalIgnoreCase))
                    suffix += "-pre-" + buildNumber;
                else
                    suffix += "-" + System.Text.RegularExpressions.Regex.Replace(branch, "[^0-9A-Za-z-]+", "-");

                // Nuget limits "special version part" to 20 chars. Add one for the hyphen.
                if (suffix.Length > 21)
                    suffix = suffix.Substring(0, 21);

                packageVersion = version + suffix;
            }
        }

        AppVeyor.UpdateBuildVersion(packageVersion);
    }

    Information("Building {0} version {1} of commercetools .NET SDK.", configuration, packageVersion);
});

//////////////////////////////////////////////////////////////////////
// CLEAN
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Description("Deletes all files in the BIN directory")
    .Does(() =>
    {
        DotNetCoreClean(SOLUTION_FILE);
    });

//////////////////////////////////////////////////////////////////////
// NUGET RESTORE
//////////////////////////////////////////////////////////////////////

Task("NuGetRestore")
    .Description("Restores NuGet Packages")
    .Does(() =>
    {
        DotNetCoreRestore(SOLUTION_FILE);
    });

//////////////////////////////////////////////////////////////////////
// BUILD FRAMEWORKS
//////////////////////////////////////////////////////////////////////

Task("Build")
    .Description("Builds the Solution")
    .IsDependentOn("NuGetRestore")
    .Does(() =>
    {
        DotNetCoreBuild(SOLUTION_FILE);
    });

//////////////////////////////////////////////////////////////////////
// TEST
//////////////////////////////////////////////////////////////////////

Task("CheckForError")
    .Description("Checks for errors running the test suites")
    .Does(() => CheckForError(ref ErrorDetail));

Task("TestNetStandard20")
    .Description("Tests the .NET Standard 2.0 version of the SDK")
    .IsDependentOn("Build")
    .OnError(exception => { ErrorDetail.Add(exception.Message); })
    .Does(() =>
    {
        var settings = new DotNetCoreTestSettings
        {
            Configuration = configuration,
            // Outputing test results as XML so that VSTS can pick it up
            // --test-adapter-path:. --logger:"nunit;LogFilePath=test-result.xml"
            ArgumentCustomization = args => args.Append("--test-adapter-path:.")
                                                .Append("--logger \"nunit;LogFileName=" + ARTIFACTS_DIR + "TestResult.xml\"")
        };
        DotNetCoreTest(TEST_PROJECT_DIR, settings);
		// if (isAppveyor)
		// {
		// 	var wc = new System.Net.WebClient();

		// 	var jobId = AppVeyor.Environment.JobId;
		// 	byte[] responseArray = wc.UploadFile("https://ci.appveyor.com/api/testresults/nunit3/" + jobId, TEST_PROJECT_DIR + "TestResults/TestResult.xml");
            
        //     Console.WriteLine("\nResponse Received.The contents of the file uploaded are:\n{0}", 
        //         System.Text.Encoding.ASCII.GetString(responseArray));

        //     AppVeyor.UploadArtifact(TEST_PROJECT_DIR + "TestResults/TestResult.xml");
		// }
    });


//////////////////////////////////////////////////////////////////////
// PACKAGE
//////////////////////////////////////////////////////////////////////

Task("PackageSDK")
    .Description("Creates NuGet packages of the SDK")
    .Does(() =>
    {
        var settings = new DotNetCorePackSettings
        {
            VersionSuffix = suffix,
            Configuration = configuration,
            OutputDirectory = ARTIFACTS_DIR
        };
        DotNetCorePack(SDK_PROJECT_DIR, settings);
    });

//////////////////////////////////////////////////////////////////////
// UPLOAD ARTIFACTS
//////////////////////////////////////////////////////////////////////

Task("UploadArtifacts")
    .Description("Uploads artifacts to AppVeyor")
    .IsDependentOn("Package")
    .Does(() =>
    {
        UploadArtifacts(PACKAGE_DIR, "*.nupkg");
    });

//////////////////////////////////////////////////////////////////////
// SETUP AND TEARDOWN TASKS
//////////////////////////////////////////////////////////////////////

Teardown(context => CheckForError(ref ErrorDetail));

//////////////////////////////////////////////////////////////////////
// HELPER METHODS - GENERAL
//////////////////////////////////////////////////////////////////////

void UploadArtifacts(string packageDir, string searchPattern)
{
    foreach(var zip in System.IO.Directory.GetFiles(packageDir, searchPattern))
        AppVeyor.UploadArtifact(zip);
}

void CheckForError(ref List<string> errorDetail)
{
    if(errorDetail.Count != 0)
    {
        var copyError = new List<string>();
        copyError = errorDetail.Select(s => s).ToList();
        errorDetail.Clear();
        throw new Exception("One or more unit tests failed, breaking the build.\n"
                              + copyError.Aggregate((x,y) => x + "\n" + y));
    }
}

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Rebuild")
    .Description("Rebuilds all versions of the framework")
    .IsDependentOn("Clean")
    .IsDependentOn("Build");

Task("Test")
    .Description("Builds and tests all versions of the framework")
    .IsDependentOn("Build")
    .IsDependentOn("TestNetStandard20");


Task("Package")
    .Description("Packages all versions of the framework")
    .IsDependentOn("Build")
    .IsDependentOn("CheckForError")
    .IsDependentOn("PackageSDK");

Task("Appveyor")
    .Description("Builds, tests and packages on AppVeyor")
    .IsDependentOn("Build")
    .IsDependentOn("Test")
    .IsDependentOn("Package");
    //.IsDependentOn("UploadArtifacts");

Task("Travis")
    .Description("Builds and tests on Travis")
    .IsDependentOn("Build")
    .IsDependentOn("Test");

Task("Default")
    .Description("Builds all versions of the framework")
    .IsDependentOn("Build");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
