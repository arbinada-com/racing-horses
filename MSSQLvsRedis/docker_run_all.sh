#!/bin/bash

docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=PassW0rd" \
   -p 1433:1433 --name mssql-2022 --hostname mssql-2022 \
   -d --rm \
   mcr.microsoft.com/mssql/server:2022-latest

docker run -d --rm \
    --name redis-server -p 6379:6379 \
    redis:latest
