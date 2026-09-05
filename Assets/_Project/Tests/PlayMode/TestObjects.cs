using System;
using System.Reflection;
using UnityEngine;

namespace TurretRush.Tests.PlayMode
{
    /// <summary>
    /// Builds configs for tests.
    ///
    /// Config assets keep their fields private and expose read-only properties,
    /// which is right for production and inconvenient here. The alternative - adding
    /// setters or a test constructor to every config - would put test scaffolding in
    /// shipping code, so the test assembly reaches in with reflection instead.
    ///
    /// Renaming a serialised field breaks these calls. That is correct rather than
    /// fragile: renaming a serialised field already breaks every .asset that stores
    /// it, so the test failing is the same warning arriving earlier.
    ///
    /// Only overrides are passed. Everything else keeps the field initialiser the
    /// config declares, so a fixture states exactly what the test cares about.
    /// </summary>
    internal static class TestObjects
    {
        public static T Config<T>(params (string Field, object Value)[] overrides)
            where T : ScriptableObject
        {
            var config = ScriptableObject.CreateInstance<T>();

            foreach (var (field, value) in overrides)
                SetField(config, field, value);

            return config;
        }

        public static void SetField(object target, string field, object value)
        {
            var info = target.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic);

            if (info == null)
                throw new MissingFieldException(target.GetType().Name, field);

            info.SetValue(target, value);
        }

        public static int RequireLayer(string name)
        {
            var layer = LayerMask.NameToLayer(name);

            if (layer < 0)
                throw new InvalidOperationException($"The project is missing the '{name}' layer.");

            return layer;
        }
    }
}
