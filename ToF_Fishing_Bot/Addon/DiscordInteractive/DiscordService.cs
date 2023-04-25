using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ToF_Fishing_Bot.Addon.DiscordInteractive;


/// <summary>
/// Contain all interactive function with Discord
/// </summary>
public class DiscordService : IDiscordService
{
    private readonly string _hookUrl;
    private readonly string? _mentionId;
    private readonly HttpClient _httpClient = new();
    private readonly DateTime EpochTime = new(1970, 1, 1);

    /// <summary>
    /// Create an instance of <see cref="DiscordService"/>
    /// </summary>
    /// <param name="hookUrl">Webhook URL. You can see how to get webhook url at: <see href="https://support.discord.com/hc/en-us/articles/228383668"/></param>
    /// <param name="mentionId">Discord user Id.</param>
    /// <exception cref="ArgumentException"></exception>
    public DiscordService(string hookUrl, string? mentionId = null)
    {
        if (Uri.IsWellFormedUriString(hookUrl, UriKind.Absolute))
        {
            var uri = new Uri(hookUrl);
            if (!uri.Host.Contains("discord.com", StringComparison.CurrentCultureIgnoreCase))
            {
                throw new ArgumentException("Discord HookUrl provided is not valid");
            }
            if (!uri.AbsolutePath.Contains("webhooks", StringComparison.CurrentCultureIgnoreCase))
            {
                throw new ArgumentException("Discord HookUrl provided is not valid");
            }
            _hookUrl = hookUrl;

        }
        else
        {
            throw new ArgumentException("Discord HookUrl provided is not valid");
        }
        _mentionId = mentionId;
    }

    /// <summary>
    /// Send message to specific webhook above
    /// </summary>
    /// <param name="content">Message content</param>
    /// <returns></returns>
    public Task SendMessage(HookContent content)
    {
        var postContent = JsonContent.Create(content, new MediaTypeWithQualityHeaderValue("application/json"), new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        });
        return _httpClient.PostAsync(_hookUrl, postContent);
    }

    /// <summary>
    /// Build a message about ran out of bait
    /// </summary>
    /// <param name="fishingStartAt">Time that user clicked to Star fishing button, provide it in UTC</param>
    /// <returns></returns>
    public Task<HookContent> BuildOutOfBaitNotification(DateTime fishingStartAt)
    {
        var strBuild = new StringBuilder("Hello");
        if (!string.IsNullOrEmpty(_mentionId))
        {
            strBuild.Append($"<@!{_mentionId}>.");
        }
        strBuild.AppendLine("This program is open source and free to use [here](https://github.com/mdnpascual/ToF-Fishing-Bot).");
        var hookContent = new HookContent()
        {
            Content = strBuild.ToString(),
            Embeds = new List<HookEmbedContent>()
            {
                new HookEmbedContent()
                {
                    Color = 5814783,
                    Title = "Ran out of bait",
                    Description = "This message is sent because ran out of bait. Fishing Session detail below:",
                    Fields = new List<HookEmbedField>()
                    {
                        new HookEmbedField("Start at:", $"<t:{(int)(fishingStartAt - EpochTime).TotalSeconds}:f>", true),
                        new HookEmbedField("End at:", $"<t:{(int)(DateTime.UtcNow - EpochTime).TotalSeconds}:f>", true),
                    },
                }
            },
            Footer = new HookEmbedFooter()
            {
                Text = "TOF Fishing Bot"
            }
        };
        return Task.FromResult(hookContent);
    }

    public Task<HookContent> BuildGenericNotification(string message)
    {
        var strBuild = new StringBuilder("Hello");
        if (!string.IsNullOrEmpty(_mentionId))
        {
            strBuild.AppendLine($"<@!{_mentionId}>.");
        }
        strBuild.AppendLine(message);
        var hookContent = new HookContent()
        {
            Content = strBuild.ToString()
        };
        return Task.FromResult(hookContent);
    }
}
