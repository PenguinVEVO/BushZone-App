using FrameLabs.AI.Extension;
using FrameLabs.AI.System;
using Mitchel.DataParser;
using UnityEngine;

namespace FlowState.Scripts
{
    public class BZO_C_FadeIn : StateExtension
    {
        public float fadeTime;
        private float timeElapsed;
        private Color oldColour = Color.white;
        private Color newColour = Color.white;

        private BZO_StateMachineDataParser dataParser;
    
        public override void Begin(StateMachine StateMachine)
        {
            base.Begin(StateMachine);
            dataParser = BZO_StateMachineDataParser.Instance;
        
            timeElapsed = 0;
            newColour.a = 0;
            dataParser.fadeImage.enabled = true;
        }

        public override void OnEnable()
        {
            AddExitCode("Return", -1);
            AddExitCode("On Finish", 0);
        }

        public override int Evaluate()
        {
            while (timeElapsed < fadeTime)
            {
                dataParser.fadeImage.color = Color.Lerp(oldColour, newColour, timeElapsed / fadeTime);
                timeElapsed += Time.deltaTime;
                return ExitCodes["Return"];
            }
            dataParser.fadeImage.color = newColour;
            return ExitCodes["On Finish"];
        }
    }
}