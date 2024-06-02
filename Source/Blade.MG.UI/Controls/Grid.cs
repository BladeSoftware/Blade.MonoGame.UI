using Blade.MG.Input;
using Blade.MG.Primitives;
using Blade.MG.UI.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blade.MG.UI.Controls
{
    public class Grid : Panel
    {
        private List<ColumnDefinition> defaultColumns = new List<ColumnDefinition> { new ColumnDefinition { Width = new GridLength(GridUnitType.Star, 1f) } };
        private List<RowDefinition> defaultRows = new List<RowDefinition> { new RowDefinition { Height = new GridLength(GridUnitType.Star, 1f) } };

        public List<ColumnDefinition> ColumnDefinitions { get; set; }
        public List<RowDefinition> RowDefinitions { get; set; }

        private Dictionary<UIComponent, int> ColumnProperty = new Dictionary<UIComponent, int>();
        private Dictionary<UIComponent, int> RowProperty = new Dictionary<UIComponent, int>();
        private Dictionary<UIComponent, int> ColumnSpanProperty = new Dictionary<UIComponent, int>();
        private Dictionary<UIComponent, int> RowSpanProperty = new Dictionary<UIComponent, int>();

        private GridMeasurer columnMeasurer;
        private GridMeasurer rowMeasurer;


        public Grid()
        {
            IsHitTestVisible = false;

            Init();
        }

        private void Init()
        {
            ColumnDefinitions = new List<ColumnDefinition>();
            RowDefinitions = new List<RowDefinition>();

            // Override default Alignment for Grid to be Horizontal/Vertical = Stretch 
            HorizontalAlignment = HorizontalAlignmentType.Stretch;
            VerticalAlignment = VerticalAlignmentType.Stretch;

        }

        public int GetColumn(UIComponent element)
        {
            int column;
            if (ColumnProperty.TryGetValue(element, out column))
            {
                return column;
            }
            else
            {
                return 0;
            }
        }

        public void SetColumn(UIComponent element, int value)
        {
            if (ColumnProperty.ContainsKey(element))
            {
                ColumnProperty[element] = value;
            }
            else
            {
                ColumnProperty.Add(element, value);
            }
        }

        public int GetRow(UIComponent element)
        {
            int row;
            if (RowProperty.TryGetValue(element, out row))
            {
                return row;
            }
            else
            {
                return 0;
            }
        }

        public void SetRow(UIComponent element, int value)
        {
            if (RowProperty.ContainsKey(element))
            {
                RowProperty[element] = value;
            }
            else
            {
                RowProperty.Add(element, value);
            }
        }

        public int GetColumnSpan(UIComponent element)
        {
            int column;
            if (!ColumnSpanProperty.TryGetValue(element, out column))
            {
                column = 1;
            }

            return (column < 1) ? 1 : column;
        }

        public void SetColumnSpan(UIComponent element, int value)
        {
            if (ColumnSpanProperty.ContainsKey(element))
            {
                ColumnSpanProperty[element] = value;
            }
            else
            {
                ColumnSpanProperty.Add(element, value);
            }
        }

        public int GetRowSpan(UIComponent element)
        {
            int row;
            if (!RowSpanProperty.TryGetValue(element, out row))
            {
                row = 1;
            }

            return (row < 1) ? 1 : row;
        }

        public void SetRowSpan(UIComponent element, int value)
        {
            if (RowSpanProperty.ContainsKey(element))
            {
                RowSpanProperty[element] = value;
            }
            else
            {
                RowSpanProperty.Add(element, value);
            }
        }

        private void ClampMaxMin()
        {
            foreach (var col in columnMeasurer.Measurables)
            {
                if (!float.IsNaN(col.MaxSize))
                {
                    if (float.IsNaN(col.CalcSize))
                    {
                        col.CalcSize = col.MaxSize;
                        col.Size = new GridLength(GridUnitType.Pixel, col.CalcSize);
                    }
                    else if (col.CalcSize > col.MaxSize)
                    {
                        col.CalcSize = col.MaxSize;
                        col.Size = new GridLength(GridUnitType.Pixel, col.CalcSize);
                    }
                }

                if (!float.IsNaN(col.MinSize))
                {
                    if (float.IsNaN(col.CalcSize))
                    {
                        col.CalcSize = col.MinSize;
                        col.Size = new GridLength(GridUnitType.Pixel, col.CalcSize);
                    }
                    else if (col.CalcSize < col.MinSize)
                    {
                        col.CalcSize = col.MinSize;
                        col.Size = new GridLength(GridUnitType.Pixel, col.CalcSize);
                    }
                }
            }

            foreach (var row in rowMeasurer.Measurables)
            {
                if (!float.IsNaN(row.MaxSize))
                {
                    if (float.IsNaN(row.CalcSize))
                    {
                        row.CalcSize = row.MaxSize;
                        row.Size = new GridLength(GridUnitType.Pixel, row.CalcSize);
                    }
                    else if (row.CalcSize > row.MaxSize)
                    {
                        row.CalcSize = row.MaxSize;
                        row.Size = new GridLength(GridUnitType.Pixel, row.CalcSize);
                    }
                }

                if (!float.IsNaN(row.MinSize))
                {
                    if (float.IsNaN(row.CalcSize))
                    {
                        row.CalcSize = row.MinSize;
                        row.Size = new GridLength(GridUnitType.Pixel, row.CalcSize);
                    }
                    else if (row.CalcSize < row.MinSize)
                    {
                        row.CalcSize = row.MinSize;
                        row.Size = new GridLength(GridUnitType.Pixel, row.CalcSize);
                    }
                }
            }

        }

        public override void Measure(UIContext context, ref Size availableSize, ref Layout parentMinMax)
        {
            parentMinMax.Merge(MinWidth, MinHeight, MaxWidth, MaxHeight, availableSize);

            // If we have no Rows or Columns then add a default Row/Column set to Auto
            columnMeasurer = ColumnDefinitions.Count > 0 ? new GridMeasurer(ColumnDefinitions) : new GridMeasurer(defaultColumns);
            rowMeasurer = RowDefinitions.Count > 0 ? new GridMeasurer(RowDefinitions) : new GridMeasurer(defaultRows);


            // Calc Child Sizes
            int maxColSpan = 0;
            int maxRowSpan = 0;
            foreach (var child in Children)
            {
                Layout gridParentMinMax = parentMinMax;
                child.Measure(context, ref availableSize, ref gridParentMinMax);

                int colSpan = GetColumnSpan(child);
                int rowSpan = GetRowSpan(child);

                maxColSpan = Math.Max(colSpan, maxColSpan);
                maxRowSpan = Math.Max(rowSpan, maxRowSpan);
            }

            for (int i = 1; i <= maxColSpan; i++)
            {
                foreach (var child in Children.Where(p=> GetColumnSpan(p) == i))
                {
                    int col = GetColumn(child);
                    int colSpan = i; // GetColumnSpan(child);

                    columnMeasurer.MeasureChild(child, child.DesiredSize.Width + child.Margin.Value.Horizontal, col, colSpan);
                }
            }

            for (int i = 1; i <= maxRowSpan; i++)
            {
                foreach (var child in Children.Where(p => GetRowSpan(p) == i))
                {
                    int row = GetRow(child);
                    int rowSpan = i; // GetRowSpan(child);

                    rowMeasurer.MeasureChild(child, child.DesiredSize.Height + child.Margin.Value.Vertical, row, rowSpan);
                }
            }

            ClampMaxMin();

            float usedWidth = 0f;
            foreach (var col in columnMeasurer.Measurables)
            {
                if (!float.IsNaN(col.CalcSize) && !col.IsStar)
                {
                    usedWidth += col.CalcSize;
                }
            }

            float usedHeight = 0f;
            foreach (var row in rowMeasurer.Measurables)
            {
                if (!float.IsNaN(row.CalcSize) && !row.IsStar)
                {
                    usedHeight += row.CalcSize;
                }
            }

            columnMeasurer.MeasureStar(Math.Max(availableSize.Width - usedWidth, 0));
            rowMeasurer.MeasureStar(Math.Max(availableSize.Height - usedHeight, 0));

            // Return result
            float gridWidth = columnMeasurer.Measurables.Sum(p => float.IsNaN(p.CalcSize) ? 0f : p.CalcSize);
            float gridHeight = rowMeasurer.Measurables.Sum(p => float.IsNaN(p.CalcSize) ? 0f : p.CalcSize);

            if (float.IsNaN(gridWidth) && HorizontalAlignment.Value == HorizontalAlignmentType.Stretch)
            {
                gridWidth = availableSize.Width;
            }

            if (float.IsNaN(gridHeight) && VerticalAlignment.Value == VerticalAlignmentType.Stretch)
            {
                gridHeight = availableSize.Height;
            }


            // Cater for Margins and Padding
            gridWidth += Margin.Value.Left + Margin.Value.Right;
            gridHeight += Margin.Value.Top + Margin.Value.Bottom;

            gridWidth += Padding.Value.Left + Padding.Value.Right;
            gridHeight += Padding.Value.Top + Padding.Value.Bottom;


            DesiredSize = new Size(gridWidth, gridHeight);

            ClampDesiredSize(availableSize, parentMinMax);
        }

        /// <summary>
        /// Layout Children
        /// </summary>
        /// <param name="layoutBounds">Size of Parent Container</param>
        public override void Arrange(UIContext context, Rectangle layoutBounds, Rectangle parentLayoutBounds)
        {
            ArrangeSelf(context, layoutBounds, parentLayoutBounds);

            // Set layout bounds for children to the grid's final content rectangle
            layoutBounds = FinalContentRect;

            //-------------------   

            var columns = ColumnDefinitions.Count > 0 ? ColumnDefinitions : defaultColumns;
            var rows = RowDefinitions.Count > 0 ? RowDefinitions : defaultRows;

            // Sum Size of all Columns/Rows with known Sizes
            float usedWidth = 0f;
            foreach (var col in columnMeasurer.Measurables)
            {
                if (!float.IsNaN(col.CalcSize) && !col.IsStar)
                {
                    usedWidth += col.CalcSize;
                }
            }

            float usedHeight = 0f;
            foreach (var row in rowMeasurer.Measurables)
            {
                if (!float.IsNaN(row.CalcSize) && !row.IsStar)
                {
                    usedHeight += row.CalcSize;
                }
            }


            // Divide the remaining space between the Star columns/rows
            columnMeasurer.MeasureStar(Math.Max(layoutBounds.Width - usedWidth, 0));
            rowMeasurer.MeasureStar(Math.Max(layoutBounds.Height - usedHeight, 0));


            for (int i = 0; i < columns.Count; i++)
            {
                columns[i].ActualWidth = columnMeasurer.Measurables[i].CalcSize;
            }

            for (int i = 0; i < rows.Count; i++)
            {
                rows[i].ActualHeight = rowMeasurer.Measurables[i].CalcSize;
            }


            FinalRect = new Rectangle((int)Left, (int)Top, (int)ActualWidth, (int)ActualHeight);

            foreach (var child in Children)
            {
                child.Arrange(context, GetChildBoundingBox(context, child), FinalRect);
            }


            FinalRect = new Rectangle((int)Left, (int)Top, (int)ActualWidth, (int)ActualHeight);

            // Add padding to get content area
            int left = FinalRect.Left + Padding.Value.Left;
            int top = FinalRect.Top + Padding.Value.Top;
            int right = FinalRect.Right - Padding.Value.Right;
            int bottom = FinalRect.Bottom - Padding.Value.Bottom;

            if (left < FinalRect.Left) left = FinalRect.Left;
            if (left > FinalRect.Right) left = FinalRect.Right;
            if (right < FinalRect.Left) right = FinalRect.Left;
            if (right > FinalRect.Right) right = FinalRect.Right;
            if (top < FinalRect.Top) top = FinalRect.Top;
            if (top > FinalRect.Bottom) top = FinalRect.Bottom;
            if (bottom < FinalRect.Top) bottom = FinalRect.Top;
            if (bottom > FinalRect.Bottom) bottom = FinalRect.Bottom;

            int width = right - left;// + 1;
            int height = bottom - top;// + 1;

            if (width < 0) width = 0;
            if (height < 0) height = 0;

            //finalContentRect = finalRect;
            FinalContentRect = new Rectangle(left, top, width, height);
            //clippingRect = Rectangle.Intersect(layoutBounds, finalRect);

        }

        /// <summary>
        /// Calculate Childs layout rectangle
        /// </summary>
        /// <param name="child"></param>
        /// <returns></returns>
        public override Rectangle GetChildBoundingBox(UIContext context, UIComponent child)
        {
            var columns = ColumnDefinitions.Count > 0 ? ColumnDefinitions : defaultColumns;
            var rows = RowDefinitions.Count > 0 ? RowDefinitions : defaultRows;

            int col = GetColumn(child);
            int colSpan = GetColumnSpan(child);
            int row = GetRow(child);
            int rowSpan = GetRowSpan(child);

            float x = Padding.Value.Left;
            float w = 0;
            float y = Padding.Value.Top;
            float h = 0;

            for (int i = 0; i < col; i++)
            {
                if (i < columns.Count())
                {
                    float width = columns[i].ActualWidth;
                    x += float.IsNaN(width) ? 0f : width;
                }
            }

            for (int i = col; i < col + colSpan; i++)
            {
                if (i < columns.Count())
                {
                    float width = columns[i].ActualWidth;
                    w += float.IsNaN(width) ? 0f : width;
                }
            }

            for (int i = 0; i < row; i++)
            {
                if (i < rows.Count())
                {
                    float height = rows[i].ActualHeight;
                    y += float.IsNaN(height) ? 0f : height;
                }
            }

            for (int i = row; i < row + rowSpan; i++)
            {
                if (i < rows.Count())
                {
                    float height = rows[i].ActualHeight;
                    h += float.IsNaN(height) ? 0f : height;
                }
            }

            return new Rectangle((int)x + (int)Left, (int)y + (int)Top, (int)w, (int)h);
        }

        public override void RenderControl(UIContext context, Rectangle layoutBounds, Transform parentTransform)
        {
            base.RenderControl(context, layoutBounds, parentTransform);

            // For debugging, highlight the row and column under the mouse
            if (UIManager.RenderControlHitBoxes)
            {
                using (SpriteBatch sb = new SpriteBatch(context.GraphicsDevice))
                {
                    sb.Begin();

                    float left = FinalContentRect.Left;
                    float top = FinalContentRect.Top;

                    Color color = new Color(0.1f, 0.1f, 0.1f, 0.25f);

                    foreach (var col in columnMeasurer.Measurables)
                    {
                        Rectangle colRect = new Rectangle((int)left, (int)top, (int)col.CalcSize, FinalContentRect.Height);

                        if (colRect.Contains(InputManager.MouseState.Position))
                        {
                            Primitives2D.FillRect(sb, colRect, color);
                        }

                        left += col.CalcSize;
                    }


                    left = FinalContentRect.Left;
                    top = FinalContentRect.Top;

                    foreach (var row in rowMeasurer.Measurables)
                    {
                        Rectangle rowRect = new Rectangle((int)left, (int)top, FinalContentRect.Width, (int)row.CalcSize);

                        if (rowRect.Contains(InputManager.MouseState.Position))
                        {
                            Primitives2D.FillRect(sb, rowRect, color);
                        }

                        top += row.CalcSize;
                    }

                    sb.End();
                }
            }

        }

        public void AddChild(UIComponent item, int column, int row, object dataContext = null)
        {
            base.AddChild(item, this, dataContext);

            SetColumn(item, column);
            SetRow(item, row);
        }

        public void AddChild(UIComponent item, int column, int columnSpan, int row, int rowSpan, object dataContext = null)
        {
            base.AddChild(item, this, dataContext);

            SetColumn(item, column);
            SetColumnSpan(item, columnSpan);

            SetRow(item, row);
            SetRowSpan(item, rowSpan);
        }

    }
}
