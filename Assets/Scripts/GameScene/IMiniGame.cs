// IMiniGame.cs
public interface IMiniGame
{
    // Inicializa el minijuego. baseUI puede usarse para efectos, colores, Next, ShowError...
    void Initialize(MiniGameData data, MiniGameBaseClass baseUI);

    // Opcional: limpiar listeners/manual teardown si hace falta
    void TearDown();
}
