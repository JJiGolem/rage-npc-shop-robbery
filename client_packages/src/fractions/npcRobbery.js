const localplayer = mp.players.local;
const meeleGroupHash = 2685387236;
const maxPassedSeconds = 22;

let trackedPed = null;
let startRobberyProcess = false;

mp.game.streaming.requestAnimDict("mp_am_hold_up");

function IsAimingAGunOnPed() {
    const weaponHash = mp.game.invoke(`0x0A6DB4965674D243`, localplayer.handle);
    const groupHash = mp.game.weapon.getWeapontypeGroup(weaponHash);

    return trackedPed && trackedPed.type == "ped" && mp.game.player.isFreeAimingAtEntity(trackedPed.handle) && groupHash != meeleGroupHash;
}

function startPedAnimation() {
    if (trackedPed && !trackedPed.isPlayingAnim("mp_am_hold_up", "holdup_victim_20s", 1)) {
        trackedPed.taskPlayAnim("mp_am_hold_up", "holdup_victim_20s", 8.0, 1.0, -1, 2, 0, false, false, false);
    }
}

function stopPedAnimation() {
    if (trackedPed && trackedPed.isPlayingAnim("mp_am_hold_up", "holdup_victim_20s", 1)) {
        trackedPed.stopAnimTask("mp_am_hold_up", "holdup_victim_20s", 3.0);
    }
}

function stopRobberyProcess() {
    startRobberyProcess = false;
    stopPedAnimation();
}

setInterval(() => {
    if (!startRobberyProcess && IsAimingAGunOnPed()) {
        mp.events.callRemote("npc_robbery::start_robbery_timer");
    }
}, 500)


mp.events.add({
    "npc_robbery::start_check": (ped) => {
        trackedPed = ped;
    },
    "npc_robbery::start_process": () => {
        startRobberyProcess = true;
        let countPassedSeconds = 0;
        let robberyInterval = setInterval(() => {
            if (startRobberyProcess && trackedPed) {
                if (IsAimingAGunOnPed()) {
                    startPedAnimation();
                    countPassedSeconds++;

                    if (countPassedSeconds >= maxPassedSeconds) {
                        stopRobberyProcess();
                        clearInterval(robberyInterval);
                        mp.events.callRemote("npc_robbery::robbery_process_finished");
                    }
                    return;
                }

                stopPedAnimation();
                countPassedSeconds = 0;
                return;
            }

            stopRobberyProcess();
            clearInterval(robberyInterval);
        }, 1000);
    },
    "npc_robbery::stop_check": () => {
        stopRobberyProcess();
        trackedPed = null;
    }
})