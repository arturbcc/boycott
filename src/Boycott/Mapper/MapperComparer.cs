﻿namespace Boycott.Mapper {
    using System;

    public class MapperComparer {
        private ObjectMapper objectMapper;
        private TableMapper tableMapper;
        public string Name { get; private set; }

        public MapperComparer(Type type) {
            Name = type.Name;
            objectMapper = new ObjectMapper(type);
            tableMapper = new TableMapper(objectMapper.TableName);
        }

        public bool Check() {
            if (objectMapper.IsSynchronizable) {
                if (!tableMapper.TableExists)
                    return true;
                return !objectMapper.Equals(tableMapper);
            }

            return false;
        }

        public TableDiff GetDiff() {
            var diff = new TableDiff(objectMapper.TableName);

            if (objectMapper.IsSynchronizable) {
                foreach (var item in objectMapper.Columns) {
                    if (!tableMapper.Columns.Contains(item) && item.IsSynchronizable) {
                        diff.AddedColumns.Add(item);
                    }
                }
                
                diff.NewTable = !tableMapper.TableExists;
                
                foreach (var item in tableMapper.Columns) {
                    if (!objectMapper.Columns.Contains(item) && (objectMapper.IgnoreColumns == null || !objectMapper.IgnoreColumns.Contains(item.Name))) {
                        diff.RemovedColumns.Add(item);
                    }
                }
            }
            
            return diff;
        }

        public bool Sync() {
            if (!Check())
                return false;
            
            var cmds = GetDiff().ToSql();
            
            foreach (var query in cmds) {
                Configuration.DatabaseProvider.ExecuteNonQuery(query);
            }
            
            return true;
        }
    }
}
