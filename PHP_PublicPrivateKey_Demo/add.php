<!-- Dan Berkowitz, buildingtents.com, github.com/daberkow -->


<?PHP
	//check for items then insert, no checks, wild west styple
	if(isset($_REQUEST['loginpass']) && isset($_REQUEST['key_ID']))
	{
		mysql_connect("localhost", "root", "") or die("Could Not Connect To MYSQL");
		mysql_select_db("SSL_TEST") or die ("Could Not Connect to DATABASE");
		$result = mysql_query("INSERT INTO `SSL_TEST`.`data` (`index` ,	`data` , `key_used`) VALUES ( NULL ,  '" . $_REQUEST['loginpass'] . "',  '" . $_REQUEST['key_ID'] . "');");
		//echo "INSERT INTO `SSL_TEST`.`data` (`index` ,	`data` , `key_used`) VALUES ( NULL ,  '" . $_REQUEST['loginpass'] . "',  '" . $_REQUEST['key_ID'] . "');";
		if($result)
		{
			echo "Recorded";
		}else{
			echo "Error";
		}
		mysql_close();
	}else{
		echo "Error in passed data";
	}
?>