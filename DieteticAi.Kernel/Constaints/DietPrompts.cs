namespace DieteticAi.Constaints;

public static class DietPrompts
{
    public static string CreatePlanPrompt(int id)
        => @"
        You are a diet planner.
        Generate a monthly diet plan in valid JSON only. Return a single JSON object with this exact schema:
        {
          ""Id"": 0,
          ""DietName"": ""..."",
          ""Description"": ""..."",
          ""Age"": 0,
          ""ForWeight"": 0,
          ""ForHeight"": 0,
          ""CaloricValue"": 0,
          ""ForSex"": ""..."",
          ""DietType"": ""Standard""
        }
        Rules:
        - Return JSON only, no markdown fences, no comments, no extra text.
        - Id must be the integer value: " + (id) + @"
        - Age, ForWeight, ForHeight and CaloricValue must be JSON numbers, not strings.
        - Do not include units like kg, cm, kcal, or /day in JSON values.
        - ForSex must be one of: Male, Female, Unbinary.
        - DietType must be one of the known enum names.
        - Description may mention units in prose, but JSON numeric fields must stay numeric.

        Constraints:
        - Age: {{$age}}
        - Weight: {{$weight}} kg
        - Height: {{$height}} cm
        - Sex: {{$sex}}
        - DietType: {{$dietType}}
        ";
    
    public static string UpdatePlanPrompt(int id) 
        => @"
        You are a diet planner.
        Update an existing monthly diet plan and return valid JSON only with this exact schema:
        {
          ""Id"": 0,
          ""DietName"": ""..."",
          ""Description"": ""..."",
          ""Age"": 0,
          ""ForWeight"": 0,
          ""ForHeight"": 0,
          ""CaloricValue"": 0,
          ""ForSex"": ""..."",
          ""DietType"": ""...""
        }
        Rules:
        - Return JSON only, no markdown fences, no comments, no extra text.
        - Id must be the integer value: " + (id) + @"
        - Age, ForWeight, ForHeight and CaloricValue must be JSON numbers, not strings.
        - Do not include units like kg, cm, kcal, or /day in JSON values.
        - ForSex must be one of: Male, Female, Unbinary.
        - DietType must be one of the known enum names.
        - Description may mention units in prose, but JSON numeric fields must stay numeric.

        Previous values:
        - Previous Weight: {{$previousWeight}} kg
        - Previous Height: {{$previousHeight}} cm
        - Previous Caloric Demand: {{$previousCaloricDemand}} kcal

        Current values:
        - Age: {{$age}}
        - Weight: {{$weight}} kg
        - Height: {{$height}} cm
        - Sex: {{$sex}}
        - DietType: {{$dietType}}
        - CaloricDemand: {{$caloricDemand}} kcal
        ";
}
