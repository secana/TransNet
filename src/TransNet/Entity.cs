using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace TransNet
{
    public class Entity : ITransformable
    {
        public string EntityType { get; }
        public string Value { get; }
        public int Weight { get; }
        public List<AdditionalField> AdditionalFields { get; }
        public bool HasEdgeLabel { get; private set; }

        public Entity(string entityType, string value, int weight = 0)
        {
            EntityType = entityType ?? throw new ArgumentNullException(nameof(entityType), "EntityType cannot be null.");
            Value = value ?? throw new ArgumentNullException(nameof(value), "Value cannot be null.");
            Weight = weight;
            AdditionalFields = new List<AdditionalField>();
        }

        /// <summary>
        ///     Adds an edge label and optional edge properties to the edge.
        /// </summary>
        /// <param name="label">Label of the edge</param>
        /// <param name="properties">
        ///     Properties of the edge as a tuple where Item1 is the property name
        ///     and Item2 is the property value.
        /// </param>
        public void AddEdgeLabel(string label, List<Tuple<string, string>> properties = null)
        {
            if (HasEdgeLabel)
                throw new InvalidOperationException("Only one edge label per edge is allowed!");

            AdditionalFields.Add(new AdditionalField("link#maltego.link.label", null, label));
            AdditionalFields.Add(new AdditionalField("link#maltego.link.show-label", null, "1"));

            if (properties != null)
            {
                for (var i = 0; i < properties.Count; i++)
                {
                    AdditionalFields.Add(new AdditionalField("link#" + i, properties[i].Item1, properties[i].Item2));
                }
            }

            HasEdgeLabel = true;
        }

        public string TransformToXML()
        {
            var settings = new XmlWriterSettings {OmitXmlDeclaration = true, CheckCharacters = false};

            using (var sw = new StringWriter())
            {
                using (var xw = XmlWriter.Create(sw, settings))
                {
                    xw.WriteStartDocument();
                    xw.WriteStartElement("Entity");
                    xw.WriteAttributeString("Type", EntityType);

                    xw.WriteStartElement("Value");
                    xw.WriteString(Value);
                    xw.WriteEndElement();

                    xw.WriteStartElement("Weight");
                    xw.WriteString(Weight.ToString());
                    xw.WriteEndElement();

                    if (AdditionalFields != null)
                    {
                        xw.WriteStartElement("AdditionalFields");
                        foreach (var af in AdditionalFields)
                        {
                            xw.WriteRaw(af.TransformToXML());
                        }
                        xw.WriteEndElement();
                    }

                    xw.WriteEndElement();
                    xw.WriteEndDocument();
                }
                return sw.ToString();
            }
        }

        

        public class AdditionalField : ITransformable
        {
            /// <summary>
            ///     Creates an additional field to be used by an entity for additional information.
            ///     These fields are passed to the next transformation as STDIN parameters.
            /// </summary>
            /// <param name="name">Name of the additional field (mandatory).</param>
            /// <param name="displayName">The name which will be displayed in the UI field.</param>
            /// <param name="value">The value of the field.</param>
            /// <param name="matchingRule">
            ///     "strict" or "loose". If "strict" is chosen, the attribute will be used to distinguish two entities with the same
            ///     attribute value. If "loose" is used, entities with the same name but different attribute values will be considered
            ///     as equal entities.
            /// </param>
            public AdditionalField(
                string name,
                string displayName = null,
                string value = null,
                MatchingRule matchingRule = TransNet.MatchingRule.Loose
                )
            {
                Name = name;
                DisplayName = displayName;
                Value = value;
                MatchingRule = matchingRule.ToString().ToLower();

                if (name == null)
                {
                    throw new ArgumentNullException(nameof(name), "Name is mandatory and cannot be null.");
                }
            }

            public string Name { get; }
            public string DisplayName { get; }
            public string MatchingRule { get; }
            public string Value { get; }

            public string TransformToXML()
            {
                var settings = new XmlWriterSettings {OmitXmlDeclaration = true};

                using (var sw = new StringWriter())
                {
                    using (var xw = XmlWriter.Create(sw, settings))
                    {
                        xw.WriteStartDocument();
                        xw.WriteStartElement("Field");
                        xw.WriteAttributeString("Name", Name);
                        if (DisplayName != null)
                            xw.WriteAttributeString("DisplayName", DisplayName);
                        xw.WriteAttributeString("MatchingRule", MatchingRule);
                        xw.WriteString(Value);
                        xw.WriteEndElement();
                        xw.WriteEndDocument();
                    }
                    return sw.ToString();
                }
            }
        }
    }
}