#!/bin/bash
DIR="$( cd "$( dirname "$0" )" && pwd )"
exec /usr/bin/mono --runtime=v4.0 "$DIR/AlarmBox.exe" '$@'
