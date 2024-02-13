EDB Npgsql Rebrand tool - Xavier Fischer

Usage : edb_rebrand.exe <file | directory>

Will open every .cs file (except AssemblyInfo.cs) and make the following replacements, in this order :

"Npgsql;"	--> "EntrepriseDB.EDBClient;"
"Npgsql."	--> "EnterpriseDB.EDBClient."
"Npgsql "	--> "EDB .NET Connector "
"Npgsql"	--> "EDB"

Note: before saving files, a report will be generated and a confirmation will be asked.