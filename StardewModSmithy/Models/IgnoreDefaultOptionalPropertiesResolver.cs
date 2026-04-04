/*
https://github.com/Pathoschild/StardewXnbHack/blob/develop/StardewXnbHack/Framework/Writers/IgnoreDefaultOptionalPropertiesResolver.cs

The MIT License (MIT)

Copyright 2019 Pathoschild

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
using System.Reflection;
using Microsoft.Xna.Framework.Content;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Sickhead.Engine.Util;

namespace StardewModSmithy.Models;

/// <summary>A Json.NET contract resolver which ignores properties marked with <see cref="ContentSerializerIgnoreAttribute"/>, or (optionally) marked <see cref="ContentSerializerAttribute.Optional"/> with the default value.</summary>
/// <remarks>Construct an instance.</remarks>
/// <param name="omitDefaultValues">Whether to ignore members marked <see cref="ContentSerializerAttribute.Optional"/> which match the default value.</param>
internal class IgnoreDefaultOptionalPropertiesResolver(bool omitDefaultValues) : DefaultContractResolver
{
    /*********
    ** Fields
    *********/
    /// <summary>Whether to ignore members marked <see cref="ContentSerializerAttribute.Optional"/> which match the default value.</summary>
    private readonly bool OmitDefaultValues = omitDefaultValues;

    /// <summary>The default values for fields and properties marked <see cref="ContentSerializerAttribute.Optional"/>.</summary>
    private readonly Dictionary<string, Dictionary<string, object?>?> DefaultValues = [];

    /*********
    ** Protected methods
    *********/
    /// <inheritdoc />
    protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
    {
        JsonProperty property = base.CreateProperty(member, memberSerialization);

        // property marked ignore
        if (member.GetCustomAttribute<ContentSerializerIgnoreAttribute>() != null)
            property.ShouldSerialize = _ => false;
        // property marked optional which matches default value
        else if (this.OmitDefaultValues)
        {
            Dictionary<string, object?>? optionalMembers = this.GetDefaultValues(member.DeclaringType);
            if (optionalMembers != null && optionalMembers.TryGetValue(member.Name, out object? defaultValue))
            {
                property.ShouldSerialize = instance =>
                {
                    object value = member.GetValue(instance);
                    if (IsEmptyCollection(member, value))
                        return false;
                    return !defaultValue?.Equals(value) ?? value is not null;
                };
            }
        }

        return property;
    }

    /// <summary>check for generic type with property Count == 0</summary>
    /// <param name="member"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    private static bool IsEmptyCollection(MemberInfo member, object value)
    {
        if (value == null)
            return false;
        Type dataType = member.GetDataType();
        if (dataType == null || dataType == typeof(string) || !dataType.IsGenericType)
            return false;
        if ((dataType.GetProperty("Count")?.GetGetMethod()?.Invoke(value, []) as int?) == 0)
            return true;
        return false;
    }

    /// <summary>The default values for a type's fields and properties marked <see cref="ContentSerializerAttribute.Optional"/>, if any.</summary>
    /// <param name="type">The type whose fields and properties to get default values for.</param>
    /// <returns>Returns a dictionary of default values by member name if any were found, else <c>null</c>.</returns>
    private Dictionary<string, object?>? GetDefaultValues(Type? type)
    {
        // skip invalid
        if (
            type == null
            || !type.IsClass
            || type.FullName is null
            || type.Namespace?.StartsWith("StardewValley") != true
        )
            return null;

        // skip if already cached
        if (DefaultValues.TryGetValue(type.FullName, out Dictionary<string, object?>? defaults))
            return defaults;

        // get members
        MemberInfo[] optionalMembers = (type.GetFields().OfType<MemberInfo>())
            .Concat(type.GetProperties())
            .Where(member => member.GetCustomAttribute<ContentSerializerAttribute>()?.Optional is true)
            .ToArray();
        if (optionalMembers.Length == 0)
            return DefaultValues[type.FullName] = null;

        // get default instance
        object? defaultInstance;
        try
        {
            defaultInstance = Activator.CreateInstance(type);
        }
        catch
        {
            return this.DefaultValues[type.FullName] = null;
        }

        // get default values
        defaults = new Dictionary<string, object?>();
        foreach (MemberInfo member in optionalMembers)
            defaults[member.Name] = member.GetValue(defaultInstance);
        return this.DefaultValues[type.FullName] = defaults;
    }
}
