# Front end / Back end protocol Diagrams

## Notes

- For the sake of simplicity, BackEnd to FrontEnd messages are represented as if messages were sent from server. This is not true : server writes to the FE/BE socket, and client consumes it.

- All messages are length prefixed, omitted here for brievity

## Example FE/BE diagrams

Call SPL procedure with an IN param and an OUT param

### Prepare statement

.NET Connector uses EDBCommand to process query and orchestrate EDBConnector for reads & writes on the wire.

```plantuml
@startuml

title Prepare SPL Procedure call\n\
emp_query_small( IN :p_empno, OUT :p_ename)
actor User as U
participant ".NET Connector" as C
database EPAS as S

note right of U : Create Command :\nemp_query_small(:p_empno, :p_ename)
U -> C : Command.Prepare()
C -> C : Query rewritten to \nCALL emp_query_small($1,$2)
C -> S : ParseOut (**O**)
note over S: statement name ('_p1')\n\
query\n\
parameter count\n\
parameter's OID\n\
parameter's direction
C -> S : Describe (**D**)
note over S: statement (S)\n\
statement name ('_p1')
C -> S : Sync (**S**)
S -> C : ParseComplete (**1**)
S -> C : ParameterDescription (**t**)
note over C: parameter count\n\
parameter's OID
S -> C : RowDescription (**T**)
note over C: field count\n\
foreach field:\n\
name, \ntable OID, \nindex, \nOID, \nsize, \ntypemodifier, \nformat (text/binary)
S -> C : ReadyForQuery (**Z**)
@enduml
```

### Execute statement

EDBDataReader is instanciated in EDBConnector constructor.
```plantuml
@startuml

title Execute SPL Procedure call\n\
emp_query_small( IN :p_empno, OUT :p_ename)
actor User as U
participant ".NET Connector" as C
database EPAS as S

note right of U : Command.ExecuteReader() (returns EDBDataReader)
U -> C : Command.ExecuteReader()
C -> S : Bind (**B**)
note over S: statement name ('_p1')\n\
foreach parameter: format (text|binary)\n\
foreach parameter: value (length + value)\n\
-1 for OUT parameters
C -> S : Describe (**D**)
C -> S : DescribeOut (**u**)
note over S: for both: statement or portal (hardcoded to portal)\n\
name (empty)
C -> S : Execute (**E**)
C -> S : ExecuteOut (**v**)
note over S: portal (always empty)\n\
max rows (0)
C -> S : Sync (**S**)
C -> C : NextResult()
note over S: Standard MS way of reading next messages from back end (ADO.NET)
S -> C : BindComplete (**2**)
S -> C : NoData (**n**)
C -> C : PopulateOutputParameters()
activate C
note right of C: Called only if store procedure or has OUT/RET params
S -> C : DescribeOut (**u**)
note over C: field count\n\
foreach field:\n\
name, \ntable OID, \nindex, \nOID, \nsize, \ntypemodifier, \nformat (text/binary)
S -> C : CommandComplete (**C**)
note over C: message "EDB SPL Procedure successfully completed"
S -> C : SendOutTuple  (**v**)
note over C: Read parameters from OutTuple data row\n\
and populate parameter values
C -> U: EDBDataReader instance
deactivate C
U -> C: EDBDataReader.Close()
activate C
note over C: State is still "Executing"
C -> C: Consume()
S -> C: ReadyForQuery (**Z**)
note over C: sends rest of buffer if any
C -> C: State = "Consumed"
C -> C: State = "Closed"
deactivate C
C -> U: void
@enduml
```
