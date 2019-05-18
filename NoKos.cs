using Rocket.API;
using Rocket.API.Collections;
using Rocket.Core.Plugins;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using UnityEngine;
using Game4Freak.AdvancedZones;

using Logger = Rocket.Core.Logging.Logger;

namespace Wth.NoKos
{
    public class NoKos : RocketPlugin<NoKosConfiguration>
    {
        public static NoKos Instance;
        public const string VERSION = "0.1.0.0";
        private int frame = 10;
        public Int32 lastUnixTimestamp;

        private Dictionary<string, int> NoKOSData;

        protected override void Load()
        {
            Instance = this;
            Logger.Log("Loading NoKos v." + VERSION);

            if (AdvancedZones.Instance == null)
            {
                Logger.LogError("You need AdvancedZone Plugin in order to this plugin to work !");
                return;
            }

            NoKOSData = new Dictionary<string, int>();

            AdvancedZones.onZoneLeave += onZoneLeft;
            AdvancedZones.onZoneEnter += onZoneEnter;
            DamageTool.playerDamaged += onPlayerDamage;
        }

        protected override void Unload()
        {
            Logger.Log("UnLoading NoKos v." + VERSION);

            AdvancedZones.onZoneLeave -= onZoneLeft;
            AdvancedZones.onZoneEnter -= onZoneEnter;
            DamageTool.playerDamaged -= onPlayerDamage;

            NoKOSData.Clear();
        }

        private void onPlayerDamage(Player player, ref EDeathCause cause, ref ELimb limb, ref CSteamID killer, ref Vector3 direction, ref float damage, ref float times, ref bool canDamage)
        {
            if (cause == EDeathCause.BLEEDING || cause == EDeathCause.BONES || cause == EDeathCause.BREATH || cause == EDeathCause.BURNING || cause == EDeathCause.FOOD || cause == EDeathCause.FREEZING
                || cause == EDeathCause.INFECTION || cause == EDeathCause.ARENA || cause == EDeathCause.KILL || cause == EDeathCause.SUICIDE || cause == EDeathCause.WATER)
            {
                return;
            }
            if (cause == EDeathCause.LANDMINE || cause == EDeathCause.SHRED || cause == EDeathCause.SENTRY || cause == EDeathCause.VEHICLE || cause == EDeathCause.ROADKILL || cause == EDeathCause.ACID)
            {
                if (NoKOSData.ContainsKey(UnturnedPlayer.FromPlayer(player).Id))
                {
                    if (NoKOSData[UnturnedPlayer.FromPlayer(player).Id] + Configuration.Instance.noKOSprotectionTime >= getCurrentTime())
                    {
                        canDamage = false;
                    }
                }

                return;
            }

            if (UnturnedPlayer.FromCSteamID(killer).Player == null && AdvancedZones.Instance.playerInZoneType(UnturnedPlayer.FromPlayer(player), Zone.flagTypes[Zone.noPlayerDamage]))
            {
                if (NoKOSData.ContainsKey(UnturnedPlayer.FromPlayer(player).Id))
                {
                    if (NoKOSData[UnturnedPlayer.FromPlayer(player).Id] + Configuration.Instance.noKOSprotectionTime >= getCurrentTime())
                    {
                        if (cause == EDeathCause.ZOMBIE)
                        {
                            UnturnedPlayer.FromPlayer(player).Infection = 0;
                        }
                        canDamage = false;
                    }
                }

                return;
            }
            else if (UnturnedPlayer.FromCSteamID(killer).Player == null)
            {
                return;
            }

            if (UnturnedPlayer.FromCSteamID(killer).Player != null)
            {
                if (NoKOSData.ContainsKey(UnturnedPlayer.FromCSteamID(killer).Id))
                {
                    if (NoKOSData[UnturnedPlayer.FromCSteamID(killer).Id] + Configuration.Instance.noKOSprotectionTime >= getCurrentTime())
                    {
                        UnturnedChat.Say(UnturnedPlayer.FromPlayer(player), Translate("under_protection", UnturnedChat.GetColorFromName(NoKos.Instance.Configuration.Instance.messageColor, Color.green)));
                        canDamage = false;
                    }
                }
            }

            if (NoKOSData.ContainsKey(UnturnedPlayer.FromPlayer(player).Id))
            {
                if (NoKOSData[UnturnedPlayer.FromPlayer(player).Id] + Configuration.Instance.noKOSprotectionTime >= getCurrentTime())
                {
                    canDamage = false;
                }
            }
        }

        private void onZoneLeft(UnturnedPlayer player, Zone zone, Vector3 lastPos)
        {
            NoKOSData.Add(player.Id, getCurrentTime());
        }

        private void onZoneEnter(UnturnedPlayer player, Zone zone, Vector3 lastPos)
        {
            if (NoKOSData.ContainsKey(player.Id))
            {
                NoKOSData.Remove(player.Id);
            }
        }

        public override TranslationList DefaultTranslations
        {
            get
            {
                return new TranslationList()
                {
                    {"lost_protection", "[NoKOS] You lost your protection, be careful !" },
                    {"under_protection", "[NoKOS] Under Protection" }
                };
            }
        }

        private void Update()
        {
            frame++;
            if (frame % 10 != 0) return;

            foreach (var splayer in Provider.clients)
            {
                UnturnedPlayer player = UnturnedPlayer.FromSteamPlayer(splayer);

                if (NoKOSData.ContainsKey(player.Id))
                {
                    if (NoKOSData[player.Id] + Configuration.Instance.noKOSprotectionTime <= getCurrentTime())
                    {
                        UnturnedChat.Say(player, Translate("lost_protection", UnturnedChat.GetColorFromName(NoKos.Instance.Configuration.Instance.messageColor, Color.green)));
                        NoKOSData.Remove(player.Id);
                    }
                }
            }
        }

        public static Int32 getCurrentTime()
        {
            return (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }

    }
}