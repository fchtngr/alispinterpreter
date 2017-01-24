using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string COMMAND_PREFIX = "ttl> "; //tini-tiny lisp interpreter ;)

        private LispInterpreter.Interpreter interpreter;
        private Dictionary<string, LispInterpreter.Expression> expressions;
      
        public MainWindow()
        {
            InitializeComponent();
            expressions = new Dictionary<string, LispInterpreter.Expression>();
            interpreter = new LispInterpreter.InterpreterImpl();
            ShowEnvironment(interpreter.Env);
            EvalButton.IsEnabled = false;

            Output.SelectionChanged += Output_SelectionChanged;
            Input.KeyUp += Input_KeyUp;
        }

        private void EvalButton_Click(object sender, RoutedEventArgs e)
        {
            Eval();
        }

        private void Eval()
        {
            string inputStr = Input.Text;
            try
            {
                string outputStr = interpreter.Interpret(inputStr);
                inputStr = COMMAND_PREFIX + inputStr;
                Output.Items.Add(inputStr);
                Output.Items.Add(outputStr);

                //save expression
                expressions[inputStr] = interpreter.LastExpression;
                LispInterpreter.Environment env = interpreter.Env;

                ShowEnvironment(env);

                Input.Clear();
            }
            catch (LispInterpreter.LispException e)
            {
                Status.Content = e.Message;
            }
            catch (DivideByZeroException)
            {
                Status.Content = "dividing by zero not possible!";
            }
            catch
            {
                Status.Content = "an error occured, please try something else ;)";
            }
        }

        private void ShowEnvironment(LispInterpreter.Environment env)
        {
            environment.Items.Clear();
            Dictionary<string,LispInterpreter.Expression> dict = env.Dictionary;
            foreach (string key in dict.Keys)
            {
                LispInterpreter.Expression e = dict[key];
                environment.Items.Add(new GridItem(key, e.ToString(), e.Documentation));
            }
            
        }

        private void ShowTree(LispInterpreter.Expression exp)
        {
            ExpressionTree.Items.Clear();
            TreeViewItem item = new TreeViewItem();
            item.IsExpanded = true;
            AddToTreeItem(item, exp);
            ExpressionTree.Items.Add(item);
            
        }

        private void AddToTreeItem(TreeViewItem item, LispInterpreter.Expression exp)
        {
            item.Header = exp.Type;
            if (exp.Type == LispInterpreter.LispType.NUMBER || exp.Type == LispInterpreter.LispType.SYMBOL)
            {
                item.Header += ": " + exp.Val;
            }
            foreach (LispInterpreter.Expression e in exp.Childs)
            {
                TreeViewItem child = new TreeViewItem();
                child.IsExpanded = true;
                item.Items.Add(child);
                AddToTreeItem(child, e);
            }
        }

        void Output_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string str = Output.SelectedItem.ToString();
            if (!str.StartsWith("ttl> "))
            {
                Output.SelectedIndex = Output.SelectedIndex - 1;
                str = Output.Items[Output.SelectedIndex].ToString();
            }
            ShowTree(expressions[str]);
        }

        void Input_KeyUp(object sender, KeyEventArgs e)
        {
            // load last input and copy it to the input box
            if (e.Key == Key.Up)
            {
                if (Output.Items.Count >= 2) // is there an old input?
                {
                    StringBuilder sb = new StringBuilder(Output.Items[Output.Items.Count - 2].ToString());
                    sb.Replace("ttl> ", ""); //remove the command prefix
                    Input.Text = sb.ToString();
                }
            }

            //validate input string before trying to interpret it
            //panic mode: once an error was detected display it and abort process
            {
                EvalButton.IsEnabled = false;
                string str = Input.Text;
                // --> string must start and end with a parenthesis
                if (!str.StartsWith("("))
                {
                    Status.Content ="expression must start with a parenthesis!";
                    return;
                }
                if (!str.EndsWith(")"))
                {
                    Status.Content ="expression must end with a parenthesis!";
                    return;
                }

                // --> check if parentheses match
               
                Stack<char> opens = new Stack<char>();
                
                foreach (char c in str)
                {
                    if (c == '(')
                    {
                        opens.Push(c);
                    }
                    else if (c == ')')
                    {
                        if (opens.Count == 0)
                        {   //error, stack is empty therefore there are too many closing parentheses
                            Status.Content = "you closed too many parentheses";
                            return;
                        }
                        opens.Pop();
                    }
                }
                if (opens.Count == 1)
                {   //error, one closing parenthesis is missing
                    Status.Content = "missing closing parenthis!";
                    return;
                }
                else if (opens.Count > 1)
                {   //error, more than one parentheses are missing
                    Status.Content = "missing closing parentheses!";
                    return;
                }

                EvalButton.IsEnabled = true;
                Status.Content = "";
            } //input seems to be okay

           
            //evaluate input string on enter key
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                Eval();
                Input.Text = "";
            }
        }
    }
}