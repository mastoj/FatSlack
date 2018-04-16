#!/bin/bash

_apitoken=$1
_apptoken=$2
scripts/publish.sh
scripts/build-container.sh
scripts/publish-container.sh
scripts/run-pod.sh $_apitoken $_apptoken