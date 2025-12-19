using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1
{
    public class CalculationService
    {
        public const double AluminumPrice = 15.50;
        public const double PlasticPrice = 9.90;

        public CalculationResult LastCalculation { get; private set; }
        public List<CalculationRecord> CalculationHistory { get; } = new List<CalculationRecord>();

        public CalculationResult Calculate(double width, double height, bool isAluminum)
        {
            if (width <= 0 || height <= 0)
            {
                return null; // или выбросить исключение
            }

            double area = width * height;
            string material = isAluminum ? "Алюминий" : "Пластик";
            double pricePerSqm = isAluminum ? AluminumPrice : PlasticPrice;
            double totalCost = area * pricePerSqm;

            LastCalculation = new CalculationResult
            {
                Width = width,
                Height = height,
                Material = material,
                TotalCost = totalCost,
                Area = area
            };

            CalculationHistory.Add(new CalculationRecord
            {
                Date = DateTime.Now,
                Size = $"{width:F2}×{height:F2}",
                Material = material,
                Cost = totalCost
            });

            return LastCalculation;
        }

        public void ClearHistory()
        {
            CalculationHistory.Clear();
            LastCalculation = null;
        }
    }
}
