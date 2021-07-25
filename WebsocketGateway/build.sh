#!/bin/bash

rm -r bin/release/net5.0/linux-x64/publish/

dotnet publish \
    --configuration release \
    -r linux-x64 \
 #   --self-contained true
