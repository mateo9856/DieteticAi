using System;
using System.Collections.Generic;
using System.Linq;
using DieteticAi.Models;
using DieteticAi.Plugins;
using FluentAssertions;
using Microsoft.SemanticKernel;
using NSubstitute;
using NUnit.Framework;

namespace DietAI.Tests;

[TestFixture]
public class DietPluginTests
{
    private DietPlugin _dietPlugin = null!;
    private IList<Diets> _mockDiets = null!;
    private Kernel _mockKernel = null!;

    [SetUp]
    public void Setup()
    {
        _mockDiets = Substitute.For<IList<Diets>>();
        _mockKernel = Substitute.For<Kernel>();
        _dietPlugin = new DietPlugin(_mockDiets, _mockKernel);
    }

    [Test]
    public void GetPlanFromListOrPrompt_ReturnsExistingPlan_WhenMatchFound()
    {
        // Arrange
        var existingPlan = new Diets
        {
            Id = 1,
            DietName = "Maintenance",
            Description = "Existing plan",
            Age = 30,
            ForWeight = 80m,
            CaloricValue = 2400m,
            ForSex = SexEnum.Male
        };

        _mockDiets.FirstOrDefault(Arg.Any<Func<Diets, bool>>())
            .Returns(existingPlan);

        // Act
        var result = _dietPlugin.GetPlanFromListOrPrompt(30, 80m, 185m, 2000, SexEnum.Male, DietType.Keto);

        // Assert
        result.Should().Be("Existing plan");
        _mockDiets.Received(1).FirstOrDefault(Arg.Any<Func<Diets, bool>>());
    }

    [Test]
    public void GetPlanFromListOrPrompt_GeneratesNewPlan_WhenNoMatchFound()
    {
        // Arrange - no match found
        _mockDiets.FirstOrDefault(Arg.Any<Func<Diets, bool>>())
            .Returns((Diets)null!);

        // Mock the kernel function creation and invocation
        var mockFunction = Substitute.For<KernelFunction>();
        var mockResult = Substitute.For<FunctionResult>();
        mockResult.ToString().Returns("{\"Id\":1,\"DietName\":\"Weight Loss\",\"Description\":\"Generated plan\",\"Age\":25,\"ForWeight\":70,\"CaloricValue\":2000,\"ForSex\":\"Female\"}");
        
        _mockKernel.CreateFunctionFromPrompt(Arg.Any<string>())
            .Returns(mockFunction);
        mockFunction.InvokeAsync(Arg.Any<KernelArguments>())
            .Returns(mockResult);

        // Act
        var result = _dietPlugin.GetPlanFromListOrPrompt(25, 70m, 170m, 2500, SexEnum.Female, DietType.HighProtein);

        // Assert
        result.Should().Be("Generated plan");
        _mockDiets.Received(1).FirstOrDefault(Arg.Any<Func<Diets, bool>>());
        _mockDiets.Received(1).Add(Arg.Any<Diets>());
        _mockKernel.Received(1).CreateFunctionFromPrompt(Arg.Any<string>());
    }

    [Test]
    public void GetPlanFromListOrPrompt_ReturnsExistingPlan_WhenAgeIsWithinRange()
    {
        // Arrange
        var existingPlan = new Diets
        {
            Id = 1,
            DietName = "Maintenance",
            Description = "Existing plan",
            Age = 30,
            ForWeight = 80m,
            CaloricValue = 2400m,
            ForSex = SexEnum.Male
        };

        _mockDiets.FirstOrDefault(Arg.Any<Func<Diets, bool>>())
            .Returns(existingPlan);

        // Act - age 31 should match age 30 (within 2 year range)
        var result = _dietPlugin.GetPlanFromListOrPrompt(31, 80m, 168m, 2300, SexEnum.Male, DietType.Standard);

        // Assert
        result.Should().Be("Existing plan");
        _mockDiets.Received(1).FirstOrDefault(Arg.Any<Func<Diets, bool>>());
    }

    [Test]
    public void GetPlanFromListOrPrompt_ReturnsExistingPlan_WhenWeightIsWithinRange()
    {
        // Arrange
        var existingPlan = new Diets
        {
            Id = 1,
            DietName = "Maintenance",
            Description = "Existing plan",
            Age = 30,
            ForWeight = 80m,
            CaloricValue = 2400m,
            ForSex = SexEnum.Male
        };

        _mockDiets.FirstOrDefault(Arg.Any<Func<Diets, bool>>())
            .Returns(existingPlan);

        // Act - weight 85 should match weight 80 (within 5kg range)
        var result = _dietPlugin.GetPlanFromListOrPrompt(30, 85m, 181, 0, SexEnum.Male, DietType.GlutenFree);

        // Assert
        result.Should().Be("Existing plan");
        _mockDiets.Received(1).FirstOrDefault(Arg.Any<Func<Diets, bool>>());
    }

    [Test]
    public void GetPlanFromListOrPrompt_ThrowsException_WhenKernelReturnsEmptyResponse()
    {
        // Arrange - no match found
        _mockDiets.FirstOrDefault(Arg.Any<Func<Diets, bool>>())
            .Returns((Diets)null!);

        // Mock the kernel function creation and invocation to return empty response
        var mockFunction = Substitute.For<KernelFunction>();
        var mockResult = Substitute.For<FunctionResult>();
        mockResult.ToString().Returns(string.Empty);
        
        _mockKernel.CreateFunctionFromPrompt(Arg.Any<string>())
            .Returns(mockFunction);
        mockFunction.InvokeAsync(Arg.Any<KernelArguments>())
            .Returns(mockResult);

        // Act & Assert
        var exception = Assert.Throws<Exception>(() => 
            _dietPlugin.GetPlanFromListOrPrompt(25, 70m, 169m, 2000, SexEnum.Female, DietType.LowFat));
        
        exception!.Message.Should().Be("Model returned empty response");
        _mockDiets.Received(1).FirstOrDefault(Arg.Any<Func<Diets, bool>>());
        _mockKernel.Received(1).CreateFunctionFromPrompt(Arg.Any<string>());
    }

    [Test]
    public void GetPlanFromListOrPrompt_ThrowsException_WhenKernelReturnsInvalidJson()
    {
        // Arrange - no match found
        _mockDiets.FirstOrDefault(Arg.Any<Func<Diets, bool>>())
            .Returns((Diets)null!);

        // Mock the kernel function creation and invocation to return invalid JSON
        var mockFunction = Substitute.For<KernelFunction>();
        var mockResult = Substitute.For<FunctionResult>();
        mockResult.ToString().Returns("invalid json response");
        
        _mockKernel.CreateFunctionFromPrompt(Arg.Any<string>())
            .Returns(mockFunction);
        mockFunction.InvokeAsync(Arg.Any<KernelArguments>())
            .Returns(mockResult);

        // Act & Assert
        var exception = Assert.Throws<Exception>(() => 
            _dietPlugin.GetPlanFromListOrPrompt(25, 70m, 165m, 0, SexEnum.Female, DietType.Vegan));
        
        exception!.Message.Should().Be("Error through Json parsing plan");
        _mockDiets.Received(1).FirstOrDefault(Arg.Any<Func<Diets, bool>>());
        _mockKernel.Received(1).CreateFunctionFromPrompt(Arg.Any<string>());
    }
}


