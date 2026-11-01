namespace UI
{
    /// <summary>
    /// Эксклюзивное окно (инвентарь, квесты, записки, настройки). Окно не знает про PlayerMode, курсор и Esc:
    /// всё это решает UIManager. Окно не должно закрывать само себя в обход UIManager.Close.
    /// </summary>
    public interface IUIScreen
    {
        /// <summary>
        /// Идентификатор, под которым окно регистрируется в UIManager.
        /// </summary>
        UIScreenId Id { get; }

        /// <summary>
        /// Показывает окно. Не меняет PlayerMode.
        /// </summary>
        void Open();

        /// <summary>
        /// Скрывает окно. Должен быть идемпотентным: UIManager вызывает его и при сбросе состояния.
        /// </summary>
        void Close();
    }
}
