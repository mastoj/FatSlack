#!/bin/bash

_gitsha=$(git rev-parse --short HEAD)
docker push mastoj/fatslack.sample:$_gitsha