# Projeto-2 

Para correr o docker utilizamos as duas imagens de RabbitMQ+SQL:  

```
docker run -d --hostname rmq --name rabbit-server -p 8080:15672 -p 5672:5672 rabbitmq:3-management

docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=francisco-teste-123!" -p 11433:1433 -d mcr.microsoft.com/mssql/server:2017-latest
```
Em vez disto temos agora, através do docker-compose:
```
docker-compose up
```
# Correr os Scripts de SQL: 
Entrar no container de SQL: 
```
docker exec -it sql-server /opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P 'francisco-teste-123!'

```
Pushar os scripts: 

```
:r /scripts/createSQL.sql
GO
```
E finalmente: 

```
:r /scripts/SeedData.sql
GO
```

### Login do RabbitMq
```
guest;guest
```
### Login do MySql
```
sa;francisco-teste123! 

isto debaixo do SqlServer Authentication
```

# Utilidades do docker-compose
https://docs.docker.com/compose/reference/
