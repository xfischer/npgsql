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
        public object ConsumerName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int DequeueMode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int Navigation { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int Visibility { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int Wait { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public object msgid { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public object Correlation { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public object DeqCondition { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public object Transformation { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int DeliveryMode { get; set; }

        /// <summary>
        /// 
        /// </summary>
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
        public EDBAQDequeueOptions(object consumer_name, int dequeue_mode, int navigation, int visibility, int wait, object msgid, object correlation, object deq_condition, object transformation, int delivery_mode)
        {
            this.ConsumerName = consumer_name;
            this.DequeueMode = dequeue_mode;
            this.Navigation = navigation;
            this.Visibility = visibility;
            this.Wait = wait;
            this.msgid = msgid;
            this.Correlation = correlation;
            this.DeqCondition = deq_condition;
            this.Transformation = transformation;
            this.DeliveryMode = delivery_mode;
        }
    }
}
