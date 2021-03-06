﻿// ----------------------------------------------------------------------------------
// Microsoft Developer & Platform Evangelism
// 
// Copyright (c) Microsoft Corporation. All rights reserved.
// 
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
// EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES 
// OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
// ----------------------------------------------------------------------------------
// The example companies, organizations, products, domain names,
// e-mail addresses, logos, people, places, and events depicted
// herein are fictitious.  No association with any real company,
// organization, product, domain name, email address, logo, person,
// places, or events is intended or should be inferred.
// ----------------------------------------------------------------------------------

namespace Microsoft.Samples.SocialGames.Common.JobEngine
{
    using System;
    using Microsoft.Samples.SocialGames.Common.Storage;
    using Microsoft.WindowsAzure;

    public static class Message
    {
        public static CheckConditionWithAcceptCallback<TMessage> OfType<TMessage>(CloudStorageAccount account, string queueName) where TMessage : AzureQueueMessage
        {
            return OfType<TMessage>(new AzureQueue<TMessage>(account, queueName));
        }

        public static CheckConditionWithAcceptCallback<TMessage> OfType<TMessage>(IAzureQueue<TMessage> queue) where TMessage : AzureQueueMessage
        {
            var messageQueueCondition = new MessageOfTypeQueueCondition<TMessage>(queue);
            return new CheckConditionWithAcceptCallback<TMessage>
            {
                CheckFunc = messageQueueCondition.GetMessageFunc,
                ConfirmFunc = messageQueueCondition.DequeueFunc
            };
        }

        private class MessageOfTypeQueueCondition<TMessage> where TMessage : AzureQueueMessage
        {
            // TODO: Set visibiliy timeout for message (5 to 10 minutes should be OK)
            private static readonly System.TimeSpan MessageDefaultTimeout = TimeSpan.FromMinutes(2);

            private IAzureQueue<TMessage> queue;

            public MessageOfTypeQueueCondition(IAzureQueue<TMessage> queue)
            {
                this.queue = queue;
            }

            public Func<bool, TMessage> GetMessageFunc
            {
                get
                {
                    return (out TMessage output) =>
                    {
                        output = null;

                        var message = this.queue.GetMessage(MessageDefaultTimeout);
                        if (message != null)
                        {
                            output = message;
                            return true;
                        }

                        return false;
                    };
                }
            }

            public System.Action<TMessage> DequeueFunc
            {
                get
                {
                    return message => this.queue.DeleteMessage(message);
                }
            }
        }
    }
}