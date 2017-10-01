using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace NppPIALexer2.Forms
{
    public partial class frmLogList : Form {
        delegate void UpdateViewDelegate();
        UpdateViewDelegate _updateView;

        public frmLogList() {
            InitializeComponent();

            lviewTaskList.Columns.Add("Time", -2, HorizontalAlignment.Left);
            lviewTaskList.Columns.Add("Severity", -2, HorizontalAlignment.Left);
            lviewTaskList.Columns.Add("Source", -2, HorizontalAlignment.Left);
            lviewTaskList.Columns.Add("Info", -2, HorizontalAlignment.Left);

            _updateView = new UpdateViewDelegate(_UpdateView);
            Log.getInstance().EvtLogChanged += new Log.LogChanged(OnLogChanged);
        }

        //called by Event
        void OnLogChanged() {
            while(!this.IsHandleCreated)
                Thread.Sleep(1);
            this.Invoke(_updateView);
        }

        void _UpdateView() {
            Utility.Debug("update log view: ");
            lviewTaskList.Items.Clear();
            //TODO make threadsafe
            List<Log.LogData>.Enumerator x = Log.getInstance().GetMessages().GetEnumerator();
            while(x.MoveNext()) {
                String[] y = {x.Current.Time.ToShortTimeString(),
                   x.Current.Severity.ToString(),x.Current.Source, x.Current.Message };
                lviewTaskList.Items.Insert(0,new ListViewItem(y));
            }
        }

        private void toolStripButton2_Click(object sender, EventArgs e) {
            Log.getInstance().Clear();
        }
    }
    public class Log {
        private Log() {
            m_Data = new List<LogData>();
        }
        static Log s_Instance;
        static public Log getInstance() {
            if(s_Instance == null) {
                s_Instance = new Log();
            }
            return s_Instance;
        }
        public struct LogData {
            public String Message;
            public DateTime Time;
            public EnuSeverity Severity;
            public String Source;
        }
        public enum EnuSeverity { Error = -1, Warn = 1, Info = 2 };

        protected List<LogData> m_Data;
        protected int m_LastID = 1;
        public void Add(String Message, EnuSeverity Severity, String Source) {
            LogData _l = new LogData();
            _l.Message = Message;
            _l.Severity = Severity;
            _l.Time = DateTime.Now;
            _l.Source = Source;
            if(m_Data.Count > 1000) {
                m_Data.RemoveRange(0,900);
            }

            m_Data.Add(_l);
            if (EvtLogChanged!=null) EvtLogChanged();
        }

        public List<LogData> GetMessages() {
            return m_Data;
        }
        public void Clear() {
            m_Data.Clear();
        }
        public event LogChanged EvtLogChanged;
        public delegate void LogChanged();

    }
}
