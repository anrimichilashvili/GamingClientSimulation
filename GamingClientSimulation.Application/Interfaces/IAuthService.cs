using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamingClientSimulation.Application.Interfaces
{
    public interface IAuthService
    {
        Task<string> LoginAsync(string userName, string password);
    }
}
