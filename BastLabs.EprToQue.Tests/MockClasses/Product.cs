using System.Collections.Generic;

namespace BastLabs.EprToQue.Tests.MockClasses
{
    public class Product
    {
        public int ID { get; set; }
        public int? IDNullable { get; set; }
        public string Name { get; set; }
        public string String { get; set; }
        public decimal Decimal { get; set; }
        public decimal DecimalNullable { get; set; }

        public IEnumerable<int> IntList { get; set; } = new List<int>();
        public IEnumerable<string> StringList { get; set; } = new List<string>();
    }
}
