﻿using commercetools.Sdk.Domain;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;

namespace commercetools.Sdk.Serialization.Tests
{
    public class CustomFieldsDeserializationTests
    {
        [Fact]
        public void CustomFieldsString()
        {
            ISerializerService serializerService = TestUtils.GetSerializerService();
            string serialized = File.ReadAllText("Resources/CustomFields/String.json");
            Category deserialized = serializerService.Deserialize<Category>(serialized);
            Assert.IsType<string>(deserialized.Custom.Fields["string"]);
        }

        [Fact]
        public void CustomFieldsNumber()
        {
            ISerializerService serializerService = TestUtils.GetSerializerService();
            string serialized = File.ReadAllText("Resources/CustomFields/Number.json");
            Category deserialized = serializerService.Deserialize<Category>(serialized);
            Assert.IsType<double>(deserialized.Custom.Fields["number"]);
        }

        [Fact]
        public void CustomFieldsEnum()
        {
            ISerializerService serializerService = TestUtils.GetSerializerService();
            string serialized = File.ReadAllText("Resources/CustomFields/Enum.json");
            Category deserialized = serializerService.Deserialize<Category>(serialized);
            Assert.IsType<EnumValue>(deserialized.Custom.Fields["enum"]);
        }

        [Fact]
        public void CustomFieldsSetEnum()
        {
            ISerializerService serializerService = TestUtils.GetSerializerService();
            string serialized = File.ReadAllText("Resources/CustomFields/SetEnum.json");
            Category deserialized = serializerService.Deserialize<Category>(serialized);
            Assert.IsType<Set<EnumValue>>(deserialized.Custom.Fields["enum"]);
        }

        [Fact]
        public void CustomFieldsList()
        {
            ISerializerService serializerService = TestUtils.GetSerializerService();
            string serialized = File.ReadAllText("Resources/CustomFields/List.json");
            Category deserialized = serializerService.Deserialize<Category>(serialized);
            Assert.Equal(3, deserialized.Custom.Fields.Count);
        }
    }
}