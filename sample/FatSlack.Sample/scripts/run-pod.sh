#!/bin/bash

_gitsha=$(git rev-parse --short HEAD)
_slacktoken=$1
kubectl run fatslacksample --restart=Never --image=mastoj/fatslack.sample:$_gitsha -- $_slacktoken