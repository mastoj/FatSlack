#!/bin/bash

_token=$1
scripts/publish.sh
scripts/build-container.sh
scripts/publish-container.sh
scripts/run-pod.sh $_token