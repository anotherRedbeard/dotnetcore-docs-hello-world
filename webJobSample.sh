#!/bin/bash

set -e
echo "Working folder: $(pwd)"
cd /app
echo "defaulting to command: \"dotnet dotnetcoresample.dll\""
dotnet dotnetcoresample.dll run-command-line