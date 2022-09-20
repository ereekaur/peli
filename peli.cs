using System;
using System.Collections.Generic;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Effects;
using Jypeli.Widgets;

/// <summary>
///  @author  Erno Kauranen
///  
///  
/// </summary>
public class KahviPeli : PhysicsGame
{
   
    int vaikeus = 1; // Asettaa vaikeustasokertoimen
    List<PlatformCharacter> pommilista = new List<PlatformCharacter>();
    int alkupisteet = 10000;
    IntMeter pisteLaskuri;
    Label pisteNaytto = new Label();
    IntMeter keratytEsineet = new IntMeter(0);
    Image kupinKuva = LoadImage("kahvikuppi2");
    public object HighTolerance { get; set; }
    public object latestPlatform { get; set; }
    DoubleMeter hitPoints;
    /// <summary>
    /// Kutsutaan laskuria joka vähentää pisteitä
    /// </summary>
    public override void Begin()
    {

        int kenttaNro = 1;   // Määrää kentän mistä aloitetaan
        Itsepeli(kenttaNro, alkupisteet);
        LuoAikaLaskuri(alkupisteet, kenttaNro);

    }

    /// <summary>
    /// Aliohjelma, jossa luodaan peli kutsumalla muita aliohjelmia.
    /// </summary>
    /// <param name="kenttaNro"></param>
    /// <param name="alkupisteet"></param>

    void Itsepeli(int kenttaNro, int alkupisteet)
    {

        /// Aliohjelmakutsut:

        ClearAll();
        pommilaskuri();
        PlatformCharacter hahmo = LuoHahmo();
        LuoKentta(kenttaNro, alkupisteet, hahmo);
        Kenttajutut();
        hitPointsitKayttoon();
        LuoPommi(hahmo, kenttaNro);


        Timer timer = new Timer();
        timer.Timeout += delegate { LuoPommi(hahmo, kenttaNro); };

        if (kenttaNro == 2) timer.Interval = 1.5;
        else if (kenttaNro == 3) timer.Interval = 1;
        else timer.Interval = 2;
        timer.Start();

        pisteLaskuri = new IntMeter(0);
        pisteNaytto.BindTo(pisteLaskuri);
        pisteLaskuri.MaxValue = 3;
        pisteLaskuri.UpperLimit += delegate { KaikkiKeratty(kenttaNro, hahmo, alkupisteet); };


        /// Näppäinasetukset:
        
        Keyboard.Listen(Key.Left, ButtonState.Down, LiikutaHahmoa, "Lyö hahmoa vasemmalle", hahmo, -800);
        Keyboard.Listen(Key.Right, ButtonState.Down, LiikutaHahmoa, "Lyö hahmoa ylöspäin", hahmo, 800);
        Keyboard.Listen(Key.Up, ButtonState.Down, delegate { hahmo.Jump(3000); }, null);
        PhoneBackButton.Listen(ConfirmExit, "Lopeta peli");
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, Exit, "Lopeta peli");
        Keyboard.Listen(Key.Space, ButtonState.Pressed, Pause, "Pause");


    }
    
    /// <summary>
    /// Aliohjelma joka palauttaa hahmon.
    /// </summary>
    /// <returns></returns>
    PlatformCharacter LuoHahmo()
    {
        PlatformCharacter hahmo = new PlatformCharacter(40, 40, Shape.Circle);
        hahmo.Tag = "pelaaja";
        hahmo.X = -400;
        hahmo.Y = -300;
        hahmo.Restitution = 0.0;
        hahmo.CanMoveOnAir = true;
        Image olionKuva = LoadImage("ukko");
        hahmo.Image = olionKuva;
        hahmo.CollisionIgnorer = new Physics2DDotNet.Ignorers.ObjectIgnorer();
        Add(hahmo);
        return hahmo;

    }

    /// <summary>
    /// Aliohjelma joka lisää luodut pommit peliin satunnaisiin paikkoihin.
    /// </summary>
    /// <param name="pommi">Vihollinen johon osuessa menetetään energiaa</param>
    /// <param name="hahmo">Hahmo, jolla pelataan</param>
    /// <param name="kenttaNro">kenttaNro määrää kentän</param>
    private void lisaaPommit(PlatformCharacter pommi, PlatformCharacter hahmo, int kenttaNro)
    {
        for (int i = 0; i < 3; i++)


            pommi.X = RandomGen.NextInt(-500, 700);
        pommi.Y = 500;
        pommi.Color = Color.Orange;
        Add(pommi);
        Image pomminkuva = LoadImage("bomb2");
        pommi.Image = pomminkuva;

    }

    /// <summary>
    /// Kentän alustukset.
    /// </summary>
    private void Kenttajutut()
    {

        Image taustaKuva = LoadImage("tausta4");
        Level.Background.Image = taustaKuva;
        Level.Width = 1600;
        Level.Height = 900;
        Level.Background.ScaleToLevelFull();
        Camera.ZoomToLevel();
        Level.Background.Color = Color.Gray;
        Level.CreateBorders();
        GameObject moi = new GameObject(40, 40);
        moi.X = 540;
        moi.Y = 430;
        Image hpkuva = LoadImage("hp");
        moi.Image = hpkuva;
        Add(moi);
        Gravity = new Vector(0, -8000
          );

    }

    /// <summary>
    /// Aliohjelma, jossa luodaan pommit.
    /// </summary>
    /// <param name="hahmo"></param>
    /// <param name="kenttaNro"></param>
    private void LuoPommi(PlatformCharacter hahmo, int kenttaNro)
    {

        for (int i = 0; i < 3; i++)
        {
        
            PlatformCharacter pommi = new PlatformCharacter(40, 40, Shape.Circle);
            pommi.Color = Color.Orange;
            pommi.Brain = new RandomMoverBrain();
            Image pomminkuva = LoadImage("bomb2");
            pommi.Image = pomminkuva;
            pommi.CollisionIgnoreGroup = 2;
            pommilista.Add(pommi);


            AddCollisionHandler(pommi, hahmo, delegate
            {
                energiaaPois(pommi, hahmo);
                pommilista.Remove(pommi);
                TuhoaPommi(pommi);
                PisteitaPois(alkupisteet);
            }
            );
            
            lisaaPommit(pommi, hahmo, kenttaNro);

        }
    }

    /// <summary>
    /// Aliohjelma joka liikuttaa hahmona tietyllä voimalla
    /// </summary>
    /// <param name="hahmo"></param>
    /// <param name="suunta"></param>
    private void LiikutaHahmoa(PlatformCharacter hahmo, int suunta)
    {
        hahmo.Walk(suunta);
    }
    /// <summary>
    /// Aliohjelma joka tuhoaa hahmon sekä pommin
    /// </summary>
    /// <param name="pommi"></param>
    /// <param name="hahmo"></param>
    private void TuhoaHahmo(IPhysicsObject pommi, PhysicsObject hahmo)
    {

        Explosion rajahdys = new Explosion(10);
        rajahdys.Position = hahmo.Position;
        rajahdys.UseShockWave = false;
        Add(rajahdys);
        Remove(hahmo);
        Remove(pommi);

    }

    /// <summary>
    /// Aliohjelma joka luo alkuvalikon.
    /// </summary>
    private void AlkuValikko()
    {
        MultiSelectWindow alkuValikko = new MultiSelectWindow("", "Aloita peli", "Poistu pelistä", "Aloita peli vaikeampana");
        alkuValikko.AddItemHandler(0, Pause);
        alkuValikko.AddItemHandler(1, Exit);
        alkuValikko.AddItemHandler(2, delegate
        {
            int vaikeus2 = 10;
            vaikeus = vaikeus2;
        });
        Add(alkuValikko);
        Pause();
    }
    
    /// <summary>
    /// Aliohjelma, joka luo hitpointsmittarin ja lisää sen.
    /// </summary>
    private void hitPointsitKayttoon()
    {
        hitPoints = new DoubleMeter(100);
        hitPoints.MaxValue = 100;
        hitPoints.LowerLimit += gameOver;
        ProgressBar hitBar = new ProgressBar(200, 40);
        ProgressBar hitBar2 = new ProgressBar(100, 20);
        hitBar.X = Screen.Right - 150;
        hitBar2.Color = Color.Red;
        hitBar.Color = Color.Green;
        hitBar.Y = Screen.Top - 20;
        hitBar2.X = Screen.Right - 150;
        hitBar2.Y = Screen.Top - 20;
        hitBar.BindTo(hitPoints);

        Add(hitBar);

    }

    /// <summary>
    /// Aliohjelmaa kutsutaan, kun energia loppuu.
    /// </summary>
    private void gameOver()
    {

        MessageDisplay.Add("Game Over!");
        Pause();
        MultiSelectWindow PelinHavio = new MultiSelectWindow("Pelisi päättyi!", "restart ", "Exit");

        PelinHavio.AddItemHandler(0, delegate
        {
            AloitaAlusta();
        });

        Add(PelinHavio);
    }
    /// <summary>
    /// Aliohjelma, joka vähentää energiaa
    /// </summary>
    /// <param name="pommi"></param>
    /// <param name="hahmo"></param>
    private void energiaaPois(PhysicsObject pommi, PhysicsObject hahmo)
    {
        hitPoints.Value -= 10 * vaikeus;
        if (hitPoints <= 0)
            TuhoaHahmo(pommi, hahmo);
    }

    /// <summary>
    /// Aliohjelma jossa luodaan tasot
    /// </summary>
    /// <param name="paikka">Tason paikka</param>
    /// <param name="leveys">Tason leveys</param>
    /// <param name="korkeus">Tason korkeus</param>
    /// <param name="vari"></param>
    private void LisaaTaso(Vector paikka, double leveys, double korkeus, Color vari)
    {
        PhysicsObject taso = PhysicsObject.CreateStaticObject(200, 15);
        taso.Position = paikka;
        taso.Color = vari;
        Add(taso);
        Image tiilikuva = LoadImage("toinentiili");
        taso.Image = tiilikuva;

    }

    /// <summary>
    /// Aliohjelma jossa luodaan tasot
    /// </summary>
    /// <param name="paikka"></param>
    /// <param name="leveys"></param>
    /// <param name="korkeus"></param>
    /// <param name="vari"></param>
   private void LisaaTaso2(Vector paikka, double leveys, double korkeus, Color vari)
    {
        PhysicsObject taso2 = PhysicsObject.CreateStaticObject(200, 15);
        taso2.Position = paikka;
        taso2.Color = vari;
        Add(taso2);

    }

    /// <summary>
    // Aliohjelma jota kutsutaan törmättäessä pommiin ja joka poistaa pommin pelistä sekä lisää räjähdyksen
    //
    /// </summary>
    /// <param name="pommi"></param>
    private void TuhoaPommi(IPhysicsObject pommi)
    {

        Explosion rajahdys = new Explosion(100);
        rajahdys.Position = pommi.Position;
        rajahdys.UseShockWave = false;
        Add(rajahdys);
        Remove(pommi);

    }

    /// <summary>
    /// Aliohjelma joka kasvataa kenttaNroa yhdellä mikäli kaikki kahvikupit on kerätty kentästä.
    /// </summary>
    /// <param name="kenttaNro"></param>
    /// <param name="hahmo"></param>
    /// <param name="alkupisteet"></param>
    private void KaikkiKeratty(int kenttaNro, PlatformCharacter hahmo, int alkupisteet)
    {
        kenttaNro += 1;
        Itsepeli(kenttaNro, alkupisteet);

    }
    
    /// <summary>
    /// Kutsutaan, kun painetaan valikosta restart-näppäintä
    /// </summary>
    private void AloitaAlusta()
    {
        ClearAll();
        Begin();

    }
    
    /// <summary>
    /// Aliohjelma jossa luodaan kuppi
    /// </summary>
    /// <param name="paikka">Kupin paikka</param>
    /// <param name="leveys">Kupin leveys</param>
    /// <param name="korkeus">Kupin korkeus</param>
    /// <param name="vari"></param>
   private  void TeeKuppi(Vector paikka, double leveys, double korkeus, Color vari)
    {
        PhysicsObject kuppi = PhysicsObject.CreateStaticObject(50, 50, Shape.Rectangle);
        kuppi.Position = paikka;
        kuppi.Image = kupinKuva;
        Add(kuppi);


        AddCollisionHandler(kuppi, delegate(PhysicsObject o, PlatformCharacter hahmo)
        {

            pisteLaskuri.Value += 1;
            Remove(kuppi);
        });
    }

    /// <summary>
    /// Aliohjelma, joka vähentää pisteitä sen mukaan paljonko aikaa on kulunut.
    /// </summary>
    /// <param name="kenttaNro"></param>
    private void PisteitaPois(int kenttaNro)
    {
        int alkupisteet2 = alkupisteet - 40;
        alkupisteet = alkupisteet2;
        if (kenttaNro == 4)
            MessageDisplay.Add(alkupisteet.ToString());    // Mikäli peli on pelattu läpi niin ruudulle tulostetaan pistemäärä.
    }

    /// <summary>
    ///  Aliohjelma, jossa luodaan kentät.
    /// </summary>
   private void LuoKentta(int kenttaNro, int alkupisteet, PlatformCharacter hahmo)
    {

        /// Luodaan eri kentät sen mukaan mikä kenttaNro on.

        if (kenttaNro == 1)
        {
            AlkuValikko();
            ///  kentta1(hahmo);
            TileMap kentta = TileMap.FromLevelAsset("kentta");
            kentta.SetTileMethod('$', LisaaTaso, Color.White);
            kentta.SetTileMethod('#', LisaaTaso, Color.Brown);
            kentta.SetTileMethod('?', TeeKuppi, Color.White);
            kentta.Execute();
        }


        if (kenttaNro == 2)
        {
            MessageDisplay.Add("Taso2");


            TileMap kentta2 = TileMap.FromLevelAsset("kentta2");
            kentta2.SetTileMethod('$', LisaaTaso, Color.White);
            kentta2.SetTileMethod('#', LisaaTaso, Color.Brown);
            kentta2.SetTileMethod('?', TeeKuppi, Color.Pink);
            kentta2.Execute();

        }

        if (kenttaNro == 3)
        {
            MessageDisplay.Add("Taso3");
            TileMap kentta3 = TileMap.FromLevelAsset("kentta3");

            kentta3.SetTileMethod('$', LisaaTaso, Color.White);
            kentta3.SetTileMethod('#', LisaaTaso, Color.Brown);
            kentta3.SetTileMethod('?', TeeKuppi, Color.Pink);
            kentta3.Execute();

        }

        if (kenttaNro == 4)
        {
            ClearAll();

            MessageDisplay.Add("Peli läpäisty!");
            Label moi = new Label(5000, 500, "Onneksi olkoon, olet päässyt pelin läpi");
            moi.X = 0;
            moi.Y = 300;
            Add(moi);
            PisteitaPois(kenttaNro);
            Pause();

        }
    }

    /// <summary>
    /// Laskuri, joka kutsuu aliohjelmaa PisteitaPois tietyn väliajoin.
    /// </summary>
    /// <param name="alkupisteet">Pisteet joista vähennetään pisteitä ajan kuluessa</param>
    /// <param name="kenttaNro"></param>
    private void LuoAikaLaskuri(int alkupisteet, int kenttaNro)
    {
        Timer aikaLaskuri = new Timer();
        aikaLaskuri.Interval = 0.5;
        aikaLaskuri.Timeout += delegate { PisteitaPois(kenttaNro); };
        aikaLaskuri.Start();

    }
    
    /// <summary>
    /// Aliohjelma joka poistaa pommit tietyn väliajoin.
    /// </summary>
    private void pommilaskuri()
    {
        Timer pommilaskuri = new Timer();
        pommilaskuri.Start();
        pommilaskuri.Interval = 4;
        pommilaskuri.Timeout += delegate
        {
            foreach (PlatformCharacter pommi in pommilista)
            {
                ///pommi.Destroy();
                TuhoaPommi(pommi);

            };
        };
    }
}












    
