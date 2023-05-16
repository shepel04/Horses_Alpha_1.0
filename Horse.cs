using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace Horses
{
    partial class Horse
    {
        private string name;
        private Color horseColor;
        private TimeSpan passingTime;
        private List<ImageSource> listOfFrames;
        private Image animationControl;
        private DispatcherTimer timer;
        private int spriteIndex = 0;
        private static Random random = new Random();
        private double speed = random.Next(200, 501) / 100.0;
        private Canvas canvas;
        private double bet = 0;
        private double step = 0.7;
        private double way = 0;
        private double coefficient = random.Next(101, 201) / 100.0;
        private bool isHorseFinished = false;
        private bool isPassingStarted = false;
        private bool isRead = false;
        private bool isFirst = false;
        private bool isPlaced = false;
        DateTime startTime = new DateTime();
        DateTime finishTime = new DateTime();



        public Horse(string name, Color horseСolor) 
        {
            this.name = name;
            this.horseColor = horseСolor;
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(50);
            timer.Tick += OnTimerTick;
        }

        public string Name
        {
            get { return name; }
            private set { name = value; }
        }

        public Color HorseColor 
        {          
            get { return horseColor; }
            private set { horseColor = value; }
        }

        public TimeSpan PassingTime
        {
            get { return passingTime; }
            private set { passingTime = value; }
        }

        public double Bet
        {
            get { return bet; }
            set { bet = value; }
        }

        public double Speed
        { 
            get { return speed; }
            set { speed = value; }
        }

        public double Coefficient
        {
            get { return coefficient; }
            private set { coefficient = value; }
        }

        public List<ImageSource> ListOfFrames
        { 
            get { return listOfFrames; }
            set { listOfFrames = value; }
        }

        public Image AnimationControl
        {
            get { return animationControl; }
            set { animationControl = value; }
        }

        public Canvas Canv
        {
            get { return canvas; }
            set { canvas = value; }
        }

        public DispatcherTimer HorseTimer
        {
            get { return timer; }
            set { timer = value; }
        }

        public void OnTimerTick(object sender, EventArgs e)
        {
            if (spriteIndex >= listOfFrames.Count())
            {
                spriteIndex = 0;
            }
            else 
            {
                animationControl.Source = listOfFrames[spriteIndex];
                spriteIndex++;
            }
            
        }

        public bool IsHorseFinished
        {
            get { return isHorseFinished; }
            set { isHorseFinished = value; }
        }

        public bool IsRead
        {
            get { return isRead; }
            set { isRead = value; }
        }

        public bool IsFirst
        {
            get { return isFirst; }
            set { isFirst = value; }
        }

        public bool IsPlaced
        {
            get { return isPlaced; }
            set { isPlaced = value; }
        }

        public void HandleTimerEvent()
        {
           
            if (!isPassingStarted)
            {
                startTime = DateTime.Now;
                isPassingStarted = true;
            }
            
           
            if (spriteIndex >= listOfFrames.Count())
            {
                spriteIndex = 0;
            }
            else
            {
                animationControl.Source = listOfFrames[spriteIndex];
                spriteIndex++;
            }

            way += step * speed;

            Canvas.SetLeft(animationControl, way);
            double currentX = Canvas.GetLeft(animationControl);

            if (currentX >= 700 && !isHorseFinished)
            {
                finishTime = DateTime.Now;
                passingTime = finishTime - startTime;
                isHorseFinished = true;
                timer.IsEnabled = false;
                timer.Stop();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
