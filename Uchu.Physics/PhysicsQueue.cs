using System;
using System.Collections.Generic;
using System.Linq;

namespace Uchu.Physics
{
    public class PhysicsQueue
    {
        public List<PhysicsObject> Objects { get; }

        private Dictionary<PhysicsObject, bool> ObjectsQueue { get; } // PhysicsObject object, bool: ToAdd?
        
        private IEnumerable<PhysicsStatic> Statics
        {
            get
            {
                return Objects.OfType<PhysicsStatic>();
            }
        }

        private IEnumerable<PhysicsBody> Bodies
        {
            get
            {
                return Objects.OfType<PhysicsBody>();
            }
        }

        PhysicsQueue()
        {
            Objects = new List<PhysicsObject>();
            ObjectsQueue = new Dictionary<PhysicsObject, bool>();
        }

        public void TakeStep()
        {
            foreach (var item in ObjectsQueue)
            {
                if (item.Value) Objects.Add(item.Key);
                else Objects.Remove(item.Key);

                ObjectsQueue.Remove(item.Key);
            }
        }

        public void AddObject(PhysicsObject physicsObject) => ObjectsQueue.Add(physicsObject, true);
        public void RemoveObject(PhysicsObject physicsObject) => ObjectsQueue.Add(physicsObject, false);
    }
}