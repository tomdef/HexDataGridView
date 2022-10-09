using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WinFormsTest
{
    public partial class HexDataGridViewControl : UserControl
    {
        private readonly int rowIndexHex = 0;
        private readonly int rowIndexAscii = 1;
        private readonly string charToMeasure = "0";

        private Func<byte, string> byteToHexString;
        private Func<byte, string> byteToString;
        private Func<string, byte> stringToByte;

        public Encoding DefaultEncoding { get; set; } = Encoding.GetEncoding(1250);

        public HexDataGridViewControl()
        {
            InitializeComponent();
            InitializeEvents();

            byteToHexString = ByteToHexString;
            byteToString = ByteToString;
            stringToByte = StringToByte;
            Padding = new Padding(5);
        }

        public void SetupGridView(byte[] data)
        {
            dataGridView.AutoGenerateColumns = false;
            dataGridView.RowHeadersVisible = false;

            DataGridViewCellStyle defaultCellStyle = new DataGridViewCellStyle(dataGridView.DefaultCellStyle)
            {
                Alignment = DataGridViewContentAlignment.MiddleCenter,
                Font = new Font(FontFamily.GenericMonospace, 9)
            };

            DataGridViewCellStyle defaultHeaderCell = new DataGridViewCellStyle()
            {
                Alignment = DataGridViewContentAlignment.MiddleCenter,
                Font = new Font(FontFamily.GenericMonospace, 9),
                BackColor  = SystemColors.ActiveCaption,
                ForeColor = SystemColors.ActiveCaptionText,
            };

            Size cellSize = TextRenderer.MeasureText(charToMeasure, defaultCellStyle.Font);
            cellSize.Width += 8;
            cellSize.Height += 8;

            DataGridViewRow rowHex = new DataGridViewRow()
            {
                DefaultCellStyle = defaultCellStyle,
                Height = cellSize.Height
            };

            DataGridViewRow rowAscii = new DataGridViewRow()
            {
                DefaultCellStyle = defaultCellStyle,
                Height = cellSize.Height
            };

            // column style
            dataGridView.ColumnHeadersHeight = (cellSize.Height * 3);

            // fill

            int index = 0;
            foreach (byte b in data)
            {
                index++;
                DataGridViewColumn column = new DataGridViewColumn()
                {
                    HeaderText = (index + 999).ToString(),
                    ValueType = typeof(byte),
                    CellTemplate = new DataGridViewTextBoxCell(),
                    Width = cellSize.Width,                    
                    DefaultCellStyle = defaultCellStyle,
                };

                dataGridView.Columns.Add(column);

                rowHex.Cells.Add(new DataGridViewTextBoxCell()
                {
                    Value = b,
                    MaxInputLength = 2,
                });

                rowAscii.Cells.Add(new DataGridViewTextBoxCell()
                {
                    Value = b,
                    MaxInputLength = 1
                });
            }

            dataGridView.Rows.Add(rowHex);
            dataGridView.Rows.Add(rowAscii);
        }

        private void InitializeEvents()
        {
            dataGridView.CellParsing += CellParsing;
            dataGridView.CellValidating += CellValidating;
            dataGridView.CellFormatting += CellFormatting;
        }

        private void CellParsing(object sender, DataGridViewCellParsingEventArgs e)
        {
            if (e.RowIndex == rowIndexHex)
            {
                if (byte.TryParse(
                    e.Value.ToString(), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out byte bValue) == true)
                {
                    e.Value = bValue;
                    e.ParsingApplied = true;

                    UpdateAsciiValue(e.ColumnIndex, bValue);
                }
                else
                {
                    e.ParsingApplied = false;
                }
            }
            else
            {
                byte bValue = stringToByte(e.Value.ToString());
                e.Value = bValue;
                e.ParsingApplied = true;

                UpdateHexValue(e.ColumnIndex, bValue);
            }
        }

        private void CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            if (e.RowIndex == rowIndexHex)
            {
                var valueIsValid = byte.TryParse(
                    e.FormattedValue.ToString(), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out byte bValue);

                e.Cancel = !valueIsValid;
            }
        }

        private void CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.Value == null)
                return;

            try
            {
                byte bValue = (byte)e.Value;

                e.Value = (e.RowIndex == rowIndexHex)
                    ? byteToHexString(bValue)
                    : byteToString(bValue);

                e.FormattingApplied = true;
            }
            catch
            {
                e.FormattingApplied = false;
            }
        }

        private void UpdateAsciiValue(int columnIndex, byte value)
        {
            dataGridView.CellParsing -= CellParsing;
            dataGridView.Rows[rowIndexAscii].Cells[columnIndex].Value = value;
            dataGridView.CellParsing += CellParsing;
        }

        private void UpdateHexValue(int columnIndex, byte value)
        {
            dataGridView.CellParsing -= CellParsing;
            dataGridView.Rows[rowIndexHex].Cells[columnIndex].Value = value;
            dataGridView.CellParsing += CellParsing;
        }

        #region DefaultConversions

        private string ByteToHexString(byte value)
        {
            return value.ToString("X2");
        }

        private string ByteToString(byte value)
        {
            return DefaultEncoding.GetString(new byte[] { value }).Substring(0, 1); ;
        }

        private byte StringToByte(string value)
        {
            return DefaultEncoding.GetBytes(value).FirstOrDefault();
        }

        #endregion DefaultConversions
    }
}
