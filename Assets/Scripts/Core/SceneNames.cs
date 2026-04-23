namespace Core
{
    public enum GameScene
    {
        Night,
        Day,
        HoM,
        Menu,
        Assembly,
        IntroDialog
    }

    public static class SceneNames
    {
        public const string Assembly = "AssemblyScene";
        public const string Night = "NightScene";
        public const string Day = "DayCycle";
        public const string HoM = "HoMScene";
        public const string Menu = "MenuScene";
        public const string IntroDialog = "IntroDialog";

        public static string GetName(GameScene scene)
        {
            switch (scene)
            {
                case GameScene.Night: return Night;
                case GameScene.Day: return Day;
                case GameScene.HoM: return HoM;
                case GameScene.Menu: return Menu;
                case GameScene.Assembly: return Assembly;
                case GameScene.IntroDialog: return IntroDialog;
                default: return Night;
            }
        }
    }
}
