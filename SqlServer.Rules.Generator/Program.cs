using SqlServer.Rules.Report;
using System;
using System.Configuration;
using System.IO;
using System.Text.RegularExpressions;

namespace SqlServer.Rules.Generator
{
    public class Program
    {
        private static void Main(string[] args)
        {
            var dacpacPath = ConfigurationManager.AppSettings["Dacpac"];
            var DBConnection = ConfigurationManager.AppSettings["DBConnection"];
            var TypeofReport = ReportOutputType.XML;
            var ReportFileName = ConfigurationManager.AppSettings["ReportFileName"];
            var ReportHTMLFileName = Path.GetFileNameWithoutExtension(ReportFileName);

            if ((string.IsNullOrWhiteSpace(DBConnection) && string.IsNullOrWhiteSpace(dacpacPath)) ||
                (!string.IsNullOrWhiteSpace(DBConnection) && !string.IsNullOrWhiteSpace(dacpacPath)))
            {
                Console.WriteLine("Usage: either dacpac path or db connection string should be provided");
                return;
            }

            string extension;

            extension = Path.GetExtension(ReportFileName);
            if (string.IsNullOrEmpty(extension)) TypeofReport = ReportOutputType.XML;
            else
            {
                extension = extension.Substring(1, extension.Length - 1).ToLowerInvariant();
                if (extension == "xml") TypeofReport = ReportOutputType.XML;
                else TypeofReport = ReportOutputType.CSV;
            }

            var request = new ReportRequest
            {
                Solution = args[0],
                InputPath = dacpacPath,
                InputDB = DBConnection,
                OutputFileName = ReportFileName,
                ReportOutputType = TypeofReport,
                FileName = ReportHTMLFileName,
                Suppress = p => Regex.IsMatch(p.Problem.RuleId, @"Microsoft\.Rules.*(SR0001|SR0016|SR0005|SR0007)", RegexOptions.IgnoreCase)
            };


            if (!string.IsNullOrWhiteSpace(DBConnection))
            {
                request.InputDB = DBConnection;
                request.InputPath = string.Empty;
            }
            else
            if (string.IsNullOrWhiteSpace(dacpacPath))
            {
                request.InputDB = string.Empty;
                request.InputPath = dacpacPath;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Either dacpac file path or database connection string must be provided.");
                Console.ResetColor();
                return;
            }

            var factory = new ReportFactory();

            factory.Notify += Factory_Notify;

            factory.Create(request);
        }

        private static void Factory_Notify(string notificationMessage, NotificationType type)
        {
            switch (type)
            {
                case NotificationType.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case NotificationType.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
            }
            Console.WriteLine(notificationMessage);
            Console.ResetColor();
        }
    }
}