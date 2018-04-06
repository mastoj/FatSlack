#!/bin/bash

_token=$_
scripts/publish.sh
scripts/build-container.sh
scripts/publish-container.sh
scripts/run-pod.sh $_token