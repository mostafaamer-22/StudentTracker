using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentTracker.Domain.Entities;

namespace StudentTracker.Infrastructure.Configurations;

public class TeacherConfiguration : IEntityTypeConfiguration<Teacher>
{
    public void Configure(EntityTypeBuilder<Teacher> builder)
    {
        builder.Property(t => t.Department)
            .HasMaxLength(100);

        builder.Property(t => t.Qualifications)
            .HasMaxLength(1000);
     
        builder.HasIndex(t => t.Department);

        builder.HasMany(t => t.TeachingCourses)
            .WithOne(c => c.Teacher)
            .HasForeignKey(c => c.TeacherId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasMany(t => t.AssignedStudents)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "TeacherStudents",
                j => j.HasOne<Student>().WithMany().HasForeignKey("StudentId"),
                j => j.HasOne<Teacher>().WithMany().HasForeignKey("TeacherId"),
                j =>
                {
                    j.HasKey("TeacherId", "StudentId");
                    j.ToTable("TeacherStudents");
                });
    }
}