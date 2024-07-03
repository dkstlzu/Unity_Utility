using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace dkstlzu.Utility
{
    public static class InputSystemExtensions
    {
        private static HashSet<Guid> _set = new HashSet<Guid>();
        
        public static void EnableAlone(this InputActionAsset inputAsset)
        {
            inputAsset.Enable();
            
            foreach (InputActionMap actionMap in inputAsset.actionMaps)
            {
                if (_set.Contains(actionMap.id))
                {
                    actionMap.Disable();
                    _set.Remove(actionMap.id);
                    continue;
                }
                
                foreach (InputAction action in actionMap.actions)
                {
                    if (_set.Contains(action.id))
                    {
                        action.Disable();
                        _set.Remove(action.id);
                    }
                }
            }
        }
    
        public static void DisableAlone(this InputActionAsset inputAsset)
        {
            foreach (InputActionMap actionMap in inputAsset.actionMaps)
            {
                if (!actionMap.enabled)
                {
                    _set.Add(actionMap.id);
                    continue;
                }
                
                foreach (InputAction action in actionMap.actions)
                {
                    if (!action.enabled)
                    {
                        _set.Add(action.id);
                    }
                }
            }
            
            inputAsset.Disable();
        }
    }
}
