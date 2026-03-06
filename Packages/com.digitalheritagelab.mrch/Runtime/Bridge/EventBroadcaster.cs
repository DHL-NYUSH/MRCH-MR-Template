// Copyright (c) 2026 Digital Heritage Lab / Shengyang "Billiton" Peng
// Licensed under the MIT License. See LICENSE for details.

using System;
using Immersal;
using Immersal.XR;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MRCH.Bridge
{
    [AddComponentMenu("")]
    public abstract class EventBroadcaster : MonoBehaviour
    {
        [Title("On Immersal SDK")]
        public static event Action OnInitialized;
        
        public static event Action OnReset;

        [Title("On Localizer")]
        public static event Action OnFirstLocalized;
        
        public static event Action OnSuccessfulLocalizations;
        
        public static event Action OnFailedLocalizations;

        /// <summary>
        /// Whether OnFirstLocalized has been fired at least once.
        /// Useful for late subscribers to know if they missed the event.
        /// </summary>
        public static bool HasLocalized { get; private set; }

        private static ImmersalSDK SDK
        {
            get
            {
                if (ImmersalSDK.Instance) return ImmersalSDK.Instance;
                
                Debug.LogError("ImmersalSDK is not initialized");
                return null;
            }
        }
        
        [SerializeField, Required]
        private Localizer localizer;


        #region Broadcast Methods

        protected virtual void Start()
        {
            SDK.OnInitializationComplete.AddListener(InitializedBroadcaster);
            SDK.OnReset.AddListener(ResetBroadcaster);

            if (!localizer)
            {
                localizer = FindAnyObjectByType<Localizer>();
                if(!localizer)
                {
                    Debug.LogError($"Could not find {nameof(Localizer)} on {gameObject.name}");
                    return;
                }
            }
            
            localizer.OnFirstSuccessfulLocalization.AddListener(FirstLocalizedBroadcaster);
            localizer.OnSuccessfulLocalizations.AddListener(SuccessfulLocalizationsBroadcaster);
            localizer.OnFailedLocalizations.AddListener(FailedLocalizationsBroadcaster);
        }

        protected virtual void OnDestroy()
        {
            SDK.OnInitializationComplete.RemoveListener(InitializedBroadcaster);
            SDK.OnReset.RemoveListener(ResetBroadcaster);
            
            if (!localizer)
                return;
            
            localizer.OnFirstSuccessfulLocalization.RemoveListener(FirstLocalizedBroadcaster);
            localizer.OnSuccessfulLocalizations.RemoveListener(SuccessfulLocalizationsBroadcaster);
            localizer.OnFailedLocalizations.RemoveListener(FailedLocalizationsBroadcaster);
        }

        public virtual void InitializedBroadcaster()
        {
            OnInitialized?.Invoke();
        }

        public virtual void ResetBroadcaster()
        {
            HasLocalized = false;
            OnReset?.Invoke();
        }

        public virtual void FirstLocalizedBroadcaster()
        {
            HasLocalized = true;
            OnFirstLocalized?.Invoke();
        }

        public virtual void SuccessfulLocalizationsBroadcaster(int[] _)
        {
            OnSuccessfulLocalizations?.Invoke();
        }

        public virtual void FailedLocalizationsBroadcaster()
        {
            OnFailedLocalizations?.Invoke();
        }

        #endregion
    }
}