using System;
using System.Collections.Generic;
using FrameLabs.AI.Extension;
using FrameLabs.AI.System;
using Mitchel.DataParser;
using UnityEngine;

public class BZO_C_FakeLoad : StateExtension
{
    public List<FakeTextValues> fakeTextValues;
    private int currentIteration;
    private float timeElapsed;
    
    private BZO_StateMachineDataParser dataParser;
    
    public override void Begin(StateMachine StateMachine)
    {
        base.Begin(StateMachine);
        dataParser = BZO_StateMachineDataParser.Instance;
        currentIteration = 0;
        timeElapsed = 0;
    }

    public override void OnEnable()
    {
        AddExitCode("Return", -1);
        AddExitCode("On Finish", 0);
    }

    public override int Evaluate()
    {
        while (currentIteration < fakeTextValues.Count)
        {
            dataParser.loadingText.text = fakeTextValues[currentIteration].text;
            while (timeElapsed < fakeTextValues[currentIteration].stayTime)
            {
                timeElapsed += Time.deltaTime;
                return ExitCodes["Return"];
            }
            currentIteration++;
            timeElapsed = 0;
            return ExitCodes["Return"];
        }

        dataParser.loadingText.text = "";
        return ExitCodes["On Finish"];
    }

    [Serializable]
    public class FakeTextValues
    {
        public string text;
        public float stayTime;
    }
}