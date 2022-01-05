// Сделано by Randomchik#7909 && JJiGolem#7069
// Ищем case 22: в interactionPressed и выше вставляем

#region ColShapePol
                    case 9081:
                        nItem item = nInventory.Find(Players[player].UUID, ItemType.MoneyPack);
                        if (item != null)
                        {
                            if (Players[player].FractionID == 7 || Players[player].FractionID == 9)
                            {
                                MoneySystem.Wallet.Change(player, (int)item.Data);
                                GameLog.Money($"Игрок - #{Players[player].PersonID}", $"Server", (int)item.Data, $"Сдал пачку денег (Гос)");
                                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы сдали деньги. Получив за них - {(int)item.Data}", 3000);
                                nInventory.Remove(player, new nItem(ItemType.MoneyPack));
                            }
                        }
                        break;
                    #endregion