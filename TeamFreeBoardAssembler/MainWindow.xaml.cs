using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TeamFreeBoardAssembler
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Dictionary<string, Instruction> instructions = new Dictionary<string, Instruction>();
        List<string> one_arg = new List<string>() { "di", "ei", "retx", "wait", "scond", "bcond", "jcond" };
        Dictionary<string, string> variables = new Dictionary<string, string>();
        Dictionary<string, int> labels = new Dictionary<string, int>();
        int replace_lines = 0;

        public MainWindow()
        {
            InitializeComponent();
            InitializeInstructions();
            checkBox.IsChecked = true;
            AssemblyCode.Text = "";
            AssemblyCode.AcceptsReturn = true;
            AssemblyCode.AcceptsTab = true;
            BinaryCode.Text = "";
            BinaryCode.AcceptsReturn = true;
            BinaryCode.AcceptsTab = true;
        }

        private void InitializeInstructions()
        {
            instructions["add"] = new Instruction("0000", "0101");
            instructions["addi"] = new Instruction("0101", "");
            instructions["addu"] = new Instruction("0000", "0110");
            instructions["addui"] = new Instruction("0110", "");
            instructions["mult"] = new Instruction("0000", "1110");
            instructions["multi"] = new Instruction("1110", "");
            instructions["sub"] = new Instruction("0000", "1001");
            instructions["subi"] = new Instruction("1001", "");
            instructions["cmp"] = new Instruction("0000", "1011");
            instructions["cmpi"] = new Instruction("1011", "");
            instructions["cmpu"] = new Instruction("1000", "1000");
            instructions["cmpui"] = new Instruction("1000", "");
            instructions["and"] = new Instruction("0000", "0001");
            instructions["andi"] = new Instruction("0001", "");
            instructions["or"] = new Instruction("0000", "0010");
            instructions["ori"] = new Instruction("0010", "");
            instructions["xor"] = new Instruction("0000", "0011");
            instructions["xori"] = new Instruction("0011", "");
            instructions["mov"] = new Instruction("0000", "1101");
            instructions["movi"] = new Instruction("1101", "");
            //instructions["lui"] = new Instruction("1111", "");
            instructions["load"] = new Instruction("0100", "0000");
            instructions["store"] = new Instruction("0100", "0100");
            instructions["bcond"] = new Instruction("1100", ""); //? //if 0 in cond absolute, 1 overflow, 2 input 1 > input2, 3 input1 == input2 >>>> 11-8 bits 
            instructions["jcond"] = new Instruction("0100", "1100");
            instructions["accel"] = new Instruction("0000", "0000");
            //instructions["jal"] = new Instruction("0100", "1000");
            instructions["wait"] = new Instruction("0000", "0000");
        }
        static string current = "";
        static int b = 0;
        private void button_Click(object sender, RoutedEventArgs e)
        {
            label.Content = "";
            variables = new Dictionary<string, string>();
            labels = new Dictionary<string, int>();
            pc = 0;
            replace_lines = 1;
            try
            {
                int b = 0;
                string current = "";
                for (int i = 0; i < AssemblyCode.LineCount; i++)
                {
                    String line = AssemblyCode.GetLineText(i);
                    string[] temp = line.Split('#');
                    line = temp[0];
                    current = line;
                    b = i;
                    line = line.Replace("\n", "");
                    line = line.Replace("\t", "");
                    line = line.Replace("\r", "");
                    line = line.Replace(",", "");
                    if (line == "" || line == " ")
                        continue;
                    IsInstruction(line, i);

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("line number: " + b + "\ninstruction " + current + "\nmessage: " + ex.Message);
                MessageBox.Show(ex.ToString());
            }
            pc = 0;
            try {
                BinaryCode.Text = "";
                for (int i = 0; i < AssemblyCode.LineCount; i++)
                {
                    String line = AssemblyCode.GetLineText(i);
                    if (i > 80)
                        Console.WriteLine("dkjf");
                    string[] temp = line.Split('#');
                    line = temp[0];
                    current = line;
                    b = i;
                    line = line.Replace("\n", "");
                    line = line.Replace("\t", "");
                    line = line.Replace("\r", "");
                    line = line.Replace(",", "");
                    if (line == "" || line == " ")
                        continue;
                    string bin = ProcessLine(line, i+1);
                    if (bin != "") { 
                        //BinaryCode.Text += bin + "\n";
                        string real_bin = "";
                        int count = 0;
                        if (checkBox.IsChecked == false)
                        {
                            foreach (char c in bin)
                            {
                                if (count == 4)
                                {
                                    real_bin += "_";
                                    count = 0;
                                }
                                real_bin += c;
                                count++;
                            }
                        }
                        string retbin = real_bin;
                        
                        //convert to hex
                        if (checkBox.IsChecked == true) {
                            retbin = "";
                            string but = "";
                            retbin = "";
                            foreach (char d in bin)
                            {
                                but += d;
                                if(but.Length == 4)
                                {
                                    retbin += Convert.ToInt32(but, 2).ToString("X");
                                    but = "";
                                }
                            }
                        }
                        label.Content = "Replace lines 1 through \n" + replace_lines + " in the memory file";
                        replace_lines++;
                        BinaryCode.Text += retbin + "\n";
                    }
                }
            }catch (Exception ex)
            {
                MessageBox.Show("line number: " + b + "\ninstruction " + current + "\nmessage: " + ex.Message);
                MessageBox.Show(ex.ToString());
            }

        }

        private void IsInstruction(string line, int i)
        {
            string[] words = line.Split(' ');
            List<string> b = new List<string>();
            foreach (string s in words)
            {
                if (s != "" && s != " ")
                {
                    b.Add(s);
                }
            }
            //pc++;

            if (b.Count < 1)
                return;

            string mnemonic = b[0];
            if (mnemonic.ToLower() == "bcond")
            {
                pc++;
            }
            else if (mnemonic.ToLower() == "jcond")
            {
                pc++;
            }
            else if (instructions.ContainsKey(mnemonic))
            {
                pc++;
            }
            else if (b[0][0] == '$')
            {
                string[] d = line.Split('#');
                line = d[0];
                string[] value = line.Split('=');
                variables[value[0]] = value[1];
            }
            else if (b[0][0] == '#')
            {
                return;
            }
            else if (words[0][0] == ':')
            {
                words = words[0].Split(':');
                string lbl = words[1];
                this.labels.Add(lbl, pc);
                this.variables.Add("$" + lbl, pc.ToString());
            }
            else
            {
                MessageBox.Show("There is no instruction for " + line);
            }
            return;
        }

        static int pc = 0;

        private string ProcessLine(string line, int i)
        {
            string [] words = line.Split(' ');
            List<string> b = new List<string>();
            foreach (string s in words)
            {
                if(s != "" && s != " ")
                {
                    b.Add(s);
                }
            }
            //pc++;

            if (b.Count < 1)
                return "";
            
            string mnemonic = b[0];
            if(mnemonic.ToLower() == "bcond")
            {
                pc++;
                if(variables.ContainsKey(b[1]) && variables.ContainsKey(b[2]))
                    return instructions[mnemonic].Branch(variables[b[1]], variables[b[2]], i);
                else if (variables.ContainsKey(b[1]) && !variables.ContainsKey(b[2]))
                    return instructions[mnemonic].Branch(variables[b[1]], b[2], i);
                else if (!variables.ContainsKey(b[1]) && variables.ContainsKey(b[2]))
                    return instructions[mnemonic].Branch(b[1], variables[b[2]], i);
                else
                    return instructions[mnemonic].Branch(b[1], b[2], i);
            }
            if (mnemonic.ToLower() == "jcond")
            {
                pc++;
                if (variables.ContainsKey(b[1]))
                    return instructions[mnemonic].Jump(variables[b[1]], b[2], i);
                else
                    return instructions[mnemonic].Jump(b[1], b[2], i);
            }
            else if (instructions.ContainsKey(mnemonic))
            {
                pc++;
                if (variables.ContainsKey(b[1]))
                    return instructions[mnemonic].GetBinary(variables[b[1]], b[2], i);
                else
                    return instructions[mnemonic].GetBinary(b[1], b[2], i);
            }
            else if (b[0][0] == '$')
            {
                /*string[] d = line.Split('#');
                line = d[0];
                string[] value = line.Split('=');
                variables[value[0]] = value[1];*/
                return "";
            }
            else if (b[0][0] == '#')
            {
                return "";
            }
            else if (words[0][0] == ':')
            {
                /*words = words[0].Split(':');
                string lbl = words[1];
                this.labels.Add(lbl, pc);
                this.variables.Add("$" + lbl, pc.ToString());*/
                return "";
            }
            else
            {
                MessageBox.Show("There is no instruction for " + line);
            }
            return "";
        }

        private void checkBox_Checked(object sender, RoutedEventArgs e)
        {
            if (checkBox.IsChecked == true)
                checkBox.Content = "hex";
            else
                checkBox.Content = "binary";
        }

        private void AssemblyCode_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
