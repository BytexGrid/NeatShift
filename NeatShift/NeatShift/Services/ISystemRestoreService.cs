using System.Collections.Generic;
using System.Threading.Tasks;
using NeatShift.Models;

namespace NeatShift.Services
{
    public interface ISystemRestoreService
    {
        Task<bool> CreateRestorePoint(string description);
        Task<List<RestorePoint>> GetRestorePoints();
        Task<bool> DeleteRestorePoint(string sequenceNumber);
    }
} 