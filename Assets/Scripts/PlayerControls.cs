//------------------------------------------------------------------------------
// <auto-generated>
//     This code was auto-generated by com.unity.inputsystem:InputActionCodeGenerator
//     version 1.5.1
//     from Assets/PlayerControls.inputactions
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace GameU
{
    public partial class @PlayerControls: IInputActionCollection2, IDisposable
    {
        public InputActionAsset asset { get; }
        public @PlayerControls()
        {
            asset = InputActionAsset.FromJson(@"{
    ""name"": ""PlayerControls"",
    ""maps"": [
        {
            ""name"": ""gameplay"",
            ""id"": ""c7716d95-0f21-4564-99a1-7ab1f72f4500"",
            ""actions"": [
                {
                    ""name"": ""move"",
                    ""type"": ""Value"",
                    ""id"": ""6606bb6f-1155-4ef3-9084-89244094ea39"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""jump"",
                    ""type"": ""Button"",
                    ""id"": ""496caa51-dc02-471a-a168-0cb1110b4760"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""look"",
                    ""type"": ""Value"",
                    ""id"": ""fd2046c4-288b-424f-9914-04c6b29e553d"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""dash"",
                    ""type"": ""Button"",
                    ""id"": ""b6bf1c3d-8484-4205-b63b-5e03bb44a8b6"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                }
            ],
            ""bindings"": [
                {
                    ""name"": ""direction"",
                    ""id"": ""2403fe2f-5778-4109-8298-5e5757851423"",
                    ""path"": ""2DVector(mode=1)"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""move"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""bf065ab6-9f63-4b14-ac05-dd786e59dbe6"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""DefaultControls"",
                    ""action"": ""move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""d5e15f71-67df-4c63-8f8e-574648c594c5"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""DefaultControls"",
                    ""action"": ""move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""73704745-5d76-4d08-9dec-c0b1b066775e"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""DefaultControls"",
                    ""action"": ""move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""f9fcf655-6eba-45e8-aa61-b96015b21d6e"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""DefaultControls"",
                    ""action"": ""move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""c29543b4-fe06-453d-9fcd-2646d381f9ad"",
                    ""path"": ""<Gamepad>/leftStick"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""DefaultControls"",
                    ""action"": ""move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""1793cfaf-dee8-4b60-b7ed-2d67a002c532"",
                    ""path"": ""<Keyboard>/space"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""DefaultControls"",
                    ""action"": ""jump"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""507060c1-99af-4223-b7cd-4ce3ae1d6606"",
                    ""path"": ""<Gamepad>/buttonSouth"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""DefaultControls"",
                    ""action"": ""jump"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""c4454ae4-2148-4493-9d84-d843993f987b"",
                    ""path"": ""<Gamepad>/rightStick"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""DefaultControls"",
                    ""action"": ""look"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""3c57c951-e5a9-431b-8d6b-298fa16a8656"",
                    ""path"": ""<Keyboard>/q"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""DefaultControls"",
                    ""action"": ""dash"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""e23bbb8f-e89f-4f22-999f-95d0544f52d2"",
                    ""path"": ""<Gamepad>/buttonWest"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""DefaultControls"",
                    ""action"": ""dash"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": [
        {
            ""name"": ""DefaultControls"",
            ""bindingGroup"": ""DefaultControls"",
            ""devices"": []
        }
    ]
}");
            // gameplay
            m_gameplay = asset.FindActionMap("gameplay", throwIfNotFound: true);
            m_gameplay_move = m_gameplay.FindAction("move", throwIfNotFound: true);
            m_gameplay_jump = m_gameplay.FindAction("jump", throwIfNotFound: true);
            m_gameplay_look = m_gameplay.FindAction("look", throwIfNotFound: true);
            m_gameplay_dash = m_gameplay.FindAction("dash", throwIfNotFound: true);
        }

        public void Dispose()
        {
            UnityEngine.Object.Destroy(asset);
        }

        public InputBinding? bindingMask
        {
            get => asset.bindingMask;
            set => asset.bindingMask = value;
        }

        public ReadOnlyArray<InputDevice>? devices
        {
            get => asset.devices;
            set => asset.devices = value;
        }

        public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

        public bool Contains(InputAction action)
        {
            return asset.Contains(action);
        }

        public IEnumerator<InputAction> GetEnumerator()
        {
            return asset.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Enable()
        {
            asset.Enable();
        }

        public void Disable()
        {
            asset.Disable();
        }

        public IEnumerable<InputBinding> bindings => asset.bindings;

        public InputAction FindAction(string actionNameOrId, bool throwIfNotFound = false)
        {
            return asset.FindAction(actionNameOrId, throwIfNotFound);
        }

        public int FindBinding(InputBinding bindingMask, out InputAction action)
        {
            return asset.FindBinding(bindingMask, out action);
        }

        // gameplay
        private readonly InputActionMap m_gameplay;
        private List<IGameplayActions> m_GameplayActionsCallbackInterfaces = new List<IGameplayActions>();
        private readonly InputAction m_gameplay_move;
        private readonly InputAction m_gameplay_jump;
        private readonly InputAction m_gameplay_look;
        private readonly InputAction m_gameplay_dash;
        public struct GameplayActions
        {
            private @PlayerControls m_Wrapper;
            public GameplayActions(@PlayerControls wrapper) { m_Wrapper = wrapper; }
            public InputAction @move => m_Wrapper.m_gameplay_move;
            public InputAction @jump => m_Wrapper.m_gameplay_jump;
            public InputAction @look => m_Wrapper.m_gameplay_look;
            public InputAction @dash => m_Wrapper.m_gameplay_dash;
            public InputActionMap Get() { return m_Wrapper.m_gameplay; }
            public void Enable() { Get().Enable(); }
            public void Disable() { Get().Disable(); }
            public bool enabled => Get().enabled;
            public static implicit operator InputActionMap(GameplayActions set) { return set.Get(); }
            public void AddCallbacks(IGameplayActions instance)
            {
                if (instance == null || m_Wrapper.m_GameplayActionsCallbackInterfaces.Contains(instance)) return;
                m_Wrapper.m_GameplayActionsCallbackInterfaces.Add(instance);
                @move.started += instance.OnMove;
                @move.performed += instance.OnMove;
                @move.canceled += instance.OnMove;
                @jump.started += instance.OnJump;
                @jump.performed += instance.OnJump;
                @jump.canceled += instance.OnJump;
                @look.started += instance.OnLook;
                @look.performed += instance.OnLook;
                @look.canceled += instance.OnLook;
                @dash.started += instance.OnDash;
                @dash.performed += instance.OnDash;
                @dash.canceled += instance.OnDash;
            }

            private void UnregisterCallbacks(IGameplayActions instance)
            {
                @move.started -= instance.OnMove;
                @move.performed -= instance.OnMove;
                @move.canceled -= instance.OnMove;
                @jump.started -= instance.OnJump;
                @jump.performed -= instance.OnJump;
                @jump.canceled -= instance.OnJump;
                @look.started -= instance.OnLook;
                @look.performed -= instance.OnLook;
                @look.canceled -= instance.OnLook;
                @dash.started -= instance.OnDash;
                @dash.performed -= instance.OnDash;
                @dash.canceled -= instance.OnDash;
            }

            public void RemoveCallbacks(IGameplayActions instance)
            {
                if (m_Wrapper.m_GameplayActionsCallbackInterfaces.Remove(instance))
                    UnregisterCallbacks(instance);
            }

            public void SetCallbacks(IGameplayActions instance)
            {
                foreach (var item in m_Wrapper.m_GameplayActionsCallbackInterfaces)
                    UnregisterCallbacks(item);
                m_Wrapper.m_GameplayActionsCallbackInterfaces.Clear();
                AddCallbacks(instance);
            }
        }
        public GameplayActions @gameplay => new GameplayActions(this);
        private int m_DefaultControlsSchemeIndex = -1;
        public InputControlScheme DefaultControlsScheme
        {
            get
            {
                if (m_DefaultControlsSchemeIndex == -1) m_DefaultControlsSchemeIndex = asset.FindControlSchemeIndex("DefaultControls");
                return asset.controlSchemes[m_DefaultControlsSchemeIndex];
            }
        }
        public interface IGameplayActions
        {
            void OnMove(InputAction.CallbackContext context);
            void OnJump(InputAction.CallbackContext context);
            void OnLook(InputAction.CallbackContext context);
            void OnDash(InputAction.CallbackContext context);
        }
    }
}
