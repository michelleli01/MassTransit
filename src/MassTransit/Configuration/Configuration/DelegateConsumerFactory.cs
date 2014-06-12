﻿// Copyright 2007-2014 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
namespace MassTransit.Configuration
{
    using System;
    using System.Threading.Tasks;
    using Exceptions;
    using Util;


    public class DelegateConsumerFactory<TConsumer> :
        IConsumerFactory<TConsumer>
        where TConsumer : class
    {
        readonly Func<TConsumer> _factoryMethod;

        public DelegateConsumerFactory(Func<TConsumer> factoryMethod)
        {
            _factoryMethod = factoryMethod;
        }

        async Task IAsyncConsumerFactory<TConsumer>.GetConsumer<TMessage>(ConsumeContext<TMessage> consumeContext,
            ConsumerFactoryCallback<TConsumer, TMessage> callback)
        {
            TConsumer consumer = null;
            try
            {
                consumer = _factoryMethod();
                if (consumer == null)
                {
                    throw new ConsumerException(string.Format("Unable to resolve consumer type '{0}'.",
                        TypeMetadataCache<TConsumer>.ShortName));
                }

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


    public class DelegateAsyncConsumerFactory<TConsumer> :
        IAsyncConsumerFactory<TConsumer>
        where TConsumer : class
    {
        readonly Func<TConsumer> _factoryMethod;

        public DelegateAsyncConsumerFactory(Func<TConsumer> factoryMethod)
        {
            _factoryMethod = factoryMethod;
        }

        async Task IAsyncConsumerFactory<TConsumer>.GetConsumer<TMessage>(ConsumeContext<TMessage> consumeContext,
            ConsumerFactoryCallback<TConsumer, TMessage> callback)
        {
            TConsumer consumer = null;
            try
            {
                consumer = _factoryMethod();
                if (consumer == null)
                {
                    throw new ConsumerException(string.Format("Unable to resolve consumer type '{0}'.",
                        TypeMetadataCache<TConsumer>.ShortName));
                }

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