﻿using Funcs.TextExtractor.Data.Entities;

namespace Funcs.TextExtractor.Repositories;

public interface IImageProcessingTaskRepository
{
    Task CreateAsync(ImageProcessingTask task);

    Task UpdateAsync(ImageProcessingTask task);

    Task<ImageProcessingTask> GetByIdAsync(string id);

    Task<IEnumerable<ImageProcessingTask>> GetAllAsync();
}