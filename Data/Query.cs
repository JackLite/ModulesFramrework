﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ModulesFramework.Data.Enumerators;
using ModulesFramework.Exceptions;

namespace ModulesFramework.Data
{
    public partial class DataWorld
    {
        public sealed class Query : IDisposable
        {
            private readonly DataWorld _world;

            private EcsTable _mainTable;
            private bool[] _inc;
            
            public Query(DataWorld world)
            {
                _world = world;
            }

            internal void Init(EcsTable table)
            {
                _mainTable = table;
                _inc = new bool[_mainTable.EntitiesData.Length];
                for (var i = 0; i < _inc.Length; ++i)
                    _inc[i] = _mainTable.EntitiesData[i].isActive;
            }

            public void Dispose()
            {
                _world.ReturnQuery(this);
            }

            public Query With<T>() where T : struct
            {
                var table = _world.GetEscTable<T>();
                for (var i = 0; i < _inc.Length; ++i)
                    _inc[i] &= table.Contains(i);

                return this;
            }

            public Query Without<T>() where T : struct
            {
                var table = _world.GetEscTable<T>();
                for (var i = 0; i < _inc.Length; ++i)
                    _inc[i] &= !table.Contains(i);

                return this;
            }

            public Query Where<T>(Func<T, bool> customFilter) where T : struct
            {
                var table = _world.GetEscTable<T>();
                for (var i = 0; i < _inc.Length; ++i)
                    _inc[i] &= customFilter.Invoke(table.GetData(i));

                return this;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public EntitiesEnumerable GetEntities()
            {
                return new EntitiesEnumerable(_mainTable.EntitiesData, _inc, _world);
            }
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public EntityDataEnumerable GetEntitiesId()
            {
                return new EntityDataEnumerable(_mainTable.EntitiesData, _inc);
            }

            public bool Any()
            {
                foreach (var _ in GetEntitiesId())
                    return true;

                return false;
            }

            public bool TrySelectFirst<TRet>(out TRet c) where TRet : struct
            {
                foreach (var eid in GetEntitiesId())
                {
                    c = _world.GetComponent<TRet>(eid);
                    return true;
                }

                c = new TRet();
                return false;
            }

            public ref TRet SelectFirst<TRet>() where TRet : struct
            {
                foreach (var eid in GetEntitiesId())
                {
                    if (_world.HasComponent<TRet>(eid))
                        return ref _world.GetComponent<TRet>(eid);
                }

                throw new QuerySelectException<TRet>();
            }

            public bool TrySelectFirstEntity(out Entity e)
            {
                e = new Entity();
                foreach (var entity in GetEntities())
                {
                    e = entity;
                    return true;
                }

                return false;
            }

            public Entity SelectFirstEntity()
            {
                foreach (var entity in GetEntities())
                {
                    return entity;
                }
                
                throw new QuerySelectEntityException();
            }

            public void DestroyAll()
            {
                foreach (var eid in GetEntitiesId())
                {
                    _world.DestroyEntity(eid);
                }
            }

            public int Count()
            {
                var count = 0;
                foreach (var _ in GetEntitiesId())
                {
                    count++;
                }
                
                return count;
            }
        }
    }
}