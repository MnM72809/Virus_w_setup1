<?php

/* $computer_id = $_GET['computer_id'];
echo $computer_id;
exit(); */

// Enable exception error reporting
mysqli_report(MYSQLI_REPORT_ERROR | MYSQLI_REPORT_STRICT);

try {
    // Database connection
    $dbHost = 'sql300.infinityfree.com';
    $dbUsername = 'if0_36162692';
    $dbPassword = 'sitemm728';
    $dbName = 'if0_36162692_database';

    // Create a new mysqli instance
    $mysqli = new mysqli($dbHost, $dbUsername, $dbPassword, $dbName);

    // Get the ID from the POST data
    //$computer_id = filter_input(INPUT_GET, 'computer_id', FILTER_SANITIZE_STRING);
    $computer_id = $_GET['computer_id'];

    if ($computer_id === null || $computer_id === false) {
        http_response_code(400);
        echo json_encode([["error" => "Invalid or missing computer_id (" . $_POST['computer_id'] . ")"]], JSON_THROW_ON_ERROR);
        exit();
    }

    // Prepare an SQL statement
    $stmt = $mysqli->prepare("SELECT * FROM commands WHERE computer_id = ? AND status = 'pending'");

    // Bind the id to the statement
    $stmt->bind_param('s', $computer_id);

    // Execute the statement
    $stmt->execute();

    // Get the result
    $result = $stmt->get_result();

    // Fetch the data
    $data = $result->fetch_all(MYSQLI_ASSOC);

    // Close the statement
    $stmt->close();

    if (empty ($data)) {
        echo json_encode([["error" => "No data found for computer_id: $computer_id"]], JSON_THROW_ON_ERROR);
    } else {
        // Prepare an SQL statement
        $stmt = $mysqli->prepare("UPDATE commands SET status = 'delivered' WHERE computer_id = ?");

        // Bind the id to the statement
        $stmt->bind_param('s', $computer_id);

        // Execute the statement
        if ($stmt->execute() === false) {
            $data = [["error" => "Failed to update status"]];
        }

        // Close the statement
        $stmt->close();

        echo json_encode($data, JSON_THROW_ON_ERROR);
    }

    // Close the database connection
    $mysqli->close();
} catch (Exception $e) {
    http_response_code(500);
    echo json_encode([["error" => $e->getMessage()]], JSON_THROW_ON_ERROR);
}