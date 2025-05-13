using UnityEngine;
using UnityEngine.UIElements;

namespace FrameLabs.Utilities.NodeEditor.Debugging
{
    public class EditorInputDebugger : VisualElement
    {
        private Label debugLabel;

        public EditorInputDebugger()
        {
            name = "input-debugger";
            style.position = Position.Absolute;
            style.bottom = 10;
            style.left = 10;
            style.paddingLeft = 6;
            style.paddingRight = 6;
            style.paddingTop = 3;
            style.paddingBottom = 3;
            style.backgroundColor = new Color(0, 0, 0, 0.65f);
            style.color = Color.white;
            style.unityFontStyleAndWeight = FontStyle.Bold;
            style.borderTopLeftRadius = 4;
            style.borderTopRightRadius = 4;
            style.borderBottomLeftRadius = 4;
            style.borderBottomRightRadius = 4;
            style.borderBottomWidth = 1;
            style.borderTopWidth = 1;
            style.borderLeftWidth = 1;
            style.borderRightWidth = 1;
            style.backgroundColor = Color.gray;

            debugLabel = new Label("Input Debugger Ready");
            Add(debugLabel);
        }

        public void SetMessage(string msg)
        {
            debugLabel.text = msg;
        }
    }
}
