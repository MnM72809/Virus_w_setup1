<?php

// Enable exception error reporting
mysqli_report(MYSQLI_REPORT_ERROR | MYSQLI_REPORT_STRICT);

try {
    // Database connection
    $dbHost = 'localhost';
    $dbUsername = 'id20289598_database_1';
    $dbPassword = '?WoVA+KzX>tn~yT9';
    $dbName = 'id20289598_databasem_1';

    // Create a new mysqli instance
    $mysqli = new mysqli($dbHost, $dbUsername, $dbPassword, $dbName);

    // Get the ID from the POST data
    $computer_id = filter_input(INPUT_POST, 'computer_id', FILTER_VALIDATE_INT);

    if ($computer_id === null || $computer_id === false) {
        http_response_code(400);
        echo json_encode([["error" => "Invalid or missing computer_id"]], JSON_THROW_ON_ERROR);
        exit();
    }

    // Prepare an SQL statement
    $stmt = $mysqli->prepare("SELECT * FROM commands WHERE computer_id = ?");

    // Bind the id to the statement
    $stmt->bind_param('i', $computer_id);

    // Execute the statement
    $stmt->execute();

    // Get the result
    $result = $stmt->get_result();

    // Fetch the data
    $data = $result->fetch_all(MYSQLI_ASSOC);

    if (empty($data)) {
        echo json_encode([["error" => "No data found for computer_id: $computer_id"]], JSON_THROW_ON_ERROR);
    } else {
        echo json_encode($data, JSON_THROW_ON_ERROR);
    }

    // Close the database connection
    $mysqli->close();
} catch (Exception $e) {
    http_response_code(500);
    echo json_encode([["error" => $e->getMessage()]], JSON_THROW_ON_ERROR);
}