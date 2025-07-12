using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentTracker.Domain.Entities;

namespace StudentTracker.Infrastructure.Configurations;

public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {


        builder.Property(s => s.Grade)
            .IsRequired();

        builder.Property(s => s.ParentEmail)
            .HasMaxLength(255);

        // Existing indexes
        builder.HasIndex(s => s.Grade);

        // Additional indexes for search performance
        builder.HasIndex(s => s.FullName)
            .HasDatabaseName("IX_Students_FullName");

        builder.HasIndex(s => s.Email)
            .HasDatabaseName("IX_Students_Email");

        builder.HasIndex(s => s.ParentEmail)
            .HasDatabaseName("IX_Students_ParentEmail");

        builder.HasIndex(s => s.CreatedOnUtc)
            .HasDatabaseName("IX_Students_CreatedOnUtc");

        // Composite index for common search patterns
        builder.HasIndex(s => new { s.Grade, s.IsActive })
            .HasDatabaseName("IX_Students_Grade_IsActive");

        builder.HasMany(s => s.ProgressRecords)
            .WithOne(p => p.Student)
            .HasForeignKey(p => p.StudentId)
            .OnDelete(DeleteBehavior.NoAction);


        builder.HasMany(s => s.Assessments)
            .WithOne(a => a.Student)
            .HasForeignKey(a => a.StudentId)
            .OnDelete(DeleteBehavior.NoAction);


        builder.HasMany(s => s.StudentCourses)
            .WithOne(sc => sc.Student)
            .HasForeignKey(sc => sc.StudentId)
           .OnDelete(DeleteBehavior.NoAction);

    }
}