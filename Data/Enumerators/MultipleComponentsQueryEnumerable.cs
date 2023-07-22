﻿namespace ModulesFramework.Data.Enumerators
{
    public readonly struct MultipleComponentsQueryEnumerable<T> where T : struct
    {
        private readonly EcsTable<T> _table;
        private readonly bool[] _filter;

        public MultipleComponentsQueryEnumerable(EcsTable<T> table, bool[] filter)
        {
            _table = table;
            _filter = filter;
        }
        
        public MultipleComponentsQueryEnumerator<T> GetEnumerator()
        {
            return new MultipleComponentsQueryEnumerator<T>(_table, _filter);
        }
    }
}