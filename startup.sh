#!/bin/sh
set -e

# Copy webjob files to the wwwroot folder
#mkdir -p /home/site/wwwroot/App_Data/jobs/triggered/TriggeredDemo
#cp /webjobs/triggered/triggeredDemo/* /home/site/wwwroot/App_Data/jobs/triggered/TriggeredDemo

echo "Working folder: $(pwd)"
echo "defaulting to command: \"dotnet dotnetcoresample.dll\""
dotnet dotnetcoresample.dll