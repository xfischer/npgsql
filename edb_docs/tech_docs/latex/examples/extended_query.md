# Simple parameterized query with extended protocol

- Query used in this example : `SELECT * FROM emp WHERE deptno = $1`
- Bind parameter value 20
- Same Result set

    > |empno|ename|job    |mgr |hiredate           |sal    |comm|deptno|
    > |-----|-----|-------|----|-------------------|-------|----|------|
    > |7566 |JONES|MANAGER|7839|1981-04-02 00:00:00|2975.00|    |20    |
    > |7876 |ADAMS|CLERK  |7788|1987-05-23 00:00:00|1100.00|    |20    |
    > |7902 |FORD |ANALYST|7566|1981-12-03 00:00:00|3000.00|    |20    |
    > |7788 |SCOTT|ANALYST|7566|1987-04-19 00:00:00|3000.00|    |20    |
    > |7369 |SMITH|CLERK  |7902|1980-12-17 00:00:00|800.00 |    |20    |

## Messages exchanged on the wire

1. FrontEnd SENDS

    [Parse](https://www.postgresql.org/docs/current/protocol-message-formats.html#PROTOCOL-MESSAGE-FORMATS-PARSE)
    ![Parse](../postgres/parse.png)

    [Bind](https://www.postgresql.org/docs/current/protocol-message-formats.html#PROTOCOL-MESSAGE-FORMATS-BIND)
    ![Bind](../postgres/bind.png)

    [Describe](https://www.postgresql.org/docs/current/protocol-message-formats.html#PROTOCOL-MESSAGE-FORMATS-DESCRIBE)
    ![Describe](../postgres/describe.png)

    [Execute](https://www.postgresql.org/docs/current/protocol-message-formats.html#PROTOCOL-MESSAGE-FORMATS-EXECUTE)
    ![Execute](../postgres/execute.png)

    [Sync](https://www.postgresql.org/docs/current/protocol-message-formats.html#PROTOCOL-MESSAGE-FORMATS-SYNC)
    ![Sync](../postgres/sync.png)

2. BackEnd SENDS

    [ParseComplete](https://www.postgresql.org/docs/current/protocol-message-formats.html#PROTOCOL-MESSAGE-FORMATS-PARSECOMPLETE)
    ![ParseComplete](../postgres/parsecomplete.png)

    [BindComplete](https://www.postgresql.org/docs/current/protocol-message-formats.html#PROTOCOL-MESSAGE-FORMATS-BINDCOMPLETE)
    ![BindComplete](../postgres/bindcomplete.png)

    [RowDescription](https://www.postgresql.org/docs/current/protocol-message-formats.html#PROTOCOL-MESSAGE-FORMATS-ROWDESCRIPTION)
    ![Row description](../postgres/rowdescription.png)

    [DataRow](https://www.postgresql.org/docs/current/protocol-message-formats.html#PROTOCOL-MESSAGE-FORMATS-DATAROW)
    ![DataRow](../postgres/datarow.png)

    [CommandComplete](https://www.postgresql.org/docs/current/protocol-message-formats.html#PROTOCOL-MESSAGE-FORMATS-COMMANDCOMPLETE)
    ![CommandComplete](../postgres/commandcomplete.png)

    [ReadyForQuery](https://www.postgresql.org/docs/current/protocol-message-formats.html#PROTOCOL-MESSAGE-FORMATS-READYFORQUERY)
    ![ReadyForQuery](../postgres/readyforquery.png)
