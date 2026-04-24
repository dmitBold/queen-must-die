namespace Core
{
    public enum GameScene
    {
        Night,
        Day,
        HoM,
        Menu,
        Assembly,
        IntroDialog,
        IntroDialogue2,
        Throne
    }

    public static class SceneNames
    {
        public const string Assembly = "AssemblyScene";
        public const string Night = "NightCycle";
        public const string Day = "DayCycle";
        public const string HoM = "HoMScene";
        public const string Menu = "MenuScene";
        public const string IntroDialog = "IntroDialog";
        public const string IntroDialogue2 = "IntroDialogue2";
        public const string Throne = "Throne";

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
                case GameScene.IntroDialogue2: return IntroDialogue2;
                case GameScene.Throne: return Throne;
                default: return Night;
            }
        }
    }
}
