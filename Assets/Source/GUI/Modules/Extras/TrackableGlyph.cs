using UnityEngine.UI;

namespace BZApp.GUI.Modules
{
    public class TrackableGlyph
    {
        public Image ImageToTrack { get; private set; }
        public string ActionMapName { get; private set; }
        public string ActionName { get; private set; }

        public TrackableGlyph(Image imageToTrack, string actionMapName, string actionName)
        {
            ImageToTrack = imageToTrack;
            ActionMapName = actionMapName;
            ActionName = actionName;

            if (!GuiSystem.Instance || !imageToTrack) return;
            imageToTrack.sprite = GuiSystem.Instance.Get<GlyphsManager>().GetGlyph(actionMapName, actionName);
        }
    }
}
