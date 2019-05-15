using Microsoft.Bot.Schema;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace SimpleWebApiBot.Bots
{
    public class Conversations : ConcurrentDictionary<string, ConversationReference>
    {
        public ConversationReference Get(string userName)
        {
            if (TryGetValue(userName, out ConversationReference value))
            {
                return value;
            }

            return null;
        }

        public void Save(ConversationReference conversationReference)
        {
            AddOrUpdate(
                conversationReference.User.Name,
                conversationReference,
                (key, oldValue) => conversationReference);
        }
    }
}