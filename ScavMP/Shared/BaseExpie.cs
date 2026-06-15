using LiteEntitySystem;
using LiteEntitySystem.Extensions;
using ScavMP.Client;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ScavMP.Shared
{
    public class BaseExpie : PawnLogic
    {
        public Body Owner { get; private set; }
        public Vector2 NetworkedMoveDir { get; private set; }

        [SyncVarFlags(SyncFlags.Interpolated | SyncFlags.LagCompensated | SyncFlags.SyncGroup1)]
        private SyncVar<Vector2> _position;
        public readonly SyncString Name = new();

        public Vector2 Position => _position.InterpolatedValue;

        PlayerInputPacket _currentInput = new();

        public BaseExpie(EntityParams entityParams)
            : base(entityParams) { }

        public override void RegisterRPC(ref RPCRegistrator r)
        {
            base.RegisterRPC(ref r);
        }

        public void Attach(Body body)
        {
            Owner = body;
        }

        public override void OnDestroy()
        {
            GameObject.Destroy(Owner.gameObject);
        }

        public void SetInput(in PlayerInputPacket input)
        {
            _currentInput = input;
        }

        public override void Update()
        {
            base.Update();

            if (!IsServer && !IsLocalControlled)
                return;

            // Modify pos
            NetworkedMoveDir = new Vector2(_currentInput.MoveX, _currentInput.MoveY);
            if (IsServer)
                _position.Value = new Vector2(
                    Owner.transform.position.x,
                    Owner.transform.position.y
                );
        }

        public override void VisualUpdate()
        {
            Owner.transform.position = _position.InterpolatedValue;
        }

        public void Spawn()
        {
            // ... init other components
        }

        public void Spawn(Vector2 position)
        {
            _position.Value = position;
            Spawn();
        }
    }
}
