using System;

namespace CorrinoEngine.Core
{
    public class ProductionOrder
    {
        public Guid Id { get; set; }

        public string ActorTypeName { get; set; }

        public float Progress { get; set; }

        public float Duration { get; set; }

        public int Cost { get; set; }
    }
}
