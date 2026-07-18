using System;
using FrameLabs.AI.Extension;
using FrameLabs.AI.System;
using UnityEngine;

namespace FlowState.Scripts
{
    public class BZL_SetDateTime : StateExtension
    {
        public override void Begin(StateMachine StateMachine)
        {
            base.Begin(StateMachine);
        }

        public override void OnEnable()
        {
            AddExitCode("On Finish", 0);
        }

        public override int Evaluate()
        {
            DateTime dateTime = DateTime.Now;
            return ExitCodes["On Finish"];
        }
    }
}