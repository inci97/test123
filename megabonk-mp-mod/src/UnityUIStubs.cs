using UnityEngine;

// Lightweight stubs for UnityEngine.UI types so the project can compile
// when the real Unity UI assemblies aren't available (fallback builds).
// These provide only the minimal members used by the mod code.
namespace UnityEngine.UI
{
    public class Canvas : Component
    {
        public RenderMode renderMode;
        public int sortingOrder;
    }

    public class CanvasScaler : Component { }

    public class Text : Component
    {
        public string text;
        public Font font;
        public int fontSize;
        public Color color;
        public TextAnchor alignment;
    }

    public class Image : Component
    {
        public Color color;
    }

    public class InputField : Component
    {
        public string text;
    }

    public class Button : Component
    {
        public Text text;
        public ButtonClickedEvent onClick = new ButtonClickedEvent();
        public class ButtonClickedEvent : UnityEngine.Events.UnityEvent { }
    }

    public class Toggle : Component
    {
        public bool isOn;
        public UnityEngine.Events.UnityEvent onValueChanged = new UnityEngine.Events.UnityEvent();
    }

    public class Slider : Component
    {
        public float value;
    }
}

// Simple fallback for Unity events if not present in the referenced Unity stubs.
namespace UnityEngine.Events
{
    public class UnityEvent
    {
        public void Invoke() { }
        public void AddListener(System.Action d) { }
    }
}
