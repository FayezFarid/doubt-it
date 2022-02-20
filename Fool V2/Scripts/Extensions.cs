using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Extension
{
    public static int IncInt(int InitValue, int Size)
    {
        if (InitValue == Size - 1)
            InitValue = 0;
        else InitValue++;
        return InitValue;
    }
    public static int IncInt(int InitValue, List<OnlinePlayer> List)
    {
        if (InitValue == List.Count - 1)
            InitValue = 0;
        else InitValue++;
        return InitValue;

    }
    public static int DecInt(int InitValue,int size)
    {
         InitValue-- ;
        if (InitValue < 0)
            InitValue = size - 1;
        return InitValue;
    }
    public static string translateInt(int SelectedCardNumber)
    {
        string SelectedString;
        if (SelectedCardNumber == 0)
            SelectedString = "Ace";
        else if (SelectedCardNumber == 10)
            SelectedString = "Junior";
        else if (SelectedCardNumber == 11)
            SelectedString = "Queen";
        else if (SelectedCardNumber == 12)
            SelectedString = "King";
        else SelectedString = (SelectedCardNumber + 1).ToString();
        return SelectedString;
    }
 
}