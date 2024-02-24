<?php
// Database configuration
$dbHost = 'localhost';
$dbUsername = 'id20289598_database_1';
$dbPassword = '?WoVA+KzX>tn~yT9';
$dbName = 'id20289598_databasem_1';

// Create a new MySQLi instance
$conn = new mysqli($dbHost, $dbUsername, $dbPassword, $dbName);

// Check the connection
if ($conn->connect_error) {
    die("Connection failed: " . $conn->connect_error);
}

// Receive and process the command
if ($_SERVER['REQUEST_METHOD'] === 'POST') {
    $commandString = json_decode(file_get_contents("php://input"), true);
    if (json_last_error() == JSON_ERROR_NONE) {
        // Process the received command, for example, save it in a database

        // Use the computer's ID to determine the save file path
        $computerId = $commandString['computer_id'];

        // Prepare an SQL statement
        $stmt = $conn->prepare("INSERT INTO commands (computer_id, command) VALUES (?, ?)");

        // Bind parameters
        $command = $commandString['command'];
        $stmt->bind_param('ss', $computerId, $command);

        // Execute the statement
        if ($stmt->execute()) {
            // Send a successful response back to the client
            http_response_code(200);
        } else {
            // If the statement execution failed, return an error response
            echo "Failed to execute SQL statement: " . $stmt->error;
            http_response_code(500); // 500 Internal Server Error
        }
    } else {
        echo "Invalid JSON command";
        http_response_code(400); // 400 Bad Request
    }
} else {
    // If it's not a POST request, return an error response
    http_response_code(405); // Method not allowed
}

// Close the connection
$conn->close();