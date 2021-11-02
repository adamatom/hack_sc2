using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;


// I hate this file
namespace PAL9002
{
    public enum Offsets
    {
        ModelPtr = 0x8,
        TargetPositionX = 0x74,
        TargetPositionY = 0x78,
        PositionX = 0x40,
        PositionY = 0x44,
        PlayerID = 0x27,
        UnitIDModelPtr = 0x18,
        HealthLast = 0x101,
        HealthFirst = 0x102,
        MegaStructureCheck = 0x0,
        CurrentMaxPopulation = 0x458,
        CurrentPopulation = 0x450,
        Minerals = 0x460,
        Gas = 0x468,
        PlayerName = 0x3c,
        GameDatabaseToModels = 0xd8,
        GameModelsToInfos = 0x90,
        GameModelInfoClassId = 0x10,
        GameModelInfoStringPtr = 0x4,
    };



    public class LookupTable
    {
 
        public Dictionary<string, UInt32> Lookup = new Dictionary<string, UInt32>();
        public Dictionary<UInt32, string> LookupById = new Dictionary<UInt32, string>();
        public Dictionary<string, UInt32> MaxDamageLookup = new Dictionary<string, UInt32>();
        public Dictionary<string, UInt32> SortPriorityLookup = new Dictionary<string, UInt32>();

        public Dictionary<string, string> TerranCounters = new Dictionary<string, string>();
        public Dictionary<string, string> ProtossCounters = new Dictionary<string, string>();
        public Dictionary<string, string> ZergCounters = new Dictionary<string, string>();
        private ProcessMemoryReader reader;
        UInt32 GameDatabasePtr = 0;

        public LookupTable(UInt32 gameDatabasePtr, ProcessMemoryReader pread)
        {
            reader = pread;
            GameDatabasePtr = (UInt32)reader.ReadInteger(gameDatabasePtr);
            UInt32 modelsPtr = (UInt32)reader.ReadInteger(GameDatabasePtr + (UInt32)Offsets.GameDatabaseToModels);

            UInt32 modelinfoPtr = (UInt32)reader.ReadInteger(modelsPtr + (UInt32)Offsets.GameModelsToInfos);

            UInt32 modelInfo = (UInt32)reader.ReadInteger(modelinfoPtr);
            int count = 0;
            while(modelInfo != 0)
            {
                try
                {
                    UInt32 unitid = (UInt32)reader.ReadInteger(modelInfo + (UInt32)Offsets.GameModelInfoClassId);

                    // Get pointer to the string
                    UInt32 strPtr = (UInt32)reader.ReadInteger(modelInfo + (UInt32)Offsets.GameModelInfoStringPtr);
                    UInt32 strLen = (UInt32)reader.ReadInteger(strPtr);
                    string unitname = "";
                    if (strLen <= 7)
                    {
                        unitname = reader.ReadString(strPtr + 0x8, (UInt32)reader.ReadInteger(strPtr));
                    }
                    else
                    {
                        UInt32 longstring = (UInt32)reader.ReadInteger(strPtr + 0x8);
                        unitname = reader.ReadString(longstring, (UInt32)reader.ReadInteger(strPtr));
                    }
                    Lookup.Add(unitname, unitid);
                    LookupById.Add(unitid, unitname);

                }
                catch (System.Exception ex)
                {
                	
                }
                finally
                {
                    ++count;
                    modelInfo = (UInt32)reader.ReadInteger(modelinfoPtr + (UInt32)(4 * count));
                }
            }



            //terran counters to terran units
                TerranCounters["SiegeTankSieged"] = "Banshee, Battlecruiser";
            TerranCounters["SiegeTank"] = "Banshee, Battlecruiser";
            TerranCounters["VikingAssault"] = "Marine, Marauder, Siege Tank";
            TerranCounters["VikingFighter"] = "Marine, VikingFighter";
            TerranCounters["Marine"] = "Hellion, Siege Tank";
            TerranCounters["Reaper"] = "Marauder, Siege Tank, Banshee, VikingAssault, Thor";
            TerranCounters["Ghost"] = "Anything";
            TerranCounters["Marauder"] = "Siege Tank, Banshee, Battlecruiser";
            TerranCounters["Thor"] = "Marine, Marauder, Banshee, Siege Tank";
            TerranCounters["Hellion"] = "Marauder, Siege Tank, Viking, Banshee, Thor,Battlecruiser";
            TerranCounters["Medivac"] = "Anything";
            TerranCounters["Banshee"] = "Viking, Thor,Battlecruiser";
            TerranCounters["Raven"] = "Anything";
            TerranCounters["Battlecruiser"] = "Marine, Viking";

            //terran counters to protoss units
            TerranCounters["Zealot"] = "Reaper, Hellion, Banshee, Battlecruiser";
            TerranCounters["Stalker"] = "Marine, Marauder, Siege Tank";
            TerranCounters["Sentry"] = "Reaper, Hellion, Siege Tank, Banshee, Ghost, Thor, Battlecruiser";
            TerranCounters["Observer"] = "";
            TerranCounters["Immortal"] = "Marine, Banshee, Ghost, Battlecruiser";
            TerranCounters["Colossus"] = "Viking, Banshee, Ghost, Thor, Battlecruiser";
            TerranCounters["Phoenix"] = "Marine, Viking, Thor";
            TerranCounters["VoidRay"] = "Marine, Viking";
            TerranCounters["HighTemplar"] = "Siege Tank, Banshee";
            TerranCounters["DarkTemplar"] = "Hellion, Banshee, Battlecruiser";
            TerranCounters["Archon"] = "Battlecruiser, Ghost, Thor";
            TerranCounters["Carrier"] = "Viking, Battlecruiser";
            TerranCounters["Interceptor"] = "Anything";
            TerranCounters["Mothership"] = "Anything";

            //terran counters to zerg units
            TerranCounters["Zergling"] = "Reaper, Hellion, Banshee, Battlecruiser";
            TerranCounters["ZerglingBurrowed"] = "Reaper, Hellion, Banshee, Battlecruiser";
            TerranCounters["QueenBurrowed"] = "Anything";
            TerranCounters["Queen"] = "Anything";
            TerranCounters["Hydralisk"] = "Hellion, Siege Tank";
            TerranCounters["HydraliskBurrowed"] = "Hellion, Siege Tank";
            TerranCounters["Baneling"] = "Banshee, Battlecruiser";
            TerranCounters["BanelingEgg"] = "Banshee, Battlecruiser";
            TerranCounters["BanelingBurrowed"] = "Banshee, Battlecruiser";
            TerranCounters["Roach"] = "Marauder, Siege Tank, Banshee, Battlecruiser";
            TerranCounters["RoachBurrowed"] = "Marauder, Siege Tank, Banshee, Battlecruiser";
            TerranCounters["Infestor"] = "Anything";
            TerranCounters["InfestorBurrowed"] = "Anything";
            TerranCounters["Mutalisk"] = "Marine, Viking, Thor, Battlecruiser";
            TerranCounters["Corruptor"] = "Marine, Thor";
            TerranCounters["Ultralisk"] = "Marauder, Siege Tank, Banshee, Battlecruiser";
            TerranCounters["UltraliskBurrowed"] = "Marauder, Siege Tank, Banshee, Battlecruiser";
            TerranCounters["BroodLord"] = "Viking, Battlecruiser";

            //protoss counters to terran units
            ProtossCounters["SiegeTankSieged"] = "Zealot, Phoenix, Void Ray, Dark Templar, Carrier";
            ProtossCounters["SiegeTank"] = "Zealot, Phoenix, Void Ray, Dark Templar, Carrier";
            ProtossCounters["VikingAssault"] = "Immortal, Stalker";
            ProtossCounters["VikingFighter"] = "Immortal, Stalker";
            ProtossCounters["Marine"] = "Zealot, Colossus, High Templar";
            ProtossCounters["Reaper"] = "Zealot, Stalker, Colossus, High Templar";
            ProtossCounters["Ghost"] = "Anything";
            ProtossCounters["Marauder"] = "Zealot, Immortal, Void Ray, Dark Templar, Carrier";
            ProtossCounters["Thor"] = "Immortal, Zealot";
            ProtossCounters["Hellion"] = "Stalker, Immortal, Void Ray, Colossus, Carrier";
            ProtossCounters["Medivac"] = "Anything";
            ProtossCounters["Banshee"] = "Phoenix, Void Ray, Carrier";
            ProtossCounters["Raven"] = "Anything";
            ProtossCounters["Battlecruiser"] = "Immortal, Stalker";

            //protoss counters to protoss units
            ProtossCounters["Zealot"] = "Void Ray, Colossus, Dark Templar, Carrier";
            ProtossCounters["Stalker"] = "Zealot, Immortal, Dark Templar";
            ProtossCounters["Sentry"] = "Zealot, Phoenix, Void Ray, Colossus, Dark Templar";
            ProtossCounters["Observer"] = "";
            ProtossCounters["Immortal"] = "Zealot, Phoenix, Void Ray, Dark Templar, Carrier";
            ProtossCounters["Colossus"] = "Zealot, Immortal, Void Ray, Carrier";
            ProtossCounters["Phoenix"] = "Stalker, Carrier";
            ProtossCounters["VoidRay"] = "Stalker, Phoenix";
            ProtossCounters["HighTemplar"] = "Zealot, Immortal, Phoenix, Colossus, Dark Templar, Carrier";
            ProtossCounters["DarkTemplar"] = "Phoenix, Void Ray, Carrier";
            ProtossCounters["Archon"] = "Phoenix, Colossus, Carrier";
            ProtossCounters["Carrier"] = "Stalker";
            ProtossCounters["Interceptor"] = "Anything";
            ProtossCounters["Mothership"] = "Anything";

            //protoss counters to zerg units
            ProtossCounters["Zergling"] = "Zealot, Void Ray, Dark Templar, Colossus, Carrier";
            ProtossCounters["ZerglingBurrowed"] = "Zealot, Void Ray, Dark Templar, Colossus, Carrier";
            ProtossCounters["QueenBurrowed"] = "Anything";
            ProtossCounters["Queen"] = "Anything";
            ProtossCounters["Hydralisk"] = "Zealot, High Templar, Colossus";
            ProtossCounters["HydraliskBurrowed"] = "Zealot, High Templar, Colossus";
            ProtossCounters["Baneling"] = "Stalker, Immortal, Void Ray, Colossus, Carrier";
            ProtossCounters["BanelingEgg"] = "Stalker, Immortal, Void Ray, Colossus, Carrier";
            ProtossCounters["BanelingBurrowed"] = "Stalker, Immortal, Void Ray, Colossus, Carrier";
            ProtossCounters["Roach"] = "Stalker, Immortal, Void Ray, Colossus, Carrier";
            ProtossCounters["RoachBurrowed"] = "Stalker, Immortal, Void Ray, Colossus, Carrier";
            ProtossCounters["Infestor"] = "Anything";
            ProtossCounters["InfestorBurrowed"] = "Anything";
            ProtossCounters["Mutalisk"] = "Stalker, Sentry, Phoenix, High Templar";
            ProtossCounters["Corruptor"] = "Stalker";
            ProtossCounters["Ultralisk"] = "Immortal, Void Ray, Carrier";
            ProtossCounters["UltraliskBurrowed"] = "Immortal, Void Ray, Carrier";
            ProtossCounters["BroodLord"] = "Phoenix, Void Ray, Carrier";

            //zerg counters to terran units
            ZergCounters["SiegeTankSieged"] = "Zergling, Mutalisk, Brood Lord";
            ZergCounters["SiegeTank"] = "Zergling, Mutalisk, Brood Lord";
            ZergCounters["VikingAssault"] = "Zergling";
            ZergCounters["VikingFighter"] = "Hydralisk";
            ZergCounters["Marine"] = "Baneling, Brood Lord";
            ZergCounters["Reaper"] = "Roach, Baneling, Brood Lord, Ultralisk";
            ZergCounters["Ghost"] = "Anything";
            ZergCounters["Marauder"] = "Zergling, Mutalisk, Hydralisk, Brood Lord";
            ZergCounters["Thor"] = "Zergling, Baneling";
            ZergCounters["Hellion"] = "Roach, Baneling, Mutalisk, Brood Lord, Ultralisk";
            ZergCounters["Medivac"] = "Anything";
            ZergCounters["Banshee"] = "Mutalisk, Hydralisk, Corruptor";
            ZergCounters["Raven"] = "Anything";
            ZergCounters["Battlecruiser"] = "Corruptor";

            //zerg counters to protoss units
            ZergCounters["Zealot"] = "Baneling, Roach, Mutalisk, Boord Lord, Ultralisk, ";
            ZergCounters["Stalker"] = "Zergling";
            ZergCounters["Sentry"] = "Zergling, Baneling, Brood Lord, Ultralisk";
            ZergCounters["Observer"] = "";
            ZergCounters["Immortal"] = "Zergling, Mutalisk, Hydralisk, Brood Lord";
            ZergCounters["Colossus"] = "Zergling, Mutalisk, Corruptor, Brood Lord";
            ZergCounters["Phoenix"] = "Hydralisk, Corruptor";
            ZergCounters["VoidRay"] = "Hydralisk, Corruptor";
            ZergCounters["HighTemplar"] = "Brood Lord, Ultralisk";
            ZergCounters["DarkTemplar"] = "Overseer, Mutalisk, Baneling, Hydralisk, Brood Lord";
            ZergCounters["Archon"] = "Brood Lord";
            ZergCounters["Carrier"] = "Hydralisk, Corruptor";
            ZergCounters["Interceptor"] = "Anything";
            ZergCounters["Mothership"] = "Anything";

            //zerg counters to zerg units
            ZergCounters["Zergling"] = "Baneling, Roach, Mutalisk, Brood Lord, Ultralisk";
            ZergCounters["ZerglingBurrowed"] = "Baneling, Roach, Mutalisk, Brood Lord, Ultralisk";
            ZergCounters["QueenBurrowed"] = "Anything";
            ZergCounters["Queen"] = "Anything";
            ZergCounters["Hydralisk"] = "Zergling, Ultralisk";
            ZergCounters["HydraliskBurrowed"] = "Zergling, Ultralisk";
            ZergCounters["Baneling"] = "Roach, Mutalisk, Brood Lord, Ultralisk";
            ZergCounters["BanelingEgg"] = "Roach, Mutalisk, Brood Lord, Ultralisk";
            ZergCounters["BanelingBurrowed"] = "Roach, Mutalisk, Brood Lord, Ultralisk";
            ZergCounters["Roach"] = "Mutalisk, Brood Lord";
            ZergCounters["RoachBurrowed"] = "Mutalisk, Brood Lord";
            ZergCounters["Infestor"] = "Anything";
            ZergCounters["InfestorBurrowed"] = "Anything";
            ZergCounters["Mutalisk"] = "Hydralisk, Corruptor";
            ZergCounters["Corruptor"] = "Hydralisk";
            ZergCounters["Ultralisk"] = "Roach, Mutalisk, Brood Lord";
            ZergCounters["UltraliskBurrowed"] = "Roach, Mutalisk, Brood Lord";
            ZergCounters["BroodLord"] = "Mutalisks, Corruptor";

            //terran
            SortPriorityLookup["TechLab"] = 4;
            SortPriorityLookup["NuclearReactor"] = 4;
            SortPriorityLookup["PointDefenseDrone"] = 4;
            SortPriorityLookup["OrbitalCommand"] = 4;
            SortPriorityLookup["CommandCenter"] = 4;
            SortPriorityLookup["Refinery"] = 4;
            SortPriorityLookup["EngineeringBay"] = 4;
            SortPriorityLookup["SensorTower"] = 4;
            SortPriorityLookup["AutoTurret"] = 4;
            SortPriorityLookup["SupplyDepot"] = 4;
            SortPriorityLookup["GhostAcademyFlying"] = 4;
            SortPriorityLookup["BarracksTechLab"] = 4;
            SortPriorityLookup["BarracksNuclearReactor"] = 4;
            SortPriorityLookup["FactoryTechLab"] = 4;
            SortPriorityLookup["FactoryNuclearReactor"] = 4;
            SortPriorityLookup["StarportTechLab"] = 4;
            SortPriorityLookup["StarportNuclearReactor"] = 4;
            SortPriorityLookup["SupplyDepotLowered"] = 4;


            SortPriorityLookup["FactoryFlying"] = 3;
            SortPriorityLookup["StarportFlying"] = 3;
            SortPriorityLookup["OrbitalCommandFlying"] = 3;
            SortPriorityLookup["CommandCenterFlying"] = 3;
            SortPriorityLookup["PlanetaryFortress"] = 3;
            SortPriorityLookup["Barracks"] = 3;
            SortPriorityLookup["Bunker"] = 3;
            SortPriorityLookup["GhostAcademy"] = 3;
            SortPriorityLookup["Factory"] = 3;
            SortPriorityLookup["Starport"] = 3;
            SortPriorityLookup["Armory"] = 3;
            SortPriorityLookup["FusionCore"] = 3;
            SortPriorityLookup["BarracksFlying"] = 3;

            SortPriorityLookup["MissileTurret"] = 2;
            SortPriorityLookup["MULE"] = 2;
            SortPriorityLookup["SCV"] = 2;

            SortPriorityLookup["SiegeTankSieged"] = 1;
            SortPriorityLookup["SiegeTank"] = 1;
            SortPriorityLookup["VikingAssault"] = 1;
            SortPriorityLookup["VikingFighter"] = 1;
            SortPriorityLookup["Marine"] = 1;
            SortPriorityLookup["Reaper"] = 1;
            SortPriorityLookup["Ghost"] = 1;
            SortPriorityLookup["Marauder"] = 1;
            SortPriorityLookup["Thor"] = 1;
            SortPriorityLookup["Hellion"] = 1;
            SortPriorityLookup["Medivac"] = 1;
            SortPriorityLookup["Banshee"] = 1;
            SortPriorityLookup["Raven"] = 1;
            SortPriorityLookup["Battlecruiser"] = 1;
            SortPriorityLookup["Nuke"] = 1; 
            
            

            //protoss
            SortPriorityLookup["Nexus"] = 4;
            SortPriorityLookup["Pylon"] = 4;
            SortPriorityLookup["Assimilator"] = 4;
            SortPriorityLookup["Gateway"] = 4;
            SortPriorityLookup["Forge"] = 4;
            SortPriorityLookup["PhotonCannon"] = 3;
            SortPriorityLookup["WarpGate"] = 4;
            SortPriorityLookup["CyberneticsCore"] = 4;
            SortPriorityLookup["TwilightCouncil"] = 4;
            SortPriorityLookup["RoboticsFacility"] = 4;
            SortPriorityLookup["Stargate"] = 4;
            SortPriorityLookup["TemplarArchive"] = 4;
            SortPriorityLookup["DarkShrine"] = 4;
            SortPriorityLookup["RoboticsBay"] = 4;
            SortPriorityLookup["FleetBeacon"] = 4;

            SortPriorityLookup["Probe"] = 2;
            SortPriorityLookup["WarpPrism"] = 2;
            SortPriorityLookup["WarpPrismPhasing"] = 2;

            SortPriorityLookup["Zealot"] = 1;
            SortPriorityLookup["Stalker"] = 1;
            SortPriorityLookup["Sentry"] = 1;
            SortPriorityLookup["Observer"] = 1;
            SortPriorityLookup["Immortal"] = 1;

            SortPriorityLookup["Colossus"] = 1;
            SortPriorityLookup["Phoenix"] = 1;
            SortPriorityLookup["VoidRay"] = 1;
            SortPriorityLookup["HighTemplar"] = 1;
            SortPriorityLookup["DarkTemplar"] = 1;
            SortPriorityLookup["Archon"] = 1;
            SortPriorityLookup["Carrier"] = 1;
            SortPriorityLookup["Interceptor"] = 1;
            SortPriorityLookup["Mothership"] = 1;


            //zerg
            SortPriorityLookup["Hatchery"] = 4;
            SortPriorityLookup["Extractor"] = 4;
            SortPriorityLookup["SpawningPool"] = 4;
            SortPriorityLookup["EvolutionChamber"] = 4;
            SortPriorityLookup["HydraliskDen"] = 4;
            SortPriorityLookup["BanelingNest"] = 4;
            SortPriorityLookup["Lair"] = 4;
            SortPriorityLookup["RoachWarren"] = 4;
            SortPriorityLookup["InfestationPit"] = 4;
            SortPriorityLookup["Spire"] = 4;
            SortPriorityLookup["NydusNetwork"] = 4;
            SortPriorityLookup["Hive"] = 4;
            SortPriorityLookup["UltraliskCavern"] = 4;
            SortPriorityLookup["GreaterSpire"] = 4;
            SortPriorityLookup["CreepTumor"] = 4;
            SortPriorityLookup["CreepTumorBurrowed"] = 4;
            SortPriorityLookup["Larva"] = 4;
            SortPriorityLookup["Overlord"] = 4;

            SortPriorityLookup["SpineCrawler"] = 3;
            SortPriorityLookup["SpineCrawlerUprooted"] = 3;
            SortPriorityLookup["SporeCrawler"] = 3;
            SortPriorityLookup["SporeCrawlerUprooted"] = 3;
            SortPriorityLookup["Overseer"] = 3;

            SortPriorityLookup["Drone"] = 2;
            SortPriorityLookup["DroneBurrowed"] = 2;
            SortPriorityLookup["InfestedTerran"] = 2;
            SortPriorityLookup["InfestedTerranBurrowed"] = 2;
            SortPriorityLookup["InfestedTerransEgg"] = 2;
            SortPriorityLookup["NydusCanal"] = 2;

            SortPriorityLookup["Zergling"] = 1;
            SortPriorityLookup["ZerglingBurrowed"] = 1;
            SortPriorityLookup["QueenBurrowed"] = 1;
            SortPriorityLookup["Queen"] = 1;
            SortPriorityLookup["Hydralisk"] = 1;
            SortPriorityLookup["HydraliskBurrowed"] = 1;
            SortPriorityLookup["Baneling"] = 1;
            SortPriorityLookup["BanelingEgg"] = 1;
            SortPriorityLookup["BanelingBurrowed"] = 1;
            SortPriorityLookup["Roach"] = 1;
            SortPriorityLookup["RoachBurrowed"] = 1;
            SortPriorityLookup["Infestor"] = 1;
            SortPriorityLookup["InfestorBurrowed"] = 1;
            SortPriorityLookup["Mutalisk"] = 1;
            SortPriorityLookup["Corruptor"] = 1;
            SortPriorityLookup["NydusWorm"] = 1;
            SortPriorityLookup["Ultralisk"] = 1;
            SortPriorityLookup["UltraliskBurrowed"] = 1;
            SortPriorityLookup["BroodLord"] = 1;
            SortPriorityLookup["Changeling"] = 1;
            SortPriorityLookup["Broodling"] = 1;
            SortPriorityLookup["BroodlingDefault"] = 1;
            SortPriorityLookup["ChangelingZealot"] = 1;
            SortPriorityLookup["ChangelingMarineShield"] = 1;
            SortPriorityLookup["ChangelingMarine"] = 1;
            SortPriorityLookup["ChangelingZerglingWings"] = 1;
            SortPriorityLookup["ChangelingZergling"] = 1;
            //////////////////////////////////////////////////////////////////////////
            //terran
            MaxDamageLookup["TechLab"] = 400;
            MaxDamageLookup["NuclearReactor"] = 400;
            MaxDamageLookup["PointDefenseDrone"] = 50;
            MaxDamageLookup["OrbitalCommand"] = 1500;
            MaxDamageLookup["OrbitalCommandFlying"] = 1500;
            MaxDamageLookup["CommandCenter"] = 1500;
            MaxDamageLookup["CommandCenterFlying"] = 1500;
            MaxDamageLookup["PlanetaryFortress"] = 1500;
            MaxDamageLookup["Refinery"] = 500;
            MaxDamageLookup["Barracks"] = 1000;
            MaxDamageLookup["EngineeringBay"] = 850;
            MaxDamageLookup["MissileTurret"] = 200;
            MaxDamageLookup["Bunker"] = 400;
            MaxDamageLookup["SensorTower"] = 200;
            MaxDamageLookup["GhostAcademy"] = 1250;
            MaxDamageLookup["Factory"] = 1250;
            MaxDamageLookup["Starport"] = 1300;
            MaxDamageLookup["Armory"] = 750;
            MaxDamageLookup["FusionCore"] = 750;
            MaxDamageLookup["AutoTurret"] = 150;
            MaxDamageLookup["SupplyDepot"] = 500;
            MaxDamageLookup["SiegeTankSieged"] = 150;
            MaxDamageLookup["SiegeTank"] = 150;
            MaxDamageLookup["VikingAssault"] = 125;
            MaxDamageLookup["VikingFighter"] = 125;
            MaxDamageLookup["BarracksTechLab"] = 400;
            MaxDamageLookup["BarracksNuclearReactor"] = 400;
            MaxDamageLookup["FactoryTechLab"] = 400;
            MaxDamageLookup["FactoryNuclearReactor"] = 400;
            MaxDamageLookup["StarportTechLab"] = 400;
            MaxDamageLookup["StarportNuclearReactor"] = 400;
            MaxDamageLookup["FactoryFlying"] = 1250;
            MaxDamageLookup["StarportFlying"] = 1300;
            MaxDamageLookup["SCV"] = 45;
            MaxDamageLookup["BarracksFlying"] = 1000;
            MaxDamageLookup["SupplyDepotLowered"] = 500;
            MaxDamageLookup["Marine"] = 45;
            MaxDamageLookup["Reaper"] = 50;
            MaxDamageLookup["Ghost"] = 100;
            MaxDamageLookup["Marauder"] = 125;
            MaxDamageLookup["Thor"] = 400;
            MaxDamageLookup["Hellion"] = 90;
            MaxDamageLookup["Medivac"] = 150;
            MaxDamageLookup["Banshee"] = 140;
            MaxDamageLookup["Raven"] = 140;
            MaxDamageLookup["Battlecruiser"] = 550;
            MaxDamageLookup["Nuke"] = 1000; // no idea what to really use
            MaxDamageLookup["MULE"] = 60;
            MaxDamageLookup["GhostAcademyFlying"] = 1250;

            //protoss
            MaxDamageLookup["Nexus"] = 750;
            MaxDamageLookup["Pylon"] = 200;
            MaxDamageLookup["Assimilator"] = 450;
            MaxDamageLookup["Gateway"] = 500;
            MaxDamageLookup["Forge"] = 400;
            MaxDamageLookup["PhotonCannon"] = 150;
            MaxDamageLookup["WarpGate"] = 500;
            MaxDamageLookup["CyberneticsCore"] = 550;
            MaxDamageLookup["TwilightCouncil"] = 500;
            MaxDamageLookup["RoboticsFacility"] = 450;
            MaxDamageLookup["Stargate"] = 600;
            MaxDamageLookup["TemplarArchive"] = 500;
            MaxDamageLookup["DarkShrine"] = 500;
            MaxDamageLookup["RoboticsBay"] = 500;
            MaxDamageLookup["FleetBeacon"] = 500;

            MaxDamageLookup["Probe"] = 20;
            MaxDamageLookup["Zealot"] = 100;
            MaxDamageLookup["Stalker"] = 80;
            MaxDamageLookup["Sentry"] = 40;
            MaxDamageLookup["Observer"] = 40;
            MaxDamageLookup["Immortal"] = 200;
            MaxDamageLookup["WarpPrism"] = 100;
            MaxDamageLookup["WarpPrismPhasing"] = 100;
            MaxDamageLookup["Colossus"] = 200;
            MaxDamageLookup["Phoenix"] = 120;
            MaxDamageLookup["VoidRay"] = 150;
            MaxDamageLookup["HighTemplar"] = 40;
            MaxDamageLookup["DarkTemplar"] = 40;
            MaxDamageLookup["Archon"] = 10;
            MaxDamageLookup["Carrier"] = 300;
            MaxDamageLookup["Interceptor"] = 40;
            MaxDamageLookup["Mothership"] = 350;


            //zerg
            MaxDamageLookup["Hatchery"] = 1250;
            MaxDamageLookup["Extractor"] = 500;
            MaxDamageLookup["SpawningPool"] = 750;
            MaxDamageLookup["EvolutionChamber"] = 750;
            MaxDamageLookup["SpineCrawler"] = 300;
            MaxDamageLookup["SpineCrawlerUprooted"] = 300;
            MaxDamageLookup["SporeCrawler"] = 400;
            MaxDamageLookup["SporeCrawlerUprooted"] = 400;
            MaxDamageLookup["HydraliskDen"] = 850;
            MaxDamageLookup["BanelingNest"] = 850;
            MaxDamageLookup["Lair"] = 1800;
            MaxDamageLookup["RoachWarren"] = 850;
            MaxDamageLookup["InfestationPit"] = 850;
            MaxDamageLookup["Spire"] = 600;
            MaxDamageLookup["NydusNetwork"] = 850;
            MaxDamageLookup["Hive"] = 2500;
            MaxDamageLookup["UltraliskCavern"] = 600;
            MaxDamageLookup["GreaterSpire"] = 1000;
            MaxDamageLookup["CreepTumor"] = 50;
            MaxDamageLookup["CreepTumorBurrowed"] = 50;
            
            MaxDamageLookup["Larva"] = 25;
            MaxDamageLookup["Drone"] = 40;
            MaxDamageLookup["DroneBurrowed"] = 40;
            MaxDamageLookup["Overlord"] = 200;
            MaxDamageLookup["Zergling"] = 35;
            MaxDamageLookup["ZerglingBurrowed"] = 35;
            MaxDamageLookup["QueenBurrowed"] = 175;
            MaxDamageLookup["Queen"] = 175;
            MaxDamageLookup["Hydralisk"] = 80;
            MaxDamageLookup["HydraliskBurrowed"] = 80;
            MaxDamageLookup["Baneling"] = 30;
            MaxDamageLookup["BanelingEgg"] = 50;
            MaxDamageLookup["BanelingBurrowed"] = 30;
            MaxDamageLookup["Overseer"] = 200;
            MaxDamageLookup["Roach"] = 145;
            MaxDamageLookup["RoachBurrowed"] = 145;
            MaxDamageLookup["Infestor"] = 90;
            MaxDamageLookup["InfestorBurrowed"] = 90;
            MaxDamageLookup["InfestedTerran"] = 50;
            MaxDamageLookup["InfestedTerranBurrowed"] = 50;
            MaxDamageLookup["InfestedTerransEgg"] = 50;
            MaxDamageLookup["Mutalisk"] = 120;
            MaxDamageLookup["Corruptor"] = 200;
            MaxDamageLookup["NydusWorm"] = 200;
            MaxDamageLookup["NydusCanal"] = 200;
            MaxDamageLookup["Ultralisk"] = 500;
            MaxDamageLookup["UltraliskBurrowed"] = 500;
            MaxDamageLookup["BroodLord"] = 225;
            MaxDamageLookup["Changeling"] = 5;
            MaxDamageLookup["Broodling"] = 30;
            MaxDamageLookup["BroodlingDefault"] = 30;
            MaxDamageLookup["ChangelingZealot"] = 39;
            MaxDamageLookup["ChangelingMarineShield"] = 40;
            MaxDamageLookup["ChangelingMarine"] = 41;
            MaxDamageLookup["ChangelingZerglingWings"] = 42;
            MaxDamageLookup["ChangelingZergling"] = 43;




            // Unknown
            MaxDamageLookup["BroodLordCocoon"] = 144;
            MaxDamageLookup["OverlordCocoon"] = 155;
            MaxDamageLookup["BroodlingEscort"] = 169;
            MaxDamageLookup["Observatory"] = 172;

        }

    }
}
