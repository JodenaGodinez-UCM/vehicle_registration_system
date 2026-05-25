using System;
using System.IO;
using System.Collections.Generic;

class Program
{
    static string dataFile = "records.txt";
    static string auditFile = "audit.log";

    static void Main()
    {
        InitializeStorage();

        while (true)
        {
            Console.WriteLine("\n===== VEHICLE SERVICE RECORD SYSTEM =====");
            Console.WriteLine("1. Add Record");
            Console.WriteLine("2. View Records");
            Console.WriteLine("3. Search Record");
            Console.WriteLine("4. Update Record");
            Console.WriteLine("5. Delete Record");
            Console.WriteLine("6. Generate Report");
            Console.WriteLine("7. Exit");
            Console.Write("Choose: ");

            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    AddRecord();
                    break;

                case "2":
                    ViewRecords();
                    break;

                case "3":
                    SearchRecord();
                    break;

                case "4":
                    UpdateRecord();
                    break;

                case "5":
                    DeleteRecord();
                    break;

                case "6":
                    GenerateReport();
                    break;

                case "7":
                    return;

                default:
                    Console.WriteLine("Invalid choice.");
                    LogAction("ERROR", "Invalid menu choice");
                    break;
            }
        }
    }

    // =========================
    // INITIALIZE STORAGE
    // =========================
    static void InitializeStorage()
    {
        if (!File.Exists(dataFile))
            File.Create(dataFile).Close();

        if (!File.Exists(auditFile))
            File.Create(auditFile).Close();

        LogAction("SYSTEM", "Storage initialized");
    }

    // =========================
    // ADD RECORD
    // =========================
    static void AddRecord()
    {
        Console.Write("Enter Owner Name: ");
        string owner = Console.ReadLine();

        Console.Write("Enter Plate Number: ");
        string plate = Console.ReadLine();

        Console.Write("Enter Service Type: ");
        string service = Console.ReadLine();

        Console.Write("Enter Service Cost: ");
        string cost = Console.ReadLine();

        // Validation
        if (string.IsNullOrWhiteSpace(owner) ||
            string.IsNullOrWhiteSpace(plate) ||
            string.IsNullOrWhiteSpace(service))
        {
            Console.WriteLine("Invalid input.");
            LogAction("ERROR", "Invalid input during add");
            return;
        }

        string id =
            DateTime.Now.Millisecond.ToString();

        string createdAt =
            DateTime.Now.ToString();

        bool isActive = true;

        // Simple checksum
        string checksum =
            (owner + plate + service + cost)
            .GetHashCode()
            .ToString();

        string record =
            $"{id}|{owner}|{plate}|{service}|{cost}|{createdAt}|{createdAt}|{isActive}|{checksum}";

        File.AppendAllText(dataFile, record + Environment.NewLine);

        Console.WriteLine("Record added successfully.");

        LogAction("ADD", $"Record {id} added");
    }

    // =========================
    // VIEW RECORDS
    // =========================
    static void ViewRecords()
    {
        string[] records =
            File.ReadAllLines(dataFile);

        Console.WriteLine("\n--- RECORDS ---");

        foreach (string record in records)
        {
            if (!string.IsNullOrWhiteSpace(record))
            {
                string[] parts =
                    record.Split('|');

                if (parts[7] == "True")
                {
                    Console.WriteLine($"ID: {parts[0]}");
                    Console.WriteLine($"Owner: {parts[1]}");
                    Console.WriteLine($"Plate: {parts[2]}");
                    Console.WriteLine($"Service: {parts[3]}");
                    Console.WriteLine($"Cost: {parts[4]}");
                    Console.WriteLine($"Created: {parts[5]}");
                    Console.WriteLine($"Updated: {parts[6]}");
                    Console.WriteLine($"Checksum: {parts[8]}");
                    Console.WriteLine("----------------------");
                }
            }
        }

        LogAction("READ", "Viewed records");
    }

    // =========================
    // SEARCH RECORD
    // =========================
    static void SearchRecord()
    {
        Console.Write("Enter Owner Name: ");

        string keyword =
            Console.ReadLine().ToLower();

        string[] records =
            File.ReadAllLines(dataFile);

        foreach (string record in records)
        {
            if (!string.IsNullOrWhiteSpace(record))
            {
                string[] parts =
                    record.Split('|');

                if (parts[1].ToLower().Contains(keyword)
                    && parts[7] == "True")
                {
                    Console.WriteLine($"ID: {parts[0]}");
                    Console.WriteLine($"Owner: {parts[1]}");
                    Console.WriteLine($"Plate: {parts[2]}");
                    Console.WriteLine($"Service: {parts[3]}");
                    Console.WriteLine($"Cost: {parts[4]}");
                    Console.WriteLine("----------------------");
                }
            }
        }

        LogAction("SEARCH", $"Searched {keyword}");
    }

    // =========================
    // UPDATE RECORD
    // =========================
    static void UpdateRecord()
    {
        Console.Write("Enter Record ID to update: ");

        string id = Console.ReadLine();

        List<string> records =
            new List<string>(File.ReadAllLines(dataFile));

        for (int i = 0; i < records.Count; i++)
        {
            string[] parts =
                records[i].Split('|');

            if (parts[0] == id)
            {
                Console.Write("New Owner Name: ");
                parts[1] = Console.ReadLine();

                Console.Write("New Service Type: ");
                parts[3] = Console.ReadLine();

                Console.Write("New Service Cost: ");
                parts[4] = Console.ReadLine();

                // Update timestamp
                parts[6] =
                    DateTime.Now.ToString();

                // Recompute checksum
                parts[8] =
                    (parts[1] + parts[2] + parts[3] + parts[4])
                    .GetHashCode()
                    .ToString();

                records[i] =
                    string.Join("|", parts);

                File.WriteAllLines(dataFile, records);

                Console.WriteLine("Record updated.");

                LogAction("UPDATE", $"Record {id} updated");

                return;
            }
        }

        Console.WriteLine("Record not found.");
    }

    // =========================
    // DELETE RECORD
    // =========================
    static void DeleteRecord()
    {
        Console.Write("Enter Record ID to delete: ");

        string id = Console.ReadLine();

        List<string> records =
            new List<string>(File.ReadAllLines(dataFile));

        for (int i = 0; i < records.Count; i++)
        {
            string[] parts =
                records[i].Split('|');

            if (parts[0] == id)
            {
                // Soft delete
                parts[7] = "False";

                records[i] =
                    string.Join("|", parts);

                File.WriteAllLines(dataFile, records);

                Console.WriteLine("Record deleted.");

                LogAction("DELETE", $"Record {id} soft deleted");

                return;
            }
        }

        Console.WriteLine("Record not found.");
    }

    // =========================
    // GENERATE REPORT
    // =========================
    static void GenerateReport()
    {
        string[] records =
            File.ReadAllLines(dataFile);

        int active = 0;
        int deleted = 0;

        foreach (string record in records)
        {
            if (!string.IsNullOrWhiteSpace(record))
            {
                string[] parts =
                    record.Split('|');

                if (parts[7] == "True")
                    active++;
                else
                    deleted++;
            }
        }

        string report =
            $"VEHICLE SERVICE REPORT\n" +
            $"Active Records: {active}\n" +
            $"Deleted Records: {deleted}";

        File.WriteAllText("report.txt", report);

        Console.WriteLine("Report generated.");

        LogAction("REPORT", "Report generated");
    }

    // =========================
    // AUDIT LOGGER
    // =========================
    static void LogAction(string action, string details)
    {
        string log =
            $"{DateTime.Now} | {action} | {details}";

        File.AppendAllText(auditFile,
            log + Environment.NewLine);
    }
}