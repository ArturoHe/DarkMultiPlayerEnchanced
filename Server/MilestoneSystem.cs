using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace DarkMultiPlayerServer
{
    public static class MilestoneSystem
    {
        private static readonly object milestoneLock = new object();
        private static readonly Dictionary<string, MilestoneDefinition> milestoneDefinitions = new Dictionary<string, MilestoneDefinition>();
        private static readonly Dictionary<string, MilestoneRecord> milestones = new Dictionary<string, MilestoneRecord>();
        private static readonly Dictionary<string, MilestoneVesselState> vesselStates = new Dictionary<string, MilestoneVesselState>();
        private static bool loaded;
        private static readonly string defaultMilestoneFileData = string.Join(Environment.NewLine, new string[]
        {
            "# Milestones are defined as: key|title|ruleType|bodyName|threshold",
            "# ruleType values:",
            "# ORBIT|Kerbin",
            "# LANDING|Mun",
            "# VESSEL_TYPE|EVA",
            "# DOCKING|",
            "# SOI_EXIT|Kerbin",
            "# ALTITUDE_AT_LEAST|Kerbin|70000",
            "# FLAG_LAT_AT_MOST|Kerbin|-89",
            "# FLAG_LAT_AT_LEAST|Kerbin|89",
            "first-space-kerbin|Primer jugador en salir al espacio de Kerbin|ALTITUDE_AT_LEAST|Kerbin|70000",
            "first-orbit-kerbin|Primer jugador en orbitar Kerbin|ORBIT|Kerbin|",
            "first-orbit-mun|Primer jugador en orbitar Mun|ORBIT|Mun|",
            "first-orbit-minmus|Primer jugador en orbitar Minmus|ORBIT|Minmus|",
            "first-orbit-eve|Primer jugador en orbitar Eve|ORBIT|Eve|",
            "first-orbit-duna|Primer jugador en orbitar Duna|ORBIT|Duna|",
            "first-orbit-dres|Primer jugador en orbitar Dres|ORBIT|Dres|",
            "first-orbit-gilly|Primer jugador en orbitar Gilly|ORBIT|Gilly|",
            "first-orbit-ike|Primer jugador en orbitar Ike|ORBIT|Ike|",
            "first-orbit-jool|Primer jugador en orbitar Jool|ORBIT|Jool|",
            "first-orbit-laythe|Primer jugador en orbitar Laythe|ORBIT|Laythe|",
            "first-orbit-vall|Primer jugador en orbitar Vall|ORBIT|Vall|",
            "first-orbit-tylo|Primer jugador en orbitar Tylo|ORBIT|Tylo|",
            "first-orbit-bop|Primer jugador en orbitar Bop|ORBIT|Bop|",
            "first-orbit-pol|Primer jugador en orbitar Pol|ORBIT|Pol|",
            "first-orbit-moho|Primer jugador en orbitar Moho|ORBIT|Moho|",
            "first-orbit-eeloo|Primer jugador en orbitar Eeloo|ORBIT|Eeloo|",
            "first-land-mun|Primer jugador en aterrizar en Mun|LANDING|Mun|",
            "first-land-minmus|Primer jugador en aterrizar en Minmus|LANDING|Minmus|",
            "first-land-duna|Primer jugador en aterrizar en Duna|LANDING|Duna|",
            "first-land-eve|Primer jugador en aterrizar en Eve|LANDING|Eve|",
            "first-land-moho|Primer jugador en aterrizar en Moho|LANDING|Moho|",
            "first-land-dres|Primer jugador en aterrizar en Dres|LANDING|Dres|",
            "first-land-gilly|Primer jugador en aterrizar en Gilly|LANDING|Gilly|",
            "first-land-ike|Primer jugador en aterrizar en Ike|LANDING|Ike|",
            "first-land-jool|Primer jugador en aterrizar en Jool|LANDING|Jool|",
            "first-land-laythe|Primer jugador en aterrizar en Laythe|LANDING|Laythe|",
            "first-land-vall|Primer jugador en aterrizar en Vall|LANDING|Vall|",
            "first-land-tylo|Primer jugador en aterrizar en Tylo|LANDING|Tylo|",
            "first-land-bop|Primer jugador en aterrizar en Bop|LANDING|Bop|",
            "first-land-pol|Primer jugador en aterrizar en Pol|LANDING|Pol|",
            "first-land-eeloo|Primer jugador en aterrizar en Eeloo|LANDING|Eeloo|",
            "first-eva|Primer jugador en realizar una EVA|VESSEL_TYPE|EVA|",
            "first-docking|Primer jugador en acoplar dos naves|DOCKING||",
            "first-exit-kerbin-soi|Primer jugador en salir del SOI de Kerbin|SOI_EXIT|Kerbin|",
            "first-flag-kerbin-south-pole|Primer jugador en plantar una bandera en el polo sur de Kerbin|FLAG_LAT_AT_MOST|Kerbin|-89",
            "first-flag-kerbin-north-pole|Primer jugador en plantar una bandera en el polo norte de Kerbin|FLAG_LAT_AT_LEAST|Kerbin|89",
            "first-flag-mun-south-pole|Primer jugador en plantar una bandera en el polo sur de Mun|FLAG_LAT_AT_MOST|Mun|-89",
            "first-flag-mun-north-pole|Primer jugador en plantar una bandera en el polo norte de Mun|FLAG_LAT_AT_LEAST|Mun|89",
            "first-flag-minmus-south-pole|Primer jugador en plantar una bandera en el polo sur de Minmus|FLAG_LAT_AT_MOST|Minmus|-89",
            "first-flag-minmus-north-pole|Primer jugador en plantar una bandera en el polo norte de Minmus|FLAG_LAT_AT_LEAST|Minmus|89",
            "first-flag-duna-south-pole|Primer jugador en plantar una bandera en el polo sur de Duna|FLAG_LAT_AT_MOST|Duna|-89",
            "first-flag-duna-north-pole|Primer jugador en plantar una bandera en el polo norte de Duna|FLAG_LAT_AT_LEAST|Duna|89",
            "first-flag-eve-south-pole|Primer jugador en plantar una bandera en el polo sur de Eve|FLAG_LAT_AT_MOST|Eve|-89",
            "first-flag-eve-north-pole|Primer jugador en plantar una bandera en el polo norte de Eve|FLAG_LAT_AT_LEAST|Eve|89",
            "first-flag-mun-equator|Primer jugador en plantar una bandera en el ecuador de Mun|FLAG_LAT_AT_MOST|Mun|0",
            "first-flag-minmus-equator|Primer jugador en plantar una bandera en el ecuador de Minmus|FLAG_LAT_AT_MOST|Minmus|0",
            "first-flag-duna-equator|Primer jugador en plantar una bandera en el ecuador de Duna|FLAG_LAT_AT_MOST|Duna|0",
        });

        public static Dictionary<string, MilestoneRecord> GetMilestonesCopy()
        {
            EnsureLoaded();
            lock (milestoneLock)
            {
                Dictionary<string, MilestoneRecord> returnValue = new Dictionary<string, MilestoneRecord>();
                foreach (KeyValuePair<string, MilestoneRecord> kvp in milestones)
                {
                    MilestoneRecord source = kvp.Value;
                    MilestoneRecord clone = new MilestoneRecord();
                    clone.key = source.key;
                    clone.title = source.title;
                    clone.playerName = source.playerName;
                    clone.utcTicks = source.utcTicks;
                    returnValue[kvp.Key] = clone;
                }
                return returnValue;
            }
        }

        public static bool TryRegisterFromVesselProto(string playerName, string vesselGuid, byte[] vesselData, bool isDockingUpdate)
        {
            EnsureLoaded();
            string vesselString = Encoding.UTF8.GetString(vesselData);
            string vesselSituation = GetConfigValue(vesselString, "sit");
            string vesselType = GetConfigValue(vesselString, "type");
            string bodyName = GetConfigValue(vesselString, "mainBody");
            if (string.IsNullOrEmpty(bodyName))
            {
                bodyName = GetConfigValue(vesselString, "body");
            }
            if (string.IsNullOrEmpty(bodyName))
            {
                bodyName = GetBodyNameFromOrbitReference(vesselString);
            }

            bool changed = false;

            double latitude;
            bool hasLatitude = TryGetConfigDouble(vesselString, "lat", out latitude);
            double altitude;
            bool hasAltitude = TryGetConfigDouble(vesselString, "alt", out altitude);
            foreach (MilestoneDefinition definition in GetMilestoneDefinitionsCopy())
            {
                if (definition.IsSatisfied(vesselSituation, vesselType, bodyName, hasLatitude, latitude, hasAltitude, altitude, vesselGuid, isDockingUpdate, playerName))
                {
                    changed |= TrySetMilestone(definition.key, definition.title, playerName);
                }
            }

            RecordVesselState(vesselGuid, bodyName);

            if (changed)
            {
                DarkLog.Normal("Milestone update from " + playerName + " vessel " + vesselGuid);
            }
            return changed;
        }

        public static List<MilestoneDefinition> GetMilestoneDefinitionsCopy()
        {
            EnsureLoaded();
            lock (milestoneLock)
            {
                return new List<MilestoneDefinition>(milestoneDefinitions.Values);
            }
        }

        public static bool TryRegisterFromVesselUpdate(string playerName, string vesselGuid, string bodyName)
        {
            EnsureLoaded();
            bool changed = false;
            MilestoneVesselState previousState = null;
            lock (milestoneLock)
            {
                if (vesselStates.ContainsKey(vesselGuid))
                {
                    previousState = vesselStates[vesselGuid];
                }
            }

            if (previousState != null && !string.IsNullOrEmpty(previousState.bodyName) && !string.IsNullOrEmpty(bodyName))
            {
                foreach (MilestoneDefinition definition in GetMilestoneDefinitionsCopy())
                {
                    if (definition.ruleType == MilestoneRuleType.SOI_EXIT && string.Equals(definition.bodyName, previousState.bodyName, StringComparison.OrdinalIgnoreCase) && !string.Equals(previousState.bodyName, bodyName, StringComparison.OrdinalIgnoreCase))
                    {
                        changed |= TrySetMilestone(definition.key, definition.title, playerName);
                    }
                }
            }

            RecordVesselState(vesselGuid, bodyName);
            if (changed)
            {
                DarkLog.Normal("Milestone update from " + playerName + " vessel " + vesselGuid);
            }
            return changed;
        }

        private static void RecordVesselState(string vesselGuid, string bodyName)
        {
            if (string.IsNullOrEmpty(vesselGuid))
            {
                return;
            }

            lock (milestoneLock)
            {
                MilestoneVesselState currentState = new MilestoneVesselState();
                currentState.bodyName = bodyName;
                currentState.lastUpdateUtcTicks = DateTime.UtcNow.Ticks;
                vesselStates[vesselGuid] = currentState;
            }
        }

        private static bool TrySetMilestone(string key, string title, string playerName)
        {
            lock (milestoneLock)
            {
                if (milestones.ContainsKey(key))
                {
                    return false;
                }

                MilestoneRecord record = new MilestoneRecord();
                record.key = key;
                record.title = title;
                record.playerName = playerName;
                record.utcTicks = DateTime.UtcNow.Ticks;
                milestones[key] = record;
                SaveMilestonesUnsafe();
                Messages.Chat.SendChatMessageToAll(record.title + " - " + playerName);
                DarkLog.Normal("Milestone unlocked: " + record.title + " by " + playerName);
                return true;
            }
        }

        private static void EnsureLoaded()
        {
            if (loaded)
            {
                return;
            }

            lock (milestoneLock)
            {
                if (loaded)
                {
                    return;
                }

                LoadMilestoneDefinitions();
                milestones.Clear();
                vesselStates.Clear();
                string milestoneFile = GetMilestoneFile();
                if (File.Exists(milestoneFile))
                {
                    string[] lines = File.ReadAllLines(milestoneFile);
                    foreach (string line in lines)
                    {
                        if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#", StringComparison.Ordinal))
                        {
                            continue;
                        }

                        string[] parts = line.Split('\t');
                        if (parts.Length != 3)
                        {
                            continue;
                        }

                        string key = parts[0];
                        string playerName = parts[1];
                        long ticks;
                        if (!long.TryParse(parts[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out ticks))
                        {
                            continue;
                        }
                        if (!milestoneDefinitions.ContainsKey(key))
                        {
                            continue;
                        }

                        MilestoneRecord record = new MilestoneRecord();
                        record.key = key;
                        record.title = milestoneDefinitions[key].title;
                        record.playerName = playerName;
                        record.utcTicks = ticks;
                        milestones[key] = record;
                    }
                }
                loaded = true;
            }
        }

        private static void SaveMilestonesUnsafe()
        {
            string milestoneFile = GetMilestoneFile();
            string milestoneDirectory = Path.GetDirectoryName(milestoneFile);
            if (!Directory.Exists(milestoneDirectory))
            {
                Directory.CreateDirectory(milestoneDirectory);
            }

            List<string> lines = new List<string>();
            lines.Add("# key\\tplayer\\tutcTicks");
            foreach (string key in milestoneDefinitions.Keys)
            {
                if (!milestones.ContainsKey(key))
                {
                    continue;
                }
                MilestoneRecord currentRecord = milestones[key];
                lines.Add(currentRecord.key + "\t" + currentRecord.playerName + "\t" + currentRecord.utcTicks.ToString(CultureInfo.InvariantCulture));
            }

            File.WriteAllLines(milestoneFile, lines.ToArray());
        }

        private static string GetMilestoneFile()
        {
            return Path.Combine(Server.universeDirectory, "Milestones.txt");
        }

        private static string GetMilestoneDefinitionsFile()
        {
            return Path.Combine(Server.configDirectory, "Milestones.txt");
        }

        private static void LoadMilestoneDefinitions()
        {
            milestoneDefinitions.Clear();
            string definitionFile = GetMilestoneDefinitionsFile();
            if (!File.Exists(definitionFile))
            {
                Directory.CreateDirectory(Server.configDirectory);
                File.WriteAllText(definitionFile, defaultMilestoneFileData);
            }

            string[] lines = File.ReadAllLines(definitionFile);
            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#", StringComparison.Ordinal))
                {
                    continue;
                }

                string[] parts = line.Split('|');
                if (parts.Length < 4)
                {
                    continue;
                }

                string key = parts[0].Trim();
                string title = parts[1].Trim();
                string ruleType = parts[2].Trim();
                string bodyName = parts[3].Trim();
                string threshold = parts.Length > 4 ? parts[4].Trim() : string.Empty;

                MilestoneDefinition definition = MilestoneDefinition.Parse(key, title, ruleType, bodyName, threshold);
                if (definition != null && !milestoneDefinitions.ContainsKey(definition.key))
                {
                    milestoneDefinitions.Add(definition.key, definition);
                }
            }

            if (milestoneDefinitions.Count == 0)
            {
                milestoneDefinitions.Add("first-orbit-kerbin", new MilestoneDefinition("first-orbit-kerbin", "Primer jugador en orbitar Kerbin", MilestoneRuleType.ORBIT, "Kerbin", 0));
                milestoneDefinitions.Add("first-orbit-eve", new MilestoneDefinition("first-orbit-eve", "Primer jugador en orbitar Eve", MilestoneRuleType.ORBIT, "Eve", 0));
                milestoneDefinitions.Add("first-flag-kerbin-south-pole", new MilestoneDefinition("first-flag-kerbin-south-pole", "Primer jugador en plantar una bandera en el polo sur de Kerbin", MilestoneRuleType.FLAG_LAT_AT_MOST, "Kerbin", -89));
            }
        }

        private static string GetConfigValue(string configData, string key)
        {
            string pattern = "^\\s*" + Regex.Escape(key) + "\\s*=\\s*(.+?)\\s*$";
            Match match = Regex.Match(configData, pattern, RegexOptions.IgnoreCase | RegexOptions.Multiline);
            if (!match.Success)
            {
                return string.Empty;
            }
            return match.Groups[1].Value.Trim();
        }

        private static bool TryGetConfigDouble(string configData, string key, out double value)
        {
            string configValue = GetConfigValue(configData, key);
            return double.TryParse(configValue, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
        }

        private static string GetBodyNameFromOrbitReference(string configData)
        {
            Match orbitRefMatch = Regex.Match(configData, @"ORBIT\s*\{[\s\S]*?\bREF\s*=\s*(\d+)", RegexOptions.IgnoreCase);
            if (!orbitRefMatch.Success)
            {
                return string.Empty;
            }

            int referenceId;
            if (!int.TryParse(orbitRefMatch.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out referenceId))
            {
                return string.Empty;
            }

            switch (referenceId)
            {
                case 0:
                    return "Sun";
                case 1:
                    return "Kerbin";
                case 2:
                    return "Mun";
                case 3:
                    return "Minmus";
                case 4:
                    return "Moho";
                case 5:
                    return "Eve";
                case 6:
                    return "Duna";
                case 7:
                    return "Ike";
                case 8:
                    return "Jool";
                case 9:
                    return "Laythe";
                case 10:
                    return "Vall";
                case 11:
                    return "Bop";
                case 12:
                    return "Tylo";
                case 13:
                    return "Gilly";
                case 14:
                    return "Pol";
                case 15:
                    return "Dres";
                case 16:
                    return "Eeloo";
                default:
                    return string.Empty;
            }
        }
    }

    public class MilestoneRecord
    {
        public string key;
        public string title;
        public string playerName;
        public long utcTicks;
    }

    public enum MilestoneRuleType
    {
        ORBIT,
        LANDING,
        VESSEL_TYPE,
        DOCKING,
        SOI_EXIT,
        ALTITUDE_AT_LEAST,
        FLAG_LAT_AT_MOST,
        FLAG_LAT_AT_LEAST,
    }

    public class MilestoneDefinition
    {
        public string key;
        public string title;
        public MilestoneRuleType ruleType;
        public string bodyName;
        public double threshold;

        public MilestoneDefinition(string key, string title, MilestoneRuleType ruleType, string bodyName, double threshold)
        {
            this.key = key;
            this.title = title;
            this.ruleType = ruleType;
            this.bodyName = bodyName;
            this.threshold = threshold;
        }

        public static MilestoneDefinition Parse(string key, string title, string ruleType, string bodyName, string threshold)
        {
            MilestoneRuleType parsedRuleType;
            if (!Enum.TryParse(ruleType, true, out parsedRuleType))
            {
                return null;
            }

            double parsedThreshold = 0;
            if (!string.IsNullOrEmpty(threshold))
            {
                if (!double.TryParse(threshold, NumberStyles.Float, CultureInfo.InvariantCulture, out parsedThreshold))
                {
                    return null;
                }
            }

            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(title))
            {
                return null;
            }

            if (parsedRuleType != MilestoneRuleType.DOCKING && string.IsNullOrWhiteSpace(bodyName))
            {
                return null;
            }

            return new MilestoneDefinition(key, title, parsedRuleType, bodyName, parsedThreshold);
        }

        public bool IsSatisfied(string vesselSituation, string vesselType, string bodyName, bool hasLatitude, double latitude, bool hasAltitude, double altitude, string vesselGuid, bool isDockingUpdate, string playerName)
        {
            switch (ruleType)
            {
                case MilestoneRuleType.ORBIT:
                    return string.Equals(vesselSituation, "ORBITING", StringComparison.OrdinalIgnoreCase) && string.Equals(bodyName, this.bodyName, StringComparison.OrdinalIgnoreCase);
                case MilestoneRuleType.LANDING:
                    return (string.Equals(vesselSituation, "LANDED", StringComparison.OrdinalIgnoreCase) || string.Equals(vesselSituation, "SPLASHED", StringComparison.OrdinalIgnoreCase)) && string.Equals(bodyName, this.bodyName, StringComparison.OrdinalIgnoreCase);
                case MilestoneRuleType.VESSEL_TYPE:
                    return string.Equals(vesselType, this.bodyName, StringComparison.OrdinalIgnoreCase);
                case MilestoneRuleType.DOCKING:
                    return isDockingUpdate;
                case MilestoneRuleType.SOI_EXIT:
                    return false;
                case MilestoneRuleType.ALTITUDE_AT_LEAST:
                    return string.Equals(bodyName, this.bodyName, StringComparison.OrdinalIgnoreCase) && hasAltitude && altitude >= threshold;
                case MilestoneRuleType.FLAG_LAT_AT_MOST:
                    return string.Equals(vesselType, "Flag", StringComparison.OrdinalIgnoreCase) && string.Equals(bodyName, this.bodyName, StringComparison.OrdinalIgnoreCase) && hasLatitude && latitude <= threshold;
                case MilestoneRuleType.FLAG_LAT_AT_LEAST:
                    return string.Equals(vesselType, "Flag", StringComparison.OrdinalIgnoreCase) && string.Equals(bodyName, this.bodyName, StringComparison.OrdinalIgnoreCase) && hasLatitude && latitude >= threshold;
                default:
                    return false;
            }
        }
    }

    public class MilestoneVesselState
    {
        public string bodyName;
        public long lastUpdateUtcTicks;
    }
}
