using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common.Model
{
    [DataContract]
    public class Book
    {
        [DataMember]
        public long Id { get; set; }
        [DataMember]
        public string Title { get; set; } = null!;
        [DataMember]
        public string Author { get; set; } = null!;
        [DataMember]
        public int Quantity { get; set; }
        [DataMember]
        public double Price { get; set; }
    }
}
