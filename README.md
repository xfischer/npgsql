# EDB .NET Connector - the .NET data provider for EDB Postgres Advanced Server

[![stable](https://img.shields.io/nuget/v/EnterpriseDB.EDBClient.svg?label=stable)](https://www.nuget.org/packages/EnterpriseDB.EDBClient/) 

## What is EDB .NET Connector?

The EDB .NET Connector distributed with EDB Postgres Advanced Server provides connectivity between a .NET client application and an EDB Postgres Advanced Server database server. You can:

- Connect to an instance of EDB Postgres Advanced Server.
- Retrieve information from an EDB Postgres Advanced Server database.
- Update information stored on an EDB Postgres Advanced Server database.

The EDB .NET Connector functionality is built on the core functionality of the Npgsql open source project. For details, see the [Npgsql User Guide](http://www.npgsql.org/doc/index.html).

## Contributing

Rules :

- Every EDB specific addition should be prefixed with `/* EnterpriseDB : <text> */` for several lines or `// EnterpriseDB`
- EDB additions should be documented (who / why / how)