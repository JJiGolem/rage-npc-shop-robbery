let trackedPed = null;
let startRobberyProcess = false;
mp.events.add("npc_robbery::start_check", (ped) => {
    trackedPed = ped;
})
setInterval(() => {
    if (!startRobberyProcess && trackedPed && trackedPed.type == "ped") {
        let weaponHash = mp.game.invoke(`0x0A6DB4965674D243`, mp.players.local.handle);
        let groupHash = mp.game.weapon.getWeapontypeGroup(weaponHash);
        if (mp.game.player.isFreeAimingAtEntity(trackedPed.handle) && groupHash != 2685387236) {
            mp.events.callRemote("npc_robbery::start_robbery_timer");
        }
    }
}, 500)
let count = 0;
mp.game.streaming.requestAnimDict("mp_am_hold_up");
mp.events.add("npc_robbery::start_process", () => {
    startRobberyProcess = true;
    mp.game.streaming.requestAnimDict("mp_am_hold_up");
    trackedPed.taskPlayAnim("mp_am_hold_up", "holdup_victim_20s", 8.0, 1.0, -1, 2, 0, false, false, false);
    let robberyInterval = setInterval(() => {   
		function clearRobberyInterval() {
			startRobberyProcess = false;
			count = 0;
			clearInterval(robberyInterval);
			robberyInterval = null;
		}	
		if (startRobberyProcess && trackedPed && trackedPed.type == "ped") {
            let weaponHash = mp.game.invoke(`0x0A6DB4965674D243`, mp.players.local.handle);
            let groupHash = mp.game.weapon.getWeapontypeGroup(weaponHash);
            if (mp.game.player.isFreeAimingAtEntity(trackedPed.handle) && groupHash != 2685387236) {
                if (!trackedPed.isPlayingAnim("mp_am_hold_up", "holdup_victim_20s", 1)) {
                    trackedPed.taskPlayAnim("mp_am_hold_up", "holdup_victim_20s", 8.0, 1.0, -1, 2, 0, false, false, false);
                }
                count++;
                if (count >= 22) {
					clearRobberyInterval();
                    trackedPed.stopAnimTask("mp_am_hold_up", "holdup_victim_20s", 3.0);
                    mp.events.callRemote("npc_robbery::robbery_process_finished");
                }
				return;
            }
            trackedPed.stopAnimTask("mp_am_hold_up", "holdup_victim_20s", 1);
            count = 0;
            return;
        }
		clearRobberyInterval();
    }, 1000);
	
})
mp.events.add("npc_robbery::stop_check", () => {
    if (trackedPed != null && trackedPed.isPlayingAnim("mp_am_hold_up", "holdup_victim_20s", 1)) {
        trackedPed.stopAnimTask("mp_am_hold_up", "holdup_victim_20s", 1);
    }
    trackedPed = null;
    startRobberyProcess = false;
    count = 0;
})