using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hwavmvid.Rouletteshared.Events;
using Hwavmvid.Rouletteshared.Items;
using Hwavmvid.Rouletteshared.Enums;
using System.Linq;

namespace Hwavmvid.Roulette
{

    public class RouletteService
    {

        private IJSRuntime jsruntime;

        public event Action OnPlayNewRouletteGame;
        public event Action OnStopRouletteGame;
        public event Action<RouletteEvent> OnWinItemDetected;
        public event Action UupdateUI;

        public RouletteGameStatus GameStatus { get; set; } = RouletteGameStatus.StartNewGame;

        public RouletteService(IJSRuntime jsRuntime)
        {
            this.jsruntime = jsRuntime;
        }

        public void StopRouletteGame()
        {
            this.OnStopRouletteGame?.Invoke();
        }
        public void ExposeWinItem(RouletteNumber item)
        {
            this.OnWinItemDetected?.Invoke(new RouletteEvent() { WinItem = item });
        }

        public RouletteMap Map { get; set; } = null;
        public List<RouletteNumber> NumberItems = new List<RouletteNumber>();

        public const double containerwidth = rows * griditemwidth;
        public const double containerheight = cols * griditemheight;

        public const double griditemwidth = 10;
        public const double griditemheight = 10;

        private const int rows = 40;
        private const int cols = 40;

        private const double circlecalcdiff = 0.5;

        private const int totalnumbers = 38;
        private byte[] totalnumbersvalues = new byte[totalnumbers]
        {
            13,36,1,24,19,30,21,14,37,28,
            7,12,35,20,33,4,25,18,27,2,
            9,16,23,6,15,26,11,0,17,8,
            31,22,5,34,29,10,3,32
        };

        public bool loading { get; set; } = true;
        public RouletteNumber winitem { get; set; }

        public string Black { get; set; } = "black";
        public string Red { get; set; } = "red";
        public string Carpetgreen { get; set; } = "rgba(33,109,70,0.8)"; // #216d46
        public string Transparent { get; set; } = "transparent";

        public void Play_Clicked()
        {
            this.OnPlayNewRouletteGame?.Invoke();

            this.winitem = null;
            this.roulettecircleradius = 17.5;
            this.GameStatus = RouletteGameStatus.Playing;
            this.UupdateUI?.Invoke();

            this.InitRouletteBall();
            this.RunRouletteBall();
        }

        public RouletteMap GetRouletteMap()
        {

            RouletteMap map = new RouletteMap();
            for (var r = 1; r <= rows; r++)
            {

                RouletteRow row = new RouletteRow();
                row.RowId = r;
                map.Rows.Add(row);

                for (var c = 1; c <= cols; c++)
                {
                    RouletteColumn column = new RouletteColumn();
                    column.ColumnId = c;
                    column.RowId = r;
                    map.Columns.Add(column);
                }
            }

            return map;
        }

        private const double numberscircleradius = 10.0;
        private const double numberscirclemiddle_x = 20.5;
        private const double numberscirclemiddle_y = 19.5;
        private const double numberscirclesegmentslength = totalnumbers;

        public RouletteCarpet carpet { get; set; } = new RouletteCarpet() { Id = Guid.NewGuid().ToString() };
        public void InitRouletteCarpet()
        {
            foreach (var row in this.Map.Rows)
            {
                foreach (var container in this.Map.Columns.Where(item => item.RowId == row.RowId).Select((item, index) => new { item = item, index = index }))
                {
                    this.carpet.RowId = row.RowId;
                    this.carpet.ColumnId = container.index + 1;
                    this.carpet.ZIndex = 1;
                    this.carpet.Opacity = 1;
                    this.carpet.BackgroundColor = this.Carpetgreen;
                    this.carpet.Rotation = 0;
                    this.carpet.ImageWidth = 0;
                    this.carpet.ImageHeight = 0;
                    this.carpet.ImageUrl = string.Empty;
                    this.carpet.ImageUrlExtension = string.Empty;
                    this.carpet.Value = 0;

                    this.AddRouletteItem(carpet.RowId, carpet.ColumnId, this.carpet);
                }
            }
        }

        public RouletteBallRaceway ballraceway { get; set; } = new RouletteBallRaceway() { Id = Guid.NewGuid().ToString() };
        public void InitRouletteRaceway()
        {
            this.ballraceway.RowId = 1;
            this.ballraceway.ColumnId = 1;
            this.ballraceway.ZIndex = 100;
            this.ballraceway.Opacity = 1;
            this.ballraceway.BackgroundColor = this.Transparent;
            this.ballraceway.Rotation = 0;
            this.ballraceway.ImageWidth = containerwidth;
            this.ballraceway.ImageHeight = containerheight;
            this.ballraceway.ImageUrl = "ballraceway2";
            this.ballraceway.ImageUrlExtension = ".png";
            this.ballraceway.Value = 0;

            this.AddRouletteItem(this.ballraceway.RowId, this.ballraceway.ColumnId, this.ballraceway);
        }

        public RouletteBall ball { get; set; } = new RouletteBall() { Id = Guid.NewGuid().ToString() };
        public void InitRouletteBall()
        {
            this.ball.RowId = 2;
            this.ball.ColumnId = 2;
            this.ball.ZIndex = 1000;
            this.ball.Opacity = 1;
            this.ball.BackgroundColor = this.Transparent;
            this.ball.Rotation = 0;
            this.ball.ImageWidth = 10;
            this.ball.ImageHeight = 10;
            this.ball.ImageUrl = "rouletteball";
            this.ball.ImageUrlExtension = ".svg";
            this.ball.Value = 0;
        }
        public async void RunRouletteBall()
        {
            Random rndm = new Random();
            int ballpower = rndm.Next(1900 / 4, 2698 / 2);
            int delay = 9;
            int i = 0;

            while (this.GameStatus == RouletteGameStatus.Playing)
            {
                i++;
                if (ballpower - i == 400)
                {
                    this.roulettecircleradius = 16.5;
                    delay = 10;
                }
                if (ballpower - i == 300)
                {
                    this.roulettecircleradius = 15.5;
                    delay = 11;
                }
                if (ballpower - i == 200)
                {
                    this.roulettecircleradius = 14.5;
                    delay = 12;
                }
                if (ballpower - i == 100)
                {
                    this.roulettecircleradius = 13.5;
                    delay = 13;
                }
                if (ballpower - i == 64)
                {
                    this.roulettecircleradius = 12.5;
                    delay = 14;
                }
                if (ballpower - i == 37)
                {
                    this.roulettecircleradius = 11.5;
                    delay = 17;
                }
                if (ballpower - i == 17)
                {
                    this.roulettecircleradius = 10.5;
                    delay = 20;
                }
                if (ballpower == i)
                    this.roulettecircleradius = 10.0;

                this.RemoveRouletteItem(this.ball.RowId, this.ball.ColumnId, this.ball);
                this.GetNextCircleCoor(this.ball, RouletteDirection.right);
                this.AddRouletteItem(this.ball.RowId, this.ball.ColumnId, this.ball);

                if (i % 2 == 0)
                    this.UupdateUI?.Invoke();

                await Task.Delay(Convert.ToInt32(delay * 2));

                if (ballpower == i)
                {
                    this.GameStatus = RouletteGameStatus.ResultTable;

                    try
                    {
                        var column = this.Map.Columns.FirstOrDefault(item => item.RowId == this.ball.RowId && item.ColumnId == this.ball.ColumnId);
                        var columnitem = column.Items.FirstOrDefault(item => item.GetType() == typeof(RouletteNumber));
                        if (columnitem != null)
                        {
                            this.winitem = columnitem as RouletteNumber;
                            this.ExposeWinItem(winitem);
                        }
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine(exception.Message);
                    }

                    this.UupdateUI?.Invoke();
                }
            }
        }

        public List<RouletteNumber> GetRouletteNumbers()
        {

            List<RouletteNumber> numbers = new List<RouletteNumber>();
            for (var i = 1; i <= totalnumbers; i++)
            {

                RouletteNumber number = new RouletteNumber();

                var value = this.totalnumbersvalues[(i - 1)];
                number.Value = value;

                if (number.Value == 0 || number.Value == 37)
                    number.BackgroundColor = this.Carpetgreen;

                else if (number.Value % 2 == 0)
                    number.BackgroundColor = this.Black;

                else if (number.Value % 2 == 1)
                    number.BackgroundColor = this.Red;

                double coor_x = numberscirclemiddle_x + (double)Math.Cos(2 * Math.PI * (i + circlecalcdiff) / numberscirclesegmentslength) * numberscircleradius;
                double coor_y = numberscirclemiddle_y + (double)Math.Sin(2 * Math.PI * (i + circlecalcdiff) / numberscirclesegmentslength) * numberscircleradius;

                number.RowId = Convert.ToInt32(coor_y);
                number.ColumnId = Convert.ToInt32(coor_x);
                number.Id = Guid.NewGuid().ToString();
                number.ZIndex = -1;
                number.Rotation = 0;
                number.Opacity = 1;
                number.ImageWidth = 0;
                number.ImageHeight = 0;
                number.ImageUrl = string.Empty;
                number.ImageUrlExtension = string.Empty;
                numbers.Add(number);

                this.AddRouletteItem(number.RowId, number.ColumnId, number);
            }

            return numbers;
        }
        public RouletteNumbers numbers { get; set; } = new RouletteNumbers() { Id = Guid.NewGuid().ToString() };
        public void InitRouletteNumbers()
        {
            this.numbers.RowId = 1;
            this.numbers.ColumnId = 1;
            this.numbers.ZIndex = 100;
            this.numbers.Opacity = 1;
            this.numbers.BackgroundColor = this.Transparent;
            this.numbers.Rotation = 0;
            this.numbers.ImageWidth = containerwidth;
            this.numbers.ImageHeight = containerheight;
            this.numbers.ImageUrl = "ballnumbers2";
            this.numbers.ImageUrlExtension = ".png";
            this.numbers.Value = 0;
            this.AddRouletteItem(this.numbers.RowId, this.numbers.ColumnId, this.numbers);
        }
        public async void RunRouletteNumbers()
        {

            double degresult = 360;
            double degstep = 360 / Convert.ToDouble(totalnumbers);
            int i = totalnumbers;

            while (true)
            {

                if (i == 0)
                    i = totalnumbers;

                degresult -= degstep;
                if (i == totalnumbers)
                    degresult = 360;

                this.RemoveRouletteItem(this.numbers.RowId, this.numbers.ColumnId, this.numbers);
                this.numbers.Rotation = Convert.ToInt32(degresult);
                this.AddRouletteItem(this.numbers.RowId, this.numbers.ColumnId, this.numbers);

                var tempitems = this.NumberItems;
                var tempitem = this.NumberItems.FirstOrDefault();
                tempitems.Remove(tempitem); tempitems.Add(tempitem);
                this.NumberItems = tempitems;

                foreach (var container in this.NumberItems.Select((item, index) => new { item = item, index = index }))
                {
                    this.RemoveRouletteItem(container.item.RowId, container.item.ColumnId, container.item);

                    double coor_x = numberscirclemiddle_x + (double)Math.Cos(2 * Math.PI * (container.index + 2 + circlecalcdiff) / numberscirclesegmentslength) * numberscircleradius;
                    double coor_y = numberscirclemiddle_y + (double)Math.Sin(2 * Math.PI * (container.index + 2 + circlecalcdiff) / numberscirclesegmentslength) * numberscircleradius;

                    container.item.RowId = Convert.ToInt32(coor_y);
                    container.item.ColumnId = Convert.ToInt32(coor_x);

                    this.AddRouletteItem(container.item.RowId, container.item.ColumnId, container.item);
                }

                if (this.GameStatus == RouletteGameStatus.ResultTable && this.winitem != null)
                {
                    var itens = this.NumberItems.FirstOrDefault(item => item.Value == winitem.Value);
                    if (itens != null)
                    {
                        this.RemoveRouletteItem(this.ball.RowId, this.ball.ColumnId, this.ball);

                        this.ball.RowId = itens.RowId;
                        this.ball.ColumnId = itens.ColumnId;

                        this.AddRouletteItem(this.ball.RowId, this.ball.ColumnId, this.ball);
                    }
                }

                if (this.GameStatus != RouletteGameStatus.Playing)
                    this.UupdateUI?.Invoke();

                await Task.Delay(this.GameStatus == RouletteGameStatus.Playing ? 40 : 180);

                i--;
            }
        }

        public void AddRouletteItem(int rowId, int columnId, RouletteItem item)
        {
            var col = this.Map.Columns.FirstOrDefault(item => item.RowId == rowId && item.ColumnId == columnId);
            if (col != null)
            {
                col.Items.Add(item);
            }
        }
        public void RemoveRouletteItem(int rowId, int columnId, RouletteItem item)
        {
            var col = this.Map.Columns.FirstOrDefault(column => column.RowId == rowId && column.ColumnId == columnId);
            if (col != null)
            {
                var founditem = col.Items.FirstOrDefault(colitem => colitem.Id == item.Id);
                if (founditem != null)
                    col.Items.Remove(founditem);
            }
        }

        private double roulettecircleradius { get; set; } = 17.5;

        private const double roulettecirclemiddle_x = 20.5;
        private const double roulettecirclemiddle_y = 19.5;
        private const double roulettecirclesegmentslength = totalnumbers;
        private int roulettecirclecontextsegment { get; set; } = 0;

        public void ResetVariables()
        {
            this.roulettecircleradius = 17.5;
            this.roulettecirclecontextsegment = 0;
        }

        public void GetNextCircleCoor(RouletteItem rouletteitem, RouletteDirection direction)
        {

            if (!(this.roulettecirclecontextsegment < roulettecirclesegmentslength) ||
                !(this.roulettecirclecontextsegment > -(roulettecirclesegmentslength)))
            {
                this.roulettecirclecontextsegment = 0;
            }

            if (direction == RouletteDirection.right)
                this.roulettecirclecontextsegment++;

            if (direction == RouletteDirection.left)
                this.roulettecirclecontextsegment--;

            double coor_x = roulettecirclemiddle_x + (double)Math.Cos(2 * Math.PI * (this.roulettecirclecontextsegment + circlecalcdiff) / roulettecirclesegmentslength) * this.roulettecircleradius;
            double coor_y = roulettecirclemiddle_y + (double)Math.Sin(2 * Math.PI * (this.roulettecirclecontextsegment + circlecalcdiff) / roulettecirclesegmentslength) * this.roulettecircleradius;

            rouletteitem.RowId = Convert.ToInt32(coor_y);
            rouletteitem.ColumnId = Convert.ToInt32(coor_x);
        }

        #region calc circle coor example function

        public void CircleLoop()
        {
            int segmentsLength = 24;
            double middle_x = 0;
            double middle_y = 0;
            double middle_z = 0;
            double radius = 100;

            for (int i = 0; i < segmentsLength; i++)
            {
                double coor_x = middle_x + (double)Math.Cos(2 * Math.PI * i / segmentsLength) * radius;
                double coor_y = middle_y + (double)Math.Sin(2 * Math.PI * i / segmentsLength) * radius;
                double coor_z = middle_z;
            }
        }
        #endregion

    }
}