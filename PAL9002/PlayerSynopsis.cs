using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace PAL9002 
{
    public class PlayerSynopsis : INotifyPropertyChanged
    {
        private UInt32 sortPriority = 0;
        public System.UInt32 SortPriority
        {
            get { return sortPriority; }
        }
        public UInt32 id;
        private UInt32 count;
        private string counters;
        private LookupTable lookup;
        public event PropertyChangedEventHandler PropertyChanged;

        public string Type
        {
            get { return lookup.LookupById[id]; }
        }

        public UInt32 Count
        {
            get { return count; }
            set 
            { 
                count = value;
                this.NotifyPropertyChanged("Count");
            }
        }

        public string Counters
        {
            get
            {
                return counters;
            }
            set
            {
                if(value != counters)
                {
                    counters = value;
                    this.NotifyPropertyChanged("Counters");
                }

            }
        }

        public PlayerSynopsis(LookupTable look, UInt32 _type, UInt32 _count, string counter)
        {
            lookup = look;
            id = _type;
            count = _count;
            counters = counter;
            try
            {
                string unittype;
                look.LookupById.TryGetValue(_type, out unittype);

                look.SortPriorityLookup.TryGetValue(unittype, out sortPriority);

            }
            catch (System.Exception ex)
            {
                Exception SHUTTHEFUCKUP = ex;// warnings about unused ex suck
            	sortPriority = 999;
            }
             
        }


        private void NotifyPropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }
    }
}
