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
            //set our two graphs into their given variables
            _barGraph = barGraph;
            _pieGraph = pieGraph;
            //setup titles of each plot and their configuration
            _barGraph.Plot.Title("Monthly Spending");
            _pieGraph.Plot.Title("Category Spending");
            //dont need grids on this for aesthetics and remove margins
            _barGraph.Plot.HideGrid();
            _barGraph.Plot.Axes.Margins(bottom: 0);
            //clear and refresh both graphs to start
            _barGraph.Plot.Clear();
            _pieGraph.Plot.Clear();
            _barGraph.Refresh();
            _pieGraph.Refresh();
        }
        public void PopulateGraphs(IEnumerable<Transaction> list)
        {
            //clear bar graph to start
            _barGraph.Plot.Clear();
            //use LINQ query to group data by month
            var monthlyData = list
                .GroupBy(t => new DateTime(t.date.Year, t.date.Month, 1))
                .OrderBy(g => g.Key)
                .Select(g => new
                {
                    //separate based on recurring or non-recurring
                    Month = g.Key,
                    RecurringSum = g.Where(t => t.recurring).Sum(t => (double)t.amount),
                    OneTimeSum = g.Where(t => !t.recurring).Sum(t => (double)t.amount)
                })
                .ToList();
            //color pallate for bar graph
            var barPalette = new Category10();
            //scottplot bar object array with enough bars to hold how many months we have * 2 for recurring and one-time expensions for each
            var bars = new Bar[monthlyData.Count * 2];
            //loop to go through actually initalize each bar for each month
            for (int i = 0; i < monthlyData.Count; i++)
            {
                double pos = i + 1;
                double rec = monthlyData[i].RecurringSum;
                double one = monthlyData[i].OneTimeSum;
                //create each one-time expense bar by month
                bars[2 * i + 0] = new Bar()
                {
                    Position = pos,
                    ValueBase = 0,
                    Value = rec,
                    FillColor = barPalette.GetColor(0),
                };
                //create each monthly expense by by month.
                bars[2 * i + 1] = new Bar()
                {
                    Position = pos,
                    ValueBase = rec,
                    Value = rec + one,
                    FillColor = barPalette.GetColor(1),
                };
            }
            //finally clear the bars that are currently displayed and add generated bars
            _barGraph.Plot.Clear();
            _barGraph.Plot.Add.Bars(bars);
            //add ticks at bottom based on months
            var ticks = monthlyData
                .Select((x, i) => new Tick(i + 1, x.Month.ToString("MMM yyyy")))
                .ToArray();
            _barGraph.Plot.Axes.Bottom.TickGenerator = new NumericManual(ticks);
            _barGraph.Plot.Axes.Bottom.MajorTickStyle.Length = 0;
            //styling for the graph again
            _barGraph.Plot.HideGrid();
            _barGraph.Plot.Axes.Margins(bottom: 0);
            //adding legend for graph
            _barGraph.Plot.Legend.IsVisible = true;
            _barGraph.Plot.Legend.Alignment = Alignment.UpperRight;
            _barGraph.Plot.Legend.ManualItems.Clear();
            _barGraph.Plot.Legend.ManualItems.Add(new LegendItem() { LabelText = "Recurring", FillColor = barPalette.GetColor(0) });
            _barGraph.Plot.Legend.ManualItems.Add(new LegendItem() { LabelText = "One-Time", FillColor = barPalette.GetColor(1) });
            _barGraph.Refresh();
            //moving onto pie grpah
            _pieGraph.Plot.Clear();
            //another LINQ query to sort by category
            var categoryData = list
                .GroupBy(t => t.category)
                .Select(g => new
                {
                    Category = g.Key,
                    Sum = g.Sum(t => (double)t.amount)
                })
                .Where(x => x.Sum > 0)
                .ToList();
            //custom palette for pie graph colors
            Color[] palette = {
                Colors.Red, Colors.Orange, Colors.Gold,
                Colors.Green, Colors.Blue, Colors.Purple,
                Colors.Cyan, Colors.Magenta
            };
            //each 'slice' is a category
            var slices = new List<PieSlice>();
            //looping through each category and adding to slices list
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
            //add all slices
            var pie = _pieGraph.Plot.Add.Pie(slices);
            //50% inner radius
            pie.DonutFraction = 0.5;
            //legend with category labels
            _pieGraph.Plot.ShowLegend();
            //styling for pie graph, no frames to make it bigger, adding title and decreasing margins
            _pieGraph.Plot.Axes.Frameless();
            _pieGraph.Plot.Title("Category Spending");
            _pieGraph.Plot.HideGrid();
            _pieGraph.Plot.Axes.Margins(left: 0, right: 0, top: 0, bottom: 0);
            //update graph on GUI
            _pieGraph.Refresh();
        }
    }
}
