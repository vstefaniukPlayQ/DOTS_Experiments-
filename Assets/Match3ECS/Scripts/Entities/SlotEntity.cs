using Unity.Entities;

public struct SlotEntity : IComponentData
{
        /// <summary>
        /// Chip that stored by this slot
        /// </summary>
        public Entity m_Chip; 
}
