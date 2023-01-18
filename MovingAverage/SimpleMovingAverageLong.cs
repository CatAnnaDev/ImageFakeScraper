namespace MovingAverage
{
	public class SimpleMovingAverageLong
	{
		private readonly long _k;
		private readonly long[] _values;

		private long _index = 0;
		private long _sum = 0;

		public SimpleMovingAverageLong(long k)
		{
			if (k <= 0)
			{
				throw new ArgumentOutOfRangeException(nameof(k), "Must be greater than 0");
			}

			_k = k;
			_values = new long[k];
		}

		public double Update(long nextInput)
		{
			// calculate the new sum
			_sum = _sum - _values[_index] + nextInput;

			// overwrite the old value with the new one
			_values[_index] = nextInput;

			// increment the index (wrapping back to 0)
			_index = (_index + 1) % _k;

			// calculate the average
			return ((double)_sum) / _k;
		}
	}
}
