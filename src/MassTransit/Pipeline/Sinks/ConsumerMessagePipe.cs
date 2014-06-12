// Copyright 2007-2014 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace MassTransit.Pipeline.Sinks
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using Util;


    public class ConsumerMessagePipe<TConsumer, TMessage> :
        IConsumeContextPipe<TMessage>
        where TConsumer : class
        where TMessage : class
    {
        readonly IAsyncConsumerFactory<TConsumer> _consumerFactory;
        readonly IConsumerMessageAdapter<TConsumer, TMessage> _messageAdapter;
        readonly IMessageRetryPolicy _retryPolicy;

        public ConsumerMessagePipe(IAsyncConsumerFactory<TConsumer> consumerFactory,
            IConsumerMessageAdapter<TConsumer, TMessage> messageAdapter, IMessageRetryPolicy retryPolicy)
        {
            _consumerFactory = consumerFactory;
            _messageAdapter = messageAdapter;
            _retryPolicy = retryPolicy;
        }

        public async Task Send(ConsumeContext<TMessage> context)
        {
            Stopwatch timer = Stopwatch.StartNew();
            try
            {
                await _retryPolicy.Retry(context, x => _consumerFactory.GetConsumer(x, 
                    (consumer, consumeContext) => _messageAdapter.Consume(consumer, consumeContext)));

                context.NotifyConsumed(timer.Elapsed, TypeMetadataCache<TConsumer>.ShortName);
            }
            catch (Exception ex)
            {
                context.NotifyFaulted(TypeMetadataCache<TConsumer>.ShortName, ex);
                throw;
            }
        }

        public bool Inspect(IConsumeContextPipeInspector inspector)
        {
            return inspector.Inspect(this);
        }
    }
}