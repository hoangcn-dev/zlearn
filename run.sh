#!/bin/sh
docker compose down
docker rmi learn
docker load -i new.tar
docker compose up -d
