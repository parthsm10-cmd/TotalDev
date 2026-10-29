
using MongoDB.Driver;
using TotalDev.Models;

namespace TotalDev.Services
{
    public class Order_HDRService
    {
        private readonly IMongoCollection<Order_HDR> _headers;
        private readonly IMongoCollection<Order_Details> _details;

        public Order_HDRService(IMongoDatabase database)
        {
            _headers = database.GetCollection<Order_HDR>("Order_HDR");
            _details = database.GetCollection<Order_Details>("Order_Details");
        }

        public async Task CreateOrderAsync(
            Order_HDR header,
            List<Order_Details> details)
        {
            // Generate order ID before inserting the header.
            var MaxNumber = await _headers
                .Find(Builders<Order_HDR>.Filter.Empty)
                .SortByDescending(o => o.Ord_No)
                .Limit(1)
                .FirstOrDefaultAsync();
            header.Ord_No = MaxNumber != null ? MaxNumber.Ord_No + 1 : 1;

            header.CreatedOn = DateTime.UtcNow;
            header.Modify_On = DateTime.UtcNow;

            // 1. Insert main order record.
            await _headers.InsertOneAsync(header);

            // 2. Link every item to the new order number.
            foreach (var item in details)
            {
                var MaxItemNumber = await _details
                    .Find(Builders<Order_Details>.Filter.Empty)
                    .SortByDescending(i => i.V_No)
                    .Limit(1)
                    .FirstOrDefaultAsync();
                item.V_No = MaxItemNumber != null ? MaxItemNumber.V_No + 1 : 1;
                item.V_Date = DateTime.UtcNow;
                item.Ref_Ord_No = header.Ord_No;
                item.Ref_Ord_Date = header.Ord_Date;
                item.CustomerId = header.CustomerId;
                item.CreatedBy = header.CreatedBy;
                item.CreatedOn = header.CreatedOn;
                item.Modify_On = header.Modify_On;
            }

            // 3. Insert all item-wise records.
            if (details.Count > 0)
            {
                await _details.InsertManyAsync(details);
            }
        }
        public async Task<List<Order_HDR>> GetAllAsync()
        {
            return await _headers
                .Find(Builders<Order_HDR>.Filter.Empty)
                .ToListAsync();
        }
    }
}
