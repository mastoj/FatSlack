#!/bin/bash

_gitsha=$(git rev-parse --short HEAD)
_slacktoken=$1
kubectl delete po fatslacksample
kubectl run fatslacksample --port=8080 --restart=Never --image=mastoj/fatslack.sample:$_gitsha -- $_slacktoken