namespace WwXMapEditor.Models
{
    public class Unit
    {
        public int X { get; set; }
        public int Y { get; set; }
        public UnitType Type { get; set; }
        public string Owner { get; set; }
        public int HP { get; set; } = 100;
        public int Fuel { get; set; }
        public int Ammo { get; set; }
        public MovementType MovementType { get; set; }
        public int MovementRange { get; set; }
        public int VisionRange { get; set; }
        public bool HasMoved { get; set; }
        public bool HasAttacked { get; set; }

        public Unit()
        {
            SetDefaultValues();
        }

        private void SetDefaultValues()
        {
            switch (Type)
            {
                case UnitType.Infantry:
                    MovementType = MovementType.Infantry;
                    MovementRange = 3;
                    VisionRange = 2;
                    Fuel = 50;
                    Ammo = 10;
                    break;
                case UnitType.Mechanized:
                    MovementType = MovementType.Infantry;
                    MovementRange = 4;
                    VisionRange = 3;
                    Fuel = 70;
                    Ammo = 10;
                    break;
                case UnitType.Tank:
                    MovementType = MovementType.Treaded;
                    MovementRange = 5;
                    VisionRange = 3;
                    Fuel = 60;
                    Ammo = 6;
                    break;
                case UnitType.HeavyTank:
                    MovementType = MovementType.Treaded;
                    MovementRange = 4;
                    VisionRange = 3;
                    Fuel = 50;
                    Ammo = 6;
                    break;
                case UnitType.Artillery:
                    MovementType = MovementType.Wheeled;
                    MovementRange = 4;
                    VisionRange = 3;
                    Fuel = 40;
                    Ammo = 6;
                    break;
                case UnitType.RocketLauncher:
                    MovementType = MovementType.Wheeled;
                    MovementRange = 4;
                    VisionRange = 3;
                    Fuel = 40;
                    Ammo = 6;
                    break;
                case UnitType.AntiAir:
                    MovementType = MovementType.Wheeled;
                    MovementRange = 5;
                    VisionRange = 4;
                    Fuel = 50;
                    Ammo = 9;
                    break;
                case UnitType.TransportVehicle:
                    MovementType = MovementType.Wheeled;
                    MovementRange = 6;
                    VisionRange = 2;
                    Fuel = 60;
                    Ammo = 0;
                    break;
                case UnitType.SupplyTruck:
                    MovementType = MovementType.Wheeled;
                    MovementRange = 6;
                    VisionRange = 2;
                    Fuel = 60;
                    Ammo = 0;
                    break;
                case UnitType.Helicopter:
                    MovementType = MovementType.Air;
                    MovementRange = 6;
                    VisionRange = 4;
                    Fuel = 80;
                    Ammo = 6;
                    break;
                case UnitType.Fighter:
                    MovementType = MovementType.Air;
                    MovementRange = 9;
                    VisionRange = 5;
                    Fuel = 100;
                    Ammo = 6;
                    break;
                case UnitType.Bomber:
                    MovementType = MovementType.Air;
                    MovementRange = 8;
                    VisionRange = 4;
                    Fuel = 100;
                    Ammo = 6;
                    break;
                case UnitType.Stealth:
                    MovementType = MovementType.Air;
                    MovementRange = 8;
                    VisionRange = 4;
                    Fuel = 120;
                    Ammo = 6;
                    break;
                case UnitType.TransportHelicopter:
                    MovementType = MovementType.Air;
                    MovementRange = 7;
                    VisionRange = 3;
                    Fuel = 90;
                    Ammo = 0;
                    break;
                case UnitType.Battleship:
                    MovementType = MovementType.Ship;
                    MovementRange = 5;
                    VisionRange = 4;
                    Fuel = 80;
                    Ammo = 6;
                    break;
                case UnitType.Cruiser:
                    MovementType = MovementType.Ship;
                    MovementRange = 6;
                    VisionRange = 5;
                    Fuel = 80;
                    Ammo = 9;
                    break;
                case UnitType.Submarine:
                    MovementType = MovementType.Ship;
                    MovementRange = 5;
                    VisionRange = 3;
                    Fuel = 80;
                    Ammo = 6;
                    break;
                case UnitType.NavalTransport:
                    MovementType = MovementType.Ship;
                    MovementRange = 7;
                    VisionRange = 2;
                    Fuel = 90;
                    Ammo = 0;
                    break;
                case UnitType.Carrier:
                    MovementType = MovementType.Ship;
                    MovementRange = 5;
                    VisionRange = 4;
                    Fuel = 100;
                    Ammo = 0;
                    break;
                case UnitType.Lander:
                    MovementType = MovementType.Lander;
                    MovementRange = 7;
                    VisionRange = 2;
                    Fuel = 80;
                    Ammo = 0;
                    break;
            }
        }
    }

    public enum UnitType
    {
        Infantry,
        Mechanized,
        Tank,
        HeavyTank,
        Artillery,
        RocketLauncher,
        AntiAir,
        TransportVehicle,
        SupplyTruck,
        Helicopter,
        Fighter,
        Bomber,
        Stealth,
        TransportHelicopter,
        Battleship,
        Cruiser,
        Submarine,
        NavalTransport,
        Carrier,
        Lander
    }

    public enum MovementType
    {
        Infantry,
        Wheeled,
        Treaded,
        Air,
        Ship,
        Lander
    }
}