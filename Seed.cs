using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlantvilleOnline
{
    internal class Seed
    {
        public string Name { get; set; }

        public int SeedPrice { get; set; }

        public int HarvestPrice { get; set; }

        public TimeSpan HarvestDuration { get; set; }

        public Seed(string name, int seedPrice, int harvestPrice, TimeSpan harvestDuration)
        {
            this.Name = name;
            this.SeedPrice = seedPrice;
            this.HarvestPrice = harvestPrice;
            this.HarvestDuration = harvestDuration;
        }
    }
}
