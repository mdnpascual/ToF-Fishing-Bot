using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToF_Fishing_Bot.Addon.DiscordInteractive;

public class HookContent
{
    public string Content { get; set; } = string.Empty;
    public IList<HookEmbedContent> Embeds { get; set; } = new List<HookEmbedContent>();
    public HookEmbedFooter? Footer { get; set; }
}
