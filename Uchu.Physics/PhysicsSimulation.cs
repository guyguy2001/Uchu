using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuUtilities.Memory;
using Uchu.Physics.Callbacks;

namespace Uchu.Physics
{
    public class PhysicsSimulation : IDisposable
    {
        internal Simulation Simulation { get; }
        
        internal BufferPool Buffer { get; }
        
        internal NarrowPhaseCallbacks NarrowPhaseCallbacks { get; }
        
        internal PoseIntegratorCallbacks PoseIntegratorCallbacks { get; }

        internal PhysicsQueue PhysicsQueue { get; }
        public PhysicsSimulation()
        {
            Buffer = new BufferPool();
            
            NarrowPhaseCallbacks = new NarrowPhaseCallbacks
            {
                OnCollision = HandleCollision
            };

            PoseIntegratorCallbacks = new PoseIntegratorCallbacks(Vector3.Zero);

            Simulation = Simulation.Create(Buffer, NarrowPhaseCallbacks, PoseIntegratorCallbacks);
        }

        /// <summary>
        /// Steps the physics
        /// </summary>
        /// <param name="deltaTime">Delta time in milliseconds since last tick</param>
        public void Step(float deltaTime)
        {
            /*
             * DON'T SLEEP ON THE JOB!
             */
            foreach (var physicsBody in PhysicsQueue.Objects.OfType<PhysicsBody>().ToArray())
            {
                if (!physicsBody.Reference.Exists)
                {
                    PhysicsQueue.RemoveObject(physicsBody);
                    
                    continue;
                }
                
                physicsBody.Reference.Activity.SleepCandidate = false;

                if (!physicsBody.Reference.Awake) Simulation.Awakener.AwakenBody(physicsBody.Handle);
            }

            Simulation.Timestep(deltaTime / 1000);
            PhysicsQueue.TakeStep();
        }

        internal void Register(PhysicsObject obj) => PhysicsQueue.AddObject(obj);
        internal void Release(PhysicsObject obj) => PhysicsQueue.RemoveObject(obj);
        
        private bool HandleCollision(CollidableReference referenceA, CollidableReference referenceB)
        {
            var a = FindObject(referenceA.StaticHandle, referenceA.BodyHandle);
            
            var b = FindObject(referenceB.StaticHandle, referenceB.BodyHandle);

            try
            {
                a.OnCollision?.Invoke(b);

                b.OnCollision?.Invoke(a);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return false;
        }

        private PhysicsObject FindObject(StaticHandle staticHandle, BodyHandle bodyHandle)
        {
            foreach (var physicsObject in PhysicsQueue.Objects.OfType<PhysicsBody>())
            {
                if (!physicsObject.Reference.Exists) continue;
                
                if (physicsObject.Id == bodyHandle.Value)
                {
                    return physicsObject;
                }
            }
            
            foreach (var physicsObject in PhysicsQueue.Objects.OfType<PhysicsStatic>())
            {
                if (!physicsObject.Reference.Exists) continue;

                if (physicsObject.Id == staticHandle.Value)
                {
                    return physicsObject;
                }
            }

            throw new Exception($"Failed to find physics object: Got {staticHandle}/{bodyHandle}");
        }

        public void Dispose()
        {
            Simulation?.Dispose();
            Buffer?.Clear();
        }
    }
}