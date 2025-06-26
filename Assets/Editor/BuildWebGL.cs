using UnityEditor;
using UnityEditor.Build;

public class BuildWebGL
{
    [MenuItem("Build/WebGL Optimized")]
    static void BuildGame()
    {
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
        PlayerSettings.WebGL.decompressionFallback = true;
        PlayerSettings.WebGL.dataCaching = true;
        PlayerSettings.WebGL.debugSymbolMode = WebGLDebugSymbolMode.Off; // Updated
        PlayerSettings.stripEngineCode = true;
        
        // Updated for Unity 2020.3
        var buildTargetGroup = BuildTargetGroup.WebGL;
        PlayerSettings.SetManagedStrippingLevel(buildTargetGroup, ManagedStrippingLevel.High);
        
        BuildPipeline.BuildPlayer(
            EditorBuildSettings.scenes,
            "Build",
            BuildTarget.WebGL,
            BuildOptions.None
        );
    }
}