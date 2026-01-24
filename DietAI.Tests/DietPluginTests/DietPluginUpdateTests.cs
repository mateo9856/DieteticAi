using DieteticAi.Models;
using DieteticAi.Plugins;
using DieteticAi.Tools.Wrappers;
using FluentAssertions;
using Microsoft.SemanticKernel;
using Newtonsoft.Json;
using NSubstitute;
using static DietAI.Tests.DietPluginTests.DietPluginSeeds;

namespace DietAI.Tests.DietPluginTests;

public class TestableDietPlugin : DietPlugin
{
    public TestableDietPlugin(IList<Diets> diets, IKernelWrapper kernel) : base(diets, kernel)
    {
    }

    public new Diets UpdatePlanForPrompt(int age, decimal actualWeight, decimal actualHeight, SexEnum sex, DietType dietType, decimal caloricDemand, decimal previousWeight, decimal previousHeight, decimal previousCaloricDemand)
    {
        return base.UpdatePlanForPrompt(1, age, actualWeight, actualHeight, sex, dietType, caloricDemand, previousWeight, previousHeight, previousCaloricDemand);
    }
}

[TestFixture]
public class DietPluginUpdateTests
{
    private TestableDietPlugin _dietPlugin = null!;
    private List<Diets> _mockDiets = null!;
    private IKernelWrapper _mockKernel = null!;

    [SetUp]
    public void Setup()
    {
        _mockDiets = LoadDietData();
        _mockKernel = Substitute.For<IKernelWrapper>();
        _dietPlugin = new TestableDietPlugin(_mockDiets, _mockKernel);
    }

    [Test]
    public void UpdatePlanForPrompt_ReturnsUpdatedDiet_WhenKernelReturnsValidJson()
    {
        // Arrange
        var expectedDiet = new Diets
        {
            Id = 1,
            DietName = "Updated Weight Loss Plan",
            Description = "Updated plan based on weight change from 80kg to 75kg",
            Age = 30,
            ForWeight = 75m,
            ForHeight = 175m,
            CaloricValue = 2200m,
            ForSex = SexEnum.Male,
            DietType = DietType.Keto
        };

        var parsedData = JsonConvert.SerializeObject(expectedDiet);
        _mockDiets.Clear();
        _mockDiets.Count.Returns(0);
        // Mock the kernel function creation and invocation
        _mockKernel.InvokePromptAsync(Arg.Any<string>(), Arg.Any<KernelArguments>())
            .Returns(ValueTask.FromResult<object?>(parsedData));

        // Act
        var result = _dietPlugin.UpdatePlanForPrompt(
            age: 30,
            actualWeight: 75m,
            actualHeight: 175m,
            sex: SexEnum.Male,
            dietType: DietType.Keto,
            caloricDemand: 2200m,
            previousWeight: 80m,
            previousHeight: 175m,
            previousCaloricDemand: 2400m
        );

        // Assert
        result.Should().NotBeNull();
        result.DietName.Should().Be("Updated Weight Loss Plan");
        result.Description.Should().Be("Updated plan based on weight change from 80kg to 75kg");
        result.Age.Should().Be(30);
        result.ForWeight.Should().Be(75m);
        result.ForHeight.Should().Be(175m);
        result.CaloricValue.Should().Be(2200m);
        result.ForSex.Should().Be(SexEnum.Male);
        result.DietType.Should().Be(DietType.Keto);
    }

    [Test]
    public void UpdatePlanForPrompt_ThrowsException_WhenKernelReturnsEmptyResponse()
    {
        _mockKernel.InvokePromptAsync(Arg.Any<string>(), Arg.Any<KernelArguments>())
            .Returns(ValueTask.FromResult<object?>(string.Empty));

        // Act & Assert
        var exception = Assert.Throws<Exception>(() =>
            _dietPlugin.UpdatePlanForPrompt(
                age: 25,
                actualWeight: 70m,
                actualHeight: 170m,
                sex: SexEnum.Female,
                dietType: DietType.HighProtein,
                caloricDemand: 2000m,
                previousWeight: 75m,
                previousHeight: 170m,
                previousCaloricDemand: 2200m
            ));

        exception!.Message.Should().Be("Model returned empty response");
    }

    [Test]
    public void UpdatePlanForPrompt_ThrowsException_WhenKernelReturnsInvalidJson()
    {
        // Arrange
        _mockKernel.InvokePromptAsync(Arg.Any<string>(), Arg.Any<KernelArguments>())
            .Returns(ValueTask.FromResult<object?>("Invalid json response"));

        // Act & Assert
        var exception = Assert.Throws<Exception>(() =>
            _dietPlugin.UpdatePlanForPrompt(
                age: 35,
                actualWeight: 85m,
                actualHeight: 180m,
                sex: SexEnum.Male,
                dietType: DietType.Standard,
                caloricDemand: 2500m,
                previousWeight: 90m,
                previousHeight: 180m,
                previousCaloricDemand: 2700m
            ));

        exception!.Message.Should().Be("Error through Json parsing plan");
    }

    [Test]
    public void UpdatePlanForPrompt_ThrowsException_WhenDeserializationReturnsNull()
    {
        // Arrange
        _mockDiets.Clear();
        // Return valid JSON structure but with null values that might cause deserialization to return null
        var validJson = "{\"Id\":null,\"DietName\":null,\"Description\":null}";

        _mockDiets.Count.Returns(0);
        _mockKernel.InvokePromptAsync(Arg.Any<string>(), Arg.Any<KernelArguments>())
            .Returns(ValueTask.FromResult<object?>(validJson));

        // Act & Assert
        var exception = Assert.Throws<Exception>(() =>
            _dietPlugin.UpdatePlanForPrompt(
                age: 28,
                actualWeight: 68m,
                actualHeight: 165m,
                sex: SexEnum.Female,
                dietType: DietType.Vegan,
                caloricDemand: 1800m,
                previousWeight: 72m,
                previousHeight: 165m,
                previousCaloricDemand: 2000m
            ));

        exception!.Message.Should().Be("Error through deserialize, unexpected error in returned prompt.");
    }

    [Test]
    public void UpdatePlanForPrompt_PassesCorrectParametersToKernel()
    {
        // Arrange
        var validJson = "{\"Id\":1,\"DietName\":\"Updated Plan\",\"Description\":\"Test\",\"Age\":40,\"ForWeight\":90,\"ForHeight\":185,\"CaloricValue\":2600,\"ForSex\":\"Male\",\"DietType\":\"Keto\"}";
        
        _mockKernel.InvokePromptAsync(Arg.Any<string>(), Arg.Any<KernelArguments>())
            .Returns(ValueTask.FromResult<object?>(validJson));

        // Act
        _dietPlugin.UpdatePlanForPrompt(
            age: 40,
            actualWeight: 90m,
            actualHeight: 185m,
            sex: SexEnum.Male,
            dietType: DietType.Keto,
            caloricDemand: 2600m,
            previousWeight: 95m,
            previousHeight: 185m,
            previousCaloricDemand: 2800m
        );

        // Assert
        _mockKernel.Received(1).CreateFunctionFromPrompt(Arg.Is<string>(s => 
            s.Contains("Id must have a value equal to: 6") && // _diets.Count + 1 = 5 + 1 = 6
            s.Contains("Update an existing monthly diet plan") &&
            s.Contains("Previous Weight: {{previousWeight}}") &&
            s.Contains("Previous Height: {{previousHeight}}") &&
            s.Contains("Previous Caloric Demand: {{previousCaloricDemand}}") &&
            s.Contains("Age: {{age}}") &&
            s.Contains("Weight: {{weight}}") &&
            s.Contains("Height: {{height}}") &&
            s.Contains("Sex: {{sex}}") &&
            s.Contains("DietType: {{dietType}}") &&
            s.Contains("CaloricDemand: {{caloricDemand}}")));
        
    }

    [Test]
    public void UpdatePlanForPrompt_HandlesWeightLossScenario()
    {
        // Arrange
        _mockDiets.Clear();
        var validJson = "{\"Id\":1,\"DietName\":\"Weight Loss Maintenance\",\"Description\":\"Adjusted plan for weight loss from 100kg to 85kg\",\"Age\":35,\"ForWeight\":85,\"ForHeight\":180,\"CaloricValue\":2100,\"ForSex\":\"Male\",\"DietType\":\"LowFat\"}";

        _mockDiets.Count.Returns(0);
        _mockKernel.InvokePromptAsync(Arg.Any<string>(), Arg.Any<KernelArguments>())
            .Returns(ValueTask.FromResult<object?>(validJson));

        // Act
        var result = _dietPlugin.UpdatePlanForPrompt(
            age: 35,
            actualWeight: 85m,
            actualHeight: 180m,
            sex: SexEnum.Male,
            dietType: DietType.LowFat,
            caloricDemand: 2100m,
            previousWeight: 100m,
            previousHeight: 180m,
            previousCaloricDemand: 2500m
        );

        // Assert
        result.Should().NotBeNull();
        result.ForWeight.Should().Be(85m);
        result.CaloricValue.Should().Be(2100m);
        result.DietType.Should().Be(DietType.LowFat);
    }

    [Test]
    public void UpdatePlanForPrompt_HandlesWeightGainScenario()
    {
        // Arrange
        _mockDiets.Clear();
        var validJson = "{\"Id\":1,\"DietName\":\"Muscle Gain Plan\",\"Description\":\"Updated plan for weight gain from 60kg to 65kg\",\"Age\":25,\"ForWeight\":65,\"ForHeight\":170,\"CaloricValue\":2800,\"ForSex\":\"Female\",\"DietType\":\"HighProtein\"}";
        
        _mockDiets.Count.Returns(0);
        _mockKernel.InvokePromptAsync(Arg.Any<string>(), Arg.Any<KernelArguments>())
            .Returns(ValueTask.FromResult<object?>(validJson));

        // Act
        var result = _dietPlugin.UpdatePlanForPrompt(
            age: 25,
            actualWeight: 65m,
            actualHeight: 170m,
            sex: SexEnum.Female,
            dietType: DietType.HighProtein,
            caloricDemand: 2800m,
            previousWeight: 60m,
            previousHeight: 170m,
            previousCaloricDemand: 2200m
        );

        // Assert
        result.Should().NotBeNull();
        result.ForWeight.Should().Be(65m);
        result.CaloricValue.Should().Be(2800m);
        result.DietType.Should().Be(DietType.HighProtein);
        result.ForSex.Should().Be(SexEnum.Female);
    }

    [Test]
    public void UpdatePlanForPrompt_HandlesDifferentSexTypes()
    {
        // Arrange        
        _mockDiets.Clear();
        var validJson = "{\"Id\":1,\"DietName\":\"Balanced Plan\",\"Description\":\"Updated plan\",\"Age\":30,\"ForWeight\":70,\"ForHeight\":175,\"CaloricValue\":2000,\"ForSex\":\"Unbinary\",\"DietType\":\"Standard\"}";

        _mockDiets.Count.Returns(0);
        _mockKernel.InvokePromptAsync(Arg.Any<string>(), Arg.Any<KernelArguments>())
            .Returns(ValueTask.FromResult<object?>(validJson));

        // Act
        var result = _dietPlugin.UpdatePlanForPrompt(
            age: 30,
            actualWeight: 70m,
            actualHeight: 175m,
            sex: SexEnum.Unbinary,
            dietType: DietType.Standard,
            caloricDemand: 2000m,
            previousWeight: 72m,
            previousHeight: 175m,
            previousCaloricDemand: 2100m
        );

        // Assert
        result.Should().NotBeNull();
        result.ForSex.Should().Be(SexEnum.Unbinary);
    }
}

