using FluentValidation;

namespace DietAI.Api.Commands.Plan.UpdatePlan;

public class UpdatePlanCommandValidator : AbstractValidator<UpdatePlanCommand>
{
    public UpdatePlanCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty();

        RuleFor(x => x.Request.Age)
            .InclusiveBetween(15, 100);

        RuleFor(x => x.Request.ActualWeight)
            .InclusiveBetween(10.00m, 1500.00m);

        RuleFor(x => x.Request.ActualHeight)
            .InclusiveBetween(10.00m, 300.00m);

        RuleFor(x => x.Request.Sex)
            .IsInEnum();

        RuleFor(x => x.Request.DietType)
            .IsInEnum();

        RuleFor(x => x.Request.PreviousAge)
            .InclusiveBetween(15, 100);

        RuleFor(x => x.Request.PreviousWeight)
            .InclusiveBetween(10.00m, 1500.00m);

        RuleFor(x => x.Request.PreviousHeight)
            .InclusiveBetween(10.00m, 300.00m);
    }
}