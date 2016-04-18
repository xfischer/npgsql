using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace  EnterpriseDB.EDBClient
{
    internal interface ICancelable : IDisposable
    {
        void Cancel();
    }
}
