#!/bin/bash

_gitsha=$(git rev-parse --short HEAD)
docker build -t mastoj/fatslack.sample:$_gitsha .