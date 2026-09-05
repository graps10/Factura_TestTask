using System;
using System.Reflection;
using UnityEngine;

namespace TurretRush.Tests.PlayMode
{
    /// <summary>
    /// Builds configs for tests by reflection, rather than adding setters or a test
    /// constructor to shipping code. Renaming a serialised field breaks these calls,
    /// which is correct rather than fragile - it already breaks every .asset storing
    /// that field, and the test just says so sooner.
    ///
    /// Only overrides are passed; everything else keeps the config's own default, so
    /// a fixture states exactly what its test depends on.
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
