namespace DI
{
    public interface IPlayerProvider
    {
        Zenject.SpaceFighter.Player CurrentPlayer { get; set; }
        PlayerView PlayerView { get; set; }
    }

    public class PlayerProvider : IPlayerProvider
    {
        public Zenject.SpaceFighter.Player CurrentPlayer { get; set; }
        public PlayerView PlayerView { get; set; }
    }
}
