// Copyright (c) 2026 Digital Heritage Lab / Shengyang "Billiton" Peng
// Licensed under the MIT License. See LICENSE for details.

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;

namespace MRCH.Tools.Tasks
{
    
    #region Task Class

    public enum InitializationTiming
    {
        [LabelText("On Enable")]
        OnEnable,
        [LabelText("Start")]
        Start
    }

    /// <summary>
    /// UnityEvent with (int completed, int total) parameters.
    /// You can also wire parameterless methods via the static call mode in the Inspector.
    /// </summary>
    [Serializable]
    public class ProgressUnityEvent : UnityEvent<int, int> { }

    // ─── Task Definition ─────────────────────────────────────────────────

    [Serializable]
    public class ConditionTask
    {
        [TitleGroup("$TitleLabel", "$Subtitle")]

        [TitleGroup("$TitleLabel")]
        [HorizontalGroup("$TitleLabel/Row1")]
        [LabelWidth(20), LabelText("ID")]
        [Required("Task ID cannot be empty.")]
        public string id;

        [HorizontalGroup("$TitleLabel/Row1", Width = 130)]
        [LabelWidth(95)]
        [ToggleLeft][Tooltip("Events of the task can be re-trigger multiple times. You can get the times count by its CompletionCount. Re-trigger will not affect the completing progress.")]
        public bool allowRetrigger;

#if UNITY_EDITOR
        [TitleGroup("$TitleLabel")]
        [PropertyOrder(1)]
        [TextArea(1, 3)]
        [LabelText("Description (Editor Only)")]
        public string editorDescription;
#endif

        [TitleGroup("$TitleLabel")]
        [PropertyOrder(2)]
        [LabelText("Prerequisite Task ID")]
        [InfoBox("Locked until the prerequisite task is completed.",
            VisibleIf = "HasPrerequisite")]
        public string prerequisiteTaskId;

        [TitleGroup("$TitleLabel")]
        [PropertyOrder(10)]
        [LabelText("On Task Completed")]
        public UnityEvent onTaskCompleted;

        // ── Runtime state (not serialized) ──

        [NonSerialized,ShowInInspector, HideInEditorMode, ReadOnly] 
        public bool isCompleted;
        [NonSerialized] 
        public int completionCount;

        // ── Odin helpers ──

        public bool HasPrerequisite => !string.IsNullOrEmpty(prerequisiteTaskId);

#if UNITY_EDITOR
        private string TitleLabel => string.IsNullOrEmpty(id) ? "(no id)" : id;

        private string Subtitle
        {
            get
            {
                if (string.IsNullOrEmpty(editorDescription)) return "";
                return editorDescription.Length > 80
                    ? editorDescription[..80] + "…"
                    : editorDescription;
            }
        }
#endif
    }
    
    #endregion

    [HideMonoScript]
    public abstract class MultiConditionEventManager : MonoBehaviour
    {
        // ── Settings ──

        [FoldoutGroup("Settings"), PropertyOrder(20)]
        [LabelText("Initialize On")]
        [Tooltip("When to build the task dictionary and fire OnInitialized.")]
        public InitializationTiming initializeTiming = InitializationTiming.OnEnable;

        [FoldoutGroup("Settings"), PropertyOrder(20)]
        [ToggleLeft]
        [Tooltip("Enable detailed logging in the Console and runtime status in the Inspector.")]
        public bool debugMode;

        // ── Tasks ──

        [TitleGroup("Tasks"), PropertyOrder(0)]
        [ListDrawerSettings(ShowFoldout = true, DraggableItems = true)]
        [SerializeField]
        private List<ConditionTask> tasks = new();

        // ── Events ──

        [FoldoutGroup("Events"), PropertyOrder(10)]
        [LabelText("On Initialized")]
        [Tooltip("Fired once when the manager initializes (OnEnable or Start).")]
        public UnityEvent onInitialized;

        [FoldoutGroup("Events"), PropertyOrder(10)]
        [LabelText("On All Tasks Completed")]
        [Tooltip("Fired when every task has been completed.")]
        public UnityEvent onAllTasksCompleted;

        [FoldoutGroup("Events"), PropertyOrder(10)]
        [LabelText("On Progress Changed (completed, total)")]
        [Tooltip("Fired each time a new task is completed. Parameters: (completedCount, totalCount). " +
                 "You can also wire parameterless methods here.")]
        public ProgressUnityEvent onProgressChanged;

        [FoldoutGroup("Events"), PropertyOrder(10)]
        [LabelText("On Reset")]
        [Tooltip("Fired when ResetAll() is called.")]
        public UnityEvent onReset;

        // ── Runtime State ──

        private readonly Dictionary<string, ConditionTask> _taskDict = new();
        private int _completedCount;
        private bool _allCompleted;
        private bool _initialized;

        // ── Protected Access ──

        /// <summary>Read-only access to task dictionary for subclasses.</summary>
        protected IReadOnlyDictionary<string, ConditionTask> TaskDict => _taskDict;

        /// <summary>Current number of completed tasks.</summary>
        protected int CompletedCount => _completedCount;

        /// <summary>Total number of registered tasks.</summary>
        protected int TotalCount => _taskDict.Count;

        /// <summary>Whether all tasks are completed.</summary>
        protected bool IsAllCompleted => _allCompleted;

        /// <summary>Read-only access to the task list for subclasses.</summary>
        protected IReadOnlyList<ConditionTask> TaskList => tasks;

        // ════════════════════════════════════════════════════════════════
        //  Virtual Hooks — override these in subclasses
        // ════════════════════════════════════════════════════════════════

        /// <summary>
        /// Called after the dictionary is built and validated, just before OnInitialized fires.
        /// </summary>
        protected virtual void OnManagerInitialized() { }

        /// <summary>
        /// Called before a task is marked complete.
        /// Return false to block the completion (the task stays pending).
        /// </summary>
        /// <param name="task">The task about to be completed.</param>
        /// <returns>True to allow completion, false to block it.</returns>
        protected virtual bool OnTaskCompleting(ConditionTask task) => true;

        /// <summary>
        /// Called after a task is marked complete, before checking all-done.
        /// </summary>
        protected virtual void OnAfterTaskCompleted(ConditionTask task) { }

        /// <summary>
        /// Called after all tasks are completed, just before OnAllTasksCompleted fires.
        /// </summary>
        protected virtual void OnAfterAllTasksCompleted() { }

        /// <summary>
        /// Called after a single task is reset via ResetTask().
        /// </summary>
        protected virtual void OnAfterTaskReset(ConditionTask task) { }

        /// <summary>
        /// Called after all tasks are reset via ResetAll(), just before OnReset fires.
        /// </summary>
        protected virtual void OnAfterAllTasksReset() { }

        // ════════════════════════════════════════════════════════════════
        //  Runtime Debug Display
        // ════════════════════════════════════════════════════════════════

#if UNITY_EDITOR
        [ShowInInspector, ReadOnly]
        [ShowIf("@debugMode && UnityEngine.Application.isPlaying")]
        [FoldoutGroup("Runtime Status"), PropertyOrder(100)]
        private string Progress => $"{_completedCount} / {tasks.Count}";

        [ShowInInspector, ReadOnly]
        [ShowIf("@debugMode && UnityEngine.Application.isPlaying")]
        [FoldoutGroup("Runtime Status"), PropertyOrder(101)]
        [DictionaryDrawerSettings(IsReadOnly = true, DisplayMode = DictionaryDisplayOptions.Foldout)]
        private Dictionary<string, string> TaskStates
        {
            get
            {
                var dict = new Dictionary<string, string>();
                foreach (var task in tasks)
                {
                    if (string.IsNullOrEmpty(task.id)) continue;

                    string status;
                    if (task.HasPrerequisite &&
                        (!_taskDict.TryGetValue(task.prerequisiteTaskId, out var prereq) ||
                         !prereq.isCompleted))
                    {
                        status = "Locked";
                    }
                    else if (task.isCompleted)
                    {
                        status = "Completed";
                        if (task.allowRetrigger && task.completionCount > 1)
                            status += $" (×{task.completionCount})";
                    }
                    else
                    {
                        status = "Pending";
                    }

                    dict[task.id] = status;
                }
                return dict;
            }
        }
#endif

        // ════════════════════════════════════════════════════════════════
        //  Lifecycle
        // ════════════════════════════════════════════════════════════════

        protected virtual void OnEnable()
        {
            if (initializeTiming == InitializationTiming.OnEnable)
                Initialize();
        }

        protected virtual void Start()
        {
            if (initializeTiming == InitializationTiming.Start)
                Initialize();
        }

        protected virtual void OnDisable()
        {
            // Allow re-initialization on next OnEnable
            if (initializeTiming == InitializationTiming.OnEnable)
                _initialized = false;
        }

        private void Initialize()
        {
            if (_initialized) return;
            _initialized = true;

            BuildDictionary();
            ValidateTasks();

            Log($"Initialized with {tasks.Count} task(s).");

            OnManagerInitialized();
            onInitialized?.Invoke();
        }

        private void BuildDictionary()
        {
            _taskDict.Clear();
            _completedCount = 0;
            _allCompleted = false;

            foreach (var task in tasks)
            {
                // Reset runtime state
                task.isCompleted = false;
                task.completionCount = 0;

                if (string.IsNullOrEmpty(task.id))
                {
                    LogWarning("A task has an empty ID and will be skipped.");
                    continue;
                }

                if (!_taskDict.TryAdd(task.id, task))
                {
                    LogWarning($"Duplicate task ID '{task.id}' detected. " +
                               $"Only the first one is registered.");
                }
            }
        }

        private void ValidateTasks()
        {
            foreach (var task in tasks)
            {
                if (!task.HasPrerequisite) continue;

                if (task.prerequisiteTaskId == task.id)
                {
                    LogWarning($"Task '{task.id}' lists itself as a prerequisite — this will deadlock!");
                }
                else if (!_taskDict.ContainsKey(task.prerequisiteTaskId))
                {
                    LogWarning($"Task '{task.id}' has prerequisite '{task.prerequisiteTaskId}' " +
                               $"which does not exist in this manager.");
                }
            }
        }

        // ════════════════════════════════════════════════════════════════
        //  Public API — Call these from UnityEvent
        // ════════════════════════════════════════════════════════════════

        /// <summary>
        /// Mark a task as completed by its ID.
        /// Call this from InteractionTrigger or any other UnityEvent.
        /// </summary>
        public void CompleteTask(string taskId)
        {
            if (!_taskDict.TryGetValue(taskId, out var task))
            {
                LogWarning($"CompleteTask(\"{taskId}\") — no task with this ID exists. " +
                           $"Available IDs: [{string.Join(", ", _taskDict.Keys)}]");
                return;
            }

            // ── Prerequisite check ──
            if (task.HasPrerequisite)
            {
                if (!_taskDict.TryGetValue(task.prerequisiteTaskId, out var prereq) ||
                    !prereq.isCompleted)
                {
                    Log($"CompleteTask(\"{taskId}\") — blocked. " +
                        $"Prerequisite \"{task.prerequisiteTaskId}\" is not yet completed.");
                    return;
                }
            }

            // ── Already completed ──
            if (task.isCompleted)
            {
                if (task.allowRetrigger)
                {
                    task.completionCount++;
                    Log($"Task \"{taskId}\" re-triggered (×{task.completionCount}). " +
                        "Events fired again, but progress unchanged.");
                    task.onTaskCompleted?.Invoke();
                }
                else
                {
                    Log($"CompleteTask(\"{taskId}\") — already completed and retrigger is off. Ignored.");
                }
                return;
            }

            // ── Subclass gate ──
            if (!OnTaskCompleting(task))
            {
                Log($"CompleteTask(\"{taskId}\") — blocked by OnTaskCompleting override.");
                return;
            }

            // ── Complete ──
            task.isCompleted = true;
            task.completionCount++;
            _completedCount++;

            Log($"Task \"{taskId}\" completed! Progress: {_completedCount}/{_taskDict.Count}");

            OnAfterTaskCompleted(task);
            task.onTaskCompleted?.Invoke();
            onProgressChanged?.Invoke(_completedCount, _taskDict.Count);

            // ── All done? ──
            if (!_allCompleted && _completedCount >= _taskDict.Count)
            {
                _allCompleted = true;
                Log("★ All tasks completed!");
                OnAfterAllTasksCompleted();
                onAllTasksCompleted?.Invoke();
            }
        }

        /// <summary>
        /// Reset a specific task to incomplete.
        /// </summary>
        public void ResetTask(string taskId)
        {
            if (!_taskDict.TryGetValue(taskId, out var task))
            {
                LogWarning($"ResetTask(\"{taskId}\") — no task with this ID exists.");
                return;
            }

            if (!task.isCompleted)
            {
                Log($"ResetTask(\"{taskId}\") — already pending, nothing to reset.");
                return;
            }

            task.isCompleted = false;
            task.completionCount = 0;
            _completedCount = Mathf.Max(0, _completedCount - 1);
            _allCompleted = false;

            Log($"Task \"{taskId}\" reset. Progress: {_completedCount}/{_taskDict.Count}");

            OnAfterTaskReset(task);
            onProgressChanged?.Invoke(_completedCount, _taskDict.Count);
        }

        /// <summary>
        /// Reset all tasks and fire OnReset.
        /// </summary>
        public void ResetAll()
        {
            foreach (var task in _taskDict.Values)
            {
                task.isCompleted = false;
                task.completionCount = 0;
            }

            _completedCount = 0;
            _allCompleted = false;

            Log("All tasks reset.");

            OnAfterAllTasksReset();
            onProgressChanged?.Invoke(0, _taskDict.Count);
            onReset?.Invoke();
        }

        /// <summary>
        /// Query whether a specific task is completed (for advanced students).
        /// </summary>
        public bool IsTaskCompleted(string taskId)
        {
            if (_taskDict.TryGetValue(taskId, out var task))
                return task.isCompleted;

            LogWarning($"IsTaskCompleted(\"{taskId}\") — no task with this ID exists.");
            return false;
        }

        /// <summary>
        /// Query whether all tasks are completed.
        /// </summary>
        public bool AreAllTasksCompleted() => _allCompleted;

        // ════════════════════════════════════════════════════════════════
        //  Logging — protected so subclasses can use them
        // ════════════════════════════════════════════════════════════════

        protected void Log(string message)
        {
            if (debugMode)
                Debug.Log($"[ConditionManager \"{gameObject.name}\"] {message}", this);
        }

        protected void LogWarning(string message)
        {
            // Warnings always print regardless of debug mode
            Debug.LogWarning($"[ConditionManager \"{gameObject.name}\"] {message}", this);
        }

        // ════════════════════════════════════════════════════════════════
        //  Editor Validation Button
        // ════════════════════════════════════════════════════════════════

#if UNITY_EDITOR
        [TitleGroup("Tasks")]
        [Button("Validate Tasks", ButtonSizes.Medium), PropertyOrder(-1)]
        [GUIColor(0.7f, 0.9f, 1f)]
        private void EditorValidateTasks()
        {
            var ids = new HashSet<string>();
            var issues = 0;

            for (var i = 0; i < tasks.Count; i++)
            {
                var t = tasks[i];

                if (string.IsNullOrEmpty(t.id))
                {
                    Debug.LogWarning($"[Validate] Task at index {i} has an empty ID.", this);
                    issues++;
                }
                else if (!ids.Add(t.id))
                {
                    Debug.LogWarning($"[Validate] Duplicate ID \"{t.id}\" at index {i}.", this);
                    issues++;
                }

                if (t.HasPrerequisite)
                {
                    if (t.prerequisiteTaskId == t.id)
                    {
                        Debug.LogWarning($"[Validate] Task \"{t.id}\" has itself as prerequisite.", this);
                        issues++;
                    }
                    else if (tasks.All(other => other.id != t.prerequisiteTaskId))
                    {
                        Debug.LogWarning(
                            $"[Validate] Task \"{t.id}\" prerequisite \"{t.prerequisiteTaskId}\" not found.",
                            this);
                        issues++;
                    }
                }
            }

            if (issues == 0)
                Debug.Log($"[Validate] All {tasks.Count} task(s) are valid.", this);
            else
                Debug.LogWarning($"[Validate] Found {issues} issue(s). Check warnings above.", this);
        }
#endif
    }
}