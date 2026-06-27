# CYBIEEE — Cybersecurity Awareness Chatbot (Part 3 / POE)

## Project Overview

A WPF-based (XAML / C#) Cybersecurity Awareness Chatbot built on top of the Part 1 and Part 2 foundation.  
**The startup project in Visual Studio is `taskChatt`.**

---

## Features

### Task 1 — Task Assistant with Reminders (GUI + Database)
- Add cybersecurity tasks with a **title**, **description**, and optional **reminder date**
- Tasks are stored in a **SQL Server** database (`TaskChat`)
- View all tasks in the Tasks tab or by typing `show tasks` in chat
- Mark tasks as completed or delete them via the UI buttons or chat commands
- Reminder date is stored and displayed alongside each task

### Task 2 — Cybersecurity Mini-Game / Quiz (GUI)
- **12 cybersecurity questions** covering phishing, passwords, 2FA, malware, VPNs, ransomware, social engineering, and more
- Mix of **multiple-choice** and **true/false** formats
- Questions are **shuffled** each time for variety
- **Immediate feedback** after each answer with an explanation
- Final **score and personalised feedback** at the end

### Task 3 — NLP Simulation (GUI)
- Keyword detection using `string.Contains()` for flexible phrase recognition
- Recognises variations like:
  - "add task" / "new task" / "create task" / "i need to" / "remind me to"
  - "show tasks" / "list tasks" / "my tasks" / "view tasks"
  - "complete task 3" / "finish task 3" / "done with task 3"
  - "delete task 2" / "remove task 2"
  - "start quiz" / "play quiz"
  - "show activity log" / "what have you done" / "history" / "show log"
- Multi-turn conversation flow for adding tasks (title → description → reminder)
- Retains all Part 1 and Part 2 features: sentiment detection, memory, keyword responses, random responses

### Task 4 — Activity Log (GUI)
- Every significant action is timestamped and logged
- Logged events include: task added/completed/deleted, quiz started/completed, NLP interactions, session start
- Displays **last 10 actions** in the Activity Log tab
- Accessible via chat: type `show activity log` or `what have you done`
- Clear log button available in 
## Project Structure

```
taskChatt/
├── App.xaml / App.xaml.cs          — Application entry point
├── Session.cs                       — Static class to share username across windows
├── MainWindow.xaml / .cs            — Login screen (enter name to start)
├── ChatbotWindow.xaml / .cs         — Main chatbot UI (4 tabs)
├── DatabaseHelper.cs                — SQL Server data access layer (CRUD for Tasks)
├── QuizEngine.cs                    — 12 cybersecurity quiz questions
├── taskChatt.csproj                 — Project file (targets net8.0-windows)
└── TaskChatDB.sql                   — SQL script to create database and table
```

---

## Chat Commands Reference

| Command | What it does |
|---|---|
| `help` | Shows all available commands |
| `add task` | Starts multi-turn task creation flow |
| `add task Enable 2FA` | Jumps straight to description step |
| `show tasks` | Lists tasks and switches to Tasks tab |
| `complete task 3` | Marks task with ID 3 as completed |
| `delete task 2` | Deletes task with ID 2 |
| `start quiz` | Starts the cybersecurity quiz |
| `show activity log` | Shows recent activity summary |
| `what have you done` | Same as above |
| `phishing` | Info about phishing attacks |
| `password` | Password safety tips |
| `malware` | Info about malware/viruses |
| `privacy` | Privacy protection tips |
| `2fa` | Two-Factor Authentication info |
| `vpn` | VPN usage info |
| `firewall` | Firewall explanation |
| `backup` | Data backup advice |
| `how are you` | Casual greeting |
| `my favorite topic is X` | Stores your favourite topic in 
- [ ] `TaskChatDB.sql` script included
- [ ] All source files committed (no bin/obj folders)
