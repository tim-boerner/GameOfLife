using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace GameOfLife
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private System.Windows.Threading.DispatcherTimer dispatcherTimer;  //Timer 

        #region PuplicProperties

        public Collection<Cell> CurrentCells = new Collection<Cell> { };
        public Collection<Cell> NextCells;
        public ObservableCollection<string> Structures = new ObservableCollection<string> { };


        public int Step
        {
            get { return _step; }
            set { _step = value; this.OnPropertyChanged("Step"); }
        }
        private int _step;

        public string MouseCoord
        {
            get { return _mouseCoord; }
            set { _mouseCoord = value; this.OnPropertyChanged("MouseCoord"); }
        }
        private string _mouseCoord;

        public int Speed
        {
            get { return _speed; }
            set { _speed = value; this.OnPropertyChanged("Speed"); }
        }
        private int _speed;

        public int Population
        {
            get { return _population; }
            set { _population = value; this.OnPropertyChanged("Population"); }
        }
        private int _population;

        public string CurrentStructure
        {
            get { return _currentStructure; }
            set { _currentStructure = value; this.OnPropertyChanged("CurrentStructure"); }
        }
        private string _currentStructure = "";

        public int WorldSize
        {
            get { return _worldSize; }
            set { _worldSize = value; this.OnPropertyChanged("WorldSize"); }
        }
        private int _worldSize;

        #endregion PuplicProperties

        public MainWindow()
        {
            //Set start Values
            Step = 0;
            Speed = 8;
            Population = 30;
            WorldSize = 100;

            this.DataContext = this; //nessesary for working binding

            InitializeComponent();

            //Fill Structures
            Structures.Add("Point");
            Structures.Add("Light-Weight Spaceship");
            Structures.Add("Middle-Weight Spaceship");
            Structures.Add("Heavy-Weight Spaceship");
            Structures.Add("Gosper Glider Gun");
            CurrentStructure = Structures[0];

            //Generate World
            generateWorld(WorldSize);

        }

        #region Binding
        //===================Binding===============
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }
        //=============================================
        #endregion Binding

        #region ClickEvents

        private void gridWorld_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) //If Grid Backround is null, event isn’t triggered
        {

            Point PointInGrid = Mouse.GetPosition(gridWorld);

            int X = (int)(PointInGrid.X / (gridWorld.ActualWidth / WorldSize)); //double wird abgeschnitten,  Mit convertInt wird gerundet 
            int Y = (int)(PointInGrid.Y / (gridWorld.ActualHeight / WorldSize));

            int[,] arrayData; //{Y,X}
            switch (CurrentStructure)
            {
                case "Point":
                    arrayData = new int[,]
                    {
                        {0,0 },
                    };
                    break;

                case "Light-Weight Spaceship":
                    arrayData = new int[,]
                    {
                        {0,0 },
                        {0,1 },
                        {0,2 },
                        {0,3 },
                        {1,-1 },
                        {3,-1 },
                        {1,3 },
                        {2,3 },
                        {3,2 },
                    };
                    break;

                case "Middle-Weight Spaceship":

                    arrayData = new int[,]
                    {
                        {0,0 },
                        {0,1 },
                        {0,2 },
                        {0,3 },
                        {0,4 },
                        {1,-1 },
                        {3,-1 },
                        {1,4 },
                        {2,4 },
                        {3,3 },
                        {4,1 },
                    };
                    break;

                case "Heavy-Weight Spaceship":

                    arrayData = new int[,]
                    {
                        {0,0 },
                        {0,1 },
                        {0,2 },
                        {0,3 },
                        {0,4 },
                        {0,5 },
                        {1,-1 },
                        {3,-1 },
                        {1,5 },
                        {2,5 },
                        {3,4 },
                        {4,1 },
                        {4,2 },
                    };
                    break;

                case "Gosper Glider Gun":
                    arrayData = new int[,]
                    {
                        {-5,7 },
                        {-4,5 },{-4,7 },
                        {-3,-5 },{-3,-4 },{-3,3 },{-3,4 },{-3,17 },{-3,18 },
                        {-2,-6 },{-2,-2 },{-2,3 },{-2,4 },{-2,17 },{-2,18 },
                        {-1,-17 },{-1,-16 },{-1,-7 },{-1,-1 },{-1,3 },{-1,4 },
                        {0,-17 },{0,-16 },{0,-7 },{0,-3 },{0,-1 },{0,0 },{0,5 },{0,7 },
                        {1,-7 },{1,-1 },{1,7 },
                        {2,-6 },{2,-2 },
                        {3,-5 },{3,-4 },
                    };
                    break;

                default:
                    arrayData = new int[,]
                    {
                        {0,0 },
                    };
                    break;


            }

            gridWorldInsertPreview.Children.Clear();
            for (int i = 0; i < arrayData.Length / 2; i++)
            {
                CurrentCells[WorldSize * (Y + arrayData[i, 0]) + (X + arrayData[i, 1])].IsAlive = true;
            }

            drawPoints();

        }

        private void gridWorld_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point PointInGrid = Mouse.GetPosition(gridWorld);

            int X = (int)(PointInGrid.X / (gridWorld.ActualWidth / WorldSize)); //double wird abgeschnitten,  Mit convertInt wird gerundet 
            int Y = (int)(PointInGrid.Y / (gridWorld.ActualHeight / WorldSize));

            CurrentCells[WorldSize * Y + X].IsAlive = false;

            drawPoints();
        }

        private void buttonFillWorld_Click(object sender, RoutedEventArgs e)
        {
            CurrentCells.Clear();
            Step = 0;

            Random gen = new Random();
            for (int y = 0; y < WorldSize; y++)
            {
                for (int x = 0; x < WorldSize; x++)
                {
                    bool random = gen.Next(100) < Population ? true : false;

                    CurrentCells.Add(new Cell { XValue = x, YValue = y, IsAlive = random });
                }
            }

            int CellNumber = 0;

            drawPoints();


            Console.WriteLine(CellNumber.ToString());


        }

        private void buttonStart_Click(object sender, RoutedEventArgs e)
        {
            dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);

            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, (int)(810 * Math.Exp(-0.43 * Speed))); //set time here (days, hours, minutes, seconds, milliseconds)
            dispatcherTimer.Start();

            sliderWorldSize.IsEnabled = false;
        }
        //What to do on Timer_Tick
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            generateNextCells();
            drawPoints();
            Step++;
        }

        private void buttonStop_Click(object sender, RoutedEventArgs e)
        {
            dispatcherTimer.Stop();

            sliderWorldSize.IsEnabled = true;
        }

        private void buttonNext_Click(object sender, RoutedEventArgs e)
        {
            generateNextCells();
            drawPoints();
            Step++;
        }

        private void ButtonKillAll_Click(object sender, RoutedEventArgs e)
        {
            CurrentCells.Clear();
            Step = 0;

            for (int y = 0; y < WorldSize; y++)
            {
                for (int x = 0; x < WorldSize; x++)
                {
                    CurrentCells.Add(new Cell { XValue = x, YValue = y, IsAlive = false });
                }
            }

            drawPoints();

        }

        #endregion ClickEvents

        #region WorldLogic
        private int getSurroundingCells(int X, int Y)
        {
            int SurroundingCells = 0;

            if (X == 0 || X >= (WorldSize - 1) || Y == 0 || Y >= (WorldSize - 1)) { return SurroundingCells; }
            else
            {
                //+ + +
                //* x *
                //* * *
                if (CurrentCells[WorldSize * (Y - 1) + X - 1].IsAlive == true)
                {
                    SurroundingCells++;
                }

                if (CurrentCells[WorldSize * (Y - 1) + X].IsAlive == true)
                {
                    SurroundingCells++;
                }

                if (CurrentCells[WorldSize * (Y - 1) + X + 1].IsAlive == true)
                {
                    SurroundingCells++;
                }


                //* * *
                //+ x +
                //* * *
                if (CurrentCells[WorldSize * Y + X - 1].IsAlive == true)
                {
                    SurroundingCells++;
                }

                if (CurrentCells[WorldSize * Y + X + 1].IsAlive == true)
                {
                    SurroundingCells++;
                }


                //* * *
                //* x *
                //+ + +
                if (CurrentCells[WorldSize * (Y + 1) + X - 1].IsAlive == true)
                {
                    SurroundingCells++;
                }

                if (CurrentCells[WorldSize * (Y + 1) + X].IsAlive == true)
                {
                    SurroundingCells++;
                }

                if (CurrentCells[WorldSize * (Y + 1) + X + 1].IsAlive == true)
                {
                    SurroundingCells++;
                }
            }
            return SurroundingCells;
        }

        private void generateNextCells()
        {

            NextCells = new Collection<Cell> { };

            for (int y = 0; y < WorldSize; y++)
            {
                for (int x = 0; x < WorldSize; x++)
                {
                    NextCells.Add(new Cell { XValue = x, YValue = y, IsAlive = false });
                }
            }

            for (int y = 0; y < WorldSize; y++)
            {
                for (int x = 0; x < WorldSize; x++)
                {

                    int SurroundingCells = getSurroundingCells(x, y);
                    bool Alive = CurrentCells[WorldSize * y + x].IsAlive;

                    if (Alive == false & SurroundingCells == 3)
                    {
                        NextCells[WorldSize * y + x].IsAlive = true;

                    }


                    if (Alive == true)
                    {
                        if (SurroundingCells == 2 | SurroundingCells == 3)
                        {
                            NextCells[WorldSize * y + x].IsAlive = true;
                        }
                    }
                }
            }
            CurrentCells = NextCells;
        }

        //Generate World
        private void generateWorld(int WorldSize)
        {
            for (int y = 0; y < WorldSize; y++)
            {
                for (int x = 0; x < WorldSize; x++)
                {
                    CurrentCells.Add(new Cell { XValue = x, YValue = y, IsAlive = false });
                }
            }
        }
        #endregion WorldLogic

        #region Drawing


        private void drawGridLines(Grid grid, int X, int Y)
        {
            for (int i = 0; i < X; i++)
            {
                Line gridLineVertical = new Line();
                gridLineVertical.Stroke = Brushes.DarkGray;

                gridLineVertical.X1 = i * gridWorldLines.ActualWidth / X;
                gridLineVertical.Y1 = 0;

                gridLineVertical.X2 = i * gridWorldLines.ActualWidth / X;
                gridLineVertical.Y2 = gridWorldLines.ActualHeight;
                grid.Children.Add(gridLineVertical);
            }

            for (int i = 0; i < Y; i++)
            {

                Line gridLineHorizontal = new Line();
                gridLineHorizontal.Stroke = Brushes.DarkGray;

                gridLineHorizontal.Y1 = i * gridWorldLines.ActualHeight / Y;
                gridLineHorizontal.X1 = 0;

                gridLineHorizontal.Y2 = i * gridWorldLines.ActualHeight / Y;
                gridLineHorizontal.X2 = gridWorldLines.ActualWidth;
                grid.Children.Add(gridLineHorizontal);

            }
        }

        private void drawPoints()
        {
            gridWorld.Children.Clear();

            int CellNumber = 0;
            for (int i = 0; i < CurrentCells.Count; i++)
            {

                if (CurrentCells[i].IsAlive == true)
                {
                    Rectangle myRect = new Rectangle { Width = gridWorld.ActualWidth / WorldSize, Height = gridWorld.ActualHeight / WorldSize };
                    myRect.Fill = Brushes.Green;
                    myRect.Margin = new Thickness(CurrentCells[i].XValue * gridWorld.ActualWidth / WorldSize, CurrentCells[i].YValue * gridWorld.ActualHeight / WorldSize, 0, 0);
                    myRect.HorizontalAlignment = HorizontalAlignment.Left;
                    myRect.VerticalAlignment = VerticalAlignment.Top;

                    gridWorld.Children.Add(myRect);

                    CellNumber++;
                }


            }
            Console.WriteLine(CellNumber.ToString());
        }
        #endregion Drawing

        #region MouseEvents
        private void gridWorld_MouseEnter(object sender, MouseEventArgs e)
        {
            Point PointInGrid = Mouse.GetPosition(gridWorld);

            int X = (int)(PointInGrid.X / (gridWorld.ActualWidth / WorldSize)); //double wird abgeschnitten,  Mit convertInt wird gerundet 
            int Y = (int)(PointInGrid.Y / (gridWorld.ActualHeight / WorldSize));

            MouseCoord = "X = " + X + "   Y = " + Y;

            int[,] arrayData; //{Y,X}
            switch (CurrentStructure)
            {
                case "Point":
                    arrayData = new int[,]
                    {
                        {0,0 },
                    };
                    break;

                case "Light-Weight Spaceship":
                    arrayData = new int[,]
                    {
                        {0,0 },
                        {0,1 },
                        {0,2 },
                        {0,3 },
                        {1,-1 },
                        {3,-1 },
                        {1,3 },
                        {2,3 },
                        {3,2 },
                    };
                    break;

                case "Middle-Weight Spaceship":

                    arrayData = new int[,]
                    {
                        {0,0 },
                        {0,1 },
                        {0,2 },
                        {0,3 },
                        {0,4 },
                        {1,-1 },
                        {3,-1 },
                        {1,4 },
                        {2,4 },
                        {3,3 },
                        {4,1 },
                    };
                    break;

                case "Heavy-Weight Spaceship":

                    arrayData = new int[,]
                    {
                        {0,0 },
                        {0,1 },
                        {0,2 },
                        {0,3 },
                        {0,4 },
                        {0,5 },
                        {1,-1 },
                        {3,-1 },
                        {1,5 },
                        {2,5 },
                        {3,4 },
                        {4,1 },
                        {4,2 },
                    };
                    break;

                case "Gosper Glider Gun":
                    arrayData = new int[,]
                    {
                        {-5,7 },
                        {-4,5 },{-4,7 },
                        {-3,-5 },{-3,-4 },{-3,3 },{-3,4 },{-3,17 },{-3,18 },
                        {-2,-6 },{-2,-2 },{-2,3 },{-2,4 },{-2,17 },{-2,18 },
                        {-1,-17 },{-1,-16 },{-1,-7 },{-1,-1 },{-1,3 },{-1,4 },
                        {0,-17 },{0,-16 },{0,-7 },{0,-3 },{0,-1 },{0,0 },{0,5 },{0,7 },
                        {1,-7 },{1,-1 },{1,7 },
                        {2,-6 },{2,-2 },
                        {3,-5 },{3,-4 },
                    };
                    break;

                default:
                    arrayData = new int[,]
                    {
                        {0,0 },
                    };
                    break;


            }



            gridWorldInsertPreview.Children.Clear();
            for (int i = 0; i < arrayData.Length / 2; i++)
            {

                Rectangle myRect = new Rectangle { Width = gridWorld.ActualWidth / WorldSize, Height = gridWorld.ActualHeight / WorldSize };
                myRect.Stroke = Brushes.Red;
                myRect.Margin = new Thickness((X + arrayData[i, 1]) * gridWorld.ActualWidth / WorldSize, (Y + arrayData[i, 0]) * gridWorld.ActualHeight / WorldSize, 0, 0);
                myRect.HorizontalAlignment = HorizontalAlignment.Left;
                myRect.VerticalAlignment = VerticalAlignment.Top;

                gridWorldInsertPreview.Children.Add(myRect);

            }

        }

        private void gridWorldMouse_MouseLeave(object sender, MouseEventArgs e)
        {
            gridWorldInsertPreview.Children.Clear();
        }

        private void sliderWorldSize_LostMouseCapture(object sender, MouseEventArgs e)
        {
            gridWorldLines.Children.Clear();
            drawGridLines(gridWorldLines, WorldSize, WorldSize);

            CurrentCells.Clear();
            generateWorld(WorldSize);
            drawPoints();
            Step = 0;

        }
        #endregion MouseEvents



        //Set Source for Combobox
        private void comboBox_Loaded(object sender, RoutedEventArgs e)
        {
            var combo = sender as ComboBox;
            combo.ItemsSource = Structures;
            combo.SelectedIndex = 0;
        }

        //Draw Lines when loaded
        private void gridWorldLines_Loaded(object sender, RoutedEventArgs e)
        {
            drawGridLines(gridWorldLines, WorldSize, WorldSize);
        }

        //Refresh on Window Size changed
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            gridWorldLines.Children.Clear();
            drawGridLines(gridWorldLines, WorldSize, WorldSize);
            drawPoints();
        }

        //Change Speed
        private void sliderSpeed_LostMouseCapture(object sender, MouseEventArgs e)
        {
            try
            {
                dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, (int)(810 * Math.Exp(-0.43 * Speed)));
            }
            catch { }
        }
    }

    public class Cell
    {
        public int XValue { set; get; }
        public int YValue { set; get; }
        public bool IsAlive { set; get; }
    }
}
