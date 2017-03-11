namespace Doggo.Discord.Github
{
    public struct GithubEntity
    {
        public bool IsUser { get; }
        public string Value { get; }

        public GithubEntity(string value, bool user)
        {
            IsUser = user;
            Value = value;
        }
    }
}
