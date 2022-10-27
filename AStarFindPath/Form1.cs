using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
//https://www.redblobgames.com/pathfinding/a-star/introduction.html

//https://qiao.github.io/PathFinding.js/visual/

//http://itmining.tistory.com/66

// http://blog.naver.com/PostView.nhn?blogId=denoil&logNo=221014139704 ( G값 계산에 대한 설명 )

//코딩순서( 대략적인 )

// 1. 맵( 2차원 배열 )을 만든다. 

// 2. 시작위치, 종료위치를 셀 지정.

// 3. 벽위치를 셀 지정

// 4. 경로 탐색!

// == 경로탐색 방법 코딩순서 ==

// 셀탐색( 대상셀 )
//      isSearching() 
//      {
// 8방 셀 탐색
//----------------------
//  ↘   |   ↓   |   ↙
//----------------------
//  →    | 중심셀|   ←
//----------------------
//  ↗   |   ↑   |   ↖
//----------------------

// 지정셀 = GetCell( x, y );
// 지정셀.이전셀 = 대상셀
// *벽이면 x, 탐색된셀이면 x
// 아니면 오픈셀 등록
// 계산.

// 8방향 모드 끝나면 목록 중 F값이 낮은 목록 검색
// 셀탐색( 검색된 셀 );
//      }

/*
      만들어낸 코드순서 정리.

    . 자료목록
        - 검색대상목록 : Openeds
        - 제외대상목록 : Closeds

    . 맵을 생성
    . 시작점, 도착점 위치 셀정보를 취득 ( 고정값으로 맵을 만들때 지정하거나 직접 이동시킨다. )
    . 탐색!
        . 검색대상목록 추가 : 시작위치셀
        . while 1.도착점을 아직 못찾았다. 검색대상 셀목록이 있다.
            . 검색대상목록 중 F값이 제일 낮은 목록 취득
            . foreach 검색대상셀(A) in 취득된 검색대상목록
                . 검색대상셀에 인접한 셀 목록 취득
                . foreach 인접한셀(B) in 취득된 인접한 셀목록
                    . if 인접한셀 벽인가?,   제외대상목록인가? 
                            continue;
                    . if 검색대상목록에 있는 셀인가?
                            x
                    . else 
                            검색대상목록에 추가
                            인접한셀(B).이전노드 = 취득된 검색대상셀(A)
                            인접한셀(B).계산()
                                : F, G, H계산
                                : 이전노드 위치를 가리키는 방향계산    
 */

namespace AStarFindPath
{
    public partial class Form1 : Form
    {
        Cell[,] Cells = null;

        Cell StartCell = null;

        Cell EndCell = null;
       
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
        
 /*
        string MapString = @"###########/" +
                           @"#         #/" +
                           @"#         #/" +
                           @"#         #/" +
                           @"# S #     #/" +
                           @"#   # E   #/" +
                           @"#   #     #/" +
                           @"#         #/" +
                           @"#         #/" +
                           @"###########/";

*/

        public Form1()
        {
            InitializeComponent();

            DoubleBuffered = true;

            //Cells = new Cell[10/*x*/, 10/*y*/];

            //for (int row = 0; row < Cells.GetLength(1); row++)
            //{
            //    for (int col = 0; col < Cells.GetLength(0); col++)
            //    {
            //        Cells[col, row] = new Cell()
            //        {
            //            X = col,
            //            Y = row
            //        };
            //    }
            //}
            CreateMap();
        }

        private void CreateMap()
        {
            string[] lines = MapString.Split("/".ToArray(), StringSplitOptions.RemoveEmptyEntries);

            int rows = lines.Length;
            int cols = 0;
            if (0 < lines.Length) cols = lines[0].Length;

            Cells = new Cell[cols/*x*/, rows/*y*/];

            for (int row = 0; row < rows; row++)
            {
                //if (string.IsNullOrWhiteSpace(lines[row])) continue;
                for (int col = 0; col < cols; col++)
                {
                    Cells[col, row] = new Cell()
                    {
                        X = col,
                        Y = row,
                    };

                    if (lines[row][col] == 'S')
                    {
                        SetStartCell(Cells[col, row]);
                    }
                    else if (lines[row][col] == 'E')
                    {
                        SetEndCell(Cells[col, row]);
                    }

                    if (lines[row][col] == '#') Cells[col, row].ToggleWall();

                }
            } 
        }
        
        protected override void OnClosing(CancelEventArgs e)
        {
            if( Cells != null) Array.Clear(Cells, 0 , Cells.Length);
            StartCell = null;
            EndCell = null;
            stepCurrentCell = null;
            base.OnClosing(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            for (int col = 0; col < Cells.GetLength(0); col++)
            {
                for (int row = 0; row < Cells.GetLength(1); row++)
                {
                    Cells[col, row].Draw(e.Graphics);
                   // Cells[col, row].DrawInfo(e.Graphics);
                    Cells[col, row].DrawArrow(e.Graphics);
                }
            }

            if (EndCell?.PrevCell != null)
            {
                EndCell.DrawMark(e.Graphics);
                Cell moveCell = EndCell?.PrevCell ?? null;
                while (moveCell != null && StartCell?.Equals(moveCell) == false)
                {
                    moveCell.DrawMark(e.Graphics);
                    moveCell = moveCell?.PrevCell;
                }
                StartCell.DrawMark(e.Graphics);
            }

            for (int col = 0; col < Cells.GetLength(0); col++)
            {
                for (int row = 0; row < Cells.GetLength(1); row++)
                {
                 //   Cells[col, row].Draw(e.Graphics);
                 //   Cells[col, row].DrawInfo(e.Graphics);
                    Cells[col, row].DrawArrow(e.Graphics);
                }
            }

            if(stepCurrentCell != null) stepCurrentCell.DrawMark(e.Graphics);
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            Cell cell = FindCell(PointToClient(MousePosition));
            ToggleWallCells(cell);
        }

        private void ToggleWallCells(Cell cell)
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

        private void SetEndCell(Cell cell)
        {
            if (EndCell != null) EndCell.BackgroundColor = Color.White;

            EndCell = cell;

            if (EndCell != null)
            {
                EndCell.BackgroundColor = Color.Yellow;
                EndCell.IsWall = false;
            }
        }

        private void SetStartCell(Cell cell)
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
        Cell movingCell = null;
        bool isWall = false;
        
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            isMD = e.Button == MouseButtons.Left;
            Cell cell = FindCell(PointToClient(MousePosition));
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
                Cell cell = FindCell(PointToClient(MousePosition));
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
        }
        
        int state = 0; 
        private void button1_Click(object sender, EventArgs e)
        {
            // tlwkr
            state = 1;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            state = 9;
        }
         
        List<Cell> Openeds = new List<Cell>();
        List<Cell> Closeds = new List<Cell>();

        private void button3_Click(object sender, EventArgs e)
        {
            state = 0;
            step = 0;

            // 찾기.
            if (StartCell == null)
            {
                MessageBox.Show("시작지점 확인!");
                return;
            }
            if (EndCell == null) {
                MessageBox.Show("종료지점 확인!");
                return;
            } 
            Openeds.Clear();
            Closeds.Clear();
            
            Openeds.Add(StartCell);

            ClearFGH();
            bool IsFinding = true;
             
            while (IsFinding )
            {
                Cell[] TargetCells = MinFCells(Openeds);

                Cell currentCell = null; 
                foreach (var cell in TargetCells)
                {
                    currentCell = cell;

                    Openeds.Remove(currentCell);
                    Closeds.Add(currentCell);

                    Cell[] aroundCells = GetAroundCells(currentCell.X, currentCell.Y);

                    foreach (var arcell in aroundCells)
                    {
                        if (arcell.IsWall) continue; // 벽
                        if (Closeds.Contains(arcell)) continue; // 검색이 필요없는 셀

                        if (Openeds.Contains(arcell))
                        {
                            // G비교?
                            int rstG = arcell.CalcG(currentCell);
                            if (rstG < arcell.G)
                            {
                                arcell.PrevCell = currentCell;
                                arcell.CalcFGH(EndCell);
                            }
                        }
                        else
                        {
                            Openeds.Add(arcell);
                            arcell.PrevCell = currentCell;
                            arcell.CalcFGH(EndCell);
                        }
                    }
                }

                if (Openeds.Count <= 0 || EndCell.PrevCell != null)
                {
                    IsFinding = false;
                }
            } 
            Invalidate();
            
        }

        int step = 0;
        Cell stepCurrentCell = null;
        private void button4_Click(object sender, EventArgs e)
        {
            state = 0;
            step++;

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

            Openeds.Add(StartCell);

            ClearFGH();
           
            int loop = step;

            while (0 < loop)
            {
                Cell[] TargetCells = MinFCells(Openeds);
                 
                foreach (var cell in TargetCells)
                {
                    Cell currentCell = stepCurrentCell = cell; 
                    Openeds.Remove(currentCell);
                    Closeds.Add(currentCell);

                    Cell[] aroundCells = GetAroundCells(currentCell.X, currentCell.Y);
                     
                    foreach (var arcell in aroundCells)
                    {
                        if (arcell.IsWall) continue; // 벽
                        if (Closeds.Contains(arcell)) continue; // 검색이 필요없는 셀
                         
                        if (Openeds.Contains(arcell))
                        {
                            // G비교?
                            int rstG = arcell.CalcG(currentCell);
                            if (rstG < arcell.G)
                            {
                                arcell.PrevCell = currentCell;
                                arcell.CalcFGH(EndCell);
                            }
                        }
                        else
                        {
                            Openeds.Add(arcell);
                            arcell.PrevCell = currentCell;
                            arcell.CalcFGH(EndCell);
                        }
                    }
                }  
                loop--;
            } 
            Invalidate(); 
        }
        
        private Cell[] GetAroundCells(int x, int y)
        {
            Cell cell_left = FindCell(x - 1, y);
            Cell cell_lefttop = FindCell(x - 1, y - 1);
            Cell cell_top = FindCell(x, y - 1);
            Cell cell_righttop = FindCell(x + 1, y - 1);
            Cell cell_right = FindCell(x + 1, y);
            Cell cell_rightbottom = FindCell(x + 1, y + 1);
            Cell cell_bottom = FindCell(x, y + 1);
            Cell cell_leftbottom = FindCell(x - 1, y + 1);

            List<Cell> aroundCells = new List<Cell>();

            if (cell_left != null) { aroundCells.Add(cell_left); }
           // if (cell_lefttop != null ) { aroundCells.Add(cell_lefttop); }
            if (cell_top != null) { aroundCells.Add(cell_top); }
           // if (cell_righttop != null ) { aroundCells.Add(cell_righttop); }
            if (cell_right != null) { aroundCells.Add(cell_right); }
           // if (cell_rightbottom != null) { aroundCells.Add(cell_rightbottom); }
            if (cell_bottom != null) { aroundCells.Add(cell_bottom); }
           // if (cell_leftbottom != null ) { aroundCells.Add(cell_leftbottom); }

           /*
             # 노멀... 
            if (cell_left != null) { aroundCells.Add(cell_left); }
            if (cell_lefttop != null ) { aroundCells.Add(cell_lefttop); }
            if (cell_top != null) { aroundCells.Add(cell_top); }
            if (cell_righttop != null ) { aroundCells.Add(cell_righttop); }
            if (cell_right != null) { aroundCells.Add(cell_right); }
            if (cell_rightbottom != null) { aroundCells.Add(cell_rightbottom); }
            if (cell_bottom != null) { aroundCells.Add(cell_bottom); }
            if (cell_leftbottom != null ) { aroundCells.Add(cell_leftbottom); }
           */
           /*
             # 벽체크.. 
           if (cell_left != null) { aroundCells.Add(cell_left); }
           if (cell_lefttop != null && !cell_left.IsWall) { aroundCells.Add(cell_lefttop); }
           if (cell_top != null) { aroundCells.Add(cell_top); }
           if (cell_righttop != null && !cell_right.IsWall) { aroundCells.Add(cell_righttop); }
           if (cell_right != null) { aroundCells.Add(cell_right); }
           if (cell_rightbottom != null && !cell_right.IsWall) { aroundCells.Add(cell_rightbottom); }
           if (cell_bottom != null) { aroundCells.Add(cell_bottom); }
           if (cell_leftbottom != null && !cell_left.IsWall) { aroundCells.Add(cell_leftbottom); }
           */
           /*
            //대각선 못가게.. 
            if (cell_left != null && cell_lefttop.IsWall) { aroundCells.Add(cell_left); }
            if (cell_lefttop != null && !(cell_left.IsWall || cell_top.IsWall)) { aroundCells.Add(cell_lefttop); }
            if (cell_top != null) { aroundCells.Add(cell_top); }
            if (cell_righttop != null && !(cell_top.IsWall || cell_right.IsWall)) { aroundCells.Add(cell_righttop); }
            if (cell_right != null) { aroundCells.Add(cell_right); }
            if (cell_rightbottom != null && !(cell_right.IsWall || cell_bottom.IsWall)) { aroundCells.Add(cell_rightbottom); }
            if (cell_bottom != null) { aroundCells.Add(cell_bottom); }
            if (cell_leftbottom != null && !(cell_left.IsWall || cell_right.IsWall)) { aroundCells.Add(cell_leftbottom); }
           */
            return aroundCells.ToArray();
        }
        
        private Cell FindCell(int x, int y)
        {
            int cols = Cells.GetLength(0);
            int rows = Cells.GetLength(1);
            
            if ((0 <= x && x < cols) && (0<= y && y < rows))
            {
                return Cells[x, y];
            }
            return null;
        }

        private Cell FindCell(Point point)
        {
            Cell cell = null;
            for (int col = 0; col < Cells.GetLength(0); col++)
            {
                for (int row = 0; row < Cells.GetLength(1); row++)
                {
                    if (Cells[col, row].Contains(point))
                    {
                        cell = Cells[col, row];
                        break;
                    }
                }
            }
            return cell;
        }

        private Cell[] MinFCells(List<Cell> openeds)
        {
            List<Cell> finds = new List<Cell>();
            int min = openeds.Min(c => c.F);
            Openeds.ForEach(c =>
            {
                if (min == c.F)
                    finds.Add(c);
            });
            return finds.ToArray();
        }

        private void ClearFGH( bool IsWallClear = false)
        {
            for (int col = 0; col < Cells.GetLength(0); col++)
            {
                for (int row = 0; row < Cells.GetLength(1); row++)
                {
                    Cells[col, row].Direction = CellDirection.NONE;
                    Cells[col, row].InitFGH();
                    if (IsWallClear)
                    {
                        Cells[col, row].IsWall = true;
                        Cells[col, row].ToggleWall();

                    }
                }
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            // 초기화! 
            step = 0; state = 0;

            ClearFGH(true);

            StartCell = Cells[2, 3];
            StartCell.BackgroundColor = Color.ForestGreen;
            StartCell.PrevCell = null;
            StartCell.IsWall = false;

            EndCell = Cells[6, 3];
            EndCell.BackgroundColor = Color.Yellow;
            EndCell.IsWall = false;
            EndCell.PrevCell = null;

            Cells[4, 2].ToggleWall();
            Cells[4, 3].ToggleWall(); 
            Cells[4, 4].ToggleWall();

            Invalidate();
        }
    }
     
    public enum CellDirection
    {
        NONE,
        LEFT,
        TOP,
        RIGHT,
        BOTTOM,
        LEFTTOP,
        RIGHTTOP,
        LEFTBOTTOM,
        RIGHTBOTTOM
    }

    public class Cell
    {
        public readonly static int _Width = 20;
        public readonly static int _Height = 20;
         
        public int X { get; set; }
        public int Y { get; set; }

        public CellDirection Direction { get; set; } = CellDirection.NONE;

        /// <summary>
        /// F = G + H
        /// </summary>
        public int F { get { return G + H; } }
        /// <summary>
        /// 시작타일과의 거리 ( 부모로 지정된 타일의 G값을 상속받음.
        /// </summary>
        public int G { get; set; }
        /// <summary>
        /// 종료타일과 거리
        /// </summary>
        public int H { get; set; }

        public Cell PrevCell { get; set; }
         
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
            TextRenderer.DrawText(g, "" + G, font, new Point(box.X + 5, box.Y + box.Height - 15), BolderColor);
            TextRenderer.DrawText(g, "" + H, font, new Point(box.X + box.Height - 15, box.Y + box.Height - 15), BolderColor);
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

        internal void InitFGH()
        {
            G = 0;
            H = 0;
            PrevCell = null;
        }

        internal void CalcFGH(Cell endCell)
        {
            // 현재 타일이 가리키고 있는 부모타일의 방향값
            //
            //---------------------------------------------------
            //  ↘   |   ↓   |   ↙
            //---------------------------------------------------
            //  →    | 중심셀|   ←
            //---------------------------------------------------
            //  ↗   |   ↑   |   ↖
            //---------------------------------------------------
            //
            // 중심셀 타일과 현재 타일과의 위치를 비교하여 방향을 결정함. 

            // x 가 - 값이면! 왼쪽
            // x 가 + 값이면! 오른쪽
            // x 가 0 이면 중심

            // y 가 - 값이면! 위쪽
            // y 가 + 값이면! 아래쪽
            // y 가 0 이면 중심  
            if (PrevCell == null)
            {
                Direction = CellDirection.NONE;
                InitFGH();
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


            ////////////
            int g = 0;
            switch (Direction)
            {
                default:
                case CellDirection.NONE:
                    break;
                case CellDirection.LEFT: 
                case CellDirection.TOP: 
                case CellDirection.RIGHT: 
                case CellDirection.BOTTOM:
                    g = 10;
                    break;
                case CellDirection.LEFTTOP:
                case CellDirection.RIGHTTOP:
                case CellDirection.LEFTBOTTOM:
                case CellDirection.RIGHTBOTTOM:
                    g = 14;
                    break;
            }

            G = (PrevCell?.G ?? 0 ) + g;

            H = (Math.Abs(endCell.X - X) + Math.Abs(endCell.Y - Y)) * 10; 
        }

        internal int CalcG(Cell tmpParentCell)
        {
            // 현재 타일이 가리키고 있는 부모타일의 방향값
            //
            //---------------------------------------------------
            //  ↘   |   ↓   |   ↙
            //---------------------------------------------------
            //  →    | 중심셀|   ←
            //---------------------------------------------------
            //  ↗   |   ↑   |   ↖
            //---------------------------------------------------
            //
            // 중심셀 타일과 현재 타일과의 위치를 비교하여 방향을 결정함. 

            // x 가 - 값이면! 왼쪽
            // x 가 + 값이면! 오른쪽
            // x 가 0 이면 중심

            // y 가 - 값이면! 위쪽
            // y 가 + 값이면! 아래쪽
            // y 가 0 이면 중심  
            CellDirection tempDirection = CellDirection.NONE;
            int resultG = int.MaxValue;
            if (tmpParentCell == null)
            {
                tempDirection = CellDirection.NONE;
                resultG = int.MaxValue;
                return resultG;
            }

            if (X < tmpParentCell.X && Y == tmpParentCell.Y)
            {
                tempDirection = CellDirection.LEFT;
            }
            else if (X < tmpParentCell.X && Y < tmpParentCell.Y)
            {
                tempDirection = CellDirection.LEFTTOP;
            }
            else if (X == tmpParentCell.X && Y < tmpParentCell.Y)
            {
                tempDirection = CellDirection.TOP;
            }
            else if (tmpParentCell.X < X && Y < tmpParentCell.Y)
            {
                tempDirection = CellDirection.RIGHTTOP;
            }
            else if (tmpParentCell.X < X && Y == tmpParentCell.Y)
            {
                tempDirection = CellDirection.RIGHT;
            }
            else if (tmpParentCell.X < X && tmpParentCell.Y < Y)
            {
                tempDirection = CellDirection.RIGHTBOTTOM;
            }
            else if (X == tmpParentCell.X && tmpParentCell.Y < Y)
            {
                tempDirection = CellDirection.BOTTOM;
            }
            else if (X < tmpParentCell.X && tmpParentCell.Y < Y)
            {
                tempDirection = CellDirection.LEFTBOTTOM;
            }
             
            ////////////
            int g = int.MaxValue;
            switch (tempDirection)
            {
                default:
                case CellDirection.NONE:
                    break;
                case CellDirection.LEFT:
                case CellDirection.TOP:
                case CellDirection.RIGHT:
                case CellDirection.BOTTOM:
                    g = 10;
                    break;
                case CellDirection.LEFTTOP:
                case CellDirection.RIGHTTOP:
                case CellDirection.LEFTBOTTOM:
                case CellDirection.RIGHTBOTTOM:
                    g = 14;
                    break;
            }

            resultG = (tmpParentCell?.G ?? 0) + g;

            return resultG;
        }

        internal void DrawMark(Graphics g)
        {
            int x = X * _Width;
            int y = Y * _Height;
            Rectangle box = new Rectangle(x + _Width / 2 - (_Width / 2)/2, y + _Height / 2 - (_Height / 2)/2, _Width / 2, _Height / 2);
            g.FillEllipse( new SolidBrush( Pens.DodgerBlue.Color ), box);
        }
    }
     
}
