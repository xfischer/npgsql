using namespace System;
using namespace System::Data;
using namespace EnterpriseDB::EDBClient;

using namespace System;

int main(array<System::String ^> ^args)
{
    auto conn = gcnew EDBConnection("Server=localhost;Port=5444;User Id=enterprisedb;Password=Fusion123;Database=edb");
    try
    {
        conn->Open();

        //Simple select statement using EDBCommand object
        auto EDBSeletCommand = gcnew EDBCommand("SELECT EMPNO,ENAME,JOB,MGR,HIREDATE FROM EMP", conn);
        auto SelectResult = EDBSeletCommand->ExecuteReader();
        while (SelectResult->Read())
        {
            Console::WriteLine("Emp No" + " " + SelectResult->GetInt32(0));
            Console::WriteLine("Emp Name" + " " + SelectResult->GetString(1));
            if (SelectResult->IsDBNull(2) == false)
                Console::WriteLine("Job" + " " + SelectResult->GetString(2));
            else
                Console::WriteLine("Job" + " null ");
            if (SelectResult->IsDBNull(3) == false)
                Console::WriteLine("Mgr" + " " + SelectResult->GetInt32(3));
            else
                Console::WriteLine("Mgr" + "null");
            if (SelectResult->IsDBNull(4) == false)
                Console::WriteLine("Hire Date" + " " + SelectResult->GetDateTime(4));
            else
                Console::WriteLine("Hire Date" + " null");
            Console::WriteLine("---------------------------------");
        }

        SelectResult->Close();

        //Insert statement using EDBCommand Object
        auto EDBInsertCommand = gcnew EDBCommand("INSERT INTO EMP(EMPNO,ENAME) VALUES((SELECT COUNT(EMPNO) FROM EMP),'JACKSON')", conn);
        EDBInsertCommand->ExecuteScalar();
        Console::WriteLine("Record inserted");

        //Update  using EDBCommand Object
        auto EDBUpdateCommand = gcnew EDBCommand("UPDATE EMP SET ENAME ='DOTNET' WHERE EMPNO < 100", conn);
        EDBUpdateCommand->ExecuteNonQuery();
        Console::WriteLine("Record has been updated");
        auto EDBDeletCommand = gcnew EDBCommand("DELETE FROM EMP WHERE EMPNO < 100", conn);
        EDBDeletCommand->CommandType = CommandType::Text;
        EDBDeletCommand->ExecuteScalar();
        Console::WriteLine("Record deleted");

        //procedure call example
        try
        {
            auto callable_command = gcnew EDBCommand("emp_query(:p_deptno,:p_empno,:p_ename,:p_job,:p_hiredate,:p_sal)", conn);
            callable_command->CommandType = CommandType::StoredProcedure;
            callable_command->Parameters->Add(gcnew EDBParameter("p_deptno", EDBTypes::EDBDbType::Numeric, 10, "p_deptno", ParameterDirection::Input, false, 2, 2, System::Data::DataRowVersion::Current, 20));
            callable_command->Parameters->Add(gcnew EDBParameter("p_empno", EDBTypes::EDBDbType::Numeric, 10, "p_empno", ParameterDirection::InputOutput, false, 2, 2, System::Data::DataRowVersion::Current, 7369));
            callable_command->Parameters->Add(gcnew EDBParameter("p_ename", EDBTypes::EDBDbType::Varchar, 10, "p_ename", ParameterDirection::InputOutput, false, 2, 2, System::Data::DataRowVersion::Current, "SMITH"));
            callable_command->Parameters->Add(gcnew EDBParameter("p_job", EDBTypes::EDBDbType::Varchar, 10, "p_job", ParameterDirection::Output, false, 2, 2, System::Data::DataRowVersion::Current, nullptr));
            callable_command->Parameters->Add(gcnew EDBParameter("p_hiredate", EDBTypes::EDBDbType::Date, 200, "p_hiredate", ParameterDirection::Output, false, 2, 2, System::Data::DataRowVersion::Current, nullptr));
            callable_command->Parameters->Add(gcnew EDBParameter("p_sal", EDBTypes::EDBDbType::Numeric, 200, "p_sal", ParameterDirection::Output, false, 2, 2, System::Data::DataRowVersion::Current, nullptr));
            callable_command->Prepare();
            callable_command->Parameters[0]->Value = 20;
            callable_command->Parameters[1]->Value = 7369;
            auto result = callable_command->ExecuteReader();
            int fc = result->FieldCount;
            for (int i = 0; i < fc; i++)
                Console::WriteLine("RESULT[" + i + "]=" + Convert::ToString(callable_command->Parameters[i]->Value));
            result->Close();
        }
        catch (EDBException^ exp)
        {
            if (exp->ErrorCode.Equals("01403"))
                Console::WriteLine("No data found");
            else if (exp->ErrorCode.Equals("01422"))
                Console::WriteLine("More than one rows were returned by the query");
            else
                Console::WriteLine("There was an error Calling the procedure. \nRoot Cause:\n");
            Console::WriteLine(exp->Message->ToString());
        }

        //Prepared statement
        String^ updateQuery = "update emp set ename = :Name where empno = :ID";
        auto Prepared_command = gcnew EDBCommand(updateQuery, conn);
        Prepared_command->CommandType = CommandType::Text;
        Prepared_command->Parameters->Add(gcnew EDBParameter("ID", EDBTypes::EDBDbType::Integer));
        Prepared_command->Parameters->Add(gcnew EDBParameter("Name", EDBTypes::EDBDbType::Text));
        Prepared_command->Prepare();
        Prepared_command->Parameters[0]->Value = 7369;
        Prepared_command->Parameters[1]->Value = "Mark";
        Prepared_command->ExecuteNonQuery();
        Console::WriteLine("Record Updated...");
    }

    catch (EDBException^ exp)
    {
        Console::WriteLine(exp->ToString());
    }
    finally
    {
        conn->Close();
    }
    return 0;
}
