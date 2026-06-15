using System;
using System.Collections.Generic;
using LiteEntitySystem;
using LiteEntitySystem.Internal;

namespace ScavMP.Shared
{
    public sealed class RuntimeEntityTypesMap : EntityTypesMap
    {
        bool _lock = false;
        public static RuntimeEntityTypesMap Instance;

        public RuntimeEntityTypesMap Register<TEntity>(
            ushort id,
            EntityConstructor<TEntity> constructor
        )
            where TEntity : InternalEntity
        {
            if (_lock)
                throw new Exception(
                    "TypeMap is locked. Cannot register new entities after initialization."
                );
            var typeInfo = new RegisteredTypeInfo(id, id.ToString(), constructor);
            if (RegisteredTypes.ContainsKey(typeof(TEntity)))
                throw new Exception(
                    $"Type {typeof(TEntity).Name} is already registered in TypeMap"
                );
            RegisteredTypes.Add(typeof(TEntity), typeInfo);
            return this;
        }

        public void Lock()
        {
            _lock = true;
        }
    }

    public static class NetworkGeneral
    {
        public const int GameFPS = 30;
    }
}
