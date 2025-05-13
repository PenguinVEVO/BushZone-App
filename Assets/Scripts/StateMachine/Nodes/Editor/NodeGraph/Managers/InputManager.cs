using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace FrameLabs.Utilities.NodeEditor
{
    public class EventHandler
    {
        public virtual void MouseMove(MouseMoveEvent evt) { }
        public virtual void MouseUp(MouseUpEvent evt) { }
        public virtual void MouseDown(MouseDownEvent evt)  { }
        public virtual void MouseEnter (MouseEnterEvent evt) { }
        public virtual void MouseLeave( MouseLeaveEvent evt) { }
        public virtual void MouseOver(MouseOverEvent evt) { }         
        public virtual void PointerEnter(PointerEnterEvent evt) { }        
        public virtual void PointerLeave(PointerLeaveEvent evt) { }        
        public virtual void Focus(FocusEvent evt) { }                      
        public virtual void Blur(BlurEvent evt) { }                        
        public virtual void KeyDown(KeyDownEvent evt) { }
        public virtual void KeyUp(KeyUpEvent evt) { }
    }

    public class InputManager
    {
        private static readonly InputManager instance = new InputManager();
        public static InputManager Instance => instance;

        private readonly Dictionary<VisualElement, EventHandler> eventHandlers = new();

        private InputManager() { }

        /// <summary>
        /// Registers a VisualElement with a new event handler.
        /// </summary>
        public void RegisterElement(VisualElement element)
        {
            if (element == null || eventHandlers.ContainsKey(element))
            {
                Debug.LogWarning("[InputManager] Attempted to register a null or existing element.");
                return;
            }

            eventHandlers[element] = null;
            RegisterCallbacks(element);
        }

        /// <summary>
        /// Unregisters an element, removing its event handlers.
        /// </summary>
        public void UnregisterElement(VisualElement element)
        {
            if (element == null || !eventHandlers.ContainsKey(element))
                return;

            UnregisterCallbacks(element);

            eventHandlers.Remove(element);
        }

        /// <summary>
        /// Checks if a VisualElement already has an event handler registered.
        /// </summary>
        public bool HasEventHandler(VisualElement element)
        {
            return element != null && eventHandlers.ContainsKey(element);
        }

        /// <summary>
        /// Sets an element's event handler dynamically.
        /// </summary>
        public void SetEventHandler(VisualElement element, EventHandler handler)
        {
            if (element == null) return;

            if (!eventHandlers.ContainsKey(element))
            {
                RegisterElement(element);
            }

            eventHandlers[element] = handler;
        }

        /// <summary>
        /// Registers event callbacks for an element.
        /// </summary>
        private void RegisterCallbacks(VisualElement element)
        {
            element.RegisterCallback<MouseMoveEvent>(DispatchMouseMove);
            element.RegisterCallback<MouseDownEvent>(DispatchMouseDown);
            element.RegisterCallback<MouseEnterEvent>(DispatchMouseEnter);
            element.RegisterCallback<MouseLeaveEvent>(DispatchMouseLeave);
            element.RegisterCallback<MouseOverEvent>(DispatchMouseOver);               
            element.RegisterCallback<PointerEnterEvent>(DispatchPointerEnter);         
            element.RegisterCallback<PointerLeaveEvent>(DispatchPointerLeave);         
            element.RegisterCallback<FocusEvent>(DispatchFocus);                       
            element.RegisterCallback<BlurEvent>(DispatchBlur);                         
            element.RegisterCallback<MouseUpEvent>(DispatchMouseUp);
            element.RegisterCallback<KeyDownEvent>(DispatchKeyDown);
            element.RegisterCallback<KeyUpEvent>(DispatchKeyUp);
        }

        /// <summary>
        /// Unregisters event callbacks for an element.
        /// </summary>
        private void UnregisterCallbacks(VisualElement element)
        {
            element.UnregisterCallback<MouseMoveEvent>(DispatchMouseMove);
            element.UnregisterCallback<MouseDownEvent>(DispatchMouseDown);
            element.UnregisterCallback<MouseEnterEvent>(DispatchMouseEnter);
            element.UnregisterCallback<MouseLeaveEvent>(DispatchMouseLeave);
            element.UnregisterCallback<MouseOverEvent>(DispatchMouseOver);             
            element.UnregisterCallback<PointerEnterEvent>(DispatchPointerEnter);       
            element.UnregisterCallback<PointerLeaveEvent>(DispatchPointerLeave);       
            element.UnregisterCallback<FocusEvent>(DispatchFocus);                     
            element.UnregisterCallback<BlurEvent>(DispatchBlur);                       
            element.UnregisterCallback<MouseUpEvent>(DispatchMouseUp);
            element.UnregisterCallback<KeyDownEvent>(DispatchKeyDown);
            element.UnregisterCallback<KeyUpEvent>(DispatchKeyUp);
        }

        private void DispatchKeyDown(KeyDownEvent evt) => DispatchEvent(evt.target, evt, (h, e) => h.KeyDown(e));
        private void DispatchKeyUp(KeyUpEvent evt) => DispatchEvent(evt.target, evt, (h, e) => h.KeyUp(e));
        private void DispatchMouseMove(MouseMoveEvent evt) => DispatchEvent(evt.target, evt, (h, e) => h.MouseMove(e));
        private void DispatchMouseDown(MouseDownEvent evt) => DispatchEvent(evt.target, evt, (h, e) => h.MouseDown(e));
        private void DispatchMouseUp(MouseUpEvent evt) => DispatchEvent(evt.target, evt, (h, e) => h.MouseUp(e));
        private void DispatchMouseEnter(MouseEnterEvent evt) => DispatchEvent(evt.target, evt, (h, e) => h.MouseEnter(e));
        private void DispatchMouseLeave(MouseLeaveEvent evt) => DispatchEvent(evt.target, evt, (h, e) => h.MouseLeave(e));
        private void DispatchMouseOver(MouseOverEvent evt) => DispatchEvent(evt.target, evt, (h, e) => h.MouseOver(e));
        private void DispatchPointerEnter(PointerEnterEvent evt) => DispatchEvent(evt.target, evt, (h, e) => h.PointerEnter(e));
        private void DispatchPointerLeave(PointerLeaveEvent evt) => DispatchEvent(evt.target, evt, (h, e) => h.PointerLeave(e));
        private void DispatchFocus(FocusEvent evt) => DispatchEvent(evt.target, evt, (h, e) => h.Focus(e));
        private void DispatchBlur(BlurEvent evt) => DispatchEvent(evt.target, evt, (h, e) => h.Blur(e));




        /// <summary>
        /// Generic method to dispatch an event to the appropriate handler.
        /// </summary>
        private void DispatchEvent<T>(IEventHandler target,T evt, Action<EventHandler, T> action, 
            bool allowBubble = true, int maxDepth = 10 ) where T : EventBase<T>, new()

        {
            if (target is not VisualElement element)
                return;

            int depth = 0;

            while (element != null && depth < maxDepth)
            {
                if (eventHandlers.TryGetValue(element, out var handler) && handler != null)
                {
                    action(handler, evt);
                    return;
                }

                element = element.parent;
                depth++;
            }

            Debug.LogWarning($"[InputManager] No handler found for {typeof(T).Name} (bubbled up {depth} levels) from {((VisualElement)target).name}");
        }

    }
}