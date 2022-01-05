using GTANetworkAPI;

namespace Fractions.Addons.StoreRobbery
{
    class Events : Script
    {
        [ServerEvent(Event.ResourceStart)]
        public void ResourceStart()
        {
            Repository.ResourceStart();
        }

        [ServerEvent(Event.PlayerDeath)]
        public void OnPlayerDeath(Player player, Player killer, uint reason)
        {
            if (player == null) return;
            if (player.HasData("NPCROBBERY"))
                player.GetData<StoreNPC>("NPCROBBERY")?.StopRobbery(player);
        }

        [ServerEvent(Event.PlayerDisconnected)]
        public void OnPlayerDisconnected(Player player, DisconnectionType type, string reason)
        {
            if (player == null) return;
            if (player.HasData("NPCROBBERY"))
                player.GetData<StoreNPC>("NPCROBBERY")?.StopRobbery(player);
        }

        [RemoteEvent("npc_robbery::start_robbery_timer")]
        public void TryStartRobberyProcess(Player player)
        {
            if (player.HasData("NPCROBBERY"))
            {
                StoreNPC npc = player.GetData<StoreNPC>("NPCROBBERY");
                if (npc != null && npc.CanStartRobbery())
                    npc.StartRobbery(player);
            }
        }

        [RemoteEvent("npc_robbery::robbery_process_finished")]
        public void FinishRobberyProcess(Player player)
        {
            if (player.HasData("NPCROBBERY"))
                player.GetData<StoreNPC>("NPCROBBERY")?.FinishRobberyProccess(player);
        }
    }
}