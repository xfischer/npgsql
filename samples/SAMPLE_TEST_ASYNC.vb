Imports System.Data
Imports EnterpriseDB.EDBClient

'
' This class provides an example to perform DML operation in EnterpriseDB Advanced Server using .NET async calls in VB.NET.
' @revision 1.0
'

Module Program
    Sub Main(args As String())
        MainAsync().Wait()
    End Sub

    Async Function MainAsync() As Task
        Dim conn = New EDBConnection("Server=localhost;Port=5444;User Id=enterprisedb;Password=password;Database=edb")
        Try
            Await conn.OpenAsync()

            'Simple select statement using EDBCommand object
            Dim EDBSeletCommand = New EDBCommand("SELECT EMPNO,ENAME,JOB,MGR,HIREDATE FROM EMP", conn)
            Dim SelectResult = Await EDBSeletCommand.ExecuteReaderAsync()

            While Await SelectResult.ReadAsync()
                Console.WriteLine("Emp No" & " " & SelectResult.GetInt32(0))
                Console.WriteLine("Emp Name" & " " & SelectResult.GetString(1))
                If SelectResult.IsDBNull(2) = False Then
                    Console.WriteLine("Job" & " " & SelectResult.GetString(2))
                Else
                    Console.WriteLine("Job" & " null ")
                End If
                If SelectResult.IsDBNull(3) = False Then
                    Console.WriteLine("Mgr" & " " & SelectResult.GetInt32(3))
                Else
                    Console.WriteLine("Mgr" & "null")
                End If
                If SelectResult.IsDBNull(4) = False Then
                    Console.WriteLine("Hire Date" & " " & SelectResult.GetDateTime(4))
                Else
                    Console.WriteLine("Hire Date" & " null")
                End If
                Console.WriteLine("---------------------------------")
            End While

            Await SelectResult.CloseAsync()

            'Insert statement using EDBCommand Object
            Dim EDBInsertCommand = New EDBCommand("INSERT INTO EMP(EMPNO,ENAME) VALUES((SELECT COUNT(EMPNO) FROM EMP),'JACKSON')", conn)
            Await EDBInsertCommand.ExecuteScalarAsync()
            Console.WriteLine("Record inserted")

            'Update  using EDBCommand Object
            Dim EDBUpdateCommand = New EDBCommand("UPDATE EMP SET ENAME ='DOTNET' WHERE EMPNO < 100", conn)
            Await EDBUpdateCommand.ExecuteNonQueryAsync()
            Console.WriteLine("Record has been updated")
            Dim EDBDeletCommand = New EDBCommand("DELETE FROM EMP WHERE EMPNO < 100", conn)
            EDBDeletCommand.CommandType = CommandType.Text
            Await EDBDeletCommand.ExecuteScalarAsync()
            Console.WriteLine("Record deleted")

            'procedure call example
            Try
                Dim callable_command = New EDBCommand("emp_query(:p_deptno,:p_empno,:p_ename,:p_job,:p_hiredate,:p_sal)", conn)
                callable_command.CommandType = CommandType.StoredProcedure
                callable_command.Parameters.Add(New EDBParameter("p_deptno", EDBTypes.EDBDbType.Numeric, 10, "p_deptno", ParameterDirection.Input, False, 2, 2, System.Data.DataRowVersion.Current, 20))
                callable_command.Parameters.Add(New EDBParameter("p_empno", EDBTypes.EDBDbType.Numeric, 10, "p_empno", ParameterDirection.InputOutput, False, 2, 2, System.Data.DataRowVersion.Current, 7369))
                callable_command.Parameters.Add(New EDBParameter("p_ename", EDBTypes.EDBDbType.Varchar, 10, "p_ename", ParameterDirection.InputOutput, False, 2, 2, System.Data.DataRowVersion.Current, "SMITH"))
                callable_command.Parameters.Add(New EDBParameter("p_job", EDBTypes.EDBDbType.Varchar, 10, "p_job", ParameterDirection.Output, False, 2, 2, System.Data.DataRowVersion.Current, Nothing))
                callable_command.Parameters.Add(New EDBParameter("p_hiredate", EDBTypes.EDBDbType.Date, 200, "p_hiredate", ParameterDirection.Output, False, 2, 2, System.Data.DataRowVersion.Current, Nothing))
                callable_command.Parameters.Add(New EDBParameter("p_sal", EDBTypes.EDBDbType.Numeric, 200, "p_sal", ParameterDirection.Output, False, 2, 2, System.Data.DataRowVersion.Current, Nothing))
                Await callable_command.PrepareAsync()
                callable_command.Parameters(0).Value = 20
                callable_command.Parameters(1).Value = 7369
                Dim result = Await callable_command.ExecuteReaderAsync()
                Dim fc = result.FieldCount
                For i As Integer = 0 To fc Step 1
                    Console.WriteLine("RESULT[" & i & "]=" & Convert.ToString(callable_command.Parameters(i).Value))
                Next
                Await result.CloseAsync()
            Catch exp As EDBException
                If exp.ErrorCode.Equals("01403") Then
                    Console.WriteLine("No data found")
                ElseIf exp.ErrorCode.Equals("01422") Then
                    Console.WriteLine("More than one rows were returned by the query")
                Else
                    Console.WriteLine("There was an error Calling the procedure. \nRoot Cause:\n")
                    Console.WriteLine(exp.Message.ToString())
                End If
            End Try

            'Prepared statement
            Dim updateQuery = "update emp set ename = :Name where empno = :ID"
            Dim Prepared_command = New EDBCommand(updateQuery, conn)
            Prepared_command.CommandType = CommandType.Text
            Prepared_command.Parameters.Add(New EDBParameter("ID", EDBTypes.EDBDbType.Integer))
            Prepared_command.Parameters.Add(New EDBParameter("Name", EDBTypes.EDBDbType.Text))
            Await Prepared_command.PrepareAsync()
            Prepared_command.Parameters(0).Value = 7369
            Prepared_command.Parameters(1).Value = "Mark"
            Await Prepared_command.ExecuteNonQueryAsync()
            Console.WriteLine("Record Updated...")

        Catch exp As EDBException
            Console.WriteLine(exp.ToString())
        End Try

        Await conn.CloseAsync()
    End Function
End Module
