using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AStarFindPath
{
    public partial class Form2 : Form
    {
        Tile[,] Tiles = null;

        Tile StartCell = null;

        Tile EndCell = null;

        string MapString = @"################################################################/" +
                           @"#S                                                             #/" +
                           @"#############################################################  #/" +
                           @"#E##                                                           #/" +
                           @"# ##  ##########################################################/" +
                           @"# ##  ##                                                  ###  #/" +
                           @"# ##  ##  #############################################  ####  #/" +
                           @"# ##      ###                            ##                    #/" +
                           @"# ###########  ####################  ##  ##  ###################/" +
                           @"#          ##  ##         ##         ##  ##  ##         ##     #/" +
                           @"#########  ##      #####  ##    #######  ##  ##  ####   ## ##  #/" +
                           @"#          ##     ######  ##########  #  ##      ##        ##  #/" +
                           @"#  ##########                  ###   ##  ####################  #/" +
                           @"#          #######################  ###                        #/" +
                           @"#######                                   ######################/" +
                           @"################################################################/";

        public Form2()
        {
            InitializeComponent();

            DoubleBuffered = true;

            CreateMap();

            //Tiles = new Tile[40/*x*/, 20/*y*/];

            //for (int row = 0; row < Tiles.GetLength(1); row++)
            //{
            //    for (int col = 0; col < Tiles.GetLength(0); col++)
            //    {
            //        Tiles[col, row] = new Tile()
            //        {
            //            X = col,
            //            Y = row
            //        };
            //    }
            //}
            //Init();
        }

        private void CreateMap()
        {
            string[] lines = MapString.Split("/".ToArray(), StringSplitOptions.RemoveEmptyEntries);

            int rows = lines.Length;
            int cols = 0;
            if (0 < lines.Length) cols = lines[0].Length;

            Tiles = new Tile[cols/*x*/, rows/*y*/];

            for (int row = 0; row < rows; row++)
            {
                //if (string.IsNullOrWhiteSpace(lines[row])) continue;
                for (int col = 0; col < cols; col++)
                {  
                    Tiles[col, row] = new Tile()
                    {
                        X = col,
                        Y = row,   
                    };

                    if (lines[row][col] == 'S')
                    {
                        SetStartCell(Tiles[col, row]);
                    }
                    else if (lines[row][col] == 'E')
                    {
                        SetEndCell(Tiles[col, row]);
                    }

                    if (lines[row][col] == '#') Tiles[col, row].ToggleWall();

                }
            }

        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            for (int col = 0; col < Tiles.GetLength(0); col++)
            {
                for (int row = 0; row < Tiles.GetLength(1); row++)
                {
                    Tiles[col, row].Draw(e.Graphics);
                    Tiles[col, row].DrawInfo(e.Graphics);
                    //  Tiles[col, row].DrawArrow(e.Graphics);
                }
            }

            //if (EndCell?.PrevCell != null)
            //{
            //    EndCell.DrawMark(e.Graphics);
            //    Tile moveCell = GetPrevCell(EndCell);
            //    while (moveCell != null && StartCell?.Equals(moveCell) == false)
            //    {
            //        moveCell.DrawMark(e.Graphics);
            //        moveCell = GetPrevCell(moveCell);
            //    }
            //    StartCell.DrawMark(e.Graphics);
            //}

            if (EndCell?.PrevCell != null)
            {
                EndCell.DrawMark(e.Graphics);
                Tile moveCell = EndCell?.PrevCell ?? null;
                while (moveCell != null && StartCell?.Equals(moveCell) == false)
                {
                    moveCell.DrawMark(e.Graphics);
                    moveCell = moveCell?.PrevCell;
                }
                StartCell.DrawMark(e.Graphics);
            }


            for (int col = 0; col < Tiles.GetLength(0); col++)
            {
                for (int row = 0; row < Tiles.GetLength(1); row++)
                {
                    Tiles[col, row].DrawArrow(e.Graphics);
                }
            }
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            Tile cell = FindCell(PointToClient(MousePosition));
            ToggleWallCells(cell);
        }

        private void ToggleWallCells(Tile cell)
        {
            if (cell != null)
            {
                // MessageBox.Show( ""+ ( 1 + (cell.Y * Cells.GetLength(0)) + cell.X ));
                if (state == 1)
                {
                    SetStartCell(cell);
                }
                else if (state == 9)
                {
                    SetEndCell(cell);
                }
                else
                {
                    if (StartCell == cell) return;
                    if (EndCell == cell) return;

                    if (isWall && cell.IsWall == false) cell.ToggleWall();
                    else if (isWall == false && cell.IsWall == true) cell.ToggleWall();
                }
                state = 0;

                Invalidate();
            }
        }

        private void SetEndCell(Tile cell)
        {
            if (EndCell != null) EndCell.BackgroundColor = Color.White;

            EndCell = cell;

            if (EndCell != null)
            {
                EndCell.BackgroundColor = Color.Yellow;
                EndCell.IsWall = false;
            }
        }

        private void SetStartCell(Tile cell)
        {
            if (StartCell != null) StartCell.BackgroundColor = Color.White;

            StartCell = cell;

            if (StartCell != null)
            {
                StartCell.BackgroundColor = Color.ForestGreen;
                StartCell.IsWall = false;
            }
        }

        bool isMD = false;
        Tile movingCell = null;
        bool isWall = false;

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            isMD = e.Button == MouseButtons.Left;
            Tile cell = FindCell(PointToClient(MousePosition));
            if (cell != movingCell)
            {
                movingCell = cell;
                if (movingCell == StartCell)
                {
                    return;
                }
                else if (movingCell == EndCell)
                {
                    return;
                }
                isWall = !movingCell.IsWall;
                ToggleWallCells(movingCell);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (isMD)
            {
                Tile cell = FindCell(PointToClient(MousePosition));
                if (cell != movingCell)
                {
                    if (movingCell == StartCell)
                    {
                        SetStartCell(cell);
                        movingCell = cell;
                        Invalidate();
                        return;
                    }
                    else if (movingCell == EndCell)
                    {
                        SetEndCell(cell);
                        movingCell = cell;
                        Invalidate();
                        return;
                    }

                    movingCell = cell;
                    ToggleWallCells(movingCell);
                }
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            isMD = false;

            ToggleWallCells(movingCell);
            movingCell = null;
            step = 0;
        }
         
        int state = 0;
        private void startSetButton_Click(object sender, EventArgs e)
        {
            // tlwkr
            state = 1;
        }

        private void endSetButton_Click(object sender, EventArgs e)
        {
            state = 9;
        }

        private void Init()
        {
            step = 0;
            ClearF(true);

            StartCell = Tiles[1, 3];
            StartCell.BackgroundColor = Color.ForestGreen;
            StartCell.PrevCell = null;
            StartCell.IsWall = false;

            EndCell = Tiles[5, 4];
            EndCell.BackgroundColor = Color.Yellow;
            EndCell.PrevCell = null;
            EndCell.IsWall = false;

            Tiles[3, 2].ToggleWall();
            Tiles[3, 3].ToggleWall();
            Tiles[3, 4].ToggleWall();
            Tiles[3, 5].ToggleWall();

            Invalidate();
        }

        private void ClearF(bool IsWallClear = false)
        {
            for (int col = 0; col < Tiles.GetLength(0); col++)
            {
                for (int row = 0; row < Tiles.GetLength(1); row++)
                {
                    Tiles[col, row].InitF();
                    if (IsWallClear)
                    {
                        Tiles[col, row].IsWall = true;
                        Tiles[col, row].ToggleWall();
                    }
                }
            }
        }
         
        List<Tile> Openeds = new List<Tile>();
        List<Tile> Closeds = new List<Tile>();
        private void button1_Click(object sender, EventArgs e)
        {
            // 검색!
            // 찾기.
            if (StartCell == null)
            {
                MessageBox.Show("시작지점 확인!");
                return;
            }
            if (EndCell == null)
            {
                MessageBox.Show("종료지점 확인!");
                return;
            }
            Openeds.Clear();
            Closeds.Clear();

            StartCell.PrevCell = null;
            EndCell.PrevCell = null;

            Openeds.Add(StartCell);

            ClearF();

            bool IsFinding = true;

            while (IsFinding)
            {
                Tile[] TargetCells = Openeds.ToArray();

                Tile currentCell = null;
                foreach (var cell in TargetCells)
                {
                    currentCell = cell;

                    Openeds.Remove(currentCell);
                    Closeds.Add(currentCell);

                    Tile[] roundCells = GetAroundCells(currentCell.X, currentCell.Y);

                    Tile prevCell = currentCell;
                    foreach (var rcell in roundCells)
                    {
                        if (prevCell.F == rcell.F && rcell.H < prevCell.H)
                        {
                            prevCell = rcell;
                        }
                    }

                    foreach (var rcell in roundCells)
                    {
                        if (rcell.IsWall) continue;            // 벽
                        if (0 < rcell.F) continue;              // 설정된 것
                        if (Closeds.Contains(rcell)) continue;  // 검색이 필요없는 셀

                        if (!Openeds.Contains(rcell))
                        {
                            Openeds.Add(rcell);
                        }
                        rcell.PrevCell = prevCell;
                        rcell.CalcF(EndCell);//.F = currentCell.F + 1;
                    }
                }

                if (Openeds.Count <= 0 || EndCell.PrevCell != null)
                {
                    IsFinding = false;
                }
            }

            AdjustPath();

            Invalidate();
        }

        int step = 0;
        private void button2_Click(object sender, EventArgs e)
        {
            // 검색!
            // 찾기.
            step++;

            if (StartCell == null)
            {
                MessageBox.Show("시작지점 확인!");
                return;
            }
            if (EndCell == null)
            {
                MessageBox.Show("종료지점 확인!");
                return;
            }
            Openeds.Clear();
            Closeds.Clear();

            StartCell.PrevCell = null;
            EndCell.PrevCell = null;

            Openeds.Add(StartCell);

            ClearF();

            int loop = step;

            while (0 < loop)
            {
                Tile[] TargetCells = Openeds.ToArray();

                Tile currentCell = null;
                foreach (var cell in TargetCells)
                {
                    currentCell = cell;

                    Openeds.Remove(currentCell);
                    Closeds.Add(currentCell);

                    Tile[] roundCells = GetAroundCells(currentCell.X, currentCell.Y);

                    Tile prevCell = currentCell;
                    foreach (var rcell in roundCells)
                    {
                        if (prevCell.F == rcell.F && rcell.H < prevCell.H)
                        {
                            prevCell = rcell;
                        }
                    }

                    foreach (var rcell in roundCells)
                    {
                        if (rcell.IsWall) continue;            // 벽
                        if (0 < rcell.F) continue;              // 설정된 것
                        if (Closeds.Contains(rcell)) continue;  // 검색이 필요없는 셀

                        if (!Openeds.Contains(rcell))
                        {
                            Openeds.Add(rcell);
                        }
                        rcell.PrevCell = prevCell;
                        rcell.CalcF(EndCell);//.F = currentCell.F + 1;                      
                    }
                }

                loop--;
                if (Openeds.Count <= 0 || EndCell.PrevCell != null)
                {
                    loop = 0;
                }
            }

            AdjustPath();

            Invalidate();

        }

        private void AdjustPath()
        {
            if (EndCell?.PrevCell != null)
            { 
                Tile moveCell = GetPrevCell(EndCell);
                EndCell.PrevCell = moveCell;
                while (moveCell != null && StartCell?.Equals(moveCell) == false)
                { 
                    Tile tempCell = GetPrevCell(moveCell);
                    moveCell.PrevCell = tempCell;
                    moveCell = tempCell;
                }
            }
        }

        private Tile GetPrevCell(Tile cell)
        {
            Tile prevTile = null;

            switch (cell.Direction)
            {
                default:
                case CellDirection.NONE:
                    break;
                case CellDirection.LEFT:
                    prevTile = FindCell(cell.X + 1, cell.Y);
                    break;
                case CellDirection.TOP:
                    prevTile = FindCell(cell.X, cell.Y + 1);
                    break;
                case CellDirection.RIGHT:
                    prevTile = FindCell(cell.X - 1, cell.Y);
                    break;
                case CellDirection.BOTTOM:
                    prevTile = FindCell(cell.X, cell.Y - 1);
                    break;
                case CellDirection.LEFTTOP:
                    prevTile = FindCell(cell.X + 1, cell.Y + 1);
                    break;
                case CellDirection.RIGHTTOP:
                    prevTile = FindCell(cell.X - 1, cell.Y + 1);
                    break;
                case CellDirection.LEFTBOTTOM:
                    prevTile = FindCell(cell.X + 1, cell.Y - 1);
                    break;
                case CellDirection.RIGHTBOTTOM:
                    prevTile = FindCell(cell.X - 1, cell.Y - 1);
                    break;
            }


            return prevTile;
        }

        private Tile[] GetAroundCells(int x, int y)
        {
            Tile cell_left = FindCell(x - 1, y);
            Tile cell_lefttop = FindCell(x - 1, y - 1);
            Tile cell_top = FindCell(x, y - 1);
            Tile cell_righttop = FindCell(x + 1, y - 1);
            Tile cell_right = FindCell(x + 1, y);
            Tile cell_rightbottom = FindCell(x + 1, y + 1);
            Tile cell_bottom = FindCell(x, y + 1);
            Tile cell_leftbottom = FindCell(x - 1, y + 1);

            List<Tile> aroundCells = new List<Tile>();
            if (cell_left != null) { aroundCells.Add(cell_left); }
            //if (cell_lefttop != null) { aroundCells.Add(cell_lefttop); }
            if (cell_top != null) { aroundCells.Add(cell_top); }
            //if (cell_righttop != null) { aroundCells.Add(cell_righttop); }
            if (cell_right != null) { aroundCells.Add(cell_right); }
            //if (cell_rightbottom != null) { aroundCells.Add(cell_rightbottom); }
            if (cell_bottom != null) { aroundCells.Add(cell_bottom); }
            //if (cell_leftbottom != null) { aroundCells.Add(cell_leftbottom); }
            return aroundCells.ToArray();
        }

        private Tile FindCell(int x, int y)
        {
            int cols = Tiles.GetLength(0);
            int rows = Tiles.GetLength(1);

            if ((0 <= x && x < cols) && (0 <= y && y < rows))
            {
                return Tiles[x, y];
            }
            return null;
        }

        private Tile FindCell(Point point)
        {
            Tile cell = null;
            for (int col = 0; col < Tiles.GetLength(0); col++)
            {
                for (int row = 0; row < Tiles.GetLength(1); row++)
                {
                    if (Tiles[col, row].Contains(point))
                    {
                        cell = Tiles[col, row];
                        break;
                    }
                }
            }
            return cell;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Init();
        }
    }


    public class Tile
    {
        public readonly static int _Width = 20;
        public readonly static int _Height = 20;

        public int X { get; set; }
        public int Y { get; set; }

        public Tile PrevCell { get; set; }

        public int F { get; set; }

        /// <summary>
        /// 종료타일과 거리
        /// </summary>
        public int H { get; set; }

        public CellDirection Direction { get; set; } = CellDirection.NONE;

        Pen bolderPen { get; set; } = Pens.Black;
        public Color BolderColor { get { return bolderPen.Color; } set { bolderPen = new Pen(value); } }

        SolidBrush backColorBrush { get; set; } = new SolidBrush(Color.White);
        public Color BackgroundColor { get { return backColorBrush.Color; } set { backColorBrush = new SolidBrush(value); } }

        Font font = new Font("굴림체", 8f);
        Font font2 = new Font("굴림체", 10f, FontStyle.Bold);

        public void Draw(Graphics g)
        {
            int x = X * _Width;
            int y = Y * _Height;
            Rectangle box = new Rectangle(x, y, _Width, _Height);

            g.FillRectangle(backColorBrush, box);
            g.DrawRectangle(bolderPen, box);
        }

        public void DrawInfo(Graphics g)
        {
            int x = X * _Width;
            int y = Y * _Height;
            Rectangle box = new Rectangle(x, y, _Width, _Height);

            TextRenderer.DrawText(g, "" + F, font, new Point(box.X + 5, box.Y + 5), BolderColor);
            //TextRenderer.DrawText(g, "" + H, font, new Point(box.X + box.Height - 15, box.Y + box.Height - 15), BolderColor);
        }

        internal void DrawMark(Graphics g)
        {
            int x = X * _Width;
            int y = Y * _Height;
            Rectangle box = new Rectangle(x + _Width / 2 - (_Width / 2) / 2, y + _Height / 2 - (_Height / 2) / 2, _Width / 2, _Height / 2);
            g.FillEllipse(new SolidBrush(Pens.DodgerBlue.Color), box);
        }

        public void DrawArrow(Graphics g)
        {
            int x = X * _Width;
            int y = Y * _Height;
            Rectangle box = new Rectangle(x, y, _Width, _Height);

            string direction = "";
            switch (Direction)
            {
                default:
                case CellDirection.NONE:
                    break;
                case CellDirection.LEFT:
                    direction = "→";
                    break;
                case CellDirection.TOP:
                    direction = "↓";
                    break;
                case CellDirection.RIGHT:
                    direction = "←";
                    break;
                case CellDirection.BOTTOM:
                    direction = "↑";
                    break;
                case CellDirection.LEFTTOP:
                    direction = "↘";
                    break;
                case CellDirection.RIGHTTOP:
                    direction = "↙";
                    break;
                case CellDirection.LEFTBOTTOM:
                    direction = "↗";
                    break;
                case CellDirection.RIGHTBOTTOM:
                    direction = "↖";
                    break;
            }

            if (string.IsNullOrWhiteSpace(direction) == false)
            {
                TextRenderer.DrawText(g, direction, font2, box, Color.Red, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }
        }

        public bool Contains(Point point)
        {
            int x = X * _Width;
            int y = Y * _Height;
            Rectangle box = new Rectangle(x, y, _Width, _Height);
            return box.Contains(point);
        }

        public bool IsWall { get; set; } = false;
        internal void ToggleWall()
        {
            IsWall = !IsWall;
            if (IsWall)
            {
                BackgroundColor = Color.DarkGray;
            }
            else
            {
                BackgroundColor = Color.White;
            }
        }

        internal void InitF()
        {
            PrevCell = null;
            Direction = CellDirection.NONE;
            F = 0;
        }

        internal void CalcF(Tile endCell)
        {
            if (PrevCell == null)
            {
                Direction = CellDirection.NONE;
                InitF();
                return;
            }

            if (X < PrevCell.X && Y == PrevCell.Y)
            {
                Direction = CellDirection.LEFT;
            }
            else if (X < PrevCell.X && Y < PrevCell.Y)
            {
                Direction = CellDirection.LEFTTOP;
            }
            else if (X == PrevCell.X && Y < PrevCell.Y)
            {
                Direction = CellDirection.TOP;
            }
            else if (PrevCell.X < X && Y < PrevCell.Y)
            {
                Direction = CellDirection.RIGHTTOP;
            }
            else if (PrevCell.X < X && Y == PrevCell.Y)
            {
                Direction = CellDirection.RIGHT;
            }
            else if (PrevCell.X < X && PrevCell.Y < Y)
            {
                Direction = CellDirection.RIGHTBOTTOM;
            }
            else if (X == PrevCell.X && PrevCell.Y < Y)
            {
                Direction = CellDirection.BOTTOM;
            }
            else if (X < PrevCell.X && PrevCell.Y < Y)
            {
                Direction = CellDirection.LEFTBOTTOM;
            }

            F = PrevCell.F + 1;

            H = (Math.Abs(endCell.X - X) + Math.Abs(endCell.Y - Y)) * 10;
        }

        public override string ToString()
        {
            return $"X={X}, Y={Y}   ({F}, {H})";
        }

        internal void SetPrevCell(Tile currentCell)
        {
            PrevCell = null;
        }
    }
}
