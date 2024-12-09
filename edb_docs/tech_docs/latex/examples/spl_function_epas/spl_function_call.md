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

![Full messages](messages/epasfunction.png)
