# 1. Azure Cache for [NHK World TV Kodi Plugin](https://github.com/sbroenne/plugin.video.nhkworldtv)

- [1. Azure Cache for NHK World TV Kodi Plugin](#1-azure-cache-for-nhk-world-tv-kodi-plugin)
  - [1.1. Disclaimer](#11-disclaimer)
  - [1.2. Overview](#12-overview)
  - [1.3. Design & Implementation on Azure](#13-design--implementation-on-azure)
  - [1.4. Azure Functions](#14-azure-functions)
  - [1.5. CosmosDB Set-up](#15-cosmosdb-set-up)
  - [1.6. Development Environment](#16-development-environment)
  - [1.7. Deployment on Azure](#17-deployment-on-azure)

## 1.1. Disclaimer

This Azure Function is a fan project and not related in any way to NHK! I built this plugin because NHK does not support Android TV (yet).

## 1.2. Overview

[NHK World TV](https://github.com/sbroenne/plugin.video.nhkworldtv) is a Kodi plug-in that displays most of the content from [NHK World Japan](https://www3.nhk.or.jp/nhkworld/en/live/) in Kodi in the highest possible quality (1080p).

This repo provides a persistent backend-cache hosted on Microsoft Azure to speed up episode video Url lookup operations where the NHK API is very slow.

## 1.3. Design & Implementation on Azure

The cache is implemented as a set of Azure Functions backed by a CosmosDB (free tier) in "SQL-Mode". It's 100% serverless/PaaS.

That was the easiest, most cost effective (because of CosmosDB free tier and serverless), lowest operations and most importantly, most fun way of doing things for me.

## 1.4. Azure Functions

Azure Functions are implemented in [sbroennelab.nhkworldtv](./sbroennelab.nhkworldtv) .Net Core 3.1 project using Azure Functions v3 in C#. The Unit Tests can be found in [sbroennelab.nhkworldtv.Tests](./sbroennelab.nhkworldtv.Tests) project (I am using xUnit).

The function app has been deployed to Azure in West Europe (I live in Germany)

There are two functions:

- https://nhkworldtvwe.azurewebsites.net/api/Program/v2/List/{maxItems} - returns a JSON with the paths (1080P and 720P) for the episodes of the video-on-demand programs
- PopulateCache - runs on a timer to update the the episode information in CosmosDB 

You need a key to access the functions in order to prevent abuse by bots.

## 1.5. CosmosDB Set-up

There is a an ARM template you can deploy in [sbroennelab.nhkworldtv/arm/cosmosdb-nhkdb](.sbroennelab.nhkworldtv/arm/cosmosdb-nhkdb). It will create the CosmosDB **nhkdb** in the resource group **nhkworldtv** in **West Europe** ny default. It will create a container **VodProgram**.

## 1.6. Development Environment

I develop with Visual Studio Code on a Windows 10 box using WSL2 running Ubuntu (remote/WLS2 based development).

I use the Azurite extension instead of the Azure Storage Emulator.

**Important hint while unit testing**: You need to set the application setting as Environment variables in **Windows 10/WSLv2** if you want to run the unit tests from the test runner (in my case .NET Test Explorer). Otherwise the functions will not find them - even though they are defined and used in local.settings.json.

It is a pure .Net Core solution so you should be able to use any DEV environment you prefer. I also tested it on Visual Studio 2019.

## 1.7. Deployment on Azure

You will need to add the following Application Settings in your function app:

```
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
```

