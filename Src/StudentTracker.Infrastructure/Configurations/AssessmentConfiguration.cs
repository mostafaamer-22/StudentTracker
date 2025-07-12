using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentTracker.Domain.Entities;

namespace StudentTracker.Infrastructure.Configurations;

public class AssessmentConfiguration : IEntityTypeConfiguration<Assessment>
{
    public void Configure(EntityTypeBuilder<Assessment> builder)
    {
        builder.ToTable("Assessments");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Score)
            .HasPrecision(10, 2);

        builder.Property(a => a.MaxScore)
            .HasPrecision(10, 2);

        builder.Property(a => a.Feedback)
            .HasMaxLength(2000);

        builder.HasIndex(a => a.StudentId);
        builder.HasIndex(a => a.AssignmentId);
        builder.HasIndex(a => a.Type);
        builder.HasIndex(a => a.TakenAt);

        builder.HasOne(a => a.Student)
            .WithMany(s => s.Assessments)
            .HasForeignKey(a => a.StudentId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(a => a.Assignment)
            .WithMany(ass => ass.Assessments)
            .HasForeignKey(a => a.AssignmentId)
            .OnDelete(DeleteBehavior.NoAction);

    }
}