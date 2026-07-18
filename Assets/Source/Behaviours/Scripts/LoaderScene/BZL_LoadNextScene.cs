using FlowState.DataParser;
using FrameLabs.AI.Extension;
using FrameLabs.AI.System;
using Mitchel.Utilities;
using TMPro;
using UnityEngine;

namespace FlowState.Scripts
{
    public class BZL_LoadNextScene : StateExtension
    {
        public int SceneIndex;
        private LoaderSceneDataParser dataParser;
        private float timeElapsed;
        private readonly float fakeDelay = 0.5f;

        public override void Begin(StateMachine StateMachine)
        {
            base.Begin(StateMachine);
            dataParser = LoaderSceneDataParser.Instance;
            timeElapsed = 0;

            SceneLoader.Instance.BeginAsyncLoad(SceneIndex, false);

            dataParser.loadingStatusTextGroup.SetActive(true);
            SetText("");
        }

        public override void OnEnable()
        {
            AddExitCode("Return", -1);
            AddExitCode("On Scene Ready", 0);
        }

        public override int Evaluate()
        {
            while (SceneLoader.Instance.SceneLoaderProgress != 0.9f)
                return ExitCodes["Return"];

            while (timeElapsed < fakeDelay)
            {
                timeElapsed += Time.deltaTime;
                return ExitCodes["Return"];
            }

            return ExitCodes["On Scene Ready"];
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