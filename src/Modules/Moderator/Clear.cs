﻿using Discord;
using Discord.Commands;
using NukoBot.Common;
using System.Linq;
using System.Threading.Tasks;

namespace NukoBot.Modules.Moderator
{
    public partial class Moderator
    {
        [Command("Clear")]
        [Alias("clearchat", "purge", "purgechat")]
        [Summary("Clear any amount of messages from the channel this command is ran in.")]
        public async Task Clear([Summary("The amount of messages to clear.")] int amountOfMessages = 50, [Summary("The reason for clearing the chat.")][Remainder] string reason = null)
        {
            if (amountOfMessages < 1 || amountOfMessages > 150)
            {
                await ReplyErrorAsync("you must clear more than 1 and less than 150 messages.");
                return;
            }

            var channel = Context.Channel as ITextChannel;
            var messages = await channel.GetMessagesAsync(amountOfMessages + 1).Flatten().ToArray();

            await channel.DeleteMessagesAsync(messages);

            await _moderationService.ModLogAsync(Context.DbGuild, Context.Guild, "Clear", Configuration.ClearColor, reason, Context.User as IGuildUser, null);

            var message = await Context.Channel.SendMessageAsync($"You have successfully cleared **{amountOfMessages}** messages.");

            await Task.Delay(2000);

            await message.DeleteAsync();
        }
    }
}
