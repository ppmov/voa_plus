using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;

public class OnlineTurnSwitcher : TurnSwitcher
{
    public override bool IsMyTurnNow => Player == (PhotonNetwork.IsMasterClient ? Library.Player.Green : Library.Player.Red);

    private PhotonView photonView;

    private void Awake()
    {
        photonView = GetComponent<PhotonView>();
    }

    public override void SelectDot(Dot dot)
    {
        // call rpc only if it really needed
        if (IsMoving || dot == null)
            return;

        // other player shouldn't know about first selections
        if (Turn == Library.Turn.Start)
        {
            base.SelectDot(dot);
            return;
        }

        if (dot == Selected)
        {
            CancelSelect();
            return;
        }

        // find selected dots indexes and call rpc
        for (int i = 0; i < Dots.Length; i++)
            if (dot == Dots[i])
            {
                for (int j = 0; j < Dots.Length; j++)
                    if (Selected == Dots[j])
                    {
                        photonView.RPC(nameof(SelectDotInTwoStep), RpcTarget.Others, j, i);
                        break;
                    }

                base.SelectDot(dot);
                return;
            }
    }

    [PunRPC]
    private void SelectDotInTwoStep(int start, int attack)
    {
        base.SelectDot(Dots[start]);
        base.SelectDot(Dots[attack]);
    }
}
