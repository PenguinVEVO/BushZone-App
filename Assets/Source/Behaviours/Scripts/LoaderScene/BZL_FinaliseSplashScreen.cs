using FlowState.DataParser;
using FrameLabs.AI.Extension;
using FrameLabs.AI.System;
using Mitchel.DataParser;
using UnityEngine;

namespace FlowState.Scripts
{
    public class BZL_FinaliseSplashScreen : StateExtension
    {
        private LoaderSceneDataParser dataParser;

        public float fadeTime;
        private float timeElapsed;
        private Color opaque;
        private Color transparent;

        public override void Begin(StateMachine StateMachine)
        {
            base.Begin(StateMachine);
            dataParser = LoaderSceneDataParser.Instance;
            timeElapsed = 0;

            opaque = dataParser.whiteImage.color;
            opaque.a = 1;
            transparent = dataParser.whiteImage.color;
            transparent.a = 0;
            dataParser.whiteImage.enabled = true;
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
                dataParser.whiteImage.color = Color.Lerp(transparent, opaque, timeElapsed / fadeTime);
                timeElapsed += Time.deltaTime;
                return ExitCodes["Return"];
            }

            dataParser.whiteImage.color = opaque;
            return ExitCodes["On Finish"];
        }
    }
}
