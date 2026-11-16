namespace ConceptDemo.Player
{
    /// <summary>PlayerController 的輸入來源，每幀在 Update 讀取一次。</summary>
    public interface IPlayerInputSource
    {
        PlayerInputFrame Read();
    }
}
