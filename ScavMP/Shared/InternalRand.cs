using System;

namespace ScavMP.Shared;

public class InternalRand
{
    private Random _worldRng = new(1337);
    private int seed = 1337;

    // I didn't use it :(
    public Random WorldRng => _worldRng;
    public int WorldSeed
    {
        get { return seed; }
        set
        {
            seed = value;
            _worldRng = new(value);
        }
    }

    public static InternalRand Instance;
}
