using TuColmadoRD.Core.Domain.Enums.Inventory_Purchasing;

namespace TuColmadoRD.Core.Domain.Entities.Inventory
{
    public class UnitOfMeasureEntity
    {
        public UnitOfMeasure Id { get; private set; }
        public string Name { get; private set; }
        public string Abbreviation { get; private set; }
        public bool IsFractionable { get; private set; }

        public UnitOfMeasureEntity(UnitOfMeasure id, string name, string abbreviation, bool isFractionable)
        {
            Id = id;
            Name = name;
            Abbreviation = abbreviation;
            IsFractionable = isFractionable;
        }
    }
}
