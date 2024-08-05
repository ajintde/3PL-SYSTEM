using System.Reflection;

namespace DapperAPI.EntityModel
{
    public static class AttributeHelper
    {
        public static T GetCustomAttribute<T>(PropertyInfo propertyInfo) where T : Attribute
        {
            return (T)propertyInfo.GetCustomAttributes(typeof(T), false).FirstOrDefault();
        }
    }
}
