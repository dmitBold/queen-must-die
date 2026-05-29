namespace DI
{
    public interface IPlayerProvider
    {
        Zenject.SpaceFighter.Player CurrentPlayer { get; set; }
    }

    public class PlayerProvider : IPlayerProvider
    {
        public Zenject.SpaceFighter.Player CurrentPlayer { get; set; }
    }
}
