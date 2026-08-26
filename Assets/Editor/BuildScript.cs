using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// game-ci / Unity 批处理模式的构建入口。
/// 由 workflow 通过 -executeMethod BuildCommand.Build 调用。
/// </summary>
public static class BuildCommand
{
    private const string Android = "Android";
    private const string Ios = "iOS";

    private static string GetArgument(string name)
    {
        string[] args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i].Contains(name))
            {
                return args[i + 1];
            }
        }
        return null;
    }

    private static string[] GetEnabledScenes()
    {
        return EditorBuildSettings.scenes
            .Where(scene => scene.enabled)
            .Select(scene => scene.path)
            .ToArray();
    }

    private static BuildTarget GetBuildTarget()
    {
        string name = GetArgument("customBuildTarget");
        if (string.IsNullOrEmpty(name))
        {
            name = EditorUserBuildSettings.activeBuildTarget.ToString();
        }

        Console.WriteLine(":: Build target: " + name);
        if (name.Equals(Android, StringComparison.OrdinalIgnoreCase))
        {
            return BuildTarget.Android;
        }
        if (name.Equals(Ios, StringComparison.OrdinalIgnoreCase))
        {
            return BuildTarget.iOS;
        }
        return BuildTarget.NoTarget;
    }

    private static string GetBuildPath()
    {
        string path = GetArgument("customBuildPath");
        if (string.IsNullOrEmpty(path))
        {
            path = "build";
        }

        Console.WriteLine(":: Build path: " + path);
        return path;
    }

    private static string GetBuildName()
    {
        string name = GetArgument("customBuildName");
        if (string.IsNullOrEmpty(name))
        {
            name = Application.productName;
        }

        Console.WriteLine(":: Build name: " + name);
        return name;
    }

    public static void Build()
    {
        BuildTarget buildTarget = GetBuildTarget();
        string buildPath = GetBuildPath();
        string buildName = GetBuildName();

        // 显式指定包名，避免默认 com.DefaultCompany.* 的潜在问题
        if (buildTarget == BuildTarget.Android)
        {
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.cabriter.unitycitest");
        }
        else if (buildTarget == BuildTarget.iOS)
        {
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, "com.cabriter.unitycitest");
        }

        string[] scenes = GetEnabledScenes();
        if (scenes.Length == 0)
        {
            // 空项目：动态创建一个空场景，保证 BuildPlayer 有场景可用
            const string scenePath = "Assets/Scenes/Main.unity";
            string sceneDir = Path.GetDirectoryName(scenePath);
            if (!string.IsNullOrEmpty(sceneDir) && !Directory.Exists(sceneDir))
            {
                Directory.CreateDirectory(sceneDir);
            }

            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), scenePath);
            scenes = new[] { scenePath };
            Console.WriteLine(":: Created empty scene at " + scenePath);
        }

        string outputPath = buildTarget == BuildTarget.Android
            ? Path.Combine(buildPath, buildName + ".apk")
            : Path.Combine(buildPath, buildName);

        Console.WriteLine(":: Building to " + outputPath);
        BuildReport report = BuildPipeline.BuildPlayer(scenes, outputPath, buildTarget, BuildOptions.None);

        if (report.summary.result != BuildResult.Succeeded)
        {
            throw new Exception(
                $"Build failed: {report.summary.result} ({report.summary.totalErrors} errors)");
        }

        Console.WriteLine(":: Build succeeded");
    }
}
