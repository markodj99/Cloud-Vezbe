using System.Runtime.Serialization;

namespace Common.Model
{
    [DataContract]
    public class BankAccount
    {
        [DataMember]
        public long AccountNumber { get; set; }
        [DataMember]
        public double AmountOfMoney { get; set; }

    }
}
