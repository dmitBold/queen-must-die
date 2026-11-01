namespace NightCycle
{
    public enum PlayerMode
    {
        FreeMovement,
        DoorState,
        Focused,
        // Добавлен в конец, чтобы не сдвинуть числовые значения, сериализованные в сценах и префабах.
        Frozen
    }
}