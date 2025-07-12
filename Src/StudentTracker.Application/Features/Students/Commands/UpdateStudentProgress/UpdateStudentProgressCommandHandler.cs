using Application.Abstractions.Messaging;
using StudentTracker.Application.Abstractions.Messaging;
using StudentTracker.Domain.Entities;
using StudentTracker.Domain.Repositories;
using StudentTracker.Domain.Shared;

namespace StudentTracker.Application.Features.Students.Commands.UpdateStudentProgress;

internal sealed class UpdateStudentProgressCommandHandler : ICommandHandler<UpdateStudentProgressCommand>
{
    private readonly IGenericRepository<Student> _studentRepository;
    private readonly IGenericRepository<Assignment> _assignmentRepository;
    private readonly IGenericRepository<StudentProgress> _progressRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateStudentProgressCommandHandler(
        IGenericRepository<Student> studentRepository,
        IGenericRepository<Assignment> assignmentRepository,
        IGenericRepository<StudentProgress> progressRepository,
        IUnitOfWork unitOfWork)
    {
        _studentRepository = studentRepository;
        _assignmentRepository = assignmentRepository;
        _progressRepository = progressRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateStudentProgressCommand command, CancellationToken cancellationToken)
    {
        var student = await _studentRepository.GetByIdAsync(command.StudentId, cancellationToken);
        var assignment = await _assignmentRepository.GetByIdAsync(command.AssignmentId, cancellationToken);

        var progress = await _progressRepository.GetByPropertyAsync(
            p => p.StudentId == command.StudentId && p.AssignmentId == command.AssignmentId,
            cancellationToken);

        if (progress is null)
        {
            progress = StudentProgress.Create(
                command.CompletionPercentage,
                command.TimeSpentMinutes,
                command.Status,
                command.EarnedPoints,
                command.StartedAt,
                command.CompletedAt,
                command.LastAccessedAt,
                command.AccessCount,
                command.Notes,
                command.StudentId,
                command.AssignmentId);

            await _progressRepository.AddAsync(progress, cancellationToken);
        }
        else
        {
            progress.UpdateProgress(
                command.CompletionPercentage,
                command.TimeSpentMinutes,
                command.Status,
                command.EarnedPoints,
                command.StartedAt,
                command.CompletedAt,
                command.LastAccessedAt,
                command.AccessCount,
                command.Notes);

            _progressRepository.Update(progress);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}