﻿using System;
using System.IO;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Maple2.Database.Context;
using Maple2.Database.Storage;
using Maple2.Model.Metadata;
using Maple2.Server.Core.Modules;
using Maple2.Server.Global.Service;
using Maple2.Server.World.Containers;
using Maple2.Server.World.Service;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

IConfigurationRoot configRoot = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", true, true)
    .Build();
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configRoot)
    .CreateLogger();

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddSerilog(dispose: true);

builder.Services.AddGrpc();
builder.Services.RegisterModule<ChannelClientModule>();
builder.Services.AddMemoryCache();

builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
builder.Host.ConfigureContainer<ContainerBuilder>(autofac => {
    // Database
    autofac.RegisterModule<GameDbModule>();
    autofac.RegisterModule<DataDbModule>();

    autofac.RegisterType<PlayerChannelLookup>()
        .SingleInstance();
});

WebApplication app = builder.Build();
app.UseRouting();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client.");
app.MapGrpcService<WorldService>();
app.MapGrpcService<GlobalService>();


string? gameDbConnection = Environment.GetEnvironmentVariable("GAME_DB_CONNECTION");
if (gameDbConnection == null) {
    throw new ArgumentException("GAME_DB_CONNECTION environment variable was not set");
}


DbContextOptions options = new DbContextOptionsBuilder()
    .UseMySql(gameDbConnection, ServerVersion.AutoDetect(gameDbConnection)).Options;
await using (var initContext = new InitializationContext(options)) {
    // Initialize database if needed
    if (initContext.Initialize()) {
        ILifetimeScope root = app.Services.GetAutofacRoot();
        var gameStorage = root.Resolve<GameStorage>();
        var mapStorage = root.Resolve<MapMetadataStorage>();

        using GameStorage.Request db = gameStorage.Context();
        if (!db.InitUgcMap(mapStorage.GetAllUgc())) {
            Log.Fatal("Failed to initialize UgcMap");
            return;
        }

        Log.Debug("Database has been initialized");
    } else {
        Log.Debug("Database has already been initialized");
    }
}

await app.RunAsync();
