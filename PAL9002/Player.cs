using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Forms;
using System.Threading;

namespace PAL9002
{
    class Player
    {
        /// <summary>
        /// key is the location in memory
        /// </summary>
        Dictionary<UInt32, Unit> unitList = new Dictionary<UInt32, Unit>();
        public Dictionary<UInt32, Unit> UnitList
        {
            get { return unitList; }
        }

        // Contains a hash table with units worth paying attention to
        public LookupTable units;

        private static Mutex mut = new Mutex();

        DataGridView dataView;
        public SortableBindingList<PlayerSynopsis> UnitCounts = new SortableBindingList<PlayerSynopsis>();
        public BindingSource bindingSource;

        ProcessMemoryReader pReader;
        UInt32 playerNum;
        public System.UInt32 PlayerNum
        {
            get { return playerNum; }
        }

        string race;
        public string Race
        {
            get
            { 
                return race; 
            }
            set
            {
                race = value;
                DefineCounters();


            }
        }

        private void DefineCounters()
        {

            foreach (PlayerSynopsis ps in UnitCounts)
            {

                switch (race)
                {
                    case "Terran":
                        if (units.TerranCounters.ContainsKey(ps.Type))
                        {
                            ps.Counters = units.TerranCounters[ps.Type];
                        }
                        else
                        {
                            ps.Counters = "";
                        }
                        break;
                    case "Protoss":
                        if (units.ProtossCounters.ContainsKey(ps.Type))
                        {
                            ps.Counters = units.ProtossCounters[ps.Type];
                        }
                        else
                        {
                            ps.Counters = "";
                        }
                        break;
                    case "Zerg":
                        if (units.ZergCounters.ContainsKey(ps.Type))
                        {
                            ps.Counters = units.ZergCounters[ps.Type];
                        }
                        else
                        {
                            ps.Counters = "";
                        }
                        break;
                    default:
                        break;
                }



            }
        }

        

        public Player(UInt32 playernum, LookupTable u, ProcessMemoryReader r, DataGridView dgv, BindingSource bs, string prace)
        {
            playerNum = playernum;
            units = u;
            pReader = r;
            dataView = dgv;
            race = prace;
            bindingSource = bs;
        }

        // This can be called very rapidly (for location updates)
        public void UpdateUnits()
        {
            mut.WaitOne();
            List<Unit> pruneme = new List<Unit>();
            foreach(Unit unit in unitList.Values)
            {
                if(unit.Update() == false)
                {
                    pruneme.Add(unit);
                }
            }

            foreach(Unit unit in pruneme)
            {
                unitList.Remove(unit.BaseAddress);
            }
            mut.ReleaseMutex();
        }

        // This updates the list. Called less frequently
        public void UpdateUnitList(List<UInt32> unitpointers)
        {
            mut.WaitOne();

            // Add or update all units being pushed to us
            foreach (UInt32 unitpointer in unitpointers)
            {
                // check if we know about this object
                if (unitList.ContainsKey(unitpointer) == false) // new unit
                {
                    Unit newunit = new Unit(unitpointer, units, pReader);
                    newunit.Update();

                    unitList.Add(unitpointer, newunit);
                }
            }

            List<Unit> pruneme = new List<Unit>();

            // Go through our own list and remove units that are no longer ours
            foreach (Unit unit in unitList.Values)
            {
                unit.Update();
                bool found = false;
                foreach (UInt32 unitpointer in unitpointers)
                {
                    if (unit.BaseAddress == unitpointer)
                    {
                        found = true;
                        break;
                    }
                }

                //Find any units we are tracking that have been repurposed as 
                // a different player. This is for infestors, and when a dead unit slot
                // is reused
                if (unit.PlayerID != playerNum)
                {
                    found = false;
                }


                // a unit in our local list was not found in the incoming list
                //  the incoming list is truth
                if (found == false)
                {
                    pruneme.Add(unit);
                }
            }

            foreach (Unit unit in pruneme)
            {
                unitList.Remove(unit.BaseAddress);
            }


            // Now we have to update the unit counts. First count the units
            Dictionary<UInt32, UInt32> UnitTypeCount = new Dictionary<UInt32, UInt32>();

            foreach (Unit unit in unitList.Values)
            {
                if (unit.IsDead) continue;
                if (UnitTypeCount.ContainsKey(unit.TypeID))
                {
                    UnitTypeCount[unit.TypeID] = UnitTypeCount[unit.TypeID] + 1;
                }
                else
                {
                    UnitTypeCount[unit.TypeID] = 1;
                }
            }// count is done

            List<PlayerSynopsis> pruneme2 = new List<PlayerSynopsis>();
            // loop through our watched list (data is updated on UI) and update
            foreach(PlayerSynopsis ps in UnitCounts)
            {
                if(UnitTypeCount.ContainsKey(ps.id) == true)
                {
                    if (ps.Count != UnitTypeCount[ps.id])
                    {
                        ps.Count = UnitTypeCount[ps.id];
 
                    }
                    
                    // remove update entries. At the end if any are left in the 
                    // dict then they are a new unit count and deserve a new line 
                    // in the datagridview/PlayerSynopsis
                    UnitTypeCount.Remove(ps.id);  
                }
                else // if the unit type count doesnt contain an entry for this id we have to remove this playersynopsis
                {
                    pruneme2.Add(ps);
                    
                }
            }
            foreach (PlayerSynopsis ps in pruneme2)
            {
                UnitCounts.Remove(ps);
            }

            //now create any new PlayerSynopsis/unit counts
            foreach (UInt32 id in UnitTypeCount.Keys)//this is empty if nothing new to add
            {
                string counter = "";
                string unitstr = "";

                    unitstr = units.LookupById[id];

                switch (race)
                {
                    case "Terran":
                        if (units.TerranCounters.ContainsKey(unitstr))
                        {
                            counter = units.TerranCounters[unitstr];
                        }
                        break;
                    case "Protoss":
                        if (units.ProtossCounters.ContainsKey(unitstr))
                        {
                            counter = units.TerranCounters[unitstr];
                        }
                        break;
                    case "Zerg":
                        if (units.ZergCounters.ContainsKey(unitstr))
                        {
                            counter = units.TerranCounters[unitstr];
                        }
                        break;
                    default:
                        break;
                }

                UnitCounts.Add(new PlayerSynopsis(units, id, UnitTypeCount[id], counter));

                for (int i = 0; i < dataView.Columns.Count; i++)
                {
                    dataView.AutoResizeColumn(i);
                }
                if (dataView.Columns.Count > 2)
                {
                    dataView.Columns[0].Visible = false;

                    dataView.Columns[2].Width = 30;
                    dataView.Columns[2].HeaderText = "#";
                    dataView.Columns[3].HeaderText = "Weak Against:";
                    dataView.Columns[3].Width = 300;
                    bindingSource.Sort = "SortPriority ASC, Count DESC";
                }

            }

            DefineCounters();
            //mut.ReleaseMutex();
        }
    }
}
