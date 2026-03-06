// Copyright (c) 2026 Digital Heritage Lab / Shengyang "Billiton" Peng
// Licensed under the MIT License. See LICENSE for details.

using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;
#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEngine.UI;
#endif

namespace MRCH.Tools.Edit
{
    /// <summary>
    /// Abstract base for editor-only XR Origin movement/rotation control.
    /// Entire functionality is stripped from builds.
    /// </summary>
    [DisallowMultipleComponent]
    public abstract class XROriginEditorControl : MonoBehaviour
    {
#if UNITY_EDITOR
        [Title("Movement")]
        [SerializeField, Unit(Units.Meter)] protected float moveSpeed = 2f;
        [SerializeField, Unit(Units.Meter)] protected float fastMoveSpeed = 6f;
        [SerializeField, Unit(Units.Meter)] protected float elevationSpeed = 2f;

        [Title("Rotation")]
        [SerializeField, Unit(Units.Degree)] protected float rotationSpeed = 15f;
        [SerializeField] protected float keyboardRotationMultiplier = 3f;
        [SerializeField, Range(-89f, 0f)] protected float minPitch = -80f;
        [SerializeField, Range(0f, 89f)] protected float maxPitch = 80f;

        protected Vector3 moveDirection = Vector3.zero;
        protected float currentSpeed;
        protected float currentPitch;

        [Title("Control Hint")]
        [SerializeField] private bool showControlHint = true;
        [SerializeField, ShowIf("showControlHint")]
        private int hintFontSize = 16;
        [SerializeField, ShowIf("showControlHint")]
        private Vector2 hintPanelSize = new Vector2(280f, 260f);

        [FoldoutGroup("Gizmos Setting")]
        [SerializeField] private bool alwaysShowDirectionsGizmos = true;
        [FoldoutGroup("Gizmos Setting"), ColorPalette]
        [SerializeField] private Color directionsGizmosColor = Color.blue;

        [FoldoutGroup("Gizmos Setting")] [SerializeField, PropertyRange(0f, 15f)]
        private float directionsGizmosLength = 5f;

        private InputAction moveAction;
        private InputAction elevateAction;
        private InputAction shiftAction;
        private InputAction rotateAction;
        private InputAction mouseDeltaAction;
        private InputAction rightClickAction;

        // Hint UI references
        private GameObject hintCanvasGO;
        private GameObject hintPanelGO;
        private TextMeshProUGUI hintText;
        private TextMeshProUGUI toggleButtonText;
        private bool hintExpanded = true;

        private const string HintContent =
            "<b>— Movement —</b>\n" +
            "W / A / S / D  —  Move\n" +
            "Q / E  —  Down / Up\n" +
            "Shift  —  Fast Move\n\n" +
            "<b>— Rotation —</b>\n" +
            "I / K  —  Pitch Up / Down\n" +
            "J / L  —  Yaw Left / Right\n" +
            "Right-Click + Drag  —  Look";

        protected virtual void Awake()
        {
            moveAction = new InputAction();
            moveAction.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");

            elevateAction = new InputAction();
            elevateAction.AddCompositeBinding("1DAxis")
                .With("Negative", "<Keyboard>/q")
                .With("Positive", "<Keyboard>/e");

            shiftAction = new InputAction("Shift", binding: "<Keyboard>/shift");
            rightClickAction = new InputAction("RightClick", binding: "<Mouse>/rightButton");

            rotateAction = new InputAction();
            rotateAction.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/i")
                .With("Down", "<Keyboard>/k")
                .With("Left", "<Keyboard>/j")
                .With("Right", "<Keyboard>/l");

            mouseDeltaAction = new InputAction("MouseDelta", binding: "<Mouse>/delta");

            if (showControlHint)
                CreateHintUI();
        }

        protected virtual void OnEnable()
        {
            moveAction?.Enable();
            elevateAction?.Enable();
            shiftAction?.Enable();
            rotateAction?.Enable();
            rightClickAction?.Enable();
            mouseDeltaAction?.Enable();

            if (hintCanvasGO != null)
                hintCanvasGO.SetActive(showControlHint);
        }

        protected virtual void OnDisable()
        {
            moveAction?.Disable();
            elevateAction?.Disable();
            shiftAction?.Disable();
            rotateAction?.Disable();
            rightClickAction?.Disable();
            mouseDeltaAction?.Disable();
        }

        protected virtual void OnDestroy()
        {
            moveAction?.Dispose();
            elevateAction?.Dispose();
            shiftAction?.Dispose();
            rotateAction?.Dispose();
            rightClickAction?.Dispose();
            mouseDeltaAction?.Dispose();

            if (hintCanvasGO != null)
                DestroyImmediate(hintCanvasGO);
        }

        protected virtual void Update()
        {
            HandleMovement();
            HandleRotation();
        }

        protected virtual void HandleMovement()
        {
            currentSpeed = shiftAction.ReadValue<float>() > 0 ? fastMoveSpeed : moveSpeed;
            var moveInput = moveAction.ReadValue<Vector2>();
            var elevationInput = elevateAction.ReadValue<float>();

            moveDirection = new Vector3(moveInput.x, elevationInput, moveInput.y);
            transform.Translate(moveDirection * (currentSpeed * Time.deltaTime), Space.Self);
        }

        protected virtual void HandleRotation()
        {
            float yaw, pitchDelta;

            if (rightClickAction.ReadValue<float>() > 0)
            {
                var mouseDelta = mouseDeltaAction.ReadValue<Vector2>();
                yaw = mouseDelta.x * rotationSpeed * Time.deltaTime;
                pitchDelta = -mouseDelta.y * rotationSpeed * Time.deltaTime;
            }
            else
            {
                var rotateInput = rotateAction.ReadValue<Vector2>();
                var multiplied = rotationSpeed * keyboardRotationMultiplier * Time.deltaTime;
                yaw = rotateInput.x * multiplied;
                pitchDelta = -rotateInput.y * multiplied;
            }

            currentPitch = Mathf.Clamp(currentPitch + pitchDelta, minPitch, maxPitch);

            transform.Rotate(Vector3.up, yaw, Space.World);
            var euler = transform.localEulerAngles;
            euler.x = currentPitch;
            transform.localEulerAngles = euler;
        }

        #region Control Hint UI

        private void CreateHintUI()
        {
            // ── Canvas ──
            hintCanvasGO = new GameObject("Canvas")
            {
                layer = LayerMask.NameToLayer("UI")
            };
            hintCanvasGO.transform.SetParent(transform);

            var canvas = hintCanvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999;

            var scaler = hintCanvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            hintCanvasGO.AddComponent<GraphicRaycaster>();

            // ── Panel (anchored bottom-right) ──
            hintPanelGO = CreateUIObject("ControlHintPanel", hintCanvasGO.transform);
            var panelRect = hintPanelGO.GetComponent<RectTransform>();
            SetAnchor(panelRect, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f));
            panelRect.anchoredPosition = new Vector2(-20f, 20f);
            panelRect.sizeDelta = hintPanelSize;

            var panelImage = hintPanelGO.AddComponent<Image>();
            panelImage.color = new Color(0f, 0f, 0f, 0.7f);

            var panelLayout = hintPanelGO.AddComponent<VerticalLayoutGroup>();
            panelLayout.padding = new RectOffset(12, 12, 8, 12);
            panelLayout.spacing = 4f;
            panelLayout.childAlignment = TextAnchor.UpperLeft;
            panelLayout.childControlWidth = true;
            panelLayout.childControlHeight = false;
            panelLayout.childForceExpandWidth = true;
            panelLayout.childForceExpandHeight = false;

            // ── Header row (title + X button) ──
            var headerGO = CreateUIObject("Header", hintPanelGO.transform);
            var headerRect = headerGO.GetComponent<RectTransform>();
            headerRect.sizeDelta = new Vector2(0f, 30f);

            var headerLayout = headerGO.AddComponent<HorizontalLayoutGroup>();
            headerLayout.childAlignment = TextAnchor.MiddleLeft;
            headerLayout.childControlWidth = true;
            headerLayout.childControlHeight = true;
            headerLayout.childForceExpandWidth = true;
            headerLayout.childForceExpandHeight = true;

            // Title
            var titleGO = CreateUIObject("Title", headerGO.transform);
            var titleTMP = titleGO.AddComponent<TextMeshProUGUI>();
            titleTMP.text = "Controls";
            titleTMP.fontSize = hintFontSize + 2;
            titleTMP.fontStyle = FontStyles.Bold;
            titleTMP.color = Color.white;
            titleTMP.alignment = TextAlignmentOptions.MidlineLeft;

            // X / ||| toggle button
            var toggleBtnGO = CreateUIObject("ToggleButton", headerGO.transform);
            var toggleBtnLayout = toggleBtnGO.AddComponent<LayoutElement>();
            toggleBtnLayout.preferredWidth = 30f;

            var toggleBtnImage = toggleBtnGO.AddComponent<Image>();
            toggleBtnImage.color = new Color(1f, 1f, 1f, 0.15f);

            var toggleBtn = toggleBtnGO.AddComponent<Button>();
            toggleBtn.targetGraphic = toggleBtnImage;
            var btnColors = toggleBtn.colors;
            btnColors.highlightedColor = new Color(1f, 1f, 1f, 0.3f);
            btnColors.pressedColor = new Color(1f, 1f, 1f, 0.4f);
            toggleBtn.colors = btnColors;
            toggleBtn.onClick.AddListener(ToggleHintExpanded);

            toggleButtonText = toggleBtnGO.AddComponent<TextMeshProUGUI>() 
                               ?? toggleBtnGO.GetComponent<TextMeshProUGUI>();
            // Button already has Image, so TMP is added separately — we rely on overlay
            // Actually, create a child text for the button
            DestroyImmediate(toggleButtonText);
            var toggleTextGO = CreateUIObject("Text", toggleBtnGO.transform);
            var toggleTextRect = toggleTextGO.GetComponent<RectTransform>();
            toggleTextRect.anchorMin = Vector2.zero;
            toggleTextRect.anchorMax = Vector2.one;
            toggleTextRect.sizeDelta = Vector2.zero;
            toggleTextRect.anchoredPosition = Vector2.zero;
            toggleButtonText = toggleTextGO.AddComponent<TextMeshProUGUI>();
            toggleButtonText.text = "x";
            toggleButtonText.fontSize = hintFontSize;
            toggleButtonText.color = Color.white;
            toggleButtonText.alignment = TextAlignmentOptions.Center;

            // ── Hint body text ──
            var bodyGO = CreateUIObject("HintBody", hintPanelGO.transform);
            var bodyLayout = bodyGO.AddComponent<LayoutElement>();
            bodyLayout.flexibleHeight = 1f;

            hintText = bodyGO.AddComponent<TextMeshProUGUI>();
            hintText.text = HintContent;
            hintText.fontSize = hintFontSize;
            hintText.color = new Color(0.9f, 0.9f, 0.9f, 1f);
            hintText.alignment = TextAlignmentOptions.TopLeft;
            hintText.lineSpacing = 4f;
            hintText.textWrappingMode = TextWrappingModes.Normal;

            RefreshHintExpandedState();
        }

        private void ToggleHintExpanded()
        {
            hintExpanded = !hintExpanded;
            RefreshHintExpandedState();
        }

        private void RefreshHintExpandedState()
        {
            if (hintText != null)
                hintText.gameObject.SetActive(hintExpanded);

            if (toggleButtonText != null)
                toggleButtonText.text = hintExpanded ? "x" : "|||";

            if (hintPanelGO != null)
            {
                var rect = hintPanelGO.GetComponent<RectTransform>();
                rect.sizeDelta = hintExpanded
                    ? hintPanelSize
                    : new Vector2(hintPanelSize.x, 46f); // collapsed: header only
            }
        }

        private static GameObject CreateUIObject(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform))
            {
                layer = LayerMask.NameToLayer("UI")
            };
            go.transform.SetParent(parent, false);
            return go;
        }

        private static void SetAnchor(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
        }

        #endregion

        protected void OnDrawGizmosSelected()
        {
            Handles.Label(transform.position + transform.forward * directionsGizmosLength / 2
                , "Camera Forward");

            if (alwaysShowDirectionsGizmos) return;
            Gizmos.color = directionsGizmosColor;
            Gizmos.DrawRay(transform.position, transform.forward * directionsGizmosLength);
        }

        protected void OnDrawGizmos()
        {
            if (!alwaysShowDirectionsGizmos) return;
            Gizmos.color = directionsGizmosColor;
            Gizmos.DrawRay(transform.position, transform.forward * directionsGizmosLength);
        }
#endif
    }
}