// Editor-only helper: creates the single procedural scene (if it does not exist),
// puts it in Build Settings, and builds the Windows player.
// Command line:  Unity -batchmode -projectPath UnityProject
//                      -executeMethod DaoDenLong.EditorTools.BuildScript.BuildWindows -quit

#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DaoDenLong.EditorTools
{
    public static class BuildScript
    {
        private const string ScenePath = "Assets/DaoDenLong/Scene/Game.unity";
        private const string OutDir = "Builds/Windows";

        [MenuItem("DaoDenLong/Create Scene")]
        public static void CreateScene()
        {
            if (File.Exists(ScenePath)) { Debug.Log("Scene exists: " + ScenePath); return; }
            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject go = new GameObject("GameBootstrap");
            go.AddComponent<View.GameBootstrap>();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            Debug.Log("Created " + ScenePath);
        }

        [MenuItem("DaoDenLong/Build Windows")]
        public static void BuildWindows()
        {
            if (!File.Exists(ScenePath)) CreateScene();
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };

            BuildPlayerOptions opt = new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = OutDir + "/DaoDenLong.exe",
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };
            BuildReport report = BuildPipeline.BuildPlayer(opt);
            Debug.Log("Build result: " + report.summary.result + " -> " + Path.GetFullPath(OutDir));
        }
    }
}
#endif
