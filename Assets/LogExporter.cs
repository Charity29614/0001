#if UNITY_EDITOR
using UnityEngine;
using System.IO;
using System;

public class LogExporter : MonoBehaviour
{
    private static string logFilePath;

    private void Awake()
    {
        ExportLogsToFile();
    }

    private void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    static void ExportLogsToFile()
    {
        // Set the folder path where you want to save the log file
        string folderPath = @"D:\apks\aLogs";

        // Ensure the directory exists
        Directory.CreateDirectory(folderPath);

        // Generate a unique file name based on current date and time
        string currentDateTime = DateTime.Now.ToString("yyyy-MM-dd___hh-mm-ss_tt");
        string logFileName = $"UnityLogs_{currentDateTime}.txt";

        // Set the file path using the class-level variable
        logFilePath = Path.Combine(folderPath, logFileName);

        // Clear existing log file (optional)
        File.WriteAllText(logFilePath, "");
        UnityEngine.Debug.Log($"Logging started. Exporting logs to: {logFilePath}");
        // Start listening for future logs
        Application.logMessageReceived += HandleLog;
    }

    static void HandleLog(string logMessage, string stackTrace, LogType type)
    {
        // Append new log message to the file
        using (StreamWriter writer = new StreamWriter(logFilePath, true))
        {
            string currentDateTime = DateTime.Now.ToString("hh:mm:ss.fff tt");
            string message = $"{type} - {currentDateTime}: {logMessage}";
            //string message = $"{type}: {logMessage}";
            writer.WriteLine(message);
            if (type == LogType.Error || type == LogType.Exception)
            {
                writer.WriteLine(stackTrace);
            }
        }
    }
}
#endif