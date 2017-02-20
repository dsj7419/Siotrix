namespace GynBot
{
    using Interfaces;
    using System;
    using System.Reflection;

    public static partial class ConfigHandler
    {

        internal static class ConfigBuilder
        {

            internal static T CreateBotConfig<T>() where T : IBotConfig, new()
            {
                object boxedConfig = new T();
                foreach (var prop in typeof(T).GetRuntimeProperties())
                {
                    if ((prop.PropertyType != typeof(string)) || (prop.PropertyType != typeof(ulong)))
                    {
                        prop.SetValue(boxedConfig, Activator.CreateInstance(prop.PropertyType));
                        continue;
                    }
                    Console.WriteLine($"Input the value for property {prop.Name}:");
                    var propValue = Console.ReadLine();
                    prop.SetValue(boxedConfig, Convert.ChangeType(propValue, prop.PropertyType));
                }
                return (T)boxedConfig;
            }
        }
    }
}