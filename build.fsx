// --------------------------------------------------------------------------------------
// FAKE build script
// --------------------------------------------------------------------------------------

#r @"packages/FAKE/tools/FakeLib.dll"
open Fake
open Fake.Git
open Fake.AssemblyInfoFile
open Fake.ReleaseNotesHelper
open System
open System.IO

let project = "Zipkin.Tracer"
let summary = "A minimalistic .NET client library for Twitter Zipkin tracing."
let solutionFile  = "Zipkin.Tracer.sln"
let testAssemblies = "tests/**/bin/Release/*Tests*.dll"
let gitOwner = "bazingatechnologies" 
let gitHome = "https://github.com/" + gitOwner
let gitName = "Zipkin.Tracer"
let gitRaw = environVarOrDefault "gitRaw" "https://raw.github.com/openzipkin"

let binDir = currentDirectory @@ "bin"

// Read additional information from the release notes document
let release = LoadReleaseNotes "RELEASE_NOTES.md"

// Helper active pattern for project types
let (|Fsproj|Csproj|Vbproj|) (projFileName:string) = 
    match projFileName with
    | f when f.EndsWith("fsproj") -> Fsproj
    | f when f.EndsWith("csproj") -> Csproj
    | f when f.EndsWith("vbproj") -> Vbproj
    | _                           -> failwith (sprintf "Project file %s not supported. Unknown project type." projFileName)

// Generate assembly info files with the right version & up-to-date information
Target "AssemblyInfo" (fun _ ->
    let getAssemblyInfoAttributes projectName =
        [ Attribute.Title (projectName)
          Attribute.Product project
          Attribute.Description summary
          Attribute.Version release.AssemblyVersion
          Attribute.FileVersion release.AssemblyVersion
          Attribute.InternalsVisibleTo "Zipkin.Tracer.Tests" ]

    let getProjectDetails projectPath =
        let projectName = System.IO.Path.GetFileNameWithoutExtension(projectPath)
        ( projectPath, 
          projectName,
          System.IO.Path.GetDirectoryName(projectPath),
          (getAssemblyInfoAttributes projectName)
        )

    !! "src/**/*.??proj"
    |> Seq.map getProjectDetails
    |> Seq.iter (fun (projFileName, projectName, folderName, attributes) ->
        match projFileName with
        | Fsproj -> CreateFSharpAssemblyInfo (folderName @@ "AssemblyInfo.fs") attributes
        | Csproj -> CreateCSharpAssemblyInfo ((folderName @@ "Properties") @@ "AssemblyInfo.cs") attributes
        | Vbproj -> CreateVisualBasicAssemblyInfo ((folderName @@ "My Project") @@ "AssemblyInfo.vb") attributes
        )
)

// Copies binaries from default VS location to expected bin folder
// But keeps a subdirectory structure for each project in the 
// src folder to support multiple project outputs
Target "CopyBinaries" (fun _ ->
    !! "src/**/*.??proj"
    |>  Seq.map (fun f -> ((System.IO.Path.GetDirectoryName f) @@ "bin/Release", "bin" @@ (System.IO.Path.GetFileNameWithoutExtension f)))
    |>  Seq.iter (fun (fromDir, toDir) -> CopyDir toDir fromDir (fun _ -> true))
)

// --------------------------------------------------------------------------------------
// Clean build results

Target "Clean" (fun _ ->
    CleanDirs ["bin"; "temp"]
)

Target "CleanDocs" (fun _ ->
    CleanDirs ["docs/output"]
)

// --------------------------------------------------------------------------------------
// Build library & test project

Target "Build" (fun _ ->
    !! solutionFile
    |> MSBuildRelease "" "Rebuild"
    |> ignore
)

// --------------------------------------------------------------------------------------
// Run the unit tests using test runner
open Fake.Testing.XUnit2
Target "RunTests" (fun _ ->
    !! testAssemblies
    |> xUnit2 (fun p ->
        { p with
            TimeOut = TimeSpan.FromMinutes 20.
            XmlOutputPath = Some "TestResults.xml"
            ToolPath = "packages/xunit.runner.console/tools/xunit.console.exe" })
)

// --------------------------------------------------------------------------------------
// Build a NuGet package

Target "NuGet" (fun _ ->
    !! "src/**/*.nuspec"
    |> Seq.toArray
    |> Array.iter (fun nuspec ->
        let project = Path.GetFileNameWithoutExtension nuspec 
        let dir = (Path.GetDirectoryName nuspec)
        let packagesFile = dir @@ "packages.config"
        let dependencies = NuGetHelper.getDependencies packagesFile
        let buildDir = binDir @@ project
        NuGetHelper.NuGetPack
            (fun p ->
                { p with
                    Copyright = "Bazinga Technologies Inc."
                    Project =  project
                    Properties = ["Configuration", "Release"]
                    ReleaseNotes = release.Notes |> String.concat "\n"
                    Version = release.NugetVersion
                    IncludeReferencedProjects = true
                    OutputPath = buildDir                    
                    WorkingDir = dir
                    Dependencies = dependencies })
            nuspec)
)

Target "PublishNuget" (fun _ ->
    let rec publishPackage trialsLeft nupkg =
        let nugetExe = NuGetHelper.NuGetDefaults().ToolPath
        let key = getBuildParam "key"
        let url = "https://www.nuget.org/api/v2/package"
        let nugetCmd = sprintf "push \"%s\" %s -source %s" nupkg key url
        tracefn "Pushing %s Attempts left: %d" nupkg trialsLeft
        try 
            let result = ExecProcess (fun info -> 
                    info.FileName <- nugetExe
                    info.WorkingDirectory <- (Path.GetDirectoryName nupkg)
                    info.Arguments <- nugetCmd) (TimeSpan.FromSeconds 10.0)
            if result <> 0 then failwithf "Error during NuGet symbol push. %s %s" nugetExe nugetCmd
        with exn -> 
            if (trialsLeft > 0) then (publishPackage (trialsLeft-1) nupkg)
            else raise exn

    !! "**/*.nupkg"
    |> Seq.toArray
    |> Array.iter (publishPackage 5)
)

Target "KeepRunning" (fun _ ->    
    use watcher = new FileSystemWatcher(DirectoryInfo("docs/content").FullName,"*.*")
    watcher.EnableRaisingEvents <- true

    traceImportant "Waiting for help edits. Press any key to stop."

    System.Console.ReadKey() |> ignore

    watcher.EnableRaisingEvents <- false
    watcher.Dispose()
)

Target "BuildPackage" DoNothing

// --------------------------------------------------------------------------------------
// Run all targets by default. Invoke 'build <Target>' to override

Target "All" DoNothing

"Clean"
  ==> "AssemblyInfo"
  ==> "Build"
  ==> "CopyBinaries"
  ==> "RunTests"
  ==> "All"

"All" 
  ==> "NuGet"
  ==> "BuildPackage"

"BuildPackage"
  ==> "PublishNuget"

RunTargetOrDefault "All"
