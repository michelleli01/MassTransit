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
    using System.Threading.Tasks;


    public class DefaultConstructorAsyncConsumerFactory<TConsumer> :
        IConsumerFactory<TConsumer>
        where TConsumer : class, new()
    {
        public async Task GetConsumer<TMessage>(ConsumeContext<TMessage> consumeContext,
            ConsumerFactoryCallback<TConsumer, TMessage> callback)
            where TMessage : class
        {
            TConsumer consumer = null;
            try
            {
                consumer = new TConsumer();

                await callback(consumer, consumeContext);
            }
            finally
            {
                var disposable = consumer as IDisposable;
                if (disposable != null)
                    disposable.Dispose();
            }
        }
    }
}