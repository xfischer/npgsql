

namespace EnterpriseDB.EDBClient;

/// <summary>
/// Provides message types for payload
/// </summary>
public enum EDBAQMessageType
{
    /// <summary>
    /// Raw (Not Supported)
    /// </summary>
    /// <value>The raw message type.Note: Currently, this payload type is not supported.</value>
    Raw = 1,
    /// <summary>
    /// User Defined Type
    /// </summary>
    /// <value>The user defined type message.</value>
    Udt = 2,

    /// <summary>
    /// Xml (Not Supported)
    /// </summary>
    /// <value>The XML type message.Note: Currently, this payload type is not supported.</value>
    Xml = 3
}
