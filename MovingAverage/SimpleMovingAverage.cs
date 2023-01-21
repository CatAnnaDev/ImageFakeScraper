#pragma warning disable
namespace MovingAverage
{
	//public class SimpleMovingAverage
	//{
    //    private readonly int _windowSize;
    //    private readonly Queue<double> _window;
    //    private double _sum;
    //
    //    public SimpleMovingAverage(int windowSize)
    //    {
    //        _windowSize = windowSize;
    //        _window = new Queue<double>(windowSize);
    //    }
    //
    //    public double Update(double value)
    //    {
    //        _sum += value;
    //        _window.Enqueue(value);
    //
    //        if (_window.Count > _windowSize)
    //        {
    //            _sum -= _window.Dequeue();
    //        }
    //
    //        return _sum / _window.Count;
    //    }
    //}

public class SimpleMovingAverage
    {
        private readonly int _windowSize;
        private readonly Queue<double> _window;
        private double _sum;

        public SimpleMovingAverage(int windowSize)
        {
            if (windowSize <= 0)
            {
                //throw new ArgumentException("Window size must be a positive integer", nameof(windowSize));
            }
            _windowSize = windowSize;
            _window = new Queue<double>(windowSize);
        }

        public void Add(double value)
        {
            _sum += value;
            _window.Enqueue(value);
            if (_window.Count > _windowSize)
            {
                _sum -= _window.Dequeue();
            }
        }

        public double Average()
        {
            if (_window.Count == 0)
            {
                //throw new InvalidOperationException("No values have been added");
            }
            return _sum / _window.Count;
        }
    }

}
