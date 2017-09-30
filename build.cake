#addin "Cake.DocFx"
#tool "docfx.console"

var target = Argument("target", "Default");
var apiKey = Argument<string>("apiKey", null);	// ./build.ps1 --target push -apiKey="your github api key"                                            
var testFailed = false;
var solutionDir = System.IO.Directory.GetCurrentDirectory();
var testResultDir = System.IO.Path.Combine(solutionDir, "testResults");
var artifactDir = "./artifacts";

Information("Solution Directory: {0}", solutionDir);
Information("Test Results Directory: {0}", testResultDir);

Task("PrepareDirectories")
	.Does(() =>
	{
		EnsureDirectoryExists(testResultDir);
		EnsureDirectoryExists(artifactDir);
	});

Task("Clean")
	.IsDependentOn("PrepareDirectories")
	.Does(() =>
	{
		var delSettings = new DeleteDirectorySettings { Recursive = true, Force = true };
		CleanDirectory(testResultDir);
		CleanDirectory(artifactDir);

		var binDirs = GetDirectories("./**/bin");
		var objDirs = GetDirectories("./**/obj");
		var testResDirs = GetDirectories("./**/test-results");
		
		DeleteDirectories(binDirs, delSettings);
		DeleteDirectories(objDirs, delSettings);
		DeleteDirectories(testResDirs, delSettings);
	});

Task("Restore")
	.Does(() =>
	{
		DotNetCoreRestore();	  
	});

Task("Build")
	.IsDependentOn("Restore")
	.Does(() =>
	{
		var solution = GetFiles("./*.sln").ElementAt(0);
		Information("Build solution: {0}", solution);

		var settings = new DotNetCoreBuildSettings
		{
			Configuration = "Release"
		};

		DotNetCoreBuild(solution.FullPath, settings);
	});

Task("Test")
	.IsDependentOn("Clean")
	.IsDependentOn("Build")
	.ContinueOnError()
	.Does(() =>
	{
		var tests = GetFiles("./test/**/*Test/*.csproj");

		if(tests.Count == 0)
		{
			Information("Found no test projects");
			return;
		}
			

		foreach(var test in tests)
		{
			var projectFolder = System.IO.Path.GetDirectoryName(test.FullPath);
			try
			{
				DotNetCoreTest(test.FullPath, new DotNetCoreTestSettings
				{
					ArgumentCustomization = args => args.Append("-l trx"),
					WorkingDirectory = projectFolder
				});
			}
			catch(Exception e)
			{
				testFailed = true;
				Error(e.Message.ToString());
			}
		}

		// Copy test result files.
		var tmpTestResultFiles = GetFiles("./**/*.trx");
		CopyFiles(tmpTestResultFiles, testResultDir);
	});

Task("Pack")
	.IsDependentOn("Clean")
	.IsDependentOn("Test")
	.Does(() =>
	{
		if(testFailed)
		{
			Information("Do not pack because tests failed");
			return;
		}

		var projects = GetFiles("./src/**/*.csproj");
		var settings = new DotNetCorePackSettings
		{
			Configuration = "Release",
			OutputDirectory = artifactDir
		};
		
		foreach(var project in projects)
		{
			Information("Pack {0}", project.FullPath);
			DotNetCorePack(project.FullPath, settings);
		}
	});

Task("Publish")
	.IsDependentOn("Clean")
	.IsDependentOn("Test")
	.Does(() =>
	{
		if(testFailed)
		{
			Information("Do not publish because tests failed");
			return;
		}
		var projects = GetFiles("./src/**/*.csproj");

		foreach(var project in projects)
		{
			var projectDir = System.IO.Path.GetDirectoryName(project.FullPath);
			var projectName = new System.IO.DirectoryInfo(projectDir).Name;
			var outputDir = System.IO.Path.Combine(artifactDir, projectName);
			EnsureDirectoryExists(outputDir);

			Information("Publish {0} to {1}", projectName, outputDir);

			var settings = new DotNetCorePublishSettings
			{
				OutputDirectory = outputDir,
				Configuration = "Release"
			};
			DotNetCorePublish(project.FullPath, settings);
		}
	});

Task("Push")
	.IsDependentOn("Pack")
	.Does(() =>
	{
		var package = GetFiles($"{artifactDir}/TransNet.*.nupkg").ElementAt(0);
        var source = "https://www.nuget.org/api/v2/package";

        if(apiKey==null)
            throw new ArgumentNullException(nameof(apiKey), "The \"apiKey\" argument must be set for this task.");

        Information($"Push {package} to {source}");

        NuGetPush(package, new NuGetPushSettings {
            Source = source,
            ApiKey = apiKey
        });
	});


// This taks does not work currently because DocFX cannot build for .Net Standard 2.0
Task("Doc").Does(() => 
	{
		DocFxMetadata();
		DocFxBuild();
	});

Task("Default")
	.IsDependentOn("Test")
	.Does(() =>
	{
		Information("Build and test the whole solution.");
		Information("To pack (nuget) the application use the cake build argument: -Target Pack");
		Information("To publish (to run it somewhere else) the application use the cake build argument: -Target Publish");
		Information("To push the package to nuget.org use the cake build argument: -Target Push --apiKey=\"your nuget.org API key\"");
	});

RunTarget(target);