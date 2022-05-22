using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{
    private const float CAMERA_ORTHO_SIZE = 50f;
    private const float PIPE_WIDTH = 7.8f;
    private const float PIPE_HEAD_HEIGHT = 3.75f;
    private const float PIPE_MOVE_SPEED = 20f;
    private const float PIPE_DESTROY_X_POSITION = -100f;
    private const float PIPE_SPAWN_X_POSITION = 100f;
    private const float BIRD_POSITION = 0f;
    private static Level instance;

    public static Level GetInstance(){
        return instance;
    }

    private List<Pipe> pipeList;
    private int pipesPassedCount = 0;
    private int pipesSpawned;
    private float pipeSpawnTimer;
    private float pipeSpawnTimerMax;
    private float gapSize;
    public enum Difficulty{
        Easy, 
        Medium,
        Hard,
        Impossible
    }

    private void Awake(){
        instance = this;
        pipeList = new List<Pipe>();
        pipeSpawnTimerMax = 1f;
        setDifficulty(Difficulty.Easy);
    }

    private void Start(){
        CreatePipe(40f, 20f, true);
        CreatePipe(30f, 20f, false);
    }   

    private void Update(){
        HandlePipeMovement();
        HandlePipeSpawning();
    }

    private void HandlePipeSpawning(){
        pipeSpawnTimer -= Time.deltaTime;
        if(pipeSpawnTimer < 0){
            //Spawn another pipe
            pipeSpawnTimer += pipeSpawnTimerMax;

            float heightEdgeLimit = 10f;
            float minHeight = gapSize * .5f + heightEdgeLimit; 
            float totalHeight = CAMERA_ORTHO_SIZE * 2f;
            float maxHeight = totalHeight - gapSize* .5f - heightEdgeLimit;

            float height = UnityEngine.Random.Range(minHeight, maxHeight);
            CreateGapPipes(height, gapSize, PIPE_SPAWN_X_POSITION);
        }
    }

    private void setDifficulty(Difficulty difficulty){
        switch(difficulty){
            case Difficulty.Easy:
                gapSize = 45f;
                pipeSpawnTimer = 1.35f;
                break;
            case Difficulty.Medium:
                gapSize = 37f;
                pipeSpawnTimer = 1.3f;
                break;
            case Difficulty.Hard:
                gapSize = 32f;
                pipeSpawnTimer = 1.2f;
                break;
            case Difficulty.Impossible:
                gapSize = 28f;
                pipeSpawnTimer = 1.1f;
                break;
            
        }
    }
    private Difficulty GetDifficulty(){
        if(pipesSpawned >= 30) return Difficulty.Impossible;
        if(pipesSpawned >= 20) return Difficulty.Hard;
        if(pipesSpawned >= 10) return Difficulty.Medium;
        return Difficulty.Easy;
    }

    private void CreateGapPipes(float gapY, float gapSize, float xPosition)
    {
        CreatePipe(gapY - gapSize * .5f, xPosition, true);
        CreatePipe(CAMERA_ORTHO_SIZE * 2f - gapY - gapSize * .5f, xPosition, false);
        pipesSpawned++;
        setDifficulty(GetDifficulty());
    }

    private void HandlePipeMovement(){
        for(int i = 0; i < pipeList.Count; i++){
            Pipe pipe = pipeList[i];
            bool isPassedTheBird = pipe.GetXPosition() > BIRD_POSITION;

            pipe.Move();
            if(isPassedTheBird && pipe.GetXPosition() <= BIRD_POSITION){
                pipesPassedCount++;
            }

            if(pipe.GetXPosition() < PIPE_DESTROY_X_POSITION){
                pipe.pipeDestroy();
                pipeList.Remove(pipe);
                i--;
            }
        }
    }

    private void CreatePipe(float height, float xPosition, bool createBottom){
        //Pipe head
        Transform pipeHead = Instantiate(GameAssets.GetInstance().pfPipeHead);

        float pipeHeadYPosition;
        if(createBottom){
            pipeHeadYPosition = -CAMERA_ORTHO_SIZE + height - PIPE_HEAD_HEIGHT * .5f;
        }else{
            pipeHeadYPosition = CAMERA_ORTHO_SIZE - height + PIPE_HEAD_HEIGHT * .5f;
        }
        pipeHead.position = new Vector3(xPosition, pipeHeadYPosition);

        //Pipe body
        Transform pipeBody = Instantiate(GameAssets.GetInstance().pfPipeBody);

        float pipeBodyYPosition;
        if(createBottom){
            pipeBodyYPosition = -CAMERA_ORTHO_SIZE;
        }else{
            pipeBodyYPosition = CAMERA_ORTHO_SIZE;
            pipeBody.localScale = new Vector3(1, -1, 1);
        }
        pipeBody.position = new Vector3(xPosition, pipeBodyYPosition);
 
        //Render
        SpriteRenderer pipeBodySpriteRenderer = pipeBody.GetComponent<SpriteRenderer>();
        pipeBodySpriteRenderer.size = new Vector2(PIPE_WIDTH, height);

        BoxCollider2D pipBodyBoxCollider = pipeBody.GetComponent<BoxCollider2D>();
        pipBodyBoxCollider.size = new Vector2(PIPE_WIDTH, height);
        pipBodyBoxCollider.offset = new Vector2(0f, height * .5f);

        Pipe pipe = new Pipe(pipeHead, pipeBody);
        pipeList.Add(pipe);
    } 

    public int getPipesSpawned(){
        return pipesSpawned;
    }

    public int getPipesPassedCount(){
        return pipesPassedCount;
    }

    //-------------------------------------------- Pipe class ----------------------------------------------//
    private class Pipe{
        private Transform pipeHeadTransform;
        private Transform pipeBodyTransform;

        public Pipe(Transform pipeHeadTransform, Transform pipeBodyTransform){
            this.pipeBodyTransform = pipeBodyTransform;
            this.pipeHeadTransform = pipeHeadTransform;
        }

        public void Move(){
            pipeHeadTransform.position += new Vector3(-1, 0, 0) * PIPE_MOVE_SPEED * Time.deltaTime; 
            pipeBodyTransform.position += new Vector3(-1, 0, 0) * PIPE_MOVE_SPEED * Time.deltaTime; 
        }

        public float GetXPosition(){
            return pipeHeadTransform.position.x;
        }

        public void pipeDestroy(){
            Destroy(pipeHeadTransform.gameObject);
            Destroy(pipeBodyTransform.gameObject);
        }
    }
}
