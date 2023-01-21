namespace MovingAverage
{
	public class SimpleMovingAverageLong
	{
        private readonly int _windowSize;
        private readonly Queue<double> _window;
        private double _sum;

        public SimpleMovingAverageLong(int windowSize)
        {
            _windowSize = windowSize;
            _window = new Queue<double>(windowSize);
        }

        public double Update(double value)
        {
            _sum += value;
            _window.Enqueue(value);

            if (_window.Count > _windowSize)
            {
                _sum -= _window.Dequeue();
            }

            return _sum / _window.Count;
        }
    }
}
