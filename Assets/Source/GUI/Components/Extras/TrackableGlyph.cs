using UnityEngine.UI;

namespace BZApp.GUI.Components
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
        }
    }
}
