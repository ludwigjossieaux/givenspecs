# GivenSpecs

GivenSpecs is a gherkin compatible testing framework for .NET core, based on xUnit. It can be used to implement BDD testing.

It aims at remaining mostly compatible with Specflow, but offers reporting possibilities that have been removed in the latest versions of Specflow.

## Getting Started

### 1. Install the global tool

    dotnet tool install --global GivenSpecs.CommandLine

### 2. Create project and install nugets

* Create a xUnit test project
* Install the nuget package GivenSpecs

### 3. Edit the project file .csproj

Add the following pre-compile script at the end of the Project section, where \<Namespace> is the namespace where you would like the xunit test cases to be generated. Typically, if your application namespace is "TodoApp.Specs", you may want to use the same namespace here. 

    <Target Name="PrecompileScript" BeforeTargets="BeforeBuild">
        <Exec Command="dotnet givenspecs generate -f $(ProjectDir) -n <Namespace>" />
    </Target>

### 3. Create a feature file

At the root of the test project, create a feature file Apple.feature

    Feature: Apple

    Scenario: An apple addition
        Given I have "10" apples
        When I add "5" apples
        Then I have "15" apples

### 4. Create a steps file

At the root of the test project, create a new class file AppleSteps.cs. In order for the steps to be correctly recognized, you have to add the \[Binding] attribute to the class. You have also to add a constructor so the class can interact with the ScenarioContext.

    [Binding]
    public sealed class AppleSteps
    {
        private ScenarioContext _c;
        public AppleSteps(ScenarioContext context)
        {
            _c = context;
        }
        ...

Within the AppleSteps class, let's add a test initializer method, in order to setup an apple counter, and to setup reporting. The path to the reporting must exist, GivenSpecs will not create it. The file "report.json" will be overwritten if it is already present. 

        [BeforeScenario]
        public void BeforeTest()
        {
            _c.Data["apple-counter"] = 0;
            _c.SetCucumberReportPath(@"c:\temp\apple\report.json");
        }

Let's now implement the steps of the "An apple addition" scenario

        [Given(@"I have ""(.*)"" apples")]
        public void GivenIHaveXApples(int initApples)
        {
            _c.Data["apple-counter"] = initApples;
        }

        [When(@"I add ""(.*)"" apples")]
        public void WhenIAddXApples(int appleToAdd)
        {
            var apples = (int) _c.Data["apple-counter"];
            apples += appleToAdd;
            _c.Data["apple-counter"] = apples;
        }

        [Then(@"I have ""(.*)"" apples")]
        public void ThenIHaveXApples(int expected)
        {
            var apples = (int)_c.Data["apple-counter"];
            Assert.Equal(expected, apples);
        }

If we compile the project, and run the test, it passes 

    -> Given I have "10" apples
    ... ok
    -> When I add "5" apples
    ... ok
    -> Then I have "15" apples
    ... ok

In the reporting folder, a file report.json has been created in Cucumber report JSON format (see https://cucumber.io/docs/cucumber/reporting/) 