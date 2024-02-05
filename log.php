<?php

$currentDateTime = date("H:i:s d-m-Y");
$cdt = $currentDateTime;

?>


<!DOCTYPE html>

<html lang="en" xmlns="http://www.w3.org/1999/xhtml">
<head>
    <meta charset="utf-8" />
    <title>Program access <?php echo $cdt; ?></title>
    <!-- Google tag (gtag.js) -->
        <script async src="https://www.googletagmanager.com/gtag/js?id=G-6VG8KMTNYP"></script>
        <script>
        window.dataLayer = window.dataLayer || [];

        function gtag() {
            dataLayer.push(arguments);
        }
        gtag('js', new Date());

        gtag('config', 'G-6VG8KMTNYP');
        </script>
</head>
<body>
</body>
</html>