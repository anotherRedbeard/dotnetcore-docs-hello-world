#!/bin/sh
set -e

echo "Working folder: $(pwd)"
echo "defaulting to command: \"dotnet dotnetcoresample.dll\""
dotnet dotnetcoresample.dll