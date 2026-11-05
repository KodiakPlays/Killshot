using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

/// <summary>
/// Stops any Windows build that is not Intel 64-bit. On a Mac with an Apple chip, Unity sets
/// Windows builds to ARM 64-bit by itself, and normal Windows PCs refuse to open them.
/// Steps are in README.md, "Building the game".
/// </summary>
public class WindowsBuildCheck : IPreprocessBuildWithReport
{
    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report)
    {
        var platform = report.summary.platform;
        if (platform != BuildTarget.StandaloneWindows64 && platform != BuildTarget.StandaloneWindows) return;

        // Looked up by name: a Mac without Windows Build Support doesn't have this type, and
        // naming it directly would stop the project compiling there.
        var settings = System.Type.GetType("UnityEditor.WindowsStandalone.UserBuildSettings, UnityEditor.WindowsStandalone.Extensions");
        var architecture = settings?.GetProperty("architecture")?.GetValue(null)?.ToString();
        if (architecture != "x64")
            throw new BuildFailedException(
                $"Windows build stopped. Architecture is {architecture ?? "unknown"}, and it must be Intel 64-bit. " +
                "Set it in File > Build Profiles > Windows > Architecture. See README.md, \"Building the game\".");
    }
}
