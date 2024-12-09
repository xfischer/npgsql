# Simple Query

- Query used in this example : `SELECT * FROM emp WHERE deptno = 20;`
- Result set

    > |empno|ename|job    |mgr |hiredate           |sal    |comm|deptno|
    > |-----|-----|-------|----|-------------------|-------|----|------|
    > |7566 |JONES|MANAGER|7839|1981-04-02 00:00:00|2975.00|    |20    |
    > |7876 |ADAMS|CLERK  |7788|1987-05-23 00:00:00|1100.00|    |20    |
    > |7902 |FORD |ANALYST|7566|1981-12-03 00:00:00|3000.00|    |20    |
    > |7788 |SCOTT|ANALYST|7566|1987-04-19 00:00:00|3000.00|    |20    |
    > |7369 |SMITH|CLERK  |7902|1980-12-17 00:00:00|800.00 |    |20    |

## Messages exchanged on the wire

[Query](https://www.postgresql.org/docs/current/protocol-message-formats.html#PROTOCOL-MESSAGE-FORMATS-QUERY)
![Simple query](messages/packet0001_message0001.png)

[RowDescription](https://www.postgresql.org/docs/current/protocol-message-formats.html#PROTOCOL-MESSAGE-FORMATS-ROWDESCRIPTION)
![Row description](messages/packet0002_message0002.png)

[DataRow](https://www.postgresql.org/docs/current/protocol-message-formats.html#PROTOCOL-MESSAGE-FORMATS-DATAROW)
![DataRow](messages/packet0002_message0003.png)
![DataRow](messages/packet0002_message0004.png)

[CommandComplete](https://www.postgresql.org/docs/current/protocol-message-formats.html#PROTOCOL-MESSAGE-FORMATS-COMMANDCOMPLETE)
![CommandComplete](messages/packet0002_message0005.png)

[ReadyForQuery](https://www.postgresql.org/docs/current/protocol-message-formats.html#PROTOCOL-MESSAGE-FORMATS-READYFORQUERY)
![ReadyForQuery](messages/packet0002_message0006.png)
