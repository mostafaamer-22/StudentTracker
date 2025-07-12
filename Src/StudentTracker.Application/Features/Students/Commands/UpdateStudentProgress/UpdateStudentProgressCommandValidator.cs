using FluentValidation;
using StudentTracker.Application.Extensions.FluentValidator;
using StudentTracker.Domain.Entities;
using StudentTracker.Domain.Repositories;

namespace StudentTracker.Application.Features.Students.Commands.UpdateStudentProgress;

public sealed class UpdateStudentProgressCommandValidator : AbstractValidator<UpdateStudentProgressCommand>
{
    private readonly IGenericRepository<Student> _studentRepository;
    private readonly IGenericRepository<Assignment> _assignmentRepository;

    public UpdateStudentProgressCommandValidator(
        IGenericRepository<Student> studentRepository,
        IGenericRepository<Assignment> assignmentRepository)
    {
        _studentRepository = studentRepository;
        _assignmentRepository = assignmentRepository;

        RuleFor(x => x.StudentId)
            .NotEmpty()
            .MustAsync(StudentExists)
            .WithMessage("Student not found");

        RuleFor(x => x.AssignmentId)
            .NotEmpty()
            .MustAsync(AssignmentExists)
            .WithMessage("Assignment not found");

        RuleFor(x => x.CompletionPercentage)
            .InclusiveBetween(0, 100)
            .WithMessage("Completion percentage must be between 0 and 100");

        RuleFor(x => x.TimeSpentMinutes)
            .GreaterThan(0)
            .WithMessage("Time spent must be greater than 0 minutes");


        When(x => x.Notes != null, () =>
        {
            RuleFor(x => x.Notes)
                .MaximumLength(500)
                .WithMessage("Notes cannot exceed 500 characters");
        });
    }

    private async Task<bool> StudentExists(Guid studentId, CancellationToken cancellationToken)
    {
        var student = await _studentRepository.GetByIdAsync(studentId, cancellationToken);
        return student != null;
    }

    private async Task<bool> AssignmentExists(Guid assignmentId, CancellationToken cancellationToken)
    {
        var assignment = await _assignmentRepository.GetByIdAsync(assignmentId, cancellationToken);
        return assignment != null;
    }


}