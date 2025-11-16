using DieteticAi.Models;
using DieteticAi.Plugins;
using FluentAssertions;
using Microsoft.SemanticKernel;
using NSubstitute;

namespace DietAI.Tests;

public class TestableDietPlugin : DietPlugin
{
    public TestableDietPlugin(IList<Diets> diets, Kernel kernel) : base(diets, kernel)
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
    private IList<Diets> _mockDiets = null!;
    private Kernel _mockKernel = null!;

    [SetUp]
    public void Setup()
    {
        _mockDiets = Substitute.For<IList<Diets>>();
        _mockKernel = Substitute.For<Kernel>();
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

        var validJson = "{\"Id\":1,\"DietName\":\"Updated Weight Loss Plan\",\"Description\":\"Updated plan based on weight change from 80kg to 75kg\",\"Age\":30,\"ForWeight\":75,\"ForHeight\":175,\"CaloricValue\":2200,\"ForSex\":\"Male\",\"DietType\":\"Keto\"}";

        var mockFunction = Substitute.For<KernelFunction>();
        var mockResult = Substitute.For<FunctionResult>();
        mockResult.ToString().Returns(validJson);

        _mockDiets.Count.Returns(0);
        _mockKernel.CreateFunctionFromPrompt(Arg.Any<string>())
            .Returns(mockFunction);
        mockFunction.InvokeAsync(Arg.Any<KernelArguments>())
            .Returns(mockResult);

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
        _mockKernel.Received(1).CreateFunctionFromPrompt(Arg.Any<string>());
    }

    [Test]
    public void UpdatePlanForPrompt_ThrowsException_WhenKernelReturnsEmptyResponse()
    {
        // Arrange
        var mockFunction = Substitute.For<KernelFunction>();
        var mockResult = Substitute.For<FunctionResult>();
        mockResult.ToString().Returns(string.Empty);

        _mockDiets.Count.Returns(0);
        _mockKernel.CreateFunctionFromPrompt(Arg.Any<string>())
            .Returns(mockFunction);
        mockFunction.InvokeAsync(Arg.Any<KernelArguments>())
            .Returns(mockResult);

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
        _mockKernel.Received(1).CreateFunctionFromPrompt(Arg.Any<string>());
    }

    [Test]
    public void UpdatePlanForPrompt_ThrowsException_WhenKernelReturnsInvalidJson()
    {
        // Arrange
        var mockFunction = Substitute.For<KernelFunction>();
        var mockResult = Substitute.For<FunctionResult>();
        mockResult.ToString().Returns("invalid json response");

        _mockDiets.Count.Returns(0);
        _mockKernel.CreateFunctionFromPrompt(Arg.Any<string>())
            .Returns(mockFunction);
        mockFunction.InvokeAsync(Arg.Any<KernelArguments>())
            .Returns(mockResult);

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
        _mockKernel.Received(1).CreateFunctionFromPrompt(Arg.Any<string>());
    }

    [Test]
    public void UpdatePlanForPrompt_ThrowsException_WhenDeserializationReturnsNull()
    {
        // Arrange
        var mockFunction = Substitute.For<KernelFunction>();
        var mockResult = Substitute.For<FunctionResult>();
        // Return valid JSON structure but with null values that might cause deserialization to return null
        mockResult.ToString().Returns("{\"Id\":null,\"DietName\":null,\"Description\":null}");

        _mockDiets.Count.Returns(0);
        _mockKernel.CreateFunctionFromPrompt(Arg.Any<string>())
            .Returns(mockFunction);
        mockFunction.InvokeAsync(Arg.Any<KernelArguments>())
            .Returns(mockResult);

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
        _mockKernel.Received(1).CreateFunctionFromPrompt(Arg.Any<string>());
    }

    [Test]
    public void UpdatePlanForPrompt_PassesCorrectParametersToKernel()
    {
        // Arrange
        var mockFunction = Substitute.For<KernelFunction>();
        var mockResult = Substitute.For<FunctionResult>();
        var validJson = "{\"Id\":1,\"DietName\":\"Updated Plan\",\"Description\":\"Test\",\"Age\":40,\"ForWeight\":90,\"ForHeight\":185,\"CaloricValue\":2600,\"ForSex\":\"Male\",\"DietType\":\"Keto\"}";
        mockResult.ToString().Returns(validJson);

        _mockDiets.Count.Returns(5);
        _mockKernel.CreateFunctionFromPrompt(Arg.Any<string>())
            .Returns(mockFunction);
        mockFunction.InvokeAsync(Arg.Any<KernelArguments>())
            .Returns(mockResult);

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

        mockFunction.Received(1).InvokeAsync(Arg.Any<KernelArguments>());
    }

    [Test]
    public void UpdatePlanForPrompt_HandlesWeightLossScenario()
    {
        // Arrange
        var mockFunction = Substitute.For<KernelFunction>();
        var mockResult = Substitute.For<FunctionResult>();
        var validJson = "{\"Id\":1,\"DietName\":\"Weight Loss Maintenance\",\"Description\":\"Adjusted plan for weight loss from 100kg to 85kg\",\"Age\":35,\"ForWeight\":85,\"ForHeight\":180,\"CaloricValue\":2100,\"ForSex\":\"Male\",\"DietType\":\"LowFat\"}";
        mockResult.ToString().Returns(validJson);

        _mockDiets.Count.Returns(0);
        _mockKernel.CreateFunctionFromPrompt(Arg.Any<string>())
            .Returns(mockFunction);
        mockFunction.InvokeAsync(Arg.Any<KernelArguments>())
            .Returns(mockResult);

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
        var mockFunction = Substitute.For<KernelFunction>();
        var mockResult = Substitute.For<FunctionResult>();
        var validJson = "{\"Id\":1,\"DietName\":\"Muscle Gain Plan\",\"Description\":\"Updated plan for weight gain from 60kg to 65kg\",\"Age\":25,\"ForWeight\":65,\"ForHeight\":170,\"CaloricValue\":2800,\"ForSex\":\"Female\",\"DietType\":\"HighProtein\"}";
        mockResult.ToString().Returns(validJson);

        _mockDiets.Count.Returns(0);
        _mockKernel.CreateFunctionFromPrompt(Arg.Any<string>())
            .Returns(mockFunction);
        mockFunction.InvokeAsync(Arg.Any<KernelArguments>())
            .Returns(mockResult);

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
        var mockFunction = Substitute.For<KernelFunction>();
        var mockResult = Substitute.For<FunctionResult>();
        var validJson = "{\"Id\":1,\"DietName\":\"Balanced Plan\",\"Description\":\"Updated plan\",\"Age\":30,\"ForWeight\":70,\"ForHeight\":175,\"CaloricValue\":2000,\"ForSex\":\"Unbinary\",\"DietType\":\"Standard\"}";
        mockResult.ToString().Returns(validJson);

        _mockDiets.Count.Returns(0);
        _mockKernel.CreateFunctionFromPrompt(Arg.Any<string>())
            .Returns(mockFunction);
        mockFunction.InvokeAsync(Arg.Any<KernelArguments>())
            .Returns(mockResult);

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

