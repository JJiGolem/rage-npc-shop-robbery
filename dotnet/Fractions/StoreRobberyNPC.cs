using GTANetworkAPI;
using Space.Core;
using SpaceServer;
using System;
using System.Collections.Generic;

namespace Space.Fractions
{
    class StoreRobberyNPC : Script
    {
        private static Random _rnd = new Random();
        [ServerEvent(Event.ResourceStart)]
        public void ResourceStart()
        {
            new NPC(PedHash.Priest, 42.6f, new Vector3(5.1962724, 6510.8755, 31.957832));
            #region Creating Marker && ColShape
            NAPI.Marker.CreateMarker(1, new Vector3(437.7109, -979.41895, 29.7696), new Vector3(), new Vector3(), 0.6f, new Color(255, 255, 255, 0), false);
            ColShape col = NAPI.ColShape.CreateCylinderColShape(new Vector3(437.7109, -979.41895, 29.5696), 1, 1);
            col.SetData("INTERACT", 9081);
            col.OnEntityEnterColShape += (s, e) =>
            {
                NAPI.Data.SetEntityData(e, "INTERACTIONCHECK", 9081);
            };
            col.OnEntityExitColShape += (s, e) =>
            {
                NAPI.Data.SetEntityData(e, "INTERACTIONCHECK", 0);
            };
            #endregion
        }
        private void DropMoneyPack(Player player)
        {
            nItem item = nInventory.Find(Main.Players[player].UUID, ItemType.MoneyPack);
            while (item != null)
            {
                nInventory.Remove(player, item.Type, item.Count);
                Items.onDrop(player, item, item.Data);
                item = nInventory.Find(Main.Players[player].UUID, ItemType.MoneyPack);
            }
        }
        [ServerEvent(Event.PlayerDeath)]
        public void OnPlayerDeath(Player player, Player killer, uint reason)
        {
            if (!Main.Players.ContainsKey(player) || player == null) return;
            if (player.HasData("NPCROBBERY"))
                player.GetData<NPC>("NPCROBBERY").PlayerDeath(player);
            DropMoneyPack(player);
        }
        [ServerEvent(Event.PlayerDisconnected)]
        public void Event_PlayerDisconnected(Player player, DisconnectionType type, string reason)
        {
            if (!Main.Players.ContainsKey(player) || player == null) return;
            if (player.HasData("NPCROBBERY"))
                player.GetData<NPC>("NPCROBBERY").PlayerDeath(player);
            DropMoneyPack(player);
        }
        [RemoteEvent("npc_robbery::start_robbery_timer")]
        public void StartRobberyProcess(Player player)
        {
            if (player.HasData("NPCROBBERY"))
            {
                NPC npc = player.GetData<NPC>("NPCROBBERY");
                if (npc.CanStartRobbery())
                    npc.StartRobbery(player);
            }
        }
        [RemoteEvent("npc_robbery::robbery_process_finished")]
        public void FinishRobberyProcess(Player player)
        {
            if (player.HasData("NPCROBBERY"))
            {
                NPC npc = player.GetData<NPC>("NPCROBBERY");
                npc.FinishRobberyProccess(player);
            }
        }
        private class NPC
        {
            private Vector3 _position;
            private float _heading;
            private int _cash;
            private int _countPossibleRobbery;
            private bool _isRobbering = false;
            private Player _robber;
            private Ped _ped;
            private ColShape _shape;
            public NPC(PedHash hash, float heading, Vector3 position)
            {
                _heading = heading;
                _position = position;
                CreateGTAElements(hash);
                UpdateCountPossibleRobbery();
                UpdateCash();
            }
            public bool CanStartRobbery()
            {
                return !_isRobbering && _countPossibleRobbery > 0;
            }
            public void StartRobbery(Player player)
            {
                _isRobbering = true;
                _robber = player;
                NotifyARobbery();
                Trigger.ClientEvent(player, "npc_robbery::start_process");
            }
            public void PlayerDeath(Player player)
            {
                if (_isRobbering && _robber == player)
                {
                    _robber = null;
                    _isRobbering = false;
                    Trigger.ClientEvent(player, "npc_robbery::stop_check");
                }
            }
            private void NotifyARobbery()
            {
                foreach (KeyValuePair<int, Business> biz in BusinessManager.BizList)
                {
                    if (biz.Value != null)
                    {
                        if (_position.DistanceTo2D(biz.Value.EnterPoint) < 8)
                        {
                            foreach (Player p in NAPI.Pools.GetAllPlayers())
                            {
                                if (p == null || !Main.Players.ContainsKey(p)) return;
                                if (Main.Players[p].FractionID == 7 || Main.Players[p].FractionID == 9)
                                    p.SendChatMessage($"~b~[ФРАКЦИЯ] Совершается ограбление бизнеса {BusinessManager.BusinessTypeNames[biz.Value.Type]}. Номер бизнеса - {biz.Value.ID}.");
                            }
                        }
                    }
                }
            }
            public void FinishRobberyProccess(Player player)
            {
                if (_isRobbering && _robber == player)
                {
                    _robber = null;
                    _isRobbering = false;
                    _countPossibleRobbery--;
                    Notify.Succ(player, $"Вы успешно ограбили магазин!");
                    Items.onDrop(player, new nItem(ItemType.MoneyPack, 1, _cash), null);
                    UpdateCash();
                    if (_countPossibleRobbery <= 0)
                        Trigger.ClientEvent(player, "npc_robbery::stop_check");
                }
            }
            private void CreateGTAElements(PedHash hash)
            {
                _ped = NAPI.Ped.CreatePed((uint)hash, _position, _heading, false, true, true, true, 0);
                _shape = NAPI.ColShape.CreateCylinderColShape(_position, 3, 1.5f, 0);
                _shape.OnEntityEnterColShape += (s, e) =>
                {
                    if (_countPossibleRobbery <= 0 || _isRobbering) return;
                    e.SetData("NPCROBBERY", this);
                    Trigger.ClientEvent(e, "npc_robbery::start_check", _ped);
                };
                _shape.OnEntityExitColShape += (s, e) =>
                {
                    if (_robber == e)
                    {
                        _isRobbering = false;
                        _robber = null;
                    }
                    e.ResetData("NPCROBBERY");
                    Trigger.ClientEvent(e, "npc_robbery::stop_check");
                };
            }
            private void UpdateCountPossibleRobbery()
            {
                _countPossibleRobbery = _rnd.Next(3, 11);
            }
            private void UpdateCash()
            {
                int cash = _rnd.Next(1500, 5001);
                while (cash == _cash)
                    cash = _rnd.Next(1500, 5001);
                _cash = cash;
            }
        }
    }
}
