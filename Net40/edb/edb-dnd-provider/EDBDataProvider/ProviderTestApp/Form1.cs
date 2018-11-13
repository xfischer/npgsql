using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using EnterpriseDB.EDBClient;


namespace ProviderTestApp
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class Form1 : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Button button2;
		private System.Windows.Forms.Button button4;
		private System.Windows.Forms.Button button6;
		private System.Windows.Forms.Button button7;
		private System.Windows.Forms.Button button8;
		private System.Windows.Forms.Button button9;
		private System.Windows.Forms.Button button10;
		private System.Windows.Forms.Button button11;
		private System.Windows.Forms.Button button12;
		private System.Windows.Forms.Button button13;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Button button3;
		private System.Windows.Forms.Button button5;
		private System.Windows.Forms.Button button14;
		private System.Windows.Forms.Button btnUpdate;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public Form1()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.button2 = new System.Windows.Forms.Button();
			this.button4 = new System.Windows.Forms.Button();
			this.button6 = new System.Windows.Forms.Button();
			this.button7 = new System.Windows.Forms.Button();
			this.button8 = new System.Windows.Forms.Button();
			this.button9 = new System.Windows.Forms.Button();
			this.button10 = new System.Windows.Forms.Button();
			this.button11 = new System.Windows.Forms.Button();
			this.button12 = new System.Windows.Forms.Button();
			this.button13 = new System.Windows.Forms.Button();
			this.button1 = new System.Windows.Forms.Button();
			this.button3 = new System.Windows.Forms.Button();
			this.button5 = new System.Windows.Forms.Button();
			this.button14 = new System.Windows.Forms.Button();
			this.btnUpdate = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// button2
			// 
			this.button2.Location = new System.Drawing.Point(243, 224);
			this.button2.Name = "button2";
			this.button2.Size = new System.Drawing.Size(277, 32);
			this.button2.TabIndex = 1;
			this.button2.Text = "Simple Query";
			this.button2.Click += new System.EventHandler(this.button2_Click);
			// 
			// button4
			// 
			this.button4.Location = new System.Drawing.Point(8, 224);
			this.button4.Name = "button4";
			this.button4.Size = new System.Drawing.Size(224, 32);
			this.button4.TabIndex = 5;
			this.button4.Text = "jdbc_test_10(a OUT int4) ";
			this.button4.Click += new System.EventHandler(this.button4_Click_2);
			// 
			// button6
			// 
			this.button6.Location = new System.Drawing.Point(6, 175);
			this.button6.Name = "button6";
			this.button6.Size = new System.Drawing.Size(224, 40);
			this.button6.TabIndex = 6;
			this.button6.Text = " jdbc_test_11(a OUT int4,b OUT int4,c OUT int4) return varchar AS ";
			this.button6.Click += new System.EventHandler(this.button6_Click);
			// 
			// button7
			// 
			this.button7.Location = new System.Drawing.Point(5, 120);
			this.button7.Name = "button7";
			this.button7.Size = new System.Drawing.Size(224, 48);
			this.button7.TabIndex = 7;
			this.button7.Text = "jdbc_test_6(a IN int4, b OUT int4,c OUT int4,d OUT int4, e IN OUT int4, f IN OUT " +
				"int4,g IN OUT int4, h IN OUT int4)";
			this.button7.Click += new System.EventHandler(this.button7_Click);
			// 
			// button8
			// 
			this.button8.Location = new System.Drawing.Point(6, 64);
			this.button8.Name = "button8";
			this.button8.Size = new System.Drawing.Size(218, 48);
			this.button8.TabIndex = 8;
			this.button8.Text = "pg_jdbc_test5(a IN int4, b OUT int4, c IN int2, d OUT int2)";
			this.button8.Click += new System.EventHandler(this.button8_Click);
			// 
			// button9
			// 
			this.button9.Location = new System.Drawing.Point(4, 8);
			this.button9.Name = "button9";
			this.button9.Size = new System.Drawing.Size(224, 43);
			this.button9.TabIndex = 9;
			this.button9.Text = "jdbc_test_5(a IN int4,b OUT int4,c IN int2, d OUT int2) ";
			this.button9.Click += new System.EventHandler(this.button9_Click);
			// 
			// button10
			// 
			this.button10.Location = new System.Drawing.Point(240, 178);
			this.button10.Name = "button10";
			this.button10.Size = new System.Drawing.Size(280, 40);
			this.button10.TabIndex = 10;
			this.button10.Text = "jdbc_test_4b(a OUT int4,b IN int2,c IN OUT int2, d OUT VARCHAR, e IN OUT float4) " +
				"AS";
			this.button10.Click += new System.EventHandler(this.button10_Click);
			// 
			// button11
			// 
			this.button11.Location = new System.Drawing.Point(240, 120);
			this.button11.Name = "button11";
			this.button11.Size = new System.Drawing.Size(280, 46);
			this.button11.TabIndex = 11;
			this.button11.Text = " jdbc_test_3(a OUT int4,b OUT int4) AS";
			this.button11.Click += new System.EventHandler(this.button11_Click);
			// 
			// button12
			// 
			this.button12.Location = new System.Drawing.Point(240, 64);
			this.button12.Name = "button12";
			this.button12.Size = new System.Drawing.Size(280, 48);
			this.button12.TabIndex = 12;
			this.button12.Text = "jdbc_test_2(a IN int4,b IN int4) AS";
			this.button12.Click += new System.EventHandler(this.button12_Click);
			// 
			// button13
			// 
			this.button13.Location = new System.Drawing.Point(240, 8);
			this.button13.Name = "button13";
			this.button13.Size = new System.Drawing.Size(280, 48);
			this.button13.TabIndex = 13;
			this.button13.Text = "jdbc_test_1(a IN OUT int4,b IN OUT int4, c IN OUT varchar) AS";
			this.button13.Click += new System.EventHandler(this.button13_Click);
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(8, 264);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(80, 23);
			this.button1.TabIndex = 14;
			this.button1.Text = "EMP_Select ";
			this.button1.Click += new System.EventHandler(this.button1_Click_1);
			// 
			// button3
			// 
			this.button3.Location = new System.Drawing.Point(88, 264);
			this.button3.Name = "button3";
			this.button3.TabIndex = 15;
			this.button3.Text = "DEPT_SELECT";
			this.button3.Click += new System.EventHandler(this.button3_Click_1);
			// 
			// button5
			// 
			this.button5.Location = new System.Drawing.Point(24, 320);
			this.button5.Name = "button5";
			this.button5.TabIndex = 16;
			this.button5.Text = "EMP_INSERT1";
			this.button5.Click += new System.EventHandler(this.button5_Click_1);
			// 
			// button14
			// 
			this.button14.Location = new System.Drawing.Point(32, 400);
			this.button14.Name = "button14";
			this.button14.TabIndex = 18;
			this.button14.Text = "button14";
			this.button14.Click += new System.EventHandler(this.button14_Click_1);
			// 
			// btnUpdate
			// 
			this.btnUpdate.Location = new System.Drawing.Point(336, 304);
			this.btnUpdate.Name = "btnUpdate";
			this.btnUpdate.Size = new System.Drawing.Size(88, 23);
			this.btnUpdate.TabIndex = 19;
			this.btnUpdate.Text = "Emp Update";
			this.btnUpdate.Click += new System.EventHandler(this.btnUpdate_Click);
			// 
			// Form1
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(624, 446);
			this.Controls.Add(this.btnUpdate);
			this.Controls.Add(this.button14);
			this.Controls.Add(this.button5);
			this.Controls.Add(this.button3);
			this.Controls.Add(this.button1);
			this.Controls.Add(this.button13);
			this.Controls.Add(this.button12);
			this.Controls.Add(this.button11);
			this.Controls.Add(this.button10);
			this.Controls.Add(this.button9);
			this.Controls.Add(this.button8);
			this.Controls.Add(this.button7);
			this.Controls.Add(this.button6);
			this.Controls.Add(this.button4);
			this.Controls.Add(this.button2);
			this.Name = "Form1";
			this.Text = "Form1";
			this.Load += new System.EventHandler(this.Form1_Load);
			this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new Form1());
		}

		private void button1_Click(object sender, System.EventArgs e)
		{
			/*try  
			{
				for(int i = 0; i<3 ;i++)
				FunxCall(); 
//				SPWithCharArray(); 
//				SPWithStringArray();  
//				SPWithBoolArray();				
//				SPWithShortArray();		 		
//				SPWithIntArray();				
//				SPWithLongArray();				
//				SPWithFloatArray();				
//				SPWithDoubleArray();				
//				SPWithIntArray();
//				SimpleQueries();
//				PreparedOne() ;
			}			
			catch(Exception ex) 
			{
				MessageBox.Show(ex.ToString());
			}*/
			//for (int i =0 ;i<100 ;i++)
				test();
		}
		void FunxCall() 
		{
			EDBConnection conn = new EDBConnection("Server=10.90.1.18;Port=5444;User Id=postgres;Password=;Database=testdb");
			conn.Open();
			try
			{
				//
				//				EDBCommand command = new EDBCommand("cursor_test", conn);
				//				command.CommandType = CommandType.StoredProcedure;
				//				
				//				Object result = command.ExecuteScalar();
				//				
				//				MessageBox.Show(result.ToString());

				//				EDBCommand command = new EDBCommand("newcallee(:param1,:param2,:param3,:param4,param5)", conn);
				//				command.CommandType = CommandType.StoredProcedure;
				//				//command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Integer,,,,,ParameterDirection.Output));
				//				command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Integer,10,"param",ParameterDirection.Output,false ,2,2,System.Data.DataRowVersion.Current,1));
				//				command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Integer,10,"param1",ParameterDirection.Output,false ,2,2,System.Data.DataRowVersion.Current,1));
				//				
				//				command.Parameters.Add(new EDBParameter("param2", EDBTypes.EDBDbType.Integer));
				//				command.Prepare();
				//				command.Parameters[0].Value = 10;
				//				command.Parameters[1].Value = 20;
				//				Object result = command.ExecuteScalar();
				//				MessageBox.Show(result.ToString());


				//EDBCommand command = new EDBCommand("callee(:param1)", conn);
				EDBCommand command = new EDBCommand("newcallee(:param1,:param2,:param3,:param4,:param5)", conn);
				command.CommandType = CommandType.StoredProcedure;
				//command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Integer,,,,,ParameterDirection.Output));
				//command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Integer));
								
				command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Integer,10,"param1",ParameterDirection.Output,false ,2,2,System.Data.DataRowVersion.Current,1));
				command.Parameters.Add(new EDBParameter("param2", EDBTypes.EDBDbType.Integer,10,"param2",ParameterDirection.Input,false ,2,2,System.Data.DataRowVersion.Current,1));
				command.Parameters.Add(new EDBParameter("param3", EDBTypes.EDBDbType.Integer,10,"param3",ParameterDirection.InputOutput,false ,2,2,System.Data.DataRowVersion.Current,1));
				command.Parameters.Add(new EDBParameter("param4", EDBTypes.EDBDbType.Integer,10,"param4",ParameterDirection.Input,false ,2,2,System.Data.DataRowVersion.Current,1));
				command.Parameters.Add(new EDBParameter("param5", EDBTypes.EDBDbType.Integer,10,"param5",ParameterDirection.InputOutput,false ,2,2,System.Data.DataRowVersion.Current,1));
				command.Prepare();
				command.Parameters[0].Value = 1;
				command.Parameters[1].Value = 100;
				command.Parameters[2].Value = 100;
				command.Parameters[3].Value = 100;
				command.Parameters[4].Value = 100;
				
				//Object result = command.ExecuteScalar();
				//Object  result = new object();
				//command.ExecuteReader();
				
				EDBDataReader  result = command.ExecuteReader();
				while(result.Read())
				{
					for(int i=0;i<result.FieldCount;i++)
						//	MessageBox.Show("Val["+i+"]="+result.GetValue(i).ToString());
						Console.WriteLine("Val["+i+"]="+result.GetValue(i).ToString());
					//MessageBox.Show(result.GetValue(1).ToString());
					//MessageBox.Show(result.GetValue(2).ToString());
					//MessageBox.Show(result.GetValue(3).ToString());
				}
				//command.Parameters[0].ToString();
				/*	MessageBox.Show("OUT: " +command.Parameters[0].Value.ToString());   
					MessageBox.Show("IN: " +command.Parameters[1].Value.ToString());   
					MessageBox.Show("INOUT: " +command.Parameters[2].Value.ToString());   
					MessageBox.Show("IN: " +command.Parameters[3].Value.ToString());   
					MessageBox.Show("INOUT: " +command.Parameters[4].Value.ToString());
				*/	
				
			}
			catch(Exception ex)
			{
				Console.WriteLine(ex.StackTrace);
			}
			finally
			{
				conn.Close();
			}
		}


		void jdbc_test_1() 
		{		
				
			EDBConnection conn = new EDBConnection("Server=10.90.1.18;Port=5444;User Id=postgres;Password=;Database=test");
			conn.Open();
			try
			{
				EDBCommand command = new EDBCommand("jdbc_test_11(:param1,:param2,:param3)", conn);
				command.CommandType = CommandType.StoredProcedure;
				//command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Integer,,,,,ParameterDirection.Output));
				//command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Integer));
								
				command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Integer,10,"param1",ParameterDirection.Output,false ,2,2,System.Data.DataRowVersion.Current,1));
				command.Parameters.Add(new EDBParameter("param2", EDBTypes.EDBDbType.Integer,10,"param2",ParameterDirection.Output,false ,2,2,System.Data.DataRowVersion.Current,1));
				command.Parameters.Add(new EDBParameter("param3", EDBTypes.EDBDbType.Integer,10,"param3",ParameterDirection.Output,false ,2,2,System.Data.DataRowVersion.Current,1));
				command.Parameters.Add(new EDBParameter("param4", EDBTypes.EDBDbType.Text,10,"param4",ParameterDirection.ReturnValue,false ,2,2,System.Data.DataRowVersion.Current,1));
				
				
				command.Prepare();
				
				command.Parameters[0].Value = null;
				command.Parameters[1].Value = null;
				command.Parameters[2].Value = null;
				
				
				//command.Parameters[3].Value = 1;
				
				
				//Object result = command.ExecuteScalar();
				//Object  result = new object();
				//command.ExecuteReader();
				
				EDBDataReader  result = command.ExecuteReader();
				int fc=result.FieldCount;
				//MessageBox.Show("RESULT Count="+fc);
				while(result.Read())
				{
					for(int i=0;i<fc;i++)
						//MessageBox.Show("RESULT["+i+"]="+result.GetValue(i).ToString());
						//MessageBox.Show("RESULT["+i+"]="+ Convert.ToString(command.Parameters[i].Value));
						Console.WriteLine("RESULT["+i+"]="+ Convert.ToString(command.Parameters[i].Value));
					//command.Parameters[0].ToString();
					//MessageBox.Show(""command.Parameters[0].Value.ToString());   
					//MessageBox.Show(command.Parameters[1].Value.ToString());   
					//MessageBox.Show(command.Parameters[2].Value.ToString());   
					//MessageBox.Show(command.Parameters["ret_value"].Value.ToString());   
				}
			}
			finally 
			{
				conn.Close();
			
			}

		}



		void test() 
		{		
				
			EDBConnection conn = new EDBConnection("Server=10.90.1.18;Port=5444;User Id=postgres;Password=;Database=test");
			conn.Open();
			try
			{
				EDBCommand command = new EDBCommand("dotnettest(:param1)", conn);
				command.CommandType = CommandType.StoredProcedure;
				command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Integer,10,"param1",ParameterDirection.InputOutput,false ,2,2,System.Data.DataRowVersion.Current,1));
				command.Parameters.Add(new EDBParameter("param2", EDBTypes.EDBDbType.Text,10,"param2",ParameterDirection.ReturnValue,false ,2,2,System.Data.DataRowVersion.Current,1));
				
				command.Prepare();
				
				command.Parameters[0].Value = 7369;
				
				EDBDataReader  result = command.ExecuteReader();
				int fc=result.FieldCount;
				
				
				while(result.Read())
				{
					for(int i=0;i<fc;i++)
							Console.WriteLine("RESULT["+i+"]="+ Convert.ToString(command.Parameters[i].Value));
				
				}
			}
			finally 
			{
				conn.Close();
			
			}

		}




		static void WithoutParam() 
		{
			EDBConnection conn = new EDBConnection("Server=10.90.1.63;Port=5444;User Id=postgres;Password=;Database=hello");
			conn.Open();
			
			try
			{
				EDBCommand command = new EDBCommand("add(10,12)", conn);
				command.CommandType = CommandType.StoredProcedure;
				Object result = command.ExecuteScalar();
				MessageBox.Show(result.ToString());
			}
    
			finally
			{
				conn.Close();
			}
		}
		static void WithParam() 
		{
			EDBConnection conn = new EDBConnection("Server=127.0.0.1;Port=5432;User Id=postgres;Password=yespass;Database=TestDB;SSL=True");
			conn.Open();
    
      
			try
			{
				int[] ia = new int[]{100,20,300};
				EDBCommand command = new EDBCommand("select * from emp", conn);
				command.CommandType = CommandType.StoredProcedure;
				command.Parameters.Add(new EDBParameter("a", DbType.String));
				command.Prepare();
				command.Parameters[0].Value = ia;
				Object result = command.ExecuteScalar();
				MessageBox.Show(result.ToString());
			}
    
			finally
			{
				conn.Close();
			}
		
		}
		static void SPWithBoolArray() 
		{
			EDBConnection conn = new EDBConnection("Server=127.0.0.1;Port=5432;User Id=postgres;Password=yespass;Database=TestDB;SSL=True");
			conn.Open();
			try
			{
				bool[] boolArray = new bool[]{false,true};
				EDBCommand command = new EDBCommand("testforbool(:boolArray)", conn);
				command.CommandType = CommandType.StoredProcedure;
				command.Parameters.Add(new EDBParameter("boolArray",EDBTypes.EDBDbType.BooleanArray));
				command.Prepare();
				command.Parameters[0].Value = boolArray ;
				Object result = command.ExecuteScalar();
				MessageBox.Show(result.ToString());
			}
			finally
			{
				conn.Close();
			}
		}
		static void SPWithShortArray() 
		{
			EDBConnection conn = new EDBConnection("Server=127.0.0.1;Port=5432;User Id=postgres;Password=yespass;Database=TestDB;SSL=True");
			conn.Open();
			try
			{
				short[] shortArray = new short[]{1,2,3,4};
				EDBCommand command = new EDBCommand("testforshort(:shortArray)", conn);
				command.CommandType = CommandType.StoredProcedure;
				command.Parameters.Add(new EDBParameter("shortArray", EDBTypes.EDBDbType.SmallintArray));
				command.Prepare();
				command.Parameters[0].Value = shortArray ;
				Object result = command.ExecuteScalar();
				MessageBox.Show(result.ToString());
			}
			finally
			{
				conn.Close();
			}
		}
		static void SPWithIntArray() 
		{
			EDBConnection conn = new EDBConnection("Server=10.90.1.63;Port=5444;User Id=postgres;Password=;Database=hello");
			conn.Open();
			try
			{
				int[] intArray = new int[]{10,20,30,70};
				EDBCommand command = new EDBCommand("test(:intArray)", conn);
				command.CommandType = CommandType.StoredProcedure;
				command.Parameters.Add(new EDBParameter("intArray", EDBTypes.EDBDbType.IntegerArray));
				command.Prepare();
				command.Parameters[0].Value = intArray ;
				Object result = command.ExecuteScalar();
				MessageBox.Show(result.ToString());
			}
			finally
			{
				conn.Close();
			}
		}
		static void SPWithLongArray() 
		{
			EDBConnection conn = new EDBConnection("Server=127.0.0.1;Port=5432;User Id=postgres;Password=yespass;Database=TestDB;SSL=True");
			conn.Open();
			try
			{
				long[] longArray = new long[]{100000,20,30};
				EDBCommand command = new EDBCommand("testforlong(:longArray)", conn);
				command.CommandType = CommandType.StoredProcedure;
				command.Parameters.Add(new EDBParameter("longArray", EDBTypes.EDBDbType.LongArray));
				command.Prepare();
				command.Parameters[0].Value = longArray ;
				Object result = command.ExecuteScalar();
				MessageBox.Show(result.ToString());
			}
			finally
			{
				conn.Close();
			}
		}
		static void SPWithFloatArray() 
		{
			EDBConnection conn = new EDBConnection("Server=127.0.0.1;Port=5432;User Id=postgres;Password=yespass;Database=TestDB;SSL=True");
			conn.Open();
			try
			{
				float[] floatArray = new float[]{10.5f,20.1f,30.2f};
				EDBCommand command = new EDBCommand("testforfloat(:floatArray)", conn);
				command.CommandType = CommandType.StoredProcedure;
				command.Parameters.Add(new EDBParameter("floatArray", EDBTypes.EDBDbType.FloatArray));
				command.Prepare();
				command.Parameters[0].Value = floatArray ;
				Object result = command.ExecuteScalar();
				MessageBox.Show(result.ToString());
			}
			finally
			{
				conn.Close();
			}
		}
		static void SPWithDoubleArray() 
		{
			EDBConnection conn = new EDBConnection("Server=127.0.0.1;Port=5432;User Id=postgres;Password=yespass;Database=TestDB;SSL=True");
			conn.Open();
			try
			{
				double[] doubleArray = new double[]{10000.555,20.444,30};
				EDBCommand command = new EDBCommand("testfordouble(:doubleArray)", conn);
				command.CommandType = CommandType.StoredProcedure;
				command.Parameters.Add(new EDBParameter("doubleArray", EDBTypes.EDBDbType.DoubleArray));
				command.Prepare();
				command.Parameters[0].Value = doubleArray ;
				Object result = command.ExecuteScalar();
				MessageBox.Show(result.ToString());
			}
			finally
			{
				conn.Close();
			}
		}
		static void SPWithStringArray() 
		{
			EDBConnection conn = new EDBConnection("Server=127.0.0.1;Port=5432;User Id=postgres;Password=yespass;Database=TestDB;SSL=True");
			conn.Open();
			try
			{
				string[] strArray= new string[]{"OneandonlyOne","Two","Three","Four","Five"};
				EDBCommand command = new EDBCommand("testforstr(:strArray)", conn);
				command.CommandType = CommandType.StoredProcedure;
				command.Parameters.Add(new EDBParameter("strArray", EDBTypes.EDBDbType.StringArray));
				command.Prepare();
				command.Parameters[0].Value = strArray ;
				Object result = command.ExecuteScalar();
				MessageBox.Show(result.ToString());
			}
			finally
			{
				conn.Close();
			}
		}
		static void Plpgsqlfunc() 
		{
			EDBConnection conn = new EDBConnection("Server=10.90.1.18;Port=5444;User Id=postgres;Password=;Database=test");
			conn.Open();
			try
			{
				//				char[] charArray= new char[]{'s','a','f','i'};
				//				EDBCommand command = new EDBCommand("testforchar(:charArray)", conn);
				//				command.CommandType = CommandType.StoredProcedure;
				//				command.Parameters.Add(new EDBParameter("charArray",EDBTypes.EDBDbType.CharArray));
				//				command.Prepare();
				//				command.Parameters[0].Value = charArray ;
				//				Object result = command.ExecuteScalar();
				//				MessageBox.Show(result.ToString());



				
				EDBCommand command = new EDBCommand("dotNettest_1(1)", conn);
				command.CommandType = CommandType.StoredProcedure;
				//command.Parameters.Add(new EDBParameter("charArray",EDBTypes.EDBDbType.CharArray));
				//command.Prepare();
				//command.Parameters[0].Value = charArray ;
				Object result = command.ExecuteScalar();
				MessageBox.Show(result.ToString());



			}
			finally
			{
				conn.Close();
			}
		}
		static void SimpleQueries() 
		{
						
			EDBConnection conn = new EDBConnection("Server=10.90.1.63;Port=5444;User Id=postgres;Password=;Database=");
			conn.Open();
			try
			{	
				EDBCommand command = new EDBCommand(" insert into test values('safi')", conn);
				Int32 rowsaffected;
				rowsaffected = command.ExecuteNonQuery();
				MessageBox.Show(string.Format("It was added {0} lines in table test", rowsaffected));

				//			EDBCommand command = new EDBCommand("select version()", conn);
				//			String serverversion;
				//			serverversion = (String)command.ExecuteScalar();
				//			MessageBox.Show(string.Format("PostgreSQL server version: {0}", serverversion));
				//			EDBCommand command = new EDBCommand("select * from test", conn);
				//			EDBDataReader dr = command.ExecuteReader();
				//			while(dr.Read())
				//			{
				//				for (int i = 0; i < dr.FieldCount; i++)
				//				{
				//					MessageBox.Show(string.Format("{0}", dr[i]));
				//				}
				//			}
			}
			finally 
			{
				conn.Close();
			}
		}
		void PreparedOne() 
		{
			EDBConnection conn = new EDBConnection("Server=10.90.1.63;Port=5444;User Id=postgres;Password=;Database=hello");
			conn.Open();
			
			// Declare the parameter in the query string
			//	EDBCommand command = new EDBCommand(";select * from test where name = :name ", conn);
			EDBCommand command = new EDBCommand("update  test set a = 'kamran' where a= 'zahid' ", conn);
			//EDBCommand command = new EDBCommand(";select * from test ", conn);
			command.ExecuteNonQuery();
			// Now add the parameter to the parameter collection of the command specifying its type.
			//command.Parameters.Add(new EDBParameter("name", DbType.Int32));
			// Now, prepare the statement.
			//command.Prepare();
			// Now, add a value to it and later execute the command as usual.
			//command.Parameters[0].Value = "xyz";
			
			try
			{
				EDBDataReader dr = command.ExecuteReader();
				while(dr.Read())
				{
					for (int i = 0; i < dr.FieldCount; i++)
					{
						MessageBox.Show(dr[i].ToString());
					}
					
				}

			}
    
			finally
			{
				conn.Close();
			}
		
		}

		private void Form1_Load(object sender, System.EventArgs e)
		{
		
		}

		private void button2_Click(object sender, System.EventArgs e)
		{
			SimpleQueries() ;
		
		}

		private void button3_Click(object sender, System.EventArgs e)
		{
			Plpgsqlfunc() ;
		}

		private void button5_Click(object sender, System.EventArgs e)
		{
				for (int i =0 ;i<50 ;i++)
			 jdbc_test_1();
		
		}

		private void button4_Click(object sender, System.EventArgs e)
		{
			
				
			//jdbc_test_1();
			
			//			catch (Exception ex)
			//			{
			//				Console.WriteLine("Exception Occured");
			//				Console.WriteLine(ex.StackTrace);
			//				//conn.Close();
			//			}
			
		}

		private void button4_Click_1(object sender, System.EventArgs e)
		{
			for (int i =0 ;i<50 ;i++)
				test();
		}
		////////////////////////

		private void button4_Click_2(object sender, System.EventArgs e)
		{
			
						
			EDBConnection conn = new EDBConnection("Server=10.90.1.18;Port=5444;User Id=postgres;Password=;Database=testdb");
			conn.Open();
		
			try
			{
			
			
			
				EDBCommand command = new EDBCommand("jdbc_test_10(:param1) ", conn);
				command.CommandType = CommandType.StoredProcedure;
				command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Integer,10,"param1",ParameterDirection.Output,false ,2,2,System.Data.DataRowVersion.Current,1));
				command.Parameters.Add(new EDBParameter("param3", EDBTypes.EDBDbType.Integer,10,"param3",ParameterDirection.ReturnValue,false ,2,2,System.Data.DataRowVersion.Current,1));
				
				command.Prepare();
				
				command.Parameters[0].Value = null;
				
				EDBDataReader  result = command.ExecuteReader();
				int fc=result.FieldCount;
				//MessageBox.Show("RESULT Count="+fc);
				while(result.Read())
				{
					for(int i=0;i<fc;i++)
						Console.WriteLine("RESULT["+i+"]="+ Convert.ToString(command.Parameters[i].Value));
					
				}
			}

			finally 
			{
				conn.Close();
			
			
			}
				
			
	
		}

		private void button6_Click(object sender, System.EventArgs e)
		{
			
			for(int j= 0 ;j<10; j++){
								  
			
									  EDBConnection conn = new EDBConnection("Server=10.90.1.18;Port=5444;User Id=postgres;Password=;Database=testdb");
									  conn.Open();
									  try
									  {
										  EDBCommand command = new EDBCommand("jdbc_test_11(:param1,:param2,:param3)", conn);
										  command.CommandType = CommandType.StoredProcedure;
				
										  command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Integer,10,"param1",ParameterDirection.Output,false ,2,2,System.Data.DataRowVersion.Current,1));
										  command.Parameters.Add(new EDBParameter("param2", EDBTypes.EDBDbType.Integer,10,"param2",ParameterDirection.Output,false ,2,2,System.Data.DataRowVersion.Current,1));
										  command.Parameters.Add(new EDBParameter("param3", EDBTypes.EDBDbType.Integer,10,"param3",ParameterDirection.Output,false ,2,2,System.Data.DataRowVersion.Current,1));
										  command.Parameters.Add(new EDBParameter("param4", EDBTypes.EDBDbType.Text,10,"param4",ParameterDirection.ReturnValue,false ,2,2,System.Data.DataRowVersion.Current,1));
				
				
										  command.Prepare();
				
										  command.Parameters[0].Value = null;
										  command.Parameters[1].Value = null;
										  command.Parameters[2].Value = null;
				
										  EDBDataReader  result = command.ExecuteReader();
										  int fc=result.FieldCount;
				
										  while(result.Read())
										  {
											  for(int i=0;i<fc;i++)
												  Console.WriteLine("RESULT["+i+"]="+ Convert.ToString(command.Parameters[i].Value));
				
										  }
									  }
									  finally 
									  {
										  conn.Close();
			
									  }
								  }
		}

		private void button7_Click(object sender, System.EventArgs e)
		{
			EDBConnection conn = new EDBConnection("Server=10.90.1.18;Port=5444;User Id=postgres;Password=;Database=testdb");
			conn.Open();
			try
			{  //jdbc_test_6(a IN int4, b OUT int4,c OUT int4,d OUT int4, e IN OUT int4, f IN OUT int4,g IN OUT int4, h IN OUT int4)

				EDBCommand command = new EDBCommand("jdbc_test_6(:param1,:param2,:param3,:param4,:param5,:param6:,:param7,param8)", conn);
				command.CommandType = CommandType.StoredProcedure;
				
				command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Integer,10,"param1",ParameterDirection.Input,false ,2,2,System.Data.DataRowVersion.Current,1));
				command.Parameters.Add(new EDBParameter("param2", EDBTypes.EDBDbType.Integer,10,"param2",ParameterDirection.Output,false ,2,2,System.Data.DataRowVersion.Current,1));
				command.Parameters.Add(new EDBParameter("param3", EDBTypes.EDBDbType.Integer,10,"param3",ParameterDirection.Output,false ,2,2,System.Data.DataRowVersion.Current,1));
				command.Parameters.Add(new EDBParameter("param4", EDBTypes.EDBDbType.Integer,10,"param4",ParameterDirection.Output,false ,2,2,System.Data.DataRowVersion.Current,1));
				command.Parameters.Add(new EDBParameter("param5", EDBTypes.EDBDbType.Integer,10,"param5",ParameterDirection.InputOutput,false ,2,2,System.Data.DataRowVersion.Current,1));
				command.Parameters.Add(new EDBParameter("param6", EDBTypes.EDBDbType.Integer,10,"param6",ParameterDirection.InputOutput,false ,2,2,System.Data.DataRowVersion.Current,1));
				command.Parameters.Add(new EDBParameter("param7", EDBTypes.EDBDbType.Integer,10,"param7",ParameterDirection.InputOutput,false ,2,2,System.Data.DataRowVersion.Current,1));
				command.Parameters.Add(new EDBParameter("param8", EDBTypes.EDBDbType.Integer,10,"param8",ParameterDirection.InputOutput,false ,2,2,System.Data.DataRowVersion.Current,1));
				
				command.Prepare();
				
				command.Parameters[0].Value = 10;
				command.Parameters[1].Value = null;
				command.Parameters[2].Value = null;
				command.Parameters[3].Value = null;
				command.Parameters[4].Value = 10;
				command.Parameters[5].Value = 10;
				command.Parameters[6].Value = 10;
				command.Parameters[7].Value = 10;
			
				
				
				




				EDBDataReader  result = command.ExecuteReader();
				int fc=result.FieldCount;
				
				while(result.Read())
				{
					for(int i=0;i<fc;i++)
						Console.WriteLine("RESULT["+i+"]="+ Convert.ToString(command.Parameters[i].Value));
				
				}
			}
			finally 
			{
				conn.Close();
			
			}
		}

		private void button8_Click(object sender, System.EventArgs e)
		{
			EDBConnection conn = new EDBConnection("Server=10.90.1.63;Port=5444;User Id=postgres;Password=;Database=hello");
			conn.Open();
			try
			{ 

				EDBCommand command = new EDBCommand("IN_OUT_FUNC(:param1,:param2,:param3,:param4)", conn);
				command.CommandType = CommandType.StoredProcedure;
				
				command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Integer,10,"param1",ParameterDirection.Input,false ,2,2,System.Data.DataRowVersion.Current,1));
				command.Parameters.Add(new EDBParameter("param2", EDBTypes.EDBDbType.Integer,10,"param2",ParameterDirection.Output,false ,2,2,System.Data.DataRowVersion.Current,1));
				command.Parameters.Add(new EDBParameter("param3", EDBTypes.EDBDbType.Integer,10,"param3",ParameterDirection.Input,false ,2,2,System.Data.DataRowVersion.Current,1));
				command.Parameters.Add(new EDBParameter("param4", EDBTypes.EDBDbType.Integer,10,"param4",ParameterDirection.Output,false ,2,2,System.Data.DataRowVersion.Current,1));
				command.Parameters.Add(new EDBParameter("param5", EDBTypes.EDBDbType.Integer,10,"param5",ParameterDirection.ReturnValue,false ,2,2,System.Data.DataRowVersion.Current,1));

				command.Prepare();
				
				command.Parameters[0].Value = 1;
				command.Parameters[1].Value = null;
				command.Parameters[2].Value = 3;
				command.Parameters[3].Value = null;
				command.ExecuteNonQuery();
				
				//EDBDataReader  result = command.ExecuteReader();
				Console.WriteLine("RESULT[4]="+ Convert.ToString(command.Parameters[0].Value));
				Console.WriteLine("RESULT[4]="+ Convert.ToString(command.Parameters[1].Value));
				Console.WriteLine("RESULT[4]="+ Convert.ToString(command.Parameters[2].Value));
				Console.WriteLine("RESULT[4]="+ Convert.ToString(command.Parameters[3].Value));
				Console.WriteLine("RESULT[4]="+ Convert.ToString(command.Parameters[4].Value));
				//int fc=result.FieldCount;
////				
				//while(result.Read())
				//{
				//	for(int i=0;i<fc;i++)
				//		Console.WriteLine("RESULT["+i+"]="+ Convert.ToString(command.Parameters[i].Value));
				
				//}
			}
			catch(EDBException exp)
			{
				Console.WriteLine(exp.Message); 
			}
			finally 
			{
				conn.Close();
			
			}



//////				EDBCommand command = new EDBCommand("Dotnet_IN_OUT_FUNC(:param1,:param2,:param3,:param4)", conn);
//////				command.CommandType = CommandType.StoredProcedure;
//////				
//////				command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Integer,10,"param1",ParameterDirection.Input,false ,2,2,System.Data.DataRowVersion.Current,1));
//////				command.Parameters.Add(new EDBParameter("param2", EDBTypes.EDBDbType.Integer,10,"param2",ParameterDirection.Output,false ,2,2,System.Data.DataRowVersion.Current,1));
//////				command.Parameters.Add(new EDBParameter("param3", EDBTypes.EDBDbType.Integer,10,"param3",ParameterDirection.Input,false ,2,2,System.Data.DataRowVersion.Current,1));
//////				command.Parameters.Add(new EDBParameter("param4", EDBTypes.EDBDbType.Integer,10,"param4",ParameterDirection.Output,false ,2,2,System.Data.DataRowVersion.Current,1));
//////				command.Parameters.Add(new EDBParameter("param5", EDBTypes.EDBDbType.Text,10,"param5",ParameterDirection.ReturnValue,false ,2,2,System.Data.DataRowVersion.Current,1));
//////								
//////				command.Prepare();
//////				
//////				command.Parameters[0].Value = 10;
//////				command.Parameters[1].Value = null;
//////				command.Parameters[2].Value = 10;
//////				command.Parameters[3].Value = null;
//////				
//////				EDBDataReader  result = command.ExecuteReader();
//////				int fc=result.FieldCount;
//////				
//////				while(result.Read())
//////				{
//////					for(int i=0;i<fc;i++)
//////						Console.WriteLine("RESULT["+i+"]="+ Convert.ToString(command.Parameters[i].Value));
//////				
//////				}




			
		}

		private void button9_Click(object sender, System.EventArgs e)
		{
			EDBConnection conn = new EDBConnection("Server=10.90.1.63;Port=5444;User Id=postgres;Password=;Database=hello");
			conn.Open();
			try
			{  // jdbc_test_5(a IN int4,b OUT int4,c IN int2, d OUT int2) 


				EDBCommand command = new EDBCommand("fl_chk(:param1,param2)", conn);
				command.CommandType = CommandType.StoredProcedure;
				
				command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Integer,10,"param1",ParameterDirection.Output,false ,2,2,System.Data.DataRowVersion.Current,1));
				command.Parameters.Add(new EDBParameter("param2", EDBTypes.EDBDbType.Text,10,"param2",ParameterDirection.Output,false ,2,2,System.Data.DataRowVersion.Current,1));
				
				command.Prepare();
				
				command.Parameters[0].Value = null;
				command.Parameters[1].Value = null;
				
				EDBDataReader  result = command.ExecuteReader();
				int fc=result.FieldCount;
				
				while(result.Read())
				{
					for(int i=0;i<fc;i++)
						Console.WriteLine("RESULT["+i+"]="+ Convert.ToString(command.Parameters[i].Value));
				
				}
			}
			finally 
			{
				conn.Close();
			
			}
		}

		private void button10_Click(object sender, System.EventArgs e)
		{
//			EDBConnection conn = new EDBConnection("Server=10.90.1.18;Port=5444;User Id=postgres;Password=;Database=test");
//			conn.Open();
//			try
//			{  //jdbc_test_4b(a OUT int4,b IN int2,c IN OUT int2, d OUT VARCHAR, e IN OUT float4) AS
//
//
//				EDBCommand command = new EDBCommand("jdbc_test_4b(:param1,:param2,:param3,:param4,:param5)", conn);
//				command.CommandType = CommandType.StoredProcedure;
//				
//				command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Integer,10,"param1",ParameterDirection.Output,false ,2,2,System.Data.DataRowVersion.Current,1));
//				command.Parameters.Add(new EDBParameter("param2", EDBTypes.EDBDbType.Integer,10,"param2",ParameterDirection.Input,false ,2,2,System.Data.DataRowVersion.Current,1));
//				command.Parameters.Add(new EDBParameter("param3", EDBTypes.EDBDbType.Integer,10,"param3",ParameterDirection.InputOutput,false ,2,2,System.Data.DataRowVersion.Current,1));
//				command.Parameters.Add(new EDBParameter("param4", EDBTypes.EDBDbType.Text,10,"param4",ParameterDirection.Output,false ,2,2,System.Data.DataRowVersion.Current,1));
//				command.Parameters.Add(new EDBParameter("param5", System.Data.SqlDbType.Float,10,"param5",ParameterDirection.InputOutput,false ,2,2,System.Data.DataRowVersion.Current,1));
//				command.Prepare();
//				
//				command.Parameters[0].Value = null;
//				command.Parameters[1].Value = 10;
//				command.Parameters[2].Value = 10;
//				command.Parameters[3].Value = null;
//				command.Parameters[4].Value = 100;
//
//				EDBDataReader  result = command.ExecuteReader();
//				int fc=result.FieldCount;
//				
//				while(result.Read())
//				{
//					for(int i=0;i<fc;i++)
//						Console.WriteLine("RESULT["+i+"]="+ Convert.ToString(command.Parameters[i].Value));
//				
//				}
//			}
//			finally 
//			{
//				conn.Close();
//			
//			}
		}

		private void button11_Click(object sender, System.EventArgs e)
		{
			EDBConnection conn = new EDBConnection("Server=10.90.1.18;Port=5444;User Id=postgres;Password=;Database=testdb");
			conn.Open();
			try
			{  // jdbc_test_3(a OUT int4,b OUT int4) AS



				EDBCommand command = new EDBCommand("jdbc_test_3(:param1,:param2)", conn);
				command.CommandType = CommandType.StoredProcedure;
				
				command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Integer,10,"param1",ParameterDirection.Output,false ,2,2,System.Data.DataRowVersion.Current,1));
				command.Parameters.Add(new EDBParameter("param2", EDBTypes.EDBDbType.Integer,10,"param2",ParameterDirection.Output,false ,2,2,System.Data.DataRowVersion.Current,1));
				
				command.Prepare();
				
				command.Parameters[0].Value = -1;
				command.Parameters[1].Value = 20;
				
				EDBDataReader  result = command.ExecuteReader();
				int fc=result.FieldCount;
				
				while(result.Read())
				{
					MessageBox.Show(result.GetValue(0).ToString());
					for(int i=0;i<fc;i++)
						Console.WriteLine("RESULT["+i+"]="+ Convert.ToString(command.Parameters[i].Value));
				
				}
			}
			finally 
			{
				conn.Close();
			
			}
		}

		private void button12_Click(object sender, System.EventArgs e)
		{
			EDBConnection conn = new EDBConnection("Server=10.90.1.18;Port=5444;User Id=postgres;Password=;Database=testdb");
			conn.Open();
			try
			{  // jdbc_test_2(a IN int4,b IN int4) AS




				EDBCommand command = new EDBCommand("jdbc_test_2(:param1,:param2)", conn);
				command.CommandType = CommandType.StoredProcedure;
				
				command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Integer,10,"param1",ParameterDirection.Input,false ,2,2,System.Data.DataRowVersion.Current,1));
				command.Parameters.Add(new EDBParameter("param2", EDBTypes.EDBDbType.Integer,10,"param2",ParameterDirection.Input,false ,2,2,System.Data.DataRowVersion.Current,1));
				
				command.Prepare();
				
				command.Parameters[0].Value = 10;
				command.Parameters[1].Value = 10;
				
				EDBDataReader  result = command.ExecuteReader();
				int fc=result.FieldCount;
				
				while(result.Read())
				{
					for(int i=0;i<fc;i++)
						Console.WriteLine("RESULT["+i+"]="+ Convert.ToString(command.Parameters[i].Value));
				
				}
			}
			finally 
			{
				conn.Close();
			
			}
		}

		private void button13_Click(object sender, System.EventArgs e)
		{
			EDBConnection conn = new EDBConnection("Server=10.90.1.18;Port=5444;User Id=postgres;Password=;Database=testdb");
			conn.Open();
			try
			{  // jdbc_test_1(a IN OUT int4,b IN OUT int4, c IN OUT varchar) AS

				EDBCommand command = new EDBCommand("jdbc_test_1(:param1,:param2)", conn);
				command.CommandType = CommandType.StoredProcedure;
				
				command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Integer,10,"param1",ParameterDirection.Input,false ,2,2,System.Data.DataRowVersion.Current,1));
				command.Parameters.Add(new EDBParameter("param2", EDBTypes.EDBDbType.Integer,10,"param2",ParameterDirection.InputOutput,false ,2,2,System.Data.DataRowVersion.Current,1));
				command.Parameters.Add(new EDBParameter("param3", EDBTypes.EDBDbType.Text,10,"param3",ParameterDirection.InputOutput,false ,2,2,System.Data.DataRowVersion.Current,1));
				command.Prepare();
				
				command.Parameters[0].Value = null;
				command.Parameters[1].Value = 10;
				command.Parameters[2].Value = "hello";

				EDBDataReader  result = command.ExecuteReader();
				int fc=result.FieldCount;
				
				while(result.Read())
				{
					for(int i=0;i<fc;i++)
						Console.WriteLine("RESULT["+i+"]="+ Convert.ToString(command.Parameters[i].Value));
				
				}
			}
			finally 
			{
				conn.Close();
			
			}
		}

		private void button14_Click(object sender, System.EventArgs e)
		{
			EDBConnection conn = new EDBConnection("Server=10.90.1.18;Port=5444;User Id=postgres;Password=;Database=test");
			conn.Open();
			try
			{  // caller in int4

				EDBCommand command = new EDBCommand("caller(:param1)", conn);
				command.CommandType = CommandType.StoredProcedure;
				
				command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Integer,10,"param1",ParameterDirection.Input,false ,2,2,System.Data.DataRowVersion.Current,1));
				
				
				command.Prepare();
				
				command.Parameters[0].Value = 10;
				
				

				EDBDataReader  result = command.ExecuteReader();
				int fc=result.FieldCount;
				
				while(result.Read())
				{
					for(int i=0;i<fc;i++)
						Console.WriteLine("RESULT["+i+"]="+ Convert.ToString(command.Parameters[i].Value));
				
				}
			}
			finally 
			{
				conn.Close();
			
			}
		
		}

		private void button15_Click(object sender, System.EventArgs e)
		{


			for (int j = 0 ;j<10 ;j++)
			{
				EDBConnection conn = new EDBConnection("Server=10.90.1.18;Port=5444;User Id=postgres;Password=;Database=testdb");
				conn.Open();
				try
				{  // callee inout int4

					EDBCommand command = new EDBCommand("callee(:param1)", conn);
					command.CommandType = CommandType.StoredProcedure;
				
					command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Integer,10,"param1",ParameterDirection.InputOutput,false ,2,2,System.Data.DataRowVersion.Current,1));
					command.Prepare();
					command.Parameters[0].Value = 10;
				
					EDBDataReader  result = command.ExecuteReader();
					int fc=result.FieldCount;
				
					while(result.Read())
					{
						for(int i=0;i<fc;i++)
							Console.WriteLine("RESULT["+i+"]="+ Convert.ToString(command.Parameters[i].Value));
				
					}
				}
				finally 
				{
					conn.Close();
			
				}
			}
		}

		private void button16_Click(object sender, System.EventArgs e)
		{
			//newcallee
			for (int i = 0;i<10 ;i++)
			FunxCall(); 
		}

		private void button17_Click(object sender, System.EventArgs e)
		{
		
			EDBConnection conn = new EDBConnection("Server=10.90.1.18;Port=5444;User Id=postgres;Password=;Database=testdb");
			conn.Open();
			try
			{  //pgfunc

				EDBCommand command = new EDBCommand("pgfunc(:param1)", conn);
				command.CommandType = CommandType.StoredProcedure;
				
				command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Integer,10,"param1",ParameterDirection.Input,false ,2,2,System.Data.DataRowVersion.Current,1));
				//command.Parameters.Add(new EDBParameter("param2", EDBTypes.EDBDbType.Integer,10,"param1",ParameterDirection.Input,false ,2,2,System.Data.DataRowVersion.Current,1));
				command.Prepare();
				command.Parameters[0].Value = 10;
				
				EDBDataReader  result = command.ExecuteReader();	
				int fc=result.FieldCount;
				
				while(result.Read())
				{
					for(int i=0;i<fc;i++)
						Console.WriteLine("RESULT["+i+"]="+ Convert.ToString(command.Parameters[i].Value));
				
				}
			}
			finally 
			{
				conn.Close();
			
			}



		}

		private void button1_Click_1(object sender, System.EventArgs e)
		{
			//EMP_SELECT
			EDBConnection conn = new EDBConnection("Server=10.90.1.63;Port=5444;User Id=postgres;Password=;Database=hello");
			
			if (conn.State == ConnectionState.Closed) 
				conn.Open();

			try
			{
				
								
				EDBCommand command = new EDBCommand("EMP_SELECT(:empNo,:name,:job,:sal,:comm)", conn);
				command.CommandType = CommandType.StoredProcedure;
					
				command.Parameters.Add(new EDBParameter("empNo", EDBTypes.EDBDbType.Integer,10,"empNo",ParameterDirection.InputOutput,false ,2,2,System.Data.DataRowVersion.Current,1));
				command.Parameters.Add(new EDBParameter("name", EDBTypes.EDBDbType.Varchar,10,"name",ParameterDirection.Output,false ,2,2,System.Data.DataRowVersion.Current,1));
				command.Parameters.Add(new EDBParameter("job", EDBTypes.EDBDbType.Varchar,10,"job",ParameterDirection.Output,false ,2,2,System.Data.DataRowVersion.Current,1));
				command.Parameters.Add(new EDBParameter("sal", EDBTypes.EDBDbType.Float,10,"sal",ParameterDirection.Output,false ,2,2,System.Data.DataRowVersion.Current,1));
				command.Parameters.Add(new EDBParameter("comm", EDBTypes.EDBDbType.Float,200,"comm",ParameterDirection.Output,false ,2,2,System.Data.DataRowVersion.Current,1));
			
//			

				command.Prepare();
					
				command.Parameters[0].Value = 7369;
					
				EDBDataReader result = command.ExecuteReader();								
				int fc=result.FieldCount;
					
				while(result.Read())
				{
					for(int i=0;i<fc;i++)
						Console.WriteLine("RESULT["+i+"]="+ Convert.ToString(command.Parameters[i].Value));
						
				}
			}
			catch(EDBException exp)
			{
				MessageBox.Show(exp.ToString()); 
			}
			finally
			{
				try
				{
					conn.Close();
				}
				catch (Exception exp)
				{
					MessageBox.Show("EXCEPTION OCCURED"); 
				}
			}

		}

		private void button3_Click_1(object sender, System.EventArgs e)
		{
			EDBConnection conn = new EDBConnection("Server=10.90.1.63;Port=5444;User Id=postgres;Password=;Database=hello");
			conn.Open();

			try
			{
				//pDEPTNO IN INTEGER,pDNAME OUT VARCHAR2,pLOC OUT VARCHAR2
							
				EDBCommand command = new EDBCommand("DEPT_SELECT(:pDEPTNO,:pDNAME,:pLOC)", conn);
				command.CommandType = CommandType.StoredProcedure;
					
				command.Parameters.Add(new EDBParameter("pDEPTNO", EDBTypes.EDBDbType.Integer,10,"empNo",ParameterDirection.Input,false ,2,2,System.Data.DataRowVersion.Current,1));
				command.Parameters.Add(new EDBParameter("pDNAME", EDBTypes.EDBDbType.Varchar,10,"name",ParameterDirection.Output,false ,2,2,System.Data.DataRowVersion.Current,1));
				command.Parameters.Add(new EDBParameter("pLOC", EDBTypes.EDBDbType.Varchar,10,"job",ParameterDirection.Output,false ,2,2,System.Data.DataRowVersion.Current,1));
				
				command.Prepare();
					
				command.Parameters[0].Value = 10;
				
				command.ExecuteNonQuery();
	
				Console.WriteLine(command.Parameters["pDNAME"].Value.ToString());
				Console.WriteLine(command.Parameters["pLOC"].Value.ToString());		
				

			}

			catch(EDBException exp)
			{
				MessageBox.Show(exp.ToString()); 
			}
			finally
			{
				conn.Close();
			}

		}

		private void button5_Click_1(object sender, System.EventArgs e)
		{
			EDBConnection conn = new EDBConnection("Server=10.90.1.63;Port=5444;User Id=postgres;Password=;Database=hello");
			conn.Open();

			try
			{
				
				EDBCommand command = new EDBCommand("EMP_INSERT1(:pEMPNO,:pENAME,:pJOB,:pSAL,:pCOMM)", conn);
				command.CommandType = CommandType.StoredProcedure;
					
				command.Parameters.Add(new EDBParameter("pEMPNO", EDBTypes.EDBDbType.Integer,10,"pEMPNO",ParameterDirection.Output,false ,2,2,System.Data.DataRowVersion.Current,1));
				command.Parameters.Add(new EDBParameter("pENAME", EDBTypes.EDBDbType.Varchar,10,"pENAME",ParameterDirection.Input,false ,2,2,System.Data.DataRowVersion.Current,1));
				command.Parameters.Add(new EDBParameter("pJOB", EDBTypes.EDBDbType.Varchar,10,"pJOB",ParameterDirection.Input,false ,2,2,System.Data.DataRowVersion.Current,1));
				command.Parameters.Add(new EDBParameter("pSAL", EDBTypes.EDBDbType.Float,10,"pSAL",ParameterDirection.Input,false ,2,2,System.Data.DataRowVersion.Current,1));
				command.Parameters.Add(new EDBParameter("pCOMM", EDBTypes.EDBDbType.Float,10,"pCOMM",ParameterDirection.Input,false ,2,2,System.Data.DataRowVersion.Current,1));
			

				command.Prepare();
					
				command.Parameters[0].Value = null;
				command.Parameters[1].Value = "Tom";
				command.Parameters[2].Value = "Anayst";
				command.Parameters[3].Value = 1000;
				command.Parameters[4].Value = 90;
				command.ExecuteScalar();
				Console.WriteLine(command.Parameters["pEMPNO"].Value.ToString());					

			}

			catch(EDBException exp)
			{
				Console.WriteLine(exp.ToString()); 
			}
			finally
			{
				conn.Close();
			}

		}

		private void btnEmpUpdate_Click(object sender, System.EventArgs e)
		{			
			EDBConnection conn = new EDBConnection("Server=10.90.1.63;Port=5444;User Id=postgres;Password=;Database=hello");
			      
			try
			{				
 					
				string updateQuery  = "update emp set ename = :Name where empno = :ID";
				conn.Open(); 
				EDBCommand command = new EDBCommand(updateQuery, conn);
				command.CommandType = CommandType.Text;

				command.Parameters.Add(new EDBParameter("ID", EDBTypes.EDBDbType.Integer));
				command.Parameters.Add(new EDBParameter("Name", EDBTypes.EDBDbType.Text));

				command.Prepare();

				command.Parameters[0].Value = 7369;
				command.Parameters[1].Value = "Mark";				

				command.ExecuteNonQuery();
				MessageBox.Show("Record Updated");
				 
			}
			catch(Exception exp)
			{
				MessageBox.Show(exp.ToString()); 
			}    
			finally
			{
				conn.Close();
			}
		}

		private void button14_Click_1(object sender, System.EventArgs e)
		{
						
			EDBConnection conn = new EDBConnection("Server=10.90.1.63;Port=5444;User Id=postgres;Password=;Database=");
			conn.Open();
			try
			{	
				EDBCommand command = new EDBCommand(" select * from emp')", conn);
				
				//command.Parameters[1].Value = null;
				
				EDBDataReader  result = command.ExecuteReader();
				int fc=result.FieldCount;
				
				while(result.Read())
				{
					for(int i=0;i<fc;i++)
						Console.WriteLine("RESULT["+i+"]="+ Convert.ToString(command.Parameters[i].Value));
				
				}
			}
			finally 
			{
				conn.Close();
			
			}
		}

		private void btnUpdate_Click(object sender, System.EventArgs e)
		{
			EDBConnection conn = new EDBConnection("Server=10.90.1.63;Port=5444;User Id=postgres;Password=;Database=hello");
			      
			try
			{
				string updateQuery  = "update emp set ename = :Name where empno = :ID";
				conn.Open(); 
				EDBCommand command = new EDBCommand(updateQuery, conn);
				command.CommandType = CommandType.Text;

				command.Parameters.Add(new EDBParameter("ID", EDBTypes.EDBDbType.Integer));
				command.Parameters.Add(new EDBParameter("Name", EDBTypes.EDBDbType.Varchar));

				command.Prepare();

				command.Parameters[0].Value = 7369;
				command.Parameters[1].Value = "CLARA";				

				command.ExecuteNonQuery();
				MessageBox.Show("Record Updated");
			}
			catch(EDBException exp)
			{
				MessageBox.Show(exp.Message);
			}    
			finally
			{
				conn.Close();
			}
		}
		
	}

}

/////////////////////
///




