<?php
// Database configuration
$dbHost = 'sql300.infinityfree.com';
$dbUsername = 'if0_36162692';
$dbPassword = 'sitemm728';
$dbName = 'if0_36162692_database';

try {
    // Create a new MySQLi instance
    $conn = new mysqli($dbHost, $dbUsername, $dbPassword, $dbName);

    // Check the connection
    if ($conn->connect_error) {
        throw new Exception("Connection failed: " . $conn->connect_error);
    }

    // Receive and process the command
    if ($_SERVER['REQUEST_METHOD'] === 'POST') {
        $commandString = json_decode(file_get_contents("php://input"), true);
        if (json_last_error() == JSON_ERROR_NONE) {
            if (isset($commandString['webpage']) && $commandString['updates']) {

                // Haal de gegevens op uit de POST-request
                $id = $commandString['id'];
                $updates = $commandString['updates']; // Dit zou een associatieve array moeten zijn met kolomnamen als sleutels en bijbehorende nieuwe waarden
                // Bouw de SET-clause van de query op
                $setClause = "";
                foreach ($updates as $column => $value) {
                    $setClause .= "`$column` = ?, ";
                }

                // Verwijder de laatste komma en spatie
                $setClause = rtrim($setClause, ", ");


                // CLEAN THE ROW
                $clean_sql = "UPDATE commands SET
                              computer_id = 'CLEANING',
                              command = 'CLEANING',
                              parameters = 'CLEANING',
                              status = 'CLEANING'
                              WHERE id = ?";
                // Prepare the statement
                $clean_stmt = $conn->prepare($clean_sql);
                // Bind the parameter
                $clean_stmt->bind_param('i', $id);
                // Execute the statement
                $clean_stmt->execute();
                // Close the statement
                $clean_stmt->close();

                // CLEANED THE ROW
                $cleaned_sql = "UPDATE commands SET
                                computer_id = '',
                                command = '',
                                parameters = '',
                                status = ''
                                WHERE id = ?";
                // Prepare the statement
                $cleaned_stmt = $conn->prepare($cleaned_sql);
                // Bind the parameter
                $cleaned_stmt->bind_param('i', $id);
                // Execute the statement
                $cleaned_stmt->execute();
                // Check if the update is successful
                if ($cleaned_stmt->affected_rows <= 0) {
                    throw new Exception("Failed to clean row");
                }
                // Close the statement
                $cleaned_stmt->close();



                // Bereid de UPDATE-query voor
                $sql = "UPDATE commands SET $setClause WHERE id = ?";

                // Bereid de statement voor
                $stmt = $conn->prepare($sql);

                // Voeg de parameters toe aan een array in de juiste volgorde
                $params = array_values($updates);
                $params[] = $id;

                // Bind de parameters aan de voorbereide statement
                $types = str_repeat('s', count($params)); // 's' staat voor string, verander naar 'i' als de ID een integer is
                $stmt->bind_param($types, ...$params);

                // Voer de statement uit
                $stmt->execute();

                // Controleer of de update is gelukt
                if ($stmt->affected_rows > 0) {
                    http_response_code(200); // OK
                    echo "Update successful";
                } else {
                    throw new Exception("Update failed");
                }

                // Sluit de statement en de databaseverbinding
                $stmt->close();
                $conn->close();
                exit;
            }

            // Use the computer's ID to determine the save file path
            $computerId = $commandString['computer_id'];

            // Save computer ID into a file with a list of id's
            $computerId = $_POST['computerId'];

            $file = 'computers.txt';

            // Check if the file exists
            if (file_exists($file)) {
                $contents = file_get_contents($file);

                // Check if the computerId is already in the file
                if (strpos($contents, $computerId) === false) {
                    // If not, append it
                    file_put_contents($file, $computerId . PHP_EOL, FILE_APPEND);
                    //echo "ComputerId added successfully.";
                } else {
                    //echo "ComputerId already exists in the file.";
                }
            } else {
                // If the file doesn't exist, create it and add the computerId
                file_put_contents($file, $computerId . PHP_EOL);
                //echo "File created and ComputerId added successfully.";
            }

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
                throw new Exception("Failed to execute SQL statement: " . $stmt->error);
            }
        } else {
            throw new Exception("Invalid JSON command");
        }
    } else {
        // If it's not a POST request, return an error response
        throw new Exception("Method not allowed");
    }

    // Close the connection
    $conn->close();
} catch (InvalidArgumentException $e) {
    http_response_code(400); // 400 Bad Request
    echo $e->getMessage();
} catch (RuntimeException $e) {
    http_response_code(500); // 500 Internal Server Error
    echo $e->getMessage();
} catch (Exception $e) {
    http_response_code($e->getCode() > 0 ? $e->getCode() : 500); // Use the exception code as the HTTP status code
    echo $e->getMessage();
}