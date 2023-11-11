using System.Runtime.Serialization;

namespace Common.Model
{
    [DataContract]
    public class ExampleModel
    {
        [DataMember]
        public string? FirstName { get; set; }

        [DataMember]
        public string? LastName { get; set; }

        [DataMember]
        public string? CardNumber { get; set; }

        [DataMember]
        public string? BookName { get; set; }

        [DataMember]
        public int Quantity { get; set; }
    }
}
