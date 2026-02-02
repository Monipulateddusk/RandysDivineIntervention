using System.Diagnostics;
using TurnBased;
using Unity.VisualScripting;
using UnityEngine;

public abstract class Phase
{
    public BattleMediator ConcreteMediator;

    public Phase(BattleMediator concreteMediator)
    {
        this.ConcreteMediator = concreteMediator;
    }
    public abstract void OnEnter();
    public abstract void Update();
    public abstract void OnExit();
}

public class BeginRoundPhase : Phase
{
    public BeginRoundPhase(BattleMediator concreteMediator) : base(concreteMediator)
    {
    }

    public override void OnEnter()
    {
        /*  Check if the Turn-Order List is empty. If not, we don't want to be here.    */
        if (ConcreteMediator.GetTurnOrderList().Count > 0)
        {
            ConcreteMediator.ChangeToNextStateInOrder();
            return;
        }

        /*  When we enter this phase, we want to create the Turn Order List awaiting any Tasks that need to be done from external classes.  */
        ConcreteMediator.CreateTurnOrderList();

        /*  After that, get the Intention of all Enemy Units to reveal that information to the Player.  */

        /*  Once everything is done, we want to move onto the next Phase.   */
        ConcreteMediator.ChangeToNextStateInOrder();        
    }

    public override void OnExit()
    {

    }

    public override void Update()
    {

    }
}

public class PreTurnPhase : Phase
{
    public PreTurnPhase(BattleMediator concreteMediator) : base(concreteMediator)
    {
    }

    public override void OnEnter()
    {
        /*  Pop out the next Unit in turn order, move on to the Unit Turn Phase after this. */
        UnitSlot? unit = ConcreteMediator.PopNextUnitInTurnOrder();
        if(unit != null)
        {
            ConcreteMediator.ChangeToNextStateInOrder();
        }
        /*  If there is no Unit available in the Turn order, we are at the end of the Turn order and then we want to start the Round anew.  */
        else
        {
            ConcreteMediator.ChangeState(BattleMediator.PHASE_TYPES.END_ROUND);
        }        
    }

    public override void OnExit()
    {
        
    }

    public override void Update()
    {
        
    }
}

public class UnitTurnPhase : Phase
{
    UnitSlot? currentUnit;
    private float _enemyTurnTimer;

    public UnitTurnPhase(BattleMediator concreteMediator) : base(concreteMediator)
    {
    }

    public override void OnEnter()
    {
        /*  When we enter this Phase, we want to process any Start-Of-Turn Status Effects.  */
        currentUnit = ConcreteMediator.GetCurrentUnit();
        _enemyTurnTimer = 2.0f;

        MonoBehaviour.print("<color=black>Processing Turn of Unit Named: </color>" + currentUnit?.Unit.gameObject.name);

    }

    public override void OnExit()
    {
        /*  When we exit this Phase, we want to process any End-Of-Turn Status Effects.  */
    }

    public override void Update()
    {
        /*  Check if the Unit has an intention already planned. If so, execute it.  */

        /*  Otherwise, we want to create the intention by polling the Unit's Move Selection Component. */

        /*  Once a move is selected, get the Targetting data from that move and proceed to targetting Units for that move. As we are doing this in Update, it makes it easy for us to use a State-Machine to go backwards a step of this Phase. */

        /*  Once we have the move and Targetting data, proceed to processing the move. */

        /*  Once the Move is finished playing, check if the move ends the turn or not. If so, we want to tell the Mediator. If not, we want to just start again from Move selection. */

        if (currentUnit?.Team == UnitTeam.ALLY)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ConcreteMediator.ChangeToNextStateInOrder();
            }

        }
        else
        {
            _enemyTurnTimer -= Time.deltaTime;
            if (_enemyTurnTimer <= 0)
            {
                ConcreteMediator.ChangeToNextStateInOrder();
            }
        }
    }
}

public class EndRoundPhase : Phase
{
    public EndRoundPhase(BattleMediator concreteMediator) : base(concreteMediator)
    {
    }

    public override void OnEnter()
    {
        /*  Check if the Turn-Order List is empty. If not, we don't want to be here.    */
        if (ConcreteMediator.GetTurnOrderList().Count > 0)
        {
            ConcreteMediator.ChangeToNextStateInOrder();
            return;
        }
        /*  If we are supposed to be here. Process any end of round effects. Start the Round anew. */
        else
        {
            ConcreteMediator.ChangeState(BattleMediator.PHASE_TYPES.START_ROUND);
        }

    }

    public override void OnExit()
    {

    }

    public override void Update()
    {

    }
}

