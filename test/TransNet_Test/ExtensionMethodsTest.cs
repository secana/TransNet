using TransNet;
using Xunit;

namespace TransNet_Test
{
    public class ExtensionMethodsTest
    {
        [Fact]
        public void AddEntity_ValidEntity_AddsEntityToTransform()
        {
            var transform = new Transformation(new[] {"EntityValue", "field1=value1#field2=value2"});

            transform.AddEntity("EntityType", "EntityValue", 2);

            Assert.Equal("EntityType", transform.Entities[0].EntityType);
            Assert.Equal("EntityValue", transform.Entities[0].Value);
            Assert.Equal(2, transform.Entities[0].Weight);
        }

        [Fact]
        public void AddAdditionalField_ValidField_AddsFieldToEntity()
        {
            var entity = new Entity("EntityType", "EntityValue");

            entity.AddAdditionalField("Name", "DisplayName", "Value", MatchingRule.Strict);

            Assert.Equal("Name", entity.AdditionalFields[0].Name);
            Assert.Equal("DisplayName", entity.AdditionalFields[0].DisplayName);
            Assert.Equal("Value", entity.AdditionalFields[0].Value);
            Assert.Equal("strict", entity.AdditionalFields[0].MatchingRule);
        }
    }
}
