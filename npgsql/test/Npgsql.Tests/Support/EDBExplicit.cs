using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseDB.EDBClient.Tests.Support;

public class EDBExplicit : ExplicitAttribute
{
    public EDBExplicit(string? reason) : base(reason)
    {
        
    }
}
