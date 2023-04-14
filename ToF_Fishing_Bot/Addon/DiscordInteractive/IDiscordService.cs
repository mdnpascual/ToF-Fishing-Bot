using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToF_Fishing_Bot.Addon.DiscordInteractive;

public interface IDiscordService
{
    public Task SendMessage(HookContent content);
    public Task<HookContent> BuildOutOfBaitNotification(DateTime fishingStartAt);
}
