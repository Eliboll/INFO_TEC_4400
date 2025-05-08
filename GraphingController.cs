using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScottPlot;
using ScottPlot.Palettes;
using ScottPlot.Plottables;
using ScottPlot.TickGenerators;
using ScottPlot.WPF;

namespace Transaction_Tracker
{
    public class GraphingController
    {
        private readonly WpfPlot _barGraph;
        private readonly WpfPlot _pieGraph;
        public GraphingController(WpfPlot barGraph, WpfPlot pieGraph)
        {
            _barGraph = barGraph;
            _pieGraph = pieGraph;

            _barGraph.Plot.Title("Monthly Spending");
            _pieGraph.Plot.Title("Category Spending");
            _barGraph.Plot.HideGrid();
            _barGraph.Plot.Axes.Margins(bottom: 0);
            _barGraph.Plot.Clear();
            _pieGraph.Plot.Clear();
            _barGraph.Refresh();
            _pieGraph.Refresh();
        }
        public void PopulateGraphs(IEnumerable<Transaction> list)
        {
            _barGraph.Plot.Clear();

            var monthlyData = list
                .GroupBy(t => new DateTime(t.date.Year, t.date.Month, 1))
                .OrderBy(g => g.Key)
                .Select(g => new
                {
                    Month = g.Key,
                    RecurringSum = g.Where(t => t.recurring).Sum(t => (double)t.amount),
                    OneTimeSum = g.Where(t => !t.recurring).Sum(t => (double)t.amount)
                })
                .ToList();

            var barPalette = new Category10();

            var bars = new Bar[monthlyData.Count * 2];
            for (int i = 0; i < monthlyData.Count; i++)
            {
                double pos = i + 1;
                double rec = monthlyData[i].RecurringSum;
                double one = monthlyData[i].OneTimeSum;

                bars[2 * i + 0] = new Bar()
                {
                    Position = pos,
                    ValueBase = 0,
                    Value = rec,
                    FillColor = barPalette.GetColor(0),
                    Label = "Recurring"
                };

                bars[2 * i + 1] = new Bar()
                {
                    Position = pos,
                    ValueBase = rec,
                    Value = rec + one,
                    FillColor = barPalette.GetColor(1),
                    Label = "One-Time"
                };
            }

            _barGraph.Plot.Clear();
            _barGraph.Plot.Add.Bars(bars);

            var ticks = monthlyData
                .Select((x, i) => new Tick(i + 1, x.Month.ToString("MMM yyyy")))
                .ToArray();
            _barGraph.Plot.Axes.Bottom.TickGenerator = new NumericManual(ticks);
            _barGraph.Plot.Axes.Bottom.MajorTickStyle.Length = 0;

            _barGraph.Plot.HideGrid();
            _barGraph.Plot.Axes.Margins(bottom: 0);
            _barGraph.Plot.Legend.IsVisible = true;
            _barGraph.Plot.Legend.Alignment = Alignment.UpperLeft;
            _barGraph.Plot.Legend.ManualItems.Clear();
            _barGraph.Plot.Legend.ManualItems.Add(new LegendItem() { LabelText = "Recurring", FillColor = barPalette.GetColor(0) });
            _barGraph.Plot.Legend.ManualItems.Add(new LegendItem() { LabelText = "One-Time", FillColor = barPalette.GetColor(1) });
            _barGraph.Refresh();

            _pieGraph.Plot.Clear();

            var categoryData = list
                .GroupBy(t => t.category)
                .Select(g => new
                {
                    Category = g.Key,
                    Sum = g.Sum(t => (double)t.amount)
                })
                .Where(x => x.Sum > 0)
                .ToList();
            Color[] palette = {
                Colors.Red, Colors.Orange, Colors.Gold,
                Colors.Green, Colors.Blue, Colors.Purple,
                Colors.Cyan, Colors.Magenta
            };

            var slices = new List<PieSlice>();
            for (int i = 0; i < categoryData.Count; i++)
            {
                var cat = categoryData[i];
                slices.Add(new PieSlice()
                {
                    Value = cat.Sum,
                    Label = cat.Category,
                    FillColor = palette[i % palette.Length]
                });
            }

            var pie = _pieGraph.Plot.Add.Pie(slices);
            pie.DonutFraction = 0.5;           // 50% inner radius
            _pieGraph.Plot.ShowLegend();       // legend with category labels
            
            _pieGraph.Plot.Axes.Frameless();
            _pieGraph.Plot.Title("Category Spending");
            _pieGraph.Plot.HideGrid();
            _pieGraph.Plot.Axes.Margins(left: 0, right: 0, top: 0, bottom: 0);
            _pieGraph.Refresh();
        }
    }
}
