// Copyright (c) 2026 Digital Heritage Lab / Shengyang "Billiton" Peng
// Licensed under the MIT License. See LICENSE for details.

using MRCH.Tools.Edit;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace MRCH.Editor
{
    public class MapModelBuildPreprocessor : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            foreach (var mapModel in Object.FindObjectsByType<MapModel>(FindObjectsSortMode.None))
            {
                Debug.Log($"[MapModel] Removing '{mapModel.gameObject.name}' before build.");
                Object.DestroyImmediate(mapModel.gameObject);
            }
        }
    }
}