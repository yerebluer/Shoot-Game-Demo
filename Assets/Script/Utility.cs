using System.Collections;
using System.Collections.Generic;

public static class Utility
{
    public static T[] ShuffleArray<T>(T[] array,int seed)
    {
        System.Random prng = new System.Random(seed);
        int len = array.Length;
        for (int i = 0; i < len; i++)
        {
            int ranidx = prng.Next(i, len);
            T item = array[ranidx];
            array[ranidx] = array[i];
            array[i] = item;
        }
        return array;
    }
}
