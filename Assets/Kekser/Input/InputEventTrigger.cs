using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Kekser.Input
{
    public class InputEventTrigger : MonoBehaviour
    {
        [SerializeField] 
        private InputHolder _input;
        [SerializeField]
        private UnityEvent _event;

        private void OnEnable()
        {
            _input
                .Enable()
                .Performed(OnAction);
        }

        private void OnDisable()
        {
            _input
                .RemovePerformed();
        }
        
        private void OnAction(InputAction.CallbackContext context)
        {
            if (!context.performed || !gameObject.activeInHierarchy)
                return;

            _event?.Invoke();
        }
    }
}