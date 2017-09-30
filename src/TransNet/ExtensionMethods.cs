namespace TransNet
{
    /// <summary>
    /// Extension methods to simplify the usage of the library.
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// Add an entity to the current transformation to be 
        /// returned to Maltego,
        /// </summary>
        /// <param name="transform">Transformation object.</param>
        /// <param name="entityType">The entity type to add to the transformation.</param>
        /// <param name="entityValue">Value of the entity.</param>
        /// <param name="weight">Weight of the entity.</param>
        /// <returns>The added entity.</returns>
        public static Entity AddEntity(
            this Transformation transform, 
            string entityType, 
            string entityValue,
            int weight = 0
            )
        {
            var entity = new Entity(entityType, entityValue, weight);
            transform.Entities.Add(entity);
            return entity;
        }

        /// <summary>
        /// Add an additional field to the entity.
        /// </summary>
        /// <param name="entity">Entity where the field is added.</param>
        /// <param name="name">Name of the additional field.</param>
        /// <param name="displayName">Display name of the field.</param>
        /// <param name="value">Value of the field.</param>
        /// <param name="matchingRule">Matching rule of the field.</param>
        /// <returns>The entity where the field was added.</returns>
        public static Entity AddAdditionalField(
            this Entity entity,
            string name,
            string displayName = null,
            string value = null,
            MatchingRule matchingRule = MatchingRule.Loose
            )
        {
            var additionalField = new Entity.AdditionalField(name, displayName, value, matchingRule);
            entity.AdditionalFields.Add(additionalField);
            return entity;
        }
    }
}