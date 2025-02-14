using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Sockets;
using GraphQL.Types;
using Platform.Data.Doublets;
using Microsoft.Extensions.DependencyInjection;
using Platform.Data;
using Platform.Memory;

namespace GraphQL.Samples.Schemas.Link
{
    public class LinkType : ObjectGraphType<Link>
    {
        public LinkType()
        {
            Field(o => o.id);
            Field(o => o.from_id);
            Field(o => o.from, false, type: typeof(LinkType)).Resolve(ResolveFrom);
            Field(o => o.to, type: typeof(LinkType)).Resolve(ResolveTo);
            Field(o => o.to_id);
            Field(o => o.type, type: typeof(LinkType)).Resolve(ResolveType);
            Field(o => o.type_id);
            Field<ListGraphType<LinkType>>().Name("in").Resolve(ResolveIn);
            Field<ListGraphType<LinkType>>().Name("out").Resolve(ResolveOut);
            Field(o => o.limit);
        }

        private List<Link> ResolveIn(IResolveFieldContext<Link> context)
        {
            var inList = new List<Link>();
            ILinks<ulong> Links = (ILinks<ulong>) context.RequestServices.GetService(typeof(ILinks<ulong>));
            var query = new Link<UInt64>(index: Links.Constants.Any, source: Links.Constants.Any, target: (ulong)context.Source.id);
            Links.Each(link =>
            {
                inList.Add(new Link()
                {
                    id = (long)Links.GetIndex(link),
                    from_id = (long)Links.GetSource(link),
                    type_id = (long)Links.GetTarget(link)
                });
                return Links.Constants.Continue;
            }, query);
            return inList;
        }
        private List<Link> ResolveOut(IResolveFieldContext<Link> context)
        {
            var outList = new List<Link>();
            ILinks<ulong> Links = (ILinks<ulong>)context.RequestServices.GetService(typeof(ILinks<ulong>));
            var query = new Link<UInt64>(index: Links.Constants.Any, source: (ulong)context.Source.id ,target: Links.Constants.Any);
            Links.Each(link =>
            {
                outList.Add(new Link()
                {
                    id = (long)Links.GetIndex(link),
                    from_id = (long)Links.GetSource(link),
                    type_id = (long)Links.GetTarget(link)
                });
                return Links.Constants.Continue;
            }, query);
            return outList;
        }

        private Link ResolveFrom(IResolveFieldContext<Link> context)
        {
            return context.Source.from ?? GetLinkOrDefault(context.RequestServices.GetService(typeof(ILinks<ulong>)), context.Source.from_id);
        }

        private Link ResolveTo(IResolveFieldContext<Link> context)
        {
            return context.Source.to ?? GetLinkOrDefault(context.RequestServices.GetService(typeof(ILinks<ulong>)), context.Source.to_id);
        }

        private Link ResolveType(IResolveFieldContext<Link> context)
        {
            return context.Source.type ?? GetLinkOrDefault(context.RequestServices.GetService(typeof(ILinks<ulong>)) , context.Source.type_id);
        }
        public static Link GetLinkOrDefault(object service, long linkid)
        {
            ILinks<ulong> Links = (ILinks<ulong>)service;
            if (Links.Exists((ulong)linkid))
            {
                var fromLink = Links.GetLink((ulong)linkid);
                return new Link()
                {
                    id = (long)Links.GetIndex(fromLink),
                    from_id = (long)Links.GetSource(fromLink),
                    to_id = (long)Links.GetTarget(fromLink)
                };
            }
            else
            {
                return null;
            }
        }
    }
}
