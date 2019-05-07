#!/bin/bash

pushd Inversion.Messaging/bin/Debug
  dotnet nuget push *.nupkg -k $NUGET_KEY -s https://api.nuget.org/v3/index.json
popd

pushd Inversion.Messaging.AmazonSNS/bin/Debug
  dotnet nuget push *.nupkg -k $NUGET_KEY -s https://api.nuget.org/v3/index.json
popd

pushd Inversion.Messaging.AmazonSQS/bin/Debug
  dotnet nuget push *.nupkg -k $NUGET_KEY -s https://api.nuget.org/v3/index.json
popd

pushd Inversion.Messaging.DynamoDB/bin/Debug
  dotnet nuget push *.nupkg -k $NUGET_KEY -s https://api.nuget.org/v3/index.json
popd

pushd Inversion.Messaging.Filesystem/bin/Debug
  dotnet nuget push *.nupkg -k $NUGET_KEY -s https://api.nuget.org/v3/index.json
popd

pushd Inversion.Messaging.MsSql/bin/Debug
  dotnet nuget push *.nupkg -k $NUGET_KEY -s https://api.nuget.org/v3/index.json
popd

pushd Inversion.Messaging.PostgreSQL/bin/Debug
  dotnet nuget push *.nupkg -k $NUGET_KEY -s https://api.nuget.org/v3/index.json
popd

pushd Inversion.Messaging.Redis/bin/Debug
  dotnet nuget push *.nupkg -k $NUGET_KEY -s https://api.nuget.org/v3/index.json
popd

pushd Inversion.Messaging.Sql/bin/Debug
  dotnet nuget push *.nupkg -k $NUGET_KEY -s https://api.nuget.org/v3/index.json
popd
