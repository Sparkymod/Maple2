﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Maple2.Model.Enum;
using Maple2.Model.Game;
using Maple2.Model.Metadata;
using Maple2.Server.Game.Model;
using Maple2.Server.Game.Packets;
using Maple2.Tools.Extensions;

namespace Maple2.Server.Game.Trigger;

public partial class TriggerContext {
    public void ChangeBackground(string dds) {
        DebugLog("[ChangeBackground] dds:{Dds}", dds);
        Field.SetBackground(dds);
    }

    public void EnableLocalCamera(bool enabled) {
        DebugLog("[EnableLocalCamera] enabled:{Enabled}", enabled);
        Field.AddFieldProperty(new FieldPropertyLocalCamera { Enabled = enabled });
    }

    public void LockMyPc(bool isLock) {
        DebugLog("[LockMyPc] isLock:{IsLock}", isLock);
        if (isLock) {
            Field.AddFieldProperty(new FieldPropertyLockPlayer());
        } else {
            Field.RemoveFieldProperty(FieldProperty.LockPlayer);
        }
    }

    public void SetAmbientLight(Vector3 color) {
        DebugLog("[SetAmbientLight] color:{Color}", color);
        Field.AddFieldProperty(new FieldPropertyAmbientLight { Color = color });
    }

    public void SetDirectionalLight(Vector3 diffuseColor, Vector3 specularColor) {
        DebugLog("[SetDirectionalLight] diffuseColor:{DiffuseColor}, specularColor:{SpecularColor}", diffuseColor, specularColor);
        Field.AddFieldProperty(new FieldPropertyDirectionalLight {
            DiffuseColor = diffuseColor,
            SpecularColor = specularColor,
        });
    }

    public void SetGravity(float gravity) {
        DebugLog("[SetGravity] gravity:{Gravity}", gravity);
        Field.AddFieldProperty(new FieldPropertyGravity(gravity));
    }

    public void SetPhotoStudio(bool enabled) {
        DebugLog("[SetPhotoStudio] enabled:{Enabled}", enabled);
        Field.AddFieldProperty(new FieldPropertyPhotoStudio { Enabled = enabled });
    }

    public void UserTagSymbol(string symbol1, string symbol2) {
        DebugLog("[UserTagSymbol] symbol1:{Symbol1}, symbol2:{Symbol2}", symbol1, symbol2);
        Broadcast(FieldPropertyPacket.Add(new FieldPropertyUserTagSymbol { Symbol1 = symbol1, Symbol2 = symbol2 }));
    }

    public void VisibleMyPc(bool visible) {
        DebugLog("[VisibleMyPc] visible:{Visible}", visible);
        if (visible) {
            Field.RemoveFieldProperty(FieldProperty.HidePlayer);
        } else {
            Field.AddFieldProperty(new FieldPropertyHidePlayer());
        }
    }

    public void Weather(Maple2.Trigger.Enum.WeatherType weatherType) {
        DebugLog("[Weather] weatherType:{Type}", weatherType);
        Field.AddFieldProperty(new FieldPropertyWeather { WeatherType = (WeatherType) weatherType });
    }

    public void SightRange(bool enabled, byte range, float rangeZ, byte border) {
        DebugLog("[SightRange] enabled:{Enabled}, range:{Range}, rangeZ:{RangeZ}, border:{Border}", enabled, range, rangeZ, border);
        // range seems to be some ID? (3)
        if (enabled) {
            Field.AddFieldProperty(new FieldPropertySightRange {
                Range = rangeZ,
                Opacity = border,
            });
        } else {
            Field.RemoveFieldProperty(FieldProperty.SightRange);
        }
    }

    public void SetPvpZone(int boxId, byte arg2, int duration, int additionalEffectId, byte arg5, params int[] boxIds) {
        ErrorLog("[SetPvpZone] boxId:{BoxId}, arg2:{Arg2}, duration:{Duration}, additionalEffectId:{AdditionalEffectId}, arg5:{Arg5}, boxIds:{BoxIds}",
            boxId, arg2, duration, additionalEffectId, arg5, string.Join(", ", boxIds));
    }

    public void SetTimeScale(bool enabled, float startScale, float endScale, float duration, byte interpolator) {
        DebugLog("[SetTimeScale] enabled:{Enabled}, startScale:{StartScale}, endScale:{EndScale}, duration:{Duration}, interpolator:{Interpolator}",
            enabled, startScale, endScale, duration, interpolator);
        // TODO: Does this need to be persisted on the field?
        Broadcast(FieldPropertyPacket.TimeScale(enabled, startScale, endScale, duration, interpolator));
    }

    public void SetLocalCamera(int cameraId, bool enabled) {
        DebugLog("[SetLocalCamera] cameraId:{Id}, enabled:{Enabled}", cameraId, enabled);
        Broadcast(CameraPacket.Local(cameraId, enabled));
    }

    public void CameraReset(float interpolationTime) {
        DebugLog("[CameraReset] interpolationTime:{Time}", interpolationTime);
        Broadcast(CameraPacket.Interpolate(interpolationTime));
    }

    public void CameraSelect(int triggerId, bool enabled) {
        DebugLog("[CameraSelect] triggerId:{Id}, enabled:{Enabled}", triggerId, enabled);
        if (!Objects.Cameras.TryGetValue(triggerId, out TriggerObjectCamera? camera)) {
            return;
        }
        if (camera.Visible == enabled) {
            return;
        }

        camera.Visible = enabled;
        Broadcast(TriggerPacket.Update(camera));
    }

    public void CameraSelectPath(int[] pathIds, bool returnView) {
        DebugLog("[CameraSelectPath] pathIds:{Ids}, returnView:{ReturnView}", string.Join(", ", pathIds), returnView);
        Broadcast(TriggerPacket.CameraStart(pathIds, returnView));
    }

    #region Entities
    public void SetActor(int triggerId, bool visible, string initialSequence, bool arg4, bool arg5) {
        DebugLog("[SetActor] triggerId:{Id}, visible:{Visible}, initialSequence:{InitialSequence}, arg4:{Arg4}, arg5:{Arg5}",
            triggerId, visible, initialSequence, arg4, arg5);
        if (!Objects.Actors.TryGetValue(triggerId, out TriggerObjectActor? actor)) {
            return;
        }

        actor.Visible = visible;
        actor.SequenceName = initialSequence;
        Broadcast(TriggerPacket.Update(actor));
    }

    public void SetAgent(int[] triggerIds, bool visible) {
        ErrorLog("[SetAgent] triggerIds:{Ids}, visible:{Visible}", string.Join(", ", triggerIds), visible);
    }

    public void SetBreakable(int[] triggerIds, bool enabled) {
        DebugLog("[SetBreakable] triggerIds:{Ids}, enabled:{Enabled}", string.Join(", ", triggerIds), enabled);
        var updated = new List<FieldBreakable>(triggerIds.Length);
        foreach (int triggerId in triggerIds) {
            if (!Field.TryGetBreakable(triggerId, out FieldBreakable? breakable)) {
                continue;
            }

            breakable.UpdateState(BreakableState.Show);
            updated.Add(breakable);
        }

        Field.Broadcast(BreakablePacket.Update(updated));
    }

    public void SetCube(int[] triggerIds, bool visible, int randomCount) {
        DebugLog("[SetCube] triggerIds:{Ids}, visible:{Visible}, randomCount:{Count}", string.Join(", ", triggerIds), visible, randomCount);
        int count = triggerIds.Length;
        if (randomCount > 0 && randomCount < triggerIds.Length) {
            count = randomCount;
            Random.Shared.Shuffle(triggerIds);
        }

        for (int i = 0; i < count; i++) {
            if (!Objects.Cubes.TryGetValue(triggerIds[i], out TriggerObjectCube? cube)) {
                continue;
            }
            if (cube.Visible == visible) {
                continue;
            }

            cube.Visible = visible;
            Broadcast(TriggerPacket.Update(cube));
        }
    }

    public void SetEffect(int[] triggerIds, bool visible, int arg3, byte arg4) {
        DebugLog("[SetEffect] triggerIds:{Ids}, visible:{Visible}, arg3:{Arg3}, arg4:{Arg4}", string.Join(", ", triggerIds), visible, arg3, arg4);
        foreach (int triggerId in triggerIds) {
            if (!Objects.Effects.TryGetValue(triggerId, out TriggerObjectEffect? effect)) {
                continue;
            }
            if (effect.Visible == visible) {
                continue;
            }

            effect.Visible = visible;
            Broadcast(TriggerPacket.Update(effect));
        }
    }

    public void SetInteractObject(int[] interactIds, byte stateValue, bool arg3, bool arg4) {
        var state = (InteractState) stateValue;
        DebugLog("[SetInteractObject] interactIds:{Ids}, state:{State}, arg3:{Arg3}, arg4:{Arg4}", string.Join(", ", interactIds), state, arg3, arg4);
        foreach (FieldInteract interact in Field.EnumerateInteract()) {
            if (interactIds.Contains(interact.Value.Id)) {
                interact.SetState(state);
            }
        }
    }

    public void SetLadder(int triggerId, bool visible, bool animationEffect, int animationDelay) {
        DebugLog("[SetLadder] triggerId:{Id}, visible:{Visible}, animationEffect:{Effect}, animationDelay:{Delay}",
            triggerId, visible, animationEffect, animationDelay);
        if (!Objects.Ladders.TryGetValue(triggerId, out TriggerObjectLadder? ladder)) {
            return;
        }

        ladder.Visible = visible;
        ladder.Animate = animationEffect;
        ladder.Delay = animationDelay;
        Broadcast(TriggerPacket.Update(ladder));
    }

    public void SetMesh(int[] triggerIds, bool visible, int arg3, int delay, float scale) {
        DebugLog("[SetMesh] triggerIds:{Ids}, visible:{Visible}, arg3:{Arg3}, delay:{Delay}, scale:{Scale}",
            string.Join(", ", triggerIds), visible, arg3, delay, scale);
        UpdateMesh(triggerIds, visible, arg3, delay, scale);
    }

    // examples: arg3=200, arg4=3
    public void SetMeshAnimation(int[] triggerIds, bool visible, int arg3, byte arg4) {
        DebugLog("[SetMeshAnimation] triggerIds:{Ids}, visible:{Visible}, arg3:{Arg3}, arg4:{Arg4}", string.Join(", ", triggerIds), visible, arg3, arg4);
        foreach (int triggerId in triggerIds) {
            if (!Objects.Meshes.TryGetValue(triggerId, out TriggerObjectMesh? mesh)) {
                continue;
            }
            if (mesh.Visible == visible) {
                continue;
            }

            mesh.Visible = visible;
            mesh.Fade = arg3;
            Broadcast(TriggerPacket.Update(mesh));
        }
    }

    public void SetPortal(int portalId, bool visible, bool enabled, bool minimapVisible, bool arg5) {
        DebugLog("[SetPortal] portalId:{Id}, visible:{Visible}, enabled:{Enabled}, minimapVisible:{MinimapVisible}, arg5:{Arg5}",
            portalId, visible, enabled, minimapVisible, arg5);
        if (!Field.TryGetPortal(portalId, out FieldPortal? portal)) {
            return;
        }

        portal.Visible = visible;
        portal.Enabled = enabled;
        portal.MinimapVisible = minimapVisible;
        Broadcast(PortalPacket.Update(portal));
    }

    public void SetRandomMesh(int[] triggerIds, bool visible, int meshCount, int arg4, int delay) {
        DebugLog("[SetRandomMesh] triggerIds:{Ids}, visible:{Visible}, meshCount:{MeshCount}, arg4:{Arg4}, delay:{Delay}",
            string.Join(", ", triggerIds), visible, meshCount, arg4, delay);
        int count = triggerIds.Length;
        if (meshCount > 0 && meshCount < triggerIds.Length) {
            count = meshCount;
            Random.Shared.Shuffle(triggerIds);
        }

        UpdateMesh(new ArraySegment<int>(triggerIds, 0, count), visible, arg4, delay);
    }

    private void UpdateMesh(ArraySegment<int> triggerIds, bool visible, int fade, int delay, float scale = 0) {
        foreach (int triggerId in triggerIds) {
            if (!Objects.Meshes.TryGetValue(triggerId, out TriggerObjectMesh? mesh)) {
                logger.Warning("Invalid mesh: {Id}", triggerId);
                continue;
            }
            if (mesh.Visible == visible) {
                continue;
            }

            if (delay > 0) {
                Events.Schedule(() => UpdateSetMesh(mesh), delay);
            } else {
                UpdateSetMesh(mesh);
            }
        }

        void UpdateSetMesh(TriggerObjectMesh mesh) {
            mesh.Visible = visible;
            mesh.Fade = fade;
            if (scale != 0) {
                mesh.Scale = scale;
            }
            Broadcast(TriggerPacket.Update(mesh));
            // TODO: Should Fade be reset after sending packet?
        }
    }

    public void SetRope(int triggerId, bool visible, bool animationEffect, int animationDelay) {
        DebugLog("[SetRope] triggerId:{Id}, visible:{Visible}, animationEffect:{Effect}, animationDelay:{Delay}",
            triggerId, visible, animationEffect, animationDelay);
        if (!Objects.Ropes.TryGetValue(triggerId, out TriggerObjectRope? rope)) {
            return;
        }

        rope.Visible = visible;
        rope.Animate = animationEffect;
        rope.Delay = animationDelay;
        Broadcast(TriggerPacket.Update(rope));
    }

    public void SetSkill(int[] triggerIds, bool enabled) {
        ErrorLog("[SetSkill] triggerIds:{Ids}, enabled:{Enabled}", string.Join(", ", triggerIds), enabled);
    }

    public void SetSound(int triggerId, bool enabled) {
        DebugLog("[SetSound] triggerId:{Id}, enabled:{Enabled}", triggerId, enabled);
        if (!Objects.Sounds.TryGetValue(triggerId, out TriggerObjectSound? sound)) {
            return;
        }

        sound.Visible = enabled;
        Broadcast(TriggerPacket.Update(sound));
    }

    public void SetVisibleBreakableObject(int[] triggerIds, bool visible) {
        DebugLog("[SetVisibleBreakableObject] triggerIds:{Ids}, visible:{Visible}", string.Join(", ", triggerIds), visible);
        var updated = new List<FieldBreakable>(triggerIds.Length);
        foreach (int triggerId in triggerIds) {
            if (!Field.TryGetBreakable(triggerId, out FieldBreakable? breakable)) {
                continue;
            }

            breakable.Visible = visible;
            updated.Add(breakable);
        }

        Field.Broadcast(BreakablePacket.Update(updated));
    }

    public void AddBuff(int[] boxIds, int buffId, short level, bool isPlayer, bool isSkillSet, string feature) {
        DebugLog("[AddBuff] boxIds:{Ids}, buffId:{BuffId}, level:{Level}, isPlayer:{IsPlayer}, isSkillSet:{Arg5}, feature:{Feature}",
            string.Join(", ", boxIds), buffId, level, isPlayer, isSkillSet, feature);
        if (isSkillSet) {
            logger.Error("[AddBuff] SkillSets not implemented...");
            return;
        }

        if (isPlayer) {
            foreach (IActor player in PlayersInBox(boxIds)) {
                player.AddBuff(Field.FieldActor, player, buffId, level);
            }
        } else {
            foreach (IActor monster in MonstersInBox(boxIds)) {
                monster.AddBuff(Field.FieldActor, monster, buffId, level);
            }
        }
    }

    public void RemoveBuff(int boxId, int buffId, bool isPlayer) {
        ErrorLog("[RemoveBuff] boxId:{Id}, buffId:{BuffId}, isPlayer:{IsPlayer}", boxId, buffId, isPlayer);
        if (isPlayer) {
            foreach (IActor player in PlayersInBox(boxId)) {
                player.Buffs.Remove(buffId);
            }
        } else {
            foreach (IActor monster in MonstersInBox(boxId)) {
                monster.Buffs.Remove(buffId);
            }
        }
    }

    public void CreateItem(int[] spawnIds, int triggerId, int itemId, int arg5) {
        ErrorLog("[CreateItem] spawnIds:{Ids}, triggerId:{TriggerId}, itemId:{ItemId}, arg5:{Arg5}", string.Join(", ", spawnIds), triggerId, itemId, arg5);

        // Getting player to be able to create items. Should we move CreateItem elsewhere?
        FieldPlayer player = Field.Players.First().Value;
        foreach (int spawnId in spawnIds) {
            ICollection<Item> items = new List<Item>();
            if (itemId != 0) {
                Item? item = player.Session.Item.CreateItem(itemId);
                if (item != null) {
                    items.Add(item);
                }
            }

            if (!Field.Entities.EventItemSpawns.TryGetValue(spawnId, out EventSpawnPointItem? spawn)) {
                continue;
            }

            if (spawn.IndividualDropBoxId > 0) {
                items = items.Concat(player.Session.Item.GetIndividualDropBoxItems(spawn.IndividualDropBoxId)).ToList();
            }

            // TODO: Implement globalDropBoxId

            foreach (Item item in items) {
                FieldItem fieldItem = Field.SpawnItem(spawn.Position, spawn.Rotation, item, true);

                Field.Broadcast(FieldPacket.DropItem(fieldItem));
            }
        }
    }

    public void SpawnItemRange(int[] rangeIds, int randomPickCount) {
        ErrorLog("[SpawnItemRange] rangeIds:{Ids}, randomPickCount:{Count}", string.Join(", ", rangeIds), randomPickCount);
    }

    public void StartCombineSpawn(int[] groupIds, bool isStart) {
        ErrorLog("[StartCombineSpawn] groupIds:{Ids}, isStart:{IsStart}", string.Join(", ", groupIds), isStart);
    }

    public void SetTimer(string timerId, int seconds, bool autoRemove, bool display, int vOffset, string type) {
        DebugLog("[SetTimer] timerId:{Id}, seconds:{Seconds}", timerId, seconds);
        Field.Timers[timerId] = new TickTimer(seconds * 1000);
        if (display) {
            Broadcast(TriggerPacket.TimerDialog(Field.Timers[timerId]));
        }
    }

    public void ResetTimer(string timerId) {
        DebugLog("[ResetTimer] timerId:{Id}", timerId);
        if (Field.Timers.TryGetValue(timerId, out TickTimer? timer)) {
            timer.Reset();
            Broadcast(TriggerPacket.TimerDialog(timer));
        }
    }

    public void RoomExpire() {
        ErrorLog("[RoomExpire]");
    }

    public void FieldWarEnd(bool isClear) {
        ErrorLog("[FieldWarEnd] isClear:{IsClear}", isClear);
    }
    #endregion

    #region Conditions
    public bool DetectLiftableObject(int[] boxIds, int itemId) {
        ErrorLog("[DetectLiftableObject] boxIds:{Ids}, itemId:{ItemId}", string.Join(", ", boxIds), itemId);
        return false;
    }

    public bool ObjectInteracted(int[] interactIds, byte stateValue) {
        var state = (InteractState) stateValue;
        DebugLog("[ObjectInteracted] interactIds:{Ids}, state:{State}", string.Join(", ", interactIds), state);
        foreach (FieldInteract interact in Field.EnumerateInteract()) {
            if (interactIds.Contains(interact.Value.Id) && interact.State != state) {
                return false;
            }
        }

        return true;
    }

    public bool PvpZoneEnded(int boxId) {
        ErrorLog("[PvpZoneEnded] boxId:{BoxId}", boxId);
        return false;
    }

    public bool TimeExpired(string timerId) {
        DebugLog("[TimeExpired] timerId:{TimerId}", timerId);
        if (Field.Timers.TryGetValue(timerId, out TickTimer? timer)) {
            return timer.Expired();
        }

        return true;
    }
    #endregion
}
