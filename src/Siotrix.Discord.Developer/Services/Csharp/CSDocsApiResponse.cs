using System;
using System.Collections.Generic;
using System.Text;

namespace Siotrix.Discord.Developer
{
    public class CSDocsApiResponse
    {
        public List<CSDocsMember> Results { get; set; } = new List<CSDocsMember>();
        public int Count { get; set; }
    }

    public class CSDocsMember
    {
        public string DisplayName { get; set; }
        public string Url { get; set; }
        public Type ItemType { get; set; }
        public Kind ItemKind { get; set; }
        public string Description { get; set; }
    }

    public enum Type
    {
        Type,
        Namespace,
        Member,
    }

    public enum Kind
    {
        Namespace,
        Class,
        Enumeration,
        Method,
        Structure,
        Property,
        Constructor,
        Field,
        Event,
        Interface,
        Delegate
    }
}