using System.Collections.Generic;

namespace ToF_Fishing_Bot.Addon.DiscordInteractive;

public class HookEmbedContent
{
    public string Title { get; set; }
    public string Description { get; set; }
    public long Color { get; set; }
    public IList<HookEmbedField> Fields { get; set; } = new List<HookEmbedField>();
}