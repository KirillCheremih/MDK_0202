using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using WpfApp1;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        private CalculationService _calculationService;

        [TestInitialize]
        public void Setup()
        {
            _calculationService = new CalculationService();
        }

        [TestMethod]
        public void TestMethod1()
        {
            double width = 1000.5;
            double height = 2000.75;
            bool isAluminum = true;
            double expectedArea = width * height;
            double expectedCost = expectedArea * CalculationService.AluminumPrice;

            var result = _calculationService.Calculate(width, height, isAluminum);

            Assert.IsNotNull(result);
            Assert.AreEqual(expectedArea, result.Area, 0.01);
            Assert.AreEqual(expectedCost, result.TotalCost, 0.01);
            Assert.AreEqual("Алюминий", result.Material);
            Assert.AreEqual(1, _calculationService.CalculationHistory.Count);
        }

        [TestMethod]
        public void Calculate_WithMaxDoubleValue_ReturnsCorrectResultOrNull()
        {
            double width = double.MaxValue;
            double height = double.MaxValue;
            bool isAluminum = true;

            var result = _calculationService.Calculate(width, height, isAluminum);

            // Проверяем либо корректный результат (если сервис обрабатывает большие значения),
            if (result != null)
            {
                Assert.IsTrue(double.IsInfinity(result.TotalCost) ||
                             result.TotalCost > 0);
            }
        }

        [TestMethod]
        public void Calculate_WithNegativeNumbers_ReturnsNull()
        {
            double width = -10.5;
            double height = 20.0;
            bool isAluminum = true;

            var result = _calculationService.Calculate(width, height, isAluminum);

            Assert.IsNull(result);
            Assert.AreEqual(0, _calculationService.CalculationHistory.Count);
        }

        [TestMethod]
        public void Calculate_WithEmptyOrNullInputs_ReturnsNull()
        {
            // Оба параметра равны 0 (аналог "пустых" полей)
            var result1 = _calculationService.Calculate(0, 0, true);
            Assert.IsNull(result1);

            // Один параметр равен 0, другой очень маленькое значение
            var result2 = _calculationService.Calculate(0, 0.0001, false);
            Assert.IsNull(result2);

            // Проверка что метод не должен падать при минимальных double значениях
            var result3 = _calculationService.Calculate(double.MinValue, double.MinValue, true);
            Assert.IsNull(result3); // Ожидаем null для отрицательных значений
        }

        [TestMethod]
        public void Calculate_WithVeryLargePositiveNumbers_HandlesCorrectly()
        {
            double width = 1e308; // Очень большое число
            double height = 10.0;
            bool isAluminum = true;

            var result = _calculationService.Calculate(width, height, isAluminum);

            // Проверяем либо корректный расчет, либо null при переполнении
            if (result != null)
            {
                Assert.IsTrue(double.IsInfinity(result.TotalCost) ||
                             result.TotalCost > 0);
            }
            // Если сервис возвращает null при переполнении - это тоже валидно
        }

        [TestMethod]
        public void Calculate_WithBoundaryValues_ReturnsExpectedResults()
        {
            // Значения на границе "больших" чисел
            var result2 = _calculationService.Calculate(1000000, 1000000, false);
            Assert.IsNotNull(result2);
            Assert.IsTrue(result2.TotalCost > 0);
        }

        [TestMethod]
        public void Calculate_WithPlasticMaterial_ReturnsPlasticCost()
        {
            double width = 2.5;
            double height = 3.0;
            bool isAluminum = false;
            double expectedArea = 7.5;
            double expectedCost = expectedArea * CalculationService.PlasticPrice;

            var result = _calculationService.Calculate(width, height, isAluminum);

            Assert.IsNotNull(result);
            Assert.AreEqual(expectedCost, result.TotalCost, 0.01);
            Assert.AreEqual("Пластик", result.Material);
        }

        [TestMethod]
        public void ClearHistory_ClearsCalculationHistory()
        {
            _calculationService.Calculate(2.0, 3.0, true);
            _calculationService.Calculate(1.5, 2.5, false);

            _calculationService.ClearHistory();

            Assert.IsNull(_calculationService.LastCalculation);
            Assert.AreEqual(0, _calculationService.CalculationHistory.Count);
        }

        [TestMethod]
        public void Calculate_MultipleCalls_AddsToHistory()
        {
            var result1 = _calculationService.Calculate(2.0, 3.0, true);
            var result2 = _calculationService.Calculate(1.5, 2.5, false);
            var result3 = _calculationService.Calculate(3.0, 4.0, true);

            Assert.IsNotNull(result1);
            Assert.IsNotNull(result2);
            Assert.IsNotNull(result3);
            Assert.AreEqual(3, _calculationService.CalculationHistory.Count);
            Assert.AreEqual(result3, _calculationService.LastCalculation);
        }

        [TestMethod]
        public void Calculate_WithMixedNegativePositive_ReturnsNull()
        {
            // различные комбинации отрицательных чисел
            var testCases = new[]
            {
                new { Width = -10.5, Height = -20.0 },
                new { Width = -10.5, Height = 20.0 },
                new { Width = 10.5, Height = -20.0 }
            };

            foreach (var testCase in testCases)
            {
                var result = _calculationService.Calculate(testCase.Width, testCase.Height, true);

                Assert.IsNull(result, $"Should return null for width={testCase.Width}, height={testCase.Height}");
            }
        }
    }
}