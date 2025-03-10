using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace pcap2latex;

class PostgresMessageRegistry : IPostgresMessageRegistry
{
    private readonly Dictionary<char, PostgresMessage> backendMessages = [];
    private readonly Dictionary<char, PostgresMessage> frontendMessages = [];

    public void AddOrReplaceBackendMessage(PostgresMessage pgMessage)
    {
        if (backendMessages.TryGetValue(pgMessage.Code, out var _))
        {
            //Trace.TraceWarning("Message '{Code}' exists (name: {Name}). Replacing with {NewName}",
            //                    existingMessage.Code,
            //                    existingMessage.Name,
            //                    message.Name);
        }
        backendMessages[pgMessage.Code] = pgMessage;
    }

    public void AddOrReplaceFrontendMessage(PostgresMessage pgMessage)
    {
        if (frontendMessages.TryGetValue(pgMessage.Code, out var _))
        {
            //logger.LogWarning("Message '{Code}' exists (name: {Name}). Replacing with {NewName}",
            //                    existingMessage.Code,
            //                    existingMessage.Name,
            //                    message.Name);
        }
        frontendMessages[pgMessage.Code] = pgMessage;
    }

    public PostgresMessage? GetMessage(char messageCode, bool? frontEnd)
    {
        if (frontEnd ?? false)
        {
            if (frontendMessages.TryGetValue(messageCode, out var message))
                return message;
        }
        else
        {
            if (backendMessages.TryGetValue(messageCode, out var message))
                return message;
        }
        return null;
    }
}
