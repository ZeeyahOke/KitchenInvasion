using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace KitchenInvasion.EditorTools
{
    public static class ProjectSetup
    {
        static AddAndRemoveRequest packageRequest;

        [MenuItem("Kitchen Invasion/1 Install Packages")]
        public static void InstallPackages()
        {
            string[] add = { "com.unity.xr.arfoundation", "com.unity.xr.arkit", "com.unity.xr.management", "com.unity.inputsystem", "com.unity.ugui" };
            string[] remove = { "com.unity.visualscripting", "com.unity.collab-proxy", "com.unity.ai.navigation" };
            packageRequest = Client.AddAndRemove(add, remove);
            EditorApplication.update += WaitForPackages;
        }

        static void WaitForPackages()
        {
            if (!packageRequest.IsCompleted) return;
            EditorApplication.update -= WaitForPackages;
            if (packageRequest.Status == StatusCode.Success) Debug.Log("KI_SETUP packages installed");
            else Debug.LogError("KI_SETUP package error " + packageRequest.Error.message);
        }
    }
}
