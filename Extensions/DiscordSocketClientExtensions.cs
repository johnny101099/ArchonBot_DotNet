using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchonBot.Extensions
{
    public static class DiscordSocketClientExtensions
    {
        public static async Task<IUserMessage?> GetMessageAsync(this DiscordSocketClient client, ulong channel_id, ulong message_id)
        {
            var channel = await client.GetChannelAsync(channel_id) as IMessageChannel;
            if(channel == null)
            {
                return null;
            }
            var message = await channel.GetMessageAsync(message_id) as IUserMessage;
            return message;
        }
    }
}
