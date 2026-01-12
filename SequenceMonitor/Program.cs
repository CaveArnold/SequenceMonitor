/*
 * Developer: Cave Arnold
 * AI Assistant: Gemini (Google)
 * Date: 2026-01-09
 * Version: 1.0.0
 * * Logic Abstraction:
 * This application functions as a background monitor for Sequence financial accounts.
 * 1. It parses command-line arguments for authentication, scheduling, and alerting configuration.
 * 2. It initializes a Serilog logger for console and file outputs (rotated daily).
 * 3. It enters a continuous loop based on the specified time interval (n seconds).
 * 4. In each iteration, it performs a secure HTTP POST to the Sequence API using RestSharp.
 * 5. It deserializes the JSON response and iterates through account balances.
 * 6. Account balances are rounded to the nearest cent (2 decimal places).
 * 7. The system compares current balances against the last known state (cached in memory).
 * 8. If a change is detected, the new record is committed to the MSSQLLocalDB 
 * and an email alert is dispatched (if SMTP is configured).
 */

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using RestSharp;
using Serilog;

namespace SequenceMonitor
{
    // --- 1. Command Line Options ---
    public class Options
    {
        [Option('t', "token", Required = true, HelpText = "Bearer access token for the API.")]
        public string AccessToken { get; set; }

        [Option('n', "interval", Required = true, HelpText = "Interval in seconds between checks.")]
        public int IntervalSeconds { get; set; }

        [Option("smtp-server", Required = false, HelpText = "SMTP Server Address (Optional).")]
        public string SmtpServer { get; set; }

        [Option("smtp-port", Required = false, Default = 587, HelpText = "SMTP Port (Optional).")]
        public int SmtpPort { get; set; }

        [Option("email-from", Required = false, HelpText = "Email Sender Address (Optional).")]
        public string EmailFrom { get; set; }

        [Option("email-to", Required = false, HelpText = "Email Recipient Address (Optional).")]
        public string EmailTo { get; set; }

        [Option("smtp-user", Required = false, HelpText = "SMTP Username.")]
        public string SmtpUser { get; set; }

        [Option("smtp-pass", Required = false, HelpText = "SMTP Password.")]
        public string SmtpPass { get; set; }

        [Option("enable-ssl", Required = false, Default = false, HelpText = "Enable SSL for SMTP.")]
        public bool EnableSsl { get; set; }
    }

    // --- 2. JSON Data Models ---
    public class ApiResponse
    {
        public string Message { get; set; }
        public string RequestId { get; set; }
        public ApiData Data { get; set; }
    }

    public class ApiData
    {
        public List<Account> Accounts { get; set; }
    }

    public class Account
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public BalanceInfo Balance { get; set; }
    }

    public class BalanceInfo
    {
        public decimal? AmountInDollars { get; set; }
        public string Error { get; set; }
    }

    // --- 3. Main Logic ---
    class Program
    {
        // Connection string for LocalDB
        private const string ConnectionString = "Server=(localdb)\\MSSQLLocalDB;Database=Guyton-Klinger-Withdrawals;Trusted_Connection=True;MultipleActiveResultSets=true";

        static async Task Main(string[] args)
        {
            // Configure Serilog: Console + Daily Rolling File
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            try
            {
                await Parser.Default.ParseArguments<Options>(args)
                    .WithParsedAsync(RunLoop);
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application failed to start correctly.");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        static async Task RunLoop(Options opts)
        {
            Log.Information("Starting Sequence Monitor...");
            Log.Information("Target Database: Guyton-Klinger-Withdrawals on (localdb)\\MSSQLLocalDB");
            Log.Information("Poll Interval: {Interval} seconds", opts.IntervalSeconds);

            var client = new RestClient("https://api.getsequence.io/accounts");

            while (true)
            {
                try
                {
                    Log.Debug("Polling API...");
                    
                    var request = new RestRequest();
                    request.Method = Method.Post;
                    request.AddHeader("x-sequence-access-token", $"Bearer {opts.AccessToken}");
                    request.AddHeader("Content-Type", "application/json");
                    request.AddJsonBody(new { }); // Empty JSON object per requirement

                    var response = await client.ExecuteAsync(request);

                    if (!response.IsSuccessful)
                    {
                        Log.Error("API Error: {Status} - {Content}", response.StatusCode, response.Content);
                    }
                    else
                    {
                        var data = JsonConvert.DeserializeObject<ApiResponse>(response.Content);
                        if (data?.Data?.Accounts != null)
                        {
                            await ProcessAccounts(data.Data.Accounts, opts);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "An error occurred during the polling cycle.");
                }

                await Task.Delay(opts.IntervalSeconds * 1000);
            }
        }

        private static async Task ProcessAccounts(List<Account> accounts, Options opts)
        {
            var changesDetected = new List<string>();

            foreach (var acc in accounts)
            {
                // Rounding Logic: Round to nearest cent
                decimal? currentBalance = null;
                if (acc.Balance.AmountInDollars.HasValue)
                {
                    currentBalance = Math.Round(acc.Balance.AmountInDollars.Value, 2, MidpointRounding.AwayFromZero);
                }

                // Check Database for previous state
                decimal? lastDbBalance = GetLastBalanceFromDb(acc.Id);

                bool shouldInsert = false;
                string changeMessage = "";

                // Logic: 
                // 1. If we have no record in DB (lastDbBalance is null but we have a current balance) -> Insert (New Account)
                // 2. If we have a record, and the current balance is different -> Insert (Change)
                // 3. If current is null (error state) but we haven't logged that error yet? 
                //    (Simplified: we only compare amounts if they exist).

                if (currentBalance.HasValue)
                {
                    if (lastDbBalance == null)
                    {
                        // New Account found
                        shouldInsert = true;
                        changeMessage = $"New Account Tracked: {acc.Name} - Balance: {currentBalance:C}";
                    }
                    else if (lastDbBalance.Value != currentBalance.Value)
                    {
                        // Balance Changed
                        shouldInsert = true;
                        changeMessage = $"Balance Changed: {acc.Name}. Old: {lastDbBalance:C}, New: {currentBalance:C}";
                    }
                }
                else if (!string.IsNullOrEmpty(acc.Balance.Error))
                {
                    // Optional: If you want to track Error state changes, you would need to check the last Error in DB.
                    // Based on prompt "only when balance amount... has changed", we focus on amount.
                    // However, if we want to log the error to DB, we can insert it. 
                    // Let's Insert if it's an error we haven't just seen (handling errors is tricky without storing last error state).
                    // For this implementation, we will log errors to console but only Insert/Email on Balance numeric changes or initial discovery.
                }

                if (shouldInsert)
                {
                    Log.Information(changeMessage);
                    changesDetected.Add(changeMessage);
                    InsertIntoDatabase(acc, currentBalance);
                }
            }

            // Send Email if applicable
            if (changesDetected.Any() && !string.IsNullOrEmpty(opts.SmtpServer))
            {
                SendEmail(changesDetected, opts);
            }
        }

        private static decimal? GetLastBalanceFromDb(string sequenceId)
        {
            try
            {
                using (var conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    string sql = "SELECT TOP 1 Balance FROM Balances WHERE SequenceID = @SeqId ORDER BY LastUpdate DESC";

                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@SeqId", sequenceId);
                        var result = cmd.ExecuteScalar();

                        if (result != null && result != DBNull.Value)
                        {
                            return (decimal)result;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve last balance from DB.");
            }
            return null;
        }

        private static void InsertIntoDatabase(Account acc, decimal? roundedBalance)
        {
            try
            {
                using (var conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    string sql = @"
                        INSERT INTO [dbo].[Balances] 
                        ([SequenceID], [Name], [Balance], [Type], [Error], [LastUpdate])
                        VALUES 
                        (@SeqId, @Name, @Balance, @Type, @Error, @LastUpdate)";

                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@SeqId", acc.Id);
                        cmd.Parameters.AddWithValue("@Name", acc.Name ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Balance", roundedBalance ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Type", acc.Type ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Error", acc.Balance.Error ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@LastUpdate", DateTime.Now);

                        cmd.ExecuteNonQuery();
                    }
                }
                Log.Debug("Database record inserted for {AccountName}", acc.Name);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Database insert failed for {AccountName}", acc.Name);
            }
        }

        private static void SendEmail(List<string> messages, Options opts)
        {
            if (string.IsNullOrWhiteSpace(opts.SmtpServer)) return;

            try
            {
                using (var smtp = new SmtpClient(opts.SmtpServer, opts.SmtpPort))
                {
                    // Basic configuration - adjust if your SMTP server requires SSL or specific Credentials
                    smtp.EnableSsl = opts.EnableSsl;
                    Log.Information("Enable SSL {EnableSsL}", opts.EnableSsl);
                    
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;

                    // Note: If Auth is required, add opts.SmtpUser/Pass arguments to command line
                    smtp.Credentials = new NetworkCredential(opts.SmtpUser, opts.SmtpPass);

                    var mail = new MailMessage();
                    mail.From = new MailAddress(opts.EmailFrom);
                    mail.To.Add(opts.EmailTo);
                    //mail.Subject = "Sequence Monitor Alert: Balance Changes Detected";
                    mail.Subject = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name + " - " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version + " - " + DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss");

                    //mail.Body = messages;
                    mail.Body = string.Join(Environment.NewLine + Environment.NewLine, messages);

                    smtp.Send(mail);
                    Log.Information("Email alert sent via {SmtpServer}", opts.SmtpServer);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to send email alert.");
            }
        }
    }
}