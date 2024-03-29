using System;                                   // System contains a lot of default C# libraries 
using GXPEngine;                                // GXPEngine contains the engine
using System.Drawing;							// System.Drawing contains drawing tools such as Color definitions
using System.Media;
using System.Drawing.Text;
using System.Collections.Generic;
using System.Collections;
using System.Threading;
using System.Security.Cryptography;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.IO;
using System.Net.NetworkInformation;
using System.Reflection;


public class MyGame : Game {

    public Random rnd = new Random();

    private Sprite background;
	private Sprite background1;
	private float backgroundSpeed = 1f;
	private float baseSpeed = 1f;

	private Sound gameStart = new Sound("Start.wav");

	public Sound multUp = new Sound("Multiplier Up.wav");
    private Sound multLost = new Sound("Multiplier Lost.wav");

	private SoundChannel deathSC;

	private Sound seagull = new Sound("Seagull.wav");

	private SoundChannel engineSC; 
	private SoundChannel waterSC;
	private SoundChannel bgmSC;

    private AnimationSprite beach;
	private Player player;

	private Sprite enemyPlace;
	private List<Enemy> enemies = new List<Enemy>();
    private List<Wave> waves = new List<Wave>();
    private List<Pickup> pickups = new List<Pickup>();
    private bool spawnEnemy = false;
	private float enemyCooldown = 1.5f;

    private List<int> toDestroy = new List<int>();

	private EasyDraw startScreen;
	private EasyDraw gameOverScreen;

	private Sprite title;


    public float difficulty = 1;
	public int score { get; set; } = 0;
	public int multiplier = 1;
	public bool multiplierPU;
	private int finalScore;

	public EasyDraw UI;
	public EasyDraw timedUI;

	public bool expression;

	public String[] enemyList = new string[5];
	public String[] expressions = new string[5];

    private bool restart;
    public bool gameOver = true;
	private bool playerDestroyed = true;
    public MyGame() : base(1366, 768, false)
    {
		//targetFps = 60;

		//TODO:

		//UI menu screen still needs to be finalized(Art will come)

		//Scoreboard still needs to be implemented


        Settings.Load();

        background1 = new Sprite("background1.png", false, false);
        background1.SetXY(0, -height);
        background = new Sprite("background.png", false, false);
        background.SetXY(0, -height);
		AddChild(background1);
        AddChild(background);

        timedUI = new EasyDraw(width, height, false);

		

        startScreen = new EasyDraw(width, height);
        title = new Sprite("title.png", false, false);
        AddChild(startScreen);
		startScreen.AddChild(title);
		title.x += 250;
		startScreen.Fill(Color.Black);
        startScreen.TextFont(Utils.LoadFont("CheerfulPeach.otf", 40));
        startScreen.Text("Press S to Start", width / 2.75f + 3, height / 2);
		startScreen.Fill(Color.White);
        startScreen.TextFont(Utils.LoadFont("CheerfulPeach.otf", 40));
        startScreen.Text("Press S to Start", width / 2.75f, height / 2);

        gameOverScreen = new EasyDraw(width, height);
        gameOverScreen.TextFont("CheerfulPeach.otf", 40);
		gameOverScreen.TextAlign(CenterMode.Center, CenterMode.Center);

		enemyList[0] = "shark.png";
		enemyList[1] = "wood.png";
		enemyList[2] = "tentacle.png";
		enemyList[3] = "rock.png";
		enemyList[4] = "rock1.png";

		expressions[0] = "AWESOME!!";
        expressions[1] = "GREAT!!";
        expressions[2] = "SUPER!!";
        expressions[3] = "EPIC!!";
        expressions[4] = "JETTASTIC!!";

        beach = new AnimationSprite("beach.png", 4, 2, -1, false, false);
		beach.SetCycle(0,8,12);
		AddChild(beach);

        enemyPlace = new Sprite("shark.png", false, false);
        enemyPlace.alpha = 0;
        AddChild(enemyPlace);

        player = new Player("jetski.png", 7, 7, this, 48);
        AddChild(player);
		waterSC = new Sound("Water.wav", true, true).Play();
		
		AddChild(timedUI);
    }

	// For every game object, Update is called every frame, by the engine:
	void Update()
	{

        beach.Animate();
		MoveBackground();

		if (!gameOver)
		{

			player.Update();
			EnemyUpdate();
			WaveUpdate();
			PickupUpdate();
			UI.ClearTransparent();
			UpdateUI();
            

			if (player.lives < 1)
			{
				gameOver = true;
				deathSC = new Sound("Death.wav").Play();
                bgmSC.Stop();
                engineSC.Stop();
                waterSC.IsPaused = true;
            }
		}
		else if (!playerDestroyed)
		{
			if (!deathSC.IsPlaying)
			GameOver(); //Death is inevitable 8(
		}
		else
		{
			if (Input.GetKeyDown(Key.S))
			{
				StartGame();
			}
		}
		
	}
	private void StartGame()
	{
        background1.SetXY(background1.x, ((int)background1.y));
        background.SetXY(background.x, ((int)background.y));
        if (!restart) startScreen.Destroy();

		else
		{
			RemoveChild(background);
			RemoveChild(gameOverScreen);
			UI = new EasyDraw(width, 200, false);
			timedUI = new EasyDraw(width, height, false); 
            background.SetXY(0, -height);
            background1.SetXY(0, -height);

			AddChild(background1);
            AddChild(background);
			AddChild(beach);
            enemyPlace = new Sprite("shark.png", false, false);
            enemyPlace.alpha = 0;
            
            AddChild(enemyPlace);
            beach.SetXY(0, 0);
            player = new Player("jetski.png", 7, 7, this, 48);
			
			AddChild(player);
			waterSC.IsPaused = false;
        }

        

        UI = new EasyDraw(width, height, false);
        

        AddChild(new Coroutine(enemyLoop()));
		AddChild(new Coroutine(WaveLoop()));
		AddChild(new Coroutine(PickupLoop()));
		AddChild(new Coroutine(difficultyLoop()));
		AddChild(new Coroutine(scoreTime()));
		AddChild(new Coroutine(gullLoop()));

        AddChild(UI);
        UI.TextFont(Utils.LoadFont("CheerfulPeach.otf", 36));
        UI.Fill(0);

		AddChild(timedUI);

		gameOver = false;
        playerDestroyed = false;
		player.start = true;
		gameStart.Play();
        player.SetCycle(0, 12, 24);
        engineSC = new Sound("Jetski Engine.wav", true, true).Play();
        bgmSC = new Sound("bgm.mp3", true, true).Play(false, 0, 0.8f);
    }

	private void GameOver()
	{
		
		while (deathSC.IsPlaying) ;
		gameOverScreen.ClearTransparent();
		restart = true;
		player.LateDestroy();
		playerDestroyed = true;
		RemoveChild(player);
        foreach (GameObject obj in GetChildren())
        {
			RemoveChild(obj);
			if(obj.GetType().Equals(typeof(Enemy)))
			{
				obj.LateDestroy();
			}
        }
		finalScore = score;
        Console.WriteLine(finalScore);
        score = 0;
		multiplier = 1;
		difficulty = 1;
		AddChild(background);
		AddChild(gameOverScreen);
		
        Console.WriteLine(Settings.highScore);
        gameOverScreen.Fill(Color.Black);
        gameOverScreen.TextFont(Utils.LoadFont("CheerfulPeach.otf", 40));
        gameOverScreen.Text("    You Lost" + "\n" + "Score:" + finalScore + "\nPress S to try again", width / 2f + 3, height / 2);
        gameOverScreen.Fill(Color.White);
        gameOverScreen.Text("    You Lost" + "\n" + "Score:" + finalScore + "\nPress S to try again", width / 2f, height / 2);
    }

	public void changeScore(int change)
	{
		score += change * multiplier;
        
    }

	static void Main()                          // Main() is the first method that's called when the program is run
	{
		new MyGame().Start();                   // Create a "MyGame" and start it
	}
	private void MoveBackground()
	{
		backgroundSpeed = Math.Max(baseSpeed * (difficulty/2), baseSpeed);
		if (!gameOver)
		{
			beach.Translate(0, backgroundSpeed * Time.deltaTime / 5);
			background.Translate(0, backgroundSpeed * Time.deltaTime / 5);
			background1.Translate(0, backgroundSpeed * Time.deltaTime / 5);
		}
		else
		{
			background1.Translate(0, backgroundSpeed * (Time.deltaTime / 5) / 5);
            background.Translate(0, backgroundSpeed * (Time.deltaTime / 5) / 5);
        }

		if (background.y > -4)
		{
			background.SetXY(0, -height);
            background1.SetXY(0, -height);

        }
        if (beach.y > height) RemoveChild(beach);
		background.alpha = Math.Max(1 - (difficulty - 1) / 2, 0);
        background1.alpha = Math.Min(0 + (difficulty - 1) / 2, 1);
    }

	private void EnemyUpdate()
	{
        foreach (Enemy enemy in enemies)
        {
            enemy.Update();

            if (enemy.flagged && enemy.breakable)
            {
                if (!enemy.dead)
                {
                    enemy.dead = true;

                    enemy.Death();

                    changeScore(50);
                    if (multiplier < 3)
                    {
						if (!expression && !player.hit)
						{
							expression = true;
							text(expressions[rnd.Next(5)], 28, 150, Color.Yellow, 1, 36);
						}
                        multiplier++;
                        multUp.Play();
                    }
                }
            }
            if (enemy.y > height)
            {
                if (enemy.breakable && enemy.flagged == false && multiplier != 1 && !multiplierPU)
                {
					text("Multiplier Lost!", enemy.x - 100, game.height - 50, Color.Red, 1.5f, 30);
                    multiplier = 1;
                    multLost.Play();
                }
                toDestroy.Add(enemies.IndexOf(enemy));
            }

        }
        foreach (int index in toDestroy)
        {
            enemies[index].LateDestroy();
            enemies.RemoveAt(index);

        }
        toDestroy.Clear();
    }
	
	public void text(string text, float x, float y, Color color, float seconds, int fontSize)
	{
		LateAddChild(new Coroutine(ScreenText(text, x, y, color, seconds, fontSize)));
	}

	private void PickupUpdate()
	{
		foreach (Pickup pickup in pickups)
		{
			pickup.Update();
		}
	}

	private void WaveUpdate()
	{
		foreach(Wave wave in waves)
		{
			wave.Update();
			if (wave.y > height + 100)
			{ toDestroy.Add(waves.IndexOf(wave)); }
		}
        foreach (int index in toDestroy)
        {
            waves[index].LateDestroy();
            waves.RemoveAt(index);

        }
        toDestroy.Clear();
    }

	private void UpdateUI()
	{
		
		UI.Fill(Color.Black);
        UI.TextFont(Utils.LoadFont("CheerfulPeach.otf", 36));
        UI.Text("Score: " + score, 28, 60);
        UI.TextFont(Utils.LoadFont("CheerfulPeach.otf", 24));
        UI.Text("Multiplier: " + multiplier + "x", 28, 100);
        UI.TextFont(Utils.LoadFont("CheerfulPeach.otf", 36));
		UI.Text("Lives: " + Math.Floor(player.lives), width - 222, 60);

        
        UI.Fill(Color.White);

        UI.Text("Score: " + score, 25, 60);
        UI.TextFont(Utils.LoadFont("CheerfulPeach.otf", 24));
        UI.Text("Multiplier: " + multiplier + "x", 25, 100);
        UI.TextFont(Utils.LoadFont("CheerfulPeach.otf", 36));
        UI.Text("Lives: " + Math.Floor(player.lives), width - 225, 60);
    }

	IEnumerator ScreenText(string text, float x, float y, Color color, float seconds, int fontSize)
	{
		
		timedUI.Fill(Color.Black);
		timedUI.TextFont(Utils.LoadFont("CheerfulPeach.otf", fontSize));
		timedUI.Text(text, x + 3, y + 3);

		timedUI.Fill(color);
		timedUI.TextFont(Utils.LoadFont("CheerfulPeach.otf", fontSize));
		timedUI.Text(text, x, y);
		yield return new WaitForSeconds(seconds);
		timedUI.ClearTransparent();
		if (y == 150)
			expression = false;
    }

	IEnumerator WaveLoop()
	{
		while (!gameOver)
		{
			int random = rnd.Next(10);
			if(random > 4)
			{
				yield return new WaitForSeconds(random);
				Wave newWave = new Wave("wave.png", 3, 2, rnd.Next(width - 150), 5);
				newWave.SetCycle(0, 5, 30);
				enemyPlace.AddChild(newWave);
				waves.Add(newWave);
			}
		}
	}

    IEnumerator PickupLoop()
    {
		while (!gameOver)
		{

			yield return new WaitForSeconds(25);
			int random = rnd.Next(3);
			Pickup pickup;

            switch (random)
			{
				case 0:
                    pickup = new Pickup("speed.png", rnd.Next(width - 150), player, this, random,3,3);
                    break;
				case 1:
                    pickup = new Pickup("life.png", rnd.Next(width - 150), player, this, random,3,3);
                    break;
				case 2:
                    pickup = new Pickup("multiplier.png", rnd.Next(width - 150), player, this, random,3,3);
                    break;
				default:
                    pickup = new Pickup("triangle.png", rnd.Next(width - 150), player, this, random, 1, 1);
					break;
            }
            pickup.SetCycle(0, 9, 24);
            enemyPlace.AddChild(pickup);
			pickups.Add(pickup);

		}
    }

    IEnumerator enemyLoop()
	{
		while (!gameOver)
		{
			int randomNumber = rnd.Next(5);
            switch(randomNumber)
			{
				case 0:
					{
						Enemy newEnemy = new Enemy(enemyList[randomNumber], 5, 5, 24);
                        enemies.Add(newEnemy);
                        enemyPlace.AddChild(newEnemy);
                        break;
					}
                case 1:
                    {
                        Enemy newEnemy = new Enemy(enemyList[randomNumber], 2, 2);
                        enemies.Add(newEnemy);
                        enemyPlace.AddChild(newEnemy);
                        break;
                    }
                case 2:
                    {
                        Enemy newEnemy = new Enemy(enemyList[randomNumber], 5, 3);
                        enemies.Add(newEnemy);
                        enemyPlace.AddChild(newEnemy);
                        break;
                    }
                case 3:
                    {
                        Enemy newEnemy = new Enemy(enemyList[randomNumber], 2, 2);
                        enemies.Add(newEnemy);
                        enemyPlace.AddChild(newEnemy);
                        break;
                    }
                case 4:
                    {
                        Enemy newEnemy = new Enemy(enemyList[randomNumber], 2, 2);
                        enemies.Add(newEnemy);
                        enemyPlace.AddChild(newEnemy);
                        break;
                    }
            }
            
				Console.WriteLine(difficulty);

				

			yield return new WaitForSeconds(enemyCooldown / difficulty);

			
		}
    }

	IEnumerator scoreTime()
	{
		while (!gameOver)
		{
			yield return new WaitForSeconds(0.5f);
			changeScore(3);
		}
	}

	IEnumerator gullLoop()
	{
		int random;
		while (!gameOver)
		{
			random = rnd.Next(30);
			if (random > 9)
			{
				yield return new WaitForSeconds(random);
				seagull.Play();
			}
		}
	}

	IEnumerator difficultyLoop() 
	{
			while (difficulty < 3)
			{
				yield return new WaitForSeconds(0.5f);

                difficulty += 0.01f;

			}    
    }


}