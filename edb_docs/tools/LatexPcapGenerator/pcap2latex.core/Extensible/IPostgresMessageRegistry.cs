using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pcap2latex;

public interface IPostgresMessageRegistry
{
    PostgresMessage? GetMessage(char messageCode, bool? frontEnd);

    void AddOrReplaceBackendMessage(PostgresMessage pgMessage);

    void AddOrReplaceFrontendMessage(PostgresMessage pgMessage);

}
