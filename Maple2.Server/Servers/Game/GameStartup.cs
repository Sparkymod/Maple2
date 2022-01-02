﻿using System;
using System.Net.Http;
using Autofac;
using Maple2.Server.Modules;
using Maple2.Server.Network;
using Maple2.Server.PacketHandlers;
using Maple2.Server.Servers.World.Service;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Maple2.Server.Servers.Game;

public class GameStartup {
    public IConfiguration Configuration { get; }

    public GameStartup(IConfiguration configuration) {
        this.Configuration = configuration;
    }

    // ConfigureServices is where you register dependencies. This gets called by the runtime before the
    // ConfigureContainer method, below.
    public void ConfigureServices(IServiceCollection services) {
        services.AddGrpcClient<Greeter.GreeterClient>(options => {
            options.Address = new Uri("https://localhost:5001");
            options.ChannelOptionsActions.Add(options => {
                // Return "true" to allow certificates that are untrusted/invalid
                options.HttpHandler = new HttpClientHandler {
                    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                };
            });
        });

        services.AddSingleton<GameServer>();
        services.AddHostedService<GameServer>(provider => provider.GetService<GameServer>());
    }

    // ConfigureContainer is where you can register things directly with Autofac. This runs after
    // ConfigureServices so the things here will override registrations made in ConfigureServices.
    // Don't build the container; that gets done for you by the factory.
    public void ConfigureContainer(ContainerBuilder builder) {
        builder.RegisterModule<NLogModule>();

        builder.RegisterType<PacketRouter<GameSession>>()
            .As<PacketRouter<GameSession>>()
            .SingleInstance();
        builder.RegisterType<GameSession>()
            .AsSelf();

        // Make all packet handlers available to PacketRouter
        builder.RegisterAssemblyTypes(typeof(IPacketHandler<>).Assembly)
            .Where(type => typeof(IPacketHandler<GameSession>).IsAssignableFrom(type))
            .As<IPacketHandler<GameSession>>()
            .SingleInstance();
    }

    // Configure is where you add middleware. This is called after ConfigureContainer. You can use
    // IApplicationBuilder.ApplicationServices here if you need to resolve things from the container.
    public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory) { }
}