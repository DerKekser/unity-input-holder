﻿using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Kekser.Input
{
    [Serializable]
    public class InputHolder
    {
        [SerializeField]
        public InputActionAsset _inputAsset;
        [SerializeField]
        public string _actionName;
        [SerializeField]
        public string _actionId;
        
        private string _lastActionName;
        private InputAction _action;
        
        private event Action<InputAction.CallbackContext> _performed;
        private event Action<InputAction.CallbackContext> _canceled;
        private event Action<InputAction.CallbackContext> _started;

        private InputAction Action
        {
            get
            {
                if (_inputAsset == null)
                    return null;
                
                if (_action != null && _lastActionName == _actionName)
                    return _action;

                if (_action != null)
                {
                    _action.performed -= OnPerformed;
                    _action.canceled -= OnCanceled;
                    _action.started -= OnStarted;
                }

                InputAction found = _actionId != null ? _inputAsset.FindAction(_actionId) : null;
                _lastActionName = _actionName;
                foreach (InputActionMap actionMap in _inputAsset.actionMaps)
                {
                    foreach (InputAction action in actionMap.actions)
                    {
                        string actionName = $"{actionMap.name}/{action.name}";
                        if (found == null || actionName == _actionName)
                        {
                            found = action;
                            _lastActionName = _actionName;
                        }
                    }
                }
                
                _action = found;
                if (_action != null)
                {
                    _action.performed += OnPerformed;
                    _action.canceled += OnCanceled;
                    _action.started += OnStarted;
                }
                return found;
            }
        }
        
        public InputHolder Enable()
        {
            if (!Application.isPlaying)
                return this;
            Action?.Enable();
            return this;
        }
        
        public InputHolder Disable()
        {
            if (!Application.isPlaying)
                return this;
            Action?.Disable();
            return this;
        }
        
        public InputHolder Enable(string otherActionMap)
        {
            if (!Application.isPlaying)
                return this;
            _inputAsset.FindActionMap(otherActionMap)?.Enable();
            return this;
        }
        
        public InputHolder Disable(string otherActionMap)
        {
            if (!Application.isPlaying)
                return this;
            _inputAsset.FindActionMap(otherActionMap)?.Disable();
            return this;
        }
        
        public InputHolder EnableAll()
        {
            if (!Application.isPlaying)
                return this;
            foreach (InputActionMap actionMap in _inputAsset.actionMaps)
            {
                actionMap.Enable();
            }
            return this;
        }
        
        public InputHolder DisableAll()
        {
            if (!Application.isPlaying)
                return this;
            foreach (InputActionMap actionMap in _inputAsset.actionMaps)
            {
                actionMap.Disable();
            }
            return this;
        }

        public InputHolder Performed(Action<InputAction.CallbackContext> callback)
        {
            if (!Application.isPlaying)
                return this;
            _performed = callback;
            return this;
        }
        
        public InputHolder Canceled(Action<InputAction.CallbackContext> callback)
        {
            if (!Application.isPlaying)
                return this;
            _canceled = callback;
            return this;
        }
        
        public InputHolder PerformedCanceled(Action<InputAction.CallbackContext> callback)
        {
            if (!Application.isPlaying)
                return this;
            Performed(callback);
            Canceled(callback);
            return this;
        }
        
        public InputHolder Started(Action<InputAction.CallbackContext> callback)
        {
            if (!Application.isPlaying)
                return this;
            _started = callback;
            return this;
        }
        
        public InputHolder RemovePerformed()
        {
            if (!Application.isPlaying)
                return this;
            _performed = null;
            return this;
        }
        
        public InputHolder RemoveCanceled()
        {
            if (!Application.isPlaying)
                return this;
            _canceled = null;
            return this;
        }
        
        public InputHolder RemovePerformedCanceled()
        {
            if (!Application.isPlaying)
                return this;
            RemovePerformed();
            RemoveCanceled();
            return this;
        }
        
        public InputHolder RemoveStarted()
        {
            if (!Application.isPlaying)
                return this;
            _started = null;
            return this;
        }
        
        public InputHolder RegisterStarted(Action<InputAction.CallbackContext> callback)
        {
            if (!Application.isPlaying)
                return this;
            _started += callback;
            return this;
        }
        
        public InputHolder RegisterPerformed(Action<InputAction.CallbackContext> callback)
        {
            if (!Application.isPlaying)
                return this;
            _performed += callback;
            return this;
        }
        
        public InputHolder RegisterCanceled(Action<InputAction.CallbackContext> callback)
        {
            if (!Application.isPlaying)
                return this;
            _canceled += callback;
            return this;
        }
        
        public InputHolder RegisterPerformedCanceled(Action<InputAction.CallbackContext> callback)
        {
            if (!Application.isPlaying)
                return this;
            RegisterPerformed(callback);
            RegisterCanceled(callback);
            return this;
        }
        
        public InputHolder UnregisterStarted(Action<InputAction.CallbackContext> callback)
        {
            if (!Application.isPlaying)
                return this;
            _started -= callback;
            return this;
        }
        
        public InputHolder UnregisterPerformed(Action<InputAction.CallbackContext> callback)
        {
            if (!Application.isPlaying)
                return this;
            _performed -= callback;
            return this;
        }
        
        public InputHolder UnregisterCanceled(Action<InputAction.CallbackContext> callback)
        {
            if (!Application.isPlaying)
                return this;
            _canceled -= callback;
            return this;
        }
        
        public InputHolder UnregisterPerformedCanceled(Action<InputAction.CallbackContext> callback)
        {
            if (!Application.isPlaying)
                return this;
            UnregisterPerformed(callback);
            UnregisterCanceled(callback);
            return this;
        }
        
#if UNITY_2021_3_OR_NEWER
        public bool GetButton()
        {
            return Action?.IsPressed() ?? false;
        }
        
        public bool GetButtonDown()
        {
            return Action?.WasPressedThisFrame() ?? false;
        }
        
        public bool GetButtonUp()
        {
            return Action?.WasReleasedThisFrame() ?? false;
        }
#endif
        
        public T ReadValue<T>() where T : struct
        {
            return Action?.ReadValue<T>() ?? default;
        }
        
        public InputActionRebindingExtensions.RebindingOperation Rebind(int bindingIndex = -1, string withControlsExcluding = "Mouse")
        {
            DisableAll();
            return Action?.PerformInteractiveRebinding(bindingIndex)
                .WithControlsExcluding(withControlsExcluding)
                .WithControlsExcluding("Mouse")
                .WithControlsExcluding("<Keyboard>/escape")
                .WithCancelingThrough("<Keyboard>/escape")
                .OnMatchWaitForAnother(0.1f)
                .Start()
                .OnComplete(operation =>
                {
                    operation.Dispose();
                    EnableAll();
                })
                .OnCancel(operation =>
                {
                    operation.Dispose();
                    EnableAll();
                });
        }
        
        public async Task<InputActionRebindingExtensions.RebindingOperation> RebindAsync(int bindingIndex = -1, string withControlsExcluding = "Mouse")
        {
            InputActionRebindingExtensions.RebindingOperation completed = null;
            DisableAll();
            Action?.PerformInteractiveRebinding(bindingIndex)
                .WithControlsExcluding(withControlsExcluding)
                .WithControlsExcluding("Mouse")
                .WithControlsExcluding("<Keyboard>/escape")
                .WithCancelingThrough("<Keyboard>/escape")
                .OnMatchWaitForAnother(0.1f)
                .Start()
                .OnComplete(operation => completed = operation)
                .OnCancel(operation => completed = operation);
            
            while (completed == null)
                await Task.Yield();
            completed.Dispose();
            EnableAll();
            return completed;
        }

        public string GetDisplayString(int bindingIndex = -1)
        {
            return Action?.GetBindingDisplayString(bindingIndex);
        }
        
        private void OnPerformed(InputAction.CallbackContext context)
        {
            _performed?.Invoke(context);
        }
        
        private void OnCanceled(InputAction.CallbackContext context)
        {
            _canceled?.Invoke(context);
        }
        
        private void OnStarted(InputAction.CallbackContext context)
        {
            _started?.Invoke(context);
        }
    }
}