using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace FrameLabs.Utilities.NodeEditor
{
    public enum MessageCategory
    {
        Info,
        Warning,
        Error,
        Success
    }

    public class EditorMessageBox : VisualElement
    {
        private struct QueuedMessage
        {
            public string Text;
            public float Duration;
            public MessageCategory Category;
        }

        private readonly List<VisualElement> activeToasts = new();
        private readonly Queue<QueuedMessage> messageQueue = new();

        public EditorMessageBox()
        {
            name = "editor-message-box";
            LoadMessageBoxStyle();

            RegisterCallback<AttachToPanelEvent>(_ =>
            {
                schedule.Execute(ProcessQueue).Every(300);
            });
        }

        public void ShowMessage(string msg, float duration = 3f, MessageCategory category = MessageCategory.Info)
        {
            messageQueue.Enqueue(new QueuedMessage
            {
                Text = msg,
                Duration = duration,
                Category = category
            });
        }

        private void ProcessQueue()
        {
            if (messageQueue.Count == 0 || activeToasts.Count >= 3) return;

            var next = messageQueue.Dequeue();
            var toast = CreateToast(next.Text, next.Duration, next.Category);
            Add(toast);
            activeToasts.Add(toast);
        }

        private VisualElement CreateToast(string text, float duration, MessageCategory category)
        {
            var toast = new VisualElement();
            toast.AddToClassList("message-toast");
            toast.AddToClassList(category.ToString().ToLower());

            var label = new Label(text);
            label.AddToClassList("message-text");
            toast.Add(label);

            toast.style.opacity = 0;

            // Fade in manually
            schedule.Execute(() =>
            {
                toast.style.opacity = 1;
            }).ExecuteLater(50); // small delay to allow rendering

            // Fade out manually
            schedule.Execute(() =>
            {
                toast.style.opacity = 0;

                schedule.Execute(() =>
                {
                    Remove(toast);
                    activeToasts.Remove(toast);
                }).ExecuteLater(300); // allow fade-out time
            }).ExecuteLater((int)(duration * 1000));

            return toast;
        }


        private void LoadMessageBoxStyle()
        {
            string basePath = Application.dataPath;
            string filePath = Utility.Instance.FindFileInPath(basePath, "EditorMessageBox.uss");

            if (!string.IsNullOrEmpty(filePath))
            {
                var styleSheet = UnityEditor.AssetDatabase.LoadAssetAtPath<StyleSheet>(filePath);
                if (styleSheet != null)
                {
                    styleSheets.Add(styleSheet);
                }
            }
        }

    }
}
