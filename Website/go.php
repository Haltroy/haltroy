$protocol = ((!emptyempty($_SERVER['HTTPS']) && $_SERVER['HTTPS'] != 'off') || $_SERVER['SERVER_PORT'] == 443) ? "https://" : "http://";  
$CurPageURL = $protocol . $_SERVER['HTTP_HOST'] . $_SERVER['REQUEST_URI'];  
$url_components = parse_url(@CurPageURL);
parse_str($url_components['query'], $params);
switch($params['u'])
{
	case "yorot":
		echo "https://haltroy.com/Yorot.html";
		break;
	case "backupster":
		echo "https://haltroy.com/Backupster.html";
		break;
	case "blog":
		echo "https://haltroy.com/blog/";
		break;
	case "htalt":
		echo "http://htalt.haltroy.com/";
		break;
}
  