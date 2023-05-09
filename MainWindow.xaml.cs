using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using System.Windows.Threading;
using System.Security.Permissions;
using System.Collections;
using Horses.UserControls;
using System.Windows.Media.TextFormatting;
using System.Windows.Markup;
using System.Timers;
using System.Diagnostics.PerformanceData;
using Microsoft.Win32;

namespace Horses
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int shift = 0;
        Image background = new System.Windows.Controls.Image();
        DispatcherTimer animTimer = new DispatcherTimer();
        int betStep = 5;
        private int currentIndex = 0;
        List<ImageSource> horses = new List<ImageSource>();
        List<Horse> participants = new List<Horse>();
        List<string> horseNames = new List<string>() { "OstWind", "Sugar", "Cash", "Spirit", "Rebel", "Lucky", "Chance", "Rusty", "Noir", "Adobe", "Cinnamon", "Pepper", "Rembrand", "Jet", "Rio" };
        Random rnd = new Random();
        DispatcherTimer checker = new DispatcherTimer();
        int finishCounter = 0;
        bool isFirstHorse = false;
        double currentBet;
        string nameIsBettingTo;
        int counterForLabels = 0;
        int amountOfParticipants = 5;


        public MainWindow()
        {
            InitializeComponent();
            animTimer.Interval = new TimeSpan(0, 0, 0, 0, 25);
            animTimer.Tick += new EventHandler(animTimer_Tick);

           

            DispatcherTimer statChecker = new DispatcherTimer();
            statChecker.Interval = new TimeSpan(0, 0, 0, 0, 50);
            statChecker.Tick += new EventHandler(StatChecker_Tick);

            Thread statCheckerThread = new Thread(() => { statChecker.Start(); });
            statCheckerThread.Start();

            DispatcherTimer speedChecker = new DispatcherTimer();
            speedChecker.Interval = new TimeSpan(0, 0, 0, 1, 0);
            speedChecker.Tick += new EventHandler(SpeedChecker_Tick);

            Thread speedCheckerThread = new Thread(() => { speedChecker.Start(); });
            speedCheckerThread.Start();

            checker.Interval = new TimeSpan(0, 0, 0, 0, 25);
            checker.Tick += new EventHandler(Checker_Tick);

            checker.IsEnabled = true;
            checker.Start();

            Thread newTh = new Thread(() => { PlaceHorse(); });
            newTh.Start();

            //HorseNameLabel.Content = participants[0].Name.ToString();
            PreviousHorse.IsEnabled = false;
            BetHigher.IsEnabled = false;

            AnimationBackground();
            //AnimationHorse(Color.FromArgb(8, 124, 23, 32));

            


            //Thread animThread = new Thread(RunAnimation);
            //animThread.Start();
        }

        private void SpeedChecker_Tick(object sender, EventArgs e)
        {
            foreach (var item in participants)
            {
                item.Speed = rnd.Next(50, 401) / 100.0;
            }
        }

        private void StatChecker_Tick(object sender, EventArgs e)
        {
            if (GetTBToInt(UserBalance) >= GetTBToInt(BetAmountTextBlock))
            {
                PlaceBetButton.IsEnabled = true;
            }
            else PlaceBetButton.IsEnabled = false;

            if (currentBet == 0)
            {
                StartRaceBtn.IsEnabled = false;
            }
            else StartRaceBtn.IsEnabled = true;
        }

        private void AnimationBackground()
        {
            background.Source = new BitmapImage(new Uri("D:\\C#\\2 course\\2 sem\\OOP\\Horses\\Images\\Background\\Track.png"));
            grid1.Background = new ImageBrush()
            {
                ImageSource = background.Source,
                Stretch = Stretch.Uniform,
                TileMode = TileMode.Tile,
                Viewport = new Rect(0, 0, 600, 650),                
            };

            

        }

        private void Checker_Tick(object sender, EventArgs e)
        {
            foreach (var item in participants)
            {
                if (!item.IsRead)
                {
                    if (item.IsHorseFinished)
                    {
                        RaceData.ItemsSource = participants;
                        RaceData.Items.Refresh();
                        finishCounter++;
                        item.IsRead = true;
                        canv.Children.Remove(item.AnimationControl);
                        if (!isFirstHorse)
                        {
                            item.IsFirst = true;
                            isFirstHorse = true;
                        }

                        if (nameIsBettingTo == item.Name && item.IsFirst)
                        {
                            double newUserBalance = (GetTBToInt(UserBalance) - currentBet) + RecountingUserBalance(item);
                            UserBalance.Text = newUserBalance.ToString();
                            MessageBox.Show($"You won {RecountingUserBalance(item)}$");
                        }
                        else if (nameIsBettingTo == item.Name && !item.IsFirst)
                        {
                            double newUserBalance = GetTBToInt(UserBalance) - currentBet;
                            UserBalance.Text = newUserBalance.ToString();
                            MessageBox.Show($"You lost {currentBet}$");
                            if (newUserBalance <= 0)
                            {
                                MessageBox.Show("Bro, you got into debt. Prepare the kidney");
                            }
                            if (newUserBalance < GetTBToInt(AmountOfParticipants))
                            {
                                PlaceBetButton.IsEnabled = false;
                            }

                            StartRaceBtn.IsEnabled = false;
                            

                        }

                    }
                }

                
            }

            if (finishCounter == GetTBToInt(AmountOfParticipants))
            {
                animTimer.Stop();
                animTimer.IsEnabled = false;
                PlacedToTB.Text = String.Empty;
                if (GetTBToInt(BetAmountTextBlock) > GetTBToInt(UserBalance))
                {
                    PlaceBetButton.IsEnabled = false;
                }
                else PlaceBetButton.IsEnabled = true;

            }


        }

        private double RecountingUserBalance(Horse horse)
        {
            double currentUserBalance = 0;
            currentUserBalance = currentBet * horse.Coefficient;
            return currentUserBalance;
        }


        private void animTimer_Tick(object sender, EventArgs e)
        {
            shift -= 6;
            shift %= 600;
            grid1.Background = new ImageBrush()
            {
                ImageSource = background.Source,
                Stretch = Stretch.Uniform,
                TileMode = TileMode.Tile,
                ViewportUnits = BrushMappingMode.Absolute,
                Viewport = new Rect(shift, 0, 600, 650)
            };

            
            currentIndex++;

            // If we've displayed all the images, reset the index to 0
            if (currentIndex >= horses.Count)
            {
                currentIndex = 0;
            }



        }

        
        public List<ImageSource> GetHorseAnimation(Color color)
        {
            try
            {
                const int count = 12;

                var image_list = ReadImageList("Resource", "WithOutBorder_", ".png", count);
                var mask_list = ReadImageList("Resource", "mask_", ".png", count);


                return image_list.Select((item, index) => GetImageWithColor(item, mask_list[index], color)).ToList();
            }
            catch (NotImplementedException)
            {

                throw;
            }
            
            
        }

        

        private List<BitmapImage> ReadImageList(string path, string name, string format, int count)
        {
            path = $"D:\\C#\\2 course\\2 sem\\OOP\\Horses\\Images\\{name}";
            List<BitmapImage> list = new List<BitmapImage>();
            for (int i = 0; i < count; i++)
            {
                var uri = path + string.Format("{0:0000}", i) + format;
                var img = new BitmapImage(new Uri(uri));
                list.Add(img);
            }
            return list;
        }

        private ImageSource GetImageWithColor(BitmapImage image, BitmapImage mask, System.Windows.Media.Color color) 
        {
            WriteableBitmap image_bmp = new WriteableBitmap(image);
            WriteableBitmap mask_bmp = new WriteableBitmap(mask);
            WriteableBitmap output_bmp = BitmapFactory.New(image.PixelWidth, image.PixelHeight);

            output_bmp.ForEach((x, y, c) =>
            {
                return MultiplyColors(image_bmp.GetPixel(x, y), color, mask_bmp.GetPixel(x, y).A);   
            });

            return output_bmp;
        }

        private Color MultiplyColors(Color color_1, Color color_2, byte alpha)
        {
            try
            {
                var amount = alpha / 255.0;
                byte r = (byte)(color_2.R * amount + color_1.R * (1 - amount));
                byte g = (byte)(color_2.G * amount + color_1.G * (1 - amount));
                byte b = (byte)(color_2.B * amount + color_1.B * (1 - amount));

                return Color.FromArgb(color_1.A, r, g, b);
            }
            catch (NotImplementedException)
            {

                throw;
            }
            
        }

        private double GetTBToInt(TextBlock text)
        {

            double result = 0;
            text.Dispatcher.Invoke(() =>
            {
                if (!double.TryParse(text.Text, out result))
                {
                    result = 0;
                }
            });
            return result;

        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void StartRace_Click(object sender, RoutedEventArgs e)
        {
            
            animTimer.IsEnabled = true;
            animTimer.Start();
               
            foreach (Horse horse in participants)
            {
                // Create a new DispatcherTimer for each Horse object
                DispatcherTimer timer = new DispatcherTimer();

                horse.HorseTimer = timer;

                // Set the interval and event handler for the timer
                timer.Interval = new TimeSpan(0, 0, 0, 0, 25);
                timer.Tick += async (sender1, e1) => await StartHorseTimer(horse);

                if (horse.IsHorseFinished)
                {
                    timer.Stop();
                }

                Thread th11 = new Thread(() => { checker.Start(); });

                // Start the timer
                th11.Start();
                timer.Start();
            }
        }

        private void BetLower_Click(object sender, RoutedEventArgs e)
        {
            if (GetTBToInt(BetAmountTextBlock) == 10)
            {
                BetLower.IsEnabled = false;
            }
            else BetLower.IsEnabled = true;

            BetHigher.IsEnabled = true;
            BetAmountTextBlock.Text = (GetTBToInt(BetAmountTextBlock) - betStep).ToString();

        }

        private void BetHigher_Click(object sender, RoutedEventArgs e)
        {
            if (GetTBToInt(BetAmountTextBlock) == GetTBToInt(UserBalance) - 5)
            {                
                BetHigher.IsEnabled = false;              
            }
            else BetHigher.IsEnabled = true;

            BetLower.IsEnabled = true;
            BetAmountTextBlock.Text = (GetTBToInt(BetAmountTextBlock) + betStep).ToString();
            
        }

        private void PreviousHorse_Click(object sender, RoutedEventArgs e)
        {
            if (counterForLabels == 1)
            {
                PreviousHorse.IsEnabled = false;
            }
            else PreviousHorse.IsEnabled = true;

            counterForLabels--;
            NextHorse.IsEnabled = true;
            HorseNameLabel.Content = participants[counterForLabels].Name.ToString();
        }

        private void NextHorse_Click(object sender, RoutedEventArgs e)
        {
            if (counterForLabels == participants.Count() - 2)
            {
                NextHorse.IsEnabled = false;
            }
            else NextHorse.IsEnabled = true;

            counterForLabels++;
            PreviousHorse.IsEnabled = true;
            HorseNameLabel.Content = participants[counterForLabels].Name.ToString();
        }

        private void PlaceHorses_Click(object sender, RoutedEventArgs e)
        {
            PlaceHorse();           
        }

        private void PlaceHorse()
        {
            Dispatcher.Invoke(() =>
            {
                
                foreach (var item in participants)
                {
                    canv.Children.Remove(item.AnimationControl);
                }
                participants.Clear();
                horses.Clear();
                double startRend = 100;
                double imageShift = 210 / (GetTBToInt(AmountOfParticipants) * 2);

                for (int i = 0; i < GetTBToInt(AmountOfParticipants); i++)
                {
                    Horse horse = new Horse(horseNames[i], Color.FromArgb(8, (byte)rnd.Next(256), (byte)rnd.Next(256), (byte)rnd.Next(256)));
                    horse.ListOfFrames = GetHorseAnimation(horse.HorseColor);
                    participants.Add(horse);
                    startRend += imageShift;
                    Image image = new Image();
                    image.Width = 200;
                    image.Height = 200;
                    Canvas.SetTop(image, startRend);
                    canv.Children.Add(image);
                    image.Source = participants[i].ListOfFrames[0];
                    horse.AnimationControl = image;
                    DispatcherTimer horseTimer = new DispatcherTimer();
                    horseTimer.Interval = new TimeSpan(0, 0, 0, 0, 25);
                    horse.Canv = canv;
                    horse.HorseTimer = horseTimer;
                    horse.IsHorseFinished = false;
                    horse.IsFirst = false;
                    horse.IsRead = false;
                    
                    
                }

                finishCounter = 0;
                counterForLabels = 0;
                PreviousHorse.IsEnabled = false;
                isFirstHorse = false;
                HorseNameLabel.Content = participants[0].Name.ToString();
                RaceData.ItemsSource = participants;
            });

        }

        private void horseTimer_Tick(object sender, EventArgs e)
        {
            
        }

        private void LessPart_Click(object sender, RoutedEventArgs e)
        {
            if (GetTBToInt(AmountOfParticipants) == 2)
            {
                LessParticipantsButton.IsEnabled = false;
            }
            else LessParticipantsButton.IsEnabled = true;

            amountOfParticipants--;
            MoreParticipantsButton.IsEnabled = true;
            AmountOfParticipants.Text = amountOfParticipants.ToString();
        }

        private void MorePart_Click(object sender, RoutedEventArgs e)
        {
            if (GetTBToInt(AmountOfParticipants) == 9)
            {
                MoreParticipantsButton.IsEnabled = false;
            }
            else MoreParticipantsButton.IsEnabled = true;

            amountOfParticipants++;
            LessParticipantsButton.IsEnabled = true;
            AmountOfParticipants.Text = amountOfParticipants.ToString();
        }

        private void PlaceBet_Click(object sender, RoutedEventArgs e)
        {
            currentBet = GetTBToInt(BetAmountTextBlock);
            nameIsBettingTo = HorseNameLabel.Content.ToString();
            PlaceBetButton.IsEnabled = false;
            PlacedToTB.Text = nameIsBettingTo.ToString();
        }

        private void Test_Click(object sender, RoutedEventArgs e)
        {
            
        }

        async Task StartHorseTimer(Horse horse)
        {
            // Wait for the timer interval
            await Task.Delay(horse.HorseTimer.Interval);

            // Call the event handler for the Horse object
            horse.HandleTimerEvent();
            
        }

    }


    
    
}
