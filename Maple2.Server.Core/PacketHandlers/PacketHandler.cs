﻿using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Network;
using Serilog;

namespace Maple2.Server.Core.PacketHandlers;

// All implementing classes should be thread safe and stateless.
// All state should be stored in Session
public abstract class PacketHandler<T> where T : Session {
    public abstract ushort OpCode { get; }

    protected readonly ILogger Logger = Log.Logger.ForContext<T>();

    protected PacketHandler() { }

    public abstract void Handle(T session, IByteReader packet);

    public override string ToString() => $"[0x{OpCode:X4}] {GetType().Name}";
}
