using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PAL9002
{
    class Unit
    {
        private UInt32 m_baseAddress;
        public System.UInt32 BaseAddress
        {
            get { return m_baseAddress; }
        }
        private LookupTable lookup;
        private ProcessMemoryReader reader;
        bool isDead = false;
        public bool IsDead
        {
            get { return isDead; }
        }
        private UInt32 m_id;
        public UInt32 Id
        {
            get { return m_id; }
        }

        private UInt32 m_playerID;
        public UInt32 PlayerID
        {
            get { return m_playerID; }
        }

        private UInt32 m_typeid;
        public UInt32 TypeID
        {
            get { return m_typeid; }
        }
        public string TypeIDString
        {
            get { return lookup.LookupById[m_typeid]; }
        }

        private float m_LocationX;
        public float LocationX
        {
            get { return m_LocationX; }
        }

        private float m_LocationY;
        public float LocationY
        {
            get { return m_LocationY; }
        }

        private float m_targetLocationX;
        public float TargetLocationX
        {
            get { return m_targetLocationX; }
        }

        private float m_targetLocationY;
        public float TargetLocationY
        {
            get { return m_targetLocationY; }
        }

        /// <summary>
        /// Constructor for the Star craft Unit object
        /// </summary>
        /// <param name="BaseAddress">The starting location for this unit</param>
        /// <param name="LookUp">The unit lookup object from the main class</param>
        /// <param name="Read">The memory reader object from the main class</param>
        public Unit(UInt32 BaseAddress, LookupTable LookUp, ProcessMemoryReader Read)
        {
            m_baseAddress = BaseAddress;
            lookup = LookUp;
            reader = Read;
            Update(); // call update to fill in our known info
        }

        /// <summary>
        /// Update the values for this unit 
        /// </summary>
        public bool Update()
        {
            try
            {
                // Get this object's ID
                m_id = (UInt32)reader.ReadInteger(m_baseAddress) >> 0x12;

                //Fill in the player ID
                m_playerID = (UInt32)reader.ReadByte(m_baseAddress + (UInt32)Offsets.PlayerID);

                // Get the unit type
                UInt32 modelptr = (UInt32)reader.ReadInteger(m_baseAddress + (UInt32)Offsets.ModelPtr);
                modelptr = modelptr << 5;
                m_typeid = (UInt32)reader.ReadInteger(modelptr + (UInt32)Offsets.UnitIDModelPtr);
                m_typeid = m_typeid;// +1;

                //Get position
                int posx = reader.ReadInteger(m_baseAddress + (UInt32)Offsets.PositionX);
                int posy = reader.ReadInteger(m_baseAddress + (UInt32)Offsets.PositionY);

                m_LocationX = (float)(posx) / 10000.0f;
                m_LocationY = (float)(posy) / 10000.0f;

                //Get target position
                posx = reader.ReadInteger(m_baseAddress + (UInt32)Offsets.TargetPositionX);
                posy = reader.ReadInteger(m_baseAddress + (UInt32)Offsets.TargetPositionY);

                m_targetLocationX = (float)(posx) / 10000.0f;
                m_targetLocationY = (float)(posy) / 10000.0f;

                byte[] healthArr = new byte[4];
                //check health
                healthArr[2] = 0;
                healthArr[3] = 0;
                healthArr[0] = reader.ReadByte(m_baseAddress + (UInt32)Offsets.HealthLast);
                healthArr[1] = reader.ReadByte(m_baseAddress + (UInt32)Offsets.HealthFirst);
                //healthArr[0] = (byte)(healthArr[0] >> 4);

                int damage = BitConverter.ToInt32(healthArr, 0) >> 4;
                if (damage >= lookup.MaxDamageLookup[lookup.LookupById[m_typeid]])
                {
                    isDead = true;
                }
                else
                    isDead = false;


                return true;
            }
            catch (System.Exception ex)
            {
                Exception SHUTTHEFUCKUP = ex; // warnings about unused ex suck
                isDead = true;
                return false;
            }
            

        }
    }
}
