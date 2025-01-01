using System.Collections.Generic;
using System.Threading.Tasks;
using NeatShift.Models;

namespace NeatShift.Services
{
    public interface INeatSavesService
    {
        Task<bool> CreateNeatSave(string sourcePath, string destinationPath, string description);
        List<NeatSavesOperation> GetNeatSaves();
        Task<(bool success, string operationId)> RestoreNeatSave(string operationId);
        Task<bool> DeleteNeatSave(string operationId);
        Task<bool> Initialize();
        bool IsInitialized { get; }
    }
} 