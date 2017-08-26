using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using NppPIALexer2.Tag;

namespace NppPIALexer2.Forms
{
    public partial class frmSettings : Form
    {
        public frmSettings()
        {
            InitializeComponent();
        }

        private void frmSettings_Load(object sender, EventArgs e)
        {
            _Init();
            _BindSupportedLanguage();
        }

        void _Init()
        {
            chkEnableAutoC.Checked = Config.Instance.AutoCompletion;
            cboxLangList.Items.Clear();
            foreach (Language lang in TagParser.GetSupportedLanguage)
                cboxLangList.Items.Add(lang);
            foreach (string strLang in Config.Instance.AutoApplyTemplateLangs.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                try
                {
                    Language lang = (Language)Enum.Parse(typeof(Language), strLang);
                    for (int i = 0; i < cboxLangList.Items.Count; ++i)
                        if ((Language)cboxLangList.Items[i] == lang)
                        {
                            cboxLangList.SetItemChecked(i, true);
                            break;
                        }
                }
                catch { }
            }
            cboxLangList.Enabled = chkEnableAutoC.Checked;

            chkEnableTemplate.Checked = Config.Instance.AutoApplyTemplate;
            btnEditTemplate.Enabled = chkEnableTemplate.Checked;
        }

        private void chkEnableAutoC_CheckedChanged(object sender, EventArgs e)
        {
            cboxLangList.Enabled = chkEnableAutoC.Checked;
        }

        private void chkEnableTemplate_CheckedChanged(object sender, EventArgs e)
        {
            btnEditTemplate.Enabled = chkEnableTemplate.Checked;
        }

        private void btnEditTemplate_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(Config.Instance.TemplateDir))
                Directory.CreateDirectory(Config.Instance.TemplateDir);
            System.Diagnostics.Process.Start("explorer.exe", Config.Instance.TemplateDir);
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            Config.Instance.AutoCompletion = chkEnableAutoC.Checked;
            StringBuilder sb = new StringBuilder();
            foreach (int index in cboxLangList.CheckedIndices)
            {
                if (sb.Length == 0)
                    sb.Append(((Language)cboxLangList.Items[index]).ToString());
                else
                    sb.Append("," + ((Language)cboxLangList.Items[index]).ToString());
            }
            Config.Instance.AutoApplyTemplateLangs = sb.ToString();
            Config.Instance.AutoApplyTemplate = chkEnableTemplate.Checked;
            Config.Instance.Save();
            Hide();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Hide();
        }

        private void frmSettings_VisibleChanged(object sender, EventArgs e)
        {
            _Init();
        }

        private void frmSettings_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        void _BindSupportedLanguage()
        {
            lboxLangList.Items.Clear();
            foreach (Language lang in TagParser.GetSupportedLanguage)
            {
                lboxLangList.Items.Add(lang);
            }
        }

        private void lboxLangList_SelectedIndexChanged(object sender, EventArgs e)
        {
            lboxExtList.Items.Clear();
            if (lboxLangList.SelectedItem != null)
            {
                Language lang = (Language)lboxLangList.SelectedItem;
                foreach (string ext in TagParser.Ext2Lang.Keys)
                    if (TagParser.Ext2Lang[ext] == lang)
                        lboxExtList.Items.Add(ext);
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            string ext = txtExt.Text.Trim();
            if (string.IsNullOrEmpty(ext))
                return;
            if (lboxLangList.SelectedIndex == -1)
            {
                Utility.Error("No language selected.");
                return;
            }

            if (!ext.StartsWith("."))
                ext = "." + ext;
            ext = ext.ToLower();
            if (TagParser.Ext2Lang.ContainsKey(ext))
            {
                Utility.Error("Extension '{0}' has mapped to {1}", ext, TagParser.Ext2Lang[ext]);
                return;
            }
            TagParser.Ext2Lang[ext] = (Language)lboxLangList.SelectedItem;
            TagParser.SaveExt2LanguageMap();
            lboxExtList.Items.Add(ext);
            txtExt.Text = "";
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (lboxExtList.SelectedIndex == -1)
                return;
            string ext = (string)lboxExtList.SelectedItem;
            TagParser.Ext2Lang.Remove(ext);
            TagParser.SaveExt2LanguageMap();
            lboxExtList.Items.RemoveAt(lboxExtList.SelectedIndex);
        }
    }
}
