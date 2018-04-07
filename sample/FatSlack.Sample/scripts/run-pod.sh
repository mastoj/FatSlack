#!/bin/bash

_gitsha=$(git rev-parse --short HEAD)
_apiToken=$1
_appToken=$2
echo "$_apiToken $_appToken"
kubectl delete po fatslacksample
sleep 5
kubectl run fatslacksample --port=8080 --restart=Never --image=mastoj/fatslack.sample:$_gitsha -- $_apiToken $_appToken