// created on 09/07/2003 at 20:20
// Npgsql.EDBParameterCollection.cs
//
// Author:
// Brar Piening (brar@gmx.de)
//
// Rewritten from the scratch to derive from MarshalByRefObject instead of ArrayList.
// Recycled some parts of the original EDBParameterCollection.cs
// by Francisco Jr. (fxjrlists@yahoo.com.br)
//
// Copyright (C) 2002 The Npgsql Development Team
// npgsql-general@gborg.postgresql.org
// http://gborg.postgresql.org/project/npgsql/projdisplay.php
//
// Permission to use, copy, modify, and distribute this software and its
// documentation for any purpose, without fee, and without a written
// agreement is hereby granted, provided that the above copyright notice
// and this paragraph and the following two paragraphs appear in all copies.
// 
// IN NO EVENT SHALL THE NPGSQL DEVELOPMENT TEAM BE LIABLE TO ANY PARTY
// FOR DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES,
// INCLUDING LOST PROFITS, ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS
// DOCUMENTATION, EVEN IF THE NPGSQL DEVELOPMENT TEAM HAS BEEN ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
// 
// THE NPGSQL DEVELOPMENT TEAM SPECIFICALLY DISCLAIMS ANY WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY
// AND FITNESS FOR A PARTICULAR PURPOSE. THE SOFTWARE PROVIDED HEREUNDER IS
// ON AN "AS IS" BASIS, AND THE NPGSQL DEVELOPMENT TEAM HAS NO OBLIGATIONS
// TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Resources;
using EDBTypes;
using System.Reflection;

#if WITHDESIGN

#endif

namespace EnterpriseDB.EDBClient
{
	/// <summary>
	/// Represents a collection of parameters relevant to a <see cref="Npgsql.NpgsqlCommand">NpgsqlCommand</see>
	/// as well as their respective mappings to columns in a <see cref="System.Data.DataSet">DataSet</see>.
	/// This class cannot be inherited.
	/// </summary>

#if WITHDESIGN
    [ListBindable(false)]
    [Editor(typeof(NpgsqlParametersEditor), typeof(System.Drawing.Design.UITypeEditor))]
#endif

	public sealed class EDBParameterCollection : DbParameterCollection, IList<EDBParameter>
	{
		private readonly List<EDBParameter> InternalList = new List<EDBParameter>();
        private EDBParameter return_param = null;
        private int return_index = -1;

		// Logging related value
        private static readonly String CLASSNAME = MethodBase.GetCurrentMethod().DeclaringType.Name;

        // Our resource manager
        private static readonly ResourceManager resman = new ResourceManager(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Initializes a new instance of the EDBParameterCollection class.
		/// </summary>
		internal EDBParameterCollection()
		{
			EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, CLASSNAME);
		}

        public EDBParameter ReturnParam
        {
            get { return return_param; }
        }
        public int ReturnIndex
        {
            get { return return_index; }
        }

		#region EDBParameterCollection Member

		/// <summary>
		/// Gets the <see cref="Npgsql.EDBParameter">EDBParameter</see> with the specified name.
		/// </summary>
		/// <param name="parameterName">The name of the <see cref="Npgsql.EDBParameter">EDBParameter</see> to retrieve.</param>
		/// <value>The <see cref="Npgsql.EDBParameter">EDBParameter</see> with the specified name, or a null reference if the parameter is not found.</value>

#if WITHDESIGN
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
#endif

		public new EDBParameter this[string parameterName]
		{
			get
			{
				EDBEventLog.LogIndexerGet(LogLevel.Debug, CLASSNAME, parameterName);
				return this.InternalList[IndexOf(parameterName)];
			}
			set
			{
				EDBEventLog.LogIndexerSet(LogLevel.Debug, CLASSNAME, parameterName, value);
				this.InternalList[IndexOf(parameterName)] = value;
			}
		}

		/// <summary>
		/// Gets the <see cref="Npgsql.EDBParameter">EDBParameter</see> at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index of the <see cref="Npgsql.EDBParameter">EDBParameter</see> to retrieve.</param>
		/// <value>The <see cref="Npgsql.EDBParameter">EDBParameter</see> at the specified index.</value>

#if WITHDESIGN
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
#endif

		public new EDBParameter this[int index]
		{
			get
			{
				EDBEventLog.LogIndexerGet(LogLevel.Debug, CLASSNAME, index);
				return this.InternalList[index];
			}
			set
			{
				EDBEventLog.LogIndexerSet(LogLevel.Debug, CLASSNAME, index, value);
				this.InternalList[index] = value;
			}
		}

		/// <summary>
		/// Adds the specified <see cref="Npgsql.EDBParameter">EDBParameter</see> object to the <see cref="Npgsql.EDBParameterCollection">EDBParameterCollection</see>.
		/// </summary>
		/// <param name="value">The <see cref="Npgsql.EDBParameter">EDBParameter</see> to add to the collection.</param>
		/// <returns>The index of the new <see cref="Npgsql.EDBParameter">EDBParameter</see> object.</returns>
		public EDBParameter Add(EDBParameter value)
		{
			EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "Add", value);

            if (value.Direction.ToString() != "ReturnValue")
            {
                // Do not allow parameters without name.

                this.InternalList.Add(value);

                // Check if there is a name. If not, add a name based in the index of parameter.
                if (value.ParameterName.Trim() == String.Empty || (value.ParameterName.Length == 1 && value.ParameterName[0] == ':'))
                {
                    value.ParameterName = ":" + "Parameter" + (IndexOf(value) + 1);
                }
            }
            else
            {
                return_param = value;
                return_index = this.InternalList.Count;
                return null;
            }


			return value;
		}

		/// <summary>
		/// Adds a <see cref="Npgsql.EDBParameter">EDBParameter</see> to the <see cref="Npgsql.EDBParameterCollection">EDBParameterCollection</see> given the specified parameter name and value.
		/// </summary>
		/// <param name="parameterName">The name of the <see cref="Npgsql.EDBParameter">EDBParameter</see>.</param>
		/// <param name="value">The Value of the <see cref="Npgsql.EDBParameter">EDBParameter</see> to add to the collection.</param>
		/// <returns>The index of the new <see cref="Npgsql.EDBParameter">EDBParameter</see> object.</returns>
		/// <remarks>
		/// Use caution when using this overload of the
		/// <b>Add</b> method to specify integer parameter values.
		/// Because this overload takes a <i>value</i> of type Object,
		/// you must convert the integral value to an <b>Object</b>
		/// type when the value is zero, as the following C# example demonstrates.
		/// <code>parameters.Add(":pname", Convert.ToInt32(0));</code>
		/// If you do not perform this conversion, the compiler will assume you
		/// are attempting to call the EDBParameterCollection.Add(string, DbType) overload.
		/// </remarks>
		public EDBParameter Add(string parameterName, object value)
		{
			EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "Add", parameterName, value);
			return this.Add(new EDBParameter(parameterName, value));
		}

		/// <summary>
		/// Adds a <see cref="Npgsql.EDBParameter">EDBParameter</see> to the <see cref="Npgsql.EDBParameterCollection">EDBParameterCollection</see> given the parameter name and the data type.
		/// </summary>
		/// <param name="parameterName">The name of the parameter.</param>
		/// <param name="parameterType">One of the DbType values.</param>
		/// <returns>The index of the new <see cref="Npgsql.EDBParameter">EDBParameter</see> object.</returns>
		public EDBParameter Add(string parameterName, EDBDbType parameterType)
		{
			EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "Add", parameterName, parameterType);
			return this.Add(new EDBParameter(parameterName, parameterType));
		}

		/// <summary>
		/// Adds a <see cref="Npgsql.EDBParameter">EDBParameter</see> to the <see cref="Npgsql.EDBParameterCollection">EDBParameterCollection</see> with the parameter name, the data type, and the column length.
		/// </summary>
		/// <param name="parameterName">The name of the parameter.</param>
		/// <param name="parameterType">One of the DbType values.</param>
		/// <param name="size">The length of the column.</param>
		/// <returns>The index of the new <see cref="Npgsql.EDBParameter">EDBParameter</see> object.</returns>
		public EDBParameter Add(string parameterName, EDBDbType parameterType, int size)
		{
			EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "Add", parameterName, parameterType, size);
			return this.Add(new EDBParameter(parameterName, parameterType, size));
		}

		/// <summary>
		/// Adds a <see cref="Npgsql.EDBParameter">EDBParameter</see> to the <see cref="Npgsql.EDBParameterCollection">EDBParameterCollection</see> with the parameter name, the data type, the column length, and the source column name.
		/// </summary>
		/// <param name="parameterName">The name of the parameter.</param>
		/// <param name="parameterType">One of the DbType values.</param>
		/// <param name="size">The length of the column.</param>
		/// <param name="sourceColumn">The name of the source column.</param>
		/// <returns>The index of the new <see cref="Npgsql.EDBParameter">EDBParameter</see> object.</returns>
		public EDBParameter Add(string parameterName, EDBDbType parameterType, int size, string sourceColumn)
		{
			EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "Add", parameterName, parameterType, size, sourceColumn);
			return this.Add(new EDBParameter(parameterName, parameterType, size, sourceColumn));
		}

		#endregion

		#region IDataParameterCollection Member

		/// <summary>
		/// Removes the specified <see cref="Npgsql.EDBParameter">EDBParameter</see> from the collection using the parameter name.
		/// </summary>
		/// <param name="parameterName">The name of the <see cref="Npgsql.EDBParameter">EDBParameter</see> object to retrieve.</param>
		public override void RemoveAt(string parameterName)
		{
			EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "RemoveAt", parameterName);
			this.InternalList.RemoveAt(IndexOf(parameterName));
		}

		/// <summary>
		/// Gets a value indicating whether a <see cref="Npgsql.EDBParameter">EDBParameter</see> with the specified parameter name exists in the collection.
		/// </summary>
		/// <param name="parameterName">The name of the <see cref="Npgsql.EDBParameter">EDBParameter</see> object to find.</param>
		/// <returns><b>true</b> if the collection contains the parameter; otherwise, <b>false</b>.</returns>
		public override bool Contains(string parameterName)
		{
			EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "Contains", parameterName);
			return (IndexOf(parameterName) != -1);
		}

		/// <summary>
		/// Gets the location of the <see cref="Npgsql.EDBParameter">EDBParameter</see> in the collection with a specific parameter name.
		/// </summary>
		/// <param name="parameterName">The name of the <see cref="Npgsql.EDBParameter">EDBParameter</see> object to find.</param>
		/// <returns>The zero-based location of the <see cref="Npgsql.EDBParameter">EDBParameter</see> in the collection.</returns>
		public override int IndexOf(string parameterName)
		{
			EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "IndexOf", parameterName);

			// Iterate values to see what is the index of parameter.
			int index = 0;
		        int bestChoose = -1;
			if ((parameterName[0] == ':') || (parameterName[0] == '@'))
			{
				parameterName = parameterName.Remove(0, 1);
			}


			foreach (EDBParameter parameter in this)
			{
				// allow for optional use of ':' and '@' in the ParameterName property
                		string cleanName = parameter.CleanName;
		                if(cleanName == parameterName)
				{
					return index;
				}
				if(string.Compare(parameterName, cleanName, StringComparison.InvariantCultureIgnoreCase) == 0)
				{
				    bestChoose = index;
				}
				index++;
			}
			return bestChoose;
		}

		#endregion

		#region IList Member

		public override bool IsReadOnly
		{
			get
			{
				EDBEventLog.LogPropertyGet(LogLevel.Debug, CLASSNAME, "IsReadOnly");
				return false;
			}
		}

		/// <summary>
		/// Removes the specified <see cref="Npgsql.EDBParameter">EDBParameter</see> from the collection using a specific index.
		/// </summary>
		/// <param name="index">The zero-based index of the parameter.</param>
		public override void RemoveAt(int index)
		{
			EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "RemoveAt", index);
			this.InternalList.RemoveAt(index);
		}

		/// <summary>
		/// Inserts a <see cref="Npgsql.EDBParameter">EDBParameter</see> into the collection at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index where the parameter is to be inserted within the collection.</param>
		/// <param name="value">The <see cref="Npgsql.EDBParameter">EDBParameter</see> to add to the collection.</param>
		public override void Insert(int index, object value)
		{
			EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "Insert", index, value);
			CheckType(value);
			this.InternalList.Insert(index, (EDBParameter) value);
		}

		/// <summary>
		/// Removes the specified <see cref="Npgsql.EDBParameter">EDBParameter</see> from the collection.
		/// </summary>
		/// <param name="value">The <see cref="Npgsql.EDBParameter">EDBParameter</see> to remove from the collection.</param>
		public override void Remove(object value)
		{
			EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "Remove", value);
			CheckType(value);
			this.InternalList.Remove((EDBParameter) value);
		}

		/// <summary>
		/// Gets a value indicating whether a <see cref="Npgsql.EDBParameter">EDBParameter</see> exists in the collection.
		/// </summary>
		/// <param name="value">The value of the <see cref="Npgsql.EDBParameter">EDBParameter</see> object to find.</param>
		/// <returns>true if the collection contains the <see cref="Npgsql.EDBParameter">EDBParameter</see> object; otherwise, false.</returns>
		public override bool Contains(object value)
		{
			EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "Contains", value);
			if (!(value is EDBParameter))
			{
				return false;
			}
			return this.InternalList.Contains((EDBParameter) value);
		}


		/// <summary>
		/// Gets a value indicating whether a <see cref="Npgsql.EDBParameter">EDBParameter</see> with the specified parameter name exists in the collection.
		/// </summary>
		/// <param name="parameterName">The name of the <see cref="Npgsql.EDBParameter">EDBParameter</see> object to find.</param>
		/// <param name="parameter">A reference to the requested parameter is returned in this out param if it is found in the list.  This value is null if the parameter is not found.</param>
		/// <returns><b>true</b> if the collection contains the parameter and param will contain the parameter; otherwise, <b>false</b>.</returns>
		public bool TryGetValue(string parameterName, out EDBParameter parameter)
		{
			EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "TryGetValue", parameterName);

			int index = IndexOf(parameterName);

			if (index != -1)
			{
				parameter = this[index];

				return true;
			}

			else
			{
				parameter = null;

				return false;
			}
		}


		/// <summary>
		/// Removes all items from the collection.
		/// </summary>
		public override void Clear()
		{
			EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "Clear");
			this.InternalList.Clear();
		}

		/// <summary>
		/// Gets the location of a <see cref="Npgsql.EDBParameter">EDBParameter</see> in the collection.
		/// </summary>
		/// <param name="value">The value of the <see cref="Npgsql.EDBParameter">EDBParameter</see> object to find.</param>
		/// <returns>The zero-based index of the <see cref="Npgsql.EDBParameter">EDBParameter</see> object in the collection.</returns>
		public override int IndexOf(object value)
		{
			EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "IndexOf", value);
			CheckType(value);
			return this.InternalList.IndexOf((EDBParameter) value);
		}

		/// <summary>
		/// Adds the specified <see cref="Npgsql.EDBParameter">EDBParameter</see> object to the <see cref="Npgsql.EDBParameterCollection">EDBParameterCollection</see>.
		/// </summary>
		/// <param name="value">The <see cref="Npgsql.EDBParameter">EDBParameter</see> to add to the collection.</param>
		/// <returns>The zero-based index of the new <see cref="Npgsql.EDBParameter">EDBParameter</see> object.</returns>
		public override int Add(object value)
		{
			EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "Add", value);
			CheckType(value);
			this.Add((EDBParameter) value);
			return IndexOf(value);
		}

		public override bool IsFixedSize
		{
			get
			{
				EDBEventLog.LogPropertyGet(LogLevel.Debug, CLASSNAME, "IsFixedSize");
				return false;
			}
		}

		#endregion

		#region ICollection Member

		public override bool IsSynchronized
		{
			get
			{
				EDBEventLog.LogPropertyGet(LogLevel.Debug, CLASSNAME, "IsSynchronized");
				return (InternalList as ICollection).IsSynchronized;
			}
		}

		/// <summary>
		/// Gets the number of <see cref="Npgsql.EDBParameter">EDBParameter</see> objects in the collection.
		/// </summary>
		/// <value>The number of <see cref="Npgsql.EDBParameter">EDBParameter</see> objects in the collection.</value>

#if WITHDESIGN
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
#endif

		public override int Count
		{
			get
			{
				EDBEventLog.LogPropertyGet(LogLevel.Debug, CLASSNAME, "Count");
				return this.InternalList.Count;
			}
		}

		/// <summary>
		/// Copies <see cref="Npgsql.EDBParameter">EDBParameter</see> objects from the <see cref="Npgsql.EDBParameterCollection">EDBParameterCollection</see> to the specified array.
		/// </summary>
		/// <param name="array">An <see cref="System.Array">Array</see> to which to copy the <see cref="Npgsql.EDBParameter">EDBParameter</see> objects in the collection.</param>
		/// <param name="index">The starting index of the array.</param>
		public override void CopyTo(Array array, int index)
		{
			EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "CopyTo", array, index);
			(InternalList as ICollection).CopyTo(array, index);
			IRaiseItemChangedEvents x = InternalList as IRaiseItemChangedEvents;
		}

		public override object SyncRoot
		{
			get
			{
				EDBEventLog.LogPropertyGet(LogLevel.Debug, CLASSNAME, "SyncRoot");
				return (InternalList as ICollection).SyncRoot;
			}
		}

		#endregion

		#region IEnumerable Member

		/// <summary>
		/// Returns an enumerator that can iterate through the collection.
		/// </summary>
		/// <returns>An <see cref="System.Collections.IEnumerator">IEnumerator</see> that can be used to iterate through the collection.</returns>
		public override IEnumerator GetEnumerator()
		{
			EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "GetEnumerator");
			return this.InternalList.GetEnumerator();
		}

		#endregion

		public override void AddRange(Array values)
		{
			EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "AddRange", values);
			foreach (EDBParameter parameter in values)
			{
				Add(parameter);
			}
		}

		protected override DbParameter GetParameter(string parameterName)
		{
			return this[parameterName];
		}

		protected override DbParameter GetParameter(int index)
		{
			return this[index];
		}

		protected override void SetParameter(string parameterName, DbParameter value)
		{
			this[parameterName] = (EDBParameter) value;
		}

		protected override void SetParameter(int index, DbParameter value)
		{
			this[index] = (EDBParameter) value;
		}

		/// <summary>
		/// In methods taking an object as argument this method is used to verify
		/// that the argument has the type <see cref="Npgsql.EDBParameter">EDBParameter</see>
		/// </summary>
		/// <param name="Object">The object to verify</param>
		private void CheckType(object Object)
		{
			EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "CheckType", Object);
			if (!(Object is EDBParameter))
			{
				throw new InvalidCastException(
					String.Format(resman.GetString("Exception_WrongType"), Object.GetType()));
			}
		}

/*
		/// <summary>
		/// In methods taking an array as argument this method is used to verify
		/// that the argument has the type <see cref="Npgsql.EDBParameter">EDBParameter</see>[]
		/// </summary>
		/// <param name="array">The array to verify</param>
		private void CheckType(Array array)
		{
			EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "CheckType", array);
			if (array.GetType() != typeof (EDBParameter[]))
			{
				throw new InvalidCastException(
					String.Format(this.resman.GetString("Exception_WrongType"), array.GetType().ToString()));
			}
		}
*/

		EDBParameter IList<EDBParameter>.this[int index]
		{
			get { return InternalList[index]; }
			set { InternalList[index] = value; }
		}

		public int IndexOf(EDBParameter item)
		{
			return InternalList.IndexOf(item);
		}

		public void Insert(int index, EDBParameter item)
		{
			InternalList.Insert(index, item);
		}

		public bool Contains(EDBParameter item)
		{
			return InternalList.Contains(item);
		}

		public bool Remove(EDBParameter item)
		{
			return Remove(item);
		}

		IEnumerator<EDBParameter> IEnumerable<EDBParameter>.GetEnumerator()
		{
			return InternalList.GetEnumerator();
		}

		public void CopyTo(EDBParameter[] array, int arrayIndex)
		{
			InternalList.CopyTo(array, arrayIndex);
		}

		void ICollection<EDBParameter>.Add(EDBParameter item)
		{
			Add(item);
		}
	}
}
