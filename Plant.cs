using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlantvilleOnline
{
    internal class Plant
    {
        public Seed Seed { get; set; }

        public DateTime HarvestTime { get; set; }

        public int Quantity { get; set; }

        public bool IsSpoiled { get; set; }

        public Plant(Seed seed)
        {
            this.Seed = seed;
            this.HarvestTime = DateTime.Now;
            this.IsSpoiled = false;
            this.Quantity = 1;
        }

        public Plant()
        {
        }
    }
}
