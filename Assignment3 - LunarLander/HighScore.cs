using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace CS5410
{
    /// <summary>
    /// This class demonstrates how to create an object that can be serialized
    /// under the XNA framework.
    /// </summary>
    //[Serializable]
    [DataContract(Name = "HighScore")]
    public class HighScore
    {
        /// <summary>
        /// Have to have a default constructor for the XmlSerializer.Deserialize method
        /// </summary>
        public HighScore() {
        }

        /// <summary>
        /// Overloaded constructor used to create an object for long term storage
        /// </summary>
        /// <param name="score"></param>
        /// <param name="level"></param>
        public HighScore(uint score)
        {
            this.Score = score;
            this.TimeStamp = DateTime.Now;

            keys.Add(1, "one");
            keys.Add(2, "two");
        }

        [DataMember()]

        public uint Score { get; set; }
        [DataMember()]
        public DateTime TimeStamp { get; set; }
        [DataMember()]
        public Dictionary<int, String> keys = new Dictionary<int, string>();
    }
}
