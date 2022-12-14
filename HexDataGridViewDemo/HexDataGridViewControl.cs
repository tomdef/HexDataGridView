using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
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

        private readonly Font defaultFont = new Font(FontFamily.GenericMonospace, 9);
        private readonly Font defaultHeaderFont = new Font(FontFamily.GenericMonospace, 8);
        private DataGridViewCellStyle defaultCellStyle;
        private DataGridViewCellStyle defaultHexCellStyle;
        private DataGridViewCellStyle defaultAsciiCellStyle;
        private DataGridViewCellStyle defaultHeaderCellStyle;
        private Size cellSize;
        private int maxOffsetLength;

        private DataGridViewRow rowHex;
        private DataGridViewRow rowAscii;

        private StringFormat sf = new StringFormat()
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center,
        };

        [Browsable(false)]
        public Encoding DefaultEncoding { get; set; } = Encoding.GetEncoding(1250);

        public IEnumerable<byte> GetHexValue()
        {
            foreach (DataGridViewTextBoxCell cell in rowHex.Cells)
            {
                yield return (byte)cell.Value;
            }
        }

        public HexDataGridViewControl()
        {
            InitializeComponent();

            InitializeGridViewStyle();
            InitializeEvents();

            DoubleBuffered = true;
            SetDoubleBuffer(this.dataGridView, true);

            byteToHexString = ByteToHexString;
            byteToString = ByteToString;
            stringToByte = StringToByte;
        }

        public void SetupGridView(long beginOffset, byte[] data)
        {
            SuspendLayout();

            int index = 0;
            foreach (byte b in data)
            {
                index++;
                DataGridViewColumn column = new DataGridViewColumn()
                {
                    HeaderText = (index + beginOffset).ToString(),
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
                    Style = defaultHexCellStyle
                });

                rowAscii.Cells.Add(new DataGridViewTextBoxCell()
                {
                    Value = b,
                    MaxInputLength = 1,
                    Style = defaultAsciiCellStyle
                });
            }

            dataGridView.Rows.Add(rowHex);
            dataGridView.Rows.Add(rowAscii);


            // column style
            maxOffsetLength = (beginOffset + data.Length).ToString().Length;
            dataGridView.ColumnHeadersHeight = (cellSize.Height * maxOffsetLength);

            ResumeLayout();
        }

        private void InitializeGridViewStyle()
        {
            defaultCellStyle = new DataGridViewCellStyle(dataGridView.DefaultCellStyle)
            {
                Alignment = DataGridViewContentAlignment.MiddleCenter,
                Font = defaultFont
            };

            defaultHexCellStyle = new DataGridViewCellStyle(defaultCellStyle)
            {
                BackColor = SystemColors.Info,
                ForeColor = SystemColors.InfoText
            };

            defaultAsciiCellStyle = new DataGridViewCellStyle(defaultCellStyle);

            defaultHeaderCellStyle = new DataGridViewCellStyle(defaultCellStyle)
            {
                BackColor = SystemColors.Control,
                ForeColor = SystemColors.ControlText,
                Font = defaultHeaderFont
            };

            cellSize = TextRenderer.MeasureText(charToMeasure, defaultCellStyle.Font);
            cellSize.Width += 6;
            cellSize.Height += 6;

            dataGridView.AutoGenerateColumns = false;
            dataGridView.RowHeadersVisible = false;
            dataGridView.ColumnHeadersDefaultCellStyle = defaultHeaderCellStyle;

            rowHex = new DataGridViewRow()
            {
                DefaultCellStyle = defaultHexCellStyle,
                Height = cellSize.Height * 2
            };

            rowAscii = new DataGridViewRow()
            {
                DefaultCellStyle = defaultAsciiCellStyle,
                Height = cellSize.Height
            };
        }

        private void InitializeEvents()
        {
            dataGridView.CellParsing += CellParsing;
            dataGridView.CellValidating += CellValidating;
            dataGridView.CellFormatting += CellFormatting;
            dataGridView.CellPainting += CellPainting;

            dataGridView.CellEndEdit += (s, e) =>
            {
                int columnindex = e.ColumnIndex + 1;
                if (columnindex < dataGridView.ColumnCount)
                {
                    dataGridView.Rows[e.RowIndex].Cells[columnindex].Selected = true;
                }
            };
        }
        static void SetDoubleBuffer(Control dgv, bool DoubleBuffered)
        {
            typeof(Control).InvokeMember("DoubleBuffered",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty,
                null, dgv, new object[] { DoubleBuffered });
        }

        // -------------------------------------------------------------------------

        #region DataGridView Events

        private void CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex == 0)
            {
                DrawHexCell(e);
                e.Handled = true;
            }
            else
            {
                if (e.RowIndex == -1)
                {
                    DrawHeaderCell(e);
                    e.Handled = true;
                }
            }
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
                if (e.RowIndex >= 0)
                {
                    byte bValue = (byte)e.Value;

                    e.Value = (e.RowIndex == rowIndexHex)
                        ? byteToHexString(bValue)
                        : byteToString(bValue);

                    e.FormattingApplied = true;
                }
            }
            catch
            {
                e.FormattingApplied = false;
            }
        }

        #endregion DataGridView Events


        #region DataGridView Draw

        private void DrawHexCell(DataGridViewCellPaintingEventArgs e)
        {
            var val = e.FormattedValue.ToString();

            if (val.Length == 2)
            {
                bool isSelected = (e.State & DataGridViewElementStates.Selected) == DataGridViewElementStates.Selected;
                e.PaintBackground(e.CellBounds, isSelected);

                SizeF charSize = e.Graphics.MeasureString(charToMeasure, defaultHexCellStyle.Font);
                float cellHalfHeight = e.CellBounds.Height / 2;
                int m = 0;

                PointF p1 = new PointF(e.CellBounds.X + m, e.CellBounds.Y + m);
                PointF p2 = new PointF(e.CellBounds.X + m, e.CellBounds.Y + m + charSize.Height);

                RectangleF r1 = new RectangleF(p1, new SizeF(e.CellBounds.Width, cellHalfHeight));
                RectangleF r2 = new RectangleF(p2, new SizeF(e.CellBounds.Width, cellHalfHeight));

                e.Graphics.DrawString(
                    val[0].ToString(), defaultHexCellStyle.Font, new SolidBrush(defaultHexCellStyle.ForeColor), r1, sf);

                e.Graphics.DrawString(
                    val[1].ToString(), defaultHexCellStyle.Font, new SolidBrush(defaultHexCellStyle.ForeColor), r2, sf);
            }
        }

        private void DrawHeaderCell(DataGridViewCellPaintingEventArgs e)
        {
            var val = e.FormattedValue.ToString().PadLeft(maxOffsetLength, '0');

            if (val.Length > 0)
            {
                SuspendLayout();
                
                bool isSelected = (e.State & DataGridViewElementStates.Selected) == DataGridViewElementStates.Selected;
                e.PaintBackground(e.CellBounds, isSelected);
                RectangleF backRect = new RectangleF(e.CellBounds.X + 1, e.CellBounds.Y + 1, e.CellBounds.Width - 2, e.CellBounds.Height - 2);
                e.Graphics.FillRectangle(new SolidBrush(defaultHeaderCellStyle.BackColor), backRect);
                
                int m = 0;
                SizeF charSize = e.Graphics.MeasureString(charToMeasure, defaultHeaderCellStyle.Font);
                float cellCharHeight = (e.CellBounds.Height - (val.Length * m)) / (val.Length);
                
                PointF p = new PointF(e.CellBounds.X, e.CellBounds.Y + (m * 2));

                for (int i = 0; i < val.Length; i++)
                {
                    RectangleF r = new RectangleF(p, new SizeF(e.CellBounds.Width, cellCharHeight));

                    e.Graphics.DrawString(
                        val[i].ToString(), defaultHeaderCellStyle.Font, new SolidBrush(defaultHeaderCellStyle.ForeColor), r, sf);


                    p = new PointF(p.X, p.Y + cellCharHeight);
                }
                ResumeLayout();
            }
        }

        #endregion DataGridView Draw

        #region Private methods

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

        #endregion Private methods

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
