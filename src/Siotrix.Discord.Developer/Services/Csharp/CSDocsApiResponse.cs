using System.Collections.Generic;

namespace Siotrix.Discord.Developer
{
    public class CsDocsApiResponse
    {
        public List<CsDocsMember> Results { get; set; } = new List<CsDocsMember>();
        public int Count { get; set; }
    }

    public class CsDocsMember
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
        Member
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