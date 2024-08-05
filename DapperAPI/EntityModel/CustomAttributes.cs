namespace DapperAPI.EntityModel
{
    public class CustomAttributes
    {
        [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
        public sealed class PrimaryKeyAttribute : Attribute { }

        [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
        public sealed class ForeignKeyAttribute : Attribute
        {
            public string TableName { get; }
            public string ColumnName { get; }
            public Type ModelType { get; } // Add this property

            public ForeignKeyAttribute(string tableName, string columnName, Type modelType)
            {
                TableName = tableName;
                ColumnName = columnName;
                ModelType = modelType;
            }
        }
        [AttributeUsage(AttributeTargets.Property)]
        public class SequenceKeyAttribute : Attribute
        {
        }

        public class TableAttribute : Attribute
        {
            public string Name { get; set; }
        }
    }
}
