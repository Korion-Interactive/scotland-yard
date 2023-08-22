using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;

[CustomEditor(typeof(PlayerBase), true)]
public class PlayerBaseEditor : Editor
{

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        PlayerBase obj = target as PlayerBase;

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Tickets Taxi", obj.PlayerState.Tickets.TaxiTickets.TicketsLeft.ToString());
        EditorGUILayout.LabelField("Tickets Bus", obj.PlayerState.Tickets.BusTickets.TicketsLeft.ToString());
        EditorGUILayout.LabelField("Tickets Metro", obj.PlayerState.Tickets.MetroTickets.TicketsLeft.ToString());

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Tickets Black", obj.PlayerState.Tickets.BlackTickets.TicketsLeft.ToString());
        EditorGUILayout.LabelField("Double Tickets", obj.PlayerState.Tickets.DoubleTickets.ToString());

        
    }

}
