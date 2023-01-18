namespace ImageFakeScraperExample.Bloom;
#pragma warning disable
public class Bloom<T>
{
    private readonly int _hashFunctionCount;
    private readonly BitArray _hashBits;
    private readonly HashFunction _getHashSecondary;

    public Bloom(int capacity)
        : this(capacity, null)
    {
    }

    public Bloom(int capacity, float errorRate)
        : this(capacity, errorRate, null)
    {
    }

    public Bloom(int capacity, HashFunction hashFunction)
        : this(capacity, BestErrorRate(capacity), hashFunction)
    {
    }

    public Bloom(int capacity, float errorRate, HashFunction hashFunction)
        : this(capacity, errorRate, hashFunction, BestM(capacity, errorRate), BestK(capacity, errorRate))
    {
    }

    public Bloom(int capacity, float errorRate, HashFunction hashFunction, int m, int k)
    {
        // validate the params are in range
        if (capacity < 1)
        {
            throw new ArgumentOutOfRangeException("capacity", capacity, "capacity must be > 0");
        }

        if (errorRate is >= 1 or <= 0)
        {
            throw new ArgumentOutOfRangeException("errorRate", errorRate, string.Format("errorRate must be between 0 and 1, exclusive. Was {0}", errorRate));
        }

        // from overflow in bestM calculation
        if (m < 1)
        {
            throw new ArgumentOutOfRangeException(string.Format("The provided capacity and errorRate values would result in an array of length > int.MaxValue. Please reduce either of these values. Capacity: {0}, Error rate: {1}", capacity, errorRate));
        }

        // set the secondary hash function
        if (hashFunction == null)
        {
            if (typeof(T) == typeof(string))
            {
                _getHashSecondary = HashString;
            }
            else
            {
                _getHashSecondary = typeof(T) == typeof(int)
                    ? HashInt32
                    : throw new ArgumentNullException("hashFunction", "Please provide a hash function for your type T, when T is not a string or int.");
            }
        }
        else
        {
            _getHashSecondary = hashFunction;
        }

        _hashFunctionCount = k;
        _hashBits = new BitArray(m);
    }

    public delegate int HashFunction(T input);

    public double Truthiness => (double)TrueBits() / _hashBits.Count;

    public void Add(T item)
    {
        int primaryHash = item.GetHashCode();
        int secondaryHash = _getHashSecondary(item);
        for (int i = 0; i < _hashFunctionCount; i++)
        {
            int hash = ComputeHash(primaryHash, secondaryHash, i);
            _hashBits[hash] = true;
        }
    }

    public bool Contains(T item)
    {
        int primaryHash = item.GetHashCode();
        int secondaryHash = _getHashSecondary(item);
        for (int i = 0; i < _hashFunctionCount; i++)
        {
            int hash = ComputeHash(primaryHash, secondaryHash, i);
            if (_hashBits[hash] == false)
            {
                return false;
            }
        }

        return true;
    }

    private static int BestK(int capacity, float errorRate)
    {
        return (int)Math.Round(Math.Log(2.0) * BestM(capacity, errorRate) / capacity);
    }

    private static int BestM(int capacity, float errorRate)
    {
        return (int)Math.Ceiling(capacity * Math.Log(errorRate, 1.0 / Math.Pow(2, Math.Log(2.0))));
    }

    private static float BestErrorRate(int capacity)
    {
        float c = (float)(1.0 / capacity);
        return c != 0 ? c : (float)Math.Pow(0.6185, int.MaxValue / capacity);
    }

    private static int HashInt32(T input)
    {
        uint? x = input as uint?;
        unchecked
        {
            x = ~x + (x << 15);
            x ^= (x >> 12);
            x += (x << 2);
            x ^= (x >> 4);
            x *= 2057;
            x ^= (x >> 16);
            return (int)x;
        }
    }

    private static int HashString(T input)
    {
        string? s = input as string;
        int hash = 0;

        for (int i = 0; i < s.Length; i++)
        {
            hash += s[i];
            hash += hash << 10;
            hash ^= hash >> 6;
        }

        hash += hash << 3;
        hash ^= hash >> 11;
        hash += hash << 15;
        return hash;
    }

    private int TrueBits()
    {
        int output = 0;
        foreach (bool bit in _hashBits)
        {
            if (bit == true)
            {
                output++;
            }
        }

        return output;
    }

    private int ComputeHash(int primaryHash, int secondaryHash, int i)
    {
        int resultingHash = (primaryHash + (i * secondaryHash)) % _hashBits.Count;
        return Math.Abs(resultingHash);
    }
}