using System;
using System.Collections.Generic;
using UnityEngine;

public enum DataKey
{
    NONE,
    ACTION_CHAT,
    ACTION_SET_ID,
    ACTION_TEAM_REQUEST,
    ACTION_START_GAME_BY_HOST,
    ACTION_CHESS_GAME_INFO,
    ACTION_ROOM_INFO,
    ACTION_PLAY_MOVE,
    ACTION_LEAVE_ROOM,
    ACTION_TEAM_INFO,
    ACTION_RESET_GAME,
    ACTION_END_GAME,
    ACTION_ADD_WINNER,

    GAME_MANAGER,
    PLAYER_NICKNAME,
    SERVER_IP,
    IS_HOST,
    IS_WHITE,
    IS_BLACK,
    IS_SPECTATOR,
    SERVER,
    CLIENT,
}

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/BlackBoard", order = 1)]

public class BlackBoard : ScriptableObject
{
    public TypeMapping TypeMapping = new TypeMapping();
    public Dictionary<DataKey, object> Data = new Dictionary<DataKey, object>();

    public void AddData<T>(DataKey key, T y)
    {
        TypeMapping.AddValueType(key, typeof(T));
        if (!Data.ContainsKey(key)) Data.Add(key, y);
    }

    public T GetValue<T>(DataKey key)
    {
        // V�rifier si le key existe dans Data
        if (Data.TryGetValue(key, out object value))
        {
            // V�rifier que le type attendu correspond � celui dans le typeMapping
            if (TypeMapping.typeMapping.TryGetValue(key, out Type expectedType))
            {
                // V�rifier que T correspond bien au type attendu
                if (typeof(T) == expectedType)
                {
                    // Tenter de caster en T
                    if (value is T castValue)
                    {
                        return castValue;
                    }
                    else
                    {
                        Debug.LogError($"La valeur pour la cl� '{key}' ne peut pas �tre cast�e en {typeof(T)}.");
                        return default;
                    }
                }
                else
                {
                    Debug.LogError($"Le type attendu pour la cl� '{key}' est {expectedType}, mais {typeof(T)} a �t� fourni.");
                    return default;
                }
            }
            else
            {
                Debug.LogError($"Type non trouv� pour la cl� '{key}' dans le mapping.");
                return default;
            }
        }
        else
        {
            return default;
        }
    }

    public void ClearData(DataKey key)
    {
        if (ContainsData(key))
        {
            Data.Remove(key);

        }
        if (TypeMapping.typeMapping.ContainsKey(key))
        {
            TypeMapping.typeMapping.Remove(key);
        }
    }

    public bool ContainsData(DataKey key)
    {
        return Data.ContainsKey(key);
    }

    public void SetData(DataKey x, object y)
    {
        Data[x] = y;
    }
}