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
    public static PostgresMessage Undefined => new('0', "Undefined");

    private readonly Dictionary<char, PostgresMessage> backendMessages = [];
    private readonly Dictionary<char, PostgresMessage> frontendMessages = [];

    public void AddOrReplaceBackendMessage(PostgresMessage message)
    {
        if (backendMessages.TryGetValue(message.Code, out var _))
        {
            //Trace.TraceWarning("Message '{Code}' exists (name: {Name}). Replacing with {NewName}",
            //                    existingMessage.Code,
            //                    existingMessage.Name,
            //                    message.Name);
        }
        backendMessages[message.Code] = message;
    }

    public void AddOrReplaceFrontendMessage(PostgresMessage message)
    {
        if (frontendMessages.TryGetValue(message.Code, out var _))
        {
            //logger.LogWarning("Message '{Code}' exists (name: {Name}). Replacing with {NewName}",
            //                    existingMessage.Code,
            //                    existingMessage.Name,
            //                    message.Name);
        }
        frontendMessages[message.Code] = message;
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
