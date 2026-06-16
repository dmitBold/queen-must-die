namespace Core
{
    public enum GameScene
    {
        Night,
        Day,
        Memories1,
        HoM,
        MainMenu,
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
        public const string Memories1 = "Memories1";
        public const string HoM = "HoMScene";
        public const string MainMenu = "MainMenu";
        public const string IntroDialog = "IntroDialog";
        public const string IntroDialogue2 = "IntroDialogue2";
        public const string Throne = "Throne";

        public static string GetName(GameScene scene)
        {
            switch (scene)
            {
                case GameScene.Night: return Night;
                case GameScene.Day: return Day;
                case GameScene.Memories1: return Memories1;
                case GameScene.HoM: return HoM;
                case GameScene.MainMenu: return MainMenu;
                case GameScene.Assembly: return Assembly;
                case GameScene.IntroDialog: return IntroDialog;
                case GameScene.IntroDialogue2: return IntroDialogue2;
                case GameScene.Throne: return Throne;
                default: return Night;
            }
        }
    }
}
