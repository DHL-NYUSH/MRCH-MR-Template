// Copyright (c) 2026 Digital Heritage Lab / Shengyang "Billiton" Peng
// Licensed under the MIT License. See LICENSE for details.

using DG.Tweening;
using MRCH.Toast.Interface;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace MRCH.Toast
{
    /// <summary>
    /// Manages a single reusable toast notification in an Overlay Canvas.
    /// Uses kill-and-replace: calling ShowToast while one is active kills the current and starts a new one.
    /// <br/><br/>
    /// The toast prefab must have a component implementing <see cref="IToastInstance"/>
    /// (e.g. <see cref="DefaultToastInstance"/>).
    /// <br/><br/>
    /// <b>Animation flow:</b>
    /// <list type="number">
    ///   <item>Start at restPosition - offset, alpha 0.5</item>
    ///   <item>Tween up to restPosition, alpha 1.0 (enter)</item>
    ///   <item>Hold for displayDuration seconds</item>
    ///   <item>Tween up to restPosition + offset, alpha 0.0 (exit)</item>
    /// </list>
    /// <br/>
    /// <b>Students:</b> Use the UnityEvents (OnToastShow, OnToastFullyVisible, etc.) to trigger
    /// sounds, particles, or any other effects without writing code.
    /// Call <c>ToastManager.Instance.ShowToast("message")</c> from a UnityEvent
    /// or use the provided wrapper components.
    /// </summary>
    public abstract class ToastManager : MonoBehaviour
    {
        #region Singleton

        private static ToastManager _instance;
        public static ToastManager Instance;

        protected virtual void Awake()
        {
            if (_instance && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            Instance = this;

            InitializeToastInstance();
        }

        protected virtual void OnDestroy()
        {
            KillCurrentToast();

            if (_instance != this) return;
            _instance = null;
            Instance = null;
        }

        #endregion

        #region Settings

        [TitleGroup("Prefab")]
        [SerializeField, Required,
         Tooltip("The toast prefab or scene GameObject. Must have a component implementing IToastInstance (e.g. DefaultToastInstance). " +
                 "If a scene object is assigned, it will be used directly instead of instantiating a new one.")]
        private GameObject toastPrefab;

        [TitleGroup("Prefab")]
        [SerializeField, Required,
         Tooltip("Parent transform for the instantiated toast. Should be inside an Overlay Canvas.")]
        private Transform toastParent;

        [TitleGroup("Position")]
        [SerializeField, Tooltip("The anchored position where the toast rests when fully visible.")]
        private Vector2 restAnchoredPosition = new(0f, -400f);

        [TitleGroup("Position")]
        [SerializeField, Min(0f), Tooltip("Y offset for enter/exit animation. Toast enters from below and exits upward.")]
        private float animationOffset = 80f;

        [TitleGroup("Timing")]
        [SerializeField, Min(0f), Tooltip("How long (seconds) the toast stays fully visible before exiting.")]
        private float displayDuration = 2f;

        [TitleGroup("Timing")]
        [SerializeField, Min(0f), Tooltip("Duration (seconds) of the enter tween.")]
        private float enterDuration = 0.35f;

        [TitleGroup("Timing")]
        [SerializeField, Min(0f), Tooltip("Duration (seconds) of the exit tween.")]
        private float exitDuration = 0.3f;

        [TitleGroup("Easing")]
        [SerializeField, Tooltip("Ease curve for the enter animation.")]
        private Ease enterEase = Ease.OutCubic;

        [TitleGroup("Easing")]
        [SerializeField, Tooltip("Ease curve for the exit animation.")]
        private Ease exitEase = Ease.InCubic;

        #endregion

        #region Settings (Protected Accessors)

        protected Vector2 RestAnchoredPosition => restAnchoredPosition;
        protected float AnimationOffset => animationOffset;
        protected float DisplayDuration => displayDuration;
        protected float EnterDuration => enterDuration;
        protected float ExitDuration => exitDuration;
        protected Ease EnterEase => enterEase;
        protected Ease ExitEase => exitEase;
        protected Transform ToastParent => toastParent;
        protected bool DebugMode => debugMode;

        #endregion

        #region Events

        [FoldoutGroup("Events")]
        [Tooltip("Fired when a toast starts showing (before enter animation).")]
        public UnityEvent onToastShow;

        [FoldoutGroup("Events")]
        [Tooltip("Fired when the toast reaches its rest position and is fully visible.")]
        public UnityEvent onToastFullyVisible;

        [FoldoutGroup("Events")]
        [Tooltip("Fired when the toast starts its exit animation.")]
        public UnityEvent onToastHideStart;

        [FoldoutGroup("Events")]
        [Tooltip("Fired when the toast is fully hidden after the exit animation.")]
        public UnityEvent onToastFullyHidden;

        #endregion

        #region Debug

        [FoldoutGroup("Debug")]
        [SerializeField, ToggleLeft, Tooltip("Enable detailed logging for debugging the toast system.")]
        private bool debugMode;

        [FoldoutGroup("Debug")]
        [ShowInInspector, ReadOnly, ShowIf("debugMode")]
        private bool _isToastActive;

        [FoldoutGroup("Debug")]
        [ShowInInspector, ReadOnly, ShowIf("debugMode")]
        private string _lastMessage = "(none)";

        #endregion

        #region Runtime State

        private IToastInstance _toastInstance;
        private Sequence _currentSequence;
        private bool _initialized;

        /// <summary>
        /// The current toast instance. Available after Awake/initialization.
        /// </summary>
        protected IToastInstance ToastInstance => _toastInstance;

        /// <summary>
        /// The currently active DOTween Sequence, or null if no toast is animating.
        /// </summary>
        protected Sequence CurrentSequence => _currentSequence;

        /// <summary>
        /// Whether the toast system has been successfully initialized.
        /// </summary>
        protected bool Initialized => _initialized;

        #endregion

        #region Initialization

        protected virtual void InitializeToastInstance()
        {
            if (!toastParent)
            {
                toastParent = transform;
                Log("Toast parent was null, using this transform as parent.");
            }

            if (!toastPrefab)
            {
                LogError("Toast prefab / scene object is not assigned! Cannot initialize.");
                return;
            }

            GameObject go;
            var isSceneObject = toastPrefab.scene.IsValid(); // true if it's already in a scene

            if (isSceneObject)
            {
                // Use the scene object directly
                go = toastPrefab;
                Log($"Using existing scene object '{go.name}' as toast instance.");
            }
            else
            {
                // Instantiate from prefab asset
                go = Instantiate(toastPrefab, toastParent);
                Log($"Instantiated toast instance from prefab '{toastPrefab.name}' under '{toastParent.name}'.");
            }

            _toastInstance = go.GetComponent<IToastInstance>();

            if (_toastInstance == null)
            {
                LogError(
                    $"Toast object '{go.name}' does not have a component implementing IToastInstance! " +
                    "Add DefaultToastInstance or your own implementation to the object root.");

                if (!isSceneObject) Destroy(go);
                return;
            }

            _toastInstance.Initialize();
            _initialized = true;
        }

        #endregion

        #region Public API

        /// <summary>
        /// Show a toast with a text message and optional icon.
        /// Kills any currently active toast before showing.
        /// <br/><br/>
        /// <b>Students:</b> You can call this from a UnityEvent by dragging the ToastManager
        /// and selecting ShowToast(string).
        /// </summary>
        [Button("Preview Toast (Play Mode Only)"), ShowIf("@UnityEngine.Application.isPlaying")]
        public virtual void ShowToast(
            [Tooltip("The message to display")] string message,
            [Tooltip("Optional icon sprite")] Sprite icon = null)
        {
            if (!_initialized)
            {
                LogError("ToastManager is not initialized. Check that the prefab is assigned and has an IToastInstance.");
                return;
            }

            Log($"ShowToast called — message: \"{message}\", icon: {(icon ? icon.name : "null")}");

            // Kill any active toast immediately
            KillCurrentToast();

            // Prepare instance
            _toastInstance.ResetContent();
            _toastInstance.SetContent(message, icon);

            _lastMessage = message;
            _isToastActive = true;

            // Build and play animation
            _currentSequence = CreateAnimationSequence();
            _currentSequence.Play();

            Log("Toast animation sequence started.");
        }

        /// <summary>
        /// Convenience overload: show a toast with just a message (no icon).
        /// Useful for UnityEvent wiring where you only need a string parameter.
        /// </summary>
        public void ShowToast(string message) => ShowToast(message, null);

        /// <summary>
        /// Immediately kill and hide the current toast if active.
        /// </summary>
        [Button("Kill Toast (Play Mode Only)"), ShowIf("@UnityEngine.Application.isPlaying && _isToastActive")]
        public virtual void KillCurrentToast()
        {
            if (_currentSequence != null && _currentSequence.IsActive())
            {
                _currentSequence.Kill();
                Log("Killed active toast sequence.");
            }

            _currentSequence = null;

            if (_toastInstance != null)
            {
                ResetInstanceVisual();
            }

            _isToastActive = false;
        }

        /// <summary>
        /// Returns true if a toast is currently being displayed or animating.
        /// </summary>
        public bool IsToastActive => _isToastActive;

        #endregion

        #region Animation

        /// <summary>
        /// Creates the DOTween Sequence for the toast enter → hold → exit cycle.
        /// Override this to customize the animation behavior.
        /// </summary>
        protected virtual Sequence CreateAnimationSequence()
        {
            var rt = _toastInstance.RectTransform;
            var cg = _toastInstance.CanvasGroup;

            // Calculate positions
            var enterStart = new Vector2(restAnchoredPosition.x, restAnchoredPosition.y - animationOffset);
            var exitEnd = new Vector2(restAnchoredPosition.x, restAnchoredPosition.y + animationOffset);

            // Set initial state
            rt.anchoredPosition = enterStart;
            cg.alpha = 0.5f;
            _toastInstance.GameObj.SetActive(true);

            Log($"Animation — enter from {enterStart}, rest at {restAnchoredPosition}, exit to {exitEnd}");

            var seq = DOTween.Sequence();

            // --- Enter ---
            seq.AppendCallback(() =>
            {
                Log("Animation phase: ENTER");
                onToastShow?.Invoke();
            });
            seq.Append(rt.DOAnchorPos(restAnchoredPosition, enterDuration).SetEase(enterEase));
            seq.Join(cg.DOFade(1f, enterDuration).SetEase(enterEase));

            // --- Fully Visible ---
            seq.AppendCallback(() =>
            {
                Log("Animation phase: FULLY VISIBLE");
                onToastFullyVisible?.Invoke();
            });
            seq.AppendInterval(displayDuration);

            // --- Exit ---
            seq.AppendCallback(() =>
            {
                Log("Animation phase: EXIT");
                onToastHideStart?.Invoke();
            });
            seq.Append(rt.DOAnchorPos(exitEnd, exitDuration).SetEase(exitEase));
            seq.Join(cg.DOFade(0f, exitDuration).SetEase(exitEase));

            // --- Complete ---
            seq.AppendCallback(() =>
            {
                Log("Animation phase: COMPLETE — toast fully hidden.");
                _toastInstance.GameObj.SetActive(false);
                _isToastActive = false;
                onToastFullyHidden?.Invoke();
            });

            seq.SetAutoKill(true);
            seq.SetUpdate(true); // Use unscaled time so toasts work even when Time.timeScale is 0

            return seq;
        }

        /// <summary>
        /// Reset the toast instance to its hidden state.
        /// Override this if you have additional visual elements to reset.
        /// </summary>
        protected virtual void ResetInstanceVisual()
        {
            if (_toastInstance is not Object obj || !obj) return;
            if (!_toastInstance.GameObj) return;
    
            var cg = _toastInstance.CanvasGroup;
            if (!cg) return;
    
            cg.alpha = 0f;
            _toastInstance.GameObj.SetActive(false);
        }

        #endregion

        #region Logging

        private const string LOG_PREFIX = "<b>[ToastManager]</b> ";

        protected void Log(string message)
        {
            if (!debugMode) return;
            Debug.Log($"{LOG_PREFIX}{message}", this);
        }

        protected void LogWarning(string message)
        {
            if (!debugMode) return;
            Debug.LogWarning($"{LOG_PREFIX}{message}", this);
        }

        protected void LogError(string message)
        {
            // Errors always log regardless of debug mode
            Debug.LogError($"{LOG_PREFIX}{message}", this);
        }

        #endregion

        #region Editor Helpers

#if UNITY_EDITOR
        [FoldoutGroup("Debug")]
        [Button("Log Current State"), ShowIf("debugMode")]
        private void LogCurrentState()
        {
            Debug.Log($"{LOG_PREFIX}--- Toast Manager State ---\n" +
                      $"  Initialized: {_initialized}\n" +
                      $"  Instance: {(_toastInstance != null ? "OK" : "NULL")}\n" +
                      $"  Active: {_isToastActive}\n" +
                      $"  Sequence alive: {(_currentSequence != null && _currentSequence.IsActive())}\n" +
                      $"  Last message: \"{_lastMessage}\"",
                this);
        }
#endif

        #endregion
    }
}