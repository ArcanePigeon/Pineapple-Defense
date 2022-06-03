using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TowerRadius : MonoBehaviour
{
    private Tower tower;
    [SerializeField] public Transform radius;
    [SerializeField] public Transform pivot;
    [SerializeField] public Animator animator;
    [SerializeField] public Transform projectileSpawn;
    [SerializeField] public TMP_Text levelIndicator;
    [SerializeField] public GameObject towerLevel;
    [SerializeField] public GameObject towerRadiusDisplay;
    [SerializeField] public Slider upgradeProgressSlider;
    public void Init(Tower tower)
    {
        this.tower = tower;
    }
    public void AddEnemyToList(Enemy enemy)
    {
        tower.AddEnemyToList(enemy);
    }
}
