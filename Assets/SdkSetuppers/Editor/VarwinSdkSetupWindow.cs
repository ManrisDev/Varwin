#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;
using UnityEngine.UIElements;

namespace Varwin
{
    public class VarwinSdkSetupWindow : EditorWindow
    {
        private static readonly Vector2 WindowSize = new(450, 150);
        private static VarwinSdkSetupWindow _instance;

        [InitializeOnLoadMethod]
        public static void ShowSetupWindow() => EditorApplication.update += ShowWindow; //Нужно, чтобы правильно отработал метод GetWindow.

        private static void ShowWindow()
        {
            EditorApplication.update -= ShowWindow;

            if (PackagesInstaller.InstallRequired || !VarwinSDKInstaller.Installed())
            {
                _instance = GetWindow<VarwinSdkSetupWindow>(false, "Varwin SDK Installer", true);
                _instance.minSize = _instance.maxSize = WindowSize;
                _instance.Show();

                EditorApplication.update += InstallProcessUpdate;
            }
        }

        private static void InstallProcessUpdate()
        {
            if (EditorApplication.isCompiling || PackagesInstaller.IsProcessing || VarwinSDKInstaller.IsProcessing)
            {
                return;
            }

            if (PackagesInstaller.InstallRequired)
            {
                PackagesInstaller.EmbedInstallAllPackages();
                return;
            }

            if (!VarwinSDKInstaller.Installed())
            {
                VarwinSDKInstaller.InstallSDK();
                return;
            }

            EditorApplication.update -= InstallProcessUpdate;
        }

        public void CreateGUI()
        {
            VisualElement labelRow = new VisualElement();
            labelRow.style.flexDirection = FlexDirection.Row;
            labelRow.style.justifyContent = Justify.Center;

            Label label = new Label("Varwin SDK Installer");
            label.style.alignSelf = Align.Center;
            label.style.flexDirection = FlexDirection.Row;
            label.style.fontSize = 25;

            Image image = new Image();
            image.style.paddingRight = 10;
            image.style.alignSelf = Align.Center;
            image.sprite = (Sprite)AssetDatabase.LoadAssetAtPath("Assets/Varwin/Icons/icon256.png", typeof(Sprite));
            image.style.width = image.style.height = 35;

            labelRow.Add(image);
            labelRow.Add(label);
            rootVisualElement.Add(labelRow);

            DrawRequiredSteps(rootVisualElement);

            CompilationPipeline.compilationFinished += UpdateRequiredSteps;
        }

        private void OnDisable()
        {
            CompilationPipeline.compilationFinished -= UpdateRequiredSteps;
        }

        private void UpdateRequiredSteps(object _)
        {
            rootVisualElement.Clear();
            DrawRequiredSteps(rootVisualElement);
        }

        private void DrawRequiredSteps(VisualElement root)
        {
            ScrollView steps = new ScrollView();

            var requiredPackages = PackagesInstaller.SdkRequiredPackages;
            var installed = PackagesInstaller.GetInstalledPackages();

            for (var i = 0; i < requiredPackages.Count; i++)
            {
                var line = new Box();
                line.style.flexDirection = FlexDirection.Row;

                bool isPackageInstalled = requiredPackages[i].IsPackageInstalled(installed.packageManifes, installed.packageLock);

                string packageTitle = $"{requiredPackages[i].PackageIdentified}";

                var packageLabel = new Label(packageTitle);
                packageLabel.style.width = 300;

                var statusLabel = new Label($"{(isPackageInstalled ? "OK" : "Not Installed")}");
                statusLabel.style.color = isPackageInstalled ? Color.green : Color.yellow;

                line.Add(packageLabel);
                line.Add(statusLabel);
                steps.Add(line);
            }

            var sdkLine = new Box();
            sdkLine.style.flexDirection = FlexDirection.Row;

            var sdkTitle = new Label("Internal Package");
            sdkTitle.style.width = 300;

            var sdkStatus = new Label($"{(VarwinSDKInstaller.Installed() ? "OK" : "Not Installed")}");
            sdkStatus.style.color = VarwinSDKInstaller.Installed() ? Color.green : Color.yellow;

            sdkLine.Add(sdkTitle);
            sdkLine.Add(sdkStatus);

            steps.Add(sdkLine);
            root.Add(steps);
        }
    }
}
#endif