using DieteticAi.Models;

namespace DietAI.Tests.DietPluginTests;

public static class DietPluginSeeds
{
    public static List<Diets> LoadDietData() => new()
    {
        new Diets
        {
            Id = 1,
            DietName = "Maintenance",
            Description = "Existing plan",
            Age = 30,
            ForWeight = 80m,
            CaloricValue = 2400m,
            ForSex = SexEnum.Male
        },
    };
}