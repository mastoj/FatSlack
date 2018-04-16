#!/bin/bash

kubectl expose pod fatslacksample --port=8080 --type=LoadBalancer --name=fatslacksample