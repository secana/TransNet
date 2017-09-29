using System;
using TransNet;
using Xunit;

namespace TransNet_Test
{
    public class EntityTest
    {
        [Fact]
        public void Entity_ArgumentEntityTypeIsNull_ThrowsArgumentNullException()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => new Entity(null, "value", 0));
        }

        [Fact]
        public void Entity_ArgumentValueIsNull_ThrowsArgumentNullException()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => new Entity("entityType" , null, 0));
        }

        [Fact]
        public void Entity_CorrectArguments_SetsAllProperties()
        {
            var entity = new Entity("entityType", "value", 10);

            Assert.Equal("entityType", entity.EntityType);
            Assert.Equal("value", entity.Value);
            Assert.Equal(10, entity.Weight);
            Assert.NotNull(entity.AdditionalFields);
        }

        [Fact]
        public void AddEdgeLabel_CorrectArguments_AddsEdgeToAdditionalFields()
        {
            var entity = new Entity("entityType", "value", 10);

            entity.AddEdgeLabel("label", new System.Collections.Generic.List<Tuple<string, string>> { new Tuple<string, string>("key", "value") });

            Assert.True(entity.HasEdgeLabel);
            Assert.Equal(3, entity.AdditionalFields.Count);
            Assert.Equal("link#maltego.link.label", entity.AdditionalFields[0].Name);
            Assert.Equal("loose", entity.AdditionalFields[0].MatchingRule);
            Assert.Equal("label", entity.AdditionalFields[0].Value);
            Assert.Equal("link#maltego.link.show-label", entity.AdditionalFields[1].Name);
            Assert.Equal("loose", entity.AdditionalFields[1].MatchingRule);
            Assert.Equal("1", entity.AdditionalFields[1].Value);
            Assert.Equal("link#0", entity.AdditionalFields[2].Name);
            Assert.Equal("loose", entity.AdditionalFields[2].MatchingRule);
            Assert.Equal("value", entity.AdditionalFields[2].Value);
        }

        [Fact]
        public void AddEdgeLabel_EdgeLabelAlreadySet_ThrowsInvalidOperationException()
        {
            var entity = new Entity("entityType", "value", 10);
            entity.AddEdgeLabel("label", new System.Collections.Generic.List<Tuple<string, string>> { new Tuple<string, string>("key", "value") });

            Assert.Throws<InvalidOperationException>(() => entity.AddEdgeLabel("label2", new System.Collections.Generic.List<Tuple<string, string>> { new Tuple<string, string>("key", "value") }));
        }

        [Fact]
        public void ToXML_FullEntity_CreatesCorrectXMLOutput()
        {
            var expected = "<Entity Type=\"entityType\"><Value>value</Value><Weight>10</Weight><AdditionalFields><Field Name=\"name\" DisplayName=\"displayName\" MatchingRule=\"strict\">value</Field><Field Name=\"link#maltego.link.label\" MatchingRule=\"loose\">label</Field><Field Name=\"link#maltego.link.show-label\" MatchingRule=\"loose\">1</Field><Field Name=\"link#0\" DisplayName=\"key\" MatchingRule=\"loose\">value</Field></AdditionalFields></Entity>";
            var entity = new Entity("entityType", "value", 10);
            entity.AdditionalFields.Add(new Entity.AdditionalField("name", "displayName", "value", MatchingRule.Strict));
            entity.AddEdgeLabel("label", new System.Collections.Generic.List<Tuple<string, string>> { new Tuple<string, string>("key", "value") });

            var xml = entity.TransformToXML();

            Assert.Equal(expected, xml);
        }

        [Fact]
        public void AdditionalField_NameIsNull_ThrowsArgumentNullException()
        {
           Assert.Throws<ArgumentNullException>(() => new Entity.AdditionalField(null, "displayName", "value"));
        }
    }
}
