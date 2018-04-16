#!/bin/bash

_gitsha=$(git rev-parse --short HEAD)
_token=$1
docker run --name fatslacksample -it mastoj/fatslack.sample:$_gitsha $_token