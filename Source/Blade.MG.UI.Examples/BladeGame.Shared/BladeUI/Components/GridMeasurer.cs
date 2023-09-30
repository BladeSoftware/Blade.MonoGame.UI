using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BladeGame.BladeUI.Controls;

namespace BladeGame.BladeUI.Components
{
    internal class GridMeasurer
    {

        public GridMeasurerSizing[] Measurables;

        public GridMeasurer()
        {
        }

        public GridMeasurer(List<ColumnDefinition> columns)
        {
            Measurables = new GridMeasurerSizing[columns.Count];

            for (int i = 0; i < columns.Count; i++)
            {
                Measurables[i] = new GridMeasurerSizing(columns[i]);
            }
        }

        public GridMeasurer(List<RowDefinition> rows)
        {
            Measurables = new GridMeasurerSizing[rows.Count];

            for (int i = 0; i < rows.Count; i++)
            {
                Measurables[i] = new GridMeasurerSizing(rows[i]);
            }

        }

        public void MeasureChild(UIComponent child, float desiredSize, int origin, int span)
        {
            // If Span is Absolute then the Size of the Child doesn't affect the layout
            if (IsSpanAbsolute(origin, span))
            {
                return;
            }

            // If Span is Auto then Equally divide the span by the child length
            if (IsSpanAuto(origin, span))
            {
                float existingSize = 0;
                for (int i = origin; i < origin + span; i++)
                {
                    if (!float.IsNaN(Measurables[i].CalcSize))
                    {
                        existingSize += Measurables[i].CalcSize;
                    }
                }

                desiredSize -= existingSize;
                if (desiredSize >= 0)
                {
                    float delta = desiredSize / (float)span;
                    for (int i = origin; i < origin + span; i++)
                    {
                        if (float.IsNaN(Measurables[i].CalcSize))
                        {
                            Measurables[i].CalcSize = delta;
                        }
                        else
                        {
                            Measurables[i].CalcSize += delta;
                        }
                    }
                }

                return;
            }

            // Handle Mixed Column types
            // Auto columns = 0
            // Absolute Columns remain
            // Extra space divided amongst Star columns 
            // Min Size = Child Size
            float minSize = desiredSize;
            float absoluteSize = 0;
            bool hasStarColumns = false;
            int numAbsoluteColumns = 0;

            //if (Measurables.Count() == 0)
            //{
            //    hasStarColumns = true;
            //}
            //else
            //{
            for (int i = origin; i < origin + span; i++)
            {
                if (Measurables[i].IsAbsolute)
                {
                    absoluteSize += Measurables[i].CalcSize;
                    numAbsoluteColumns++;
                }
                else if (Measurables[i].IsAuto)
                {
                    Measurables[i].CalcSize = 0;
                }
                else if (Measurables[i].IsStar)
                {
                    hasStarColumns = true;
                }
            }
            //}

            // minSize - absoluteSize > 0 then divvy equally amongst absolute columns (provided there aren't any star columns)
            float remainingSize = desiredSize - absoluteSize;
            if (!hasStarColumns && remainingSize > 0)
            {
                float delta = remainingSize / numAbsoluteColumns;

                for (int i = origin; i < origin + span; i++)
                {
                    if (Measurables[i].IsAbsolute)
                    {
                        Measurables[i].CalcSize += delta;
                    }
                }
            }

        }

        public void MeasureStar(float availableSize)
        {
            // Calculate Total Weighting
            float totalWeight = 0;
            for (int i = 0; i < Measurables.Count(); i++)
            {
                GridMeasurerSizing measurable = Measurables[i];
                if (measurable.IsStar)
                {
                    totalWeight += measurable.Size.Value;
                }
            }

            // Divvy up available space according to weighting
            if (availableSize <= 0 || totalWeight < 0)
            {
                totalWeight = 1;
                availableSize = 0;
            }

            for (int i = 0; i < Measurables.Count(); i++)
            {
                GridMeasurerSizing measurable = Measurables[i];
                if (measurable.IsStar)
                {
                    measurable.CalcSize = (measurable.Size.Value / totalWeight) * availableSize;
                }
            }
        }

        /// <summary>
        /// Returns True if the entire span is set to Absolute
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="span"></param>
        /// <returns></returns>
        public bool IsSpanAbsolute(int origin, int span)
        {
            if (Measurables.Count() == 0)
            {
                return false;
            }

            int max = origin + span;
            if (max > Measurables.Count())
            {
                max = Measurables.Count();
            }

            for (int i = origin; i < max; i++)
            {
                if (!Measurables[i].IsAbsolute)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Returns true if the entire span is set to Auto
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="span"></param>
        /// <returns></returns>
        public bool IsSpanAuto(int origin, int span)
        {
            if (Measurables.Count() == 0)
            {
                return false;
            }

            for (int i = origin; i < origin + span; i++)
            {
                if (!Measurables[i].IsAuto)
                {
                    return false;
                }
            }

            return true;
        }

    }
}
