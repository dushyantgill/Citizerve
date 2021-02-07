using Citizerve.ProvisionAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Citizerve.ProvisionAPI.Messaging
{
    public interface IQueuePublisher
    {
        Task PublishProvisionResource(Resource resource);
    }
}
