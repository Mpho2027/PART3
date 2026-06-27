using System.Collections.Generic;

namespace taskChatt
{
    public class QuizQuestion
    {
        public string Question { get; set; } = "";
        public List<string> Options { get; set; } = new();
        public int CorrectIndex { get; set; }   // 0-based
        public string Explanation { get; set; } = "";
    }

    public static class QuizEngine
    {
        public static readonly List<QuizQuestion> Questions = new()
        {
            new QuizQuestion
            {
                Question = "What should you do if you receive an email asking for your password?",
                Options = new() { "Reply with your password", "Delete the email",
                                  "Report the email as phishing", "Ignore it" },
                CorrectIndex = 2,
                Explanation = "Reporting phishing emails helps protect yourself and others. Legitimate organisations never ask for your password by email."
            },
            new QuizQuestion
            {
                Question = "Which of the following is the strongest password?",
                Options = new() { "123456", "password", "MyDog2010", "T#9kL!mZ@4qP" },
                CorrectIndex = 3,
                Explanation = "Strong passwords mix uppercase, lowercase, numbers and special characters and are at least 12 characters long."
            },
            new QuizQuestion
            {
                Question = "What does '2FA' stand for?",
                Options = new() { "Two-Factor Authentication", "Two-File Access",
                                  "Transfer File Automatically", "Two-Form Application" },
                CorrectIndex = 0,
                Explanation = "Two-Factor Authentication adds an extra layer of security beyond just a password."
            },
            new QuizQuestion
            {
                Question = "True or False: Using the same password for all accounts is safe if it is strong.",
                Options = new() { "True", "False" },
                CorrectIndex = 1,
                Explanation = "False. If one site is breached, attackers can access ALL your accounts. Use unique passwords for each."
            },
            new QuizQuestion
            {
                Question = "What is malware?",
                Options = new() { "A type of firewall", "Harmful software designed to damage or steal data",
                                  "A secure email protocol", "A VPN service" },
                CorrectIndex = 1,
                Explanation = "Malware (malicious software) includes viruses, ransomware, spyware, and trojans."
            },
            new QuizQuestion
            {
                Question = "What is phishing?",
                Options = new() { "A fishing game online", "Encrypting files for ransom",
                                  "Tricking users into revealing personal info via fake messages",
                                  "A type of firewall" },
                CorrectIndex = 2,
                Explanation = "Phishing uses deceptive emails, texts, or websites to steal sensitive information."
            },
            new QuizQuestion
            {
                Question = "True or False: Public Wi-Fi is safe to use for online banking.",
                Options = new() { "True", "False" },
                CorrectIndex = 1,
                Explanation = "False. Public Wi-Fi can be monitored by attackers. Use a VPN or mobile data for sensitive activities."
            },
            new QuizQuestion
            {
                Question = "What does HTTPS in a URL indicate?",
                Options = new() { "The site is government-owned", "The connection is encrypted",
                                  "The site has been verified as safe", "The site has no ads" },
                CorrectIndex = 1,
                Explanation = "HTTPS means the connection between your browser and the site is encrypted, but it does NOT guarantee the site itself is trustworthy."
            },
            new QuizQuestion
            {
                Question = "What is social engineering in cybersecurity?",
                Options = new() { "Building social media platforms", "Manipulating people into revealing confidential information",
                                  "Engineering social networks", "A type of encryption" },
                CorrectIndex = 1,
                Explanation = "Social engineering exploits human psychology rather than technical vulnerabilities to gain access to systems."
            },
            new QuizQuestion
            {
                Question = "True or False: Antivirus software alone is enough to protect your computer.",
                Options = new() { "True", "False" },
                CorrectIndex = 1,
                Explanation = "False. Good security requires a layered approach: antivirus, firewall, updates, strong passwords, and safe browsing habits."
            },
            new QuizQuestion
            {
                Question = "What should you do before clicking a link in an email?",
                Options = new() { "Click it immediately", "Hover over it to check the actual URL",
                                  "Forward it to friends", "Reply to the sender" },
                CorrectIndex = 1,
                Explanation = "Hovering over a link reveals its real destination. If it looks suspicious or doesn't match the sender, don't click it."
            },
            new QuizQuestion
            {
                Question = "What is ransomware?",
                Options = new() { "Software that improves PC performance", "Malware that encrypts files and demands payment",
                                  "A password manager", "A VPN tool" },
                CorrectIndex = 1,
                Explanation = "Ransomware locks your files and demands payment (ransom) to restore access. Regular backups are the best defence."
            }
        };
    }
}
