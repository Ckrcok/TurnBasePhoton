using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerController : MonoBehaviourPun
{
    public Player photonPlayer;                 // Photon.Realtime.Player class
    public string[] unitsToSpawn;
    public Transform[] spawnPoints;      // array of all spawn positions for this player

    public List<Unit> units = new List<Unit>(); // list of all our units
    private Unit selectedUnit;                  // currently selected unit

    public static PlayerController me;          // local player
    public static PlayerController enemy;       // non-local enemy player

    // called when the game begins
    [PunRPC]
    void Initialize (Player player)
    {
        photonPlayer = player;

        // if this is our local player, spawn the units
        if(player.IsLocal)
        {
            me = this;
            SpawnUnits();
        }
        else
            enemy = this;

        // set the player text
        GameUI.instance.SetPlayerText(this);
    }

        void Update ()
        {
          if(!photonView.IsMine)
              return;

          if(Input.GetMouseButton(0) && GameManager.instance.curPlayer == this)
          {
              Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
              TrySelect(new Vector3 (Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y), 0));
          }

        }

        void SpawnUnits ()
        {
          for(int x = 0; x < unitsToSpawn.Length; ++x)
          {
              GameObject unit = PhotonNetwork.Instantiate(unitsToSpawn[x], spawnPoints[x].position, Quaternion.identity);
              unit.GetPhotonView().RPC("Initialize", RpcTarget.Others, false);
              unit.GetPhotonView().RPC("Initialize", photonPlayer, true);
          }
        }

        public void BeginTurn ()
        {
            foreach(Unit unit in units)
                unit.usedThisTurn= false;

            // update the UI
            GameUI.instance.UpdateWaitingUnitsText(units.Count);
        }

        void TrySelect (Vector3 selectPos)
      {
        // are we selecting out unit?
        Unit unit = units.Find( x => x.transform.position == selectPos);

        if(unit != null)
        {
          SelectUnit(unit);
          return;
        }

        if(!selectedUnit)
            return;

        // are we selecting an enemy unit?
        Unit enemyUnit = enemy.units.Find( x => x.transform.position == selectPos);

        if(enemyUnit != null)
        {
          TryAttack(enemyUnit);
          return;
        }

        TryMove(selectPos);
      }

      void SelectUnit (Unit unitToSelect)
      {
        // can we select the unit
        if(!unitToSelect)
            return;

        // un-select the current unit
        if(selectedUnit != null)
            selectedUnit.ToggleSelect (false);


        // select the new unit
        selectedUnit = unitToSelect;
        selectedUnit.ToggleSelect(true);

        // set the unit info text
      }

      void DeSelectUnit ()
      {
          selectedUnit.ToggleSelect(false);
          selectedUnit = null;

          // disable the unit info text
          GameUI.instance.SetUnitInfoText(selectedUnit);
      }


      void SelectNextAvailableUnit ()
      {
        Unit availableUnit = units.Find (x => x.CanSelect());

        if(availableUnit != null)
            SelectUnit(availableUnit);
        else
            DeSelectUnit();
      }

      void TryAttack (Unit enemyUnit)
      {
        if(selectedUnit.CanAttack(enemyUnit.transform.position))
        {
          selectedUnit.Attack(enemyUnit);
          SelectNextAvailableUnit();

          //update the UI
          GameUI.instance.UpdateWaitingUnitsText(units.FindAll(x => x.CanSelect()).Count);
        }
      }

      void TryMove(Vector3 movePos)
      {
        if(selectedUnit.CanMove(movePos))
        {
          selectedUnit.Move(movePos);
          SelectNextAvailableUnit();

          //update UI
          GameUI.instance.UpdateWaitingUnitsText(units.FindAll(x => x.CanSelect()).Count);
        }
      }

      public void EndTurn()
      {
          // de -select the unit
          if(selectedUnit != null )
              DeSelectUnit();

          // start the next turn
          GameManager.instance.photonView.RPC("SetNextTurn", RpcTarget.All);
      }
}
