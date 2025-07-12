using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentTracker.Application.Features.Teachers.Queries.GetTeacherStudents;
public sealed record GetStudentsForSpecificalTeacherDto(
    Guid Id,
    string FullName,
    string Email,
    int Grade,
    int Age);