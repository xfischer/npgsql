using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseDB.EDBClient;

// EnterpriseDB: Used by CompositeConverter to allow Text to composite conversion (EC-3164)
internal interface ITextFormatConverter
{
    object ReadFromValues(params object[] values);
}
