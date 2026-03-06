// Copyright (c) 2026 Digital Heritage Lab / Shengyang "Billiton" Peng
// Licensed under the MIT License. See LICENSE for details.

using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MRCH.Interact.Trigger
{
    /// <summary>
    /// author: Shengyang Billiton Peng
    /// 
    /// Enables three kinds of triggers to invoke events: collider, distance, and look at.
    /// DO NOT CHANGE THE SCRIPT! COPY THE CODE TO YOUR FOLDER AND CHANGE THE CLASS NAME IF YOU WANT TO MODIFY IT.
    /// You can inherit this class and override specific methods.
    /// </summary>
    public abstract class InteractionTrigger : MonoBehaviour
    {
        #region Variables

        #region Collider Trigger

        [Title("Collider Trigger"), SerializeField]
        [Tooltip("Enable trigger zone detection using a Collider component on this GameObject")]
        private bool useColliderTrigger;
        
        private Collider _colliderTrigger;

        [Space, ShowIf("useColliderTrigger"), SerializeField, Indent]
        [Tooltip("Fires once the first time a Player enters the trigger collider")]
        private UnityEvent onTriggerFirstEnter;

        [ShowIf("useColliderTrigger"), SerializeField, Indent]
        [Tooltip("Fires every time a Player enters the trigger collider")]
        private UnityEvent onTriggerEnter;

        [ShowIf("useColliderTrigger"), SerializeField, Indent]
        [Tooltip("Fires when a Player exits the trigger collider")]
        private UnityEvent onTriggerExit;

        private bool _firstColliderEnter = true;

        #endregion

        #region Distance Trigger

        [Title("Distance Trigger"), Space, SerializeField]
        [Tooltip("Enable proximity detection based on distance to the Player")]
        private bool useDistanceTrigger;

        [ShowIf("useDistanceTrigger"), SerializeField, Indent, Unit(Units.Meter)]
        [Tooltip("Radius within which the Player triggers distance events")]
        protected float distance = 10f;

        [Space, ShowIf("useDistanceTrigger"), SerializeField, Indent]
        [Tooltip("Fires once the first time the Player enters the distance range")]
        private UnityEvent onDistanceFirstEnter;

        [ShowIf("useDistanceTrigger"), SerializeField, Indent]
        [Tooltip("Fires every time the Player enters the distance range")]
        private UnityEvent onDistanceEnter;

        [ShowIf("useDistanceTrigger"), SerializeField, Indent]
        [Tooltip("Fires when the Player moves outside the distance range")]
        private UnityEvent onDistanceExit;

        private bool _firstDistanceEnter = true;
        private bool _alreadyInDistance;

        #endregion

        #region LookAt Trigger

        [Title("LookAt Trigger"), Space, SerializeField]
        [Tooltip("Enable gaze detection — fires when the Player looks at this object")]
        private bool useLookAtTrigger;

        [ShowIf("useLookAtTrigger"), SerializeField, Indent, Unit(Units.Degree)]
        [Tooltip("Max angle (degrees) between Player forward and direction to this object to count as 'looking at'")]
        protected float lookAtAngle = 25f;

        [ShowIf("useLookAtTrigger"), SerializeField, Indent, Unit(Units.Meter), MinValue(0f)]
        [Tooltip("Max distance for look-at detection to be active. 0 means unlimited")]
        [InfoBox("0 means unlimited", VisibleIf = "@lookAtDistance == 0")]
        protected float lookAtDistance;

        [Space, ShowIf("useLookAtTrigger"), SerializeField, Indent]
        [Tooltip("Fires once the first time the Player looks at this object")]
        private UnityEvent onLookAtFirstEnter;

        [ShowIf("useLookAtTrigger"), SerializeField, Indent]
        [Tooltip("Fires every time the Player looks at this object")]
        private UnityEvent onLookAtEnter;

        [ShowIf("useLookAtTrigger"), SerializeField, Indent]
        [Tooltip("Fires when the Player moves outside the lookAt distance range")]
        private UnityEvent onLookAtDistanceExit;

        private bool _firstLookAtEnter = true;
        private bool _alreadyLookAt;

        #endregion

        #region Unity Lifecycle Events

        [Title("Events Triggers"), Space, SerializeField]
        [Tooltip("Enable Unity lifecycle event triggers (Start, OnEnable, Update, OnDisable)")]
        private bool useEventsTriggers;

        [Space, ShowIf("useEventsTriggers"), SerializeField, Indent]
        [Tooltip("Fire an event on Start()")]
        private bool useStartTrigger;

        [ShowIf("@this.useEventsTriggers && this.useStartTrigger"), SerializeField, Indent(2)]
        [Tooltip("Fires during Start()")]
        private UnityEvent onStart;

        [ShowIf("useEventsTriggers"), SerializeField, Indent]
        [Tooltip("Fire an event on OnEnable()")]
        private bool useOnEnableTrigger;

        [ShowIf("@this.useEventsTriggers && this.useOnEnableTrigger"), SerializeField, Indent(2)]
        [Tooltip("Fires during OnEnable()")]
        private UnityEvent onEnable;

        [ShowIf("useEventsTriggers"), SerializeField, Indent]
        [Tooltip("Fire an event every Update() frame — use sparingly!")]
        private bool useUpdateTrigger;

        [ShowIf("@this.useEventsTriggers && this.useUpdateTrigger"), SerializeField, Indent(2),
         InfoBox("WAIT, ARE YOU SURE YOU NEED THIS??", InfoMessageType.Warning, "useUpdateTrigger")]
        [Tooltip("Fires every Update() frame")]
        private UnityEvent onUpdate;

        [ShowIf("useEventsTriggers"), SerializeField, Indent]
        [Tooltip("Fire an event on OnDisable()")]
        private bool useOnDisableTrigger;

        [ShowIf("@this.useEventsTriggers && this.useOnDisableTrigger"), SerializeField, Indent(2)]
        [Tooltip("Fires during OnDisable()")]
        private UnityEvent onDisable;

        #endregion

        #region Global Variables

        private GameObject _player;
        private Transform _playerTransform;
        private Collider _playerCollider; 
        
        private bool _hasInitialized;
        
        /// <summary>
        /// Distance and LookAt triggers are checked once every N frames to reduce CPU overhead.
        /// 25 means checks run at ~2fps on a 50fps game, ~2.4fps on 60fps.
        /// Lower = more responsive but more expensive. Collider triggers are physics-driven and unaffected.
        /// </summary>
        private const int CheckRateFreq = 25; 

        #endregion

        #region Setting

        [Space, Title("Setting", bold: false), SerializeField]
        [Tooltip("Draw gizmos in Scene view for distance and lookAt ranges")]
        protected bool showGizmos = true;

        [InfoBox("This will print a log when any events are triggered", "debugMode"), SerializeField]
        [Tooltip("Log a message to Console whenever any event is triggered")]
        protected bool debugMode;

        #endregion

        #endregion

        protected virtual void Awake()
        {
            CheckAndInitSetting();

            _player = GameObject.FindGameObjectWithTag("Player");
            if (!_player)
            {
                Debug.LogError("No main camera found in the scene");
                return;
            }

            _playerTransform = _player.transform;
            _playerCollider = _player.GetComponent<Collider>();
        }

        protected virtual void Start()
        {
            if (useEventsTriggers && useStartTrigger)
                TriggerOnStart();
            
            _hasInitialized = true;
        }

        protected virtual void OnEnable()
        {
            if (_hasInitialized && useColliderTrigger && _colliderTrigger && _playerCollider)
            {
                var closestOnTrigger = _colliderTrigger.ClosestPoint(_playerTransform.position);
                var closestOnPlayer = _playerCollider.ClosestPoint(closestOnTrigger);
                if ((closestOnTrigger - closestOnPlayer).sqrMagnitude < 0.0001f)
                {
                    OnTriggerEnter(_playerCollider);
                }
            }

            if (useEventsTriggers && useOnEnableTrigger)
                TriggerOnEnable();
        }

        protected virtual void Update()
        {
            if (useDistanceTrigger && CheckRateLimiter(CheckRateFreq))
            {
                if (InDistance(distance) && !_alreadyInDistance)
                {
                    if (_firstDistanceEnter)
                    {
                        TriggerOnDistanceFirstEnter();
                        _firstDistanceEnter = false;
                    }

                    TriggerOnDistanceEnter();
                    _alreadyInDistance = true;
                }
                else if (!InDistance(distance) && _alreadyInDistance)
                {
                    TriggerOnDistanceExit();
                    _alreadyInDistance = false;
                }
            }

            if (useLookAtTrigger && CheckRateLimiter(CheckRateFreq))
            {
                if (InLookAtRange() && !_alreadyLookAt)
                {
                    if (Vector3.Angle(_playerTransform.forward,
                            (transform.position - _playerTransform.position).normalized) <= lookAtAngle)
                    {
                        if (_firstLookAtEnter)
                        {
                            TriggerOnLookAtFirstEnter();
                            _firstLookAtEnter = false;
                        }

                        TriggerOnLookAtEnter();
                        _alreadyLookAt = true;
                    }
                }
                else if (!InLookAtRange() && _alreadyLookAt)
                {
                    TriggerOnLookAtExit();
                    _alreadyLookAt = false;
                }
            }

            if (useEventsTriggers && useUpdateTrigger)
                TriggerOnUpdate();
        }

        protected virtual void OnDisable()
        {
            if (useEventsTriggers && useOnDisableTrigger)
                TriggerOnDisable();
        }

        protected bool InDistance(float dist)
        {
            return Vector3.Distance(transform.position, _playerTransform.position) <= dist;
        }
        
        protected bool InLookAtRange()
        {
            if (lookAtDistance <= 0f) return true; // 0 = unlimited
            if(!Application.isPlaying && Application.isEditor)
                if(Camera.main)
                    return Vector3.Distance(transform.position, Camera.main.transform.position) <= lookAtDistance;
                else
                    Debug.LogWarning("There is no main camera found in the scene.");
                
            return Vector3.Distance(transform.position, _playerTransform.position) <= lookAtDistance;
        }

        public virtual void OnTriggerEnter(Collider other)
        {
            if (!enabled) return; 
            if (!useColliderTrigger) return;
            if (!(other.CompareTag("Player") || other.CompareTag("MainCamera"))) return;

            if(debugMode)
                Debug.Log($"OnTriggerEnter: {other.name}, {other.tag}");
            
            if (_firstColliderEnter)
            {
                TriggerOnTriggerFirstEnter();
                _firstColliderEnter = false;
            }

            TriggerOnTriggerEnter();
        }

        protected virtual void OnTriggerExit(Collider other)
        {
            if (!enabled) return; 
            if (!useColliderTrigger) return;
            if (!other.CompareTag("Player")) return;

            TriggerOnTriggerExit();
        }

        public virtual void OnTriggerStay(Collider other)
        {
        }

        protected void CheckAndInitSetting()
        {
            if (useColliderTrigger)
            {
                _colliderTrigger = GetComponent<Collider>();
                if (!_colliderTrigger)
                    Debug.LogError("Collider Trigger is enabled but no collider is attached to " + gameObject.name);
                else if (!_colliderTrigger.isTrigger)
                    Debug.LogWarning("YOU SHOULD PROBABLY TURN ON 'isTrigger' of the collider on " + gameObject.name);

                if (onTriggerFirstEnter == null && onTriggerEnter == null && onTriggerExit == null)
                    Debug.LogWarning("No events are assigned to Collider Trigger on " + gameObject.name);
            }

            if (useDistanceTrigger)
            {
                if (distance == 0)
                    Debug.LogWarning("Distance Trigger is enabled but distance is set to 0 on " + gameObject.name);

                if (onDistanceFirstEnter == null && onDistanceEnter == null && onDistanceExit == null)
                    Debug.LogWarning("No events are assigned to Distance Trigger on " + gameObject.name);
            }

            if (useLookAtTrigger)
            {
                if (onLookAtFirstEnter == null && onLookAtEnter == null && onLookAtDistanceExit == null)
                    Debug.LogWarning("No events are assigned to LookAt Trigger on " + gameObject.name);
            }

            if (useEventsTriggers)
            {
                if (useStartTrigger && onStart == null)
                    Debug.LogWarning("No events are assigned to Start Trigger on " + gameObject.name);
                if (useOnEnableTrigger && onEnable == null)
                    Debug.LogWarning("No events are assigned to OnEnable Trigger on " + gameObject.name);
                if (useUpdateTrigger && onUpdate == null)
                    Debug.LogWarning("No events are assigned to Update Trigger on " + gameObject.name);
                if (useOnDisableTrigger && onDisable == null)
                    Debug.LogWarning("No events are assigned to OnDisable Trigger on " + gameObject.name);
            }
        }

        private static bool CheckRateLimiter(float frequency)
        {
            return Time.frameCount % frequency == 0;
        }

        #region Gizmos - Editor
#if UNITY_EDITOR
        protected void OnDrawGizmosSelected()
        {
            if (!enabled || !showGizmos) return;
            if (useDistanceTrigger)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(transform.position, distance);

                var labelPosition = transform.position + Vector3.forward * distance;
                Handles.Label(labelPosition, "Distance Trigger Range");

            }

            if (useLookAtTrigger)
            {
                if (Camera.main != null)
                {
                    var camTransform = Camera.main.transform;
                    var angle = Vector3.Angle(camTransform.forward,
                        (transform.position - camTransform.position).normalized);
        
                    var isLookingAt = angle <= lookAtAngle;
                    var isInRange = InLookAtRange(); // 0 = unlimited already handled

                    Color stateColor;
                    if (isLookingAt && isInRange)
                        stateColor = Color.green;         // looking at + in range
                    else if (isLookingAt || isInRange)
                        stateColor = Color.blue;          // one condition met, not both
                    else
                        stateColor = Color.red;           // neither

                    // Sphere range
                    Gizmos.color = stateColor;
                    Gizmos.DrawWireSphere(transform.position, lookAtDistance);

                    // Line from camera to object
                    Gizmos.color = stateColor;
                    Gizmos.DrawLine(camTransform.position, transform.position);

                    // Camera forward direction preview
                    Gizmos.color = Color.white;
                    Gizmos.DrawLine(camTransform.position,
                        camTransform.position + camTransform.forward * 3f);

                    // Labels
                    Handles.color = stateColor;
                    Handles.Label((camTransform.position + transform.position) / 2,
                        $"Angle: {angle:F2}°  {(isLookingAt ? "👀" : "")} {(isInRange ? "📏" : "")}");
                    
                    Handles.color = Color.white;
                    Handles.Label(transform.position + Vector3.forward * lookAtDistance,
                        "LookAt Trigger Distance Range");

                }
            }
        }
#endif
        #endregion
        

        #region TriggerEachEvents

        public virtual void TriggerOnTriggerFirstEnter()
        {
            if (debugMode)
                Debug.Log("onTriggerFirstEnter is triggered on " + gameObject.name);
            onTriggerFirstEnter?.Invoke();
        }

        public virtual void TriggerOnTriggerEnter()
        {
            if (debugMode)
                Debug.Log("onTriggerEnter is triggered on " + gameObject.name);
            onTriggerEnter?.Invoke();
        }

        public virtual void TriggerOnTriggerExit()
        {
            if (debugMode)
                Debug.Log("onTriggerExit is triggered on " + gameObject.name);
            onTriggerExit?.Invoke();
        }

        public virtual void TriggerOnDistanceFirstEnter()
        {
            if (debugMode)
                Debug.Log("onDistanceFirstEnter is triggered on " + gameObject.name);
            onDistanceFirstEnter?.Invoke();
        }

        public virtual void TriggerOnDistanceEnter()
        {
            if (debugMode)
                Debug.Log("onDistanceEnter is triggered on " + gameObject.name);
            onDistanceEnter?.Invoke();
        }

        public virtual void TriggerOnDistanceExit()
        {
            if (debugMode)
                Debug.Log("onDistanceExit is triggered on " + gameObject.name);
            onDistanceExit?.Invoke();
        }

        public virtual void TriggerOnLookAtFirstEnter()
        {
            if (debugMode)
                Debug.Log("onLookAtFirstEnter is triggered on " + gameObject.name);
            onLookAtFirstEnter?.Invoke();
        }

        public virtual void TriggerOnLookAtEnter()
        {
            if (debugMode)
                Debug.Log("onLookAtEnter is triggered on " + gameObject.name);
            onLookAtEnter?.Invoke();
        }

        public virtual void TriggerOnLookAtExit()
        {
            if (debugMode)
                Debug.Log("onLookAtExit is triggered on " + gameObject.name);
            onLookAtDistanceExit?.Invoke();
        }

        public virtual void TriggerOnStart()
        {
            if (debugMode)
                Debug.Log("onStart is triggered on " + gameObject.name);
            onStart?.Invoke();
        }

        public virtual void TriggerOnEnable()
        {
            if (debugMode)
                Debug.Log("onEnable is triggered on " + gameObject.name);
            onEnable?.Invoke();
        }

        public virtual void TriggerOnUpdate()
        {
            if (debugMode)
                Debug.Log("onUpdate is triggered on " + gameObject.name);
            onUpdate?.Invoke();
        }

        public virtual void TriggerOnDisable()
        {
            if (debugMode)
                Debug.Log("onDisable is triggered on " + gameObject.name);
            onDisable?.Invoke();
        }

        #endregion
    }
}