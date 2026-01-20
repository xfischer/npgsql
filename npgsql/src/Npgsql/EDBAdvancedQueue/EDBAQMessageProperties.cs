using System;
using System.Text;

namespace EnterpriseDB.EDBClient;

/// <summary>
/// Provides Message Properties available.
/// <see href="https://www.enterprisedb.com/docs/epas/latest/reference/oracle_compatibility_reference/epas_compat_bip_guide/03_built-in_packages/02_dbms_aq/01_enqueue/"/>
/// </summary>
public class EDBAQMessageProperties
{
    /// <summary>
    /// 
    /// </summary>
    /// <value>The priority of the message.</value>
    public int Priority { get; set; } = 1;
    /// <summary>
    /// 
    /// </summary>
    /// <value>The duration post which the message is available for dequeuing. This is specified in second.</value>
    public int Delay { get; set; } = 0;
    /// <summary>
    /// 
    /// </summary>
    /// <value>The duration for which the message is available for dequeuing. This is specified in seconds.</value>
    public object? Expiration { get; set; } = null;
    /// <summary>
    /// 
    /// </summary>
    /// <value>The correlation identifie.</value>
    public object? Correlation { get; set; } = null;
    /// <summary>
    /// 
    /// </summary>
    /// <value>The number of attempts taken to dequeue the message.</value>
    public object? Attempts { get; set; } = null;
    /// <summary>
    /// 
    /// </summary>
    /// <value>The receipients list that overthrows the default queue subscribers.</value>
    public object? RecipientList { get; set; } = null;
    /// <summary>
    /// 
    /// </summary>
    /// <value>The name of the queue where the unprocessed messages should be moved.</value>
    public object? ExceptionQueue { get; set; } = null;
    /// <summary>
    /// 
    /// </summary>
    /// <value>The time when the message was enqueued.</value>
    public object? EnqueueTime { get; set; } = null;
    /// <summary>
    /// 
    /// </summary>
    /// <value>The state of the message while dequeue.</value>

    public object? State { get; set; } = null;
    /// <summary>
    /// 
    /// </summary>
    /// <value>The message identifier in the last queue.</value>
    public object? OriginalMsgid { get; set; } = null;
    /// <summary>
    /// 
    /// </summary>
    /// <value>The transaction group for the dequeued messages.</value>
    public object? TransactionGroup { get; set; } = null;
    /// <summary>
    /// 
    /// </summary>
    /// <value>The delivery mode of the dequeued message.</value>
    public int DeliveryMode { get; set; } = 0;


    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    internal string ToTextParam()
    {
        var sb = new StringBuilder();
        sb.Append('(');
        var props = GetType().GetProperties();
        foreach (var prp in props)
        {
            var value = prp.GetValue(this, null);
            if (value != null)
            {
                sb.Append(value.ToString() + ",");
            }
            else
            {
                sb.Append(',');
            }
        }
        sb.Remove(sb.Length - 1, 1);
        sb.Append(')');
        return sb.ToString();
    }

    internal EDBAQMessageProperties ToObjectParam(string value)
    {
        if (value == null || value.Length <= 0)
        {
            throw new InvalidOperationException("Value must be set");
        }
        value = value.Replace(@"(", string.Empty).Replace(@")", string.Empty);
        var obj = new EDBAQMessageProperties();
        var arr = value.Split(',');
        var props = GetType().GetProperties();
        for (var i = 0; i < arr.Length; i++)
        {
            if (arr[i] != null && arr[i] != "")
            {
                if (props[i].PropertyType.FullName == "System.Int32")
                    props[i].SetValue(obj, Convert.ToInt32(arr[i]), null);

                if (props[i].Name == "EnqueueTime")
                    props[i].SetValue(obj, Convert.ToString(arr[i]), null);
            }

        }

        return obj;
    }
}
