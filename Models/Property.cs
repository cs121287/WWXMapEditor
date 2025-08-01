namespace WwXMapEditor.Models
{
    public class Property
    {
        public int X { get; set; }
        public int Y { get; set; }
        public PropertyType Type { get; set; }
        public string Owner { get; set; } = "Neutral";
        public int VisionRange { get; set; }
        public int Income { get; set; }

        public Property()
        {
            // Initialize with default values based on Type
            // Since Type defaults to 0 (City), set City defaults
            Type = PropertyType.City;
            VisionRange = 2;
            Income = 1000;
        }

        public void SetDefaultValues()
        {
            switch (Type)
            {
                case PropertyType.City:
                    VisionRange = 2;
                    Income = 1000;
                    break;
                case PropertyType.Factory:
                    VisionRange = 3;
                    Income = 1500;
                    break;
                case PropertyType.HQ:
                    VisionRange = 4;
                    Income = 2000;
                    break;
                case PropertyType.Airport:
                    VisionRange = 3;
                    Income = 1500;
                    break;
                case PropertyType.Port:
                    VisionRange = 3;
                    Income = 1500;
                    break;
            }
        }
    }

    public enum PropertyType
    {
        City,
        Factory,
        HQ,
        Airport,
        Port
    }
}