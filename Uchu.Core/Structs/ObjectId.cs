using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Uchu.Core
{
    [SuppressMessage("ReSharper", "CA2225")]
    public struct ObjectId : IEquatable<ObjectId>
    {
        private static readonly Random Random = new Random();

        private ulong Value { get; }

        public uint Identifier => (uint) (Value & uint.MaxValue);

        public ObjectIdFlags Flags => (ObjectIdFlags) Value;

        public ObjectId(ulong value)
        {
            Value = value;
        }

        #region Networking

        [SuppressMessage("ReSharper", "CA2000")]
        public async Task<InventoryItem> FindItemAsync()
        {
            await using var ctx = new UchuContext();

            var id = (long) Value;

            return await ctx.InventoryItems.FirstOrDefaultAsync(
                i => i.Id == id
            ).ConfigureAwait(false);
        }

        public InventoryItem FindItem()
        {
            using var ctx = new UchuContext();

            var id = (long) Value;

            return ctx.InventoryItems.FirstOrDefault(
                i => i.Id == id
            );
        }

        #endregion

        #region Standard

        public override string ToString()
        {
            return Value.ToString(CultureInfo.InvariantCulture);
        }

        public bool Equals(ObjectId other)
        {
            return other.Value == Value;
        }
        
        public override bool Equals(object obj)
        {
            return Value.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        #endregion

        #region Static
        
        public static bool operator ==(ObjectId left, ObjectId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ObjectId left, ObjectId right)
        {
            return !(left == right);
        }
        
        public static implicit operator ulong(ObjectId objectId)
        {
            return objectId.Value;
        }

        public static implicit operator ObjectId(ulong id)
        {
            return new ObjectId(id);
        }
        
        public static implicit operator long(ObjectId objectId)
        {
            return (long) objectId.Value;
        }
        
        public static implicit operator ObjectId(long id)
        {
            return new ObjectId((ulong) id);
        }

        public static ObjectId FromFlags(ObjectIdFlags flags)
        {
            var id = (long) Random.Next(1000000000, 2000000000);
            
            id |= (long) flags;

            return id;
        }

        public static ObjectId Standalone => RandomLong(1000000000000000000, 1999999999999999999);

        public static ObjectId Invalid => 0L;
        
        private static long RandomLong(long min, long max)
        {
            var buf = new byte[8];

            Random.NextBytes(buf);

            var res = BitConverter.ToInt64(buf, 0);

            return Math.Abs(res % (max - min)) + min;
        }

        #endregion
    }
}