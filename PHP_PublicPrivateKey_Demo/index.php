<!-- Dan Berkowitz, buildingtents.com, github.com/daberkow -->

<html>
	<head>
		<!-- Jquery just to do ajax and easy data injection, then the rest of it is used to interpret public key stuff -->
		<script src="./Script_Library/jquery-1.6.2.min.js"></script>
		<script language="JavaScript" type="text/javascript" src="./Script_Library/jsbn.js"></script>
		<script language="JavaScript" type="text/javascript" src="./Script_Library/prng4.js"></script>
		<script language="JavaScript" type="text/javascript" src="./Script_Library/rng.js"></script>
		<script language="JavaScript" type="text/javascript" src="./Script_Library/rsa.js"></script>
		<script>
			//configure RSA
			function submit_items()
			{
				// make a general key and then insert my redentials
				var rsa = new RSAKey();
				<?PHP
					mysql_connect("localhost", "root", "") or die("Could Not Connect To MYSQL");
					mysql_select_db("SSL_TEST") or die ("Could Not Connect to DATABASE");
					$result = mysql_query("SELECT * FROM `keys` WHERE `active`=1");
					if( mysql_num_rows($result) == 1)
					{
						while ($row = mysql_fetch_array($result))//Should be run once
						{	
							$modulus = $row['modulus'];
							$exponent = $row['exponent'];
							$key_ID = $row['index'];
						}
					}else{
						echo "Received multiple keys, requesting Database maintenance...";
						//Code needs added here to call job
					}
					
					echo "rsa.setPublic('". $modulus . "', '" . $exponent . "');";
					echo "var key_ID_mysql = " . $key_ID . ";"
				?>
				
				//if there is a value go and encrpt it
				if($("#loginpass").val() != "")
				{
					var enc_loginpass = rsa.encrypt($("#username").val());
				}else{
					var enc_loginpass = "";
				}
				
				$("#encrypted").html(enc_loginpass); // print it out for funzies

				//ajax post that stuff
				$.ajax({
						type: 'POST',
						url: "./add.php",
						data: {loginpass: enc_loginpass, key_ID: key_ID_mysql},
						success: function(data) {
							$("#result").html(data);
						},
					});
			}
				
		</script>
	</head>
	
	<body style='background: grey;'>
		<div id="main_body" style='width: 800px; background: white; height: 300px; margin:auto;'>
			<form name='new_device'>
				<p>Data</p><input class='input_box' type='text' name='username' id='username'/></li>
				<input TYPE='button' id='cmdSubmit' VALUE='Submit' onclick='submit_items()'/>
			</form>
			Encrypted<div id='encrypted' style='word-wrap: normal; 	text-overflow: clip;'></div>
			<div id='result'></div>
		</div>
	</body>

</html>