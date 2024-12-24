using System.Threading.Tasks;

namespace NeatShift.Services
{
    public interface ISystemRestoreService
    {
        Task<bool> CreateRestorePoint(string description);
    }
} 