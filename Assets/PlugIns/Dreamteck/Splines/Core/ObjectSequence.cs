using System;
using UnityEngine;
using Random = System.Random;

namespace Dreamteck.Splines
{
    [Serializable]
    public class ObjectSequence<T>
    {
        public enum Iteration
        {
            Ordered,
            Random
        }

        public T         startObject;
        public T         endObject;
        public T[]       objects;
        public Iteration iteration = Iteration.Ordered;

        [SerializeField] [HideInInspector] private int _randomSeed = 1;

        [SerializeField] [HideInInspector] private int index;

        [SerializeField] [HideInInspector] private Random randomizer;

        public ObjectSequence()
        {
            randomizer = new Random(_randomSeed);
        }

        public int randomSeed
        {
            get => _randomSeed;
            set
            {
                if (value != _randomSeed)
                {
                    _randomSeed = value;
                    randomizer  = new Random(_randomSeed);
                }
            }
        }

        public T GetFirst()
        {
            if (startObject != null) return startObject;
            return Next();
        }

        public T GetLast()
        {
            if (endObject != null) return endObject;
            return Next();
        }

        public T Next()
        {
            if (iteration == Iteration.Ordered)
            {
                if (index >= objects.Length) index = 0;
                return objects[index++];
            }

            var randomIndex = randomizer.Next(objects.Length - 1);
            return objects[randomIndex];
        }
    }
}