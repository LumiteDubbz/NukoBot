﻿using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using NukoBot.Common;
using NukoBot.Database.Repositories;
using NukoBot.Common.Preconditions.Command;
using NukoBot.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace NukoBot.Modules
{
    [Name("Owner")]
    [Summary("These commands can only be used by the owner of the server they are ran in.")]
    [RequireContext(ContextType.Guild)]
    [RequireGuildOwner]
    public sealed class OwnerModule : ModuleBase<Context>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Text _text;
        private readonly GuildRepository _guildRepo;

        public OwnerModule(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _text = _serviceProvider.GetRequiredService<Text>();
            _guildRepo = _serviceProvider.GetRequiredService<GuildRepository>();
        }

        [Command("AddModRole")]
        [Alias("addmoderatorrole", "modifymodrole", "modifymoderatorrole")]
        [Summary("Add a role to the list of moderators.")]
        public async Task AddModRole([Summary("The role you wish to assign a permission level.")] IRole modRole, [Summary("The permission level you want to give the role.")] int permissionLevel = 1)
        {
            if (permissionLevel < 1 || permissionLevel > 3)
            {
                await _text.ReplyErrorAsync(Context.User, Context.Channel, "The permission level can only be 1, 2 or 3, corresponding to Moderator, Administrator and Owner respectively.");
                return;
            }

            if (Context.DbGuild.ModRoles.ElementCount == 0)
            {
                await _guildRepo.ModifyAsync(Context.DbGuild, x => x.ModRoles.Add(modRole.Id.ToString(), permissionLevel));
            }
            else
            {
                if (Context.DbGuild.ModRoles.Any(x => x.Name == modRole.Id.ToString()))
                {
                    await _text.ReplyAsync(Context.User, Context.Channel, $"you have successfully updated the moderator role {modRole.Mention} to have a permission level of {permissionLevel}.");
                }

                await _guildRepo.ModifyAsync(Context.DbGuild, x => x.ModRoles.Add(modRole.Id.ToString(), permissionLevel));
            }

            await _text.ReplyAsync(Context.Message.Author, Context.Channel, $"you have successfully added {modRole.Mention} as a mod role with the permission level of {permissionLevel}.");
        }

        [Command("RemoveModRole")]
        [Alias("removemoderatorrole")]
        [Summary("Remove a role from the list of moderators.")]
        public async Task RemoveModRole([Summary("The role you want to remove from the list of mod roles")][Remainder] IRole modRole)
        {
            if (Context.DbGuild.ModRoles.Any(x => x.Name == modRole.Id.ToString()))
            {
                await _guildRepo.ModifyAsync(Context.DbGuild, x => x.ModRoles.Remove(modRole.Id.ToString()));

                await _text.ReplyAsync(Context.User, Context.Channel, $"You have successfully removed {modRole.Mention} from the list of moderators.");
            }
            else
            {
                await _text.ReplyErrorAsync(Context.User, Context.Channel, $"The role {modRole.Mention} is not a moderator role, and therefore cannot be removed.");
            }
        }
    }
}