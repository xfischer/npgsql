using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using EnterpriseDB.EDBClient;

namespace WinFormsTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private async void btnOpenAsync_Click(object sender, EventArgs e)
        {
            lblResult.Text = "Please wait... This test should not be hanging...";
            btnOpenAsync.Enabled = false;
            btnOpenSync.Enabled = false;
            Application.DoEvents();

            try
            {
                await RunSampleAsync(txtConString.Text);
            }
            finally
            {
                lblResult.Text = "Sample completed.";
                btnOpenAsync.Enabled = true;
                btnOpenSync.Enabled = true;
            }
        }

        private void btnOpenSync_Click(object sender, EventArgs e)
        {
            lblResult.Text = "Please wait... This test should not be hanging...";
            btnOpenAsync.Enabled = false;
            btnOpenSync.Enabled = false;
            Application.DoEvents();

            try
            {

                RunSample(txtConString.Text);
                lblResult.Text = "Sample completed.";
            }
            catch (Exception ex)
            {
                lblResult.Text = $"Error: {ex.Message}.";
            }
            finally
            {
                btnOpenAsync.Enabled = true;
                btnOpenSync.Enabled = true;
            }
        }

        void RunSample(string connectionString)
        {
            try
            {
                var dataSourceBuilder = new EDBDataSourceBuilder(connectionString);
                var dataSource = dataSourceBuilder.Build();
                {

                    using (var conn = dataSource.OpenConnection()) // HANGS
                    {

                        //2 selects batched, in order to test NextResult()
                        var EDBSelectCommand = new EDBCommand("SELECT EMPNO,ENAME,JOB,MGR,HIREDATE FROM EMP;SELECT EMPNO,ENAME,JOB,MGR,HIREDATE FROM EMP", conn);
                        using (var SelectResult = EDBSelectCommand.ExecuteReader())
                        {
                            do
                            {
                                while (SelectResult.Read())
                                {
                                    Console.WriteLine("Emp No" + " " + SelectResult.GetInt32(0));
                                    Console.WriteLine("Emp Name" + " " + SelectResult.GetString(1));
                                    if (SelectResult.IsDBNull(2) == false)
                                        Console.WriteLine("Job" + " " + SelectResult.GetString(2));
                                    else
                                        Console.WriteLine("Job" + " null ");
                                    if (SelectResult.IsDBNull(3) == false)
                                        Console.WriteLine("Mgr" + " " + SelectResult.GetInt32(3));
                                    else
                                        Console.WriteLine("Mgr" + "null");
                                    if (SelectResult.IsDBNull(4) == false)
                                        Console.WriteLine("Hire Date" + " " + SelectResult.GetDateTime(4));
                                    else
                                        Console.WriteLine("Hire Date" + " null");
                                    Console.WriteLine("---------------------------------");
                                }
                            }
                            while (SelectResult.NextResult());
                            SelectResult.Close();
                        }

                        //Close the connection
                        conn.Close();
                    }
                }
            }
            catch (EDBException exp)
            {
                Console.WriteLine(exp.ToString());
            }
        }

        async Task RunSampleAsync(string connectionString)
        {
            try
            {
                var dataSourceBuilder = new EDBDataSourceBuilder(connectionString);
                var dataSource = dataSourceBuilder.Build();
                {

                    using (var conn = await dataSource.OpenConnectionAsync()) // HANGS
                    {

                        //2 selects batched, in order to test NextResult()
                        var EDBSelectCommand = new EDBCommand("SELECT EMPNO,ENAME,JOB,MGR,HIREDATE FROM EMP;SELECT EMPNO,ENAME,JOB,MGR,HIREDATE FROM EMP", conn);
                        using (var SelectResult = await EDBSelectCommand.ExecuteReaderAsync())
                        {
                            do
                            {
                                while (await SelectResult.ReadAsync())
                                {
                                    Console.WriteLine("Emp No" + " " + SelectResult.GetInt32(0));
                                    Console.WriteLine("Emp Name" + " " + SelectResult.GetString(1));
                                    if (SelectResult.IsDBNull(2) == false)
                                        Console.WriteLine("Job" + " " + SelectResult.GetString(2));
                                    else
                                        Console.WriteLine("Job" + " null ");
                                    if (SelectResult.IsDBNull(3) == false)
                                        Console.WriteLine("Mgr" + " " + SelectResult.GetInt32(3));
                                    else
                                        Console.WriteLine("Mgr" + "null");
                                    if (SelectResult.IsDBNull(4) == false)
                                        Console.WriteLine("Hire Date" + " " + SelectResult.GetDateTime(4));
                                    else
                                        Console.WriteLine("Hire Date" + " null");
                                    Console.WriteLine("---------------------------------");
                                }
                            }
                            while (await SelectResult.NextResultAsync());
                            await SelectResult.CloseAsync();
                        }

                        //Close the connection
                        await conn.CloseAsync();
                    }
                }
            }
            catch (EDBException exp)
            {
                Console.WriteLine(exp.ToString());
            }
        }

    }
}
