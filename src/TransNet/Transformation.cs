using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace TransNet
{
    /// <summary>
    /// This class provides the capabilities to read and write
    /// Maltego transformations.
    /// </summary>
    public class Transformation : ITransformable
    {
        /// <summary>
        /// List with input arguments from the command line.
        /// </summary>
        public Dictionary<string, string> InputArguments { get; }

        /// <summary>
        ///     The entity value of the input to the transformation.
        ///     This is equal to the first STDIN argument and the main
        ///     property of the input entity.
        /// </summary>
        public string EntityValue => InputArguments["EntityValue"];

        /// <summary>
        ///     List with entities to return to Maltego based on
        ///     the input of the transformation.
        /// </summary>
        public readonly List<Entity> Entities = new List<Entity>();

        private string _optionalParameter = null;

        /// <summary>
        ///     Create a representation of a Maltego transformation.
        /// </summary>
        /// <param name="args">Command line arguments given to a transformation application by Maltego.</param>
        public Transformation(string[] args)
        {
            InputArguments = ParseSTDIN(args);
        }

        /// <summary>
        ///     Create a representation of a Maltego transformation.
        /// </summary>
        /// <param name="xml">Maltego transformation XML</param>
        public Transformation(string xml)
        {
            ParseTransformOutput(xml);
        }

        /// <summary>
        /// Convert the whole transformation into an XML readable by Maltego.
        /// </summary>
        /// <returns>Maltego readable XML.</returns>
        public string TransformToXML()
        {
            var settings = new XmlWriterSettings {OmitXmlDeclaration = true};

            using (var sw = new StringWriter())
            {
                using (var xw = XmlWriter.Create(sw, settings))
                {
                    xw.WriteStartDocument();
                    xw.WriteStartElement("MaltegoMessage");
                    xw.WriteStartElement("MaltegoTransformResponseMessage");
                    xw.WriteStartElement("_entities");

                    foreach (var e in Entities)
                    {
                        xw.WriteRaw(e.TransformToXML());
                    }

                    xw.WriteEndElement();
                    xw.WriteEndElement();
                    xw.WriteEndElement();
                    xw.WriteEndDocument();
                }
                return sw.ToString();
            }
        }

        private void ParseTransformOutput(string xml)
        {
            var doc = XDocument.Parse(xml);

            var entities = doc.Descendants("Entity");
            foreach (var entity in entities)
            {
                var entityType = entity.Attribute("Type").Value;
                var value = entity.Element("Value").Value;
                var weight = Convert.ToInt32(entity.Element("Weight").Value);

                var newEntity = new Entity(entityType, value, weight);

                var addFields = entity.Element("AdditionalFields");
                foreach (var field in addFields.Elements())
                {
                    var attributes = field.Attributes().ToList();
                    var name = attributes.FirstOrDefault(x => x.Name == "Name")?.Value;
                    var dname = attributes.FirstOrDefault(x => x.Name == "DisplayName")?.Value;
                    var mr = (MatchingRule) Enum.Parse(typeof(MatchingRule), attributes.FirstOrDefault(x => x.Name == "MatchingRule")?.Value, true);


                    newEntity.AdditionalFields.Add(new Entity.AdditionalField(
                        name,
                        dname,
                        field.Value,
                        mr
                    ));
                }

                Entities.Add(newEntity);
            }
        }

        /// <summary>
        ///     Prints a debug message to Maltego.
        /// </summary>
        /// <param name="message">Message to display as debug in Maltego.</param>
        public void PrintDebug(string message)
        {
            Console.Error.WriteLine("D:{0}", message);
        }

        /// <summary>
        ///     Sets the progress bar in Maltego.
        /// </summary>
        /// <param name="percentage">Number of percent to set the progress bar to.</param>
        public void PrintProgess(int percentage)
        {
            if (percentage < 0 || percentage > 100)
                throw new ArgumentOutOfRangeException(nameof(percentage), "Percentage has to be in range 0-100.");

            Console.Error.WriteLine("% {0}", percentage);
        }


        /// <summary>
        ///     Split the STDIN arguments and return them as a dictionary.
        ///     The first argument is mandatory and represents the "EntityValue".
        ///     The optional second argument is a string with additional fields of the
        ///     format "field1=value1#field2=value2 ...".
        /// </summary>
        /// <param name="args">Command line arguments of an transformation application.</param>
        /// <returns>Dictionary with all key/value pairs of the command line arguments.</returns>
        private Dictionary<string, string> ParseSTDIN(string[] args)
        {
            var dictionary = new Dictionary<string, string>();

            if (args.Length == 0 || args.Length > 3)
                throw new ArgumentOutOfRangeException(nameof(args),
                    "Wrong number of arguments. Only 1-3 arguments are allowed.");

            // Add the first optional argument if it exists
            _optionalParameter = args.Length == 3 ? args[0] : null;

            // Add the mandatory argument which is the entity value.
            var evPos = args.Length == 2 ? 0 : 1;
            dictionary.Add("EntityValue", args[evPos]);

            // If a second/third argument exists, it is of the form "field1=value1#field2=value2 ..."
            if (args.Length >= 2)
            {
                var pair = args[args.Length - 1].Split('#');
                foreach (var p in pair)
                {
                    var keyValue = p.Split('=');
                    if (keyValue.Length != 2)
                        throw new ArgumentException("Secondary argument cannot be split at \"=\". Format error.");
                    dictionary.Add(keyValue[0], keyValue[1]);
                }
            }

            return dictionary;
        }
    }
}