namespace DieteticAi.Constaints;

public static class DietPrompts
{
    public static string CreatePlanPrompt(int id)
        => @"
        You are a diet planner.
        Generate a monthly diet plan in **JSON format** like this:
        {
          ""Id"": ""..."",
          ""DietName"": ""..."",
          ""Description"": ""..."",
          ""Age"": ""..."",
          ""ForWeight"": ""..."",
          ""CaloricValue"": ""..."",
          ""ForSex"": ""..."",

        }
        Id must have a value eqaul to: " + (id) + @"
        DietName should return basically diet topic name and Description summary plan with calculated value.
        Rest of fields similar to input, only CaloricValue should return calculated daily caloric.

        Constraints:
        - Age: {{age}}
        - Weight: {{weight}} kg
        - Height: {{height}} cm
        - Sex: {{sex}}
        - DietType: {{dietType}}
        ";
    
    public static string UpdatePlanPrompt(int id) 
        => @"
        You are a diet planner.
        Update an existing monthly diet plan in **JSON format** like this:
        {
          ""Id"": ""..."",
          ""DietName"": ""..."",
          ""Description"": ""..."",
          ""Age"": ""..."",
          ""ForWeight"": ""..."",
          ""ForHeight"": ""..."",
          ""CaloricValue"": ""..."",
          ""ForSex"": ""..."",
          ""DietType"": ""...""
        }
        Id must have a value equal to: " + (id) + @"
        DietName should return updated diet topic name and Description should reflect the changes from previous to current values.
        Rest of fields should match current input values, only CaloricValue should return calculated daily caloric based on current values.

        Previous values:
        - Previous Weight: {{previousWeight}} kg
        - Previous Height: {{previousHeight}} cm
        - Previous Caloric Demand: {{previousCaloricDemand}} kcal

        Current values:
        - Age: {{age}}
        - Weight: {{weight}} kg
        - Height: {{height}} cm
        - Sex: {{sex}}
        - DietType: {{dietType}}
        - CaloricDemand: {{caloricDemand}} kcal
        ";
}