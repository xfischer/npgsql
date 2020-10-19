using Microsoft.VisualStudio.Data.Framework;
using Microsoft.VisualStudio.Data.Framework.AdoDotNet;
using Microsoft.VisualStudio.Data.Services.SupportEntities;
using System;
using System.Reflection;
using System.Runtime.InteropServices;

#nullable disable

namespace EnterpriseDB.EDBClient.VSIX
{
    [Guid(Guid)]
    class EDBProviderObjectFactory : DataProviderObjectFactory
    {
        internal const string Guid = "555cd66B-3393-4bab-84d9-3f2caa639699";

        public override object CreateObject(Type objType)
        {
            if (objType == typeof(IVsDataConnectionSupport))
                return new AdoDotNetConnectionSupport();
            if (objType == typeof(IVsDataConnectionProperties) || objType == typeof(IVsDataConnectionUIProperties))
                return new EDBConnectionProperties();
            if (objType == typeof(IVsDataConnectionUIControl))
                return new EDBConnectionUIControl();
            if (objType == typeof(IVsDataSourceInformation))
                return new EDBSourceInformation();
            if (objType == typeof(IVsDataObjectSupport))
                return new DataObjectSupport($"{GetType().Namespace}.EDBDataObjectSupport", Assembly.GetExecutingAssembly());
            if (objType == typeof(IVsDataViewSupport))
                return new DataViewSupport($"{GetType().Namespace}.EDBDataViewSupport", Assembly.GetExecutingAssembly());
            if (objType == typeof(IVsDataConnectionEquivalencyComparer))
                return new EDBConnectionEquivalencyComparer();
            return null;
        }
    }
}
