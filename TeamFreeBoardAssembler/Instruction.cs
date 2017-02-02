using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamFreeBoardAssembler
{
    class Instruction
    {
        public String op_code;              //bits 15-12
        public String r_dest;
        public String op_code_ext;
        public String r_src;
        public bool is_immediate;           //if it is immediate, 7-0 are immediate value or op_code_ext + r_src
        public String binary;
        Dictionary<String, String> registers = new Dictionary<string, string>();

        public Instruction(String op_code, String op_code_ext)
        {
            this.op_code = op_code;
            this.r_dest = "";
            this.op_code_ext = op_code_ext;
            this.r_src = "";

            if (op_code_ext.Length == 0)
                this.is_immediate = true;
            else
                this.is_immediate = false;

            registers["$accel"] = "0000";  //IMU
            registers["$efb"] = "0001";  
            registers["$top_row"] = "0010";  
            registers["$guy_location"] = "0011";
            registers["$endfb"] = "0100";
            registers["$loc"] = "0101";
            registers["$newloc"] = "0110";
            registers["$temp"] = "0111";
            registers["$right_boundary"] = "1000";
            registers["$left_boundary"] = "1001";
            registers["$r10"] = "1010";
            registers["$r11"] = "1011";
            registers["$r12"] = "1100";
            registers["$r13"] = "1101";
            registers["$iter"] = "1110";
            registers["$score"] = "1111";

            this.binary = "";
        }

        public bool IsImmediate()
        {
            return is_immediate;
        }

        public String GetBinary(String r_src, String r_dest, int i)
        {
            if (!registers.ContainsKey(r_dest))
                System.Windows.MessageBox.Show("Register " + r_dest + "does not exist, on line " + i);

            if (is_immediate)
                this.binary = this.op_code + registers[r_dest] + Immediate_To_Binary(r_src, i);
            else
            {
                if (!registers.ContainsKey(r_src))
                    System.Windows.MessageBox.Show("Register " + r_src + "does not exist, on line " + i);
                this.binary = this.op_code + registers[r_dest] + op_code_ext + registers[r_src];
            }

            if (this.binary.Length != 16)
                System.Windows.MessageBox.Show("Bad instruction - binary is larger than 16 bits, on line " + i);
            return this.binary;
        }

        public bool IsBranch()
        {
            if (this.op_code == "0100" && this.op_code_ext == "1100")
                return true;
            else
                return false;
        }

        private string Immediate_To_Binary(string r_src, int i)
        {
            string immediate_bin = "";
            int temp;
            if(Int32.TryParse(r_src, out temp))
            {
                immediate_bin = Convert.ToString(temp, 2);
                while (immediate_bin.Length < 8)
                    immediate_bin = "0" + immediate_bin;
            }
            else
            {
                System.Windows.MessageBox.Show("Bad immediate " + r_src + ", on line " + i);
                return "";
            }
            return immediate_bin;
        }

        private string Cond_Immediate_To_Binary(string r_src, bool cond, int i)
        {
            string immediate_bin = "";
            int temp;
            if (Int32.TryParse(r_src, out temp))
            {
                if (temp < 0)
                {
                    immediate_bin = Convert.ToString(temp, 2);
                    int remaining = 8;
                    if (cond)
                        remaining = 4;
                    string b = immediate_bin.Substring(immediate_bin.Length - remaining + 1);
                    b = "1" + b;
                    return b;
                }
                else
                {
                    immediate_bin = Convert.ToString(temp, 2);

                    int remaining = 8;
                    if (cond)
                        remaining = 4;
                    while (immediate_bin.Length < remaining)
                        immediate_bin = "0" + immediate_bin;
                }
            }
            else
            {
                System.Windows.MessageBox.Show("Bad immediate " + r_src + ", on line " + i);
                return "";
            }
            return immediate_bin;
        }

        internal string Branch(string cond, string location, int i)
        {
            this.binary = this.op_code + Cond_Immediate_To_Binary(cond, true, i) + Cond_Immediate_To_Binary(location, false, i);

            if (this.binary.Length != 16)
                System.Windows.MessageBox.Show("Bad branch instruction - binary is not 16 bits, on line " + i + " bcond " + cond + ", " + location );
            return this.binary;
        }

        internal string Jump(string cond, string reg, int i)
        {
            if (registers.ContainsKey(reg))
                this.binary = this.op_code + Cond_Immediate_To_Binary(cond, true, i) + this.op_code_ext + registers[reg];
            else
                System.Windows.MessageBox.Show("Bad register in jump instruction line number " + i);

            return this.binary;
        }
    }
}
