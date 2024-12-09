# SPL Function call with OUT parameters and return value

- Function used in this example :

    ```sql
    CREATE OR REPLACE FUNCTION myFunc(a IN text, b OUT text)
    RETURN int
    AS
    BEGIN
        b := 'I now have a value :)';
        return a;
    END;
    ```

- Parameters

    | Parameter name | a     | b    |  myFunc          |
    |----------------|-------|------|------------------|
    | Direction      | IN    | OUT  |  RETURN VALUE    |
    | Input value    | 10    | -    |  -               |

- Results

    | Parameter name | a     | b                         | mixArgFunc_Test |
    |----------------|-------|---------------------------|-----------------|
    | Direction      | IN    | OUT                       | RETURN VALUE    |
    | Expected value | -     | "I now have a value :)"   | 10              |

## Messages exchanged on the wire

### Prepare

1. FrontEnd SENDS

    ParseOut (EPAS specific)
    ![ParseOut](../../epas/parseout.png)

    [Describe](https://www.postgresql.org/docs/current/protocol-message-formats.html#PROTOCOL-MESSAGE-FORMATS-DESCRIBE)
    ![Describe](../../postgres/describe.png)

    [Sync](https://www.postgresql.org/docs/current/protocol-message-formats.html#PROTOCOL-MESSAGE-FORMATS-SYNC)
    ![Sync](../../postgres/sync.png)

2. BackEnd SENDS

    [ParseComplete](https://www.postgresql.org/docs/current/protocol-message-formats.html#PROTOCOL-MESSAGE-FORMATS-PARSECOMPLETE)
    ![ParseComplete](../../postgres/parsecomplete.png)

    [ParameterDescription](https://www.postgresql.org/docs/current/protocol-message-formats.html#PROTOCOL-MESSAGE-FORMATS-PARAMETERDESCRIPTION)
    ![Parameter description](../../postgres/parameterdescription.png)

    [RowDescription](https://www.postgresql.org/docs/current/protocol-message-formats.html#PROTOCOL-MESSAGE-FORMATS-ROWDESCRIPTION)

    > There is only one field here : the return value, with name equal to the function name

    ![Row description](../../postgres/rowdescription.png)

    [ReadyForQuery](https://www.postgresql.org/docs/current/protocol-message-formats.html#PROTOCOL-MESSAGE-FORMATS-READYFORQUERY)
    ![ReadyForQuery](../../postgres/readyforquery.png)

### Execute

1. FrontEnd SENDS

    [Bind](https://www.postgresql.org/docs/current/protocol-message-formats.html#PROTOCOL-MESSAGE-FORMATS-BIND)

    > Only one result format as it is the function's return value

    ![Bind](../../postgres/bind.png)

    [Describe](https://www.postgresql.org/docs/current/protocol-message-formats.html#PROTOCOL-MESSAGE-FORMATS-DESCRIBE)
    ![Describe](../../postgres/describe.png)

    DescribeOut (EPAS SPECIFIC)
    ![Describe Out](../../epas/describeout.png)

    [Execute](https://www.postgresql.org/docs/current/protocol-message-formats.html#PROTOCOL-MESSAGE-FORMATS-EXECUTE)
    ![Execute](../../postgres/execute.png)

    ExecuteOut (EPAS SPECIFIC)
    ![Execute Out](../../epas/executeout.png)

    [Sync](https://www.postgresql.org/docs/current/protocol-message-formats.html#PROTOCOL-MESSAGE-FORMATS-SYNC)
    ![Sync](../../postgres/sync.png)

2. BackEnd SENDS

    [BindComplete](https://www.postgresql.org/docs/current/protocol-message-formats.html#PROTOCOL-MESSAGE-FORMATS-BINDCOMPLETE)
    ![BindComplete](../../postgres/bindcomplete.png)

    [RowDescription](https://www.postgresql.org/docs/current/protocol-message-formats.html#PROTOCOL-MESSAGE-FORMATS-ROWDESCRIPTION)
    ![Row description](../../postgres/rowdescription.png)

    OutDescription (EPAS SPECIFIC)
    ![OutDescription](../../epas/outdescription.png)

    [DataRow](https://www.postgresql.org/docs/current/protocol-message-formats.html#PROTOCOL-MESSAGE-FORMATS-DATAROW)

    > Only one row for function return value

    ![DataRow](../../postgres/datarow.png)

    [CommandComplete](https://www.postgresql.org/docs/current/protocol-message-formats.html#PROTOCOL-MESSAGE-FORMATS-COMMANDCOMPLETE)
    ![CommandComplete](../../postgres/commandcomplete.png)

    SendOutTuple (EPAS SPECIFIC)
    ![Send Out Tuple](../../epas/sendouttuple.png)

    [RowDescription](https://www.postgresql.org/docs/current/protocol-message-formats.html#PROTOCOL-MESSAGE-FORMATS-ROWDESCRIPTION)

    > I have no explanation on why another row description is returned. Maybe the Front end meesages are wrong ?

    ![Row description](../../postgres/rowdescription.png)

    [ReadyForQuery](https://www.postgresql.org/docs/current/protocol-message-formats.html#PROTOCOL-MESSAGE-FORMATS-READYFORQUERY)
    ![ReadyForQuery](../../postgres/readyforquery.png)

### In the real world

![Full messages](messages/epasfunction.png)
