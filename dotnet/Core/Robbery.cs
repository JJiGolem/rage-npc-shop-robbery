// Ищем  [RemoteEvent("mavrbuy")] и после последнего кейса добавляем у каждого номер может меняться

case 8:
                        nItem item = nInventory.Find(Main.Players[player].UUID, ItemType.MoneyPack);
                        if (Manager.FractionTypes[Main.Players[player].FractionID] != 2 && Main.Players[player].FractionID != 0)
                        {
                            if (item == null)
                            {
                                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"У вас нету пачек с деньгами.", 3000);
                                return;
                            }
                            Wallet.Change(player, (int)item.Data);
                            GameLog.Money($"Игрок - #{Main.Players[player].PersonID}", $"Server", (int)item.Data, $"Сдал пачку денег");
                            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы сдали деньги. Получив за них - {(int)item.Data}", 3000);
                            nInventory.Remove(player, new nItem(ItemType.MoneyPack));
                        }
                        return;
						
						