# Azure Cache for [NHK World TV Kodi Plugin](https://github.com/sbroenne/plugin.video.nhkworldtv)

## Disclaimer

This Azure Function is a fan project and not related in any way to NHK! I built this plugin because NHK does not support Android TV (yet).

## Overview

[NHK World TV](https://github.com/sbroenne/plugin.video.nhkworldtv) is a Kodi plug-in that displays most of the content from [NHK World Japan](https://www3.nhk.or.jp/nhkworld/en/live/) in Kodi in the highest possible quality (1080p).

This repo provides a persistent backend-cache hosted on Microsoft Azure to speed up certain operations where either the NHK API is very slow or doesn't provide the functionality in the required form.

## Design & Implementation on Azure

The cache is implemented as a set of Azure Functions backed by a CosmosDB (free tier) in "SQL-Mode". It's 100% serverless/PaaS.

That was the easiest, most cost effective (because of CosmosDB free tier and serverless), lowest operations and most importantly, most fun way of doing things for me.

## Azure Functions

Azure Functions are implemented in [sbroennelab.nhkworldtv](./sbroennelab.nhkworldtv) .Net Core 3.1 project using Azure Functions v3 in C#. The Unit Tests can be found in [sbroennelab.nhkworldtv.Tests](./sbroennelab.nhkworldtv.Tests) project (I am using xUnit).

The function app has been deployed to Azure in West Europe (I live in Germany)

There are three functions:

- https://nhkworldtvwe.azurewebsites.net/api/Program/List/{maxItems} - returns a JSON with all the video-on-demand programs including the paths to the actual episodes 720P and 1080P 
- https://nhkworldtvwe.azurewebsites.net/api/Program/{vodId} - gets a single episode
- PopulateCache - runs on a time to update the underlying CosmosDB

You need a key to access these functions in order to prevent abuse by bots.

## CosmosDB Set-up

You can use the following [code](./sbroennelab.nhkworldtv/DDL_VodProgram.cs) to create the CosmosDB database and container. There is also a corresponding test so you can use your favorite test runner as well.

## Development Environment

I develop with Visual Studio Code on a Windows 10 box using WSL2 running Ubuntu (remote/WLS2 based development). I used to develop on a MacBook Pro but I prefer the Windows 10 UI (I know, I know, ...) and I actually prefer Ubuntu to MacOsX as well.

I use the Azurite extension instead of the Azure Storage Emulator.

**Important hint while unit testing**: You need to set the application setting as Environment variables in **Windows 10** if you want to run the unit tests from the test runner (in my case .NET Test Explorer). Otherwise the functions will not find them - even though they are defined and used in local.settings.json.

It is a pure .Net Core solution so you should be able to use any DEV environment you prefer.

## Deployment on Azure

You will need to add the following Application Settings in your function app:

~~~~
  {
    "name": "COSMOS_ACCOUNT_KEY",
    "value": "<your Cosmos Account Key value>",
    "slotSetting": false
  },
  {
    "name": "DATABASE_ID",
    "value": "<your CosmosDB database value>",
    "slotSetting": false
  },
  {
    "name": "DATABSE_CONTAINER_VOD_PROGRAM",
    "value": "VodProgram",
    "slotSetting": false
  },
  {
    "name": "ENDPOINT_URL",
    "value": "<your Cosmos DB endpoint value>",
    "slotSetting": false
  }
~~~~

I will add ARM templates soon so that should make everything easier but there aren't many steps to automate.