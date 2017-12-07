using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace  EnterpriseDB.EDBClient
{
    interface ICancelable : IDisposable
    {
        void Cancel();
    }
}
