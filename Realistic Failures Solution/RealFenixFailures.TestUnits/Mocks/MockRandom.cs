namespace RealFenixFailures.TestUnits.Mocks;

public class MockRandom : Random {
    private readonly Queue<int> _nextInts = new();
    private readonly Queue<double> _nextDoubles = new();

    public void SetupNextInt(int value) {
        _nextInts.Enqueue(value);
    }
    public override int Next(int minValue, int maxValue) {
        if (_nextInts.TryDequeue(out var result))
            return result;
        return base.Next(minValue, maxValue); // fallback
    }

    public void SetupNextDouble(double value) {
        _nextDoubles.Enqueue(value);
    }

    public override double NextDouble() {
        if (_nextDoubles.TryDequeue(out var result))
            return result;
        return base.NextDouble(); // fallback
    }
}