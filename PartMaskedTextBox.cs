using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PartMaskedTextBox
{
    // 1.不允許使用者使用delete鍵刪除資料
    // 2.當使用者按下Backspace時,會從最後一個字元開始刪除,且一次只允許刪除一個字元
    /// <summary>
    /// 
    /// </summary>
    public partial class PartMaskedTextBox : TextBox
    {
        private const Int32 WM_CHAR = 0x0102;

        public PartMaskedTextBox()
        {
            InitializeComponent();
            base.ShortcutsEnabled = false;//關閉鍵盤快速鍵
            this.MaskChar = '*';
            this.MaskStartIndex = -1;
            this.MaskLength = 0;
        }
        /// <summary>
        /// 取得或設定遮蔽用字元,預設為*
        /// </summary>
        [Description("取得或設定遮蔽用字元,預設為*")]
        public char MaskChar { get; set; }
        /// <summary>
        /// 從第幾個字元後開始遮蔽,預設為-1(不遮蔽)
        /// </summary>
        [Description("從第幾個字元後開始遮蔽,預設為-1(不遮蔽)")]
        public int MaskStartIndex { get; set; }
        /// <summary>
        /// 要遮蔽多少個字
        /// </summary>
        [Description("取得或設定要遮蔽多少個字")]
        public int MaskLength { get; set; }
        //記錄原始value
        StringBuilder sBuilder = new StringBuilder();

        //重寫Text屬性
        //設定時將Text Value加上遮罩,取得時則是取回原始value
        public new string Text
        {
            get
            {
                return sBuilder.ToString();
            }
            set
            {
                //先清除
                base.Text = string.Empty;
                sBuilder.Clear();

                if (!string.IsNullOrEmpty(value))
                {
                    base.Text = GetMaskText(value);
                    sBuilder.Append(value);
                }
            }
        }

        protected override void WndProc(ref Message m)
        {
            int keyChar = m.WParam.ToInt32();
            switch (m.Msg)
            {
                case WM_CHAR:
                    //不允許複製貼上剪下,以免StringBuilder記錄的值出現錯誤
                    bool backspace = keyChar == Keys.Back.GetHashCode()
                        , delete = keyChar == Keys.Delete.GetHashCode()
                        ;
                    if (backspace)
                    {
                        string strBeforeIns = sBuilder.ToString().Substring(0, base.SelectionStart)
                                , strAfterIns = sBuilder.ToString().Substring(base.SelectionStart + base.SelectionLength)
                                ;
                        if (base.SelectionLength == 0 && strBeforeIns.Trim().Length > 0)
                            strBeforeIns = strBeforeIns.Substring(0, strBeforeIns.Length - 1);
                        sBuilder.Clear();
                        sBuilder.AppendFormat("{0}{1}", strBeforeIns, strAfterIns);
                    }
                    else if (delete)
                    {
                        string strBeforeIns = sBuilder.ToString().Substring(0, base.SelectionStart)
                                , strAfterIns = sBuilder.ToString().Substring(base.SelectionStart + base.SelectionLength)
                                ;
                        if (base.SelectionLength == 0 && strAfterIns.Trim().Length > 0)
                            strAfterIns = strAfterIns.Substring(1);
                        sBuilder.Clear();
                        sBuilder.AppendFormat("{0}{1}", strBeforeIns, strAfterIns);
                    }
                    else
                    {
                        if (sBuilder.Length < base.MaxLength)
                        {
                            char ch = (char)keyChar;
                            if (IsValidChar(ch) && MaskStartIndex >= 0)
                            {
                                if (char.IsLower(ch))
                                    ch = char.ToUpper(ch);//小寫轉大寫
                                //從中間插入字元,而且有可能有取代字串
                                string strBeforeIns = sBuilder.ToString().Substring(0, base.SelectionStart)
                                    , strAfterIns = sBuilder.ToString().Substring(base.SelectionStart + base.SelectionLength)
                                    ;
                                sBuilder.Clear();
                                sBuilder.Append(strBeforeIns);
                                sBuilder.Append(ch);
                                sBuilder.Append(strAfterIns);
                                if (base.SelectionStart >= this.MaskStartIndex && base.SelectionStart <= this.MaskStartIndex + MaskLength)
                                    m.WParam = new IntPtr((int)MaskChar);
                            }
                            else
                            {
                                m.WParam = IntPtr.Zero;
                            }
                        }
                        else
                        {
                            m.WParam = IntPtr.Zero;
                        }
                    }
                    break;
                default: break;
            }
            base.WndProc(ref m);
        }

        /// <summary>
        /// 判斷輸入字元是否合乎規則
        /// </summary>
        /// <param name="ch"></param>
        /// <returns></returns>
        private bool IsValidChar(char ch)
        {
            return char.IsLetterOrDigit(ch);//目前允許文數字
        }

        /// <summary>
        /// 取得加上遮罩後的文字
        /// </summary>
        /// <param name="strValue"></param>
        /// <returns></returns>
        private string GetMaskText(string strValue)
        {
            StringBuilder sbMasked = new StringBuilder();
            foreach (char ch in strValue.ToCharArray())
            {
                if (sbMasked.Length >= base.MaxLength) break;//當超出最大字元數時跳出
                if (IsValidChar(ch) && this.MaskStartIndex >= 0)
                {
                    if (sbMasked.Length >= this.MaskStartIndex 
                        && 
                        sbMasked.Length < this.MaskStartIndex + MaskLength)
                        sbMasked.Append(this.MaskChar);
                    else
                        sbMasked.Append(ch);
                }
            }
            return sbMasked.ToString();
        }
    }
}
