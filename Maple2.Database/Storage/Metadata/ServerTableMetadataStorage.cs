﻿using System;
using Maple2.Database.Context;
using Maple2.Model.Metadata;

namespace Maple2.Database.Storage;

public class ServerTableMetadataStorage {
    private readonly Lazy<InstanceFieldTable> instanceFieldTable;
    private readonly Lazy<ScriptConditionTable> scriptConditionTable;
    private readonly Lazy<ScriptFunctionTable> scriptFunctionTable;
    private readonly Lazy<JobConditionTable> jobConditionTable;
    private readonly Lazy<BonusGameTable> bonusGameTable;

    public InstanceFieldTable InstanceFieldTable => instanceFieldTable.Value;
    public ScriptConditionTable ScriptConditionTable => scriptConditionTable.Value;
    public ScriptFunctionTable ScriptFunctionTable => scriptFunctionTable.Value;
    public JobConditionTable JobConditionTable => jobConditionTable.Value;
    public BonusGameTable BonusGameTable => bonusGameTable.Value;

    public ServerTableMetadataStorage(MetadataContext context) {
        instanceFieldTable = Retrieve<InstanceFieldTable>(context, "instancefield.xml");
        scriptConditionTable = Retrieve<ScriptConditionTable>(context, "*scriptCondition.xml");
        scriptFunctionTable = Retrieve<ScriptFunctionTable>(context, "*scriptFunction.xml");
        jobConditionTable = Retrieve<JobConditionTable>(context, "jobConditionTable.xml");
        bonusGameTable = Retrieve<BonusGameTable>(context, "bonusGame*.xml");
    }

    private static Lazy<T> Retrieve<T>(MetadataContext context, string key) where T : ServerTable {
        var result = new Lazy<T>(() => {
            lock (context) {
                ServerTableMetadata? row = context.ServerTableMetadata.Find(key);
                if (row?.Table is not T result) {
                    throw new InvalidOperationException($"Row does not exist: {key}");
                }

                return result;
            }
        });

#if !DEBUG
        // No lazy loading for RELEASE build.
        _ = result.Value;
#endif
        return result;
    }
}
