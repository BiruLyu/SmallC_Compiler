using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Collections;


namespace 编译原理实践
{
    public partial class Form1 : Form
    {

        const int norw = 24,								//保留字的个数
        txmax = 100,										//标识符表的长度
        nmax = 14,										//数字的位数长度
        al = 9,											//标识符的长度
        chsetsize = 128,									//ASCII字符的长度
        maxerr = 30,										//最大错误数
        cxmax = 200,										//生成编码的最大长度
        amax = 2048,										//地址最大长度
        levmax = 3,										//最大层数
        stacksize = 20;									//堆栈的最大长度

        enum objekt { constant, variable, prozedure };
        struct INSTRUCTION      //指令类型
        {
            public fct f;       //指令
            public int l;       //层级
            public int a;       //放置的地址
        };
        struct INFO             //值、层数信息
        {
            public int val;     //值
            public int[] levelinfo;//层数信息（层数、地址、大小）
        }
        struct USERTABLE        //标识符表格
        {
            public string name; //标识符
            public objekt kind; //标识符类型
            public INFO value;  //标识符值、层数信息
        }
        INSTRUCTION[] code;
        USERTABLE[] idtable;
        string[] word = new string[100];
        string[] wsym = new string[100];
        string[] errortype = new string[100];
        string[] mnemonic = new string[20];
        string[] declbegsys = new string[1000], statbegsys = new string[1000], facbegsys = new string[1000], cursys = new string[1000];
        enum fct { LIT, OPR, LOD, STO, CAL, INT, JMP, JPC, PAR, RAD, WRT, WIT };
        Dictionary<string, string> ssym = new Dictionary<string, string>();//符号数组
        int times = 0, err = 0, cc = 0, ll = 0, kk, predeclar = 0, maincx = 0, cur_cc = 0, num, firstblank = 0, lev = -1, tempdx = 3;
        int maincome = 0, cx = 0, tx = -1, paramount = 0;
        bool longstatement = true;
        char ch = ' ';
        string sym;
        char[] a = new char[9], line = new char[1000];
        string id;//= new char[10];
        int linenumber = -1;
        StreamReader sr;
        StreamWriter ew;
        int[] s = new int[2000];
        int p, b, t, op;
        int funcsize = 0;
        int paranumber = 0;
        int[] exit = new int[100];
        int exiti = 0, exitcx = 0;
        int continuei = 0;
        int[] continuecx = new int[100];
        int isreturn = 0;
        int returnresult = 0;

        public Form1()
        {

            word[0] = "begin    ";
            word[1] = "break    ";
            word[2] = "call     ";
            word[3] = "case     ";
            word[4] = "const    ";
            word[5] = "continue ";
            word[6] = "do       ";
            word[7] = "end      ";
            word[8] = "exit     ";
            word[9] = "for      ";
            word[10] = "if       ";
            word[11] = "int      ";
            word[12] = "main     ";
            word[13] = "odd      ";
            word[14] = "read     ";
            word[15] = "repeat   ";
            word[16] = "return   ";
            word[17] = "switch   ";
            word[18] = "then     ";
            word[19] = "until    ";
            word[20] = "void     ";
            word[21] = "while    ";
            word[22] = "write    ";
            word[23] = "xor      ";

            wsym[0] = "beginsym";
            wsym[1] = "breaksym";
            wsym[2] = "callsym";
            wsym[3] = "casesym";
            wsym[4] = "constsym";
            wsym[5] = "continuesym";
            wsym[6] = "dosym";
            wsym[7] = "endsym";
            wsym[8] = "exitsym";
            wsym[9] = "forsym";
            wsym[10] = "ifsym";
            wsym[11] = "varsym";
            wsym[12] = "mainsym";
            wsym[13] = "oddsym";
            wsym[14] = "readsym";
            wsym[15] = "repeatsym";
            wsym[16] = "returnsym";
            wsym[17] = "switchsym";
            wsym[18] = "thensym";
            wsym[19] = "untilsym";
            wsym[20] = "procsym";
            wsym[21] = "whilesym";
            wsym[22] = "writesym";
            wsym[23] = "xorsym";

            errortype[4] = "这里必须是一个标识符";
            errortype[5] = "丢了一个分号(或逗号)";
            errortype[13] = "赋值符号是':='";
            errortype[19] = "该语句跟着一个不正确的使用符号";
            //errortype[];
            errortype[22] = "丢了右括号";
            errortype[23] = "丢了左括号";


            ssym.Add("+", "plus");
            ssym.Add("-", "minus");
            ssym.Add("*", "times");
            ssym.Add("/", "slash");
            ssym.Add("(", "lparen");
            ssym.Add(")", "rparen");
            ssym.Add("=", "eql");
            ssym.Add(",", "comma");
            ssym.Add("{", "beginsym");
            ssym.Add("}", "endsym");
            ssym.Add(";", "semicolon");
            ssym.Add("<", "lss");
            ssym.Add(">", "gtr");
            ssym.Add("!", "not");
            ssym.Add(":", "colon");
            ssym.Add("%", "mode");
            ssym.Add("^", "xorsym");
            ssym.Add("#", "neq");
            ssym.Add(".", "period");

            mnemonic[(int)fct.LIT] = " LIT ";
            mnemonic[(int)fct.OPR] = " OPR ";
            mnemonic[(int)fct.LOD] = " LOD ";
            mnemonic[(int)fct.STO] = " STO ";
            mnemonic[(int)fct.CAL] = " CAL ";
            mnemonic[(int)fct.INT] = " INT ";
            mnemonic[(int)fct.JMP] = " JMP ";
            mnemonic[(int)fct.JPC] = " JPC ";
            mnemonic[(int)fct.PAR] = " PAR ";

            declbegsys[0] = "constsym";
            declbegsys[1] = "varsym";
            declbegsys[2] = "procsym";

            statbegsys[0] = "beginsym";
            statbegsys[1] = "callsym";
            statbegsys[2] = "ifsym";
            statbegsys[3] = "whilesym";
            statbegsys[4] = "repeatsym";
            statbegsys[5] = "switchsym";
            statbegsys[6] = "readsym";
            statbegsys[7] = "writesym";
            statbegsys[8] = "forsym";

            facbegsys[0] = "ident";
            facbegsys[1] = "number";
            facbegsys[2] = "lparen";
            facbegsys[3] = "untilsym";
            facbegsys[4] = "casesym";
            facbegsys[5] = "breaksym";
            facbegsys[6] = "exitsym";
            facbegsys[7] = "continuesym";
            InitializeComponent();
            this.skinEngine2.SkinFile = "SportsCyan.ssk";
        }



        private void 打开ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofile = new OpenFileDialog();
            ofile.Filter = "C文件|*.c|文本文件|*.txt|所有文件|*.*";
            if (ofile.ShowDialog() == DialogResult.OK)
            {

                textBox1.Text = ofile.FileName;
                richTextBox1.Clear();
                richTextBox2.Clear();
                richTextBox3.Clear();
                richTextBox4.Clear();
                StreamReader sr = new StreamReader(ofile.FileName);
                while (sr.EndOfStream != true)
                    richTextBox1.AppendText(sr.ReadLine() + '\n');
                sr.Close();
                times = 0;
            }
        }

        private void 保存ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfile = new SaveFileDialog();
            sfile.Filter = "C文件|*.c|文本文件|*.txt|所有文件|*.*";
            if (sfile.ShowDialog() == DialogResult.OK)
            {
                richTextBox1.SaveFile(sfile.FileName, RichTextBoxStreamType.PlainText);
            }
        }


        private void begin()
        {
            cur_cc = 0; ll = 0; err = 0; cx = 0; tx = -1; lev = -1; tempdx = 3;
            err = 0;
            code = new INSTRUCTION[1000];
            idtable = new USERTABLE[2000];
            FileStream stream;
            if (err == 0)
            {
                stream = File.Open("error.txt", FileMode.OpenOrCreate, FileAccess.Write);
                stream.Seek(0, SeekOrigin.Begin);
                stream.SetLength(0); //清空txt文件
                stream.Close();
            }
            ew = new StreamWriter("error.txt", true);
            richTextBox1.SaveFile("1.c", RichTextBoxStreamType.PlainText);
            sr = new StreamReader("1.c");
            cc = 0; ll = 0; ch = ' ';
            kk = al - 1;
            getsym();
            cursys[symsetcount(cursys)] = "period";
            for (int m = 0; m < 3; m++)
                cursys[symsetcount(cursys)] = declbegsys[m];
            for (int m = 0; m <= 5; m++)
                cursys[symsetcount(cursys)] = statbegsys[m];
            predeclar = 0;
            block(cursys);
            code[0].a = maincx;
            if (sym != "period") error(9);
        }


        private void 编译ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //err = 0;
            //code = new INSTRUCTION[1000];
            //idtable = new USERTABLE[2000];
            //FileStream stream;
            //if (err == 0)
            //{
            //    stream = File.Open("error.txt", FileMode.OpenOrCreate, FileAccess.Write);
            //    stream.Seek(0, SeekOrigin.Begin);
            //    stream.SetLength(0); //清空txt文件
            //    stream.Close();
            //}
            //ew = new StreamWriter("error.txt", true);
            //richTextBox1.SaveFile("1.c", RichTextBoxStreamType.PlainText);
            //sr = new StreamReader("1.c");
            begin();
            //cc = 0; ll = 0; ch = ' ';
            //kk = al - 1;
            //getsym();
            //cursys[symsetcount(cursys)] = "period";
            //for (int m = 0; m < 3; m++)
            //    cursys[symsetcount(cursys)] = declbegsys[m];
            //for (int m = 0; m <= 4; m++)
            //    cursys[symsetcount(cursys)] = statbegsys[m];
            //predeclar = 0;
            //block(cursys);
            //code[0].a = maincx;
            //if (sym != "period") error(9);
            if (err == 0)
            {
                listall();
                //interpret();
            }
            else
            {
                richTextBox2.AppendText("Errors in smallC program!\n");
                //richTextBox2.AppendText("For details, see error.txt!\n");

            }
            //if (sr.EndOfStream)
            //{
            ew.Close();
            sr.Close();
            //}
        }

        private void getsym()
        {
            int i, j, k;

            while (ch == ' ' || ch == 9 || ch == 10 || ch == '\0') getch();
            if (char.IsLower(ch))
            {
                k = 0;
                do
                {
                    if (k < al)
                    {
                        a[k++] = ch;
                    }
                    if (line[cur_cc] == 10 || ch == 10) { getch(); break; }
                    getch();
                } while (char.IsLower(ch) || char.IsDigit(ch));
                if (k > kk)
                    kk = k;
                else
                {
                    do
                    {
                        a[kk] = ' ';
                        kk--;
                    } while (kk != (k - 1));
                }
                //for (int m = 0; m < al; m++)
                //    id[m] = a[m];
                id = new string(a);
                i = 0;
                j = norw - 1;
                do
                {
                    k = (i + j) / 2;
                    if (id.ToString() == word[k])
                    {
                        sym = wsym[k];
                        break;
                    }
                    if (id.ToString().CompareTo(word[k]) < 0) j = k - 1;
                    else
                        i = k + 1;
                    sym = "ident";
                } while (i <= j);
            }
            else if (ch == '{' || ch == '}')
            {
                if (ch == '{')
                    sym = "beginsym";
                else
                    sym = "endsym";
                getch();
            }
            //else if(ch=='('||ch==')')
            //{
            //	if(ch=='(')
            //		sym="lparen";
            //	else
            //		sym="rparen";
            //	getch();
            //}
            else if (char.IsDigit(ch))
            {
                k = 0;
                num = 0;
                sym = "number";
                do
                {
                    num = num * 10 + (ch - '0');
                    k++;
                    getch();
                } while (char.IsDigit(ch));
                if (k > nmax) error(30);
            }
            //else if(ch==':')
            //{
            //	getch();
            //	if(ch=='=')
            //	{	
            //		sym="becomes";
            //		getch();
            //	}
            //	else
            //		sym=NULL;
            //}
            else if (ch == '<')
            {
                getch();
                if (ch == '=')
                {
                    sym = "leq";
                    getch();
                }
                else
                    sym = "lss";
            }
            else if (ch == '>')
            {
                getch();
                if (ch == '=')
                {
                    sym = "geq";
                    getch();
                }
                else
                    sym = "gtr";
            }
            else if (ch == '=')
            {
                getch();
                if (ch == '=')
                {
                    sym = "eql";
                    getch();
                }
                else
                    sym = "becomes";
            }
            else if (ch == '+')
            {
                getch();
                if (ch == '+')
                {
                    sym = "selfplus";
                    getch();
                }
                else sym = "plus";
            }
            else if (ch == '-')
            {
                getch();
                if (ch == '-')
                {
                    sym = "selfminus";
                    getch();
                }
                else sym = "minus";
            }
            else if (ch == '!')
            {
                getch();
                if (ch == '=')
                {
                    sym = "neq";
                    getch();
                }
                else
                    sym = "not";
            }
            else
            {
                // if (ssym.Keys.Contains(ch.ToString()))
                sym = ssym[ch.ToString()];
                //else
                //getsym();
                if (!sr.EndOfStream) getch();
            }
        }

        private void getch()
        {
            string s;
            if (cur_cc == ll)
            {
                if (sr.EndOfStream)
                {
                    return;
                }
                ll = 0;
                cc = 0;
                //s = "";
                s = sr.ReadLine();
                linenumber++;
                if (s == null)
                {
                    richTextBox2.AppendText("fgets error\n");
                }
                for (int i = 0; i < 1000; i++)
                    line[i] = '\0';
                s.CopyTo(0, line, 0, s.Length);
                firstblank = 0;
                for (int m = 0; m < 100; m++)
                {
                    if (line[m] == ' ') firstblank++;
                    else break;
                }
                for (int m = 0; m < 100; m++)
                {
                    if (line[m] == '\0' || line[m] == 10)
                    {
                        ll++;
                        break;
                    }
                    else
                        ll++;
                }
            }
            ch = line[firstblank + cc];
            cc++;
            cur_cc = firstblank + cc;
        }

        private void prodeclaration(string[] fsys)
        {
            int dx;
            int paranumber = 0;
            lev++;
            tempdx = 3;
            dx = tempdx;

            if (sym == "ident")
            {
                enter(objekt.prozedure, -1);
                getsym();
            }
            else if (sym == "mainsym")
            {
                enter(objekt.prozedure, -1);
                getsym();
                maincome = 1;
                maincx = cx;
            }
            else error(4);
            if (sym == "lparen")
            {
                getsym();
                if (sym == "varsym")
                {
                    paranumber++;
                    getsym();
                    vardeclaration(dx); dx = tempdx;
                    while (sym == "comma")
                    {
                        getsym();
                        if (sym == "varsym")
                        {
                            paranumber++;
                            getsym();
                            vardeclaration(dx); dx = tempdx;
                        }
                    }
                }

                if (sym == "rparen")
                {
                    getsym();
                    if (sym == "beginsym")
                    {
                        lev--;
                        getsym();
                        predeclar = dx - 3;
                        //predeclar=0;
                        block(fsys);
                        predeclar = 0;
                        if (sym == "endsym")
                            getsym();
                        else error(105);
                    }
                    else
                        error(102);
                }
                else error(101);
                idtable[tx].value.levelinfo[3] = paranumber;
            }
            else
                error(100);
        }


        private void block(string[] fsys)
        {
            int dx, tx0, cx0;

            tempdx = 3 + predeclar;
            dx = tempdx;
            tx0 = tx;
            //idtable[tx-predeclar].value.levelinfo[1]=cx;//记录函数的起始地址
            if (tx >= 0)
                idtable[tx - predeclar].value.levelinfo[1] = cx;
            else
            {
                idtable[0].value.levelinfo = new int[4];
                idtable[0].value.levelinfo[1] = cx;
            }
            gen(fct.JMP, 0, 0);
            lev++;

            if (lev > levmax) error(32);
            while (sym == "constsym" || sym == "varsym" || sym == "procsym")
            {
                if (sym == "constsym")
                {
                    getsym();
                    constdeclaration();
                    while (sym == "comma")
                    {
                        getsym();
                        constdeclaration();
                    }
                    if (sym == "semicolon")
                        getsym();
                    else error(5);
                }
                if (sym == "varsym")
                {
                    getsym();
                    vardeclaration(dx); dx = tempdx;
                    while (sym == "comma")
                    {
                        getsym();
                        vardeclaration(dx); dx = tempdx;
                    }
                    if (sym == "semicolon")
                        getsym();
                    else error(5);
                }
                while (sym == "procsym")
                {
                    getsym();
                    prodeclaration(fsys);

                }
                string[] temp = new string[200];
                for (int m = 0; m < symsetcount(statbegsys); m++)
                    temp[symsetcount(temp)] = statbegsys[m];
                temp[symsetcount(temp)] = "ident";
                temp[symsetcount(temp)] = "period";
                temp[symsetcount(temp)] = "endsym";
                temp[symsetcount(temp)] = "varsym";

                test(temp, declbegsys, 7);
            }

            if (tx0 >= 0)
            {
                code[idtable[tx0 - predeclar].value.levelinfo[1]].a = cx;//JMP的回填
                idtable[tx0 - predeclar].value.levelinfo[1] = cx;//函数的地址
                idtable[tx0 - predeclar].value.levelinfo[2] = dx;//函数的变量空间的大小
            }
            else
            {
                code[0].a = cx;
            }
            //code[idtable[tx0-predeclar].value.levelinfo[1]].a=cx;//JMP的回填
            //idtable[tx0-predeclar].value.levelinfo[1]=cx;//函数的地址
            //idtable[tx0-predeclar].value.levelinfo[2]=dx;//函数的变量空间的大小
            cx0 = cx;

            if (lev > 0)//不要产生最后面的两行代码
                gen(fct.INT, 0, dx);


            string[] temp1 = new string[200];
            temp1[symsetcount(temp1)] = "semicolon";
            temp1[symsetcount(temp1)] = "endsym";
            temp1[symsetcount(temp1)] = "untilsym";
            temp1[symsetcount(temp1)] = "breaksym";
            temp1[symsetcount(temp1)] = "ident";
            temp1[symsetcount(temp1)] = "readsym";
            temp1[symsetcount(temp1)] = "writesym";
            temp1[symsetcount(temp1)] = "returnsym";

            for (int m = 0; m < symsetcount(fsys); m++)
                temp1[symsetcount(temp1)] = fsys[m];
            statement(temp1);

            if (lev > 0)//不要产生最后面的两行代码
                gen(fct.OPR, 0, 0);
            else//处于最外层的block时，回填所有的exit
            {
                for (int i = 0; i < exiti; i++)
                {
                    code[exit[i] - 1].a = exitcx;
                }
            }

            if (maincome == 1)
            {
                maincome = 0;
                exitcx = cx - 1;
            }

            temp1[symsetcount(temp1)] = "period";
            //test(fsys,temp1,8);
            lev--;
        }


        private int symsetcount(string[] fsys)
        {
            int m;
            for (m = 0; m < 128; m++)
                if (fsys[m] == null)
                    break;
            return m;
        }


        void constdeclaration()
        {
            if (sym == "ident")
            {
                getsym();
                if (sym == "eql" || sym == "becomes")
                {
                    if (sym == "eql") error(1);
                    getsym();
                    if (sym == "number")
                    {
                        enter(objekt.constant, -1);
                        getsym();
                    }
                    else error(2);
                }
                else error(3);
            }
            else error(4);
        }


        void vardeclaration(int number)
        {
            if (sym == "ident")
            {
                enter(objekt.variable, number);
                getsym();
            }
            else error(4);
        }


        void statement(string[] fsys)
        {
            int i, cx1, cx2;
            bool hasbegin = false;
            if (longstatement || sym == "beginsym")
            {
                if (sym == "beginsym")
                {
                    hasbegin = true;
                    getsym();
                }
                string[] temp = new string[200];
                temp[symsetcount(temp)] = "semicolon";
                temp[symsetcount(temp)] = "endsym";
                for (int m = 0; m < symsetcount(fsys); m++)
                    temp[symsetcount(temp)] = fsys[m];
                longstatement = false;
                statement(temp);
                longstatement = true;
                while (sym == "exitsym" || sym == "returnsym" || sym == "continue" || sym == "semicolon" || sym == "ident" || sym == "beginsym" || sym == "callsym" || sym == "ifsym" || sym == "whilesym" || sym == "forsym" || sym == "repeatsym" || sym == "switchsym" || sym == "readsym" || sym == "writesym")
                {
                    if (sym == "semicolon")
                        getsym();
                    //else
                    //	error(10);
                    longstatement = false;
                    statement(temp);
                    longstatement = true;
                }
                if (hasbegin)
                {
                    if (sym == "endsym")
                        getsym();
                    else
                        error(17);
                }

            }
            else if (sym == "returnsym")
            {
                getsym();
                if (sym == "ident")
                {
                    getsym();
                    int ii = position(id);
                    gen(fct.LOD, lev - idtable[ii].value.levelinfo[0], idtable[ii].value.levelinfo[1]);
                    isreturn = 1;
                }
                else error(20000);
            }
            else if (sym == "exitsym")
            {
                getsym();
                gen(fct.JMP, 0, 0);
                exit[exiti++] = cx;
                if (sym == "semicolon")
                    getsym();
                else error(10000);
            }
            else if (sym == "callsym")
            {
                paramount = 0;
                //memset(para,int,sizeof(para));

                //int address = 3;

                string[] temp = new string[200];
                temp[symsetcount(temp)] = "semicolon";
                temp[symsetcount(temp)] = "endsym";
                temp[symsetcount(temp)] = "comma";
                temp[symsetcount(temp)] = "rparen";
                for (int m = 0; m < symsetcount(fsys); m++)
                    temp[symsetcount(temp)] = fsys[m];
                getsym();
                if (sym != "ident") error(14);
                else
                {
                    i = position(id);
                    getsym();
                    if (i == -1) error(11);

                    if (sym == "lparen")
                    {
                        getsym();
                        if (sym == "ident" || sym == "number")
                        {
                            paramount++;
                            expression(temp);
                            //gen(fct::STO,lev-idtable[i].value.levelinfo[0],address++);
                            while (sym == "comma")
                            {
                                paramount++;
                                getsym();
                                expression(temp);
                                //gen(fct::STO,lev-idtable[i].value.levelinfo[0],address++);
                            }
                        }
                        if (sym == "rparen")
                            getsym();
                        else error(1000);
                    }

                    gen(fct.PAR, 0, paramount);
                    if (idtable[i].kind == objekt.prozedure)
                        gen(fct.CAL, lev - idtable[i].value.levelinfo[0], idtable[i].value.levelinfo[1]);
                    else error(15);

                }
            }
            else if (sym == "ifsym")
            {
                getsym();
                string[] temp = new string[200];
                temp[symsetcount(temp)] = "thensym";
                temp[symsetcount(temp)] = "dosym";
                for (int m = 0; m < symsetcount(fsys); m++)
                    temp[symsetcount(temp)] = fsys[m];
                if (sym == "lparen")
                {
                    getsym();
                    condition(temp);
                    if (sym == "rparen")
                        getsym();
                    else error(201);
                }
                else error(200);
                //if(sym=="thensym")
                //	getsym();
                //else
                //	error(16);
                cx1 = cx;
                gen(fct.JPC, 0, 0);
                longstatement = true;
                statement(fsys);
                longstatement = true;
                code[cx1].a = cx;
            }
            else if (sym == "readsym")
            {
                getsym();
                if (sym != "ident") error(21);//read后不是变量
                else
                {
                    i = position(id);
                    if (i == -1) error(14);
                    else if (idtable[i].kind == objekt.variable)
                        gen(fct.RAD, lev - idtable[i].value.levelinfo[0], idtable[i].value.levelinfo[1]);
                    else error(21);
                    string[] temp4 = new string[1000];
                    getsym();
                    string[] temp6 = new string[1000];
                    temp6[symsetcount(temp6)] = "semicolon";
                    temp6[symsetcount(temp6)] = "endsym";
                    for (int m = 0; m < symsetcount(fsys); m++)
                        temp6[symsetcount(temp6)] = fsys[m];
                    while ((sym == "semicolon") && !sr.EndOfStream)
                    {
                        if (sym == "semicolon" && !sr.EndOfStream)
                            getsym();
                        else
                            error(10);
                    }
                }
            }
            else if (sym == "writesym")
            {
                getsym();
                if (sym != "ident") error(21);//read后不是变量
                else
                {
                    i = position(id);
                    if (i == -1) error(14);
                    else if (idtable[i].kind == objekt.variable)
                        gen(fct.WRT, lev - idtable[i].value.levelinfo[0], idtable[i].value.levelinfo[1]);
                    else if (idtable[i].kind == objekt.constant)
                        gen(fct.WIT, 0, idtable[i].value.val);
                    else error(21);
                    getsym();
                    string[] temp4 = new string[1000];
                    temp4[symsetcount(temp4)] = "semicolon";
                    temp4[symsetcount(temp4)] = "endsym";
                    for (int m = 0; m < symsetcount(fsys); m++)
                        temp4[symsetcount(temp4)] = fsys[m];
                    while ((sym == "semicolon") && !sr.EndOfStream)
                    {
                        if (sym == "semicolon" && !sr.EndOfStream)
                            getsym();
                        else
                            error(10);
                    }
                }
            }
            else if (sym == "ident")
            {
                i = position(id);
                if (i == -1) error(11);
                else if (idtable[i].kind != objekt.variable)
                {
                    error(12);
                    i = 0;
                }
                getsym();
                if (sym == "becomes")
                {
                    getsym();
                    if (sym == "ident" || sym == "number")
                        expression(fsys);
                    else
                    {
                        longstatement = false;
                        statement(fsys);
                        longstatement = true;
                    }
                }
                else if (sym == "selfplus")
                {
                    gen(fct.LOD, lev - idtable[i].value.levelinfo[0], idtable[i].value.levelinfo[1]);
                    gen(fct.LIT, 0, 1);
                    gen(fct.OPR, 0, 2);
                    getsym();
                }
                else if (sym == "selfminus")
                {
                    gen(fct.LOD, lev - idtable[i].value.levelinfo[0], idtable[i].value.levelinfo[1]);
                    gen(fct.LIT, 0, 1);
                    gen(fct.OPR, 0, 3);
                    getsym();
                }
                else ;//error(13);
                if (i != -1)
                    gen(fct.STO, lev - idtable[i].value.levelinfo[0], idtable[i].value.levelinfo[1]);
            }
            else if (sym == "switchsym")
            {
                int precase = 0;
                int[] refill = new int[100];
                int refilli = 0;
                getsym();
                if (sym == "lparen")
                {
                    getsym();
                    if (sym == "ident")
                    {

                        int j = position(id);
                        int val = idtable[j].value.levelinfo[2];
                        getsym();
                        //expression(fsys);
                        if (sym == "rparen")
                        {
                            getsym();
                            while (sym == "casesym")
                            {
                                if (precase != 0)
                                {
                                    code[precase - 1].a = cx;
                                }
                                getsym();
                                if (sym == "number")
                                {
                                    gen(fct.LOD, lev - idtable[j].value.levelinfo[0], idtable[j].value.levelinfo[1]);
                                    expression(fsys);
                                    gen(fct.OPR, 0, 8);
                                    gen(fct.JPC, 0, 0);
                                    precase = cx;
                                    if (sym == "colon")
                                    {
                                        getsym();
                                        longstatement = true;
                                        statement(fsys);
                                    }
                                    if (sym == "breaksym")
                                    {
                                        getsym();
                                        if (sym == "semicolon")
                                            getsym();
                                        else error(505);
                                        gen(fct.JMP, 0, 0);
                                        refill[refilli++] = cx;
                                    }
                                }
                                else error(504);
                            }
                            for (int k = 0; k < refilli; k++)
                                code[refill[k] - 1].a = cx;
                            code[precase - 1].a = cx;

                        }
                        else error(502);
                    }
                    else error(501);
                }
                else error(500);
            }
            else if (sym == "repeatsym")
            {
                int cxrepeat = cx;

                getsym();
                statement(fsys);
                if (sym == "untilsym")
                {
                    getsym();
                    if (sym == "lparen")
                    {
                        getsym();
                        while (continuei > 0)
                        {

                            code[continuecx[continuei - 1]].a = cx;
                            continuei--;
                        }
                        condition(fsys);
                        if (sym == "rparen")
                        {
                            gen(fct.JPC, 0, cx + 2);
                            gen(fct.JMP, 0, cxrepeat);

                            getsym();
                        }
                        else error(302);
                    }
                    else error(301);
                }
                else error(300);
            }
            else if (sym == "continuesym")
            {
                continuecx[continuei++] = cx;
                gen(fct.JMP, 0, 0);
                getsym();
            }
            else if (sym == "whilesym")
            {
                cx1 = cx;
                getsym();
                string[] temp = new string[200];
                temp[symsetcount(temp)] = "dosym";
                for (int m = 0; m < symsetcount(fsys); m++)
                    temp[symsetcount(temp)] = fsys[m];
                if (sym == "lparen")
                {
                    getsym();
                    condition(temp);
                    if (sym == "rparen")
                        getsym();
                    else error(22);
                }
                else error(23);
                cx2 = cx;
                gen(fct.JPC, 0, 0);
                //if (sym == "dosym")
                //    getsym();
                //else
                //    error(18);
                longstatement = true;
                statement(fsys);
                longstatement = true;
                gen(fct.JMP, 0, cx1);
                code[cx2].a = cx;//回填
            }
            else if (sym == "forsym")
            {

                //int forcx;
                int index_of_flag = 0;
                int index_of_flag2 = 0;
                int index_of_jmp1 = 0;
                int index_of_jmp2 = 0;
                int index_of_jmp3 = 0;
                int index_of_jpc = 0;
                //cx1 = cx;
                getsym();
                string[] temp = new string[200];
                temp[symsetcount(temp)] = "dosym";
                for (int m = 0; m < symsetcount(fsys); m++)
                    temp[symsetcount(temp)] = fsys[m];
                string[] temp2 = new string[200];
                temp2[symsetcount(temp)] = "dosym";
                for (int m = 0; m < symsetcount(fsys); m++)
                    temp2[symsetcount(temp)] = fsys[m];
                //forcx = cx+2;
                //cx2 = cx+5;
                if (sym == "lparen")
                {
                    getsym();
                    statement(fsys);
                    index_of_flag = cx;
                    if (sym == "semicolon")
                    {
                        getsym();
                        condition(temp);
                        index_of_jpc = cx;
                        //cx2 = cx;
                        gen(fct.JPC, 0, 0);
                        index_of_jmp1 = cx;
                        gen(fct.JMP, 0, 0);
                        index_of_flag2 = cx;
                        if (sym == "semicolon")
                        {
                            getsym();
                            statement(fsys);
                            index_of_jmp2 = cx;
                            gen(fct.JMP, 0, index_of_flag);
                            code[index_of_jmp1].a = cx;
                            if (sym == "semicolon")
                            {
                                getsym();
                                if (sym == "rparen")
                                    getsym();
                                else
                                    error(22);
                            }
                            else
                                error(5);
                        }
                        else
                            error(5);//缺少分号

                    }
                    else
                        error(5);//缺少分号
                }
                else error(23);

                //cx2 = cx;
                //gen(fct.JPC, 0, 0);

                longstatement = true;
                statement(fsys);
                longstatement = true;
                index_of_jmp3 = cx;
                gen(fct.JMP, 0, 12);
                code[index_of_jpc].a = cx;
            }
            string[] temp1 = new string[200];
            test(fsys, temp1, 19);
        }


        void condition(string[] fsys)
        {
            //char* relop;
            string relop;

            if (sym == "oddsym")
            {
                getsym();
                expression(fsys);
                gen(fct.OPR, 0, 6);
            }
            else
            {
                string[] temp = new string[200];
                temp[symsetcount(temp)] = "eql";
                temp[symsetcount(temp)] = "neq";
                temp[symsetcount(temp)] = "lss";
                temp[symsetcount(temp)] = "gtr";
                temp[symsetcount(temp)] = "leq";
                temp[symsetcount(temp)] = "geq";
                for (int m = 0; m < symsetcount(fsys); m++)
                    temp[symsetcount(temp)] = fsys[m];
                expression(temp);
                if (sym != "eql" && sym != "neq" && sym != "lss" && sym != "leq" && sym != "gtr" && sym != "geq")
                    error(20);
                else
                {
                    relop = sym;
                    getsym();
                    expression(fsys);
                    switch (relop[0])
                    {
                        case 'e':						//"eql"
                            gen(fct.OPR, 0, 8);
                            break;
                        case 'n':						//"neq"
                            gen(fct.OPR, 0, 9);
                            break;
                        case 'l':						//lss & leq
                            if (relop[1] == 's')
                                gen(fct.OPR, 0, 10);
                            else
                                gen(fct.OPR, 0, 13);
                            break;
                        case 'g':						//geq & gtr
                            if (relop[1] == 'e')
                                gen(fct.OPR, 0, 11);
                            else
                                gen(fct.OPR, 0, 12);
                            break;
                    }
                }
            }
        }



        void expression(string[] fsys)
        {
            //char* addop;
            string addop;

            string[] temp = new string[200];
            temp[symsetcount(temp)] = "plus";
            temp[symsetcount(temp)] = "minus";
            temp[symsetcount(temp)] = "mode";
            temp[symsetcount(temp)] = "xorsym";
            for (int m = 0; m < symsetcount(fsys); m++)
                temp[symsetcount(temp)] = fsys[m];
            if (sym == "plus" || sym == "minus")
            {
                addop = sym;
                getsym();
                term(temp);
                if (addop == "minus")
                    gen(fct.OPR, 0, 1);
            }
            else
                term(temp);
            while (sym == "plus" || sym == "minus" || sym == "xorsym" || sym == "mode")
            {
                addop = sym;
                getsym();
                term(temp);
                if (addop == "plus")
                    gen(fct.OPR, 0, 2);
                else if (addop == "minus")
                    gen(fct.OPR, 0, 3);
                else if (addop == "mode")
                    gen(fct.OPR, 0, 15);
                else if (addop == "xorsym")
                    gen(fct.OPR, 0, 14);
            }
        }


        void term(string[] fsys)
        {
            //char* mulop;
            string mulop;

            string[] temp = new string[200];
            temp[symsetcount(temp)] = "times";
            temp[symsetcount(temp)] = "slash";
            for (int m = 0; m < symsetcount(fsys); m++)
                temp[symsetcount(temp)] = fsys[m];
            //temp[symsetcount(temp)]="beginsym";
            /*temp[symsetcount(temp)]="plus";
            temp[symsetcount(temp)]="minus";
            temp[symsetcount(temp)]="times";
            temp[symsetcount(temp)]="slash";*/
            temp[symsetcount(temp)] = "lparen";
            temp[symsetcount(temp)] = "rparen";
            temp[symsetcount(temp)] = "colon";
            temp[symsetcount(temp)] = "not";

            //temp[symsetcount(temp)]="eql";
            //temp[symsetcount(temp)]="comma";
            //temp[symsetcount(temp)]="period";
            //temp[symsetcount(temp)]="neq";
            //temp[symsetcount(temp)]="lss";
            //temp[symsetcount(temp)]="gtr";
            //temp[symsetcount(temp)]="semicolon";
            factor(temp);
            while (sym == "times" || sym == "slash")
            {
                mulop = sym;
                getsym();
                factor(temp);
                if (mulop == "times")
                    gen(fct.OPR, 0, 4);
                else
                    gen(fct.OPR, 0, 5);
            }
        }



        void factor(string[] fsys)
        {
            int i;
            test(facbegsys, fsys, 24);
            if (sym == "ident")
            {
                i = position(id);
                if (i == -1) error(11);
                else
                {
                    switch (idtable[i].kind)
                    {
                        case objekt.constant:
                            gen(fct.LIT, 0, idtable[i].value.val);
                            break;
                        case objekt.variable:
                            gen(fct.LOD, lev - idtable[i].value.levelinfo[0], idtable[i].value.levelinfo[1]);
                            break;
                        case objekt.prozedure:
                            error(21);
                            break;
                    }
                }
                getsym();
            }
            else if (sym == "number")
            {
                if (num > amax)
                {
                    error(31);
                    num = 0;
                }
                gen(fct.LIT, 0, num);
                getsym();
            }
            else if (sym == "lparen")
            {
                getsym();
                string[] temp = new string[200];
                temp[symsetcount(temp)] = "rparen";
                for (int m = 0; m < symsetcount(fsys); m++)
                    temp[symsetcount(temp)] = fsys[m];
                expression(temp);
                if (sym == "rparen")
                    getsym();
                else
                    error(22);
            }
            string[] temp2 = new string[200];
            temp2[symsetcount(temp2)] = "lparen";
            //fsys[symsetcount(fsys)]="beginsym";
            //fsys[symsetcount(fsys)]="plus";
            //fsys[symsetcount(fsys)]="minus";
            //fsys[symsetcount(fsys)]="times";
            //fsys[symsetcount(fsys)]="slash";
            //fsys[symsetcount(fsys)]="lparen";
            //fsys[symsetcount(fsys)]="rparen";
            //fsys[symsetcount(fsys)]="eql";
            //fsys[symsetcount(fsys)]="comma";
            //fsys[symsetcount(fsys)]="period";
            //fsys[symsetcount(fsys)]="neq";
            //fsys[symsetcount(fsys)]="lss";
            //fsys[symsetcount(fsys)]="gtr";
            //fsys[symsetcount(fsys)]="semicolon";
            test(fsys, temp2, 23);
        }


        int position(string a)
        {
            int i;
            bool flag = true;
            i = tx;
            for (int m = i; m >= 0; m--)
            {
                flag = true;
                for (int n = 0; n < al; n++)
                {
                    if (idtable[m].name[n] != a[n])
                    {
                        flag = false; break;
                    }
                }
                if (flag == true)
                {
                    i = m; break;
                }
                if (m == 0 && flag == false)
                {
                    i = -1; break;
                }
            }
            return i;	//if no found return -1
        }


        void test(string[] s1, string[] s2, int n)
        {
            bool flag = false;
            for (int m = 0; m < symsetcount(s1); m++)
            {
                if (s1[m] == sym) { flag = true; break; }
            }
            if (flag == false)
            {
                error(n);
                for (int m = 0; m < symsetcount(s2); m++)
                {
                    s1[symsetcount(s1)] = s2[m];
                }
                flag = false;
                while (true)
                {
                    for (int m = 0; m < symsetcount(s1); m++)
                    {
                        if (s1[m] == null) break;
                        if (s1[m] == sym) { flag = true; break; }
                    }
                    if (flag == true) break;
                    getsym();
                }
            }
        }



        void enter(objekt k, int number)
        {
            tx++;
            //for(int m=0;m<al;m++)
            //    idtable[tx].name[m]=id[m];
            idtable[tx].name = id.ToString();
            idtable[tx].kind = k;
            idtable[tx].value.levelinfo = new int[4];
            switch (k)
            {
                case objekt.constant:					//constant
                    if (num > amax)
                    {
                        error(31); num = 0;
                    }
                    idtable[tx].value.val = num;
                    break;
                case objekt.variable:					//variable
                    idtable[tx].value.levelinfo[0] = lev;
                    idtable[tx].value.levelinfo[1] = number;
                    tempdx++;
                    break;
                case objekt.prozedure:					//prozedure
                    idtable[tx].value.levelinfo[0] = lev;
                    break;
            }
        }


        void error(int n)
        {
            for (int i = 0; i < cur_cc - 1; i++)
                ew.Write(" ");
            string lines = linenumber.ToString();
            string number = n.ToString();
            richTextBox2.AppendText(lines + "：error" + number + ":" + errortype[n] + "\n");
            ew.Write("^%d\n", n);
            err += 1;
            if (err > maxerr)
            {
                ew.Close();
                Environment.Exit(0);
            }
        }



        void listall()
        {
            int i;
            ew.WriteLine("\nAll the PL/0 object code:");
            richTextBox2.Clear();
            for (i = 0; i < cx; i++)
            {
                //ew.Write(i.ToString());
                richTextBox2.AppendText(i.ToString() + ":\t" + code[i].f.ToString() + "\t" + code[i].l.ToString() + "\t" + code[i].a.ToString() + "\n");
                //show(code[i].f);
                //ew.WriteLine( "%3d%5d", code[i].l, code[i].a);
            }
        }



        void gen(fct x, int y, int z)
        {
            if (cx > cxmax)
            {
                ew.WriteLine("program too long");
                ew.Close();
                Environment.Exit(0);
            }
            code[cx].f = x;
            code[cx].l = y;
            code[cx].a = z;
            cx++;
        }
         


        void interpret()
        {
            //int p, b, t;
            Read r = new Read();

            returnresult = 0;

            funcsize = 0;
            paranumber = 0;

            t = -1;
            b = 0;
            p = 0;
            s[0] = 0;
            s[1] = 0;
            s[2] = 0;
            do
            {
                int i = (int)code[p].f;
                int _a = code[p].a;
                int j = 1;
                switch (i)
                {
                    case 0:								//lit
                        t = t + 1;
                        s[t] = _a;
                        break;
                    case 1:								//opr
                        switch (_a)
                        {
                            case 0:
                                if (isreturn == 1)
                                {
                                    returnresult = s[t];
                                }

                                t = b - 1;
                                p = s[t + 3];
                                b = s[t + 2];
                                if (isreturn == 1)
                                {
                                    s[++t] = returnresult;
                                    isreturn = 0;
                                }

                                break;
                            case 1:
                                s[t] = -s[t];
                                break;
                            case 2:
                                t = t - 1;
                                s[t] = s[t] + s[t + 1];
                                break;
                            case 3:
                                t = t - 1;
                                s[t] = s[t] - s[t + 1];
                                break;
                            case 4:
                                t = t - 1;
                                s[t] = s[t] * s[t + 1];
                                break;
                            case 5:
                                t = t - 1;
                                s[t] = s[t] / s[t + 1];
                                break;
                            case 6:
                                s[t] = s[t] % 2;
                                break;
                            case 8:
                                t = t - 1;
                                s[t] = (s[t] == s[t + 1]) ? 1 : 0;
                                break;
                            case 9:
                                t = t - 1;
                                s[t] = (s[t] != s[t + 1]) ? 1 : 0;
                                break;
                            case 10:
                                t = t - 1;
                                s[t] = (s[t] < s[t + 1]) ? 1 : 0;
                                break;
                            case 11:
                                t = t - 1;
                                s[t] = (s[t] >= s[t + 1]) ? 1 : 0;
                                break;
                            case 12:
                                t = t - 1;
                                s[t] = (s[t] > s[t + 1]) ? 1 : 0;
                                break;
                            case 13:
                                t = t - 1;
                                s[t] = (s[t] <= s[t + 1]) ? 1 : 0;
                                break;
                            case 14://异或
                                t = t - 1;
                                s[t] = s[t] ^ s[t + 1];
                                break;
                            case 15://mode
                                t = t - 1;
                                s[t] = s[t] % s[t + 1];
                                break;
                        }
                        break;
                    case 2:								//lod
                        t = t + 1;
                        s[t] = s[basex(code[p].l, b) + _a];
                        break;
                    case 3:								//sto
                        s[basex(code[p].l, b) + _a] = s[t];
                        //printf("%d\n", s[t]);
                        //fprintf_s(wstream, "%d\n", s[t]);
                        t = t - 1;
                        break;
                    case 4:								//cal
                        s[t + 1] = basex(code[p].l, b);
                        s[t + 2] = b;
                        s[t + 3] = p + 1;
                        b = t + 1;
                        p = _a;

                        /*int i=0;
                        while(i<)*/

                        break;
                    case 5:								//int
                        t = t + _a;
                        funcsize = _a;
                        j = 1;
                        while (j <= paranumber)
                        {
                            s[t - funcsize + 3 + j] = s[t - funcsize - paranumber + j];
                            j++;
                        }

                        break;
                    case 6:								//jmp
                        p = _a;
                        break;
                    case 7:								//jpc
                        if (s[t] == 0)
                            p = _a;
                        t = t - 1;
                        break;
                    case 8:
                        paranumber = 0;
                        funcsize = 0;
                        paranumber = _a;
                        break;


                    case 9:
                        r.ShowDialog();
                        s[basex(code[p].l, b) + _a] = r.a;
                        break;

                    case 10:
                        t = t + 1;
                        s[t] = s[basex(code[p].l, b) + _a];
                        richTextBox4.AppendText(s[basex(code[p].l, b) + _a].ToString() + "\n");
                        t = t - 1;
                        break;

                    case 11:
                        richTextBox4.AppendText(_a.ToString() + "\n");
                        break;

                }
                p++;
                if ((i == 1 && _a == 0) || (i == 4) || (i == 6) || (i == 7 && s[t + 1] == 0)) p--;
            } while (p != 0);
        }



        private int basex(int l, int basel)
        {
            int b1;
            b1 = basel;
            while (l > 0)
            {
                b1 = s[b1];
                l = l - 1;
            }
            return b1;
        }

        private void 运行ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            begin();
            if (err == 0)
            {
                listall();
                interpret();
            }
            ew.Close();
            sr.Close();
        }

        private void onebyone()
        {
            Read r = new Read();
            int length = 0;
            if (op != p)
            {
                for (int m = 0; m < op; m++)
                    length += (1 + richTextBox2.Lines[m].Length);
                richTextBox2.Select(length, richTextBox2.Lines[op].Length);
                richTextBox2.SelectionBackColor = Color.White;
            }
            length = 0;
            for (int m = 0; m < p; m++)
                length += (1 + richTextBox2.Lines[m].Length);
            richTextBox2.Select(length, richTextBox2.Lines[p].Length);
            richTextBox2.SelectionBackColor = Color.Cyan;
            op = p;
            int i = (int)code[p].f;
            int _a = code[p].a;



            int j = 1;
            switch (i)
            {
                case 0:								//lit
                    t = t + 1;
                    s[t] = _a;
                    break;
                case 1:								//opr
                    switch (_a)
                    {
                        case 0:
                            if (isreturn == 1)
                            {
                                returnresult = s[t];
                            }

                            t = b - 1;
                            p = s[t + 3];
                            b = s[t + 2];
                            if (isreturn == 1)
                            {
                                s[++t] = returnresult;
                                isreturn = 0;
                            }
                            break;
                        case 1:
                            s[t] = -s[t];
                            break;
                        case 2:
                            t = t - 1;
                            s[t] = s[t] + s[t + 1];
                            break;
                        case 3:
                            t = t - 1;
                            s[t] = s[t] - s[t + 1];
                            break;
                        case 4:
                            t = t - 1;
                            s[t] = s[t] * s[t + 1];
                            break;
                        case 5:
                            t = t - 1;
                            s[t] = s[t] / s[t + 1];
                            break;
                        case 6:
                            s[t] = s[t] % 2;
                            break;
                        case 8:
                            t = t - 1;
                            s[t] = (s[t] == s[t + 1]) ? 1 : 0;
                            break;
                        case 9:
                            t = t - 1;
                            s[t] = (s[t] != s[t + 1]) ? 1 : 0;
                            break;
                        case 10:
                            t = t - 1;
                            s[t] = (s[t] < s[t + 1]) ? 1 : 0;
                            break;
                        case 11:
                            t = t - 1;
                            s[t] = (s[t] >= s[t + 1]) ? 1 : 0;
                            break;
                        case 12:
                            t = t - 1;
                            s[t] = (s[t] > s[t + 1]) ? 1 : 0;
                            break;
                        case 13:
                            t = t - 1;
                            s[t] = (s[t] <= s[t + 1]) ? 1 : 0;
                            break;
                        case 14://异或
                            t = t - 1;
                            s[t] = s[t] ^ s[t + 1];
                            break;
                        case 15://mode
                            t = t - 1;
                            s[t] = s[t] % s[t + 1];
                            break;
                    }
                    break;
                case 2:								//lod
                    t = t + 1;
                    s[t] = s[basex(code[p].l, b) + _a];
                    break;
                case 3:								//sto
                    s[basex(code[p].l, b) + _a] = s[t];
                    //printf("%d\n", s[t]);
                    //fprintf_s(wstream, "%d\n", s[t]);
                    t = t - 1;
                    break;
                case 4:								//cal
                    s[t + 1] = basex(code[p].l, b);
                    s[t + 2] = b;
                    s[t + 3] = p + 1;
                    b = t + 1;
                    p = _a;

                    /*int i=0;
                    while(i<)*/

                    break;
                case 5:								//int
                    t = t + _a;
                    funcsize = _a;
                    j = 1;
                    while (j <= paranumber)
                    {
                        s[t - funcsize + 3 + j] = s[t - funcsize - paranumber + j];
                        j++;
                    }

                    break;
                case 6:								//jmp
                    p = _a;
                    break;
                case 7:								//jpc
                    if (s[t] == 0)
                        p = _a;
                    t = t - 1;
                    break;
                case 8://PAR
                    paranumber = 0;
                    funcsize = 0;
                    paranumber = _a;
                    break;

                case 9:
                    r.ShowDialog();
                    s[basex(code[p].l, b) + _a] = r.a;
                    break;

                case 10:
                    t = t + 1;
                    s[t] = s[basex(code[p].l, b) + _a];
                    richTextBox4.AppendText(s[basex(code[p].l, b) + _a].ToString() + "\n");
                    t = t - 1;
                    break;

                case 11:
                    richTextBox4.AppendText(_a.ToString() + "\n");
                    break;
            }
            //p++;
            if ((i == 1 && _a == 0) || (i == 4) || (i == 6) || (i == 7 && s[t + 1] == 0)) p--;

        }


        private void 单步ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (times == 0)
            {
                begin();
                p = 0;
                t = -1;
                b = 0;
                s[0] = 0;
                s[1] = 0;
                s[2] = 0;
                op = p;
                if (err == 0)
                {
                    listall();
                }
                times = 1;
            }
            if (p >= 0)
            {
                onebyone();
                richTextBox3.Clear();
                for (int i = 0; i <= t; i++)
                {
                    richTextBox3.AppendText(s[i].ToString() + "\n");
                }
                p++;
                if (p == 0)
                {
                    p = -1;
                    ew.Close();
                    sr.Close();
                }
            }
        }


    }
}
    


