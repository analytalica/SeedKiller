//Import various C# things.
using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;

//Import Procon things.
using PRoCon.Core;
using PRoCon.Core.Plugin;
using PRoCon.Core.Plugin.Commands;
using PRoCon.Core.Players;
using PRoCon.Core.Players.Items;
using PRoCon.Core.Battlemap;
using PRoCon.Core.Maps;
using PRoCon.Core.HttpServer;

namespace PRoConEvents
{
    public class SeedKiller : PRoConPluginAPI, IPRoConPluginInterface
    {
        private bool pluginEnabled = false;
        private List<String> customPlayerList = new List<String>();
        private int debugLevel = 1;

        public SeedKiller()
        {

        }

        public string GetPluginName()
        {
            return "SeedKiller";
        }

        public string GetPluginVersion()
        {
            return "1.0.0";
        }

        public string GetPluginAuthor()
        {
            return "Analytalica";
        }

        public string GetPluginWebsite()
        {
            return "purebattlefield.org";
        }

        public string GetPluginDescription()
        {
            return @"SeedKiller is a plugin that automatically kills select players immediately after they spawn in.";
        }

        public void toChat(String message)
        {
            toChat(message, "all");
        }

        public void toChat(String message, String playerName)
        {
            if (!message.Contains("\n") && !String.IsNullOrEmpty(message))
            {
                if (playerName == "all")
                {
                    this.ExecuteCommand("procon.protected.send", "admin.say", message, "all");
                }
                else
                {
                    this.ExecuteCommand("procon.protected.send", "admin.say", message, "player", playerName);
                }
            }
            else if (message != "\n")
            {
                string[] multiMsg = message.Split(new string[] { "\n" }, StringSplitOptions.None);
                foreach (string send in multiMsg)
                {
                    if (!String.IsNullOrEmpty(message))
                        toChat(send, playerName);
                }
            }
        }

        public void toConsole(int msgLevel, String message)
        {
            if (debugLevel >= msgLevel)
            {
                this.ExecuteCommand("procon.protected.pluginconsole.write", "SeedKiller: " + message);
            }
        }

        public void OnPluginLoaded(string strHostName, string strPort, string strPRoConVersion)
        {
            this.RegisterEvents(this.GetType().Name, "OnPluginLoaded", "OnPlayerSpawned");
        }

        public virtual void OnPlayerSpawned(string soldierName, Inventory spawnedInventory) 
        {
            if (pluginEnabled && this.customPlayerList.Contains(soldierName.ToLower()))
            {
                this.toConsole(2, "Killing " + soldierName + " who just spawned...");
                this.ExecuteCommand("procon.protected.send", "admin.killPlayer", soldierName);
            }
        }

        public void OnPluginEnable()
        {
            this.pluginEnabled = true;
            this.toConsole(1, "SeedKiller Enabled!");
        }

        public void OnPluginDisable()
        {
            this.pluginEnabled = false;
            this.toConsole(1, "SeedKiller Disabled!");
        }

        public List<CPluginVariable> GetDisplayPluginVariables()
        {
            List<CPluginVariable> lstReturn = new List<CPluginVariable>();
            lstReturn.Add(new CPluginVariable("Seeder List|Add a soldier name... (ci)", typeof(string), ""));
            this.customPlayerList.Sort();
            for (int i = 0; i < customPlayerList.Count; i++ )
            {
                String thisPlayer = customPlayerList[i];
                if (String.IsNullOrEmpty(thisPlayer))
                {
                    customPlayerList.Remove(thisPlayer);
                    i--;
                }
                else
                {
                    lstReturn.Add(new CPluginVariable("Seeder List|" + i.ToString() + ". Soldier name:", typeof(string), thisPlayer));
                }
            }
            lstReturn.Add(new CPluginVariable("Settings|Debug Level", typeof(string), this.debugLevel.ToString()));
            return lstReturn;
        }

        public List<CPluginVariable> GetPluginVariables()
        {
            return GetDisplayPluginVariables();
        }

        public int getConfigIndex(string configString)
        {
            int lineLocation = configString.IndexOf('|');
            return Int32.Parse(configString.Substring(lineLocation + 1, configString.IndexOf('.') - lineLocation - 1));
        }

        public void SetPluginVariable(String strVariable, String strValue)
        {
            try
            {
                if (strVariable.Contains("Soldier name:"))
                {
                    int n = getConfigIndex(strVariable);
                    try
                    {
                        customPlayerList[n] = strValue.Trim().ToLower();
                    }
                    catch (ArgumentOutOfRangeException e)
                    {
                        customPlayerList.Add(strValue.Trim().ToLower());
                    }
                }
                else if (strVariable.Contains("Add a soldier name..."))
                {
                    customPlayerList.Add(strValue.Trim().ToLower());
                }
                else if (strVariable.Contains("Debug Level"))
                {
                    try
                    {
                        this.debugLevel = Int32.Parse(strValue);
                    }
                    catch (Exception z)
                    {
                        this.toConsole(1, "Invalid debug level! Choose 0, 1, or 2 only.");
                        this.debugLevel = 1;
                    }
                }
            }
            catch (Exception e)
            {
                this.toConsole(1, e.ToString());
            }
        }
    }
}