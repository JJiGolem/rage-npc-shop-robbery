using System;
using GTANetworkAPI;

namespace Fractions.Addons.StoreRobbery.Models
{
    class StoreNPC
    {
        private static Random _rnd = new Random();

        private int _cash;
        private int _countPossibleRobbery;
        private bool _isRobbering = false;
        private Player _robber;
        private Ped _ped;
        private ColShape _shape;

        public NPC(PedHash hash, float heading, Vector3 position)
        {
            CreateGTAElements(hash, position, heading);
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
            player.TriggerClientEvent("npc_robbery::start_process");
        }

        public void StopRobbery(Player player)
        {
            if (_isRobbering && _robber == player)
            {
                _robber = null;
                _isRobbering = false;
                player.TriggerClientEvent("npc_robbery::stop_check");
            }
        }

        public void FinishRobberyProccess(Player player)
        {
            if (_isRobbering && _robber == player)
            {
                _robber = null;
                _isRobbering = false;
                _countPossibleRobbery--;

                player.SendNotify($"Вы ограбили кассу на ~g~${_cash}");

                if (_countPossibleRobbery <= 0)
                    player.TriggerClientEvent("npc_robbery::stop_check");

                UpdateCash();
            }
        }

        public void UpdateCountPossibleRobbery()
        {
            _countPossibleRobbery = _rnd.Next(3, 5);
        }

        private void UpdateCash()
        {
            _cash = _rnd.Next(100, 1300);
        }

        private void CreateGTAElements(PedHash hash, Vector3 position, float heading)
        {
            _ped = NAPI.Ped.CreatePed((uint)hash, position, heading, false, true, true, true, 0);
            _shape = NAPI.ColShape.CreateCylinderColShape(position, 3, 1.5f, 0);
            _shape.OnEntityEnterColShape += (shape, player) =>
            {
                if (_countPossibleRobbery <= 0 || _isRobbering)
                    return;

                player.SetData("NPCROBBERY", this);
                player.TriggerClientEvent("npc_robbery::start_check", _ped);
            };
            _shape.OnEntityExitColShape += (shape, player) =>
            {
                if (_robber == player)
                {
                    _isRobbering = false;
                    _robber = null;
                }
                player.ResetData("NPCROBBERY");
                player.TriggerClientEvent("npc_robbery::stop_check");
            };
        }
    }
}