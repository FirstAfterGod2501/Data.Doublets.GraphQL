using System;
using GraphQL.Types;

namespace GraphQL.Samples.Schemas.Link
{
    public class LinkSchema : Schema
    {
        public LinkSchema(IServiceProvider provider) : base(provider)
        {
            Query = new LinkQuery();
        }
    }
}
