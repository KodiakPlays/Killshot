// =================================================================================================
// WindowsBuildCheck.cs
//
// What it does:
// This stops a Windows build of the game unless it is set to Intel 64-bit. On a Mac with an Apple
// chip, Unity sets Windows builds to ARM 64-bit. Normal Windows PCs will not open those. It runs
// in the editor, at the start of every build.
//
// How it works:
// - Unity calls it before each build starts. It does nothing for builds that are not for Windows.
// - For a Windows build it reads the Windows architecture setting. It finds the setting by its
//   name. If it cannot find it, the architecture reads as unknown.
// - If the setting is not x64, it stops the build with a message. The message says where to fix
//   it: File > Build Profiles > Windows > Architecture. It points to README.md.
//
// Technical:
// - It is an IPreprocessBuildWithReport, order 0. Unity finds it by itself. No script calls it.
// - It checks report.summary.platform for StandaloneWindows64 and StandaloneWindows.
// - The setting is UserBuildSettings.architecture, in UnityEditor.WindowsStandalone.Extensions.
//   It is looked up by name.
// - Trap: naming that class directly breaks the compile on a Mac without Windows Build Support.
// - A failed check throws BuildFailedException with the message. That stops the build.
// - The steps are in README.md, "Building the game".
// =================================================================================================

using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

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
