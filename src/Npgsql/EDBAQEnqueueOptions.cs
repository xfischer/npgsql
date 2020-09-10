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
    /// Provides EnqueueOptions available.
    /// </summary>
    public class EDBAQEnqueueOptions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <value>This is set from EDBAQVisibility. It represents whether the new message is enqueued or not as part of the current transaction.</value>
        public int Visibility { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <value>The relative message identifier.</value>
        public object RelativeMsgid { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <value>The sequence when the message should be dequeued.</value>
        public object SequenceDeviation { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <value>The transformation that will be applied before enqueuing the message.</value>
        public object Transformation { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <value>The delivery mode of the enqueued message.</value>
        public int DeliveryMode { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="visibility"></param>
        /// <param name="relative_msgid"></param>
        /// <param name="sequence_deviation"></param>
        /// <param name="transformation"></param>
        /// <param name="delivery_mode"></param>
        public EDBAQEnqueueOptions(int visibility, object relative_msgid, object sequence_deviation, object transformation, int delivery_mode)
        {
            this.Visibility = visibility;
            this.RelativeMsgid = relative_msgid;
            this.SequenceDeviation = sequence_deviation;
            this.Transformation = transformation;
            this.DeliveryMode = delivery_mode;
        }

        /// <summary>
        /// 
        /// </summary>
        public EDBAQEnqueueOptions()
        {

        }
    }

}
#nullable restore