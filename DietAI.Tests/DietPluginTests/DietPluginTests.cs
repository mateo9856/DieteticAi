using DieteticAi.Models;
using DieteticAi.Plugins;
using DieteticAi.Tools.Wrappers;
using FluentAssertions;
using Microsoft.SemanticKernel;
using Newtonsoft.Json;
using NSubstitute;
using static DietAI.Tests.DietPluginTests.DietPluginSeeds;

namespace DietAI.Tests.DietPluginTests;

[TestFixture]
public class DietPluginTests
{
    private DietPlugin _dietPlugin = null!;
    private List<Diets> _mockDiets = null!;
    private IKernelWrapper _mockKernel = null!;

    [SetUp]
    public void Setup()
    {
        _mockDiets = LoadDietData();
        _mockKernel = Substitute.For<IKernelWrapper>();
        _dietPlugin = new DietPlugin(_mockDiets, _mockKernel);
    }

    [Test]
    public void GetPlanFromListOrPrompt_ReturnsExistingPlan_WhenMatchFound()
    {
        // Act
        var result = _dietPlugin.GetPlanFromListOrPrompt(30, 80m, 185m, 2000, SexEnum.Male, DietType.Keto);

        // Assert
        result.Description.Should().Be("Existing plan");
    }

    [Test]
    public void GetPlanFromListOrPrompt_GeneratesNewPlan_WhenNoMatchFound()
    {
        // Arrange
        var mockData = _mockDiets.Find(d => d.Id == 1);
        mockData.Description = "New generated plan";
        var parsedData = JsonConvert.SerializeObject(mockData);
        
        // Mock the kernel function creation and invocation
        _mockKernel.InvokePromptAsync(Arg.Any<string>(), Arg.Any<KernelArguments>())
            .Returns(ValueTask.FromResult<object?>(parsedData));

        // Act
        var result = _dietPlugin.GetPlanFromListOrPrompt(25, 70m, 170m, 2500, SexEnum.Female, DietType.HighProtein);

        // Assert
        result.Description.Should().Be(mockData.Description);
    }

    [Test]
    public void GetPlanFromListOrPrompt_ReturnsExistingPlan_WhenAgeIsWithinRange()
    {
        // Act - age 31 should match age 30 (within 2 year range)
        var result = _dietPlugin.GetPlanFromListOrPrompt(31, 80m, 168m, 2300, SexEnum.Male, DietType.Keto);

        // Assert
        result.Description.Should().Be("Existing plan");
    }

    [Test]
    public void GetPlanFromListOrPrompt_ReturnsExistingPlan_WhenWeightIsWithinRange()
    {
        // Act - weight 85 should match weight 80 (within 5kg range)
        var result = _dietPlugin.GetPlanFromListOrPrompt(30, 85m, 181, 0, SexEnum.Male, DietType.Keto);

        // Assert
        result.Description.Should().Be("Existing plan");
    }

    [Test]
    public void GetPlanFromListOrPrompt_ThrowsException_WhenKernelReturnsEmptyResponse()
    {
        // Mock the kernel function creation and invocation to return empty response
        _mockKernel.InvokePromptAsync(Arg.Any<string>(), Arg.Any<KernelArguments>())
            .Returns(ValueTask.FromResult<object?>(string.Empty));

        // Act & Assert
        var exception = Assert.Throws<Exception>(() => 
            _dietPlugin.GetPlanFromListOrPrompt(25, 70m, 169m, 2000, SexEnum.Female, DietType.LowFat));
        
        exception!.Message.Should().Be("Model returned empty response");
    }

    [Test]
    public void GetPlanFromListOrPrompt_ThrowsException_WhenKernelReturnsInvalidJson()
    {        
        _mockKernel.InvokePromptAsync(Arg.Any<string>(), Arg.Any<KernelArguments>())
            .Returns(ValueTask.FromResult<object?>("invalid json response"));

        // Act & Assert
        var exception = Assert.Throws<Exception>(() => 
            _dietPlugin.GetPlanFromListOrPrompt(25, 70m, 165m, 0, SexEnum.Female, DietType.Vegan));
        
        exception!.Message.Should().Be("Error through Json parsing plan");
    }
}


