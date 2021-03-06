﻿using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Forms;
using static PKHeX.Util;
using static Pk3DSRNGTool.StringItem;

namespace Pk3DSRNGTool
{
    public partial class MainForm : Form
    {
        private void B_EggReset_Click(object sender, EventArgs e)
        {
            IV_Male = new[] { 31, 31, 31, 31, 31, 31 };
            IV_Female = new[] { 31, 31, 31, 31, 31, 31 };
            Egg_GenderRatio.SelectedIndex = 1;
            Homogeneity.Checked = NidoType.Checked = MM.Checked =
            M_ditto.Checked = F_ditto.Checked = false;
            M_Items.SelectedIndex = F_Items.SelectedIndex =
            M_ability.SelectedIndex = F_ability.SelectedIndex = 0;
        }

        private void B_Fast_Click(object sender, EventArgs e)
        {
            B_EggReset_Click(null, null);
            IV_Female = new int[6];
            M_Items.SelectedIndex = 2;
            MM.Checked = Homogeneity.Checked = true;
        }

        private void Ditto_CheckedChanged(object sender, EventArgs e)
        {
            if ((sender as CheckBox)?.Checked ?? false)
            {
                (sender == F_ditto ? M_ditto : F_ditto).Checked =
                Homogeneity.Enabled = Homogeneity.Checked = false;
            }
            else
                Homogeneity.Enabled = !(NidoType.Checked || M_ditto.Checked || F_ditto.Checked || (int)Egg_GenderRatio.SelectedValue == 254);
        }

        private void NidoType_CheckedChanged(object sender, EventArgs e)
        {
            Homogeneity.Enabled = !(NidoType.Checked || M_ditto.Checked || F_ditto.Checked);
            if (NidoType.Checked)
                Homogeneity.Checked = false;
        }

        private void Egg_GenderRatio_SelectedIndexChanged(object sender, EventArgs e)
        {
            var gender = (int)Egg_GenderRatio.SelectedValue;
            M_ditto.Enabled = F_ditto.Enabled = true;
            NidoType.Checked = false;
            NidoType.Enabled = gender == 127;
            switch (gender)
            {
                case 000:
                case 255:
                    M_ditto.Enabled = F_ditto.Enabled = false; F_ditto.Checked = true; return;
                case 254:
                    F_ditto.Enabled = F_ditto.Checked = false; Homogeneity.Checked = Homogeneity.Enabled = false; return;
                default:
                    M_ditto.Checked = F_ditto.Checked = false; return;
            }
        }

        private void MM_CheckedChanged(object sender, EventArgs e)
        {
            if (Method != 3)
                return;
            bool mainrng = !(ShinyCharm.Checked || MM.Checked);
            if ((MainRNGEgg.Visible = mainrng && Gen7) && MainRNGEgg.Checked)
            {
                NPC.Value = 4;
                Timedelay.Value = 38;
                return;
            }
            if (Gen6)
            {
                if (mainrng && sender != RB_Reject && sender != RB_Accept) RB_Accept.Checked = true;
                Timedelay.Value = RB_Accept.Checked ? 16 : 0;
            }
        }

        private void B_Backup_Click(object sender, EventArgs e)
        {
            string backupfile = "Backup_" + DateTime.Now.ToString("yyMMdd_HHmmss") + ".txt";
            File.WriteAllLines(backupfile, new[] { St3.Text, St2.Text, St1.Text, St0.Text, });
            Alert(backupfile + " saved");
        }

        private void B_Load_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog OFD = new OpenFileDialog();
                DialogResult result = OFD.ShowDialog();
                if (result == DialogResult.OK)
                {
                    string file = OFD.FileName;
                    if (File.Exists(file))
                    {
                        string[] list = File.ReadAllLines(file);

                        uint.TryParse(list[3], System.Globalization.NumberStyles.HexNumber, null, out uint s0);
                        uint.TryParse(list[2], System.Globalization.NumberStyles.HexNumber, null, out uint s1);
                        uint.TryParse(list[1], System.Globalization.NumberStyles.HexNumber, null, out uint s2);
                        uint.TryParse(list[0], System.Globalization.NumberStyles.HexNumber, null, out uint s3);
                        Status = new[] { s0, s1, s2, s3 };
                    }
                }
            }
            catch
            {
                Error(FILEERRORSTR[lindex]);
            }
        }

        private void B_TSVList_Click(object sender, EventArgs e)
        {
            var editor = new TSVListForm(List2str(OtherTSVList));
            editor.ShowDialog();
            if (editor.other_tsv.Count > 0)
                OtherTSVList = editor.other_tsv;
            Properties.Settings.Default.TSVList = List2str(OtherTSVList);
            Properties.Settings.Default.Save();
        }

        private void Loadlist(string tsvstr)
        {
            OtherTSVList.Clear();
            try
            {
                string[] lines = tsvstr.Split(',');
                for (int i = 0; i < lines.Length; i++)
                {
                    if (!int.TryParse(lines[i], out int val))
                        continue;

                    if (0 > val || val > 4095)
                        continue;

                    OtherTSVList.Add(val);
                }
            }
            catch
            {
            }
        }

        private string List2str(List<int> list)
        {
            return string.Join(",", list.Select(i => i.ToString()).ToArray());
        }

        public void SyncGen7EggSeed(object sender, EventArgs e)
        {
            try
            {
                if (!NTRHelper.ntrclient?.Connected ?? true)
                {
                    Program.mainform.TryToConnectNTR(false);
                    return;
                }
                NTRHelper.ntrclient.ReadTiny("EggSeed");
            }
            catch
            {
            }
        }

        private string getEggListString(int eggnum, int rejectnum, bool path = false)
        {
            string tmp = "";
            if (eggnum < 0)
            {
                switch (lindex)
                {
                    case 0: return "Egg number is too small";
                    case 1: return "Egg number is too small";
                    case 2: return "Egg number is too small";
                    case 3: return "蛋数范围太小";
                }
            }
            switch (lindex)
            {
                case 0: tmp += $"Accept {eggnum} eggs"; break;
                case 1: tmp += $"Accept {eggnum} eggs"; break;
                case 2: tmp += $"Accept {eggnum} eggs"; break;
                case 3: tmp += $"接受 {eggnum} 个蛋"; break;
            }
            if (rejectnum == 0)
                return tmp;
            switch (lindex)
            {
                case 0: tmp += path ? $".\nReject {rejectnum} times" : $",\nand then reject {rejectnum} times"; break;
                case 1: tmp += path ? $".\nReject {rejectnum} times" : $",\nand then reject {rejectnum} times"; break;
                case 2: tmp += path ? $".\nReject {rejectnum} times" : $",\nand then reject {rejectnum} times"; break;
                case 3: tmp += path ? $",\n拒绝 {rejectnum} 次" : $",\n然后拒绝 {rejectnum} 次"; break;
            }
            return tmp;
        }

        #region DGV menu

        private void DGV_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                if (e.Button == MouseButtons.Right)
                {
                    var hti = DGV.HitTest(e.X, e.Y);
                    DGV.ClearSelection();
                    DGV.CurrentCell = DGV.Rows[hti.RowIndex].Cells[hti.ColumnIndex];
                }
            }
            catch { }
        }

        private void SetAsCurrent_Click(object sender, EventArgs e)
        {
            try
            {
                if (Gen6 && dgv_rand64.Visible)
                {
                    ulong key = (ulong)DGV.CurrentRow.Cells["dgv_rand64"].Value;
                    Key1.Value = (uint)(key >> 32);
                    Key0.Value = (uint)key;
                    return;
                }
                var seed = (string)DGV.CurrentRow.Cells["dgv_tinystate"].Value;
                if (Gen7)
                    Status = FuncUtil.SeedStr2Array(seed) ?? Status;
            }
            catch (NullReferenceException)
            {
                Error(NOSELECTION_STR[lindex]);
            }
        }

        private void SetAsAfter_Click(object sender, EventArgs e)
        {
            try
            {
                var seed = (string)DGV.CurrentRow.Cells["dgv_tinystate"].Value;
                int adv = (Frames[DGV.CurrentRow.Index].rt as Core.EggResult).FramesUsed;
                uint[] St = FuncUtil.SeedStr2Array(seed);
                var tmt = new RNG.TinyMT(St);
                for (int i = adv; i > 0; i--)
                    tmt.Next();
                Status = tmt.status;
            }
            catch
            { }
        }

        private void B_Template_Click(object sender, EventArgs e)
        {
            IVTemplate newform = new IVTemplate();
            newform.Show();
        }

        private void ConsiderOtherTSV_CheckedChanged(object sender, EventArgs e)
        {
            if (ConsiderOtherTSV.Checked) ShinyOnly.Checked = true;
        }
        #endregion
    }
}
