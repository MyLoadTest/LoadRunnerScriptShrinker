using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using ICSharpCode.Core;
using Omnifactotum;
using Omnifactotum.Annotations;

namespace MyLoadTest.LoadRunnerScriptShrinker.UI.AddIn
{
    internal sealed class PropertyPersistor<TObject>
        where TObject : class
    {
        #region Constants and Fields

        private readonly Dictionary<PropertyInfo, PersistenceInfo> _registrations;

        #endregion

        #region Constructors

        public PropertyPersistor()
        {
            _registrations = new Dictionary<PropertyInfo, PersistenceInfo>();
        }

        #endregion

        #region Public Methods

        public void RegisterProperty<TProperty>(
            [NotNull] Expression<Func<TObject, TProperty>> propertyGetterExpression,
            TProperty defaultPropertyValue = default(TProperty))
        {
            #region Argument Check

            if (propertyGetterExpression == null)
            {
                throw new ArgumentNullException(nameof(propertyGetterExpression));
            }

            #endregion

            var propertyInfo = Factotum.For<TObject>.GetPropertyInfo(propertyGetterExpression);
            if (!propertyInfo.CanRead || !propertyInfo.CanWrite)
            {
                throw new ArgumentException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        @"The property must be both readable and writable ({0}).",
                        propertyInfo.Name),
                    nameof(propertyGetterExpression));
            }

            var key = GetPropertyKey(propertyInfo);

            var persistenceInfo = new PersistenceInfo
            {
                ReadProperty = obj => ReadPropertyFromSettings(obj, key, propertyInfo, defaultPropertyValue),
                WriteProperty = obj => WritePropertyToSettings<TProperty>(obj, key, propertyInfo)
            };

            _registrations.Add(propertyInfo, persistenceInfo);
        }

        public void LoadProperties(TObject @object)
        {
            #region Argument Check

            if (@object == null)
            {
                throw new ArgumentNullException(nameof(@object));
            }

            #endregion

            foreach (var persistenceInfo in _registrations.Values)
            {
                persistenceInfo.ReadProperty(@object);
            }
        }

        public void SaveProperties(TObject @object)
        {
            #region Argument Check

            if (@object == null)
            {
                throw new ArgumentNullException(nameof(@object));
            }

            #endregion

            foreach (var persistenceInfo in _registrations.Values)
            {
                persistenceInfo.WriteProperty(@object);
            }
        }

        #endregion

        #region Private Methods

        private static string GetPropertyKey(PropertyInfo propertyInfo)
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0}{1}{2}",
                propertyInfo.DeclaringType.EnsureNotNull().GetFullName(),
                Type.Delimiter,
                propertyInfo.Name);
        }

        private static void ReadPropertyFromSettings<TProperty>(
            TObject obj,
            string key,
            PropertyInfo propertyInfo,
            TProperty defaultPropertyValue)
        {
            var value = PropertyService.Get(key, defaultPropertyValue);
            propertyInfo.SetValue(obj, value);
        }

        private static void WritePropertyToSettings<TProperty>(TObject obj, string key, PropertyInfo propertyInfo)
        {
            var value = (TProperty)propertyInfo.GetValue(obj);
            PropertyService.Set(key, value);
        }

        #endregion

        #region PersistenceInfo Class

        private sealed class PersistenceInfo
        {
            public Action<TObject> ReadProperty
            {
                get;
                set;
            }

            public Action<TObject> WriteProperty
            {
                get;
                set;
            }
        }

        #endregion
    }
}