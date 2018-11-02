using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer.DataAbstraction;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.Logging;
using EPiServer.PlugIn;

namespace ScheduledJobsCleaner
{
    [InitializableModule]
    public class InitializationModule : IInitializableModule
    {
        private static readonly ILogger Logger = LogManager.GetLogger();

        public void Initialize(InitializationEngine context)
        {
            var scheduledJobsRepository = context.Locate.Advanced.GetInstance<IScheduledJobRepository>();
            var ghostJobs = GetGhostJobs(scheduledJobsRepository);

            foreach (var ghostJob in ghostJobs)
            {
                scheduledJobsRepository.Delete(ghostJob.Id);
                Logger.Information($"Deleted scheduled job. Id: {ghostJob.Id:D}, Name: {ghostJob.Name}, Assembly name: {ghostJob.AssemblyName} ");
            }
        }

        public void Uninitialize(InitializationEngine context)
        {
        }

        private List<ScheduledJob> GetGhostJobs(IScheduledJobRepository scheduledJobRepository)
        {
            var scheduledJobs = PlugInLocator.Search(new ScheduledPlugInAttribute());

            var scheduledJobDescriptors = PlugInDescriptor.GetAttributeArray(
                scheduledJobs,
                typeof(ScheduledPlugInAttribute));

            var ghostJobs =
                (from job in scheduledJobRepository.List()
                let attribute = scheduledJobDescriptors.FirstOrDefault(x =>
                string.Equals(x.PlugInType.FullName, job.TypeName, StringComparison.InvariantCultureIgnoreCase))
                select new ScheduledJob
                {
                    Id = job.ID,
                    FromCode = attribute != null,
                    Name = job.Name,
                    AssemblyName = job.AssemblyName
                })
                .Where(x => !x.FromCode)
                .ToList();

            return ghostJobs;
        }
    }
}