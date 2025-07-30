using System;
using System.Threading.Tasks;

namespace EnterpriseDB.EDBClient;

interface ICancelable : IDisposable, IAsyncDisposable
{
    void Cancel();

    Task CancelAsync();
}