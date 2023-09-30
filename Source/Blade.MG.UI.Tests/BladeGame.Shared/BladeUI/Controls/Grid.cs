using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BladeGame.BladeUI;
using BladeGame.BladeUI.Components;
using Microsoft.Xna.Framework;

namespace BladeGame.BladeUI.Controls
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
            if (ColumnSpanProperty.TryGetValue(element, out column))
            {
                return column;
            }
            else
            {
                return 1;
            }
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
            if (RowSpanProperty.TryGetValue(element, out row))
            {
                return row;
            }
            else
            {
                return 1;
            }
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

        public override void Measure(UIContext context, Size availableSize, ref Layout parentMinMax)
        {
            parentMinMax.Merge(MinWidth, MinHeight, MaxWidth, MaxHeight);

            // If we have no Rows or Columns then add a default Row/Column set to Auto
            columnMeasurer = (ColumnDefinitions.Count > 0) ? new GridMeasurer(ColumnDefinitions) : new GridMeasurer(defaultColumns);
            rowMeasurer = (RowDefinitions.Count > 0) ? new GridMeasurer(RowDefinitions) : new GridMeasurer(defaultRows);

            // Calc Child Sizes
            foreach (var child in Children)
            {
                child.Measure(context, availableSize, ref parentMinMax);

                int col = this.GetColumn(child);
                int colSpan = this.GetColumnSpan(child);
                int row = this.GetRow(child);
                int rowSpan = this.GetRowSpan(child);

                columnMeasurer.MeasureChild(child, child.DesiredSize.Width, col, colSpan);
                rowMeasurer.MeasureChild(child, child.DesiredSize.Height, row, rowSpan);
            }

            var usedWidth = (from col in columnMeasurer.Measurables
                             where !float.IsNaN(col.CalcSize)
                                && !col.IsStar
                             select col.CalcSize
                           ).Sum();

            var usedHeight = (from row in rowMeasurer.Measurables
                              where !float.IsNaN(row.CalcSize)
                                 && !row.IsStar
                              select row.CalcSize
                              ).Sum();

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

            this.DesiredSize = new Size(gridWidth, gridHeight);

            ClampDesiredSize(availableSize, parentMinMax);
        }

        /// <summary>
        /// Layout Children
        /// </summary>
        /// <param name="layoutRect">Size of Parent Container</param>
        public override void Arrange(Rectangle layoutRect)
        {
            var columns = ColumnDefinitions.Count > 0 ? ColumnDefinitions : defaultColumns;
            var rows = RowDefinitions.Count > 0 ? RowDefinitions : defaultRows;


            // Calculate Grid Size
            if (float.IsNaN(Width))
            {
                ActualWidth = layoutRect.Width;
            }
            else
            {
                ActualWidth = UIHelper.Clamp(Width, 0, layoutRect.Width);
            }

            if (float.IsNaN(Height))
            {
                ActualHeight = layoutRect.Height;
            }
            else
            {
                ActualHeight = MathHelper.Clamp(Height, 0, layoutRect.Height);
            }


            // Sum Size of all Columns/Rows with known Sizes
            var usedWidth = (from col in columnMeasurer.Measurables
                             where !float.IsNaN(col.CalcSize)
                                && !col.IsStar
                             select col.CalcSize
                           ).Sum();

            var usedHeight = (from row in rowMeasurer.Measurables
                              where !float.IsNaN(row.CalcSize)
                                 && !row.IsStar
                              select row.CalcSize
                              ).Sum();


            // Divide the remaining space between the Start columns
            columnMeasurer.MeasureStar(Math.Max(layoutRect.Width - usedWidth, 0));
            rowMeasurer.MeasureStar(Math.Max(layoutRect.Height - usedHeight, 0));


            for (int i = 0; i < columns.Count; i++)
            {
                columns[i].ActualWidth = columnMeasurer.Measurables[i].CalcSize;
            }

            for (int i = 0; i < rows.Count; i++)
            {
                rows[i].ActualHeight = rowMeasurer.Measurables[i].CalcSize;
            }


            foreach (var child in Children)
            {
                child.Arrange(GetChildBoundingBox(child));
            }


            finalRect = new Rectangle((int)Left, (int)Top, (int)ActualWidth, (int)ActualHeight);
            finalContentRect = finalRect;
        }

        /// <summary>
        /// Calculate Childs layout rectangle
        /// </summary>
        /// <param name="child"></param>
        /// <returns></returns>
        public override Rectangle GetChildBoundingBox(UIComponent child)
        {
            var columns = ColumnDefinitions.Count > 0 ? ColumnDefinitions : defaultColumns;
            var rows = RowDefinitions.Count > 0 ? RowDefinitions : defaultRows;

            int col = GetColumn(child);
            int colSpan = GetColumnSpan(child);
            int row = GetRow(child);
            int rowSpan = GetRowSpan(child);

            float x = 0;
            float w = 0;
            float y = 0;
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

            return new Rectangle((int)x, (int)y, (int)w, (int)h);
        }

        public override void RenderControl(UIContext context, Rectangle layoutBounds)
        {
            base.RenderControl(context, layoutBounds);
        }

        public void AddChild(UIComponent item, int column, int row)
        {
            base.AddChild(item, this);
            //Children.Add(item);

            SetColumn(item, column);
            SetRow(item, row);
        }

        public void AddChild(UIComponent item, int column, int columnSpan, int row, int rowSpan)
        {
            //Children.Add(item);
            base.AddChild(item, this);

            SetColumn(item, column);
            SetColumnSpan(item, columnSpan);

            SetRow(item, row);
            SetRowSpan(item, rowSpan);
        }

    }
}
