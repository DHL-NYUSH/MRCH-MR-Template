// Copyright (c) 2026 Digital Heritage Lab / Shengyang "Billiton" Peng
// Licensed under the MIT License. See LICENSE for details.

using Sirenix.OdinInspector;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MRCH.Tools.Edit
{
    /// <summary>
    /// Displays a wireframe cube gizmo with a floating label in the Scene view.
    /// Used in MR tool templates to help students locate learnable tool areas.
    /// The label size/opacity responds to Unity's 3D Icons slider.
    /// </summary>
    [AddComponentMenu("MRCH/Edit/MR Area Label")]
    public class MRAreaLabel : MonoBehaviour
    {
        [TitleGroup("Area Settings"), SerializeField, Tooltip("use the position of the game object, use a relative position if set to false.")]
        private bool useTransform = true;

        [TitleGroup("Area Settings"), SerializeField, HideIf("useTransform")]
        private Vector3 center = Vector3.zero;
            
        [TitleGroup("Area Settings"), SerializeField, HideIf("useTransform")]
        private Vector3 rotation = Vector3.zero;
        
        [TitleGroup("Area Settings"), SerializeField]
        private Vector3 areaSize = new(1f, 1f, 1f);
        
        [Title("Area Label")]
        [LabelText("Label Text")]
        public string labelText = "Tool Area";

        [TitleGroup("Appearance")]
        [ColorPalette, SerializeField]
        private Color gizmoColor = new(0f, 0.8f, 1f, 1f);

        [TitleGroup("Appearance"), SerializeField, OnValueChanged("SetTextSize")]
        private float setTextSize = 1f;
        
        [OnValueChanged("SyncTextSize")]
        private static float _textSize = 1f;
        
        private void SetTextSize() => _textSize = setTextSize;
        private void SyncTextSize() => setTextSize = _textSize;

        [LabelText("Text Offset Y")]
        [Range(0f, 2f)]
        public float textOffsetY = 0.1f;

        [LabelText("Text Font Size")]
        [Range(8, 32)]
        public int baseFontSize = 14;

        [FoldoutGroup("Culling Distance"), ShowInInspector, ReadOnly]
        private static float _cullingDistance = 60f;
        [FoldoutGroup("Culling Distance"), Button]
        private void SetCullingDistance(float distance) => _cullingDistance = distance;
        

#if UNITY_EDITOR
        
        private void OnDrawGizmos()
        {
            if(CullingGizmos()) return;
            
            var pos = useTransform ? transform.position : transform.InverseTransformPoint(center);
            var rot = useTransform ? transform.rotation : transform.rotation * Quaternion.Euler(rotation);

            // --- Wireframe Cube ---
            Gizmos.color = gizmoColor;
            Gizmos.matrix = Matrix4x4.TRS(pos, rot, Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, areaSize);
            Gizmos.matrix = Matrix4x4.identity;

            if(_textSize <= 0f)
                return;
            
            var labelPos = pos
                           + transform.up * (areaSize.y * 0.5f + textOffsetY);

            var style = new GUIStyle
            {
                normal =
                {
                    textColor = gizmoColor * new Color(1, 1, 1, 1)
                },
                fontSize = Mathf.RoundToInt(baseFontSize * Mathf.Lerp(0.5f, 1f, _textSize)),
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold
            };

            Handles.Label(labelPos, labelText, style);
            
            return;

            bool CullingGizmos()
            {
#if UNITY_EDITOR
                var sceneCamera = SceneView.currentDrawingSceneView?.camera;
                if (!sceneCamera)
                {
                    Debug.Log("There is no scene view camera found in the scene.");
                    return false;
                }
                var distance = Vector3.Distance(transform.position, sceneCamera.transform.position);
                
                return distance > _cullingDistance;
#else
                return false;
#endif
            }
        }
#endif
    }
}