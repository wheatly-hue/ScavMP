using LiteEntitySystem;

namespace ScavMP.Shared;

public class BaseWorld : SingletonEntityLogic
{
    WorldGeneration _worldGen;

    public void Init(WorldGeneration worldGen)
    {
        _worldGen = worldGen;
    }

    public BaseWorld(EntityParams entityParams)
        : base(entityParams) { }
}
