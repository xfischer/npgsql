#if NETSTANDARD2_0 || NETFRAMEWORK

using System.Net;
using System.Reflection;

namespace EnterpriseDB.EDBClient.Internal;

public partial class EDBConnector
{
    static readonly object disableSystemDefaultTlsVersionsLock = new();

    // volatile shouldn't be necessary since lock guarantees acquire/release semantics, but just in case
    static volatile bool disableSystemDefaultTlsVersionsChecked;
    static bool disableSystemDefaultTlsVersions;

    static bool DisableSystemDefaultTlsVersions
    {
        get
        {
            if (!disableSystemDefaultTlsVersionsChecked)
            {
                lock (disableSystemDefaultTlsVersionsLock)
                {
                    if (!disableSystemDefaultTlsVersionsChecked)
                    {
                        try
                        {
                            var spmType = typeof(ServicePointManager);
                            var disableDefaultProperty = spmType.GetProperty("DisableSystemDefaultTlsVersions", BindingFlags.Static | BindingFlags.NonPublic);
                            if (disableDefaultProperty is not null)
                            {
                                disableSystemDefaultTlsVersions = (bool)disableDefaultProperty.GetValue(null);
                            }
                            else
                            {
                                disableSystemDefaultTlsVersions = true;
                            }
                        }
                        catch
                        {
                            disableSystemDefaultTlsVersions = true;
                        }

                        disableSystemDefaultTlsVersionsChecked = true;
                    }
                }
            }

            return disableSystemDefaultTlsVersions;
        }
    }
}

#endif
