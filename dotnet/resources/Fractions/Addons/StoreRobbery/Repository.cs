using System.Collections.Generic;
using GTANetworkAPI;
using Fractions.Addons.StoreRobbery.Models;

namespace Fractions.Addons.StoreRobbery
{
    class Repository
    {
        private static List<StoreNPC> _allRobberyShops;

        public static void ResourceStart()
        {
            _allRobberyShops = new List<StoreNPC>()
            {
                new StoreNPC(PedHash.Molly, 42.6f, new Vector3(5.1962724, 6510.8755, 31.957832))
            };
        }

        public static void PaydayTrigger()
        {
            _allRobberyShops.ForEach(x => x.UpdateCountPossibleRobbery());
        }
    }
}
