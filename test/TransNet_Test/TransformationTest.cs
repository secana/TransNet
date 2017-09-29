using System;
using System.IO;
using TransNet;
using Xunit;

namespace TransNet_Test
{
    public class TransformationTest
    {
        [Fact]
        public void Transformation_CorrectCommandLineArgs_SetsAllProperties()
        {
            var transform = new Transformation(new []{"EntityValue", "field1=value1#field2=value2"});

            Assert.Equal("EntityValue", transform.EntityValue);
            Assert.Equal(3, transform.InputArguments.Count);
            Assert.Equal("EntityValue", transform.InputArguments["EntityValue"]);
            Assert.Equal("value1", transform.InputArguments["field1"]);
            Assert.Equal("value2", transform.InputArguments["field2"]);
        }

        [Fact]
        public void Transformation_NullArguments_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Transformation(new string []{}));
        }

        [Fact]
        public void Transformation_MoreThanTheeArguments_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Transformation(new[] { "1", "2", "3", "4" }));
        }

        [Fact]
        public void Transformation_WrongArgumentFormat_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new Transformation(new[] {"1", "2", "wrongFormant"}));
        }

        [Fact]
        public void TransformToXML_ValidInput_ReturnsXMLTransform()
        {
            var transform = new Transformation(new [] {"EntityValue", "field1=value1#field2=value2"});
            transform.Entities.Add(GetExampleEntity());

            var actual = transform.TransformToXML();

            Assert.Equal(GetExampleXML(), actual);
        }

        [Fact]
        public void Transformation_ValidInputXML_ParsesXML()
        {
            var transform = new Transformation(GetExampleXML());   

            Assert.Equal("EntityType", transform.Entities[0].EntityType);
            Assert.Equal("Value", transform.Entities[0].Value);
            Assert.Equal(1, transform.Entities[0].Weight);
            Assert.Equal("AdditionalField", transform.Entities[0].AdditionalFields[0].Name);
            Assert.Equal("DisplayName", transform.Entities[0].AdditionalFields[0].DisplayName);
            Assert.Equal("Value", transform.Entities[0].AdditionalFields[0].Value);
            Assert.Equal("strict", transform.Entities[0].AdditionalFields[0].MatchingRule);
        }

        [Fact]
        public void PrintDebug_Message_LogsDebugMessage()
        {
            var transform = new Transformation(new[] { "EntityValue", "field1=value1#field2=value2" });
            var sw = new StringWriter();
            Console.SetError(sw);

            transform.PrintDebug("Debug message.");
            var actual = sw.ToString().TrimEnd();

            Assert.Equal("D:Debug message.", actual);
        }

        [Fact]
        public void PrintProgess_Percentage_LogsPercentage()
        {
            var transform = new Transformation(new[] { "EntityValue", "field1=value1#field2=value2" });
            var sw = new StringWriter();
            Console.SetError(sw);

            transform.PrintProgess(50);
            var actual = sw.ToString().TrimEnd();

            Assert.Equal("% 50", actual);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(101)]
        public void PrintProgress_InvalidPercentage_ThrowsArgumentOutOfRangeException(int percentage)
        {
            var transform = new Transformation(new[] { "EntityValue", "field1=value1#field2=value2" });

            Assert.Throws<ArgumentOutOfRangeException>(() => transform.PrintProgess(percentage));
        }

        private Entity GetExampleEntity()
        {
            var entity = new Entity("EntityType", "Value", 1);
            entity.AdditionalFields.Add(
                new Entity.AdditionalField("AdditionalField", "DisplayName", "Value", MatchingRule.Strict));

            return entity;
        }

        private string GetExampleXML()
        {
            return
                "<MaltegoMessage><MaltegoTransformResponseMessage><_entities><Entity Type=\"EntityType\"><Value>Value</Value><Weight>1</Weight><AdditionalFields><Field Name=\"AdditionalField\" DisplayName=\"DisplayName\" MatchingRule=\"strict\">Value</Field></AdditionalFields></Entity></_entities></MaltegoTransformResponseMessage></MaltegoMessage>";
        }
    }
}
