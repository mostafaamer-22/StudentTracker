using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentTracker.Domain.Entities;

namespace StudentTracker.Infrastructure.Configurations;

public class StudentProgressConfiguration : IEntityTypeConfiguration<StudentProgress>
{
    public void Configure(EntityTypeBuilder<StudentProgress> builder)
    {
        builder.ToTable("StudentProgress");

        builder.HasKey(sp => sp.Id);

        builder.Property(sp => sp.CompletionPercentage)
            .HasPrecision(5, 2);

        builder.Property(sp => sp.EarnedPoints)
            .HasPrecision(10, 2);

        builder.Property(sp => sp.Notes)
            .HasMaxLength(1000);

        builder.HasIndex(sp => sp.StudentId);
        builder.HasIndex(sp => sp.AssignmentId);
        builder.HasIndex(sp => sp.Status);
        builder.HasIndex(sp => sp.LastAccessedAt);
        builder.HasIndex(sp => new { sp.StudentId, sp.AssignmentId }).IsUnique();

        builder.HasOne(sp => sp.Student)
            .WithMany(s => s.ProgressRecords)
            .HasForeignKey(sp => sp.StudentId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(sp => sp.Assignment)
            .WithMany(a => a.StudentProgress)
            .HasForeignKey(sp => sp.AssignmentId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}