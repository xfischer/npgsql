using System;

namespace EnterpriseDB.EDBClient
{
    interface ICancelable : IDisposable
    {
        void Cancel();
    }
}
