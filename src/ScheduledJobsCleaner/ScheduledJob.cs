using System;

namespace ScheduledJobsCleaner
{
    internal class ScheduledJob
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string AssemblyName { get; set; }
        public bool FromCode { get; set; }
    }
}