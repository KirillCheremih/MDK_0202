using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

using System.Windows;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace WpfApp1
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public double AluminumPrice = 15.50;
        public double PlasticPrice = 9.90;

        public List<CalculationRecord> calculationHistory = new List<CalculationRecord>();
        private CalculationService _calculationService = new CalculationService();
        public CalculationResult lastCalculation;

        public MainWindow()
        {
            InitializeComponent();
        }
        //Подсчет
        public void CalculateButton_Click(object sender, RoutedEventArgs e)
        {
            if (!double.TryParse(WidthTextBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out double width) ||
                !double.TryParse(HeightTextBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out double height))
            {
                MessageBox.Show("Пожалуйста, введите корректные числовые значения для размеров.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var result = _calculationService.Calculate(width, height, AluminumRadio.IsChecked == true);

            if (result == null)
            {
                MessageBox.Show("Размеры должны быть положительными числами.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Сохраняем результат для создания чека
            lastCalculation = result;

            // Обновление UI
            SizeText.Text = $"Размер: {result.Width:F2} м × {result.Height:F2} м = {result.Area:F2} м²";
            MaterialText.Text = $"Материал: {result.Material}";
            CostText.Text = $"Стоимость: {result.TotalCost:F2} руб.";

            HistoryListView.ItemsSource = null;
            HistoryListView.ItemsSource = _calculationService.CalculationHistory;

            CreateReceiptButton.IsEnabled = true;
        }
        //Сохранение ПДФ файла
        public void CreateReceiptButton_Click(object sender, RoutedEventArgs e)
        {
            if (lastCalculation == null)
            {
                MessageBox.Show("Сначала выполните расчет стоимости.", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            string uniqueNumber = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
            string dateString = DateTime.Now.ToString("dd.MM.yy");
            string costString = lastCalculation.TotalCost.ToString("F2", CultureInfo.InvariantCulture);

            // Изменяем расширение на .pdf
            string fileName = $"{uniqueNumber}_{DateTime.Now:yyyyMMdd_HHmmss}_{costString.Replace(".", "_")}.pdf";
            string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "BlindsReceipts");
            string filePath = Path.Combine(folderPath, fileName);
            Directory.CreateDirectory(folderPath);

            // Создаем PDF документ
            CreatePdfReceipt(filePath, uniqueNumber, dateString, lastCalculation);

            MessageBox.Show($"Квитанция сохранена:\n{filePath}", "Успешно",
                MessageBoxButton.OK, MessageBoxImage.Information);

        }
        //Создание ПДФ файла
        public void CreatePdfReceipt(string filePath, string uniqueNumber, string dateString, CalculationResult calculation)
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Create))
            {
                Document document = new Document(PageSize.A4, 25, 25, 30, 30);
                PdfWriter writer = PdfWriter.GetInstance(document, fs);

                document.Open();

                // Настройка шрифтов
                BaseFont baseFont = BaseFont.CreateFont("c:/windows/fonts/arial.ttf", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED);
                Font titleFont = new Font(baseFont, 14, Font.BOLD);
                Font headerFont = new Font(baseFont, 12, Font.BOLD);
                Font regularFont = new Font(baseFont, 10, Font.NORMAL);
                Font smallFont = new Font(baseFont, 8, Font.NORMAL);

                // Заголовок
                Paragraph title = new Paragraph("ООО \"Уютный Дом\"", titleFont);
                title.Alignment = Element.ALIGN_CENTER;
                document.Add(title);

                document.Add(new Paragraph("Добро пожаловать", regularFont));
                document.Add(new Paragraph(" "));

                // Информация о чеке
                document.Add(new Paragraph($"ККМ 00075411     #{uniqueNumber}", regularFont));
                document.Add(new Paragraph($"ИНН 1087746942040", regularFont));
                document.Add(new Paragraph($"ЭКЛЗ 3851495566", regularFont));
                document.Add(new Paragraph($"Чек №_______", regularFont));
                document.Add(new Paragraph($"{DateTime.Now:dd.MM.yy HH:mm} СИС.", regularFont));
                document.Add(new Paragraph(" "));

                // Таблица с товаром
                PdfPTable table = new PdfPTable(2);
                table.WidthPercentage = 100;
                table.SetWidths(new float[] { 70, 30 });

                // Заголовок таблицы
                PdfPCell cell = new PdfPCell(new Phrase("наименование товара", regularFont));
                cell.Border = Rectangle.NO_BORDER;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase("", regularFont));
                cell.Border = Rectangle.NO_BORDER;
                table.AddCell(cell);

                // Данные о жалюзи
                table.AddCell(new Phrase($"жалюзи {calculation.Width:F2}×{calculation.Height:F2} м", regularFont));
                table.AddCell(new Phrase("", regularFont));
                table.AddCell(new Phrase($"материал {calculation.Material}", regularFont));
                table.AddCell(new Phrase("", regularFont));

                // Итоговая стоимость
                string totalCost = calculation.TotalCost.ToString("F2", CultureInfo.InvariantCulture);
                table.AddCell(new Phrase("Итог", regularFont));
                table.AddCell(new Phrase($"={totalCost} руб.", regularFont));
                table.AddCell(new Phrase("Сдача", regularFont));
                table.AddCell(new Phrase("=0 руб.", regularFont));
                table.AddCell(new Phrase("Сумма итого:", headerFont));
                table.AddCell(new Phrase($"={totalCost} руб.", headerFont));
                document.Add(table);
                document.Add(new Paragraph(" "));

                // Разделитель
                document.Add(new Paragraph("************************", regularFont));
                document.Add(new Paragraph("      00003751# 059705", regularFont));
                document.Add(new Paragraph(" "));

                // Дополнительная информация
                document.Add(new Paragraph($"Цена за м²: {(calculation.Material == "Алюминий" ? AluminumPrice : PlasticPrice):F2} руб.", smallFont));
                document.Add(new Paragraph($"Площадь: {calculation.Area:F2} м²", smallFont));
                document.Add(new Paragraph($"Дата расчета: {DateTime.Now:dd.MM.yyyy HH:mm:ss}", smallFont));
                document.Add(new Paragraph($"Номер чека: {uniqueNumber}", smallFont));

                document.Close();
            }
        }
        //Очистка истории
        public void ClearHistoryButton_Click(object sender, RoutedEventArgs e)
        {
            calculationHistory.Clear();
            HistoryListView.ItemsSource = null;
            SizeText.Text = "";
            MaterialText.Text = "";
            CostText.Text = "";
            CreateReceiptButton.IsEnabled = false;
            lastCalculation = null;
        }
    }

    public class CalculationResult
    {
        public double Width { get; set; }
        public double Height { get; set; }
        public double Area { get; set; }
        public string Material { get; set; } = "";
        public double TotalCost { get; set; }
    }

    public class CalculationRecord
    {
        public DateTime Date { get; set; }
        public string Size { get; set; } = "";
        public string Material { get; set; } = "";
        public double Cost { get; set; }
    }
}