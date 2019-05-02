using System;
using System.Collections.Generic;
using System.Text;

namespace GivenSpecs
{
    public enum HookType
    {
        BeforeTestRun,
        AfterTestRun,
        BeforeFeature,
        AfterFeature,
        BeforeScenario,
        AfterScenario,
        BeforeScenarioBlock,
        AfterScenarioBlock,
        BeforeStep,
        AfterStep
    }
}
