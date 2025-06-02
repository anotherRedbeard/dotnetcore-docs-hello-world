#!/bin/sh
set -e

# Copy webjob files to the wwwroot folder
mkdir -p /home/site/wwwroot/App_Data/jobs/triggered
cp /webjobs/triggered/* /home/site/wwwroot/App_Data/jobs/triggered/

echo "Working folder: $(pwd)"
echo "defaulting to command: \"dotnet dotnetcoresample.dll\""
dotnet dotnetcoresample.dll