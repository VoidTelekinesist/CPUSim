﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Reflection;
using System.IO;

namespace CPUSim
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public DispatcherTimer timer = new DispatcherTimer();

        public MainWindow()
        {
            InitializeComponent();
            BuildRegisters();
            BuildRAM();
            Sim.SetRAM(RAM);
            Sim.Setreg(Registers);
            SetHelp();
            BuildList();

            timer.Interval = TimeSpan.FromMilliseconds(1000);
            timer.Tick += CPU_Cycle;
            

        }

        public Int16 GetMem(string addr)
        {
            TextBox cell = (TextBox)FindName("M" + addr);
            Int16 value = Int16.Parse(cell.Text, System.Globalization.NumberStyles.HexNumber);
            return value;
        }

        public void SetMem(string addr, string value)
        {
            TextBox cell = (TextBox)this.FindName("M" + addr);
            cell.Text = value;
        }

        public Int16 GetReg(string addr)
        {
            TextBox cell = (TextBox)this.FindName("R" + addr);
            Int16 value = Int16.Parse(cell.Text, System.Globalization.NumberStyles.HexNumber);
            return value;
        }

        public void SetReg(string addr, string value)
        {
            TextBox cell = (TextBox)this.FindName("R" + addr);
            cell.Text = value;
        }

        void BuildRegisters()
        {
            for(int i = 0; i < 16; i++)
            {
                RowDefinition row = new RowDefinition()
                {
                    Height = new GridLength(18)
                };
                Registers.RowDefinitions.Add(row);

                StackPanel st = new StackPanel()
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(5, 0, 0, 0)
                };
                Grid.SetColumn(st, 0);
                Grid.SetRow(st, i);

                TextBlock name = new TextBlock
                {
                    Text = i.ToString("X"),
                    Width = 25,
                    TextAlignment = TextAlignment.Center,
                };
                st.Children.Add(name);

                TextBox tb = new TextBox
                {
                    Name = "R" + i.ToString(),
                    Width = 50,
                    Text = "00",
                    
                };
                tb.TextChanged += new TextChangedEventHandler(TextBox_TextChanged);
                tb.LostFocus += new RoutedEventHandler(TextBox_LostFocus);
                tb.GotFocus += new RoutedEventHandler(TextBox_GotFocus);
                st.Children.Add(tb);

                Registers.Children.Add(st);

            }
            RowDefinition space = new RowDefinition() //Adds a space between PC and the other registers
            {
                Height = new GridLength(10),
            };
            RowDefinition PC = new RowDefinition()
            {
                Height = new GridLength(20),
            };
            Registers.RowDefinitions.Add(space);
            Registers.RowDefinitions.Add(PC);

                                                       //Why the 2s? Because variable names. THAT is why
            StackPanel st2 = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(5, 0, 0, 0)
            };
            Grid.SetColumn(st2, 0);
            Grid.SetRow(st2, 17);

            TextBlock name2 = new TextBlock
            {
                Text = "PC",
                Width = 25,
                TextAlignment = TextAlignment.Center,
            };
            st2.Children.Add(name2);

            TextBox tb2 = new TextBox
            {
                Name = "PC",
                Width = 50,
                Text = "00",
            };
            tb2.TextChanged += new TextChangedEventHandler(TextBox_TextChanged);
            tb2.LostFocus += new RoutedEventHandler(TextBox_LostFocus);
            tb2.GotFocus += new RoutedEventHandler(TextBox_GotFocus);
            st2.Children.Add(tb2);

            Registers.Children.Add(st2);

        }
        void BuildRAM()
        {
            for (int y = 0; y < 17; y++)
            {
                ColumnDefinition col = new ColumnDefinition();
                RAM.ColumnDefinitions.Add(col);
                for (int x = 0; x < 17; x++)
                {
                    RowDefinition row = new RowDefinition();
                    RAM.RowDefinitions.Add(row);
                    WrapPanel wp = new WrapPanel()
                    {
                        Height = 18,
                        Width = 25,
                        HorizontalAlignment = HorizontalAlignment.Center
                    };
                    if (y == 0 && x == 0)
                    {
                        //Do nothing
                    }
                    else if (y == 0)
                    {
                        TextBlock tb = new TextBlock()
                        {
                            Width = 25,
                            Text = (x-1).ToString("X"),
                            TextAlignment = TextAlignment.Center
                        };
                        wp.Children.Add(tb);
                    }
                    else if (x == 0)
                    {
                        TextBlock tb = new TextBlock()
                        {
                            Height = 25,
                            Width = 25,
                            TextAlignment = TextAlignment.Center,
                            Text = (y - 1).ToString("X"),
                        };
                        wp.Children.Add(tb);
                    }
                    else
                    {
                        TextBox tb = new TextBox()
                        {
                            Height = 25,
                            Width = 25,
                            TextAlignment = TextAlignment.Center,
                            Text = "00",
                            Name = "M" + (y - 1).ToString("X") + (x-1).ToString("X"),
                        };
                        tb.TextChanged += new TextChangedEventHandler(TextBox_TextChanged);
                        tb.LostFocus += new RoutedEventHandler(TextBox_LostFocus);
                        tb.GotFocus += new RoutedEventHandler(TextBox_GotFocus);
                        wp.Children.Add(tb);
                    }
                    Grid.SetRow(wp, y);
                    Grid.SetColumn(wp, x);
                    RAM.Children.Add(wp);
                }
            }
        }

        private void BuildList()
        {
            for (int i = 0; i < 255; i+=2)
            {
                string temp = i.ToString("X");
                if (temp.Length == 1)
                {
                    temp = "0" + temp;
                }
                ListOfAdresses.Text += temp + "\n";
            }
        }

        private void Step_Click(object sender, RoutedEventArgs e)
        {
            Sim.Step();
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            tb.SelectAll();     //Does not work well with clicking, but it is not unusable
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e) //Only intented for RAM and register autoformatting while typing
        {
            TextBox tb = (TextBox)sender;
            if (tb.Text.Length > 2)
            {
                if (tb.CaretIndex == 1)
                {
                    tb.Text = tb.Text[0].ToString() + tb.Text[2].ToString();
                    tb.CaretIndex = 1;
                }
                else if (tb.CaretIndex == 2)
                {
                    tb.Text = tb.Text[0].ToString() + tb.Text[1].ToString();
                    tb.CaretIndex = 2;
                }
                else if (tb.CaretIndex == 3)
                {
                    tb.Text = tb.Text[1].ToString() + tb.Text[2].ToString();
                    tb.CaretIndex = 2;
                }
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e) //Only intented for RAM and register formatting. Sanitizes TextBox content
        {
            TextBox tb = (TextBox)sender;
            if (tb.Text.Length > 2)
            {
                tb.Text = tb.Text[1].ToString() + tb.Text[2].ToString();
            }
            else if (tb.Text.Length == 1)
            {
                tb.Text = "0" + tb.Text;
            }
            else if (tb.Text.Length == 0)
            {
                tb.Text = "00";
            }
            tb.Text = Regex.Replace(tb.Text, "[^0-9|a-f|A-F]", "0").ToUpper(); //Replaces illigal characters with 0, and makes them uppercase for consistency

            ExportButton_Click(sender, e);
            
            /**Override the textapp field**/

            /*int num = int.Parse(tb.Name.Substring(1), System.Globalization.NumberStyles.HexNumber);
            int line = (int)(num / 2);
            
            if (TextCode.LineCount <= line)
            {
                TextCode.Text += new String('\n', line + 1 - TextCode.LineCount);   //Adds the missing number of lines
            }

            int charind = TextCode.GetCharacterIndexFromLineIndex(line);

            int missing = (4 - TextCode.GetLineLength(line));
            if (missing > 0)
            {
                int linelength = TextCode.GetLineLength(line);
                string front = TextCode.Text.Substring(0, charind + linelength);
                string back = "";
                if (TextCode.Text.Length > charind + linelength + 1)
                {
                    back = TextCode.Text.Substring(charind + linelength + 1);
                }
                TextCode.Text = front + new String('0', missing) + back;
            }

            if (num % 2 == 0)
            {
                string front = TextCode.Text.Substring(0, charind);
                string back = TextCode.Text.Substring(charind + 2);
                TextCode.Text = front + tb.Text + back;
            }
            else
            {
                string front = TextCode.Text.Substring(0, charind + 2);
                string back = TextCode.Text.Substring(charind + 4);
                TextCode.Text = front + tb.Text + back;
            }*/
            
        }

        GridLength HelpSidePanelWidth = new GridLength(0);
        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            Button bt = (Button)sender;
            TextBlock tb = (TextBlock)bt.Content;
            WrapPanel sp = (WrapPanel)FindName("HelpSideBar");
            ColumnDefinition gc = (ColumnDefinition)FindName("SideBar");
            if (tb.Text == "Hide Sidebar")
            {
                tb.Text = "Show Sidebar";
                HelpSidePanelWidth = SideBar.Width;
                SideBar.Width = new GridLength(0);
            }
            else
            {
                tb.Text = "Hide Sidebar";
                SideBar.Width = HelpSidePanelWidth;
            }
        }

        private void RunButton_Click(object sender, RoutedEventArgs e)
        {
            TextBlock tb = (TextBlock)RunButton.Content;
            if (Sim.isRunning)
            {
                tb.Text = "Run";
                Sim.isRunning = false;
                timer.Stop();
            }
            else
            {
                tb.Text = "Pause";
                Sim.isRunning = true;
                timer.Start();
            }
            
        }
        private void CPU_Cycle(object sender, EventArgs e)
        {
            bool stop = Sim.Cycle();
            if (stop)
            {
                RunButton_Click(sender, new RoutedEventArgs());
            }
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Slider sl = (Slider)sender;
            if (Frequency != null)
            {
                int val = (int)Math.Round(0.99310918 * Math.Pow(1.00693863, sl.Value));
                Frequency.Text = val.ToString() + "Hz";
                timer.Interval = TimeSpan.FromMilliseconds(1000 / val);
            }
        }

        private void SetHelp()
        {
            using (Stream st = Assembly.GetExecutingAssembly().GetManifestResourceStream("CPUSim.Help.txt"))
            using (StreamReader reader = new StreamReader(st))
            {
                Help.Text = reader.ReadToEnd();
            }
        }
        
        private void ProgramButton_Click(object sender, RoutedEventArgs e)
        {
            int Shown = Grid.GetColumn(SidebarContent);
            int hid = Grid.GetColumn(Hidden);
            Grid.SetColumn(SidebarContent, hid);
            Grid.SetColumn(Hidden, Shown);
            TextBlock tb = (TextBlock)((Button)sender).Content;
            if (Shown > hid)
            {
                tb.Text = "Text Program";
            }
            else
            {
                tb.Text = "Help";
            }
        }
        
        private void TextCode_TextChanged(object sender, TextChangedEventArgs e)
        {

            int carind = TextCode.CaretIndex;                                                   //Gets the original cursor position for reference
            int prev = TextCode.Text.Length;                                                    //Gets the original string length for reference
            TextCode.Text = Regex.Replace(TextCode.Text, "[^0-9|a-f|A-F|\n]", "").ToUpper();    //Remove all dissalowed characters, and set to uppercase
            if (TextCode.LineCount > 128)
            {
                TextCode.Text = TextCode.Text.Substring(0, TextCode.GetCharacterIndexFromLineIndex(128)-1);
                carind++;
            }
            carind -= prev - TextCode.Text.Length;                                      //Move the cursor postion back to accomedate changed string
            prev = TextCode.Text.Length;                                                //Gets the sanitized string lenght for reference
            TextCode.Text = Regex.Replace(TextCode.Text, "(.)(....)", "$2");        //Prevent linelength to be more than 4 characters (excluding newline)
            
            
            TextCode.CaretIndex = carind;                                       //Sets the cursor position to the calculated correct position
            TextProgramToRam();
            TextCodeToWritten();
        }

        private void TextCodeToWritten()
        {
            string words = "";
            for (int i = 0; i < TextCode.LineCount; i++)
            {
                string line = TextCode.GetLineText(i).Replace("\n", "");
                if (line.Length < 4)
                {
                    line += new string('0', 4 - line.Length);
                }
                
                switch (line[0])
                {
                    case '0':
                        words += "Go to next instruction";
                        break;
                    case '1':
                        words += "Copy content of RAM adress " + line.Substring(2) + " to register " + line[1];
                        break;
                    case '2':
                        words += "Set the content of register " + line[1] + " to " + line.Substring(2);
                        break;
                    case '3':
                        words += "Copy the content of register " + line[1] + " to RAM adress " + line.Substring(2);
                        break;
                    case '4':
                        words += "Copy the content of register " + line[2] + " to register " + line[3];
                        break;
                    case '5':
                        words += "Add registers " + line[2] + " and " + line[3] + " as two's compliment, and store result in register " + line[1];
                        break;
                    case '6':
                        words += "Add registers " + line[2] + " and " + line[3] + " as floating point, and store result in register " + line[1];
                        break;
                    case '7':
                        words += "Perform bitwise OR with registers " + line[2] + " and " + line[3] + ", and store result in register " + line[1];
                        break;
                    case '8':
                        words += "Perform bitwise AND with registers " + line[2] + " and " + line[3] + ", and store result in register " + line[1];
                        break;
                    case '9':
                        words += "Perform bitwise XOR with registers " + line[2] + " and " + line[3] + ", and store result in register " + line[1];
                        break;
                    case 'A':
                        words += "Perform cyclical rotation on register " + line[1] + ", " + line[3] + " time";
                        if (line[3] != '1')
                        {
                            words += 's';
                        }
                        break;
                    case 'B':
                        words += "Set PC to " + line.Substring(2) + " if register " + line[1] + " is equal to register 0";
                        break;
                    case 'C':
                        words += "Halt the program";
                        break;
                    case 'D':
                        words += "Set PC to " + line.Substring(2) + " if register " + line[1] + " is larger than register 0 in two's compliment";
                        break;
                        
                    default:
                        words += "Undefined behaviour";
                        break;
                }
                words += "\n";
            }
            WhatItDo.Text = words;
        }

        private void TextProgramToRam()
        {
            if (TextCode.LineCount > 128)
            {
                //MessageBox.Show("Program does not fit in RAM");
                return;
            }

            
            for (int l = 0; l < TextCode.LineCount; l++)            //For every line
            {
                string temp = "";
                string line = TextCode.GetLineText(l);              //Get the text of said line

                if (line != "\n")                                   //Skip if line is empty
                {
                    temp = line;
                    temp = temp.Replace("\n", "");                  //Remove newline
                    
                    temp += new string('0', 4 - temp.Length);       //Pad empty space with zeroes
                    Sim.RAM[l * 2].Text = temp.Substring(0, 2);     //Write to even RAM words
                    Sim.RAM[l * 2 + 1].Text = temp.Substring(2);    //Write to uneven RAM words
                }
            }
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)   //Ovewrides the text program with program in RAM
        {
            string temp = "";
            for (int i = 0; i < 256; i += 2)
            {
                if (Sim.RAM[i].Text == "00" && Sim.RAM[i+1].Text == "00")
                {
                    temp += "\n";
                }
                else
                {
                    temp += Sim.RAM[i].Text + Sim.RAM[i + 1].Text + "\n";
                }
            }
            TextCode.Text = Regex.Replace(temp, @"\n*\z", "");
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < Sim.reg.Length; i++)
            {
                Sim.reg[i].Text = "00";
            }
            Sim.PC.Text = "00";
        }
    }
}
