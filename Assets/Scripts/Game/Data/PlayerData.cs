using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public partial class PlayerData
{
    record LevelData(int Atk, int RequireExp);
    private List<LevelData> levelData = new List<LevelData>()
    {
        new LevelData(5, 0),
        new LevelData(7, 10),
        new LevelData(9, 30),
        new LevelData(11, 60),
        new LevelData(13, 100),
        new LevelData(16, 150),
        new LevelData(19, 230),
        new LevelData(22, 350),
        new LevelData(25, 500),
        new LevelData(29, 700),
        new LevelData(33, 950),
        new LevelData(37, 1200),
        new LevelData(41, 1500),
        new LevelData(46, 1800),
        new LevelData(51, 2300),
        new LevelData(56, 3000),
        new LevelData(61, 4000),
        new LevelData(65, 6000),
        new LevelData(71, 9000),
        new LevelData(74, 15000),
        new LevelData(77, 23000),
        new LevelData(80, 33000),
        new LevelData(83, 45000),
        new LevelData(86, 60000),
        new LevelData(89, 80000),
        new LevelData(90, 100000),
        new LevelData(91, 130000),
        new LevelData(92, 180000),
        new LevelData(93, 240000),
        new LevelData(94, 300000),
        new LevelData(95, 400000),
        new LevelData(96, 500000),
        new LevelData(97, 600000),
        new LevelData(98, 700000),
        new LevelData(99, 800000),
        new LevelData(100, 900000),
        new LevelData(100, 999999),
    };
}
