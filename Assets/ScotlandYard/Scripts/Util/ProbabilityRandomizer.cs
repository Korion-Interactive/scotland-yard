using System;
using System.Collections.Generic;

//namespace Sunbow.Util.Data
//{
    /// <summary>
    /// The ProbabilityRandomizer stores objects with associated weight values and can return a random object. 
    /// The weight defines the probability with thich the specific object is chosen.
    /// </summary>
    public class ProbabilityRandomizer<T>
    {
        #region nested class Weigth
        public struct WeightValuePair
        {
            internal T Value;
            internal int Weight;

            internal WeightValuePair(int weight, T value)
            {
                Value = value;
                Weight = weight;
            }
        }
        #endregion

        #region Variables
        // -- VARIABLES --

        /// <summary>Gets or Sets the randomizer which is used to choose a value</summary>
        public System.Random Randomizer { get { return random; } set { random = value; } }

        private System.Random random = new System.Random();

        List<WeightValuePair> weights = new List<WeightValuePair>();
        int maxWeight = 0;
        public int MaxWeight { get { return maxWeight; } }
        public int Count { get { return weights.Count; } }

        #endregion // Variables


        #region Methods

        public void Clear()
        {
            weights.Clear();
            maxWeight = 0;
        }
        // -- METHODS --
        /// <summary>
        /// Adds a value with corresponding value
        /// </summary>
        /// <param name="weight">the weight which inicates how probable this value is chosen</param>
        /// <param name="value">the value which is returned by Next() when chosen</param>
        /// <param name="values">as many other values as you want with the same weight</param>
        public void Add(int weight, T value, params T[] values)
        {
            if (weight <= 0) 
                return;

            Add(weight, value);

            foreach (T t in values) 
                Add(weight, t);
        }
        /// <summary>
        /// Adds a value with corresponding value
        /// </summary>
        /// <param name="weight">the weight which indicates how probable this value is chosen</param>
        /// <param name="value">the value which is returned by Next() when chosen</param>
        public void Add(int weight, T value)
        {
            if (weight <= 0) 
                return;

            weights.Add(new WeightValuePair(weight, value));
            maxWeight += weight;
        }

        /// <summary>
        /// Tries to remove a weight-value-pair and returns true if it has successfully been removed (otherwise false)
        /// </summary>
        /// <param name="weight">the weight of the entry</param>
        /// <param name="value">the value of the entry</param>
        public bool TryRemove(int weight, T value)
        {
            for (int i = 0; i < weights.Count; i++)
            {
                if (weights[i].Weight == weight && weights[i].Value.Equals(value))
                {
                    maxWeight -= weights[i].Weight;
                    weights.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Removes all weight-value-pairs which contains the passed value
        /// </summary>
        public void Remove(T value)
        {
            for (int i = 0; i < weights.Count; i++)
            {
                if (weights[i].Value.Equals(value))
                {
                    maxWeight -= weights[i].Weight;
                    weights.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Returns one value of the added values.
        /// The more weight the value has, the more probable it is chosen here.
        /// </summary>
        public T Next()
        {
            if (maxWeight <= 0)
                throw new IndexOutOfRangeException("there is no value added to the probability randomizer");

            return GetNext();
        }

        /// <summary>
        /// Tries to get the next value. Returns true if successfull, otherwise false.
        /// </summary>
        /// <param name="outValue">the value which gets the result assigned (if successful)</param>
        public bool TryNext(ref T outValue)
        {
            if (maxWeight <= 0)
                return false;

            outValue = GetNext();
            return true;
        }

        private T GetNext()
        {
            return GetSpecific(random.Next(maxWeight));
        }

        public T GetSpecific(int index)
        {
            index++;
            int v = 0;
            int i = -1;

            while (v < index)
            {
                v += weights[++i].Weight;
            }
            return weights[i].Value;
        }

        public string Print()
        {
            string s = string.Empty;
            foreach (WeightValuePair pair in weights)
                s += string.Format("{0} - {1}\n", pair.Value, pair.Weight);

            return s;
        }

        #endregion // Methods
    }
//}