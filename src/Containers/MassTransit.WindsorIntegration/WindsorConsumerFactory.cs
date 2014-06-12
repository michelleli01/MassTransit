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
namespace MassTransit.WindsorIntegration
{
    using System.Threading.Tasks;
    using Castle.MicroKernel.Lifestyle;
    using Castle.Windsor;
    using Exceptions;
    using Pipeline.Sinks;
    using Util;


    public class WindsorConsumerFactory<TConsumer> :
        IConsumerFactory<TConsumer>
        where TConsumer : class
    {
        readonly IWindsorContainer _container;

        public WindsorConsumerFactory(IWindsorContainer container)
        {
            _container = container;
        }

        async Task IAsyncConsumerFactory<TConsumer>.GetConsumer<TMessage>(ConsumeContext<TMessage> consumeContext,
            ConsumerFactoryCallback<TConsumer, TMessage> callback)
        {
            using (_container.BeginScope())
            {
                var consumer = _container.Resolve<TConsumer>();
                if (consumer == null)
                {
                    throw new ConsumerException(string.Format("Unable to resolve consumer type '{0}'.",
                        TypeMetadataCache<TConsumer>.ShortName));
                }

                try
                {
                    await callback(consumer, consumeContext);
                }
                finally
                {
                    _container.Release(consumer);
                }
            }
        }
    }
}