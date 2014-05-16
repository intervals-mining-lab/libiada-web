using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace LibiadaWeb.Models.Repositories
{ 
    public class BinaryCharacteristicRepository : IBinaryCharacteristicRepository
    {
        private readonly LibiadaWebEntities db;

        public BinaryCharacteristicRepository(LibiadaWebEntities db)
        {
            this.db = db;
        }

        public Int64 CreateBinaryCharacteristic(Int64 chainId, int characteristicId, int linkId, Int64 firstElementId, Int64 secondElementId, double value)
        {
            binary_characteristic characteristic = new binary_characteristic
            {
                chain_id = chainId,
                characteristic_type_id = characteristicId,
                link_id = linkId,
                first_element_id = firstElementId,
                second_element_id = secondElementId,
                created = DateTime.Now,
                value = value,
                value_string = value.ToString()
            };
            db.binary_characteristic.Add(characteristic);
            db.SaveChanges();
            return characteristic.id;
        }

        public void Dispose() 
        {
            db.Dispose();
        }
    }
}