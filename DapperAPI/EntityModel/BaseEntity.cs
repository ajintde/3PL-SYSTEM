namespace DapperAPI.EntityModel
{
    public abstract class BaseEntity
    {
        //public abstract string StoreProcedureName { get; }
        //public abstract string inserSqlTemplate { get; }
        //public abstract string updateSqlTemplate { get; }
        //public abstract string tableName { get; }
        //public abstract string primaryColumnName { get; }

        private static readonly Dictionary<string, BaseEntity> _entities
            = new Dictionary<string, BaseEntity>();

        public static BaseEntity CreateEmptyInstances<T>()
        {
            var key=typeof(T).FullName;
            if(!_entities.ContainsKey(key))
            {
                var obj=Activator.CreateInstance<T>() as BaseEntity;
                _entities.Add(key, obj);
            }
            return _entities[key];
        }
    }
}
