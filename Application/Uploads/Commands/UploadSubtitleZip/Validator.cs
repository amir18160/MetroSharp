using FluentValidation;

namespace Application.Uploads.Commands.UploadSubtitleZip;

public class Validator : AbstractValidator<Command>
{
    public Validator()
    {
        RuleFor(x => x.File)
            .NotEmpty();
    }
}