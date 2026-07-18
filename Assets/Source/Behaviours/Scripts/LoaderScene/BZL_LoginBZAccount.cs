using FlowState.DataParser;
using FrameLabs.AI.Extension;
using FrameLabs.AI.System;
using TMPro;
using UnityEngine;

namespace FlowState.Scripts
{
    public class BZL_LoginBZAccount : StateExtension
    {
        private float fakeTimer = 8f;
        private LoaderSceneDataParser dataParser;
        private float timeElapsed;
        private int stage;

        public override void Begin(StateMachine StateMachine)
        {
            base.Begin(StateMachine);
            dataParser = LoaderSceneDataParser.Instance;
            timeElapsed = 0;
        }

        public override void OnEnable()
        {
            AddExitCode("Return", -1);
            AddExitCode("Logged In", 0);
        }

        public override int Evaluate()
        {
            while (timeElapsed < fakeTimer)
            {
                if (timeElapsed >= 0 && stage == 0)
                {
                    SetText("Reading credentials...");
                    stage++;
                }
                else if (timeElapsed >= 0.1f && stage == 1)
                {
                    SetText("Logging in...");
                    stage++;
                }
                else if (timeElapsed >= 3f && stage == 2)
                {
                    SetText("Downloading account data...");
                    stage++;
                }

                timeElapsed += Time.deltaTime;
                return ExitCodes["Return"];
            }

            SetText("");
            return ExitCodes["Logged In"];
        }

        private void SetText(string value)
        {
            foreach (TextMeshProUGUI loadingText in dataParser.loadingStatusTextGroup
                         .GetComponentsInChildren<TextMeshProUGUI>())
            {
                loadingText.text = value;
            }
        }
    }
}