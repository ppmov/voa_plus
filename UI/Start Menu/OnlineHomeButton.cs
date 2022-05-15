public class OnlineHomeButton : HomeButton
{
    protected override void GoHome()
    {
        Photon.Pun.PhotonNetwork.Disconnect();
    }
}
