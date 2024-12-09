using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartNextStage : MonoBehaviour
{
    public player_agent nextAI_push_easy;
    public player_agent nextAI_push_normal;
    public player_agent nextAI_push_hard;
    public player_agent1 nextAI_jin_easy;
    public player_agent1 nextAI_jin_normal;
    public player_agent1 nextAI_jin_hard;

    public void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            if(nextAI_push_easy != null)
            {
                switch(other.GetComponent<player_agent1>().difficultIndex)
                {
                    case 1:
                        nextAI_push_easy.gameObject.SetActive(true);
                        nextAI_push_easy.isActive = true;
                        break;
                    case 2:
                        nextAI_push_normal.gameObject.SetActive(true);
                        nextAI_push_normal.isActive = true;
                        break;
                    case 3:
                        nextAI_push_hard.gameObject.SetActive(true);
                        nextAI_push_hard.isActive = true;
                        break;
                }
                
            }
            else if(nextAI_jin_easy)
            {
                switch (other.GetComponent<player_agent>().difficultIndex)
                {
                    case 1:
                        nextAI_jin_easy.gameObject.SetActive(true);
                        nextAI_jin_easy.isActive = true;
                        break;
                    case 2:
                        nextAI_jin_normal.gameObject.SetActive(true);
                        nextAI_jin_normal.isActive = true;
                        break;
                    case 3:
                        nextAI_jin_hard.gameObject.SetActive(true);
                        nextAI_jin_hard.isActive = true;
                        break;
                }
            }
        }
    }
}
