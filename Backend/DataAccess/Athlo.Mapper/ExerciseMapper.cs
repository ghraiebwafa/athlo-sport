using Athlo.Models.DTOs.Exercises;
using Athlo.Models.Entities;

namespace Athlo.Mapper;

public static class ExerciseMapper
{
    public static ExerciseDto ToDto(Exercise exercise) =>
        new()
        {
            Id = exercise.Id,
            Name = exercise.Name,
            ImageUrl = exercise.ImageUrl
        };
}
