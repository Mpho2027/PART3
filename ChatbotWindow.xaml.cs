using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace taskChatt
{
    public partial class ChatbotWindow : Window
    {
        // ── State ─────────────────────────────────────────────────────
        private static readonly Random Rng = new();
        private string _favTopic = "";

        // Task multi-turn state machine: 0=idle,1=awaiting title,2=awaiting desc,3=awaiting reminder
        private int    _taskFlowState    = 0;
        private string _pendingTaskTitle = "";
        private string _pendingTaskDesc  = "";

        // ── Activity log ──────────────────────────────────────────────
        private readonly List<string> _activityLog = new();

        // ── Quiz state ────────────────────────────────────────────────
        private List<QuizQuestion> _quizQuestions = new();
        private int  _quizIndex   = 0;
        private int  _quizScore   = 0;
        private bool _quizAnswered = false;

        // ================================================================
        public ChatbotWindow()
        {
            InitializeComponent();
            lblUser.Text = $"👤 {Session.UserName}";
            BotSay($"Welcome {Session.UserName}! I'm CYBIEEE, your cybersecurity assistant. " +
                   "Ask me about phishing, passwords, malware, privacy, or type 'help' to see what I can do.");
            LogAction("Session started");
        }

        // ================================================================
        // CHAT
        // ================================================================
        private void btnSend_Click(object sender, RoutedEventArgs e) => SendMessage();
        private void txtMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) SendMessage();
        }

        private void SendMessage()
        {
            string input = txtMessage.Text.Trim();
            if (string.IsNullOrWhiteSpace(input)) return;
            AddBubble(input, isUser: true);
            txtMessage.Clear();
            BotSay(GetResponse(input));
        }

        private void BotSay(string msg) => AddBubble(msg, isUser: false);

        private void AddBubble(string text, bool isUser)
        {
            Border bubble = new()
            {
                Background = isUser
                    ? new SolidColorBrush(Color.FromRgb(80, 80, 80))
                    : new SolidColorBrush(Color.FromRgb(40, 90, 55)),
                CornerRadius = new CornerRadius(13),
                Padding      = new Thickness(12, 8, 12, 8),
                Margin       = new Thickness(5, 3, 5, 3),
                MaxWidth     = 540,
                HorizontalAlignment = isUser ? HorizontalAlignment.Right : HorizontalAlignment.Left
            };
            bubble.Child = new TextBlock
            {
                Text         = text,
                Foreground   = Brushes.White,
                FontSize     = 14,
                TextWrapping = TextWrapping.Wrap
            };
            pnlChat.Children.Add(bubble);
            ChatScrollViewer.ScrollToEnd();
        }

        // ================================================================
        // NLP / RESPONSE ENGINE
        // ================================================================
        private string GetResponse(string raw)
        {
            string input = raw.ToLower().Trim();

            // ── Multi-turn task flow ──────────────────────────────────
            if (_taskFlowState == 1) // awaiting title
            {
                _pendingTaskTitle = raw.Trim();
                _taskFlowState = 2;
                return $"Got it: '{_pendingTaskTitle}'. Now give me a brief description for this task.";
            }
            if (_taskFlowState == 2) // awaiting description
            {
                _pendingTaskDesc = raw.Trim();
                _taskFlowState = 3;
                return "Would you like a reminder? Type a date (e.g. '25 Jul 2025'), 'in 3 days', or 'no' to skip.";
            }
            if (_taskFlowState == 3) // awaiting reminder
            {
                _taskFlowState = 0;
                DateTime? reminder = null;
                if (input != "no" && input != "none" && input != "skip")
                {
                    if (DateTime.TryParse(raw, out DateTime d))
                        reminder = d;
                    else if (TryParseDays(input, out int days))
                        reminder = DateTime.Now.AddDays(days);
                }
                bool ok = DatabaseHelper.AddTask(_pendingTaskTitle, _pendingTaskDesc, reminder);
                if (ok)
                {
                    string msg = $"Task '{_pendingTaskTitle}' added to the database.";
                    if (reminder.HasValue) msg += $" Reminder set for {reminder.Value:dd MMM yyyy}.";
                    LogAction($"Task added: '{_pendingTaskTitle}'" +
                              (reminder.HasValue ? $" (reminder {reminder.Value:dd MMM yyyy})" : ""));
                    RefreshTaskList();
                    return msg;
                }
                return "Could not save the task. Check that the database is connected.";
            }

            // ── Help ─────────────────────────────────────────────────
            if (input.Contains("help") || input.Contains("what can you do"))
                return "I can help you with:\n" +
                       "• Cybersecurity info — phishing, passwords, malware, privacy, scams, 2fa, vpn, firewall, backup\n" +
                       "• Tasks — 'add task', 'show tasks', 'complete task [id]', 'delete task [id]'\n" +
                       "• Quiz — 'start quiz'\n" +
                       "• Activity log — 'show activity log' or 'what have you done'";

            // ── Activity log ─────────────────────────────────────────
            if (input.Contains("activity log") || input.Contains("what have you done") ||
                input.Contains("show log") || input.Contains("history"))
            {
                LogAction("User viewed activity log via chat");
                MainTabs.SelectedIndex = 3;
                return ShowActivityLogSummary();
            }

            // ── Quiz ─────────────────────────────────────────────────
            if (input.Contains("start quiz") || input.Contains("play quiz") ||
                (input.Contains("quiz") && !input.Contains("question")))
            {
                LogAction("Quiz started via chat");
                MainTabs.SelectedIndex = 2;
                StartQuiz();
                return "Switching to the Quiz tab! Good luck!";
            }

            // ── Show tasks ───────────────────────────────────────────
            if (input.Contains("show task") || input.Contains("view task") ||
                input.Contains("list task") || input.Contains("my task") ||
                input.Contains("what tasks"))
            {
                LogAction("User viewed tasks via chat");
                MainTabs.SelectedIndex = 1;
                return ShowTasksSummary();
            }

            // ── Complete task ────────────────────────────────────────
            if ((input.Contains("complete") || input.Contains("finish") || input.Contains("done with")) &&
                input.Contains("task"))
            {
                int id = ExtractId(input);
                if (id > 0)
                {
                    if (DatabaseHelper.MarkCompleted(id))
                    {
                        LogAction($"Task {id} marked complete via chat");
                        RefreshTaskList();
                        return $"Task {id} marked as completed!";
                    }
                    return "Could not update that task. Is the ID correct?";
                }
                return "Please specify the task ID, e.g. 'complete task 3'.";
            }

            // ── Delete task ──────────────────────────────────────────
            if ((input.Contains("delete") || input.Contains("remove")) && input.Contains("task"))
            {
                int id = ExtractId(input);
                if (id > 0)
                {
                    if (DatabaseHelper.DeleteTask(id))
                    {
                        LogAction($"Task {id} deleted via chat");
                        RefreshTaskList();
                        return $"Task {id} has been deleted.";
                    }
                    return "Could not delete that task.";
                }
                return "Please specify the task ID, e.g. 'delete task 2'.";
            }

            // ── Add task ─────────────────────────────────────────────
            if (input.Contains("add task") || input.Contains("new task") ||
                input.Contains("create task") || input.Contains("set up task") ||
                input.Contains("remind me to") || input.Contains("i need to") ||
                input.Contains("enable 2fa") || input.Contains("update password") ||
                input.Contains("review") && input.Contains("settings"))
            {
                string title = ExtractAfterKeyword(raw,
                    new[] { "add task", "new task", "create task", "set up task",
                            "remind me to", "i need to" });
                if (!string.IsNullOrWhiteSpace(title))
                {
                    _pendingTaskTitle = title.Trim();
                    _taskFlowState = 2;
                    return $"Adding task: '{_pendingTaskTitle}'. Please give me a brief description.";
                }
                _taskFlowState = 1;
                return "Sure! What is the title of your cybersecurity task?";
            }

            // ── 2FA ──────────────────────────────────────────────────
            if (input.Contains("2fa") || input.Contains("two-factor") ||
                input.Contains("two factor") || input.Contains("multi-factor"))
            {
                LogAction("NLP: 2FA info requested");
                return "Two-Factor Authentication (2FA) adds a second layer of security. Even if your password is stolen, " +
                       "the attacker still needs your second factor (SMS code, authenticator app, or hardware key). Enable 2FA everywhere you can!";
            }

            // ── Basic conversation ───────────────────────────────────
            if (input.Contains("how are you"))
                return $"I'm functioning perfectly, {Session.UserName}! Ready to help you stay safe online.";

            if (input.Contains("your purpose") || input.Contains("what do you do") || input.Contains("who are you"))
                return "I'm CYBIEEE, your Cybersecurity Awareness Chatbot. I help you learn about online safety, manage tasks, and quiz your knowledge!";

            // ── Sentiment ────────────────────────────────────────────
            if (input.Contains("worried") || input.Contains("scared") || input.Contains("nervous"))
                return $"Don't worry, {Session.UserName}! Small steps make a big difference. Start with strong passwords and enabling 2FA!";

            if (input.Contains("happy") || input.Contains("great") || input.Contains("excited"))
                return "That's the spirit! Staying positive while learning cybersecurity is key!";

            if (input.Contains("angry") || input.Contains("frustrated") || input.Contains("annoyed"))
                return "Cybersecurity problems can be frustrating. Take a breath — learning these skills will protect you in the long run!";

            // ── Memory ───────────────────────────────────────────────
            if (input.Contains("my favorite topic is") || input.Contains("my favourite topic is"))
            {
                string[] parts = input.Split("is", 2);
                if (parts.Length > 1)
                {
                    _favTopic = parts[1].Trim();
                    LogAction($"User set favourite topic: {_favTopic}");
                    return $"I'll remember that your favourite topic is '{_favTopic}'!";
                }
            }
            if (input.Contains("favorite topic") || input.Contains("favourite topic"))
                return string.IsNullOrEmpty(_favTopic)
                    ? "You haven't told me your favourite topic yet. Say 'My favorite topic is ...'."
                    : $"Your favourite topic is '{_favTopic}'.";

            // ── Cybersecurity keywords ───────────────────────────────
            if (input.Contains("phishing"))
            {
                LogAction("NLP: phishing info requested");
                string[] opts = {
                    "Phishing tricks you into revealing personal info via fake emails or websites. Always verify the sender!",
                    "Hover over links before clicking — phishing URLs often look almost legitimate but have small differences.",
                    "Never provide passwords or banking details via email links. Go directly to the official website instead."
                };
                return opts[Rng.Next(opts.Length)];
            }
            if (input.Contains("password"))
            {
                LogAction("NLP: password info requested");
                string[] opts = {
                    "Strong passwords use symbols, numbers, uppercase and lowercase — at least 12 characters.",
                    "Never reuse passwords across different sites. A password manager like Bitwarden can help!",
                    "Avoid obvious passwords like '123456'. Use a long passphrase such as 'BlueSky!Rain42' instead."
                };
                return opts[Rng.Next(opts.Length)];
            }
            if (input.Contains("privacy"))
            {
                LogAction("NLP: privacy info requested");
                return "Privacy protects your personal data from unauthorised access. Review app permissions regularly and minimise what you share online.";
            }
            if (input.Contains("scam"))
                return "Scammers create urgency to make you act without thinking. Pause, verify the source, and never send money or share info under pressure.";

            if (input.Contains("malware") || input.Contains("virus") || input.Contains("ransomware"))
                return "Malware includes viruses, ransomware, and spyware — all designed to steal data or damage systems. Keep your OS and antivirus updated.";

            if (input.Contains("link") || input.Contains("url") || input.Contains("website"))
                return "Before clicking any link, hover over it to reveal the real URL. Check for HTTPS and look for typos in the domain name.";

            if (input.Contains("vpn"))
                return "A VPN encrypts your internet traffic, making it much harder for attackers to intercept your data — especially important on public Wi-Fi.";

            if (input.Contains("firewall"))
                return "A firewall monitors incoming and outgoing network traffic and blocks unauthorised access. It is your network's first line of defence.";

            if (input.Contains("backup") || input.Contains("back up"))
                return "Regular backups protect you from ransomware and hardware failure. Follow the 3-2-1 rule: 3 copies, on 2 different media, with 1 kept offsite.";

            if (input.Contains("social engineering"))
                return "Social engineering exploits human psychology rather than technical weaknesses. Attackers may pose as IT support or colleagues to extract sensitive info.";

            // ── Default ──────────────────────────────────────────────
            return "I am not quite sure I understand that. Try asking about:\n" +
                   "phishing   passwords   malware   privacy   scams   2FA   VPN   firewall   backup\n" +
                   "Or say 'help' to see all commands.";
        }

        // ── Helpers ──────────────────────────────────────────────────
        private static string ExtractAfterKeyword(string input, string[] keywords)
        {
            string lower = input.ToLower();
            foreach (string kw in keywords)
            {
                int idx = lower.IndexOf(kw);
                if (idx >= 0)
                {
                    string after = input.Substring(idx + kw.Length).Trim();
                    if (after.StartsWith(":")) after = after.Substring(1).Trim();
                    return after;
                }
            }
            return "";
        }

        private static int ExtractId(string input)
        {
            string digits = new(input.Where(char.IsDigit).ToArray());
            return int.TryParse(digits, out int id) ? id : 0;
        }

        private static bool TryParseDays(string input, out int days)
        {
            days = 0;
            foreach (string word in input.Split(' '))
                if (int.TryParse(word, out int n)) { days = n; return true; }
            return false;
        }

        private string ShowTasksSummary()
        {
            var tasks = DatabaseHelper.GetAllTasks();
            if (tasks.Count == 0) return "You have no tasks yet. Say 'add task' to create one!";
            var sb = new System.Text.StringBuilder($"You have {tasks.Count} task(s):\n");
            foreach (var t in tasks.Take(5)) sb.AppendLine($"  {t}");
            if (tasks.Count > 5) sb.AppendLine($"  ...and {tasks.Count - 5} more. Check the Tasks tab!");
            return sb.ToString();
        }

        // ================================================================
        // TASK TAB
        // ================================================================
        private void btnAddTask_Click(object sender, RoutedEventArgs e)
        {
            string title = txtTaskTitle.Text.Trim();
            string desc  = txtTaskDesc.Text.Trim();
            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(desc))
            {
                MessageBox.Show("Please fill in both Title and Description.", "Missing Info",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            DateTime? reminder = dpReminder.SelectedDate;
            if (DatabaseHelper.AddTask(title, desc, reminder))
            {
                LogAction($"Task added via UI: '{title}'" +
                          (reminder.HasValue ? $" (reminder {reminder.Value:dd MMM yyyy})" : ""));
                MessageBox.Show("Task added successfully!", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                txtTaskTitle.Clear(); txtTaskDesc.Clear(); dpReminder.SelectedDate = null;
                RefreshTaskList();
            }
        }

        private void btnMarkComplete_Click(object sender, RoutedEventArgs e)
        {
            if (lstTasks.SelectedItem is CyberTask task)
            {
                if (DatabaseHelper.MarkCompleted(task.TaskId))
                {
                    LogAction($"Task marked complete: '{task.Title}' (ID {task.TaskId})");
                    RefreshTaskList();
                }
            }
            else MessageBox.Show("Please select a task first.", "No Selection",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void btnDeleteTask_Click(object sender, RoutedEventArgs e)
        {
            if (lstTasks.SelectedItem is CyberTask task)
            {
                if (MessageBox.Show($"Delete task '{task.Title}'?", "Confirm",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    if (DatabaseHelper.DeleteTask(task.TaskId))
                    {
                        LogAction($"Task deleted: '{task.Title}' (ID {task.TaskId})");
                        RefreshTaskList();
                    }
                }
            }
            else MessageBox.Show("Please select a task first.", "No Selection",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void btnRefreshTasks_Click(object sender, RoutedEventArgs e) => RefreshTaskList();

        private void RefreshTaskList()
        {
            lstTasks.Items.Clear();
            foreach (var t in DatabaseHelper.GetAllTasks())
                lstTasks.Items.Add(t);
        }

        // ================================================================
        // QUIZ TAB
        // ================================================================
        private void btnStartQuiz_Click(object sender, RoutedEventArgs e) => StartQuiz();

        private void StartQuiz()
        {
            _quizQuestions = QuizEngine.Questions.OrderBy(_ => Rng.Next()).ToList();
            _quizIndex     = 0;
            _quizScore     = 0;
            _quizAnswered  = false;
            LogAction("Quiz started");
            btnStartQuiz.Visibility    = Visibility.Collapsed;
            btnNextQuestion.Visibility = Visibility.Visible;
            ShowQuestion();
        }

        private void ShowQuestion()
        {
            if (_quizIndex >= _quizQuestions.Count) { EndQuiz(); return; }
            var q = _quizQuestions[_quizIndex];
            _quizAnswered = false;
            borderFeedback.Visibility  = Visibility.Collapsed;
            btnNextQuestion.Visibility = Visibility.Collapsed;
            lblQuestion.Text       = q.Question;
            lblQuizProgress.Text   = $"Question: {_quizIndex + 1} / {_quizQuestions.Count}";
            lblQuizScore.Text      = $"Score: {_quizScore}";
            pnlAnswers.Children.Clear();

            for (int i = 0; i < q.Options.Count; i++)
            {
                int ci = i;
                var btn = new Button
                {
                    Content    = $"{(char)('A' + i)})  {q.Options[i]}",
                    Margin     = new Thickness(0, 5, 0, 5),
                    Background = new SolidColorBrush(Color.FromRgb(58, 58, 92)),
                    Foreground = Brushes.White,
                    FontSize   = 14,
                    HorizontalContentAlignment = HorizontalAlignment.Left,
                    Cursor     = Cursors.Hand,
                    Padding    = new Thickness(14, 10, 14, 10)
                };
                btn.Template = MakeRoundedTemplate();
                btn.Click   += (s, e) => HandleAnswer(ci, btn);
                pnlAnswers.Children.Add(btn);
            }
        }

        private static ControlTemplate MakeRoundedTemplate()
        {
            var t = new ControlTemplate(typeof(Button));
            var f = new FrameworkElementFactory(typeof(Border));
            f.SetBinding(Border.BackgroundProperty,
                new System.Windows.Data.Binding("Background")
                { RelativeSource = new System.Windows.Data.RelativeSource(
                    System.Windows.Data.RelativeSourceMode.TemplatedParent) });
            f.SetValue(Border.CornerRadiusProperty, new CornerRadius(8));
            f.SetValue(Border.PaddingProperty, new Thickness(14, 10, 14, 10));
            var cp = new FrameworkElementFactory(typeof(ContentPresenter));
            cp.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Left);
            f.AppendChild(cp);
            t.VisualTree = f;
            return t;
        }

        private void HandleAnswer(int selected, Button clicked)
        {
            if (_quizAnswered) return;
            _quizAnswered = true;
            var q       = _quizQuestions[_quizIndex];
            bool correct = selected == q.CorrectIndex;
            if (correct)
            {
                _quizScore++;
                clicked.Background       = new SolidColorBrush(Color.FromRgb(30, 110, 40));
                lblFeedback.Text         = $"Correct! {q.Explanation}";
                lblFeedback.Foreground   = new SolidColorBrush(Color.FromRgb(144, 238, 144));
            }
            else
            {
                clicked.Background = new SolidColorBrush(Color.FromRgb(160, 30, 30));
                if (pnlAnswers.Children.Count > q.CorrectIndex &&
                    pnlAnswers.Children[q.CorrectIndex] is Button cb)
                    cb.Background = new SolidColorBrush(Color.FromRgb(30, 110, 40));
                lblFeedback.Text       = $"Incorrect. {q.Explanation}";
                lblFeedback.Foreground = new SolidColorBrush(Color.FromRgb(255, 200, 0));
            }
            borderFeedback.Visibility  = Visibility.Visible;
            btnNextQuestion.Visibility = Visibility.Visible;
            lblQuizScore.Text          = $"Score: {_quizScore}";
            foreach (UIElement ch in pnlAnswers.Children)
                if (ch is Button b) b.IsEnabled = false;
        }

        private void btnNextQuestion_Click(object sender, RoutedEventArgs e)
        {
            _quizIndex++;
            ShowQuestion();
        }

        private void EndQuiz()
        {
            pnlAnswers.Children.Clear();
            borderFeedback.Visibility  = Visibility.Collapsed;
            btnNextQuestion.Visibility = Visibility.Collapsed;
            btnStartQuiz.Content       = "Play Again";
            btnStartQuiz.Visibility    = Visibility.Visible;

            int    total = _quizQuestions.Count;
            double pct   = (double)_quizScore / total * 100;
            string grade = pct >= 80 ? "Great job! You're a cybersecurity pro!"
                         : pct >= 50 ? "Good effort! Keep learning to stay safe online."
                                     : "Keep learning! Cybersecurity knowledge is key to staying safe.";

            lblQuestion.Text     = $"Quiz Complete!\n\nScore: {_quizScore} / {total} ({pct:0}%)\n\n{grade}";
            lblQuizProgress.Text = "Quiz Complete";
            LogAction($"Quiz completed — score: {_quizScore}/{total} ({pct:0}%)");
            BotSay($"Quiz finished! You scored {_quizScore}/{total}. {grade}");
        }

        // ================================================================
        // ACTIVITY LOG
        // ================================================================
        private void LogAction(string description)
        {
            _activityLog.Add($"[{DateTime.Now:HH:mm:ss}] {description}");
            if (_activityLog.Count > 50) _activityLog.RemoveAt(0);
            RefreshLogDisplay();
        }

        private void RefreshLogDisplay()
        {
            lstActivityLog.Items.Clear();
            foreach (var item in _activityLog.TakeLast(10).Reverse())
                lstActivityLog.Items.Add(item);
        }

        private string ShowActivityLogSummary()
        {
            if (_activityLog.Count == 0) return "No activity recorded yet.";
            var sb = new System.Text.StringBuilder("Here's a summary of recent actions:\n");
            int start = Math.Max(0, _activityLog.Count - 5);
            for (int i = start; i < _activityLog.Count; i++)
                sb.AppendLine($"  {i - start + 1}. {_activityLog[i]}");
            return sb.ToString();
        }

        private void btnClearLog_Click(object sender, RoutedEventArgs e)
        {
            _activityLog.Clear();
            RefreshLogDisplay();
        }
    }
}
