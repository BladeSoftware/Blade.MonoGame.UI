namespace Blade.MG.UI.Components
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

            // Handle Mixed Column types
            //float minSize = desiredSize;
            float absoluteSize = 0;
            float existingSize = 0;
            bool hasStarColumns = false;
            int numAbsoluteColumns = 0;
            int numAutoColumns = 0;

            for (int i = origin; i < origin + span; i++)
            {
                var measurable = Measurables[i];
                float size = FloatHelper.ValueOrZero(measurable.CalcSize);

                bool isMaxSize = false;

                // Clamp size to Min/Max
                if (!FloatHelper.IsNaN(measurable.MinSize) && (size < measurable.MinSize))
                {
                    size = measurable.MinSize;
                    measurable.CalcSize = size;
                }

                if (!FloatHelper.IsNaN(measurable.MaxSize) && (size > measurable.MaxSize))
                {
                    size = measurable.MaxSize;
                    measurable.CalcSize = size;
                    isMaxSize = true;
                }


                if (Measurables[i].IsAbsolute)
                {
                    absoluteSize += size; // Measurables[i].CalcSize;
                    existingSize += size; // Measurables[i].CalcSize;
                    numAbsoluteColumns++;
                }
                else if (Measurables[i].IsAuto)
                {
                    existingSize += size;

                    if (!isMaxSize)
                    {
                        numAutoColumns++;
                    }
                }
                else if (Measurables[i].IsStar)
                {
                    existingSize += size; // ??
                    hasStarColumns = true;
                }
            }


            // minSize - absoluteSize > 0 then divvy equally amongst auto columns
            float remainingSize = desiredSize - existingSize;
            bool allocated = false;
            //if (!hasStarColumns && remainingSize > 0)
            if (remainingSize > 0)
            {
                do
                {
                    float delta = remainingSize / numAutoColumns;

                    numAutoColumns = 0;
                    for (int i = origin; i < origin + span; i++)
                    {
                        var measurable = Measurables[i];
                        float size = delta;

                        if (measurable.IsAuto)
                        {
                            if (FloatHelper.IsNaN(Measurables[i].CalcSize))
                            {
                                if (!FloatHelper.IsNaN(measurable.MaxSize) && (size > measurable.MaxSize))
                                {
                                    size = measurable.MaxSize;
                                }
                                else
                                {
                                    numAutoColumns++;
                                }

                                Measurables[i].CalcSize = size;
                            }
                            else
                            {
                                if (!FloatHelper.IsNaN(measurable.MaxSize) && (Measurables[i].CalcSize + size > measurable.MaxSize))
                                {
                                    size = Math.Max(measurable.MaxSize - Measurables[i].CalcSize, 0f);
                                }
                                else
                                {
                                    numAutoColumns++;
                                }

                                Measurables[i].CalcSize += size;
                            }

                            remainingSize -= size;
                        }

                        if (size > 0)
                        {
                            allocated = true;
                        }
                    }

                } while (remainingSize > 0 && allocated && numAutoColumns > 0);
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

            GridMeasurerSizing lastMeasurable = null;
            float sizeUsed = 0f;

            for (int i = 0; i < Measurables.Count(); i++)
            {
                GridMeasurerSizing measurable = Measurables[i];
                if (measurable.IsStar)
                {
                    if (measurable.Size.Value != 0f)
                    {
                        lastMeasurable = measurable;
                    }

                    //measurable.CalcSize = (measurable.Size.Value / totalWeight) * availableSize;
                    measurable.CalcSize = (float)Math.Round(measurable.Size.Value / totalWeight * availableSize, 0);

                    sizeUsed += measurable.CalcSize;
                }
            }

            float delta = availableSize - sizeUsed;
            if (lastMeasurable != null && Math.Abs(delta) > 0.1f)
            {
                lastMeasurable.CalcSize += delta;
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
