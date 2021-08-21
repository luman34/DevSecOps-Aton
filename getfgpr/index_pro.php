<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.01//EN" 
  "http://www.w3.org/TR/html4/strict.dtd">
<html>
 <head>
   <title>!DOCTYPE</title>
   <meta http-equiv="Content-Type" content="text/html; charset=utf-8">
   <script async src="https://cdn.jsdelivr.net/npm/@fingerprintjs/fingerprintjs-pro@3/dist/fp.min.js" onload="initFingerprintJS()"></script>
 </head>
 <body>

 <?php

    if (isset($_POST['request'])){
        $result = file_put_contents("log.txt", $_POST['request']);
    }

 ?>
  <div class="BrowserInfo">
  </div> 
 </body> 

 <script>
    function initFingerprintJS() {
     const fpPromise = FingerprintJS.load({token: 'XDdYsoO7UyRXLIeiOaQC'});
        // The FingerprintJS agent is ready.
        // Get a visitor identifier when you'd like to.
    fpPromise
      .then(fp => fp.get())
      .then(result => {
		            var elem = document.body.getElementsByClassName("BrowserInfo")[0];
          var json = JSON.stringify(result);
          document.body.appendChild(document.createElement('pre')).innerHTML = syntaxHighlight(json);

          var xhr = new XMLHttpRequest();
          xhr.open("POST", '/', true);
          xhr.setRequestHeader("Content-type", "application/x-www-form-urlencoded");
          xhr.send("request="+ json);
	  });
        
    }
    function syntaxHighlight(json) {
    if (typeof json != 'string') {
         json = JSON.stringify(json, undefined, 2);
    }
    json = json.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;');
    return json.replace(/("(\\u[a-zA-Z0-9]{4}|\\[^u]|[^\\"])*"(\s*:)?|\b(true|false|null)\b|-?\d+(?:\.\d*)?(?:[eE][+\-]?\d+)?)/g, function (match) {
        var cls = 'number';
        if (/^"/.test(match)) {
            if (/:$/.test(match)) {
                cls = 'key';
            } else {
                cls = 'string';
            }
        } else if (/true|false/.test(match)) {
            cls = 'boolean';
        } else if (/null/.test(match)) {
            cls = 'null';
        }
        return '<span class="' + cls + '">' + match + '</span>';
    });
}
  </script>

    <style>
        pre {
            outline: 1px solid #ccc;
            padding: 5px;
            margin: 5px;
        }

        .string {
            color: green;
        }

        .number {
            color: darkorange;
        }

        .boolean {
            color: blue;
        }

        .null {
            color: magenta;
        }

        .key {
            color: red;
        }
    </style>
  

</html>
