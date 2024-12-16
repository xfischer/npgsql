#region License
// The PostgreSQL License
//
// Copyright (C) 2019 The EnterpriseDB.EDBClient Development Team
//
// Permission to use, copy, modify, and distribute this software and its
// documentation for any purpose, without fee, and without a written
// agreement is hereby granted, provided that the above copyright notice
// and this paragraph and the following two paragraphs appear in all copies.
//
// IN NO EVENT SHALL THE EnterpriseDB.EDBClient DEVELOPMENT TEAM BE LIABLE TO ANY PARTY
// FOR DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES,
// INCLUDING LOST PROFITS, ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS
// DOCUMENTATION, EVEN IF THE EnterpriseDB.EDBClient DEVELOPMENT TEAM HAS BEEN ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
//
// THE EnterpriseDB.EDBClient DEVELOPMENT TEAM SPECIFICALLY DISCLAIMS ANY WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY
// AND FITNESS FOR A PARTICULAR PURPOSE. THE SOFTWARE PROVIDED HEREUNDER IS
// ON AN "AS IS" BASIS, AND THE EnterpriseDB.EDBClient DEVELOPMENT TEAM HAS NO OBLIGATIONS
// TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
#nullable disable
namespace EnterpriseDB.EDBClient
{
    /// <summary>
    /// Provides Message Properties available.
    /// </summary>
    public class EDBAQMessageProperties
    {
        /// <summary>
        /// 
        /// </summary>
        /// <value>The priority of the message.</value>
        public int Priority { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <value>The duration post which the message is available for dequeuing. This is specified in second.</value>
        public int Delay { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <value>The duration for which the message is available for dequeuing. This is specified in seconds.</value>
        public object Expiration { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <value>The correlation identifie.</value>
        public object Correlation { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <value>The number of attempts taken to dequeue the message.</value>
        public object Attempts { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <value>The receipients list that overthrows the default queue subscribers.</value>
        public object RecipientList { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <value>The name of the queue where the unprocessed messages should be moved.</value>
        public object ExceptionQueue { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <value>The time when the message was enqueued.</value>
        public object EnqueueTime { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <value>The state of the message while dequeue.</value>

        public object State { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <value>The message identifier in the last queue.</value>
        public object OriginalMsgid { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <value>The transaction group for the dequeued messages.</value>
        public object TransactionGroup { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <value>The delivery mode of the dequeued message.</value>
        public int DeliveryMode { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="priority"></param>
        /// <param name="delay"></param>
        /// <param name="expiration"></param>
        /// <param name="correlation"></param>
        /// <param name="attempts"></param>
        /// <param name="recipient_list"></param>
        /// <param name="exception_queue"></param>
        /// <param name="enqueue_time"></param>
        /// <param name="state"></param>
        /// <param name="original_msgid"></param>
        /// <param name="transaction_group"></param>
        /// <param name="delivery_mode"></param>
        public EDBAQMessageProperties(int priority, int delay, object expiration, object correlation, object attempts, object recipient_list, object exception_queue, object enqueue_time, object state, object original_msgid, object transaction_group, int delivery_mode)
        {
            Priority = priority;
            Delay = delay;
            Expiration = expiration;
            Correlation = correlation;
            Attempts = attempts;
            RecipientList = recipient_list;
            ExceptionQueue = exception_queue;
            EnqueueTime = enqueue_time;
            State = state;
            OriginalMsgid = original_msgid;
            TransactionGroup = transaction_group;
            DeliveryMode = delivery_mode;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        internal string ToTextParam()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("(");
            PropertyInfo[] props = GetType().GetProperties();
            foreach (PropertyInfo prp in props)
            {
                object value = prp.GetValue(this, null);
                if (value != null)
                {
                    sb.Append(value.ToString() + ",");
                }
                else
                {
                    sb.Append(",");
                }
            }
            sb.Remove(sb.Length - 1, 1);
            sb.Append(")");
            return sb.ToString();
        }

        internal EDBAQMessageProperties ToObjectParam(string value)
        {
            if (value == null || value.Length <= 0)
            {
                throw new InvalidOperationException("Valuue must be set");
            }
            value = value.Replace(@"(", string.Empty).Replace(@")", string.Empty); ;
            EDBAQMessageProperties obj = new EDBAQMessageProperties(1, 0, null, null, null, null, null, null, null, null, null, 0);
            string[] arr = value.Split(',');
            PropertyInfo[] props = GetType().GetProperties();
            for (int i = 0; i < arr.Length; i++)
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
}
#nullable restore
