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
using System.Text;
using System.Threading.Tasks;
#nullable disable
namespace EnterpriseDB.EDBClient
{
    /// <summary>
    /// Provides DequeueOptions available.
    /// </summary>
    public class EDBAQDequeueOptions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <value>Confirms receipt of the message.</value>
        public string ConsumerName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <value>The name of the consumer for which to dequeue the message.</value>
        public int DequeueMode { get; set; } = EDBAQDequeueMode.REMOVE;
        /// <summary>
        /// 
        /// </summary>
        /// <value>This is set from EDBAQDequeueMode. It represents the locking behavior linked with the dequeue option.</value>
        public int Navigation { get; set; } = EDBAQNavigationMode.NEXT_MESSAGE;
        /// <summary>
        /// 
        /// </summary>
        /// <value>This is set from EDBAQNavigationMode.  It represents the position of the message that will be fetched.</value>
        public int Visibility { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <value>This is set from EDBAQVisibility. It represents whether the new message is dequeued or not as part of the current transaction.</value>
        public int Wait { get; set; } = 0; // No wait
        /// <summary>
        /// 
        /// </summary>
        /// <value>The wait time for a message as per the search criteria.</value>
        public byte[] Msgid { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <value>The message identifier.</value>
        public string Correlation { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <value>The correlation identifier.</value>
        public string DeqCondition { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <value>The dequeuer condition. It is a Boolean expression.</value>
        public string Transformation { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <value>The transformation that will be applied before dequeuing the message.</value>
        public int DeliveryMode { get; set; } = 0;

        /// <summary>
        /// 
        /// </summary>
        /// <value>The delivery mode of the dequeued message.</value>
        public EDBAQDequeueOptions() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="consumer_name"></param>
        /// <param name="dequeue_mode"></param>
        /// <param name="navigation"></param>
        /// <param name="visibility"></param>
        /// <param name="wait"></param>
        /// <param name="msgid"></param>
        /// <param name="correlation"></param>
        /// <param name="deq_condition"></param>
        /// <param name="transformation"></param>
        /// <param name="delivery_mode"></param>
        public EDBAQDequeueOptions(string consumer_name, int dequeue_mode, int navigation, int visibility, int wait, byte[] msgid, string correlation, string deq_condition, string transformation, int delivery_mode)
        {
            ConsumerName = consumer_name;
            DequeueMode = dequeue_mode;
            Navigation = navigation;
            Visibility = visibility;
            Wait = wait;
            Msgid = msgid;
            Correlation = correlation;
            DeqCondition = deq_condition;
            Transformation = transformation;
            DeliveryMode = delivery_mode;
        }
    }
}
#nullable restore
