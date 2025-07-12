using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentTracker.Domain.Entities;

namespace StudentTracker.Infrastructure.Configurations;

public class AssignmentConfiguration : IEntityTypeConfiguration<Assignment>
{
    public void Configure(EntityTypeBuilder<Assignment> builder)
    {
        builder.ToTable("Assignments");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Title)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(a => a.Description)
            .HasMaxLength(2000);

        builder.Property(a => a.MaxPoints)
            .HasPrecision(10, 2);

        builder.Property(a => a.Instructions)
            .HasMaxLength(5000);

        builder.HasIndex(a => a.CourseId);
        builder.HasIndex(a => a.DueDate);
        builder.HasIndex(a => a.Type);

        builder.HasOne(a => a.Course)
            .WithMany(c => c.Assignments)
            .HasForeignKey(a => a.CourseId);

        builder.HasMany(a => a.StudentProgress)
            .WithOne(sp => sp.Assignment)
            .HasForeignKey(sp => sp.AssignmentId)
            .OnDelete(DeleteBehavior.NoAction);


        builder.HasMany(a => a.Assessments)
            .WithOne(ass => ass.Assignment)
            .HasForeignKey(ass => ass.AssignmentId)
           .OnDelete(DeleteBehavior.NoAction);

    }
}