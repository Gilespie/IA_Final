using UnityEngine;

public class SolderPersuit : State<NPCState>
{
    Transform _target;
    Solder _solder; 
    public SolderPersuit(FSM<NPCState> fsm) : base(fsm)
    {

    }
}