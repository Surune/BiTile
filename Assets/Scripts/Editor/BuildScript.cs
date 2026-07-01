using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using Debug = UnityEngine.Debug;

public static class BuildScript
{
    private const string BuildInfoPath = "Assets/Resources/build_info.txt";
    private const int GitHashLength = 12;

    public static void Build()
    {
        Build(GetBuildTarget());
    }

    [MenuItem("Build/Build Windows")]
    public static void BuildWindows()
    {
        Build(BuildTarget.StandaloneWindows64);
    }

    [MenuItem("Build/Build macOS")]
    public static void BuildMacOS()
    {
        Build(BuildTarget.StandaloneOSX);
    }

    private static void Build(BuildTarget buildTarget)
    {
        var gitHash = GetGitHash();
        var buildPath = GetBuildPath(buildTarget, gitHash);

        WriteGitHash(gitHash);
        AssetDatabase.ImportAsset(BuildInfoPath);

        var fullBuildPath = Path.IsPathRooted(buildPath) ? buildPath : Path.Combine(ProjectPath, buildPath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullBuildPath));

        var scenes = EditorBuildSettings.scenes
            .Where(scene => scene.enabled)
            .Select(scene => scene.path)
            .ToArray();

        var report = BuildPipeline.BuildPlayer(scenes, fullBuildPath, buildTarget, BuildOptions.None);
        if (report.summary.result != BuildResult.Succeeded)
        {
            throw new Exception($"Build failed: {report.summary.result}");
        }

        OpenBuildFolder(buildTarget, report.summary.outputPath);
        Debug.Log($"Build Succeeded: {report.summary.outputPath}");
    }

    private static string ProjectPath => Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath, ".."));

    private static void WriteGitHash(string gitHash)
    {
        File.WriteAllText(Path.Combine(ProjectPath, BuildInfoPath), gitHash);
    }

    private static string GetGitHash()
    {
        var process = Process.Start(new ProcessStartInfo
        {
            FileName = "git",
            Arguments = $"rev-parse --short={GitHashLength} HEAD",
            WorkingDirectory = ProjectPath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        });

        var output = process.StandardOutput.ReadToEnd();
        var error = process.StandardError.ReadToEnd();
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            throw new Exception(error.Trim());
        }

        return output.Trim();
    }

    private static BuildTarget GetBuildTarget()
    {
        var args = Environment.GetCommandLineArgs();
        var buildTargetIndex = Array.IndexOf(args, "-biTileBuildTarget");
        if (buildTargetIndex >= 0)
        {
            return (BuildTarget)Enum.Parse(typeof(BuildTarget), args[buildTargetIndex + 1]);
        }

        return EditorUserBuildSettings.activeBuildTarget;
    }

    private static string GetBuildPath(BuildTarget buildTarget, string gitHash)
    {
        var args = Environment.GetCommandLineArgs();
        var buildPathIndex = Array.IndexOf(args, "-buildPath");
        if (buildPathIndex >= 0)
        {
            return args[buildPathIndex + 1];
        }

        var projectName = PlayerSettings.productName;
        var folderName = $"{DateTime.Now:yyyyMMdd}_{PlayerSettings.bundleVersion}_{gitHash}";
        return buildTarget switch
        {
            BuildTarget.Android => $"Builds/Android/{folderName}/{projectName}.apk",
            BuildTarget.StandaloneOSX => $"Builds/macOS/{folderName}/{projectName}.app",
            BuildTarget.StandaloneWindows => $"Builds/Windows/{folderName}/{projectName}.exe",
            BuildTarget.StandaloneWindows64 => $"Builds/Windows/{folderName}/{projectName}.exe",
            BuildTarget.WebGL => $"Builds/WebGL/{folderName}",
            _ => $"Builds/{buildTarget}/{folderName}/{projectName}",
        };
    }

    private static void OpenBuildFolder(BuildTarget buildTarget, string outputPath)
    {
        var buildFolderPath = buildTarget == BuildTarget.WebGL ? outputPath : Path.GetDirectoryName(outputPath);
        Process.Start(new ProcessStartInfo
        {
            FileName = "explorer.exe",
            Arguments = $"\"{buildFolderPath}\"",
            UseShellExecute = true,
        });
    }
}
