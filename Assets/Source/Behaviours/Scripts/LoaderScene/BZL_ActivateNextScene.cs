using FrameLabs.AI.Extension;
using FrameLabs.AI.System;
using Mitchel.Utilities;

namespace FlowState.Scripts
{
    public class BZL_ActivateNextScene : StateExtension
    {
        public override void Begin(StateMachine StateMachine)
        {
            base.Begin(StateMachine);
        }

        public override void OnEnable()
        {
            AddExitCode("On Scene Activate", 0);
        }

        public override int Evaluate()
        {
            SceneLoader.Instance.ActivateQueuedScene();
            return ExitCodes["On Scene Activate"];
        }
    }
}
