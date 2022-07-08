﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Maple2.Database.Storage;
using Maple2.Model.Game;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game.Manager.Field;

public partial class FieldManager {
    public readonly ConcurrentDictionary<int, Plot> Plots = new();

    public bool UpdatePlotInfo(PlotInfo plotInfo) {
        if (MapId != plotInfo.MapId || !Plots.ContainsKey(plotInfo.Number)) {
            return false;
        }

        if (plotInfo is Plot plot) {
            Plots[plot.Number] = plot;
        } else {
            plot = Plots[plotInfo.Number];
            plot.OwnerId = plotInfo.OwnerId;
            plot.Name = plotInfo.Name;
            plot.ExpiryTime = plotInfo.ExpiryTime;
        }

        Multicast(CubePacket.UpdatePlot(plot));
        return true;
    }

    private void CommitPlot(GameSession session) {
        Home home = session.Player.Value.Home;
        // TODO: Also check user is an owner of this specific house.
        using GameStorage.Request db = GameStorage.Context();
        if (home.Indoor.MapId == MapId && Plots.TryGetValue(home.Indoor.Number, out Plot? indoorPlot)) {
            SavePlot(indoorPlot);
        }
        if (home.Outdoor != null && home.Outdoor.MapId == MapId && Plots.TryGetValue(home.Outdoor.Number, out Plot? outdoorPlot)) {
            SavePlot(outdoorPlot);
        }

        void SavePlot(Plot plot) {
            lock (Plots) {
                ICollection<UgcItemCube>? results = db.SaveCubes(plot, plot.Cubes.Values);
                if (results == null) {
                    logger.Fatal("Failed to save plot cubes: {PlotId}", plot.Id);
                    throw new InvalidOperationException($"Failed to save plot cubes: {plot.Id}");
                }

                plot.Cubes.Clear();
                foreach (UgcItemCube result in results) {
                    plot.Cubes.Add(result.Position, result);
                }
            }
        }
    }
}